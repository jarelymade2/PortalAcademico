using Microsoft.EntityFrameworkCore;
using PortalAcademico.Data;
using PortalAcademico.Dto;
using PortalAcademico.Models;

namespace PortalAcademico.Services
{
    public class CursoService : ICursoService
    {
        private readonly ApplicationDbContext _db;
        public CursoService(ApplicationDbContext db) => _db = db;

        public async Task<List<Curso>> GetActivosAsync(CursoFilterDto? f = null)
        {
            var q = _db.Cursos.AsNoTracking().Where(c => c.Activo);

            if (!string.IsNullOrWhiteSpace(f?.Nombre))
                q = q.Where(c => EF.Functions.Like(c.Nombre, $"%{f!.Nombre}%"));

            if (f?.CreditosMin is not null)
            {
                if (f.CreditosMin < 0) throw new ArgumentException("Créditos mínimos no pueden ser negativos.");
                q = q.Where(c => c.Creditos >= f.CreditosMin);
            }

            if (f?.CreditosMax is not null)
            {
                if (f.CreditosMax < 0) throw new ArgumentException("Créditos máximos no pueden ser negativos.");
                q = q.Where(c => c.Creditos <= f.CreditosMax);
            }

            if (f?.HoraDesde is not null)
                q = q.Where(c => c.HorarioInicio >= f.HoraDesde);

            if (f?.HoraHasta is not null)
                q = q.Where(c => c.HorarioFin <= f.HoraHasta);

            var lista = await q.ToListAsync();
            return lista.OrderBy(c => c.HorarioInicio).ToList();
        }

        public Task<Curso?> GetByIdAsync(int id) =>
            _db.Cursos.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id)!;
    }
}
