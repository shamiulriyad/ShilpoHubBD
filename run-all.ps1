<#
  run-all.ps1  -  One-command dev startup for ShilpoHubBD.

  Starts all 4 services, each in its own visible PowerShell window (so logs stay readable
  and closing one doesn't kill the others):
    1. Backend   (.NET API)          http://localhost:5065
    2. Frontend  (Vite/React)        http://localhost:5173
    3. Heritage RAG (Python)         http://localhost:8000
    4. Product Search RAG (Python)   http://localhost:8001

  What it does for you automatically:
    - Creates any missing .env file from its .env.example (you still must fill in real
      secrets - Gemini API key, DB connection - it will tell you exactly which ones).
    - Kills stray processes from a previous run that are still holding files/ports locked
      (the #1 cause of "Building..." or "Address already in use" hangs).
    - Creates the Python venv and installs dependencies if missing.
    - Waits for each service to actually respond before declaring it ready.

  Usage (from repo root):
    powershell -ExecutionPolicy Bypass -File .\run-all.ps1
#>

$ErrorActionPreference = "Stop"
$root = $PSScriptRoot

function Write-Step($msg) { Write-Host "`n==> $msg" -ForegroundColor Cyan }
function Write-Ok($msg)   { Write-Host "    OK: $msg" -ForegroundColor Green }
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
        Write-Warn "$($pair.Path) was missing - created from .env.example. You MUST edit it and fill in real values."
        $needsSecrets = $true
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
    if ($rootEnvContent -match [regex]::Escape($req.Key) + "=\s*(.*)") {
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
# 2. Kill stray processes from a previous run that are still holding locks/ports
# ---------------------------------------------------------------------------
Write-Step "Clearing stray processes from previous runs"

# .NET: kill any dotnet.exe whose command line is THIS project's API (not VS Code's build
# host or generic MSBuild node-reuse workers, which we must not touch).
Get-CimInstance Win32_Process -Filter "Name='dotnet.exe'" |
    Where-Object { $_.CommandLine -match "ShilpoHubBD\.Api" } |
    ForEach-Object {
        Write-Warn "Killing stale backend process PID $($_.ProcessId)"
        Stop-Process -Id $_.ProcessId -Force -ErrorAction SilentlyContinue
    }

# Python: kill any python.exe running uvicorn for main:app or product_main:app from this repo
Get-CimInstance Win32_Process -Filter "Name='python.exe'" |
    Where-Object { $_.CommandLine -match "uvicorn" -and ($_.CommandLine -match "main:app" -or $_.CommandLine -match "product_main:app") } |
    ForEach-Object {
        Write-Warn "Killing stale RAG process PID $($_.ProcessId)"
        Stop-Process -Id $_.ProcessId -Force -ErrorAction SilentlyContinue
    }

# Frontend: kill any stray vite dev server from this repo (avoids a silent port bump to 5174)
Get-CimInstance Win32_Process -Filter "Name='node.exe'" |
    Where-Object { $_.CommandLine -match "vite" -and $_.CommandLine -match [regex]::Escape("$root\frontend") } |
    ForEach-Object {
        Write-Warn "Killing stale frontend process PID $($_.ProcessId)"
        Stop-Process -Id $_.ProcessId -Force -ErrorAction SilentlyContinue
    }

# Safety net: whatever's still LISTENING on our 4 ports gets removed too, but only if it's
# one of our own runtimes (dotnet/node/python) - never touch an unrelated process.
foreach ($port in 5065, 5173, 8000, 8001) {
    Get-NetTCPConnection -LocalPort $port -State Listen -ErrorAction SilentlyContinue |
        ForEach-Object {
            $proc = Get-Process -Id $_.OwningProcess -ErrorAction SilentlyContinue
            if ($proc -and $proc.ProcessName -in @("dotnet", "node", "python")) {
                Write-Warn "Killing process still listening on port $port ($($proc.ProcessName), PID $($proc.Id))"
                Stop-Process -Id $proc.Id -Force -ErrorAction SilentlyContinue
            }
        }
}

Start-Sleep -Seconds 1
Write-Ok "Stray processes cleared"

# ---------------------------------------------------------------------------
# 3. Make sure dependencies are installed
# ---------------------------------------------------------------------------
Write-Step "Checking dependencies"

if (-not (Test-Path "$root\frontend\node_modules")) {
    Write-Warn "frontend\node_modules missing - running npm install (this can take a few minutes)"
    Push-Location "$root\frontend"; npm install; Pop-Location
} else {
    Write-Ok "frontend dependencies installed"
}

$ragVenvPython = "$root\rag\venv\Scripts\python.exe"
if (-not (Test-Path $ragVenvPython)) {
    Write-Warn "rag\venv missing - creating it and installing requirements (this can take a few minutes)"
    Push-Location "$root\rag"
    py -3.11 -m venv venv 2>$null
    if (-not (Test-Path $ragVenvPython)) { python -m venv venv }
    & "$root\rag\venv\Scripts\pip.exe" install -r requirements.txt
    Pop-Location
} else {
    Write-Ok "rag venv exists"
}

# ---------------------------------------------------------------------------
# 4. Launch all 4 services, each in its own window
# ---------------------------------------------------------------------------
Write-Step "Starting services"

Start-Process powershell -ArgumentList @(
    "-NoExit", "-Command",
    "cd '$root\backend'; Write-Host 'Backend API - http://localhost:5065/swagger' -ForegroundColor Cyan; dotnet run --project src/ShilpoHubBD.Api"
)
Write-Ok "Backend launching in its own window"

Start-Process powershell -ArgumentList @(
    "-NoExit", "-Command",
    "cd '$root\frontend'; Write-Host 'Frontend - http://localhost:5173' -ForegroundColor Cyan; npm run dev"
)
Write-Ok "Frontend launching in its own window"

Start-Process powershell -ArgumentList @(
    "-NoExit", "-Command",
    "cd '$root\rag'; Write-Host 'Heritage RAG - http://localhost:8000/docs' -ForegroundColor Cyan; .\venv\Scripts\python.exe -m uvicorn main:app --port 8000"
)
Write-Ok "Heritage RAG launching in its own window"

Start-Process powershell -ArgumentList @(
    "-NoExit", "-Command",
    "cd '$root\rag'; Write-Host 'Product Search RAG - http://localhost:8001/health' -ForegroundColor Cyan; .\venv\Scripts\python.exe -m uvicorn product_main:app --port 8001"
)
Write-Ok "Product Search RAG launching in its own window"

# ---------------------------------------------------------------------------
# 5. Wait for each to actually respond
# ---------------------------------------------------------------------------
Write-Step "Waiting for services to become healthy (up to 3 minutes each)"

function Wait-Healthy($name, $url, $timeoutSec = 180) {
    $elapsed = 0
    while ($elapsed -lt $timeoutSec) {
        try {
            $resp = Invoke-WebRequest -Uri $url -UseBasicParsing -TimeoutSec 5
            if ($resp.StatusCode -ge 200 -and $resp.StatusCode -lt 500) {
                Write-Ok "$name is up ($url)"
                return $true
            }
        } catch { }
        Start-Sleep -Seconds 3
        $elapsed += 3
    }
    Write-Err "$name did not respond within $timeoutSec s - check its window for errors."
    return $false
}

Wait-Healthy "Backend"             "http://localhost:5065/swagger/index.html"
Wait-Healthy "Frontend"            "http://localhost:5173/"
Wait-Healthy "Heritage RAG"        "http://localhost:8000/docs"
Wait-Healthy "Product Search RAG"  "http://localhost:8001/health"

Write-Host "`nAll set. Open http://localhost:5173 to use the app." -ForegroundColor Cyan
Write-Host "Each service is running in its own window - close a window (or Ctrl+C in it) to stop that service."
