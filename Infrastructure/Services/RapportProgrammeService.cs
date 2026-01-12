using G_StockVente.Domain.Models;
using G_StockVente.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace G_StockVente.Infrastructure.Services;

/// <summary>
/// Service pour gérer les rapports programmés
/// </summary>
public interface IRapportProgrammeService
{
    Task<RapportProgramme?> GetByIdAsync(Guid id);
    Task<List<RapportProgramme>> GetAllAsync(Guid? boutiqueId = null);
    Task<List<RapportProgramme>> GetActifsAsync();
    Task<RapportProgramme> CreateAsync(RapportProgramme rapport);
    Task<bool> UpdateAsync(RapportProgramme rapport);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ActiverAsync(Guid id, bool actif);
    Task<List<RapportProgramme>> GetARapporterAsync();
    Task MarquerCommeExecuteAsync(Guid id);
}

public class RapportProgrammeService : IRapportProgrammeService
{
    private readonly ApplicationDbContext _context;

    public RapportProgrammeService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<RapportProgramme?> GetByIdAsync(Guid id)
    {
        return await _context.RapportsProgrammes
            .Include(r => r.Boutique)
            .Include(r => r.UtilisateurCreateur)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<List<RapportProgramme>> GetAllAsync(Guid? boutiqueId = null)
    {
        var query = _context.RapportsProgrammes
            .Include(r => r.Boutique)
            .Include(r => r.UtilisateurCreateur)
            .AsQueryable();

        if (boutiqueId.HasValue)
        {
            query = query.Where(r => r.BoutiqueId == boutiqueId || r.BoutiqueId == null);
        }

        return await query
            .OrderByDescending(r => r.DateCreation)
            .ToListAsync();
    }

    public async Task<List<RapportProgramme>> GetActifsAsync()
    {
        return await _context.RapportsProgrammes
            .Where(r => r.EstActif)
            .Include(r => r.Boutique)
            .Include(r => r.UtilisateurCreateur)
            .OrderBy(r => r.ProchaineExecution)
            .ToListAsync();
    }

    public async Task<RapportProgramme> CreateAsync(RapportProgramme rapport)
    {
        rapport.Id = Guid.NewGuid();
        rapport.DateCreation = DateTime.UtcNow;
        rapport.EstActif = true;

        // Calculer la prochaine exécution
        rapport.ProchaineExecution = CalculerProchaineExecution(rapport.Frequence);

        _context.RapportsProgrammes.Add(rapport);
        await _context.SaveChangesAsync();

        return rapport;
    }

    public async Task<bool> UpdateAsync(RapportProgramme rapport)
    {
        var existing = await _context.RapportsProgrammes.FindAsync(rapport.Id);
        if (existing == null)
            return false;

        existing.TypeRapport = rapport.TypeRapport;
        existing.Frequence = rapport.Frequence;
        existing.BoutiqueId = rapport.BoutiqueId;
        existing.EmailDestinataire = rapport.EmailDestinataire;
        existing.EstActif = rapport.EstActif;

        // Recalculer la prochaine exécution si nécessaire
        if (existing.ProchaineExecution.HasValue && existing.ProchaineExecution < DateTime.UtcNow)
        {
            existing.ProchaineExecution = CalculerProchaineExecution(existing.Frequence);
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var rapport = await _context.RapportsProgrammes.FindAsync(id);
        if (rapport == null)
            return false;

        _context.RapportsProgrammes.Remove(rapport);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ActiverAsync(Guid id, bool actif)
    {
        var rapport = await _context.RapportsProgrammes.FindAsync(id);
        if (rapport == null)
            return false;

        rapport.EstActif = actif;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<RapportProgramme>> GetARapporterAsync()
    {
        var maintenant = DateTime.UtcNow;
        return await _context.RapportsProgrammes
            .Where(r => r.EstActif 
                && r.ProchaineExecution.HasValue
                && r.ProchaineExecution <= maintenant
                && (r.DerniereExecution == null 
                    || r.DerniereExecution < r.ProchaineExecution))
            .Include(r => r.Boutique)
            .ToListAsync();
    }

    public async Task MarquerCommeExecuteAsync(Guid id)
    {
        var rapport = await _context.RapportsProgrammes.FindAsync(id);
        if (rapport == null)
            return;

        rapport.DerniereExecution = DateTime.UtcNow;
        rapport.ProchaineExecution = CalculerProchaineExecution(rapport.Frequence);

        await _context.SaveChangesAsync();
    }

    private DateTime CalculerProchaineExecution(FrequenceRapport frequence)
    {
        var maintenant = DateTime.UtcNow;
        return frequence switch
        {
            FrequenceRapport.Journalier => maintenant.AddDays(1).Date,
            FrequenceRapport.Hebdomadaire => maintenant.AddDays(7).Date,
            FrequenceRapport.Mensuel => maintenant.AddMonths(1).Date,
            _ => maintenant.AddDays(1).Date
        };
    }
}

