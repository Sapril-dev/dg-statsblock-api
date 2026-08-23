# Scripts SQL versionnés

`001_initial_statblocks.sql` est le script SQL Server idempotent généré depuis la migration EF Core `InitialStatblocks`.

Exécutez-le une seule fois avec un compte disposant du droit de création de tables dans la base SmarterASP. Le script crée `__EFMigrationsHistory`; les prochaines migrations doivent être appliquées par le même mécanisme EF Core, jamais automatiquement au démarrage de l'API.
