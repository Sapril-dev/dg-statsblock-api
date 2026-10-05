using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DunorGames.Data.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDaggerheartReferenceValues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReferenceValues",
                columns: table => new
                {
                    SystemCode = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReferenceValues", x => new { x.SystemCode, x.Category, x.Code });
                    table.ForeignKey(
                        name: "FK_ReferenceValues_GameSystems_SystemCode",
                        column: x => x.SystemCode,
                        principalTable: "GameSystems",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "ReferenceValues",
                columns: new[] { "Category", "Code", "SystemCode", "DisplayName", "IsActive", "SortOrder" },
                values: new object[,]
                {
                    { "adversaryType", "bruiser", "daggerheart", "Bruiser", true, 1 },
                    { "adversaryType", "horde", "daggerheart", "Horde", true, 2 },
                    { "adversaryType", "leader", "daggerheart", "Leader", true, 3 },
                    { "adversaryType", "minion", "daggerheart", "Minion", true, 4 },
                    { "adversaryType", "ranged", "daggerheart", "Ranged", true, 5 },
                    { "adversaryType", "skulk", "daggerheart", "Skulk", true, 6 },
                    { "adversaryType", "social", "daggerheart", "Social", true, 7 },
                    { "adversaryType", "solo", "daggerheart", "Solo", true, 8 },
                    { "adversaryType", "standard", "daggerheart", "Standard", true, 9 },
                    { "adversaryType", "support", "daggerheart", "Support", true, 10 },
                    { "attackRange", "close", "daggerheart", "Close", true, 3 },
                    { "attackRange", "far", "daggerheart", "Far", true, 4 },
                    { "attackRange", "melee", "daggerheart", "Melee", true, 1 },
                    { "attackRange", "very close", "daggerheart", "Very Close", true, 2 },
                    { "attackRange", "very far", "daggerheart", "Very Far", true, 5 },
                    { "damageDie", "10", "daggerheart", "d10", true, 4 },
                    { "damageDie", "12", "daggerheart", "d12", true, 5 },
                    { "damageDie", "20", "daggerheart", "d20", true, 6 },
                    { "damageDie", "4", "daggerheart", "d4", true, 1 },
                    { "damageDie", "6", "daggerheart", "d6", true, 2 },
                    { "damageDie", "8", "daggerheart", "d8", true, 3 },
                    { "damageType", "magical", "daggerheart", "Magical", true, 2 },
                    { "damageType", "physical", "daggerheart", "Physical", true, 1 },
                    { "tier", "1", "daggerheart", "Tier 1", true, 1 },
                    { "tier", "2", "daggerheart", "Tier 2", true, 2 },
                    { "tier", "3", "daggerheart", "Tier 3", true, 3 },
                    { "tier", "4", "daggerheart", "Tier 4", true, 4 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReferenceValues_SystemCode_Category_SortOrder",
                table: "ReferenceValues",
                columns: new[] { "SystemCode", "Category", "SortOrder" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReferenceValues");
        }
    }
}
