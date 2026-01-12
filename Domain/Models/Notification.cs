using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace G_StockVente.Domain.Models;

/// <summary>
/// Type de notification
/// </summary>
public enum TypeNotification
{
    StockBas = 1,
    RuptureStock = 2,
    EcartInventaire = 3,
    AnnulationVente = 4,
    TransfertEnAttente = 5,
    TransfertRecu = 6,
    RapportDisponible = 7,
    Autre = 99
}

/// <summary>
/// Notification système pour les utilisateurs
/// </summary>
public class Notification
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Utilisateur destinataire (null = notification globale)
    /// </summary>
    public Guid? UtilisateurId { get; set; }

    /// <summary>
    /// Boutique concernée (null = notification réseau)
    /// </summary>
    public Guid? BoutiqueId { get; set; }

    /// <summary>
    /// Type de notification
    /// </summary>
    [Required]
    public TypeNotification Type { get; set; }

    /// <summary>
    /// Titre de la notification
    /// </summary>
    [Required]
    [StringLength(200)]
    public string Titre { get; set; } = string.Empty;

    /// <summary>
    /// Message de la notification
    /// </summary>
    [Required]
    [StringLength(1000)]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Lien vers l'action à effectuer (URL)
    /// </summary>
    [StringLength(500)]
    public string? LienAction { get; set; }

    /// <summary>
    /// Indique si la notification a été lue
    /// </summary>
    public bool EstLue { get; set; } = false;

    /// <summary>
    /// Date de création
    /// </summary>
    public DateTime DateCreation { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date de lecture
    /// </summary>
    public DateTime? DateLecture { get; set; }

    // Navigation properties
    [ForeignKey(nameof(UtilisateurId))]
    public virtual ApplicationUser? Utilisateur { get; set; }

    [ForeignKey(nameof(BoutiqueId))]
    public virtual Boutique? Boutique { get; set; }
}

