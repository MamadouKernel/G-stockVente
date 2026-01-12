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
public class StocksController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IBoutiqueActiveService _boutiqueActiveService;

    public StocksController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IBoutiqueActiveService boutiqueActiveService)
    {
        _context = context;
        _userManager = userManager;
        _boutiqueActiveService = boutiqueActiveService;
    }

    // GET: Stocks
    public async Task<IActionResult> Index(Guid? depotId, bool? stockBas)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var boutiqueId = await _boutiqueActiveService.GetBoutiqueActiveIdAsync(user.Id);
        if (boutiqueId == null && !await _userManager.IsInRoleAsync(user, "AdminReseau"))
        {
            return RedirectToAction("SelectionBoutique", "Boutiques");
        }

        IQueryable<Stock> query = _context.Stocks
            .Include(s => s.ProduitBoutique)
                .ThenInclude(pb => pb.Produit)
            .Include(s => s.Depot)
            .ThenInclude(d => d.Boutique);

        if (boutiqueId != null)
        {
            query = query.Where(s => s.Depot.BoutiqueId == boutiqueId);
        }

        if (depotId.HasValue)
        {
            query = query.Where(s => s.DepotId == depotId.Value);
        }

        if (stockBas == true)
        {
            query = query.Where(s => s.ProduitBoutique.SeuilStockBas > 0 
                && s.Quantite <= s.ProduitBoutique.SeuilStockBas);
        }

        var stocks = await query.OrderBy(s => s.ProduitBoutique.Produit.Nom).ToListAsync();

        // Liste des dépôts pour le filtre
        IQueryable<Depot> depotsQuery = _context.Depots.Include(d => d.Boutique);
        if (boutiqueId != null)
        {
            depotsQuery = depotsQuery.Where(d => d.BoutiqueId == boutiqueId);
        }
        ViewBag.Depots = new SelectList(await depotsQuery.OrderBy(d => d.Nom).ToListAsync(), "Id", "Nom", depotId);

        ViewBag.DepotId = depotId;
        ViewBag.StockBas = stockBas;
        return View(stocks);
    }

    // GET: Stocks/Details/5
    public async Task<IActionResult> Details(Guid? id)
    {
        if (id == null) return NotFound();

        var stock = await _context.Stocks
            .Include(s => s.ProduitBoutique)
                .ThenInclude(pb => pb.Boutique)
            .Include(s => s.ProduitBoutique)
                .ThenInclude(pb => pb.Produit)
            .Include(s => s.Depot)
                .ThenInclude(d => d.Boutique)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (stock == null) return NotFound();

        // Vérifier les droits d'accès : ManagerBoutique ne peut voir que les stocks de sa boutique
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        if (!await _userManager.IsInRoleAsync(user, "AdminReseau"))
        {
            var boutiqueId = await _boutiqueActiveService.GetBoutiqueActiveIdAsync(user.Id);
            if (boutiqueId != null && stock.Depot.BoutiqueId != boutiqueId)
            {
                return Forbid();
            }
        }

        // Récupérer l'historique des mouvements
        var mouvements = await _context.MouvementsStock
            .Include(m => m.Utilisateur)
            .Where(m => m.ProduitBoutiqueId == stock.ProduitBoutiqueId && m.DepotId == stock.DepotId)
            .OrderByDescending(m => m.DateMouvement)
            .Take(50)
            .ToListAsync();

        ViewBag.Mouvements = mouvements;
        return View(stock);
    }

    // GET: Stocks/Ajustement
    [Authorize(Roles = "AdminReseau,ManagerBoutique,GestionnaireStock")]
    public async Task<IActionResult> Ajustement(Guid? stockId)
    {
        if (stockId == null) return NotFound();

        var stock = await _context.Stocks
            .Include(s => s.ProduitBoutique)
                .ThenInclude(pb => pb.Boutique)
            .Include(s => s.ProduitBoutique)
                .ThenInclude(pb => pb.Produit)
            .Include(s => s.Depot)
                .ThenInclude(d => d.Boutique)
            .FirstOrDefaultAsync(m => m.Id == stockId);

        if (stock == null) return NotFound();

        // Vérifier les droits d'accès : ManagerBoutique ne peut ajuster que les stocks de sa boutique
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        if (!await _userManager.IsInRoleAsync(user, "AdminReseau"))
        {
            var boutiqueId = await _boutiqueActiveService.GetBoutiqueActiveIdAsync(user.Id);
            if (boutiqueId != null && stock.Depot.BoutiqueId != boutiqueId)
            {
                return Forbid();
            }
        }

        ViewBag.Stock = stock;
        return View();
    }

    // POST: Stocks/Ajustement
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "AdminReseau,ManagerBoutique,GestionnaireStock")]
    public async Task<IActionResult> Ajustement(Guid stockId, int nouvelleQuantite, string? commentaire)
    {
        var stock = await _context.Stocks
            .Include(s => s.ProduitBoutique)
            .Include(s => s.Depot)
                .ThenInclude(d => d.Boutique)
            .FirstOrDefaultAsync(s => s.Id == stockId);

        if (stock == null) return NotFound();

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        // Vérifier les droits d'accès : ManagerBoutique ne peut ajuster que les stocks de sa boutique
        if (!await _userManager.IsInRoleAsync(user, "AdminReseau"))
        {
            var boutiqueId = await _boutiqueActiveService.GetBoutiqueActiveIdAsync(user.Id);
            if (boutiqueId != null && stock.Depot.BoutiqueId != boutiqueId)
            {
                return Forbid();
            }
        }

        var ecart = nouvelleQuantite - stock.Quantite;

        if (ecart != 0)
        {
            // Créer le mouvement d'ajustement
            var mouvement = new MouvementStock
            {
                ProduitBoutiqueId = stock.ProduitBoutiqueId,
                DepotId = stock.DepotId,
                TypeMouvement = ecart > 0 ? TypeMouvementStock.Ajustement : TypeMouvementStock.Ajustement,
                Quantite = ecart,
                PrixUnitaire = stock.ProduitBoutique.PrixAchat,
                Commentaire = commentaire ?? $"Ajustement de stock: {stock.Quantite} → {nouvelleQuantite}",
                UtilisateurId = user.Id,
                DateMouvement = DateTime.UtcNow
            };

            _context.MouvementsStock.Add(mouvement);

            // Mettre à jour le stock
            stock.Quantite = nouvelleQuantite;
            stock.DateDerniereMaj = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Details), new { id = stockId });
    }

    // GET: Stocks/Mouvements
    public async Task<IActionResult> Mouvements(DateTime? dateDebut, DateTime? dateFin, Guid? produitBoutiqueId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var boutiqueId = await _boutiqueActiveService.GetBoutiqueActiveIdAsync(user.Id);
        if (boutiqueId == null && !await _userManager.IsInRoleAsync(user, "AdminReseau"))
        {
            return RedirectToAction("SelectionBoutique", "Boutiques");
        }

        // PostgreSQL (timestamptz) attend des DateTime en UTC pour les paramètres
        if (dateDebut.HasValue && dateDebut.Value.Kind == DateTimeKind.Unspecified)
        {
            dateDebut = DateTime.SpecifyKind(dateDebut.Value, DateTimeKind.Utc);
        }
        if (dateFin.HasValue && dateFin.Value.Kind == DateTimeKind.Unspecified)
        {
            dateFin = DateTime.SpecifyKind(dateFin.Value, DateTimeKind.Utc);
        }

        IQueryable<MouvementStock> query = _context.MouvementsStock
            .Include(m => m.ProduitBoutique)
                .ThenInclude(pb => pb.Produit)
            .Include(m => m.Depot)
            .Include(m => m.Utilisateur);

        if (boutiqueId != null)
        {
            query = query.Where(m => m.Depot.BoutiqueId == boutiqueId);
        }

        if (produitBoutiqueId.HasValue)
        {
            query = query.Where(m => m.ProduitBoutiqueId == produitBoutiqueId.Value);
        }

        if (dateDebut.HasValue)
        {
            query = query.Where(m => m.DateMouvement >= dateDebut.Value);
        }

        if (dateFin.HasValue)
        {
            query = query.Where(m => m.DateMouvement <= dateFin.Value.AddDays(1));
        }

        var mouvements = await query.OrderByDescending(m => m.DateMouvement).Take(500).ToListAsync();

        // Liste des produits pour le filtre
        IQueryable<ProduitBoutique> produitsQuery = _context.ProduitsBoutique
            .Include(pb => pb.Produit)
            .Include(pb => pb.Boutique);

        if (boutiqueId != null)
        {
            produitsQuery = produitsQuery.Where(pb => pb.BoutiqueId == boutiqueId);
        }

        ViewBag.Produits = new SelectList(
            await produitsQuery.OrderBy(pb => pb.Produit.Nom).ToListAsync(),
            "Id", "Produit.Nom", produitBoutiqueId);

        ViewBag.DateDebut = dateDebut;
        ViewBag.DateFin = dateFin;
        ViewBag.ProduitBoutiqueId = produitBoutiqueId;

        return View(mouvements);
    }
}
