-- Provenance for rows copied out of cosmos by the migration endpoints: the id of the source document.
-- Null for every row the dispatchers write, which is why the unique index below is filtered.
-- Guarded per object, the way 0002 is: a missing ReactDataEntities must fail loudly rather than journal
-- this script as applied while leaving the column uncreated.
IF COL_LENGTH(N'[dbo].[ReactDataEntities]', N'CosmosDocumentId') IS NULL
BEGIN
    ALTER TABLE [dbo].[ReactDataEntities]
        ADD [CosmosDocumentId] VARCHAR(128) NULL
END
GO
-- Separate batch: the index cannot reference a column added in the batch above.
-- Unique, so a concurrent second migration pass is rejected by the database rather than relying on the
-- insert's own NOT EXISTS check winning a race. Entity type leads the key because document ids are only
-- unique within a container.
IF NOT EXISTS (SELECT 1
               FROM sys.indexes
               WHERE [name] = N'UX_ReactDataEntities_EntityType_CosmosDocumentId'
                 AND [object_id] = OBJECT_ID(N'[dbo].[ReactDataEntities]'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [UX_ReactDataEntities_EntityType_CosmosDocumentId]
        ON [dbo].[ReactDataEntities]([EntityType], [CosmosDocumentId])
        WHERE [CosmosDocumentId] IS NOT NULL
END
