using G_StockVente.Domain.Models;
using G_StockVente.Infrastructure.Data;
using G_StockVente.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace G_StockVente.Controllers;

[Authorize(Roles = "AdminReseau,ManagerBoutique,Caissier")]
public class VentesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IBoutiqueActiveService _boutiqueActiveService;
    private readonly IPaiementIntegreService _paiementIntegreService;

    public VentesController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IBoutiqueActiveService boutiqueActiveService,
        IPaiementIntegreService paiementIntegreService)
    {
        _context = context;
        _userManager = userManager;
        _boutiqueActiveService = boutiqueActiveService;
        _paiementIntegreService = paiementIntegreService;
    }

    // GET: Ventes
    public async Task<IActionResult> Index(DateTime? dateDebut, DateTime? dateFin)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var boutiqueId = await _boutiqueActiveService.GetBoutiqueActiveIdAsync(user.Id);
        if (boutiqueId == null && !await _userManager.IsInRoleAsync(user, "AdminReseau"))
        {
            return RedirectToAction("SelectionBoutique", "Boutiques");
        }

        IQueryable<Vente> query = _context.Ventes
            .Include(v => v.Boutique)
            .Include(v => v.Utilisateur)
            .Include(v => v.LignesVente)
            .ThenInclude(lv => lv.ProduitBoutique)
            .ThenInclude(pb => pb.Produit);

        if (boutiqueId != null)
        {
            query = query.Where(v => v.BoutiqueId == boutiqueId);
        }

        if (dateDebut.HasValue)
        {
            query = query.Where(v => v.DateVente >= dateDebut.Value);
        }

        if (dateFin.HasValue)
        {
            query = query.Where(v => v.DateVente <= dateFin.Value.AddDays(1));
        }

        var ventes = await query.OrderByDescending(v => v.DateVente).Take(100).ToListAsync();

        ViewBag.DateDebut = dateDebut;
        ViewBag.DateFin = dateFin;
        return View(ventes);
    }

    // GET: Ventes/Caisse (Interface de caisse)
    public async Task<IActionResult> Caisse()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var boutiqueId = await _boutiqueActiveService.GetBoutiqueActiveIdAsync(user.Id);
        if (boutiqueId == null)
        {
            return RedirectToAction("SelectionBoutique", "Boutiques");
        }

        var boutique = await _boutiqueActiveService.GetBoutiqueActiveAsync(user.Id);
        ViewBag.Boutique = boutique;
        ViewBag.TvaParDefaut = boutique?.TvaParDefaut ?? 0;

        // Charger le panier existant
        var panier = PanierService.GetPanier(HttpContext.Session);
        ViewBag.Panier = panier;
        ViewBag.TotalPanier = CalculerTotalPanier(panier);

        return View();
    }

    // POST: Ventes/AjouterAuPanier
    [HttpPost]
    public async Task<IActionResult> AjouterAuPanier(Guid produitBoutiqueId, int quantite = 1)
    {
        var produitBoutique = await _context.ProduitsBoutique
            .Include(pb => pb.Produit)
            .FirstOrDefaultAsync(pb => pb.Id == produitBoutiqueId && pb.EstActif);

        if (produitBoutique == null)
        {
            return Json(new { success = false, message = "Produit non trouvé" });
        }

        var panier = PanierService.GetPanier(HttpContext.Session);
        var ligneExistante = panier.FirstOrDefault(l => l.ProduitBoutiqueId == produitBoutiqueId);

        if (ligneExistante != null)
        {
            ligneExistante.Quantite += quantite;
            RecalculerLigne(ligneExistante, produitBoutique);
        }
        else
        {
            var nouvelleLigne = new LignePanier
            {
                ProduitBoutiqueId = produitBoutiqueId,
                NomProduit = produitBoutique.Produit.Nom,
                Sku = produitBoutique.Sku,
                Quantite = quantite,
                PrixUnitaire = produitBoutique.PrixVente,
                TauxTVA = await GetTvaParDefautAsync()
            };
            RecalculerLigne(nouvelleLigne, produitBoutique);
            panier.Add(nouvelleLigne);
        }

        PanierService.SavePanier(HttpContext.Session, panier);
        return Json(new { success = true, total = CalculerTotalPanier(panier) });
    }

    // POST: Ventes/ModifierQuantite
    [HttpPost]
    public IActionResult ModifierQuantite(Guid produitBoutiqueId, int quantite)
    {
        var panier = PanierService.GetPanier(HttpContext.Session);
        var ligne = panier.FirstOrDefault(l => l.ProduitBoutiqueId == produitBoutiqueId);

        if (ligne == null)
        {
            return Json(new { success = false, message = "Ligne non trouvée" });
        }

        if (quantite <= 0)
        {
            panier.Remove(ligne);
        }
        else
        {
            ligne.Quantite = quantite;
            ligne.MontantHT = ligne.PrixUnitaire * ligne.Quantite * (1 - ligne.Remise / 100);
            ligne.MontantTVA = ligne.MontantHT * ligne.TauxTVA / 100;
            ligne.MontantTTC = ligne.MontantHT + ligne.MontantTVA;
        }

        PanierService.SavePanier(HttpContext.Session, panier);
        return Json(new { success = true, total = CalculerTotalPanier(panier) });
    }

    // POST: Ventes/RetirerDuPanier
    [HttpPost]
    public IActionResult RetirerDuPanier(Guid produitBoutiqueId)
    {
        var panier = PanierService.GetPanier(HttpContext.Session);
        var ligne = panier.FirstOrDefault(l => l.ProduitBoutiqueId == produitBoutiqueId);

        if (ligne != null)
        {
            panier.Remove(ligne);
            PanierService.SavePanier(HttpContext.Session, panier);
        }

        return Json(new { success = true, total = CalculerTotalPanier(panier) });
    }

    // POST: Ventes/ViderPanier
    [HttpPost]
    public IActionResult ViderPanier()
    {
        PanierService.ClearPanier(HttpContext.Session);
        return Json(new { success = true });
    }

    // POST: Ventes/ValiderVente
    [HttpPost]
    public async Task<IActionResult> ValiderVente(ModePaiement modePaiement, string? commentaire)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Json(new { success = false, message = "Utilisateur non trouvé" });

        var boutiqueId = await _boutiqueActiveService.GetBoutiqueActiveIdAsync(user.Id);
        if (boutiqueId == null)
        {
            return Json(new { success = false, message = "Boutique active requise" });
        }

        var panier = PanierService.GetPanier(HttpContext.Session);
        if (!panier.Any())
        {
            return Json(new { success = false, message = "Panier vide" });
        }

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Générer le numéro de vente
            var numeroVente = await GenererNumeroVenteAsync(boutiqueId.Value);

            // Créer la vente
            var vente = new Vente
            {
                NumeroVente = numeroVente,
                BoutiqueId = boutiqueId.Value,
                Statut = StatutVente.Validee,
                ModePaiement = modePaiement,
                Commentaire = commentaire,
                UtilisateurId = user.Id,
                DateVente = DateTime.UtcNow
            };

            decimal totalHT = 0, totalTVA = 0, totalTTC = 0;

            // Créer les lignes de vente
            foreach (var lignePanier in panier)
            {
                var produitBoutique = await _context.ProduitsBoutique
                    .Include(pb => pb.Produit)
                    .FirstOrDefaultAsync(pb => pb.Id == lignePanier.ProduitBoutiqueId);

                if (produitBoutique == null) continue;

                var ligneVente = new LigneVente
                {
                    Vente = vente,
                    ProduitBoutiqueId = lignePanier.ProduitBoutiqueId,
                    Quantite = lignePanier.Quantite,
                    PrixUnitaire = lignePanier.PrixUnitaire,
                    Remise = lignePanier.Remise,
                    TauxTVA = lignePanier.TauxTVA,
                    MontantHT = lignePanier.MontantHT,
                    MontantTVA = lignePanier.MontantTVA,
                    MontantTTC = lignePanier.MontantTTC
                };

                totalHT += ligneVente.MontantHT;
                totalTVA += ligneVente.MontantTVA;
                totalTTC += ligneVente.MontantTTC;

                vente.LignesVente.Add(ligneVente);

                // Créer le mouvement de stock (sortie)
                await CreerMouvementStockAsync(
                    produitBoutique.Id,
                    boutiqueId.Value,
                    -lignePanier.Quantite,
                    lignePanier.PrixUnitaire,
                    TypeMouvementStock.Sortie,
                    vente.Id,
                    user.Id);
            }

            vente.MontantHT = totalHT;
            vente.MontantTVA = totalTVA;
            vente.MontantTTC = totalTTC;

            _context.Ventes.Add(vente);
            await _context.SaveChangesAsync();

            // Si paiement intégré (Mobile Money ou Carte), créer l'enregistrement
            if (modePaiement == ModePaiement.MobileMoney || modePaiement == ModePaiement.Carte)
            {
                var typePaiement = modePaiement == ModePaiement.MobileMoney 
                    ? TypePaiementIntegre.MobileMoney 
                    : TypePaiementIntegre.CarteBancaire;

                // Initier le paiement intégré (sera confirmé via l'API externe)
                await _paiementIntegreService.InitierPaiementAsync(vente.Id, typePaiement, totalTTC);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            // Vider le panier
            PanierService.ClearPanier(HttpContext.Session);

            return Json(new { success = true, venteId = vente.Id, numeroVente = vente.NumeroVente });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return Json(new { success = false, message = $"Erreur: {ex.Message}" });
        }
    }

    // GET: Ventes/Details/5
    public async Task<IActionResult> Details(Guid? id)
    {
        if (id == null) return NotFound();

        var vente = await _context.Ventes
            .Include(v => v.Boutique)
            .Include(v => v.Utilisateur)
            .Include(v => v.LignesVente)
                .ThenInclude(lv => lv.ProduitBoutique)
                    .ThenInclude(pb => pb.Produit)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (vente == null) return NotFound();

        return View(vente);
    }

    // POST: Ventes/Annuler/5
    [HttpPost]
    [Authorize(Roles = "AdminReseau,ManagerBoutique")]
    public async Task<IActionResult> Annuler(Guid id, string? raison)
    {
        var vente = await _context.Ventes
            .Include(v => v.LignesVente)
            .FirstOrDefaultAsync(v => v.Id == id);

        if (vente == null) return NotFound();

        if (vente.Statut == StatutVente.Annulee)
        {
            return Json(new { success = false, message = "Vente déjà annulée" });
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Json(new { success = false, message = "Utilisateur non trouvé" });

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            vente.Statut = StatutVente.Annulee;
            vente.DateAnnulation = DateTime.UtcNow;
            vente.UtilisateurAnnulationId = user.Id;

            // Créer les mouvements de stock inversés (retours)
            foreach (var ligne in vente.LignesVente)
            {
                await CreerMouvementStockAsync(
                    ligne.ProduitBoutiqueId,
                    vente.BoutiqueId,
                    ligne.Quantite,
                    ligne.PrixUnitaire,
                    TypeMouvementStock.Retour,
                    vente.Id,
                    user.Id,
                    $"Annulation vente {vente.NumeroVente}: {raison}");
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return Json(new { success = false, message = $"Erreur: {ex.Message}" });
        }
    }

    private async Task<string> GenererNumeroVenteAsync(Guid boutiqueId)
    {
        var boutique = await _context.Boutiques.FindAsync(boutiqueId);
        var prefixe = boutique?.Nom.Substring(0, Math.Min(3, boutique.Nom.Length)).ToUpper() ?? "VEN";
        var annee = DateTime.UtcNow.Year;
        var mois = DateTime.UtcNow.Month.ToString("00");

        var dernierNumero = await _context.Ventes
            .Where(v => v.BoutiqueId == boutiqueId && v.NumeroVente.StartsWith($"{prefixe}{annee}{mois}"))
            .OrderByDescending(v => v.NumeroVente)
            .Select(v => v.NumeroVente)
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

    private async Task CreerMouvementStockAsync(
        Guid produitBoutiqueId,
        Guid boutiqueId,
        int quantite,
        decimal prixUnitaire,
        TypeMouvementStock type,
        Guid? venteId,
        Guid utilisateurId,
        string? commentaire = null)
    {
        // Trouver le dépôt principal de la boutique
        var depot = await _context.Depots
            .FirstOrDefaultAsync(d => d.BoutiqueId == boutiqueId && d.EstActif);

        if (depot == null) return;

        // Créer le mouvement
        var mouvement = new MouvementStock
        {
            ProduitBoutiqueId = produitBoutiqueId,
            DepotId = depot.Id,
            TypeMouvement = type,
            Quantite = quantite,
            PrixUnitaire = prixUnitaire,
            VenteId = venteId,
            UtilisateurId = utilisateurId,
            Commentaire = commentaire,
            DateMouvement = DateTime.UtcNow
        };

        _context.MouvementsStock.Add(mouvement);

        // Mettre à jour le stock
        var stock = await _context.Stocks
            .FirstOrDefaultAsync(s => s.ProduitBoutiqueId == produitBoutiqueId && s.DepotId == depot.Id);

        if (stock == null)
        {
            stock = new Stock
            {
                ProduitBoutiqueId = produitBoutiqueId,
                DepotId = depot.Id,
                Quantite = 0
            };
            _context.Stocks.Add(stock);
        }

        stock.Quantite += quantite;
        stock.DateDerniereMaj = DateTime.UtcNow;
    }

    private async Task<decimal> GetTvaParDefautAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return 0;

        var boutique = await _boutiqueActiveService.GetBoutiqueActiveAsync(user.Id);
        return boutique?.TvaParDefaut ?? 0;
    }

    private void RecalculerLigne(LignePanier ligne, ProduitBoutique produitBoutique)
    {
        ligne.PrixUnitaire = produitBoutique.PrixVente;
        ligne.MontantHT = ligne.PrixUnitaire * ligne.Quantite * (1 - ligne.Remise / 100);
        ligne.MontantTVA = ligne.MontantHT * ligne.TauxTVA / 100;
        ligne.MontantTTC = ligne.MontantHT + ligne.MontantTVA;
    }

    private decimal CalculerTotalPanier(List<LignePanier> panier)
    {
        return panier.Sum(l => l.MontantTTC);
    }
}
