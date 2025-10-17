/**
 * Map Controls - Centralized map control functions for GamePlay Arena
 * Handles fullscreen, reset view, export, and token toggling functionality
 */

// Global variables for map controls
let isFullscreen = false;
let isEditMode = false;
let currentBasemap = 'Offline'; // Default to offline for security

/**
 * Initialize basemap dropdown with saved state - OFFLINE ONLY
 */
function initializeBasemapDropdown() {
    // Load saved basemap from localStorage, default to Offline
    const savedBasemap = localStorage.getItem('currentBasemap');
    if (savedBasemap && savedBasemap.startsWith('Offline')) {
        currentBasemap = savedBasemap;
    } else {
        // Default to offline mode for security
        currentBasemap = 'Offline';
        localStorage.setItem('currentBasemap', currentBasemap);
    }
    
    // Update dropdown to show current basemap
    const basemapDropdown = document.getElementById('basemapDropdown');
    if (basemapDropdown) {
        basemapDropdown.value = currentBasemap;
    }
    
    console.log(`🗺️ Basemap dropdown initialized with: ${currentBasemap} (OFFLINE ONLY)`);
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
        // Reset to Qatar view
        window.gameMap.setView([25.2854, 51.5310], 6);
        console.log('Map view reset to default position');
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
        
        // Add new basemap - ALL OFFLINE SOURCES ONLY
        let newLayer;
        
        // Base configuration for all offline tiles
        const baseConfig = {
            minZoom: 3,
            maxZoom: 22,
            tileSize: 256,
            updateWhenIdle: true,
            updateWhenZooming: false,
            keepBuffer: 2,
            crossOrigin: true,
            detectRetina: true
        };
        
        switch (basemapType) {
            case 'Offline':
                // Default offline tiles from local mbtiles service
                newLayer = L.tileLayer('/tiles/{z}/{x}/{y}.png', {
                    ...baseConfig,
                    attribution: 'Offline tiles from mbtiles'
                });
                break;
            case 'Offline_Dark':
                // Dark style offline tiles with CSS filters
                newLayer = L.tileLayer('/tiles/{z}/{x}/{y}.png', {
                    ...baseConfig,
                    attribution: 'Offline tiles - Dark style',
                    className: 'dark-tiles'
                });
                break;
            case 'Offline_Light':
                // Light style offline tiles with CSS filters
                newLayer = L.tileLayer('/tiles/{z}/{x}/{y}.png', {
                    ...baseConfig,
                    attribution: 'Offline tiles - Light style',
                    className: 'light-tiles'
                });
                break;
            case 'Offline_Terrain':
                // Terrain style offline tiles with CSS filters
                newLayer = L.tileLayer('/tiles/{z}/{x}/{y}.png', {
                    ...baseConfig,
                    attribution: 'Offline tiles - Terrain style',
                    className: 'terrain-tiles'
                });
                break;
            default:
                // Fallback to default offline tiles
                newLayer = L.tileLayer('/tiles/{z}/{x}/{y}.png', {
                    ...baseConfig,
                    attribution: 'Offline tiles from mbtiles'
                });
        }
        
        newLayer.addTo(window.gameMap);
        currentBasemap = basemapType;
        
        // Save basemap to localStorage
        localStorage.setItem('currentBasemap', basemapType);
        
        // Update basemap dropdown
        const basemapDropdown = document.getElementById('basemapDropdown');
        if (basemapDropdown) {
            basemapDropdown.value = basemapType;
        }
        
        console.log(`Basemap changed to: ${basemapType} (OFFLINE ONLY)`);
        
        // Show notification about offline mode
        if (typeof showNotification === 'function') {
            showNotification(`Map style changed to: ${basemapType} (Offline Mode)`, 'info');
        }
        
        // Load and apply mbtiles metadata if available
        loadMbtilesMetadata();
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
    
    console.log('Map controls initialized');
}

// Initialize when DOM is ready
$(document).ready(function() {
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
