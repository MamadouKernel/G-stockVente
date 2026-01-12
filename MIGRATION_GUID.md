# 🔄 Migration vers GUID - Guide de mise à jour

## ✅ Modifications effectuées

### Domain Models
- ✅ Tous les `Id` convertis de `int` vers `Guid` avec `= Guid.NewGuid()`
- ✅ Toutes les clés étrangères converties vers `Guid`
- ✅ `ApplicationUser` : Utilise `IdentityUser<Guid>`
- ✅ `UtilisateurId` : Converti de `string` vers `Guid`

### Infrastructure
- ✅ `ApplicationDbContext` : Utilise `IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>`
- ✅ `BoutiqueActiveService` : Méthodes mises à jour pour `Guid`
- ✅ Configuration PostgreSQL : Génération automatique avec `gen_random_uuid()`

### Controllers
- ✅ Tous les paramètres `int? id` → `Guid? id`
- ✅ Tous les paramètres `int id` → `Guid id`
- ✅ ViewModels mis à jour

## ⚠️ Points d'attention

### 1. FindByIdAsync avec Guid
```csharp
// Avant (string)
await _userManager.FindByIdAsync(userId);

// Maintenant (Guid)
await _userManager.FindByIdAsync(userId.ToString());
```

### 2. Conversions dans les vues
Les vues peuvent nécessiter des ajustements pour afficher/éditer les GUIDs.

### 3. Routes
Les routes MVC gèrent automatiquement les GUIDs dans les URLs.

### 4. JSON Serialization
Le panier en session (JSON) fonctionne avec les GUIDs.

## 📝 Prochaines étapes

1. **Créer une nouvelle migration** :
   ```bash
   dotnet ef migrations add MigrationVersGUID
   ```

2. **Supprimer l'ancienne base de données** (si données de test) :
   ```bash
   dotnet ef database drop
   ```

3. **Créer la nouvelle base** :
   ```bash
   dotnet ef database update
   ```

4. **Vérifier les vues** qui affichent des IDs pour s'assurer qu'elles fonctionnent avec les GUIDs.

## 🔧 Corrections manuelles possibles

Si certaines vues ou formulaires nécessitent des ajustements pour les GUIDs, vérifier :
- Les champs cachés avec des IDs
- Les liens de routage avec des IDs
- Les formulaires qui soumettent des IDs

Les GUIDs sont automatiquement convertis dans les routes ASP.NET Core MVC.

