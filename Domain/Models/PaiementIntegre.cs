using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace G_StockVente.Domain.Models;

/// <summary>
/// Type de paiement intégré
/// </summary>
public enum TypePaiementIntegre
{
    MobileMoney = 1,
    CarteBancaire = 2
}

/// <summary>
/// Statut d'un paiement intégré
/// </summary>
public enum StatutPaiement
{
    EnAttente = 1,
    Valide = 2,
    Echec = 3,
    Rembourse = 4
}

/// <summary>
/// Paiement intégré (Mobile Money, Carte bancaire)
/// </summary>
public class PaiementIntegre
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Vente associée
    /// </summary>
    [Required]
    public Guid VenteId { get; set; }

    /// <summary>
    /// Type de paiement
    /// </summary>
    [Required]
    public TypePaiementIntegre Type { get; set; }

    /// <summary>
    /// Référence externe du paiement (fournie par le prestataire)
    /// </summary>
    [Required]
    [StringLength(200)]
    public string ReferenceExterne { get; set; } = string.Empty;

    /// <summary>
    /// Montant du paiement
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    [Range(0, double.MaxValue)]
    public decimal Montant { get; set; }

    /// <summary>
    /// Statut du paiement
    /// </summary>
    [Required]
    public StatutPaiement Statut { get; set; } = StatutPaiement.EnAttente;

    /// <summary>
    /// Date de création
    /// </summary>
    public DateTime DateCreation { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date de validation
    /// </summary>
    public DateTime? DateValidation { get; set; }

    /// <summary>
    /// Données de réponse du prestataire (JSON)
    /// </summary>
    [Column(TypeName = "text")]
    public string? DonneesReponse { get; set; }

    /// <summary>
    /// Message d'erreur (si échec)
    /// </summary>
    [StringLength(1000)]
    public string? MessageErreur { get; set; }

    // Navigation properties
    [ForeignKey(nameof(VenteId))]
    public virtual Vente Vente { get; set; } = null!;
}

