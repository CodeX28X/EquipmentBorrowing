param([string]$PsqlPath)
$ErrorActionPreference = 'Stop'
$root = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
Set-Location $root
. .\scripts\lab5\Use-AcceptanceDatabase.ps1
$b = [System.Data.Common.DbConnectionStringBuilder]::new()
$b.set_ConnectionString($env:EQUIPMENTBORROWING_CONNECTION_STRING)
if ($b.get_Item('Database') -ne 'equipment_borrowing_lab5_acceptance') { throw 'Wrong database' }
if (!$PsqlPath) {
  $p = Get-Command psql.exe -ErrorAction SilentlyContinue
  if ($p) { $PsqlPath = $p.Source }
  else { $PsqlPath = (Get-ChildItem "$env:ProgramFiles\PostgreSQL\*\bin\psql.exe" -ErrorAction SilentlyContinue | Select-Object -Last 1).FullName }
}
if (!$PsqlPath -or !(Test-Path $PsqlPath)) { throw 'psql.exe not found; supply -PsqlPath' }
$vars = @{'PGHOST'='Host'; 'PGPORT'='Port'; 'PGUSER'='Username'; 'PGPASSWORD'='Password'; 'PGDATABASE'='Database'}
$old = @{}
try {
  foreach ($k in $vars.Keys) {
    $old[$k] = [Environment]::GetEnvironmentVariable($k,'Process')
    if ($b.ContainsKey($vars[$k])) { [Environment]::SetEnvironmentVariable($k,[string]$b.get_Item($vars[$k]),'Process') }
  }
  $env:LAB5_PSQL = $PsqlPath
  py -3 scripts/lab5/run_acceptance.py
  if ($LASTEXITCODE -ne 0) { throw 'Acceptance runner reported failures; inspect evidence.' }
} finally {
  foreach ($k in $old.Keys) { [Environment]::SetEnvironmentVariable($k,$old[$k],'Process') }
  Remove-Item Env:LAB5_PSQL -ErrorAction SilentlyContinue
  $env:EQUIPMENTBORROWING_CONNECTION_STRING = [Environment]::GetEnvironmentVariable('EQUIPMENTBORROWING_CONNECTION_STRING','User')
}
