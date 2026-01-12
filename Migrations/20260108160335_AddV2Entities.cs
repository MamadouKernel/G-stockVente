using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace G_StockVente.Migrations
{
    /// <inheritdoc />
    public partial class AddV2Entities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TransfertStockId",
                table: "MouvementsStock",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "JournalConnexions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    UtilisateurId = table.Column<Guid>(type: "uuid", nullable: true),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    AdresseIP = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Succes = table.Column<bool>(type: "boolean", nullable: false),
                    RaisonEchec = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DateTentative = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JournalConnexions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JournalConnexions_AspNetUsers_UtilisateurId",
                        column: x => x.UtilisateurId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    UtilisateurId = table.Column<Guid>(type: "uuid", nullable: true),
                    BoutiqueId = table.Column<Guid>(type: "uuid", nullable: true),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Titre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    LienAction = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    EstLue = table.Column<bool>(type: "boolean", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateLecture = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_AspNetUsers_UtilisateurId",
                        column: x => x.UtilisateurId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Notifications_Boutiques_BoutiqueId",
                        column: x => x.BoutiqueId,
                        principalTable: "Boutiques",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "PaiementsIntegres",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    VenteId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    ReferenceExterne = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Montant = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Statut = table.Column<string>(type: "text", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateValidation = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DonneesReponse = table.Column<string>(type: "text", nullable: true),
                    MessageErreur = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaiementsIntegres", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaiementsIntegres_Ventes_VenteId",
                        column: x => x.VenteId,
                        principalTable: "Ventes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RapportsProgrammes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    BoutiqueId = table.Column<Guid>(type: "uuid", nullable: true),
                    TypeRapport = table.Column<string>(type: "text", nullable: false),
                    Frequence = table.Column<string>(type: "text", nullable: false),
                    EmailDestinataire = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    EstActif = table.Column<bool>(type: "boolean", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DerniereExecution = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ProchaineExecution = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UtilisateurCreateurId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RapportsProgrammes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RapportsProgrammes_AspNetUsers_UtilisateurCreateurId",
                        column: x => x.UtilisateurCreateurId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RapportsProgrammes_Boutiques_BoutiqueId",
                        column: x => x.BoutiqueId,
                        principalTable: "Boutiques",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "TransfertsStock",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    Numero = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DepotSourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    BoutiqueSourceId = table.Column<Guid>(type: "uuid", nullable: true),
                    DepotDestinationId = table.Column<Guid>(type: "uuid", nullable: false),
                    BoutiqueDestinationId = table.Column<Guid>(type: "uuid", nullable: true),
                    Statut = table.Column<string>(type: "text", nullable: false),
                    UtilisateurCreateurId = table.Column<Guid>(type: "uuid", nullable: false),
                    UtilisateurValidateurId = table.Column<Guid>(type: "uuid", nullable: true),
                    UtilisateurRecepteurId = table.Column<Guid>(type: "uuid", nullable: true),
                    DateCreation = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateValidation = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DateReception = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DateAnnulation = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransfertsStock", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransfertsStock_AspNetUsers_UtilisateurCreateurId",
                        column: x => x.UtilisateurCreateurId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TransfertsStock_AspNetUsers_UtilisateurRecepteurId",
                        column: x => x.UtilisateurRecepteurId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TransfertsStock_AspNetUsers_UtilisateurValidateurId",
                        column: x => x.UtilisateurValidateurId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TransfertsStock_Boutiques_BoutiqueDestinationId",
                        column: x => x.BoutiqueDestinationId,
                        principalTable: "Boutiques",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TransfertsStock_Boutiques_BoutiqueSourceId",
                        column: x => x.BoutiqueSourceId,
                        principalTable: "Boutiques",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TransfertsStock_Depots_DepotDestinationId",
                        column: x => x.DepotDestinationId,
                        principalTable: "Depots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TransfertsStock_Depots_DepotSourceId",
                        column: x => x.DepotSourceId,
                        principalTable: "Depots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LignesTransfertStock",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    TransfertStockId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProduitBoutiqueId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantite = table.Column<int>(type: "integer", nullable: false),
                    QuantiteRecue = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LignesTransfertStock", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LignesTransfertStock_ProduitsBoutique_ProduitBoutiqueId",
                        column: x => x.ProduitBoutiqueId,
                        principalTable: "ProduitsBoutique",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LignesTransfertStock_TransfertsStock_TransfertStockId",
                        column: x => x.TransfertStockId,
                        principalTable: "TransfertsStock",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MouvementsStock_TransfertStockId",
                table: "MouvementsStock",
                column: "TransfertStockId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalConnexions_Email_DateTentative",
                table: "JournalConnexions",
                columns: new[] { "Email", "DateTentative" });

            migrationBuilder.CreateIndex(
                name: "IX_JournalConnexions_UtilisateurId_DateTentative",
                table: "JournalConnexions",
                columns: new[] { "UtilisateurId", "DateTentative" });

            migrationBuilder.CreateIndex(
                name: "IX_LignesTransfertStock_ProduitBoutiqueId",
                table: "LignesTransfertStock",
                column: "ProduitBoutiqueId");

            migrationBuilder.CreateIndex(
                name: "IX_LignesTransfertStock_TransfertStockId",
                table: "LignesTransfertStock",
                column: "TransfertStockId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_BoutiqueId",
                table: "Notifications",
                column: "BoutiqueId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UtilisateurId_EstLue_DateCreation",
                table: "Notifications",
                columns: new[] { "UtilisateurId", "EstLue", "DateCreation" });

            migrationBuilder.CreateIndex(
                name: "IX_PaiementsIntegres_ReferenceExterne",
                table: "PaiementsIntegres",
                column: "ReferenceExterne");

            migrationBuilder.CreateIndex(
                name: "IX_PaiementsIntegres_VenteId",
                table: "PaiementsIntegres",
                column: "VenteId");

            migrationBuilder.CreateIndex(
                name: "IX_RapportsProgrammes_BoutiqueId",
                table: "RapportsProgrammes",
                column: "BoutiqueId");

            migrationBuilder.CreateIndex(
                name: "IX_RapportsProgrammes_UtilisateurCreateurId",
                table: "RapportsProgrammes",
                column: "UtilisateurCreateurId");

            migrationBuilder.CreateIndex(
                name: "IX_TransfertsStock_BoutiqueDestinationId",
                table: "TransfertsStock",
                column: "BoutiqueDestinationId");

            migrationBuilder.CreateIndex(
                name: "IX_TransfertsStock_BoutiqueSourceId_Numero",
                table: "TransfertsStock",
                columns: new[] { "BoutiqueSourceId", "Numero" },
                unique: true,
                filter: "\"BoutiqueSourceId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TransfertsStock_DepotDestinationId",
                table: "TransfertsStock",
                column: "DepotDestinationId");

            migrationBuilder.CreateIndex(
                name: "IX_TransfertsStock_DepotSourceId",
                table: "TransfertsStock",
                column: "DepotSourceId");

            migrationBuilder.CreateIndex(
                name: "IX_TransfertsStock_UtilisateurCreateurId",
                table: "TransfertsStock",
                column: "UtilisateurCreateurId");

            migrationBuilder.CreateIndex(
                name: "IX_TransfertsStock_UtilisateurRecepteurId",
                table: "TransfertsStock",
                column: "UtilisateurRecepteurId");

            migrationBuilder.CreateIndex(
                name: "IX_TransfertsStock_UtilisateurValidateurId",
                table: "TransfertsStock",
                column: "UtilisateurValidateurId");

            migrationBuilder.AddForeignKey(
                name: "FK_MouvementsStock_TransfertsStock_TransfertStockId",
                table: "MouvementsStock",
                column: "TransfertStockId",
                principalTable: "TransfertsStock",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MouvementsStock_TransfertsStock_TransfertStockId",
                table: "MouvementsStock");

            migrationBuilder.DropTable(
                name: "JournalConnexions");

            migrationBuilder.DropTable(
                name: "LignesTransfertStock");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "PaiementsIntegres");

            migrationBuilder.DropTable(
                name: "RapportsProgrammes");

            migrationBuilder.DropTable(
                name: "TransfertsStock");

            migrationBuilder.DropIndex(
                name: "IX_MouvementsStock_TransfertStockId",
                table: "MouvementsStock");

            migrationBuilder.DropColumn(
                name: "TransfertStockId",
                table: "MouvementsStock");
        }
    }
}
