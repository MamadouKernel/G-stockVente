using G_StockVente.Domain.Models;
using G_StockVente.Infrastructure.Data;
using G_StockVente.Infrastructure.Services;
using G_StockVente.Infrastructure.BackgroundServices;
using G_StockVente.Infrastructure.Filters;
using G_StockVente.Hubs;
using G_StockVente.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configuration de la base de données PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("La chaîne de connexion 'DefaultConnection' est introuvable.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Configuration de l'authentification Identity avec GUID
// NOTE: Les mots de passe sont automatiquement hashés par Identity avec PBKDF2
builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
{
    // Configuration des mots de passe
    // Les mots de passe sont automatiquement hashés avec PBKDF2 (PasswordHasher)
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;

    // Configuration de l'utilisateur
    options.User.RequireUniqueEmail = true;

    // Configuration de la connexion
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configuration des cookies d'authentification
// L'authentification se fait via des cookies HTTP sécurisés
builder.Services.ConfigureApplicationCookie(options =>
{
    // Chemins
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    
    // Durée de vie du cookie
    options.ExpireTimeSpan = TimeSpan.FromHours(1); // 1 heure d'inactivité avant déconnexion
    options.SlidingExpiration = true; // Renouvelle le cookie à chaque activité
    
    // Sécurité des cookies
    options.Cookie.HttpOnly = true; // Empêche l'accès JavaScript (protection XSS)
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // HTTPS en production
    options.Cookie.SameSite = SameSiteMode.Lax; // Protection CSRF
    options.Cookie.Name = "GStockVente.Auth"; // Nom du cookie
    
    // Protection des données du cookie
    options.Cookie.IsEssential = true; // Nécessaire pour le fonctionnement
    
    // Timeout si inactif
    options.SlidingExpiration = true;
    
    // Permet de retourner au formulaire de connexion après expiration
    options.ReturnUrlParameter = "returnUrl";
});

// Configuration Email Settings
builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection(EmailSettings.SectionName));
builder.Services.AddScoped<IEmailService, EmailService>();

// Services personnalisés
builder.Services.AddScoped<IBoutiqueActiveService, BoutiqueActiveService>();
builder.Services.AddScoped<ITransfertStockService, TransfertStockService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IRapportProgrammeService, RapportProgrammeService>();
builder.Services.AddScoped<IPaiementIntegreService, PaiementIntegreService>();
builder.Services.AddScoped<IJournalConnexionService, JournalConnexionService>();
builder.Services.AddScoped<IDelegationService, DelegationService>();
builder.Services.AddHttpContextAccessor();

// Add services to the container.
builder.Services.AddControllersWithViews(options =>
{
    // Ajouter le filtre pour charger automatiquement la boutique active
    options.Filters.Add<LoadBoutiqueActiveFilter>();
});

// Configuration SignalR pour notifications temps réel
builder.Services.AddSignalR();

// Background Services
builder.Services.AddHostedService<RapportProgrammeBackgroundService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// Mapper le Hub SignalR pour les notifications
app.MapHub<NotificationHub>("/notificationHub");

// Initialiser les données de base
using (var scope = app.Services.CreateScope())
{
    await SeedData.InitializeAsync(scope.ServiceProvider);
}

app.Run();
