namespace PortalAcademico.Services
{
    public interface IMatriculaService
    {
        Task<(bool ok, string? error)> InscribirAsync(string usuarioId, int cursoId);
    }
}
