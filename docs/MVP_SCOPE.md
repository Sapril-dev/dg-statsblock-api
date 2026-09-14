# DunorGames — Périmètre fonctionnel MVP

## But du MVP

Livrer une première application Web permettant à un utilisateur de créer, modifier, prévisualiser, sauvegarder et exporter des **stats blocks (SB)** DunorGames.

Le premier SB de référence est **Street Bandit** pour **Daggerheart (DH)**. Son rendu HTML/CSS approuvé devient le modèle de qualité pour les prochains templates.

## Résultat utilisateur attendu

Un utilisateur peut :

1. créer un SB DH à partir d’un formulaire ;
2. voir l’aperçu se mettre à jour immédiatement ;
3. enregistrer puis retrouver son SB ;
4. exporter ce même rendu en PNG haute résolution ;
5. modifier ultérieurement le SB sans perte de mise en forme ni de données.

## Inclus dans le MVP

### Application Web

- Bibliothèque personnelle de SB : liste, recherche simple, ouverture, duplication et archivage.
- Création et édition de SB DH.
- Aperçu à partir du même template HTML/CSS que les exports.
- Rendu à largeur fixe de 3,53 po, avec hauteur automatique selon le contenu.
- Chargement local des polices Cardo et Montserrat ainsi que des assets DunorGames.
- Préservation des emphases nécessaires : gras, italique, gras-italique, titres et séparateurs.

### API et données

- API REST versionnée pour le cycle de vie complet d’un SB : création, lecture, modification, suppression et liste.
- Persistance des SB, de leur système, de leur propriétaire, de leur statut et de leurs métadonnées.
- Validation côté client et côté serveur des champs DH.
- Authentification et autorisation minimales avant publication : chaque utilisateur accède seulement à ses SB.

### Exports

- PNG sans perte à 600 PPI, avec résolution physique intégrée.
- Détection des débordements avant l’export.

### Référence visuelle DH

- Palette DunorGames : `#B2DDFD`, `#E7F4FD`, `#050FAF`, noir et blanc.
- Header DC20 fourni avec son extension arrondie et ses flocons.
- Panneau DH clair avec lignes bleues supérieure/inférieure et séparateur pointillé avant Experience.
- Hauteur déterminée par le contenu, avec interlignes et marges de paragraphes lisibles.

## Hors périmètre du MVP

- Édition complète de Draw Steel, DC20, D&D et Tales of the Valiant.
- Conversion automatisée entre systèmes.
- Collaboration en temps réel, partage public, commentaires ou gestion d’équipes.
- Import/OCR automatique de captures d’écran ou de documents PDF.
- Gestion avancée d’images, de portraits, de bestiaires publics ou de contenu communautaire.
- Mise en page de livres complets et composition de PDF multi-pages.
- Facturation, rôles organisationnels avancés ou intégrations tierces.

## Jalons fonctionnels

| Jalon | Définition de terminé |
| --- | --- |
| Référence DH | Street Bandit HTML/CSS est approuvé visuellement. |
| Édition locale | Un SB DH peut être créé, modifié et prévisualisé dans le navigateur. |
| Persistance | Le SB survit à une fermeture de session et est retrouvé dans la bibliothèque. |
| Export | Le PNG correspondent à l’aperçu validé. |
| MVP publié | L’application est déployée et utilisable sur l’infrastructure Smarter ASP. |

## Décisions à prendre avant l’implémentation

Ces décisions sont volontairement reportées à la tâche `1.2` et `1.3` : capacités de Smarter ASP, stack Web/API, identité et stratégie des exports.

## Critères d’acceptation de la tâche 1.1

- Le périmètre MVP, les exclusions et les jalons sont écrits et traçables.
- DH et Street Bandit sont explicitement définis comme première référence de rendu.
- Les éléments risquant de modifier l’architecture sont identifiés avant le choix de stack.
