using G_StockVente.Domain.Models;
using G_StockVente.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace G_StockVente.Controllers;

[Authorize]
public class CategoriesController : Controller
{
    private readonly ApplicationDbContext _context;

    public CategoriesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Categories
    public async Task<IActionResult> Index()
    {
        var categories = await _context.Categories
            .Include(c => c.CategorieParente)
            .Include(c => c.SousCategories)
            .Where(c => c.CategorieParenteId == null)
            .OrderBy(c => c.Nom)
            .ToListAsync();

        return View(categories);
    }

    // GET: Categories/Details/5
    public async Task<IActionResult> Details(Guid? id)
    {
        if (id == null) return NotFound();

        var categorie = await _context.Categories
            .Include(c => c.CategorieParente)
            .Include(c => c.SousCategories)
            .Include(c => c.Produits)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (categorie == null) return NotFound();

        return View(categorie);
    }

    // GET: Categories/Create
    [Authorize(Roles = "AdminReseau,ManagerBoutique")]
    public async Task<IActionResult> Create()
    {
        ViewData["CategorieParenteId"] = new SelectList(
            await _context.Categories.OrderBy(c => c.Nom).ToListAsync(),
            "Id", "Nom");
        return View();
    }

    // POST: Categories/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "AdminReseau,ManagerBoutique")]
    public async Task<IActionResult> Create([Bind("Nom,Description,CategorieParenteId")] Categorie categorie)
    {
        if (ModelState.IsValid)
        {
            categorie.DateCreation = DateTime.UtcNow;
            _context.Add(categorie);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        ViewData["CategorieParenteId"] = new SelectList(
            await _context.Categories.OrderBy(c => c.Nom).ToListAsync(),
            "Id", "Nom", categorie.CategorieParenteId);
        return View(categorie);
    }

    // GET: Categories/Edit/5
    [Authorize(Roles = "AdminReseau,ManagerBoutique")]
    public async Task<IActionResult> Edit(Guid? id)
    {
        if (id == null) return NotFound();

        var categorie = await _context.Categories.FindAsync(id);
        if (categorie == null) return NotFound();

        ViewData["CategorieParenteId"] = new SelectList(
            await _context.Categories.Where(c => c.Id != id).OrderBy(c => c.Nom).ToListAsync(),
            "Id", "Nom", categorie.CategorieParenteId);
        return View(categorie);
    }

    // POST: Categories/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "AdminReseau,ManagerBoutique")]
    public async Task<IActionResult> Edit(Guid id, [Bind("Id,Nom,Description,CategorieParenteId,DateCreation")] Categorie categorie)
    {
        if (id != categorie.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(categorie);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CategorieExists(categorie.Id))
                    return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }
        ViewData["CategorieParenteId"] = new SelectList(
            await _context.Categories.Where(c => c.Id != id).OrderBy(c => c.Nom).ToListAsync(),
            "Id", "Nom", categorie.CategorieParenteId);
        return View(categorie);
    }

    // GET: Categories/Delete/5
    [Authorize(Roles = "AdminReseau,ManagerBoutique")]
    public async Task<IActionResult> Delete(Guid? id)
    {
        if (id == null) return NotFound();

        var categorie = await _context.Categories
            .Include(c => c.CategorieParente)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (categorie == null) return NotFound();

        return View(categorie);
    }

    // POST: Categories/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "AdminReseau,ManagerBoutique")]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var categorie = await _context.Categories
            .Include(c => c.Produits)
            .Include(c => c.SousCategories)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (categorie == null) return NotFound();

        if (categorie.Produits.Any() || categorie.SousCategories.Any())
        {
            TempData["Error"] = "Impossible de supprimer une catégorie qui contient des produits ou des sous-catégories.";
            return RedirectToAction(nameof(Index));
        }

        _context.Categories.Remove(categorie);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool CategorieExists(Guid id)
    {
        return _context.Categories.Any(e => e.Id == id);
    }
}

