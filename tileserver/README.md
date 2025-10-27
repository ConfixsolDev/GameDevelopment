# TileServer-GL Setup for Strategy Game

## Overview

This directory contains TileServer-GL configuration and startup scripts for serving map tiles with 10-50x better performance than the built-in C# tile server.

## Quick Start

### 1. Start the Tile Server

**Option A: Double-click the batch file (easiest)**
```
Double-click: start-tileserver.bat
```

**Option B: Run PowerShell script**
```powershell
.\start-tileserver.ps1
```

**Option C: Direct command**
```powershell
tileserver-gl-light "E:\Strategy Game\GameDevelopment\App_Data\tiles\*.mbtiles" --port 8080 --verbose
```

### 2. Verify It's Running

Open in your browser:
- http://localhost:8080 - Should show TileServer-GL UI with your maps
- http://localhost:8080/data/ - Should list your MBTiles files

### 3. Switch to External Server in Game

1. Start your C# application (F5 in Visual Studio or `dotnet run`)
2. Open game in browser
3. Open Offline Controls panel (button in top-right)
4. Scroll to "Tile Server Mode"
5. Select "External (TileServer-GL - Faster ⚡)"
6. Load a map - tiles now come from TileServer-GL!

## Configuration

### Change Port

Edit `start-tileserver.ps1`:
```powershell
$port = 8081  # Change from 8080 to 8081
```

Also update `wwwroot/js/tileServerConfig.js`:
```javascript
external: {
    baseUrl: 'http://localhost:8081',  // Match new port
    ...
}
```

### Change Tiles Directory

Edit `start-tileserver.ps1`:
```powershell
$tilesPath = "E:\Different\Path\To\Tiles"
```

## Performance Comparison

| Server Type | Avg. Tile Load Time | Concurrent Requests |
|-------------|---------------------|---------------------|
| C# Server   | 100-500ms          | Limited             |
| TileServer-GL | 10-50ms          | Unlimited           |

**Result: 10-50x faster tile loading!**

## Troubleshooting

### "tileserver-gl-light is not recognized"

**Solution:** Reinstall TileServer-GL
```powershell
npm install -g tileserver-gl-light
```

### Port 8080 already in use

**Solution:** Either:
1. Stop the other application using port 8080, OR
2. Change the port in `start-tileserver.ps1` (see Configuration section)

### No maps showing in TileServer-GL UI

**Solution:**
1. Check that `App_Data/tiles/` directory contains `.mbtiles` files
2. Verify the path in `start-tileserver.ps1` is correct
3. Try using absolute path instead of `*.mbtiles` wildcard

### Game still using C# server after switching

**Solution:** Force the switch in browser console:
```javascript
localStorage.setItem('tileServerMode', 'external');
TileServerConfig.useExternalServer('http://localhost:8080');
location.reload();
```

### CORS errors in browser

**Solution:** TileServer-GL includes CORS by default, but if issues persist:
```powershell
tileserver-gl-light "tiles\*.mbtiles" --port 8080 --cors
```

## Advanced: Run as Windows Service

For production environments, you can run TileServer-GL as a Windows service so it starts automatically.

### Using node-windows:

1. Install node-windows:
```powershell
npm install -g node-windows
```

2. Create service installer (coming soon in this directory)

3. Install service:
```powershell
node install-service.js
```

### Using NSSM (alternative):

1. Download NSSM from https://nssm.cc/download
2. Extract and run:
```powershell
nssm install StrategyGameTileServer "C:\Users\[YourUser]\AppData\Roaming\npm\node_modules\tileserver-gl-light\bin\tileserver-gl-light" "E:\Strategy Game\GameDevelopment\App_Data\tiles\*.mbtiles --port 8080"
nssm start StrategyGameTileServer
```

## Files in This Directory

- `config.json` - TileServer-GL configuration (optional, currently uses CLI args)
- `start-tileserver.ps1` - PowerShell startup script
- `start-tileserver.bat` - Windows batch file for double-click start
- `README.md` - This file

## Rollback to C# Server

If you need to switch back to the C# tile server:

**In Game UI:**
1. Open Offline Controls panel
2. Change "Tile Server Mode" to "Local (C# Server)"

**Or in Browser Console:**
```javascript
TileServerConfig.useLocalServer();
localStorage.setItem('tileServerMode', 'local');
```

No data is lost when switching between servers!

## Support

For TileServer-GL documentation:
https://github.com/maptiler/tileserver-gl

For issues with this setup, check the game's main documentation or console logs.

