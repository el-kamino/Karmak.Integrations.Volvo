-- Remove indexes and columns that supported the migration of data from cosmos to sql

DROP INDEX IF EXISTS [UX_ReactDataEntities_EntityType_CosmosDocumentId] ON [dbo].[ReactDataEntities]

ALTER TABLE [dbo].[ReactDataEntities]
    DROP COLUMN IF EXISTS [CosmosDocumentId]
