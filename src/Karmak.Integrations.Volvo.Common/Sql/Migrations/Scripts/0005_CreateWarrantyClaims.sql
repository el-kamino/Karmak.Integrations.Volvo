IF OBJECT_ID(N'[dbo].[WarrantyClaims]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[WarrantyClaims]
    (
        [Id]                            INT IDENTITY(1,1) NOT NULL CONSTRAINT [PK_WarrantyClaims] PRIMARY KEY,

        -- Identity. ClaimId is the claim's own id and InstanceIdentifier is the dealer
        -- instance it belongs to, which together replace the cosmos id + partition key.
        [ClaimId]                       VARCHAR(128) NOT NULL,
        [InstanceIdentifier]            VARCHAR(128) NOT NULL,
        [BranchIdentifier]              VARCHAR(128) NULL,
        [CorrelationId]                 VARCHAR(128) NULL,
        [IsDeleted]                     BIT NOT NULL CONSTRAINT [DF_WarrantyClaims_IsDeleted] DEFAULT (0),

        -- Columns promoted out of the claim document for searching.
        [CausalPartIdentifier]          VARCHAR(128) NULL,
        [ClaimIdentifier]               VARCHAR(128) NULL,
        [CompanyName]                   NVARCHAR(256) NULL,
        [ClaimTotal]                    DECIMAL(19,4) NULL,
        [RepairOrderCompletedDate]      DATETIME2 NULL,
        [RepairOrderOpenedDate]         DATETIME2 NULL,
        [CustomerIdentifier]            VARCHAR(128) NULL,
        [InvoiceIdentifier]             VARCHAR(128) NULL,
        [Oem]                           NVARCHAR(256) NULL,
        [RepairOrderIdentifier]         VARCHAR(128) NULL,
        [WarrantyRepairOrderIdentifier] VARCHAR(128) NULL,
        [ClaimStatus]                   NVARCHAR(256) NULL,
        [VehicleIdentifier]             VARCHAR(128) NULL,

        [CreatedOn]                     DATETIME2 NOT NULL,
        [UpdatedOn]                     DATETIME2 NOT NULL,

        -- The claim document itself.
        [JsonData]                      JSON NOT NULL
    );

    -- Makes the upsert deterministic and enforces one row per claim per instance.
    CREATE UNIQUE NONCLUSTERED INDEX [UQ_WarrantyClaims_Instance_ClaimId]
        ON [dbo].[WarrantyClaims]([InstanceIdentifier], [ClaimId]);

    -- Covers the repair order lookups the claims service issues on every create and submit.
    CREATE NONCLUSTERED INDEX [IX_WarrantyClaims_Instance_Branch_RepairOrder]
        ON [dbo].[WarrantyClaims]([InstanceIdentifier], [BranchIdentifier], [WarrantyRepairOrderIdentifier])
        INCLUDE ([RepairOrderIdentifier], [CorrelationId], [IsDeleted]);

    -- Covers searches that lead with a claim identifier or the repair order number.
    CREATE NONCLUSTERED INDEX [IX_WarrantyClaims_Instance_ClaimIdentifier]
        ON [dbo].[WarrantyClaims]([InstanceIdentifier], [ClaimIdentifier]);

    CREATE NONCLUSTERED INDEX [IX_WarrantyClaims_Instance_RepairOrderIdentifier]
        ON [dbo].[WarrantyClaims]([InstanceIdentifier], [RepairOrderIdentifier]);
END
