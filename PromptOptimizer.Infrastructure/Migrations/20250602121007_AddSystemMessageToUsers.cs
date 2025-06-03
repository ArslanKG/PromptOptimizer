using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PromptOptimizer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSystemMessageToUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SystemMessage",
                table: "Users",
                type: "TEXT",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SystemMessage",
                table: "Users");
        }
    }
}