using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace G_StockVente.Controllers;

[Authorize]
public class DocumentationController : Controller
{
    private readonly IWebHostEnvironment _env;

    public DocumentationController(IWebHostEnvironment env)
    {
        _env = env;
    }

    // GET: Documentation/GuideUtilisation
    public IActionResult GuideUtilisation()
    {
        var filePath = Path.Combine(_env.ContentRootPath, "GUIDE_UTILISATION.md");
        if (!System.IO.File.Exists(filePath))
        {
            return NotFound();
        }

        var content = System.IO.File.ReadAllText(filePath);
        ViewBag.Title = "Guide d'utilisation";
        ViewBag.FileName = "GUIDE_UTILISATION.md";
        ViewBag.Content = content;
        return View("Markdown");
    }

    // GET: Documentation/WorkflowComplet
    public IActionResult WorkflowComplet()
    {
        var filePath = Path.Combine(_env.ContentRootPath, "WORKFLOW_COMPLET.md");
        if (!System.IO.File.Exists(filePath))
        {
            return NotFound();
        }

        var content = System.IO.File.ReadAllText(filePath);
        ViewBag.Title = "Workflow complet";
        ViewBag.FileName = "WORKFLOW_COMPLET.md";
        ViewBag.Content = content;
        return View("Markdown");
    }

    // GET: Documentation/IndexDocumentation
    public IActionResult IndexDocumentation()
    {
        var filePath = Path.Combine(_env.ContentRootPath, "INDEX_DOCUMENTATION.md");
        if (!System.IO.File.Exists(filePath))
        {
            return NotFound();
        }

        var content = System.IO.File.ReadAllText(filePath);
        ViewBag.Title = "Index de la documentation";
        ViewBag.FileName = "INDEX_DOCUMENTATION.md";
        ViewBag.Content = content;
        return View("Markdown");
    }

    // GET: Documentation/PRDV2
    public IActionResult PRDV2()
    {
        var filePath = Path.Combine(_env.ContentRootPath, "PRD_V2.md");
        if (!System.IO.File.Exists(filePath))
        {
            return NotFound();
        }

        var content = System.IO.File.ReadAllText(filePath);
        ViewBag.Title = "PRD Version 2";
        ViewBag.FileName = "PRD_V2.md";
        ViewBag.Content = content;
        return View("Markdown");
    }

    // GET: Documentation/PlanImplementationV2
    public IActionResult PlanImplementationV2()
    {
        var filePath = Path.Combine(_env.ContentRootPath, "PLAN_IMPLEMENTATION_V2.md");
        if (!System.IO.File.Exists(filePath))
        {
            return NotFound();
        }

        var content = System.IO.File.ReadAllText(filePath);
        ViewBag.Title = "Plan d'implémentation V2";
        ViewBag.FileName = "PLAN_IMPLEMENTATION_V2.md";
        ViewBag.Content = content;
        return View("Markdown");
    }

    // GET: Documentation/EtatAvancementV2
    public IActionResult EtatAvancementV2()
    {
        var filePath = Path.Combine(_env.ContentRootPath, "ETAT_AVANCEMENT_V2.md");
        if (!System.IO.File.Exists(filePath))
        {
            return NotFound();
        }

        var content = System.IO.File.ReadAllText(filePath);
        ViewBag.Title = "État d'avancement V2";
        ViewBag.FileName = "ETAT_AVANCEMENT_V2.md";
        ViewBag.Content = content;
        return View("Markdown");
    }

    // GET: Documentation/ConfigurationEmail
    public IActionResult ConfigurationEmail()
    {
        var filePath = Path.Combine(_env.ContentRootPath, "CONFIGURATION_EMAIL.md");
        if (!System.IO.File.Exists(filePath))
        {
            return NotFound();
        }

        var content = System.IO.File.ReadAllText(filePath);
        ViewBag.Title = "Configuration Email";
        ViewBag.FileName = "CONFIGURATION_EMAIL.md";
        ViewBag.Content = content;
        return View("Markdown");
    }

    // GET: Documentation/MigrationGuide
    public IActionResult MigrationGuide()
    {
        var filePath = Path.Combine(_env.ContentRootPath, "MIGRATION_GUID.md");
        if (!System.IO.File.Exists(filePath))
        {
            return NotFound();
        }

        var content = System.IO.File.ReadAllText(filePath);
        ViewBag.Title = "Guide de migration";
        ViewBag.FileName = "MIGRATION_GUID.md";
        ViewBag.Content = content;
        return View("Markdown");
    }

    // GET: Documentation/Securite
    public IActionResult Securite()
    {
        var filePath = Path.Combine(_env.ContentRootPath, "SECURITE.md");
        if (!System.IO.File.Exists(filePath))
        {
            return NotFound();
        }

        var content = System.IO.File.ReadAllText(filePath);
        ViewBag.Title = "Sécurité";
        ViewBag.FileName = "SECURITE.md";
        ViewBag.Content = content;
        return View("Markdown");
    }

    // GET: Documentation/CompteAdmin
    public IActionResult CompteAdmin()
    {
        var filePath = Path.Combine(_env.ContentRootPath, "COMPTE_ADMIN.md");
        if (!System.IO.File.Exists(filePath))
        {
            return NotFound();
        }

        var content = System.IO.File.ReadAllText(filePath);
        ViewBag.Title = "Compte administrateur";
        ViewBag.FileName = "COMPTE_ADMIN.md";
        ViewBag.Content = content;
        return View("Markdown");
    }

    // GET: Documentation/README
    public IActionResult README()
    {
        var filePath = Path.Combine(_env.ContentRootPath, "README.md");
        if (!System.IO.File.Exists(filePath))
        {
            return NotFound();
        }

        var content = System.IO.File.ReadAllText(filePath);
        ViewBag.Title = "README";
        ViewBag.FileName = "README.md";
        ViewBag.Content = content;
        return View("Markdown");
    }

    // GET: Documentation/VerificationPRDs
    public IActionResult VerificationPRDs()
    {
        var filePath = Path.Combine(_env.ContentRootPath, "VERIFICATION_PRDS.md");
        if (!System.IO.File.Exists(filePath))
        {
            return NotFound();
        }

        var content = System.IO.File.ReadAllText(filePath);
        ViewBag.Title = "Vérification PRDs";
        ViewBag.FileName = "VERIFICATION_PRDS.md";
        ViewBag.Content = content;
        return View("Markdown");
    }
}

