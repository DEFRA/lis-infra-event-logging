# lis-infra-event-logging

PostgreSQL-backed API for recording livestock events and storing event artefacts in S3.
The service accepts writes asynchronously: clients receive the reserved event identifiers immediately,
while background workers reliably create the event or artefact through SQS.

The database schema is managed by Liquibase in `changelog/` and represented by EF Core
entities in `src/Database`.

## Event submission process

The write path uses the [transactional outbox pattern](https://microservices.io/patterns/data/transactional-outbox.html).
This prevents an accepted submission from being lost in the gap between committing data to
PostgreSQL and publishing work to SQS.

```text
Client
  |
  | POST /log, /log/with-artefact, or /log/{logId}/artefacts
  v
API
  |-- validate request and submission headers
  |-- stage artefact in S3 when present
  |-- one PostgreSQL transaction:
  |     1. insert event_submissions (Pending)
  |     2. insert outbox_messages
  v
202 Accepted (logId, optional artefactId)

OutboxPublisherService
  |-- poll unpublished outbox_messages
  |-- publish payload to SQS
  |-- record publication success or failure
  v
SQS event-submission queue
  v
EventSubmissionConsumerService
  |-- deserialize and validate message schema version
  |-- create the event and/or artefact in PostgreSQL
  |-- mark the submission Completed
  |-- generate a thumbnail when applicable
  |-- delete the SQS message after successful processing
```

### Supported submission types

| Type | Purpose | Relationship to the event |
| --- | --- | --- |
| `CreateEvent` | Create an event without an artefact | Reserves a new `log_id`; the event insert generates `url_short_code` |
| `CreateEventWithArtefact` | Create an event and its first artefact | Reserves a new event and artefact ID and stages the file in S3 |
| `AddArtefact` | Attach an artefact to an existing event | Uses the existing event ID as `log_id` and reserves a new artefact ID |

### `event_submissions`

`event_submissions` is an internal delivery and idempotency record. It is not an API resource
and is never queried directly by external callers. It stores:

- the operation type and its `Pending`, `Processing`, `Completed`, or `Failed` status;
- the reserved `log_id` and optional `artefact_id`;
- client, idempotency, request fingerprint, and correlation information;
- staged S3 object metadata for artefact submissions; and
- submission, processing, completion, and failure information.

`event_submissions.log_id` and `events.id` form an application-managed relationship; there
is deliberately no foreign key between them. A create submission reserves its event ID before
the event exists, so a `Pending` submission may legitimately refer to no `events` row yet. Once
the create submission completes, the worker has created the event using the reserved ID.

The worker does not generate `url_short_code` in application code or store it on the submission.
PostgreSQL's `events_generate_short_url_code` trigger generates it while inserting the event and
checks the authoritative `events` table for a collision. The `events` table remains the sole
source of truth for both event data and its URL short code.

One event may therefore be associated logically with its original creation submission and any
number of later `AddArtefact` submissions. For an `AddArtefact` request, the API first verifies
that the referenced event already exists.

### `outbox_messages`

An outbox row contains the versioned JSON message needed by the worker, plus publication state:
`published_at`, `attempt_count`, and `last_error`. It has an enforced foreign key to its
`event_submissions` row.

The API inserts the submission and outbox message with one `SaveChangesAsync` call. EF Core
therefore commits both in one database transaction. This avoids both unsafe direct-publication
orders:

- database first: the database commit could succeed and the SQS send could fail, leaving work
  permanently unqueued;
- SQS first: the message could be consumed even though its submission was never committed.

`OutboxPublisherService` selects rows whose `published_at` is null, oldest first, and publishes
their JSON payloads in batches. A successful send sets `published_at` and increments
`attempt_count`. A failed send leaves the row unpublished, increments `attempt_count`, and records
`last_error`, allowing a later poll to retry it.

### Queue consumption and retries

`EventSubmissionConsumerService` long-polls SQS. It deletes a message only after the submission
processor finishes successfully. If processing throws, the message is not deleted and becomes
visible again after the configured SQS visibility timeout.

Delivery is **at least once**, so duplicate messages are expected and processing is idempotent:

- an already completed submission is not persisted again;
- an event is inserted only when its `events.id` does not already exist; and
- an artefact is inserted only when its `event_artefacts.id` does not already exist.

Persistence errors set the submission to `Failed` with failure code `persistence_failed`, then
the SQS message is retained for retry. A later successful retry can complete that submission.

For artefacts, the original object is uploaded before the database transaction because the
request stream cannot be put in the queue. If database persistence fails, the API attempts to
delete that staged object. During consumption, the worker creates the artefact record and then
generates its thumbnail. Unsupported media types are recorded as such; thumbnail generation
failures are recorded on the artefact and leave the SQS message available for retry. The
submission itself is completed when its event/artefact records have been persisted, before
thumbnail processing finishes.

### Idempotency

All submission endpoints require these headers:

- `x-api-key` identifies the client; its SHA-256 hash is stored as `client_id`;
- `idempotency-key` identifies a request from that client; and
- `x-cdp-request-id` is a UUID used as the correlation ID.

The database uniquely constrains `(client_id, idempotency_key)`. Repeating the same request with
the same key returns the same reserved event identifiers instead of creating new work. A SHA-256
request fingerprint is also stored; reusing the key with different request content is rejected.

For artefacts, the fingerprint includes event/file metadata but not the file bytes. It includes
the MIME type, original filename and size, and also the target event ID for `AddArtefact`.

### Internal retention

There is no public submission-status endpoint. Reads and searches are performed against
`events` through `/events` and `/query` only. `EventSubmissionCleanupService` periodically removes
terminal `Completed` or `Failed` submissions after the configured retention period, but only when
all associated outbox messages have been published. It deletes the dependent outbox rows first.
The default retention period is 24 hours, which also defines the effective idempotency window.

## Data model

Events are classified by species, taxonomy and sub-taxonomy. Their payloads are stored as JSONB.
Configured extraction rules project searchable, strongly typed values into
`event_extracted_values`. Event artefact rows reference original objects held in S3; generated
thumbnail bytes and metadata are stored on the artefact row in PostgreSQL.

The important delivery relationships are:

```text
event_submissions.id  1 <--- * outbox_messages.submission_id  (database foreign key)
event_submissions.log_id ---> events.id                       (logical relationship)
events.id              1 <--- * event_artefacts.event_id      (database foreign key)
```

## Configuration

Submission workers are hosted in the API process and use the `EventSubmissionQueue` section:

| Setting | Default | Purpose |
| --- | ---: | --- |
| `QueueUrl` | empty | Required SQS queue URL |
| `WaitTimeSeconds` | `20` | SQS long-poll duration |
| `VisibilityTimeoutSeconds` | `120` | Time before an unacknowledged message can be retried |
| `OutboxBatchSize` | `10` | Maximum unpublished rows read per batch |
| `OutboxPollIntervalSeconds` | `2` | Delay when no unpublished rows are found |
| `SubmissionRetentionHours` | `24` | Retention period for terminal internal submissions |
| `CleanupIntervalSeconds` | `3600` | Delay between cleanup batches |
| `CleanupBatchSize` | `100` | Maximum terminal submissions deleted per cleanup run |

Artefact storage uses `ArtefactStorage:BucketName`. PostgreSQL uses the
`PostgresConnection` and `ReadOnlyPostgresConnection` connection strings.

## Running locally

The Docker Compose environment contains PostgreSQL, Liquibase, Floci for local S3/SQS emulation,
Redis, and the API service. Floci initialization files create the required AWS resources.

```bash
docker compose up --build -d
```

A more extensive setup is available in the
[CDP local environment](https://github.com/DEFRA/cdp-local-environment).

To run the API directly after providing PostgreSQL, AWS, queue, and bucket configuration:

```bash
dotnet run --project src/Api/Api.csproj --launch-profile Development
```

## Testing

```bash
dotnet test
```

## Database migrations

Liquibase changelogs live in `changelog/`; `changelog/db.changelog.xml` is the root changelog.
Docker Compose runs Liquibase after PostgreSQL becomes healthy and before starting the API.

## Licence

This project is licensed under the Open Government Licence. See [LICENSE](LICENSE) for details.
