using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PortalAcademico.Models;

namespace PortalAcademico.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Curso> Cursos => Set<Curso>();
        public DbSet<Matricula> Matriculas => Set<Matricula>();

        protected override void OnModelCreating(ModelBuilder b)
        {
            base.OnModelCreating(b);

            // Curso: código único + checks solicitados
            b.Entity<Curso>()
                .HasIndex(c => c.Codigo)
                .IsUnique();

              b.Entity<Curso>()
                .HasIndex(c => c.Codigo).IsUnique();

            b.Entity<Curso>()
                .ToTable(tb =>
                {
                    tb.HasCheckConstraint("CK_Curso_Creditos_Pos", "Creditos > 0");
                    // Al estar mapeado a ticks, este check cambia a comparar longs:
                    tb.HasCheckConstraint("CK_Curso_Horas", "HorarioInicio < HorarioFin");
                });

            b.Entity<Matricula>()
                .HasIndex(m => new { m.UsuarioId, m.CursoId }).IsUnique();
        }
    }
}
