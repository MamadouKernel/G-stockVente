using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace G_StockVente.Domain.Models;

/// <summary>
/// Journal des tentatives de connexion
/// </summary>
public class JournalConnexion
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Utilisateur (null si échec de connexion)
    /// </summary>
    public Guid? UtilisateurId { get; set; }

    /// <summary>
    /// Email utilisé pour la tentative de connexion
    /// </summary>
    [Required]
    [StringLength(200)]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Adresse IP de la tentative
    /// </summary>
    [StringLength(50)]
    public string? AdresseIP { get; set; }

    /// <summary>
    /// Indique si la connexion a réussi
    /// </summary>
    public bool Succes { get; set; }

    /// <summary>
    /// Raison de l'échec (si Succes = false)
    /// </summary>
    [StringLength(500)]
    public string? RaisonEchec { get; set; }

    /// <summary>
    /// Date et heure de la tentative
    /// </summary>
    public DateTime DateTentative { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey(nameof(UtilisateurId))]
    public virtual ApplicationUser? Utilisateur { get; set; }
}

