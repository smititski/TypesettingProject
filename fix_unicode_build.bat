@echo off
setlocal enabledelayedexpansion

echo === Build Script for TypesettingCore.dll with Unicode Support ===
echo.

:: Find Visual Studio installation using vswhere
set "VSWHERE=%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe"
if not exist "%VSWHERE%" (
    echo ERROR: vswhere.exe not found at %VSWHERE%
    echo Trying alternative location...
    set "VSWHERE=%ProgramData%\Microsoft\VisualStudio\Setup\vswhere.exe"
)

if not exist "%VSWHERE%" (
    echo ERROR: vswhere.exe not found. Please install Visual Studio.
    exit /b 1
)

echo Found vswhere at: %VSWHERE%
echo.

:: Get Visual Studio installation path
for /f "delims=" %%i in ('"%VSWHERE%" -latest -products * -requires Microsoft.Component.MSBuild -property installationPath') do (
    set "VS_PATH=%%i"
)

if not defined VS_PATH (
    echo ERROR: No Visual Studio installation with MSBuild found.
    exit /b 1
)

echo Found Visual Studio at: %VS_PATH%
echo.

:: Find MSBuild
set "MSBUILD=%VS_PATH%\MSBuild\Current\Bin\MSBuild.exe"
if not exist "%MSBUILD%" (
    set "MSBUILD=%VS_PATH%\MSBuild\15.0\Bin\MSBuild.exe"
)

if not exist "%MSBUILD%" (
    echo ERROR: MSBuild.exe not found.
    exit /b 1
)

echo Found MSBuild at: %MSBUILD%
echo.

:: Set project directories
set "PROJECT_ROOT=%~dp0"
set "COREENGINE=%PROJECT_ROOT%CoreEngine"
set "BUILD_DIR=%COREENGINE%\build"
set "UI_BIN=%PROJECT_ROOT%UI_Application (WPF Core)\bin\Debug\net9.0-windows"

echo Project root: %PROJECT_ROOT%
echo CoreEngine: %COREENGINE%
echo Build dir: %BUILD_DIR%
echo.

:: Create build directory if not exists
if not exist "%BUILD_DIR%" mkdir "%BUILD_DIR%"

:: Check if we have a solution file or need to use CMake
echo Looking for solution files...
dir /b "%COREENGINE%\*.sln" 2>nul
if %errorlevel% == 0 (
    echo Found solution file(s)
    for %%f in ("%COREENGINE%\*.sln") do (
        echo Building solution: %%f
        "%MSBUILD%" "%%f" /p:Configuration=Release /p:Platform=x64 /p:OutDir="%BUILD_DIR%\bin\Release\\"
        if !errorlevel! neq 0 (
            echo ERROR: Build failed
            exit /b 1
        )
    )
) else (
    echo No solution file found. Using CMake...
    
    :: Find CMake
    set "CMAKE=%VS_PATH%\Common7\IDE\CommonExtensions\Microsoft\CMake\CMake\bin\cmake.exe"
    if not exist "%CMAKE%" (
        set "CMAKE=cmake.exe"
    )
    
    echo Using CMake: %CMAKE%
    
    :: Configure with CMake
    echo Configuring project...
    "%CMAKE%" -S "%COREENGINE%" -B "%BUILD_DIR%" -G "Visual Studio 17 2022" -A x64
    if !errorlevel! neq 0 (
        echo ERROR: CMake configuration failed
        exit /b 1
    )
    
    :: Build with CMake
    echo Building project...
    "%CMAKE%" --build "%BUILD_DIR%" --config Release
    if !errorlevel! neq 0 (
        echo ERROR: Build failed
        exit /b 1
    )
)

echo.
echo === Build completed successfully ===
echo.

:: Find and copy the DLL
echo Looking for TypesettingCore.dll...

set "DLL_FOUND="
for /r "%BUILD_DIR%" %%f in (TypesettingCore.dll) do (
    set "DLL_FOUND=%%f"
    echo Found DLL at: %%f
)

if not defined DLL_FOUND (
    echo ERROR: TypesettingCore.dll not found in build directory
    exit /b 1
)

echo.
echo Copying DLL to UI bin directory...
echo Source: %DLL_FOUND%
echo Destination: %UI_BIN%

if not exist "%UI_BIN%" mkdir "%UI_BIN%"

copy /Y "%DLL_FOUND%" "%UI_BIN%\TypesettingCore.dll"
if %errorlevel% neq 0 (
    echo ERROR: Failed to copy DLL
    exit /b 1
)

echo.
echo === DLL copied successfully ===
echo.
echo Build process complete! You can now run the application.
echo.
pause
