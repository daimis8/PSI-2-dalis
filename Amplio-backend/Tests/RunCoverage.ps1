# RunCoverage.ps1
# -----------------------
# Runs tests, collects coverage, generates HTML, opens report

# -----------------------
# 1. Determine script folder
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path

# -----------------------
# 2. Set paths
$coverageDir = Join-Path $scriptDir "coverage-report"
$testProjectDir = $scriptDir  # assuming this script is inside your test project folder
$testResultsDir = Join-Path $testProjectDir "TestResults"

# -----------------------
# 3. Clean old coverage HTML folder
if (Test-Path $coverageDir) {
    Write-Host "Cleaning old coverage report..."
    Remove-Item -Recurse -Force $coverageDir
}

# -----------------------
# 4. Run tests with Coverlet
Write-Host "Running tests with Coverlet coverage..."
dotnet test $testProjectDir --collect:"XPlat Code Coverage"

# -----------------------
# 5. Find the latest coverage XML
if (-Not (Test-Path $testResultsDir)) {
    Write-Host "Error: TestResults folder not found!"
    exit 1
}

$latestTestResult = Get-ChildItem $testResultsDir -Directory | Sort-Object LastWriteTime -Descending | Select-Object -First 1

if (-Not $latestTestResult) {
    Write-Host "Error: No test result folders found!"
    exit 1
}

$coverageXml = Join-Path $latestTestResult.FullName "coverage.cobertura.xml"

if (-Not (Test-Path $coverageXml)) {
    Write-Host "Error: Coverage XML file not found!"
    exit 1
}

# -----------------------
# 6. Generate HTML report
Write-Host "Generating HTML report..."
reportgenerator -reports:$coverageXml -targetdir:$coverageDir -reporttypes:Html

# -----------------------
# 7. Open report in default browser
$indexHtml = Join-Path $coverageDir "index.html"
if (Test-Path $indexHtml) {
    Write-Host "Opening coverage report..."
    Start-Process $indexHtml
} else {
    Write-Host "Error: HTML report not found!"
}
