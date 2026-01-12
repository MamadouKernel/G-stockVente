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

[Authorize(Roles = "AdminReseau,ManagerBoutique")]
public class UtilisateursController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly IBoutiqueActiveService _boutiqueActiveService;

    public UtilisateursController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<Guid>> roleManager,
        IBoutiqueActiveService boutiqueActiveService)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
        _boutiqueActiveService = boutiqueActiveService;
    }

    // GET: Utilisateurs
    public async Task<IActionResult> Index(string? searchTerm, string? roleFilter, bool? actifFilter, bool? inclureSupprimes)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return NotFound();

        var isAdminReseau = await _userManager.IsInRoleAsync(currentUser, "AdminReseau");
        var boutiqueId = await _boutiqueActiveService.GetBoutiqueActiveIdAsync(currentUser.Id);

        IQueryable<ApplicationUser> query = _userManager.Users;

        // Si ManagerBoutique, filtrer uniquement les utilisateurs de sa boutique
        if (!isAdminReseau)
        {
            if (boutiqueId == null)
            {
                // ManagerBoutique sans boutique active → rediriger
                return RedirectToAction("SelectionBoutique", "Boutiques");
            }
            query = query.Where(u => u.BoutiqueActiveId == boutiqueId);
        }

        // Par défaut, exclure les utilisateurs supprimés (soft delete)
        if (inclureSupprimes != true)
        {
            query = query.Where(u => !u.EstSupprime);
        }

        // Filtre par recherche
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(u =>
                u.Nom.Contains(searchTerm) ||
                u.Prenom.Contains(searchTerm) ||
                u.Email.Contains(searchTerm) ||
                u.UserName.Contains(searchTerm));
        }

        // Filtre par rôle
        if (!string.IsNullOrWhiteSpace(roleFilter))
        {
            var usersInRole = await _userManager.GetUsersInRoleAsync(roleFilter);
            var userIds = usersInRole.Select(u => u.Id);
            query = query.Where(u => userIds.Contains(u.Id));
        }

        // Filtre par statut actif
        if (actifFilter.HasValue)
        {
            query = query.Where(u => u.EstActif == actifFilter.Value);
        }

        var utilisateurs = await query
            .Include(u => u.BoutiqueActive)
            .OrderBy(u => u.Nom)
            .ThenBy(u => u.Prenom)
            .ToListAsync();

        // Charger les rôles pour chaque utilisateur
        var utilisateursAvecRoles = new List<UtilisateurViewModel>();
        foreach (var user in utilisateurs)
        {
            var roles = await _userManager.GetRolesAsync(user);
            utilisateursAvecRoles.Add(new UtilisateurViewModel
            {
                Id = user.Id,
                Prenom = user.Prenom,
                Nom = user.Nom,
                Email = user.Email,
                UserName = user.UserName,
                EstActif = user.EstActif,
                EstSupprime = user.EstSupprime,
                DateSuppression = user.DateSuppression,
                BoutiqueActiveId = user.BoutiqueActiveId,
                BoutiqueActiveNom = user.BoutiqueActive?.Nom,
                Roles = roles.ToList(),
                DateCreation = user.DateCreation,
                DateDerniereConnexion = user.DateDerniereConnexion,
                MustChangePassword = user.MustChangePassword
            });
        }

        ViewBag.SearchTerm = searchTerm;
        ViewBag.RoleFilter = roleFilter;
        ViewBag.ActifFilter = actifFilter;
        ViewBag.InclureSupprimes = inclureSupprimes;
        ViewBag.IsAdminReseau = isAdminReseau;
        ViewBag.Roles = await _roleManager.Roles.OrderBy(r => r.Name).ToListAsync();

        return View(utilisateursAvecRoles);
    }

    // GET: Utilisateurs/Details/5
    public async Task<IActionResult> Details(Guid? id)
    {
        if (id == null) return NotFound();

        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return NotFound();

        var isAdminReseau = await _userManager.IsInRoleAsync(currentUser, "AdminReseau");
        var boutiqueId = await _boutiqueActiveService.GetBoutiqueActiveIdAsync(currentUser.Id);

        var user = await _userManager.Users
            .Include(u => u.BoutiqueActive)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null || user.EstSupprime) return NotFound();

        // Vérifier les droits d'accès : ManagerBoutique ne peut voir que les utilisateurs de sa boutique
        if (!isAdminReseau && boutiqueId != null && user.BoutiqueActiveId != boutiqueId)
        {
            return Forbid();
        }

        var roles = await _userManager.GetRolesAsync(user);
        var allRoles = await _roleManager.Roles.OrderBy(r => r.Name).ToListAsync();

        var viewModel = new UtilisateurDetailsViewModel
        {
            Id = user.Id,
            Prenom = user.Prenom,
            Nom = user.Nom,
            Email = user.Email,
            UserName = user.UserName,
            EstActif = user.EstActif,
            BoutiqueActiveId = user.BoutiqueActiveId,
            BoutiqueActiveNom = user.BoutiqueActive?.Nom,
            Roles = roles.ToList(),
            AllRoles = allRoles.Select(r => r.Name).ToList(),
            DateCreation = user.DateCreation,
            DateDerniereConnexion = user.DateDerniereConnexion,
            MustChangePassword = user.MustChangePassword
        };

        return View(viewModel);
    }

    // GET: Utilisateurs/Create
    public async Task<IActionResult> Create()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return NotFound();

        var isAdminReseau = await _userManager.IsInRoleAsync(currentUser, "AdminReseau");
        var boutiqueId = await _boutiqueActiveService.GetBoutiqueActiveIdAsync(currentUser.Id);

        // Si ManagerBoutique, forcer sa boutique et limiter les rôles
        if (!isAdminReseau)
        {
            if (boutiqueId == null)
            {
                return RedirectToAction("SelectionBoutique", "Boutiques");
            }

            // Boutique forcée pour ManagerBoutique
            var boutique = await _context.Boutiques.FindAsync(boutiqueId);
            ViewData["BoutiqueId"] = new SelectList(
                new List<Boutique> { boutique! },
                "Id", "Nom", boutiqueId);

            // Rôles limités : uniquement Caissier et GestionnaireStock
            var rolesAutorises = await _roleManager.Roles
                .Where(r => r.Name == "Caissier" || r.Name == "GestionnaireStock")
                .OrderBy(r => r.Name)
                .ToListAsync();
            ViewData["Roles"] = rolesAutorises;
            ViewData["IsManagerBoutique"] = true;
        }
        else
        {
            // AdminReseau : toutes les boutiques et tous les rôles
            ViewData["BoutiqueId"] = new SelectList(
                await _context.Boutiques.Where(b => b.EstActive).OrderBy(b => b.Nom).ToListAsync(),
                "Id", "Nom");
            ViewData["Roles"] = await _roleManager.Roles.OrderBy(r => r.Name).ToListAsync();
            ViewData["IsManagerBoutique"] = false;
        }

        return View();
    }

    // POST: Utilisateurs/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateUtilisateurViewModel model)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return NotFound();

        var isAdminReseau = await _userManager.IsInRoleAsync(currentUser, "AdminReseau");
        var boutiqueId = await _boutiqueActiveService.GetBoutiqueActiveIdAsync(currentUser.Id);

        // Sécurité : ManagerBoutique ne peut créer que pour sa boutique
        if (!isAdminReseau && boutiqueId != null)
        {
            // Forcer la boutique active du ManagerBoutique
            model.BoutiqueId = boutiqueId;

            // Valider que les rôles sont autorisés (Caissier, GestionnaireStock uniquement)
            if (model.Roles != null && model.Roles.Any())
            {
                var rolesAutorises = new[] { "Caissier", "GestionnaireStock" };
                var rolesNonAutorises = model.Roles.Except(rolesAutorises).ToList();
                if (rolesNonAutorises.Any())
                {
                    ModelState.AddModelError("Roles", $"Vous ne pouvez assigner que les rôles : {string.Join(", ", rolesAutorises)}");
                }
            }
        }

        if (ModelState.IsValid)
        {
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                Prenom = model.Prenom,
                Nom = model.Nom,
                BoutiqueActiveId = model.BoutiqueId,
                EstActif = model.EstActif,
                MustChangePassword = model.MustChangePassword,
                EmailConfirmed = true,
                DateCreation = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Attribuer les rôles
                if (model.Roles != null && model.Roles.Any())
                {
                    var roleResult = await _userManager.AddToRolesAsync(user, model.Roles);
                    if (!roleResult.Succeeded)
                    {
                        // Logger les erreurs d'assignation de rôles
                        foreach (var error in roleResult.Errors)
                        {
                            ModelState.AddModelError("Roles", error.Description);
                        }
                    }
                }

                TempData["Success"] = $"Utilisateur '{user.Prenom} {user.Nom}' créé avec succès.";
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        // Recharger les données pour la vue en cas d'erreur
        if (!isAdminReseau && boutiqueId != null)
        {
            var boutique = await _context.Boutiques.FindAsync(boutiqueId);
            ViewData["BoutiqueId"] = new SelectList(
                new List<Boutique> { boutique! },
                "Id", "Nom", boutiqueId);
            var rolesAutorises = await _roleManager.Roles
                .Where(r => r.Name == "Caissier" || r.Name == "GestionnaireStock")
                .OrderBy(r => r.Name)
                .ToListAsync();
            ViewData["Roles"] = rolesAutorises;
            ViewData["IsManagerBoutique"] = true;
        }
        else
        {
            ViewData["BoutiqueId"] = new SelectList(
                await _context.Boutiques.Where(b => b.EstActive).OrderBy(b => b.Nom).ToListAsync(),
                "Id", "Nom", model.BoutiqueId);
            ViewData["Roles"] = await _roleManager.Roles.OrderBy(r => r.Name).ToListAsync();
            ViewData["IsManagerBoutique"] = false;
        }

        return View(model);
    }

    // GET: Utilisateurs/Edit/5
    public async Task<IActionResult> Edit(Guid? id)
    {
        if (id == null) return NotFound();

        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return NotFound();

        var isAdminReseau = await _userManager.IsInRoleAsync(currentUser, "AdminReseau");
        var boutiqueId = await _boutiqueActiveService.GetBoutiqueActiveIdAsync(currentUser.Id);

        var user = await _userManager.Users
            .Include(u => u.BoutiqueActive)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null || user.EstSupprime) return NotFound();

        // Vérifier les droits d'accès : ManagerBoutique ne peut modifier que les utilisateurs de sa boutique
        if (!isAdminReseau && boutiqueId != null && user.BoutiqueActiveId != boutiqueId)
        {
            return Forbid();
        }

        var userRoles = await _userManager.GetRolesAsync(user);

        var model = new EditUtilisateurViewModel
        {
            Id = user.Id,
            Prenom = user.Prenom,
            Nom = user.Nom,
            Email = user.Email,
            BoutiqueId = user.BoutiqueActiveId,
            EstActif = user.EstActif,
            MustChangePassword = user.MustChangePassword,
            SelectedRoles = userRoles.ToList()
        };

        // Si ManagerBoutique, forcer sa boutique et limiter les rôles
        if (!isAdminReseau && boutiqueId != null)
        {
            var boutique = await _context.Boutiques.FindAsync(boutiqueId);
            ViewData["BoutiqueId"] = new SelectList(
                new List<Boutique> { boutique! },
                "Id", "Nom", boutiqueId);

            // Rôles limités : uniquement Caissier et GestionnaireStock
            var rolesAutorises = await _roleManager.Roles
                .Where(r => r.Name == "Caissier" || r.Name == "GestionnaireStock")
                .OrderBy(r => r.Name)
                .ToListAsync();
            ViewData["Roles"] = rolesAutorises;
            ViewData["IsManagerBoutique"] = true;
        }
        else
        {
            ViewData["BoutiqueId"] = new SelectList(
                await _context.Boutiques.Where(b => b.EstActive).OrderBy(b => b.Nom).ToListAsync(),
                "Id", "Nom", model.BoutiqueId);
            ViewData["Roles"] = await _roleManager.Roles.OrderBy(r => r.Name).ToListAsync();
            ViewData["IsManagerBoutique"] = false;
        }

        return View(model);
    }

    // POST: Utilisateurs/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, EditUtilisateurViewModel model)
    {
        if (id != model.Id) return NotFound();

        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return NotFound();

        var isAdminReseau = await _userManager.IsInRoleAsync(currentUser, "AdminReseau");
        var boutiqueId = await _boutiqueActiveService.GetBoutiqueActiveIdAsync(currentUser.Id);

        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null || user.EstSupprime) return NotFound();

        // Vérifier les droits d'accès : ManagerBoutique ne peut modifier que les utilisateurs de sa boutique
        if (!isAdminReseau && boutiqueId != null && user.BoutiqueActiveId != boutiqueId)
        {
            return Forbid();
        }

        // Sécurité : ManagerBoutique ne peut modifier que pour sa boutique
        if (!isAdminReseau && boutiqueId != null)
        {
            // Forcer la boutique active du ManagerBoutique
            model.BoutiqueId = boutiqueId;

            // Valider que les rôles sont autorisés (Caissier, GestionnaireStock uniquement)
            if (model.SelectedRoles != null && model.SelectedRoles.Any())
            {
                var rolesAutorises = new[] { "Caissier", "GestionnaireStock" };
                var rolesNonAutorises = model.SelectedRoles.Except(rolesAutorises).ToList();
                if (rolesNonAutorises.Any())
                {
                    ModelState.AddModelError("SelectedRoles", $"Vous ne pouvez assigner que les rôles : {string.Join(", ", rolesAutorises)}");
                }
            }
        }

        if (ModelState.IsValid)
        {
            // Mettre à jour les propriétés
            user.Prenom = model.Prenom;
            user.Nom = model.Nom;
            user.Email = model.Email;
            user.UserName = model.Email;
            user.BoutiqueActiveId = model.BoutiqueId;
            user.EstActif = model.EstActif;
            user.MustChangePassword = model.MustChangePassword;

            var updateResult = await _userManager.UpdateAsync(user);

            if (updateResult.Succeeded)
            {
                // Gérer les rôles
                var currentRoles = await _userManager.GetRolesAsync(user);
                var rolesToRemove = currentRoles.Except(model.SelectedRoles ?? new List<string>());
                var rolesToAdd = (model.SelectedRoles ?? new List<string>()).Except(currentRoles);

                if (rolesToRemove.Any())
                {
                    await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                }

                if (rolesToAdd.Any())
                {
                    await _userManager.AddToRolesAsync(user, rolesToAdd);
                }

                TempData["Success"] = $"Utilisateur '{user.Prenom} {user.Nom}' modifié avec succès.";
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in updateResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        // Recharger les données pour la vue en cas d'erreur
        if (!isAdminReseau && boutiqueId != null)
        {
            var boutique = await _context.Boutiques.FindAsync(boutiqueId);
            ViewData["BoutiqueId"] = new SelectList(
                new List<Boutique> { boutique! },
                "Id", "Nom", boutiqueId);
            var rolesAutorises = await _roleManager.Roles
                .Where(r => r.Name == "Caissier" || r.Name == "GestionnaireStock")
                .OrderBy(r => r.Name)
                .ToListAsync();
            ViewData["Roles"] = rolesAutorises;
            ViewData["IsManagerBoutique"] = true;
        }
        else
        {
            ViewData["BoutiqueId"] = new SelectList(
                await _context.Boutiques.Where(b => b.EstActive).OrderBy(b => b.Nom).ToListAsync(),
                "Id", "Nom", model.BoutiqueId);
            ViewData["Roles"] = await _roleManager.Roles.OrderBy(r => r.Name).ToListAsync();
            ViewData["IsManagerBoutique"] = false;
        }

        return View(model);
    }

    // GET: Utilisateurs/Delete/5
    public async Task<IActionResult> Delete(Guid? id)
    {
        if (id == null) return NotFound();

        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return NotFound();

        var isAdminReseau = await _userManager.IsInRoleAsync(currentUser, "AdminReseau");
        var boutiqueId = await _boutiqueActiveService.GetBoutiqueActiveIdAsync(currentUser.Id);

        var user = await _userManager.Users
            .Include(u => u.BoutiqueActive)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null || user.EstSupprime) return NotFound();

        // Vérifier les droits d'accès : ManagerBoutique ne peut supprimer que les utilisateurs de sa boutique
        if (!isAdminReseau && boutiqueId != null && user.BoutiqueActiveId != boutiqueId)
        {
            return Forbid();
        }

        var roles = await _userManager.GetRolesAsync(user);

        var viewModel = new UtilisateurViewModel
        {
            Id = user.Id,
            Prenom = user.Prenom,
            Nom = user.Nom,
            Email = user.Email,
            UserName = user.UserName,
            EstActif = user.EstActif,
            BoutiqueActiveId = user.BoutiqueActiveId,
            BoutiqueActiveNom = user.BoutiqueActive?.Nom,
            Roles = roles.ToList(),
            DateCreation = user.DateCreation,
            DateDerniereConnexion = user.DateDerniereConnexion
        };

        return View(viewModel);
    }

    // POST: Utilisateurs/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return NotFound();

        var isAdminReseau = await _userManager.IsInRoleAsync(currentUser, "AdminReseau");
        var boutiqueId = await _boutiqueActiveService.GetBoutiqueActiveIdAsync(currentUser.Id);

        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null || user.EstSupprime) return NotFound();

        // Vérifier les droits d'accès : ManagerBoutique ne peut supprimer que les utilisateurs de sa boutique
        if (!isAdminReseau && boutiqueId != null && user.BoutiqueActiveId != boutiqueId)
        {
            return Forbid();
        }

        // Vérifier qu'on ne supprime pas le dernier admin (uniquement pour AdminReseau)
        if (isAdminReseau)
        {
            var isAdmin = await _userManager.IsInRoleAsync(user, "AdminReseau");
            if (isAdmin)
            {
                var adminsActifs = await _userManager.GetUsersInRoleAsync("AdminReseau");
                var adminCount = adminsActifs.Count(u => !u.EstSupprime);
                if (adminCount <= 1)
                {
                    TempData["Error"] = "Impossible de supprimer le dernier administrateur réseau.";
                    return RedirectToAction(nameof(Index));
                }
            }
        }

        // ManagerBoutique ne peut pas supprimer un autre ManagerBoutique ou AdminReseau
        if (!isAdminReseau)
        {
            var userRoles = await _userManager.GetRolesAsync(user);
            if (userRoles.Contains("AdminReseau") || userRoles.Contains("ManagerBoutique"))
            {
                TempData["Error"] = "Vous ne pouvez pas supprimer un administrateur réseau ou un autre manager de boutique.";
                return RedirectToAction(nameof(Index));
            }
        }

        // Soft delete : marquer comme supprimé au lieu de supprimer réellement
        user.EstSupprime = true;
        user.EstActif = false; // Désactiver aussi l'utilisateur
        user.DateSuppression = DateTime.UtcNow;
        
        var result = await _userManager.UpdateAsync(user);

        if (result.Succeeded)
        {
            TempData["Success"] = $"Utilisateur '{user.Prenom} {user.Nom}' supprimé avec succès (soft delete).";
        }
        else
        {
            TempData["Error"] = "Erreur lors de la suppression de l'utilisateur.";
        }

        return RedirectToAction(nameof(Index));
    }

    // POST: Utilisateurs/Restaurer/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Restaurer(Guid id)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return NotFound();

        var isAdminReseau = await _userManager.IsInRoleAsync(currentUser, "AdminReseau");
        var boutiqueId = await _boutiqueActiveService.GetBoutiqueActiveIdAsync(currentUser.Id);

        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null || !user.EstSupprime) return NotFound();

        // Vérifier les droits d'accès : ManagerBoutique ne peut restaurer que les utilisateurs de sa boutique
        if (!isAdminReseau && boutiqueId != null && user.BoutiqueActiveId != boutiqueId)
        {
            return Forbid();
        }

        // Restaurer l'utilisateur (soft delete)
        user.EstSupprime = false;
        user.DateSuppression = null;
        // Note: EstActif reste à false, l'administrateur devra le réactiver manuellement si nécessaire
        
        var result = await _userManager.UpdateAsync(user);

        if (result.Succeeded)
        {
            TempData["Success"] = $"Utilisateur '{user.Prenom} {user.Nom}' restauré avec succès.";
        }
        else
        {
            TempData["Error"] = "Erreur lors de la restauration de l'utilisateur.";
        }

        return RedirectToAction(nameof(Index), new { inclureSupprimes = true });
    }

    // POST: Utilisateurs/ResetPassword/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(Guid id, string newPassword)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null) return NotFound();

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

        if (result.Succeeded)
        {
            user.MustChangePassword = true;
            await _userManager.UpdateAsync(user);
            TempData["Success"] = "Mot de passe réinitialisé avec succès. L'utilisateur devra le changer à la prochaine connexion.";
        }
        else
        {
            TempData["Error"] = "Erreur lors de la réinitialisation du mot de passe.";
        }

        return RedirectToAction(nameof(Details), new { id });
    }
}

// ViewModels
public class UtilisateurViewModel
{
    public Guid Id { get; set; }
    public string Prenom { get; set; } = string.Empty;
    public string Nom { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public bool EstActif { get; set; }
    public bool EstSupprime { get; set; }
    public DateTime? DateSuppression { get; set; }
    public Guid? BoutiqueActiveId { get; set; }
    public string? BoutiqueActiveNom { get; set; }
    public List<string> Roles { get; set; } = new();
    public DateTime DateCreation { get; set; }
    public DateTime? DateDerniereConnexion { get; set; }
    public bool MustChangePassword { get; set; }
}

public class UtilisateurDetailsViewModel : UtilisateurViewModel
{
    public List<string> AllRoles { get; set; } = new();
}

public class CreateUtilisateurViewModel
{
    [Required(ErrorMessage = "Le prénom est obligatoire")]
    [StringLength(100)]
    [Display(Name = "Prénom")]
    public string Prenom { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le nom est obligatoire")]
    [StringLength(100)]
    [Display(Name = "Nom")]
    public string Nom { get; set; } = string.Empty;

    [Required(ErrorMessage = "L'email est obligatoire")]
    [EmailAddress(ErrorMessage = "Format d'email invalide")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le mot de passe est obligatoire")]
    [StringLength(100, ErrorMessage = "Le mot de passe doit contenir au moins {2} caractères.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "Mot de passe")]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Confirmer le mot de passe")]
    [Compare("Password", ErrorMessage = "Les mots de passe ne correspondent pas.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Display(Name = "Boutique active")]
    public Guid? BoutiqueId { get; set; }

    [Display(Name = "Actif")]
    public bool EstActif { get; set; } = true;

    [Display(Name = "Changer le mot de passe à la première connexion")]
    public bool MustChangePassword { get; set; } = true;

    [Display(Name = "Rôles")]
    public List<string>? Roles { get; set; }
}

public class EditUtilisateurViewModel
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Le prénom est obligatoire")]
    [StringLength(100)]
    [Display(Name = "Prénom")]
    public string Prenom { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le nom est obligatoire")]
    [StringLength(100)]
    [Display(Name = "Nom")]
    public string Nom { get; set; } = string.Empty;

    [Required(ErrorMessage = "L'email est obligatoire")]
    [EmailAddress(ErrorMessage = "Format d'email invalide")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Boutique active")]
    public Guid? BoutiqueId { get; set; }

    [Display(Name = "Actif")]
    public bool EstActif { get; set; } = true;

    [Display(Name = "Changer le mot de passe à la première connexion")]
    public bool MustChangePassword { get; set; }

    [Display(Name = "Rôles")]
    public List<string>? SelectedRoles { get; set; }
}

