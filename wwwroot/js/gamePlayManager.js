/**
 * GamePlayManager - Main coordinator for the GamePlay Arena
 * Handles initialization and coordination between all systems
 */
class GamePlayManager {
    constructor() {
        this.initialized = false;
        this.map = null;
        
        // Performance optimization: Enable/disable verbose logging
        this.verboseLogging = false; // Set to true only for debugging
        
        // Performance timing
        this.performanceTimes = {
            start: performance.now(),
            map: 0,
            managers: 0,
            backgroundData: 0,
            total: 0
        };
        
        // Core components are now directly included in the view (no lazy loading)
        this.modalComponents = [
            'data-entry-modal', 'token-management-modal', 'token-selection-modal',
            'simulation-panel', 'unit-deployment-modal', 'movement-plan-modal',
            'movement-modal', 'battle-modal', 'objective-modal', 'settings-modal'
        ];
        
        // Simple notification callback
        this.notificationCallback = (message, type) => {
            if (this.verboseLogging) {
                console.log(`[${type.toUpperCase()}] ${message}`);
            }
            // You can enhance this to show actual notifications if needed
        };
    }
    
    /**
     * Conditional logging for performance
     */
    log(message, force = false) {
        if (this.verboseLogging || force) {
            console.log(message);
        }
    }
    
    /**
     * Log performance milestone
     */
    logPerformance(milestone, force = true) {
        const now = performance.now();
        const elapsed = now - this.performanceTimes.start;
        if (force) {
            console.log(`⚡ ${milestone}: ${elapsed.toFixed(0)}ms`);
        }
        return now;
    }

    /**
     * Initialize the GamePlay Arena (OPTIMIZED)
     */
    async init() {
        if (this.initialized) {
            this.log('GamePlayManager already initialized');
            return;
        }

        try {
            console.log('🚀 Initializing Game Play Arena (Optimized)...');
            this.logPerformance('START');
            
            // PHASE 1: Critical path only - parallel where possible
            await Promise.all([
                this.loadCoreComponents(), // Direct includes, instant
                this.initializeMap()       // Map initialization
            ]);
            this.performanceTimes.map = this.logPerformance('Phase 1: Map Ready');
            
            // Team info is already set from server-side session in the view
            this.log('✅ Team info available from session:', true);
            
            // PHASE 2: Essential managers in parallel
            await Promise.all([
                this.initializeTokenManager(),
                this.initializeTokenActionModeManager(),
                this.initializeDefensePlanningManager()
            ]);
            this.performanceTimes.managers = this.logPerformance('Phase 2: Core Managers Ready');
            
            // PHASE 3: Setup UI (synchronous, fast)
            this.setupControlHandlers();
            this.initializeBasemapControls();
            
            this.initialized = true;
            console.log('✅ Game Play Arena core initialized');
            this.logPerformance('Phase 3: UI Ready');
            
            // Hide initial loading overlay ASAP
            this.hideInitialLoadingOverlay();
            
            // PHASE 4: Background tasks (non-blocking) - fully parallel
            requestIdleCallback(() => {
                this.loadBackgroundData();
            }, { timeout: 2000 });
            
        } catch (error) {
            console.error('❌ Error initializing GamePlay Arena:', error);
        }
    }

    /**
     * Hide the initial loading overlay
     */
    hideInitialLoadingOverlay() {
        const overlay = document.getElementById('loadingOverlay');
        if (overlay) {
            overlay.style.display = 'none';
            console.log('✅ Initial loading overlay hidden');
        }
    }

    /**
     * Update zoom level display in UI
     */
    updateZoomDisplay(zoomLevel) {
        // Update the zoom value in coordinates display
        const zoomElement = document.getElementById('zoomVal');
        if (zoomElement) {
            zoomElement.textContent = zoomLevel;
        }
        
        // Also show in console for debugging
        console.log(`🔍 Zoom display updated to: ${zoomLevel}`);
    }

    /**
     * Load background data (deferred, non-blocking) - OPTIMIZED ORDER
     */
    async loadBackgroundData() {
        console.log('📦 Loading background data (optimized order)...');
        const bgStart = performance.now();
        
        try {
            // PHASE 1: Load tokens and critical data first (these are needed by other systems)
            await Promise.allSettled([
                this.restorePlacedTokens(),
                this.initializeSuspectedTokenManager(),
                this.loadDefenseElements(),
                this.preloadCriticalModals()
            ]);
            
            this.log('✅ Phase 1: Tokens and data loaded', true);
            
            // PHASE 2: Load managers that depend on tokens (attack visualization needs tokens)
            await Promise.allSettled([
                this.initializeRegionManager(),
                this.initializeLabelManager(),
                this.initializeAttackVisualizationManager()
            ]);
            
            const bgEnd = performance.now();
            console.log(`✅ Background data loaded in ${(bgEnd - bgStart).toFixed(0)}ms`);
            this.performanceTimes.backgroundData = bgEnd - this.performanceTimes.start;
            this.performanceTimes.total = bgEnd - this.performanceTimes.start;
            
            console.log('📊 Total initialization time:', {
                map: `${(this.performanceTimes.map - this.performanceTimes.start).toFixed(0)}ms`,
                managers: `${(this.performanceTimes.managers - this.performanceTimes.map).toFixed(0)}ms`,
                background: `${(this.performanceTimes.backgroundData - this.performanceTimes.managers).toFixed(0)}ms`,
                total: `${this.performanceTimes.total.toFixed(0)}ms`
            });
        } catch (error) {
            console.error('⚠️ Error loading background data:', error);
            // Don't fail the whole app, just log the error
        }
    }
    
    /**
     * Load defense elements from database
     */
    async loadDefenseElements() {
        console.log('🛡️ Loading defense elements...');
        
        try {
            if (window.defensePlanningManager && typeof window.defensePlanningManager.loadDefenseElements === 'function') {
                const result = await window.defensePlanningManager.loadDefenseElements();
                
                if (result.success) {
                    console.log(`✅ Loaded ${result.count} defense elements`);
                } else {
                    console.warn('⚠️ Failed to load defense elements:', result.message);
                }
            } else {
                console.warn('⚠️ Defense planning manager not available or loadDefenseElements not defined');
            }
        } catch (error) {
            console.error('❌ Error loading defense elements:', error);
        }
    }

    /**
     * Load core components (now directly included - no lazy loading)
     */
    async loadCoreComponents() {
        this.log('📦 Core components already included in page (direct partial includes)');
        // Partials are now directly included in the view for better performance and consistency
        // No need to lazy load them via AJAX
        this.log('✅ Core components ready');
    }

    /**
     * Initialize the map
     */
    async initializeMap() {
        this.log('🗺️ Initializing map...', true);
        
        if (typeof L !== 'undefined') {
            // Hide map container until properly loaded with bounds
            const mapContainer = document.getElementById('gameMap');
            if (mapContainer) {
                mapContainer.style.opacity = '0';
            }
            
            // Get default zoom level from region settings
            let defaultZoom = 14;
            if (window.regionSettingsManager) {
                defaultZoom = window.regionSettingsManager.getDefaultZoomLevel();
            }
            
            // Initialize map WITHOUT fixed bounds - will be set when map is loaded
            this.map = L.map('gameMap', {
                worldCopyJump: false,
                zoomSnap: 1,
                zoomDelta: 1,
                inertia: true,
                inertiaDeceleration: 6000,
                preferCanvas: true,
                zoomAnimation: false,
                markerZoomAnimation: false,
                fadeAnimation: false,
                minZoom: 3,
                maxZoom: 22,
                center: [0, 0], // Default center, will be updated
                zoom: defaultZoom, // Default zoom level from region settings
                // Optimize tile loading
                maxZoom: 19, // Limit max zoom to reduce tile requests
                tileSize: 256,
                zoomOffset: 0,
                // Add tile loading optimization
                updateWhenIdle: true,
                updateWhenZooming: false,
                keepBuffer: 2,
                // Disable double-click zoom to prevent conflicts
                doubleClickZoom: false
            });
            this.map.zoomControl.setPosition('bottomright');
            
        // Add debounced zoom change event listener to reduce logging
        let zoomTimeout;
        this.map.on('zoomend', () => {
            clearTimeout(zoomTimeout);
            zoomTimeout = setTimeout(() => {
                const currentZoom = this.map.getZoom();
                console.log(`🔍 Current zoom level: ${currentZoom}`);
                this.updateZoomDisplay(currentZoom);
            }, 100);
        });
        
        // Add debounced moveend listener to reduce logging
        let moveTimeout;
        this.map.on('moveend', () => {
            clearTimeout(moveTimeout);
            moveTimeout = setTimeout(() => {
                const currentZoom = this.map.getZoom();
                const center = this.map.getCenter();
                console.log(`📍 Map moved - Zoom: ${currentZoom}, Center: [${center.lat.toFixed(6)}, ${center.lng.toFixed(6)}]`);
            }, 100);
        });
        
        // Add debounced viewreset listener to reduce logging
        let viewResetTimeout;
        this.map.on('viewreset', () => {
            clearTimeout(viewResetTimeout);
            viewResetTimeout = setTimeout(() => {
                const currentZoom = this.map.getZoom();
                console.log(`🔄 Map view reset - Zoom: ${currentZoom}`);
            }, 100);
        });
            
            console.log(`🔍 Map initialized with default zoom: ${this.map.getZoom()}`);
            
            // Create layer groups
            window.regionGroup = new L.FeatureGroup().addTo(this.map);
            window.BlueGroup = new L.FeatureGroup().addTo(this.map);
            window.foxGroup = new L.FeatureGroup().addTo(this.map);
            window.labelGroup = new L.FeatureGroup().addTo(this.map);
            window.intelGroup = new L.FeatureGroup().addTo(this.map);
            window.reconGroup = new L.FeatureGroup().addTo(this.map);
            window.tokenLayer = new L.FeatureGroup().addTo(this.map);
            
            // Store reference to current tile layer
            this.currentTileLayer = null;
            this.currentBounds = null;
            
            // No default layer added - will be loaded via map selector
            
            // Store map globally
            window.gameMap = this.map;
            window.gamePlayManager = this;
            
            console.log('✅ Map initialized with dynamic bounds support and layer groups');
        } else {
            console.warn('⚠️ Leaflet not loaded, map initialization skipped');
        }
    }

    /**
     * Load current team information for force-based colors
     */
    async loadCurrentTeamInfo() {
        console.log('👥 Loading current team information...');
        
        try {
            const response = await fetch('/GamePlay/GetCurrentTeamInfo');
            console.log('📡 Response status:', response.status);
            
            const data = await response.json();
            console.log('📦 Raw API response:', data);
            
            if (data.success && data.team) {
                this.currentTeamInfo = data.team;
                window.currentTeamInfo = data.team; // Make it globally accessible
                
                // Also save to sessionStorage for persistence
                try {
                    sessionStorage.setItem('currentTeamInfo', JSON.stringify(data.team));
                    console.log('💾 Saved to sessionStorage:', data.team);
                } catch (e) {
                    console.warn('⚠️ Could not save to sessionStorage:', e);
                }
                
                console.log('✅ Team information loaded:', {
                    teamName: data.team.name,
                    forceType: data.team.forceType,
                    teamId: data.team.id
                });
                
                // Verify the force type is valid
                if (!data.team.forceType || data.team.forceType === 'Neutral') {
                    console.warn('⚠️ Force type is Neutral or undefined - this may cause color issues');
                }
            } else {
                console.warn('⚠️ Could not load team information:', data);
            }
        } catch (error) {
            console.error('❌ Error loading team information:', error);
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
            if (typeof LabelManager !== 'undefined' && this.map) {
                this.labelManager = new LabelManager(this.map);
                await this.labelManager.initialize();
                console.log('✅ Label manager initialized');
            } else {
                console.log('⚠️ LabelManager not available or map not ready');
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
        
        // Initialize defense planning manager
        await this.initializeDefensePlanningManager();
    }

    /**
     * Initialize attack visualization manager (OPTIMIZED)
     */
    async initializeAttackVisualizationManager() {
        this.log('🎯 Initializing attack visualization manager...');
        
        if (typeof AttackVisualizationManager !== 'undefined' && window.attackVisualizationManager) {
            // Initialize with map
            await window.attackVisualizationManager.initialize(this.map);
            this.log('✅ Attack visualization manager initialized');
            
            // Defer attack lines loading to idle time
            requestIdleCallback(async () => {
                try {
                    await this.reloadAttackLines();
                    this.log('✅ Attack lines loaded', true);
                } catch (error) {
                    console.error('❌ Error loading attack lines:', error);
                }
            }, { timeout: 3000 });
        } else {
            this.log('⚠️ Attack visualization manager not available', true);
        }
    }

    /**
     * Initialize defense planning manager
     */
    async initializeDefensePlanningManager() {
        console.log('🛡️ Initializing defense planning manager...');
        
        if (typeof DefensePlanningManager !== 'undefined') {
            // Initialize with map
            window.defensePlanningManager = new DefensePlanningManager(this.map);
            
            // Defense elements will be loaded by the main initialization process
            // No need for duplicate loading here
            
            console.log('✅ Defense planning manager initialized');
        } else {
            console.warn('⚠️ Defense planning manager not available');
        }
    }
    
    
    // Note: clearAllDefenseZones method removed - use clearAllDefenseElements() instead
    
    /**
     * Reload attack lines from database
     */
    async reloadAttackLines() {
        console.log('🎯 Reloading attack lines...');
        
        try {
        if (window.attackVisualizationManager) {
                // Clear existing attack lines
                window.attackVisualizationManager.clearAllAttackLines();
                
                // Wait a moment
                await new Promise(resolve => setTimeout(resolve, 500));
                
                // Reload from database
                await window.attackVisualizationManager.loadAttackOrdersFromDatabase();
                
                console.log('✅ Attack lines reloaded successfully');
            } else {
                console.error('❌ Attack visualization manager not available');
            }
        } catch (error) {
            console.error('❌ Error reloading attack lines:', error);
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
                // After restoring, DON'T fit bounds to avoid zoom out
                // Just log the restored tokens for debugging
                if (restoredLatLngs.length > 0) {
                    console.log(`📍 Restored ${restoredLatLngs.length} tokens - keeping current zoom level`);
                    // Don't call fitBounds to prevent zoom out
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
            
            // Adjudicate Move button handler
            const adjudicateMoveBtn = document.getElementById('adjudicateMoveBtn');
            if (adjudicateMoveBtn) {
                console.log('🔍 Adjudicate Move button found: YES');
                
                adjudicateMoveBtn.addEventListener('click', async (e) => {
                    e.preventDefault();
                    console.log('🖱️ Adjudicate Move button clicked!');
                    
                    // Call military adjudication function
                    if (typeof runMilitaryAdjudication === 'function') {
                        await runMilitaryAdjudication();
                    } else {
                        console.error('Military adjudication function not available');
                    }
                });
                
                console.log('✅ Adjudicate Move handler attached');
            } else {
                console.log('🔍 Adjudicate Move button found: NO');
            }
        }, 500);
    }

    /**
     * Initialize basemap controls and map selector
     */
    initializeBasemapControls() {
        console.log('🗺️ Initializing basemap controls...');
        
        // Initialize basemap dropdown with saved state
        if (typeof initializeBasemapDropdown === 'function') {
            initializeBasemapDropdown();
        }
        
        // Load map selector
        this.loadMapSelector();
        
        console.log('✅ Basemap controls initialized');
    }

    /**
     * Load available MBTiles maps into selector (optimized - no auto-load)
     */
    async loadMapSelector() {
        // Prevent multiple simultaneous loads
        if (this.isLoadingMapSelector) {
            console.log('⚠️ Map selector already loading, skipping duplicate call');
            return;
        }
        
        console.log('🗺️ Loading map selector and auto-loading default map...');
        
        try {
            this.isLoadingMapSelector = true;
            
            const selector = document.getElementById('mapSelector');
            
            if (!selector) {
                console.warn('⚠️ Map selector element not found - will retry later');
                this.isLoadingMapSelector = false;
                // Retry after a delay
                setTimeout(() => this.loadMapSelector(), 1000);
                return;
            }
            
            console.log('📍 Map selector element found, fetching maps...');
            
            const response = await fetch('/mbtiles/list');
            if (response.ok) {
                const maps = await response.json();
                console.log(`📍 Received ${maps.length} maps from server:`, maps);
                
                if (maps.length > 0) {
                    selector.innerHTML = '<option value="">⚡ Select a map to start...</option>';
                    
                    maps.forEach(map => {
                        const option = document.createElement('option');
                        option.value = map.path;
                        option.textContent = `${map.name} (${(map.size / 1024 / 1024).toFixed(1)} MB)`;
                        selector.appendChild(option);
                    });
                    
                    // Add event listener for map selection (only once)
                    selector.addEventListener('change', async (e) => {
                        if (e.target.value) {
                            await window.switchGamePlayMap(e.target.value);
                        }
                    }, { once: false });
                    
                    console.log(`✅ Loaded ${maps.length} maps into selector`);
                    
                    // AUTO-LOAD DEFAULT MAP: Load the first (most recent) map automatically
                    if (maps.length > 0) {
                        const defaultMap = maps[0].path;
                        console.log(`🗺️ Auto-loading default map: ${defaultMap}`);
                        console.log('🔍 About to call switchGamePlayMap for auto-load...');
                        selector.value = defaultMap;
                        await window.switchGamePlayMap(defaultMap);
                        console.log('🔍 switchGamePlayMap auto-load completed');
                    }
                } else {
                    selector.innerHTML = '<option value="">No offline maps available</option>';
                    console.warn('⚠️ No maps available from server');
                }
            } else {
                console.error('❌ Failed to fetch maps list, status:', response.status);
                selector.innerHTML = '<option value="">Error loading maps</option>';
            }
        } catch (error) {
            console.error('❌ Error loading map selector:', error);
            const selector = document.getElementById('mapSelector');
            if (selector) {
                selector.innerHTML = '<option value="">Error loading maps</option>';
            }
        } finally {
            this.isLoadingMapSelector = false;
        }
    }

    /**
     * Load terrain database for the current map
     * @param {string} mapPath - Path to MBTiles file (e.g., "job-123/map.mbtiles")
     */
    async loadTerrainDatabase(mapPath) {
        try {
            // Extract folder name from path
            const folderName = mapPath.split('/')[0];
            console.log(`🗺️ Loading terrain database for folder: ${folderName}`);
            
            // Find terrain database with matching folder
            const terrainResponse = await fetch('/terrain/list');
            const terrainFiles = await terrainResponse.json();
            
            console.log('🔍 Available terrain files:', terrainFiles);
            console.log('🔍 Looking for mapFolder:', folderName);
            
            const terrainDb = terrainFiles.find(f => f.mapFolder === folderName);
            
            if (terrainDb) {
                // Store globally for use in adjudication
                window.currentTerrainDb = terrainDb.path;
                
                // Also set in session on server-side
                await fetch('/GamePlay/SetTerrainDatabase', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ terrainDbPath: terrainDb.path })
                });
                
                console.log(`✅ Terrain database loaded: ${terrainDb.path}`);
                this.log(`Terrain data loaded for ${folderName}`, true);
            } else {
                window.currentTerrainDb = null;
                console.warn(`⚠️ No terrain database found for folder: ${folderName}`);
                console.warn('Available mapFolders:', terrainFiles.map(f => f.mapFolder));
                this.log('⚠️ No terrain data available for this map', false);
            }
        } catch (error) {
            console.error('❌ Failed to load terrain database:', error);
            window.currentTerrainDb = null;
        }
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
     * Preload critical modals (now directly included - no preloading needed)
     */
    preloadCriticalModals() {
        console.log('✅ Critical modals already included in page (no preloading needed)');
        // All modals are now directly included in the view for instant availability
    }

    /**
     * Show notification
     */
    showNotification(message, type = 'info') {
        console.log(`📢 ${type.toUpperCase()}: ${message}`);
        
        // Simple fallback notification system
        if (type === 'error') {
            console.error('Error: ' + message);
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
    
    // Use setView instead of fitBounds to maintain zoom level
    const boundsCenter = this.gameAreaBounds.getCenter();
    const currentZoom = this.map.getZoom();
    const targetZoom = Math.max(currentZoom, 17); // Don't zoom out below 17
    
    this.map.setView(boundsCenter, targetZoom, { animate: true });
    
    console.log(`🎯 View reset to game area center at zoom: ${targetZoom}`);
};

GamePlayManager.prototype.setupResizeHandler = function() {
    let resizeTimeout;
    
    window.addEventListener('resize', () => {
        // Debounce resize events
        clearTimeout(resizeTimeout);
        resizeTimeout = setTimeout(() => {
            if (this.gameAreaBounds) {
                // Just invalidate size, don't change zoom level
                this.map.invalidateSize(false);
                console.log('🔄 Map size invalidated after resize (keeping zoom level)');
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
                console.error('Error: Modal element not found');
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
                console.error(errorMessage);
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

// Map switching function with bounded logic (OPTIMIZED for performance)
window.switchGamePlayMap = async function(mapPath) {
    if (!mapPath) return;
    
    console.log('🗺️ Switching to map:', mapPath);
    console.log('🔍 switchGamePlayMap called with mapPath:', mapPath);
    
    // Show loading overlay
    const loadingOverlay = document.createElement('div');
    loadingOverlay.id = 'mapLoadingOverlay';
    loadingOverlay.style.cssText = 'position: fixed; top: 0; left: 0; width: 100%; height: 100%; background: rgba(0,0,0,0.8); z-index: 9999; display: flex; align-items: center; justify-content: center;';
    loadingOverlay.innerHTML = `
        <div style="text-align: center; color: #00ff88;">
            <div class="spinner-border" style="width: 3rem; height: 3rem; border: 4px solid rgba(0,255,136,0.3); border-top-color: #00ff88;" role="status"></div>
            <p style="margin-top: 20px; font-size: 18px;">🗺️ Loading Map...</p>
        </div>
    `;
    document.body.appendChild(loadingOverlay);
    
    try {
        // Fetch map metadata (with timeout for faster failure)
        const controller = new AbortController();
        const timeoutId = setTimeout(() => controller.abort(), 5000); // 5 second timeout
        
        const response = await fetch(`/mbtiles/metadata?file=${encodeURIComponent(mapPath)}`, {
            signal: controller.signal
        });
        clearTimeout(timeoutId);
        
        if (!response.ok) {
            throw new Error(`Failed to load map metadata: ${response.statusText}`);
        }
        
        const data = await response.json();
        const metadata = data?.metadata || {};
        
        console.log('📋 Map metadata loaded:', metadata);
        
        // Parse metadata with minimum zoom of 12
        const minZoom = Math.max(12, parseInt(metadata.minzoom || 12));
        const maxZoom = parseInt(metadata.maxzoom || 22);
        
        console.log(`🔍 Map zoom limits - Min: ${minZoom}, Max: ${maxZoom}`);
        
        // Parse bounds (west, south, east, north) - CRITICAL for bounded behavior
        let boundsObj = null;
        if (metadata.bounds && typeof metadata.bounds === 'string') {
            try {
            const parts = metadata.bounds.split(',').map(parseFloat);
                if (parts.length === 4 && parts.every(p => !isNaN(p))) {
                    // Create Leaflet bounds object (southwest, northeast)
                    boundsObj = L.latLngBounds(
                        L.latLng(parts[1], parts[0]), // southwest (south, west)
                        L.latLng(parts[3], parts[2])  // northeast (north, east)
                    );
                    console.log('✅ Bounds parsed:', parts);
                }
            } catch (e) {
                console.warn('Failed to parse bounds:', e);
            }
        }
        
        // Parse center
        let center = [0, 0];
        // Get default zoom level from region settings (default to 14)
        let initialZoom = 14;
        if (window.regionSettingsManager) {
            initialZoom = window.regionSettingsManager.getDefaultZoomLevel();
            console.log(`🔧 Using stored default zoom level: ${initialZoom}`);
        } else {
            console.log(`🔧 Using fallback default zoom level: ${initialZoom}`);
        }
        
        if (metadata.center && typeof metadata.center === 'string') {
            try {
            const parts = metadata.center.split(',').map(parseFloat);
            if (parts.length >= 2) {
                    center = [parts[1], parts[0]]; // [lat, lon]
                    if (parts.length >= 3 && !isNaN(parts[2])) {
                        initialZoom = Math.max(minZoom, Math.min(maxZoom, parseInt(parts[2])));
                    } else {
                        // If no zoom in metadata, use 17 as default
                        initialZoom = Math.max(minZoom, Math.min(maxZoom, 17));
                    }
                }
            } catch (e) {
                console.warn('Failed to parse center:', e);
            }
        } else if (boundsObj) {
            // Use bounds center if no explicit center
            center = boundsObj.getCenter();
        }
        
        console.log(`🔍 Initial zoom level will be: ${initialZoom}`);
        
        // Remove old tile layer and boundary
        window.gameMap.eachLayer(layer => {
            if (layer instanceof L.TileLayer) {
                window.gameMap.removeLayer(layer);
            }
        });
        
        // Remove old game area boundary if exists
        if (window.gamePlayManager.gameAreaBoundary) {
            window.gameMap.removeLayer(window.gamePlayManager.gameAreaBoundary);
            window.gamePlayManager.gameAreaBoundary = null;
        }
        
        // Apply RELAXED bounds constraints to map (to prevent zoom disappearing)
        if (boundsObj) {
            // Expand bounds slightly to prevent edge issues during zoom
            const expandedBounds = boundsObj.pad(0.1); // 10% padding
            window.gameMap.setMaxBounds(expandedBounds);
            window.gameMap.options.maxBoundsViscosity = 0.8; // FIXED: Less strict (was 1.0)
            window.gamePlayManager.currentBounds = boundsObj;
            
            // Add visual boundary overlay
            const boundaryOverlay = L.rectangle(boundsObj, {
                color: '#00ff88',
                weight: 3,
                opacity: 0.7,
                fillColor: 'transparent',
                fillOpacity: 0,
                dashArray: '10, 10',
                className: 'game-area-boundary'
            }).addTo(window.gameMap);
            
            window.gamePlayManager.gameAreaBoundary = boundaryOverlay;
            console.log('🎯 Game area boundary added for selected map');
        }
        
        // Update map zoom limits
        window.gameMap.setMinZoom(minZoom);
        window.gameMap.setMaxZoom(maxZoom);
        
        // Load terrain database for this map
        console.log('🔍 About to call loadTerrainDatabase with mapPath:', mapPath);
        if (window.gamePlayManager) {
            console.log('🔍 gamePlayManager exists, calling loadTerrainDatabase...');
            await window.gamePlayManager.loadTerrainDatabase(mapPath);
            console.log('🔍 loadTerrainDatabase call completed');
        } else {
            console.warn('⚠️ gamePlayManager not found, cannot load terrain database');
        }
        
        // Create new tile layer with BALANCED settings (performance + reliability)
        // Use TileServerConfig to get the correct tile URL based on mode
        const tileUrl = window.TileServerConfig ? 
            window.TileServerConfig.getTileUrl(mapPath) : 
            `/mbtiles/tile/{z}/{x}/{y}.png?file=${encodeURIComponent(mapPath)}`;
        
        console.log('🗺️ Using tile URL:', tileUrl);
        
        const newTileLayer = L.tileLayer(tileUrl, {
            minZoom: minZoom,
            maxZoom: maxZoom,
            maxNativeZoom: maxZoom,  // Allow overzoom beyond native tiles
            minNativeZoom: minZoom,  // Start fetching at min zoom
            bounds: boundsObj,       // Only request tiles within bounds
            noWrap: true,            // No wrapping around the world
            keepBuffer: 2,           // FIXED: Keep buffer for smooth zooming (was 0)
            updateWhenIdle: false,   // FIXED: Update during movement for better UX
            updateWhenZooming: true, // FIXED: Update during zoom (was false)
            tileSize: 256,
            attribution: metadata.name || 'MBTiles Offline Map',
            // Performance optimizations
            updateInterval: 100,     // Faster updates
            errorTileUrl: '',        // Don't show error tiles
            zoomOffset: 0,
            zoomReverse: false
        });
        
        // Set view BEFORE adding tile layer to prevent visual jump
        if (boundsObj) {
            // Invalidate size first
            window.gameMap.invalidateSize(false);
            
            // Instead of fitBounds (which zooms out), set view to bounds center at stored zoom level
            const boundsCenter = boundsObj.getCenter();
            // Get default zoom level from region settings
            let targetZoom = 14;
            if (window.regionSettingsManager) {
                targetZoom = window.regionSettingsManager.getDefaultZoomLevel();
            }
            targetZoom = Math.min(targetZoom, maxZoom);
            
            window.gameMap.setView(boundsCenter, targetZoom, { 
                animate: false,
                duration: 0
            });
            
            const currentZoom = window.gameMap.getZoom();
            console.log(`✅ Map view set to bounds center with zoom: ${currentZoom} (target: ${targetZoom})`);
            console.log(`📍 Map center: [${boundsCenter.lat.toFixed(6)}, ${boundsCenter.lng.toFixed(6)}]`);
        } else {
            // Fallback: use center and zoom
            window.gameMap.setView(center, initialZoom, { animate: false });
            console.log(`✅ Map view set to center with zoom: ${initialZoom}`);
        }
        
        // NOW add the tile layer after view is set
        newTileLayer.addTo(window.gameMap);
        window.gamePlayManager.currentTileLayer = newTileLayer;
        
        // Set up event handlers for bounds enforcement (after tiles are added)
        if (boundsObj) {
            setupBoundedMapEvents(boundsObj, maxZoom);
        }
        
        // Show map container now that it's properly loaded
        const mapContainer = document.getElementById('gameMap');
        if (mapContainer && mapContainer.style.opacity === '0') {
            mapContainer.style.transition = 'opacity 0.3s ease-in';
            mapContainer.style.opacity = '1';
            console.log('✅ Map container shown');
        }
        
        console.log('✅ Map switched successfully');
        
        // Show success notification
        if (typeof toastr !== 'undefined') {
            toastr.success(`${metadata.name || 'Map'} loaded`, 'Ready');
        }
        
    } catch (error) {
        console.error('❌ Error switching map:', error);
        
        if (typeof toastr !== 'undefined') {
            toastr.error('Error loading map: ' + error.message, 'Error');
        } else {
        console.error('Error loading map: ' + error.message);
        }
    } finally {
        // ALWAYS remove loading overlay immediately
        const overlay = document.getElementById('mapLoadingOverlay');
        if (overlay) {
            overlay.remove();
            console.log('✅ Map loading overlay removed');
        }
    }
};

// Helper function to set up bounded map event handlers (DEBOUNCED for better performance)
function setupBoundedMapEvents(boundsObj, maxZoom) {
    // Remove previous handlers
    window.gameMap.off('zoomend');
    window.gameMap.off('moveend');
    
    let boundsCheckTimeout = null;
    
    // Debounced bounds check function
    const checkBounds = () => {
        if (!window.gameMap || !boundsObj) return;
        
        const currentCenter = window.gameMap.getCenter();
        
        // Only snap back if significantly outside bounds (not just a little bit)
        if (!boundsObj.contains(currentCenter)) {
            // Get nearest point within bounds
            const bounds = boundsObj;
            const lat = Math.max(bounds.getSouth(), Math.min(bounds.getNorth(), currentCenter.lat));
            const lng = Math.max(bounds.getWest(), Math.min(bounds.getEast(), currentCenter.lng));
            
            // Only snap if we're more than a small threshold outside
            const threshold = 0.001; // Small threshold to avoid unnecessary snapping
            if (Math.abs(currentCenter.lat - lat) > threshold || Math.abs(currentCenter.lng - lng) > threshold) {
                window.gameMap.panTo([lat, lng], { animate: true, duration: 0.25 });
            }
        }
    };
    
    // Prevent zoom out beyond dataset visibility (debounced)
    window.gameMap.on('zoomend', function() {
        clearTimeout(boundsCheckTimeout);
        boundsCheckTimeout = setTimeout(checkBounds, 100);
    });
    
    // Prevent panning outside bounds (debounced)
    window.gameMap.on('moveend', function() {
        clearTimeout(boundsCheckTimeout);
        boundsCheckTimeout = setTimeout(checkBounds, 100);
    });
    
    console.log('🔒 Bounded map event handlers setup (debounced)');
}

// Create global instance
const gamePlayManager = new GamePlayManager();
window.gamePlayManager = gamePlayManager; // Make it globally available

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

console.log('✅ Defense planning initialized');

// Global function to manually reload attack lines
window.reloadAttackLines = async function() {
    console.log('🎯 Manually reloading attack lines...');
    
    if (window.gamePlayManager && window.gamePlayManager.reloadAttackLines) {
        await window.gamePlayManager.reloadAttackLines();
    } else {
        console.error('❌ GamePlayManager not available');
    }
};

// Global function to check attack line status
window.checkAttackLines = function() {
    console.log('🔍 Checking attack line status...');
    
    if (window.attackVisualizationManager) {
        console.log('✅ AttackVisualizationManager available');
        console.log('📊 Attack lines in memory:', window.attackVisualizationManager.attackLines.size);
        console.log('📊 Attack orders in memory:', window.attackVisualizationManager.attackOrders.size);
        
        if (window.attackVisualizationManager.map) {
            let linesOnMap = 0;
            window.attackVisualizationManager.map.eachLayer((layer) => {
                if (layer instanceof L.Polyline && layer.options.className === 'attack-line-solid') {
                    linesOnMap++;
                }
            });
            console.log('📊 Attack lines on map:', linesOnMap);
        } else {
            console.warn('⚠️ Map not available');
        }
    } else {
        console.error('❌ AttackVisualizationManager not available');
    }
};

console.log('✅ Attack line functions loaded globally');

// Export for use in other modules
if (typeof module !== 'undefined' && module.exports) {
    module.exports = GamePlayManager;
}
