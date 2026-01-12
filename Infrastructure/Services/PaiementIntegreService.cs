using G_StockVente.Domain.Models;
using G_StockVente.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace G_StockVente.Infrastructure.Services;

/// <summary>
/// Service pour gérer les paiements intégrés
/// </summary>
public interface IPaiementIntegreService
{
    Task<PaiementIntegre?> GetByIdAsync(Guid id);
    Task<List<PaiementIntegre>> GetByVenteAsync(Guid venteId);
    Task<PaiementIntegre> InitierPaiementAsync(Guid venteId, TypePaiementIntegre type, decimal montant);
    Task<bool> ConfirmerPaiementAsync(Guid paiementId, string referenceExterne, string? donneesReponse = null);
    Task<bool> EchouerPaiementAsync(Guid paiementId, string messageErreur);
    Task<bool> RembourserPaiementAsync(Guid paiementId);
}

public class PaiementIntegreService : IPaiementIntegreService
{
    private readonly ApplicationDbContext _context;

    public PaiementIntegreService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaiementIntegre?> GetByIdAsync(Guid id)
    {
        return await _context.PaiementsIntegres
            .Include(p => p.Vente)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<List<PaiementIntegre>> GetByVenteAsync(Guid venteId)
    {
        return await _context.PaiementsIntegres
            .Where(p => p.VenteId == venteId)
            .OrderByDescending(p => p.DateCreation)
            .ToListAsync();
    }

    public async Task<PaiementIntegre> InitierPaiementAsync(Guid venteId, TypePaiementIntegre type, decimal montant)
    {
        var vente = await _context.Ventes.FindAsync(venteId);
        if (vente == null)
            throw new ArgumentException("Vente introuvable", nameof(venteId));

        // Générer une référence temporaire unique
        var referenceTemp = $"TMP-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";

        var paiement = new PaiementIntegre
        {
            Id = Guid.NewGuid(),
            VenteId = venteId,
            Type = type,
            Montant = montant,
            Statut = StatutPaiement.EnAttente,
            DateCreation = DateTime.UtcNow,
            ReferenceExterne = referenceTemp // Référence temporaire, sera mise à jour lors de la confirmation
        };

        _context.PaiementsIntegres.Add(paiement);
        await _context.SaveChangesAsync();

        return paiement;
    }

    public async Task<bool> ConfirmerPaiementAsync(Guid paiementId, string referenceExterne, string? donneesReponse = null)
    {
        var paiement = await _context.PaiementsIntegres.FindAsync(paiementId);
        if (paiement == null || paiement.Statut != StatutPaiement.EnAttente)
            return false;

        paiement.Statut = StatutPaiement.Valide;
        paiement.ReferenceExterne = referenceExterne;
        paiement.DateValidation = DateTime.UtcNow;
        paiement.DonneesReponse = donneesReponse;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> EchouerPaiementAsync(Guid paiementId, string messageErreur)
    {
        var paiement = await _context.PaiementsIntegres.FindAsync(paiementId);
        if (paiement == null || paiement.Statut != StatutPaiement.EnAttente)
            return false;

        paiement.Statut = StatutPaiement.Echec;
        paiement.MessageErreur = messageErreur;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RembourserPaiementAsync(Guid paiementId)
    {
        var paiement = await _context.PaiementsIntegres.FindAsync(paiementId);
        if (paiement == null || paiement.Statut != StatutPaiement.Valide)
            return false;

        paiement.Statut = StatutPaiement.Rembourse;

        await _context.SaveChangesAsync();
        return true;
    }
}

