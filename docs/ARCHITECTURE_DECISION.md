# DunorGames — Décision d’architecture et stack

**Décision :** 2026-08-21  
**Statut :** Acceptée pour le MVP

## Architecture

```text
Navigateur
  ↓ HTTPS
DunorGames.Web (React / TypeScript)
  ↓ HTTPS + OpenAPI
DunorGames.Api (ASP.NET Core 10)
  ↓ EF Core 10
SQL Server
```

Le Web, l’API REST et la base de données constituent trois tiers distincts. La séparation reste valable même si le Web et l’API sont temporairement déployés sous le même site IIS pendant les premières phases.

## Choix retenus

| Domaine | Choix | Justification |
| --- | --- | --- |
| Application Web | React + TypeScript, construit avec Vite et déployé comme fichiers statiques. | Excellent contrôle HTML/CSS pour un SB, aperçu immédiat et aucune dépendance Node.js à l’exécution. |
| Styles | CSS natif structuré par composants, tokens CSS DunorGames et feuilles `@media print`. | Le rendu imprimé doit être déterministe ; aucune bibliothèque d’interface ne doit imposer ses styles aux SB. |
| API | ASP.NET Core 10 LTS, contrôleurs REST. | Compatible IIS/Smarter ASP ; support .NET 10 jusqu’au 14 novembre 2028. |
| Contrat | OpenAPI 3.1 généré par l’API ; client TypeScript généré dans le Web. | Une frontière Web/API explicite, testable et versionnée. |
| Persistance | EF Core 10, migrations versionnées. | Écosystème .NET standard et migrations reproductibles. |
| Base de données | SQL Server. | Choix le plus direct pour IIS/ASP.NET et l’hébergement Windows ; PostgreSQL reste une option de remplacement documentée. |
| Authentification | ASP.NET Core Identity, cookies sécurisés HttpOnly et anti-forgery. | Pas de fournisseur externe requis pour le MVP ; adapté à une API sous le même domaine parent. |
| PNG | Rasterisation côté navigateur à haute densité. | Découple l’export PNG des limites de processus serveur ; validation détaillée à l’épic 4. |
| Tests | xUnit pour l’API, Playwright pour les parcours et les régressions visuelles. | Les régressions de rendu sont détectées avant livraison. |

## Conventions de solution

```text
/src
  /DunorGames.Web          React / TypeScript
  /DunorGames.Api          ASP.NET Core REST
  /DunorGames.Domain       Modèle métier et règles sans dépendance HTTP/EF
  /DunorGames.Infrastructure EF Core, SQL Server et accès externes
  /DunorGames.Contracts    DTO et génération OpenAPI
/tests
  /DunorGames.Api.Tests
  /DunorGames.Web.E2E
/docs
```

- Les templates DH, DS, DC20 et D&D vivent côté Web comme composants de rendu indépendants des formulaires.
- Les données de SB sont versionnées et validées côté client **et** côté API.
- Les migrations ne sont jamais appliquées automatiquement au démarrage en production : elles passent par une étape de déploiement explicite.
- Configuration et secrets sont fournis par environnement, jamais committés.

## Déploiement cible

- `app.<domaine>` : SPA React statique.
- `api.<domaine>` : API ASP.NET Core sous IIS.
- SQL Server : instance Smarter ASP dédiée au compte.
- CORS : uniquement l’origine `app.<domaine>`, avec credentials nécessaires.
- HTTPS obligatoire ; cookies `Secure`, `HttpOnly` et stratégie `SameSite` adaptée aux sous-domaines.

Si le forfait ne permet pas deux sites, l’API est exposée sous `/api` par une application ASP.NET Core qui sert aussi le bundle Web. Les projets et le contrat REST restent séparés.

## Décisions différées

- Migration vers PostgreSQL seulement si le forfait ou l’exploitation le justifie.
- Génération PNG serveur seulement après preuve que les processus nécessaires sont autorisés sur le forfait.
- Choix du fournisseur externe d’identité seulement si des exigences SSO/fédération apparaissent.
- Mise en cache, file d’attente et stockage d’objets lorsque les besoins réels le justifieront.

## Validation

Smarter ASP annonce l’hébergement ASP.NET Core/.NET 10, SQL Server, PostgreSQL, IIS et Git/Web Deploy. .NET 10 est supporté jusqu’au 14 novembre 2028. ASP.NET Core propose la génération OpenAPI intégrée et EF Core maintient le provider SQL Server.

## Sources

- [Smarter ASP — ASP.NET Core Hosting](https://www.smarterasp.net/asp.net_core_hosting)
- [Cycle de vie .NET — Microsoft](https://learn.microsoft.com/en-us/lifecycle/products/microsoft-net-and-net-core)
- [OpenAPI dans ASP.NET Core — Microsoft](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/overview?view=aspnetcore-10.0)
- [Provider SQL Server pour EF Core — Microsoft](https://learn.microsoft.com/en-us/ef/core/providers/sql-server/)
