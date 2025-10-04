
## Sobre el proyecto

Este proyecto es mi examen parcial de Programación I.  
La idea era crear un portal académico donde los estudiantes puedan ver los cursos disponibles, inscribirse y que los coordinadores puedan administrar los cursos y las matrículas.  

Usé ASP.NET Core MVC con **Identity** para la autenticación, **EF Core** con SQLite para la base de datos, y **Redis** para las sesiones y el cache de los cursos activos.  
También lo desplegué en Render usando Docker.

---

## Cómo está organizado

PortalAcademico/
├── Controllers/ -> Controladores para MVC
├── Models/ -> Modelos Curso y Matrícula
├── Dto/ -> Objetos de transferencia de datos
├── Services/ -> Lógica de negocio
├── Data/ -> DbContext y Seed
├── Views/ -> Vistas Razor
├── Program.cs
├── appsettings.json
├── Dockerfile
└── README.md

yaml
Copiar código

---

## Cómo correrlo local

Clonar el repositorio:
git clone https://github.com/<tu-usuario>/PortalAcademico.git
cd PortalAcademico

## Crear la base de datos y aplicar migraciones:
dotnet ef migrations add InitialCreate --context PortalAcademico.Data.ApplicationDbContext
dotnet ef database update --context PortalAcademico.Data.ApplicationDbContext

## Ejecutar la aplicación:
dotnet watch run
Luego abrir en el navegador: http://localhost:5000

## Qué hace el sistema:
Cursos:
-Se pueden crear, editar y desactivar (solo coordinador).
-Cada curso tiene: código único, nombre, créditos, cupo, horario y estado activo/inactivo.

Validaciones importantes:
-Los créditos no pueden ser negativos.
-El horario de fin debe ser mayor que el de inicio.

Matrículas:
-Los estudiantes autenticados pueden inscribirse en cursos.
-No se puede superar el cupo máximo ni solaparse con otro curso del mismo horario.
-Al inscribirse, la matrícula queda en estado “Pendiente”.

Sesiones y Redis:
-Guardo el último curso visitado en sesión para mostrar un enlace “Volver al curso {Nombre}”.
-Cacheé los cursos activos por 60 segundos para que el catálogo cargue más rápido.
-Si se crea, edita o desactiva un curso, la caché se invalida automáticamente.

Panel del Coordinador:
-Solo accesible con rol Coordinador.
-Permite CRUD de cursos y ver matrículas.
-Cuando un curso se desactiva, aparece como “Inactivo” y no se muestra en el catálogo público.

La rama correccion-cursos corrige la visualización correcta de activos/inactivos.

## Despliegue en Render
Rama: deploy/render

Se desplegó como Web Service usando Docker.
Dockerfile:

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build-env
WORKDIR /app

COPY *.csproj ./
RUN dotnet restore

COPY . ./
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build-env /app/out .
ENV APP_NET_CORE PortalAcademico.dll
CMD ASPNETCORE_URLS=http://*:$PORT dotnet $APP_NET_CORE


## Variables mínimas en Render:

ASPNETCORE_ENVIRONMENT = Production

ASPNETCORE_URLS = http://0.0.0.0:${PORT}

ConnectionStrings__DefaultConnection = Data Source=/data/app.db

Redis__ConnectionString = (vacía o tu URL de RedisLabs)

## Ramas del proyecto
Pregunta	Rama	Qué hace
P1	feature/bootstrap-dominio	Proyecto base y modelos con seeding inicial
P2	feature/catalogo-cursos	Catálogo de cursos con filtros
P3	feature/matriculas	Inscripciones y validaciones
P4	feature/sesion-redis	Sesión y cache Redis
P5	feature/panel-coordinador	CRUD de cursos y panel de coordinador
Extra	correccion-cursos	Corrección de cursos activos/inactivos
P6	deploy/render	Despliegue en Render

Todas las ramas se integraron a main mediante Pull Request.

Datos de prueba
Coordinador inicial:
Email: coordinador@demo.com
Password: Coordinador123$

Cursos precargados:
Código	Nombre	Créditos	Cupo	Horario
INF101	Introducción a Programación	3	30	08:00 - 10:00
MAT201	Cálculo II	4	25	10:00 - 12:00
ADM300	Gestión de Proyectos	3	20	14:00 - 16:00
