# Déploiement API SmarterASP

Destination : **dgstatsblockapi / site7**, https://sa8techno-001-site7.gtempurl.com/.

Le workflow `.github/workflows/smarterasp.yml` restaure la solution, exécute les tests et produit un paquet Release IIS. Push et PR lancent uniquement ces vérifications. Le job de publication ne s'exécute qu'au déclenchement manuel sur `main`.

## Paramètres GitHub Actions

Configurer dans le dépôt API les quatre secrets suivants avec les valeurs **VSDeploy / Web Deploy** du site7 :

- `SMARTERASP_WEBDEPLOY_SITE_NAME` : nom technique `sa8techno-001-site7`;
- `SMARTERASP_WEBDEPLOY_URL` : URL HTTPS complète du service, avec `/MsDeploy.axd?site=sa8techno-001-site7`;
- `SMARTERASP_WEBDEPLOY_USERNAME` : utilisateur de publication;
- `SMARTERASP_WEBDEPLOY_PASSWORD` : mot de passe de publication.

L'URL exacte du serveur n'est pas déduite de l'URL publique du site ou du serveur FTP. Utiliser celle de VSDeploy. Si SmarterASP affiche un nom technique différent, revoir le ciblage dans `scripts/Deploy-SmarterAsp.ps1` avant le premier lancement.

Les secrets ne sont injectés que dans l'étape de synchronisation. Le script utilise l'API .NET de Web Deploy : aucune interpolation de secrets dans du code PowerShell, ni mot de passe dans les arguments d'un processus. Le certificat HTTPS est validé normalement.

## Premier lancement

1. Publier le workflow sur `main` après revue.
2. Dans GitHub **Actions → SmarterASP API → Run workflow**, sélectionner `main` et `simulate`.
3. Après validation des accès et de la destination, relancer avec `deploy`.

La simulation utilise `WhatIf` et n'active pas `AppOffline`. Le déploiement réel utilise `AppOffline` pendant la synchronisation des fichiers. `DoNotDelete` préserve les fichiers absents du paquet; les fichiers de même nom sont remplacés, dont `web.config` et `appsettings.Production.json`. Sauvegarder les configurations serveur existantes avant le premier transfert. Ne pas placer de secrets manuellement dans ces fichiers remplacés.

Web Deploy 3.6 est installé sur le runner Windows si nécessaire. Le site doit accepter les publications Web Deploy et disposer du runtime/hébergement ASP.NET Core **10**, car le paquet est framework-dependent. Le script ne change pas la version .NET de l'hébergement.

## Configuration de production

Le fichier `appsettings.Production.json` fournit l'origine CORS HTTPS du site6, nécessaire au démarrage. La publication génère `web.config` avec `ASPNETCORE_ENVIRONMENT=Production`. Les secrets de connexion SQL seront configurés côté serveur lors de l'implémentation de la persistance applicative.

L'API actuelle ne raccorde pas encore le DbContext aux endpoints. `/health` vérifie son fonctionnement HTTP, pas la connexion SQL. Aucune migration, création de compte SQL, restauration ou modification de BD n'est exécutée par ce workflow. Les routes `/api/v1/statblocks` restent à implémenter dans l'épic 6.

## Validation et retour arrière

Validation locale : cinq tests, publication Release IIS, démarrage du paquet en Production, `/health` à 200, CORS autorisant le site6 et refusant une autre origine. La connexion Web Deploy, le certificat distant et l'exécution IIS restent à valider en simulation puis sur l'hébergement.

Les artefacts de build sont conservés sept jours. Avant le premier déploiement, sauvegarder les fichiers existants dans SmarterASP. Pour un retour arrière ultérieur, publier une version revue de `main` (par exemple un revert) et relancer le workflow; cela n'annule jamais des migrations SQL. Aucun rollback automatique n'est configuré.

Sources : [publication GitHub Actions SmarterASP](https://www.smarterasp.net/support/kb/a2275/how-to-use-github-actions-to-auto-publish-your-website-file-to-our-server_.aspx), [options de synchronisation Web Deploy](https://learn.microsoft.com/en-us/dotnet/api/microsoft.web.deployment.deploymentsyncoptions?view=iis-dotnet), [authentification Web Deploy](https://learn.microsoft.com/en-us/dotnet/api/microsoft.web.deployment.deploymentbaseoptions?view=iis-dotnet).
