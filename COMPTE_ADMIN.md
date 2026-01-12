# 👤 Compte Administrateur par Défaut

## Informations de connexion

Un compte administrateur est **automatiquement créé** lors du premier démarrage de l'application.

### 🔑 Identifiants par défaut

- **Email** : `admin@gstockvente.com`
- **Mot de passe** : `Admin123!`
- **Rôle** : `AdminReseau` (Accès complet au réseau)

### ⚠️ Important

**Changez ce mot de passe immédiatement après la première connexion !**

---

## Comment ça fonctionne

### Création automatique

Le compte admin est créé automatiquement par la classe `SeedData` qui s'exécute au démarrage de l'application dans `Program.cs` :

```csharp
// Initialiser les données de base
using (var scope = app.Services.CreateScope())
{
    await SeedData.InitializeAsync(scope.ServiceProvider);
}
```

### Processus de création

1. **Vérification** : L'application vérifie si le compte existe déjà
2. **Création** : Si inexistant, le compte est créé avec :
   - Email : `admin@gstockvente.com`
   - Mot de passe : `Admin123!` (automatiquement hashé)
   - Prénom : "Administrateur"
   - Nom : "Réseau"
   - Statut : Actif
3. **Attribution du rôle** : Le rôle `AdminReseau` est attribué

### Sécurité

- ✅ Le mot de passe est **automatiquement hashé** avant stockage (PBKDF2)
- ✅ Le compte n'est créé qu'**une seule fois** (vérification à chaque démarrage)
- ✅ Si le compte existe déjà, aucune modification n'est effectuée

---

## Première connexion

### Étapes

1. **Démarrer l'application**
2. **Aller sur la page de connexion** : `/Account/Login`
3. **Saisir les identifiants** :
   - Email : `admin@gstockvente.com`
   - Mot de passe : `Admin123!`
4. **Se connecter**
5. **⚠️ Changement de mot de passe obligatoire** :
   - À la première connexion, vous serez automatiquement redirigé vers la page de changement de mot de passe
   - Vous devez saisir :
     - Votre mot de passe actuel : `Admin123!`
     - Un nouveau mot de passe (min 6 caractères, majuscules, minuscules, chiffres)
     - Confirmer le nouveau mot de passe
   - Après validation, vous serez automatiquement reconnecté avec le nouveau mot de passe

### Après la connexion

Comme administrateur réseau, vous pouvez :
- ✅ Gérer toutes les boutiques
- ✅ Créer des utilisateurs
- ✅ Attribuer des rôles
- ✅ Accéder à toutes les données
- ✅ Modifier les paramètres globaux

---

## Changer le mot de passe

### Via l'interface (à implémenter)

Un formulaire de changement de mot de passe peut être ajouté dans le profil utilisateur.

### Via le code (pour développement)

```csharp
var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
var admin = await userManager.FindByEmailAsync("admin@gstockvente.com");
var token = await userManager.GeneratePasswordResetTokenAsync(admin);
var result = await userManager.ResetPasswordAsync(admin, token, "NouveauMotDePasse123!");
```

### Via la base de données (non recommandé)

⚠️ **Ne modifiez jamais directement le hash en base de données !**

Le mot de passe doit être changé via Identity pour garantir le hashage correct.

---

## Vérifier que le compte existe

### Via SQL

```sql
SELECT "Id", "Email", "UserName", "Prenom", "Nom", "EstActif"
FROM "AspNetUsers"
WHERE "Email" = 'admin@gstockvente.com';
```

### Via les logs

Lors du démarrage, les logs indiquent :
```
Vérification du compte administrateur...
Compte administrateur créé avec succès: admin@gstockvente.com
Rôle 'AdminReseau' ajouté au compte administrateur.
```

ou

```
Compte administrateur existe déjà: admin@gstockvente.com
```

---

## Dépannage

### Le compte n'est pas créé

1. **Vérifier les logs** : Regarder les erreurs lors du démarrage
2. **Vérifier la connexion BDD** : La base doit être accessible
3. **Vérifier les migrations** : Les migrations doivent être appliquées

### Impossible de se connecter

1. **Vérifier l'email** : Doit être exactement `admin@gstockvente.com`
2. **Vérifier le mot de passe** : `Admin123!` (sensible à la casse)
3. **Vérifier que le compte est actif** : `EstActif = true`

### Erreur de rôle

Si le rôle `AdminReseau` n'est pas attribué :
- Il sera automatiquement ajouté au prochain démarrage
- Ou créer manuellement via la gestion des utilisateurs (après connexion)

---

## Résumé

✅ **Compte créé automatiquement** au premier démarrage
✅ **Mot de passe hashé** automatiquement
✅ **Rôle AdminReseau** attribué automatiquement
✅ **Vérification à chaque démarrage** (pas de duplication)
⚠️ **Changer le mot de passe** après la première connexion !

**Le compte admin est prêt à être utilisé !** 🎉

