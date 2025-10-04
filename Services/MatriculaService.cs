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
                UsuarioId = usuarioId,
                CursoId = cursoId,
                Estado = EstadoMatricula.Pendiente
            });
            await _db.SaveChangesAsync();
            return (true, null);
        }
        
        
        public async Task<bool> ConfirmarAsync(int id)
        {
            var m = await _db.Matriculas.Include(x => x.Curso).FirstOrDefaultAsync(x => x.Id == id);
            if (m is null || m.Estado == EstadoMatricula.Cancelada) return false;

            // revalidar cupo solo para Confirmadas:
            var confirmadas = await _db.Matriculas.CountAsync(x => x.CursoId == m.CursoId && x.Estado == EstadoMatricula.Confirmada);
            if (confirmadas >= m.Curso!.CupoMaximo) return false;

            m.Estado = EstadoMatricula.Confirmada;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CancelarAsync(int id)
        {
            var m = await _db.Matriculas.FindAsync(id);
            if (m is null) return false;
            m.Estado = EstadoMatricula.Cancelada;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<List<(int Id, string UsuarioId, string Estado, DateTime Fecha)>> GetByCursoAsync(int cursoId)
        {
            return await _db.Matriculas
                .Where(m => m.CursoId == cursoId)
                .OrderByDescending(m => m.FechaRegistro)
                .Select(m => new ValueTuple<int,string,string,DateTime>(m.Id, m.UsuarioId, m.Estado.ToString(), m.FechaRegistro))
                .ToListAsync();
        }
    }
}
