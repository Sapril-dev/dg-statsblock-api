# DunorGames Statblocks API

API REST ASP.NET Core 10 et persistance SQL Server pour les Stat Blocks DunorGames.

Le site React est maintenu séparément dans `dg-statsblock`. Le contrat d’échange est défini ici et constitue la source de vérité pour le client Web.

## Contenu

- `src/DunorGames.Api` : hôte HTTP;
- `src/DunorGames.Domain` : modèle métier sans dépendance d’infrastructure;
- `src/DunorGames.Contracts` : DTO et contrats publics;
- `src/DunorGames.Infrastructure` : EF Core, SQL Server et migrations;
- `docs/openapi` : contrat HTTP versionné;
- `docs/sql` : script SQL Server idempotent de la migration initiale.

## Développement local

```powershell
dotnet restore DunorGames.slnx
dotnet test DunorGames.slnx --no-restore
```

## Base de données

La migration initiale crée `GameSystems`, `Statblocks` et `StatblockTags`. Le script d’amorçage est `docs/sql/001_initial_statblocks.sql`.

Les migrations sont toujours appliquées comme une étape explicite de déploiement; l’API ne les exécute jamais automatiquement au démarrage.

## Déploiement

L’API cible le site SmarterASP `dgstatsblockapi`. Les chaînes de connexion et secrets restent dans la configuration de l’environnement; ils ne doivent jamais être commités.
