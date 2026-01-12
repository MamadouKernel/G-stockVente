using G_StockVente.Domain.Models;
using G_StockVente.Infrastructure.Data;
using G_StockVente.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace G_StockVente.Controllers;

[Authorize]
public class PaiementsIntegresController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IPaiementIntegreService _paiementService;
    private readonly IBoutiqueActiveService _boutiqueActiveService;

    public PaiementsIntegresController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IPaiementIntegreService paiementService,
        IBoutiqueActiveService boutiqueActiveService)
    {
        _context = context;
        _userManager = userManager;
        _paiementService = paiementService;
        _boutiqueActiveService = boutiqueActiveService;
    }

    // GET: PaiementsIntegres
    [Authorize(Roles = "AdminReseau,ManagerBoutique")]
    public async Task<IActionResult> Index(DateTime? dateDebut, DateTime? dateFin, Guid? boutiqueId, StatutPaiement? statut)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var isAdminReseau = await _userManager.IsInRoleAsync(user, "AdminReseau");
        if (!isAdminReseau && boutiqueId == null)
        {
            boutiqueId = await _boutiqueActiveService.GetBoutiqueActiveIdAsync(user.Id);
        }

        var query = _context.PaiementsIntegres
            .Include(p => p.Vente)
                .ThenInclude(v => v.Boutique)
            .AsQueryable();

        if (boutiqueId.HasValue)
        {
            query = query.Where(p => p.Vente.BoutiqueId == boutiqueId.Value);
        }

        if (dateDebut.HasValue)
        {
            query = query.Where(p => p.DateCreation >= dateDebut.Value);
        }

        if (dateFin.HasValue)
        {
            var dateFinUtc = dateFin.Value.Date.AddDays(1).AddTicks(-1);
            query = query.Where(p => p.DateCreation <= dateFinUtc);
        }

        if (statut.HasValue)
        {
            query = query.Where(p => p.Statut == statut.Value);
        }

        var paiements = await query
            .OrderByDescending(p => p.DateCreation)
            .Take(200)
            .ToListAsync();

        ViewBag.Boutiques = await _context.Boutiques
            .Where(b => b.EstActive)
            .OrderBy(b => b.Nom)
            .Select(b => new { b.Id, b.Nom })
            .ToListAsync();

        ViewBag.DateDebut = dateDebut;
        ViewBag.DateFin = dateFin;
        ViewBag.BoutiqueId = boutiqueId;
        ViewBag.Statut = statut;

        return View(paiements);
    }

    // GET: PaiementsIntegres/Details/5
    public async Task<IActionResult> Details(Guid? id)
    {
        if (id == null) return NotFound();

        var paiement = await _paiementService.GetByIdAsync(id.Value);
        if (paiement == null) return NotFound();

        // Vérifier les droits
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var isAdminReseau = await _userManager.IsInRoleAsync(user, "AdminReseau");
        if (!isAdminReseau)
        {
            var boutiqueId = await _boutiqueActiveService.GetBoutiqueActiveIdAsync(user.Id);
            if (paiement.Vente.BoutiqueId != boutiqueId)
            {
                return Forbid();
            }
        }

        return View(paiement);
    }

    // POST: PaiementsIntegres/Rembourser/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "AdminReseau,ManagerBoutique")]
    public async Task<IActionResult> Rembourser(Guid id)
    {
        var paiement = await _paiementService.GetByIdAsync(id);
        if (paiement == null)
        {
            TempData["Error"] = "Paiement introuvable.";
            return RedirectToAction(nameof(Index));
        }

        // Vérifier les droits
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var isAdminReseau = await _userManager.IsInRoleAsync(user, "AdminReseau");
        if (!isAdminReseau)
        {
            var boutiqueId = await _boutiqueActiveService.GetBoutiqueActiveIdAsync(user.Id);
            if (paiement.Vente.BoutiqueId != boutiqueId)
            {
                return Forbid();
            }
        }

        var success = await _paiementService.RembourserPaiementAsync(id);
        if (success)
        {
            TempData["Success"] = "Paiement remboursé avec succès.";
        }
        else
        {
            TempData["Error"] = "Impossible de rembourser ce paiement.";
        }

        return RedirectToAction(nameof(Details), new { id });
    }
}

