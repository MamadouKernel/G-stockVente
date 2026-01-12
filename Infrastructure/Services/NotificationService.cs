using G_StockVente.Domain.Models;
using G_StockVente.Infrastructure.Data;
using G_StockVente.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace G_StockVente.Infrastructure.Services;

/// <summary>
/// Service pour gérer les notifications
/// </summary>
public interface INotificationService
{
    Task<Notification> CreerAsync(Notification notification);
    Task<List<Notification>> GetNonLuesAsync(Guid utilisateurId);
    Task<List<Notification>> GetAllAsync(Guid utilisateurId, bool? estLue = null);
    Task<bool> MarquerCommeLueAsync(Guid notificationId, Guid utilisateurId);
    Task<int> MarquerToutesCommeLuesAsync(Guid utilisateurId);
    Task<int> GetNombreNonLuesAsync(Guid utilisateurId);
    Task CreerAlerteStockBasAsync(Guid produitBoutiqueId, Guid boutiqueId);
    Task CreerAlerteRuptureStockAsync(Guid produitBoutiqueId, Guid boutiqueId);
    Task CreerNotificationTransfertAsync(Guid transfertId, TypeNotification type, Guid? utilisateurId = null);
}

public class NotificationService : INotificationService
{
    private readonly ApplicationDbContext _context;
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationService(ApplicationDbContext context, IHubContext<NotificationHub> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
    }

    public async Task<Notification> CreerAsync(Notification notification)
    {
        notification.Id = Guid.NewGuid();
        notification.DateCreation = DateTime.UtcNow;
        notification.EstLue = false;

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        // Envoyer la notification via SignalR en temps réel
        if (notification.UtilisateurId.HasValue)
        {
            var nombreNonLues = await GetNombreNonLuesAsync(notification.UtilisateurId.Value);
            await _hubContext.Clients.User(notification.UtilisateurId.Value.ToString())
                .SendAsync("ReceiveNotification", new
                {
                    id = notification.Id,
                    titre = notification.Titre,
                    message = notification.Message,
                    type = notification.Type.ToString(),
                    dateCreation = notification.DateCreation.ToString("dd/MM/yyyy HH:mm"),
                    lienAction = notification.LienAction,
                    count = nombreNonLues
                });
            
            // Mettre à jour le compteur
            await _hubContext.Clients.User(notification.UtilisateurId.Value.ToString())
                .SendAsync("UpdateNotificationCount", nombreNonLues);
        }

        return notification;
    }

    public async Task<List<Notification>> GetNonLuesAsync(Guid utilisateurId)
    {
        return await _context.Notifications
            .Where(n => n.UtilisateurId == utilisateurId && !n.EstLue)
            .Include(n => n.Boutique)
            .OrderByDescending(n => n.DateCreation)
            .ToListAsync();
    }

    public async Task<List<Notification>> GetAllAsync(Guid utilisateurId, bool? estLue = null)
    {
        var query = _context.Notifications
            .Where(n => n.UtilisateurId == utilisateurId)
            .Include(n => n.Boutique)
            .AsQueryable();

        if (estLue.HasValue)
        {
            query = query.Where(n => n.EstLue == estLue.Value);
        }

        return await query
            .OrderByDescending(n => n.DateCreation)
            .ToListAsync();
    }

    public async Task<bool> MarquerCommeLueAsync(Guid notificationId, Guid utilisateurId)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UtilisateurId == utilisateurId);

        if (notification == null)
            return false;

        notification.EstLue = true;
        notification.DateLecture = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<int> MarquerToutesCommeLuesAsync(Guid utilisateurId)
    {
        var notifications = await _context.Notifications
            .Where(n => n.UtilisateurId == utilisateurId && !n.EstLue)
            .ToListAsync();

        foreach (var notification in notifications)
        {
            notification.EstLue = true;
            notification.DateLecture = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return notifications.Count;
    }

    public async Task<int> GetNombreNonLuesAsync(Guid utilisateurId)
    {
        return await _context.Notifications
            .CountAsync(n => n.UtilisateurId == utilisateurId && !n.EstLue);
    }

    public async Task CreerAlerteStockBasAsync(Guid produitBoutiqueId, Guid boutiqueId)
    {
        var produitBoutique = await _context.ProduitsBoutique
            .Include(pb => pb.Produit)
            .FirstOrDefaultAsync(pb => pb.Id == produitBoutiqueId);

        if (produitBoutique == null)
            return;

        // Calculer le stock actuel depuis la table Stock
        var stockTotal = await _context.Stocks
            .Where(s => s.ProduitBoutiqueId == produitBoutiqueId)
            .SumAsync(s => s.Quantite);

        // Récupérer les utilisateurs de la boutique (managers et gestionnaires de stock)
        var utilisateurs = await _context.Users
            .Where(u => u.BoutiqueActiveId == boutiqueId)
            .ToListAsync();

        foreach (var utilisateur in utilisateurs)
        {
            var notification = new Notification
            {
                UtilisateurId = utilisateur.Id,
                BoutiqueId = boutiqueId,
                Type = TypeNotification.StockBas,
                Titre = "Alerte Stock Bas",
                Message = $"Stock bas pour {produitBoutique.Produit.Nom}. Quantité actuelle : {stockTotal} (seuil : {produitBoutique.SeuilStockBas})",
                LienAction = $"/Produits/Details/{produitBoutique.ProduitId}"
            };

            await CreerAsync(notification);
        }
    }

    public async Task CreerAlerteRuptureStockAsync(Guid produitBoutiqueId, Guid boutiqueId)
    {
        var produitBoutique = await _context.ProduitsBoutique
            .Include(pb => pb.Produit)
            .FirstOrDefaultAsync(pb => pb.Id == produitBoutiqueId);

        if (produitBoutique == null)
            return;

        var utilisateurs = await _context.Users
            .Where(u => u.BoutiqueActiveId == boutiqueId)
            .ToListAsync();

        foreach (var utilisateur in utilisateurs)
        {
            var notification = new Notification
            {
                UtilisateurId = utilisateur.Id,
                BoutiqueId = boutiqueId,
                Type = TypeNotification.RuptureStock,
                Titre = "Rupture de Stock",
                Message = $"Rupture de stock pour {produitBoutique.Produit.Nom}",
                LienAction = $"/Produits/Details/{produitBoutique.ProduitId}"
            };

            await CreerAsync(notification);
        }
    }

    public async Task CreerNotificationTransfertAsync(Guid transfertId, TypeNotification type, Guid? utilisateurId = null)
    {
        var transfert = await _context.TransfertsStock
            .Include(t => t.BoutiqueSource)
            .Include(t => t.BoutiqueDestination)
            .FirstOrDefaultAsync(t => t.Id == transfertId);

        if (transfert == null)
            return;

            var (titre, message) = type switch
        {
            TypeNotification.TransfertEnAttente => ("Transfert en Attente", $"Nouveau transfert créé : {transfert.Numero}"),
            TypeNotification.TransfertRecu => ("Transfert Reçu", $"Transfert reçu : {transfert.Numero}"),
            _ => ("Notification Transfert", $"Notification pour transfert {transfert.Numero}")
        };

        // Si utilisateurId est fourni, notifier uniquement cet utilisateur
        if (utilisateurId.HasValue)
        {
            var notification = new Notification
            {
                UtilisateurId = utilisateurId.Value,
                BoutiqueId = transfert.BoutiqueDestinationId,
                Type = type,
                Titre = titre,
                Message = message,
                LienAction = $"/Transferts/Details/{transfertId}"
            };
            await CreerAsync(notification);
        }
        else
        {
            // Notifier les managers des boutiques concernées
            var utilisateurs = await _context.Users
                .Where(u => u.BoutiqueActiveId == transfert.BoutiqueSourceId 
                    || u.BoutiqueActiveId == transfert.BoutiqueDestinationId)
                .ToListAsync();

            foreach (var utilisateur in utilisateurs)
            {
                var notification = new Notification
                {
                    UtilisateurId = utilisateur.Id,
                    BoutiqueId = utilisateur.BoutiqueActiveId,
                    Type = type,
                    Titre = titre,
                    Message = message,
                    LienAction = $"/Transferts/Details/{transfertId}"
                };
                await CreerAsync(notification);
            }
        }
    }
}

