$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot

Push-Location $root
try {
    docker compose up -d postgres

    Write-Host ''
    Write-Host 'PostgreSQL is starting on localhost:5432.' -ForegroundColor Green
    Write-Host 'Next steps:' -ForegroundColor Cyan
    Write-Host '  1. Run .\scripts\set-dev-secrets.ps1'
    Write-Host '  2. Run dotnet run --project backend/Fintrack.Api'
    Write-Host '  3. Run npm run dev --prefix frontend'
}
finally {
    Pop-Location
}
