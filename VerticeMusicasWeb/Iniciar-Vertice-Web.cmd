@echo off
cd /d "%~dp0"
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0Iniciar-Vertice-Web.ps1" %*
if errorlevel 1 pause
