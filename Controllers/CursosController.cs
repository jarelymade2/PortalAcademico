using Microsoft.AspNetCore.Mvc;
using PortalAcademico.Dto;
using PortalAcademico.Services;

namespace PortalAcademico.Controllers
{
    public class CursosController : Controller
    {
        private readonly ICursoService _cursos;
        public CursosController(ICursoService cursos) => _cursos = cursos;

        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] CursoFilterDto f)
        {
            try
            {
                var data = await _cursos.GetActivosAsync(f);
                return View(data);
            }
            catch (ArgumentException ex)
            {
                TempData["Error"] = ex.Message;
                var data = await _cursos.GetActivosAsync(null);
                return View(data);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Detalle(int id)
        {
            var c = await _cursos.GetByIdAsync(id);
            if (c is null || !c.Activo) return NotFound();

            // P4: guardar en sesión el último curso
            HttpContext.Session.SetInt32("LastCourseId", c.Id);
            HttpContext.Session.SetString("LastCourseName", c.Nombre);

            return View(c);
        }
    }
}
