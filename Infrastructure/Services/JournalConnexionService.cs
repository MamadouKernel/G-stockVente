using G_StockVente.Domain.Models;
using G_StockVente.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace G_StockVente.Infrastructure.Services;

/// <summary>
/// Service pour gérer le journal des connexions
/// </summary>
public interface IJournalConnexionService
{
    Task EnregistrerConnexionAsync(string email, string? adresseIp, bool succes, string? raisonEchec = null);
    Task<List<JournalConnexion>> GetByUtilisateurAsync(Guid utilisateurId, int? limite = null);
    Task<List<JournalConnexion>> GetTentativesEchoueesAsync(string email, TimeSpan? periode = null);
    Task<int> GetNombreTentativesEchoueesAsync(string email, TimeSpan periode);
}

public class JournalConnexionService : IJournalConnexionService
{
    private readonly ApplicationDbContext _context;

    public JournalConnexionService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task EnregistrerConnexionAsync(string email, string? adresseIp, bool succes, string? raisonEchec = null)
    {
        var utilisateur = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email);

        var journal = new JournalConnexion
        {
            Id = Guid.NewGuid(),
            UtilisateurId = utilisateur?.Id,
            Email = email,
            AdresseIP = adresseIp,
            Succes = succes,
            RaisonEchec = raisonEchec,
            DateTentative = DateTime.UtcNow
        };

        _context.JournalConnexions.Add(journal);
        await _context.SaveChangesAsync();
    }

    public async Task<List<JournalConnexion>> GetByUtilisateurAsync(Guid utilisateurId, int? limite = null)
    {
        var query = _context.JournalConnexions
            .Where(j => j.UtilisateurId == utilisateurId)
            .OrderByDescending(j => j.DateTentative)
            .AsQueryable();

        if (limite.HasValue)
        {
            query = query.Take(limite.Value);
        }

        return await query.ToListAsync();
    }

    public async Task<List<JournalConnexion>> GetTentativesEchoueesAsync(string email, TimeSpan? periode = null)
    {
        var dateLimite = periode.HasValue 
            ? DateTime.UtcNow.Subtract(periode.Value) 
            : DateTime.UtcNow.AddHours(-1);

        return await _context.JournalConnexions
            .Where(j => j.Email == email 
                && !j.Succes 
                && j.DateTentative >= dateLimite)
            .OrderByDescending(j => j.DateTentative)
            .ToListAsync();
    }

    public async Task<int> GetNombreTentativesEchoueesAsync(string email, TimeSpan periode)
    {
        var dateLimite = DateTime.UtcNow.Subtract(periode);

        return await _context.JournalConnexions
            .CountAsync(j => j.Email == email 
                && !j.Succes 
                && j.DateTentative >= dateLimite);
    }
}

