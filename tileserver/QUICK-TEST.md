# Quick Test - TileServer-GL

## 🚀 Test Right Now (2 Minutes)

### Step 1: Start TileServer-GL (30 seconds)

Open PowerShell in this directory and run:
```powershell
.\start-tileserver.ps1
```

**OR** just double-click: `start-tileserver.bat`

### Step 2: Verify It's Running (30 seconds)

Open your browser and go to:
```
http://localhost:8080
```

✅ **Success:** You should see TileServer-GL UI with "map" listed

❌ **Problem:** If page doesn't load, check:
- Console window shows "Listening at http://[::]:8080/"
- No firewall blocking port 8080
- Try: `http://127.0.0.1:8080` instead

### Step 3: Test in Your Game (1 minute)

1. Keep TileServer-GL running (don't close console window)
2. Start your C# game application (F5 in Visual Studio)
3. Open game in browser
4. Click **Offline Controls** button (top-right corner)
5. Scroll down to **"Tile Server Mode"**
6. Select **"External (TileServer-GL - Faster ⚡)"**
7. Load a map (use map selector dropdown)

### Step 4: Verify It's Working

Press **F12** (DevTools) → **Network** tab → Filter by "png"

Look at tile requests:
- ✅ **Working:** URLs like `http://localhost:8080/styles/map/14/...`
- ❌ **Not Working:** URLs like `/gameplay/mbtiles/tile/...`

### Performance Check

Watch the **Time** column in Network tab:
- **TileServer-GL:** Should be **10-50ms** ⚡
- **C# Server:** Usually **100-500ms** 🐢

**That's it! You should notice tiles loading MUCH faster!**

---

## 🔄 To Switch Back

In Offline Controls panel:
- Change "Tile Server Mode" back to **"Local (C# Server)"**

---

## ❓ Quick FAQ

**Q: Do I need to start TileServer-GL every time?**  
A: Yes, for now. See `README.md` for Windows Service setup.

**Q: What if I forget to start it?**  
A: Game will detect it's not running and stay on C# server automatically.

**Q: Can I close the console window?**  
A: No - closing it stops TileServer-GL. Minimize it instead.

**Q: What's the black console window that appears?**  
A: That's TileServer-GL running. Keep it open while using external mode.

---

## 🎯 Expected Results

**Before (C# Server):**
- Tiles load in 100-500ms each
- Noticeable delay when panning/zooming
- ~20 tiles = 2-10 seconds to fully load

**After (TileServer-GL):**
- Tiles load in 10-50ms each ⚡
- Near-instant appearance
- ~20 tiles = 0.2-1 seconds to fully load

**Difference: 10-50x faster!**

---

For detailed instructions, see `TILESERVER-SETUP-GUIDE.md` in the parent directory.

