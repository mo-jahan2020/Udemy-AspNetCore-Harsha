@echo off
REM ================================================================
REM TextToSqlApi - Run Script (Windows)
REM ----------------------------------------------------------
REM اجرا در Command Prompt یا PowerShell:
REM   run.bat
REM ================================================================

echo.
echo Starting TextToSqlApi...
echo.
echo Swagger UI: http://localhost:5000/swagger
echo Health:     http://localhost:5000/api/health
echo.
echo Press Ctrl+C to stop the service.
echo ----------------------------------------------------------
echo.

dotnet run --project src\TextToSqlApi

pause
