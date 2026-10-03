# Scripts SQL versionnés

Ces scripts idempotents sont conservés comme historique. Les scripts de transition
EF du modèle actuel vivent dans `src/DunorGames.Data/Script SQL/` : voir
[Configuration SQL](../CONFIGURATION_SQL.md). Ne pas appliquer les deux variantes
d'une même migration à une base où elle est déjà enregistrée.

`001_initial_statblocks.sql` est le script SQL Server idempotent généré depuis la migration EF Core `InitialStatblocks`.

`002_add_statblock_aliases.sql` ajoute la colonne JSON nullable qui conserve les alias du modèle public.

Exécutez les scripts dans l’ordre numérique avec un compte disposant des droits
requis dans la base SmarterASP. Chaque script consulte `__EFMigrationsHistory` et
peut donc être relancé sans réappliquer une migration déjà enregistrée. Les
migrations ne sont jamais exécutées automatiquement au démarrage de l’API.
