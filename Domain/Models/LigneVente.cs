using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace G_StockVente.Domain.Models;

/// <summary>
/// Ligne d'une vente (produit vendu)
/// </summary>
public class LigneVente
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Vente
    /// </summary>
    [Required]
    public Guid VenteId { get; set; }

    /// <summary>
    /// Produit paramétré pour la boutique
    /// </summary>
    [Required]
    public Guid ProduitBoutiqueId { get; set; }

    /// <summary>
    /// Quantité vendue
    /// </summary>
    [Range(1, int.MaxValue)]
    public int Quantite { get; set; }

    /// <summary>
    /// Prix unitaire au moment de la vente
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    [Range(0, double.MaxValue)]
    public decimal PrixUnitaire { get; set; }

    /// <summary>
    /// Remise sur cette ligne (en pourcentage ou montant)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    [Range(0, double.MaxValue)]
    public decimal Remise { get; set; } = 0;

    /// <summary>
    /// Taux de TVA appliqué
    /// </summary>
    [Column(TypeName = "decimal(5,2)")]
    [Range(0, 100)]
    public decimal TauxTVA { get; set; } = 0;

    /// <summary>
    /// Montant HT de la ligne
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    [Range(0, double.MaxValue)]
    public decimal MontantHT { get; set; }

    /// <summary>
    /// Montant TVA de la ligne
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    [Range(0, double.MaxValue)]
    public decimal MontantTVA { get; set; }

    /// <summary>
    /// Montant TTC de la ligne
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    [Range(0, double.MaxValue)]
    public decimal MontantTTC { get; set; }

    // Navigation properties
    [ForeignKey(nameof(VenteId))]
    public virtual Vente Vente { get; set; } = null!;

    [ForeignKey(nameof(ProduitBoutiqueId))]
    public virtual ProduitBoutique ProduitBoutique { get; set; } = null!;
}

