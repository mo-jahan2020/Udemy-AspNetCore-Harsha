@echo off
REM ================================================================
REM TextToSqlApi - Run Tests Script (Windows)
REM ----------------------------------------------------------
REM اجرا در Command Prompt یا PowerShell:
REM   test.bat
REM ================================================================

echo.
echo Running TextToSqlApi tests...
echo ----------------------------------------------------------
echo.

dotnet test --verbosity normal

if errorlevel 1 (
    echo.
    echo ERROR: Some tests failed!
    pause
    exit /b 1
)

echo.
echo All tests passed successfully!
pause
