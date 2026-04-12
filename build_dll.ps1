param(
    [string]$RootPath = $PSScriptRoot
)

$ErrorActionPreference = "Stop"

# Paths
$coreEngine = Join-Path $RootPath "CoreEngine"
$buildDir = Join-Path $coreEngine "build"
$uiBin = Join-Path $RootPath "UI_Application (WPF Core)\bin\Debug\net9.0-windows"

Write-Host "=== Building TypesettingCore.dll ===" -ForegroundColor Cyan
Write-Host "Root: $RootPath"
Write-Host "CoreEngine: $coreEngine"
Write-Host "Build: $buildDir"
Write-Host "UI Bin: $uiBin"
Write-Host ""

# Create directories
if (!(Test-Path $buildDir)) {
    New-Item -ItemType Directory -Path $buildDir -Force | Out-Null
    Write-Host "Created build directory" -ForegroundColor Green
}

# Find CMake
$cmakePaths = @(
    (Join-Path $RootPath "cmake-4.3.1\bin\cmake.exe"),
    "C:\Program Files\CMake\bin\cmake.exe",
    "C:\Program Files (x86)\CMake\bin\cmake.exe",
    "cmake.exe"
)

$cmake = $null
foreach ($path in $cmakePaths) {
    if (Test-Path $path) {
        $cmake = $path
        break
    }
}

if (!$cmake) {
    # Try to find cmake in PATH
    try {
        $cmake = (Get-Command cmake -ErrorAction Stop).Source
    } catch {
        Write-Error "CMake not found!"
        exit 1
    }
}

Write-Host "Found CMake: $cmake" -ForegroundColor Green

# Configure
Write-Host "`nConfiguring project..." -ForegroundColor Yellow
& $cmake -S $coreEngine -B $buildDir -G "Visual Studio 17 2022" -A x64 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Error "CMake configuration failed!"
    exit 1
}

# Build
Write-Host "`nBuilding project..." -ForegroundColor Yellow
& $cmake --build $buildDir --config Release 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Error "Build failed!"
    exit 1
}

# Find and copy DLL
Write-Host "`nLooking for DLL..." -ForegroundColor Yellow
$dll = Get-ChildItem -Path $buildDir -Recurse -Filter "TypesettingCore.dll" | Select-Object -First 1
if (!$dll) {
    Write-Error "TypesettingCore.dll not found!"
    exit 1
}

Write-Host "Found DLL: $($dll.FullName)" -ForegroundColor Green

# Copy to UI bin
if (!(Test-Path $uiBin)) {
    New-Item -ItemType Directory -Path $uiBin -Force | Out-Null
}

Copy-Item -Path $dll.FullName -Destination (Join-Path $uiBin "TypesettingCore.dll") -Force
Write-Host "`nDLL copied successfully!" -ForegroundColor Green
Write-Host "Source: $($dll.FullName)"
Write-Host "Destination: $uiBin\TypesettingCore.dll"

Write-Host "`n=== Build Complete ===" -ForegroundColor Cyan
