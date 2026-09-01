-- liquibase formatted sql

-- changeset system:initial-seed-2
create table public.event_taxonomies
(
  id   uuid default uuid_generate_v4() not null
    constraint event_taxonomies_pk
      primary key,
  name text                            not null
    constraint event_taxonomies_name_uq
      unique
);

alter table public.event_taxonomies
  owner to lis_infra_event_logging_ddl;

create table public.event_species
(
  id   uuid default uuid_generate_v4() not null
    constraint event_species_pk
      primary key,
  name text                            not null
    constraint event_species_name_uq
      unique
);

alter table public.event_species
  owner to lis_infra_event_logging_ddl;

create table public.event_sub_taxonomies
(
  id          uuid default uuid_generate_v4() not null
    constraint event_sub_taxonomy_pk
      primary key,
  taxonomy_id uuid                            not null
    constraint event_sub_taxonomy_event_taxonomies_id_fk
      references public.event_taxonomies,
  species_id  uuid                            not null
    constraint event_sub_taxonomies_event_species_id_fk
      references public.event_species,
  name        text                            not null,
  constraint event_sub_taxonomies_taxonomy_species_name_uq
    unique (taxonomy_id, species_id, name)
);

alter table public.event_sub_taxonomies
  owner to lis_infra_event_logging_ddl;

create index event_sub_taxonomies_taxonomy_id_idx
  on public.event_sub_taxonomies (taxonomy_id);

create index event_sub_taxonomies_species_id_idx
  on public.event_sub_taxonomies (species_id);

create table public.events
(
  id                    uuid                     default uuid_generate_v4() not null
    constraint events_pk
      primary key,
  short_id              varchar(32)                                         not null
    constraint events_short_id_uq
      unique,
  county_parish_holding text                                                not null
    constraint cph_structure_check
      check (county_parish_holding ~ '^\d{2}/\d{3}/\d{4}$'::text),
    county                text generated always as ("left"(county_parish_holding, 2)) stored,
    parish                text generated always as (substr(county_parish_holding, 4, 3)) stored,
    holding               text generated always as ("right"(county_parish_holding, 4)) stored,
    created_at            timestamp with time zone default now()              not null,
    title                 text                                                not null,
    sub_taxonomy_id       uuid                                                not null
        constraint events_event_sub_taxonomies_id_fk
            references public.event_sub_taxonomies,
    data                  jsonb,
    created_by            text                                                not null,
    constraint events_id_sub_taxonomy_uq
        unique (id, sub_taxonomy_id)
);

alter table public.events
  owner to lis_infra_event_logging_ddl;

create index events_county_index
  on public.events (county);

create index events_county_parish_holding_index
  on public.events (county_parish_holding);

create index events_created_at_index
  on public.events (created_at);

create index events_created_by_index
  on public.events (created_by);

create index events_holding_index
  on public.events (holding);

create index events_parish_index
  on public.events (parish);

create index events_sub_taxonomy_id_idx
  on public.events (sub_taxonomy_id);

create index events_created_at_id_idx
  on public.events (created_at desc, id desc);

create index events_cph_created_at_id_idx
  on public.events (county_parish_holding, created_at desc, id desc);

create table public.event_extraction_tokens
(
  id          uuid default uuid_generate_v4() not null
    constraint event_extraction_tokens_pk
      primary key,
  name        text                            not null,
  description text
);

create unique index event_extraction_tokens_name_uq
  on public.event_extraction_tokens (name);

alter table public.event_extraction_tokens
  owner to lis_infra_event_logging_ddl;

create table public.event_extraction_rules
(
  id              uuid                     default uuid_generate_v4() not null
    constraint event_extraction_rules_pk
      primary key,
  sub_taxonomy_id uuid                                                not null
    constraint event_extraction_rules_sub_taxonomy_fk
      references public.event_sub_taxonomies,
  token_id        uuid                                                not null
    constraint event_extraction_rules_event_extraction_tokens_id_fk
      references public.event_extraction_tokens,
  json_path       jsonpath                                            not null,
  value_type      text                                                not null
    constraint event_extraction_rules_value_type_check
      check (value_type = ANY
             (ARRAY ['text'::text, 'number'::text, 'boolean'::text, 'date'::text, 'timestamp'::text, 'uuid'::text, 'json'::text])),
  required        boolean                  default false              not null,
  allows_multiple boolean                  default false              not null,
  created_at      timestamp with time zone default now()              not null,
  constraint event_extraction_rules_id_sub_taxonomy_type_uq
    unique (id, sub_taxonomy_id, value_type),
  constraint event_extraction_rules_sub_taxonomy_name_uq
    unique (sub_taxonomy_id, token_id)
);

alter table public.event_extraction_rules
  owner to lis_infra_event_logging_ddl;

create index event_extraction_rules_sub_taxonomy_idx
  on public.event_extraction_rules (sub_taxonomy_id);

create table public.event_extracted_values
(
  event_id           uuid              not null,
  extraction_rule_id uuid              not null,
  sub_taxonomy_id    uuid              not null,
  value_type         text              not null,
  value_ordinal      integer default 0 not null
    constraint event_extracted_values_ordinal_check
      check (value_ordinal >= 0),
  value_text         text,
  value_number       numeric,
  value_boolean      boolean,
  value_date         date,
  value_timestamp    timestamp with time zone,
  value_uuid         uuid,
  value_json         jsonb,
  constraint event_extracted_values_pk
    primary key (event_id, extraction_rule_id, value_ordinal),
  constraint event_extracted_values_event_fk
    foreign key (event_id, sub_taxonomy_id) references public.events (id, sub_taxonomy_id)
      on delete cascade,
  constraint event_extracted_values_rule_fk
    foreign key (extraction_rule_id, sub_taxonomy_id, value_type) references public.event_extraction_rules (id, sub_taxonomy_id, value_type),
  constraint event_extracted_values_single_value_check
    check (num_nonnulls(value_text, value_number, value_boolean, value_date, value_timestamp, value_uuid,
                        value_json) = 1),
  constraint event_extracted_values_matching_type_check
    check (((value_type = 'text'::text) AND (value_text IS NOT NULL)) OR
           ((value_type = 'number'::text) AND (value_number IS NOT NULL)) OR
           ((value_type = 'boolean'::text) AND (value_boolean IS NOT NULL)) OR
           ((value_type = 'date'::text) AND (value_date IS NOT NULL)) OR
           ((value_type = 'timestamp'::text) AND (value_timestamp IS NOT NULL)) OR
           ((value_type = 'uuid'::text) AND (value_uuid IS NOT NULL)) OR
           ((value_type = 'json'::text) AND (value_json IS NOT NULL)))
);

alter table public.event_extracted_values
  owner to lis_infra_event_logging_ddl;

create index event_extracted_values_text_filter_idx
  on public.event_extracted_values (extraction_rule_id, value_text, event_id)
  where (value_text IS NOT NULL);

create index event_extracted_values_number_filter_idx
  on public.event_extracted_values (extraction_rule_id, value_number, event_id)
  where (value_number IS NOT NULL);

create index event_extracted_values_boolean_filter_idx
  on public.event_extracted_values (extraction_rule_id, value_boolean, event_id)
  where (value_boolean IS NOT NULL);

create index event_extracted_values_date_filter_idx
  on public.event_extracted_values (extraction_rule_id, value_date, event_id)
  where (value_date IS NOT NULL);

create index event_extracted_values_timestamp_filter_idx
  on public.event_extracted_values (extraction_rule_id, value_timestamp, event_id)
  where (value_timestamp IS NOT NULL);

create index event_extracted_values_uuid_filter_idx
  on public.event_extracted_values (extraction_rule_id, value_uuid, event_id)
  where (value_uuid IS NOT NULL);

create index event_extracted_values_json_filter_idx
  on public.event_extracted_values using gin (value_json)
  where (value_json IS NOT NULL);

create table public.event_submissions
(
  id                    uuid                     default uuid_generate_v4() not null
    constraint event_submissions_pk
      primary key,
  type                  text                                                not null
    constraint event_submissions_type_check
      check (type in ('CreateEvent', 'CreateEventWithArtefact', 'AddArtefact')),
  status                text                     default 'Pending'           not null
    constraint event_submissions_status_check
      check (status in ('Pending', 'Processing', 'Completed', 'Failed')),
  log_id                uuid                                                not null,
  artefact_id           uuid unique,
  short_id              varchar(32)                                         not null,
  client_id             varchar(100)                                        not null,
  idempotency_key       varchar(255)                                        not null,
  request_fingerprint   char(64)                                            not null,
  correlation_id        uuid                                                not null,
  pending_s3_key        varchar(1024),
  original_filename     varchar(255),
  mime_type             varchar(255),
  failure_code          varchar(100),
  submitted_at          timestamp with time zone default now()              not null,
  processing_started_at timestamp with time zone,
  completed_at          timestamp with time zone,
  updated_at            timestamp with time zone default now()              not null,
  constraint event_submissions_client_idempotency_uq
    unique (client_id, idempotency_key)
);

alter table public.event_submissions
  owner to lis_infra_event_logging_ddl;

create index event_submissions_log_id_idx
  on public.event_submissions (log_id);

create unique index event_submissions_created_event_short_id_uq
  on public.event_submissions (short_id)
  where type in ('CreateEvent', 'CreateEventWithArtefact');

create table public.outbox_messages
(
  id             uuid                     default uuid_generate_v4() not null
    constraint outbox_messages_pk
      primary key,
  submission_id  uuid                                                not null
    constraint outbox_messages_event_submissions_id_fk
      references public.event_submissions,
  message_type   varchar(100)                                        not null,
  schema_version integer                                             not null,
  payload        jsonb                                               not null,
  created_at     timestamp with time zone default now()              not null,
  published_at   timestamp with time zone,
  attempt_count  integer                  default 0                  not null,
  last_error     text
);

alter table public.outbox_messages
  owner to lis_infra_event_logging_ddl;

create index outbox_messages_unpublished_idx
  on public.outbox_messages (published_at, created_at);

create table public.event_artefacts
(
  id                uuid                     default uuid_generate_v4() not null
    constraint event_artefacts_pk
      primary key,
  event_id          uuid                                                not null
    constraint event_artefacts_events_id_fk
      references public.events
      on delete no action,
  mime_type         text                                                not null,
  original_filename text                                                not null,
  s3_path           text                                                not null,
  thumbnail         bytea,
  thumbnail_mime_type varchar(100),
  thumbnail_width   integer,
  thumbnail_height  integer,
  thumbnail_status  varchar(20)              default 'Pending'         not null,
  thumbnail_failure_code varchar(100),
  created_at        timestamp with time zone default now()              not null
);

alter table public.event_artefacts
  owner to lis_infra_event_logging_ddl;

create index event_artefacts_event_id_idx
  on public.event_artefacts (event_id);
