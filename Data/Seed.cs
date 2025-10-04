using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PortalAcademico.Models;

namespace PortalAcademico.Data
{
    public static class Seed
    {
        public static async Task RunAsync(IServiceProvider sp)
        {
            using var scope = sp.CreateScope();
            var ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await ctx.Database.MigrateAsync();

            var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

            const string rol = "Coordinador";
            if (!await roleMgr.RoleExistsAsync(rol))
                await roleMgr.CreateAsync(new IdentityRole(rol));

            var email = "coordinador@demo.com";
            var u = await userMgr.FindByEmailAsync(email);
            if (u is null)
            {
                u = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
                await userMgr.CreateAsync(u, "Coordinador123$");
                await userMgr.AddToRoleAsync(u, rol);
            }

            if (!await ctx.Cursos.AnyAsync())
            {
                ctx.Cursos.AddRange(
                    new Curso { Codigo="INF101", Nombre="Introducción a Programación", Creditos=3, CupoMaximo=30, HorarioInicio=TimeSpan.FromHours(8),  HorarioFin=TimeSpan.FromHours(10), Activo=true },
                    new Curso { Codigo="MAT201", Nombre="Cálculo II",                   Creditos=4, CupoMaximo=25, HorarioInicio=TimeSpan.FromHours(10), HorarioFin=TimeSpan.FromHours(12), Activo=true },
                    new Curso { Codigo="ADM300", Nombre="Gestión de Proyectos",         Creditos=3, CupoMaximo=20, HorarioInicio=TimeSpan.FromHours(14), HorarioFin=TimeSpan.FromHours(16), Activo=true }
                );
                await ctx.SaveChangesAsync();
            }
        }
    }
}
