using G_StockVente.Domain.Models;
using G_StockVente.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace G_StockVente.Infrastructure.Data;

/// <summary>
/// Classe pour initialiser les données de base (rôles, admin, etc.)
/// </summary>
public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var services = scope.ServiceProvider;

        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var logger = services.GetRequiredService<ILogger<ApplicationDbContext>>();

        try
        {
            // Appliquer les migrations
            logger.LogInformation("Application des migrations de base de données...");
            await context.Database.MigrateAsync();
            logger.LogInformation("Migrations appliquées avec succès.");

            // Créer les rôles
            logger.LogInformation("Création des rôles...");
            await EnsureRolesAsync(roleManager, logger);

            // Créer l'administrateur réseau par défaut
            logger.LogInformation("Vérification du compte administrateur...");
            await EnsureAdminAsync(userManager, context, logger);

            logger.LogInformation("Initialisation des données de base terminée.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erreur lors de l'initialisation des données de base");
            throw;
        }
    }

    private static async Task EnsureRolesAsync(RoleManager<IdentityRole<Guid>> roleManager, ILogger logger)
    {
        string[] roles = { "AdminReseau", "ManagerBoutique", "Caissier", "GestionnaireStock" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var result = await roleManager.CreateAsync(new IdentityRole<Guid>(role));
                if (result.Succeeded)
                {
                    logger.LogInformation("Rôle '{Role}' créé avec succès.", role);
                }
                else
                {
                    logger.LogWarning("Échec de la création du rôle '{Role}': {Errors}", 
                        role, string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
            else
            {
                logger.LogDebug("Rôle '{Role}' existe déjà.", role);
            }
        }
    }

    private static async Task EnsureAdminAsync(
        UserManager<ApplicationUser> userManager, 
        ApplicationDbContext context, 
        ILogger logger)
    {
        const string adminEmail = "admin@gstockvente.com";
        const string adminPassword = "Admin123!";

        var admin = await userManager.FindByEmailAsync(adminEmail);
        if (admin == null)
        {
            logger.LogInformation("Création du compte administrateur par défaut...");
            
            admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                Prenom = "Administrateur",
                Nom = "Réseau",
                EstActif = true,
                EmailConfirmed = true,
                MustChangePassword = true, // Forcer le changement de mot de passe à la première connexion
                DateCreation = DateTime.UtcNow
            };

            // Le mot de passe sera automatiquement hashé par Identity avant stockage
            var result = await userManager.CreateAsync(admin, adminPassword);
            
            if (result.Succeeded)
            {
                logger.LogInformation("Compte administrateur créé avec succès: {Email}", adminEmail);
                
                // Ajouter le rôle AdminReseau
                var roleResult = await userManager.AddToRoleAsync(admin, "AdminReseau");
                if (roleResult.Succeeded)
                {
                    logger.LogInformation("Rôle 'AdminReseau' ajouté au compte administrateur.");
                }
                else
                {
                    logger.LogWarning("Échec de l'ajout du rôle 'AdminReseau': {Errors}",
                        string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                }
            }
            else
            {
                logger.LogError("Échec de la création du compte administrateur: {Errors}",
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
        else
        {
            logger.LogInformation("Compte administrateur existe déjà: {Email}", adminEmail);
            
            // Vérifier que l'admin a le rôle AdminReseau
            if (!await userManager.IsInRoleAsync(admin, "AdminReseau"))
            {
                logger.LogInformation("Ajout du rôle 'AdminReseau' au compte administrateur existant...");
                var roleResult = await userManager.AddToRoleAsync(admin, "AdminReseau");
                if (roleResult.Succeeded)
                {
                    logger.LogInformation("Rôle 'AdminReseau' ajouté avec succès.");
                }
            }
        }
    }
}

