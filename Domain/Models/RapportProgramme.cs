using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace G_StockVente.Domain.Models;

/// <summary>
/// Type de rapport
/// </summary>
public enum TypeRapport
{
    Ventes = 1,
    Stocks = 2,
    Achats = 3,
    Consolidé = 4
}

/// <summary>
/// Fréquence d'exécution d'un rapport programmé
/// </summary>
public enum FrequenceRapport
{
    Journalier = 1,
    Hebdomadaire = 2,
    Mensuel = 3
}

/// <summary>
/// Rapport programmé (exécution automatique)
/// </summary>
public class RapportProgramme
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Boutique concernée (null = rapport consolidé réseau)
    /// </summary>
    public Guid? BoutiqueId { get; set; }

    /// <summary>
    /// Type de rapport
    /// </summary>
    [Required]
    public TypeRapport TypeRapport { get; set; }

    /// <summary>
    /// Fréquence d'exécution
    /// </summary>
    [Required]
    public FrequenceRapport Frequence { get; set; }

    /// <summary>
    /// Email destinataire
    /// </summary>
    [Required]
    [StringLength(200)]
    [EmailAddress]
    public string EmailDestinataire { get; set; } = string.Empty;

    /// <summary>
    /// Indique si le rapport programmé est actif
    /// </summary>
    public bool EstActif { get; set; } = true;

    /// <summary>
    /// Date de création
    /// </summary>
    public DateTime DateCreation { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date de dernière exécution
    /// </summary>
    public DateTime? DerniereExecution { get; set; }

    /// <summary>
    /// Date de prochaine exécution prévue
    /// </summary>
    public DateTime? ProchaineExecution { get; set; }

    /// <summary>
    /// Utilisateur ayant créé le rapport programmé
    /// </summary>
    [Required]
    public Guid UtilisateurCreateurId { get; set; }

    // Navigation properties
    [ForeignKey(nameof(BoutiqueId))]
    public virtual Boutique? Boutique { get; set; }

    [ForeignKey(nameof(UtilisateurCreateurId))]
    public virtual ApplicationUser UtilisateurCreateur { get; set; } = null!;
}

