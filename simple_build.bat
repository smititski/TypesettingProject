@echo off
echo Building TypesettingCore.dll...

:: Try to find Visual Studio
if exist "C:\Program Files\Microsoft Visual Studio\2022\Community\VC\Auxiliary\Build\vcvars64.bat" (
    call "C:\Program Files\Microsoft Visual Studio\2022\Community\VC\Auxiliary\Build\vcvars64.bat"
) else if exist "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\VC\Auxiliary\Build\vcvars64.bat" (
    call "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\VC\Auxiliary\Build\vcvars64.bat"
) else (
    echo ERROR: Visual Studio not found
    exit /b 1
)

echo VS Environment set up
echo.

:: Create build directory
if not exist "CoreEngine\build" mkdir "CoreEngine\build"

:: Compile
echo Compiling...
cl /c /W4 /EHsc /O2 /I CoreEngine\include CoreEngine\src\InteropInterface.cpp /Fo:CoreEngine\build\InteropInterface.obj
if errorlevel 1 (
    echo Compilation failed
    exit /b 1
)

:: Link
echo Linking DLL...
link /DLL /OUT:CoreEngine\build\TypesettingCore.dll CoreEngine\build\InteropInterface.obj /EXPORT:LayoutText
if errorlevel 1 (
    echo Linking failed
    exit /b 1
)

echo.
echo SUCCESS: TypesettingCore.dll built
echo Location: CoreEngine\build\TypesettingCore.dll
echo.
pause
