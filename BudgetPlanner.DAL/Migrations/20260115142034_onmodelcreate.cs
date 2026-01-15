using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BudgetPlanner.DAL.Migrations
{
    /// <inheritdoc />
    public partial class onmodelcreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 11,
                column: "ToggleGrossNet",
                value: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 11,
                column: "ToggleGrossNet",
                value: false);
        }
    }
}
