namespace G_StockVente.Models;

/// <summary>
/// Configuration des paramètres d'envoi d'emails
/// </summary>
public class EmailSettings
{
    /// <summary>
    /// Nom de la section dans appsettings.json
    /// </summary>
    public const string SectionName = "EmailSettings";

    /// <summary>
    /// Serveur SMTP (ex: smtp.gmail.com, smtp.office365.com)
    /// </summary>
    public string SmtpServer { get; set; } = string.Empty;

    /// <summary>
    /// Port SMTP (généralement 587 pour TLS, 465 pour SSL, 25 pour non sécurisé)
    /// </summary>
    public int SmtpPort { get; set; } = 587;

    /// <summary>
    /// Utiliser SSL/TLS pour la connexion SMTP
    /// </summary>
    public bool UseSsl { get; set; } = true;

    /// <summary>
    /// Utiliser TLS (StartTLS)
    /// </summary>
    public bool UseStartTls { get; set; } = true;

    /// <summary>
    /// Nom d'utilisateur SMTP (email d'envoi)
    /// </summary>
    public string SmtpUsername { get; set; } = string.Empty;

    /// <summary>
    /// Mot de passe SMTP (ou mot de passe d'application)
    /// </summary>
    public string SmtpPassword { get; set; } = string.Empty;

    /// <summary>
    /// Email de l'expéditeur (peut être différent du username)
    /// </summary>
    public string FromEmail { get; set; } = string.Empty;

    /// <summary>
    /// Nom de l'expéditeur (affiché dans le client email)
    /// </summary>
    public string FromName { get; set; } = "Gestion Stock & Vente";

    /// <summary>
    /// Email de réponse (optionnel, si différent de FromEmail)
    /// </summary>
    public string? ReplyToEmail { get; set; }

    /// <summary>
    /// Nom de l'application (utilisé dans les emails)
    /// </summary>
    public string ApplicationName { get; set; } = "Gestion Stock & Vente";

    /// <summary>
    /// Activer l'envoi d'emails (peut être désactivé en développement)
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Mode développement : envoyer tous les emails à une adresse de test
    /// </summary>
    public bool DevelopmentMode { get; set; } = false;

    /// <summary>
    /// Email de destination en mode développement (tous les emails seront envoyés ici)
    /// </summary>
    public string? DevelopmentEmail { get; set; }

    /// <summary>
    /// Timeout pour la connexion SMTP (en millisecondes)
    /// </summary>
    public int Timeout { get; set; } = 30000; // 30 secondes

    /// <summary>
    /// Valider la configuration
    /// </summary>
    public bool IsValid()
    {
        if (!Enabled) return true; // Si désactivé, on considère comme valide
        
        if (string.IsNullOrWhiteSpace(SmtpServer)) return false;
        if (SmtpPort <= 0 || SmtpPort > 65535) return false;
        if (string.IsNullOrWhiteSpace(SmtpUsername)) return false;
        if (string.IsNullOrWhiteSpace(SmtpPassword)) return false;
        if (string.IsNullOrWhiteSpace(FromEmail)) return false;

        return true;
    }
}

