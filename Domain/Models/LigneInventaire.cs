using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace G_StockVente.Domain.Models;

/// <summary>
/// Ligne d'un inventaire (comptage d'un produit)
/// </summary>
public class LigneInventaire
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Inventaire
    /// </summary>
    [Required]
    public Guid InventaireId { get; set; }

    /// <summary>
    /// Produit paramétré pour la boutique
    /// </summary>
    [Required]
    public Guid ProduitBoutiqueId { get; set; }

    /// <summary>
    /// Quantité théorique (selon le système)
    /// </summary>
    [Range(0, int.MaxValue)]
    public int QuantiteTheorique { get; set; }

    /// <summary>
    /// Quantité réelle (comptée)
    /// </summary>
    [Range(0, int.MaxValue)]
    public int QuantiteReelle { get; set; }

    /// <summary>
    /// Écart (réel - théorique)
    /// </summary>
    public int Ecart => QuantiteReelle - QuantiteTheorique;

    /// <summary>
    /// Commentaire sur l'écart
    /// </summary>
    [StringLength(500)]
    public string? CommentaireEcart { get; set; }

    // Navigation properties
    [ForeignKey(nameof(InventaireId))]
    public virtual Inventaire Inventaire { get; set; } = null!;

    [ForeignKey(nameof(ProduitBoutiqueId))]
    public virtual ProduitBoutique ProduitBoutique { get; set; } = null!;
}

