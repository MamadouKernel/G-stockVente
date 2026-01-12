using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace G_StockVente.Domain.Models;

/// <summary>
/// Utilisateur de l'application (étend IdentityUser avec Guid)
/// </summary>
public class ApplicationUser : IdentityUser<Guid>
{
    [Required(ErrorMessage = "Le prénom est obligatoire")]
    [StringLength(100)]
    public string Prenom { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le nom est obligatoire")]
    [StringLength(100)]
    public string Nom { get; set; } = string.Empty;

    /// <summary>
    /// Boutique active de l'utilisateur (peut être null pour Admin Réseau)
    /// </summary>
    public Guid? BoutiqueActiveId { get; set; }

    /// <summary>
    /// Indique si l'utilisateur est actif
    /// </summary>
    public bool EstActif { get; set; } = true;

    /// <summary>
    /// Indique si l'utilisateur doit changer son mot de passe à la prochaine connexion
    /// </summary>
    public bool MustChangePassword { get; set; } = true;

    /// <summary>
    /// Indique si l'utilisateur est supprimé (soft delete)
    /// </summary>
    public bool EstSupprime { get; set; } = false;

    /// <summary>
    /// Date de suppression (soft delete)
    /// </summary>
    public DateTime? DateSuppression { get; set; }

    public DateTime DateCreation { get; set; } = DateTime.UtcNow;
    public DateTime? DateDerniereConnexion { get; set; }

    // Navigation properties
    [ForeignKey(nameof(BoutiqueActiveId))]
    public virtual Boutique? BoutiqueActive { get; set; }

    public virtual ICollection<Boutique> Boutiques { get; set; } = new List<Boutique>();
    public virtual ICollection<MouvementStock> MouvementsStock { get; set; } = new List<MouvementStock>();
    public virtual ICollection<Vente> Ventes { get; set; } = new List<Vente>();
    public virtual ICollection<Achat> Achats { get; set; } = new List<Achat>();
    public virtual ICollection<Inventaire> Inventaires { get; set; } = new List<Inventaire>();
}

