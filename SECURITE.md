# 🔐 Sécurité - Configuration de l'authentification

## Mots de passe hashés

### Hashage automatique
ASP.NET Core Identity **hash automatiquement tous les mots de passe** avant de les stocker en base de données.

- **Algorithme utilisé** : PBKDF2 (Password-Based Key Derivation Function 2)
- **Méthode** : `IPasswordHasher<ApplicationUser>` implémentée par `PasswordHasher<T>`
- **Sécurité** : Chaque mot de passe a un salt unique généré aléatoirement

### Configuration actuelle
```csharp
// Les mots de passe sont automatiquement hashés lors de la création/modification
var result = await _userManager.CreateAsync(user, password);
// Le password est hashé automatiquement avant stockage en BDD
```

### Exemple de hash stocké
```
AQAAAAEAACcQAAAAEA... (hash PBKDF2 avec salt unique)
```

**Important** : Les mots de passe en clair ne sont **jamais** stockés en base de données.

---

## Authentification par cookies

### Cookies HTTP sécurisés
L'authentification utilise des **cookies HTTP sécurisés** configurés dans `Program.cs`.

### Configuration des cookies

#### Propriétés de sécurité
- ✅ **HttpOnly = true** : Empêche l'accès JavaScript au cookie (protection contre XSS)
- ✅ **SecurePolicy** : Force HTTPS en production
- ✅ **SameSite = Lax** : Protection contre les attaques CSRF
- ✅ **ExpireTimeSpan = 8 heures** : Durée de vie du cookie
- ✅ **SlidingExpiration = true** : Renouvellement automatique lors de l'activité

#### Nom du cookie
```
GStockVente.Auth
```

#### Contenu du cookie
Le cookie contient un **ticket d'authentification chiffré** qui inclut :
- L'identité de l'utilisateur
- Les claims/rôles
- Les informations de session

#### Cycle de vie
1. **Connexion** : `SignInManager.SignInAsync()` crée le cookie
2. **Vérification** : `UseAuthentication()` vérifie le cookie à chaque requête
3. **Renouvellement** : Si `SlidingExpiration = true`, le cookie est renouvelé après activité
4. **Expiration** : Après 8h d'inactivité, l'utilisateur est déconnecté
5. **Déconnexion** : `SignInManager.SignOutAsync()` supprime le cookie

---

## Protection des données

### En base de données
- ✅ Mots de passe hashés (PBKDF2)
- ✅ Salts uniques par mot de passe
- ✅ Pas de mots de passe en clair

### En transit
- ✅ Cookies chiffrés (protection des données sensibles)
- ✅ HTTPS recommandé en production

### Dans le navigateur
- ✅ Cookie HttpOnly (inaccessible via JavaScript)
- ✅ Protection CSRF (SameSite)
- ✅ Expiration automatique

---

## Bonnes pratiques de sécurité

### Pour les utilisateurs
1. **Changer le mot de passe admin par défaut** après la première connexion
2. **Utiliser des mots de passe forts** (min 6 caractères, majuscules, chiffres)
3. **Ne pas partager les comptes**
4. **Se déconnecter** après utilisation sur un ordinateur partagé

### Pour les développeurs
1. **Ne jamais logger les mots de passe** en clair
2. **Ne jamais stocker** les mots de passe en clair
3. **Utiliser HTTPS** en production
4. **Valider les entrées** utilisateur
5. **Limiter les tentatives de connexion** (à implémenter si nécessaire)

---

## Vérification du hashage

### Tester que les mots de passe sont hashés

```sql
-- Vérifier en base de données
SELECT "Id", "Email", "PasswordHash" 
FROM "AspNetUsers";
-- PasswordHash contient le hash, jamais le mot de passe en clair
```

### Tester la connexion par cookies

1. Se connecter avec un compte
2. Ouvrir les outils de développement (F12)
3. Aller dans l'onglet **Application > Cookies**
4. Vérifier la présence du cookie `GStockVente.Auth`
5. Le cookie doit avoir `HttpOnly` activé (non accessible via JavaScript)

---

## Configuration avancée (optionnel)

### Forcer HTTPS pour les cookies en production
```csharp
if (!app.Environment.IsDevelopment())
{
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
}
```

### Limiter les tentatives de connexion
```csharp
options.Lockout.MaxFailedAccessAttempts = 5;
options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
```

### Changer l'algorithme de hashage (non recommandé)
Identity utilise PBKDF2 par défaut, qui est sécurisé. 
Ne pas modifier sauf nécessité spécifique.

---

## 🔒 Résumé

✅ **Mots de passe hashés** : Oui, automatiquement par Identity (PBKDF2)
✅ **Authentification par cookies** : Oui, cookies HTTP sécurisés
✅ **Protection XSS** : HttpOnly activé
✅ **Protection CSRF** : SameSite activé
✅ **Expiration** : 8h avec renouvellement automatique
✅ **Chiffrement** : Tickets d'authentification chiffrés

**Tout est déjà configuré et sécurisé par défaut !** 🎉

