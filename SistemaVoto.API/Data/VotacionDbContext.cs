using Microsoft.CodeAnalysis.Scripting;
using Microsoft.EntityFrameworkCore;
using SistemaVoto.Models;

namespace SistemaVoto.API.Data
{
    public class VotacionDbContext : DbContext
    {
        public VotacionDbContext(DbContextOptions<VotacionDbContext> options)
            : base(options)
        {
        }

        // DbSets - Tablas de la base de datos
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Votante> Votantes { get; set; }
        public DbSet<JefeDeJunta> JefesDeJunta { get; set; }
        public DbSet<UbicacionVotacion> UbicacionesVotacion { get; set; }
        public DbSet<CodigoVoto> CodigosVoto { get; set; }
        public DbSet<Eleccion> Elecciones { get; set; }
        public DbSet<Candidato> Candidatos { get; set; }
        public DbSet<ListaPolitica> ListasPoliticas { get; set; }
        public DbSet<Voto> Votos { get; set; }
        public DbSet<CertificadoVotacion> CertificadosVotacion { get; set; }
        public DbSet<Incidencia> Incidencias { get; set; }
        public DbSet<LogActividad> LogsActividad { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración de Usuario
            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.ToTable("Usuarios");
                entity.HasKey(e => e.UsuarioId);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.Cedula).IsUnique();

                entity.HasOne(e => e.Votante)
                    .WithOne(v => v.Usuario)
                    .HasForeignKey<Votante>(v => v.UsuarioId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.JefeDeJunta)
                    .WithOne(j => j.Usuario)
                    .HasForeignKey<JefeDeJunta>(j => j.UsuarioId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configuración de Votante
            modelBuilder.Entity<Votante>(entity =>
            {
                entity.ToTable("Votantes");
                entity.HasKey(e => e.VotanteId);
                entity.HasIndex(e => e.UsuarioId).IsUnique();

                entity.HasOne(e => e.UbicacionAsignada)
                    .WithMany(u => u.VotantesAsignados)
                    .HasForeignKey(e => e.UbicacionAsignadaId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Configuración de JefeDeJunta
            modelBuilder.Entity<JefeDeJunta>(entity =>
            {
                entity.ToTable("JefesDeJunta");
                entity.HasKey(e => e.JefeDeJuntaId);
                entity.HasIndex(e => e.UsuarioId).IsUnique();

                entity.HasOne(e => e.UbicacionAsignada)
                    .WithMany(u => u.JefesAsignados)
                    .HasForeignKey(e => e.UbicacionAsignadaId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Configuración de UbicacionVotacion
            modelBuilder.Entity<UbicacionVotacion>(entity =>
            {
                entity.ToTable("UbicacionesVotacion");
                entity.HasKey(e => e.UbicacionId);
                entity.HasIndex(e => new { e.Nombre, e.NumeroMesa });
            });

            // Configuración de CodigoVoto
            modelBuilder.Entity<CodigoVoto>(entity =>
            {
                entity.ToTable("CodigosVoto");
                entity.HasKey(e => e.CodigoVotoId);
                entity.HasIndex(e => e.Codigo).IsUnique();
                entity.HasIndex(e => new { e.VotanteId, e.EleccionId }).IsUnique();

                entity.HasOne(e => e.Votante)
                    .WithMany(v => v.CodigosVoto)
                    .HasForeignKey(e => e.VotanteId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Eleccion)
                    .WithMany(el => el.CodigosVoto)
                    .HasForeignKey(e => e.EleccionId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.JefeDeJunta)
                    .WithMany(j => j.CodigosGenerados)
                    .HasForeignKey(e => e.JefeDeJuntaId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Configuración de Eleccion
            modelBuilder.Entity<Eleccion>(entity =>
            {
                entity.ToTable("Elecciones");
                entity.HasKey(e => e.EleccionId);
                entity.HasIndex(e => e.FechaInicio);
                entity.HasIndex(e => e.Estado);
            });

            // Configuración de Candidato
            modelBuilder.Entity<Candidato>(entity =>
            {
                entity.ToTable("Candidatos");
                entity.HasKey(e => e.CandidatoId);
                entity.HasIndex(e => new { e.Cedula, e.EleccionId }).IsUnique();

                entity.HasOne(e => e.Eleccion)
                    .WithMany(el => el.Candidatos)
                    .HasForeignKey(e => e.EleccionId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.ListaPolitica)
                    .WithMany(l => l.Candidatos)
                    .HasForeignKey(e => e.ListaPoliticaId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Configuración de ListaPolitica
            modelBuilder.Entity<ListaPolitica>(entity =>
            {
                entity.ToTable("ListasPoliticas");
                entity.HasKey(e => e.ListaPoliticaId);
                entity.HasIndex(e => e.Nombre).IsUnique();
            });

            // Configuración de Voto
            modelBuilder.Entity<Voto>(entity =>
            {
                entity.ToTable("Votos");
                entity.HasKey(e => e.VotoId);
                entity.HasIndex(e => new { e.VotanteId, e.EleccionId }).IsUnique();
                entity.HasIndex(e => e.FechaEmision);

                entity.HasOne(e => e.Eleccion)
                    .WithMany(el => el.Votos)
                    .HasForeignKey(e => e.EleccionId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Candidato es opcional (voto blanco no tiene candidato)
                entity.HasOne(e => e.Candidato)
                    .WithMany(c => c.Votos)
                    .HasForeignKey(e => e.CandidatoId)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.Restrict);

                // Lista política es opcional (solo para voto en plancha)
                entity.HasOne(e => e.ListaPolitica)
                    .WithMany()
                    .HasForeignKey(e => e.ListaPoliticaId)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Votante)
                    .WithMany(v => v.Votos)
                    .HasForeignKey(e => e.VotanteId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configuración de CertificadoVotacion
            modelBuilder.Entity<CertificadoVotacion>(entity =>
            {
                entity.ToTable("CertificadosVotacion");
                entity.HasKey(e => e.CertificadoId);
                entity.HasIndex(e => e.NumeroConfirmacion).IsUnique();
                entity.HasIndex(e => new { e.VotanteId, e.EleccionId }).IsUnique();

                entity.HasOne(e => e.Votante)
                    .WithMany(v => v.Certificados)
                    .HasForeignKey(e => e.VotanteId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Eleccion)
                    .WithMany()
                    .HasForeignKey(e => e.EleccionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configuración de Incidencia
            modelBuilder.Entity<Incidencia>(entity =>
            {
                entity.ToTable("Incidencias");
                entity.HasKey(e => e.IncidenciaId);
                entity.HasIndex(e => e.FechaReporte);
                entity.HasIndex(e => e.Estado);

                entity.HasOne(e => e.JefeDeJunta)
                    .WithMany(j => j.IncidenciasReportadas)
                    .HasForeignKey(e => e.JefeDeJuntaId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Votante)
                    .WithMany()
                    .HasForeignKey(e => e.VotanteId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Configuración de LogActividad
            modelBuilder.Entity<LogActividad>(entity =>
            {
                entity.ToTable("LogsActividad");
                entity.HasKey(e => e.LogId);
                entity.HasIndex(e => e.Fecha);
                entity.HasIndex(e => e.Accion);
                entity.HasIndex(e => e.UsuarioId);

                entity.HasOne(e => e.Usuario)
                    .WithMany(u => u.LogsActividad)
                    .HasForeignKey(e => e.UsuarioId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Datos iniciales
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Usuario administrador por defecto
            modelBuilder.Entity<Usuario>().HasData(
                new Usuario
                {
                    UsuarioId = 1,
                    Nombre = "Admin",
                    Apellido = "Sistema",
                    Cedula = "0000000000",
                    Email = "admin@votacion.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                    FotoPerfil = null,
                    Rol = "Administrador",
                    Estado = "Activa",
                    FechaCreacion = DateTime.UtcNow,
                    FechaVerificacion = DateTime.UtcNow
                }
            );

            // Ubicaciones de votación de ejemplo
            modelBuilder.Entity<UbicacionVotacion>().HasData(
                new UbicacionVotacion
                {
                    UbicacionId = 1,
                    Nombre = "Escuela Central",
                    Direccion = "Av. Principal 123",
                    NumeroMesa = "001",
                    CapacidadVotantes = 300,
                    Activo = true
                },
                new UbicacionVotacion
                {
                    UbicacionId = 2,
                    Nombre = "Colegio Norte",
                    Direccion = "Calle Norte 456",
                    NumeroMesa = "002",
                    CapacidadVotantes = 250,
                    Activo = true
                }
            );
        }
    }
}