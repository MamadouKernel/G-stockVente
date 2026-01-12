using G_StockVente.Domain.Models;
using G_StockVente.Infrastructure.Data;
using G_StockVente.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace G_StockVente.Controllers;

[Authorize]
public class DepotsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IBoutiqueActiveService _boutiqueActiveService;

    public DepotsController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IBoutiqueActiveService boutiqueActiveService)
    {
        _context = context;
        _userManager = userManager;
        _boutiqueActiveService = boutiqueActiveService;
    }

    // GET: Depots
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        IQueryable<Depot> query = _context.Depots.Include(d => d.Boutique);

        if (!await _userManager.IsInRoleAsync(user, "AdminReseau"))
        {
            var boutiqueId = await _boutiqueActiveService.GetBoutiqueActiveIdAsync(user.Id);
            if (boutiqueId != null)
            {
                query = query.Where(d => d.BoutiqueId == boutiqueId);
            }
        }

        return View(await query.OrderBy(d => d.Boutique.Nom).ThenBy(d => d.Nom).ToListAsync());
    }

    // GET: Depots/Details/5
    public async Task<IActionResult> Details(Guid? id)
    {
        if (id == null) return NotFound();

        var depot = await _context.Depots
            .Include(d => d.Boutique)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (depot == null) return NotFound();

        // Vérifier les droits d'accès : ManagerBoutique ne peut voir que les dépôts de sa boutique
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        if (!await _userManager.IsInRoleAsync(user, "AdminReseau"))
        {
            var boutiqueId = await _boutiqueActiveService.GetBoutiqueActiveIdAsync(user.Id);
            if (boutiqueId != null && depot.BoutiqueId != boutiqueId)
            {
                return Forbid();
            }
        }

        return View(depot);
    }

    // GET: Depots/Create
    [Authorize(Roles = "AdminReseau,ManagerBoutique")]
    public async Task<IActionResult> Create()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        IQueryable<Boutique> boutiquesQuery = _context.Boutiques.Where(b => b.EstActive);

        if (!await _userManager.IsInRoleAsync(user, "AdminReseau"))
        {
            var boutiqueId = await _boutiqueActiveService.GetBoutiqueActiveIdAsync(user.Id);
            if (boutiqueId != null)
            {
                boutiquesQuery = boutiquesQuery.Where(b => b.Id == boutiqueId);
            }
        }

        ViewData["BoutiqueId"] = new SelectList(
            await boutiquesQuery.OrderBy(b => b.Nom).ToListAsync(),
            "Id", "Nom");
        return View();
    }

    // POST: Depots/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "AdminReseau,ManagerBoutique")]
    public async Task<IActionResult> Create([Bind("Nom,Adresse,BoutiqueId,EstActif")] Depot depot)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        // Sécurité : ManagerBoutique ne peut créer que pour sa boutique
        if (!await _userManager.IsInRoleAsync(user, "AdminReseau"))
        {
            var boutiqueId = await _boutiqueActiveService.GetBoutiqueActiveIdAsync(user.Id);
            if (boutiqueId != null && depot.BoutiqueId != boutiqueId)
            {
                ModelState.AddModelError("BoutiqueId", "Vous ne pouvez créer des dépôts que pour votre boutique active.");
            }
            else if (boutiqueId != null)
            {
                depot.BoutiqueId = boutiqueId.Value;
            }
        }

        if (ModelState.IsValid)
        {
            depot.DateCreation = DateTime.UtcNow;
            _context.Add(depot);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        
        IQueryable<Boutique> boutiquesQuery = _context.Boutiques.Where(b => b.EstActive);
        if (!await _userManager.IsInRoleAsync(user, "AdminReseau"))
        {
            var boutiqueId = await _boutiqueActiveService.GetBoutiqueActiveIdAsync(user.Id);
            if (boutiqueId != null)
            {
                boutiquesQuery = boutiquesQuery.Where(b => b.Id == boutiqueId);
            }
        }
        
        ViewData["BoutiqueId"] = new SelectList(
            await boutiquesQuery.OrderBy(b => b.Nom).ToListAsync(),
            "Id", "Nom", depot.BoutiqueId);
        return View(depot);
    }

    // GET: Depots/Edit/5
    [Authorize(Roles = "AdminReseau,ManagerBoutique")]
    public async Task<IActionResult> Edit(Guid? id)
    {
        if (id == null) return NotFound();

        var depot = await _context.Depots
            .Include(d => d.Boutique)
            .FirstOrDefaultAsync(d => d.Id == id);
        if (depot == null) return NotFound();

        // Vérifier les droits d'accès : ManagerBoutique ne peut modifier que les dépôts de sa boutique
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        if (!await _userManager.IsInRoleAsync(user, "AdminReseau"))
        {
            var boutiqueId = await _boutiqueActiveService.GetBoutiqueActiveIdAsync(user.Id);
            if (boutiqueId != null && depot.BoutiqueId != boutiqueId)
            {
                return Forbid();
            }
        }

        IQueryable<Boutique> boutiquesQuery = _context.Boutiques.Where(b => b.EstActive);
        if (!await _userManager.IsInRoleAsync(user, "AdminReseau"))
        {
            var boutiqueId = await _boutiqueActiveService.GetBoutiqueActiveIdAsync(user.Id);
            if (boutiqueId != null)
            {
                boutiquesQuery = boutiquesQuery.Where(b => b.Id == boutiqueId);
            }
        }

        ViewData["BoutiqueId"] = new SelectList(
            await boutiquesQuery.OrderBy(b => b.Nom).ToListAsync(),
            "Id", "Nom", depot.BoutiqueId);
        return View(depot);
    }

    // POST: Depots/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "AdminReseau,ManagerBoutique")]
    public async Task<IActionResult> Edit(Guid id, [Bind("Id,Nom,Adresse,BoutiqueId,EstActif,DateCreation")] Depot depot)
    {
        if (id != depot.Id) return NotFound();

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        // Vérifier les droits d'accès : ManagerBoutique ne peut modifier que les dépôts de sa boutique
        var existingDepot = await _context.Depots.FindAsync(id);
        if (existingDepot == null) return NotFound();

        if (!await _userManager.IsInRoleAsync(user, "AdminReseau"))
        {
            var boutiqueId = await _boutiqueActiveService.GetBoutiqueActiveIdAsync(user.Id);
            if (boutiqueId != null && existingDepot.BoutiqueId != boutiqueId)
            {
                return Forbid();
            }
            
            // Sécurité : ManagerBoutique ne peut modifier que pour sa boutique
            if (boutiqueId != null && depot.BoutiqueId != boutiqueId)
            {
                ModelState.AddModelError("BoutiqueId", "Vous ne pouvez modifier les dépôts que pour votre boutique active.");
            }
            else if (boutiqueId != null)
            {
                depot.BoutiqueId = boutiqueId.Value;
            }
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(depot);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DepotExists(depot.Id))
                    return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }
        
        IQueryable<Boutique> boutiquesQuery = _context.Boutiques.Where(b => b.EstActive);
        if (!await _userManager.IsInRoleAsync(user, "AdminReseau"))
        {
            var boutiqueId = await _boutiqueActiveService.GetBoutiqueActiveIdAsync(user.Id);
            if (boutiqueId != null)
            {
                boutiquesQuery = boutiquesQuery.Where(b => b.Id == boutiqueId);
            }
        }
        
        ViewData["BoutiqueId"] = new SelectList(
            await boutiquesQuery.OrderBy(b => b.Nom).ToListAsync(),
            "Id", "Nom", depot.BoutiqueId);
        return View(depot);
    }

    private bool DepotExists(Guid id)
    {
        return _context.Depots.Any(e => e.Id == id);
    }
}

