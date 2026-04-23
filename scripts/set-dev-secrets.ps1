param(
    [string]$PostgresConnection = 'Host=localhost;Port=5432;Database=fintrack;Username=fintrack;Password=fintrack',
    [string]$JwtSigningKey = 'dev-signing-key-change-me-before-real-auth',
    [string]$AdminEmail = 'admin@fintrack.local',
    [string]$AdminPassword = 'ChangeMe123!',
    [string]$CompanyName = 'Fintrack Demo Sdn. Bhd.'
)

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root 'backend/Fintrack.Api/Fintrack.Api.csproj'

dotnet user-secrets set --project $project 'ConnectionStrings:Postgres' $PostgresConnection
dotnet user-secrets set --project $project 'Jwt:SigningKey' $JwtSigningKey
dotnet user-secrets set --project $project 'BootstrapAdmin:Email' $AdminEmail
dotnet user-secrets set --project $project 'BootstrapAdmin:Password' $AdminPassword
dotnet user-secrets set --project $project 'BootstrapAdmin:CompanyName' $CompanyName

Write-Host 'Development secrets updated for Fintrack.Api.' -ForegroundColor Green
