# DunorGames — Conventions de développement

## Branches et changements

- Une tâche du plan correspond à une branche ou une série de commits clairement identifiables.
- Les migrations de base de données sont versionnées ; aucune migration n’est appliquée automatiquement au démarrage en production.

## Configuration et secrets

- `appsettings.json` contient seulement des valeurs sûres par défaut.
- `appsettings.Development.local.json` et `.env*` contiennent les paramètres locaux et ne sont jamais committés.
- `appsettings.Development.example.json` sert de modèle sans secret.
- Production et préproduction reçoivent leurs secrets depuis la configuration Smarter ASP ou le mécanisme de déploiement choisi.

## Web et API

- Le Web consomme le contrat OpenAPI de l’API ; le domaine ne dépend ni d’HTTP ni d’EF Core.
- La validation est appliquée dans le Web pour l’expérience utilisateur et dans l’API comme autorité.
- Toute nouvelle origine Web doit être explicitement ajoutée à la politique CORS.

## Rendu de stats blocks

- Les SB sont rendus avec HTML/CSS, jamais avec des coordonnées raster codées en dur.
- Le même composant sert à l’aperçu et aux exports.
- La largeur imprimée est définie par le template ; la hauteur reste dictée par le contenu.
- Toute évolution d’un template approuvé ajoute ou met à jour un test visuel Playwright.

## Vérification locale minimale

- API : `dotnet test DunorGames.slnx` puis `dotnet run --project src/DunorGames.Api`.
- Web : `npm run lint`, `npm run build`, puis `npm run dev` depuis `src/DunorGames.Web`.
