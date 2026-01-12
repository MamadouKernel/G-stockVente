using G_StockVente.Domain.Models;
using G_StockVente.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace G_StockVente.Controllers;

[Authorize(Roles = "AdminReseau,ManagerBoutique")]
public class FournisseursController : Controller
{
    private readonly ApplicationDbContext _context;

    public FournisseursController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Fournisseurs
    public async Task<IActionResult> Index()
    {
        return View(await _context.Fournisseurs.OrderBy(f => f.Nom).ToListAsync());
    }

    // GET: Fournisseurs/Details/5
    public async Task<IActionResult> Details(Guid? id)
    {
        if (id == null) return NotFound();

        var fournisseur = await _context.Fournisseurs
            .Include(f => f.Achats)
                .ThenInclude(a => a.Boutique)
            .Include(f => f.Achats)
                .ThenInclude(a => a.Utilisateur)
            .Include(f => f.Achats)
                .ThenInclude(a => a.LignesAchat)
                    .ThenInclude(la => la.ProduitBoutique)
                        .ThenInclude(pb => pb.Produit)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (fournisseur == null) return NotFound();

        // Calculer les statistiques pour la traçabilité
        var achats = fournisseur.Achats.ToList();
        var totalAchats = achats.Count;
        var montantTotalHT = achats.Sum(a => a.MontantHT);
        var montantTotalTTC = achats.Sum(a => a.MontantTTC);
        var premierAchat = achats.OrderBy(a => a.DateCommande).FirstOrDefault();
        var dernierAchat = achats.OrderByDescending(a => a.DateCommande).FirstOrDefault();
        
        // Achats par statut
        var achatsEnAttente = achats.Count(a => a.Statut == Domain.Models.StatutAchat.EnAttente);
        var achatsReceptionnes = achats.Count(a => a.Statut == Domain.Models.StatutAchat.Receptionne);
        var achatsAnnules = achats.Count(a => a.Statut == Domain.Models.StatutAchat.Annule);

        // Achats par année/mois (pour graphique)
        var achatsParMois = achats
            .Where(a => a.DateCommande >= DateTime.UtcNow.AddMonths(-12))
            .GroupBy(a => new { a.DateCommande.Year, a.DateCommande.Month })
            .OrderBy(g => g.Key.Year)
            .ThenBy(g => g.Key.Month)
            .Select(g => new
            {
                Mois = new DateTime(g.Key.Year, g.Key.Month, 1),
                Nombre = g.Count(),
                MontantHT = g.Sum(a => a.MontantHT),
                MontantTTC = g.Sum(a => a.MontantTTC)
            })
            .ToList();

        // Achats récents (30 derniers jours)
        var achatsRecents = achats
            .Where(a => a.DateCommande >= DateTime.UtcNow.AddDays(-30))
            .OrderByDescending(a => a.DateCommande)
            .Take(10)
            .ToList();

        ViewBag.TotalAchats = totalAchats;
        ViewBag.MontantTotalHT = montantTotalHT;
        ViewBag.MontantTotalTTC = montantTotalTTC;
        ViewBag.PremierAchat = premierAchat;
        ViewBag.DernierAchat = dernierAchat;
        ViewBag.AchatsEnAttente = achatsEnAttente;
        ViewBag.AchatsReceptionnes = achatsReceptionnes;
        ViewBag.AchatsAnnules = achatsAnnules;
        ViewBag.AchatsParMois = achatsParMois;
        ViewBag.AchatsRecents = achatsRecents;

        return View(fournisseur);
    }

    // GET: Fournisseurs/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Fournisseurs/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Nom,Adresse,Telephone,Email,Notes,EstActif")] Fournisseur fournisseur)
    {
        if (ModelState.IsValid)
        {
            fournisseur.DateCreation = DateTime.UtcNow;
            _context.Add(fournisseur);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(fournisseur);
    }

    // GET: Fournisseurs/Edit/5
    public async Task<IActionResult> Edit(Guid? id)
    {
        if (id == null) return NotFound();

        var fournisseur = await _context.Fournisseurs.FindAsync(id);
        if (fournisseur == null) return NotFound();

        return View(fournisseur);
    }

    // POST: Fournisseurs/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, [Bind("Id,Nom,Adresse,Telephone,Email,Notes,EstActif,DateCreation")] Fournisseur fournisseur)
    {
        if (id != fournisseur.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                fournisseur.DateModification = DateTime.UtcNow;
                _context.Update(fournisseur);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FournisseurExists(fournisseur.Id))
                    return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }
        return View(fournisseur);
    }

    private bool FournisseurExists(Guid id)
    {
        return _context.Fournisseurs.Any(e => e.Id == id);
    }
}

