using G_StockVente.Domain.Models;
using G_StockVente.Infrastructure.Data;
using G_StockVente.Infrastructure.Services;
using G_StockVente.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace G_StockVente.Controllers;

[Authorize]
public class NotificationsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly INotificationService _notificationService;
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationsController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        INotificationService notificationService,
        IHubContext<NotificationHub> hubContext)
    {
        _context = context;
        _userManager = userManager;
        _notificationService = notificationService;
        _hubContext = hubContext;
    }

    // GET: Notifications
    public async Task<IActionResult> Index(bool? estLue)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var notifications = await _notificationService.GetAllAsync(user.Id, estLue.HasValue ? estLue.Value : null);

        ViewBag.EstLue = estLue;
        ViewBag.NombreNonLues = await _notificationService.GetNombreNonLuesAsync(user.Id);

        return View(notifications);
    }

    // POST: Notifications/MarquerCommeLue/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarquerCommeLue(Guid id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var success = await _notificationService.MarquerCommeLueAsync(id, user.Id);
        if (success)
        {
            // Mettre à jour le compteur en temps réel
            var nombreNonLues = await _notificationService.GetNombreNonLuesAsync(user.Id);
            await _hubContext.Clients.User(user.Id.ToString())
                .SendAsync("UpdateNotificationCount", nombreNonLues);
        }

        return RedirectToAction(nameof(Index));
    }

    // POST: Notifications/MarquerToutesCommeLues
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarquerToutesCommeLues()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var count = await _notificationService.MarquerToutesCommeLuesAsync(user.Id);

        // Mettre à jour le compteur en temps réel
        await _hubContext.Clients.User(user.Id.ToString())
            .SendAsync("UpdateNotificationCount", 0);

        TempData["Success"] = $"{count} notification(s) marquée(s) comme lue(s).";
        return RedirectToAction(nameof(Index));
    }

    // POST: Notifications/Supprimer/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Supprimer(Guid id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == id && n.UtilisateurId == user.Id);

        if (notification == null)
        {
            TempData["Error"] = "Notification introuvable.";
            return RedirectToAction(nameof(Index));
        }

        _context.Notifications.Remove(notification);
        await _context.SaveChangesAsync();

        // Mettre à jour le compteur en temps réel
        var nombreNonLues = await _notificationService.GetNombreNonLuesAsync(user.Id);
        await _hubContext.Clients.User(user.Id.ToString())
            .SendAsync("UpdateNotificationCount", nombreNonLues);

        TempData["Success"] = "Notification supprimée.";
        return RedirectToAction(nameof(Index));
    }

    // GET: Notifications/GetNonLues (API pour SignalR)
    [HttpGet]
    public async Task<IActionResult> GetNonLues()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var notifications = await _notificationService.GetNonLuesAsync(user.Id);
        var nombre = await _notificationService.GetNombreNonLuesAsync(user.Id);

        return Json(new
        {
            count = nombre,
            notifications = notifications.Select(n => new
            {
                id = n.Id,
                titre = n.Titre,
                message = n.Message,
                type = n.Type.ToString(),
                dateCreation = n.DateCreation.ToString("dd/MM/yyyy HH:mm"),
                lienAction = n.LienAction
            })
        });
    }
}

