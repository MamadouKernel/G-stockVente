using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace G_StockVente.Domain.Models;

/// <summary>
/// Statut d'un transfert de stock
/// </summary>
public enum StatutTransfert
{
    EnAttente = 1,
    Valide = 2,
    EnTransit = 3,
    Reçu = 4,
    Annule = 5
}

/// <summary>
/// Transfert de stock entre dépôts (intra-boutique ou inter-boutiques)
/// </summary>
public class TransfertStock
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Numéro unique de transfert
    /// Format: TRANSFERT-{BoutiqueId}-{Numéro}
    /// </summary>
    [Required]
    [StringLength(50)]
    public string Numero { get; set; } = string.Empty;

    /// <summary>
    /// Dépôt source
    /// </summary>
    [Required]
    public Guid DepotSourceId { get; set; }

    /// <summary>
    /// Boutique source (pour transferts inter-boutiques)
    /// </summary>
    public Guid? BoutiqueSourceId { get; set; }

    /// <summary>
    /// Dépôt destination
    /// </summary>
    [Required]
    public Guid DepotDestinationId { get; set; }

    /// <summary>
    /// Boutique destination (pour transferts inter-boutiques)
    /// </summary>
    public Guid? BoutiqueDestinationId { get; set; }

    /// <summary>
    /// Statut du transfert
    /// </summary>
    [Required]
    public StatutTransfert Statut { get; set; } = StatutTransfert.EnAttente;

    /// <summary>
    /// Utilisateur ayant créé le transfert
    /// </summary>
    [Required]
    public Guid UtilisateurCreateurId { get; set; }

    /// <summary>
    /// Utilisateur ayant validé le transfert
    /// </summary>
    public Guid? UtilisateurValidateurId { get; set; }

    /// <summary>
    /// Utilisateur ayant reçu le transfert
    /// </summary>
    public Guid? UtilisateurRecepteurId { get; set; }

    /// <summary>
    /// Date de création
    /// </summary>
    public DateTime DateCreation { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date de validation
    /// </summary>
    public DateTime? DateValidation { get; set; }

    /// <summary>
    /// Date de réception
    /// </summary>
    public DateTime? DateReception { get; set; }

    /// <summary>
    /// Date d'annulation
    /// </summary>
    public DateTime? DateAnnulation { get; set; }

    /// <summary>
    /// Notes ou commentaires
    /// </summary>
    [StringLength(1000)]
    public string? Notes { get; set; }

    // Navigation properties
    [ForeignKey(nameof(DepotSourceId))]
    public virtual Depot DepotSource { get; set; } = null!;

    [ForeignKey(nameof(DepotDestinationId))]
    public virtual Depot DepotDestination { get; set; } = null!;

    [ForeignKey(nameof(BoutiqueSourceId))]
    public virtual Boutique? BoutiqueSource { get; set; }

    [ForeignKey(nameof(BoutiqueDestinationId))]
    public virtual Boutique? BoutiqueDestination { get; set; }

    [ForeignKey(nameof(UtilisateurCreateurId))]
    public virtual ApplicationUser UtilisateurCreateur { get; set; } = null!;

    [ForeignKey(nameof(UtilisateurValidateurId))]
    public virtual ApplicationUser? UtilisateurValidateur { get; set; }

    [ForeignKey(nameof(UtilisateurRecepteurId))]
    public virtual ApplicationUser? UtilisateurRecepteur { get; set; }

    public virtual ICollection<LigneTransfertStock> LignesTransfert { get; set; } = new List<LigneTransfertStock>();
}

/// <summary>
/// Ligne d'un transfert de stock
/// </summary>
public class LigneTransfertStock
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Transfert parent
    /// </summary>
    [Required]
    public Guid TransfertStockId { get; set; }

    /// <summary>
    /// Produit paramétré pour la boutique
    /// </summary>
    [Required]
    public Guid ProduitBoutiqueId { get; set; }

    /// <summary>
    /// Quantité à transférer
    /// </summary>
    [Range(1, int.MaxValue)]
    public int Quantite { get; set; }

    /// <summary>
    /// Quantité réellement reçue (peut différer de Quantite)
    /// </summary>
    [Range(0, int.MaxValue)]
    public int? QuantiteRecue { get; set; }

    // Navigation properties
    [ForeignKey(nameof(TransfertStockId))]
    public virtual TransfertStock TransfertStock { get; set; } = null!;

    [ForeignKey(nameof(ProduitBoutiqueId))]
    public virtual ProduitBoutique ProduitBoutique { get; set; } = null!;
}

