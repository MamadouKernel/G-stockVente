using G_StockVente.Domain.Models;
using G_StockVente.Infrastructure.Data;
using G_StockVente.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace G_StockVente.Controllers;

[Authorize]
public class BoutiquesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IBoutiqueActiveService _boutiqueActiveService;

    public BoutiquesController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IBoutiqueActiveService boutiqueActiveService)
    {
        _context = context;
        _userManager = userManager;
        _boutiqueActiveService = boutiqueActiveService;
    }

    // GET: Boutiques
    [Authorize(Roles = "AdminReseau,ManagerBoutique")]
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        IQueryable<Boutique> query = _context.Boutiques;

        // Si l'utilisateur n'est pas Admin Réseau, filtrer par sa boutique
        if (!await _userManager.IsInRoleAsync(user, "AdminReseau"))
        {
            if (user.BoutiqueActiveId != null)
            {
                query = query.Where(b => b.Id == user.BoutiqueActiveId);
            }
            else
            {
                return RedirectToAction("SelectionBoutique");
            }
        }

        return View(await query.OrderBy(b => b.Nom).ToListAsync());
    }

    // GET: Boutiques/SelectionBoutique
    public async Task<IActionResult> SelectionBoutique()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var isAdminReseau = await _userManager.IsInRoleAsync(user, "AdminReseau");
        List<Boutique> boutiques;

        if (isAdminReseau)
        {
            // Admin Réseau peut voir toutes les boutiques actives
            boutiques = await _context.Boutiques
                .Where(b => b.EstActive)
                .OrderBy(b => b.Nom)
                .ToListAsync();
        }
        else
        {
            // ManagerBoutique ne peut voir que sa boutique assignée
            if (user.BoutiqueActiveId == null)
            {
                ModelState.AddModelError("", "Aucune boutique assignée. Contactez un administrateur.");
                boutiques = new List<Boutique>();
            }
            else
            {
                boutiques = await _context.Boutiques
                    .Where(b => b.Id == user.BoutiqueActiveId && b.EstActive)
                    .OrderBy(b => b.Nom)
                    .ToListAsync();
            }
        }

        // Passer l'ID de la boutique assignée pour l'affichage
        ViewBag.BoutiqueAssigneeId = user.BoutiqueActiveId;
        ViewBag.IsAdminReseau = isAdminReseau;

        return View(boutiques);
    }

    // POST: Boutiques/SelectionBoutique
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SelectionBoutique(Guid boutiqueId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var isAdminReseau = await _userManager.IsInRoleAsync(user, "AdminReseau");

        // Vérifier que le ManagerBoutique ne sélectionne que sa boutique assignée
        if (!isAdminReseau && user.BoutiqueActiveId != boutiqueId)
        {
            ModelState.AddModelError("", "Vous ne pouvez sélectionner que votre boutique assignée.");
            // Recharger la liste des boutiques autorisées
            if (user.BoutiqueActiveId == null)
            {
                return View(new List<Boutique>());
            }
            var boutiques = await _context.Boutiques
                .Where(b => b.Id == user.BoutiqueActiveId && b.EstActive)
                .ToListAsync();
            return View(boutiques);
        }

        var boutique = await _context.Boutiques.FindAsync(boutiqueId);
        if (boutique == null || !boutique.EstActive)
        {
            ModelState.AddModelError("", "Boutique introuvable ou inactive.");
            // Recharger la liste des boutiques autorisées
            if (isAdminReseau)
            {
                return View(await _context.Boutiques.Where(b => b.EstActive).ToListAsync());
            }
            else if (user.BoutiqueActiveId != null)
            {
                return View(await _context.Boutiques.Where(b => b.Id == user.BoutiqueActiveId && b.EstActive).ToListAsync());
            }
            return View(new List<Boutique>());
        }

        await _boutiqueActiveService.SetBoutiqueActiveAsync(user.Id, boutiqueId);
        return RedirectToAction("Index", "Home");
    }

    // GET: Boutiques/Details/5
    [Authorize(Roles = "AdminReseau,ManagerBoutique")]
    public async Task<IActionResult> Details(Guid? id)
    {
        if (id == null) return NotFound();

        var boutique = await _context.Boutiques
            .Include(b => b.Depots)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (boutique == null) return NotFound();

        // Vérifier les droits d'accès
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        if (!await _userManager.IsInRoleAsync(user, "AdminReseau") && user.BoutiqueActiveId != id)
        {
            return Forbid();
        }

        return View(boutique);
    }

    // GET: Boutiques/Organigramme/5
    [Authorize(Roles = "AdminReseau")]
    public async Task<IActionResult> Organigramme(Guid? id)
    {
        if (id == null) return NotFound();

        var boutique = await _context.Boutiques
            .FirstOrDefaultAsync(m => m.Id == id);

        if (boutique == null) return NotFound();

        // Récupérer tous les utilisateurs de cette boutique
        var utilisateurs = await _context.Users
            .Where(u => u.BoutiqueActiveId == id && !u.EstSupprime)
            .OrderBy(u => u.Nom)
            .ThenBy(u => u.Prenom)
            .ToListAsync();

        // Charger les rôles pour chaque utilisateur
        var utilisateursAvecRoles = new List<OrganigrammeViewModel>();
        foreach (var user in utilisateurs)
        {
            var roles = await _userManager.GetRolesAsync(user);
            utilisateursAvecRoles.Add(new OrganigrammeViewModel
            {
                Id = user.Id,
                Prenom = user.Prenom,
                Nom = user.Nom,
                Email = user.Email ?? string.Empty,
                Roles = roles.ToList(),
                EstActif = user.EstActif,
                DateCreation = user.DateCreation,
                DateDerniereConnexion = user.DateDerniereConnexion
            });
        }

        // Grouper par rôle pour l'affichage hiérarchique
        var managerBoutique = utilisateursAvecRoles.Where(u => u.Roles.Contains("ManagerBoutique")).ToList();
        var gestionnairesStock = utilisateursAvecRoles.Where(u => u.Roles.Contains("GestionnaireStock") && !u.Roles.Contains("ManagerBoutique")).ToList();
        var caissiers = utilisateursAvecRoles.Where(u => u.Roles.Contains("Caissier") && !u.Roles.Contains("ManagerBoutique") && !u.Roles.Contains("GestionnaireStock")).ToList();
        var autres = utilisateursAvecRoles.Where(u => !u.Roles.Any() || (!u.Roles.Contains("ManagerBoutique") && !u.Roles.Contains("GestionnaireStock") && !u.Roles.Contains("Caissier"))).ToList();

        ViewBag.Boutique = boutique;
        ViewBag.ManagerBoutique = managerBoutique;
        ViewBag.GestionnairesStock = gestionnairesStock;
        ViewBag.Caissiers = caissiers;
        ViewBag.Autres = autres;
        ViewBag.TotalUtilisateurs = utilisateursAvecRoles.Count;

        return View();
    }

    // GET: Boutiques/Create
    [Authorize(Roles = "AdminReseau")]
    public IActionResult Create()
    {
        return View();
    }

    // POST: Boutiques/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "AdminReseau")]
    public async Task<IActionResult> Create([Bind("Nom,Adresse,Telephone,Email,Logo,TvaParDefaut,EstActive")] Boutique boutique)
    {
        if (ModelState.IsValid)
        {
            boutique.DateCreation = DateTime.UtcNow;
            _context.Add(boutique);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(boutique);
    }

    // GET: Boutiques/Edit/5
    [Authorize(Roles = "AdminReseau,ManagerBoutique")]
    public async Task<IActionResult> Edit(Guid? id)
    {
        if (id == null) return NotFound();

        var boutique = await _context.Boutiques.FindAsync(id);
        if (boutique == null) return NotFound();

        // Vérifier les droits
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        if (!await _userManager.IsInRoleAsync(user, "AdminReseau") && user.BoutiqueActiveId != id)
        {
            return Forbid();
        }

        return View(boutique);
    }

    // POST: Boutiques/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "AdminReseau,ManagerBoutique")]
    public async Task<IActionResult> Edit(Guid id, [Bind("Id,Nom,Adresse,Telephone,Email,Logo,TvaParDefaut,EstActive,DateCreation")] Boutique boutique)
    {
        if (id != boutique.Id) return NotFound();

        // Vérifier les droits
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        if (!await _userManager.IsInRoleAsync(user, "AdminReseau") && user.BoutiqueActiveId != id)
        {
            return Forbid();
        }

        if (ModelState.IsValid)
        {
            try
            {
                boutique.DateModification = DateTime.UtcNow;
                _context.Update(boutique);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BoutiqueExists(boutique.Id))
                    return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }
        return View(boutique);
    }

    private bool BoutiqueExists(Guid id)
    {
        return _context.Boutiques.Any(e => e.Id == id);
    }
}

public class OrganigrammeViewModel
{
    public Guid Id { get; set; }
    public string Prenom { get; set; } = string.Empty;
    public string Nom { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
    public bool EstActif { get; set; }
    public DateTime DateCreation { get; set; }
    public DateTime? DateDerniereConnexion { get; set; }
}

