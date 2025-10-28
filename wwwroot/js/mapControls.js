/**
 * Map Controls - Centralized map control functions for GamePlay Arena
 * Handles fullscreen, reset view, export, and token toggling functionality
 */

// Global variables for map controls
let isFullscreen = false;
let isEditMode = false;
let currentBasemap = 'satellite'; // Default to satellite

/**
 * Initialize basemap dropdown with saved state - OFFLINE ONLY
 */
function initializeBasemapDropdown() {
    // Load saved basemap from localStorage, default to satellite
    const savedBasemap = localStorage.getItem('currentBasemap');
    if (savedBasemap) {
        currentBasemap = savedBasemap;
    } else {
        // Default to satellite
        currentBasemap = 'satellite';
        localStorage.setItem('currentBasemap', currentBasemap);
    }
    
    // Update dropdown to show current basemap
    const basemapDropdown = document.getElementById('basemapDropdown');
    if (basemapDropdown) {
        basemapDropdown.value = currentBasemap;
    }
    
    console.log(`🗺️ Basemap dropdown initialized with: ${currentBasemap}`);
}

/**
 * Save current zoom level to browser storage
 */
function saveZoomLevel(zoomLevel) {
    localStorage.setItem('currentZoomLevel', zoomLevel.toString());
    console.log(`💾 Zoom level saved: ${zoomLevel}`);
}

/**
 * Load saved zoom level from browser storage
 */
function loadZoomLevel() {
    const savedZoom = localStorage.getItem('currentZoomLevel');
    if (savedZoom) {
        const zoomLevel = parseInt(savedZoom);
        console.log(`📖 Zoom level loaded: ${zoomLevel}`);
        return zoomLevel;
    }
    return 15; // Default zoom level for satellite
}

/**
 * Save current basemap to browser storage
 */
function saveBasemap(basemapType) {
    localStorage.setItem('currentBasemap', basemapType);
    console.log(`💾 Basemap saved: ${basemapType}`);
}

/**
 * Load saved basemap from browser storage
 */
function loadBasemap() {
    const savedBasemap = localStorage.getItem('currentBasemap');
    if (savedBasemap) {
        console.log(`📖 Basemap loaded: ${savedBasemap}`);
        return savedBasemap;
    }
    return 'satellite'; // Default to satellite
}

/**
 * Restore saved map settings on page load
 */
function restoreMapSettings() {
    console.log('🔄 Restoring saved map settings...');
    
    // Restore basemap
    const savedBasemap = loadBasemap();
    if (savedBasemap && savedBasemap !== currentBasemap) {
        console.log(`🗺️ Restoring basemap: ${savedBasemap}`);
        changeBasemap(savedBasemap);
    }
    
    // Restore zoom level after a short delay to ensure map is ready
        setTimeout(() => {
            const savedZoom = loadZoomLevel();
            if (window.gameMap) {
                console.log(`🔍 Restoring zoom level: ${savedZoom}`);
                window.gameMap.setZoom(savedZoom);
            }
        }, 1000);
}

/**
 * Toggle fullscreen mode for the map
 */
function toggleFullscreen() {
    const mapContainer = document.querySelector('.game-map-container');
    const gameContainer = document.querySelector('.gameplay-container');
    
    if (!isFullscreen) {
        // Enter fullscreen
        if (mapContainer.requestFullscreen) {
            mapContainer.requestFullscreen();
        } else if (mapContainer.webkitRequestFullscreen) {
            mapContainer.webkitRequestFullscreen();
        } else if (mapContainer.msRequestFullscreen) {
            mapContainer.msRequestFullscreen();
        }
        
        // Add fullscreen class
        mapContainer.classList.add('fullscreen');
        gameContainer.classList.add('fullscreen-mode');
        isFullscreen = true;
        
        // Update button text
        const fullscreenBtn = document.querySelector('[onclick="toggleFullscreen()"]');
        if (fullscreenBtn) {
            fullscreenBtn.innerHTML = '<i class="fas fa-compress"></i><span>Exit Fullscreen</span>';
        }
        
        console.log('Entered fullscreen mode');
    } else {
        // Exit fullscreen
        if (document.exitFullscreen) {
            document.exitFullscreen();
        } else if (document.webkitExitFullscreen) {
            document.webkitExitFullscreen();
        } else if (document.msExitFullscreen) {
            document.msExitFullscreen();
        }
        
        // Remove fullscreen class
        mapContainer.classList.remove('fullscreen');
        gameContainer.classList.remove('fullscreen-mode');
        isFullscreen = false;
        
        // Update button text
        const fullscreenBtn = document.querySelector('[onclick="toggleFullscreen()"]');
        if (fullscreenBtn) {
            fullscreenBtn.innerHTML = '<i class="fas fa-expand"></i><span>Fullscreen</span>';
        }
        
        console.log('Exited fullscreen mode');
    }
}

/**
 * Reset map view to default position and zoom
 */
function resetMapView() {
    if (window.gameMap) {
        // Check if we have tokens to center on
        if (window.tokenLayer && window.tokenLayer.getLayers().length > 0) {
            // Center on tokens
            const bounds = window.tokenLayer.getBounds();
            if (bounds.isValid()) {
                window.gameMap.fitBounds(bounds, { padding: [20, 20] });
                console.log('Map view reset to token bounds');
                return;
            }
        }
        
        // Fallback to Yemen area where tokens are located (using positive longitude)
        window.gameMap.setView([12.7, 44.8], 15);
        console.log('Map view reset to Yemen area (token region)');
    } else {
        console.warn('Map not available for reset');
    }
}

/**
 * Export current map view as image
 */
function exportMap() {
    if (window.gameMap) {
        try {
            // Create a canvas element
            const canvas = document.createElement('canvas');
            const ctx = canvas.getContext('2d');
            
            // Set canvas size to map size
            const mapSize = window.gameMap.getSize();
            canvas.width = mapSize.x;
            canvas.height = mapSize.y;
            
            // Use Leaflet's built-in export functionality
            window.gameMap.getContainer().toBlob(function(blob) {
                const url = URL.createObjectURL(blob);
                const a = document.createElement('a');
                a.href = url;
                a.download = `map-export-${new Date().toISOString().slice(0, 10)}.png`;
                document.body.appendChild(a);
                a.click();
                document.body.removeChild(a);
                URL.revokeObjectURL(url);
            });
            
            console.log('Map exported successfully');
        } catch (error) {
            console.error('Error exporting map:', error);
            showNotification('Failed to export map', 'error');
        }
    } else {
        console.warn('Map not available for export');
        showNotification('Map not available for export', 'error');
    }
}

/**
 * Toggle token visibility on the map
 */
function toggleTokens() {
    if (window.tokenLayer) {
        const isVisible = window.gameMap.hasLayer(window.tokenLayer);
        
        if (isVisible) {
            window.gameMap.removeLayer(window.tokenLayer);
            console.log('Tokens hidden');
        } else {
            window.gameMap.addLayer(window.tokenLayer);
            console.log('Tokens shown');
        }
        
        // Update button text
        const toggleBtn = document.querySelector('[onclick="toggleTokens()"]');
        if (toggleBtn) {
            const icon = isVisible ? 'fa-eye-slash' : 'fa-eye';
            const text = isVisible ? 'Show Tokens' : 'Hide Tokens';
            toggleBtn.innerHTML = `<i class="fas ${icon}"></i><span>${text}</span>`;
        }
    } else {
        console.warn('Token layer not available');
    }
}

/**
 * Force show tokens (diagnostic function)
 */
function forceShowTokens() {
    console.log('🔧 Force showing tokens...');
    
    if (!window.tokenLayer) {
        console.error('❌ Token layer not found! Creating new one...');
        window.tokenLayer = new L.FeatureGroup();
    }
    
    if (!window.gameMap) {
        console.error('❌ Game map not found!');
        return;
    }
    
    // Ensure token layer is on the map
    if (!window.gameMap.hasLayer(window.tokenLayer)) {
        window.gameMap.addLayer(window.tokenLayer);
        console.log('✅ Token layer added to map');
    }
    
    // Check token count
    const tokenCount = window.tokenLayer.getLayers().length;
    console.log(`📊 Token layer has ${tokenCount} tokens`);
    
    if (tokenCount === 0) {
        console.warn('⚠️ No tokens in token layer. Tokens may not have been loaded properly.');
    }
    
    // Force refresh the map
    window.gameMap.invalidateSize();
    
    return tokenCount;
}

/**
 * Comprehensive token debugging function
 */
function debugTokenDisplay() {
    console.log('🔍 === COMPREHENSIVE TOKEN DEBUG ===');
    
    // Check basic objects
    console.log('1. Basic Objects:');
    console.log('  - window.gameMap exists:', !!window.gameMap);
    console.log('  - window.tokenLayer exists:', !!window.tokenLayer);
    console.log('  - tokenManager exists:', typeof tokenManager !== 'undefined');
    
    if (window.gameMap) {
        console.log('2. Map Status:');
        console.log('  - Map center:', window.gameMap.getCenter());
        console.log('  - Map zoom:', window.gameMap.getZoom());
        console.log('  - Map bounds:', window.gameMap.getBounds());
        // Count layers manually since getLayers() doesn't exist
        let layerCount = 0;
        window.gameMap.eachLayer(function(layer) {
            layerCount++;
        });
        console.log('  - Total layers on map:', layerCount);
        
        // List all layers
        console.log('  - Map layers:');
        window.gameMap.eachLayer(function(layer) {
            console.log(`    - ${layer.constructor.name}: ${layer.getLatLng ? layer.getLatLng() : 'no position'}`);
        });
    }
    
    if (window.tokenLayer) {
        console.log('3. Token Layer Status:');
        console.log('  - Token layer type:', window.tokenLayer.constructor.name);
        console.log('  - Token layer on map:', window.gameMap ? window.gameMap.hasLayer(window.tokenLayer) : 'no map');
        console.log('  - Tokens in layer:', window.tokenLayer.getLayers().length);
        
        if (window.tokenLayer.getLayers().length > 0) {
            console.log('  - Token details:');
            window.tokenLayer.getLayers().forEach((token, index) => {
                const pos = token.getLatLng ? token.getLatLng() : 'no position';
                const id = token.tokenId || token.tokenData?.id || 'no id';
                const name = token.tokenData?.name || 'no name';
                console.log(`    ${index + 1}. ${name} (${id}) at ${pos}`);
            });
        }
    }
    
    // Check DOM elements
    console.log('4. DOM Elements:');
    const tokenElements = document.querySelectorAll('.token-marker, .token-square, [data-token-id]');
    console.log('  - Token DOM elements found:', tokenElements.length);
    tokenElements.forEach((el, index) => {
        const id = el.getAttribute('data-token-id') || el.getAttribute('data-id') || 'no id';
        const name = el.getAttribute('data-token-name') || 'no name';
        console.log(`    ${index + 1}. ${name} (${id})`);
    });
    
    // Check if tokens are hidden by CSS
    console.log('5. CSS Visibility:');
    tokenElements.forEach((el, index) => {
        const style = window.getComputedStyle(el);
        console.log(`    ${index + 1}. Display: ${style.display}, Visibility: ${style.visibility}, Opacity: ${style.opacity}`);
    });
    
    // Check token manager
    if (typeof tokenManager !== 'undefined') {
        console.log('6. Token Manager:');
        console.log('  - TokenPlacementManager exists:', !!tokenManager.tokenPlacementManager);
        if (tokenManager.tokenPlacementManager) {
            console.log('  - Placed tokens count:', tokenManager.tokenPlacementManager.placedTokens?.size || 0);
        }
    }
    
    console.log('🔍 === END TOKEN DEBUG ===');
    
    return {
        mapExists: !!window.gameMap,
        tokenLayerExists: !!window.tokenLayer,
        tokenLayerOnMap: window.gameMap ? window.gameMap.hasLayer(window.tokenLayer) : false,
        tokenCount: window.tokenLayer ? window.tokenLayer.getLayers().length : 0,
        domElements: tokenElements.length
    };
}

/**
 * Force show all tokens with detailed logging
 */
function forceShowAllTokens() {
    console.log('🔧 === FORCE SHOW ALL TOKENS ===');
    
    // Ensure token layer exists
    if (!window.tokenLayer) {
        console.log('❌ Creating new token layer...');
        window.tokenLayer = new L.FeatureGroup();
    }
    
    // Ensure token layer is on map
    if (!window.gameMap.hasLayer(window.tokenLayer)) {
        console.log('✅ Adding token layer to map...');
        window.gameMap.addLayer(window.tokenLayer);
    }
    
    // Check for tokens in other layers
    console.log('🔍 Checking for tokens in other layers...');
    let foundTokens = 0;
    window.gameMap.eachLayer(function(layer) {
        if (layer instanceof L.Marker && layer.tokenId) {
            console.log(`🎯 Found token marker: ${layer.tokenData?.name || 'unknown'} (${layer.tokenId})`);
            if (!window.tokenLayer.hasLayer(layer)) {
                console.log(`✅ Moving token to token layer: ${layer.tokenData?.name || 'unknown'}`);
                window.gameMap.removeLayer(layer);
                window.tokenLayer.addLayer(layer);
                foundTokens++;
            }
        }
    });
    
    console.log(`📊 Moved ${foundTokens} tokens to token layer`);
    console.log(`📊 Total tokens in token layer: ${window.tokenLayer.getLayers().length}`);
    
    // Force map refresh
    window.gameMap.invalidateSize();
    
    console.log('🔧 === END FORCE SHOW ===');
    
    return window.tokenLayer.getLayers().length;
}

/**
 * Fix coordinate system and center map on tokens
 */
function fixTokenVisibility() {
    console.log('🔧 === FIXING TOKEN VISIBILITY ===');
    
    if (!window.tokenLayer || window.tokenLayer.getLayers().length === 0) {
        console.log('❌ No tokens found in token layer');
        return false;
    }
    
    // Get token bounds
    const bounds = window.tokenLayer.getBounds();
    console.log('🎯 Token bounds:', bounds);
    
    if (bounds.isValid()) {
        // Center map on tokens with padding
        window.gameMap.fitBounds(bounds, { 
            padding: [50, 50],
            maxZoom: 16  // Don't zoom too close
        });
        
        console.log('✅ Map centered on tokens');
        console.log('📍 New map center:', window.gameMap.getCenter());
        console.log('🔍 New map zoom:', window.gameMap.getZoom());
        
        // Force map refresh
        window.gameMap.invalidateSize();
        
        return true;
    } else {
        console.log('❌ Invalid token bounds');
        return false;
    }
}

/**
 * Toggle edit mode for the region panel
 */
function toggleEditMode() {
    isEditMode = !isEditMode;
    
    // Toggle edit controls visibility
    const editControls = document.querySelectorAll('.edit-mode-control');
    const regionPanel = document.getElementById('regionPanel');
    
    editControls.forEach(control => {
        if (isEditMode) {
            control.style.display = 'block';
        } else {
            control.style.display = 'none';
        }
    });
    
    // Update button text
    const editBtn = document.getElementById('btnEditMode');
    if (editBtn) {
        const text = isEditMode ? 'Exit Edit' : 'Edit Mode';
        const icon = isEditMode ? 'fa-times' : 'fa-edit';
        editBtn.innerHTML = `<i class="fas ${icon}"></i><span>${text}</span>`;
    }
    
    // Add/remove edit mode class
    if (regionPanel) {
        if (isEditMode) {
            regionPanel.classList.add('edit-mode');
        } else {
            regionPanel.classList.remove('edit-mode');
        }
    }
    
    console.log(`Edit mode ${isEditMode ? 'enabled' : 'disabled'}`);
}

/**
 * Change basemap layer - OFFLINE ONLY VERSION
 */
function changeBasemap(basemapType) {
    if (window.gameMap) {
        // Remove current basemap
        window.gameMap.eachLayer(function(layer) {
            if (layer instanceof L.TileLayer) {
                window.gameMap.removeLayer(layer);
            }
        });
        
        // Add new basemap using TileServerConfig
        let newLayer;
        
        // Base configuration for all tiles
        const baseConfig = {
            tileSize: 256,
            updateWhenIdle: true,
            updateWhenZooming: false,
            keepBuffer: 2,
            crossOrigin: true,
            detectRetina: true
        };
        
        // Define zoom limits for each map type
        const getZoomLimits = (basemapType) => {
            switch (basemapType) {
                case 'terrain-rgb':
                    return { minZoom: 0, maxZoom: 15 };
                case 'satellite':
                    return { minZoom: 0, maxZoom: 19 };
                case 'terrain':
                    return { minZoom: 0, maxZoom: 17 };
                default:
                    return { minZoom: 0, maxZoom: 19 };
            }
        };

        // Set default zoom level for satellite
        const getDefaultZoom = (basemapType) => {
            switch (basemapType) {
                case 'satellite':
                    return 15; // Default zoom for satellite
                default:
                    return null; // Use current zoom
            }
        };
        
        const zoomLimits = getZoomLimits(basemapType);
        
        // Use TileServerConfig to get the appropriate tile URL
        if (typeof TileServerConfig !== 'undefined' && TileServerConfig.getTileUrl && basemapType === 'Offline') {
            // Only use TileServerConfig for offline maps
            const tileConfig = TileServerConfig.getTileUrl(window.currentMapPath || 'map');
            const attribution = getAttributionForBasemap(basemapType);
            
            newLayer = L.tileLayer(tileConfig, {
                ...baseConfig,
                ...zoomLimits,
                attribution: attribution
            });
            
            console.log(`🗺️ Using TileServerConfig for offline map:`, tileConfig);
        } else {
            // Use direct tile URLs for external map types
            const subdomains = ["a", "b", "c"];
            
            switch (basemapType) {
                case 'map':
                    newLayer = L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
                        ...baseConfig,
                        ...zoomLimits,
                        attribution: getAttributionForBasemap(basemapType),
                        subdomains: subdomains
                    });
                    break;
                case 'satellite':
                    newLayer = L.tileLayer('https://server.arcgisonline.com/ArcGIS/rest/services/World_Imagery/MapServer/tile/{z}/{y}/{x}', {
                        ...baseConfig,
                        ...zoomLimits,
                        attribution: getAttributionForBasemap(basemapType)
                    });
                    break;
                case 'terrain':
                    newLayer = L.tileLayer('https://{s}.tile.opentopomap.org/{z}/{x}/{y}.png', {
                        ...baseConfig,
                        ...zoomLimits,
                        attribution: getAttributionForBasemap(basemapType),
                        subdomains: subdomains
                    });
                    break;
                case 'carto-dark':
                    newLayer = L.tileLayer('https://{s}.basemaps.cartocdn.com/dark_all/{z}/{x}/{y}.png', {
                        ...baseConfig,
                        ...zoomLimits,
                        attribution: getAttributionForBasemap(basemapType),
                        subdomains: subdomains
                    });
                    break;
                case 'carto-light':
                    newLayer = L.tileLayer('https://{s}.basemaps.cartocdn.com/light_all/{z}/{x}/{y}.png', {
                        ...baseConfig,
                        ...zoomLimits,
                        attribution: getAttributionForBasemap(basemapType),
                        subdomains: subdomains
                    });
                    break;
                case 'carto-voyager':
                    newLayer = L.tileLayer('https://{s}.basemaps.cartocdn.com/rastertiles/voyager/{z}/{x}/{y}.png', {
                        ...baseConfig,
                        ...zoomLimits,
                        attribution: getAttributionForBasemap(basemapType),
                        subdomains: subdomains
                    });
                    break;
                case 'terrain-rgb':
                    if (window.MapboxConfig && window.MapboxConfig.isTokenConfigured()) {
                        newLayer = L.tileLayer(`https://api.mapbox.com/v4/mapbox.terrain-rgb/{z}/{x}/{y}.pngraw?access_token=${window.MapboxConfig.accessToken}`, {
                            ...baseConfig,
                            ...zoomLimits,
                            attribution: getAttributionForBasemap(basemapType)
                        });
                    } else {
                        console.warn('⚠️ Mapbox token not configured. Skipping terrain-rgb layer.');
                        // Fallback to regular terrain
                        newLayer = L.tileLayer('https://{s}.tile.opentopomap.org/{z}/{x}/{y}.png', {
                            ...baseConfig,
                            ...zoomLimits,
                            attribution: getAttributionForBasemap('terrain'),
                            subdomains: subdomains
                        });
                    }
                    break;
                default:
                    // Fallback to street map
                    newLayer = L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
                        ...baseConfig,
                        ...zoomLimits,
                        attribution: getAttributionForBasemap('map'),
                        subdomains: subdomains
                    });
            }
            
            console.log(`🗺️ Using external tile URL for ${basemapType}`);
        }
        
        newLayer.addTo(window.gameMap);
        
        // Update map zoom limits to match the new basemap
        window.gameMap.setMinZoom(zoomLimits.minZoom);
        window.gameMap.setMaxZoom(zoomLimits.maxZoom);
        
        console.log(`🔍 Updated map zoom limits: ${zoomLimits.minZoom} to ${zoomLimits.maxZoom} for ${basemapType}`);
        
        // Apply default zoom level if specified
        const defaultZoom = getDefaultZoom(basemapType);
        if (defaultZoom !== null) {
            window.gameMap.setZoom(defaultZoom);
            console.log(`🔍 Set default zoom level: ${defaultZoom} for ${basemapType}`);
        }
        
        currentBasemap = basemapType;
        
        // Save basemap to localStorage
        saveBasemap(basemapType);
        
        // Save current zoom level
        saveZoomLevel(window.gameMap.getZoom());
        
        // Update basemap dropdown
        const basemapDropdown = document.getElementById('basemapDropdown');
        if (basemapDropdown) {
            basemapDropdown.value = basemapType;
        }
        
        const friendlyName = getFriendlyBasemapName(basemapType);
        console.log(`🗺️ Basemap changed to: ${friendlyName} (${basemapType})`);
        
        // Show notification about basemap change
        if (typeof showNotification === 'function') {
            showNotification(`Map style changed to: ${friendlyName}`, 'info');
        }
        
        // Load and apply mbtiles metadata if available (only for offline maps)
        if (basemapType === 'Offline' || basemapType.startsWith('Offline_')) {
            loadMbtilesMetadata();
        }
    }
}

/**
 * Load mbtiles metadata and apply to current layer
 */
async function loadMbtilesMetadata() {
    try {
        const response = await fetch('/tiles/metadata');
        if (response.ok) {
            const metadata = await response.json();
            console.log('📋 MBTiles metadata loaded:', metadata);
            
            // Update attribution if available
            if (metadata.attribution) {
                const currentLayer = window.gameMap.getLayers().find(layer => layer instanceof L.TileLayer);
                if (currentLayer) {
                    currentLayer.options.attribution = metadata.attribution;
                }
            }
            
            // Log zoom levels for debugging
            if (metadata.minZoom !== undefined && metadata.maxZoom !== undefined) {
                console.log(`🗺️ MBTiles zoom range: ${metadata.minZoom} to ${metadata.maxZoom}`);
            }
            
            if (typeof showNotification === 'function') {
                showNotification(`MBTiles loaded: ${metadata.name || 'Unknown'} (Zoom ${metadata.minZoom || '?'}-${metadata.maxZoom || '?'})`, 'success');
            }
        }
    } catch (error) {
        console.warn('Could not load mbtiles metadata:', error);
        // This is not critical - tiles will still work
    }
}

/**
 * Get attribution text for basemap type
 */
function getAttributionForBasemap(basemapType) {
    const attributions = {
        'map': '© OpenStreetMap contributors',
        'satellite': '© Esri',
        'terrain': '© OpenTopoMap & © OpenStreetMap contributors',
        'carto-dark': '© CARTO & © OpenStreetMap contributors',
        'carto-light': '© CARTO & © OpenStreetMap contributors',
        'carto-voyager': '© CARTO & © OpenStreetMap contributors',
        'terrain-rgb': '© Mapbox & © OpenStreetMap contributors'
    };
    return attributions[basemapType] || '© OpenStreetMap contributors';
}

/**
 * Get friendly name for basemap type
 */
function getFriendlyBasemapName(basemapType) {
    const names = {
        'map': 'Street Map',
        'satellite': 'Satellite',
        'terrain': 'Topographic',
        'carto-dark': 'Carto Dark',
        'carto-light': 'Carto Light',
        'carto-voyager': 'Carto Voyager',
        'terrain-rgb': '3D Terrain (RGB)'
    };
    return names[basemapType] || basemapType;
}

/**
 * Show layer toggle panel (moved to region panel)
 */
function showLayerPanel() {
    // Layer controls are now in the region panel
    console.log('Layer controls are now in the region panel');
}

/**
 * Toggle overflow menu
 */
function toggleOverflowMenu() {
    const overflowMenu = document.getElementById('overflowMenu');
    if (overflowMenu) {
        overflowMenu.style.display = overflowMenu.style.display === 'none' ? 'block' : 'none';
    }
}

/**
 * Toggle specific layer visibility
 */
function toggleLayer(layerType) {
    let layer;
    let checkbox;
    
    switch (layerType) {
        case 'blue':
            layer = window.BlueGroup;
            checkbox = document.getElementById('chkShowBlue');
            break;
        case 'fox':
            layer = window.foxGroup;
            checkbox = document.getElementById('chkShowFox');
            break;
        case 'labels':
            layer = window.labelGroup;
            checkbox = document.getElementById('chkShowLabels');
            break;
        case 'tokens':
            layer = window.tokenLayer;
            checkbox = document.getElementById('chkShowTokens');
            break;
        case 'regions':
            // Handle regions layer through region manager
            if (window.gamePlayManager && window.gamePlayManager.regionManager) {
                window.gamePlayManager.regionManager.toggleVisibility();
            }
            checkbox = document.getElementById('chkShowRegions');
            return; // Early return since we handle this differently
        default:
            return;
    }
    
    if (layer && window.gameMap) {
        const isVisible = window.gameMap.hasLayer(layer);
        
        if (isVisible) {
            window.gameMap.removeLayer(layer);
            if (checkbox) checkbox.checked = false;
        } else {
            window.gameMap.addLayer(layer);
            if (checkbox) checkbox.checked = true;
        }
        
        console.log(`${layerType} layer ${isVisible ? 'hidden' : 'shown'}`);
    }
}

/**
 * Check token layer status and diagnose issues
 */
function checkTokenLayerStatus() {
    console.log('🔍 Token Layer Diagnostic:');
    console.log('  - window.tokenLayer exists:', !!window.tokenLayer);
    console.log('  - window.gameMap exists:', !!window.gameMap);
    
    if (window.tokenLayer) {
        console.log('  - Token layer type:', window.tokenLayer.constructor.name);
        console.log('  - Token layer has layers:', window.tokenLayer.getLayers().length);
        console.log('  - Token layer bounds:', window.tokenLayer.getBounds());
        
        if (window.gameMap) {
            console.log('  - Token layer is on map:', window.gameMap.hasLayer(window.tokenLayer));
            console.log('  - Map has token layer:', window.gameMap.hasLayer(window.tokenLayer));
        }
        
        // List all layers in token layer
        const layers = window.tokenLayer.getLayers();
        console.log('  - Token layer contents:');
        layers.forEach((layer, index) => {
            console.log(`    ${index + 1}. ${layer.constructor.name} at ${layer.getLatLng ? layer.getLatLng() : 'unknown position'}`);
        });
    }
    
    if (window.gameMap) {
        console.log('  - Map layers count:', window.gameMap.getLayers().length);
        console.log('  - Map bounds:', window.gameMap.getBounds());
        console.log('  - Map zoom:', window.gameMap.getZoom());
        console.log('  - Map center:', window.gameMap.getCenter());
    }
    
    // Check if tokens are hidden by CSS or other means
    const tokenElements = document.querySelectorAll('.token-marker, .military-symbol');
    console.log('  - Token DOM elements found:', tokenElements.length);
    
    return {
        tokenLayerExists: !!window.tokenLayer,
        tokenLayerOnMap: window.gameMap ? window.gameMap.hasLayer(window.tokenLayer) : false,
        tokenCount: window.tokenLayer ? window.tokenLayer.getLayers().length : 0,
        domElements: tokenElements.length
    };
}

/**
 * Initialize map controls
 */
function initializeMapControls() {
    console.log('Initializing map controls...');
    
    // Set up event listeners for existing buttons
    const fullscreenBtn = document.querySelector('[onclick="toggleFullscreen()"]');
    if (fullscreenBtn) {
        fullscreenBtn.removeAttribute('onclick');
        fullscreenBtn.addEventListener('click', toggleFullscreen);
    }
    
    const resetBtn = document.querySelector('[onclick="resetMapView()"]');
    if (resetBtn) {
        resetBtn.removeAttribute('onclick');
        resetBtn.addEventListener('click', resetMapView);
    }
    
    const exportBtn = document.querySelector('[onclick="exportMap()"]');
    if (exportBtn) {
        exportBtn.removeAttribute('onclick');
        exportBtn.addEventListener('click', exportMap);
    }
    
    const tokensBtn = document.querySelector('[onclick="toggleTokens()"]');
    if (tokensBtn) {
        tokensBtn.removeAttribute('onclick');
        tokensBtn.addEventListener('click', toggleTokens);
    }
    
    // Set up zoom level monitoring
    if (window.gameMap) {
        window.gameMap.on('zoomend', function() {
            const currentZoom = window.gameMap.getZoom();
            saveZoomLevel(currentZoom);
        });
        console.log('Zoom level monitoring enabled');
    }
    
    console.log('Map controls initialized');
}

// Initialize when DOM is ready
document.addEventListener('DOMContentLoaded', function() {
    initializeMapControls();
});

// Make functions globally available
window.toggleFullscreen = toggleFullscreen;
window.resetMapView = resetMapView;
window.exportMap = exportMap;
window.toggleTokens = toggleTokens;
window.toggleEditMode = toggleEditMode;
window.changeBasemap = changeBasemap;
window.showLayerPanel = showLayerPanel;
window.toggleLayer = toggleLayer;
window.toggleOverflowMenu = toggleOverflowMenu;
window.initializeBasemapDropdown = initializeBasemapDropdown;
window.loadMbtilesMetadata = loadMbtilesMetadata;
window.checkTokenLayerStatus = checkTokenLayerStatus;
window.forceShowTokens = forceShowTokens;
window.debugTokenDisplay = debugTokenDisplay;
window.forceShowAllTokens = forceShowAllTokens;
window.fixTokenVisibility = fixTokenVisibility;
window.saveZoomLevel = saveZoomLevel;
window.loadZoomLevel = loadZoomLevel;
window.saveBasemap = saveBasemap;
window.loadBasemap = loadBasemap;
window.restoreMapSettings = restoreMapSettings;
