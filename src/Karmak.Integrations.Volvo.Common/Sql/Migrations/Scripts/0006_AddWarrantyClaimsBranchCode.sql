-- The branch's code, promoted out of the claim document so a search result can show the branch the
-- way a dealer names it. BranchIdentifier, already promoted, is what a search is scoped by; the code
-- is what a result list displays.
--
-- Guarded per object, the way 0003 is: a missing WarrantyClaims must fail loudly rather than journal
-- this script as applied while leaving the column uncreated.
IF COL_LENGTH(N'[dbo].[WarrantyClaims]', N'BranchCode') IS NULL
BEGIN
    ALTER TABLE [dbo].[WarrantyClaims]
        ADD [BranchCode] VARCHAR(128) NULL
END
GO
-- Separate batch: the update cannot reference a column added in the batch above.
--
-- Backfill from the claim document, which is where the code has been all along. Every row the
-- dispatchers wrote before this script has no code promoted; every row written after it gets one from
-- the upsert. One statement rather than a batched loop, because the runner applies a script inside a
-- single transaction either way, so batching would divide the work without shortening the transaction.
--
-- The path is case sensitive and the document is serialized PascalCase, hence '$.Dealer.Branch.Code'.
-- The cast is what the repository's own reads do: it serves a JsonData held as the native json type
-- and one held as nvarchar alike. A claim with no branch code recorded stays null, which is the
-- truth about it. Guarded by IS NULL so re-running the script after a partial failure resumes rather
-- than rewriting rows that already carry a code.
UPDATE [dbo].[WarrantyClaims]
SET [BranchCode] = JSON_VALUE(CAST([JsonData] AS NVARCHAR(MAX)), '$.Dealer.Branch.Code')
WHERE [BranchCode] IS NULL
