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
        this.coreComponents = ['region-panel', 'overlay-controls', 'unit-status-panel'];
        this.modalComponents = [
            'data-entry-modal', 'token-management-modal', 'token-selection-modal',
            'simulation-panel', 'unit-deployment-modal', 'movement-plan-modal',
            'movement-modal', 'battle-modal', 'objective-modal', 'settings-modal'
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
            
            // Initialize region manager
            await this.initializeRegionManager();

            // Initialize label manager (custom place labels)
            await this.initializeLabelManager();
            
            // Initialize suspected token manager (fog of war)
            await this.initializeSuspectedTokenManager();
            
            // Initialize token action mode manager
            await this.initializeTokenActionModeManager();
            
            // Initialize attack visualization manager
            await this.initializeAttackVisualizationManager();
            
            // Load and restore placed tokens
            await this.restorePlacedTokens();
            
            // Load attack orders after tokens are placed
            await this.loadAttackOrdersAfterTokensPlaced();
            
            // Setup control handlers
            this.setupControlHandlers();
            
            // Initialize basemap dropdown with saved state
            this.initializeBasemapControls();
            
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
            // Focused area: Arnarstapi, Iceland (exactly 100 km²) - moved a bit more right to reduce sea
            const centerLat = 64.780;   // Moved north (up) to reduce sea
            const centerLng = -23.720;  // Moved a bit more west (right) to reduce sea
            
            // Calculate rectangular bounds for 100km² that fits screen aspect ratio
            // At this latitude, 1 degree ≈ 111.32 km
            const kmPerDegreeLat = 111.32;
            const kmPerDegreeLng = 111.32 * Math.cos(centerLat * Math.PI / 180);
            
            // Create a rectangular area that perfectly fits the screen
            // Calculate screen dimensions and create rectangle to match exactly
            const screenDimensions = this.getScreenDimensions();
            const screenAspectRatio = screenDimensions.width / screenDimensions.height;
            
            // Calculate the area that will fit perfectly on screen
            // We'll use the screen aspect ratio directly for perfect fitting
            const aspectRatio = screenAspectRatio;
            const totalAreaKm2 = 100; // Keep 100km² total area
            
            // Calculate dimensions: width * height = area, width/height = aspectRatio
            // height = sqrt(area / aspectRatio), width = height * aspectRatio
            const heightKm = Math.sqrt(totalAreaKm2 / aspectRatio);
            const widthKm = heightKm * aspectRatio;
            
            const halfHeightKm = heightKm / 2;
            const halfWidthKm = widthKm / 2;
            
            const halfSideLatDeg = halfHeightKm / kmPerDegreeLat;
            const halfSideLngDeg = halfWidthKm / kmPerDegreeLng;
            
            const gameAreaBounds = L.latLngBounds(
                L.latLng(centerLat - halfSideLatDeg, centerLng - halfSideLngDeg),
                L.latLng(centerLat + halfSideLatDeg, centerLng + halfSideLngDeg)
            );

            this.map = L.map('gameMap', {
                maxBounds: gameAreaBounds,
                maxBoundsViscosity: 1.0, // Strict bounds - no panning outside
                worldCopyJump: false,
                // Use integer zoom levels to avoid fractional z tile requests (e.g., 12.5)
                zoomSnap: 1,
                zoomDelta: 1,
                inertia: true,
                inertiaDeceleration: 6000,
                preferCanvas: true,
                zoomAnimation: false,
                markerZoomAnimation: false,
                fadeAnimation: false,
                // Restrict zoom to levels that keep the view within bounds
                minZoom: 12, // Higher min zoom to prevent seeing outside area
                maxZoom: 18
            });
            this.map.zoomControl.setPosition('bottomright');
            
            // Add visual boundary overlay to show the restricted 100km² area
            this.addGameAreaBoundary(gameAreaBounds);
            
            // Add zoom event listeners to enforce bounds
            this.setupZoomRestrictions(gameAreaBounds);
            
            // Override zoom controls to prevent zooming out too far
            this.overrideZoomControls(gameAreaBounds);
            
            // Set initial view to fit the game area perfectly to screen
            this.map.fitBounds(gameAreaBounds, { 
                padding: [0, 0], // No padding for perfect screen fit
                maxZoom: 18 // Allow full zoom for perfect fitting
            });
            
            // Add window resize handler to keep game area fitted to screen
            this.setupResizeHandler();
            
            // Create layer groups
            window.regionGroup = new L.FeatureGroup().addTo(this.map);
            window.BlueGroup = new L.FeatureGroup().addTo(this.map);
            window.foxGroup = new L.FeatureGroup().addTo(this.map);
            window.labelGroup = new L.FeatureGroup().addTo(this.map);
            window.intelGroup = new L.FeatureGroup().addTo(this.map);
            window.reconGroup = new L.FeatureGroup().addTo(this.map);
            window.tokenLayer = new L.FeatureGroup().addTo(this.map);
            
            // Define base layers (Offline tiles from local mbtiles service)
            const layers = {
                "Offline": L.tileLayer('/tiles/{z}/{x}/{y}.png', {
                    minZoom: 3,
                    maxZoom: 22,           // allow overzoom; will cap via maxNativeZoom from metadata
                    tileSize: 256,
                    updateWhenIdle: true,
                    updateWhenZooming: false,
                    keepBuffer: 2,
                    crossOrigin: true,
                    detectRetina: true,
                    attribution: 'Offline tiles'
                })
            };
            
            // Add default layer (Offline)
            layers.Offline.addTo(this.map);

            // Enforce focus: fit strictly to Arnarstapi bounds and lock
            try {
                this.map.fitBounds(gameAreaBounds, { padding: [10, 10] });
            } catch (_) { /* ignore */ }

            // Calibrate zoom levels using mbtiles metadata (for maxNativeZoom overzoom beyond native tiles)
            try {
                const res = await fetch('/tiles/metadata');
                if (res.ok) {
                    const meta = await res.json();
                    const nativeMax = (meta && Number.isFinite(meta.maxZoom)) ? meta.maxZoom : ((meta && Number.isFinite(meta.zoom)) ? Math.floor(meta.zoom) : 13);
                    const nativeMin = (meta && Number.isFinite(meta.minZoom)) ? meta.minZoom : 3;
                    // Apply native bounds so Leaflet overzooms smoothly beyond native tiles
                    layers.Offline.options.maxNativeZoom = nativeMax;
                    layers.Offline.options.minNativeZoom = nativeMin;
                    // Allow some overzoom but cap to 22
                    const targetMaxZoom = Math.min(nativeMax + 3, 22);
                    this.map.setMaxZoom(targetMaxZoom);
                    this.map.setMinZoom(nativeMin);
                    layers.Offline.redraw();
                }
            } catch (_) { /* ignore */ }

            // Do not expand beyond Arnarstapi: ignore broader mbtiles metadata for bounds
            
            // Store map globally
            window.gameMap = this.map;
            window.gamePlayManager = this;
            
            console.log('✅ Map initialized with Arnarstapi Iceland view and layer groups');
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
     * Initialize region manager
     */
    async initializeRegionManager() {
        console.log('🗺️ Initializing region manager...');
        
        if (typeof RegionManager !== 'undefined' && this.map) {
            this.regionManager = new RegionManager(this.map, this.showNotification.bind(this));
            await this.regionManager.loadRegions();
            console.log('✅ Region manager initialized');
        } else {
            console.warn('⚠️ Region manager not available');
        }
    }

    /**
     * Initialize custom map label manager (mountains/places/etc.)
     */
    async initializeLabelManager() {
        console.log('🏷️ Initializing label manager...');
        try {
            if (typeof LabelManager === 'undefined') {
                // Dynamically load script if not present
                await this.injectScript('/js/LabelManager.js');
            }
            if (typeof LabelManager !== 'undefined' && this.map) {
                this.labelManager = new LabelManager(this.map);
                await this.labelManager.initialize();
                console.log('✅ Label manager initialized');
            }
        } catch (e) {
            console.warn('Label manager failed to initialize', e);
        }
    }

    /**
     * Initialize suspected token manager (fog of war intelligence)
     */
    async initializeSuspectedTokenManager() {
        console.log('👁️ Initializing suspected token manager...');
        
        if (typeof SuspectedTokenManager !== 'undefined' && this.map) {
            window.suspectedTokenManager = new SuspectedTokenManager(this.map, this.showNotification.bind(this));
            await window.suspectedTokenManager.loadSuspectedTokens();
            console.log('✅ Suspected token manager initialized');
        } else {
            console.warn('⚠️ Suspected token manager not available');
        }
    }

    /**
     * Initialize token action mode manager
     */
    async initializeTokenActionModeManager() {
        console.log('🎯 Initializing token action mode manager...');
        
        if (typeof TokenActionModeManager !== 'undefined' && window.tokenActionModeManager) {
            // Set map reference for the mode manager
            window.tokenActionModeManager.setMap(this.map);
            
            // Set token manager reference if available
            if (window.tokenManager) {
                window.tokenActionModeManager.setTokenManager(window.tokenManager);
            }
            
            // Set notification callback
            window.tokenActionModeManager.setNotificationCallback(this.showNotification.bind(this));
            
            console.log('✅ Token action mode manager initialized');
        } else {
            console.warn('⚠️ Token action mode manager not available');
        }
    }

    /**
     * Initialize attack visualization manager
     */
    async initializeAttackVisualizationManager() {
        console.log('🎯 Initializing attack visualization manager...');
        
        // Debug: Check what's available
        console.log('🔍 Debug - AttackVisualizationManager class:', typeof AttackVisualizationManager);
        console.log('🔍 Debug - window.attackVisualizationManager:', window.attackVisualizationManager);
        console.log('🔍 Debug - Map instance:', this.map);
        
        if (typeof AttackVisualizationManager !== 'undefined' && window.attackVisualizationManager) {
            // Initialize with map
            await window.attackVisualizationManager.initialize(this.map);
            
            console.log('✅ Attack visualization manager initialized');
        } else {
            console.warn('⚠️ Attack visualization manager not available');
            console.log('🔍 Available window objects:', Object.keys(window).filter(key => key.includes('attack') || key.includes('Attack')));
        }
    }
    
    /**
     * Load attack orders after tokens are placed
     */
    async loadAttackOrdersAfterTokensPlaced() {
        console.log('🎯 Loading attack orders after tokens are placed...');
        
        if (window.attackVisualizationManager) {
            await window.attackVisualizationManager.loadAttackOrdersAfterTokensPlaced();
            console.log('✅ Attack orders loaded after tokens placed');
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
                
                // Create coverage areas using token attributes
                tokenManager.tokenPlacementManager.createCoverageAreas(null, latlng, tokenData.forceType, tokenData);
                
                // Store token info for tracking
                tokenManager.tokenPlacementManager.placedTokens.set(tokenData.id, {
                    marker: marker,
                    token: tokenData,
                    coverageAreas: tokenData.areaCoverages || []
                });
                
                // Restore movement history and route lines with force type
                if (tokenData.movementHistory && tokenData.movementHistory.length > 1) {
                    await this.drawTokenMovementHistory(tokenData);
                }
            } else {
                // Fallback: Create basic marker
                await this.createBasicTokenMarker(tokenData, latlng);
            }
            
        } catch (error) {
            console.error(`❌ Error restoring token ${tokenData.name}:`, error);
        }
    }

    /**
     * Draw movement history for a token with color-coded paths based on force type
     */
    async drawTokenMovementHistory(tokenData) {
        try {
            const tokenId = tokenData.id;
            const movementHistory = tokenData.movementHistory;
            const forceType = tokenData.forceType;
            
            console.log(`🔄 Drawing movement history for token: ${tokenData.name} (Force: ${forceType})`);
            
            if (!movementHistory || movementHistory.length < 2) {
                console.log('No movement history to display');
                return;
            }
            
            console.log(`📍 Found ${movementHistory.length} movement points`);
            
            // Determine color based on force type (Fox Land / Blue Land)
            let lineColor = '#4299e1'; // Default blue
            if (forceType) {
                const forceTypeLower = forceType.toLowerCase();
                if (forceTypeLower.includes('fox')) {
                    lineColor = '#ff0000'; // Red for Fox Land
                } else if (forceTypeLower.includes('blue')) {
                    lineColor = '#0000ff'; // Blue for Blue Land
                }
            }
            
            // Create route lines for each movement
            const positions = movementHistory.map(m => [
                typeof m.latitude === 'string' ? parseFloat(m.latitude) : m.latitude,
                typeof m.longitude === 'string' ? parseFloat(m.longitude) : m.longitude
            ]);
            
            // Create the main route line (dotted)
            const routeLine = L.polyline(positions, {
                color: lineColor,
                weight: 3,
                opacity: 0.7,
                dashArray: '10, 10' // Dotted line pattern
            }).addTo(this.map);
            
            // Add small circular markers for each waypoint (except current position)
            const waypointMarkers = [];
            movementHistory.forEach((movement, index) => {
                if (index > 0 && index < movementHistory.length - 1) { // Skip first and last (current position)
                    const lat = typeof movement.latitude === 'string' ? parseFloat(movement.latitude) : movement.latitude;
                    const lng = typeof movement.longitude === 'string' ? parseFloat(movement.longitude) : movement.longitude;
                    
                    const waypointMarker = L.circleMarker([lat, lng], {
                        radius: 4,
                        color: lineColor,
                        fillColor: lineColor,
                        fillOpacity: 0.8,
                        weight: 2
                    }).addTo(this.map);
                    
                    // Add popup with movement details
                    waypointMarker.bindPopup(`
                        <div class="waypoint-popup">
                            <strong>${tokenData.name}</strong><br/>
                            <small>Waypoint ${index}</small><br/>
                            <small>${movement.createdDate ? new Date(movement.createdDate).toLocaleString() : 'Unknown time'}</small>
                        </div>
                    `);
                    
                    waypointMarkers.push(waypointMarker);
                }
            });
            
            // Store reference for cleanup
            if (tokenManager && tokenManager.tokenPlacementManager) {
                const tokenInfo = tokenManager.tokenPlacementManager.placedTokens.get(tokenId);
                if (tokenInfo) {
                    if (!tokenInfo.routeLines) tokenInfo.routeLines = [];
                    if (!tokenInfo.waypointMarkers) tokenInfo.waypointMarkers = [];
                    tokenInfo.routeLines.push(routeLine);
                    tokenInfo.waypointMarkers.push(...waypointMarkers);
                }
            }
            
            console.log(`✅ Movement history drawn for token ${tokenData.name} with ${lineColor} color`);
        } catch (error) {
            console.error(`❌ Error drawing movement history for token ${tokenData.name}:`, error);
        }
    }
    
    /**
     * Restore movement history and route lines for a token (legacy method - no longer needed, kept for backward compatibility)
     * Movement history is now passed with token data, no need for additional API calls
     */
    async restoreTokenMovementHistory(tokenId) {
        console.log(`ℹ️ restoreTokenMovementHistory called for ${tokenId} - This method is deprecated. Movement history is now included in token data.`);
        // This method is no longer needed as movement history is included in GetPlacedTokens response
        // Kept for backward compatibility only
    }

    /**
     * Create basic token marker (fallback when TokenPlacementManager not available)
     */
    async createBasicTokenMarker(tokenData, latlng) {
        const hasImage = tokenData.assetImagePath && tokenData.assetImagePath.trim() !== '';
        const size = 48;
        
        // Determine border color based on force type
        let borderColor = '#00ff88'; // Default green
        if (tokenData.forceType) {
            const forceTypeLower = tokenData.forceType.toLowerCase();
            if (forceTypeLower.includes('fox')) {
                borderColor = '#ff0000'; // Red for Fox Land
            } else if (forceTypeLower.includes('blue')) {
                borderColor = '#0000ff'; // Blue for Blue Land
            }
        }
        
        let iconHtml;
        if (hasImage) {
            iconHtml = `
                <div class="token-square" style="border-color: ${borderColor};">
                    <img src="${tokenData.assetImagePath}" 
                         class="token-image"
                         alt="${tokenData.name}" />
                </div>
            `;
        } else {
            iconHtml = `
                <div class="token-square" style="border-color: ${borderColor};">
                    <div class="token-fallback-icon">
                        <i class="fas fa-crosshairs"></i>
                    </div>
                </div>
            `;
        }
        
        const icon = L.divIcon({
            html: iconHtml,
            className: 'token-marker-container',
            iconSize: [size, size],
            iconAnchor: [size/2, size/2],
            popupAnchor: [0, -size/2]
        });
        
        const marker = L.marker([latlng.lat, latlng.lng], { 
            icon: icon,
            title: tokenData.name 
        });

        // Add token ID as data attribute for easy access by other frontend tools
        marker.tokenData = tokenData;
        marker.tokenId = tokenData.id;
        
        // Also add to the marker element itself for DOM access
        marker.on('add', function() {
            if (this.getElement) {
                const element = this.getElement();
                if (element) {
                    element.setAttribute('data-id', tokenData.id);
                    element.setAttribute('data-token-id', tokenData.id);
                    element.setAttribute('data-token-name', tokenData.name);
                    element.setAttribute('data-token-type', tokenData.forceType || 'Unknown');
                    element.setAttribute('data-token-guid', tokenData.id);
                    element.classList.add('token-marker');
                    
                    // Add title attribute to show token GUID on hover
                    element.setAttribute('title', `Token: ${tokenData.name} (ID: ${tokenData.id})`);
                    
                    console.log(`✅ Basic token marker DOM attributes set for ${tokenData.name}: data-id="${tokenData.id}"`);
                }
            }
        });
        
        // Add popup with token info
        marker.bindPopup(`
            <div class="token-popup">
                <h4>${tokenData.name}</h4>
                <p><strong>Force:</strong> ${tokenData.forceType || 'Unknown'}</p>
                <p><strong>Group:</strong> ${tokenData.tokenGroupName || 'Unknown'}</p>
                <p><strong>Position:</strong> ${latlng.lat.toFixed(6)}, ${latlng.lng.toFixed(6)}</p>
                <p><strong>Status:</strong> Placed</p>
                <p><strong>Token ID:</strong> ${tokenData.id}</p>
            </div>
        `);
        
        // Add to token layer
        if (window.tokenLayer) {
            window.tokenLayer.addLayer(marker);
        } else {
            this.map.addLayer(marker);
        }
        
        console.log(`✅ Basic marker created for token: ${tokenData.name} (${tokenData.forceType})`);
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
     * Initialize basemap controls
     */
    initializeBasemapControls() {
        console.log('🗺️ Initializing basemap controls...');
        
        // Initialize basemap dropdown with saved state
        if (typeof initializeBasemapDropdown === 'function') {
            initializeBasemapDropdown();
        }
        
        console.log('✅ Basemap controls initialized');
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

// Helper methods for Arnarstapi game area management
GamePlayManager.prototype.addGameAreaBoundary = function(bounds) {
    // Create a rectangle overlay to show the game area boundary
    const boundaryOverlay = L.rectangle(bounds, {
        color: '#ff6b6b',
        weight: 3,
        opacity: 0.8,
        fillColor: 'transparent',
        fillOpacity: 0,
        dashArray: '10, 10',
        className: 'game-area-boundary'
    }).addTo(this.map);

    // Store references for potential removal
    this.gameAreaBoundary = boundaryOverlay;
    this.gameAreaLabel = null; // No label

    console.log('🎯 Game area boundary added - 100km² restricted area');
};

GamePlayManager.prototype.setupZoomRestrictions = function(gameAreaBounds) {
    // Store the bounds for later use
    this.gameAreaBounds = gameAreaBounds;
    
    // Add zoom event listener to check bounds after zoom (with delay to avoid initialization issues)
    setTimeout(() => {
        this.map.on('zoomend', () => {
            this.enforceZoomBounds();
            this.updateZoomDisplay();
        });
        
        // Add move event listener to check bounds after pan
        this.map.on('moveend', () => {
            this.enforceZoomBounds();
        });
        
        // Add zoomstart event to show zoom level immediately
        this.map.on('zoomstart', () => {
            this.updateZoomDisplay();
        });
        
        // Initial zoom display update
        this.updateZoomDisplay();
    }, 1000); // Delay to ensure map is fully initialized
    
    console.log('🔒 Zoom restrictions setup - view locked to 100km² area');
};

GamePlayManager.prototype.overrideZoomControls = function(gameAreaBounds) {
    // Store original zoom methods
    const originalZoomIn = this.map.zoomIn.bind(this.map);
    const originalZoomOut = this.map.zoomOut.bind(this.map);
    const originalSetZoom = this.map.setZoom.bind(this.map);
    
    // Override zoomIn
    this.map.zoomIn = (options) => {
        originalZoomIn(options);
        this.enforceZoomBounds();
    };
    
    // Override zoomOut with bounds checking
    this.map.zoomOut = (options) => {
        const currentZoom = this.map.getZoom();
        const minAllowedZoom = this.calculateMaxZoomForBounds();
        
        if (currentZoom > minAllowedZoom) {
            originalZoomOut(options);
            this.enforceZoomBounds();
        } else {
            console.log('🔒 Zoom out prevented - would show outside game area');
        }
    };
    
    // Override setZoom with bounds checking
    this.map.setZoom = (zoomLevel, options) => {
        const minAllowedZoom = this.calculateMaxZoomForBounds();
        
        if (zoomLevel >= minAllowedZoom) {
            originalSetZoom(zoomLevel, options);
            this.enforceZoomBounds();
        } else {
            console.log(`🔒 Zoom to level ${zoomLevel} prevented - would show outside game area`);
            originalSetZoom(minAllowedZoom, options);
        }
    };
    
    console.log('🔒 Zoom controls overridden to enforce game area bounds');
};

GamePlayManager.prototype.enforceZoomBounds = function() {
    if (!this.gameAreaBounds) return;
    
    const currentZoom = this.map.getZoom();
    const minAllowedZoom = this.calculateMaxZoomForBounds();
    
    // Check if zoom level is too low (would show outside game area)
    if (currentZoom < minAllowedZoom) {
        // Zoom in to stay within bounds
        this.map.setZoom(minAllowedZoom);
        console.log(`🔒 Zoom restricted to level ${minAllowedZoom} to stay within game area`);
    }
    
    // Also ensure the map center stays within bounds
    const mapCenter = this.map.getCenter();
    if (!this.gameAreaBounds.contains(mapCenter)) {
        // Reset to game area center
        const gameCenter = this.gameAreaBounds.getCenter();
        this.map.setView(gameCenter, minAllowedZoom);
        console.log('🔒 Map center reset to game area');
    }
};

GamePlayManager.prototype.calculateMaxZoomForBounds = function() {
    if (!this.gameAreaBounds) return 12;
    
    // Use a simpler approach - just return a safe minimum zoom level
    // This prevents users from zooming out too far to see outside the game area
    return 12;
};

GamePlayManager.prototype.resetViewToGameArea = function() {
    if (!this.gameAreaBounds) return;
    
    this.map.fitBounds(this.gameAreaBounds, { 
        padding: [0, 0], // No padding for perfect screen fit
        maxZoom: 18 // Allow full zoom for perfect fitting
    });
    
    console.log('🎯 View reset to game area bounds');
};

GamePlayManager.prototype.setupResizeHandler = function() {
    let resizeTimeout;
    
    window.addEventListener('resize', () => {
        // Debounce resize events
        clearTimeout(resizeTimeout);
        resizeTimeout = setTimeout(() => {
            if (this.gameAreaBounds) {
                this.map.fitBounds(this.gameAreaBounds, { 
                    padding: [0, 0], // No padding for perfect screen fit
                    maxZoom: 18 // Allow full zoom for perfect fitting
                });
                console.log('🔄 Map refitted to screen after resize');
            }
        }, 250);
    });
    
    console.log('📱 Window resize handler setup - game area will stay fitted to screen');
};

GamePlayManager.prototype.getScreenDimensions = function() {
    const mapContainer = document.getElementById('gameMap');
    if (!mapContainer) {
        return { width: 1920, height: 1080 }; // Default dimensions
    }
    
    const rect = mapContainer.getBoundingClientRect();
    const dimensions = {
        width: rect.width,
        height: rect.height
    };
    
    console.log(`📐 Screen dimensions: ${dimensions.width}x${dimensions.height}`);
    return dimensions;
};

GamePlayManager.prototype.calculateScreenAspectRatio = function() {
    const dimensions = this.getScreenDimensions();
    const aspectRatio = dimensions.width / dimensions.height;
    
    console.log(`📐 Screen aspect ratio: ${aspectRatio.toFixed(2)}`);
    return aspectRatio;
};

GamePlayManager.prototype.isWithinGameArea = function(lat, lng) {
    if (!this.map || !this.map.getBounds) return true;
    
    const bounds = this.map.getBounds();
    return bounds.contains([lat, lng]);
};

/**
 * Update zoom level display
 */
GamePlayManager.prototype.updateZoomDisplay = function() {
    if (!this.map) return;
    
    const currentZoom = Math.round(this.map.getZoom());
    const zoomElement = document.getElementById('zoomVal');
    
    if (zoomElement) {
        zoomElement.textContent = currentZoom;
        
        // Add zoom level description
        const zoomDescriptions = {
            12: 'District View',
            13: 'District View', 
            14: 'Neighborhood View',
            15: 'Street View',
            16: 'Street View',
            17: 'Building View',
            18: 'Building View'
        };
        
        const description = zoomDescriptions[currentZoom] || 'Custom View';
        
        // Show notification for zoom level changes
        if (typeof showNotification === 'function') {
            showNotification(`Zoom Level: ${currentZoom} (${description})`, 'info');
        }
        
        console.log(`🔍 Zoom Level: ${currentZoom} (${description})`);
    }
};

/**
 * Show current zoom level message (can be called manually)
 */
GamePlayManager.prototype.showCurrentZoomLevel = function() {
    if (!this.map) {
        if (typeof showNotification === 'function') {
            showNotification('Map not initialized', 'error');
        }
        return;
    }
    
    const currentZoom = Math.round(this.map.getZoom());
    const zoomDescriptions = {
        12: 'District View',
        13: 'District View', 
        14: 'Neighborhood View',
        15: 'Street View',
        16: 'Street View',
        17: 'Building View',
        18: 'Building View'
    };
    
    const description = zoomDescriptions[currentZoom] || 'Custom View';
    const message = `Current Zoom Level: ${currentZoom} (${description})`;
    
    if (typeof showNotification === 'function') {
        showNotification(message, 'info');
    }
    
    console.log(`🔍 ${message}`);
    return { zoom: currentZoom, description: description };
};

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

// Make zoom level function globally available
window.showCurrentZoomLevel = function() {
    if (window.gamePlayManager) {
        return window.gamePlayManager.showCurrentZoomLevel();
    } else {
        console.error('GamePlayManager not initialized');
        if (typeof showNotification === 'function') {
            showNotification('GamePlayManager not initialized', 'error');
        }
    }
};

// Create global instance
const gamePlayManager = new GamePlayManager();

// Auto-initialize when DOM is ready
$(document).ready(function() {
    console.log('🎮 jQuery loaded successfully');
    console.log('🚀 Starting GamePlay Arena initialization...');
    
    // Initialize the game play arena
    gamePlayManager.init();
});

// Phase 01: Movement & Assembly Functions
window.openMovementPlanning = function() {
    console.log('Opening movement planning...');
    // Load movement modal if not already loaded
    if (!document.getElementById('movementModal')) {
        loadMovementModal();
    } else {
        // For now, open with a mock unit
        openMovementModal('unit-mock-1');
    }
};

window.openAssemblyPlanning = function() {
    console.log('Opening assembly planning...');
    // Load movement modal in assembly mode
    if (!document.getElementById('movementModal')) {
        loadMovementModal();
    } else {
        openMovementModal('unit-mock-1');
        // Set assembly mode
        setTimeout(() => {
            if (document.querySelector('input[name="actionType"][value="assembly"]')) {
                document.querySelector('input[name="actionType"][value="assembly"]').checked = true;
                updateActionType('assembly');
            }
        }, 100);
    }
};

window.showUnitStatus = function() {
    console.log('Showing unit status panel...');
    if (typeof showUnitStatus === 'function') {
        showUnitStatus();
    } else {
        // Load unit status panel if not already loaded
        loadUnitStatusPanel();
    }
};

async function loadMovementModal() {
    try {
        const response = await fetch('/GamePlay/Partials/Modals/_MovementModal.cshtml');
        const html = await response.text();
        
        // Remove existing modal if present
        const existingModal = document.getElementById('movementModal');
        if (existingModal) {
            existingModal.remove();
        }
        
        // Add new modal to body
        document.body.insertAdjacentHTML('beforeend', html);
        
        console.log('Movement modal loaded successfully');
    } catch (error) {
        console.error('Error loading movement modal:', error);
    }
}

async function loadUnitStatusPanel() {
    try {
        const response = await fetch('/GamePlay/Partials/Controls/_UnitStatusPanel.cshtml');
        const html = await response.text();
        
        // Remove existing panel if present
        const existingPanel = document.getElementById('unitStatusPanel');
        if (existingPanel) {
            existingPanel.remove();
        }
        
        // Add new panel to body
        document.body.insertAdjacentHTML('beforeend', html);
        
        console.log('Unit status panel loaded successfully');
    } catch (error) {
        console.error('Error loading unit status panel:', error);
    }
}

// Legacy function compatibility
window.planMovement = window.openMovementPlanning;

// Export for use in other modules
if (typeof module !== 'undefined' && module.exports) {
    module.exports = GamePlayManager;
}
