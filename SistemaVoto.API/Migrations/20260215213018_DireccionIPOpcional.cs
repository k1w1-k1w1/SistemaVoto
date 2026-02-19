using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaVoto.API.Migrations
{
    /// <inheritdoc />
    public partial class DireccionIPOpcional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "DireccionIP",
                table: "LogsActividad",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "UsuarioId",
                keyValue: 1,
                columns: new[] { "FechaCreacion", "FechaVerificacion", "PasswordHash" },
                values: new object[] { new DateTime(2026, 2, 15, 21, 30, 17, 793, DateTimeKind.Utc).AddTicks(2160), new DateTime(2026, 2, 15, 21, 30, 17, 793, DateTimeKind.Utc).AddTicks(2169), "$2a$11$gKgiYaqHcNT/1aTeGQ1DPOcIjIcglbvVQb.YyOOwFbASEnbfEfs7u" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "DireccionIP",
                table: "LogsActividad",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "UsuarioId",
                keyValue: 1,
                columns: new[] { "FechaCreacion", "FechaVerificacion", "PasswordHash" },
                values: new object[] { new DateTime(2026, 2, 15, 18, 32, 14, 396, DateTimeKind.Utc).AddTicks(3871), new DateTime(2026, 2, 15, 18, 32, 14, 396, DateTimeKind.Utc).AddTicks(3879), "$2a$11$eNQoXr3ILO89SLnO8W1aOuYdwwmUVHslFWy2fFjJ6.gZ.26/0imd2" });
        }
    }
}
