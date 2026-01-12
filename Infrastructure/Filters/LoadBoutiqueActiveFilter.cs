using G_StockVente.Domain.Models;
using G_StockVente.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace G_StockVente.Infrastructure.Filters;

/// <summary>
/// Filtre qui charge automatiquement la boutique active dans ViewBag pour toutes les actions
/// </summary>
public class LoadBoutiqueActiveFilter : IAsyncActionFilter
{
    private readonly IBoutiqueActiveService _boutiqueActiveService;
    private readonly UserManager<ApplicationUser> _userManager;

    public LoadBoutiqueActiveFilter(
        IBoutiqueActiveService boutiqueActiveService,
        UserManager<ApplicationUser> userManager)
    {
        _boutiqueActiveService = boutiqueActiveService;
        _userManager = userManager;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (context.HttpContext.User.Identity?.IsAuthenticated == true)
        {
            var user = await _userManager.GetUserAsync(context.HttpContext.User);
            if (user != null)
            {
                var boutique = await _boutiqueActiveService.GetBoutiqueActiveAsync(user.Id);
                if (boutique != null)
                {
                    context.HttpContext.Items["BoutiqueActive"] = boutique;
                    if (context.Controller is Controller controller)
                    {
                        controller.ViewBag.Boutique = boutique;
                        controller.ViewBag.BoutiqueNom = boutique.Nom;
                    }
                }
            }
        }

        await next();
    }
}

