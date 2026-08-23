using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DunorGames.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialStatblocks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GameSystems",
                columns: table => new
                {
                    Code = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameSystems", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "Statblocks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SchemaVersion = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    System = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Subtitle = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Origin = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    SourceSystem = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    SourceStatblockId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SourceReference = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false, defaultValue: "draft"),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SystemDataJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "CONVERT(datetimeoffset, SYSUTCDATETIME())"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "CONVERT(datetimeoffset, SYSUTCDATETIME())"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Statblocks", x => x.Id);
                    table.CheckConstraint("CK_Statblocks_Origin", "[Origin] IN ('manual', 'imported', 'converted')");
                    table.CheckConstraint("CK_Statblocks_SchemaVersion", "LEN([SchemaVersion]) > 0");
                    table.CheckConstraint("CK_Statblocks_Status", "[Status] IN ('draft', 'published', 'archived')");
                    table.CheckConstraint("CK_Statblocks_SystemDataJson", "ISJSON([SystemDataJson]) = 1");
                    table.ForeignKey(
                        name: "FK_Statblocks_GameSystems_SourceSystem",
                        column: x => x.SourceSystem,
                        principalTable: "GameSystems",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Statblocks_GameSystems_System",
                        column: x => x.System,
                        principalTable: "GameSystems",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Statblocks_Statblocks_SourceStatblockId",
                        column: x => x.SourceStatblockId,
                        principalTable: "Statblocks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StatblockTags",
                columns: table => new
                {
                    StatblockId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Tag = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatblockTags", x => new { x.StatblockId, x.Tag });
                    table.ForeignKey(
                        name: "FK_StatblockTags_Statblocks_StatblockId",
                        column: x => x.StatblockId,
                        principalTable: "Statblocks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "GameSystems",
                columns: new[] { "Code", "DisplayName", "SortOrder" },
                values: new object[,]
                {
                    { "daggerheart", "Daggerheart", 3 },
                    { "dc20", "DC20", 5 },
                    { "drawSteel", "Draw Steel", 4 },
                    { "dungeonsAndDragons", "Dungeons & Dragons", 1 },
                    { "talesOfTheValiant", "Tales of the Valiant", 2 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Statblocks_OwnerId_System_Status",
                table: "Statblocks",
                columns: new[] { "OwnerId", "System", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Statblocks_OwnerId_UpdatedAt",
                table: "Statblocks",
                columns: new[] { "OwnerId", "UpdatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Statblocks_SourceStatblockId",
                table: "Statblocks",
                column: "SourceStatblockId");

            migrationBuilder.CreateIndex(
                name: "IX_Statblocks_SourceSystem",
                table: "Statblocks",
                column: "SourceSystem");

            migrationBuilder.CreateIndex(
                name: "IX_Statblocks_System",
                table: "Statblocks",
                column: "System");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StatblockTags");

            migrationBuilder.DropTable(
                name: "Statblocks");

            migrationBuilder.DropTable(
                name: "GameSystems");
        }
    }
}
