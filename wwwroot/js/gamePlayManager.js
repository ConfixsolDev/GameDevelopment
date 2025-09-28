/**
 * GamePlayManager - Main coordinator for the GamePlay Arena
 * Handles initialization and coordination between all systems
 */
class GamePlayManager {
    constructor() {
        this.initialized = false;
        this.map = null;
        this.lazyLoader = null;
        
        // Core component IDs
        this.coreComponents = ['region-panel', 'overlay-controls'];
        this.modalComponents = [
            'data-entry-modal', 'token-management-modal', 'token-selection-modal',
            'simulation-panel', 'unit-deployment-modal', 'movement-plan-modal',
            'battle-modal', 'objective-modal', 'settings-modal'
        ];
    }

    /**
     * Initialize the GamePlay Arena
     */
    async init() {
        if (this.initialized) {
            console.log('GamePlayManager already initialized');
            return;
        }

        try {
            console.log('🚀 Initializing Game Play Arena...');
            
            // Initialize lazy loader
            if (typeof LazyLoader !== 'undefined') {
                this.lazyLoader = new LazyLoader();
            } else {
                console.error('LazyLoader not available');
                return;
            }

            // Load core components
            await this.loadCoreComponents();
            
            // Initialize map
            await this.initializeMap();
            
            // Initialize token manager
            await this.initializeTokenManager();
            
            // Load and restore placed tokens
            await this.restorePlacedTokens();
            
            // Setup control handlers
            this.setupControlHandlers();
            
            // Preload critical modals in background
            this.preloadCriticalModals();
            
            this.initialized = true;
            console.log('✅ Game Play Arena initialized successfully');
            
        } catch (error) {
            console.error('❌ Error initializing GamePlay Arena:', error);
        }
    }

    /**
     * Load core components
     */
    async loadCoreComponents() {
        console.log('📦 Loading core components...');
        
        // Load core components in parallel - much simpler now!
        const coreLoads = [
            this.lazyLoader.loadPartial('region-panel', '#regionPanelContainer', {
                onLoaded: () => console.log('📍 Region panel loaded')
            }),
            this.lazyLoader.loadPartial('overlay-controls', '#overlayControlsContainer', {
                onLoaded: () => console.log('🎮 Overlay controls loaded')
            }),
        ];
        
        await Promise.all(coreLoads);
        console.log('✅ All core components loaded successfully');
    }

    /**
     * Initialize the map
     */
    async initializeMap() {
        console.log('🗺️ Initializing map...');
        
        if (typeof L !== 'undefined') {
            this.map = L.map('gameMap').setView([25.2854, 51.5310], 6);
            this.map.zoomControl.setPosition('bottomright');
            
            // Create layer groups
            window.regionGroup = new L.FeatureGroup().addTo(this.map);
            window.BlueGroup = new L.FeatureGroup().addTo(this.map);
            window.foxGroup = new L.FeatureGroup().addTo(this.map);
            window.labelGroup = new L.FeatureGroup().addTo(this.map);
            window.intelGroup = new L.FeatureGroup().addTo(this.map);
            window.reconGroup = new L.FeatureGroup().addTo(this.map);
            window.tokenLayer = new L.FeatureGroup().addTo(this.map);
            
            // Define base layers
            const layers = {
                "OSM": L.tileLayer.provider('OpenStreetMap.Mapnik'),
                "Topo": L.tileLayer.provider('OpenTopoMap'),
                "Satellite": L.tileLayer.provider('Esri.WorldImagery'),
                "Streets": L.tileLayer.provider('Esri.WorldStreetMap')
            };
            
            // Add default layer (Satellite)
            layers.Satellite.addTo(this.map);
            
            // Store map globally
            window.gameMap = this.map;
            
            console.log('✅ Map initialized with Qatar view and layer groups');
        } else {
            console.warn('⚠️ Leaflet not loaded, map initialization skipped');
        }
    }

    /**
     * Initialize token manager
     */
    async initializeTokenManager() {
        console.log('🎯 Initializing token manager...');
        
        if (typeof tokenManager !== 'undefined' && this.map) {
            await tokenManager.initialize(this.map, this.showNotification.bind(this));
            console.log('✅ Token manager initialized');
        } else {
            console.warn('⚠️ Token manager not available');
        }
    }

    /**
     * Restore placed tokens on map after page refresh
     */
    async restorePlacedTokens() {
        console.log('🔄 Restoring placed tokens...');
        
        try {
            if (!this.map) {
                console.warn('⚠️ Map not available for token restoration');
                return;
            }

            // Get all placed tokens from server
            let placedTokens = [];
            
            if (typeof tokenManager !== 'undefined') {
                placedTokens = await tokenManager.getAllPlacedTokens();
            } else {
                // Fallback to direct API call
                try {
                    const response = await fetch('/GamePlay/GetPlacedTokens');
                    const result = await response.json();
                    if (result.success && result.tokens) {
                        placedTokens = result.tokens;
                    }
                } catch (error) {
                    console.warn('Could not load placed tokens from server:', error);
                }
            }
            
            if (placedTokens && placedTokens.length > 0) {
                console.log(`📍 Found ${placedTokens.length} placed tokens to restore`);
                
                const restoredLatLngs = [];
                for (const tokenData of placedTokens) {
                    const pos = tokenData.position;
                    if (pos && pos.lat != null && pos.lng != null) {
                        await this.restoreTokenOnMap(tokenData);
                        const latNum = typeof pos.lat === 'string' ? parseFloat(pos.lat) : pos.lat;
                        const lngNum = typeof pos.lng === 'string' ? parseFloat(pos.lng) : pos.lng;
                        if (!isNaN(latNum) && !isNaN(lngNum)) restoredLatLngs.push([latNum, lngNum]);
                    }
                }
                // After restoring, fit map to show all tokens if any were added
                if (restoredLatLngs.length > 0) {
                    try {
                        const bounds = L.latLngBounds(restoredLatLngs);
                        this.map.fitBounds(bounds, { padding: [40, 40], maxZoom: 10 });
                    } catch (_) {}
                }
                
                console.log('✅ All placed tokens restored successfully');
            } else {
                console.log('ℹ️ No placed tokens found to restore');
            }
        } catch (error) {
            console.error('❌ Error restoring placed tokens:', error);
            // Don't show error to user as this is background functionality
        }
    }

    /**
     * Restore a single token on the map
     */
    async restoreTokenOnMap(tokenData) {
        try {
            const pos = tokenData.position || {};
            const latNum = typeof pos.lat === 'string' ? parseFloat(pos.lat) : pos.lat;
            const lngNum = typeof pos.lng === 'string' ? parseFloat(pos.lng) : pos.lng;
            console.log(`🎯 Restoring token: ${tokenData.name} at ${latNum}, ${lngNum}`);
            
            const latlng = { lat: latNum, lng: lngNum };
            
            // Create marker using TokenPlacementManager if available
            if (typeof tokenManager !== 'undefined' && tokenManager.tokenPlacementManager) {
                // Use the existing TokenPlacementManager to create marker
                const marker = tokenManager.tokenPlacementManager.createTokenMarker(tokenData, latlng);
                this.map.addLayer(marker);
                
                // Create coverage areas if they exist
                if (tokenData.areaCoverages && tokenData.areaCoverages.length > 0) {
                    tokenManager.tokenPlacementManager.createCoverageAreas(tokenData.areaCoverages, latlng);
                }
                
                // Store token info for tracking
                tokenManager.tokenPlacementManager.placedTokens.set(tokenData.id, {
                    marker: marker,
                    token: tokenData,
                    coverageAreas: tokenData.areaCoverages || []
                });
                
                // Restore movement history and route lines
                await this.restoreTokenMovementHistory(tokenData.id);
            } else {
                // Fallback: Create basic marker
                await this.createBasicTokenMarker(tokenData, latlng);
            }
            
        } catch (error) {
            console.error(`❌ Error restoring token ${tokenData.name}:`, error);
        }
    }

    /**
     * Restore movement history and route lines for a token
     */
    async restoreTokenMovementHistory(tokenId) {
        try {
            console.log(`🔄 Restoring movement history for token: ${tokenId}`);
            
            // Get movement history from backend
            const response = await fetch(`/GamePlay/GetTokenMovementHistory?tokenId=${tokenId}`);
            const result = await response.json();
            
            if (result.success && result.movementHistory && result.movementHistory.length > 1) {
                console.log(`📍 Found ${result.movementHistory.length} movement points for token ${tokenId}`);
                
                // Create route lines for each movement
                const positions = result.movementHistory.map(m => [parseFloat(m.latitude), parseFloat(m.longitude)]);
                
                // Create the main route line
                const routeLine = L.polyline(positions, {
                    color: '#4299e1',
                    weight: 3,
                    opacity: 0.8,
                    dashArray: '10, 5'
                }).addTo(this.map);
                
                // Add ETA labels for each movement point
                result.movementHistory.forEach((movement, index) => {
                    if (index > 0) { // Skip first position (starting point)
                        const etaLabel = L.marker([parseFloat(movement.latitude), parseFloat(movement.longitude)], {
                            icon: L.divIcon({
                                className: 'eta-label',
                                html: `<div class="eta-tag">T+${index}</div>`,
                                iconSize: [40, 20],
                                iconAnchor: [20, 10]
                            })
                        }).addTo(this.map);
                        
                        // Store reference for cleanup
                        if (tokenManager && tokenManager.tokenPlacementManager) {
                            const tokenData = tokenManager.tokenPlacementManager.placedTokens.get(tokenId);
                            if (tokenData) {
                                if (!tokenData.routeLines) tokenData.routeLines = [];
                                if (!tokenData.etaLabels) tokenData.etaLabels = [];
                                tokenData.routeLines.push(routeLine);
                                tokenData.etaLabels.push(etaLabel);
                            }
                        }
                    }
                });
                
                console.log(`✅ Movement history restored for token ${tokenId}`);
            }
        } catch (error) {
            console.error(`❌ Error restoring movement history for token ${tokenId}:`, error);
        }
    }

    /**
     * Create basic token marker (fallback when TokenPlacementManager not available)
     */
    async createBasicTokenMarker(tokenData, latlng) {
        const hasImage = tokenData.assetImagePath && tokenData.assetImagePath.trim() !== '';
        const size = 40;
        
        let iconHtml;
        if (hasImage) {
            iconHtml = `
                <div class="token-marker" style="
                    width: ${size}px; 
                    height: ${size}px; 
                    border-radius: 50%; 
                    overflow: hidden;
                    border: 3px solid #00ff88;
                    box-shadow: 0 4px 12px rgba(0, 255, 136, 0.4);
                    background: white;
                ">
                    <img src="${tokenData.assetImagePath}" 
                         alt="${tokenData.name}" 
                         style="width: 100%; height: 100%; object-fit: cover;" />
                </div>
            `;
        } else {
            iconHtml = `
                <div class="token-marker" style="
                    width: ${size}px; 
                    height: ${size}px; 
                    border-radius: 50%; 
                    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
                    border: 3px solid #00ff88;
                    box-shadow: 0 4px 12px rgba(0, 255, 136, 0.4);
                    display: flex;
                    align-items: center;
                    justify-content: center;
                    color: white;
                    font-weight: bold;
                    font-size: 18px;
                ">
                    <i class="fas fa-map-marker-alt"></i>
                </div>
            `;
        }
        
        const icon = L.divIcon({
            html: iconHtml,
            className: 'custom-token-marker',
            iconSize: [size, size],
            iconAnchor: [size/2, size/2],
            popupAnchor: [0, -size/2]
        });
        
        const marker = L.marker([latlng.lat, latlng.lng], { 
            icon: icon,
            title: tokenData.name 
        });
        
        // Add popup with token info
        marker.bindPopup(`
            <div class="token-popup">
                <h4>${tokenData.name}</h4>
                <p><strong>Group:</strong> ${tokenData.tokenGroupName || 'Unknown'}</p>
                <p><strong>Position:</strong> ${latlng.lat.toFixed(6)}, ${latlng.lng.toFixed(6)}</p>
                <p><strong>Status:</strong> Placed</p>
            </div>
        `);
        
        // Add to token layer
        if (window.tokenLayer) {
            window.tokenLayer.addLayer(marker);
        } else {
            this.map.addLayer(marker);
        }
        
        console.log(`✅ Basic marker created for token: ${tokenData.name}`);
    }

    /**
     * Setup control handlers
     */
    setupControlHandlers() {
        console.log('🔧 Setting up control handlers...');
        
        // Use setTimeout to ensure DOM elements are loaded
        setTimeout(() => {
            // Place Token button handler
            const placeTokenBtn = document.querySelector('#btnPlaceToken, #placeTokenBtn, [data-action="place-token"], button[onclick*="openTokenPlacement"]');
            if (placeTokenBtn) {
                console.log('🔍 Place Token button found: YES');
                
                // Remove any existing onclick handlers
                placeTokenBtn.removeAttribute('onclick');
                
                // Add new event handler
                placeTokenBtn.addEventListener('click', async (e) => {
                    e.preventDefault();
                    console.log('🖱️ Place Token button clicked!');
                    await this.openTokenSelection();
                });
                
                console.log('✅ Token placement handlers attached');
            } else {
                console.log('🔍 Place Token button found: NO');
            }
        }, 500);
    }

    /**
     * Open token selection modal
     */
    async openTokenSelection() {
        console.log('🎯 Opening token selection...');
        
        if (typeof tokenManager !== 'undefined' && tokenManager.isInitialized) {
            await tokenManager.openTokenSelectionModal();
        } else if (typeof openTokenSelectionModal !== 'undefined') {
            // Fallback to global function
            await openTokenSelectionModal();
        } else {
            console.error('❌ Token manager not available or not initialized');
            this.showNotification('Token management system not available', 'error');
        }
    }

    /**
     * Preload critical modals in background
     */
    preloadCriticalModals() {
        console.log('🚀 Preloading critical modals...');
        
        const criticalModals = [
            'data-entry-modal', 
            'token-selection-modal'
            // Note: Data entry now uses AJAX to load server-rendered modals
            // No need to preload token-brigade-data-modal as it's loaded via loadCoreComponents
        ];
        if (this.lazyLoader && this.lazyLoader.preloadPartials) {
            this.lazyLoader.preloadPartials(criticalModals);
        }
    }

    /**
     * Show notification
     */
    showNotification(message, type = 'info') {
        console.log(`📢 ${type.toUpperCase()}: ${message}`);
        
        // Use external notification system if available
        if (typeof showNotification === 'function') {
            showNotification(message, type);
        }
        
        // Simple fallback notification system
        if (type === 'error') {
            alert('Error: ' + message);
        }
    }

    /**
     * Show error message
     */
    showError(message) {
        this.showNotification(message, 'error');
    }

    /**
     * Clean up resources
     */
    destroy() {
        if (typeof tokenManager !== 'undefined') {
            tokenManager.destroy();
        }
        
        this.initialized = false;
        this.map = null;
    }
}

/**
 * GLOBAL DATA ENTRY FUNCTION - Simple and Reliable
 * Available immediately when gamePlayManager loads
 */
function openDataEntry() {
    console.log('🎯 Opening Data Entry - Simple AJAX approach');
    
    // Show loading
    $("#loading").show();
    
    // Load the token selection modal via AJAX
    $.ajax({
        url: '/GamePlay/DataEntryTokenSelection',
        type: 'GET',
        success: function(data) {
            console.log('✅ AJAX Success - Response received:', typeof data, data.length || 0, 'characters');
            
            // Remove any existing modal
            $('#dataEntryTokenModal').remove();
            
            // Add the new modal to the body
            $('body').append(data);
            
            // Show the modal using same method as place token modal
            const modal = document.getElementById('dataEntryTokenModal');
            if (modal) {
                modal.style.display = 'flex';
                console.log('✅ Data entry modal loaded and displayed successfully');
            } else {
                console.error('❌ Modal element not found after append');
                alert('Error: Modal element not found');
            }
        },
        error: function(xhr, status, error) {
            console.error('❌ Error loading data entry modal:');
            console.error('   Status:', status);
            console.error('   Error:', error);
            console.error('   Status Code:', xhr.status);
            console.error('   Response Text:', xhr.responseText);
            
            let errorMessage = 'Failed to load data entry interface';
            if (xhr.status === 404) {
                errorMessage = 'Data entry endpoint not found (404)';
            } else if (xhr.status === 500) {
                errorMessage = 'Server error loading data entry (500)';
            } else if (xhr.status === 403) {
                errorMessage = 'Access denied to data entry (403)';
            }
            
            if (typeof toastr !== 'undefined') {
                toastr.error(errorMessage, 'Error');
            } else {
                alert(errorMessage);
            }
        },
        complete: function() {
            $("#loading").hide();
        }
    });
}

// Make it globally available
window.openDataEntry = openDataEntry;

// Create global instance
const gamePlayManager = new GamePlayManager();

// Auto-initialize when DOM is ready
$(document).ready(function() {
    console.log('🎮 jQuery loaded successfully');
    console.log('🚀 Starting GamePlay Arena initialization...');
    
    // Initialize the game play arena
    gamePlayManager.init();
});

// Export for use in other modules
if (typeof module !== 'undefined' && module.exports) {
    module.exports = GamePlayManager;
}
