using G_StockVente.Domain.Models;
using G_StockVente.Infrastructure.Data;
using G_StockVente.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace G_StockVente.Controllers;

[Authorize(Roles = "AdminReseau")]
public class JournalConnexionsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJournalConnexionService _journalService;

    public JournalConnexionsController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IJournalConnexionService journalService)
    {
        _context = context;
        _userManager = userManager;
        _journalService = journalService;
    }

    // GET: JournalConnexions
    public async Task<IActionResult> Index(
        DateTime? dateDebut, 
        DateTime? dateFin, 
        string? email, 
        bool? succes,
        Guid? utilisateurId,
        int page = 1)
    {
        var query = _context.JournalConnexions
            .Include(j => j.Utilisateur)
            .AsQueryable();

        if (dateDebut.HasValue)
        {
            query = query.Where(j => j.DateTentative >= dateDebut.Value);
        }

        if (dateFin.HasValue)
        {
            var dateFinUtc = dateFin.Value.Date.AddDays(1).AddTicks(-1);
            query = query.Where(j => j.DateTentative <= dateFinUtc);
        }

        if (!string.IsNullOrWhiteSpace(email))
        {
            query = query.Where(j => j.Email.Contains(email));
        }

        if (succes.HasValue)
        {
            query = query.Where(j => j.Succes == succes.Value);
        }

        if (utilisateurId.HasValue)
        {
            query = query.Where(j => j.UtilisateurId == utilisateurId.Value);
        }

        var pageSize = 50;
        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var connexions = await query
            .OrderByDescending(j => j.DateTentative)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.DateDebut = dateDebut;
        ViewBag.DateFin = dateFin;
        ViewBag.Email = email;
        ViewBag.Succes = succes;
        ViewBag.UtilisateurId = utilisateurId;
        ViewBag.Page = page;
        ViewBag.TotalPages = totalPages;
        ViewBag.TotalCount = totalCount;

        ViewBag.Utilisateurs = await _context.Users
            .OrderBy(u => u.Email)
            .Select(u => new { u.Id, u.Email })
            .ToListAsync();

        return View(connexions);
    }

    // GET: JournalConnexions/Details/5
    public async Task<IActionResult> Details(Guid? id)
    {
        if (id == null) return NotFound();

        var journal = await _context.JournalConnexions
            .Include(j => j.Utilisateur)
            .FirstOrDefaultAsync(j => j.Id == id);

        if (journal == null) return NotFound();

        return View(journal);
    }
}

