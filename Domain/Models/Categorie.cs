using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace G_StockVente.Domain.Models;

/// <summary>
/// Catégorie de produits (globale au réseau)
/// </summary>
public class Categorie
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required(ErrorMessage = "Le nom de la catégorie est obligatoire")]
    [StringLength(200)]
    public string Nom { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// Catégorie parente (pour les sous-catégories)
    /// </summary>
    public Guid? CategorieParenteId { get; set; }

    public DateTime DateCreation { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey(nameof(CategorieParenteId))]
    public virtual Categorie? CategorieParente { get; set; }

    public virtual ICollection<Categorie> SousCategories { get; set; } = new List<Categorie>();
    public virtual ICollection<Produit> Produits { get; set; } = new List<Produit>();
}

