using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace G_StockVente.Domain.Models;

/// <summary>
/// Produit global (référence commune au réseau)
/// </summary>
public class Produit
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required(ErrorMessage = "Le nom du produit est obligatoire")]
    [StringLength(200)]
    public string Nom { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// Catégorie du produit
    /// </summary>
    [Required]
    public Guid CategorieId { get; set; }

    public DateTime DateCreation { get; set; } = DateTime.UtcNow;
    public DateTime? DateModification { get; set; }

    // Navigation properties
    [ForeignKey(nameof(CategorieId))]
    public virtual Categorie Categorie { get; set; } = null!;

    public virtual ICollection<ProduitBoutique> ProduitsBoutique { get; set; } = new List<ProduitBoutique>();
}

