BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005012140_AddDaggerheartReferenceValues'
)
BEGIN
    CREATE TABLE [ReferenceValues] (
        [SystemCode] nvarchar(32) NOT NULL,
        [Category] nvarchar(40) NOT NULL,
        [Code] nvarchar(40) NOT NULL,
        [DisplayName] nvarchar(80) NOT NULL,
        [SortOrder] int NOT NULL,
        [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
        CONSTRAINT [PK_ReferenceValues] PRIMARY KEY ([SystemCode], [Category], [Code]),
        CONSTRAINT [FK_ReferenceValues_GameSystems_SystemCode] FOREIGN KEY ([SystemCode]) REFERENCES [GameSystems] ([Code]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005012140_AddDaggerheartReferenceValues'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Category', N'Code', N'SystemCode', N'DisplayName', N'IsActive', N'SortOrder') AND [object_id] = OBJECT_ID(N'[ReferenceValues]'))
        SET IDENTITY_INSERT [ReferenceValues] ON;
    EXEC(N'INSERT INTO [ReferenceValues] ([Category], [Code], [SystemCode], [DisplayName], [IsActive], [SortOrder])
    VALUES (N''adversaryType'', N''bruiser'', N''daggerheart'', N''Bruiser'', CAST(1 AS bit), 1),
    (N''adversaryType'', N''horde'', N''daggerheart'', N''Horde'', CAST(1 AS bit), 2),
    (N''adversaryType'', N''leader'', N''daggerheart'', N''Leader'', CAST(1 AS bit), 3),
    (N''adversaryType'', N''minion'', N''daggerheart'', N''Minion'', CAST(1 AS bit), 4),
    (N''adversaryType'', N''ranged'', N''daggerheart'', N''Ranged'', CAST(1 AS bit), 5),
    (N''adversaryType'', N''skulk'', N''daggerheart'', N''Skulk'', CAST(1 AS bit), 6),
    (N''adversaryType'', N''social'', N''daggerheart'', N''Social'', CAST(1 AS bit), 7),
    (N''adversaryType'', N''solo'', N''daggerheart'', N''Solo'', CAST(1 AS bit), 8),
    (N''adversaryType'', N''standard'', N''daggerheart'', N''Standard'', CAST(1 AS bit), 9),
    (N''adversaryType'', N''support'', N''daggerheart'', N''Support'', CAST(1 AS bit), 10),
    (N''attackRange'', N''close'', N''daggerheart'', N''Close'', CAST(1 AS bit), 3),
    (N''attackRange'', N''far'', N''daggerheart'', N''Far'', CAST(1 AS bit), 4),
    (N''attackRange'', N''melee'', N''daggerheart'', N''Melee'', CAST(1 AS bit), 1),
    (N''attackRange'', N''very close'', N''daggerheart'', N''Very Close'', CAST(1 AS bit), 2),
    (N''attackRange'', N''very far'', N''daggerheart'', N''Very Far'', CAST(1 AS bit), 5),
    (N''damageDie'', N''10'', N''daggerheart'', N''d10'', CAST(1 AS bit), 4),
    (N''damageDie'', N''12'', N''daggerheart'', N''d12'', CAST(1 AS bit), 5),
    (N''damageDie'', N''20'', N''daggerheart'', N''d20'', CAST(1 AS bit), 6),
    (N''damageDie'', N''4'', N''daggerheart'', N''d4'', CAST(1 AS bit), 1),
    (N''damageDie'', N''6'', N''daggerheart'', N''d6'', CAST(1 AS bit), 2),
    (N''damageDie'', N''8'', N''daggerheart'', N''d8'', CAST(1 AS bit), 3),
    (N''damageType'', N''magical'', N''daggerheart'', N''Magical'', CAST(1 AS bit), 2),
    (N''damageType'', N''physical'', N''daggerheart'', N''Physical'', CAST(1 AS bit), 1),
    (N''tier'', N''1'', N''daggerheart'', N''Tier 1'', CAST(1 AS bit), 1),
    (N''tier'', N''2'', N''daggerheart'', N''Tier 2'', CAST(1 AS bit), 2),
    (N''tier'', N''3'', N''daggerheart'', N''Tier 3'', CAST(1 AS bit), 3),
    (N''tier'', N''4'', N''daggerheart'', N''Tier 4'', CAST(1 AS bit), 4)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Category', N'Code', N'SystemCode', N'DisplayName', N'IsActive', N'SortOrder') AND [object_id] = OBJECT_ID(N'[ReferenceValues]'))
        SET IDENTITY_INSERT [ReferenceValues] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005012140_AddDaggerheartReferenceValues'
)
BEGIN
    CREATE INDEX [IX_ReferenceValues_SystemCode_Category_SortOrder] ON [ReferenceValues] ([SystemCode], [Category], [SortOrder]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005012140_AddDaggerheartReferenceValues'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261005012140_AddDaggerheartReferenceValues', N'10.0.0');
END;

COMMIT;
GO

