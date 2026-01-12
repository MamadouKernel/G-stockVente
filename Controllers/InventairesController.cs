using G_StockVente.Domain.Models;
using G_StockVente.Infrastructure.Data;
using G_StockVente.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace G_StockVente.Controllers;

[Authorize(Roles = "AdminReseau,ManagerBoutique,GestionnaireStock")]
public class InventairesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IBoutiqueActiveService _boutiqueActiveService;

    public InventairesController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IBoutiqueActiveService boutiqueActiveService)
    {
        _context = context;
        _userManager = userManager;
        _boutiqueActiveService = boutiqueActiveService;
    }

    // GET: Inventaires
    public async Task<IActionResult> Index(StatutInventaire? statut)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var boutiqueId = await _boutiqueActiveService.GetBoutiqueActiveIdAsync(user.Id);
        if (boutiqueId == null && !await _userManager.IsInRoleAsync(user, "AdminReseau"))
        {
            return RedirectToAction("SelectionBoutique", "Boutiques");
        }

        IQueryable<Inventaire> query = _context.Inventaires
            .Include(i => i.Boutique)
            .Include(i => i.Depot)
            .Include(i => i.Utilisateur);

        if (boutiqueId != null)
        {
            query = query.Where(i => i.BoutiqueId == boutiqueId);
        }

        if (statut.HasValue)
        {
            query = query.Where(i => i.Statut == statut.Value);
        }

        var inventaires = await query.OrderByDescending(i => i.DateDebut).ToListAsync();

        ViewBag.Statut = statut;
        ViewBag.Statuts = Enum.GetValues(typeof(StatutInventaire)).Cast<StatutInventaire>();
        return View(inventaires);
    }

    // GET: Inventaires/Details/5
    public async Task<IActionResult> Details(Guid? id)
    {
        if (id == null) return NotFound();

        var inventaire = await _context.Inventaires
            .Include(i => i.Boutique)
            .Include(i => i.Depot)
            .Include(i => i.Utilisateur)
            .Include(i => i.LignesInventaire)
                .ThenInclude(li => li.ProduitBoutique)
                    .ThenInclude(pb => pb.Produit)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (inventaire == null) return NotFound();

        // Vérifier les droits d'accès : ManagerBoutique ne peut voir que les inventaires de sa boutique
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        if (!await _userManager.IsInRoleAsync(user, "AdminReseau"))
        {
            var boutiqueId = await _boutiqueActiveService.GetBoutiqueActiveIdAsync(user.Id);
            if (boutiqueId != null && inventaire.BoutiqueId != boutiqueId)
            {
                return Forbid();
            }
        }

        return View(inventaire);
    }

    // GET: Inventaires/Create
    public async Task<IActionResult> Create()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var boutiqueId = await _boutiqueActiveService.GetBoutiqueActiveIdAsync(user.Id);
        if (boutiqueId == null && !await _userManager.IsInRoleAsync(user, "AdminReseau"))
        {
            return RedirectToAction("SelectionBoutique", "Boutiques");
        }

        IQueryable<Boutique> boutiquesQuery = _context.Boutiques.Where(b => b.EstActive);
        if (boutiqueId != null)
        {
            boutiquesQuery = boutiquesQuery.Where(b => b.Id == boutiqueId);
        }

        ViewData["BoutiqueId"] = new SelectList(
            await boutiquesQuery.OrderBy(b => b.Nom).ToListAsync(),
            "Id", "Nom", boutiqueId);

        ViewData["DepotId"] = new SelectList(new List<Depot>(), "Id", "Nom");

        return View();
    }

    // POST: Inventaires/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("BoutiqueId,DepotId,Commentaire")] Inventaire inventaire)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        if (ModelState.IsValid)
        {
            // Générer le numéro d'inventaire
            inventaire.NumeroInventaire = await GenererNumeroInventaireAsync(inventaire.BoutiqueId);
            inventaire.Statut = StatutInventaire.EnCours;
            inventaire.DateDebut = DateTime.UtcNow;
            inventaire.UtilisateurId = user.Id;
            inventaire.DateCreation = DateTime.UtcNow;

            _context.Add(inventaire);
            await _context.SaveChangesAsync();

            // Créer les lignes d'inventaire avec les stocks théoriques
            await CreerLignesInventaireAsync(inventaire.Id, inventaire.DepotId);

            return RedirectToAction(nameof(Details), new { id = inventaire.Id });
        }

        var boutiqueId = await _boutiqueActiveService.GetBoutiqueActiveIdAsync(user.Id);
        var boutiqueIdValue = inventaire.BoutiqueId != Guid.Empty ? inventaire.BoutiqueId : boutiqueId ?? Guid.Empty;
        
        ViewData["BoutiqueId"] = new SelectList(
            await _context.Boutiques.Where(b => b.EstActive).OrderBy(b => b.Nom).ToListAsync(),
            "Id", "Nom", boutiqueIdValue);

        ViewData["DepotId"] = new SelectList(
            await _context.Depots.Where(d => d.BoutiqueId == boutiqueIdValue).OrderBy(d => d.Nom).ToListAsync(),
            "Id", "Nom", inventaire.DepotId);

        return View(inventaire);
    }

    // GET: Inventaires/Comptage/5
    public async Task<IActionResult> Comptage(Guid? id)
    {
        if (id == null) return NotFound();

        var inventaire = await _context.Inventaires
            .Include(i => i.Boutique)
            .Include(i => i.Depot)
            .Include(i => i.LignesInventaire)
                .ThenInclude(li => li.ProduitBoutique)
                    .ThenInclude(pb => pb.Produit)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (inventaire == null) return NotFound();

        // Vérifier les droits d'accès : ManagerBoutique ne peut compter que les inventaires de sa boutique
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        if (!await _userManager.IsInRoleAsync(user, "AdminReseau"))
        {
            var boutiqueId = await _boutiqueActiveService.GetBoutiqueActiveIdAsync(user.Id);
            if (boutiqueId != null && inventaire.BoutiqueId != boutiqueId)
            {
                return Forbid();
            }
        }

        if (inventaire.Statut != StatutInventaire.EnCours)
        {
            TempData["Error"] = "Cet inventaire est déjà finalisé ou annulé.";
            return RedirectToAction(nameof(Details), new { id });
        }

        return View(inventaire);
    }

    // POST: Inventaires/EnregistrerComptage
    [HttpPost]
    public async Task<IActionResult> EnregistrerComptage(Guid ligneInventaireId, int quantiteReelle)
    {
        var ligne = await _context.LignesInventaire
            .Include(li => li.Inventaire)
                .ThenInclude(i => i.Boutique)
            .FirstOrDefaultAsync(li => li.Id == ligneInventaireId);

        if (ligne == null)
        {
            return Json(new { success = false, message = "Ligne non trouvée" });
        }

        // Vérifier les droits d'accès : ManagerBoutique ne peut compter que les inventaires de sa boutique
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Json(new { success = false, message = "Utilisateur non trouvé" });
        }

        if (!await _userManager.IsInRoleAsync(user, "AdminReseau"))
        {
            var boutiqueId = await _boutiqueActiveService.GetBoutiqueActiveIdAsync(user.Id);
            if (boutiqueId != null && ligne.Inventaire.BoutiqueId != boutiqueId)
            {
                return Json(new { success = false, message = "Accès refusé" });
            }
        }

        if (ligne.Inventaire.Statut != StatutInventaire.EnCours)
        {
            return Json(new { success = false, message = "Inventaire non modifiable" });
        }

        ligne.QuantiteReelle = quantiteReelle;
        await _context.SaveChangesAsync();

        return Json(new { success = true, ecart = ligne.Ecart });
    }

    // POST: Inventaires/Finaliser/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Finaliser(Guid id)
    {
        var inventaire = await _context.Inventaires
            .Include(i => i.Boutique)
            .Include(i => i.LignesInventaire)
                .ThenInclude(li => li.ProduitBoutique)
            .Include(i => i.Depot)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (inventaire == null) return NotFound();

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        // Vérifier les droits d'accès : ManagerBoutique ne peut finaliser que les inventaires de sa boutique
        if (!await _userManager.IsInRoleAsync(user, "AdminReseau"))
        {
            var boutiqueId = await _boutiqueActiveService.GetBoutiqueActiveIdAsync(user.Id);
            if (boutiqueId != null && inventaire.BoutiqueId != boutiqueId)
            {
                return Forbid();
            }
        }

        if (inventaire.Statut != StatutInventaire.EnCours)
        {
            TempData["Error"] = "Cet inventaire est déjà finalisé ou annulé.";
            return RedirectToAction(nameof(Details), new { id });
        }

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Créer les mouvements de stock pour chaque écart
            foreach (var ligne in inventaire.LignesInventaire)
            {
                var ecart = ligne.Ecart;
                if (ecart != 0)
                {
                    var mouvement = new MouvementStock
                    {
                        ProduitBoutiqueId = ligne.ProduitBoutiqueId,
                        DepotId = inventaire.DepotId,
                        TypeMouvement = ecart > 0 ? TypeMouvementStock.Ajustement : TypeMouvementStock.Perte,
                        Quantite = ecart,
                        PrixUnitaire = ligne.ProduitBoutique.PrixAchat,
                        InventaireId = inventaire.Id,
                        UtilisateurId = user.Id,
                        Commentaire = $"Inventaire {inventaire.NumeroInventaire}: {ligne.QuantiteTheorique} → {ligne.QuantiteReelle}",
                        DateMouvement = DateTime.UtcNow
                    };

                    _context.MouvementsStock.Add(mouvement);

                    // Mettre à jour le stock
                    var stock = await _context.Stocks
                        .FirstOrDefaultAsync(s => s.ProduitBoutiqueId == ligne.ProduitBoutiqueId && s.DepotId == inventaire.DepotId);

                    if (stock == null)
                    {
                        stock = new Stock
                        {
                            ProduitBoutiqueId = ligne.ProduitBoutiqueId,
                            DepotId = inventaire.DepotId,
                            Quantite = 0
                        };
                        _context.Stocks.Add(stock);
                    }

                    stock.Quantite = ligne.QuantiteReelle;
                    stock.DateDerniereMaj = DateTime.UtcNow;
                }
            }

            inventaire.Statut = StatutInventaire.Finalise;
            inventaire.DateFinalisation = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            TempData["Success"] = "Inventaire finalisé avec succès.";
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            TempData["Error"] = $"Erreur lors de la finalisation: {ex.Message}";
            return RedirectToAction(nameof(Details), new { id });
        }
    }

    // GET: Inventaires/DepotsParBoutique
    [HttpGet]
    public async Task<IActionResult> DepotsParBoutique(Guid boutiqueId)
    {
        var depots = await _context.Depots
            .Where(d => d.BoutiqueId == boutiqueId && d.EstActif)
            .OrderBy(d => d.Nom)
            .Select(d => new { d.Id, d.Nom })
            .ToListAsync();

        return Json(depots);
    }

    private async Task<string> GenererNumeroInventaireAsync(Guid boutiqueId)
    {
        var boutique = await _context.Boutiques.FindAsync(boutiqueId);
        var prefixe = boutique?.Nom.Substring(0, Math.Min(3, boutique.Nom.Length)).ToUpper() ?? "INV";
        var annee = DateTime.UtcNow.Year;
        var mois = DateTime.UtcNow.Month.ToString("00");

        var dernierNumero = await _context.Inventaires
            .Where(i => i.BoutiqueId == boutiqueId && i.NumeroInventaire.StartsWith($"{prefixe}{annee}{mois}"))
            .OrderByDescending(i => i.NumeroInventaire)
            .Select(i => i.NumeroInventaire)
            .FirstOrDefaultAsync();

        int numero = 1;
        if (!string.IsNullOrEmpty(dernierNumero))
        {
            var partieNumero = dernierNumero.Substring(dernierNumero.Length - 4);
            if (int.TryParse(partieNumero, out int dernier))
            {
                numero = dernier + 1;
            }
        }

        return $"{prefixe}{annee}{mois}{numero:D4}";
    }

    private async Task CreerLignesInventaireAsync(Guid inventaireId, Guid depotId)
    {
        var stocks = await _context.Stocks
            .Include(s => s.ProduitBoutique)
                .ThenInclude(pb => pb.Produit)
            .Where(s => s.DepotId == depotId)
            .ToListAsync();

        foreach (var stock in stocks)
        {
            var ligne = new LigneInventaire
            {
                InventaireId = inventaireId,
                ProduitBoutiqueId = stock.ProduitBoutiqueId,
                QuantiteTheorique = stock.Quantite,
                QuantiteReelle = stock.Quantite
            };

            _context.LignesInventaire.Add(ligne);
        }

        await _context.SaveChangesAsync();
    }
}

