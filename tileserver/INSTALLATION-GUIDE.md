# TileServer-GL Installation Guide for New Machine

## Complete Installation Commands - Copy & Paste Ready

This guide will get TileServer-GL running on a fresh Windows machine in under 5 minutes.

---

## 🔧 Prerequisites

### Step 1: Install Node.js (if not already installed)

**Check if Node.js is installed:**
```powershell
node --version
npm --version
```

**If not installed:**
1. Download Node.js LTS from: https://nodejs.org/en/download/
2. Run the installer (node-vXX.XX.X-x64.msi)
3. Accept all defaults
4. Restart PowerShell/Terminal

**Verify installation:**
```powershell
node --version
npm --version
```

Expected output:
```
v22.20.0 (or similar)
10.9.3 (or similar)
```

---

## 📦 Installation Steps

### Step 2: Install TileServer-GL Globally

Open PowerShell as Administrator (right-click → Run as Administrator) and run:

```powershell
npm install -g tileserver-gl-light
```

**Verify installation:**
```powershell
tileserver-gl-light --version
```

Expected output:
```
5.4.0 (or similar)
```

✅ **Installation complete!**

---

## 🚀 Setup for Your Project

### Step 3: Navigate to Project Directory

```powershell
cd "E:\Strategy Game\GameDevelopment"
```

### Step 4: Verify MBTiles Files Exist

```powershell
Get-ChildItem -Path "wwwroot" -Filter "*.mbtiles" -Recurse
```

Expected: You should see your MBTiles files listed (e.g., map.mbtiles)

### Step 5: Start TileServer-GL

**Option A: Using the startup script (recommended)**
```powershell
cd tileserver
.\start-tileserver.bat
```

**Option B: Direct command**
```powershell
cd "E:\Strategy Game\GameDevelopment\wwwroot"
tileserver-gl-light "littleaden-20251008172253139\map.mbtiles" --port 8080 --verbose
```

**Expected output:**
```
Starting tileserver-gl-light v5.4.0
[INFO] Automatically creating config file
Listening at http://0.0.0.0:8080/
Startup complete
```

### Step 6: Verify TileServer-GL is Running

Open browser and go to:
```
http://localhost:8080
```

You should see the TileServer-GL web UI with your map listed.

---

## 🎮 Configure Your Game

### Step 7: Start Your Game Application

```powershell
cd "E:\Strategy Game\GameDevelopment"
dotnet run
```

Or press **F5** in Visual Studio

### Step 8: Switch to TileServer-GL in Game

1. Open your game in browser
2. Open the **Region Controls** panel (left side)
3. Find **"Tile Server Mode"** dropdown
4. Select **"External (TileServer-GL ⚡)"**
5. You should see: **"Switched to TileServer-GL (10-50x faster!)"**

---

## ✅ Verification Commands

### Check if TileServer-GL is running:
```powershell
# Test if port 8080 is responding
Test-NetConnection -ComputerName localhost -Port 8080
```

### Check TileServer-GL process:
```powershell
Get-Process | Where-Object {$_.ProcessName -like "*node*"}
```

### Test tile endpoint:
```powershell
curl http://localhost:8080/data/map/14/8192/4096.png
```

Should return tile data (not 404)

---

## 🔄 Daily Usage Commands

### Start TileServer-GL:
```powershell
cd "E:\Strategy Game\GameDevelopment\tileserver"
.\start-tileserver.bat
```

### Stop TileServer-GL:
Press **Ctrl+C** in the TileServer-GL console window

### Restart TileServer-GL:
1. Press **Ctrl+C** to stop
2. Run `.\start-tileserver.bat` again

---

## 🐛 Troubleshooting Commands

### If Node.js is not recognized:
```powershell
# Check PATH
$env:Path

# Manually add Node.js to PATH (replace XX.XX.X with your version)
$env:Path += ";C:\Program Files\nodejs\"

# Verify
node --version
```

### If TileServer-GL is not recognized:
```powershell
# Check global npm packages
npm list -g --depth=0

# Reinstall TileServer-GL
npm uninstall -g tileserver-gl-light
npm install -g tileserver-gl-light
```

### If port 8080 is already in use:
```powershell
# Find what's using port 8080
netstat -ano | findstr :8080

# Kill the process (replace XXXX with PID from above)
taskkill /PID XXXX /F

# Or change port in start-tileserver.ps1 to 8081
```

### If tiles are not loading:
```powershell
# Check if MBTiles files exist
Get-ChildItem -Path "E:\Strategy Game\GameDevelopment\wwwroot" -Filter "*.mbtiles" -Recurse

# Verify TileServer-GL can read them
cd "E:\Strategy Game\GameDevelopment\wwwroot\littleaden-20251008172253139"
tileserver-gl-light "map.mbtiles" --port 8080 --verbose
```

### Reset to C# Server (if issues):
In browser console (F12):
```javascript
TileServerConfig.useLocalServer();
localStorage.setItem('tileServerMode', 'local');
location.reload();
```

---

## 📋 Quick Reference Card

### Installation (One-Time):
```powershell
# 1. Install Node.js from https://nodejs.org/
# 2. Install TileServer-GL
npm install -g tileserver-gl-light
```

### Daily Usage:
```powershell
# Start TileServer-GL
cd "E:\Strategy Game\GameDevelopment\tileserver"
.\start-tileserver.bat

# Start your game
cd "E:\Strategy Game\GameDevelopment"
dotnet run

# In game: Switch to External mode in Region Controls panel
```

### URLs to Remember:
- **TileServer-GL UI:** http://localhost:8080
- **Game:** http://localhost:5001/GamePlay (or your port)
- **Tile endpoint:** http://localhost:8080/data/map/{z}/{x}/{y}.png

---

## 🆘 Common Errors and Fixes

### Error: "npm is not recognized"
**Fix:** Node.js not installed or not in PATH
```powershell
# Download and install Node.js from https://nodejs.org/
# Restart PowerShell after installation
```

### Error: "tileserver-gl-light is not recognized"
**Fix:** Not installed globally or PATH issue
```powershell
npm install -g tileserver-gl-light
```

### Error: "Port 8080 already in use"
**Fix:** Change port or kill existing process
```powershell
# Option 1: Change port in tileserver/start-tileserver.ps1
$port = 8081

# Option 2: Kill process using port 8080
netstat -ano | findstr :8080
taskkill /PID [PID_NUMBER] /F
```

### Error: "ENOENT: no such file or directory"
**Fix:** MBTiles file path is wrong
```powershell
# Verify files exist
Get-ChildItem -Path "wwwroot" -Filter "*.mbtiles" -Recurse

# Update path in tileserver/start-tileserver.ps1
```

### Error: Tiles show 404 in game
**Fix:** Wrong endpoint or mapId
- Check TileServer-GL console - it shows the correct URL
- Verify tiles load at: http://localhost:8080/data/map/14/8192/4096.png

---

## 📦 Complete Installation Script

Copy this entire block and run in PowerShell:

```powershell
# Complete TileServer-GL Setup Script
# Run this on a fresh machine

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "TileServer-GL Installation" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan

# Check Node.js
Write-Host "`nStep 1: Checking Node.js..." -ForegroundColor Yellow
try {
    $nodeVersion = node --version
    $npmVersion = npm --version
    Write-Host "✅ Node.js installed: $nodeVersion" -ForegroundColor Green
    Write-Host "✅ npm installed: $npmVersion" -ForegroundColor Green
} catch {
    Write-Host "❌ Node.js not found!" -ForegroundColor Red
    Write-Host "Please install Node.js from https://nodejs.org/" -ForegroundColor Yellow
    exit
}

# Install TileServer-GL
Write-Host "`nStep 2: Installing TileServer-GL..." -ForegroundColor Yellow
npm install -g tileserver-gl-light

# Verify installation
Write-Host "`nStep 3: Verifying installation..." -ForegroundColor Yellow
$version = tileserver-gl-light --version
Write-Host "✅ TileServer-GL installed: v$version" -ForegroundColor Green

# Navigate to project
Write-Host "`nStep 4: Navigating to project..." -ForegroundColor Yellow
cd "E:\Strategy Game\GameDevelopment"

# Check MBTiles files
Write-Host "`nStep 5: Checking for MBTiles files..." -ForegroundColor Yellow
$mbtilesFiles = Get-ChildItem -Path "wwwroot" -Filter "*.mbtiles" -Recurse
if ($mbtilesFiles.Count -gt 0) {
    Write-Host "✅ Found $($mbtilesFiles.Count) MBTiles file(s):" -ForegroundColor Green
    foreach ($file in $mbtilesFiles) {
        Write-Host "   - $($file.FullName)" -ForegroundColor Cyan
    }
} else {
    Write-Host "⚠️ No MBTiles files found in wwwroot" -ForegroundColor Yellow
}

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "✅ Installation Complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "`nNext steps:" -ForegroundColor Yellow
Write-Host "1. cd tileserver" -ForegroundColor White
Write-Host "2. .\start-tileserver.bat" -ForegroundColor White
Write-Host "3. Open http://localhost:8080 in browser" -ForegroundColor White
Write-Host "4. Start your game and switch to External mode" -ForegroundColor White
```

---

## 🎯 Success Checklist

After running all commands, verify:

- [ ] Node.js installed: `node --version` shows version
- [ ] npm installed: `npm --version` shows version
- [ ] TileServer-GL installed: `tileserver-gl-light --version` shows version
- [ ] TileServer-GL starts: `.\start-tileserver.bat` runs without errors
- [ ] Web UI accessible: http://localhost:8080 shows map list
- [ ] Game connects: Switching to External mode shows success notification
- [ ] Tiles load: Network tab shows tiles from localhost:8080
- [ ] Performance: Tiles load in 10-50ms (vs 100-500ms before)

**All checked? You're done! 🎉**

---

## 📞 Support

If you encounter issues not covered here:

1. Check the TileServer-GL console for error messages
2. Check browser console (F12) for JavaScript errors
3. Verify files exist: `Get-ChildItem -Path "wwwroot" -Filter "*.mbtiles" -Recurse`
4. Test TileServer-GL directly: http://localhost:8080/data/map/14/8192/4096.png

---

*Installation guide for TileServer-GL v5.4.0*  
*Last updated: October 27, 2025*

