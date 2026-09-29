<#
  run-all.ps1  -  One-command dev startup for ShilpoHubBD.

  Starts all 4 services in hidden PowerShell windows, with logs in .run-logs:
    1. Backend   (.NET API)          http://localhost:5065
    2. Frontend  (Vite/React)        http://localhost:5173
    3. Heritage RAG (Python)         http://localhost:8000
    4. Product Search RAG (Python)   http://localhost:8001

  What it does for you automatically:
    - Creates any missing .env file from its .env.example (you still must fill in real
      secrets - Gemini API key, DB connection - it will tell you exactly which ones).
    - Replaces previous listeners on the four dedicated development ports.
    - Creates the Python venv and installs dependencies if missing.
    - Waits for each service to actually respond before declaring it ready.

  Usage (from repo root):
    powershell -ExecutionPolicy Bypass -File .\run-all.ps1
#>

$ErrorActionPreference = "Stop"
$root = $PSScriptRoot

function Write-Step($msg) { Write-Host "`n==> $msg" -ForegroundColor Cyan }
function Write-Ok($msg) { Write-Host "    OK: $msg" -ForegroundColor Green }
function Write-Warn($msg) { Write-Host "    WARN: $msg" -ForegroundColor Yellow }
function Write-Err($msg)  { Write-Host "    ERROR: $msg" -ForegroundColor Red }

# ---------------------------------------------------------------------------
# 1. Make sure the 3 .env files exist (create from .env.example if missing)
# ---------------------------------------------------------------------------
Write-Step "Checking .env files"
$envPairs = @(
    @{ Path = "$root\.env";          Example = "$root\.env.example" },
    @{ Path = "$root\frontend\.env"; Example = "$root\frontend\.env.example" },
    @{ Path = "$root\rag\.env";      Example = "$root\rag\.env.example" }
)
$needsSecrets = $false
foreach ($pair in $envPairs) {
    if (-not (Test-Path $pair.Path)) {
        Copy-Item $pair.Example $pair.Path
        Write-Warn "$($pair.Path) was missing - created from .env.example."
        if ($pair.Path -ne "$root\frontend\.env") {
            Write-Warn 'Review the new environment file and fill in real values before continuing.'
            $needsSecrets = $true
        }
    } else {
        Write-Ok "$($pair.Path) exists"
    }
}

# Check the root .env actually has the secrets filled in, not left as placeholders
$rootEnvContent = Get-Content "$root\.env" -Raw
$requiredKeys = @(
    @{ Key = "Gemini__ApiKey"; Placeholder = "your_gemini_api_key_here" },
    @{ Key = "ConnectionStrings__DefaultConnection"; Placeholder = "YOUR_DATABASE_HOST" }
)
foreach ($req in $requiredKeys) {
    if ($rootEnvContent -match ('(?m)^' + [regex]::Escape($req.Key) + '[ \t]*=[ \t]*([^\r\n]*)')) {
        $val = $matches[1].Trim()
        if ([string]::IsNullOrWhiteSpace($val) -or $val -like "*$($req.Placeholder)*") {
            Write-Err "$root\.env : $($req.Key) is empty or still a placeholder - fill in a real value before continuing."
            $needsSecrets = $true
        } else {
            Write-Ok "$($req.Key) is set"
        }
    } else {
        Write-Err "$root\.env : $($req.Key) is missing entirely."
        $needsSecrets = $true
    }
}

if ($needsSecrets) {
    Write-Host "`nStop: fill in the missing/placeholder values above in the .env file(s), then re-run this script." -ForegroundColor Red
    Write-Host "Get a Gemini key here: https://aistudio.google.com/app/apikey"
    Write-Host "Get your Supabase connection string from: Supabase dashboard -> Project Settings -> Database"
    exit 1
}

# ---------------------------------------------------------------------------
# 2. Stop previous service listeners on this project's dedicated development ports.
# ---------------------------------------------------------------------------
Write-Step 'Stopping services from previous runs'
foreach ($port in 5065, 5173, 8000, 8001) {
    $listeners = Get-NetTCPConnection -LocalPort $port -State Listen -ErrorAction SilentlyContinue |
        Sort-Object OwningProcess -Unique
    foreach ($listener in $listeners) {
        $process = Get-Process -Id $listener.OwningProcess -ErrorAction SilentlyContinue
        if ($process -and $process.ProcessName -in @('ShilpoHubBD.Api', 'dotnet', 'node', 'python')) {
            Write-Warn "Stopping $($process.ProcessName) PID $($process.Id) on port $port"
            Stop-Process -Id $process.Id -Force
        } elseif ($process) {
            throw "Port $port is used by unexpected process $($process.ProcessName) (PID $($process.Id))."
        }
    }
}
Start-Sleep -Seconds 2
Write-Ok 'Previous service listeners stopped'

# ---------------------------------------------------------------------------
# 3. Make sure dependencies are installed
# ---------------------------------------------------------------------------
Write-Step "Checking dependencies"

if (-not (Test-Path "$root\frontend\node_modules")) {
    Write-Warn "frontend\node_modules missing - running npm install (this can take a few minutes)"
    Push-Location "$root\frontend"
    try {
        npm.cmd install
        if ($LASTEXITCODE -ne 0) { throw "npm install failed." }
    } finally {
        Pop-Location
    }
} else {
    Write-Ok "frontend dependencies installed"
}

$ragVenvPython = "$root\rag\.venv\Scripts\python.exe"
if (-not (Test-Path $ragVenvPython) -and (Test-Path "$root\rag\venv\Scripts\python.exe")) {
    $ragVenvPython = "$root\rag\venv\Scripts\python.exe"
}
if (-not (Test-Path $ragVenvPython)) {
    Write-Warn "RAG environment missing - creating rag\.venv and installing requirements"
    py -3.12 -m venv "$root\rag\.venv"
    if ($LASTEXITCODE -ne 0) { throw "Creating the Python 3.12 environment failed." }
    & $ragVenvPython -m pip install -r "$root\rag\requirements.txt"
    if ($LASTEXITCODE -ne 0) { throw "Installing RAG requirements failed." }
} else {
    Write-Ok "rag venv exists"
}

# ---------------------------------------------------------------------------
# 4. Launch all 4 services in the background with separate logs
# ---------------------------------------------------------------------------
Write-Step "Starting services"

$logDirectory = Join-Path $root '.run-logs'
New-Item -ItemType Directory -Path $logDirectory -Force | Out-Null

function Start-ServiceProcess($name, $directory, $command) {
    $escapedDirectory = $directory.Replace("'", "''")
    $script = "Set-Location -LiteralPath '$escapedDirectory'; $command"
    $encoded = [Convert]::ToBase64String([Text.Encoding]::Unicode.GetBytes($script))
    $process = Start-Process powershell.exe -WindowStyle Hidden -PassThru -ArgumentList @(
        '-NoProfile', '-ExecutionPolicy', 'Bypass', '-EncodedCommand', $encoded
    ) -RedirectStandardOutput "$logDirectory\$name.stdout.log" -RedirectStandardError "$logDirectory\$name.stderr.log"
    Write-Ok "$name launching (PID $($process.Id)); logs: $logDirectory\$name.*.log"
}

$escapedPython = $ragVenvPython.Replace("'", "''")
$apiProject = (Join-Path $root 'backend\src\ShilpoHubBD.Api').Replace("'", "''")
# Keep the running API separate from normal Debug/Release build output.
Start-ServiceProcess 'backend' "$root\backend" "dotnet run --project '$apiProject' --configuration Development --launch-profile http"
Start-ServiceProcess 'frontend' "$root\frontend" 'npm.cmd run dev -- --host 127.0.0.1 --port 5173 --strictPort'
Start-ServiceProcess 'heritage-rag' "$root\rag" "& '$escapedPython' -m uvicorn main:app --host 127.0.0.1 --port 8000"
Start-ServiceProcess 'product-rag' "$root\rag" "& '$escapedPython' -m uvicorn product_main:app --host 127.0.0.1 --port 8001"

# ---------------------------------------------------------------------------
# 5. Wait for each service to respond; report failures accurately.
# ---------------------------------------------------------------------------
Write-Step 'Waiting for services (up to 3 minutes each)'

function Wait-Healthy($name, $url, $timeoutSec = 180) {
    $timer = [Diagnostics.Stopwatch]::StartNew()
    while ($timer.Elapsed.TotalSeconds -lt $timeoutSec) {
        try {
            $response = Invoke-WebRequest -Uri $url -UseBasicParsing -TimeoutSec 5
            if ($response.StatusCode -ge 200 -and $response.StatusCode -lt 300) {
                if ($name -eq 'Heritage RAG') {
                    $health = $response.Content | ConvertFrom-Json
                    if (-not $health.ready) { throw 'Heritage RAG is still initializing.' }
                }
                if ($name -eq 'Product Search RAG') {
                    $health = $response.Content | ConvertFrom-Json
                    if ($health.status -ne 'ok') { throw 'Product Search RAG is still initializing.' }
                }
                Write-Ok "$name is ready ($url)"
                return $true
            }
        } catch {
            # Allow startup to finish before retrying.
        }
        Start-Sleep -Seconds 3
    }
    Write-Err "$name did not become ready within $timeoutSec seconds. Check $logDirectory."
    return $false
}

$results = @(
    (Wait-Healthy 'Backend' 'http://127.0.0.1:5065/swagger/index.html')
    (Wait-Healthy 'Frontend' 'http://127.0.0.1:5173/')
    (Wait-Healthy 'Heritage RAG' 'http://127.0.0.1:8000/health')
    (Wait-Healthy 'Product Search RAG' 'http://127.0.0.1:8001/health')
)

if ($results -contains $false) {
    Write-Err "Some services failed to start. Check $logDirectory for details."
    exit 1
}

Write-Host "`nAll four services are ready. Open http://localhost:5173 to use the app." -ForegroundColor Cyan
Write-Host "Services run in the background. Logs: $logDirectory"
