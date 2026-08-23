IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823055031_InitialStatblocks'
)
BEGIN
    CREATE TABLE [GameSystems] (
        [Code] nvarchar(32) NOT NULL,
        [DisplayName] nvarchar(80) NOT NULL,
        [SortOrder] int NOT NULL,
        CONSTRAINT [PK_GameSystems] PRIMARY KEY ([Code])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823055031_InitialStatblocks'
)
BEGIN
    CREATE TABLE [Statblocks] (
        [Id] uniqueidentifier NOT NULL,
        [OwnerId] uniqueidentifier NOT NULL,
        [SchemaVersion] nvarchar(16) NOT NULL,
        [System] nvarchar(32) NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Subtitle] nvarchar(200) NULL,
        [Description] nvarchar(max) NULL,
        [Origin] nvarchar(16) NOT NULL,
        [SourceSystem] nvarchar(32) NULL,
        [SourceStatblockId] uniqueidentifier NULL,
        [SourceReference] nvarchar(1000) NULL,
        [Status] nvarchar(16) NOT NULL DEFAULT N'draft',
        [Notes] nvarchar(max) NULL,
        [SystemDataJson] nvarchar(max) NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL DEFAULT (CONVERT(datetimeoffset, SYSUTCDATETIME())),
        [UpdatedAt] datetimeoffset NOT NULL DEFAULT (CONVERT(datetimeoffset, SYSUTCDATETIME())),
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_Statblocks] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_Statblocks_Origin] CHECK ([Origin] IN ('manual', 'imported', 'converted')),
        CONSTRAINT [CK_Statblocks_SchemaVersion] CHECK (LEN([SchemaVersion]) > 0),
        CONSTRAINT [CK_Statblocks_Status] CHECK ([Status] IN ('draft', 'published', 'archived')),
        CONSTRAINT [CK_Statblocks_SystemDataJson] CHECK (ISJSON([SystemDataJson]) = 1),
        CONSTRAINT [FK_Statblocks_GameSystems_SourceSystem] FOREIGN KEY ([SourceSystem]) REFERENCES [GameSystems] ([Code]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Statblocks_GameSystems_System] FOREIGN KEY ([System]) REFERENCES [GameSystems] ([Code]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Statblocks_Statblocks_SourceStatblockId] FOREIGN KEY ([SourceStatblockId]) REFERENCES [Statblocks] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823055031_InitialStatblocks'
)
BEGIN
    CREATE TABLE [StatblockTags] (
        [StatblockId] uniqueidentifier NOT NULL,
        [Tag] nvarchar(64) NOT NULL,
        CONSTRAINT [PK_StatblockTags] PRIMARY KEY ([StatblockId], [Tag]),
        CONSTRAINT [FK_StatblockTags_Statblocks_StatblockId] FOREIGN KEY ([StatblockId]) REFERENCES [Statblocks] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823055031_InitialStatblocks'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Code', N'DisplayName', N'SortOrder') AND [object_id] = OBJECT_ID(N'[GameSystems]'))
        SET IDENTITY_INSERT [GameSystems] ON;
    EXEC(N'INSERT INTO [GameSystems] ([Code], [DisplayName], [SortOrder])
    VALUES (N''daggerheart'', N''Daggerheart'', 3),
    (N''dc20'', N''DC20'', 5),
    (N''drawSteel'', N''Draw Steel'', 4),
    (N''dungeonsAndDragons'', N''Dungeons & Dragons'', 1),
    (N''talesOfTheValiant'', N''Tales of the Valiant'', 2)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Code', N'DisplayName', N'SortOrder') AND [object_id] = OBJECT_ID(N'[GameSystems]'))
        SET IDENTITY_INSERT [GameSystems] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823055031_InitialStatblocks'
)
BEGIN
    CREATE INDEX [IX_Statblocks_OwnerId_System_Status] ON [Statblocks] ([OwnerId], [System], [Status]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823055031_InitialStatblocks'
)
BEGIN
    CREATE INDEX [IX_Statblocks_OwnerId_UpdatedAt] ON [Statblocks] ([OwnerId], [UpdatedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823055031_InitialStatblocks'
)
BEGIN
    CREATE INDEX [IX_Statblocks_SourceStatblockId] ON [Statblocks] ([SourceStatblockId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823055031_InitialStatblocks'
)
BEGIN
    CREATE INDEX [IX_Statblocks_SourceSystem] ON [Statblocks] ([SourceSystem]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823055031_InitialStatblocks'
)
BEGIN
    CREATE INDEX [IX_Statblocks_System] ON [Statblocks] ([System]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823055031_InitialStatblocks'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260823055031_InitialStatblocks', N'10.0.0');
END;

COMMIT;
GO

