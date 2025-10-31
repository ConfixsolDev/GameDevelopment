# TileServer-GL Startup Script for Strategy Game

# Find MBTiles files - search in wwwroot/maps for all .mbtiles files
$projectRoot = "E:\Strategy Game\GameDevelopment"
$mapsPath = "$projectRoot\wwwroot\maps"
$port = 8080

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Starting TileServer-GL" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Port: $port" -ForegroundColor Yellow
Write-Host "Searching in: $mapsPath" -ForegroundColor Yellow
Write-Host "Web UI: http://localhost:$port" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Get all .mbtiles files in maps directory (including subdirectories)
$mbtilesFiles = @(Get-ChildItem -Path $mapsPath -Filter "*.mbtiles" -Recurse)

if ($mbtilesFiles.Count -eq 0) {
    Write-Host "ERROR: No .mbtiles files found in $mapsPath" -ForegroundColor Red
    Write-Host "Press any key to exit..."
    $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
    exit
}

Write-Host "Found MBTiles files:" -ForegroundColor Yellow
$index = 1
foreach ($file in $mbtilesFiles) {
    $folderName = Split-Path $file.DirectoryName -Leaf
    Write-Host "  [$index] $($file.Name) in $folderName" -ForegroundColor Cyan
    $index++
}
Write-Host ""

Write-Host "Generating config.json..." -ForegroundColor Green

# Generate config.json dynamically
$configData = @{
    options = @{}
    styles = @{}
    data = @{}
}

# Add each MBTiles file to the data section
foreach ($file in $mbtilesFiles) {
    $mapId = [System.IO.Path]::GetFileNameWithoutExtension($file.Name)
    $configData.data[$mapId] = @{
        mbtiles = $file.FullName
    }
}

# Save config to tileserver directory
$configPath = "$projectRoot\tileserver\config.json"
$configData | ConvertTo-Json -Depth 10 | Set-Content -Path $configPath

Write-Host "Config saved to: $configPath" -ForegroundColor Cyan
Write-Host ""
Write-Host "Starting TileServer-GL with config.json..." -ForegroundColor Green
Write-Host ""

# Start TileServer-GL using the config file
Set-Location "$projectRoot\tileserver"
& tileserver-gl-light --port $port --verbose

# Note: Press Ctrl+C to stop the server

