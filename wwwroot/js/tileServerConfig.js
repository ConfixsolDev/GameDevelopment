/**
 * Tile Server Configuration
 * Centralized configuration for map tile sources
 * 
 * Local mode routes are served by GamePlayController:
 * - GET /gameplay/mbtiles/tile/{z}/{x}/{y}.png - Serve map tiles
 * - GET /gameplay/mbtiles/metadata - Get map metadata
 * - GET /gameplay/mbtiles/list - List available maps
 * - GET /gameplay/mbtiles/stats - Performance statistics
 * - GET /gameplay/terrain/list - List terrain databases
 */

const TileServerConfig = {
    // Tile server mode: 'local' or 'external'
    mode: 'local', // Change to 'external' when using dedicated tile server
    
    // External tile server configuration (TileServer-GL)
    external: {
        baseUrl: 'http://localhost:8080',
        endpoint: '/data/{mapId}/{z}/{x}/{y}.png',  // TileServer-GL raster tiles endpoint
        metadataEndpoint: '/data/{mapId}.json'
    },
    
    // Local tile server configuration - served by GamePlayController with /gameplay/ prefix
    local: {
        baseUrl: '',
        endpoint: '/gameplay/mbtiles/tile/{z}/{x}/{y}.png',
        metadataEndpoint: '/gameplay/mbtiles/metadata'
    },
    
    /**
     * Get tile URL for current mode
     * @param {string} mapPath - Map file path or ID
     * @returns {string} Tile URL template
     */
    getTileUrl(mapPath) {
        const config = this.mode === 'external' ? this.external : this.local;
        
        if (this.mode === 'external') {
            // TileServer-GL format: /data/{mapId}/{z}/{x}/{y}.png (for raster MBTiles)
            // Extract mapId from "folder/file.mbtiles" -> "file" (without extension)
            const fileName = mapPath ? mapPath.split('/').pop().replace('.mbtiles', '') : 'map';
            
            // Debug logging
            console.log('🗺️ TileServerConfig.getTileUrl called:', {
                mapPath: mapPath,
                fileName: fileName,
                resultUrl: `${config.baseUrl}/data/${fileName}/{z}/{x}/{y}.png`
            });
            
            return `${config.baseUrl}/data/${fileName}/{z}/{x}/{y}.png`;
        } else {
            // Local C# server format
            return `${config.endpoint}?file=${encodeURIComponent(mapPath)}`;
        }
    },
    
    /**
     * Get metadata URL for current mode
     * @param {string} mapPath - Map file path or ID
     * @returns {string} Metadata URL
     */
    getMetadataUrl(mapPath) {
        const config = this.mode === 'external' ? this.external : this.local;
        
        if (this.mode === 'external') {
            const fileName = mapPath.split('/').pop().replace('.mbtiles', '');
            return `${config.baseUrl}/data/${fileName}.json`;
        } else {
            return `${config.metadataEndpoint}?file=${encodeURIComponent(mapPath)}`;
        }
    },
    
    /**
     * Switch to external tile server
     * @param {string} serverUrl - External server URL
     */
    useExternalServer(serverUrl) {
        this.mode = 'external';
        this.external.baseUrl = serverUrl;
        console.log(`✅ Switched to external tile server: ${serverUrl}`);
    },
    
    /**
     * Switch to local tile server
     */
    useLocalServer() {
        this.mode = 'local';
        console.log('✅ Switched to local tile server');
    },
    
    /**
     * Get current mode info
     */
    getInfo() {
        return {
            mode: this.mode,
            config: this.mode === 'external' ? this.external : this.local
        };
    }
};

// Make globally available
window.TileServerConfig = TileServerConfig;

// Log current configuration
console.log('🗺️ Tile Server Config loaded:', TileServerConfig.getInfo());

