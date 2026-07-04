using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionDeConsorcios_v2_MVC.Migrations
{
    /// <inheritdoc />
    public partial class SincronizarModeloActual : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FechaRevision",
                table: "Pagos");

            migrationBuilder.DropColumn(
                name: "ObservacionAdministracion",
                table: "Pagos");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FechaRevision",
                table: "Pagos",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ObservacionAdministracion",
                table: "Pagos",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
