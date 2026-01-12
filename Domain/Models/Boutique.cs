using System.ComponentModel.DataAnnotations;

namespace G_StockVente.Domain.Models;

/// <summary>
/// Représente une boutique du réseau
/// </summary>
public class Boutique
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required(ErrorMessage = "Le nom de la boutique est obligatoire")]
    [StringLength(200)]
    public string Nom { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Adresse { get; set; }

    [StringLength(20)]
    public string? Telephone { get; set; }

    [StringLength(100)]
    public string? Email { get; set; }

    /// <summary>
    /// Logo de la boutique (chemin du fichier)
    /// </summary>
    [StringLength(500)]
    public string? Logo { get; set; }

    /// <summary>
    /// TVA par défaut pour cette boutique (en pourcentage)
    /// </summary>
    [Range(0, 100)]
    public decimal TvaParDefaut { get; set; } = 0;

    /// <summary>
    /// Indique si la boutique est active
    /// </summary>
    public bool EstActive { get; set; } = true;

    /// <summary>
    /// Date de création
    /// </summary>
    public DateTime DateCreation { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date de dernière modification
    /// </summary>
    public DateTime? DateModification { get; set; }

    // Navigation properties
    public virtual ICollection<Depot> Depots { get; set; } = new List<Depot>();
    public virtual ICollection<ProduitBoutique> ProduitsBoutique { get; set; } = new List<ProduitBoutique>();
    public virtual ICollection<Vente> Ventes { get; set; } = new List<Vente>();
    public virtual ICollection<Achat> Achats { get; set; } = new List<Achat>();
    public virtual ICollection<Inventaire> Inventaires { get; set; } = new List<Inventaire>();
    public virtual ICollection<ApplicationUser> Utilisateurs { get; set; } = new List<ApplicationUser>();
}

