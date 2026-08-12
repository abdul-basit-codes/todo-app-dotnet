# Smoke test for TodoApp.
# 1) dotnet build
# 2) Start the server
# 3) powershell -ExecutionPolicy Bypass -File test.ps1

param([string]$BaseUrl = "http://localhost:5000")

$ErrorActionPreference = "Stop"
$failed = $false

function Check([string]$label, [bool]$cond) {
  if ($cond) { Write-Host "[PASS] $label" }
  else { Write-Host "[FAIL] $label"; $script:failed = $true }
}

Write-Host "== TodoApp smoke test =="

$health = Invoke-RestMethod "$BaseUrl/api/health"
Check "health endpoint" ($health.status -eq "ok")

$created = Invoke-RestMethod -Method Post -Uri "$BaseUrl/api/todos" -ContentType "application/json" -Body '{"title":"smoke test task"}'
Check "POST creates task" ($created.id -gt 0)
$id = $created.id

$summary = Invoke-RestMethod "$BaseUrl/api/todos/summary"
Check "summary endpoint" ($summary.total -ge 1)

$list = Invoke-RestMethod "$BaseUrl/api/todos"
Check "GET lists task" (($list | Where-Object { $_.id -eq $id }).Count -eq 1)

$updated = Invoke-RestMethod -Method Put -Uri "$BaseUrl/api/todos/$id" -ContentType "application/json" -Body '{"completed":true}'
Check "PUT marks complete" ($updated.completed -eq $true)

$deleted = Invoke-RestMethod -Method Delete -Uri "$BaseUrl/api/todos/$id"
Check "DELETE removes task" ($deleted.deleted -eq $true)

$after = Invoke-RestMethod "$BaseUrl/api/todos"
Check "GET no longer lists deleted" (($after | Where-Object { $_.id -eq $id }).Count -eq 0)

$html = (Invoke-WebRequest "$BaseUrl/index.html").Content
Check "serves index.html" ($html -match "TodoApp")

$css = (Invoke-WebRequest "$BaseUrl/css/site.css").Content
Check "serves css" ($css -match "progress-fill")

$js = (Invoke-WebRequest "$BaseUrl/js/app.js").Content
Check "serves js" ($js -match "loadTasks")

if ($script:failed) { Write-Host "RESULT: FAILED"; exit 1 }
Write-Host "RESULT: ALL PASSED"
