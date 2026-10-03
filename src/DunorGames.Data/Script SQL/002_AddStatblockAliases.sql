BEGIN TRANSACTION;
ALTER TABLE [Statblocks] ADD [AliasesJson] nvarchar(max) NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260915020741_AddStatblockAliases', N'10.0.0');

COMMIT;
GO

