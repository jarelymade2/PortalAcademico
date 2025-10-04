using Microsoft.EntityFrameworkCore;
using PortalAcademico.Data;
using PortalAcademico.Models;

namespace PortalAcademico.Services
{
    public class MatriculaService : IMatriculaService
    {
        private readonly ApplicationDbContext _db;
        public MatriculaService(ApplicationDbContext db) => _db = db;

        public async Task<(bool ok, string? error)> InscribirAsync(string usuarioId, int cursoId)
        {
            // Debe existir y estar activo
            var curso = await _db.Cursos.FirstOrDefaultAsync(c => c.Id == cursoId && c.Activo);
            if (curso is null) return (false, "El curso no existe o no está activo.");

            // No duplicada (índice único también protege)
            var ya = await _db.Matriculas.AnyAsync(m =>
                m.UsuarioId == usuarioId && m.CursoId == cursoId && m.Estado != EstadoMatricula.Cancelada);
            if (ya) return (false, "Ya estás matriculado en este curso.");

            // Cupo (pendiente + confirmada cuentan)
            var ocupados = await _db.Matriculas.CountAsync(m =>
                m.CursoId == cursoId && m.Estado != EstadoMatricula.Cancelada);
            if (ocupados >= curso.CupoMaximo)
                return (false, "No hay cupos disponibles.");

            // No solape de horario con cursos no cancelados del usuario
            var otras = await _db.Matriculas
                .Where(m => m.UsuarioId == usuarioId && m.Estado != EstadoMatricula.Cancelada)
                .Select(m => m.Curso!)
                .ToListAsync();

            bool solapa = otras.Any(o =>
                !(curso.HorarioFin <= o.HorarioInicio || curso.HorarioInicio >= o.HorarioFin));
            if (solapa) return (false, "Tienes otro curso en el mismo horario.");

            _db.Matriculas.Add(new Matricula
            {
                UsuarioId = usuarioId, CursoId = cursoId, Estado = EstadoMatricula.Pendiente
            });
            await _db.SaveChangesAsync();
            return (true, null);
        }
    }
}
