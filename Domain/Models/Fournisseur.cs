using System.ComponentModel.DataAnnotations;

namespace G_StockVente.Domain.Models;

/// <summary>
/// Fournisseur
/// </summary>
public class Fournisseur
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required(ErrorMessage = "Le nom du fournisseur est obligatoire")]
    [StringLength(200)]
    public string Nom { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Adresse { get; set; }

    [StringLength(20)]
    public string? Telephone { get; set; }

    [StringLength(100)]
    [EmailAddress]
    public string? Email { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }

    /// <summary>
    /// Indique si le fournisseur est actif
    /// </summary>
    public bool EstActif { get; set; } = true;

    public DateTime DateCreation { get; set; } = DateTime.UtcNow;
    public DateTime? DateModification { get; set; }

    // Navigation properties
    public virtual ICollection<Achat> Achats { get; set; } = new List<Achat>();
}

