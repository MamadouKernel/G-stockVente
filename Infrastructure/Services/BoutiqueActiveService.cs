using G_StockVente.Domain.Models;
using G_StockVente.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace G_StockVente.Infrastructure.Services;

/// <summary>
/// Service pour gérer la boutique active de l'utilisateur
/// </summary>
public interface IBoutiqueActiveService
{
    Task<Guid?> GetBoutiqueActiveIdAsync(Guid userId);
    Task SetBoutiqueActiveAsync(Guid userId, Guid boutiqueId);
    Task<Boutique?> GetBoutiqueActiveAsync(Guid userId);
}

public class BoutiqueActiveService : IBoutiqueActiveService
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserManager<ApplicationUser> _userManager;

    public BoutiqueActiveService(
        ApplicationDbContext context,
        IHttpContextAccessor httpContextAccessor,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _userManager = userManager;
    }

    public async Task<Guid?> GetBoutiqueActiveIdAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        return user?.BoutiqueActiveId;
    }

    public async Task SetBoutiqueActiveAsync(Guid userId, Guid boutiqueId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user != null)
        {
            user.BoutiqueActiveId = boutiqueId;
            await _userManager.UpdateAsync(user);
        }
    }

    public async Task<Boutique?> GetBoutiqueActiveAsync(Guid userId)
    {
        var boutiqueId = await GetBoutiqueActiveIdAsync(userId);
        if (boutiqueId == null)
            return null;

        return await _context.Boutiques
            .FirstOrDefaultAsync(b => b.Id == boutiqueId && b.EstActive);
    }
}

