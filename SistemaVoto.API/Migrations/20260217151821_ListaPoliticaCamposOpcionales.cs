using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaVoto.API.Migrations
{
    /// <inheritdoc />
    public partial class ListaPoliticaCamposOpcionales : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "UsuarioId",
                keyValue: 1,
                columns: new[] { "FechaCreacion", "FechaVerificacion", "PasswordHash" },
                values: new object[] { new DateTime(2026, 2, 17, 15, 18, 20, 739, DateTimeKind.Utc).AddTicks(1243), new DateTime(2026, 2, 17, 15, 18, 20, 739, DateTimeKind.Utc).AddTicks(1245), "$2a$11$onrXIsnCZzmVV01vgrh8/.hlTNvzR/fzt.r318WlZt/emIi/6SSG." });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "UsuarioId",
                keyValue: 1,
                columns: new[] { "FechaCreacion", "FechaVerificacion", "PasswordHash" },
                values: new object[] { new DateTime(2026, 2, 17, 13, 51, 38, 314, DateTimeKind.Utc).AddTicks(6052), new DateTime(2026, 2, 17, 13, 51, 38, 314, DateTimeKind.Utc).AddTicks(6059), "$2a$11$PrPfrV/IMujTK8zptnGyyOSokTJD62RsOj63h4aGR3k5v8CyU0YeK" });
        }
    }
}
