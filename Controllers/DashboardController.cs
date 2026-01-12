using G_StockVente.Domain.Models;
using G_StockVente.Infrastructure.Data;
using G_StockVente.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace G_StockVente.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IBoutiqueActiveService _boutiqueActiveService;

    public DashboardController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IBoutiqueActiveService boutiqueActiveService)
    {
        _context = context;
        _userManager = userManager;
        _boutiqueActiveService = boutiqueActiveService;
    }

    // GET: Dashboard
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var isAdminReseau = await _userManager.IsInRoleAsync(user, "AdminReseau");
        var boutiqueId = await _boutiqueActiveService.GetBoutiqueActiveIdAsync(user.Id);

        if (!isAdminReseau && boutiqueId == null)
        {
            return RedirectToAction("SelectionBoutique", "Boutiques");
        }

        if (isAdminReseau)
        {
            // Dashboard consolidé pour Admin Réseau
            return await DashboardReseau();
        }
        else
        {
            // Dashboard par boutique
            return await DashboardBoutique(boutiqueId.Value);
        }
    }

    private async Task<IActionResult> DashboardBoutique(Guid boutiqueId)
    {
        // IMPORTANT: PostgreSQL (timestamp with time zone) n'accepte que des DateTime en UTC (Kind=Utc)
        var nowUtc = DateTime.UtcNow;
        var todayStart = new DateTime(nowUtc.Year, nowUtc.Month, nowUtc.Day, 0, 0, 0, DateTimeKind.Utc);
        var todayEndExclusive = todayStart.AddDays(1);

        var thisMonth = new DateTime(todayStart.Year, todayStart.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var lastMonth = thisMonth.AddMonths(-1);

        // Statistiques ventes
        var ventesAujourdhui = await _context.Ventes
            .Where(v => v.BoutiqueId == boutiqueId
                        && v.DateVente >= todayStart
                        && v.DateVente < todayEndExclusive
                        && v.Statut == StatutVente.Validee)
            .CountAsync();

        var caAujourdhui = await _context.Ventes
            .Where(v => v.BoutiqueId == boutiqueId
                        && v.DateVente >= todayStart
                        && v.DateVente < todayEndExclusive
                        && v.Statut == StatutVente.Validee)
            .SumAsync(v => v.MontantTTC);

        var caMois = await _context.Ventes
            .Where(v => v.BoutiqueId == boutiqueId && v.DateVente >= thisMonth && v.Statut == StatutVente.Validee)
            .SumAsync(v => v.MontantTTC);

        var caMoisPrecedent = await _context.Ventes
            .Where(v => v.BoutiqueId == boutiqueId && v.DateVente >= lastMonth && v.DateVente < thisMonth && v.Statut == StatutVente.Validee)
            .SumAsync(v => v.MontantTTC);

        var evolutionCA = caMoisPrecedent > 0
            ? ((caMois - caMoisPrecedent) / caMoisPrecedent * 100)
            : 0;

        // Produits en stock bas
        var stocksBas = await _context.Stocks
            .Include(s => s.ProduitBoutique)
                .ThenInclude(pb => pb.Produit)
            .Include(s => s.Depot)
            .Where(s => s.Depot.BoutiqueId == boutiqueId &&
                       s.Quantite <= s.ProduitBoutique.SeuilStockBas &&
                       s.ProduitBoutique.EstActif)
            .CountAsync();

        // Top 5 produits vendus ce mois
        var topProduits = await _context.LignesVente
            .Include(lv => lv.Vente)
            .Include(lv => lv.ProduitBoutique)
                .ThenInclude(pb => pb.Produit)
            .Where(lv => lv.Vente.BoutiqueId == boutiqueId &&
                        lv.Vente.DateVente >= thisMonth &&
                        lv.Vente.Statut == StatutVente.Validee)
            .GroupBy(lv => new { lv.ProduitBoutique.Produit.Id, lv.ProduitBoutique.Produit.Nom })
            .Select(g => new
            {
                ProduitNom = g.Key.Nom,
                QuantiteVendue = g.Sum(lv => lv.Quantite),
                MontantTotal = g.Sum(lv => lv.MontantTTC)
            })
            .OrderByDescending(x => x.QuantiteVendue)
            .Take(5)
            .ToListAsync();

        // Ventes par jour (7 derniers jours)
        var dateDebut = todayStart.AddDays(-7);
        var ventes7Jours = await _context.Ventes
            .Where(v => v.BoutiqueId == boutiqueId
                        && v.DateVente >= dateDebut
                        && v.Statut == StatutVente.Validee)
            .ToListAsync();

        var ventesParJourResult = ventes7Jours
            .GroupBy(v => v.DateVente.Date)
            .Select(g => new
            {
                Date = DateTime.SpecifyKind(g.Key, DateTimeKind.Utc),
                Montant = g.Sum(v => v.MontantTTC),
                Nombre = g.Count()
            })
            .OrderBy(x => x.Date)
            .ToList();

        // Achats en attente
        var achatsEnAttente = await _context.Achats
            .Where(a => a.BoutiqueId == boutiqueId && a.Statut == StatutAchat.EnAttente)
            .CountAsync();

        // Inventaires en cours
        var inventairesEnCours = await _context.Inventaires
            .Where(i => i.BoutiqueId == boutiqueId && i.Statut == StatutInventaire.EnCours)
            .CountAsync();

        var boutique = await _context.Boutiques.FindAsync(boutiqueId);

        ViewBag.Boutique = boutique;
        ViewBag.VentesAujourdhui = ventesAujourdhui;
        ViewBag.CAAujourdhui = caAujourdhui;
        ViewBag.CAMois = caMois;
        ViewBag.EvolutionCA = evolutionCA;
        ViewBag.StocksBas = stocksBas;
        ViewBag.TopProduits = topProduits;
        ViewBag.VentesParJour = ventesParJourResult;
        ViewBag.AchatsEnAttente = achatsEnAttente;
        ViewBag.InventairesEnCours = inventairesEnCours;

        return View("DashboardBoutique");
    }

    private async Task<IActionResult> DashboardReseau()
    {
        // IMPORTANT: PostgreSQL (timestamp with time zone) n'accepte que des DateTime en UTC (Kind=Utc)
        var nowUtc = DateTime.UtcNow;
        var todayStart = new DateTime(nowUtc.Year, nowUtc.Month, nowUtc.Day, 0, 0, 0, DateTimeKind.Utc);
        var todayEndExclusive = todayStart.AddDays(1);

        var thisMonth = new DateTime(todayStart.Year, todayStart.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var lastMonth = thisMonth.AddMonths(-1);

        // Nombre de boutiques
        var nombreBoutiques = await _context.Boutiques.Where(b => b.EstActive).CountAsync();

        // CA total aujourd'hui
        var caTotalAujourdhui = await _context.Ventes
            .Where(v => v.DateVente >= todayStart && v.DateVente < todayEndExclusive && v.Statut == StatutVente.Validee)
            .SumAsync(v => v.MontantTTC);

        // CA total ce mois
        var caTotalMois = await _context.Ventes
            .Where(v => v.DateVente >= thisMonth && v.Statut == StatutVente.Validee)
            .SumAsync(v => v.MontantTTC);

        // CA par boutique ce mois
        var caParBoutique = await _context.Ventes
            .Include(v => v.Boutique)
            .Where(v => v.DateVente >= thisMonth && v.Statut == StatutVente.Validee)
            .GroupBy(v => new { v.BoutiqueId, v.Boutique.Nom })
            .Select(g => new
            {
                BoutiqueNom = g.Key.Nom,
                Montant = g.Sum(v => v.MontantTTC),
                NombreVentes = g.Count()
            })
            .OrderByDescending(x => x.Montant)
            .ToListAsync();

        // Total stocks bas
        var totalStocksBas = await _context.Stocks
            .Include(s => s.ProduitBoutique)
            .Where(s => s.Quantite <= s.ProduitBoutique.SeuilStockBas && s.ProduitBoutique.EstActif)
            .CountAsync();

        // Top 5 produits vendus réseau
        var topProduitsReseau = await _context.LignesVente
            .Include(lv => lv.Vente)
            .Include(lv => lv.ProduitBoutique)
                .ThenInclude(pb => pb.Produit)
            .Where(lv => lv.Vente.DateVente >= thisMonth && lv.Vente.Statut == StatutVente.Validee)
            .GroupBy(lv => new { lv.ProduitBoutique.Produit.Id, lv.ProduitBoutique.Produit.Nom })
            .Select(g => new
            {
                ProduitNom = g.Key.Nom,
                QuantiteVendue = g.Sum(lv => lv.Quantite),
                MontantTotal = g.Sum(lv => lv.MontantTTC)
            })
            .OrderByDescending(x => x.QuantiteVendue)
            .Take(5)
            .ToListAsync();

        // Ventes par jour (7 derniers jours)
        var dateDebutReseau = todayStart.AddDays(-7);
        var ventes7JoursReseau = await _context.Ventes
            .Where(v => v.DateVente >= dateDebutReseau && v.Statut == StatutVente.Validee)
            .ToListAsync();

        var ventesParJourReseauResult = ventes7JoursReseau
            .GroupBy(v => v.DateVente.Date)
            .Select(g => new
            {
                Date = DateTime.SpecifyKind(g.Key, DateTimeKind.Utc),
                Montant = g.Sum(v => v.MontantTTC),
                Nombre = g.Count()
            })
            .OrderBy(x => x.Date)
            .ToList();

        // Achats en attente
        var achatsEnAttente = await _context.Achats
            .Where(a => a.Statut == StatutAchat.EnAttente)
            .CountAsync();

        ViewBag.NombreBoutiques = nombreBoutiques;
        ViewBag.CATotalAujourdhui = caTotalAujourdhui;
        ViewBag.CATotalMois = caTotalMois;
        ViewBag.CAParBoutique = caParBoutique;
        ViewBag.TotalStocksBas = totalStocksBas;
        ViewBag.TopProduitsReseau = topProduitsReseau;
        ViewBag.VentesParJour = ventesParJourReseauResult;
        ViewBag.AchatsEnAttente = achatsEnAttente;

        return View("DashboardReseau");
    }
}

