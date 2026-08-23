# DunorGames — Schémas de données par système

Chaque fichier de `schemas/` valide le contenu de `systemData` d’une enveloppe `Statblock` version `1.0`.

Les schémas imposent les éléments indispensables au rendu et conservent des objets extensibles (`additionalProperties: true`) pour les champs de règles non encore rencontrés. Un ajout de champ est donc non destructif ; une révision de champ existant exige une nouvelle version de schéma.

| Système | Schéma | Référence initiale |
| --- | --- | --- |
| Daggerheart | `daggerheart-system-data.schema.json` | Street Bandit |
| Draw Steel | `draw-steel-system-data.schema.json` | Street Bandit |
| DC20 | `dc20-system-data.schema.json` | Street Bandit et Daggerer |
| D&D 5.5 | `dnd-5-5-system-data.schema.json` | Street Bandit |

D&D 5.5 est la base de données initiale du template partagé D&D/TotV. Tales of the Valiant recevra son propre schéma dérivé lorsqu’un SB de référence sera fourni : l’apparence est commune, les règles ne sont pas présumées identiques.

## Règles de modélisation

- Les actions, traits, réactions et améliorations sont des listes ordonnées de blocs de règles. Leur texte original est conservé.
- Les champs purement d’affichage ne doivent pas servir à recalculer des règles ; les valeurs mécaniques et le texte affiché peuvent coexister.
- Toute règle de conversion entre systèmes sera ajoutée à l’épic 9, jamais déduite de ces schémas.
- Les valeurs inconnues ou non supportées restent dans une propriété additionnelle de `systemData` jusqu’à ce qu’un exemple confirme leur structure.
