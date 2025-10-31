@echo off
echo ========================================
echo Updating Database Schema
echo ========================================
echo.
echo This will apply all pending migrations...
echo.
dotnet ef database update
echo.
echo ========================================
echo Database update complete!
echo ========================================
pause

