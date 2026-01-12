using G_StockVente.Domain.Models;
using G_StockVente.Infrastructure.Data;
using G_StockVente.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace G_StockVente.Infrastructure.BackgroundServices;

/// <summary>
/// Service en arrière-plan pour exécuter les rapports programmés automatiquement
/// </summary>
public class RapportProgrammeBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RapportProgrammeBackgroundService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(5); // Vérifie toutes les 5 minutes

    public RapportProgrammeBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<RapportProgrammeBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("RapportProgrammeBackgroundService démarré. Vérification toutes les {Interval} minutes.", _checkInterval.TotalMinutes);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ExecuteRapportsProgrammesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'exécution des rapports programmés");
            }

            // Attendre avant la prochaine vérification
            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("RapportProgrammeBackgroundService arrêté.");
    }

    private async Task ExecuteRapportsProgrammesAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var rapportService = scope.ServiceProvider.GetRequiredService<IRapportProgrammeService>();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Récupérer les rapports à exécuter
        var rapportsARapporter = await rapportService.GetARapporterAsync();

        if (rapportsARapporter.Count == 0)
        {
            _logger.LogDebug("Aucun rapport programmé à exécuter.");
            return;
        }

        _logger.LogInformation("Exécution de {Count} rapport(s) programmé(s).", rapportsARapporter.Count);

        foreach (var rapport in rapportsARapporter)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            try
            {
                await ExecuterRapportAsync(rapport, rapportService, emailService, context, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'exécution du rapport {RapportId} ({Type})", 
                    rapport.Id, rapport.TypeRapport);
            }
        }
    }

    private async Task ExecuterRapportAsync(
        RapportProgramme rapport,
        IRapportProgrammeService rapportService,
        IEmailService emailService,
        ApplicationDbContext context,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Exécution du rapport {RapportId} de type {Type} pour la boutique {BoutiqueId}", 
            rapport.Id, rapport.TypeRapport, rapport.BoutiqueId);

        bool emailEnvoye = false;

        try
        {
            // Générer le rapport HTML selon le type
            string rapportHtml = await GenererRapportHtmlAsync(rapport, context, cancellationToken);

            // Construire le sujet de l'email
            var sujet = ConstruireSujetEmail(rapport);

            // Envoyer le rapport par email
            emailEnvoye = await emailService.SendReportAsync(
                rapport.EmailDestinataire,
                sujet,
                rapportHtml,
                null, // Pas de pièce jointe pour l'instant
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'envoi du rapport {RapportId} par email", rapport.Id);
            emailEnvoye = false;
        }

        if (emailEnvoye)
        {
            // Marquer comme exécuté
            await rapportService.MarquerCommeExecuteAsync(rapport.Id);
            _logger.LogInformation("Rapport {RapportId} exécuté et envoyé avec succès à {Email}", 
                rapport.Id, rapport.EmailDestinataire);
        }
        else
        {
            _logger.LogWarning("Échec de l'envoi du rapport {RapportId} à {Email}", 
                rapport.Id, rapport.EmailDestinataire);
        }
    }

    private async Task<string> GenererRapportHtmlAsync(
        RapportProgramme rapport,
        ApplicationDbContext context,
        CancellationToken cancellationToken)
    {
        var maintenant = DateTime.UtcNow;
        DateTime dateDebut, dateFin;

        // Déterminer la période selon la fréquence
        switch (rapport.Frequence)
        {
            case FrequenceRapport.Journalier:
                dateDebut = maintenant.Date;
                dateFin = maintenant.Date.AddDays(1).AddTicks(-1);
                break;
            case FrequenceRapport.Hebdomadaire:
                var debutSemaine = maintenant.AddDays(-(int)maintenant.DayOfWeek);
                dateDebut = debutSemaine.Date;
                dateFin = dateDebut.AddDays(7).AddTicks(-1);
                break;
            case FrequenceRapport.Mensuel:
                dateDebut = new DateTime(maintenant.Year, maintenant.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                dateFin = dateDebut.AddMonths(1).AddTicks(-1);
                break;
            default:
                dateDebut = maintenant.Date;
                dateFin = maintenant.Date.AddDays(1).AddTicks(-1);
                break;
        }

        var html = new System.Text.StringBuilder();

        switch (rapport.TypeRapport)
        {
            case TypeRapport.Ventes:
                html.Append(await GenererRapportVentesAsync(rapport.BoutiqueId, dateDebut, dateFin, context, cancellationToken));
                break;
            case TypeRapport.Stocks:
                html.Append(await GenererRapportStocksAsync(rapport.BoutiqueId, context, cancellationToken));
                break;
            case TypeRapport.Achats:
                html.Append(await GenererRapportAchatsAsync(rapport.BoutiqueId, dateDebut, dateFin, context, cancellationToken));
                break;
            case TypeRapport.Consolidé:
                html.Append(await GenererRapportConsolideAsync(dateDebut, dateFin, context, cancellationToken));
                break;
        }

        return html.ToString();
    }

    private async Task<string> GenererRapportVentesAsync(
        Guid? boutiqueId,
        DateTime dateDebut,
        DateTime dateFin,
        ApplicationDbContext context,
        CancellationToken cancellationToken)
    {
        var query = context.Ventes
            .Where(v => v.DateVente >= dateDebut && v.DateVente <= dateFin && v.Statut == StatutVente.Validee);

        if (boutiqueId.HasValue)
        {
            query = query.Where(v => v.BoutiqueId == boutiqueId.Value);
        }

        var ventes = await query
            .Include(v => v.Boutique)
            .Include(v => v.LignesVente)
                .ThenInclude(lv => lv.ProduitBoutique)
                    .ThenInclude(pb => pb!.Produit)
            .ToListAsync(cancellationToken);

        var totalCA = ventes.Sum(v => v.MontantTTC);
        var nombreVentes = ventes.Count;
        var nombreArticles = ventes.Sum(v => v.LignesVente.Sum(lv => lv.Quantite));

        var html = new System.Text.StringBuilder();
        html.AppendLine("<h2>Rapport des Ventes</h2>");
        html.AppendLine($"<p><strong>Période :</strong> {dateDebut:dd/MM/yyyy} au {dateFin:dd/MM/yyyy}</p>");
        html.AppendLine($"<p><strong>Nombre de ventes :</strong> {nombreVentes}</p>");
        html.AppendLine($"<p><strong>Nombre d'articles vendus :</strong> {nombreArticles}</p>");
        html.AppendLine($"<p><strong>Chiffre d'affaires total (TTC) :</strong> {totalCA:N2} €</p>");

        html.AppendLine("<h3>Détail des ventes</h3>");
        html.AppendLine("<table border='1' cellpadding='5' cellspacing='0' style='width:100%; border-collapse:collapse;'>");
        html.AppendLine("<thead><tr><th>Date</th><th>N°</th><th>Boutique</th><th>Montant TTC</th></tr></thead>");
        html.AppendLine("<tbody>");

        foreach (var vente in ventes.OrderByDescending(v => v.DateVente).Take(50))
        {
            html.AppendLine($"<tr>");
            html.AppendLine($"<td>{vente.DateVente:dd/MM/yyyy HH:mm}</td>");
            html.AppendLine($"<td>{vente.NumeroVente}</td>");
            html.AppendLine($"<td>{vente.Boutique?.Nom ?? "N/A"}</td>");
            html.AppendLine($"<td>{vente.MontantTTC:N2} €</td>");
            html.AppendLine($"</tr>");
        }

        html.AppendLine("</tbody></table>");
        return html.ToString();
    }

    private async Task<string> GenererRapportStocksAsync(
        Guid? boutiqueId,
        ApplicationDbContext context,
        CancellationToken cancellationToken)
    {
        var query = context.Stocks
            .Include(s => s.ProduitBoutique)
                .ThenInclude(pb => pb.Produit)
            .Include(s => s.Depot)
                .ThenInclude(d => d.Boutique)
            .AsQueryable();

        if (boutiqueId.HasValue)
        {
            query = query.Where(s => s.Depot.BoutiqueId == boutiqueId.Value);
        }

        var stocks = await query.ToListAsync(cancellationToken);
        var stocksBas = stocks.Where(s => s.Quantite <= (s.ProduitBoutique?.SeuilStockBas ?? 0)).ToList();

        var html = new System.Text.StringBuilder();
        html.AppendLine("<h2>Rapport des Stocks</h2>");
        html.AppendLine($"<p><strong>Nombre total de produits en stock :</strong> {stocks.Count}</p>");
        html.AppendLine($"<p><strong>Produits en stock bas :</strong> {stocksBas.Count}</p>");

        html.AppendLine("<h3>Produits en stock bas</h3>");
        html.AppendLine("<table border='1' cellpadding='5' cellspacing='0' style='width:100%; border-collapse:collapse;'>");
        html.AppendLine("<thead><tr><th>Produit</th><th>Dépôt</th><th>Quantité</th><th>Seuil</th></tr></thead>");
        html.AppendLine("<tbody>");

        foreach (var stock in stocksBas.Take(50))
        {
            html.AppendLine($"<tr>");
            html.AppendLine($"<td>{stock.ProduitBoutique?.Produit?.Nom ?? "N/A"}</td>");
            html.AppendLine($"<td>{stock.Depot?.Nom ?? "N/A"}</td>");
            html.AppendLine($"<td>{stock.Quantite}</td>");
            html.AppendLine($"<td>{stock.ProduitBoutique?.SeuilStockBas ?? 0}</td>");
            html.AppendLine($"</tr>");
        }

        html.AppendLine("</tbody></table>");
        return html.ToString();
    }

    private async Task<string> GenererRapportAchatsAsync(
        Guid? boutiqueId,
        DateTime dateDebut,
        DateTime dateFin,
        ApplicationDbContext context,
        CancellationToken cancellationToken)
    {
        var query = context.Achats
            .Where(a => a.DateCommande >= dateDebut && a.DateCommande <= dateFin);

        if (boutiqueId.HasValue)
        {
            query = query.Where(a => a.BoutiqueId == boutiqueId.Value);
        }

        var achats = await query
            .Include(a => a.Boutique)
            .Include(a => a.Fournisseur)
            .ToListAsync(cancellationToken);

        var totalAchats = achats.Sum(a => a.MontantTTC);
        var nombreAchats = achats.Count;

        var html = new System.Text.StringBuilder();
        html.AppendLine("<h2>Rapport des Achats</h2>");
        html.AppendLine($"<p><strong>Période :</strong> {dateDebut:dd/MM/yyyy} au {dateFin:dd/MM/yyyy}</p>");
        html.AppendLine($"<p><strong>Nombre d'achats :</strong> {nombreAchats}</p>");
        html.AppendLine($"<p><strong>Total des achats (TTC) :</strong> {totalAchats:N2} €</p>");

        html.AppendLine("<h3>Détail des achats</h3>");
        html.AppendLine("<table border='1' cellpadding='5' cellspacing='0' style='width:100%; border-collapse:collapse;'>");
        html.AppendLine("<thead><tr><th>Date</th><th>N°</th><th>Boutique</th><th>Fournisseur</th><th>Montant TTC</th></tr></thead>");
        html.AppendLine("<tbody>");

        foreach (var achat in achats.OrderByDescending(a => a.DateCommande).Take(50))
        {
            html.AppendLine($"<tr>");
            html.AppendLine($"<td>{achat.DateCommande:dd/MM/yyyy HH:mm}</td>");
            html.AppendLine($"<td>{achat.NumeroAchat}</td>");
            html.AppendLine($"<td>{achat.Boutique?.Nom ?? "N/A"}</td>");
            html.AppendLine($"<td>{achat.Fournisseur?.Nom ?? "N/A"}</td>");
                            html.AppendLine($"<td>{achat.MontantTTC:N2} €</td>");
            html.AppendLine($"</tr>");
        }

        html.AppendLine("</tbody></table>");
        return html.ToString();
    }

    private async Task<string> GenererRapportConsolideAsync(
        DateTime dateDebut,
        DateTime dateFin,
        ApplicationDbContext context,
        CancellationToken cancellationToken)
    {
        var ventes = await context.Ventes
            .Where(v => v.DateVente >= dateDebut && v.DateVente <= dateFin && v.Statut == StatutVente.Validee)
            .Include(v => v.Boutique)
            .ToListAsync(cancellationToken);

        var achats = await context.Achats
            .Where(a => a.DateCommande >= dateDebut && a.DateCommande <= dateFin)
            .Include(a => a.Boutique)
            .ToListAsync(cancellationToken);

        var totalCA = ventes.Sum(v => v.MontantTTC);
        var totalAchats = achats.Sum(a => a.MontantTTC);

        var html = new System.Text.StringBuilder();
        html.AppendLine("<h2>Rapport Consolidé Réseau</h2>");
        html.AppendLine($"<p><strong>Période :</strong> {dateDebut:dd/MM/yyyy} au {dateFin:dd/MM/yyyy}</p>");
        html.AppendLine($"<p><strong>Chiffre d'affaires total :</strong> {totalCA:N2} €</p>");
        html.AppendLine($"<p><strong>Total des achats :</strong> {totalAchats:N2} €</p>");
        html.AppendLine($"<p><strong>Marge brute :</strong> {(totalCA - totalAchats):N2} €</p>");

        html.AppendLine("<h3>Par boutique</h3>");
        html.AppendLine("<table border='1' cellpadding='5' cellspacing='0' style='width:100%; border-collapse:collapse;'>");
        html.AppendLine("<thead><tr><th>Boutique</th><th>CA</th><th>Achats</th><th>Marge</th></tr></thead>");
        html.AppendLine("<tbody>");

        var boutiques = await context.Boutiques.ToListAsync(cancellationToken);
        foreach (var boutique in boutiques)
        {
            var caBoutique = ventes.Where(v => v.BoutiqueId == boutique.Id).Sum(v => v.MontantTTC);
            var achatsBoutique = achats.Where(a => a.BoutiqueId == boutique.Id).Sum(a => a.MontantTTC);
            var marge = caBoutique - achatsBoutique;

            html.AppendLine($"<tr>");
            html.AppendLine($"<td>{boutique.Nom}</td>");
            html.AppendLine($"<td>{caBoutique:N2} €</td>");
            html.AppendLine($"<td>{achatsBoutique:N2} €</td>");
            html.AppendLine($"<td>{marge:N2} €</td>");
            html.AppendLine($"</tr>");
        }

        html.AppendLine("</tbody></table>");
        return html.ToString();
    }

    private string ConstruireSujetEmail(RapportProgramme rapport)
    {
        var type = rapport.TypeRapport switch
        {
            TypeRapport.Ventes => "Ventes",
            TypeRapport.Stocks => "Stocks",
            TypeRapport.Achats => "Achats",
            TypeRapport.Consolidé => "Consolidé Réseau",
            _ => "Rapport"
        };

        var frequence = rapport.Frequence switch
        {
            FrequenceRapport.Journalier => "Quotidien",
            FrequenceRapport.Hebdomadaire => "Hebdomadaire",
            FrequenceRapport.Mensuel => "Mensuel",
            _ => ""
        };

        return $"Rapport {frequence} - {type} - {DateTime.UtcNow:dd/MM/yyyy}";
    }
}

