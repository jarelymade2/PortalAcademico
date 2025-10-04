using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalAcademico.Dto;
using PortalAcademico.Services;

namespace PortalAcademico.Controllers
{
    [Authorize(Roles = "Coordinador")]
    public class CoordinadorController : Controller
    {
        private readonly ICursoService _cursos;
        private readonly IMatriculaService _mats;

        public CoordinadorController(ICursoService cursos, IMatriculaService mats)
        { _cursos = cursos; _mats = mats; }

        public async Task<IActionResult> Index()
        {
            var data = await _cursos.GetActivosAsync(null);
            return View(data);
        }

        [HttpGet]
        public IActionResult Crear() => View(new CursoEditDto());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(CursoEditDto dto)
        {
            if (dto.HorarioFin <= dto.HorarioInicio)
                ModelState.AddModelError(string.Empty, "HorarioFin debe ser mayor que HorarioInicio.");
            if (!ModelState.IsValid) return View(dto);

            try
            {
                await _cursos.CrearAsync(dto);
                return RedirectToAction(nameof(Index));
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(dto);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var c = await _cursos.GetByIdAsync(id);
            if (c is null) return NotFound();
            var dto = new CursoEditDto {
                Id=c.Id, Codigo=c.Codigo, Nombre=c.Nombre, Creditos=c.Creditos, CupoMaximo=c.CupoMaximo,
                HorarioInicio=c.HorarioInicio, HorarioFin=c.HorarioFin, Activo=c.Activo
            };
            return View(dto);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(CursoEditDto dto)
        {
            if (dto.HorarioFin <= dto.HorarioInicio)
                ModelState.AddModelError(string.Empty, "HorarioFin debe ser mayor que HorarioInicio.");
            if (!ModelState.IsValid) return View(dto);

            try
            {
                var ok = await _cursos.EditarAsync(dto);
                return ok ? RedirectToAction(nameof(Index)) : NotFound();
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(dto);
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Desactivar(int id)
        {
            await _cursos.DesactivarAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // ===== Matrículas =====

        [HttpGet]
        public async Task<IActionResult> Matriculas(int cursoId)
        {
            ViewBag.CursoId = cursoId;
            var lista = await _mats.GetByCursoAsync(cursoId);
            return View(lista);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirmar(int id, int cursoId)
        {
            await _mats.ConfirmarAsync(id);
            return RedirectToAction(nameof(Matriculas), new { cursoId });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancelar(int id, int cursoId)
        {
            await _mats.CancelarAsync(id);
            return RedirectToAction(nameof(Matriculas), new { cursoId });
        }
    }
}
