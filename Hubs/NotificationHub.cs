using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;

namespace G_StockVente.Hubs;

/// <summary>
/// Hub SignalR pour les notifications en temps réel
/// </summary>
[Authorize]
public class NotificationHub : Hub
{
    /// <summary>
    /// Groupe de notifications par utilisateur (par ID utilisateur)
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        if (!string.IsNullOrEmpty(userId))
        {
            // Ajouter l'utilisateur à son groupe de notifications
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
        }
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.UserIdentifier;
        if (!string.IsNullOrEmpty(userId))
        {
            // Retirer l'utilisateur de son groupe
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
        }
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Envoyer une notification à un utilisateur spécifique
    /// </summary>
    public async Task SendNotificationToUser(string userId, object notification)
    {
        await Clients.Group($"user_{userId}").SendAsync("ReceiveNotification", notification);
    }

    /// <summary>
    /// Envoyer une notification à tous les utilisateurs d'une boutique
    /// </summary>
    public async Task SendNotificationToBoutique(string boutiqueId, object notification)
    {
        await Clients.Group($"boutique_{boutiqueId}").SendAsync("ReceiveNotification", notification);
    }

    /// <summary>
    /// Rejoindre le groupe d'une boutique (appelé depuis le client)
    /// </summary>
    public async Task JoinBoutiqueGroup(string boutiqueId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"boutique_{boutiqueId}");
    }

    /// <summary>
    /// Quitter le groupe d'une boutique
    /// </summary>
    public async Task LeaveBoutiqueGroup(string boutiqueId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"boutique_{boutiqueId}");
    }
}

