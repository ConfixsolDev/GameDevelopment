# 🗺️ Tile Server Setup Guide

This guide will help you set up a dedicated tile server to serve map tiles directly to users, improving performance and reducing load on your ASP.NET Core application.

## 📋 Table of Contents

1. [Why Use a Dedicated Tile Server?](#why-use-a-dedicated-tile-server)
2. [Option 1: TileServer GL (Recommended)](#option-1-tileserver-gl-recommended)
3. [Option 2: Martin (Fastest)](#option-2-martin-fastest)
4. [Option 3: Simple Node.js Server](#option-3-simple-nodejs-server)
5. [Configuration](#configuration)
6. [Deployment](#deployment)

---

## Why Use a Dedicated Tile Server?

### Current Setup (Local)
- Tiles served through ASP.NET Core application
- Each tile request goes through your web app
- Higher latency and server load

### With Dedicated Tile Server
- ✅ **Faster tile delivery** - Optimized for serving tiles
- ✅ **Reduced web server load** - Offload tile requests
- ✅ **Better caching** - Specialized tile caching
- ✅ **Scalability** - Easy to scale independently
- ✅ **CDN integration** - Can be placed behind CDN

---

## Option 1: TileServer GL (Recommended)

**Best for**: Production deployments, feature-rich requirements

### Installation with Docker

1. **Create `docker-compose.yml` in your project root:**

```yaml
version: '3.8'

services:
  tileserver:
    image: maptiler/tileserver-gl
    container_name: ksagame-tileserver
    ports:
      - "8080:8080"
    volumes:
      - ./App_Data/tiles:/data
    environment:
      - VERBOSE=true
      - NODE_ENV=production
    restart: unless-stopped
    networks:
      - ksagame-network

networks:
  ksagame-network:
    driver: bridge
```

2. **Create tile server configuration `App_Data/tiles/config.json`:**

```json
{
  "options": {
    "paths": {
      "root": "/data",
      "fonts": "/usr/src/app/node_modules/tileserver-gl-styles/fonts",
      "sprites": "/usr/src/app/node_modules/tileserver-gl-styles/sprites",
      "styles": "/usr/src/app/node_modules/tileserver-gl-styles/styles",
      "mbtiles": "/data"
    },
    "serveStaticMaps": true
  },
  "styles": {},
  "data": {}
}
```

3. **Place your MBTiles files in `App_Data/tiles/`**

4. **Start the server:**

```bash
docker-compose up -d
```

5. **Verify it's working:**

Open browser to `http://localhost:8080` - you should see the TileServer GL interface

### URLs After Setup

- **Tile endpoint**: `http://localhost:8080/data/{mapname}/{z}/{x}/{y}.pbf` (vector)
- **Tile endpoint**: `http://localhost:8080/data/{mapname}/{z}/{x}/{y}.png` (raster)
- **Metadata**: `http://localhost:8080/data/{mapname}.json`
- **Viewer**: `http://localhost:8080/data/{mapname}`

---

## Option 2: Martin (Fastest)

**Best for**: High-performance requirements, minimal resource usage

### Installation with Docker

```yaml
version: '3.8'

services:
  martin:
    image: ghcr.io/maplibre/martin:latest
    container_name: ksagame-martin
    ports:
      - "3000:3000"
    volumes:
      - ./App_Data/tiles:/tiles
    command: --mbtiles /tiles
    restart: unless-stopped
```

### Start Martin

```bash
docker-compose up -d
```

### URLs After Setup

- **Tile endpoint**: `http://localhost:3000/{mapname}/{z}/{x}/{y}`
- **Health check**: `http://localhost:3000/health`
- **Catalog**: `http://localhost:3000/catalog`

---

## Option 3: Simple Node.js Server

**Best for**: Development, simple deployments

### Installation

```bash
npm install -g mbtiles-server
```

### Run Server

```bash
mbtiles-server ./App_Data/tiles --port 8080
```

### URLs After Setup

- **Tile endpoint**: `http://localhost:8080/{mapname}/{z}/{x}/{y}.png`

---

## Configuration

### Switch to External Tile Server

1. **Open `wwwroot/js/tileServerConfig.js`**

2. **Change mode and configure URL:**

```javascript
const TileServerConfig = {
    mode: 'external', // ✅ Changed from 'local' to 'external'
    
    external: {
        baseUrl: 'http://localhost:8080', // Your tile server URL
        endpoint: '/data/{mapId}/{z}/{x}/{y}.png', // Adjust based on server
        metadataEndpoint: '/data/{mapId}.json'
    },
    // ...
};
```

3. **For production, use your domain:**

```javascript
external: {
    baseUrl: 'https://tiles.yourdomain.com',
    endpoint: '/data/{mapId}/{z}/{x}/{y}.png',
    metadataEndpoint: '/data/{mapId}.json'
}
```

### Dynamic Switching

You can also switch tile servers at runtime:

```javascript
// In browser console or your code
TileServerConfig.useExternalServer('http://tiles.example.com');

// Or switch back to local
TileServerConfig.useLocalServer();
```

---

## Deployment

### Production Deployment with TileServer GL

1. **Update `docker-compose.yml` for production:**

```yaml
version: '3.8'

services:
  tileserver:
    image: maptiler/tileserver-gl
    container_name: ksagame-tileserver
    ports:
      - "127.0.0.1:8080:8080" # Only expose to localhost
    volumes:
      - ./App_Data/tiles:/data:ro # Read-only mount
    environment:
      - NODE_ENV=production
      - VERBOSE=false
    restart: always
    mem_limit: 2g # Limit memory usage
    networks:
      - ksagame-network
```

2. **Configure Nginx as reverse proxy:**

```nginx
# /etc/nginx/sites-available/tiles.conf

upstream tileserver {
    server localhost:8080;
}

server {
    listen 443 ssl http2;
    server_name tiles.yourdomain.com;

    ssl_certificate /etc/letsencrypt/live/tiles.yourdomain.com/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/tiles.yourdomain.com/privkey.pem;

    # Tile caching
    location ~* \.(png|jpg|jpeg|pbf)$ {
        proxy_pass http://tileserver;
        proxy_cache tiles_cache;
        proxy_cache_valid 200 30d;
        proxy_cache_use_stale error timeout updating http_500 http_502 http_503 http_504;
        add_header X-Cache-Status $upstream_cache_status;
        
        # CORS headers
        add_header Access-Control-Allow-Origin *;
        add_header Access-Control-Allow-Methods "GET, OPTIONS";
    }

    location / {
        proxy_pass http://tileserver;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
    }
}

# Cache configuration
proxy_cache_path /var/cache/nginx/tiles levels=1:2 keys_zone=tiles_cache:100m max_size=10g inactive=30d use_temp_path=off;
```

3. **Enable site and reload Nginx:**

```bash
sudo ln -s /etc/nginx/sites-available/tiles.conf /etc/nginx/sites-enabled/
sudo nginx -t
sudo systemctl reload nginx
```

---

## Performance Optimization

### Enable Tile Compression

In your tile server config or Nginx:

```nginx
# Compress tiles
gzip on;
gzip_vary on;
gzip_types application/x-protobuf application/json;
gzip_comp_level 6;
```

### CDN Integration

Use Cloudflare or another CDN:

1. Point DNS to your tile server
2. Enable caching rules for `.png`, `.pbf`, `.json` files
3. Set long cache TTL (30+ days)

### Monitor Performance

```bash
# Check tile server logs
docker logs -f ksagame-tileserver

# Monitor resource usage
docker stats ksagame-tileserver
```

---

## Testing

### Test Tile Loading

```bash
# Test a single tile
curl http://localhost:8080/data/map-20251008225258601/14/8192/8192.png -o test-tile.png

# Check if tile is valid
file test-tile.png
```

### Load Testing

```bash
# Install apache bench
sudo apt-get install apache2-utils

# Run load test
ab -n 1000 -c 10 http://localhost:8080/data/yourmap/14/8192/8192.png
```

---

## Troubleshooting

### Tiles not loading

1. **Check tile server is running:**
   ```bash
   docker ps | grep tileserver
   ```

2. **Check logs:**
   ```bash
   docker logs ksagame-tileserver
   ```

3. **Verify MBTiles file:**
   ```bash
   file ./App_Data/tiles/map.mbtiles
   ```

4. **Test tile endpoint directly:**
   Open `http://localhost:8080` in browser

### CORS errors

Add CORS headers to your tile server or Nginx config:

```nginx
add_header Access-Control-Allow-Origin *;
add_header Access-Control-Allow-Methods "GET, OPTIONS";
```

### Performance issues

1. Increase memory limit in docker-compose
2. Enable tile caching (Nginx/CDN)
3. Use vector tiles instead of raster when possible
4. Consider using Martin for better performance

---

## Comparison Table

| Feature | TileServer GL | Martin | Node.js Server |
|---------|---------------|--------|----------------|
| **Speed** | Good | Excellent | Good |
| **Memory** | Medium | Low | Low |
| **Features** | Rich | Basic | Basic |
| **Built-in Viewer** | ✅ Yes | ❌ No | ❌ No |
| **Vector Tiles** | ✅ Yes | ✅ Yes | ⚠️ Limited |
| **Ease of Setup** | Easy | Easy | Very Easy |
| **Production Ready** | ✅ Yes | ✅ Yes | ⚠️ Basic |

---

## Next Steps

1. ✅ Choose your tile server (TileServer GL recommended)
2. ✅ Set up with Docker using the configs above
3. ✅ Update `tileServerConfig.js` to use external mode
4. ✅ Test in development
5. ✅ Deploy to production with Nginx reverse proxy
6. ✅ Add CDN for global distribution (optional)

---

## Support

For issues or questions:
- TileServer GL: https://github.com/maptiler/tileserver-gl/issues
- Martin: https://github.com/maplibre/martin/issues
- MBTiles Spec: https://github.com/mapbox/mbtiles-spec

Happy mapping! 🗺️

