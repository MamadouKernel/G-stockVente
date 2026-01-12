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

[Authorize(Roles = "AdminReseau,ManagerBoutique,GestionnaireStock")]
public class AchatsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IBoutiqueActiveService _boutiqueActiveService;

    public AchatsController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IBoutiqueActiveService boutiqueActiveService)
    {
        _context = context;
        _userManager = userManager;
        _boutiqueActiveService = boutiqueActiveService;
    }

    // GET: Achats
    public async Task<IActionResult> Index(StatutAchat? statut)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var boutiqueId = await _boutiqueActiveService.GetBoutiqueActiveIdAsync(user.Id);
        if (boutiqueId == null && !await _userManager.IsInRoleAsync(user, "AdminReseau"))
        {
            return RedirectToAction("SelectionBoutique", "Boutiques");
        }

        IQueryable<Achat> query = _context.Achats
            .Include(a => a.Boutique)
            .Include(a => a.Fournisseur)
            .Include(a => a.Utilisateur)
            .Include(a => a.LignesAchat)
                .ThenInclude(la => la.ProduitBoutique)
                    .ThenInclude(pb => pb.Produit);

        if (boutiqueId != null)
        {
            query = query.Where(a => a.BoutiqueId == boutiqueId);
        }

        if (statut.HasValue)
        {
            query = query.Where(a => a.Statut == statut.Value);
        }

        var achats = await query.OrderByDescending(a => a.DateCommande).ToListAsync();

        ViewBag.Statut = statut;
        ViewBag.Statuts = Enum.GetValues(typeof(StatutAchat)).Cast<StatutAchat>();
        return View(achats);
    }

    // GET: Achats/Details/5
    public async Task<IActionResult> Details(Guid? id)
    {
        if (id == null) return NotFound();

        var achat = await _context.Achats
            .Include(a => a.Boutique)
            .Include(a => a.Fournisseur)
            .Include(a => a.Utilisateur)
            .Include(a => a.LignesAchat)
                .ThenInclude(la => la.ProduitBoutique)
                    .ThenInclude(pb => pb.Produit)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (achat == null) return NotFound();

        // Vérifier les droits d'accès : ManagerBoutique ne peut voir que les achats de sa boutique
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        if (!await _userManager.IsInRoleAsync(user, "AdminReseau"))
        {
            var boutiqueId = await _boutiqueActiveService.GetBoutiqueActiveIdAsync(user.Id);
            if (boutiqueId != null && achat.BoutiqueId != boutiqueId)
            {
                return Forbid();
            }
        }

        return View(achat);
    }

    // GET: Achats/Create
    public async Task<IActionResult> Create()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var boutiqueId = await _boutiqueActiveService.GetBoutiqueActiveIdAsync(user.Id);
        if (boutiqueId == null && !await _userManager.IsInRoleAsync(user, "AdminReseau"))
        {
            return RedirectToAction("SelectionBoutique", "Boutiques");
        }

        var isAdminReseau = await _userManager.IsInRoleAsync(user, "AdminReseau");

        IQueryable<Boutique> boutiquesQuery = _context.Boutiques.Where(b => b.EstActive);
        if (!isAdminReseau && boutiqueId != null)
        {
            boutiquesQuery = boutiquesQuery.Where(b => b.Id == boutiqueId);
        }

        ViewData["BoutiqueId"] = new SelectList(
            await boutiquesQuery.OrderBy(b => b.Nom).ToListAsync(),
            "Id", "Nom", boutiqueId);

        ViewData["FournisseurId"] = new SelectList(
            await _context.Fournisseurs.Where(f => f.EstActif).OrderBy(f => f.Nom).ToListAsync(),
            "Id", "Nom");

        ViewData["Produits"] = await GetProduitsBoutiqueAsync(boutiqueId);

        return View();
    }

    // POST: Achats/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateAchatViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var isAdminReseau = await _userManager.IsInRoleAsync(user, "AdminReseau");
        var boutiqueId = await _boutiqueActiveService.GetBoutiqueActiveIdAsync(user.Id);

        // Sécurité : ManagerBoutique ne peut créer que pour sa boutique
        if (!isAdminReseau && boutiqueId != null && model.BoutiqueId != boutiqueId)
        {
            ModelState.AddModelError("BoutiqueId", "Vous ne pouvez créer des achats que pour votre boutique active.");
        }
        else if (!isAdminReseau && boutiqueId != null)
        {
            model.BoutiqueId = boutiqueId.Value;
        }

        if (ModelState.IsValid && model.Lignes.Any())
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Générer le numéro d'achat
                var numeroAchat = await GenererNumeroAchatAsync(model.BoutiqueId);

                var achat = new Achat
                {
                    NumeroAchat = numeroAchat,
                    NumeroFactureFournisseur = model.NumeroFactureFournisseur,
                    BoutiqueId = model.BoutiqueId,
                    FournisseurId = model.FournisseurId,
                    Statut = StatutAchat.EnAttente,
                    DateCommande = model.DateCommande ?? DateTime.UtcNow,
                    DateReceptionPrevue = model.DateReceptionPrevue,
                    Commentaire = model.Commentaire,
                    UtilisateurId = user.Id,
                    DateCreation = DateTime.UtcNow
                };

                decimal totalHT = 0, totalTVA = 0, totalTTC = 0;

                foreach (var ligneModel in model.Lignes)
                {
                    var ligne = new LigneAchat
                    {
                        Achat = achat,
                        ProduitBoutiqueId = ligneModel.ProduitBoutiqueId,
                        QuantiteCommandee = ligneModel.QuantiteCommandee,
                        PrixUnitaire = ligneModel.PrixUnitaire,
                        TauxTVA = ligneModel.TauxTVA,
                        MontantHT = ligneModel.QuantiteCommandee * ligneModel.PrixUnitaire,
                        MontantTVA = ligneModel.QuantiteCommandee * ligneModel.PrixUnitaire * ligneModel.TauxTVA / 100,
                    };
                    ligne.MontantTTC = ligne.MontantHT + ligne.MontantTVA;

                    totalHT += ligne.MontantHT;
                    totalTVA += ligne.MontantTVA;
                    totalTTC += ligne.MontantTTC;

                    achat.LignesAchat.Add(ligne);
                }

                achat.MontantHT = totalHT;
                achat.MontantTVA = totalTVA;
                achat.MontantTTC = totalTTC;

                _context.Achats.Add(achat);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return RedirectToAction(nameof(Details), new { id = achat.Id });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError("", $"Erreur: {ex.Message}");
            }
        }

        var isAdminReseauForView = await _userManager.IsInRoleAsync(user, "AdminReseau");
        var userBoutiqueIdForView = await _boutiqueActiveService.GetBoutiqueActiveIdAsync(user.Id);

        IQueryable<Boutique> boutiquesQueryForView = _context.Boutiques.Where(b => b.EstActive);
        if (!isAdminReseauForView && userBoutiqueIdForView != null)
        {
            boutiquesQueryForView = boutiquesQueryForView.Where(b => b.Id == userBoutiqueIdForView);
        }

        ViewData["BoutiqueId"] = new SelectList(
            await boutiquesQueryForView.OrderBy(b => b.Nom).ToListAsync(),
            "Id", "Nom", model?.BoutiqueId ?? userBoutiqueIdForView);

        ViewData["FournisseurId"] = new SelectList(
            await _context.Fournisseurs.Where(f => f.EstActif).OrderBy(f => f.Nom).ToListAsync(),
            "Id", "Nom", model?.FournisseurId);

        ViewData["Produits"] = await GetProduitsBoutiqueAsync(model?.BoutiqueId ?? userBoutiqueIdForView);

        return View(model);
    }

    // GET: Achats/Reception/5
    public async Task<IActionResult> Reception(Guid? id)
    {
        if (id == null) return NotFound();

        var achat = await _context.Achats
            .Include(a => a.Boutique)
            .Include(a => a.LignesAchat)
                .ThenInclude(la => la.ProduitBoutique)
                    .ThenInclude(pb => pb.Produit)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (achat == null) return NotFound();

        // Vérifier les droits d'accès : ManagerBoutique ne peut réceptionner que les achats de sa boutique
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        if (!await _userManager.IsInRoleAsync(user, "AdminReseau"))
        {
            var boutiqueId = await _boutiqueActiveService.GetBoutiqueActiveIdAsync(user.Id);
            if (boutiqueId != null && achat.BoutiqueId != boutiqueId)
            {
                return Forbid();
            }
        }

        if (achat.Statut == StatutAchat.Receptionne)
        {
            TempData["Error"] = "Cet achat est déjà réceptionné.";
            return RedirectToAction(nameof(Details), new { id });
        }

        return View(achat);
    }

    // POST: Achats/Reception/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reception(Guid id, Dictionary<Guid, int> quantitesRecues)
    {
        var achat = await _context.Achats
            .Include(a => a.LignesAchat)
                .ThenInclude(la => la.ProduitBoutique)
            .Include(a => a.Boutique)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (achat == null) return NotFound();

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        // Vérifier les droits d'accès : ManagerBoutique ne peut réceptionner que les achats de sa boutique
        if (!await _userManager.IsInRoleAsync(user, "AdminReseau"))
        {
            var boutiqueId = await _boutiqueActiveService.GetBoutiqueActiveIdAsync(user.Id);
            if (boutiqueId != null && achat.BoutiqueId != boutiqueId)
            {
                return Forbid();
            }
        }

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Trouver le dépôt principal de la boutique
            var depot = await _context.Depots
                .FirstOrDefaultAsync(d => d.BoutiqueId == achat.BoutiqueId && d.EstActif);

            if (depot == null)
            {
                TempData["Error"] = "Aucun dépôt actif trouvé pour cette boutique.";
                return RedirectToAction(nameof(Details), new { id });
            }

            foreach (var ligne in achat.LignesAchat)
            {
                if (quantitesRecues.ContainsKey(ligne.Id))
                {
                    ligne.QuantiteRecue = quantitesRecues[ligne.Id];

                    if (ligne.QuantiteRecue > 0)
                    {
                        // Créer le mouvement de stock (entrée)
                        var mouvement = new MouvementStock
                        {
                            ProduitBoutiqueId = ligne.ProduitBoutiqueId,
                            DepotId = depot.Id,
                            TypeMouvement = TypeMouvementStock.Entree,
                            Quantite = ligne.QuantiteRecue,
                            PrixUnitaire = ligne.PrixUnitaire,
                            AchatId = achat.Id,
                            UtilisateurId = user.Id,
                            Commentaire = $"Réception achat {achat.NumeroAchat}",
                            DateMouvement = DateTime.UtcNow
                        };

                        _context.MouvementsStock.Add(mouvement);

                        // Mettre à jour le stock
                        var stock = await _context.Stocks
                            .FirstOrDefaultAsync(s => s.ProduitBoutiqueId == ligne.ProduitBoutiqueId && s.DepotId == depot.Id);

                        if (stock == null)
                        {
                            stock = new Stock
                            {
                                ProduitBoutiqueId = ligne.ProduitBoutiqueId,
                                DepotId = depot.Id,
                                Quantite = 0
                            };
                            _context.Stocks.Add(stock);
                        }

                        stock.Quantite += ligne.QuantiteRecue;
                        stock.DateDerniereMaj = DateTime.UtcNow;

                        // Mettre à jour le prix d'achat du produit
                        ligne.ProduitBoutique.PrixAchat = ligne.PrixUnitaire;
                        ligne.ProduitBoutique.DateModification = DateTime.UtcNow;
                    }
                }
            }

            achat.Statut = StatutAchat.Receptionne;
            achat.DateReception = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            TempData["Success"] = "Réception enregistrée avec succès.";
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            TempData["Error"] = $"Erreur lors de la réception: {ex.Message}";
            return RedirectToAction(nameof(Reception), new { id });
        }
    }

    private async Task<string> GenererNumeroAchatAsync(Guid boutiqueId)
    {
        var boutique = await _context.Boutiques.FindAsync(boutiqueId);
        var prefixe = boutique?.Nom.Substring(0, Math.Min(3, boutique.Nom.Length)).ToUpper() ?? "ACH";
        var annee = DateTime.UtcNow.Year;
        var mois = DateTime.UtcNow.Month.ToString("00");

        var dernierNumero = await _context.Achats
            .Where(a => a.BoutiqueId == boutiqueId && a.NumeroAchat.StartsWith($"{prefixe}{annee}{mois}"))
            .OrderByDescending(a => a.NumeroAchat)
            .Select(a => a.NumeroAchat)
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

    private async Task<List<ProduitBoutique>> GetProduitsBoutiqueAsync(Guid? boutiqueId)
    {
        if (boutiqueId == null) return new List<ProduitBoutique>();

        return await _context.ProduitsBoutique
            .Include(pb => pb.Produit)
            .Where(pb => pb.BoutiqueId == boutiqueId && pb.EstActif)
            .OrderBy(pb => pb.Produit.Nom)
            .ToListAsync();
    }
}

public class CreateAchatViewModel
{
    [Required]
    public Guid BoutiqueId { get; set; }

    [Required]
    [Display(Name = "Fournisseur")]
    public Guid FournisseurId { get; set; }

    [StringLength(100)]
    [Display(Name = "Numéro facture fournisseur")]
    public string? NumeroFactureFournisseur { get; set; }

    [Display(Name = "Date de commande")]
    public DateTime? DateCommande { get; set; }

    [Display(Name = "Date de réception prévue")]
    public DateTime? DateReceptionPrevue { get; set; }

    [StringLength(1000)]
    public string? Commentaire { get; set; }

    public List<LigneAchatViewModel> Lignes { get; set; } = new();
}

public class LigneAchatViewModel
{
    [Required]
    public Guid ProduitBoutiqueId { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    [Display(Name = "Quantité")]
    public int QuantiteCommandee { get; set; }

    [Required]
    [Range(0, double.MaxValue)]
    [Display(Name = "Prix unitaire")]
    public decimal PrixUnitaire { get; set; }

    [Range(0, 100)]
    [Display(Name = "Taux TVA (%)")]
    public decimal TauxTVA { get; set; } = 0;
}
