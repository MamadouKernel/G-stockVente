using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace G_StockVente.Domain.Models;

/// <summary>
/// Type de mouvement de stock
/// </summary>
public enum TypeMouvementStock
{
    StockInitial = 1,
    Entree = 2,
    Sortie = 3,
    Ajustement = 4,
    Perte = 5,
    Casse = 6,
    Retour = 7,
    TransfertSortie = 8,
    TransfertEntree = 9
}

/// <summary>
/// Mouvement de stock (traçabilité complète)
/// </summary>
public class MouvementStock
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Produit paramétré pour la boutique
    /// </summary>
    [Required]
    public Guid ProduitBoutiqueId { get; set; }

    /// <summary>
    /// Dépôt concerné
    /// </summary>
    [Required]
    public Guid DepotId { get; set; }

    /// <summary>
    /// Type de mouvement
    /// </summary>
    [Required]
    public TypeMouvementStock TypeMouvement { get; set; }

    /// <summary>
    /// Quantité (positive pour entrées, négative pour sorties)
    /// </summary>
    public int Quantite { get; set; }

    /// <summary>
    /// Prix unitaire au moment du mouvement
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal PrixUnitaire { get; set; }

    /// <summary>
    /// Commentaire ou raison du mouvement
    /// </summary>
    [StringLength(1000)]
    public string? Commentaire { get; set; }

    /// <summary>
    /// Référence à la vente (si mouvement lié à une vente)
    /// </summary>
    public Guid? VenteId { get; set; }

    /// <summary>
    /// Référence à l'achat (si mouvement lié à un achat)
    /// </summary>
    public Guid? AchatId { get; set; }

    /// <summary>
    /// Référence à l'inventaire (si mouvement lié à un inventaire)
    /// </summary>
    public Guid? InventaireId { get; set; }

    /// <summary>
    /// Référence au transfert (si mouvement lié à un transfert)
    /// </summary>
    public Guid? TransfertStockId { get; set; }

    /// <summary>
    /// Utilisateur ayant effectué le mouvement
    /// </summary>
    [Required]
    public Guid UtilisateurId { get; set; }

    public DateTime DateMouvement { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey(nameof(ProduitBoutiqueId))]
    public virtual ProduitBoutique ProduitBoutique { get; set; } = null!;

    [ForeignKey(nameof(DepotId))]
    public virtual Depot Depot { get; set; } = null!;

    [ForeignKey(nameof(VenteId))]
    public virtual Vente? Vente { get; set; }

    [ForeignKey(nameof(AchatId))]
    public virtual Achat? Achat { get; set; }

    [ForeignKey(nameof(InventaireId))]
    public virtual Inventaire? Inventaire { get; set; }

    [ForeignKey(nameof(TransfertStockId))]
    public virtual TransfertStock? TransfertStock { get; set; }

    [ForeignKey(nameof(UtilisateurId))]
    public virtual ApplicationUser Utilisateur { get; set; } = null!;
}

