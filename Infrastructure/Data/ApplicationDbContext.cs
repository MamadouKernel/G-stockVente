using G_StockVente.Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace G_StockVente.Infrastructure.Data;

/// <summary>
/// Contexte de base de données principal
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // DbSets
    public DbSet<Boutique> Boutiques { get; set; }
    public DbSet<Depot> Depots { get; set; }
    public DbSet<Categorie> Categories { get; set; }
    public DbSet<Produit> Produits { get; set; }
    public DbSet<ProduitBoutique> ProduitsBoutique { get; set; }
    public DbSet<Stock> Stocks { get; set; }
    public DbSet<MouvementStock> MouvementsStock { get; set; }
    public DbSet<Vente> Ventes { get; set; }
    public DbSet<LigneVente> LignesVente { get; set; }
    public DbSet<Fournisseur> Fournisseurs { get; set; }
    public DbSet<Achat> Achats { get; set; }
    public DbSet<LigneAchat> LignesAchat { get; set; }
    public DbSet<Inventaire> Inventaires { get; set; }
    public DbSet<LigneInventaire> LignesInventaire { get; set; }
    
    // V2 - Nouvelles entités
    public DbSet<TransfertStock> TransfertsStock { get; set; }
    public DbSet<LigneTransfertStock> LignesTransfertStock { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<RapportProgramme> RapportsProgrammes { get; set; }
    public DbSet<JournalConnexion> JournalConnexions { get; set; }
    public DbSet<PaiementIntegre> PaiementsIntegres { get; set; }
    public DbSet<Delegation> Delegations { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configuration de la génération automatique des GUIDs pour toutes les entités
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            var idProperty = entityType.FindProperty("Id");
            if (idProperty != null && idProperty.ClrType == typeof(Guid))
            {
                idProperty.SetDefaultValueSql("gen_random_uuid()");
            }
        }

        // Configuration des relations et contraintes

        // Boutique
        builder.Entity<Boutique>(entity =>
        {
            entity.HasIndex(e => e.Nom).IsUnique();
        });

        // Depot
        builder.Entity<Depot>(entity =>
        {
            entity.HasIndex(e => new { e.BoutiqueId, e.Nom }).IsUnique();
            entity.HasOne(d => d.Boutique)
                .WithMany(b => b.Depots)
                .HasForeignKey(d => d.BoutiqueId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Categorie
        builder.Entity<Categorie>(entity =>
        {
            entity.HasOne(c => c.CategorieParente)
                .WithMany(c => c.SousCategories)
                .HasForeignKey(c => c.CategorieParenteId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Produit
        builder.Entity<Produit>(entity =>
        {
            entity.HasOne(p => p.Categorie)
                .WithMany(c => c.Produits)
                .HasForeignKey(p => p.CategorieId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ProduitBoutique - SKU et CodeBarres uniques par boutique
        builder.Entity<ProduitBoutique>(entity =>
        {
            entity.HasIndex(e => new { e.BoutiqueId, e.Sku }).IsUnique()
                .HasFilter("\"Sku\" IS NOT NULL AND \"Sku\" != ''");
            entity.HasIndex(e => new { e.BoutiqueId, e.CodeBarres }).IsUnique()
                .HasFilter("\"CodeBarres\" IS NOT NULL AND \"CodeBarres\" != ''");
            entity.HasOne(pb => pb.Produit)
                .WithMany(p => p.ProduitsBoutique)
                .HasForeignKey(pb => pb.ProduitId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(pb => pb.Boutique)
                .WithMany(b => b.ProduitsBoutique)
                .HasForeignKey(pb => pb.BoutiqueId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Stock - Un seul stock par produit/dépôt
        builder.Entity<Stock>(entity =>
        {
            entity.HasIndex(e => new { e.ProduitBoutiqueId, e.DepotId }).IsUnique();
            entity.HasOne(s => s.ProduitBoutique)
                .WithMany(pb => pb.Stocks)
                .HasForeignKey(s => s.ProduitBoutiqueId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(s => s.Depot)
                .WithMany(d => d.Stocks)
                .HasForeignKey(s => s.DepotId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // MouvementStock
        builder.Entity<MouvementStock>(entity =>
        {
            entity.HasOne(m => m.ProduitBoutique)
                .WithMany(pb => pb.MouvementsStock)
                .HasForeignKey(m => m.ProduitBoutiqueId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(m => m.Depot)
                .WithMany(d => d.MouvementsStock)
                .HasForeignKey(m => m.DepotId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(m => m.Utilisateur)
                .WithMany(u => u.MouvementsStock)
                .HasForeignKey(m => m.UtilisateurId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(m => m.TransfertStock)
                .WithMany()
                .HasForeignKey(m => m.TransfertStockId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Vente - Numéro unique par boutique
        builder.Entity<Vente>(entity =>
        {
            entity.HasIndex(e => new { e.BoutiqueId, e.NumeroVente }).IsUnique();
            entity.HasOne(v => v.Boutique)
                .WithMany(b => b.Ventes)
                .HasForeignKey(v => v.BoutiqueId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(v => v.Utilisateur)
                .WithMany(u => u.Ventes)
                .HasForeignKey(v => v.UtilisateurId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Achat - Numéro unique par boutique
        builder.Entity<Achat>(entity =>
        {
            entity.HasIndex(e => new { e.BoutiqueId, e.NumeroAchat }).IsUnique();
            entity.HasOne(a => a.Boutique)
                .WithMany(b => b.Achats)
                .HasForeignKey(a => a.BoutiqueId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(a => a.Fournisseur)
                .WithMany(f => f.Achats)
                .HasForeignKey(a => a.FournisseurId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Inventaire - Numéro unique par boutique
        builder.Entity<Inventaire>(entity =>
        {
            entity.HasIndex(e => new { e.BoutiqueId, e.NumeroInventaire }).IsUnique();
            entity.HasOne(i => i.Boutique)
                .WithMany(b => b.Inventaires)
                .HasForeignKey(i => i.BoutiqueId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(i => i.Depot)
                .WithMany(d => d.Inventaires)
                .HasForeignKey(i => i.DepotId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ApplicationUser
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.HasOne(u => u.BoutiqueActive)
                .WithMany()
                .HasForeignKey(u => u.BoutiqueActiveId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Configuration des enums en string pour PostgreSQL
        builder.Entity<MouvementStock>()
            .Property(e => e.TypeMouvement)
            .HasConversion<string>();

        builder.Entity<Vente>()
            .Property(e => e.Statut)
            .HasConversion<string>();

        builder.Entity<Vente>()
            .Property(e => e.ModePaiement)
            .HasConversion<string>();

        builder.Entity<Achat>()
            .Property(e => e.Statut)
            .HasConversion<string>();

        builder.Entity<Inventaire>()
            .Property(e => e.Statut)
            .HasConversion<string>();

        // V2 - Configuration des nouvelles entités

        // TransfertStock - Numéro unique par boutique
        builder.Entity<TransfertStock>(entity =>
        {
            entity.HasIndex(e => new { e.BoutiqueSourceId, e.Numero }).IsUnique()
                .HasFilter("\"BoutiqueSourceId\" IS NOT NULL");
            entity.HasOne(t => t.DepotSource)
                .WithMany()
                .HasForeignKey(t => t.DepotSourceId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(t => t.DepotDestination)
                .WithMany()
                .HasForeignKey(t => t.DepotDestinationId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(t => t.BoutiqueSource)
                .WithMany()
                .HasForeignKey(t => t.BoutiqueSourceId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(t => t.BoutiqueDestination)
                .WithMany()
                .HasForeignKey(t => t.BoutiqueDestinationId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(t => t.UtilisateurCreateur)
                .WithMany()
                .HasForeignKey(t => t.UtilisateurCreateurId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // LigneTransfertStock
        builder.Entity<LigneTransfertStock>(entity =>
        {
            entity.HasOne(l => l.TransfertStock)
                .WithMany(t => t.LignesTransfert)
                .HasForeignKey(l => l.TransfertStockId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(l => l.ProduitBoutique)
                .WithMany()
                .HasForeignKey(l => l.ProduitBoutiqueId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Notification
        builder.Entity<Notification>(entity =>
        {
            entity.HasIndex(e => new { e.UtilisateurId, e.EstLue, e.DateCreation });
            entity.HasOne(n => n.Utilisateur)
                .WithMany()
                .HasForeignKey(n => n.UtilisateurId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(n => n.Boutique)
                .WithMany()
                .HasForeignKey(n => n.BoutiqueId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // RapportProgramme
        builder.Entity<RapportProgramme>(entity =>
        {
            entity.HasOne(r => r.Boutique)
                .WithMany()
                .HasForeignKey(r => r.BoutiqueId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(r => r.UtilisateurCreateur)
                .WithMany()
                .HasForeignKey(r => r.UtilisateurCreateurId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // JournalConnexion
        builder.Entity<JournalConnexion>(entity =>
        {
            entity.HasIndex(e => new { e.Email, e.DateTentative });
            entity.HasIndex(e => new { e.UtilisateurId, e.DateTentative });
            entity.HasOne(j => j.Utilisateur)
                .WithMany()
                .HasForeignKey(j => j.UtilisateurId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // PaiementIntegre
        builder.Entity<PaiementIntegre>(entity =>
        {
            entity.HasIndex(e => e.ReferenceExterne);
            entity.HasOne(p => p.Vente)
                .WithMany(v => v.PaiementsIntegres)
                .HasForeignKey(p => p.VenteId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configuration des enums V2 en string pour PostgreSQL
        builder.Entity<TransfertStock>()
            .Property(e => e.Statut)
            .HasConversion<string>();

        builder.Entity<Notification>()
            .Property(e => e.Type)
            .HasConversion<string>();

        builder.Entity<RapportProgramme>()
            .Property(e => e.TypeRapport)
            .HasConversion<string>();

        builder.Entity<RapportProgramme>()
            .Property(e => e.Frequence)
            .HasConversion<string>();

        builder.Entity<PaiementIntegre>()
            .Property(e => e.Type)
            .HasConversion<string>();

        builder.Entity<PaiementIntegre>()
            .Property(e => e.Statut)
            .HasConversion<string>();

        // Delegation
        builder.Entity<Delegation>(entity =>
        {
            entity.HasIndex(e => new { e.UtilisateurDelegantId, e.UtilisateurBeneficiaireId, e.RoleDelege, e.DateDebut, e.DateFin });
            entity.HasIndex(e => new { e.UtilisateurBeneficiaireId, e.EstActive, e.DateDebut, e.DateFin });
            entity.HasOne(d => d.UtilisateurDelegant)
                .WithMany()
                .HasForeignKey(d => d.UtilisateurDelegantId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(d => d.UtilisateurBeneficiaire)
                .WithMany()
                .HasForeignKey(d => d.UtilisateurBeneficiaireId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}

