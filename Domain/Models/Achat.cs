using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace G_StockVente.Domain.Models;

/// <summary>
/// Statut d'un achat
/// </summary>
public enum StatutAchat
{
    EnAttente = 1,
    EnReception = 2,
    Receptionne = 3,
    Annule = 4
}

/// <summary>
/// Achat fournisseur
/// </summary>
public class Achat
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Numéro unique d'achat pour cette boutique
    /// </summary>
    [Required]
    [StringLength(50)]
    public string NumeroAchat { get; set; } = string.Empty;

    /// <summary>
    /// Numéro de facture fournisseur
    /// </summary>
    [StringLength(100)]
    public string? NumeroFactureFournisseur { get; set; }

    /// <summary>
    /// Boutique
    /// </summary>
    [Required]
    public Guid BoutiqueId { get; set; }

    /// <summary>
    /// Fournisseur
    /// </summary>
    [Required]
    public Guid FournisseurId { get; set; }

    /// <summary>
    /// Statut de l'achat
    /// </summary>
    [Required]
    public StatutAchat Statut { get; set; } = StatutAchat.EnAttente;

    /// <summary>
    /// Date de commande
    /// </summary>
    public DateTime DateCommande { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date de réception prévue
    /// </summary>
    public DateTime? DateReceptionPrevue { get; set; }

    /// <summary>
    /// Date de réception effective
    /// </summary>
    public DateTime? DateReception { get; set; }

    /// <summary>
    /// Montant total HT
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    [Range(0, double.MaxValue)]
    public decimal MontantHT { get; set; }

    /// <summary>
    /// Montant total TVA
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    [Range(0, double.MaxValue)]
    public decimal MontantTVA { get; set; }

    /// <summary>
    /// Montant total TTC
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    [Range(0, double.MaxValue)]
    public decimal MontantTTC { get; set; }

    /// <summary>
    /// Commentaire ou note
    /// </summary>
    [StringLength(1000)]
    public string? Commentaire { get; set; }

    /// <summary>
    /// Utilisateur ayant créé l'achat
    /// </summary>
    [Required]
    public Guid UtilisateurId { get; set; }

    public DateTime DateCreation { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey(nameof(BoutiqueId))]
    public virtual Boutique Boutique { get; set; } = null!;

    [ForeignKey(nameof(FournisseurId))]
    public virtual Fournisseur Fournisseur { get; set; } = null!;

    [ForeignKey(nameof(UtilisateurId))]
    public virtual ApplicationUser Utilisateur { get; set; } = null!;

    public virtual ICollection<LigneAchat> LignesAchat { get; set; } = new List<LigneAchat>();
    public virtual ICollection<MouvementStock> MouvementsStock { get; set; } = new List<MouvementStock>();
}

