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
        // Try to get from global currentTeamInfo
        if (window.currentTeamInfo && window.currentTeamInfo.forceType) {
            return window.currentTeamInfo.forceType;
        }
        
        // Try to get from gamePlayManager
        if (window.gamePlayManager && window.gamePlayManager.currentTeamInfo && window.gamePlayManager.currentTeamInfo.forceType) {
            return window.gamePlayManager.currentTeamInfo.forceType;
        }
        
        // Default to neutral if not available
        console.warn('⚠️ Could not determine force type, using default');
        return 'Neutral';
    }
    
    /**
     * Handle right-click on defense element for deletion
     */
    handleDefenseElementRightClick(e, elementId, category) {
        // Prevent default context menu
        e.originalEvent.preventDefault();
        e.originalEvent.stopPropagation();
        
        // Show confirmation dialog
        const elementType = category === 'killzone' ? 'Kill Zone' : 
                          category === 'minefield' ? 'Minefield' : 
                          category.charAt(0).toUpperCase() + category.slice(1);
        
        if (confirm(`Are you sure you want to delete this ${elementType}?`)) {
            this.deleteDefenseElement(elementId, category);
        }
    }
    
    /**
     * Delete a defense element by ID
     */
    deleteDefenseElement(elementId, category) {
        try {
            console.log(`🗑️ Deleting defense element: ${elementId} (${category})`);
            
            // Remove from local storage
            if (this.defenseElements.has(elementId)) {
                const elementData = this.defenseElements.get(elementId);
                
                // Remove from map layers
                if (elementData.layers) {
                    if (elementData.layers.polygon) {
                        this.map.removeLayer(elementData.layers.polygon);
                    }
                    if (elementData.layers.label) {
                        this.map.removeLayer(elementData.layers.label);
                    }
                    if (elementData.layers.markers) {
                        elementData.layers.markers.forEach(marker => {
                            this.map.removeLayer(marker);
                        });
                    }
                }
                
                // Remove from local storage
                this.defenseElements.delete(elementId);
                
                console.log(`✅ Defense element ${elementId} deleted successfully`);
            } else {
                console.warn(`⚠️ Defense element ${elementId} not found in local storage`);
            }
            
        } catch (error) {
            console.error(`❌ Error deleting defense element ${elementId}:`, error);
        }
    }
    
    /**
     * Load all defense elements from database (single function call)
     * Similar to token placement loading pattern
     */
    async loadDefenseElements(gameSessionId = null) {
        try {
            const sessionId = gameSessionId || this.getCurrentGameSessionId();
            
            if (!sessionId) {
                console.warn('⚠️ No game session ID - cannot load defense elements');
                return { success: false, message: 'No game session ID' };
            }
            
            console.log(`📥 Loading defense elements from database for session ${sessionId}...`);
            
            const response = await fetch(`/api/DefenseElementApi/visible/${sessionId}`);
            const result = await response.json();
            
            if (result.success && result.elements) {
                console.log(`📥 Loaded ${result.elements.length} defense elements from database`);
                
                // Clear existing elements first
                this.clearAllLayers();
                this.defenseElements.clear();
                
                // Reconstruct each element on the map
                for (const dbElement of result.elements) {
                    await this.reconstructDefenseElement(dbElement);
                }
                
                console.log('✅ All defense elements loaded and visualized');
                
                return {
                    success: true,
                    count: result.elements.length,
                    elements: result.elements
                };
            } else {
                console.log('ℹ️ No defense elements found in database');
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
                element = this.renderer.createDefensiveLine(coordinates, dbElement.type);
                if (element.line) {
                    this.defensiveLineLayer.addLayer(element.line);
                }
            } else if (dbElement.category === 'defensezone') {
                // New defense zone implementation
                element = this.renderer.createDefenseZone(coordinates, dbElement.type, {
                    tokenId: dbElement.tokenId,
                    tokenName: dbElement.tokenName || 'Defense Zone'
                });
                
                // Add the entire defense zone group to the layer
                this.defenseZoneLayer.addLayer(element);
                
                // Add click event to the group
                element.on('click', () => this.showDefenseElementDetails(dbElement.elementId));
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
            e.preventDefault();
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
        
        // Show temporary polygon
        if (this.tempLayer) {
            this.map.removeLayer(this.tempLayer);
        }
        
        if (this.drawingPoints.length >= 3) {
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
        
        if (this.drawingPoints.length < 3) {
            console.warn('Need at least 3 points to create a polygon');
            alert('Please click at least 3 points to create a polygon, then double-click to finish.');
            this.cancelDrawing();
            return;
        }
        
        try {
            this.createPolygonElement(this.drawingMode, this.drawingType, this.drawingPoints);
            console.log(`✅ Successfully created ${this.drawingMode} polygon`);
        } catch (error) {
            console.error('Error creating polygon element:', error);
            alert('Error creating polygon. Please try again.');
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
                // Minefields now only return markers (NATO APP-6 standard)
                if (element.markers) {
                    console.log(`🔧 Adding ${element.markers.length} mine markers to map`);
                    element.markers.forEach(marker => {
                        this.minefieldLayer.addLayer(marker);
                        // Add right-click delete functionality to each mine marker
                        marker.on('contextmenu', (e) => this.handleDefenseElementRightClick(e, elementId, 'minefield'));
                    });
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
                visibility: 'friendly',
                createdAt: new Date().toISOString()
            };
            
            this.defenseElements.set(elementId, defenseElementData);
            
            // Save to database
            this.saveDefenseElementToDatabase(defenseElementData);
            
            // Add click event
            if (element.polygon) {
                element.polygon.on('click', () => this.showDefenseElementDetails(elementId));
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
                strength: elementData.strength,
                effectiveness: 1.0,
                visibility: elementData.visibility,
                gameSessionId: gameSessionId,
                notes: null,
                metadata: {
                    createdAt: elementData.createdAt
                }
            };
            
            const response = await fetch('/api/DefenseElementApi/create', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(requestData)
            });
            
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
        let element;
        let targetLayer;
        
        if (category === 'obstacle') {
            element = this.renderer.createObstacle(coordinates, type);
            targetLayer = this.obstacleLayer;
        } else if (category === 'withdrawal') {
            element = this.renderer.createWithdrawalRoute(coordinates, type);
            targetLayer = this.withdrawalLayer;
            targetLayer.addLayer(element.polyline);
            if (element.arrows) {
                element.arrows.forEach(arrow => targetLayer.addLayer(arrow));
            }
        } else if (category === 'line') {
            element = this.renderer.createDefensiveLine(coordinates, type);
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
            visibility: 'friendly',
            createdAt: new Date().toISOString()
        });
        
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
        const marker = this.renderer.createDefensivePosition(latlng, type);
        
        this.positionLayer.addLayer(marker);
        
        // Store element
        this.defenseElements.set(elementId, {
            id: elementId,
            category: 'position',
            type,
            coordinates: [latlng.lat, latlng.lng],
            layers: marker,
            tokenId: null,
            strength: 100,
            visibility: 'friendly',
            createdAt: new Date().toISOString()
        });
        
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
        alert(`Defense Element: ${symbolInfo?.name}\nStrength: ${element.strength}%\nVisibility: ${element.visibility}`);
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
     * Generate unique ID
     */
    generateId() {
        return `def_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
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
                const latlng = L.latLng(element.coordinates[0], element.coordinates[1]);
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
            
            // Create defense zone using renderer
            const defenseZoneGroup = this.renderer.createDefenseZone(coordinates, type, {
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
            
            // Add click handler
            defenseZoneGroup.on('click', () => this.showDefenseElementDetails(elementId));
            
            console.log(`✅ Defense zone created: ${elementId}`);
            
            return defenseElementData;
        } catch (error) {
            console.error('❌ Error creating defense zone:', error);
            throw error;
        }
    }
    
    /**
     * Generate unique element ID
     * @returns {string} Unique element ID
     */
    generateElementId() {
        return 'defensezone_' + Date.now() + '_' + Math.random().toString(36).substr(2, 9);
    }
}

// Export for module use
if (typeof module !== 'undefined' && module.exports) {
    module.exports = DefensePlanningManager;
}

console.log('✅ Defense Planning Manager loaded');

