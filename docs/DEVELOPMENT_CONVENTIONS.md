# DunorGames — Conventions de développement

## Branches et changements

- Une tâche du plan correspond à une branche ou une série de commits clairement identifiables.
- Les migrations de base de données sont versionnées ; aucune migration n’est appliquée automatiquement au démarrage en production.

## Configuration et secrets

- `appsettings.json` contient seulement des valeurs sûres par défaut.
- `appsettings.Development.json`, `appsettings.Production.json`, `appsettings.*.local.json` et `.env*` ne sont jamais committés. Seules les surcharges standard Development/Production sont chargées automatiquement.
- `appsettings.Development.example.json` sert de modèle sans secret.
- Production reçoit ses secrets dans son fichier `appsettings.Production.json` serveur; le pipeline préserve ce fichier. Voir [Configuration SQL](CONFIGURATION_SQL.md).

## Web et API

- Le Web consomme le contrat OpenAPI de l’API. Les services et DTO sont dans Business; les entités et la persistance dans Data; la composition des services dans WebApi.
- La validation est appliquée dans le Web pour l’expérience utilisateur et dans l’API comme autorité. Les brouillons peuvent être incomplets, mais chaque valeur présente doit être valide; la publication exige le schéma système complet.
- Toute nouvelle origine Web doit être explicitement ajoutée à la politique CORS.

## Rendu de stats blocks

- Les SB sont rendus avec HTML/CSS, jamais avec des coordonnées raster codées en dur.
- Le même composant sert à l’aperçu et aux exports.
- La largeur imprimée est définie par le template ; la hauteur reste dictée par le contenu.
- Toute évolution d’un template approuvé ajoute ou met à jour un test visuel Playwright.

## Vérification locale minimale

- API : `dotnet test DunorGames.slnx` puis `dotnet run --project src/DunorGames.WebApi --launch-profile http`.
- Web : `npm run lint`, `npm run build`, puis `npm run dev` depuis `src/DunorGames.Web`.
