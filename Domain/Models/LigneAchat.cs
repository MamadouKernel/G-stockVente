using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace G_StockVente.Domain.Models;

/// <summary>
/// Ligne d'un achat (produit acheté)
/// </summary>
public class LigneAchat
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Achat
    /// </summary>
    [Required]
    public Guid AchatId { get; set; }

    /// <summary>
    /// Produit paramétré pour la boutique
    /// </summary>
    [Required]
    public Guid ProduitBoutiqueId { get; set; }

    /// <summary>
    /// Quantité commandée
    /// </summary>
    [Range(1, int.MaxValue)]
    public int QuantiteCommandee { get; set; }

    /// <summary>
    /// Quantité reçue
    /// </summary>
    [Range(0, int.MaxValue)]
    public int QuantiteRecue { get; set; } = 0;

    /// <summary>
    /// Prix unitaire d'achat
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    [Range(0, double.MaxValue)]
    public decimal PrixUnitaire { get; set; }

    /// <summary>
    /// Taux de TVA
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
    [ForeignKey(nameof(AchatId))]
    public virtual Achat Achat { get; set; } = null!;

    [ForeignKey(nameof(ProduitBoutiqueId))]
    public virtual ProduitBoutique ProduitBoutique { get; set; } = null!;
}

