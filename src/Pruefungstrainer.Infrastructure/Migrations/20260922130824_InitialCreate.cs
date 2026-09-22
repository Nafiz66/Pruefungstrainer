using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pruefungstrainer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Benutzer",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Vorname = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Nachname = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: false),
                    Rolle = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    ErstelltAm = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Benutzer", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Themengebiete",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Beschreibung = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    Kategorie = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Themengebiete", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Fragen",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Fragetext = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    Typ = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Punkte = table.Column<int>(type: "INTEGER", nullable: false),
                    MusterloesungFreitext = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    ThemengebietId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fragen", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Fragen_Themengebiete_ThemengebietId",
                        column: x => x.ThemengebietId,
                        principalTable: "Themengebiete",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Karteikarten",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Frage = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Antwort = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    ThemengebietId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Karteikarten", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Karteikarten_Themengebiete_ThemengebietId",
                        column: x => x.ThemengebietId,
                        principalTable: "Themengebiete",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Lernpfade",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Titel = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Beschreibung = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    ThemengebietId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lernpfade", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Lernpfade_Themengebiete_ThemengebietId",
                        column: x => x.ThemengebietId,
                        principalTable: "Themengebiete",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Antwortoptionen",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Text = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    IstKorrekt = table.Column<bool>(type: "INTEGER", nullable: false),
                    FrageId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Antwortoptionen", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Antwortoptionen_Fragen_FrageId",
                        column: x => x.FrageId,
                        principalTable: "Fragen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KarteikartenFortschritte",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ApplicationUserId = table.Column<int>(type: "INTEGER", nullable: false),
                    KarteikarteId = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    WiederholungenAnzahl = table.Column<int>(type: "INTEGER", nullable: false),
                    LetzteBearbeitungAm = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KarteikartenFortschritte", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KarteikartenFortschritte_Benutzer_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "Benutzer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_KarteikartenFortschritte_Karteikarten_KarteikarteId",
                        column: x => x.KarteikarteId,
                        principalTable: "Karteikarten",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LernpfadElemente",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LernpfadId = table.Column<int>(type: "INTEGER", nullable: false),
                    Reihenfolge = table.Column<int>(type: "INTEGER", nullable: false),
                    Typ = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    KarteikarteId = table.Column<int>(type: "INTEGER", nullable: true),
                    FrageId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LernpfadElemente", x => x.Id);
                    table.CheckConstraint("CK_LernpfadElement_GenauEinInhalt", "(Typ = 'Karteikarte' AND KarteikarteId IS NOT NULL AND FrageId IS NULL) OR (Typ = 'Frage' AND FrageId IS NOT NULL AND KarteikarteId IS NULL)");
                    table.ForeignKey(
                        name: "FK_LernpfadElemente_Fragen_FrageId",
                        column: x => x.FrageId,
                        principalTable: "Fragen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LernpfadElemente_Karteikarten_KarteikarteId",
                        column: x => x.KarteikarteId,
                        principalTable: "Karteikarten",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LernpfadElemente_Lernpfade_LernpfadId",
                        column: x => x.LernpfadId,
                        principalTable: "Lernpfade",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Pruefungsergebnisse",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ApplicationUserId = table.Column<int>(type: "INTEGER", nullable: false),
                    LernpfadId = table.Column<int>(type: "INTEGER", nullable: true),
                    DurchgefuehrtAm = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ErreichtePunkte = table.Column<int>(type: "INTEGER", nullable: false),
                    MaxPunkte = table.Column<int>(type: "INTEGER", nullable: false),
                    Bestanden = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pruefungsergebnisse", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pruefungsergebnisse_Benutzer_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "Benutzer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Pruefungsergebnisse_Lernpfade_LernpfadId",
                        column: x => x.LernpfadId,
                        principalTable: "Lernpfade",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "PruefungsergebnisFragen",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PruefungsergebnisId = table.Column<int>(type: "INTEGER", nullable: false),
                    FrageId = table.Column<int>(type: "INTEGER", nullable: false),
                    GegebeneAntwort = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    IstKorrekt = table.Column<bool>(type: "INTEGER", nullable: false),
                    ErreichtePunkte = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PruefungsergebnisFragen", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PruefungsergebnisFragen_Fragen_FrageId",
                        column: x => x.FrageId,
                        principalTable: "Fragen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PruefungsergebnisFragen_Pruefungsergebnisse_PruefungsergebnisId",
                        column: x => x.PruefungsergebnisId,
                        principalTable: "Pruefungsergebnisse",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Antwortoptionen_FrageId",
                table: "Antwortoptionen",
                column: "FrageId");

            migrationBuilder.CreateIndex(
                name: "IX_Benutzer_Email",
                table: "Benutzer",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Fragen_ThemengebietId",
                table: "Fragen",
                column: "ThemengebietId");

            migrationBuilder.CreateIndex(
                name: "IX_Karteikarten_ThemengebietId",
                table: "Karteikarten",
                column: "ThemengebietId");

            migrationBuilder.CreateIndex(
                name: "IX_KarteikartenFortschritte_ApplicationUserId_KarteikarteId",
                table: "KarteikartenFortschritte",
                columns: new[] { "ApplicationUserId", "KarteikarteId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KarteikartenFortschritte_KarteikarteId",
                table: "KarteikartenFortschritte",
                column: "KarteikarteId");

            migrationBuilder.CreateIndex(
                name: "IX_Lernpfade_ThemengebietId",
                table: "Lernpfade",
                column: "ThemengebietId");

            migrationBuilder.CreateIndex(
                name: "IX_LernpfadElemente_FrageId",
                table: "LernpfadElemente",
                column: "FrageId");

            migrationBuilder.CreateIndex(
                name: "IX_LernpfadElemente_KarteikarteId",
                table: "LernpfadElemente",
                column: "KarteikarteId");

            migrationBuilder.CreateIndex(
                name: "IX_LernpfadElemente_LernpfadId_Reihenfolge",
                table: "LernpfadElemente",
                columns: new[] { "LernpfadId", "Reihenfolge" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PruefungsergebnisFragen_FrageId",
                table: "PruefungsergebnisFragen",
                column: "FrageId");

            migrationBuilder.CreateIndex(
                name: "IX_PruefungsergebnisFragen_PruefungsergebnisId_FrageId",
                table: "PruefungsergebnisFragen",
                columns: new[] { "PruefungsergebnisId", "FrageId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pruefungsergebnisse_ApplicationUserId_DurchgefuehrtAm",
                table: "Pruefungsergebnisse",
                columns: new[] { "ApplicationUserId", "DurchgefuehrtAm" });

            migrationBuilder.CreateIndex(
                name: "IX_Pruefungsergebnisse_LernpfadId",
                table: "Pruefungsergebnisse",
                column: "LernpfadId");

            migrationBuilder.CreateIndex(
                name: "IX_Themengebiete_Name",
                table: "Themengebiete",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Antwortoptionen");

            migrationBuilder.DropTable(
                name: "KarteikartenFortschritte");

            migrationBuilder.DropTable(
                name: "LernpfadElemente");

            migrationBuilder.DropTable(
                name: "PruefungsergebnisFragen");

            migrationBuilder.DropTable(
                name: "Karteikarten");

            migrationBuilder.DropTable(
                name: "Fragen");

            migrationBuilder.DropTable(
                name: "Pruefungsergebnisse");

            migrationBuilder.DropTable(
                name: "Benutzer");

            migrationBuilder.DropTable(
                name: "Lernpfade");

            migrationBuilder.DropTable(
                name: "Themengebiete");
        }
    }
}
