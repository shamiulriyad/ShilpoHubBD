$ErrorActionPreference = 'Stop'
Set-Location -LiteralPath $PSScriptRoot
$runtime = Join-Path $env:LOCALAPPDATA 'Programs\Python\Python312\python.exe'
& $runtime -m uvicorn main:app --host 127.0.0.1 --port 8000
