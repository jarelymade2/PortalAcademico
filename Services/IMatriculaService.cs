namespace PortalAcademico.Services
{
    public interface IMatriculaService
    {
        Task<(bool ok, string? error)> InscribirAsync(string usuarioId, int cursoId);

        Task<bool> ConfirmarAsync(int matriculaId);
        Task<bool> CancelarAsync(int matriculaId);
        
        Task<List<(int Id, string UsuarioId, string Estado, DateTime Fecha)>> GetByCursoAsync(int cursoId);
    }
}
