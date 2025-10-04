namespace PortalAcademico.Dto
{
    public class CursoFilterDto
    {
        public string? Nombre { get; set; }
        public int? CreditosMin { get; set; }
        public int? CreditosMax { get; set; }
        public TimeSpan? HoraDesde { get; set; }
        public TimeSpan? HoraHasta { get; set; }
    }
}
