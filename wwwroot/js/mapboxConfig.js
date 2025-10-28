/**
 * Mapbox Configuration
 * Centralized configuration for Mapbox API tokens and services
 */

const MapboxConfig = {
    // Mapbox API Token - Replace with your valid token
    // Get your token from: https://account.mapbox.com/access-tokens/
    accessToken: 'pk.eyJ1Ijoibm9vcmtoYW4xIiwiYSI6ImNtaGEyOWw0eDAzcm0ya3NkeDBtb2Rvc3MifQ.h_088R52Y0rneGPowla8RQ',
    
    /**
     * Get terrain-rgb tile URL with access token
     * @param {number} z - Zoom level
     * @param {number} x - Tile X coordinate
     * @param {number} y - Tile Y coordinate
     * @returns {string} Complete tile URL
     */
    getTerrainRgbUrl(z, x, y) {
        if (this.accessToken === 'YOUR_MAPBOX_TOKEN_HERE') {
            console.warn('⚠️ Mapbox token not configured. Please set a valid token in mapboxConfig.js');
            return null;
        }
        return `https://api.mapbox.com/v4/mapbox.terrain-rgb/${z}/${x}/${y}.pngraw?access_token=${this.accessToken}`;
    },
    
    /**
     * Check if Mapbox token is configured
     * @returns {boolean} True if token is configured
     */
    isTokenConfigured() {
        return this.accessToken !== 'YOUR_MAPBOX_TOKEN_HERE' && this.accessToken.length > 0;
    },
    
    /**
     * Get attribution for Mapbox services
     * @returns {string} Attribution text
     */
    getAttribution() {
        return '© <a href="https://www.mapbox.com">Mapbox</a> &copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap contributors</a>';
    }
};

// Make globally available
window.MapboxConfig = MapboxConfig;

// Log configuration status
if (MapboxConfig.isTokenConfigured()) {
    console.log('✅ Mapbox token configured');
} else {
    console.warn('⚠️ Mapbox token not configured. Terrain-RGB tiles will not work.');
    console.log('📝 To enable terrain-rgb tiles:');
    console.log('   1. Get a free token from https://account.mapbox.com/access-tokens/');
    console.log('   2. Replace YOUR_MAPBOX_TOKEN_HERE in mapboxConfig.js');
}
