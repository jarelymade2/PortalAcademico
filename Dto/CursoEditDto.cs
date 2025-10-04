using System.ComponentModel.DataAnnotations;

namespace PortalAcademico.Dto
{
    public class CursoEditDto
    {
        public int? Id { get; set; }

        [Required, StringLength(20)]
        public string Codigo { get; set; } = "";

        [Required, StringLength(120)]
        public string Nombre { get; set; } = "";

        [Range(1, int.MaxValue)]
        public int Creditos { get; set; }

        [Range(1, int.MaxValue)]
        public int CupoMaximo { get; set; }

        [Required] public TimeSpan HorarioInicio { get; set; }
        [Required] public TimeSpan HorarioFin    { get; set; }

        public bool Activo { get; set; } = true;
    }
}
