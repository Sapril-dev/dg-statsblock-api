# Contrat API REST v1

Ce document définit le contrat public entre le site Web DunorGames et son API REST. Il décrit les échanges attendus; les endpoints ne seront exposés qu'à la tâche 6.2.

La spécification exploitable est disponible dans [`openapi/dunorgames-v1.openapi.yaml`](openapi/dunorgames-v1.openapi.yaml).

## Versionnement

- Toutes les routes publiques de cette première version utilisent le préfixe `/api/v1`.
- Une évolution compatible dans v1 peut ajouter un champ ou une route; elle ne retire ni ne renomme un élément existant.
- Une rupture de compatibilité crée une nouvelle version majeure de l'API, par exemple `/api/v2`; v1 reste disponible pendant la période de migration annoncée.
- La version de l'API est indépendante de `schemaVersion` dans un SB. `schemaVersion` décrit le format du document de jeu (`1.0` aujourd'hui), alors que `v1` décrit le contrat HTTP.

## Ressource `statblocks`

| Opération | Route | Résultat |
| --- | --- | --- |
| Lister | `GET /api/v1/statblocks` | Liste synthétique, paginée par curseur. Filtres possibles: `system`, `status`, `tag`, `query`, `cursor`. |
| Lire | `GET /api/v1/statblocks/{statblockId}` | SB complet et son `ETag`. |
| Créer | `POST /api/v1/statblocks` | Crée un brouillon et retourne `201 Created`, la ressource et l'en-tête `Location`. |
| Remplacer | `PUT /api/v1/statblocks/{statblockId}` | Remplace toutes les données modifiables, dont le statut. `If-Match` obligatoire. |
| Supprimer définitivement | `DELETE /api/v1/statblocks/{statblockId}` | Retire définitivement la ressource. `If-Match` obligatoire. |

L'archivage et la restauration sont des mises à jour complètes avec `PUT`: le client transmet `status: "archived"`, puis `draft` ou `published` pour restaurer. L'archivage ne supprime pas le SB.

## Représentations de lecture et d'écriture

`StatblockResponse` est la représentation de lecture. Elle contient l'identifiant, les dates, le statut et les données propres au système.

`CreateStatblockRequest` et `UpdateStatblockRequest` sont des représentations d'écriture distinctes. Elles n'acceptent ni `id`, ni `ownerId`, ni les horodatages: ces valeurs sont produites ou contrôlées par l'API. Le serveur assigne toujours le statut initial `draft` lors d'un `POST`.

Un `PUT` est un remplacement complet des champs modifiables. Il peut modifier le statut, les notes et `systemData`, mais pas l'identifiant, le propriétaire ou la date de création.

## Concurrence

La lecture individuelle retourne un `ETag` opaque. Toute mutation (`PUT`, `DELETE`) doit fournir cet ETag dans `If-Match`.

- L'ETag correspond: la mutation est appliquée et un nouvel ETag est retourné.
- L'ETag est absent ou périmé: l'API répond `412 Precondition Failed`; le client relit le SB avant de proposer une nouvelle sauvegarde.

## Erreurs

Les erreurs utilisent le type média `application/problem+json`, selon RFC 9457. Les réponses comportent au minimum `type`, `title` et `status`; elles peuvent ajouter `detail`, `instance` et `errors` pour les erreurs de validation.

| Situation | Statut HTTP |
| --- | --- |
| Corps invalide ou champ obligatoire absent | `400 Bad Request` |
| Données valides en JSON mais incompatibles avec le schéma du système | `422 Unprocessable Content` |
| Authentification requise | `401 Unauthorized` |
| Ressource d'un autre utilisateur | `403 Forbidden` |
| SB introuvable | `404 Not Found` |
| ETag absent ou périmé | `412 Precondition Failed` |

## Contrat de pagination

Les listes sont ordonnées de façon stable par l'API. `nextCursor` est opaque: le client le transmet tel quel à la demande suivante et ne doit pas l'interpréter. Une valeur `null` signifie qu'il n'y a pas d'autre page.
