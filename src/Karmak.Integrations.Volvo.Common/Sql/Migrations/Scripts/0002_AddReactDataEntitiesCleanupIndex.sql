-- Supports the retention cleanup, which filters on [EntityType] + [CreatedOn].
-- Guarded on the index only, the way 0001 guards on its table: a missing ReactDataEntities must fail
-- loudly rather than journal this script as applied while leaving the index uncreated.
IF NOT EXISTS (SELECT 1
               FROM sys.indexes
               WHERE [name] = N'IX_ReactDataEntities_EntityType_CreatedOn'
                 AND [object_id] = OBJECT_ID(N'[dbo].[ReactDataEntities]'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_ReactDataEntities_EntityType_CreatedOn]
        ON [dbo].[ReactDataEntities]([EntityType], [CreatedOn])
END
