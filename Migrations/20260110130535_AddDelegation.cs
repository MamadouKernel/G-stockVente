using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace G_StockVente.Migrations
{
    /// <inheritdoc />
    public partial class AddDelegation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Delegations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    UtilisateurDelegantId = table.Column<Guid>(type: "uuid", nullable: false),
                    UtilisateurBeneficiaireId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleDelege = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DateDebut = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateFin = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EstActive = table.Column<bool>(type: "boolean", nullable: false),
                    Raison = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    EstSupprime = table.Column<bool>(type: "boolean", nullable: false),
                    DateSuppression = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DateCreation = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateModification = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Delegations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Delegations_AspNetUsers_UtilisateurBeneficiaireId",
                        column: x => x.UtilisateurBeneficiaireId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Delegations_AspNetUsers_UtilisateurDelegantId",
                        column: x => x.UtilisateurDelegantId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Delegations_UtilisateurBeneficiaireId_EstActive_DateDebut_D~",
                table: "Delegations",
                columns: new[] { "UtilisateurBeneficiaireId", "EstActive", "DateDebut", "DateFin" });

            migrationBuilder.CreateIndex(
                name: "IX_Delegations_UtilisateurDelegantId_UtilisateurBeneficiaireId~",
                table: "Delegations",
                columns: new[] { "UtilisateurDelegantId", "UtilisateurBeneficiaireId", "RoleDelege", "DateDebut", "DateFin" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Delegations");
        }
    }
}
