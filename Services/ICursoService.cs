using PortalAcademico.Dto;
using PortalAcademico.Models;

namespace PortalAcademico.Services
{
    public interface ICursoService
    {
        Task<List<Curso>> GetActivosAsync(CursoFilterDto? f = null);
        Task<Curso?> GetByIdAsync(int id);
        Task<List<Curso>> GetActivosCachedAsync(CursoFilterDto? f = null);
        Task<int>  CrearAsync(CursoEditDto dto);
        Task<bool> EditarAsync(CursoEditDto dto);
        Task<bool> DesactivarAsync(int id);
        Task<List<Curso>> GetAllAsync();

        Task InvalidateCacheAsync();

    }
}