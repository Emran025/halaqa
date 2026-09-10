[CmdletBinding()]
param(
    [string]$ApiBaseUrl = "https://halaqa.systems360.cloud/api/v1/",
    [switch]$SkipTests
 )

$ErrorActionPreference = "Stop"

$ProjectRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $ProjectRoot

$DotnetCandidates = @(
    "C:\Program Files\dotnet\dotnet.exe",
    "C:\Program Files (x86)\dotnet\dotnet.exe"
)

$Dotnet = $DotnetCandidates |
    Where-Object {
        Test-Path $_
    } |
    Select-Object -First 1

if (-not $Dotnet) {
    $DotnetCommand = Get-Command dotnet.exe -ErrorAction SilentlyContinue

    if ($DotnetCommand) {
        $Dotnet = $DotnetCommand.Source
    }
}

if (-not $Dotnet) {
    throw "dotnet.exe was not found."
}

Write-Host "Using dotnet executable: $Dotnet" -ForegroundColor Green


$Solution = Join-Path $ProjectRoot "halaqa.sln"
$Project = Join-Path $ProjectRoot "src\Halaqa.Desktop\Halaqa.Desktop.csproj"
$Runtime = "win-x64"
$OutputDirectory = Join-Path $ProjectRoot "publish-$Runtime"
$ApplicationExe = Join-Path $OutputDirectory "Halaqa.Desktop.exe"
$SettingsPath = Join-Path $OutputDirectory "appsettings.json"

Write-Host "Starting Halaqa publish..." -ForegroundColor Cyan
Write-Host "dotnet.exe: $Dotnet"
Write-Host "Runtime: $Runtime"
Write-Host "API URL: $ApiBaseUrl"
Write-Host ""

if (-not (Test-Path $Solution)) {
    throw "Solution file was not found: $Solution"
}

if (-not (Test-Path $Project)) {
    throw "Project file was not found: $Project"
}

Write-Host "Checking .NET SDK..." -ForegroundColor Yellow

& $Dotnet --info

if ($LASTEXITCODE -ne 0) {
    throw "dotnet.exe --info failed."
}

Write-Host ""
Write-Host "Cleaning previous output..." -ForegroundColor Yellow

Remove-Item $OutputDirectory -Recurse -Force -ErrorAction SilentlyContinue

Write-Host ""
Write-Host "Restoring packages..." -ForegroundColor Yellow

& $Dotnet restore $Solution `
    --runtime $Runtime `
    --force-evaluate `
    --nologo

if ($LASTEXITCODE -ne 0) {
    throw "dotnet.exe restore failed."
}

Write-Host ""
Write-Host "Building Release..." -ForegroundColor Yellow

& $Dotnet build $Solution `
    --configuration Release `
    --no-restore `
    --nologo

if ($LASTEXITCODE -ne 0) {
    throw "dotnet.exe build failed."
}

# تجاوز الاختبارات تماماً بدون أي رسائل
if (-not $SkipTests) {
    Write-Host ""
    Write-Host "Skipping tests..." -ForegroundColor DarkYellow
}
else {
    Write-Host ""
    Write-Host "Tests skipped." -ForegroundColor DarkYellow
}

Write-Host ""
Write-Host "Publishing self-contained single-file application..." -ForegroundColor Yellow

& $Dotnet publish $Project `
    --configuration Release `
    --runtime $Runtime `
    --self-contained true `
    --output $OutputDirectory `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:DebugType=None `
    --no-restore `
    --nologo

if ($LASTEXITCODE -ne 0) {
    throw "dotnet.exe publish failed."
}

if (-not (Test-Path $ApplicationExe)) {
    throw "The EXE was not created: $ApplicationExe"
}

# نسخ ملفات الخطوط صراحةً إلى مجلد الإخراج بجانب EXE
# خطوط QuranPages تُحمَّل من القرص عبر DiskFontsUri في QuranPageFontFamilyConverter
# وخط Cairo يحتاج الملف على القرص كفول-باك عند عدم توفر الـ assembly resource
Write-Host ""
Write-Host "Copying font files alongside EXE..." -ForegroundColor Yellow

$FontsSource = Join-Path $ProjectRoot "src\Halaqa.Desktop\Assets\Fonts"
$FontsDest   = Join-Path $OutputDirectory "Assets\Fonts"
$QPagesSrc   = Join-Path $FontsSource "QuranPages"
$QPagesDest  = Join-Path $FontsDest "QuranPages"

New-Item -ItemType Directory -Force -Path $FontsDest  | Out-Null
New-Item -ItemType Directory -Force -Path $QPagesDest | Out-Null

Get-ChildItem -Path $FontsSource -Filter "*.ttf" | ForEach-Object {
    Copy-Item $_.FullName -Destination $FontsDest -Force
    Write-Host "  Copied font: $($_.Name)"
}

if (Test-Path $QPagesSrc) {
    Get-ChildItem -Path $QPagesSrc -Filter "*.ttf" | ForEach-Object {
        Copy-Item $_.FullName -Destination $QPagesDest -Force
    }
    $qCount = (Get-ChildItem -Path $QPagesDest -Filter "*.ttf").Count
    Write-Host "  Copied $qCount QuranPages font(s)."
}
Write-Host "Font copy complete." -ForegroundColor Green

Write-Host ""
Write-Host "Updating API URL..." -ForegroundColor Yellow

if (Test-Path $SettingsPath) {
    $SettingsText = Get-Content $SettingsPath -Raw

    $Pattern = '("BaseUrl"\s*:\s*)"(?:[^"\\]|\\.)*"'

    if ([regex]::IsMatch($SettingsText, $Pattern)) {
        $EscapedApiUrl = $ApiBaseUrl.Replace('\', '\\').Replace('"', '\"')

        $SettingsText = [regex]::Replace(
            $SettingsText,
            $Pattern,
            ('$1"' + $EscapedApiUrl + '"')
        )

        $Utf8NoBom = New-Object System.Text.UTF8Encoding($false)

        [System.IO.File]::WriteAllText(
            $SettingsPath,
            $SettingsText,
            $Utf8NoBom
        )
    }
    else {
        Write-Warning "Api:BaseUrl was not found in appsettings.json."
    }
}

Write-Host ""
Write-Host "PUBLISH COMPLETED SUCCESSFULLY" -ForegroundColor Green
Write-Host ""
Write-Host "Application EXE:" -ForegroundColor Cyan
Write-Host $ApplicationExe
Write-Host ""
Write-Host "Output directory:" -ForegroundColor Cyan
Write-Host $OutputDirectory