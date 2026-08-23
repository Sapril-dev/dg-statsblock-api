# Schéma relationnel initial

La persistance DunorGames utilise SQL Server et EF Core 10. La migration initiale est [`InitialStatblocks`](../src/DunorGames.Infrastructure/Persistence/Migrations/20260823055031_InitialStatblocks.cs); son script SQL idempotent est [`sql/001_initial_statblocks.sql`](sql/001_initial_statblocks.sql).

## Tables

| Table | Rôle |
| --- | --- |
| `GameSystems` | Référentiel immuable des cinq systèmes pris en charge. |
| `Statblocks` | Données communes, provenance, statut, contenu JSON du système et contrôle de concurrence. |
| `StatblockTags` | Tags d'un SB, normalisés dans une table enfant. |

`GameSystems` est alimentée par la migration avec `dungeonsAndDragons`, `talesOfTheValiant`, `daggerheart`, `drawSteel` et `dc20`.

## `Statblocks`

La table conserve les attributs communs recherchables (`OwnerId`, système, nom, statut et dates) dans des colonnes SQL. `SystemDataJson` garde le document spécifique à DH, DS, DC20 ou D&D/TotV: ce choix respecte les schémas distincts définis à la tâche 2.2 sans dupliquer des colonnes rarement communes.

Contraintes applicatives et SQL :

- `System` et `SourceSystem` référencent `GameSystems`;
- `SourceStatblockId` peut référencer un SB source lors d'une conversion;
- `Origin` est limité à `manual`, `imported` ou `converted`;
- `Status` est limité à `draft`, `published` ou `archived`;
- `SystemDataJson` doit contenir du JSON valide;
- `RowVersion` est un `rowversion` SQL Server. Il sera converti en ETag HTTP par les endpoints de la tâche 6.2.

Les index couvrent les listes d'un propriétaire par système/statut et leur tri par date de modification. `StatblockTags` utilise une clé composée `(StatblockId, Tag)`, ce qui empêche un tag dupliqué dans un même SB.

`OwnerId` est déjà un `Guid`. Lors de l'ajout d'ASP.NET Core Identity, les utilisateurs utiliseront le même type de clé et une migration ajoutera la contrainte de clé étrangère. Elle est volontairement absente aujourd'hui, car les tables d'identité ne font pas encore partie du MVP.

## Exécution

Le script SQL versionné est idempotent et peut être exécuté par le compte de déploiement contre une base vide. Il enregistre la migration dans `__EFMigrationsHistory`; les migrations ultérieures pourront ainsi suivre le même mécanisme.

En développement, EF Core peut appliquer la migration uniquement avec une chaîne de connexion explicitement fournie :

```powershell
$env:DUNORGAMES_CONNECTION_STRING = "Server=...;Database=...;User Id=...;Password=...;TrustServerCertificate=True"
.\.tools\dotnet-ef database update --project src\DunorGames.Infrastructure --startup-project src\DunorGames.Api --context DunorGamesDbContext
```

En production, l'exécution reste une étape de déploiement explicite. L'application API ne lance jamais automatiquement les migrations au démarrage.
