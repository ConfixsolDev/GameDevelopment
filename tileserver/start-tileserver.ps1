# TileServer-GL Startup Script for Strategy Game
# Location: E:\Strategy Game\GameDevelopment\tileserver\start-tileserver.ps1

# Find MBTiles files - search in wwwroot for all .mbtiles files
$projectRoot = "E:\Strategy Game\GameDevelopment"
$wwwrootPath = "$projectRoot\wwwroot"
$port = 8080

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Starting TileServer-GL" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Port: $port" -ForegroundColor Yellow
Write-Host "Searching in: $wwwrootPath" -ForegroundColor Yellow
Write-Host "Web UI: http://localhost:$port" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Get all .mbtiles files in wwwroot (including subdirectories)
$mbtilesFiles = Get-ChildItem -Path $wwwrootPath -Filter "*.mbtiles" -Recurse | Select-Object -ExpandProperty FullName

if ($mbtilesFiles.Count -eq 0) {
    Write-Host "ERROR: No .mbtiles files found in $wwwrootPath" -ForegroundColor Red
    Write-Host "Press any key to exit..."
    $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
    exit
}

Write-Host "Found MBTiles files:" -ForegroundColor Yellow
$index = 1
foreach ($file in $mbtilesFiles) {
    $folderName = Split-Path (Split-Path $file -Parent) -Leaf
    Write-Host "  [$index] $(Split-Path $file -Leaf) in $folderName" -ForegroundColor Cyan
    $index++
}
Write-Host ""

# Use the first MBTiles file found
$selectedFile = $mbtilesFiles[0]
$selectedFolder = Split-Path (Split-Path $selectedFile -Parent) -Leaf

Write-Host "Starting with: $selectedFolder/$(Split-Path $selectedFile -Leaf)" -ForegroundColor Green
Write-Host ""

# Start TileServer-GL with the selected MBTiles file
& tileserver-gl-light $selectedFile --port $port --verbose

# Note: Press Ctrl+C to stop the server

