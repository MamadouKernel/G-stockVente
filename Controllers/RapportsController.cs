using G_StockVente.Domain.Models;
using G_StockVente.Infrastructure.Data;
using G_StockVente.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace G_StockVente.Controllers;

[Authorize]
public class RapportsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IBoutiqueActiveService _boutiqueActiveService;

    public RapportsController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IBoutiqueActiveService boutiqueActiveService)
    {
        _context = context;
        _userManager = userManager;
        _boutiqueActiveService = boutiqueActiveService;
    }

    // GET: Rapports
    public IActionResult Index()
    {
        return View();
    }

    // GET: Rapports/Ventes
    public async Task<IActionResult> Ventes(DateTime? dateDebut, DateTime? dateFin, Guid? boutiqueId, string? format)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var isAdminReseau = await _userManager.IsInRoleAsync(user, "AdminReseau");
        var userBoutiqueId = await _boutiqueActiveService.GetBoutiqueActiveIdAsync(user.Id);

        // Par défaut : ce mois
        if (!dateDebut.HasValue)
        {
            dateDebut = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        }
        else
        {
            // S'assurer que la date est en UTC
            if (dateDebut.Value.Kind != DateTimeKind.Utc)
            {
                dateDebut = DateTime.SpecifyKind(dateDebut.Value, DateTimeKind.Utc);
            }
        }

        if (!dateFin.HasValue)
        {
            var nowUtc = DateTime.UtcNow;
            var todayStart = new DateTime(nowUtc.Year, nowUtc.Month, nowUtc.Day, 0, 0, 0, DateTimeKind.Utc);
            dateFin = todayStart.AddDays(1).AddTicks(-1);
        }
        else
        {
            // S'assurer que la date est en UTC
            var dateFinUtc = dateFin.Value.Kind == DateTimeKind.Utc 
                ? dateFin.Value 
                : DateTime.SpecifyKind(dateFin.Value, DateTimeKind.Utc);
            
            // Ajouter une journée complète pour inclure toute la journée (fin de journée)
            dateFin = new DateTime(dateFinUtc.Year, dateFinUtc.Month, dateFinUtc.Day, 23, 59, 59, 999, DateTimeKind.Utc).AddTicks(9999);
        }

        IQueryable<Vente> query = _context.Ventes
            .Include(v => v.Boutique)
            .Include(v => v.Utilisateur)
            .Include(v => v.LignesVente)
                .ThenInclude(lv => lv.ProduitBoutique)
                    .ThenInclude(pb => pb.Produit)
            .Where(v => v.DateVente >= dateDebut.Value && v.DateVente <= dateFin.Value && v.Statut == StatutVente.Validee);

        // Filtre par boutique
        if (!isAdminReseau && userBoutiqueId.HasValue)
        {
            query = query.Where(v => v.BoutiqueId == userBoutiqueId.Value);
        }
        else if (boutiqueId.HasValue)
        {
            query = query.Where(v => v.BoutiqueId == boutiqueId.Value);
        }

        var ventes = await query.OrderByDescending(v => v.DateVente).ToListAsync();

        // Statistiques
        var totalHT = ventes.Sum(v => v.MontantHT);
        var totalTVA = ventes.Sum(v => v.MontantTVA);
        var totalTTC = ventes.Sum(v => v.MontantTTC);
        var nombreVentes = ventes.Count;

        // Ventes par boutique
        var ventesParBoutique = ventes
            .GroupBy(v => new { v.BoutiqueId, v.Boutique.Nom })
            .Select(g => new
            {
                BoutiqueNom = g.Key.Nom,
                NombreVentes = g.Count(),
                MontantHT = g.Sum(v => v.MontantHT),
                MontantTVA = g.Sum(v => v.MontantTVA),
                MontantTTC = g.Sum(v => v.MontantTTC)
            })
            .OrderByDescending(x => x.MontantTTC)
            .ToList();

        // Ventes par produit
        var ventesParProduit = ventes
            .SelectMany(v => v.LignesVente)
            .GroupBy(lv => new { lv.ProduitBoutique.Produit.Id, lv.ProduitBoutique.Produit.Nom })
            .Select(g => new
            {
                ProduitNom = g.Key.Nom,
                QuantiteVendue = g.Sum(lv => lv.Quantite),
                MontantTotal = g.Sum(lv => lv.MontantTTC)
            })
            .OrderByDescending(x => x.QuantiteVendue)
            .Take(20)
            .ToList();

        ViewBag.DateDebut = dateDebut.Value;
        ViewBag.DateFin = dateFin.Value;
        ViewBag.Ventes = ventes;
        ViewBag.TotalHT = totalHT;
        ViewBag.TotalTVA = totalTVA;
        ViewBag.TotalTTC = totalTTC;
        ViewBag.NombreVentes = nombreVentes;
        ViewBag.VentesParBoutique = ventesParBoutique;
        ViewBag.VentesParProduit = ventesParProduit;
        ViewBag.Boutiques = await _context.Boutiques.Where(b => b.EstActive).OrderBy(b => b.Nom).ToListAsync();
        ViewBag.IsAdminReseau = isAdminReseau;

        // Export CSV
        if (format == "csv")
        {
            return ExportVentesCSV(ventes);
        }

        return View();
    }

    // GET: Rapports/Stocks
    public async Task<IActionResult> Stocks(Guid? boutiqueId, Guid? depotId, bool? stockBas)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var isAdminReseau = await _userManager.IsInRoleAsync(user, "AdminReseau");
        var userBoutiqueId = await _boutiqueActiveService.GetBoutiqueActiveIdAsync(user.Id);

        IQueryable<Stock> query = _context.Stocks
            .Include(s => s.ProduitBoutique)
                .ThenInclude(pb => pb.Produit)
            .Include(s => s.Depot)
                .ThenInclude(d => d.Boutique);

        // Filtre par boutique
        if (!isAdminReseau && userBoutiqueId.HasValue)
        {
            query = query.Where(s => s.Depot.BoutiqueId == userBoutiqueId.Value);
        }
        else if (boutiqueId.HasValue)
        {
            query = query.Where(s => s.Depot.BoutiqueId == boutiqueId.Value);
        }

        // Filtre par dépôt
        if (depotId.HasValue)
        {
            query = query.Where(s => s.DepotId == depotId.Value);
        }

        // Filtre stock bas
        if (stockBas == true)
        {
            query = query.Where(s => s.Quantite <= s.ProduitBoutique.SeuilStockBas && s.ProduitBoutique.EstActif);
        }

        var stocks = await query.OrderBy(s => s.ProduitBoutique.Produit.Nom).ToListAsync();

        // Statistiques
        var totalValeurStock = stocks.Sum(s => s.Quantite * s.ProduitBoutique.PrixAchat);
        var nombreProduits = stocks.Count;
        var nombreStockBas = stocks.Count(s => s.Quantite <= s.ProduitBoutique.SeuilStockBas && s.ProduitBoutique.EstActif);

        ViewBag.Stocks = stocks;
        ViewBag.TotalValeurStock = totalValeurStock;
        ViewBag.NombreProduits = nombreProduits;
        ViewBag.NombreStockBas = nombreStockBas;
        ViewBag.Boutiques = await _context.Boutiques.Where(b => b.EstActive).OrderBy(b => b.Nom).ToListAsync();
        ViewBag.Depots = await _context.Depots.Where(d => d.EstActif).OrderBy(d => d.Nom).ToListAsync();
        ViewBag.IsAdminReseau = isAdminReseau;

        return View();
    }

    // GET: Rapports/Achats
    public async Task<IActionResult> Achats(DateTime? dateDebut, DateTime? dateFin, Guid? boutiqueId, Guid? fournisseurId, StatutAchat? statut)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var isAdminReseau = await _userManager.IsInRoleAsync(user, "AdminReseau");
        var userBoutiqueId = await _boutiqueActiveService.GetBoutiqueActiveIdAsync(user.Id);

        // Par défaut : ce mois
        if (!dateDebut.HasValue)
        {
            dateDebut = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        }
        else
        {
            // S'assurer que la date est en UTC
            if (dateDebut.Value.Kind != DateTimeKind.Utc)
            {
                dateDebut = DateTime.SpecifyKind(dateDebut.Value, DateTimeKind.Utc);
            }
        }

        if (!dateFin.HasValue)
        {
            var nowUtc = DateTime.UtcNow;
            var todayStart = new DateTime(nowUtc.Year, nowUtc.Month, nowUtc.Day, 0, 0, 0, DateTimeKind.Utc);
            dateFin = todayStart.AddDays(1).AddTicks(-1);
        }
        else
        {
            // S'assurer que la date est en UTC
            var dateFinUtc = dateFin.Value.Kind == DateTimeKind.Utc 
                ? dateFin.Value 
                : DateTime.SpecifyKind(dateFin.Value, DateTimeKind.Utc);
            
            // Ajouter une journée complète pour inclure toute la journée (fin de journée)
            dateFin = new DateTime(dateFinUtc.Year, dateFinUtc.Month, dateFinUtc.Day, 23, 59, 59, 999, DateTimeKind.Utc).AddTicks(9999);
        }

        IQueryable<Achat> query = _context.Achats
            .Include(a => a.Boutique)
            .Include(a => a.Fournisseur)
            .Include(a => a.Utilisateur)
            .Include(a => a.LignesAchat)
                .ThenInclude(la => la.ProduitBoutique)
                    .ThenInclude(pb => pb.Produit)
            .Where(a => a.DateCommande >= dateDebut.Value && a.DateCommande <= dateFin.Value);

        // Filtre par boutique
        if (!isAdminReseau && userBoutiqueId.HasValue)
        {
            query = query.Where(a => a.BoutiqueId == userBoutiqueId.Value);
        }
        else if (boutiqueId.HasValue)
        {
            query = query.Where(a => a.BoutiqueId == boutiqueId.Value);
        }

        // Filtre par fournisseur
        if (fournisseurId.HasValue)
        {
            query = query.Where(a => a.FournisseurId == fournisseurId.Value);
        }

        // Filtre par statut
        if (statut.HasValue)
        {
            query = query.Where(a => a.Statut == statut.Value);
        }

        var achats = await query.OrderByDescending(a => a.DateCommande).ToListAsync();

        // Statistiques
        var totalHT = achats.Sum(a => a.MontantHT);
        var totalTVA = achats.Sum(a => a.MontantTVA);
        var totalTTC = achats.Sum(a => a.MontantTTC);
        var nombreAchats = achats.Count;

        // Achats par fournisseur
        var achatsParFournisseur = achats
            .GroupBy(a => new { a.FournisseurId, a.Fournisseur.Nom })
            .Select(g => new
            {
                FournisseurNom = g.Key.Nom,
                NombreAchats = g.Count(),
                MontantHT = g.Sum(a => a.MontantHT),
                MontantTVA = g.Sum(a => a.MontantTVA),
                MontantTTC = g.Sum(a => a.MontantTTC)
            })
            .OrderByDescending(x => x.MontantTTC)
            .ToList();

        ViewBag.DateDebut = dateDebut.Value;
        ViewBag.DateFin = dateFin.Value;
        ViewBag.Achats = achats;
        ViewBag.TotalHT = totalHT;
        ViewBag.TotalTVA = totalTVA;
        ViewBag.TotalTTC = totalTTC;
        ViewBag.NombreAchats = nombreAchats;
        ViewBag.AchatsParFournisseur = achatsParFournisseur;
        ViewBag.Boutiques = await _context.Boutiques.Where(b => b.EstActive).OrderBy(b => b.Nom).ToListAsync();
        ViewBag.Fournisseurs = await _context.Fournisseurs.Where(f => f.EstActif).OrderBy(f => f.Nom).ToListAsync();
        ViewBag.Statuts = Enum.GetValues(typeof(StatutAchat)).Cast<StatutAchat>();
        ViewBag.IsAdminReseau = isAdminReseau;

        return View();
    }

    // Export CSV des ventes
    private IActionResult ExportVentesCSV(List<Vente> ventes)
    {
        var csv = new StringBuilder();
        csv.AppendLine("Numéro;Date;Boutique;Caissier;Montant HT;TVA;Montant TTC;Mode Paiement");

        foreach (var vente in ventes)
        {
            csv.AppendLine($"{vente.NumeroVente};" +
                          $"{vente.DateVente:dd/MM/yyyy HH:mm};" +
                          $"{vente.Boutique.Nom};" +
                          $"{vente.Utilisateur.Prenom} {vente.Utilisateur.Nom};" +
                          $"{vente.MontantHT:F2};" +
                          $"{vente.MontantTVA:F2};" +
                          $"{vente.MontantTTC:F2};" +
                          $"{vente.ModePaiement}");
        }

        var bytes = Encoding.UTF8.GetBytes(csv.ToString());
        return File(bytes, "text/csv", $"rapport_ventes_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
    }
}

