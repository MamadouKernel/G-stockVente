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
public class TransfertsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IBoutiqueActiveService _boutiqueActiveService;
    private readonly ITransfertStockService _transfertStockService;
    private readonly INotificationService _notificationService;

    public TransfertsController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IBoutiqueActiveService boutiqueActiveService,
        ITransfertStockService transfertStockService,
        INotificationService notificationService)
    {
        _context = context;
        _userManager = userManager;
        _boutiqueActiveService = boutiqueActiveService;
        _transfertStockService = transfertStockService;
        _notificationService = notificationService;
    }

    // GET: Transferts
    public async Task<IActionResult> Index(StatutTransfert? statut)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var boutiqueId = await _boutiqueActiveService.GetBoutiqueActiveIdAsync(user.Id);
        var isAdminReseau = await _userManager.IsInRoleAsync(user, "AdminReseau");

        if (boutiqueId == null && !isAdminReseau)
        {
            return RedirectToAction("SelectionBoutique", "Boutiques");
        }

        IQueryable<TransfertStock> query = _context.TransfertsStock
            .Include(t => t.BoutiqueSource)
            .Include(t => t.BoutiqueDestination)
            .Include(t => t.DepotSource)
            .Include(t => t.DepotDestination)
            .Include(t => t.UtilisateurCreateur)
            .Include(t => t.LignesTransfert)
                .ThenInclude(l => l.ProduitBoutique)
                    .ThenInclude(pb => pb.Produit);

        // Filtrer par boutique si pas Admin Réseau
        if (!isAdminReseau && boutiqueId.HasValue)
        {
            query = query.Where(t => t.BoutiqueSourceId == boutiqueId || t.BoutiqueDestinationId == boutiqueId);
        }

        if (statut.HasValue)
        {
            query = query.Where(t => t.Statut == statut.Value);
        }

        var transferts = await query.OrderByDescending(t => t.DateCreation).ToListAsync();

        ViewBag.Statut = statut;
        ViewBag.Statuts = Enum.GetValues(typeof(StatutTransfert)).Cast<StatutTransfert>();
        return View(transferts);
    }

    // GET: Transferts/Details/5
    public async Task<IActionResult> Details(Guid? id)
    {
        if (id == null) return NotFound();

        var transfert = await _transfertStockService.GetByIdAsync(id.Value);
        if (transfert == null) return NotFound();

        // Vérifier l'accès (boutique active)
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var boutiqueId = await _boutiqueActiveService.GetBoutiqueActiveIdAsync(user.Id);
        var isAdminReseau = await _userManager.IsInRoleAsync(user, "AdminReseau");

        if (!isAdminReseau && boutiqueId.HasValue)
        {
            if (transfert.BoutiqueSourceId != boutiqueId && transfert.BoutiqueDestinationId != boutiqueId)
            {
                return Forbid();
            }
        }

        return View(transfert);
    }

    // GET: Transferts/Create
    public async Task<IActionResult> Create()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var boutiqueId = await _boutiqueActiveService.GetBoutiqueActiveIdAsync(user.Id);
        if (boutiqueId == null)
        {
            return RedirectToAction("SelectionBoutique", "Boutiques");
        }

        // Charger les dépôts de la boutique active
        var depots = await _context.Depots
            .Where(d => d.BoutiqueId == boutiqueId)
            .ToListAsync();

        ViewBag.DepotSourceId = new SelectList(depots, "Id", "Nom");
        ViewBag.DepotDestinationId = new SelectList(depots, "Id", "Nom");

        // Pour les transferts inter-boutiques (Admin Réseau uniquement)
        var isAdminReseau = await _userManager.IsInRoleAsync(user, "AdminReseau");
        if (isAdminReseau)
        {
            var boutiques = await _context.Boutiques
                .Where(b => b.EstActive)
                .OrderBy(b => b.Nom)
                .ToListAsync();
            ViewBag.BoutiqueSourceId = new SelectList(boutiques, "Id", "Nom");
            ViewBag.BoutiqueDestinationId = new SelectList(boutiques, "Id", "Nom");
            ViewBag.TousDepots = await _context.Depots
                .Include(d => d.Boutique)
                .Where(d => d.Boutique!.EstActive)
                .OrderBy(d => d.Boutique!.Nom)
                .ThenBy(d => d.Nom)
                .Select(d => new { d.Id, Nom = $"{d.Boutique!.Nom} - {d.Nom}" })
                .ToListAsync();
        }

        // Charger les produits de la boutique active
        var produitsBoutique = await _context.ProduitsBoutique
            .Include(pb => pb.Produit)
            .Where(pb => pb.BoutiqueId == boutiqueId && pb.EstActif)
            .OrderBy(pb => pb.Produit.Nom)
            .ToListAsync();

        ViewBag.ProduitsBoutique = produitsBoutique;

        return View();
    }

    // POST: Transferts/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        Guid depotSourceId,
        Guid depotDestinationId,
        Guid? boutiqueSourceId,
        Guid? boutiqueDestinationId,
        List<Guid> produitBoutiqueIds,
        List<int> quantites,
        string? notes)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var boutiqueId = await _boutiqueActiveService.GetBoutiqueActiveIdAsync(user.Id);
        if (boutiqueId == null)
        {
            return RedirectToAction("SelectionBoutique", "Boutiques");
        }

        if (produitBoutiqueIds == null || quantites == null || produitBoutiqueIds.Count == 0)
        {
            TempData["Error"] = "Veuillez sélectionner au moins un produit.";
            return RedirectToAction(nameof(Create));
        }

        if (produitBoutiqueIds.Count != quantites.Count)
        {
            TempData["Error"] = "Le nombre de produits ne correspond pas au nombre de quantités.";
            return RedirectToAction(nameof(Create));
        }

        // Vérifier que les dépôts existent et appartiennent à la bonne boutique
        var depotSource = await _context.Depots
            .Include(d => d.Boutique)
            .FirstOrDefaultAsync(d => d.Id == depotSourceId);
        var depotDestination = await _context.Depots
            .Include(d => d.Boutique)
            .FirstOrDefaultAsync(d => d.Id == depotDestinationId);

        if (depotSource == null || depotDestination == null)
        {
            TempData["Error"] = "Dépôt source ou destination introuvable.";
            return RedirectToAction(nameof(Create));
        }

        // Déterminer les boutiques source et destination
        var isAdminReseau = await _userManager.IsInRoleAsync(user, "AdminReseau");
        if (!isAdminReseau)
        {
            boutiqueSourceId = boutiqueId;
            boutiqueDestinationId = depotDestination.BoutiqueId;
        }
        else
        {
            boutiqueSourceId = boutiqueSourceId ?? depotSource.BoutiqueId;
            boutiqueDestinationId = boutiqueDestinationId ?? depotDestination.BoutiqueId;
        }

        // Créer le transfert
        var transfert = new TransfertStock
        {
            DepotSourceId = depotSourceId,
            DepotDestinationId = depotDestinationId,
            BoutiqueSourceId = boutiqueSourceId,
            BoutiqueDestinationId = boutiqueDestinationId,
            UtilisateurCreateurId = user.Id,
            Statut = StatutTransfert.EnAttente,
            Notes = notes,
            DateCreation = DateTime.UtcNow
        };

        // Créer les lignes de transfert
        for (int i = 0; i < produitBoutiqueIds.Count; i++)
        {
            if (quantites[i] > 0)
            {
                transfert.LignesTransfert.Add(new LigneTransfertStock
                {
                    ProduitBoutiqueId = produitBoutiqueIds[i],
                    Quantite = quantites[i]
                });
            }
        }

        if (transfert.LignesTransfert.Count == 0)
        {
            TempData["Error"] = "Aucune quantité valide n'a été saisie.";
            return RedirectToAction(nameof(Create));
        }

        try
        {
            transfert = await _transfertStockService.CreateAsync(transfert);

            // Créer une notification pour les managers de la boutique destination
            if (boutiqueDestinationId.HasValue)
            {
                await _notificationService.CreerNotificationTransfertAsync(
                    transfert.Id,
                    TypeNotification.TransfertEnAttente,
                    null);
            }

            TempData["Success"] = $"Transfert {transfert.Numero} créé avec succès.";
            return RedirectToAction(nameof(Details), new { id = transfert.Id });
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Erreur lors de la création du transfert : {ex.Message}";
            return RedirectToAction(nameof(Create));
        }
    }

    // POST: Transferts/Valider/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "AdminReseau,ManagerBoutique")]
    public async Task<IActionResult> Valider(Guid id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var transfert = await _transfertStockService.GetByIdAsync(id);
        if (transfert == null)
        {
            TempData["Error"] = "Transfert introuvable.";
            return RedirectToAction(nameof(Index));
        }

        // Vérifier l'accès (doit être manager de la boutique source)
        var boutiqueId = await _boutiqueActiveService.GetBoutiqueActiveIdAsync(user.Id);
        var isAdminReseau = await _userManager.IsInRoleAsync(user, "AdminReseau");

        if (!isAdminReseau && boutiqueId.HasValue)
        {
            if (transfert.BoutiqueSourceId != boutiqueId)
            {
                TempData["Error"] = "Vous n'êtes pas autorisé à valider ce transfert.";
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        var success = await _transfertStockService.ValiderAsync(id, user.Id);
        if (success)
        {
            TempData["Success"] = $"Transfert {transfert.Numero} validé avec succès.";

            // Notifier la boutique destination
            if (transfert.BoutiqueDestinationId.HasValue)
            {
                await _notificationService.CreerNotificationTransfertAsync(
                    id,
                    TypeNotification.TransfertRecu,
                    null);
            }
        }
        else
        {
            TempData["Error"] = "Impossible de valider le transfert. Vérifiez que les stocks sont suffisants.";
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    // GET: Transferts/Recevoir/5
    [Authorize(Roles = "AdminReseau,ManagerBoutique,GestionnaireStock")]
    public async Task<IActionResult> Recevoir(Guid? id)
    {
        if (id == null) return NotFound();

        var transfert = await _transfertStockService.GetByIdAsync(id.Value);
        if (transfert == null) return NotFound();

        if (transfert.Statut != StatutTransfert.Valide && transfert.Statut != StatutTransfert.EnTransit)
        {
            TempData["Error"] = "Ce transfert ne peut pas être reçu dans son état actuel.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // Vérifier l'accès (doit être manager de la boutique destination)
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var boutiqueId = await _boutiqueActiveService.GetBoutiqueActiveIdAsync(user.Id);
        var isAdminReseau = await _userManager.IsInRoleAsync(user, "AdminReseau");

        if (!isAdminReseau && boutiqueId.HasValue)
        {
            if (transfert.BoutiqueDestinationId != boutiqueId)
            {
                TempData["Error"] = "Vous n'êtes pas autorisé à recevoir ce transfert.";
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        return View(transfert);
    }

    // POST: Transferts/Recevoir/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "AdminReseau,ManagerBoutique,GestionnaireStock")]
    public async Task<IActionResult> Recevoir(Guid id, Dictionary<Guid, int> quantitesRecues)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var transfert = await _transfertStockService.GetByIdAsync(id);
        if (transfert == null)
        {
            TempData["Error"] = "Transfert introuvable.";
            return RedirectToAction(nameof(Index));
        }

        // Vérifier l'accès
        var boutiqueId = await _boutiqueActiveService.GetBoutiqueActiveIdAsync(user.Id);
        var isAdminReseau = await _userManager.IsInRoleAsync(user, "AdminReseau");

        if (!isAdminReseau && boutiqueId.HasValue)
        {
            if (transfert.BoutiqueDestinationId != boutiqueId)
            {
                TempData["Error"] = "Vous n'êtes pas autorisé à recevoir ce transfert.";
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        try
        {
            var success = await _transfertStockService.RecevoirAsync(id, user.Id, quantitesRecues);
            if (success)
            {
                TempData["Success"] = $"Transfert {transfert.Numero} reçu avec succès. Les stocks ont été mis à jour.";
            }
            else
            {
                TempData["Error"] = "Impossible de recevoir le transfert. Vérifiez les données saisies.";
            }
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Erreur lors de la réception : {ex.Message}";
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    // POST: Transferts/Annuler/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Annuler(Guid id, string? raison)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var transfert = await _transfertStockService.GetByIdAsync(id);
        if (transfert == null)
        {
            TempData["Error"] = "Transfert introuvable.";
            return RedirectToAction(nameof(Index));
        }

        // Vérifier l'accès (créateur ou manager de la boutique source/destination)
        var boutiqueId = await _boutiqueActiveService.GetBoutiqueActiveIdAsync(user.Id);
        var isAdminReseau = await _userManager.IsInRoleAsync(user, "AdminReseau");

        if (!isAdminReseau && boutiqueId.HasValue)
        {
            if (transfert.UtilisateurCreateurId != user.Id 
                && transfert.BoutiqueSourceId != boutiqueId 
                && transfert.BoutiqueDestinationId != boutiqueId)
            {
                TempData["Error"] = "Vous n'êtes pas autorisé à annuler ce transfert.";
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        var success = await _transfertStockService.AnnulerAsync(id, user.Id, raison);
        if (success)
        {
            TempData["Success"] = $"Transfert {transfert.Numero} annulé avec succès.";
        }
        else
        {
            TempData["Error"] = "Impossible d'annuler ce transfert dans son état actuel.";
        }

        return RedirectToAction(nameof(Details), new { id });
    }
}

