using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaVoto.API.Migrations
{
    /// <inheritdoc />
    public partial class AgregarTipoVoto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "VotoEncriptado",
                table: "Votos",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<int>(
                name: "CandidatoId",
                table: "Votos",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "ListaPoliticaId",
                table: "Votos",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TipoVoto",
                table: "Votos",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "UsuarioId",
                keyValue: 1,
                columns: new[] { "FechaCreacion", "FechaVerificacion", "PasswordHash" },
                values: new object[] { new DateTime(2026, 2, 17, 13, 51, 38, 314, DateTimeKind.Utc).AddTicks(6052), new DateTime(2026, 2, 17, 13, 51, 38, 314, DateTimeKind.Utc).AddTicks(6059), "$2a$11$PrPfrV/IMujTK8zptnGyyOSokTJD62RsOj63h4aGR3k5v8CyU0YeK" });

            migrationBuilder.CreateIndex(
                name: "IX_Votos_ListaPoliticaId",
                table: "Votos",
                column: "ListaPoliticaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Votos_ListasPoliticas_ListaPoliticaId",
                table: "Votos",
                column: "ListaPoliticaId",
                principalTable: "ListasPoliticas",
                principalColumn: "ListaPoliticaId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Votos_ListasPoliticas_ListaPoliticaId",
                table: "Votos");

            migrationBuilder.DropIndex(
                name: "IX_Votos_ListaPoliticaId",
                table: "Votos");

            migrationBuilder.DropColumn(
                name: "ListaPoliticaId",
                table: "Votos");

            migrationBuilder.DropColumn(
                name: "TipoVoto",
                table: "Votos");

            migrationBuilder.AlterColumn<string>(
                name: "VotoEncriptado",
                table: "Votos",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CandidatoId",
                table: "Votos",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "UsuarioId",
                keyValue: 1,
                columns: new[] { "FechaCreacion", "FechaVerificacion", "PasswordHash" },
                values: new object[] { new DateTime(2026, 2, 15, 21, 30, 17, 793, DateTimeKind.Utc).AddTicks(2160), new DateTime(2026, 2, 15, 21, 30, 17, 793, DateTimeKind.Utc).AddTicks(2169), "$2a$11$gKgiYaqHcNT/1aTeGQ1DPOcIjIcglbvVQb.YyOOwFbASEnbfEfs7u" });
        }
    }
}
