param(
    [switch]$Clean
)

$ErrorActionPreference = "Stop"

$processNames = @(
    "VerticeMuiscaWeb",
    "VerticeMusicaWeb",
    "VerticeMusicasWeb"
)

Write-Host "Buscando procesos colgados de la app..." -ForegroundColor Cyan

foreach ($name in $processNames) {
    $procs = Get-Process -Name $name -ErrorAction SilentlyContinue
    if ($procs) {
        foreach ($proc in $procs) {
            try {
                Stop-Process -Id $proc.Id -Force -ErrorAction Stop
                Write-Host "Proceso detenido: $($proc.ProcessName) (PID $($proc.Id))" -ForegroundColor Yellow
            }
            catch {
                Write-Warning "No se pudo detener PID $($proc.Id): $($_.Exception.Message)"
            }
        }
    }
}

if ($Clean) {
    Write-Host "Ejecutando dotnet clean..." -ForegroundColor Cyan
    dotnet clean
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet clean falló con código $LASTEXITCODE"
    }
}

Write-Host "Ejecutando dotnet build..." -ForegroundColor Cyan
dotnet build
if ($LASTEXITCODE -ne 0) {
    throw "dotnet build falló con código $LASTEXITCODE"
}

Write-Host "Compilación completada correctamente." -ForegroundColor Green
