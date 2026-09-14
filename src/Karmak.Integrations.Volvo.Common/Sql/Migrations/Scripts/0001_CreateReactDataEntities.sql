IF OBJECT_ID(N'[dbo].[ReactDataEntities]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ReactDataEntities]
    (
        [Id]         INT IDENTITY(1,1) PRIMARY KEY,
        [EntityType] VARCHAR(32) NOT NULL,
        [EntityId]   VARCHAR(128) NULL,
        [CreatedOn]  DATETIME2 NOT NULL,
        [KAN]        VARCHAR(32) NOT NULL,
        [PACode]     VARCHAR(32) NOT NULL,
        [JsonData]   JSON NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_ReactDataEntities_PACode_EntityId]
        ON [dbo].[ReactDataEntities]([PACode], [EntityId]) 
END
