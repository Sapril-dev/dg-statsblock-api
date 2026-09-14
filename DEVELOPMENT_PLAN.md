# DunorGames — Plan de développement

## Objectif

Construire une application hébergée sur Smarter ASP permettant de créer, modifier, prévisualiser et exporter des **stats blocks (SB)** pour Daggerheart (DH), Draw Steel (DS), DC20 et D&D. L’application doit aussi permettre de partir d’un SB d’un système pour générer un brouillon dans un autre système, avec révision humaine des règles et de l’équilibrage.

## Principes d’architecture retenus

- Architecture trois tiers : application Web, API REST et base de données distinctes.
- Le rendu des SB est fait en HTML/CSS imprimable : largeur définie, hauteur naturelle selon le contenu.
- Les données, les règles de conversion et les templates de rendu restent indépendants.
- Le déploiement cible Smarter ASP / IIS. Les technologies précises sont validées avant le démarrage : application Web, version d’ASP.NET Core pour l’API, et SQL Server ou PostgreSQL selon l’offre de production disponible.
aperçu afin’aperçu, afin d’éviter les divergences visuelles.

## Convention de suivi

- Une tâche est identifiée par `ÉPIC.TÂCHE` (par exemple `6.2`).
- États : `À faire`, `En cours`, `Bloquée`, `Terminée`, `Retirée`.
- Une tâche ne passe à `Terminée` qu’avec son critère de sortie rempli et une vérification adaptée.
- Pour poursuivre, utiliser simplement : `next task 6.2`.

---

## Épic 1 — Cadrage et fondations du projet

| ID | Tâche | État | Critère de sortie |
| --- | --- | --- | --- |
| 1.1 | Documenter le périmètre fonctionnel initial, les systèmes pris en charge et les exclusions. | Terminée | [Périmètre MVP](docs/MVP_SCOPE.md) créé. |
| 1.2 | Vérifier les capacités exactes de l’hébergement Smarter ASP : IIS, .NET, base SQL Server/PostgreSQL, tâches planifiées, stockage de fichiers et limites d’export. | Terminée | [Évaluation d’hébergement](docs/HOSTING_ASSESSMENT.md) créée ; vérifications de compte consignées. |
| 1.3 | Choisir et documenter la stack : application Web, API REST, ORM/migrations, base de données, génération PDF et stratégie d’authentification. | Terminée | [Décision d’architecture](docs/ARCHITECTURE_DECISION.md) créée. |
| 1.4 | Créer la structure de solution et les conventions de développement, de configuration et de secrets. | Terminée | Structure .NET/React, conventions et configuration d’exemple créées. |

## Épic 2 — Modèle de données et contrats

| ID | Tâche | État | Critère de sortie |
| --- | --- | --- | --- |
| 2.1 | Définir le modèle canonique minimal commun aux SB : identité, description, tags, source, métadonnées et contenu spécifique au système. | Terminée | [Modèle canonique](docs/STATBLOCK_MODEL.md), schéma JSON et exemple DH créés. |
| 2.2 | Définir les schémas DH, DS, DC20 et D&D, en conservant les champs et terminologies propres à chaque système. | Terminée | [Schémas système](docs/SYSTEM_SCHEMAS.md) et exemples DH, DS, DC20, D&D créés. |
| 2.3 | Définir le contrat API REST et la stratégie de versionnement. | Fait | Contrat v1, DTO lecture/écriture distincts, ETag et erreurs RFC 9457. |
| 2.4 | Concevoir le schéma relationnel, les migrations et les données de référence. | Fait | Migration SQL Server, référentiel des systèmes et script SQL idempotent. |

## Épic 3 — Moteur de rendu et système visuel

| ID | Tâche | État | Critère de sortie |
| --- | --- | --- | --- |
| 3.1 | Intégrer les assets DunorGames, Cardo, Montserrat et les règles de licence dans l’application Web. | Terminée | Les polices et assets se chargent localement et de façon reproductible. |
| 3.2 | Créer les tokens CSS : palette, typographie, bordures, espacements et styles d’impression. | Terminée | Aucun rendu ne dépend de valeurs dispersées. |
| 3.3 | Construire le composant HTML/CSS DH avec largeur fixe et hauteur automatique. | Terminée | Le composant rend un SB DH structuré à partir de données. |
| 3.4 | Finaliser Street Bandit comme référence visuelle (« golden sample ») du template DH. | Terminée | Comparaison visuelle approuvée et capture de référence conservée. |
| 3.5 | Mettre en place les tests de régression visuelle du template DH. | Terminée | Toute variation significative est détectée automatiquement. |

## Épic 4 — Aperçu et export

| ID | Tâche | État | Critère de sortie |
| --- | --- | --- | --- |
| 4.1 | Créer une page d’aperçu isolée pour charger les exemples JSON et afficher le SB. | Terminée | Street Bandit est visualisable sans éditeur complet. |
| 4.2 | Produire l’export PDF à partir du rendu HTML/CSS et vérifier le résultat imprimé. | Retirée | Hors périmètre : l’intégration Affinity utilise le PNG haute résolution. |
| 4.3 | Produire l’export PNG haute résolution. | Terminée | PNG au format et à la résolution configurables. |
| 4.4 | Ajouter les contrôles de débordement, lisibilité et erreurs de contenu avant export. | Terminée | Les anomalies sont détaillées avant export; l’utilisateur peut ensuite choisir « Exporter quand même ». |

## Épic 5 — Éditeur Web MVP

| ID | Tâche | État | Critère de sortie |
| --- | --- | --- | --- |
| 5.1 | Créer la navigation, la liste de SB et le flux de création d’un SB. | Terminée | Un utilisateur peut créer, retrouver et ouvrir un brouillon Daggerheart conservé localement. |
| 5.2 | Construire le formulaire d’édition DH avec validation progressive. | Terminée | Tous les champs du schéma DH sont éditables, validés au champ et sauvegardés localement. |
| 5.3 | Connecter le formulaire à l’aperçu en direct. | Terminée | Les modifications visuelles alimentent immédiatement le rendu Daggerheart sans rechargement ni changement de route. |
| 5.4 | Ajouter la gestion des sections, capacités et mises en emphase (gras/italique). | Terminée | Les capacités sont ajoutées, retirées, ordonnées, déplacées entre sections et rendues avec une emphase contrôlée. |

## Épic 6 — API REST et persistance

| ID | Tâche | État | Critère de sortie |
| --- | --- | --- | --- |
| 6.1 | Initialiser l’API REST, sa configuration par environnement, sa journalisation et sa documentation OpenAPI. | Terminée | API démarrable localement avec endpoint de santé, document OpenAPI 3.1 et interface Swagger configurable. |
| 6.2 | Implémenter les endpoints de gestion des SB : créer, lire, modifier, supprimer et lister. | À faire | Le Web consomme l’API pour le cycle complet d’un SB. |
| 6.3 | Implémenter les migrations, le dépôt de données et les règles de concurrence. | À faire | Les données persistent correctement et les conflits sont gérés. |
| 6.4 | Mettre en place la validation serveur et les réponses d’erreurs cohérentes. | À faire | Les données invalides ne sont jamais persistées. |

## Épic 7 — Identité, sécurité et bibliothèque

| ID | Tâche | État | Critère de sortie |
| --- | --- | --- | --- |
| 7.1 | Choisir le modèle d’identité compatible avec l’hébergement et définir les rôles. | À faire | Décision documentée et flux de connexion défini. |
| 7.2 | Ajouter l’authentification, l’autorisation et la protection des API. | À faire | Un utilisateur ne peut accéder qu’à ses données autorisées. |
| 7.3 | Créer la bibliothèque : recherche, filtres, duplication, archivage et métadonnées. | À faire | Les SB sauvegardés sont facilement retrouvables. |

## Épic 8 — Templates des autres systèmes

| ID | Tâche | État | Critère de sortie |
| --- | --- | --- | --- |
| 8.1 | Construire et valider le template Draw Steel. | À faire | Un SB DS de référence est approuvé. |
| 8.2 | Construire et valider le template DC20. | À faire | Un SB DC20 de référence est approuvé. |
| 8.3 | Construire et valider le template D&D / Tales of the Valiant. | À faire | Un SB D&D/TotV de référence est approuvé. |
| 8.4 | Ajouter les formulaires d’édition pour DS, DC20 et D&D/TotV. | À faire | Les quatre systèmes sont créables et exportables. |

## Épic 9 — Conversion entre systèmes

| ID | Tâche | État | Critère de sortie |
| --- | --- | --- | --- |
| 9.1 | Définir les correspondances de données et les limites de conversion pour chaque paire de systèmes. | À faire | Matrice de conversion et cas non convertibles documentés. |
| 9.2 | Implémenter le brouillon de conversion à partir d’un SB DH. | À faire | DH vers DS, DC20 et D&D crée un brouillon révisable. |
| 9.3 | Créer l’interface de révision des champs non mappés, hypothèses et avertissements. | À faire | Aucune conversion ne se présente comme équilibrée sans intervention humaine. |
| 9.4 | Étendre la conversion aux autres systèmes source. | À faire | Toutes les conversions ciblées du MVP sont disponibles. |

## Épic 10 — Qualité, déploiement et exploitation

| ID | Tâche | État | Critère de sortie |
| --- | --- | --- | --- |
| 10.1 | Ajouter les tests unitaires, d’intégration et de régression visuelle prioritaires. | À faire | Pipeline de tests exécuté avant livraison. |
| 10.2 | Préparer les environnements développement, préproduction et production Smarter ASP. | Terminée | Déploiements Web/API documentés et reproductibles; publications SmarterASP et contrôles publics réussis. Voir [déploiement API](docs/DEPLOYMENT.md). |
| 10.3 | Configurer les migrations de production, sauvegardes, logs et surveillance. | À faire | Exploitation et diagnostic possibles sans intervention sur le code. |
| 10.4 | Réaliser la recette MVP, corriger les défauts bloquants et publier. | À faire | MVP utilisable en production. |

## Prochaine tâche proposée

**6.2 — Implémenter les endpoints de gestion des SB : créer, lire, modifier, supprimer et lister.** L’Épic 5 étant terminé, cette tâche connectera ensuite le cycle complet du Web à l’API REST.
