#Requires -Version 5.1
# Inicia Kestrel y abre el navegador cuando el puerto ya escucha.
# Uso: .\Iniciar-Vertice-Web.ps1
#      .\Iniciar-Vertice-Web.ps1 -Puerto 5290
param(
    [int] $Puerto = 5288
)

$ErrorActionPreference = 'Continue'
Set-Location $PSScriptRoot

$url = "http://127.0.0.1:$Puerto"
$urlsArg = "http://127.0.0.1:$Puerto"

$scriptAbrir = {
    param([int] $p, [string] $u)
    for ($i = 0; $i -lt 200; $i++) {
        $tcp = $null
        try {
            $tcp = New-Object System.Net.Sockets.TcpClient
            $tcp.Connect('127.0.0.1', $p)
            if ($tcp.Connected) {
                $tcp.Close()
                Start-Sleep -Milliseconds 500
                Start-Process $u
                return
            }
        }
        catch {
        }
        finally {
            if ($null -ne $tcp) {
                try { $tcp.Dispose() } catch { }
            }
        }
        Start-Sleep -Milliseconds 200
    }
}

$job = Start-Job -ScriptBlock $scriptAbrir -ArgumentList $Puerto, $url

try {
    $env:ASPNETCORE_ENVIRONMENT = 'Development'
    dotnet run --urls $urlsArg
}
finally {
    Stop-Job $job -ErrorAction SilentlyContinue | Out-Null
    Remove-Job $job -Force -ErrorAction SilentlyContinue | Out-Null
}
