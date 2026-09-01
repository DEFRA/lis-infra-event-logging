-- liquibase formatted sql

-- changeset system:initial-seed-4
INSERT INTO event_species (id, name) VALUES
  ('b382cadc-bd49-4a50-bd6d-74cf82f6c7c9', 'CTT'),
  ('88c232cf-5c3b-4b44-ae06-8930c4ed2cfd', 'SHP'),
  ('fda2dcf2-6caa-4dbb-8b0a-5882db3ac98b', 'GT');

INSERT INTO event_taxonomies (id, name) VALUES
  ('24be9980-b329-4762-8218-56fd200936a6','BIRTH'),
  ('44c7dce6-18f0-4dc2-8a8d-70431f40bb63','MOVE'),
  ('2ff11f6c-9627-4f31-a666-840b138a516e','DEATH');
                                       

INSERT INTO event_sub_taxonomies (id, taxonomy_id, species_id, name) VALUES
  -- Cattle default taxonomy captures 
  ('1b91ca25-2a65-4411-9db6-ab9223132886','24be9980-b329-4762-8218-56fd200936a6','b382cadc-bd49-4a50-bd6d-74cf82f6c7c9','DEFAULT'),
  ('afaec299-3103-4b8f-89cc-7a3a91f20563','44c7dce6-18f0-4dc2-8a8d-70431f40bb63','b382cadc-bd49-4a50-bd6d-74cf82f6c7c9','DEFAULT'),
  ('609e7aae-13ca-4e42-a3d6-509800e41326','2ff11f6c-9627-4f31-a666-840b138a516e','b382cadc-bd49-4a50-bd6d-74cf82f6c7c9','DEFAULT');

INSERT INTO event_extraction_tokens (id, name, description) VALUES
  ('e6a0e6e9-9939-4211-a71e-f54fa8bd6c7a', 'submission_ref', 'The submission reference of the item'),
  ('f1baa73d-c89a-44c9-aec1-79090a277845', 'ear_tag', 'The ear tag reference of the animal');

