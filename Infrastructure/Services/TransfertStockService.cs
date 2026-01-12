using G_StockVente.Domain.Models;
using G_StockVente.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace G_StockVente.Infrastructure.Services;

/// <summary>
/// Service pour gérer les transferts de stock
/// </summary>
public interface ITransfertStockService
{
    Task<TransfertStock?> GetByIdAsync(Guid id);
    Task<List<TransfertStock>> GetByBoutiqueAsync(Guid boutiqueId);
    Task<List<TransfertStock>> GetByDepotAsync(Guid depotId);
    Task<TransfertStock> CreateAsync(TransfertStock transfert);
    Task<bool> ValiderAsync(Guid transfertId, Guid validateurId);
    Task<bool> RecevoirAsync(Guid transfertId, Guid recepteurId, Dictionary<Guid, int> quantitesRecues);
    Task<bool> AnnulerAsync(Guid transfertId, Guid annulateurId, string? raison = null);
    Task<string> GenererNumeroTransfertAsync(Guid? boutiqueId);
}

public class TransfertStockService : ITransfertStockService
{
    private readonly ApplicationDbContext _context;

    public TransfertStockService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TransfertStock?> GetByIdAsync(Guid id)
    {
        return await _context.TransfertsStock
            .Include(t => t.BoutiqueSource)
            .Include(t => t.BoutiqueDestination)
            .Include(t => t.DepotSource)
            .Include(t => t.DepotDestination)
            .Include(t => t.UtilisateurCreateur)
            .Include(t => t.UtilisateurValidateur)
            .Include(t => t.UtilisateurRecepteur)
            .Include(t => t.LignesTransfert)
                .ThenInclude(l => l.ProduitBoutique)
                    .ThenInclude(pb => pb.Produit)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<List<TransfertStock>> GetByBoutiqueAsync(Guid boutiqueId)
    {
        return await _context.TransfertsStock
            .Where(t => t.BoutiqueSourceId == boutiqueId || t.BoutiqueDestinationId == boutiqueId)
            .Include(t => t.BoutiqueSource)
            .Include(t => t.BoutiqueDestination)
            .Include(t => t.DepotSource)
            .Include(t => t.DepotDestination)
            .OrderByDescending(t => t.DateCreation)
            .ToListAsync();
    }

    public async Task<List<TransfertStock>> GetByDepotAsync(Guid depotId)
    {
        return await _context.TransfertsStock
            .Where(t => t.DepotSourceId == depotId || t.DepotDestinationId == depotId)
            .Include(t => t.BoutiqueSource)
            .Include(t => t.BoutiqueDestination)
            .Include(t => t.DepotSource)
            .Include(t => t.DepotDestination)
            .OrderByDescending(t => t.DateCreation)
            .ToListAsync();
    }

    public async Task<TransfertStock> CreateAsync(TransfertStock transfert)
    {
        if (string.IsNullOrEmpty(transfert.Numero))
        {
            var depotSource = await _context.Depots.FindAsync(transfert.DepotSourceId);
            transfert.Numero = await GenererNumeroTransfertAsync(transfert.BoutiqueSourceId ?? depotSource?.BoutiqueId);
        }

        transfert.DateCreation = DateTime.UtcNow;
        transfert.Statut = StatutTransfert.EnAttente;

        _context.TransfertsStock.Add(transfert);
        await _context.SaveChangesAsync();

        return transfert;
    }

    public async Task<bool> ValiderAsync(Guid transfertId, Guid validateurId)
    {
        var transfert = await _context.TransfertsStock
            .Include(t => t.LignesTransfert)
            .FirstOrDefaultAsync(t => t.Id == transfertId);

        if (transfert == null || transfert.Statut != StatutTransfert.EnAttente)
            return false;

        // Charger le dépôt source pour accéder à BoutiqueId
        var depotSource = await _context.Depots.FindAsync(transfert.DepotSourceId);

        // Vérifier que les stocks sont suffisants
        foreach (var ligne in transfert.LignesTransfert)
        {
            var stock = await _context.Stocks
                .FirstOrDefaultAsync(s => s.DepotId == transfert.DepotSourceId 
                    && s.ProduitBoutiqueId == ligne.ProduitBoutiqueId);

            if (stock == null || stock.Quantite < ligne.Quantite)
            {
                return false; // Stock insuffisant
            }
        }

        transfert.Statut = StatutTransfert.Valide;
        transfert.UtilisateurValidateurId = validateurId;
        transfert.DateValidation = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RecevoirAsync(Guid transfertId, Guid recepteurId, Dictionary<Guid, int> quantitesRecues)
    {
        var transfert = await _context.TransfertsStock
            .Include(t => t.LignesTransfert)
            .FirstOrDefaultAsync(t => t.Id == transfertId);

        if (transfert == null || transfert.Statut != StatutTransfert.Valide && transfert.Statut != StatutTransfert.EnTransit)
            return false;

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Mettre à jour les quantités reçues
            foreach (var ligne in transfert.LignesTransfert)
            {
                if (quantitesRecues.TryGetValue(ligne.Id, out var quantiteRecue))
                {
                    ligne.QuantiteRecue = quantiteRecue;
                }
            }

            // Créer les mouvements de stock (sortie source, entrée destination)
            foreach (var ligne in transfert.LignesTransfert)
            {
                var quantiteRecue = ligne.QuantiteRecue ?? ligne.Quantite;
                
                // Sortie du dépôt source
                var mouvementSortie = new MouvementStock
                {
                    Id = Guid.NewGuid(),
                    DepotId = transfert.DepotSourceId,
                    ProduitBoutiqueId = ligne.ProduitBoutiqueId,
                    TypeMouvement = TypeMouvementStock.TransfertSortie,
                    Quantite = -quantiteRecue,
                    DateMouvement = DateTime.UtcNow,
                    UtilisateurId = recepteurId,
                    Commentaire = $"Transfert vers {transfert.DepotDestination?.Nom ?? "N/A"}",
                    TransfertStockId = transfert.Id
                };
                _context.MouvementsStock.Add(mouvementSortie);

                // Entrée dans le dépôt destination
                var mouvementEntree = new MouvementStock
                {
                    Id = Guid.NewGuid(),
                    DepotId = transfert.DepotDestinationId,
                    ProduitBoutiqueId = ligne.ProduitBoutiqueId,
                    TypeMouvement = TypeMouvementStock.TransfertEntree,
                    Quantite = quantiteRecue,
                    DateMouvement = DateTime.UtcNow,
                    UtilisateurId = recepteurId,
                    Commentaire = $"Transfert depuis {transfert.DepotSource?.Nom ?? "N/A"}",
                    TransfertStockId = transfert.Id
                };
                _context.MouvementsStock.Add(mouvementEntree);
            }

            transfert.Statut = StatutTransfert.Reçu;
            transfert.UtilisateurRecepteurId = recepteurId;
            transfert.DateReception = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> AnnulerAsync(Guid transfertId, Guid annulateurId, string? raison = null)
    {
        var transfert = await _context.TransfertsStock
            .FirstOrDefaultAsync(t => t.Id == transfertId);

        if (transfert == null || transfert.Statut == StatutTransfert.Reçu || transfert.Statut == StatutTransfert.Annule)
            return false;

        transfert.Statut = StatutTransfert.Annule;
        transfert.DateAnnulation = DateTime.UtcNow;
        if (!string.IsNullOrEmpty(raison))
        {
            transfert.Notes = raison;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<string> GenererNumeroTransfertAsync(Guid? boutiqueId)
    {
        string prefixe = "TRF";
        if (boutiqueId.HasValue)
        {
            var boutique = await _context.Boutiques.FindAsync(boutiqueId.Value);
            // Utiliser les 3 premières lettres du nom ou "TRF" par défaut
            prefixe = boutique != null && boutique.Nom.Length >= 3 
                ? boutique.Nom.Substring(0, 3).ToUpper() 
                : "TRF";
        }
        
        var annee = DateTime.UtcNow.Year;
        var query = _context.TransfertsStock.AsQueryable();
        
        if (boutiqueId.HasValue)
        {
            query = query.Where(t => t.BoutiqueSourceId == boutiqueId || t.DepotSource.BoutiqueId == boutiqueId);
        }
        
        var dernierNumero = await query
            .Where(t => t.Numero.StartsWith($"{prefixe}-{annee}-"))
            .OrderByDescending(t => t.Numero)
            .Select(t => t.Numero)
            .FirstOrDefaultAsync();

        int numero = 1;
        if (!string.IsNullOrEmpty(dernierNumero))
        {
            var parts = dernierNumero.Split('-');
            if (parts.Length >= 3 && int.TryParse(parts[^1], out var dernier))
            {
                numero = dernier + 1;
            }
        }

        return $"{prefixe}-{annee}-{numero:D6}";
    }
}

