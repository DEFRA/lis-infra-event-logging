-- liquibase formatted sql

-- changeset test-container:999-1 runAlways:true context:testcontainer splitStatements:false
DO $$
DECLARE
  -- SPECIES
  SPECIES_CATTLE CONSTANT uuid := 'b382cadc-bd49-4a50-bd6d-74cf82f6c7c9';
  SPECIES_SHEEP CONSTANT uuid := '88c232cf-5c3b-4b44-ae06-8930c4ed2cfd';
  SPECIES_GOAT CONSTANT uuid := 'fda2dcf2-6caa-4dbb-8b0a-5882db3ac98b';

  -- TAXONOMIES
  TAX_BIRTH CONSTANT uuid := '24be9980-b329-4762-8218-56fd200936a6';
  TAX_MOVE CONSTANT uuid := '44c7dce6-18f0-4dc2-8a8d-70431f40bb63';
  TAX_DEATH CONSTANT uuid := '2ff11f6c-9627-4f31-a666-840b138a516e';
BEGIN

  -- clear down
  TRUNCATE TABLE
    event_extraction_rules,
    event_extracted_values,
    event_artefacts,
    events
    RESTART IDENTITY
    CASCADE;


INSERT INTO event_extraction_rules (id, sub_taxonomy_id, token_id, json_path, value_type,
                                    required, allows_multiple, created_at) VALUES
('ef89641c-0c17-4a35-abd2-78ae4d6943ef','1b91ca25-2a65-4411-9db6-ab9223132886','f1baa73d-c89a-44c9-aec1-79090a277845','$.\"animals\"[*].\"earTag\"','text',true,true,'2026-08-07 16:43:29.354956 +00:00'),
('2db0a47b-fb4f-4b0f-94fa-332827d17462','1b91ca25-2a65-4411-9db6-ab9223132886','e6a0e6e9-9939-4211-a71e-f54fa8bd6c7a','$.\"clientReference\"','text',true,false,'2026-08-07 16:43:29.354956 +00:00');

END
$$;