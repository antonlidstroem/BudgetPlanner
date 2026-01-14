using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BudgetPlanner.DAL.Migrations
{
    /// <inheritdoc />
    public partial class addMonth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Month",
                table: "Transactions",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Month",
                table: "Transactions");
        }
    }
}
