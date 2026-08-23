# DunorGames — Modèle canonique minimal de Stats Block

## Principe

Le modèle canonique est une **enveloppe** commune, et non un système de règles universel. Il assure la gestion, la recherche, la provenance, l’édition et la conversion assistée de tous les SB. Les mécaniques restent intactes dans `systemData` et seront décrites par système à la tâche `2.2`.

## Enveloppe `Statblock`

| Champ | Rôle |
| --- | --- |
| `schemaVersion` | Version du contrat de données. Version initiale : `1.0`. |
| `id` | Identifiant stable du SB. |
| `system` | `dungeonsAndDragons`, `talesOfTheValiant`, `daggerheart`, `drawSteel` ou `dc20`. |
| `identity` | Nom requis, sous-titre et alias facultatifs. |
| `description` | Présentation narrative commune et facultative. |
| `tags` | Tags libres, uniques et recherchables. |
| `source` | Provenance : manuel, importé ou converti ; origine et référence facultatives. |
| `metadata` | Propriétaire, statut, dates de création/modification et notes internes. |
| `systemData` | Objet JSON obligatoire contenant exclusivement les données de jeu du système indiqué. |

## Invariants

- La conversion ne remplace jamais le SB source : le nouveau SB porte `origin: converted`, `sourceSystem` et `sourceStatblockId`.
- `systemData` est opaque pour l’enveloppe. Les templates et validateurs spécifiques à un jeu en sont responsables.
- Un SB peut être un brouillon même lorsque certaines données propres au système sont incomplètes.
- Les métadonnées d’audit et l’appartenance sont gérées par l’API, jamais acceptées aveuglément depuis le navigateur.

## Artefacts versionnés

- Schéma JSON : [statblock-envelope.schema.json](schemas/statblock-envelope.schema.json)
- Exemple DH : [street-bandit.daggerheart.statblock.json](examples/street-bandit.daggerheart.statblock.json)
- Types C# : `DunorGames.Domain.Statblocks` et `DunorGames.Contracts.Statblocks`.
