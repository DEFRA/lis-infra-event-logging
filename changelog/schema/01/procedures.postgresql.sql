-- liquibase formatted sql

-- changeset system:initial-seed-3 splitStatements:false
create or replace function clear_event_extracted_values_before_update() returns trigger
    SET search_path = pg_catalog, public
    language plpgsql
as
$$
BEGIN
  DELETE FROM public.event_extracted_values
  WHERE event_id = OLD.id;

RETURN NEW;
END;
$$;

alter function clear_event_extracted_values_before_update() owner
  to lis_infra_event_logging_ddl;

create or replace function decompose_event_data() returns trigger
    SET search_path = pg_catalog, public
    language plpgsql
as
$$
DECLARE
extraction_rule record;
    extracted_values jsonb[];
    extracted_value jsonb;
    match_count integer;
    value_ordinal integer;
BEGIN
FOR extraction_rule IN
SELECT
  eer.id,
  eet.name,
  eer.json_path,
  eer.value_type,
  eer.required,
  eer.allows_multiple
FROM public.event_extraction_rules eer left join
     public.event_extraction_tokens eet on
       eer.token_id = eet.id
WHERE eer.sub_taxonomy_id = NEW.sub_taxonomy_id
ORDER BY id

  LOOP
SELECT COALESCE(
         array_agg(matches.value ORDER BY matches.ordinality),
         ARRAY[]::jsonb[]
       )
INTO extracted_values
FROM jsonb_path_query(
       COALESCE(NEW.data, 'null'::jsonb),
       extraction_rule.json_path
     ) WITH ORDINALITY AS matches(value, ordinality);

match_count := cardinality(extracted_values);

            IF extraction_rule.required AND match_count = 0 THEN
                RAISE EXCEPTION USING
                    ERRCODE = '23514',
                    MESSAGE = format(
                            'Required extraction rule "%s" returned no value for event %s',
                            extraction_rule.name,
                            NEW.id
                              );
END IF;

            IF NOT extraction_rule.allows_multiple AND match_count > 1 THEN
                RAISE EXCEPTION USING
                    ERRCODE = '21000',
                    MESSAGE = format(
                            'Extraction rule "%s" returned %s values for event %s but allows_multiple is false',
                            extraction_rule.name,
                            match_count,
                            NEW.id
                              );
END IF;

            value_ordinal := 0;

            FOREACH extracted_value IN ARRAY extracted_values
                LOOP
BEGIN
INSERT INTO public.event_extracted_values (
  event_id,
  extraction_rule_id,
  sub_taxonomy_id,
  value_type,
  value_ordinal,
  value_text,
  value_number,
  value_boolean,
  value_date,
  value_timestamp,
  value_uuid,
  value_json
)
VALUES (
         NEW.id,
         extraction_rule.id,
         NEW.sub_taxonomy_id,
         extraction_rule.value_type,
         value_ordinal,
         CASE
           WHEN extraction_rule.value_type = 'text'
             THEN extracted_value #>> '{}'
           END,
         CASE
           WHEN extraction_rule.value_type = 'number'
             THEN (extracted_value #>> '{}')::numeric
                                       END,
         CASE
           WHEN extraction_rule.value_type = 'boolean'
             THEN (extracted_value #>> '{}')::boolean
           END,
         CASE
           WHEN extraction_rule.value_type = 'date'
             THEN (extracted_value #>> '{}')::date
                                       END,
         CASE
           WHEN extraction_rule.value_type = 'timestamp'
             THEN (extracted_value #>> '{}')::timestamp with time zone
                                       END,
         CASE
           WHEN extraction_rule.value_type = 'uuid'
             THEN (extracted_value #>> '{}')::uuid
           END,
         CASE
           WHEN extraction_rule.value_type = 'json'
             THEN extracted_value
           END
       );
EXCEPTION
                    WHEN data_exception THEN
                      RAISE EXCEPTION USING
                        ERRCODE = '22023',
                        MESSAGE = format(
                          'Value returned by extraction rule "%s" cannot be converted to %s',
                          extraction_rule.name,
                          extraction_rule.value_type
                        ),
                        DETAIL = SQLERRM;
                    END;

                    value_ordinal := value_ordinal + 1;
END LOOP;
END LOOP;

RETURN NEW;
END;
$$;

alter function decompose_event_data() owner
  to lis_infra_event_logging_ddl;

-- changeset system:initial-seed-5
create trigger events_clear_extracted_values_before_update
  before update of data, sub_taxonomy_id on public.events
  for each row execute function clear_event_extracted_values_before_update();

create trigger events_decompose_data_after_insert_or_update
  after insert or update of data, sub_taxonomy_id on public.events
  for each row execute function decompose_event_data();
