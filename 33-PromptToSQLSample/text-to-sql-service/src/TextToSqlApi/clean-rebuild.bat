@echo off
REM ================================================================
REM TextToSqlApi - Clean and Rebuild Script (Windows)
REM ----------------------------------------------------------
REM اجرا در Command Prompt یا PowerShell:
REM   clean-rebuild.bat
REM ================================================================

setlocal enabledelayedexpansion

echo.
echo [1/5] Cleaning bin and obj folders...

REM پاک‌سازی پوشه‌های bin و obj در تمام زیرپوشه‌ها
for /d /r %%i in (bin obj) do (
    if exist "%%i" (
        echo   Removing: %%i
        rmdir /s /q "%%i" 2>nul
    )
)

echo.
echo [2/5] Cleaning NuGet cache...
dotnet nuget locals all --clear

echo.
echo [3/5] Restoring packages...
dotnet restore
if errorlevel 1 (
    echo.
    echo ERROR: dotnet restore failed!
    pause
    exit /b 1
)

echo.
echo [4/5] Building project...
dotnet build --no-restore
if errorlevel 1 (
    echo.
    echo ERROR: dotnet build failed!
    pause
    exit /b 1
)

echo.
echo [5/5] Build succeeded!
echo.
echo To run the project:
echo   dotnet run --project src\TextToSqlApi
echo.
echo Swagger will be available at: http://localhost:5000/swagger
echo.
pause
