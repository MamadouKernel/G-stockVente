# Guide des Migrations Entity Framework Core

## Prérequis

Assurez-vous d'avoir installé les outils EF Core :

```bash
dotnet tool install --global dotnet-ef
```

## Créer une migration

```bash
dotnet ef migrations add NomDeLaMigration
```

## Appliquer les migrations

```bash
dotnet ef database update
```

## Annuler la dernière migration

```bash
dotnet ef database update NomDeLaMigrationPrecedente
```

## Supprimer la dernière migration (si non appliquée)

```bash
dotnet ef migrations remove
```

## Créer la base de données initiale

1. Vérifiez la chaîne de connexion dans `appsettings.json`
2. Exécutez :

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

Les données initiales (rôles et administrateur) seront créées automatiquement au démarrage de l'application.

## Connexion PostgreSQL

Format de la chaîne de connexion :

```
Host=localhost;Port=5432;Database=GStockVente;Username=postgres;Password=votre_mot_de_passe
```

