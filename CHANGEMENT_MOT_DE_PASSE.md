# 🔐 Changement de mot de passe obligatoire

## Fonctionnement

L'application force le **changement de mot de passe lors de la première connexion** de chaque utilisateur pour des raisons de sécurité.

---

## Processus

### 1. Connexion

Lorsqu'un utilisateur se connecte avec un compte qui a le flag `MustChangePassword = true` :

1. ✅ La connexion est vérifiée (identifiants valides)
2. 🔒 L'utilisateur est **déconnecté temporairement**
3. 🔄 Redirection automatique vers `/Account/ChangePasswordFirstTime`
4. ⚠️ Impossible d'accéder au reste de l'application tant que le mot de passe n'est pas changé

### 2. Changement de mot de passe

L'utilisateur doit :
- Saisir son **mot de passe actuel**
- Saisir un **nouveau mot de passe** (respectant les règles de sécurité)
- **Confirmer** le nouveau mot de passe

### 3. Validation

Une fois le mot de passe changé :
- ✅ Le flag `MustChangePassword` est mis à `false`
- ✅ L'utilisateur est **automatiquement reconnecté**
- ✅ Redirection vers l'accueil ou sélection de boutique
- ✅ L'utilisateur peut maintenant utiliser l'application normalement

---

## Configuration

### Champ dans ApplicationUser

```csharp
/// <summary>
/// Indique si l'utilisateur doit changer son mot de passe à la prochaine connexion
/// </summary>
public bool MustChangePassword { get; set; } = true;
```

### Comportement par défaut

- ✅ **Nouveaux utilisateurs** : `MustChangePassword = true` par défaut
- ✅ **Compte admin** : `MustChangePassword = true` à la création
- ✅ **Après changement** : `MustChangePassword = false`

---

## Règles de sécurité des mots de passe

Les nouveaux mots de passe doivent respecter :

- ✅ **Minimum 6 caractères**
- ✅ **Au moins une majuscule** (A-Z)
- ✅ **Au moins une minuscule** (a-z)
- ✅ **Au moins un chiffre** (0-9)
- ✅ **Caractères spéciaux** : Optionnels

Exemples valides :
- `NouveauPass123`
- `MonMdp2024`
- `SecurePwd1`

---

## Vérification dans le code

### AccountController.cs

```csharp
if (result.Succeeded)
{
    var user = await _userManager.FindByEmailAsync(model.Email);
    if (user != null)
    {
        // Vérifier si l'utilisateur doit changer son mot de passe
        if (user.MustChangePassword)
        {
            await _signInManager.SignOutAsync();
            TempData["MustChangePassword"] = true;
            TempData["UserId"] = user.Id;
            return RedirectToAction("ChangePasswordFirstTime");
        }
        // ... suite de la connexion
    }
}
```

---

## Forcer le changement pour un utilisateur existant

### Via le code

```csharp
var user = await _userManager.FindByEmailAsync("user@example.com");
if (user != null)
{
    user.MustChangePassword = true;
    await _userManager.UpdateAsync(user);
}
```

### Via SQL (à éviter en production)

```sql
UPDATE "AspNetUsers"
SET "MustChangePassword" = true
WHERE "Email" = 'user@example.com';
```

---

## Interface utilisateur

### Page de changement

- **Route** : `/Account/ChangePasswordFirstTime`
- **Style** : Alert warning pour attirer l'attention
- **Message** : "Pour des raisons de sécurité, vous devez changer votre mot de passe avant de continuer"
- **Champs** :
  - Mot de passe actuel
  - Nouveau mot de passe
  - Confirmation du nouveau mot de passe

---

## Sécurité

### Protections implémentées

- ✅ **Vérification du mot de passe actuel** avant changement
- ✅ **Validation des règles** de sécurité
- ✅ **Token de réinitialisation** utilisé pour le changement
- ✅ **Reconnexion automatique** après changement
- ✅ **Session temporaire** via TempData pour éviter l'accès direct

### Contournements impossibles

- ❌ Impossible de bypasser la page de changement
- ❌ Impossible d'accéder à l'application sans changer le mot de passe
- ❌ Le flag est vérifié à chaque connexion

---

## Dépannage

### L'utilisateur ne peut pas changer son mot de passe

1. **Vérifier le mot de passe actuel** : Doit être correct
2. **Vérifier les règles** : Le nouveau mot de passe doit respecter les contraintes
3. **Vérifier la session** : TempData peut expirer, reconnecter si nécessaire

### L'utilisateur est bloqué

Si un utilisateur est bloqué avec `MustChangePassword = true` et a oublié son mot de passe :

1. Un administrateur peut réinitialiser le mot de passe
2. Ou mettre `MustChangePassword = false` temporairement (via admin)

---

## Résumé

✅ **Changement obligatoire** à la première connexion
✅ **Vérification automatique** du flag `MustChangePassword`
✅ **Redirection automatique** vers la page de changement
✅ **Reconnexion automatique** après validation
✅ **Sécurité renforcée** pour tous les comptes

**Le changement de mot de passe obligatoire est maintenant actif !** 🔒

