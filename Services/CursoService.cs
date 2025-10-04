using Microsoft.EntityFrameworkCore;
using PortalAcademico.Data;
using PortalAcademico.Dto;
using PortalAcademico.Models;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace PortalAcademico.Services
{
    public class CursoService : ICursoService
    {
        private readonly ApplicationDbContext _db;
        private readonly IDistributedCache _cache;
        private const string CacheKey = "cursos_activos";

               public CursoService(ApplicationDbContext db, IDistributedCache cache)
        {
            _db = db;
            _cache = cache;
        }

        
        public async Task<List<Curso>> GetActivosAsync(CursoFilterDto? f = null)
        {
            var q = _db.Cursos.AsNoTracking().Where(c => c.Activo);

            if (!string.IsNullOrWhiteSpace(f?.Nombre))
                q = q.Where(c => EF.Functions.Like(c.Nombre, $"%{f.Nombre}%"));

            if (f?.CreditosMin is not null)
            {
                if (f.CreditosMin < 0) 
                    throw new ArgumentException("Créditos mínimos no pueden ser negativos.");
                q = q.Where(c => c.Creditos >= f.CreditosMin);
            }

            if (f?.CreditosMax is not null)
            {
                if (f.CreditosMax < 0) 
                    throw new ArgumentException("Créditos máximos no pueden ser negativos.");
                q = q.Where(c => c.Creditos <= f.CreditosMax);
            }

            if (f?.HoraDesde is not null)
                q = q.Where(c => c.HorarioInicio >= f.HoraDesde);

            if (f?.HoraHasta is not null)
                q = q.Where(c => c.HorarioFin <= f.HoraHasta);

            var lista = await q.ToListAsync();
            return lista.OrderBy(c => c.HorarioInicio).ToList(); // 🔹 Orden en memoria (evita error de SQLite con TimeSpan)
        }

        
        public Task<Curso?> GetByIdAsync(int id) =>
            _db.Cursos.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id)!;

       
        public async Task<List<Curso>> GetActivosCachedAsync(CursoFilterDto? f = null)
        {
            var tieneFiltros = f is not null && (
                !string.IsNullOrWhiteSpace(f.Nombre) ||
                f.CreditosMin is not null || f.CreditosMax is not null ||
                f.HoraDesde is not null || f.HoraHasta is not null
            );

            if (tieneFiltros)
                return await GetActivosAsync(f);

            var json = await _cache.GetStringAsync(CacheKey);
            if (!string.IsNullOrEmpty(json))
                return JsonSerializer.Deserialize<List<Curso>>(json) ?? new();

            var data = await GetActivosAsync(null);
            await _cache.SetStringAsync(
                CacheKey,
                JsonSerializer.Serialize(data),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60)
                }
            );
            
        
            return data;
        }

       

        public Task InvalidateCacheAsync() => _cache.RemoveAsync(CacheKey);
    }
}

