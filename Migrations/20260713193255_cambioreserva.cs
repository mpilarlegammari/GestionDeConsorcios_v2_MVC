using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionDeConsorcios_v2_MVC.Migrations
{
    /// <inheritdoc />
    public partial class cambioreserva : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Turno",
                table: "Reservas",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Turno",
                table: "Reservas");
        }
    }
}
