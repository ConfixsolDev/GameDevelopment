@echo off
cd /d "%~dp0"
powershell -ExecutionPolicy Bypass -File start-tileserver.ps1
pause

