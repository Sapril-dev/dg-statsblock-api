using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DunorGames.Data.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddStatblockAliases : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AliasesJson",
                table: "Statblocks",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AliasesJson",
                table: "Statblocks");
        }
    }
}
