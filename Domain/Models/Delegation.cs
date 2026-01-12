using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace G_StockVente.Domain.Models;

/// <summary>
/// Délégation temporaire de droits d'un utilisateur à un autre
/// </summary>
public class Delegation
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Utilisateur qui délègue ses droits
    /// </summary>
    [Required]
    public Guid UtilisateurDelegantId { get; set; }

    /// <summary>
    /// Utilisateur bénéficiaire de la délégation
    /// </summary>
    [Required]
    public Guid UtilisateurBeneficiaireId { get; set; }

    /// <summary>
    /// Rôle délégué (AdminReseau, ManagerBoutique, GestionnaireStock, Caissier)
    /// </summary>
    [Required]
    [StringLength(50)]
    public string RoleDelege { get; set; } = string.Empty;

    /// <summary>
    /// Date de début de la délégation
    /// </summary>
    [Required]
    public DateTime DateDebut { get; set; }

    /// <summary>
    /// Date de fin de la délégation
    /// </summary>
    [Required]
    public DateTime DateFin { get; set; }

    /// <summary>
    /// Indique si la délégation est active
    /// </summary>
    public bool EstActive { get; set; } = true;

    /// <summary>
    /// Raison de la délégation
    /// </summary>
    [StringLength(500)]
    public string? Raison { get; set; }

    /// <summary>
    /// Indique si la délégation est supprimée (soft delete)
    /// </summary>
    public bool EstSupprime { get; set; } = false;

    /// <summary>
    /// Date de suppression (soft delete)
    /// </summary>
    public DateTime? DateSuppression { get; set; }

    public DateTime DateCreation { get; set; } = DateTime.UtcNow;
    public DateTime? DateModification { get; set; }

    // Navigation properties
    [ForeignKey(nameof(UtilisateurDelegantId))]
    public virtual ApplicationUser? UtilisateurDelegant { get; set; }

    [ForeignKey(nameof(UtilisateurBeneficiaireId))]
    public virtual ApplicationUser? UtilisateurBeneficiaire { get; set; }
}

