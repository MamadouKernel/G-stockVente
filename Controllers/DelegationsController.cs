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

[Authorize(Roles = "AdminReseau")]
public class DelegationsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IDelegationService _delegationService;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;

    public DelegationsController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IDelegationService delegationService,
        RoleManager<IdentityRole<Guid>> roleManager)
    {
        _context = context;
        _userManager = userManager;
        _delegationService = delegationService;
        _roleManager = roleManager;
    }

    // GET: Delegations
    public async Task<IActionResult> Index(string? searchTerm, Guid? delegantId, Guid? beneficiaireId, bool? estActive)
    {
        IQueryable<Delegation> query = _context.Delegations
            .Include(d => d.UtilisateurDelegant)
            .Include(d => d.UtilisateurBeneficiaire)
            .Where(d => !d.EstSupprime);

        // Filtre par recherche
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(d =>
                d.UtilisateurDelegant!.Nom.Contains(searchTerm) ||
                d.UtilisateurDelegant!.Prenom.Contains(searchTerm) ||
                d.UtilisateurDelegant!.Email.Contains(searchTerm) ||
                d.UtilisateurBeneficiaire!.Nom.Contains(searchTerm) ||
                d.UtilisateurBeneficiaire!.Prenom.Contains(searchTerm) ||
                d.UtilisateurBeneficiaire!.Email.Contains(searchTerm) ||
                d.RoleDelege.Contains(searchTerm));
        }

        // Filtre par délégant
        if (delegantId.HasValue)
        {
            query = query.Where(d => d.UtilisateurDelegantId == delegantId.Value);
        }

        // Filtre par bénéficiaire
        if (beneficiaireId.HasValue)
        {
            query = query.Where(d => d.UtilisateurBeneficiaireId == beneficiaireId.Value);
        }

        // Filtre par statut actif
        if (estActive.HasValue)
        {
            query = query.Where(d => d.EstActive == estActive.Value);
        }

        var delegations = await query.OrderByDescending(d => d.DateCreation).ToListAsync();

        // Charger les utilisateurs et rôles pour les filtres
        ViewBag.Utilisateurs = await _userManager.Users
            .Where(u => !u.EstSupprime)
            .OrderBy(u => u.Nom)
            .ThenBy(u => u.Prenom)
            .Select(u => new SelectListItem
            {
                Value = u.Id.ToString(),
                Text = $"{u.Prenom} {u.Nom} ({u.Email})"
            })
            .ToListAsync();

        ViewBag.Roles = await _roleManager.Roles
            .OrderBy(r => r.Name)
            .Select(r => new SelectListItem
            {
                Value = r.Name!,
                Text = r.Name!
            })
            .ToListAsync();

        ViewBag.SearchTerm = searchTerm;
        ViewBag.DelegantId = delegantId;
        ViewBag.BeneficiaireId = beneficiaireId;
        ViewBag.EstActive = estActive;

        return View(delegations);
    }

    // GET: Delegations/Details/5
    public async Task<IActionResult> Details(Guid? id)
    {
        if (id == null)
            return NotFound();

        var delegation = await _delegationService.GetDelegationByIdAsync(id.Value);
        if (delegation == null)
            return NotFound();

        return View(delegation);
    }

    // GET: Delegations/Create
    public async Task<IActionResult> Create()
    {
        await LoadViewBagData();
        return View();
    }

    // POST: Delegations/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("UtilisateurDelegantId,UtilisateurBeneficiaireId,RoleDelege,DateDebut,DateFin,Raison")] CreateDelegationViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await LoadViewBagData();
            return View(model);
        }

        // Vérifier que les dates sont valides
        if (model.DateDebut >= model.DateFin)
        {
            ModelState.AddModelError("DateFin", "La date de fin doit être postérieure à la date de début.");
            await LoadViewBagData();
            return View(model);
        }

        // Vérifier que la date de début n'est pas dans le passé
        if (model.DateDebut < DateTime.UtcNow.Date)
        {
            ModelState.AddModelError("DateDebut", "La date de début ne peut pas être dans le passé.");
            await LoadViewBagData();
            return View(model);
        }

        // Vérifier que les utilisateurs existent
        var delegant = await _userManager.FindByIdAsync(model.UtilisateurDelegantId.ToString());
        var beneficiaire = await _userManager.FindByIdAsync(model.UtilisateurBeneficiaireId.ToString());

        if (delegant == null || delegant.EstSupprime)
        {
            ModelState.AddModelError("UtilisateurDelegantId", "L'utilisateur délégant est introuvable.");
            await LoadViewBagData();
            return View(model);
        }

        if (beneficiaire == null || beneficiaire.EstSupprime)
        {
            ModelState.AddModelError("UtilisateurBeneficiaireId", "L'utilisateur bénéficiaire est introuvable.");
            await LoadViewBagData();
            return View(model);
        }

        // Vérifier que le délégant possède le rôle à déléguer
        if (!await _userManager.IsInRoleAsync(delegant, model.RoleDelege))
        {
            ModelState.AddModelError("RoleDelege", $"L'utilisateur délégant ne possède pas le rôle '{model.RoleDelege}'.");
            await LoadViewBagData();
            return View(model);
        }

        // Vérifier que le bénéficiaire ne possède pas déjà ce rôle
        if (await _userManager.IsInRoleAsync(beneficiaire, model.RoleDelege))
        {
            ModelState.AddModelError("RoleDelege", $"L'utilisateur bénéficiaire possède déjà le rôle '{model.RoleDelege}'.");
            await LoadViewBagData();
            return View(model);
        }

        // Vérifier qu'il n'y a pas de délégation active en conflit
        var conflits = await _delegationService.GetActiveDelegationsForUserAsync(model.UtilisateurBeneficiaireId);
        if (conflits.Any(d => d.RoleDelege == model.RoleDelege && 
            ((d.DateDebut <= model.DateDebut && d.DateFin >= model.DateDebut) ||
             (d.DateDebut <= model.DateFin && d.DateFin >= model.DateFin) ||
             (d.DateDebut >= model.DateDebut && d.DateFin <= model.DateFin))))
        {
            ModelState.AddModelError("", "Une délégation active existe déjà pour ce rôle sur cette période.");
            await LoadViewBagData();
            return View(model);
        }

        var delegation = new Delegation
        {
            UtilisateurDelegantId = model.UtilisateurDelegantId,
            UtilisateurBeneficiaireId = model.UtilisateurBeneficiaireId,
            RoleDelege = model.RoleDelege,
            DateDebut = model.DateDebut.ToUniversalTime(),
            DateFin = model.DateFin.ToUniversalTime(),
            Raison = model.Raison
        };

        await _delegationService.CreateDelegationAsync(delegation);

        TempData["SuccessMessage"] = "Délégation créée avec succès.";
        return RedirectToAction(nameof(Index));
    }

    // GET: Delegations/Edit/5
    public async Task<IActionResult> Edit(Guid? id)
    {
        if (id == null)
            return NotFound();

        var delegation = await _delegationService.GetDelegationByIdAsync(id.Value);
        if (delegation == null)
            return NotFound();

        await LoadViewBagData();
        var model = new EditDelegationViewModel
        {
            Id = delegation.Id,
            UtilisateurDelegantId = delegation.UtilisateurDelegantId,
            UtilisateurBeneficiaireId = delegation.UtilisateurBeneficiaireId,
            RoleDelege = delegation.RoleDelege,
            DateDebut = delegation.DateDebut,
            DateFin = delegation.DateFin,
            Raison = delegation.Raison,
            EstActive = delegation.EstActive
        };

        return View(model);
    }

    // POST: Delegations/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, [Bind("Id,UtilisateurDelegantId,UtilisateurBeneficiaireId,RoleDelege,DateDebut,DateFin,Raison,EstActive")] EditDelegationViewModel model)
    {
        if (id != model.Id)
            return NotFound();

        if (!ModelState.IsValid)
        {
            await LoadViewBagData();
            return View(model);
        }

        // Vérifier que les dates sont valides
        if (model.DateDebut >= model.DateFin)
        {
            ModelState.AddModelError("DateFin", "La date de fin doit être postérieure à la date de début.");
            await LoadViewBagData();
            return View(model);
        }

        var delegation = await _delegationService.GetDelegationByIdAsync(id);
        if (delegation == null)
            return NotFound();

        // Vérifier les conflits (exclure la délégation courante)
        var conflits = await _delegationService.GetActiveDelegationsForUserAsync(model.UtilisateurBeneficiaireId);
        if (conflits.Any(d => d.Id != id && d.RoleDelege == model.RoleDelege && 
            ((d.DateDebut <= model.DateDebut && d.DateFin >= model.DateDebut) ||
             (d.DateDebut <= model.DateFin && d.DateFin >= model.DateFin) ||
             (d.DateDebut >= model.DateDebut && d.DateFin <= model.DateFin))))
        {
            ModelState.AddModelError("", "Une délégation active existe déjà pour ce rôle sur cette période.");
            await LoadViewBagData();
            return View(model);
        }

        delegation.UtilisateurDelegantId = model.UtilisateurDelegantId;
        delegation.UtilisateurBeneficiaireId = model.UtilisateurBeneficiaireId;
        delegation.RoleDelege = model.RoleDelege;
        delegation.DateDebut = model.DateDebut.ToUniversalTime();
        delegation.DateFin = model.DateFin.ToUniversalTime();
        delegation.Raison = model.Raison;
        delegation.EstActive = model.EstActive;

        await _delegationService.UpdateDelegationAsync(delegation);

        TempData["SuccessMessage"] = "Délégation modifiée avec succès.";
        return RedirectToAction(nameof(Index));
    }

    // POST: Delegations/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _delegationService.DeleteDelegationAsync(id);
        if (result)
        {
            TempData["SuccessMessage"] = "Délégation supprimée avec succès.";
        }
        else
        {
            TempData["ErrorMessage"] = "Impossible de supprimer la délégation.";
        }

        return RedirectToAction(nameof(Index));
    }

    // POST: Delegations/Activer/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Activer(Guid id)
    {
        var result = await _delegationService.ActiverDelegationAsync(id);
        if (result)
        {
            TempData["SuccessMessage"] = "Délégation activée avec succès.";
        }
        else
        {
            TempData["ErrorMessage"] = "Impossible d'activer la délégation.";
        }

        return RedirectToAction(nameof(Index));
    }

    // POST: Delegations/Desactiver/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Desactiver(Guid id)
    {
        var result = await _delegationService.DesactiverDelegationAsync(id);
        if (result)
        {
            TempData["SuccessMessage"] = "Délégation désactivée avec succès.";
        }
        else
        {
            TempData["ErrorMessage"] = "Impossible de désactiver la délégation.";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task LoadViewBagData()
    {
        ViewBag.Utilisateurs = await _userManager.Users
            .Where(u => !u.EstSupprime)
            .OrderBy(u => u.Nom)
            .ThenBy(u => u.Prenom)
            .Select(u => new SelectListItem
            {
                Value = u.Id.ToString(),
                Text = $"{u.Prenom} {u.Nom} ({u.Email})"
            })
            .ToListAsync();

        ViewBag.Roles = await _roleManager.Roles
            .OrderBy(r => r.Name)
            .Select(r => new SelectListItem
            {
                Value = r.Name!,
                Text = r.Name!
            })
            .ToListAsync();
    }
}

// ViewModels
public class CreateDelegationViewModel
{
    [Required(ErrorMessage = "L'utilisateur délégant est obligatoire")]
    [Display(Name = "Utilisateur délégant")]
    public Guid UtilisateurDelegantId { get; set; }

    [Required(ErrorMessage = "L'utilisateur bénéficiaire est obligatoire")]
    [Display(Name = "Utilisateur bénéficiaire")]
    public Guid UtilisateurBeneficiaireId { get; set; }

    [Required(ErrorMessage = "Le rôle délégué est obligatoire")]
    [Display(Name = "Rôle délégué")]
    public string RoleDelege { get; set; } = string.Empty;

    [Required(ErrorMessage = "La date de début est obligatoire")]
    [Display(Name = "Date de début")]
    [DataType(DataType.Date)]
    public DateTime DateDebut { get; set; } = DateTime.UtcNow.Date;

    [Required(ErrorMessage = "La date de fin est obligatoire")]
    [Display(Name = "Date de fin")]
    [DataType(DataType.Date)]
    public DateTime DateFin { get; set; } = DateTime.UtcNow.Date.AddDays(7);

    [StringLength(500)]
    [Display(Name = "Raison (optionnel)")]
    public string? Raison { get; set; }
}

public class EditDelegationViewModel
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "L'utilisateur délégant est obligatoire")]
    [Display(Name = "Utilisateur délégant")]
    public Guid UtilisateurDelegantId { get; set; }

    [Required(ErrorMessage = "L'utilisateur bénéficiaire est obligatoire")]
    [Display(Name = "Utilisateur bénéficiaire")]
    public Guid UtilisateurBeneficiaireId { get; set; }

    [Required(ErrorMessage = "Le rôle délégué est obligatoire")]
    [Display(Name = "Rôle délégué")]
    public string RoleDelege { get; set; } = string.Empty;

    [Required(ErrorMessage = "La date de début est obligatoire")]
    [Display(Name = "Date de début")]
    [DataType(DataType.DateTime)]
    public DateTime DateDebut { get; set; }

    [Required(ErrorMessage = "La date de fin est obligatoire")]
    [Display(Name = "Date de fin")]
    [DataType(DataType.DateTime)]
    public DateTime DateFin { get; set; }

    [StringLength(500)]
    [Display(Name = "Raison (optionnel)")]
    public string? Raison { get; set; }

    [Display(Name = "Active")]
    public bool EstActive { get; set; }
}

