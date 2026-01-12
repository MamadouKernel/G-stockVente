using G_StockVente.Domain.Models;
using G_StockVente.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace G_StockVente.Infrastructure.Services;

/// <summary>
/// Service de gestion des délégations temporaires de droits
/// </summary>
public interface IDelegationService
{
    Task<List<Delegation>> GetAllDelegationsAsync();
    Task<List<Delegation>> GetDelegationsByDelegantAsync(Guid delegantId);
    Task<List<Delegation>> GetDelegationsByBeneficiaireAsync(Guid beneficiaireId);
    Task<List<Delegation>> GetActiveDelegationsForUserAsync(Guid userId);
    Task<Delegation?> GetDelegationByIdAsync(Guid id);
    Task<Delegation> CreateDelegationAsync(Delegation delegation);
    Task<Delegation> UpdateDelegationAsync(Delegation delegation);
    Task<bool> DeleteDelegationAsync(Guid id);
    Task<bool> ActiverDelegationAsync(Guid id);
    Task<bool> DesactiverDelegationAsync(Guid id);
    Task<bool> VerifierDelegationActiveAsync(Guid beneficiaireId, string role);
}

public class DelegationService : IDelegationService
{
    private readonly ApplicationDbContext _context;

    public DelegationService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Delegation>> GetAllDelegationsAsync()
    {
        return await _context.Delegations
            .Where(d => !d.EstSupprime)
            .Include(d => d.UtilisateurDelegant)
            .Include(d => d.UtilisateurBeneficiaire)
            .OrderByDescending(d => d.DateCreation)
            .ToListAsync();
    }

    public async Task<List<Delegation>> GetDelegationsByDelegantAsync(Guid delegantId)
    {
        return await _context.Delegations
            .Where(d => !d.EstSupprime && d.UtilisateurDelegantId == delegantId)
            .Include(d => d.UtilisateurDelegant)
            .Include(d => d.UtilisateurBeneficiaire)
            .OrderByDescending(d => d.DateCreation)
            .ToListAsync();
    }

    public async Task<List<Delegation>> GetDelegationsByBeneficiaireAsync(Guid beneficiaireId)
    {
        return await _context.Delegations
            .Where(d => !d.EstSupprime && d.UtilisateurBeneficiaireId == beneficiaireId)
            .Include(d => d.UtilisateurDelegant)
            .Include(d => d.UtilisateurBeneficiaire)
            .OrderByDescending(d => d.DateCreation)
            .ToListAsync();
    }

    public async Task<List<Delegation>> GetActiveDelegationsForUserAsync(Guid userId)
    {
        var now = DateTime.UtcNow;
        return await _context.Delegations
            .Where(d => !d.EstSupprime 
                && d.EstActive 
                && d.UtilisateurBeneficiaireId == userId
                && d.DateDebut <= now 
                && d.DateFin >= now)
            .Include(d => d.UtilisateurDelegant)
            .Include(d => d.UtilisateurBeneficiaire)
            .OrderByDescending(d => d.DateCreation)
            .ToListAsync();
    }

    public async Task<Delegation?> GetDelegationByIdAsync(Guid id)
    {
        return await _context.Delegations
            .Include(d => d.UtilisateurDelegant)
            .Include(d => d.UtilisateurBeneficiaire)
            .FirstOrDefaultAsync(d => d.Id == id && !d.EstSupprime);
    }

    public async Task<Delegation> CreateDelegationAsync(Delegation delegation)
    {
        delegation.Id = Guid.NewGuid();
        delegation.DateCreation = DateTime.UtcNow;
        delegation.EstActive = true;
        delegation.EstSupprime = false;

        _context.Delegations.Add(delegation);
        await _context.SaveChangesAsync();

        return await GetDelegationByIdAsync(delegation.Id) ?? delegation;
    }

    public async Task<Delegation> UpdateDelegationAsync(Delegation delegation)
    {
        delegation.DateModification = DateTime.UtcNow;
        _context.Delegations.Update(delegation);
        await _context.SaveChangesAsync();

        return await GetDelegationByIdAsync(delegation.Id) ?? delegation;
    }

    public async Task<bool> DeleteDelegationAsync(Guid id)
    {
        var delegation = await _context.Delegations.FindAsync(id);
        if (delegation == null || delegation.EstSupprime)
            return false;

        delegation.EstSupprime = true;
        delegation.DateSuppression = DateTime.UtcNow;
        delegation.DateModification = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ActiverDelegationAsync(Guid id)
    {
        var delegation = await _context.Delegations.FindAsync(id);
        if (delegation == null || delegation.EstSupprime)
            return false;

        delegation.EstActive = true;
        delegation.DateModification = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DesactiverDelegationAsync(Guid id)
    {
        var delegation = await _context.Delegations.FindAsync(id);
        if (delegation == null || delegation.EstSupprime)
            return false;

        delegation.EstActive = false;
        delegation.DateModification = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> VerifierDelegationActiveAsync(Guid beneficiaireId, string role)
    {
        var now = DateTime.UtcNow;
        return await _context.Delegations
            .AnyAsync(d => !d.EstSupprime 
                && d.EstActive 
                && d.UtilisateurBeneficiaireId == beneficiaireId
                && d.RoleDelege == role
                && d.DateDebut <= now 
                && d.DateFin >= now);
    }
}

