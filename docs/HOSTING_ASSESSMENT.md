# DunorGames — Évaluation de l’hébergement Smarter ASP

## Conclusion

L’hébergement public Smarter ASP est compatible avec l’architecture trois tiers visée : application Web, API REST ASP.NET Core et base relationnelle. La documentation publique annonce Windows Server 2022, ASP.NET Core/.NET 10, SQL Server, PostgreSQL, SSL et plusieurs voies de déploiement.

Cette évaluation confirme la faisabilité. Les restrictions propres au forfait déjà détenu doivent être relevées dans le panneau Smarter ASP avant de figer la stack à la tâche `1.3`.

## Capacités annoncées

| Besoin DunorGames | Capacité annoncée | Incidence |
| --- | --- | --- |
| Web et API REST | ASP.NET Core avec .NET 10, 9, 8 et antérieurs sur Windows Server 2022. | Web et API ASP.NET Core sont hébergeables. |
| Base relationnelle | SQL Server et PostgreSQL, avec accès distant et outils de gestion. | EF Core est approprié ; aucun moteur n’est imposé avant `1.3`. |
| HTTPS | Certificats Let’s Encrypt gratuits. | Web et API doivent être HTTPS dès la préproduction. |
| Déploiement | Git/GitHub, Visual Studio/Web Deploy et FTP/FTPS. | Déploiement reproductible possible. |
| Assets web | Fichiers statiques ASP.NET Core. | Polices et assets DunorGames peuvent être livrés localement. |
| Données | Sauvegarde/restauration annoncées pour SQL Server et PostgreSQL. | Les migrations restent versionnées et les sauvegardes de production seront testées. |

## Contraintes et décisions recommandées

### Topologie

- Déployer le Web et l’API comme deux applications/sites distincts si le forfait le permet.
- Sinon, déployer temporairement une application ASP.NET Core unique qui sert le Web et expose `/api`, tout en conservant des projets et un contrat API séparés.
- Prévoir un CORS restrictif seulement si les origines sont séparées.

### Base de données

- **SQL Server est recommandé par défaut** : il est le choix le plus direct pour IIS/ASP.NET et l’exploitation Windows.
- PostgreSQL reste un choix valide si l’infrastructure existante ou les préférences d’exploitation le justifient.
- La décision est reportée à `1.3`, après vérification du quota et du moteur réellement disponibles dans le forfait.

### Export PNG

- Le rendu source sera HTML/CSS, donc identique pour aperçu et export PNG.
- Ne pas dépendre au départ d’un Chromium/Playwright serveur sur un hébergement mutualisé : processus enfants, mémoire et binaires peuvent être limités.
- Prévoir le PNG côté navigateur. Un export serveur ne sera ajouté qu'après validation de l'environnement ou via un service dédié.

### Fichiers et secrets

- Les polices et assets versionnés sont servis comme fichiers statiques.
- Les futurs uploads doivent être stockés hors du répertoire public, validés et servis par une route contrôlée.
- Secrets et chaînes de connexion restent hors dépôt et hors client.

## Vérifications à effectuer dans le compte

| Vérification | Impact avant `1.3` |
| --- | --- |
| Nombre de sites/applications et sous-domaines. | Confirme la séparation Web/API. |
| Version .NET sélectionnable et mode de publication. | Fixe la cible de l’API. |
| Moteur, version, quota et accès distant de la base incluse. | Confirme SQL Server ou PostgreSQL. |
| Tâches planifiées, processus de longue durée, exécutables autorisés et mémoire. | Détermine la faisabilité d’un export serveur. |
| Stockage persistant, quota et sauvegardes pour uploads/exportations. | Définit la stratégie de fichiers. |
| SSL, DNS et variables de configuration par environnement. | Prépare préproduction et production. |

## Sortie de la tâche 1.2

L’architecture est faisable sur Smarter ASP. La tâche `1.3` peut sélectionner une stack, sous réserve des vérifications de compte consignées ci-dessus.

## Sources

- [ASP.NET Core Hosting — SmarterASP.NET](https://www.smarterasp.net/asp.net_core_hosting)
- [Hosting features and deployment FAQ — SmarterASP.NET](https://www.smarterasp.net/index)
- [Serving static files in ASP.NET Core — SmarterASP.NET](https://www.smarterasp.net/support/kb/a2220/how-to-serve-static-files-in-asp_net-core.aspx)
