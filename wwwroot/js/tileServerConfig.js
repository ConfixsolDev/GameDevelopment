/**
 * Tile Server Configuration
 * Centralized configuration for map tile sources
 */

const TileServerConfig = {
    // Tile server mode: 'local' or 'external'
    mode: 'local', // Change to 'external' when using dedicated tile server
    
    // External tile server configuration
    external: {
        baseUrl: 'http://localhost:8080', // Your tile server URL
        endpoint: '/styles/basic/{z}/{x}/{y}.png', // TileServer GL endpoint
        metadataEndpoint: '/data/{mapId}.json' // Metadata endpoint
    },
    
    // Local tile server configuration (current ASP.NET endpoint)
    local: {
        baseUrl: '',
        endpoint: '/mbtiles/tile/{z}/{x}/{y}.png',
        metadataEndpoint: '/mbtiles/metadata'
    },
    
    /**
     * Get tile URL for current mode
     * @param {string} mapPath - Map file path or ID
     * @returns {string} Tile URL template
     */
    getTileUrl(mapPath) {
        const config = this.mode === 'external' ? this.external : this.local;
        
        if (this.mode === 'external') {
            // Extract map ID from path (e.g., "map-123/map.mbtiles" -> "map-123")
            const mapId = mapPath.split('/')[0] || 'game-map';
            return `${config.baseUrl}${config.endpoint.replace('{mapId}', mapId)}`;
        } else {
            // Local mode includes file parameter
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
            const mapId = mapPath.split('/')[0] || 'game-map';
            return `${config.baseUrl}${config.metadataEndpoint.replace('{mapId}', mapId)}`;
        } else {
            return `${config.endpoint}?file=${encodeURIComponent(mapPath)}`;
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

