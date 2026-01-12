using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using G_StockVente.Domain.Models;
using System.Text;
using Markdig;

namespace G_StockVente.Controllers;

[Authorize]
public class AideController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IWebHostEnvironment _environment;

    public AideController(UserManager<ApplicationUser> userManager, IWebHostEnvironment environment)
    {
        _userManager = userManager;
        _environment = environment;
    }

    // GET: Aide
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        // Déterminer le rôle principal de l'utilisateur pour afficher le contenu approprié
        var isAdminReseau = await _userManager.IsInRoleAsync(user, "AdminReseau");
        var isManagerBoutique = await _userManager.IsInRoleAsync(user, "ManagerBoutique");
        var isCaissier = await _userManager.IsInRoleAsync(user, "Caissier");
        var isGestionnaireStock = await _userManager.IsInRoleAsync(user, "GestionnaireStock");

        ViewBag.IsAdminReseau = isAdminReseau;
        ViewBag.IsManagerBoutique = isManagerBoutique;
        ViewBag.IsCaissier = isCaissier;
        ViewBag.IsGestionnaireStock = isGestionnaireStock;

        return View();
    }

    // GET: Aide/Documentation?document=guide-utilisation
    public IActionResult Documentation(string document)
    {
        // Mapping des noms de documents aux fichiers
        var documentMap = new Dictionary<string, (string fileName, string title)>
        {
            { "guide-utilisation", ("GUIDE_UTILISATION.md", "Guide d'utilisation") },
            { "workflow-complet", ("WORKFLOW_COMPLET.md", "Workflow complet") },
            { "index", ("INDEX_DOCUMENTATION.md", "Index de la documentation") }
        };

        if (string.IsNullOrEmpty(document) || !documentMap.ContainsKey(document))
        {
            // Par défaut, afficher l'index
            document = "index";
        }

        var (fileName, title) = documentMap[document];

        // Chemin du fichier dans la racine du projet
        var filePath = Path.Combine(_environment.ContentRootPath, fileName);

        if (!System.IO.File.Exists(filePath))
        {
            ViewBag.Error = $"Le fichier de documentation '{fileName}' n'a pas été trouvé.";
            ViewBag.Title = title;
            ViewBag.DocumentName = document;
            ViewBag.AvailableDocuments = documentMap;
            return View();
        }

        // Lire le contenu du fichier markdown
        var markdownContent = System.IO.File.ReadAllText(filePath, Encoding.UTF8);

        // Convertir le markdown en HTML avec Markdig
        var pipeline = new Markdig.MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
        var htmlContent = Markdig.Markdown.ToHtml(markdownContent, pipeline);

        ViewBag.Title = title;
        ViewBag.MarkdownContent = markdownContent;
        ViewBag.HtmlContent = htmlContent;
        ViewBag.DocumentName = document;
        ViewBag.AvailableDocuments = documentMap;
        ViewBag.FileName = fileName;

        return View();
    }
}

