# Biblioteca Digital API

API REST con ASP.NET Core y .NET 10 para gestionar libros y préstamos. Los datos se
almacenan en memoria.

## Requisitos

- .NET 10 SDK
- Git

## Restaurar, compilar y probar

Desde la raíz de la solución:

```powershell
dotnet restore BibliotecaDigitalE2E.sln
dotnet build BibliotecaDigitalE2E.sln --configuration Release
dotnet test tests/Biblioteca.UnitTests/Biblioteca.UnitTests.csproj --configuration Release
dotnet test tests/Biblioteca.IntegrationTests/Biblioteca.IntegrationTests.csproj --configuration Release
```

Las pruebas E2E requieren una instancia independiente de la API. Iníciala en una
terminal:

```powershell
$env:ASPNETCORE_URLS = "http://localhost:5080"
dotnet run --project src/Biblioteca.Api/Biblioteca.Api.csproj --configuration Release
```

En otra terminal, ejecuta las pruebas:

```powershell
$env:E2E_BASE_URL = "http://localhost:5080"
dotnet test tests/Biblioteca.E2ETests/Biblioteca.E2ETests.csproj --configuration Release
```

## Integración continua

`.github/workflows/tests.yml` compila y ejecuta las pruebas en cada `push`, en
Pull Requests dirigidos a `main` y manualmente desde GitHub Actions. Guarda los
resultados TRX como artefacto.

`.github/workflows/sonar-quality.yml` ejecuta el análisis de SonarQube Cloud y publica
los informes de cobertura OpenCover. Para habilitarlo, configura en **Settings →
Secrets and variables → Actions**:

- Secreto `SONAR_TOKEN`: token de SonarQube Cloud.
- Variable `SONAR_ORGANIZATION`: clave de la organización.
- Variable `SONAR_PROJECT_KEY`: clave del proyecto.

No guardes el token en el repositorio ni lo incluyas en capturas o documentación.
