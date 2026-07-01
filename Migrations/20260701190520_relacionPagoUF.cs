using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionDeConsorcios_v2_MVC.Migrations
{
    /// <inheritdoc />
    public partial class relacionPagoUF : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UnidadFuncionalId",
                table: "Pagos",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_UnidadFuncionalId",
                table: "Pagos",
                column: "UnidadFuncionalId");

            migrationBuilder.AddForeignKey(
                name: "FK_Pagos_UnidadesFuncionales_UnidadFuncionalId",
                table: "Pagos",
                column: "UnidadFuncionalId",
                principalTable: "UnidadesFuncionales",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pagos_UnidadesFuncionales_UnidadFuncionalId",
                table: "Pagos");

            migrationBuilder.DropIndex(
                name: "IX_Pagos_UnidadFuncionalId",
                table: "Pagos");

            migrationBuilder.DropColumn(
                name: "UnidadFuncionalId",
                table: "Pagos");
        }
    }
}
