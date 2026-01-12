using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace G_StockVente.Domain.Models;

/// <summary>
/// Stock d'un produit dans un dépôt spécifique
/// </summary>
public class Stock
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Produit paramétré pour la boutique
    /// </summary>
    [Required]
    public Guid ProduitBoutiqueId { get; set; }

    /// <summary>
    /// Dépôt
    /// </summary>
    [Required]
    public Guid DepotId { get; set; }

    /// <summary>
    /// Quantité en stock
    /// </summary>
    [Range(0, int.MaxValue)]
    public int Quantite { get; set; } = 0;

    /// <summary>
    /// Date de dernière mise à jour
    /// </summary>
    public DateTime DateDerniereMaj { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey(nameof(ProduitBoutiqueId))]
    public virtual ProduitBoutique ProduitBoutique { get; set; } = null!;

    [ForeignKey(nameof(DepotId))]
    public virtual Depot Depot { get; set; } = null!;
}

