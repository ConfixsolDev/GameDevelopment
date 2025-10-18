/**
 * Defense Planning Manager
 * Manages creation, editing, and visualization of defense elements
 * Integrates with token management for defense planning
 */

class DefensePlanningManager {
    constructor(map) {
        this.map = map;
        this.defenseElements = new Map(); // Store all defense elements
        this.currentElement = null;
        this.drawingMode = null;
        this.drawingPoints = [];
        this.tempLayer = null;
        
        // Layer groups for different defense types
        this.killZoneLayer = L.layerGroup().addTo(this.map);
        this.minefieldLayer = L.layerGroup().addTo(this.map);
        this.obstacleLayer = L.layerGroup().addTo(this.map);
        this.positionLayer = L.layerGroup().addTo(this.map);
        this.withdrawalLayer = L.layerGroup().addTo(this.map);
        this.defensiveLineLayer = L.layerGroup().addTo(this.map);
        this.defenseZoneLayer = L.layerGroup().addTo(this.map); // New defense zone layer
        
        this.renderer = window.defenseSymbolRenderer;
        
        // Check if renderer is available, if not, wait for it
        if (!this.renderer) {
            console.warn('⚠️ Defense symbol renderer not yet available, will retry...');
            this.waitForRenderer();
        } else {
            // Set map reference for zoom-responsive sizing
            this.renderer.setMap(this.map);
        }
        
        // Add zoom event listener to refresh minefield sizes
        this.map.on('zoomend', () => {
            this.refreshMinefieldSizes();
        });
        
        console.log('🛡️ Defense Planning Manager initialized');
    }
    
    /**
     * Refresh minefield sizes based on current zoom level
     */
    refreshMinefieldSizes() {
        if (!this.renderer || !this.renderer.map) return;
        
        const currentZoom = this.renderer.map.getZoom();
        const newCellSize = Math.max(16, Math.min(48, 20 + (currentZoom - 10) * 2));
        
        console.log(`🔄 Refreshing minefield sizes for zoom ${currentZoom}, cell size: ${newCellSize}px`);
        
        // Update all existing minefield markers
        this.defenseElements.forEach((element, elementId) => {
            if (element.category === 'minefield' && element.layers && element.layers.markers) {
                element.layers.markers.forEach(marker => {
                    const element = marker.getElement();
                    if (element) {
                        const mineIcon = element.querySelector('.mine-icon-container');
                        if (mineIcon) {
                            const iconSize = newCellSize * 0.9;
                            mineIcon.style.width = `${iconSize}px`;
                            mineIcon.style.height = `${iconSize}px`;
                        }
                        
                        const cell = element.querySelector('.minefield-grid-cell');
                        if (cell) {
                            cell.style.width = `${newCellSize}px`;
                            cell.style.height = `${newCellSize}px`;
                        }
                    }
                });
            }
        });
    }
    
    /**
     * Wait for the defense symbol renderer to become available
     */
    waitForRenderer() {
        const checkRenderer = () => {
            if (window.defenseSymbolRenderer) {
                this.renderer = window.defenseSymbolRenderer;
                this.renderer.setMap(this.map); // Set map reference for zoom-responsive sizing
                console.log('✅ Defense symbol renderer now available');
            } else {
                // Retry after 100ms
                setTimeout(checkRenderer, 100);
            }
        };
        checkRenderer();
    }
    
    /**
     * Get current team's force type for color-coding defense elements
     */
    getCurrentForceType() {
        console.log('🔍 Getting current force type from session...');
        console.log('🔍 window.currentTeamInfo:', window.currentTeamInfo);
        
        // Get from session data (set directly from server-side)
        if (window.currentTeamInfo && window.currentTeamInfo.forceType) {
            console.log(`✅ Force type from session: ${window.currentTeamInfo.forceType}`);
            return window.currentTeamInfo.forceType;
        }
        
        // Default to neutral if not available
        console.warn('⚠️ Force type not available in session, using default');
        return 'Neutral';
    }
    
    /**
     * Debug method to check force type detection
     */
    debugForceType() {
        console.log('🔍 DEBUG: Force Type Detection');
        console.log('window.currentTeamInfo:', window.currentTeamInfo);
        console.log('window.gamePlayManager:', window.gamePlayManager);
        console.log('window.gamePlayManager?.currentTeamInfo:', window.gamePlayManager?.currentTeamInfo);
        
        const forceType = this.getCurrentForceType();
        console.log('Detected force type:', forceType);
        
        // Test the color detection
        if (window.defensePlanningManager && window.defensePlanningManager.renderer) {
            const color = window.defensePlanningManager.renderer.getForceColor(forceType);
            console.log('Force color:', color);
        }
        
        return forceType;
    }

    /**
     * Debug method to test right-click functionality
     */
    debugRightClick() {
        console.log('🔍 DEBUG: Right-Click Functionality');
        console.log('Defense elements count:', this.defenseElements.size);
        
        this.defenseElements.forEach((element, id) => {
            console.log(`Element ${id}:`, {
                category: element.category,
                type: element.type,
                hasLayers: !!element.layers,
                hasElement: !!element.element,
                hasDbId: !!element.dbId,
                dbId: element.dbId,
                layersType: element.layers?.constructor?.name
            });
        });
        
        // Test right-click handler
        console.log('Testing right-click handler...');
        this.handleDefenseElementRightClick({ originalEvent: { preventDefault: () => {}, stopPropagation: () => {} } }, 'test-id', 'defensezone');
        
        return this.defenseElements.size;
    }

    /**
     * Debug method to test delete functionality
     */
    async debugDelete() {
        console.log('🔍 DEBUG: Delete Functionality');
        console.log('Defense elements count:', this.defenseElements.size);
        
        if (this.defenseElements.size === 0) {
            console.log('No elements to test delete on');
            return;
        }
        
        // Get first element for testing
        const [firstId, firstElement] = this.defenseElements.entries().next().value;
        console.log(`Testing delete on element: ${firstId}`, firstElement);
        
        // Test delete without actually deleting
        console.log('Testing delete method (dry run)...');
        await this.deleteDefenseElement(firstId, firstElement.category);
        
        return firstId;
    }

    /**
     * Clean up inactive elements from local storage
     * This ensures local storage only contains active elements
     */
    async cleanupInactiveElements() {
        console.log('🧹 Cleaning up inactive elements from local storage...');
        
        try {
            // Get all active elements from database
            const result = await this.loadDefenseElements();
            
            if (result.success) {
                const activeElementIds = new Set(result.elements.map(e => e.elementId));
                
                // Remove any local elements that are not in the active list
                const elementsToRemove = [];
                this.defenseElements.forEach((element, id) => {
                    if (!activeElementIds.has(id)) {
                        elementsToRemove.push(id);
                    }
                });
                
                if (elementsToRemove.length > 0) {
                    console.log(`🧹 Removing ${elementsToRemove.length} inactive elements from local storage`);
                    elementsToRemove.forEach(id => {
                        this.defenseElements.delete(id);
                    });
                } else {
                    console.log('✅ Local storage is clean - no inactive elements found');
                }
            }
        } catch (error) {
            console.error('❌ Error cleaning up inactive elements:', error);
        }
    }

    /**
     * Force refresh team info and test force type detection
     */
    async forceRefreshTeamInfo() {
        console.log('🔄 Force refreshing team info...');
        
        try {
            if (window.gamePlayManager && typeof window.gamePlayManager.loadCurrentTeamInfo === 'function') {
                await window.gamePlayManager.loadCurrentTeamInfo();
                console.log('✅ Team info refreshed');
                
                // Test force type detection
                const forceType = this.getCurrentForceType();
                console.log(`🎨 Current force type: ${forceType}`);
                
                // Test color detection
                if (window.defensePlanningManager && window.defensePlanningManager.renderer) {
                    const color = window.defensePlanningManager.renderer.getForceColor(forceType);
                    console.log(`🎨 Force color: ${color}`);
                }
                
                return forceType;
            } else {
                console.warn('⚠️ GamePlayManager not available');
                return null;
            }
        } catch (error) {
            console.error('❌ Error refreshing team info:', error);
            return null;
        }
    }

    /**
     * Test the API call directly to debug force type issues
     */
    async testApiCall() {
        console.log('🧪 Testing GetCurrentTeamInfo API call directly...');
        
        try {
            const response = await fetch('/GamePlay/GetCurrentTeamInfo');
            console.log('📡 Direct API Response status:', response.status);
            
            const data = await response.json();
            console.log('📦 Direct API Response data:', data);
            
            if (data.success && data.team) {
                console.log('✅ API call successful:');
                console.log('  - Team Name:', data.team.name);
                console.log('  - Team ID:', data.team.id);
                console.log('  - Force Type:', data.team.forceType);
                
                // Test force color with this data
                if (this.renderer) {
                    const color = this.renderer.getForceColor(data.team.forceType);
                    console.log('  - Detected Color:', color);
                }
                
                return data.team;
            } else {
                console.warn('⚠️ API call failed:', data);
                return null;
            }
        } catch (error) {
            console.error('❌ Error in direct API call:', error);
            return null;
        }
    }

    /**
     * Test defense zone color creation with current force type
     */
    testDefenseZoneColor() {
        console.log('🧪 Testing defense zone color creation...');
        
        // Get current force type
        const forceType = this.getCurrentForceType();
        console.log(`🎨 Current force type: ${forceType}`);
        
        // Test renderer color detection
        if (this.renderer) {
            const color = this.renderer.getForceColor(forceType);
            console.log(`🎨 Renderer detected color: ${color}`);
            
            // Test different force types
            console.log('🧪 Testing different force types:');
            const testTypes = ['Foxland', 'Blueland', 'Fox Land', 'Blue Land', 'Neutral'];
            testTypes.forEach(testType => {
                const testColor = this.renderer.getForceColor(testType);
                console.log(`  ${testType} -> ${testColor}`);
            });
        } else {
            console.warn('⚠️ Renderer not available');
        }
        
        // Check if we have any existing defense zones
        let defenseZoneCount = 0;
        this.defenseElements.forEach((element, id) => {
            if (element.category === 'defensezone') {
                defenseZoneCount++;
                console.log(`🛡️ Existing defense zone ${id}:`, {
                    type: element.type,
                    hasElement: !!element.element,
                    hasShape: !!element.element?.shape
                });
            }
        });
        
        console.log(`📊 Found ${defenseZoneCount} existing defense zones`);
        
        return {
            currentForceType: forceType,
            detectedColor: this.renderer?.getForceColor(forceType),
            existingDefenseZones: defenseZoneCount
        };
    }

    /**
     * Refresh all defense zones with correct colors based on current force type
     * Call this after team info is loaded to fix any color mismatches
     */
    async refreshDefenseZoneColors() {
        console.log('🎨 Refreshing defense zone colors...');
        
        try {
            // Get current force type
            const forceType = this.getCurrentForceType();
            console.log(`🎨 Current force type for refresh: ${forceType}`);
            
            // Clear all layers and reload from database
            console.log('🔄 Clearing and reloading defense elements...');
            await this.loadDefenseElements();
            
            console.log('✅ Defense zone colors refreshed');
            
            return {
                success: true,
                forceType: forceType
            };
        } catch (error) {
            console.error('❌ Error refreshing defense zone colors:', error);
            return {
                success: false,
                error: error.message
            };
        }
    }

    /**
     * Handle right-click on defense element for deletion
     */
    async handleDefenseElementRightClick(e, elementId, category) {
        console.log(`🖱️ Right-click detected on ${category} element: ${elementId}`);
        
        // Prevent default context menu
        e.originalEvent.preventDefault();
        e.originalEvent.stopPropagation();
        
        // Show confirmation dialog
        const elementType = category === 'killzone' ? 'Kill Zone' : 
                          category === 'minefield' ? 'Minefield' : 
                          category === 'defensezone' ? 'Defense Zone' :
                          category.charAt(0).toUpperCase() + category.slice(1);
        
        console.log(`🗑️ Showing delete confirmation for ${elementType}`);
        
        if (confirm(`Are you sure you want to delete this ${elementType}?`)) {
            console.log(`✅ User confirmed deletion of ${elementType}`);
            await this.deleteDefenseElement(elementId, category);
        } else {
            console.log(`❌ User cancelled deletion of ${elementType}`);
        }
    }
    
    /**
     * Delete a defense element by ID
     */
    async deleteDefenseElement(elementId, category) {
        try {
            console.log(`🗑️ Deleting defense element: ${elementId} (${category})`);
            
            // Get element data
            const elementData = this.defenseElements.get(elementId);
            if (!elementData) {
                console.warn(`⚠️ Defense element ${elementId} not found in local storage`);
                return;
            }
            
            console.log(`🔍 Element data structure:`, {
                id: elementData.id,
                category: elementData.category,
                type: elementData.type,
                hasLayers: !!elementData.layers,
                hasDbId: !!elementData.dbId,
                dbId: elementData.dbId,
                layersType: elementData.layers?.constructor?.name
            });
            
            // Remove from map layers based on category
            let removedFromMap = false;
            
            if (category === 'killzone') {
                if (elementData.layers && elementData.layers.polygon) {
                    this.killZoneLayer.removeLayer(elementData.layers.polygon);
                    removedFromMap = true;
                }
                if (elementData.layers && elementData.layers.label) {
                    this.killZoneLayer.removeLayer(elementData.layers.label);
                    removedFromMap = true;
                }
            } else if (category === 'minefield') {
                if (elementData.layers && elementData.layers.markers) {
                    elementData.layers.markers.forEach(marker => {
                        this.minefieldLayer.removeLayer(marker);
                    });
                    removedFromMap = true;
                }
                // Also remove the border if it exists
                if (elementData.layers && elementData.layers.border) {
                    this.minefieldLayer.removeLayer(elementData.layers.border);
                    removedFromMap = true;
                }
                // Also remove the label if it exists
                if (elementData.layers && elementData.layers.label) {
                    this.minefieldLayer.removeLayer(elementData.layers.label);
                    removedFromMap = true;
                }
            } else if (category === 'obstacle') {
                if (elementData.layers && elementData.layers.line) {
                    this.obstacleLayer.removeLayer(elementData.layers.line);
                    removedFromMap = true;
                }
            } else if (category === 'withdrawal' || category === 'route') {
                if (elementData.layers && elementData.layers.route) {
                    this.withdrawalLayer.removeLayer(elementData.layers.route);
                    removedFromMap = true;
                }
            } else if (category === 'line') {
                if (elementData.layers && elementData.layers.line) {
                    this.defensiveLineLayer.removeLayer(elementData.layers.line);
                    removedFromMap = true;
                }
            } else if (category === 'position') {
                if (elementData.layers) {
                    this.positionLayer.removeLayer(elementData.layers);
                    removedFromMap = true;
                }
            } else if (category === 'defensezone') {
                if (elementData.layers || elementData.element) {
                    const layerToRemove = elementData.layers || elementData.element;
                    this.defenseZoneLayer.removeLayer(layerToRemove);
                    removedFromMap = true;
                }
            }
            
            if (!removedFromMap) {
                console.warn(`⚠️ Could not remove ${category} from map - no valid layer found`);
            } else {
                console.log(`✅ Removed ${category} from map`);
            }
            
            // Delete from database if it has a database ID
            let databaseDeleted = false;
            if (elementData.dbId) {
                try {
                    console.log(`🗄️ Deleting from database with ID: ${elementData.dbId}`);
                    const response = await fetch(`/api/DefenseElementApi/delete/${elementData.dbId}`, {
                        method: 'DELETE'
                    });
                    
                    if (response.ok) {
                        const result = await response.json();
                        console.log(`✅ Defense element ${elementId} deleted from database:`, result);
                        databaseDeleted = true;
                    } else {
                        const errorText = await response.text();
                        console.warn(`⚠️ Failed to delete defense element ${elementId} from database:`, errorText);
                    }
                } catch (dbError) {
                    console.error(`❌ Error deleting defense element ${elementId} from database:`, dbError);
                }
            } else {
                console.warn(`⚠️ No database ID found for element ${elementId} - skipping database deletion`);
            }
            
            // Remove from local storage
            this.defenseElements.delete(elementId);
            
            console.log(`✅ Defense element ${elementId} deleted successfully`);
            
            // Refresh elements from database only if database deletion was successful
            // This ensures that if the page is refreshed, deleted elements won't reappear
            if (databaseDeleted) {
                console.log('🔄 Refreshing defense elements from database to ensure consistency...');
                await this.loadDefenseElements();
            }
            
        } catch (error) {
            console.error(`❌ Error deleting defense element ${elementId}:`, error);
        }
    }
    
    /**
     * Load all defense elements from database (single function call)
     * Similar to token placement loading pattern
     * Only loads ACTIVE elements (soft delete handled at database level)
     */
    async loadDefenseElements(gameSessionId = null) {
        try {
            const sessionId = gameSessionId || this.getCurrentGameSessionId();
            
            let response;
            if (sessionId) {
                console.log(`📥 Loading ACTIVE defense elements from database for session ${sessionId}...`);
                response = await fetch(`/api/DefenseElementApi/visible/${sessionId}`);
            } else {
                console.log(`📥 Loading ACTIVE defense elements from database for team (no session)...`);
                response = await fetch(`/api/DefenseElementApi/team`);
            }
            
            if (!response.ok) {
                const errorText = await response.text();
                console.error(`❌ API error (${response.status}):`, errorText);
                return { success: false, message: `API error: ${response.status}` };
            }
            
            const result = await response.json();
            console.log('📥 API response:', result);
            
            if (result.success && result.elements) {
                console.log(`📥 Loaded ${result.elements.length} ACTIVE defense elements from database`);
                
                // Clear existing elements first (removes any locally deleted elements)
                this.clearAllLayers();
                this.defenseElements.clear();
                
                // Reconstruct each ACTIVE element on the map
                for (const dbElement of result.elements) {
                    // Double-check element is active (should be filtered by DAL, but extra safety)
                    if (dbElement.status === 'active') {
                        await this.reconstructDefenseElement(dbElement);
                    } else {
                        console.warn(`⚠️ Skipping inactive element: ${dbElement.elementId} (status: ${dbElement.status})`);
                    }
                }
                
                console.log('✅ All ACTIVE defense elements loaded and visualized');
                
                return {
                    success: true,
                    count: result.elements.length,
                    elements: result.elements
                };
            } else {
                console.log('ℹ️ No ACTIVE defense elements found in database');
                return { success: true, count: 0, elements: [] };
            }
        } catch (error) {
            console.error('❌ Error loading defense elements from database:', error);
            return { success: false, message: error.message };
        }
    }
    
    /**
     * Clear all defense layers
     */
    clearAllLayers() {
        this.killZoneLayer.clearLayers();
        this.minefieldLayer.clearLayers();
        this.obstacleLayer.clearLayers();
        this.positionLayer.clearLayers();
        this.withdrawalLayer.clearLayers();
        this.defensiveLineLayer.clearLayers();
        this.defenseZoneLayer.clearLayers();
    }
    
    /**
     * Reconstruct defense element from database data
     */
    async reconstructDefenseElement(dbElement) {
        try {
            // Parse coordinates from JSON
            const coordinates = JSON.parse(dbElement.coordinates);
            
            // Get current team's force type for color-coding
            const forceType = this.getCurrentForceType();
            
            // Create the visual element using the renderer
            let element;
            
            if (dbElement.category === 'killzone') {
                element = this.renderer.createKillZone(coordinates, dbElement.type, { forceType });
                this.killZoneLayer.addLayer(element.polygon);
                if (element.label) {
                    this.killZoneLayer.addLayer(element.label);
                }
                
                // Add click event
                if (element.polygon) {
                    element.polygon.on('click', () => this.showDefenseElementDetails(dbElement.elementId));
                    element.polygon.on('contextmenu', (e) => this.handleDefenseElementRightClick(e, dbElement.elementId, 'killzone'));
                }
                if (element.label) {
                    element.label.on('contextmenu', (e) => this.handleDefenseElementRightClick(e, dbElement.elementId, 'killzone'));
                }
            } else if (dbElement.category === 'minefield') {
                element = this.renderer.createMinefield(coordinates, dbElement.type, { forceType });
                if (element.markers) {
                    element.markers.forEach(marker => {
                        this.minefieldLayer.addLayer(marker);
                        marker.on('contextmenu', (e) => this.handleDefenseElementRightClick(e, dbElement.elementId, 'minefield'));
                    });
                }
                // Add single border around entire minefield
                if (element.border) {
                    console.log(`🔧 Adding reconstructed minefield border to map:`, element.border);
                    this.minefieldLayer.addLayer(element.border);
                    element.border.on('contextmenu', (e) => this.handleDefenseElementRightClick(e, dbElement.elementId, 'minefield'));
                    console.log(`🔧 Reconstructed minefield border added successfully`);
                } else {
                    console.log(`🔧 No border element found in reconstructed minefield`);
                }
                // Add "Mine field" text label above the minefield
                if (element.label) {
                    console.log(`🔧 Adding reconstructed minefield label to map:`, element.label);
                    this.minefieldLayer.addLayer(element.label);
                    element.label.on('contextmenu', (e) => this.handleDefenseElementRightClick(e, dbElement.elementId, 'minefield'));
                    console.log(`🔧 Reconstructed minefield label added successfully`);
                } else {
                    console.log(`🔧 No label element found in reconstructed minefield`);
                }
            } else if (dbElement.category === 'obstacle') {
                element = this.renderer.createObstacle(coordinates, dbElement.type, { forceType });
                if (element.line) {
                    this.obstacleLayer.addLayer(element.line);
                    element.line.on('contextmenu', (e) => this.handleDefenseElementRightClick(e, dbElement.elementId, 'obstacle'));
                }
            } else if (dbElement.category === 'route') {
                element = this.renderer.createWithdrawalRoute(coordinates, dbElement.type, { forceType });
                if (element.route) {
                    this.withdrawalLayer.addLayer(element.route);
                }
            } else if (dbElement.category === 'line') {
                element = this.renderer.createDefensiveLine(coordinates, dbElement.type, { forceType });
                if (element.line) {
                    this.defensiveLineLayer.addLayer(element.line);
                }
            } else if (dbElement.category === 'position') {
                // Defensive position - coordinates format: [[lat, lng]]
                const latlng = L.latLng(coordinates[0][0], coordinates[0][1]);
                element = this.renderer.createDefensivePosition(latlng, dbElement.type, { forceType });
                this.positionLayer.addLayer(element);
                
                // Add click and right-click events
                element.on('click', () => this.showDefenseElementDetails(dbElement.elementId));
                element.on('contextmenu', (e) => this.handleDefenseElementRightClick(e, dbElement.elementId, 'position'));
            } else if (dbElement.category === 'defensezone') {
                // New defense zone implementation
                console.log(`🔄 Reconstructing defense zone with force type: ${forceType}`);
                element = this.renderer.createDefenseZone(coordinates, dbElement.type, {
                    forceType: forceType,
                    tokenId: dbElement.tokenId,
                    tokenName: dbElement.tokenName || 'Defense Zone'
                });
                
                // Add the entire defense zone group to the layer
                this.defenseZoneLayer.addLayer(element);
                
                // Add click and right-click events to both group and shape
                element.on('click', () => this.showDefenseElementDetails(dbElement.elementId));
                element.on('contextmenu', (e) => this.handleDefenseElementRightClick(e, dbElement.elementId, 'defensezone'));
                
                // Also attach to the shape itself for better event handling
                if (element.shape) {
                    element.shape.on('click', () => this.showDefenseElementDetails(dbElement.elementId));
                    element.shape.on('contextmenu', (e) => this.handleDefenseElementRightClick(e, dbElement.elementId, 'defensezone'));
                }
                
                // Also attach to the token marker for complete coverage
                if (element.tokenMarker) {
                    element.tokenMarker.on('click', () => this.showDefenseElementDetails(dbElement.elementId));
                    element.tokenMarker.on('contextmenu', (e) => this.handleDefenseElementRightClick(e, dbElement.elementId, 'defensezone'));
                }
            }
            
            // Store element locally
            const defenseElementData = {
                id: dbElement.elementId,
                dbId: dbElement.id, // Database ID
                category: dbElement.category,
                type: dbElement.type,
                coordinates: coordinates,
                layers: element,
                tokenId: dbElement.tokenId,
                strength: dbElement.strength,
                visibility: dbElement.visibility,
                createdAt: dbElement.createdDate,
                teamId: dbElement.teamId
            };
            
            this.defenseElements.set(dbElement.elementId, defenseElementData);
            
            console.log(`✅ Reconstructed ${dbElement.category} (${dbElement.type}) - ID: ${dbElement.elementId}${dbElement.tokenId ? ` → Token ${dbElement.tokenId}` : ''}`);
            
        } catch (error) {
            console.error(`❌ Error reconstructing defense element ${dbElement.elementId}:`, error);
        }
    }

    /**
     * Start drawing a kill zone
     */
    startKillZoneDrawing(type = 'primary') {
        this.startPolygonDrawing('killzone', type);
    }

    /**
     * Start drawing a minefield
     */
    startMinefieldDrawing(type = 'mixed') {
        this.startPolygonDrawing('minefield', type);
    }

    /**
     * Start drawing an obstacle
     */
    startObstacleDrawing(type = 'wire') {
        this.startPolylineDrawing('obstacle', type);
    }

    /**
     * Place a defensive position
     */
    placeDefensivePosition(type = 'foxhole') {
        this.drawingMode = 'position';
        this.drawingType = type;
        this.map.getContainer().style.cursor = 'crosshair';
        
        const clickHandler = (e) => {
            this.createDefensivePosition(e.latlng, type);
            this.map.off('click', clickHandler);
            this.map.getContainer().style.cursor = '';
            this.drawingMode = null;
        };
        
        this.map.on('click', clickHandler);
    }

    /**
     * Start drawing a withdrawal route
     */
    startWithdrawalRouteDrawing(type = 'primary') {
        this.startPolylineDrawing('withdrawal', type);
    }

    /**
     * Start drawing a defensive line
     */
    startDefensiveLineDrawing(type = 'feba') {
        this.startPolylineDrawing('line', type);
    }

    /**
     * Start drawing a defense zone (same pattern as kill zones)
     */
    startDefenseZoneDrawing(type = 'primary') {
        this.startPolygonDrawing('defensezone', type);
    }

    /**
     * Start polygon drawing (for kill zones and minefields)
     */
    startPolygonDrawing(category, type) {
        this.drawingMode = category;
        this.drawingType = type;
        this.drawingPoints = [];
        this.map.getContainer().style.cursor = 'crosshair';
        
        console.log(`🖊️ Started drawing ${category} (${type})`);
        
        // Remove any existing event listeners first
        this.map.off('click', this.handlePolygonClick);
        this.map.off('dblclick', this.finishPolygonDrawing);
        
        // Add new event listeners
        this.map.on('click', this.handlePolygonClick.bind(this));
        this.map.on('dblclick', this.finishPolygonDrawing.bind(this));
        
        // Add right-click to finish drawing as alternative
        this.map.on('contextmenu', (e) => {
            if (e.originalEvent) {
                e.originalEvent.preventDefault();
            }
            if (this.drawingMode === category) {
                this.finishPolygonDrawing(e);
            }
        });
        
        // Add keyboard shortcut (Enter key) to finish drawing
        this.keyboardHandler = (e) => {
            if (e.key === 'Enter' && this.drawingMode === category) {
                e.preventDefault();
                this.finishPolygonDrawing(e);
            } else if (e.key === 'Escape') {
                e.preventDefault();
                this.cancelDrawing();
            }
        };
        document.addEventListener('keydown', this.keyboardHandler);
        
        // Show instruction message
        this.showDrawingInstructions(category, type);
    }
    
    /**
     * Show drawing instructions (unified for all polygon types)
     */
    showDrawingInstructions(category, type) {
        // Remove any existing instruction
        const existingInstruction = document.getElementById('defenseDrawingInstructions');
        if (existingInstruction) {
            existingInstruction.remove();
        }
        
        const categoryNames = {
            'killzone': 'Kill Zone',
            'minefield': 'Minefield',
            'defensezone': 'Defense Zone'
        };
        
        const displayName = categoryNames[category] || category;
        
        const instructionDiv = document.createElement('div');
        instructionDiv.id = 'defenseDrawingInstructions';
        instructionDiv.innerHTML = `
            <div style="
                position: fixed;
                top: 20px;
                left: 50%;
                transform: translateX(-50%);
                background: rgba(0, 102, 204, 0.95);
                color: white;
                padding: 12px 20px;
                border-radius: 6px;
                border: 2px solid rgba(255, 255, 255, 0.8);
                box-shadow: 0 4px 12px rgba(0, 0, 0, 0.4);
                font-family: Arial, sans-serif;
                font-size: 14px;
                font-weight: bold;
                z-index: 10000;
                text-align: center;
            ">
                🛡️ Drawing ${displayName} (${type})<br>
                <small style="opacity: 0.9;">Click 4 points, then double-click/Enter/right-click to finish</small>
            </div>
        `;
        document.body.appendChild(instructionDiv);
    }
    
    /**
     * Hide drawing instructions
     */
    hideDrawingInstructions() {
        const instructions = document.getElementById('defenseDrawingInstructions');
        if (instructions) {
            instructions.remove();
        }
    }

    /**
     * Start polyline drawing (for obstacles, withdrawal routes, defensive lines)
     */
    startPolylineDrawing(category, type) {
        this.drawingMode = category;
        this.drawingType = type;
        this.drawingPoints = [];
        this.map.getContainer().style.cursor = 'crosshair';
        
        console.log(`🖊️ Started drawing ${category} (${type})`);
        
        this.map.on('click', this.handlePolylineClick.bind(this));
        this.map.on('dblclick', this.finishPolylineDrawing.bind(this));
    }

    /**
     * Handle polygon click
     */
    handlePolygonClick(e) {
        if (!this.drawingMode) return;
        
        this.drawingPoints.push([e.latlng.lat, e.latlng.lng]);
        
        // Update instruction with point count
        const instructions = document.getElementById('defenseDrawingInstructions');
        if (instructions) {
            const pointsNeeded = Math.max(0, 4 - this.drawingPoints.length);
            const small = instructions.querySelector('small');
            if (small) {
                if (pointsNeeded > 0) {
                    small.textContent = `Click ${pointsNeeded} more point${pointsNeeded !== 1 ? 's' : ''} (${this.drawingPoints.length}/4)`;
                } else {
                    small.textContent = `${this.drawingPoints.length} points - double-click/Enter/right-click to finish`;
                }
            }
        }
        
        // Show temporary polygon
        if (this.tempLayer) {
            this.map.removeLayer(this.tempLayer);
        }
        
        if (this.drawingPoints.length >= 4) {
            this.tempLayer = L.polygon(this.drawingPoints, {
                color: '#ff6600',
                fillOpacity: 0.2,
                weight: 2,
                dashArray: '5, 5'
            }).addTo(this.map);
        } else if (this.drawingPoints.length >= 2) {
            this.tempLayer = L.polyline(this.drawingPoints, {
                color: '#ff6600',
                weight: 2,
                dashArray: '5, 5'
            }).addTo(this.map);
        } else {
            this.tempLayer = L.circleMarker(e.latlng, {
                radius: 5,
                color: '#ff6600'
            }).addTo(this.map);
        }
    }

    /**
     * Handle polyline click
     */
    handlePolylineClick(e) {
        if (!this.drawingMode) return;
        
        this.drawingPoints.push([e.latlng.lat, e.latlng.lng]);
        
        // Show temporary polyline
        if (this.tempLayer) {
            this.map.removeLayer(this.tempLayer);
        }
        
        if (this.drawingPoints.length >= 2) {
            this.tempLayer = L.polyline(this.drawingPoints, {
                color: '#ff6600',
                weight: 2,
                dashArray: '5, 5'
            }).addTo(this.map);
        } else {
            this.tempLayer = L.circleMarker(e.latlng, {
                radius: 5,
                color: '#ff6600'
            }).addTo(this.map);
        }
    }

    /**
     * Finish polygon drawing
     */
    finishPolygonDrawing(e) {
        if (e) {
            L.DomEvent.stop(e);
        }
        
        console.log(`🖊️ Finishing polygon drawing with ${this.drawingPoints.length} points`);
        
        if (this.drawingPoints.length < 4) {
            console.warn('Need at least 4 points to create a polygon - cancelling drawing');
            this.cancelDrawing();
            return;
        }
        
        try {
            this.createPolygonElement(this.drawingMode, this.drawingType, this.drawingPoints);
            console.log(`✅ Successfully created ${this.drawingMode} polygon`);
        } catch (error) {
            console.error('Error creating polygon element:', error);
        }
        
        this.cancelDrawing();
    }

    /**
     * Finish polyline drawing
     */
    finishPolylineDrawing(e) {
        L.DomEvent.stop(e);
        
        if (this.drawingPoints.length < 2) {
            console.warn('Need at least 2 points to create a polyline');
            this.cancelDrawing();
            return;
        }
        
        this.createPolylineElement(this.drawingMode, this.drawingType, this.drawingPoints);
        this.cancelDrawing();
    }

    /**
     * Cancel drawing
     */
    cancelDrawing() {
        // Remove all event listeners
        this.map.off('click', this.handlePolygonClick);
        this.map.off('click', this.handlePolylineClick);
        this.map.off('dblclick', this.finishPolygonDrawing);
        this.map.off('dblclick', this.finishPolylineDrawing);
        this.map.off('contextmenu');
        
        // Remove keyboard handler
        if (this.keyboardHandler) {
            document.removeEventListener('keydown', this.keyboardHandler);
            this.keyboardHandler = null;
        }
        
        // Clean up temporary layer
        if (this.tempLayer) {
            this.map.removeLayer(this.tempLayer);
            this.tempLayer = null;
        }
        
        // Hide drawing instructions
        this.hideDrawingInstructions();
        
        // Reset drawing state
        this.drawingMode = null;
        this.drawingType = null;
        this.drawingPoints = [];
        this.map.getContainer().style.cursor = '';
        
        console.log('🛑 Drawing cancelled and cleaned up');
    }

    /**
     * Create polygon element (kill zone or minefield)
     */
    createPolygonElement(category, type, coordinates) {
        try {
            console.log(`🔧 Creating ${category} element with ${coordinates.length} coordinates`);
            
            // Ensure renderer is available
            if (!this.renderer) {
                console.warn('⚠️ Renderer not available, trying to get it...');
                this.renderer = window.defenseSymbolRenderer;
                
                if (!this.renderer) {
                    console.error('❌ Defense symbol renderer not available');
                    throw new Error('Defense symbol renderer not initialized. Please refresh the page.');
                }
            }
            
            const elementId = this.generateId();
            
            // Get current team's force type for color-coding
            const forceType = this.getCurrentForceType();
            console.log(`🎨 Using force type for defense element: ${forceType}`);
            
            let element;
            
            if (category === 'killzone') {
                element = this.renderer.createKillZone(coordinates, type, { forceType });
                if (!element) {
                    throw new Error('Failed to create kill zone element');
                }
                this.killZoneLayer.addLayer(element.polygon);
                if (element.label) {
                    this.killZoneLayer.addLayer(element.label);
                }
                
                // Add right-click delete functionality
                element.polygon.on('contextmenu', (e) => this.handleDefenseElementRightClick(e, elementId, 'killzone'));
                if (element.label) {
                    element.label.on('contextmenu', (e) => this.handleDefenseElementRightClick(e, elementId, 'killzone'));
                }
                
            } else if (category === 'minefield') {
                console.log(`🔧 Creating minefield with type: ${type}`);
                element = this.renderer.createMinefield(coordinates, type, { forceType });
                if (!element) {
                    throw new Error('Failed to create minefield element');
                }
                // Minefields now return markers and border
                if (element.markers) {
                    console.log(`🔧 Adding ${element.markers.length} mine markers to map`);
                    element.markers.forEach(marker => {
                        this.minefieldLayer.addLayer(marker);
                        // Add right-click delete functionality to each mine marker
                        marker.on('contextmenu', (e) => this.handleDefenseElementRightClick(e, elementId, 'minefield'));
                    });
                }
                // Add single border around entire minefield
                if (element.border) {
                    console.log(`🔧 Adding minefield border to map:`, element.border);
                    this.minefieldLayer.addLayer(element.border);
                    element.border.on('contextmenu', (e) => this.handleDefenseElementRightClick(e, elementId, 'minefield'));
                    console.log(`🔧 Minefield border added successfully`);
                } else {
                    console.log(`🔧 No border element found in minefield`);
                }
                // Add "Mine field" text label above the minefield
                if (element.label) {
                    console.log(`🔧 Adding minefield label to map:`, element.label);
                    this.minefieldLayer.addLayer(element.label);
                    element.label.on('contextmenu', (e) => this.handleDefenseElementRightClick(e, elementId, 'minefield'));
                    console.log(`🔧 Minefield label added successfully`);
                } else {
                    console.log(`🔧 No label element found in minefield`);
                }
            } else if (category === 'defensezone') {
                console.log(`🔧 Creating defense zone with type: ${type}`);
                element = this.renderer.createDefenseZone(coordinates, type, {
                    forceType: forceType,
                    tokenName: `${type.charAt(0).toUpperCase() + type.slice(1)} Defense Zone`,
                    elementId: elementId
                });
                if (!element) {
                    throw new Error('Failed to create defense zone element');
                }
                this.defenseZoneLayer.addLayer(element);
                
                // Add click and right-click handlers to both group and shape
                element.on('click', () => this.showDefenseElementDetails(elementId));
                element.on('contextmenu', (e) => this.handleDefenseElementRightClick(e, elementId, 'defensezone'));
                
                // Also attach to the shape itself for better event handling
                if (element.shape) {
                    element.shape.on('click', () => this.showDefenseElementDetails(elementId));
                    element.shape.on('contextmenu', (e) => this.handleDefenseElementRightClick(e, elementId, 'defensezone'));
                }
                
                // Also attach to the token marker for complete coverage
                if (element.tokenMarker) {
                    element.tokenMarker.on('click', () => this.showDefenseElementDetails(elementId));
                    element.tokenMarker.on('contextmenu', (e) => this.handleDefenseElementRightClick(e, elementId, 'defensezone'));
                }
            } else {
                throw new Error(`Unknown category: ${category}`);
            }
            
            // Store element locally
            const defenseElementData = {
                id: elementId,
                category,
                type,
                coordinates,
                layers: element,
                tokenId: null,
                strength: 100,
                effectiveness: 1.0,
                visibility: 'friendly',
                createdAt: new Date().toISOString()
            };
            
            this.defenseElements.set(elementId, defenseElementData);
            
            // Save to database
            this.saveDefenseElementToDatabase(defenseElementData);
            
            // Add click event
            if (element.polygon) {
                element.polygon.on('click', () => this.showDefenseElementDetails(elementId));
            } else if (category === 'defensezone') {
                element.on('click', () => this.showDefenseElementDetails(elementId));
            }
            
            console.log(`✅ Created ${category} (${type}) with ID: ${elementId}`);
            
            return elementId;
            
        } catch (error) {
            console.error(`❌ Error creating ${category} element:`, error);
            throw error; // Re-throw to be caught by the calling method
        }
    }
    
    /**
     * Save defense element to database
     */
    async saveDefenseElementToDatabase(elementData) {
        try {
            const gameSessionId = this.getCurrentGameSessionId();
            
            const requestData = {
                elementId: elementData.id,
                category: elementData.category,
                type: elementData.type,
                coordinates: elementData.coordinates,
                tokenId: elementData.tokenId,
                strength: elementData.strength || 100,
                effectiveness: elementData.effectiveness || 1.0,
                visibility: elementData.visibility || 'friendly',
                gameSessionId: gameSessionId,
                notes: elementData.notes || null,
                metadata: {
                    createdAt: elementData.createdAt
                }
            };
            
            console.log('📤 Sending defense element to database:', requestData);
            
            const response = await fetch('/api/DefenseElementApi/create', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(requestData)
            });
            
            if (!response.ok) {
                const errorText = await response.text();
                console.error(`❌ Server error (${response.status}):`, errorText);
                console.error('Request data that failed:', requestData);
                return;
            }
            
            const result = await response.json();
            
            if (result.success) {
                console.log(`💾 Defense element saved to database: ${elementData.id}`);
                
                // Store database ID for future updates
                elementData.dbId = result.id;
            } else {
                console.warn(`⚠️ Failed to save defense element to database: ${result.message}`);
            }
        } catch (error) {
            console.error('❌ Error saving defense element to database:', error);
        }
    }
    
    /**
     * Get current game session ID
     */
    getCurrentGameSessionId() {
        // Try to get from session storage or a global variable
        const sessionId = sessionStorage.getItem('currentGameSessionId');
        if (sessionId) {
            return parseInt(sessionId);
        }
        
        // Fallback: try to get from window
        if (window.currentGameSessionId) {
            return window.currentGameSessionId;
        }
        
        // Default to null (will need to be set by user)
        console.warn('⚠️ Game session ID not found - defense element saved without session');
        return null;
    }

    /**
     * Create polyline element (obstacle, withdrawal route, or defensive line)
     */
    createPolylineElement(category, type, coordinates) {
        const elementId = this.generateId();
        
        // Get current team's force type for color-coding
        const forceType = this.getCurrentForceType();
        console.log(`🎨 Using force type for ${category}: ${forceType}`);
        
        let element;
        let targetLayer;
        
        if (category === 'obstacle') {
            element = this.renderer.createObstacle(coordinates, type, { forceType });
            targetLayer = this.obstacleLayer;
        } else if (category === 'withdrawal') {
            element = this.renderer.createWithdrawalRoute(coordinates, type, { forceType });
            targetLayer = this.withdrawalLayer;
            targetLayer.addLayer(element.polyline);
            if (element.arrows) {
                element.arrows.forEach(arrow => targetLayer.addLayer(arrow));
            }
        } else if (category === 'line') {
            element = this.renderer.createDefensiveLine(coordinates, type, { forceType });
            targetLayer = this.defensiveLineLayer;
        }
        
        if (category !== 'withdrawal') {
            targetLayer.addLayer(element);
        }
        
        // Store element
        this.defenseElements.set(elementId, {
            id: elementId,
            category,
            type,
            coordinates,
            layers: element,
            tokenId: null,
            strength: 100,
            effectiveness: 1.0,
            visibility: 'friendly',
            createdAt: new Date().toISOString()
        });
        
        // Save to database
        this.saveDefenseElementToDatabase(this.defenseElements.get(elementId));
        
        // Add click event
        const clickableElement = element.polyline || element;
        clickableElement.on('click', () => this.showDefenseElementDetails(elementId));
        
        console.log(`✅ Created ${category} (${type}) with ID: ${elementId}`);
        
        return elementId;
    }

    /**
     * Create defensive position
     */
    createDefensivePosition(latlng, type) {
        const elementId = this.generateId();
        
        // Get current team's force type for color-coding
        const forceType = this.getCurrentForceType();
        console.log(`🎨 Using force type for defensive position: ${forceType}`);
        
        const marker = this.renderer.createDefensivePosition(latlng, type, { forceType });
        
        this.positionLayer.addLayer(marker);
        
        // Store element
        this.defenseElements.set(elementId, {
            id: elementId,
            category: 'position',
            type,
            coordinates: [[latlng.lat, latlng.lng]],
            layers: marker,
            tokenId: null,
            strength: 100,
            effectiveness: 1.0,
            visibility: 'friendly',
            createdAt: new Date().toISOString()
        });
        
        // Save to database
        this.saveDefenseElementToDatabase(this.defenseElements.get(elementId));
        
        // Add click event
        marker.on('click', () => this.showDefenseElementDetails(elementId));
        
        console.log(`✅ Created defensive position (${type}) with ID: ${elementId}`);
        
        return elementId;
    }

    /**
     * Show defense element details
     */
    showDefenseElementDetails(elementId) {
        const element = this.defenseElements.get(elementId);
        if (!element) {
            console.warn(`Defense element ${elementId} not found`);
            return;
        }
        
        const symbolInfo = this.renderer.getDefenseSymbolInfo(element.type, element.category);
        
        console.log(`📋 Defense Element Details:`, {
            id: element.id,
            category: element.category,
            type: element.type,
            name: symbolInfo?.name,
            tokenId: element.tokenId,
            strength: element.strength,
            visibility: element.visibility,
            createdAt: element.createdAt
        });
        
        // TODO: Show modal with defense element details
        console.log(`Defense Element: ${symbolInfo?.name}, Strength: ${element.strength}%, Visibility: ${element.visibility}`);
    }

    /**
     * Associate defense element with token
     */
    async associateWithToken(elementId, tokenId) {
        const element = this.defenseElements.get(elementId);
        if (element) {
            element.tokenId = tokenId;
            console.log(`🔗 Associated defense element ${elementId} with token ${tokenId}`);
            
            // Update in database if element has dbId
            if (element.dbId) {
                await this.updateTokenAssociationInDatabase(element.dbId, tokenId);
            }
        }
    }
    
    /**
     * Update token association in database
     */
    async updateTokenAssociationInDatabase(defenseElementDbId, tokenId) {
        try {
            const response = await fetch(`/api/DefenseElementApi/associate/${defenseElementDbId}/token/${tokenId}`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                }
            });
            
            const result = await response.json();
            
            if (result.success) {
                console.log(`💾 Token association saved to database. Total defense strength: ${result.totalDefenseStrength}`);
                return result.totalDefenseStrength;
            } else {
                console.warn(`⚠️ Failed to save token association: ${result.message}`);
            }
        } catch (error) {
            console.error('❌ Error saving token association to database:', error);
        }
        
        return null;
    }

    /**
     * Get defense elements for a token
     */
    getTokenDefenseElements(tokenId) {
        const elements = [];
        this.defenseElements.forEach((element, id) => {
            if (element.tokenId === tokenId) {
                elements.push(element);
            }
        });
        return elements;
    }

    /**
     * Remove defense element
     */
    removeDefenseElement(elementId) {
        const element = this.defenseElements.get(elementId);
        if (!element) return;
        
        // Remove from map
        const targetLayer = this.getLayerForCategory(element.category);
        if (element.layers.polygon) {
            targetLayer.removeLayer(element.layers.polygon);
        }
        if (element.layers.label) {
            targetLayer.removeLayer(element.layers.label);
        }
        if (element.layers.markers) {
            element.layers.markers.forEach(marker => targetLayer.removeLayer(marker));
        }
        if (element.layers.polyline) {
            targetLayer.removeLayer(element.layers.polyline);
        }
        if (element.layers.arrows) {
            element.layers.arrows.forEach(arrow => targetLayer.removeLayer(arrow));
        }
        if (element.category === 'position') {
            targetLayer.removeLayer(element.layers);
        }
        if (element.category === 'obstacle' || element.category === 'line') {
            targetLayer.removeLayer(element.layers);
        }
        
        this.defenseElements.delete(elementId);
        console.log(`🗑️ Removed defense element ${elementId}`);
    }

    /**
     * Get layer for category
     */
    getLayerForCategory(category) {
        const layerMap = {
            'killzone': this.killZoneLayer,
            'minefield': this.minefieldLayer,
            'obstacle': this.obstacleLayer,
            'position': this.positionLayer,
            'withdrawal': this.withdrawalLayer,
            'line': this.defensiveLineLayer
        };
        return layerMap[category];
    }

    /**
     * Toggle layer visibility
     */
    toggleLayer(category, visible) {
        const layer = this.getLayerForCategory(category);
        if (visible) {
            layer.addTo(this.map);
        } else {
            this.map.removeLayer(layer);
        }
    }

    /**
     * Generate unique ID (GUID)
     */
    generateId() {
        return crypto.randomUUID();
    }

    /**
     * Clear all defense elements
     */
    clearAll() {
        this.defenseElements.forEach((element, id) => {
            this.removeDefenseElement(id);
        });
        console.log('🧹 Cleared all defense elements');
    }

    /**
     * Export defense elements to JSON
     */
    exportDefenseElements() {
        const elements = [];
        this.defenseElements.forEach((element) => {
            elements.push({
                id: element.id,
                category: element.category,
                type: element.type,
                coordinates: element.coordinates,
                tokenId: element.tokenId,
                strength: element.strength,
                visibility: element.visibility,
                createdAt: element.createdAt
            });
        });
        return elements;
    }

    /**
     * Import defense elements from JSON
     */
    importDefenseElements(elements) {
        elements.forEach(element => {
            if (element.category === 'killzone' || element.category === 'minefield') {
                this.createPolygonElement(element.category, element.type, element.coordinates);
            } else if (element.category === 'obstacle' || element.category === 'withdrawal' || element.category === 'line') {
                this.createPolylineElement(element.category, element.type, element.coordinates);
            } else if (element.category === 'position') {
                const latlng = L.latLng(element.coordinates[0][0], element.coordinates[0][1]);
                this.createDefensivePosition(latlng, element.type);
            }
        });
        console.log(`📥 Imported ${elements.length} defense elements`);
    }
    
    /**
     * Create a new defense zone
     * @param {Array} coordinates - Array of [lat, lng] coordinates defining the zone boundary
     * @param {string} type - Defense zone type ('primary', 'secondary', 'support')
     * @param {Object} options - Additional options (tokenId, tokenName, etc.)
     * @returns {Object} Created defense zone element
     */
    createDefenseZone(coordinates, type = 'primary', options = {}) {
        try {
            console.log(`🛡️ Creating defense zone: ${type} with ${coordinates.length} points`);
            
            // Generate unique element ID
            const elementId = options.elementId || this.generateElementId();
            
            // Get current team's force type for color-coding
            const forceType = this.getCurrentForceType();
            console.log(`🎨 Using force type for defense zone: ${forceType}`);
            
            // Create defense zone using renderer
            const defenseZoneGroup = this.renderer.createDefenseZone(coordinates, type, {
                forceType: forceType,
                elementId: elementId,
                tokenId: options.tokenId,
                tokenName: options.tokenName || 'Defense Zone'
            });
            
            // Add to appropriate layer
            this.defenseZoneLayer.addLayer(defenseZoneGroup);
            
            // Store element data
            const defenseElementData = {
                id: elementId,
                category: 'defensezone',
                type: type,
                coordinates: coordinates,
                element: defenseZoneGroup,
                tokenId: options.tokenId,
                tokenName: options.tokenName || 'Defense Zone',
                strength: options.strength || 100,
                effectiveness: options.effectiveness || 1.0,
                visibility: options.visibility || 'friendly',
                status: 'active',
                notes: options.notes || ''
            };
            
            this.defenseElements.set(elementId, defenseElementData);
            
            // Save to database
            this.saveDefenseElementToDatabase(defenseElementData);
            
            // Add click and right-click handlers to both group and shape
            defenseZoneGroup.on('click', () => this.showDefenseElementDetails(elementId));
            defenseZoneGroup.on('contextmenu', (e) => this.handleDefenseElementRightClick(e, elementId, 'defensezone'));
            
            // Also attach to the shape itself for better event handling
            if (defenseZoneGroup.shape) {
                defenseZoneGroup.shape.on('click', () => this.showDefenseElementDetails(elementId));
                defenseZoneGroup.shape.on('contextmenu', (e) => this.handleDefenseElementRightClick(e, elementId, 'defensezone'));
            }
            
            // Also attach to the token marker for complete coverage
            if (defenseZoneGroup.tokenMarker) {
                defenseZoneGroup.tokenMarker.on('click', () => this.showDefenseElementDetails(elementId));
                defenseZoneGroup.tokenMarker.on('contextmenu', (e) => this.handleDefenseElementRightClick(e, elementId, 'defensezone'));
            }
            
            console.log(`✅ Defense zone created: ${elementId}`);
            
            return defenseElementData;
        } catch (error) {
            console.error('❌ Error creating defense zone:', error);
            throw error;
        }
    }
    
    /**
     * Generate unique element ID (GUID)
     * @returns {string} Unique element ID
     */
    generateElementId() {
        return crypto.randomUUID();
    }
}

// Export for module use
if (typeof module !== 'undefined' && module.exports) {
    module.exports = DefensePlanningManager;
}

console.log('✅ Defense Planning Manager loaded');

