using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace G_StockVente.Domain.Models;

/// <summary>
/// Statut d'un inventaire
/// </summary>
public enum StatutInventaire
{
    EnCours = 1,
    Finalise = 2,
    Annule = 3
}

/// <summary>
/// Inventaire de stock
/// </summary>
public class Inventaire
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Numéro unique d'inventaire pour cette boutique
    /// </summary>
    [Required]
    [StringLength(50)]
    public string NumeroInventaire { get; set; } = string.Empty;

    /// <summary>
    /// Boutique
    /// </summary>
    [Required]
    public Guid BoutiqueId { get; set; }

    /// <summary>
    /// Dépôt inventorié
    /// </summary>
    [Required]
    public Guid DepotId { get; set; }

    /// <summary>
    /// Statut de l'inventaire
    /// </summary>
    [Required]
    public StatutInventaire Statut { get; set; } = StatutInventaire.EnCours;

    /// <summary>
    /// Date de début d'inventaire
    /// </summary>
    public DateTime DateDebut { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date de finalisation
    /// </summary>
    public DateTime? DateFinalisation { get; set; }

    /// <summary>
    /// Commentaire
    /// </summary>
    [StringLength(1000)]
    public string? Commentaire { get; set; }

    /// <summary>
    /// Utilisateur ayant créé l'inventaire
    /// </summary>
    [Required]
    public Guid UtilisateurId { get; set; }

    public DateTime DateCreation { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey(nameof(BoutiqueId))]
    public virtual Boutique Boutique { get; set; } = null!;

    [ForeignKey(nameof(DepotId))]
    public virtual Depot Depot { get; set; } = null!;

    [ForeignKey(nameof(UtilisateurId))]
    public virtual ApplicationUser Utilisateur { get; set; } = null!;

    public virtual ICollection<LigneInventaire> LignesInventaire { get; set; } = new List<LigneInventaire>();
    public virtual ICollection<MouvementStock> MouvementsStock { get; set; } = new List<MouvementStock>();
}

