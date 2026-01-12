using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace G_StockVente.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDeleteToUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Boutiques",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    Nom = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Adresse = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Telephone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Logo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    TvaParDefaut = table.Column<decimal>(type: "numeric", nullable: false),
                    EstActive = table.Column<bool>(type: "boolean", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateModification = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Boutiques", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    Nom = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CategorieParenteId = table.Column<Guid>(type: "uuid", nullable: true),
                    DateCreation = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Categories_Categories_CategorieParenteId",
                        column: x => x.CategorieParenteId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Fournisseurs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    Nom = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Adresse = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Telephone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    EstActif = table.Column<bool>(type: "boolean", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateModification = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fournisseurs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    Prenom = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Nom = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    BoutiqueActiveId = table.Column<Guid>(type: "uuid", nullable: true),
                    EstActif = table.Column<bool>(type: "boolean", nullable: false),
                    MustChangePassword = table.Column<bool>(type: "boolean", nullable: false),
                    EstSupprime = table.Column<bool>(type: "boolean", nullable: false),
                    DateSuppression = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DateCreation = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateDerniereConnexion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUsers_Boutiques_BoutiqueActiveId",
                        column: x => x.BoutiqueActiveId,
                        principalTable: "Boutiques",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Depots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    Nom = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Adresse = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    BoutiqueId = table.Column<Guid>(type: "uuid", nullable: false),
                    EstActif = table.Column<bool>(type: "boolean", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Depots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Depots_Boutiques_BoutiqueId",
                        column: x => x.BoutiqueId,
                        principalTable: "Boutiques",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Produits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    Nom = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CategorieId = table.Column<Guid>(type: "uuid", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateModification = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Produits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Produits_Categories_CategorieId",
                        column: x => x.CategorieId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Achats",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    NumeroAchat = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    NumeroFactureFournisseur = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    BoutiqueId = table.Column<Guid>(type: "uuid", nullable: false),
                    FournisseurId = table.Column<Guid>(type: "uuid", nullable: false),
                    Statut = table.Column<string>(type: "text", nullable: false),
                    DateCommande = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateReceptionPrevue = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DateReception = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MontantHT = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    MontantTVA = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    MontantTTC = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Commentaire = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    UtilisateurId = table.Column<Guid>(type: "uuid", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Achats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Achats_AspNetUsers_UtilisateurId",
                        column: x => x.UtilisateurId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Achats_Boutiques_BoutiqueId",
                        column: x => x.BoutiqueId,
                        principalTable: "Boutiques",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Achats_Fournisseurs_FournisseurId",
                        column: x => x.FournisseurId,
                        principalTable: "Fournisseurs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationUserBoutique",
                columns: table => new
                {
                    BoutiquesId = table.Column<Guid>(type: "uuid", nullable: false),
                    UtilisateursId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationUserBoutique", x => new { x.BoutiquesId, x.UtilisateursId });
                    table.ForeignKey(
                        name: "FK_ApplicationUserBoutique_AspNetUsers_UtilisateursId",
                        column: x => x.UtilisateursId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApplicationUserBoutique_Boutiques_BoutiquesId",
                        column: x => x.BoutiquesId,
                        principalTable: "Boutiques",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Ventes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    NumeroVente = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    NumeroFacture = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    BoutiqueId = table.Column<Guid>(type: "uuid", nullable: false),
                    Statut = table.Column<string>(type: "text", nullable: false),
                    MontantHT = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    MontantTVA = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    MontantTTC = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Remise = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ModePaiement = table.Column<string>(type: "text", nullable: false),
                    Commentaire = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    UtilisateurId = table.Column<Guid>(type: "uuid", nullable: false),
                    DateVente = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateAnnulation = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UtilisateurAnnulationId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ventes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ventes_AspNetUsers_UtilisateurAnnulationId",
                        column: x => x.UtilisateurAnnulationId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Ventes_AspNetUsers_UtilisateurId",
                        column: x => x.UtilisateurId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Ventes_Boutiques_BoutiqueId",
                        column: x => x.BoutiqueId,
                        principalTable: "Boutiques",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Inventaires",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    NumeroInventaire = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    BoutiqueId = table.Column<Guid>(type: "uuid", nullable: false),
                    DepotId = table.Column<Guid>(type: "uuid", nullable: false),
                    Statut = table.Column<string>(type: "text", nullable: false),
                    DateDebut = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateFinalisation = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Commentaire = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    UtilisateurId = table.Column<Guid>(type: "uuid", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inventaires", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Inventaires_AspNetUsers_UtilisateurId",
                        column: x => x.UtilisateurId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Inventaires_Boutiques_BoutiqueId",
                        column: x => x.BoutiqueId,
                        principalTable: "Boutiques",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Inventaires_Depots_DepotId",
                        column: x => x.DepotId,
                        principalTable: "Depots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProduitsBoutique",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    ProduitId = table.Column<Guid>(type: "uuid", nullable: false),
                    BoutiqueId = table.Column<Guid>(type: "uuid", nullable: false),
                    Sku = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CodeBarres = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PrixAchat = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    PrixVente = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    SeuilStockBas = table.Column<int>(type: "integer", nullable: false),
                    EstActif = table.Column<bool>(type: "boolean", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateModification = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProduitsBoutique", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProduitsBoutique_Boutiques_BoutiqueId",
                        column: x => x.BoutiqueId,
                        principalTable: "Boutiques",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProduitsBoutique_Produits_ProduitId",
                        column: x => x.ProduitId,
                        principalTable: "Produits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LignesAchat",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    AchatId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProduitBoutiqueId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuantiteCommandee = table.Column<int>(type: "integer", nullable: false),
                    QuantiteRecue = table.Column<int>(type: "integer", nullable: false),
                    PrixUnitaire = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TauxTVA = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    MontantHT = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    MontantTVA = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    MontantTTC = table.Column<decimal>(type: "numeric(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LignesAchat", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LignesAchat_Achats_AchatId",
                        column: x => x.AchatId,
                        principalTable: "Achats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LignesAchat_ProduitsBoutique_ProduitBoutiqueId",
                        column: x => x.ProduitBoutiqueId,
                        principalTable: "ProduitsBoutique",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LignesInventaire",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    InventaireId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProduitBoutiqueId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuantiteTheorique = table.Column<int>(type: "integer", nullable: false),
                    QuantiteReelle = table.Column<int>(type: "integer", nullable: false),
                    CommentaireEcart = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LignesInventaire", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LignesInventaire_Inventaires_InventaireId",
                        column: x => x.InventaireId,
                        principalTable: "Inventaires",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LignesInventaire_ProduitsBoutique_ProduitBoutiqueId",
                        column: x => x.ProduitBoutiqueId,
                        principalTable: "ProduitsBoutique",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LignesVente",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    VenteId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProduitBoutiqueId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantite = table.Column<int>(type: "integer", nullable: false),
                    PrixUnitaire = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Remise = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TauxTVA = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    MontantHT = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    MontantTVA = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    MontantTTC = table.Column<decimal>(type: "numeric(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LignesVente", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LignesVente_ProduitsBoutique_ProduitBoutiqueId",
                        column: x => x.ProduitBoutiqueId,
                        principalTable: "ProduitsBoutique",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LignesVente_Ventes_VenteId",
                        column: x => x.VenteId,
                        principalTable: "Ventes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MouvementsStock",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    ProduitBoutiqueId = table.Column<Guid>(type: "uuid", nullable: false),
                    DepotId = table.Column<Guid>(type: "uuid", nullable: false),
                    TypeMouvement = table.Column<string>(type: "text", nullable: false),
                    Quantite = table.Column<int>(type: "integer", nullable: false),
                    PrixUnitaire = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Commentaire = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    VenteId = table.Column<Guid>(type: "uuid", nullable: true),
                    AchatId = table.Column<Guid>(type: "uuid", nullable: true),
                    InventaireId = table.Column<Guid>(type: "uuid", nullable: true),
                    UtilisateurId = table.Column<Guid>(type: "uuid", nullable: false),
                    DateMouvement = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MouvementsStock", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MouvementsStock_Achats_AchatId",
                        column: x => x.AchatId,
                        principalTable: "Achats",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MouvementsStock_AspNetUsers_UtilisateurId",
                        column: x => x.UtilisateurId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MouvementsStock_Depots_DepotId",
                        column: x => x.DepotId,
                        principalTable: "Depots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MouvementsStock_Inventaires_InventaireId",
                        column: x => x.InventaireId,
                        principalTable: "Inventaires",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MouvementsStock_ProduitsBoutique_ProduitBoutiqueId",
                        column: x => x.ProduitBoutiqueId,
                        principalTable: "ProduitsBoutique",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MouvementsStock_Ventes_VenteId",
                        column: x => x.VenteId,
                        principalTable: "Ventes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Stocks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    ProduitBoutiqueId = table.Column<Guid>(type: "uuid", nullable: false),
                    DepotId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantite = table.Column<int>(type: "integer", nullable: false),
                    DateDerniereMaj = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stocks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Stocks_Depots_DepotId",
                        column: x => x.DepotId,
                        principalTable: "Depots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Stocks_ProduitsBoutique_ProduitBoutiqueId",
                        column: x => x.ProduitBoutiqueId,
                        principalTable: "ProduitsBoutique",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Achats_BoutiqueId_NumeroAchat",
                table: "Achats",
                columns: new[] { "BoutiqueId", "NumeroAchat" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Achats_FournisseurId",
                table: "Achats",
                column: "FournisseurId");

            migrationBuilder.CreateIndex(
                name: "IX_Achats_UtilisateurId",
                table: "Achats",
                column: "UtilisateurId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUserBoutique_UtilisateursId",
                table: "ApplicationUserBoutique",
                column: "UtilisateursId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_BoutiqueActiveId",
                table: "AspNetUsers",
                column: "BoutiqueActiveId");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Boutiques_Nom",
                table: "Boutiques",
                column: "Nom",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categories_CategorieParenteId",
                table: "Categories",
                column: "CategorieParenteId");

            migrationBuilder.CreateIndex(
                name: "IX_Depots_BoutiqueId_Nom",
                table: "Depots",
                columns: new[] { "BoutiqueId", "Nom" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Inventaires_BoutiqueId_NumeroInventaire",
                table: "Inventaires",
                columns: new[] { "BoutiqueId", "NumeroInventaire" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Inventaires_DepotId",
                table: "Inventaires",
                column: "DepotId");

            migrationBuilder.CreateIndex(
                name: "IX_Inventaires_UtilisateurId",
                table: "Inventaires",
                column: "UtilisateurId");

            migrationBuilder.CreateIndex(
                name: "IX_LignesAchat_AchatId",
                table: "LignesAchat",
                column: "AchatId");

            migrationBuilder.CreateIndex(
                name: "IX_LignesAchat_ProduitBoutiqueId",
                table: "LignesAchat",
                column: "ProduitBoutiqueId");

            migrationBuilder.CreateIndex(
                name: "IX_LignesInventaire_InventaireId",
                table: "LignesInventaire",
                column: "InventaireId");

            migrationBuilder.CreateIndex(
                name: "IX_LignesInventaire_ProduitBoutiqueId",
                table: "LignesInventaire",
                column: "ProduitBoutiqueId");

            migrationBuilder.CreateIndex(
                name: "IX_LignesVente_ProduitBoutiqueId",
                table: "LignesVente",
                column: "ProduitBoutiqueId");

            migrationBuilder.CreateIndex(
                name: "IX_LignesVente_VenteId",
                table: "LignesVente",
                column: "VenteId");

            migrationBuilder.CreateIndex(
                name: "IX_MouvementsStock_AchatId",
                table: "MouvementsStock",
                column: "AchatId");

            migrationBuilder.CreateIndex(
                name: "IX_MouvementsStock_DepotId",
                table: "MouvementsStock",
                column: "DepotId");

            migrationBuilder.CreateIndex(
                name: "IX_MouvementsStock_InventaireId",
                table: "MouvementsStock",
                column: "InventaireId");

            migrationBuilder.CreateIndex(
                name: "IX_MouvementsStock_ProduitBoutiqueId",
                table: "MouvementsStock",
                column: "ProduitBoutiqueId");

            migrationBuilder.CreateIndex(
                name: "IX_MouvementsStock_UtilisateurId",
                table: "MouvementsStock",
                column: "UtilisateurId");

            migrationBuilder.CreateIndex(
                name: "IX_MouvementsStock_VenteId",
                table: "MouvementsStock",
                column: "VenteId");

            migrationBuilder.CreateIndex(
                name: "IX_Produits_CategorieId",
                table: "Produits",
                column: "CategorieId");

            migrationBuilder.CreateIndex(
                name: "IX_ProduitsBoutique_BoutiqueId_CodeBarres",
                table: "ProduitsBoutique",
                columns: new[] { "BoutiqueId", "CodeBarres" },
                unique: true,
                filter: "\"CodeBarres\" IS NOT NULL AND \"CodeBarres\" != ''");

            migrationBuilder.CreateIndex(
                name: "IX_ProduitsBoutique_BoutiqueId_Sku",
                table: "ProduitsBoutique",
                columns: new[] { "BoutiqueId", "Sku" },
                unique: true,
                filter: "\"Sku\" IS NOT NULL AND \"Sku\" != ''");

            migrationBuilder.CreateIndex(
                name: "IX_ProduitsBoutique_ProduitId",
                table: "ProduitsBoutique",
                column: "ProduitId");

            migrationBuilder.CreateIndex(
                name: "IX_Stocks_DepotId",
                table: "Stocks",
                column: "DepotId");

            migrationBuilder.CreateIndex(
                name: "IX_Stocks_ProduitBoutiqueId_DepotId",
                table: "Stocks",
                columns: new[] { "ProduitBoutiqueId", "DepotId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ventes_BoutiqueId_NumeroVente",
                table: "Ventes",
                columns: new[] { "BoutiqueId", "NumeroVente" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ventes_UtilisateurAnnulationId",
                table: "Ventes",
                column: "UtilisateurAnnulationId");

            migrationBuilder.CreateIndex(
                name: "IX_Ventes_UtilisateurId",
                table: "Ventes",
                column: "UtilisateurId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApplicationUserBoutique");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "LignesAchat");

            migrationBuilder.DropTable(
                name: "LignesInventaire");

            migrationBuilder.DropTable(
                name: "LignesVente");

            migrationBuilder.DropTable(
                name: "MouvementsStock");

            migrationBuilder.DropTable(
                name: "Stocks");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "Achats");

            migrationBuilder.DropTable(
                name: "Inventaires");

            migrationBuilder.DropTable(
                name: "Ventes");

            migrationBuilder.DropTable(
                name: "ProduitsBoutique");

            migrationBuilder.DropTable(
                name: "Fournisseurs");

            migrationBuilder.DropTable(
                name: "Depots");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Produits");

            migrationBuilder.DropTable(
                name: "Boutiques");

            migrationBuilder.DropTable(
                name: "Categories");
        }
    }
}
