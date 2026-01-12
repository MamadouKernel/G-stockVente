using G_StockVente.Domain.Models;
using G_StockVente.Infrastructure.Data;
using G_StockVente.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace G_StockVente.Controllers;

[Authorize]
public class ProduitsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IBoutiqueActiveService _boutiqueActiveService;

    public ProduitsController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IBoutiqueActiveService boutiqueActiveService)
    {
        _context = context;
        _userManager = userManager;
        _boutiqueActiveService = boutiqueActiveService;
    }

    // GET: Produits
    public async Task<IActionResult> Index(string? searchTerm)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var boutiqueId = await _boutiqueActiveService.GetBoutiqueActiveIdAsync(user.Id);
        if (boutiqueId == null && !await _userManager.IsInRoleAsync(user, "AdminReseau"))
        {
            return RedirectToAction("SelectionBoutique", "Boutiques");
        }

        IQueryable<ProduitBoutique> query = _context.ProduitsBoutique
            .Include(pb => pb.Produit)
                .ThenInclude(p => p.Categorie)
            .Include(pb => pb.Boutique)
            .Include(pb => pb.Stocks)
                .ThenInclude(s => s.Depot);

        if (boutiqueId != null)
        {
            query = query.Where(pb => pb.BoutiqueId == boutiqueId);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(pb =>
                pb.Produit.Nom.Contains(searchTerm) ||
                pb.Sku != null && pb.Sku.Contains(searchTerm) ||
                pb.CodeBarres != null && pb.CodeBarres.Contains(searchTerm));
        }

        var produits = await query.OrderBy(pb => pb.Produit.Nom).ToListAsync();

        ViewBag.SearchTerm = searchTerm;
        ViewBag.BoutiqueId = boutiqueId;
        return View(produits);
    }

    // GET: Produits/Details/5
    public async Task<IActionResult> Details(Guid? id)
    {
        if (id == null) return NotFound();

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var isAdminReseau = await _userManager.IsInRoleAsync(user, "AdminReseau");
        var boutiqueId = await _boutiqueActiveService.GetBoutiqueActiveIdAsync(user.Id);

        var produitBoutique = await _context.ProduitsBoutique
            .Include(pb => pb.Produit)
                .ThenInclude(p => p.Categorie)
            .Include(pb => pb.Boutique)
            .Include(pb => pb.Stocks)
                .ThenInclude(s => s.Depot)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (produitBoutique == null) return NotFound();

        // Sécurité : ManagerBoutique/GestionnaireStock ne peut voir que les produits de sa boutique
        if (!isAdminReseau && boutiqueId != null && produitBoutique.BoutiqueId != boutiqueId)
        {
            return Forbid();
        }

        return View(produitBoutique);
    }

    // GET: Produits/Create
    [Authorize(Roles = "AdminReseau,ManagerBoutique,GestionnaireStock")]
    public async Task<IActionResult> Create()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var isAdminReseau = await _userManager.IsInRoleAsync(user, "AdminReseau");
        var boutiqueId = await _boutiqueActiveService.GetBoutiqueActiveIdAsync(user.Id);
        
        if (boutiqueId == null && !isAdminReseau)
        {
            return RedirectToAction("SelectionBoutique", "Boutiques");
        }

        ViewData["CategorieId"] = new SelectList(
            await _context.Categories.OrderBy(c => c.Nom).ToListAsync(),
            "Id", "Nom");

        // Si ManagerBoutique ou GestionnaireStock, ne montrer que sa boutique
        IQueryable<Boutique> boutiquesQuery = _context.Boutiques.Where(b => b.EstActive);
        if (!isAdminReseau && boutiqueId != null)
        {
            boutiquesQuery = boutiquesQuery.Where(b => b.Id == boutiqueId);
        }

        ViewData["BoutiqueId"] = new SelectList(
            await boutiquesQuery.OrderBy(b => b.Nom).ToListAsync(),
            "Id", "Nom", boutiqueId);

        return View();
    }

    // POST: Produits/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "AdminReseau,ManagerBoutique,GestionnaireStock")]
    public async Task<IActionResult> Create(CreateProduitViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var isAdminReseau = await _userManager.IsInRoleAsync(user, "AdminReseau");
        var boutiqueId = await _boutiqueActiveService.GetBoutiqueActiveIdAsync(user.Id);

        // Sécurité : ManagerBoutique/GestionnaireStock ne peut créer que pour sa boutique
        if (!isAdminReseau && boutiqueId != null && model.BoutiqueId != boutiqueId)
        {
            ModelState.AddModelError("BoutiqueId", "Vous ne pouvez créer des produits que pour votre boutique active.");
        }

        if (ModelState.IsValid)
        {
            // Si ManagerBoutique/GestionnaireStock, forcer la boutique active
            if (!isAdminReseau && boutiqueId != null)
            {
                model.BoutiqueId = boutiqueId.Value;
            }

            // Créer le produit global
            var produit = new Produit
            {
                Nom = model.Nom,
                Description = model.Description,
                CategorieId = model.CategorieId,
                DateCreation = DateTime.UtcNow
            };
            _context.Add(produit);
            await _context.SaveChangesAsync();

            // Créer le paramétrage boutique
            var produitBoutique = new ProduitBoutique
            {
                ProduitId = produit.Id,
                BoutiqueId = model.BoutiqueId,
                Sku = model.Sku,
                CodeBarres = model.CodeBarres,
                PrixAchat = model.PrixAchat,
                PrixVente = model.PrixVente,
                SeuilStockBas = model.SeuilStockBas,
                EstActif = model.EstActif,
                DateCreation = DateTime.UtcNow
            };
            _context.Add(produitBoutique);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        ViewData["CategorieId"] = new SelectList(
            await _context.Categories.OrderBy(c => c.Nom).ToListAsync(),
            "Id", "Nom", model.CategorieId);

        // Si ManagerBoutique ou GestionnaireStock, ne montrer que sa boutique
        IQueryable<Boutique> boutiquesQuery = _context.Boutiques.Where(b => b.EstActive);
        if (!isAdminReseau && boutiqueId != null)
        {
            boutiquesQuery = boutiquesQuery.Where(b => b.Id == boutiqueId);
        }

        ViewData["BoutiqueId"] = new SelectList(
            await boutiquesQuery.OrderBy(b => b.Nom).ToListAsync(),
            "Id", "Nom", model.BoutiqueId);

        return View(model);
    }

    // GET: Produits/Edit/5
    [Authorize(Roles = "AdminReseau,ManagerBoutique,GestionnaireStock")]
    public async Task<IActionResult> Edit(Guid? id)
    {
        if (id == null) return NotFound();

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var isAdminReseau = await _userManager.IsInRoleAsync(user, "AdminReseau");
        var boutiqueId = await _boutiqueActiveService.GetBoutiqueActiveIdAsync(user.Id);

        var produitBoutique = await _context.ProduitsBoutique
            .Include(pb => pb.Produit)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (produitBoutique == null) return NotFound();

        // Sécurité : ManagerBoutique/GestionnaireStock ne peut modifier que les produits de sa boutique
        if (!isAdminReseau && boutiqueId != null && produitBoutique.BoutiqueId != boutiqueId)
        {
            return Forbid();
        }

        var model = new EditProduitViewModel
        {
            Id = produitBoutique.Id,
            ProduitId = produitBoutique.ProduitId,
            Nom = produitBoutique.Produit.Nom,
            Description = produitBoutique.Produit.Description,
            CategorieId = produitBoutique.Produit.CategorieId,
            BoutiqueId = produitBoutique.BoutiqueId,
            Sku = produitBoutique.Sku,
            CodeBarres = produitBoutique.CodeBarres,
            PrixAchat = produitBoutique.PrixAchat,
            PrixVente = produitBoutique.PrixVente,
            SeuilStockBas = produitBoutique.SeuilStockBas,
            EstActif = produitBoutique.EstActif
        };

        ViewData["CategorieId"] = new SelectList(
            await _context.Categories.OrderBy(c => c.Nom).ToListAsync(),
            "Id", "Nom", model.CategorieId);

        return View(model);
    }

    // POST: Produits/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "AdminReseau,ManagerBoutique,GestionnaireStock")]
    public async Task<IActionResult> Edit(Guid id, EditProduitViewModel model)
    {
        if (id != model.Id) return NotFound();

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var isAdminReseau = await _userManager.IsInRoleAsync(user, "AdminReseau");
        var boutiqueId = await _boutiqueActiveService.GetBoutiqueActiveIdAsync(user.Id);

        if (ModelState.IsValid)
        {
            try
            {
                var produitBoutique = await _context.ProduitsBoutique
                    .Include(pb => pb.Produit)
                    .FirstOrDefaultAsync(pb => pb.Id == id);

                if (produitBoutique == null) return NotFound();

                // Sécurité : ManagerBoutique/GestionnaireStock ne peut modifier que les produits de sa boutique
                if (!isAdminReseau && boutiqueId != null && produitBoutique.BoutiqueId != boutiqueId)
                {
                    return Forbid();
                }

                // Mettre à jour le produit global
                produitBoutique.Produit.Nom = model.Nom;
                produitBoutique.Produit.Description = model.Description;
                produitBoutique.Produit.CategorieId = model.CategorieId;
                produitBoutique.Produit.DateModification = DateTime.UtcNow;

                // Mettre à jour le paramétrage boutique
                produitBoutique.Sku = model.Sku;
                produitBoutique.CodeBarres = model.CodeBarres;
                produitBoutique.PrixAchat = model.PrixAchat;
                produitBoutique.PrixVente = model.PrixVente;
                produitBoutique.SeuilStockBas = model.SeuilStockBas;
                produitBoutique.EstActif = model.EstActif;
                produitBoutique.DateModification = DateTime.UtcNow;

                _context.Update(produitBoutique);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProduitBoutiqueExists(model.Id))
                    return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }

        ViewData["CategorieId"] = new SelectList(
            await _context.Categories.OrderBy(c => c.Nom).ToListAsync(),
            "Id", "Nom", model.CategorieId);

        return View(model);
    }

    // Recherche par code-barres (pour scanner)
    [HttpPost]
    public async Task<IActionResult> RechercherParCodeBarres(string codeBarres)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Json(new { success = false });

        var boutiqueId = await _boutiqueActiveService.GetBoutiqueActiveIdAsync(user.Id);
        if (boutiqueId == null) return Json(new { success = false });

        var produit = await _context.ProduitsBoutique
            .Include(pb => pb.Produit)
            .Where(pb => pb.BoutiqueId == boutiqueId && pb.CodeBarres == codeBarres && pb.EstActif)
            .FirstOrDefaultAsync();

        if (produit == null)
        {
            return Json(new { success = false, message = "Produit non trouvé" });
        }

        return Json(new
        {
            success = true,
            produit = new
            {
                id = produit.Id,
                nom = produit.Produit.Nom,
                prixVente = produit.PrixVente,
                sku = produit.Sku
            }
        });
    }

    private bool ProduitBoutiqueExists(Guid id)
    {
        return _context.ProduitsBoutique.Any(e => e.Id == id);
    }
}

public class CreateProduitViewModel
{
    [Required(ErrorMessage = "Le nom est obligatoire")]
    [StringLength(200)]
    public string Nom { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    [Required]
    [Display(Name = "Catégorie")]
    public Guid CategorieId { get; set; }

    [Required]
    [Display(Name = "Boutique")]
    public Guid BoutiqueId { get; set; }

    [StringLength(100)]
    [Display(Name = "SKU")]
    public string? Sku { get; set; }

    [StringLength(100)]
    [Display(Name = "Code-barres")]
    public string? CodeBarres { get; set; }

    [Required]
    [Range(0, double.MaxValue)]
    [Display(Name = "Prix d'achat")]
    public decimal PrixAchat { get; set; }

    [Required]
    [Range(0, double.MaxValue)]
    [Display(Name = "Prix de vente")]
    public decimal PrixVente { get; set; }

    [Range(0, int.MaxValue)]
    [Display(Name = "Seuil stock bas")]
    public int SeuilStockBas { get; set; } = 0;

    [Display(Name = "Actif")]
    public bool EstActif { get; set; } = true;
}

public class EditProduitViewModel
{
    public Guid Id { get; set; }
    public Guid ProduitId { get; set; }

    [Required(ErrorMessage = "Le nom est obligatoire")]
    [StringLength(200)]
    public string Nom { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    [Required]
    [Display(Name = "Catégorie")]
    public Guid CategorieId { get; set; }

    public Guid BoutiqueId { get; set; }

    [StringLength(100)]
    [Display(Name = "SKU")]
    public string? Sku { get; set; }

    [StringLength(100)]
    [Display(Name = "Code-barres")]
    public string? CodeBarres { get; set; }

    [Required]
    [Range(0, double.MaxValue)]
    [Display(Name = "Prix d'achat")]
    public decimal PrixAchat { get; set; }

    [Required]
    [Range(0, double.MaxValue)]
    [Display(Name = "Prix de vente")]
    public decimal PrixVente { get; set; }

    [Range(0, int.MaxValue)]
    [Display(Name = "Seuil stock bas")]
    public int SeuilStockBas { get; set; } = 0;

    [Display(Name = "Actif")]
    public bool EstActif { get; set; } = true;
}
