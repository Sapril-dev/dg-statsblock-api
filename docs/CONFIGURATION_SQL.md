# Configuration SQL et publication automatisée

## Structure retenue

La solution applicative contient trois projets :

- `DunorGames.WebApi` : contrôleurs, composition des services, configuration et démarrage.
- `DunorGames.Business` : services, DTO et validation des SB.
- `DunorGames.Data` : modèles de données, repository, entités EF, DbContext, `Migrations/` et `Script SQL/`.

Le projet de tests reste séparé. `WebApi` référence `Business` et `Data`;
`Business` référence `Data`. Le DbContext reçoit ses options par injection.
Il ne lit aucune configuration et ne contient pas de `OnConfiguring`.
La factory EF contenant une connexion en dur a été retirée : EF utilise le projet
de démarrage WebApi. Les identifiants des migrations existantes sont conservés.

## Fichiers à remplir

`src/DunorGames.WebApi/appsettings.json` est versionné et contient la forme des
sections et des défauts locaux sans secret. Une seule connexion existe :
`ConnectionStrings:DefaultConnection`, avec repli sur localhost en authentification Windows.

Les surcharges suivantes ne sont pas versionnées :

- `appsettings.Development.json` : LocalDB, base `DunorGamesDb`, authentification Windows, Swagger activé. Aucun mot de passe à remplir.
- `appsettings.Production.json` : remplacer `<serveur>`, `<base>`, `<user>` et `<motdepasse>` par les informations SQL SmarterASP. `<serveur>` est la partie avant `.site4now.net`. Ne pas recopier ce suffixe deux fois.

Les fichiers `appsettings.Development.example.json` et
`appsettings.Production.example.json` sont des modèles versionnés sans secret.
Après un nouveau clone, les copier sans le segment `.example` dans leur nom.
Les fichiers `appsettings.*.local.json` sont ignorés mais ne sont pas chargés
automatiquement : utiliser les deux surcharges standard ci-dessus.

Il n'existe pas encore de section JWT, mot de passe SMTP ou clé API dans cette
application; aucune n'est ajoutée artificiellement.

Ce modèle stocke les secrets en clair dans le fichier de configuration local/serveur,
hors Git. Gitignore ne chiffre pas les fichiers. Ne pas partager le fichier rempli.

## LocalDB

Depuis la racine du dépôt API :

```powershell
dotnet tool restore
$env:ASPNETCORE_ENVIRONMENT = 'Development'
dotnet ef database update -p src/DunorGames.Data -s src/DunorGames.WebApi
dotnet run --project src/DunorGames.WebApi --launch-profile http
```

Le profil `http` définit Development et écoute sur `http://localhost:5169`.
Dans un autre terminal :

```powershell
Invoke-RestMethod http://localhost:5169/api/v1/statblocks
```

Cet appel lit effectivement la base SQL. `/health` vérifie seulement le démarrage HTTP.
Ne jamais exécuter `database update` en production. Le fournisseur InMemory reste
réservé aux tests locaux Playwright; il n'est actif qu'en Development et sur demande explicite.

## SmarterASP et GitHub Actions

Installer une fois `appsettings.Production.json` rempli à côté du `web.config`
du site API. Ne définir ni ASPNETCORE_ENVIRONMENT ni DOTNET_ENVIRONMENT sur le
serveur : ASP.NET Core utilise Production par défaut. Le paquet IIS ne force
aucune variable d'environnement. OutOfProcess reste configuré dans le csproj.

Chaque push sur `main` compile, teste, publie puis contrôle `/health` et la lecture
des SB. Les pull requests compilent/testent sans déployer. Le déclenchement manuel
reste disponible avec `simulate` ou `deploy`. Les publications automatiques et
manuelles partagent le même groupe de concurrence pour ne pas synchroniser le site
simultanément.

La publication CI utilise `ContinuousIntegrationBuild=true` : les surcharges
Development/Production et les modèles sont exclus des artefacts. Le script refuse
un paquet CI contenant `appsettings.Production.json`. `DoNotDelete` préserve le
fichier déjà installé sur SmarterASP. Aucun secret SQL ne transite dans GitHub.
Les quatre secrets Web Deploy existants restent nécessaires à la publication.

Une publication locale Visual Studio/dotnet sans cette propriété CI inclut le
fichier Production local, conformément au modèle choisi. Elle n'est pas nécessaire
pour les publications régulières.

Avant le premier push de cette réorganisation, renseigner le fichier Production
sur le serveur avec la clé `DefaultConnection`. Sans cela, le repli localhost peut
laisser `/health` répondre, mais le contrôle de lecture SQL échouera.

Lors du passage en production réelle, remettre le job `publish` sous la seule
condition `workflow_dispatch`, ou introduire un environnement GitHub avec approbation.

## Scripts SQL

Les deux scripts de `src/DunorGames.Data/Script SQL/` sont générés directement par
EF 10, en UTF-8 BOM et CRLF. Leur corps SQL n'est pas édité. Contrairement aux
anciens scripts idempotents conservés dans `docs/sql`, ce sont des transitions
précises : ne pas les rejouer sur une migration déjà enregistrée.

```powershell
$env:ASPNETCORE_ENVIRONMENT = 'Development'
dotnet ef migrations script 0 20260823055031_InitialStatblocks -o "src/DunorGames.Data/Script SQL/001_InitialStatblocks.sql" -p src/DunorGames.Data -s src/DunorGames.WebApi
dotnet ef migrations script 20260823055031_InitialStatblocks 20260915020741_AddStatblockAliases -o "src/DunorGames.Data/Script SQL/002_AddStatblockAliases.sql" -p src/DunorGames.Data -s src/DunorGames.WebApi
```

En production, contrôler `__EFMigrationsHistory` dans SSMS et exécuter manuellement
uniquement les transitions manquantes. Aucun SQL n'est exécuté par le pipeline ni
par le démarrage de l'application. `HasData` reste limité au référentiel des systèmes;
les données modifiables doivent être initialisées par du SQL idempotent dans une migration.
