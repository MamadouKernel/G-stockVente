# 📧 Configuration du Service Email

Ce document explique comment configurer le service d'envoi d'emails pour l'application Gestion Stock & Vente.

## 📋 Vue d'ensemble

Le service email est utilisé pour :
- Envoyer les rapports programmés (Phase 4 V2)
- Notifier les utilisateurs par email
- Envoyer des alertes importantes

## ⚙️ Configuration

### 1. Paramètres dans `appsettings.json`

La configuration se trouve dans la section `EmailSettings` :

```json
{
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "UseSsl": true,
    "UseStartTls": true,
    "SmtpUsername": "votre-email@gmail.com",
    "SmtpPassword": "votre-mot-de-passe-application",
    "FromEmail": "noreply@gstockvente.com",
    "FromName": "Gestion Stock & Vente",
    "ReplyToEmail": null,
    "ApplicationName": "Gestion Stock & Vente",
    "Enabled": true,
    "DevelopmentMode": false,
    "DevelopmentEmail": null,
    "Timeout": 30000
  }
}
```

### 2. Paramètres détaillés

| Paramètre | Type | Description | Exemple |
|-----------|------|-------------|---------|
| `SmtpServer` | string | Adresse du serveur SMTP | `smtp.gmail.com`, `smtp.office365.com` |
| `SmtpPort` | int | Port SMTP (587 TLS, 465 SSL, 25 non sécurisé) | `587` |
| `UseSsl` | bool | Utiliser SSL/TLS | `true` |
| `UseStartTls` | bool | Utiliser StartTLS | `true` |
| `SmtpUsername` | string | Email d'authentification SMTP | `votre-email@gmail.com` |
| `SmtpPassword` | string | Mot de passe ou mot de passe d'application | `xxxxxxxxxxxxx` |
| `FromEmail` | string | Email expéditeur (affiché dans les emails) | `noreply@gstockvente.com` |
| `FromName` | string | Nom de l'expéditeur | `Gestion Stock & Vente` |
| `ReplyToEmail` | string? | Email de réponse (optionnel) | `support@gstockvente.com` |
| `ApplicationName` | string | Nom de l'application | `Gestion Stock & Vente` |
| `Enabled` | bool | Activer/désactiver l'envoi d'emails | `true` |
| `DevelopmentMode` | bool | Mode développement (rediriger tous les emails) | `false` |
| `DevelopmentEmail` | string? | Email de destination en mode développement | `dev@example.com` |
| `Timeout` | int | Timeout SMTP en millisecondes | `30000` (30s) |

## 🔧 Configuration par Provider

### Gmail

```json
{
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "UseSsl": true,
    "UseStartTls": true,
    "SmtpUsername": "votre-email@gmail.com",
    "SmtpPassword": "mot-de-passe-application-gmail",
    "FromEmail": "votre-email@gmail.com"
  }
}
```

**Important pour Gmail :**
1. Activez l'authentification à deux facteurs sur votre compte Gmail
2. Générez un [mot de passe d'application](https://myaccount.google.com/apppasswords)
3. Utilisez ce mot de passe d'application (pas votre mot de passe Gmail normal)

### Office 365 / Outlook

```json
{
  "EmailSettings": {
    "SmtpServer": "smtp.office365.com",
    "SmtpPort": 587,
    "UseSsl": true,
    "UseStartTls": true,
    "SmtpUsername": "votre-email@domaine.com",
    "SmtpPassword": "votre-mot-de-passe",
    "FromEmail": "votre-email@domaine.com"
  }
}
```

### SendGrid

```json
{
  "EmailSettings": {
    "SmtpServer": "smtp.sendgrid.net",
    "SmtpPort": 587,
    "UseSsl": true,
    "UseStartTls": true,
    "SmtpUsername": "apikey",
    "SmtpPassword": "votre-cle-api-sendgrid",
    "FromEmail": "noreply@votre-domaine.com"
  }
}
```

### Mailgun

```json
{
  "EmailSettings": {
    "SmtpServer": "smtp.mailgun.org",
    "SmtpPort": 587,
    "UseSsl": true,
    "UseStartTls": true,
    "SmtpUsername": "postmaster@votre-domaine.mailgun.org",
    "SmtpPassword": "votre-cle-api-mailgun",
    "FromEmail": "noreply@votre-domaine.com"
  }
}
```

## 🔒 Sécurité

### Variables d'environnement (Recommandé en production)

Pour éviter de stocker les mots de passe en clair dans `appsettings.json`, utilisez les variables d'environnement :

**Windows (PowerShell) :**
```powershell
$env:EmailSettings__SmtpPassword = "votre-mot-de-passe"
$env:EmailSettings__SmtpUsername = "votre-email@gmail.com"
```

**Linux/Mac (Bash) :**
```bash
export EmailSettings__SmtpPassword="votre-mot-de-passe"
export EmailSettings__SmtpUsername="votre-email@gmail.com"
```

**appsettings.Production.json** (optionnel) :
```json
{
  "EmailSettings": {
    "SmtpPassword": "",  // Sera remplacé par la variable d'environnement
    "SmtpUsername": ""   // Sera remplacé par la variable d'environnement
  }
}
```

### Azure App Settings / Configuration

Si vous déployez sur Azure, utilisez les **Configuration** dans le portail Azure :
- `EmailSettings__SmtpPassword`
- `EmailSettings__SmtpUsername`

## 🧪 Mode Développement

En développement, vous pouvez utiliser le mode test :

```json
{
  "EmailSettings": {
    "Enabled": false,  // Désactive l'envoi réel
    "DevelopmentMode": true,
    "DevelopmentEmail": "dev@example.com"  // Redirige tous les emails ici
  }
}
```

Avec `DevelopmentMode: true`, tous les emails seront envoyés à `DevelopmentEmail`, même si le destinataire réel est différent.

## ✅ Validation

Le service valide automatiquement la configuration au démarrage. Vérifiez les logs pour des erreurs de configuration :

```
Email settings are invalid. Please check configuration.
```

## 📝 Exemples d'utilisation

### Dans un service

```csharp
public class MonService
{
    private readonly IEmailService _emailService;

    public MonService(IEmailService emailService)
    {
        _emailService = emailService;
    }

    public async Task EnvoyerRapportAsync(string email, string rapportHtml)
    {
        await _emailService.SendReportAsync(
            email, 
            "Rapport mensuel", 
            rapportHtml
        );
    }
}
```

### Dans un contrôleur

```csharp
public class RapportsController : Controller
{
    private readonly IEmailService _emailService;

    public RapportsController(IEmailService emailService)
    {
        _emailService = emailService;
    }

    [HttpPost]
    public async Task<IActionResult> EnvoyerParEmail(string email)
    {
        var rapportHtml = GenerateRapportHtml();
        var success = await _emailService.SendReportAsync(
            email, 
            "Votre rapport", 
            rapportHtml
        );

        if (success)
        {
            TempData["Success"] = "Rapport envoyé par email avec succès.";
        }
        else
        {
            TempData["Error"] = "Erreur lors de l'envoi de l'email.";
        }

        return RedirectToAction("Index");
    }
}
```

## 🐛 Dépannage

### Problème : "SMTP authentication failed"

**Solution :**
- Vérifiez le nom d'utilisateur et le mot de passe
- Pour Gmail, utilisez un mot de passe d'application, pas votre mot de passe normal
- Vérifiez que l'authentification à deux facteurs est activée (Gmail)

### Problème : "Connection timeout"

**Solution :**
- Vérifiez que le port est correct (587 pour TLS, 465 pour SSL)
- Vérifiez que votre pare-feu/autorise les connexions SMTP
- Augmentez la valeur de `Timeout` si nécessaire

### Problème : "SSL/TLS error"

**Solution :**
- Vérifiez que `UseSsl` est correct selon votre serveur
- Pour le port 587, utilisez `UseStartTls: true`
- Pour le port 465, utilisez `UseSsl: true` et `UseStartTls: false`

### Problème : Les emails ne sont pas envoyés

**Solution :**
- Vérifiez que `Enabled: true` dans la configuration
- Vérifiez les logs de l'application
- Vérifiez le dossier spam du destinataire
- Testez avec un email simple en premier

## 📚 Documentation supplémentaire

- [Documentation System.Net.Mail](https://docs.microsoft.com/dotnet/api/system.net.mail.smtpclient)
- [Configuration ASP.NET Core](https://docs.microsoft.com/aspnet/core/fundamentals/configuration/)

