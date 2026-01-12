using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace G_StockVente.Domain.Models;

/// <summary>
/// Représente un dépôt rattaché à une boutique
/// </summary>
public class Depot
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required(ErrorMessage = "Le nom du dépôt est obligatoire")]
    [StringLength(200)]
    public string Nom { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Adresse { get; set; }

    /// <summary>
    /// Boutique à laquelle appartient ce dépôt
    /// </summary>
    [Required]
    public Guid BoutiqueId { get; set; }

    /// <summary>
    /// Indique si le dépôt est actif
    /// </summary>
    public bool EstActif { get; set; } = true;

    public DateTime DateCreation { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey(nameof(BoutiqueId))]
    public virtual Boutique Boutique { get; set; } = null!;

    public virtual ICollection<MouvementStock> MouvementsStock { get; set; } = new List<MouvementStock>();
    public virtual ICollection<Stock> Stocks { get; set; } = new List<Stock>();
    public virtual ICollection<Inventaire> Inventaires { get; set; } = new List<Inventaire>();
}

