using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaVoto.API.Migrations
{
    /// <inheritdoc />
    public partial class AgregarTelefonoUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Telefono",
                table: "Usuarios",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "UsuarioId",
                keyValue: 1,
                columns: new[] { "FechaCreacion", "FechaVerificacion", "PasswordHash", "Telefono" },
                values: new object[] { new DateTime(2026, 2, 15, 18, 3, 3, 453, DateTimeKind.Utc).AddTicks(4243), new DateTime(2026, 2, 15, 18, 3, 3, 453, DateTimeKind.Utc).AddTicks(4248), "$2a$11$xSK3/CovLuGkkj6.tRC0Cu43hiTkDlV.Qpriu.J0JjBbdKriWtV9G", null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Telefono",
                table: "Usuarios");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "UsuarioId",
                keyValue: 1,
                columns: new[] { "FechaCreacion", "FechaVerificacion", "PasswordHash" },
                values: new object[] { new DateTime(2026, 2, 15, 17, 20, 46, 200, DateTimeKind.Utc).AddTicks(4176), new DateTime(2026, 2, 15, 17, 20, 46, 200, DateTimeKind.Utc).AddTicks(4184), "$2a$11$dgz2Sy4YkMYdjRphomc4jeZ.aF107AUwLa/0po621ns9hPIO6ILWy" });
        }
    }
}
