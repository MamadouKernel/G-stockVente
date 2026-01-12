using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace G_StockVente.Domain.Models;

/// <summary>
/// Paramétrage d'un produit pour une boutique spécifique
/// Permet d'avoir des SKU, codes-barres, prix différents par boutique
/// </summary>
public class ProduitBoutique
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Produit global
    /// </summary>
    [Required]
    public Guid ProduitId { get; set; }

    /// <summary>
    /// Boutique
    /// </summary>
    [Required]
    public Guid BoutiqueId { get; set; }

    /// <summary>
    /// SKU unique pour cette boutique
    /// </summary>
    [StringLength(100)]
    public string? Sku { get; set; }

    /// <summary>
    /// Code-barres ou QR code (unique pour cette boutique)
    /// </summary>
    [StringLength(100)]
    public string? CodeBarres { get; set; }

    /// <summary>
    /// Prix d'achat unitaire
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    [Range(0, double.MaxValue, ErrorMessage = "Le prix d'achat doit être positif")]
    public decimal PrixAchat { get; set; }

    /// <summary>
    /// Prix de vente unitaire
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    [Range(0, double.MaxValue, ErrorMessage = "Le prix de vente doit être positif")]
    public decimal PrixVente { get; set; }

    /// <summary>
    /// Seuil d'alerte de stock bas
    /// </summary>
    [Range(0, int.MaxValue)]
    public int SeuilStockBas { get; set; } = 0;

    /// <summary>
    /// Indique si le produit est actif dans cette boutique
    /// </summary>
    public bool EstActif { get; set; } = true;

    public DateTime DateCreation { get; set; } = DateTime.UtcNow;
    public DateTime? DateModification { get; set; }

    // Navigation properties
    [ForeignKey(nameof(ProduitId))]
    public virtual Produit Produit { get; set; } = null!;

    [ForeignKey(nameof(BoutiqueId))]
    public virtual Boutique Boutique { get; set; } = null!;

    public virtual ICollection<LigneVente> LignesVente { get; set; } = new List<LigneVente>();
    public virtual ICollection<LigneAchat> LignesAchat { get; set; } = new List<LigneAchat>();
    public virtual ICollection<Stock> Stocks { get; set; } = new List<Stock>();
    public virtual ICollection<MouvementStock> MouvementsStock { get; set; } = new List<MouvementStock>();
}

