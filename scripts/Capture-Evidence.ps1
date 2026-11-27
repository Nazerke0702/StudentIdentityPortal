param(
  [string]$BaseUrl = "http://127.0.0.1:5188"
)

$ErrorActionPreference = "Stop"
$projectDir = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot ".."))
$scriptsDir = [System.IO.Path]::GetFullPath($PSScriptRoot)
$tempRunId = [Guid]::NewGuid().ToString("N")
$tempDir = [System.IO.Path]::GetFullPath((Join-Path $env:TEMP "StudentIdentityPortal-Evidence-$tempRunId"))
$dbPath = Join-Path $tempDir "evidence.db"
$stdoutPath = Join-Path $tempDir "server.stdout.log"
$stderrPath = Join-Path $tempDir "server.stderr.log"
$adminEmail = "evidence-admin-$tempRunId@example.test"
$random = [byte[]]::new(32)
$generator = [System.Security.Cryptography.RandomNumberGenerator]::Create()
try { $generator.GetBytes($random) } finally { $generator.Dispose() }
$adminPassword = "Aa1!" + [Convert]::ToBase64String($random).Replace("+", "x").Replace("/", "y").Replace("=", "z")

New-Item -ItemType Directory -Path $tempDir | Out-Null
$envNames = @(
  "ASPNETCORE_ENVIRONMENT", "DOTNET_ENVIRONMENT", "ASPNETCORE_URLS", "ConnectionStrings__DefaultConnection",
  "Admin__Email", "Admin__Password", "PORTAL_BASE_URL", "PORTAL_ADMIN_EMAIL",
  "PORTAL_ADMIN_PASSWORD"
)
$oldEnvironment = @{}
foreach ($name in $envNames) { $oldEnvironment[$name] = [Environment]::GetEnvironmentVariable($name, "Process") }
$server = $null

try {
  $env:ASPNETCORE_ENVIRONMENT = "Development"
  $env:DOTNET_ENVIRONMENT = "Development"
  $env:ASPNETCORE_URLS = $BaseUrl
  $env:ConnectionStrings__DefaultConnection = "Data Source=$dbPath"
  $env:Admin__Email = $adminEmail
  $env:Admin__Password = $adminPassword
  $env:PORTAL_BASE_URL = $BaseUrl
  $env:PORTAL_ADMIN_EMAIL = $adminEmail
  $env:PORTAL_ADMIN_PASSWORD = $adminPassword

  dotnet build (Join-Path $projectDir "StudentIdentityPortal.csproj") --no-restore --maxcpucount:1
  if ($LASTEXITCODE -ne 0) { throw "The app build failed." }

  $server = Start-Process -FilePath "dotnet" -ArgumentList @((Join-Path $projectDir "bin\Debug\net10.0\StudentIdentityPortal.dll")) -WorkingDirectory $projectDir -WindowStyle Hidden -PassThru -RedirectStandardOutput $stdoutPath -RedirectStandardError $stderrPath
  $ready = $false
  for ($attempt = 0; $attempt -lt 45; $attempt++) {
    if ($server.HasExited) { throw "The isolated Development server exited. Its logs were stored in a temporary folder." }
    try {
      $response = Invoke-WebRequest -Uri "$BaseUrl/Account/Login" -UseBasicParsing -TimeoutSec 2
      if ($response.StatusCode -eq 200) { $ready = $true; break }
    } catch {
      Start-Sleep -Milliseconds 500
    }
  }
  if (-not $ready) { throw "The isolated Development server did not become ready." }

  Push-Location $scriptsDir
  try {
    npm install --no-audit --no-fund
    if ($LASTEXITCODE -ne 0) { throw "Could not install the Playwright dependency." }
    npx playwright install chromium
    if ($LASTEXITCODE -ne 0) { throw "Could not install the Playwright Chromium browser." }
    npm run capture:evidence
    if ($LASTEXITCODE -ne 0) { throw "Playwright evidence capture failed." }
  } finally {
    Pop-Location
  }
}
finally {
  if ((Get-Location).Path -eq $scriptsDir) { Pop-Location }
  if ($server -and -not $server.HasExited) {
    Stop-Process -Id $server.Id -Force -ErrorAction SilentlyContinue
    Wait-Process -Id $server.Id -Timeout 10 -ErrorAction SilentlyContinue
  }
  foreach ($name in $envNames) {
    if ($null -eq $oldEnvironment[$name]) { Remove-Item "Env:$name" -ErrorAction SilentlyContinue }
    else { Set-Item -Path "Env:$name" -Value $oldEnvironment[$name] }
  }
  $tempRoot = [System.IO.Path]::GetFullPath($env:TEMP).TrimEnd([System.IO.Path]::DirectorySeparatorChar) + [System.IO.Path]::DirectorySeparatorChar
  if ($tempDir.StartsWith($tempRoot, [System.StringComparison]::OrdinalIgnoreCase) -and
      (Split-Path $tempDir -Leaf) -eq "StudentIdentityPortal-Evidence-$tempRunId") {
    Remove-Item -LiteralPath $tempDir -Recurse -Force -ErrorAction SilentlyContinue
  }
}
