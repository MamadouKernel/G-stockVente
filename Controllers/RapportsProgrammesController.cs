using G_StockVente.Domain.Models;
using G_StockVente.Infrastructure.Data;
using G_StockVente.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace G_StockVente.Controllers;

[Authorize(Roles = "AdminReseau,ManagerBoutique")]
public class RapportsProgrammesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IRapportProgrammeService _rapportService;
    private readonly IBoutiqueActiveService _boutiqueActiveService;

    public RapportsProgrammesController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IRapportProgrammeService rapportService,
        IBoutiqueActiveService boutiqueActiveService)
    {
        _context = context;
        _userManager = userManager;
        _rapportService = rapportService;
        _boutiqueActiveService = boutiqueActiveService;
    }

    // GET: RapportsProgrammes
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var isAdminReseau = await _userManager.IsInRoleAsync(user, "AdminReseau");
        Guid? boutiqueId = null;

        if (!isAdminReseau)
        {
            boutiqueId = await _boutiqueActiveService.GetBoutiqueActiveIdAsync(user.Id);
        }

        var rapports = await _rapportService.GetAllAsync(boutiqueId);
        return View(rapports);
    }

    // GET: RapportsProgrammes/Create
    public async Task<IActionResult> Create()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var isAdminReseau = await _userManager.IsInRoleAsync(user, "AdminReseau");
        ViewBag.Boutiques = await GetBoutiquesSelectList(isAdminReseau, user);
        ViewBag.TypesRapport = GetTypesRapportSelectList();
        ViewBag.Frequences = GetFrequencesSelectList();

        return View();
    }

    // POST: RapportsProgrammes/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RapportProgramme rapport)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var isAdminReseau = await _userManager.IsInRoleAsync(user, "AdminReseau");

        // Validation : si pas Admin, doit spécifier une boutique
        if (!isAdminReseau && !rapport.BoutiqueId.HasValue)
        {
            var boutiqueId = await _boutiqueActiveService.GetBoutiqueActiveIdAsync(user.Id);
            rapport.BoutiqueId = boutiqueId;
        }

        if (ModelState.IsValid)
        {
            rapport.UtilisateurCreateurId = user.Id;
            await _rapportService.CreateAsync(rapport);

            TempData["Success"] = "Rapport programmé créé avec succès.";
            return RedirectToAction(nameof(Index));
        }

        ViewBag.Boutiques = await GetBoutiquesSelectList(isAdminReseau, user);
        ViewBag.TypesRapport = GetTypesRapportSelectList();
        ViewBag.Frequences = GetFrequencesSelectList();
        return View(rapport);
    }

    // GET: RapportsProgrammes/Edit/5
    public async Task<IActionResult> Edit(Guid? id)
    {
        if (id == null) return NotFound();

        var rapport = await _rapportService.GetByIdAsync(id.Value);
        if (rapport == null) return NotFound();

        // Vérifier les droits
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var isAdminReseau = await _userManager.IsInRoleAsync(user, "AdminReseau");
        if (!isAdminReseau && rapport.UtilisateurCreateurId != user.Id)
        {
            return Forbid();
        }

        ViewBag.Boutiques = await GetBoutiquesSelectList(isAdminReseau, user);
        ViewBag.TypesRapport = GetTypesRapportSelectList();
        ViewBag.Frequences = GetFrequencesSelectList();

        return View(rapport);
    }

    // POST: RapportsProgrammes/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, RapportProgramme rapport)
    {
        if (id != rapport.Id) return NotFound();

        var existing = await _rapportService.GetByIdAsync(id);
        if (existing == null) return NotFound();

        // Vérifier les droits
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var isAdminReseau = await _userManager.IsInRoleAsync(user, "AdminReseau");
        if (!isAdminReseau && existing.UtilisateurCreateurId != user.Id)
        {
            return Forbid();
        }

        if (ModelState.IsValid)
        {
            var success = await _rapportService.UpdateAsync(rapport);
            if (success)
            {
                TempData["Success"] = "Rapport programmé mis à jour avec succès.";
                return RedirectToAction(nameof(Index));
            }
            TempData["Error"] = "Erreur lors de la mise à jour.";
        }

        ViewBag.Boutiques = await GetBoutiquesSelectList(isAdminReseau, user);
        ViewBag.TypesRapport = GetTypesRapportSelectList();
        ViewBag.Frequences = GetFrequencesSelectList();

        return View(rapport);
    }

    // POST: RapportsProgrammes/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var rapport = await _rapportService.GetByIdAsync(id);
        if (rapport == null)
        {
            TempData["Error"] = "Rapport programmé introuvable.";
            return RedirectToAction(nameof(Index));
        }

        // Vérifier les droits
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var isAdminReseau = await _userManager.IsInRoleAsync(user, "AdminReseau");
        if (!isAdminReseau && rapport.UtilisateurCreateurId != user.Id)
        {
            return Forbid();
        }

        var success = await _rapportService.DeleteAsync(id);
        if (success)
        {
            TempData["Success"] = "Rapport programmé supprimé avec succès.";
        }
        else
        {
            TempData["Error"] = "Erreur lors de la suppression.";
        }

        return RedirectToAction(nameof(Index));
    }

    // POST: RapportsProgrammes/ToggleActif/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActif(Guid id)
    {
        var rapport = await _rapportService.GetByIdAsync(id);
        if (rapport == null)
        {
            TempData["Error"] = "Rapport programmé introuvable.";
            return RedirectToAction(nameof(Index));
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var isAdminReseau = await _userManager.IsInRoleAsync(user, "AdminReseau");
        if (!isAdminReseau && rapport.UtilisateurCreateurId != user.Id)
        {
            return Forbid();
        }

        var newEtat = !rapport.EstActif;
        var success = await _rapportService.ActiverAsync(id, newEtat);
        if (success)
        {
            TempData["Success"] = $"Rapport programmé {(newEtat ? "activé" : "désactivé")} avec succès.";
        }
        else
        {
            TempData["Error"] = "Erreur lors de la modification.";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task<SelectList> GetBoutiquesSelectList(bool isAdminReseau, ApplicationUser user)
    {
        List<Boutique> boutiques;

        if (isAdminReseau)
        {
            boutiques = await _context.Boutiques
                .Where(b => b.EstActive)
                .OrderBy(b => b.Nom)
                .ToListAsync();
            
            // Ajouter l'option "Toutes les boutiques" (consolidé)
            boutiques.Insert(0, new Boutique { Id = Guid.Empty, Nom = "Toutes les boutiques (Consolidé)" });
        }
        else
        {
            var boutiqueId = await _boutiqueActiveService.GetBoutiqueActiveIdAsync(user.Id);
            boutiques = await _context.Boutiques
                .Where(b => b.Id == boutiqueId && b.EstActive)
                .OrderBy(b => b.Nom)
                .ToListAsync();
        }

        return new SelectList(boutiques, "Id", "Nom");
    }

    private SelectList GetTypesRapportSelectList()
    {
        var types = Enum.GetValues(typeof(TypeRapport))
            .Cast<TypeRapport>()
            .Select(t => new { Value = (int)t, Text = t.ToString() })
            .ToList();

        return new SelectList(types, "Value", "Text");
    }

    private SelectList GetFrequencesSelectList()
    {
        var frequences = Enum.GetValues(typeof(FrequenceRapport))
            .Cast<FrequenceRapport>()
            .Select(f => new { Value = (int)f, Text = f switch
            {
                FrequenceRapport.Journalier => "Journalier",
                FrequenceRapport.Hebdomadaire => "Hebdomadaire",
                FrequenceRapport.Mensuel => "Mensuel",
                _ => f.ToString()
            }})
            .ToList();

        return new SelectList(frequences, "Value", "Text");
    }
}

