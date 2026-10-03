# DunorGames Statblocks API

API REST ASP.NET Core 10 et persistance SQL Server pour les Stat Blocks DunorGames.

Le site React est maintenu séparément dans `dg-statsblock`. Le contrat d’échange est défini ici et constitue la source de vérité pour le client Web.

## Contenu

- `src/DunorGames.WebApi` : hôte HTTP et configuration;
- `src/DunorGames.Business` : services, validation et DTO;
- `src/DunorGames.Data` : modèles, EF Core, SQL Server et migrations;
- `docs/openapi` : contrat HTTP versionné;
- `src/DunorGames.Data/Script SQL` : scripts EF par transition;
- `docs/sql` : anciens scripts idempotents conservés comme historique.

## Développement local

Voir [Configuration SQL](docs/CONFIGURATION_SQL.md) pour les fichiers ignorés,
la création de la base LocalDB et les paramètres à remplir sur SmarterASP.

```powershell
dotnet restore DunorGames.slnx
dotnet test DunorGames.slnx --no-restore
```

## Base de données

La migration initiale crée `GameSystems`, `Statblocks` et `StatblockTags`. Les scripts EF se trouvent dans `src/DunorGames.Data/Script SQL/`.

Les migrations sont toujours appliquées comme une étape explicite de déploiement; l’API ne les exécute jamais automatiquement au démarrage.

## Déploiement

L’API cible le site SmarterASP `dgstatsblockapi`. Chaque push sur `main` déploie après les tests. Le fichier `appsettings.Production.json`, installé séparément sur le serveur, est préservé et n'entre jamais dans un artefact CI.
