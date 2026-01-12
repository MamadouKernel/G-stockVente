using G_StockVente.Domain.Models;
using G_StockVente.Infrastructure.Data;
using G_StockVente.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace G_StockVente.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly ApplicationDbContext _context;
    private readonly IBoutiqueActiveService _boutiqueActiveService;
    private readonly IJournalConnexionService _journalConnexionService;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        RoleManager<IdentityRole<Guid>> roleManager,
        ApplicationDbContext context,
        IBoutiqueActiveService boutiqueActiveService,
        IJournalConnexionService journalConnexionService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _context = context;
        _boutiqueActiveService = boutiqueActiveService;
        _journalConnexionService = journalConnexionService;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Vérifier le nombre de tentatives échouées (dans les 30 dernières minutes)
        var tentativesEchouees = await _journalConnexionService.GetNombreTentativesEchoueesAsync(
            model.Email, 
            TimeSpan.FromMinutes(30));

        // Verrouillage temporaire après 5 tentatives échouées
        if (tentativesEchouees >= 5)
        {
            var adresseIp = HttpContext.Connection.RemoteIpAddress?.ToString();
            await _journalConnexionService.EnregistrerConnexionAsync(
                model.Email, 
                adresseIp, 
                false, 
                "Compte temporairement verrouillé (trop de tentatives)");
            
            ModelState.AddModelError(string.Empty, 
                "Trop de tentatives de connexion échouées. Veuillez réessayer dans 30 minutes.");
            return View(model);
        }

        var adresseIP = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await _signInManager.PasswordSignInAsync(
            model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

        if (result.Succeeded)
        {
            // Enregistrer la connexion réussie
            await _journalConnexionService.EnregistrerConnexionAsync(
                model.Email, 
                adresseIP, 
                true);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null)
            {
                // Vérifier si l'utilisateur est supprimé (soft delete)
                if (user.EstSupprime)
                {
                    await _signInManager.SignOutAsync();
                    await _journalConnexionService.EnregistrerConnexionAsync(
                        model.Email, 
                        adresseIP, 
                        false, 
                        "Compte supprimé");
                    
                    ModelState.AddModelError(string.Empty, "Ce compte utilisateur a été supprimé.");
                    return View(model);
                }

                user.DateDerniereConnexion = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);

                // Vérifier si l'utilisateur doit changer son mot de passe
                if (user.MustChangePassword)
                {
                    // Déconnecter temporairement pour forcer le changement
                    await _signInManager.SignOutAsync();
                    TempData["MustChangePassword"] = true;
                    TempData["UserId"] = user.Id.ToString();
                    return RedirectToAction("ChangePasswordFirstTime");
                }

                // Si l'utilisateur n'a pas de boutique active, rediriger vers la sélection
                if (user.BoutiqueActiveId == null && !await _userManager.IsInRoleAsync(user, "AdminReseau"))
                {
                    return RedirectToAction("SelectionBoutique", "Boutiques");
                }
            }

            return RedirectToLocal(returnUrl);
        }

        // Enregistrer la connexion échouée
        string raisonEchec = result.IsLockedOut ? "Compte verrouillé" :
                            result.IsNotAllowed ? "Compte non autorisé" :
                            result.RequiresTwoFactor ? "Authentification à deux facteurs requise" :
                            "Email ou mot de passe incorrect";

        await _journalConnexionService.EnregistrerConnexionAsync(
            model.Email, 
            adresseIP, 
            false, 
            raisonEchec);

        ModelState.AddModelError(string.Empty, "Tentative de connexion non valide.");
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction(nameof(HomeController.Index), "Home");
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult AccessDenied()
    {
        return View();
    }

    // GET: Account/ChangePasswordFirstTime
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> ChangePasswordFirstTime()
    {
        if (!TempData.ContainsKey("MustChangePassword") || !TempData.ContainsKey("UserId"))
        {
            return RedirectToAction("Login");
        }

        var userIdStr = TempData["UserId"]?.ToString();
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out _))
        {
            return RedirectToAction("Login");
        }

        var currentUser = await _userManager.FindByIdAsync(userIdStr);
        if (currentUser == null)
        {
            return RedirectToAction("Login");
        }

        ViewBag.Email = currentUser.Email;
        TempData.Keep("UserId");
        return View();
    }

    // POST: Account/ChangePasswordFirstTime
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePasswordFirstTime(ChangePasswordFirstTimeViewModel model)
    {
        if (!ModelState.IsValid)
        {
            if (TempData.ContainsKey("UserId"))
            {
                TempData.Keep("UserId");
                var userIdTempStr = TempData["UserId"]?.ToString();
                if (!string.IsNullOrEmpty(userIdTempStr))
                {
                    var currentUser = await _userManager.FindByIdAsync(userIdTempStr);
                    if (currentUser != null)
                    {
                        ViewBag.Email = currentUser.Email;
                    }
                }
            }
            return View(model);
        }

        var userIdFromTempStr = TempData["UserId"]?.ToString();
        if (string.IsNullOrEmpty(userIdFromTempStr))
        {
            ModelState.AddModelError("", "Session expirée. Veuillez vous reconnecter.");
            return RedirectToAction("Login");
        }

        var user = await _userManager.FindByIdAsync(userIdFromTempStr);
        if (user == null)
        {
            ModelState.AddModelError("", "Utilisateur introuvable.");
            return RedirectToAction("Login");
        }

        // Vérifier que le mot de passe actuel est correct
        var passwordCheck = await _signInManager.CheckPasswordSignInAsync(user, model.CurrentPassword, lockoutOnFailure: false);
        if (!passwordCheck.Succeeded)
        {
            ModelState.AddModelError("CurrentPassword", "Le mot de passe actuel est incorrect.");
            ViewBag.Email = user.Email;
            TempData.Keep("UserId");
            return View(model);
        }

        // Changer le mot de passe
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);

        if (result.Succeeded)
        {
            // Marquer que le mot de passe a été changé
            user.MustChangePassword = false;
            await _userManager.UpdateAsync(user);

            // Connecter l'utilisateur avec le nouveau mot de passe
            await _signInManager.SignInAsync(user, isPersistent: false);

            TempData["Success"] = "Votre mot de passe a été modifié avec succès. Vous êtes maintenant connecté.";

            // Si l'utilisateur n'a pas de boutique active, rediriger vers la sélection
            if (user.BoutiqueActiveId == null && !await _userManager.IsInRoleAsync(user, "AdminReseau"))
            {
                return RedirectToAction("SelectionBoutique", "Boutiques");
            }

            return RedirectToAction("Index", "Home");
        }
        else
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
            ViewBag.Email = user.Email;
            TempData.Keep("UserId");
            return View(model);
        }
    }

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }
        return RedirectToAction(nameof(HomeController.Index), "Home");
    }
}

public class LoginViewModel
{
    [Required(ErrorMessage = "L'email est obligatoire")]
    [EmailAddress(ErrorMessage = "Format d'email invalide")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le mot de passe est obligatoire")]
    [DataType(DataType.Password)]
    [Display(Name = "Mot de passe")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Se souvenir de moi")]
    public bool RememberMe { get; set; }
}

public class ChangePasswordFirstTimeViewModel
{
    [Required(ErrorMessage = "Le mot de passe actuel est obligatoire")]
    [DataType(DataType.Password)]
    [Display(Name = "Mot de passe actuel")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le nouveau mot de passe est obligatoire")]
    [StringLength(100, ErrorMessage = "Le mot de passe doit contenir au moins {2} caractères.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "Nouveau mot de passe")]
    public string NewPassword { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Confirmer le nouveau mot de passe")]
    [Compare("NewPassword", ErrorMessage = "Les mots de passe ne correspondent pas.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

