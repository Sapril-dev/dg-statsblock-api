BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260915020741_AddStatblockAliases'
)
BEGIN
    ALTER TABLE [Statblocks] ADD [AliasesJson] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260915020741_AddStatblockAliases'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260915020741_AddStatblockAliases', N'10.0.0');
END;

COMMIT;
GO

