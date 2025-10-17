# 🚀 Quick Start: Directory Tiles

## ✅ What Changed?

The offline map system now uses **directory tiles** instead of MBTiles database!

### Old System:
```
App_Data/tiles/map.mbtiles  (single database file)
```

### New System:
```
App_Data/tiles/
├── 13/
│   └── 4915/
│       └── 3207.png
└── 14/
    └── 9830/
        └── 6414.png
```

---

## 🎯 Quick Test (3 Steps)

### Step 1: Use Online Tiles (Immediate)
1. Run your application
2. Go to **GamePlay** page
3. Open **"Region Controls"** panel (left side)
4. Change **"Basemap"** to: **"🌐 Online Test (OSM)"**
5. ✅ **You should see Little Aden, Yemen!**

### Step 2: Download Tiles (5 minutes)
```powershell
# Run the provided script
powershell -ExecutionPolicy Bypass .\download_tiles.ps1

# Choose option 1 or 2 when prompted
# Wait for download to complete
```

### Step 3: Use Offline Tiles
1. Refresh GamePlay page
2. Change basemap back to **"Offline Map"**
3. ✅ **Map now works offline!**

---

## 📦 What's Included?

### Files Created:
- ✅ `Services/OfflineMapService.cs` - **Updated** to support directory tiles
- ✅ `App_Data/tiles/README_TILES.md` - Complete tile documentation
- ✅ `download_tiles.ps1` - PowerShell script to download tiles
- ✅ `LITTLE_ADEN_MAP_SETUP.md` - Updated setup guide
- ✅ `TILES_QUICKSTART.md` - This quick start guide

### Features:
- ✅ Supports **directory tiles** (`{z}/{x}/{y}.png`)
- ✅ Supports **MBTiles** as fallback
- ✅ Auto-detects PNG, JPG, WEBP formats
- ✅ Handles both XYZ and TMS coordinate systems
- ✅ Configured for **Little Aden, Yemen** (12.8°N, 45.0°E)

---

## 📥 Download Options

### Option 1: PowerShell Script (Easiest)
```powershell
# In project root
powershell -ExecutionPolicy Bypass .\download_tiles.ps1

# Choose zoom levels:
# 1 = Quick test (25 tiles, 2MB)
# 2 = Recommended (125 tiles, 10MB) ⭐
# 3 = Detailed (525 tiles, 40MB)
```

### Option 2: MOBAC (Best Quality)
1. Download: https://mobac.sourceforge.io/
2. Select OpenStreetMap
3. Draw box around Little Aden, Yemen
4. Choose zoom 10-15
5. Select "OSM PNG" format
6. Extract to `App_Data/tiles/`

---

## 🗂️ Directory Structure

Your tiles should look like this:

```
App_Data/tiles/
├── 13/              ← Zoom level
│   ├── 4913/        ← X coordinate
│   │   ├── 3205.png ← Y coordinate (tile image)
│   │   ├── 3206.png
│   │   └── 3207.png
│   ├── 4914/
│   └── 4915/
├── 14/
│   ├── 9826/
│   └── 9830/
└── README_TILES.md
```

---

## 🧪 Testing

### Test 1: Check Service
Open browser console and check logs when loading GamePlay:
```
OfflineMapService initialized: useMbtiles=false
Tiles base path: E:\Strategy Game\GameDevelopment\App_Data\tiles
```

### Test 2: Test Single Tile
Open in browser:
```
http://localhost:5000/tiles/13/4915/3207.png
```

Should show a map tile image (or 404 if not downloaded yet).

### Test 3: Check Metadata
```
http://localhost:5000/tiles/metadata
```

Should return:
```json
{
  "bounds": [44.9, 12.7, 45.1, 12.9],
  "center": [45.0, 12.8],
  "zoom": 13,
  "minZoom": 10,
  "maxZoom": 18,
  "name": "Little Aden, Yemen - Offline Tiles",
  "format": "png"
}
```

---

## 📍 Tile Coordinates Reference

For Little Aden (12.8°N, 45.0°E):

| Zoom | Center X | Center Y | Description |
|------|----------|----------|-------------|
| 13   | 4915     | 3207     | Main view   |
| 14   | 9830     | 6414     | Detailed    |
| 15   | 19660    | 12828    | Street level|

### Tile Ranges for Downloads:

**Zoom 13** (5x5 grid, 25 tiles):
```
X: 4913 to 4917
Y: 3205 to 3209
```

**Zoom 14** (9x9 grid, 81 tiles):
```
X: 9826 to 9834
Y: 6410 to 6418
```

---

## 🔧 Configuration

### Change Target Area

To focus on a different location, edit:

**1. wwwroot/js/gamePlayManager.js** (line 114-116):
```javascript
const centerLat = 12.8;     // Your latitude
const centerLng = 45.0;     // Your longitude
```

**2. wwwroot/js/mapControls.js** (line 94):
```javascript
window.gameMap.setView([12.8, 45.0], 13);
```

**3. Services/OfflineMapService.cs** (line 162-172):
```csharp
Center = new[] { 45.0, 12.8 }, // [lon, lat]
Bounds = new[] { 44.9, 12.7, 45.1, 12.9 },
```

**4. Download appropriate tiles** for the new location

---

## ⚠️ Important Notes

### OSM Usage Policy
- ✅ Use MOBAC or download script (includes delays)
- ❌ Don't bulk download directly from OSM
- ✅ Respect 150ms delay between requests
- ✅ Provide attribution when using OSM tiles

### Tile Storage
- **Zoom 10-13**: ~10 MB (good for testing)
- **Zoom 10-15**: ~150 MB (recommended)
- **Zoom 10-18**: ~5-10 GB (very detailed)

### File Formats
- **PNG**: Best quality, supports transparency
- **JPG**: Smaller files, no transparency
- **WEBP**: Modern, best compression

Service automatically detects all three!

---

## 🐛 Troubleshooting

### Problem: Tiles not loading
1. Check directory structure: `App_Data/tiles/{z}/{x}/{y}.png`
2. Verify file permissions
3. Check browser console for 404 errors
4. Try online tiles: "🌐 Online Test (OSM)"

### Problem: Wrong area showing
1. Clear browser cache
2. Check coordinates in gamePlayManager.js
3. Verify tile coordinates match Little Aden
4. Use coordinate calculator: https://www.maptiler.com/google-maps-coordinates-tile-bounds-projection/

### Problem: Map is blank
1. Switch to "🌐 Online Test (OSM)" to verify centering
2. If online works → download tiles
3. If online doesn't work → check coordinates

---

## 📚 More Information

- **Complete Documentation**: `App_Data/tiles/README_TILES.md`
- **Setup Guide**: `LITTLE_ADEN_MAP_SETUP.md`
- **OSM Tile Docs**: https://wiki.openstreetmap.org/wiki/Slippy_map_tilenames
- **MOBAC**: https://mobac.sourceforge.io/

---

## ✅ Checklist

Before deploying to production:

- [ ] Download tiles for target area
- [ ] Test with offline tiles (not online fallback)
- [ ] Verify correct coordinates
- [ ] Check storage requirements
- [ ] Add proper attribution
- [ ] Test on different zoom levels
- [ ] Backup tiles directory

---

**Quick Start Complete!** 🎉

Run `.\download_tiles.ps1` to get started with offline tiles for Little Aden, Yemen!

