using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace G_StockVente.Domain.Models;

/// <summary>
/// Statut d'une vente
/// </summary>
public enum StatutVente
{
    EnCours = 1,
    Validee = 2,
    Annulee = 3
}

/// <summary>
/// Mode de paiement
/// </summary>
public enum ModePaiement
{
    Especes = 1,
    MobileMoney = 2,
    Carte = 3
}

/// <summary>
/// Vente (transaction de caisse)
/// </summary>
public class Vente
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Numéro unique de vente pour cette boutique
    /// </summary>
    [Required]
    [StringLength(50)]
    public string NumeroVente { get; set; } = string.Empty;

    /// <summary>
    /// Numéro de facture (si facturée)
    /// </summary>
    [StringLength(50)]
    public string? NumeroFacture { get; set; }

    /// <summary>
    /// Boutique
    /// </summary>
    [Required]
    public Guid BoutiqueId { get; set; }

    /// <summary>
    /// Statut de la vente
    /// </summary>
    [Required]
    public StatutVente Statut { get; set; } = StatutVente.Validee;

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
    /// Remise globale (en pourcentage ou montant fixe)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    [Range(0, double.MaxValue)]
    public decimal Remise { get; set; } = 0;

    /// <summary>
    /// Mode de paiement
    /// </summary>
    [Required]
    public ModePaiement ModePaiement { get; set; } = ModePaiement.Especes;

    /// <summary>
    /// Commentaire ou note
    /// </summary>
    [StringLength(1000)]
    public string? Commentaire { get; set; }

    /// <summary>
    /// Utilisateur ayant effectué la vente
    /// </summary>
    [Required]
    public Guid UtilisateurId { get; set; }

    public DateTime DateVente { get; set; } = DateTime.UtcNow;
    public DateTime? DateAnnulation { get; set; }

    /// <summary>
    /// Utilisateur ayant annulé (si annulée)
    /// </summary>
    public Guid? UtilisateurAnnulationId { get; set; }

    // Navigation properties
    [ForeignKey(nameof(BoutiqueId))]
    public virtual Boutique Boutique { get; set; } = null!;

    [ForeignKey(nameof(UtilisateurId))]
    public virtual ApplicationUser Utilisateur { get; set; } = null!;

    [ForeignKey(nameof(UtilisateurAnnulationId))]
    public virtual ApplicationUser? UtilisateurAnnulation { get; set; }

    public virtual ICollection<LigneVente> LignesVente { get; set; } = new List<LigneVente>();
    public virtual ICollection<MouvementStock> MouvementsStock { get; set; } = new List<MouvementStock>();
    public virtual ICollection<PaiementIntegre> PaiementsIntegres { get; set; } = new List<PaiementIntegre>();
}

