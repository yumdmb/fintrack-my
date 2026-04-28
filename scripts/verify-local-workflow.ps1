param(
    [string]$PostgresConnection = 'Host=localhost;Port=5432;Database=fintrack;Username=fintrack;Password=fintrack',
    [string]$JwtSigningKey = 'dev-signing-key-change-me-before-real-auth',
    [string]$AdminEmail = 'admin@fintrack.local',
    [string]$AdminPassword = 'ChangeMe123!',
    [string]$CompanyName = 'Fintrack Demo Sdn. Bhd.',
    [string]$RegistrationNumber = '202401000001',
    [string]$TaxIdentificationNumber = 'C1234567890',
    [string]$SalesAndServiceTaxNumber = 'SST12345678',
    [string]$DefaultCurrencyCode = 'MYR',
    [string]$BackendBaseUrl = 'http://127.0.0.1:5232',
    [string]$FrontendBaseUrl = 'http://127.0.0.1:5173'
)

$ErrorActionPreference = 'Stop'

function Wait-UntilReady {
    param(
        [string]$Uri,
        [string]$Name,
        [int]$TimeoutSeconds = 120
    )

    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
    do {
        try {
            $response = Invoke-WebRequest -Uri $Uri -UseBasicParsing -TimeoutSec 5
            if ($response.StatusCode -eq 200) {
                return $response
            }
        }
        catch {
            Start-Sleep -Seconds 2
        }
    } while ((Get-Date) -lt $deadline)

    throw "$Name did not become ready at $Uri within $TimeoutSeconds seconds."
}

function Invoke-JsonRequest {
    param(
        [string]$Method,
        [string]$Uri,
        [object]$Body = $null,
        [string]$BearerToken
    )

    $headers = @{}
    if ($BearerToken) {
        $headers['Authorization'] = "Bearer $BearerToken"
    }

    $params = @{
        Method = $Method
        Uri = $Uri
        Headers = $headers
        TimeoutSec = 20
    }

    if ($null -ne $Body) {
        $params['ContentType'] = 'application/json'
        $params['Body'] = ($Body | ConvertTo-Json -Depth 10)
    }

    Invoke-RestMethod @params
}

$root = Split-Path -Parent $PSScriptRoot
$logDir = Join-Path $root '.omx/logs'
$backendLog = Join-Path $logDir 'verify-backend.log'
$backendErrorLog = Join-Path $logDir 'verify-backend.err.log'
$frontendLog = Join-Path $logDir 'verify-frontend.log'
$frontendErrorLog = Join-Path $logDir 'verify-frontend.err.log'
$backendProcess = $null
$frontendProcess = $null

New-Item -ItemType Directory -Force -Path $logDir | Out-Null
Remove-Item $backendLog, $backendErrorLog, $frontendLog, $frontendErrorLog -Force -ErrorAction SilentlyContinue

Push-Location $root
try {
    & (Join-Path $root 'scripts/dev-up.ps1')
    & (Join-Path $root 'scripts/set-dev-secrets.ps1') `
        -PostgresConnection $PostgresConnection `
        -JwtSigningKey $JwtSigningKey `
        -AdminEmail $AdminEmail `
        -AdminPassword $AdminPassword `
        -CompanyName $CompanyName `
        -RegistrationNumber $RegistrationNumber `
        -TaxIdentificationNumber $TaxIdentificationNumber `
        -SalesAndServiceTaxNumber $SalesAndServiceTaxNumber `
        -DefaultCurrencyCode $DefaultCurrencyCode

    $backendProcess = Start-Process dotnet `
        -ArgumentList @('run', '--project', 'backend/Fintrack.Api', '--launch-profile', 'http') `
        -WorkingDirectory $root `
        -RedirectStandardOutput $backendLog `
        -RedirectStandardError $backendErrorLog `
        -WindowStyle Hidden `
        -PassThru

    $frontendProcess = Start-Process npm.cmd `
        -ArgumentList @('run', 'dev', '--prefix', 'frontend', '--', '--host', '127.0.0.1') `
        -WorkingDirectory $root `
        -RedirectStandardOutput $frontendLog `
        -RedirectStandardError $frontendErrorLog `
        -WindowStyle Hidden `
        -PassThru

    Wait-UntilReady -Uri "$BackendBaseUrl/health" -Name 'Backend health endpoint' | Out-Null
    $frontendResponse = Wait-UntilReady -Uri "$FrontendBaseUrl/sign-in" -Name 'Frontend sign-in route'
    if ($frontendResponse.Content -notmatch 'id="root"') {
        throw 'Frontend sign-in route did not return the SPA shell.'
    }

    Wait-UntilReady -Uri "$FrontendBaseUrl/health" -Name 'Frontend proxy health endpoint' | Out-Null

    $auth = Invoke-JsonRequest -Method Post -Uri "$FrontendBaseUrl/api/auth/sign-in" -Body @{
        email = $AdminEmail
        password = $AdminPassword
    }

    if (-not $auth.accessToken) {
        throw 'Sign-in succeeded without returning an access token.'
    }

    $invoice = Invoke-JsonRequest -Method Post -Uri "$FrontendBaseUrl/api/invoices" -BearerToken $auth.accessToken -Body @{
        invoiceNumber = "SMOKE-$([Guid]::NewGuid().ToString('N'))"
        customerName = 'Smoke Customer Sdn. Bhd.'
        customerRegistrationNumber = '202604290001'
        customerTaxIdentificationNumber = 'C0000000001'
        issueDate = '2026-04-29'
        dueDate = '2026-05-29'
        lineItems = @(
            @{
                description = 'Smoke consulting'
                quantity = 2
                unitPrice = 75
                taxRate = 6
            }
        )
    }

    $expense = Invoke-JsonRequest -Method Post -Uri "$FrontendBaseUrl/api/expenses" -BearerToken $auth.accessToken -Body @{
        expenseDate = '2026-04-29'
        category = 'Smoke Ops'
        description = 'Smoke verification expense'
        amount = 42.5
    }

    $finalized = Invoke-JsonRequest -Method Post -Uri "$FrontendBaseUrl/api/invoices/$($invoice.id)/finalize" -BearerToken $auth.accessToken
    $dashboard = Invoke-JsonRequest -Method Get -Uri "$FrontendBaseUrl/api/dashboard/summary?startDate=2026-04-01&endDate=2026-04-30" -BearerToken $auth.accessToken
    $exportJson = Invoke-JsonRequest -Method Get -Uri "$FrontendBaseUrl/api/invoices/$($invoice.id)/export/json" -BearerToken $auth.accessToken
    $exportCsv = Invoke-WebRequest -Uri "$FrontendBaseUrl/api/invoices/$($invoice.id)/export/csv" -Headers @{ Authorization = "Bearer $($auth.accessToken)" } -UseBasicParsing -TimeoutSec 20
    $exportCsvBody = $exportCsv.Content

    if ($finalized.status -ne 'Finalized') {
        throw "Invoice $($invoice.invoiceNumber) was not finalized."
    }

    if ($dashboard.revenue -lt $invoice.grandTotal) {
        throw 'Dashboard revenue did not include the smoke invoice.'
    }

    if ($dashboard.expenses -lt $expense.amount) {
        throw 'Dashboard expenses did not include the smoke expense.'
    }

    if ($exportJson.invoiceNumber -ne $invoice.invoiceNumber) {
        throw 'JSON export returned the wrong invoice number.'
    }

    if ($exportCsv.Headers['Content-Type'] -notlike 'text/csv*') {
        throw 'CSV export did not return text/csv content.'
    }

    if ($exportCsvBody -notmatch [regex]::Escape($invoice.invoiceNumber)) {
        throw 'CSV export did not contain the smoke invoice number.'
    }

    [pscustomobject]@{
        frontend = $FrontendBaseUrl
        backend = $BackendBaseUrl
        signedInEmail = $auth.user.email
        invoiceId = $invoice.id
        expenseId = $expense.id
        dashboardRevenue = $dashboard.revenue
        dashboardExpenses = $dashboard.expenses
        exportInvoiceNumber = $exportJson.invoiceNumber
    } | ConvertTo-Json -Depth 5
}
finally {
    foreach ($process in @($frontendProcess, $backendProcess)) {
        if ($null -ne $process -and -not $process.HasExited) {
            Stop-Process -Id $process.Id -Force
        }
    }

    Pop-Location
}
