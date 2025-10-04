using Microsoft.AspNetCore.Mvc;
using PortalAcademico.Dto;
using PortalAcademico.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace PortalAcademico.Controllers
{
    public class CursosController : Controller
    {

    private readonly ICursoService _cursos;
    private readonly IMatriculaService _mats;

    public CursosController(ICursoService cursos, IMatriculaService mats)
    {
        _cursos = cursos;
        _mats = mats;
    }
    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] CursoFilterDto f)
    {
        try
        {
            // usa caché si NO hay filtros, se salta si hay filtros
            var data = await _cursos.GetActivosCachedAsync(f);
            return View(data);
        }
        catch (ArgumentException ex)
        {
            TempData["Error"] = ex.Message;
            var data = await _cursos.GetActivosCachedAsync(null);
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

        [Authorize]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Inscribirse(int cursoId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Challenge();

            var (ok, error) = await _mats.InscribirAsync(userId, cursoId);
            if (!ok) TempData["Error"] = error;
            else     TempData["Ok"]    = "Matrícula creada en estado PENDIENTE.";

            return RedirectToAction("Detalle", new { id = cursoId });
        }

    }
}
