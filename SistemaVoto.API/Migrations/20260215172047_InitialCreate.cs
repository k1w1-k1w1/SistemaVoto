using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SistemaVoto.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Elecciones",
                columns: table => new
                {
                    EleccionId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Descripcion = table.Column<string>(type: "text", nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Tipo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Estado = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ResultadosPublicos = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Elecciones", x => x.EleccionId);
                });

            migrationBuilder.CreateTable(
                name: "ListasPoliticas",
                columns: table => new
                {
                    ListaPoliticaId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Siglas = table.Column<string>(type: "text", nullable: false),
                    Logo = table.Column<string>(type: "text", nullable: false),
                    Descripcion = table.Column<string>(type: "text", nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ListasPoliticas", x => x.ListaPoliticaId);
                });

            migrationBuilder.CreateTable(
                name: "UbicacionesVotacion",
                columns: table => new
                {
                    UbicacionId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Direccion = table.Column<string>(type: "text", nullable: false),
                    NumeroMesa = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    CapacidadVotantes = table.Column<int>(type: "integer", nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UbicacionesVotacion", x => x.UbicacionId);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    UsuarioId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Apellido = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Cedula = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    FotoPerfil = table.Column<string>(type: "text", nullable: true),
                    Rol = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Estado = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaVerificacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TokenVerificacion = table.Column<string>(type: "text", nullable: true),
                    TokenExpiracion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.UsuarioId);
                });

            migrationBuilder.CreateTable(
                name: "Candidatos",
                columns: table => new
                {
                    CandidatoId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Apellido = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Cedula = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Foto = table.Column<string>(type: "text", nullable: false),
                    ListaPoliticaId = table.Column<int>(type: "integer", nullable: true),
                    Propuestas = table.Column<string>(type: "text", nullable: false),
                    EleccionId = table.Column<int>(type: "integer", nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Candidatos", x => x.CandidatoId);
                    table.ForeignKey(
                        name: "FK_Candidatos_Elecciones_EleccionId",
                        column: x => x.EleccionId,
                        principalTable: "Elecciones",
                        principalColumn: "EleccionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Candidatos_ListasPoliticas_ListaPoliticaId",
                        column: x => x.ListaPoliticaId,
                        principalTable: "ListasPoliticas",
                        principalColumn: "ListaPoliticaId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "JefesDeJunta",
                columns: table => new
                {
                    JefeDeJuntaId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UsuarioId = table.Column<int>(type: "integer", nullable: false),
                    UbicacionAsignadaId = table.Column<int>(type: "integer", nullable: true),
                    FechaAsignacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JefesDeJunta", x => x.JefeDeJuntaId);
                    table.ForeignKey(
                        name: "FK_JefesDeJunta_UbicacionesVotacion_UbicacionAsignadaId",
                        column: x => x.UbicacionAsignadaId,
                        principalTable: "UbicacionesVotacion",
                        principalColumn: "UbicacionId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_JefesDeJunta_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LogsActividad",
                columns: table => new
                {
                    LogId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UsuarioId = table.Column<int>(type: "integer", nullable: true),
                    Accion = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Descripcion = table.Column<string>(type: "text", nullable: false),
                    DireccionIP = table.Column<string>(type: "text", nullable: false),
                    Fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DatosAdicionales = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogsActividad", x => x.LogId);
                    table.ForeignKey(
                        name: "FK_LogsActividad_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Votantes",
                columns: table => new
                {
                    VotanteId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UsuarioId = table.Column<int>(type: "integer", nullable: false),
                    UbicacionAsignadaId = table.Column<int>(type: "integer", nullable: true),
                    Estado = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FechaPresenciaRecinto = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Votantes", x => x.VotanteId);
                    table.ForeignKey(
                        name: "FK_Votantes_UbicacionesVotacion_UbicacionAsignadaId",
                        column: x => x.UbicacionAsignadaId,
                        principalTable: "UbicacionesVotacion",
                        principalColumn: "UbicacionId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Votantes_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CertificadosVotacion",
                columns: table => new
                {
                    CertificadoId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VotanteId = table.Column<int>(type: "integer", nullable: false),
                    EleccionId = table.Column<int>(type: "integer", nullable: false),
                    NumeroConfirmacion = table.Column<string>(type: "text", nullable: false),
                    FechaEmision = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CertificadosVotacion", x => x.CertificadoId);
                    table.ForeignKey(
                        name: "FK_CertificadosVotacion_Elecciones_EleccionId",
                        column: x => x.EleccionId,
                        principalTable: "Elecciones",
                        principalColumn: "EleccionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CertificadosVotacion_Votantes_VotanteId",
                        column: x => x.VotanteId,
                        principalTable: "Votantes",
                        principalColumn: "VotanteId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CodigosVoto",
                columns: table => new
                {
                    CodigoVotoId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VotanteId = table.Column<int>(type: "integer", nullable: false),
                    EleccionId = table.Column<int>(type: "integer", nullable: false),
                    JefeDeJuntaId = table.Column<int>(type: "integer", nullable: true),
                    Codigo = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    FechaGeneracion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaUso = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Estado = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CodigosVoto", x => x.CodigoVotoId);
                    table.ForeignKey(
                        name: "FK_CodigosVoto_Elecciones_EleccionId",
                        column: x => x.EleccionId,
                        principalTable: "Elecciones",
                        principalColumn: "EleccionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CodigosVoto_JefesDeJunta_JefeDeJuntaId",
                        column: x => x.JefeDeJuntaId,
                        principalTable: "JefesDeJunta",
                        principalColumn: "JefeDeJuntaId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CodigosVoto_Votantes_VotanteId",
                        column: x => x.VotanteId,
                        principalTable: "Votantes",
                        principalColumn: "VotanteId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Incidencias",
                columns: table => new
                {
                    IncidenciaId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    JefeDeJuntaId = table.Column<int>(type: "integer", nullable: false),
                    VotanteId = table.Column<int>(type: "integer", nullable: true),
                    Tipo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Descripcion = table.Column<string>(type: "text", nullable: false),
                    FechaReporte = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Estado = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Resolucion = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Incidencias", x => x.IncidenciaId);
                    table.ForeignKey(
                        name: "FK_Incidencias_JefesDeJunta_JefeDeJuntaId",
                        column: x => x.JefeDeJuntaId,
                        principalTable: "JefesDeJunta",
                        principalColumn: "JefeDeJuntaId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Incidencias_Votantes_VotanteId",
                        column: x => x.VotanteId,
                        principalTable: "Votantes",
                        principalColumn: "VotanteId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Votos",
                columns: table => new
                {
                    VotoId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EleccionId = table.Column<int>(type: "integer", nullable: false),
                    CandidatoId = table.Column<int>(type: "integer", nullable: false),
                    VotanteId = table.Column<int>(type: "integer", nullable: false),
                    FechaEmision = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    VotoEncriptado = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Votos", x => x.VotoId);
                    table.ForeignKey(
                        name: "FK_Votos_Candidatos_CandidatoId",
                        column: x => x.CandidatoId,
                        principalTable: "Candidatos",
                        principalColumn: "CandidatoId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Votos_Elecciones_EleccionId",
                        column: x => x.EleccionId,
                        principalTable: "Elecciones",
                        principalColumn: "EleccionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Votos_Votantes_VotanteId",
                        column: x => x.VotanteId,
                        principalTable: "Votantes",
                        principalColumn: "VotanteId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "UbicacionesVotacion",
                columns: new[] { "UbicacionId", "Activo", "CapacidadVotantes", "Direccion", "Nombre", "NumeroMesa" },
                values: new object[,]
                {
                    { 1, true, 300, "Av. Principal 123", "Escuela Central", "001" },
                    { 2, true, 250, "Calle Norte 456", "Colegio Norte", "002" }
                });

            migrationBuilder.InsertData(
                table: "Usuarios",
                columns: new[] { "UsuarioId", "Apellido", "Cedula", "Email", "Estado", "FechaCreacion", "FechaVerificacion", "FotoPerfil", "Nombre", "PasswordHash", "Rol", "TokenExpiracion", "TokenVerificacion" },
                values: new object[] { 1, "Sistema", "0000000000", "admin@votacion.com", "Activa", new DateTime(2026, 2, 15, 17, 20, 46, 200, DateTimeKind.Utc).AddTicks(4176), new DateTime(2026, 2, 15, 17, 20, 46, 200, DateTimeKind.Utc).AddTicks(4184), null, "Admin", "$2a$11$dgz2Sy4YkMYdjRphomc4jeZ.aF107AUwLa/0po621ns9hPIO6ILWy", "Administrador", null, null });

            migrationBuilder.CreateIndex(
                name: "IX_Candidatos_Cedula_EleccionId",
                table: "Candidatos",
                columns: new[] { "Cedula", "EleccionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Candidatos_EleccionId",
                table: "Candidatos",
                column: "EleccionId");

            migrationBuilder.CreateIndex(
                name: "IX_Candidatos_ListaPoliticaId",
                table: "Candidatos",
                column: "ListaPoliticaId");

            migrationBuilder.CreateIndex(
                name: "IX_CertificadosVotacion_EleccionId",
                table: "CertificadosVotacion",
                column: "EleccionId");

            migrationBuilder.CreateIndex(
                name: "IX_CertificadosVotacion_NumeroConfirmacion",
                table: "CertificadosVotacion",
                column: "NumeroConfirmacion",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CertificadosVotacion_VotanteId_EleccionId",
                table: "CertificadosVotacion",
                columns: new[] { "VotanteId", "EleccionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CodigosVoto_Codigo",
                table: "CodigosVoto",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CodigosVoto_EleccionId",
                table: "CodigosVoto",
                column: "EleccionId");

            migrationBuilder.CreateIndex(
                name: "IX_CodigosVoto_JefeDeJuntaId",
                table: "CodigosVoto",
                column: "JefeDeJuntaId");

            migrationBuilder.CreateIndex(
                name: "IX_CodigosVoto_VotanteId_EleccionId",
                table: "CodigosVoto",
                columns: new[] { "VotanteId", "EleccionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Elecciones_Estado",
                table: "Elecciones",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_Elecciones_FechaInicio",
                table: "Elecciones",
                column: "FechaInicio");

            migrationBuilder.CreateIndex(
                name: "IX_Incidencias_Estado",
                table: "Incidencias",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_Incidencias_FechaReporte",
                table: "Incidencias",
                column: "FechaReporte");

            migrationBuilder.CreateIndex(
                name: "IX_Incidencias_JefeDeJuntaId",
                table: "Incidencias",
                column: "JefeDeJuntaId");

            migrationBuilder.CreateIndex(
                name: "IX_Incidencias_VotanteId",
                table: "Incidencias",
                column: "VotanteId");

            migrationBuilder.CreateIndex(
                name: "IX_JefesDeJunta_UbicacionAsignadaId",
                table: "JefesDeJunta",
                column: "UbicacionAsignadaId");

            migrationBuilder.CreateIndex(
                name: "IX_JefesDeJunta_UsuarioId",
                table: "JefesDeJunta",
                column: "UsuarioId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ListasPoliticas_Nombre",
                table: "ListasPoliticas",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LogsActividad_Accion",
                table: "LogsActividad",
                column: "Accion");

            migrationBuilder.CreateIndex(
                name: "IX_LogsActividad_Fecha",
                table: "LogsActividad",
                column: "Fecha");

            migrationBuilder.CreateIndex(
                name: "IX_LogsActividad_UsuarioId",
                table: "LogsActividad",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_UbicacionesVotacion_Nombre_NumeroMesa",
                table: "UbicacionesVotacion",
                columns: new[] { "Nombre", "NumeroMesa" });

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Cedula",
                table: "Usuarios",
                column: "Cedula",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Votantes_UbicacionAsignadaId",
                table: "Votantes",
                column: "UbicacionAsignadaId");

            migrationBuilder.CreateIndex(
                name: "IX_Votantes_UsuarioId",
                table: "Votantes",
                column: "UsuarioId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Votos_CandidatoId",
                table: "Votos",
                column: "CandidatoId");

            migrationBuilder.CreateIndex(
                name: "IX_Votos_EleccionId",
                table: "Votos",
                column: "EleccionId");

            migrationBuilder.CreateIndex(
                name: "IX_Votos_FechaEmision",
                table: "Votos",
                column: "FechaEmision");

            migrationBuilder.CreateIndex(
                name: "IX_Votos_VotanteId_EleccionId",
                table: "Votos",
                columns: new[] { "VotanteId", "EleccionId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CertificadosVotacion");

            migrationBuilder.DropTable(
                name: "CodigosVoto");

            migrationBuilder.DropTable(
                name: "Incidencias");

            migrationBuilder.DropTable(
                name: "LogsActividad");

            migrationBuilder.DropTable(
                name: "Votos");

            migrationBuilder.DropTable(
                name: "JefesDeJunta");

            migrationBuilder.DropTable(
                name: "Candidatos");

            migrationBuilder.DropTable(
                name: "Votantes");

            migrationBuilder.DropTable(
                name: "Elecciones");

            migrationBuilder.DropTable(
                name: "ListasPoliticas");

            migrationBuilder.DropTable(
                name: "UbicacionesVotacion");

            migrationBuilder.DropTable(
                name: "Usuarios");
        }
    }
}
