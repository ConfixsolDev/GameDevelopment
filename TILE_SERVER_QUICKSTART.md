# 🚀 Tile Server Quick Start (5 Minutes)

Get your dedicated tile server running in 5 minutes!

## Prerequisites

- Docker installed
- Your MBTiles files in `App_Data/tiles/`

## Step 1: Start Tile Server (30 seconds)

```bash
# Start TileServer GL
docker-compose -f docker-compose.tileserver.yml up -d

# Check it's running
docker ps | grep tileserver
```

## Step 2: Verify Tile Server (30 seconds)

Open in browser: **http://localhost:8080**

You should see the TileServer GL interface with your maps listed.

## Step 3: Configure Your App (1 minute)

Open `wwwroot/js/tileServerConfig.js` and change:

```javascript
const TileServerConfig = {
    mode: 'external', // ✅ Change from 'local' to 'external'
    
    external: {
        baseUrl: 'http://localhost:8080',
        endpoint: '/data/{mapId}/{z}/{x}/{y}.png',
        metadataEndpoint: '/data/{mapId}.json'
    },
    // ...
};
```

## Step 4: Test Your Application (2 minutes)

1. Restart your ASP.NET Core app
2. Open GamePlay Arena
3. Check browser console - you should see:
   ```
   🗺️ Tile Server Config loaded: {mode: 'external', ...}
   🗺️ Using tile URL: http://localhost:8080/data/...
   ```

## Done! 🎉

Your tiles are now served by a dedicated tile server!

### What Changed?

- ✅ Tiles now served from port 8080 (tile server)
- ✅ Your ASP.NET Core app no longer handles tile requests
- ✅ Better performance and caching
- ✅ Can scale tile server independently

### Next Steps

- 📖 Read [TILE_SERVER_SETUP.md](./TILE_SERVER_SETUP.md) for production deployment
- 🔧 Enable Nginx caching for even better performance
- ☁️ Add CDN for global distribution

### Rollback to Local Mode

If you need to switch back:

```javascript
// In tileServerConfig.js
mode: 'local', // Change back to 'local'
```

Or dynamically in browser console:
```javascript
TileServerConfig.useLocalServer();
location.reload();
```

### Troubleshooting

**Tiles not loading?**
```bash
# Check tile server logs
docker logs ksagame-tileserver-gl

# Test tile endpoint directly
curl http://localhost:8080/data/map-20251008225258601/14/8192/8192.png -o test.png
```

**Port 8080 already in use?**

Change port in `docker-compose.tileserver.yml`:
```yaml
ports:
  - "8888:8080"  # Use port 8888 instead
```

Then update `tileServerConfig.js`:
```javascript
baseUrl: 'http://localhost:8888',
```

### Performance Monitoring

```bash
# Watch resource usage
docker stats ksagame-tileserver-gl

# See logs in real-time
docker logs -f ksagame-tileserver-gl
```

Happy mapping! 🗺️✨

