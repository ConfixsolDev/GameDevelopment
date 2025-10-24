/**
 * Attack Visualization Manager
 * Handles drawing attack lines/arrows on the map and managing attack relationships
 */
class AttackVisualizationManager {
    constructor() {
        this.map = null;
        this.attackLines = new Map(); // Store attack line data
        this.attackOrders = new Map(); // Store attack order data
        this.attackLineGroup = null; // Leaflet layer group for attack lines
        this.attackMarkers = new Map(); // Store attack markers (arrows)
        
        console.log('🎯 AttackVisualizationManager initialized');
    }

    /**
     * Initialize the attack visualization manager
     * @param {L.Map} mapInstance - Leaflet map instance
     */
    async initialize(mapInstance) {
        this.map = mapInstance;
        
        // Create layer group for attack lines
        this.attackLineGroup = L.layerGroup().addTo(this.map);
        
        console.log('🎯 AttackVisualizationManager initialized with map');
        
        // Don't load attack orders immediately - wait for tokens to be loaded
        // This will be called after tokens are placed
    }
    
    /**
     * Load attack orders after tokens are placed on the map
     */
    async loadAttackOrdersAfterTokensPlaced() {
        console.log('🎯 Loading attack orders after tokens are placed...');
        
        // Wait a bit for tokens to be fully loaded
        await new Promise(resolve => setTimeout(resolve, 1000));
        
        // Load existing attack orders from database
        await this.loadAttackOrdersFromDatabase();
    }

    /**
     * Load attack orders from database
     */
    async loadAttackOrdersFromDatabase() {
        try {
            console.log('🎯 Loading attack orders from database...');
            
            const response = await fetch('/AttackPlanning/GetAllAttackOrders');
            console.log('📡 Response status:', response.status);
            
            const result = await response.json();
            console.log('📦 Response data:', result);
            
            if (result.success && result.attackOrders) {
                console.log(`🎯 Found ${result.attackOrders.length} attack orders in database`);
                
                // Get all current tokens on map using enhanced functions
                const allTokenMarkers = this.getAllTokenMarkers();
                console.log(`🎯 Current tokens on map: ${allTokenMarkers.length}`);
                
                let processedCount = 0;
                
                for (const order of result.attackOrders) {
                    console.log(`🔍 Processing attack order: ${order.id}`);
                    
                    // Use token IDs directly from database (no mapping needed)
                    const attackerTokenId = order.attackerTokenId;
                    const targetTokenId = order.targetTokenId;
                    
                    console.log(`  🎯 Looking for attacker: ${attackerTokenId}`);
                    console.log(`  🎯 Looking for target: ${targetTokenId}`);
                    
                    // Find tokens on map using enhanced lookup
                    const attackerToken = allTokenMarkers.find(tm => tm.tokenId === attackerTokenId);
                    const targetToken = allTokenMarkers.find(tm => tm.tokenId === targetTokenId);
                    
                    if (attackerToken && targetToken) {
                        console.log(`  ✅ Found tokens: ${attackerToken.tokenData.name} -> ${targetToken.tokenData.name}`);
                        
                        // Create attack line
                        const attackId = this.addAttackLine(attackerToken.tokenData, targetToken.tokenData);
                        
                        if (attackId) {
                            processedCount++;
                            console.log(`  🎯 Attack line created: ${attackId}`);
                        } else {
                            console.warn(`  ⚠️ Failed to create attack line for order: ${order.id}`);
                        }
                    } else {
                        console.warn(`  ⚠️ Could not find tokens for order: ${order.id}`);
                        if (!attackerToken) console.warn(`    - Attacker token not found: ${attackerTokenId}`);
                        if (!targetToken) console.warn(`    - Target token not found: ${targetTokenId}`);
                    }
                }
                
                console.log(`✅ Successfully processed ${processedCount} out of ${result.attackOrders.length} attack orders`);
            } else {
                console.log('🎯 No attack orders found in database');
            }
        } catch (error) {
            console.error('❌ Error loading attack orders from database:', error);
        }
    }

    /**
     * Restore attack order from database
     * @param {Object} orderData - Attack order data from database
     */
    async restoreAttackOrderFromDatabase(orderData) {
        try {
            // Get token information
            const attackerToken = await this.getTokenById(orderData.attackerTokenId);
            const targetToken = await this.getTokenById(orderData.targetTokenId);
            
            if (!attackerToken || !targetToken) {
                console.warn('⚠️ Could not find tokens for attack order:', orderData.id);
                return;
            }
            
            // Parse attack order data
            const attackOrder = {
                id: orderData.id,
                intent: orderData.intentJson ? JSON.parse(orderData.intentJson) : null,
                timing: orderData.timingJson ? JSON.parse(orderData.timingJson) : null,
                movement: orderData.movementJson ? JSON.parse(orderData.movementJson) : null,
                fires: orderData.firesJson ? JSON.parse(orderData.firesJson) : null,
                fogOfWar: orderData.fogOfWarJson ? JSON.parse(orderData.fogOfWarJson) : null,
                logistics: orderData.logisticsJson ? JSON.parse(orderData.logisticsJson) : null,
                roe: orderData.roeJson ? JSON.parse(orderData.roeJson) : null,
                createdDate: orderData.createdDate,
                updatedDate: orderData.updatedDate
            };
            
            // Add attack line
            const attackId = this.addAttackLine(attackerToken, targetToken, attackOrder);
            
            if (attackId) {
                console.log('✅ Attack order restored:', attackerToken.name, '->', targetToken.name);
            }
        } catch (error) {
            console.error('❌ Error restoring attack order:', error);
        }
    }

    /**
     * Get token by ID
     * @param {string} tokenId - Token ID
     */
    async getTokenById(tokenId) {
        console.log('🔍 Looking for token using data-token-guid:', tokenId);
        
        // Method 1: Use data-token-guid attribute from DOM (most reliable)
        const tokenElement = document.querySelector(`[data-token-guid="${tokenId}"]`);
        if (tokenElement) {
            console.log('✅ Found token using data-token-guid:', tokenElement.getAttribute('data-token-name'));
            
            // Get the marker from the element
            const marker = this.getMarkerFromElement(tokenElement);
            if (marker && marker.tokenData) {
                return marker.tokenData;
            }
        }
        
        // Method 2: Search through map markers
        if (this.map) {
            let foundToken = null;
            this.map.eachLayer((layer) => {
                if (layer instanceof L.Marker && layer.tokenId === tokenId) {
                    console.log('✅ Found token marker on map:', layer.tokenData.name);
                    foundToken = layer.tokenData;
                }
            });
            if (foundToken) {
                return foundToken;
            }
        }
        
        // Method 3: Try to get from placed tokens
        if (window.tokenPlacementManager && window.tokenPlacementManager.placedTokens) {
            const tokenInfo = window.tokenPlacementManager.placedTokens.get(tokenId);
            if (tokenInfo && tokenInfo.token) {
                console.log('✅ Found token in placed tokens:', tokenInfo.token.name);
                return tokenInfo.token;
            }
        }
        
        console.warn('⚠️ Token not found:', tokenId);
        return null;
    }

    /**
     * Get marker from DOM element
     */
    getMarkerFromElement(element) {
        if (!element || !this.map) return null;
        
        // Find the marker that corresponds to this DOM element
        let foundMarker = null;
        this.map.eachLayer((layer) => {
            if (layer instanceof L.Marker && layer.getElement && layer.getElement() === element) {
                foundMarker = layer;
            }
        });
        
        return foundMarker;
    }

    /**
     * Get all token markers from the map with their IDs
     */
    getAllTokenMarkers() {
        const tokenMarkers = [];
        if (this.map) {
            this.map.eachLayer((layer) => {
                if (layer instanceof L.Marker && layer.tokenId) {
                    tokenMarkers.push({
                        marker: layer,
                        tokenId: layer.tokenId,
                        tokenData: layer.tokenData,
                        position: layer.getLatLng()
                    });
                }
            });
        }
        return tokenMarkers;
    }

    /**
     * Find token marker by ID from DOM elements
     */
    findTokenMarkerById(tokenId) {
        // Try to find from DOM elements
        const domElements = document.querySelectorAll(`[data-token-id="${tokenId}"]`);
        if (domElements.length > 0) {
            console.log('✅ Found token marker in DOM:', tokenId);
            return domElements[0];
        }
        
        // Try to find from map markers
        const tokenMarkers = this.getAllTokenMarkers();
        const tokenMarker = tokenMarkers.find(tm => tm.tokenId === tokenId);
        if (tokenMarker) {
            console.log('✅ Found token marker on map:', tokenId);
            return tokenMarker.marker;
        }
        
        console.log('⚠️ Token marker not found:', tokenId);
        return null;
    }

    /**
     * Save attack order to database
     * @param {string} attackId - Attack line ID
     * @param {Object} attackOrder - Attack order data
     */
    async saveAttackOrderToDatabase(attackId, attackOrder) {
        try {
            const attackLineData = this.attackLines.get(attackId);
            if (!attackLineData) {
                console.error('❌ Attack line not found for saving:', attackId);
                return false;
            }

            const saveData = {
                orderId: attackOrder.id || attackId,
                tabName: 'complete',
                attackerTokenId: attackLineData.attacker.id,
                targetTokenId: attackLineData.target.id,
                data: {
                    intent: attackOrder.intent,
                    timing: attackOrder.timing,
                    movement: attackOrder.movement,
                    fires: attackOrder.fires,
                    fogOfWar: attackOrder.fogOfWar,
                    logistics: attackOrder.logistics,
                    roe: attackOrder.roe
                }
            };

            const response = await fetch('/AttackPlanning/SaveDraft', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(saveData)
            });

            const result = await response.json();
            
            if (result.success) {
                console.log('✅ Attack order saved to database:', attackId);
                return true;
            } else {
                console.error('❌ Failed to save attack order:', result.message);
                return false;
            }
        } catch (error) {
            console.error('❌ Error saving attack order to database:', error);
            return false;
        }
    }

    /**
     * Delete attack order from database
     * @param {string} attackId - Attack line ID
     */
    async deleteAttackOrderFromDatabase(attackId) {
        try {
            const attackLineData = this.attackLines.get(attackId);
            if (!attackLineData || !attackLineData.attackOrder) {
                console.log('🎯 No database record to delete for attack:', attackId);
                return true;
            }

            const response = await fetch('/AttackPlanning/DeleteAttackOrder', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    orderId: attackLineData.attackOrder.id
                })
            });

            const result = await response.json();
            
            if (result.success) {
                console.log('✅ Attack order deleted from database:', attackId);
                return true;
            } else {
                console.error('❌ Failed to delete attack order:', result.message);
                return false;
            }
        } catch (error) {
            console.error('❌ Error deleting attack order from database:', error);
            return false;
        }
    }

    /**
     * Add an attack line between attacker and target tokens
     * @param {Object} attackerToken - Attacker token data
     * @param {Object} targetToken - Target token data
     * @param {Object} attackOrder - Attack order data (optional)
     */
    addAttackLine(attackerToken, targetToken, attackOrder = null) {
        console.log('🎯 Adding attack line:', attackerToken.name, '->', targetToken.name);
        
        // Validate that attacker and target are different tokens
        if (attackerToken.id === targetToken.id) {
            console.warn('⚠️ Cannot create attack line: attacker and target are the same token');
            return null;
        }
        
        // Create unique attack ID
        const attackId = `${attackerToken.id}_${targetToken.id}`;
        
        // Check if attack line already exists
        if (this.attackLines.has(attackId)) {
            console.log('🎯 Attack line already exists, updating...');
            this.removeAttackLine(attackId);
        }
        
        // Get token positions
        const attackerPos = this.getTokenPosition(attackerToken);
        const targetPos = this.getTokenPosition(targetToken);
        
        if (!attackerPos || !targetPos) {
            console.error('❌ Could not get token positions for attack line');
            return null;
        }
        
        // Validate positions
        if (!isFinite(attackerPos.lat) || !isFinite(attackerPos.lng) || 
            !isFinite(targetPos.lat) || !isFinite(targetPos.lng)) {
            console.error('❌ Invalid token positions:', { attackerPos, targetPos });
            return null;
        }
        
        // Create attack line data
        const attackLineData = {
            id: attackId,
            attacker: attackerToken,
            target: targetToken,
            attackerPos: attackerPos,
            targetPos: targetPos,
            attackOrder: attackOrder,
            created: new Date()
        };
        
        // Draw the attack line
        const attackLine = this.drawAttackLine(attackLineData);
        
        if (attackLine) {
            // Store the attack line
            this.attackLines.set(attackId, attackLineData);
            this.attackOrders.set(attackId, attackOrder);
            
            console.log('✅ Attack line added successfully');
            return attackId;
        }
        
        return null;
    }

    /**
     * Draw an attack line on the map
     * @param {Object} attackLineData - Attack line data
     */
    drawAttackLine(attackLineData) {
        const { attackerPos, targetPos, attacker, target, id } = attackLineData;
        
        // Validate positions before creating curved path
        if (!attackerPos || !targetPos || 
            !isFinite(attackerPos.lat) || !isFinite(attackerPos.lng) || 
            !isFinite(targetPos.lat) || !isFinite(targetPos.lng)) {
            console.error('❌ Invalid positions for attack line:', { attackerPos, targetPos });
            return null;
        }
        
        // Count existing attack lines between the same tokens for spacing
        const existingLines = this.countAttackLinesBetweenTokens(attackerPos, targetPos);
        
        // Create curved path for the attack line with spacing
        const curvedPath = this.createCurvedPath(attackerPos, targetPos, existingLines);
        
        // Validate curved path
        if (!curvedPath || curvedPath.length < 2) {
            console.error('❌ Invalid curved path generated');
            return null;
        }
        
        // Create attack line with NATO styling (includes integrated arrowhead)
        // Get attack type from attack order if available
        const attackType = attackLineData.attackOrder?.intent?.natoAttackType || 'attack-main';
        
        console.log('🎯 AttackVisualizationManager: Creating attack from', attacker.name, 'to', target.name);
        
        let attackLine;
        if (window.attackSymbolRenderer) {
            // Pass attacker and target info for symbol and color determination
            const options = {
                attackerName: attacker.name,
                attackerSymbol: attacker.name,
                attackerToken: attacker,  // Full attacker token data (includes organizationLevel, unitDesignation)
                targetToken: target,      // Full target token data (has placerSide)
                placerSide: target.placerSide || attacker.placerSide  // Use target's placerSide first
            };
            
            console.log('🎯 AttackVisualizationManager: Passing options to renderer:', options);
            
            attackLine = window.attackSymbolRenderer.createAttackLine(curvedPath, attackType, options);
        } else {
            // Fallback to original implementation
            attackLine = L.polyline(curvedPath, {
                color: '#ff4444',
                weight: 4,
                opacity: 0.9,
                className: 'attack-line-solid',
                dashArray: null
            });
        }
        
        // Add click event to show attack summary
        if (attackLine.on) {
        attackLine.on('click', (e) => {
            this.showAttackSummary(attackLineData);
        });
        
            // Add hover effects (brighten the current arrow color)
            const originalColor = attackLine.arrowheadPolygon ? 
                attackLine.arrowheadPolygon.options.color : '#ff4444';
            const hoverColor = this.brightenColor(originalColor);
            
            attackLine.on('mouseover', (e) => {
                // Apply hover effect to individual lines and arrowhead
                if (attackLine.line1 && attackLine.line1.setStyle) {
                    attackLine.line1.setStyle({
                        weight: 5,
                        opacity: 1,
                        color: hoverColor
                    });
                }
                if (attackLine.line2 && attackLine.line2.setStyle) {
                    attackLine.line2.setStyle({
                        weight: 5,
                        opacity: 1,
                        color: hoverColor
                    });
                }
                if (attackLine.arrowheadPolygon && attackLine.arrowheadPolygon.setStyle) {
                    attackLine.arrowheadPolygon.setStyle({
                        weight: 5,
                        color: hoverColor
                    });
                }
            });
            
            attackLine.on('mouseout', (e) => {
                // Restore original style to individual lines and arrowhead
                if (attackLine.line1 && attackLine.line1.setStyle) {
                    attackLine.line1.setStyle({
                        weight: 3,
                        opacity: 0.9,
                        color: originalColor
                    });
                }
                if (attackLine.line2 && attackLine.line2.setStyle) {
                    attackLine.line2.setStyle({
                        weight: 3,
                        opacity: 0.9,
                        color: originalColor
                    });
                }
                if (attackLine.arrowheadPolygon && attackLine.arrowheadPolygon.setStyle) {
                    attackLine.arrowheadPolygon.setStyle({
                        weight: 3,
                        color: originalColor
                    });
                }
            });
        }
        
        // Add to map
        this.attackLineGroup.addLayer(attackLine);
        
        // Store line reference (arrowhead is now part of attackLine)
        attackLineData.line = attackLine;
        
        // Set up token movement tracking
        this.setupTokenMovementTracking(id, attacker, target);
        
        return attackLine;
    }

    /**
     * Create an attack arrow marker
     * @param {L.LatLng} targetPos - Target position
     * @param {L.LatLng} attackerPos - Attacker position
     */
    /**
     * Calculate arrow position based on target and source positions
     * @param {L.LatLng} targetPosition - Target position
     * @param {L.LatLng} sourcePosition - Source position
     * @returns {L.LatLng} Calculated arrow position
     */
    calculateArrowPosition(targetPosition, sourcePosition) {
        // Position arrow at the center of the target token
        // No offset needed - arrow should point directly to the center
        return targetPosition;
    }

    createAttackArrow(targetPos, attackerPos, attackType = 'attack', options = {}) {
        // Use the new AttackSymbolRenderer if available
        if (window.attackSymbolRenderer) {
            return window.attackSymbolRenderer.createAttackArrow(targetPos, attackerPos, attackType, options);
        }
        
        // Fallback to original implementation with symbol number
        const dx = targetPos.lng - attackerPos.lng;
        const dy = targetPos.lat - attackerPos.lat;
        const angle = Math.atan2(dy, dx) * 180 / Math.PI;
        
        // Extract symbol number for fallback
        const attackerName = options.attackerName || options.attackerSymbol || '';
        const symbolMatch = attackerName.match(/(\d{1,3})$/);
        const symbolNumber = symbolMatch ? symbolMatch[1] : attackerName.substring(0, 3).toUpperCase();
        
        const arrowIcon = L.divIcon({
            html: `<div class="attack-arrow-fallback" style="transform: rotate(${angle}deg);">
                <svg width="25" height="20" viewBox="0 0 25 20">
                    <path d="M 5,10 L 20,6 L 20,8 L 23,10 L 20,12 L 20,14 L 5,12 Z" 
                          fill="#ff4444" stroke="#000" stroke-width="1"/>
                    <text x="14" y="10" text-anchor="middle" dominant-baseline="middle" 
                          fill="#FFF" font-size="6" font-weight="bold">${symbolNumber}</text>
                </svg>
            </div>`,
            className: 'attack-arrow-marker',
            iconSize: [25, 20],
            iconAnchor: [5, 10]  // Anchor at left side
        });
        
        // Position arrowhead near the ATTACKER (at the start)
        const ratio = 0.05;
        const arrowheadPos = L.latLng(
            attackerPos.lat + dy * ratio,
            attackerPos.lng + dx * ratio
        );
        
        const arrowMarker = L.marker(arrowheadPos, { icon: arrowIcon });
        
        return arrowMarker;
    }

    /**
     * Count existing attack lines between the same two tokens
     * @param {L.LatLng} start - Start position
     * @param {L.LatLng} end - End position
     * @returns {number} Number of existing lines between these tokens
     */
    countAttackLinesBetweenTokens(start, end) {
        let count = 0;
        const tolerance = 0.0001; // Small tolerance for position comparison
        
        this.attackLines.forEach((attackLineData) => {
            const lineStart = attackLineData.attackerPos;
            const lineEnd = attackLineData.targetPos;
            
            // Check if this line is between the same tokens (with tolerance)
            if (lineStart && lineEnd &&
                Math.abs(lineStart.lat - start.lat) < tolerance &&
                Math.abs(lineStart.lng - start.lng) < tolerance &&
                Math.abs(lineEnd.lat - end.lat) < tolerance &&
                Math.abs(lineEnd.lng - end.lng) < tolerance) {
                count++;
            }
        });
        
        return count;
    }

    /**
     * Create a curved path between two points
     * @param {L.LatLng} start - Start position
     * @param {L.LatLng} end - End position
     * @param {number} lineIndex - Index for spacing multiple lines
     * @returns {Array} Array of LatLng points for the curved path
     */
    createCurvedPath(start, end, lineIndex = 0) {
        // Validate input coordinates
        if (!start || !end || 
            !isFinite(start.lat) || !isFinite(start.lng) || 
            !isFinite(end.lat) || !isFinite(end.lng)) {
            console.error('❌ Invalid coordinates for curved path:', { start, end });
            return [start, end]; // Return simple line if invalid
        }
        
        // Check if start and end are the same point
        if (start.lat === end.lat && start.lng === end.lng) {
            console.warn('⚠️ Start and end points are the same, returning simple line');
            return [start, end];
        }
        
        // Calculate control point for the curve
        const midLat = (start.lat + end.lat) / 2;
        const midLng = (start.lng + end.lng) / 2;
        
        // Calculate distance between points
        const distance = start.distanceTo(end);
        
        // If distance is too small, return simple line
        if (distance < 0.001) {
            console.warn('⚠️ Distance too small for curve, returning simple line');
            return [start, end];
        }
        
        // Create control point offset perpendicular to the line with maximum spacing for multiple lines
        const baseOffset = distance * 0.6; // 60% of the distance as base curve height
        const spacingOffset = distance * 0.25 * lineIndex; // 25% spacing per line for maximum separation
        
        // Alternate direction for even/odd lines for better visual separation
        const direction = lineIndex % 2 === 0 ? 1 : -1;
        const offset = direction * (baseOffset + spacingOffset);
        
        // Calculate perpendicular direction
        const dx = end.lng - start.lng;
        const dy = end.lat - start.lat;
        const length = Math.sqrt(dx * dx + dy * dy);
        
        // Check for division by zero
        if (length === 0) {
            console.warn('⚠️ Zero length vector, returning simple line');
            return [start, end];
        }
        
        // Normalize and rotate 90 degrees
        const perpX = -dy / length;
        const perpY = dx / length;
        
        // Convert offset to lat/lng (approximate)
        const controlLat = midLat + perpY * (offset / 111000); // Rough conversion
        const controlLng = midLng + perpX * (offset / (111000 * Math.cos(midLat * Math.PI / 180)));
        
        // Validate control point
        if (!isFinite(controlLat) || !isFinite(controlLng)) {
            console.warn('⚠️ Invalid control point, returning simple line');
            return [start, end];
        }
        
        const controlPoint = L.latLng(controlLat, controlLng);
        
        // Create curved path using quadratic Bezier curve
        const points = [];
        const steps = 20; // Number of points in the curve
        
        for (let i = 0; i <= steps; i++) {
            const t = i / steps;
            
            // Quadratic Bezier curve formula: B(t) = (1-t)²P₀ + 2(1-t)tP₁ + t²P₂
            const lat = (1 - t) * (1 - t) * start.lat + 2 * (1 - t) * t * controlPoint.lat + t * t * end.lat;
            const lng = (1 - t) * (1 - t) * start.lng + 2 * (1 - t) * t * controlPoint.lng + t * t * end.lng;
            
            // Validate each point
            if (isFinite(lat) && isFinite(lng)) {
                points.push(L.latLng(lat, lng));
            } else {
                console.warn('⚠️ Invalid Bezier point, skipping');
            }
        }
        
        // Ensure we have at least start and end points
        if (points.length < 2) {
            console.warn('⚠️ Not enough valid points, returning simple line');
            return [start, end];
        }
        
        return points;
    }

    /**
     * Set up token movement tracking for attack lines
     * @param {string} attackId - Attack line ID
     * @param {Object} attackerToken - Attacker token data
     * @param {Object} targetToken - Target token data
     */
    setupTokenMovementTracking(attackId, attackerToken, targetToken) {
        if (!this.map) return;
        
        // Find the markers for both tokens
        let attackerMarker = null;
        let targetMarker = null;
        
        this.map.eachLayer((layer) => {
            if (layer instanceof L.Marker) {
                if (layer.tokenId === attackerToken.id) {
                    attackerMarker = layer;
                }
                if (layer.tokenId === targetToken.id) {
                    targetMarker = layer;
                }
            }
        });
        
        if (attackerMarker && targetMarker) {
            // Set up drag event listeners for both markers
            const updateAttackLine = () => {
                const attackLineData = this.attackLines.get(attackId);
                if (attackLineData && attackLineData.line) {
                    // Get new positions
                    const newAttackerPos = attackerMarker.getLatLng();
                    const newTargetPos = targetMarker.getLatLng();
                    
                    // Create new curved path
                    const newCurvedPath = this.createCurvedPath(newAttackerPos, newTargetPos);
                    
                    // Since arrowhead is integrated, we need to recreate the entire attack line
                    // Remove old line
                    this.attackLineGroup.removeLayer(attackLineData.line);
                    
                    // Create new line with integrated arrowhead
                    const attackType = attackLineData.attackOrder?.intent?.natoAttackType || 'attack-main';
                    const newAttackLine = window.attackSymbolRenderer.createAttackLine(newCurvedPath, attackType, {
                        attackerName: attackLineData.attacker.name,
                        attackerSymbol: attackLineData.attacker.name,
                        attackerToken: attackLineData.attacker,
                        targetToken: attackLineData.target,
                        placerSide: attackLineData.target.placerSide || attackLineData.attacker.placerSide
                    });
                    
                    // Add event handlers to new line
                    if (newAttackLine.on) {
                        newAttackLine.on('click', (e) => {
                            this.showAttackSummary(attackLineData);
                        });
                        
                        // Add hover effects with proper color
                        const originalColor = newAttackLine.arrowheadPolygon ? 
                            newAttackLine.arrowheadPolygon.options.color : '#ff4444';
                        const hoverColor = this.brightenColor(originalColor);
                        
                        newAttackLine.on('mouseover', (e) => {
                            // Apply hover effect to individual lines and arrowhead
                            if (newAttackLine.line1 && newAttackLine.line1.setStyle) {
                                newAttackLine.line1.setStyle({
                                    weight: 5,
                                    opacity: 1,
                                    color: hoverColor
                                });
                            }
                            if (newAttackLine.line2 && newAttackLine.line2.setStyle) {
                                newAttackLine.line2.setStyle({
                                    weight: 5,
                                    opacity: 1,
                                    color: hoverColor
                                });
                            }
                            if (newAttackLine.arrowheadPolygon && newAttackLine.arrowheadPolygon.setStyle) {
                                newAttackLine.arrowheadPolygon.setStyle({
                                    weight: 5,
                                    color: hoverColor
                                });
                            }
                        });
                        
                        newAttackLine.on('mouseout', (e) => {
                            // Restore original style to individual lines and arrowhead
                            if (newAttackLine.line1 && newAttackLine.line1.setStyle) {
                                newAttackLine.line1.setStyle({
                                    weight: 3,
                                    opacity: 0.9,
                                    color: originalColor
                                });
                            }
                            if (newAttackLine.line2 && newAttackLine.line2.setStyle) {
                                newAttackLine.line2.setStyle({
                                    weight: 3,
                                    opacity: 0.9,
                                    color: originalColor
                                });
                            }
                            if (newAttackLine.arrowheadPolygon && newAttackLine.arrowheadPolygon.setStyle) {
                                newAttackLine.arrowheadPolygon.setStyle({
                                    weight: 3,
                                    color: originalColor
                                });
                            }
                        });
                    }
                    
                    // Add new line to map
                    this.attackLineGroup.addLayer(newAttackLine);
                    
                    // Update stored references
                    attackLineData.line = newAttackLine;
                    attackLineData.attackerPos = newAttackerPos;
                    attackLineData.targetPos = newTargetPos;
                    
                    console.log('🔄 Attack line updated for token movement:', attackId);
                }
            };
            
            // Add drag event listeners
            attackerMarker.on('drag', updateAttackLine);
            targetMarker.on('drag', updateAttackLine);
            
            console.log('✅ Token movement tracking set up for attack line:', attackId);
        } else {
            console.warn('⚠️ Could not set up movement tracking - markers not found');
        }
    }

    /**
     * Show attack summary popup
     * @param {Object} attackLineData - Attack line data
     */
    showAttackSummary(attackLineData) {
        const { attacker, target, attackOrder } = attackLineData;
        
        // Create summary content
        let summaryContent = `
            <div class="attack-summary-popup">
                <h4><i class="fas fa-crosshairs"></i> Attack Summary</h4>
                <div class="attack-participants">
                    <div class="attacker-info">
                        <strong>Attacker:</strong> ${attacker.name}
                    </div>
                    <div class="target-info">
                        <strong>Target:</strong> ${target.name}
                    </div>
                </div>
        `;
        
        if (attackOrder) {
            summaryContent += `
                <div class="attack-details">
                    <h5>Attack Details:</h5>
                    <div class="detail-item">
                        <strong>Type:</strong> ${attackOrder.intent?.attackType || 'Not specified'}
                    </div>
                    <div class="detail-item">
                        <strong>Maneuver:</strong> ${attackOrder.intent?.maneuverForm || 'Not specified'}
                    </div>
                    <div class="detail-item">
                        <strong>Effect:</strong> ${attackOrder.intent?.desiredEffect || 'Not specified'}
                    </div>
                    <div class="detail-item">
                        <strong>Start Turn:</strong> ${attackOrder.timing?.startTurn || 'Not specified'}
                    </div>
                    <div class="detail-item">
                        <strong>Duration:</strong> ${attackOrder.timing?.durationTurns || 'Not specified'} turns
                    </div>
                </div>
            `;
        } else {
            summaryContent += `
                <div class="attack-details">
                    <p><em>Attack order details not available</em></p>
                </div>
            `;
        }
        
        summaryContent += `
                <div class="attack-actions">
                    <button class="btn btn-sm btn-primary" onclick="window.attackVisualizationManager.editAttackOrder('${attackLineData.id}')">
                        <i class="fas fa-edit"></i> Edit Order
                    </button>
                    <button class="btn btn-sm btn-danger" onclick="window.attackVisualizationManager.removeAttackLine('${attackLineData.id}')">
                        <i class="fas fa-trash"></i> Remove
                    </button>
                </div>
            </div>
        `;
        
        // Create popup
        const popup = L.popup({
            className: 'attack-line-popup',
            closeButton: true,
            autoClose: false,
            closeOnClick: false
        })
        .setLatLng(attackLineData.targetPos)
        .setContent(summaryContent)
        .openOn(this.map);
        
        // Store popup reference
        this.currentSummaryPopup = popup;
        
        console.log('📋 Attack summary popup displayed');
    }

    /**
     * Remove an attack line
     * @param {string} attackId - Attack line ID
     */
    async removeAttackLine(attackId) {
        console.log('🎯 Removing attack line:', attackId);
        
        const attackLineData = this.attackLines.get(attackId);
        if (!attackLineData) {
            console.log('❌ Attack line not found:', attackId);
            return false;
        }
        
        // Remove from database first
        await this.deleteAttackOrderFromDatabase(attackId);
        
        // Remove from map (arrowhead is part of the line now)
        if (attackLineData.line) {
            this.attackLineGroup.removeLayer(attackLineData.line);
        }
        
        // Remove from storage
        this.attackLines.delete(attackId);
        this.attackOrders.delete(attackId);
        
        console.log('✅ Attack line removed successfully');
        return true;
    }

    /**
     * Get all attack lines
     */
    getAllAttackLines() {
        return Array.from(this.attackLines.values());
    }

    /**
     * Clear all attack lines
     */
    clearAllAttackLines() {
        console.log('🎯 Clearing all attack lines');
        
        this.attackLineGroup.clearLayers();
        this.attackLines.clear();
        this.attackOrders.clear();
        this.attackSequenceCounter = 0; // Reset counter when clearing all attacks
        
        console.log('✅ All attack lines cleared');
    }
    
    /**
     * Reset attack sequence counter
     * Call this at the start of a new game session or scenario
     */
    resetAttackSequenceCounter() {
        this.attackSequenceCounter = 0;
        console.log('🔄 Attack sequence counter reset');
    }

    /**
     * Get token position from the map
     * @param {Object} token - Token data
     */
    getTokenPosition(token) {
        console.log('📍 Getting position for token:', token.name, token.id);
        
        // Try to find token marker on the map
        if (window.tokenPlacementManager && window.tokenPlacementManager.placedTokens) {
            const tokenInfo = window.tokenPlacementManager.placedTokens.get(token.id);
            if (tokenInfo && tokenInfo.marker) {
                const position = tokenInfo.marker.getLatLng();
                console.log('✅ Found token position in placed tokens:', position);
                return position;
            }
        }
        
        // Fallback: search through map layers
        if (this.map) {
            const markers = this.map._layers;
            
            for (const layerId in markers) {
                const layer = markers[layerId];
                if (layer && layer.getLatLng && layer.tokenData && layer.tokenData.id === token.id) {
                    const position = layer.getLatLng();
                    console.log('✅ Found token position in map layers:', position);
                    return position;
                }
            }
        }
        
        console.warn('⚠️ Could not find position for token:', token.name);
        return null;
    }

    /**
     * Edit attack order
     * @param {string} attackId - Attack line ID
     */
    editAttackOrder(attackId) {
        const attackLineData = this.attackLines.get(attackId);
        if (!attackLineData) {
            console.error('❌ Attack line not found for editing');
            return;
        }
        
        // Close any existing popups
        this.map.closePopup();
        
        // Open attack planning modal
        if (window.tokenActionModeManager) {
            window.tokenActionModeManager.openAttackDataEntry(
                attackLineData.attacker,
                attackLineData.target
            );
        }
    }

    /**
     * Update attack order data
     * @param {string} attackId - Attack line ID
     * @param {Object} attackOrder - Updated attack order data
     */
    updateAttackOrder(attackId, attackOrder) {
        if (this.attackOrders.has(attackId)) {
            this.attackOrders.set(attackId, attackOrder);
            console.log('✅ Attack order updated:', attackId);
        }
    }

    /**
     * Show attack lines for a specific token
     * @param {string} tokenId - Token ID
     */
    showTokenAttacks(tokenId) {
        const tokenAttacks = this.getAllAttackLines().filter(attack => 
            attack.attacker.id === tokenId || attack.target.id === tokenId
        );
        
        console.log(`🎯 Found ${tokenAttacks.length} attacks for token:`, tokenId);
        return tokenAttacks;
    }

    /**
     * Brighten a hex color for hover effect
     * @param {string} hexColor - Hex color code (e.g., '#ff4444')
     * @returns {string} Brightened hex color
     */
    brightenColor(hexColor) {
        // Remove # if present
        let color = hexColor.replace('#', '');
        
        // Parse RGB
        let r = parseInt(color.substr(0, 2), 16);
        let g = parseInt(color.substr(2, 2), 16);
        let b = parseInt(color.substr(4, 2), 16);
        
        // Brighten by adding 40 to each component (max 255)
        r = Math.min(255, r + 40);
        g = Math.min(255, g + 40);
        b = Math.min(255, b + 40);
        
        // Convert back to hex
        return '#' + 
            r.toString(16).padStart(2, '0') + 
            g.toString(16).padStart(2, '0') + 
            b.toString(16).padStart(2, '0');
    }
}

// Create global instance
const attackVisualizationManager = new AttackVisualizationManager();

// Make globally accessible
window.attackVisualizationManager = attackVisualizationManager;

// Debug function to manually test attack line drawing
window.debugAttackLines = async function() {
    console.log('🔍 DEBUG: Starting manual attack line test...');
    
    if (!window.attackVisualizationManager) {
        console.error('❌ AttackVisualizationManager not available');
        return;
    }
    
    if (!window.attackVisualizationManager.map) {
        console.error('❌ Map not initialized in AttackVisualizationManager');
        return;
    }
    
    console.log('🔍 DEBUG: Loading attack orders manually...');
    
    try {
        const response = await fetch('/AttackPlanning/GetAllAttackOrders');
        const result = await response.json();
        
        console.log('🔍 DEBUG: API Response:', result);
        
        if (result.success && result.attackOrders) {
            console.log(`🔍 DEBUG: Found ${result.attackOrders.length} attack orders`);
            
            for (const order of result.attackOrders) {
                console.log('🔍 DEBUG: Processing order:', order.id);
                console.log('🔍 DEBUG: Attacker ID:', order.attackerTokenId);
                console.log('🔍 DEBUG: Target ID:', order.targetTokenId);
                
                // Try to find tokens
                const attackerToken = await window.attackVisualizationManager.getTokenById(order.attackerTokenId);
                const targetToken = await window.attackVisualizationManager.getTokenById(order.targetTokenId);
                
                console.log('🔍 DEBUG: Attacker token found:', attackerToken);
                console.log('🔍 DEBUG: Target token found:', targetToken);
                
                if (attackerToken && targetToken) {
                    console.log('🔍 DEBUG: Both tokens found, attempting to draw line...');
                    const attackId = window.attackVisualizationManager.addAttackLine(attackerToken, targetToken, order);
                    console.log('🔍 DEBUG: Attack line result:', attackId);
                } else {
                    console.warn('⚠️ DEBUG: Could not find tokens for order:', order.id);
                }
            }
        } else {
            console.log('🔍 DEBUG: No attack orders found or API error');
        }
    } catch (error) {
        console.error('❌ DEBUG: Error loading attack orders:', error);
    }
};

// Debug function to analyze attack orders and token mapping
window.debugAttackOrdersAndTokens = async function() {
    console.log('🔍 DEBUG: Analyzing attack orders and token mapping...');
    
    try {
        // Step 1: Get attack orders from database
        const response = await fetch('/AttackPlanning/GetAllAttackOrders');
        const result = await response.json();
        
        if (!result.success || !result.attackOrders) {
            console.error('❌ Could not get attack orders from database');
            return;
        }
        
        console.log(`📋 Found ${result.attackOrders.length} attack orders in database`);
        
        // Step 2: Get all tokens currently on the map
        const allTokens = [];
        
        // Get tokens from DOM using data-token-guid
        const tokenElements = document.querySelectorAll('[data-token-guid]');
        tokenElements.forEach(element => {
            const tokenGuid = element.getAttribute('data-token-guid');
            const tokenName = element.getAttribute('data-token-name');
            const tokenType = element.getAttribute('data-token-type');
            
            if (tokenGuid && tokenName) {
                allTokens.push({
                    id: tokenGuid,
                    name: tokenName,
                    type: tokenType,
                    source: 'DOM'
                });
            }
        });
        
        console.log(`📋 Found ${allTokens.length} tokens on map:`, allTokens);
        
        // Step 3: Analyze attack orders
        const attackAnalysis = {
            validOrders: [],
            invalidOrders: [],
            duplicateOrders: new Map(),
            tokenMapping: {}
        };
        
        result.attackOrders.forEach(order => {
            const attackerToken = allTokens.find(t => t.id === order.attackerTokenId);
            const targetToken = allTokens.find(t => t.id === order.targetTokenId);
            
            if (attackerToken && targetToken) {
                attackAnalysis.validOrders.push({
                    orderId: order.id,
                    attacker: attackerToken.name,
                    target: targetToken.name,
                    attackerId: order.attackerTokenId,
                    targetId: order.targetTokenId
                });
            } else {
                attackAnalysis.invalidOrders.push({
                    orderId: order.id,
                    attackerTokenId: order.attackerTokenId,
                    targetTokenId: order.targetTokenId,
                    attackerFound: !!attackerToken,
                    targetFound: !!targetToken
                });
            }
            
            // Check for duplicates
            const key = `${order.attackerTokenId}_${order.targetTokenId}`;
            if (attackAnalysis.duplicateOrders.has(key)) {
                attackAnalysis.duplicateOrders.get(key).push(order.id);
            } else {
                attackAnalysis.duplicateOrders.set(key, [order.id]);
            }
        });
        
        console.log('📊 Attack Order Analysis:');
        console.log(`✅ Valid orders: ${attackAnalysis.validOrders.length}`);
        console.log(`❌ Invalid orders: ${attackAnalysis.invalidOrders.length}`);
        console.log(`🔄 Duplicate groups: ${attackAnalysis.duplicateOrders.size}`);
        
        // Step 4: Show detailed analysis
        console.log('📋 Valid Attack Orders:', attackAnalysis.validOrders);
        console.log('❌ Invalid Attack Orders:', attackAnalysis.invalidOrders);
        
        // Step 5: Show duplicates
        console.log('🔄 Duplicate Attack Orders:');
        for (const [key, orderIds] of attackAnalysis.duplicateOrders) {
            if (orderIds.length > 1) {
                console.log(`  ${key}: ${orderIds.length} orders`, orderIds);
            }
        }
        
        // Step 6: Try to create attack lines for valid orders
        if (attackAnalysis.validOrders.length > 0) {
            console.log('🎯 Attempting to create attack lines for valid orders...');
            
            let createdCount = 0;
            for (const validOrder of attackAnalysis.validOrders) {
                try {
                    const attackerToken = allTokens.find(t => t.id === validOrder.attackerId);
                    const targetToken = allTokens.find(t => t.id === validOrder.targetId);
                    
                    if (attackerToken && targetToken) {
                        // Get full token data from TokenPlacementManager
                        let attackerTokenData = null;
                        let targetTokenData = null;
                        
                        if (window.tokenPlacementManager && window.tokenPlacementManager.placedTokens) {
                            for (const [tokenId, tokenInfo] of window.tokenPlacementManager.placedTokens) {
                                if (tokenInfo.token && tokenInfo.token.id === validOrder.attackerId) {
                                    attackerTokenData = tokenInfo.token;
                                }
                                if (tokenInfo.token && tokenInfo.token.id === validOrder.targetId) {
                                    targetTokenData = tokenInfo.token;
                                }
                            }
                        }
                        
                        if (attackerTokenData && targetTokenData) {
                            const attackId = window.attackVisualizationManager.addAttackLine(attackerTokenData, targetTokenData);
                            if (attackId) {
                                createdCount++;
                                console.log(`✅ Created attack line: ${attackerTokenData.name} -> ${targetTokenData.name}`);
                            }
                        }
                    }
                } catch (error) {
                    console.error(`❌ Error creating attack line for order ${validOrder.orderId}:`, error);
                }
            }
            
            console.log(`🎉 Successfully created ${createdCount} attack lines`);
        }
        
        return attackAnalysis;
        
    } catch (error) {
        console.error('❌ Error analyzing attack orders:', error);
    }
};

// Comprehensive diagnostic function to find why attack lines aren't showing
window.diagnoseAttackLines = async function() {
    console.log('🔍 COMPREHENSIVE DIAGNOSTIC: Why attack lines aren\'t showing...');
    
    try {
        // Step 1: Check if AttackVisualizationManager exists
        console.log('1️⃣ Checking AttackVisualizationManager...');
        if (!window.attackVisualizationManager) {
            console.error('❌ AttackVisualizationManager not found!');
            return;
        }
        console.log('✅ AttackVisualizationManager exists');
        
        // Step 2: Check map
        console.log('2️⃣ Checking map...');
        if (!window.attackVisualizationManager.map) {
            console.error('❌ Map not found!');
            return;
        }
        console.log('✅ Map exists');
        
        // Step 3: Check attack line group
        console.log('3️⃣ Checking attack line group...');
        if (!window.attackVisualizationManager.attackLineGroup) {
            console.error('❌ Attack line group not found!');
            return;
        }
        console.log('✅ Attack line group exists');
        
        // Step 4: Get attack orders from database
        console.log('4️⃣ Fetching attack orders from database...');
        const response = await fetch('/AttackPlanning/GetAllAttackOrders');
        const result = await response.json();
        
        if (!result.success || !result.attackOrders) {
            console.error('❌ Could not get attack orders from database');
            return;
        }
        
        console.log(`✅ Found ${result.attackOrders.length} attack orders in database`);
        
        // Step 5: Get all tokens on map
        console.log('5️⃣ Getting tokens on map...');
        const tokensOnMap = [];
        
        // Method 1: From DOM
        const tokenElements = document.querySelectorAll('[data-token-guid]');
        console.log(`📋 Found ${tokenElements.length} token elements in DOM`);
        
        tokenElements.forEach(element => {
            const tokenGuid = element.getAttribute('data-token-guid');
            const tokenName = element.getAttribute('data-token-name');
            const tokenType = element.getAttribute('data-token-type');
            
            if (tokenGuid && tokenName) {
                tokensOnMap.push({
                    id: tokenGuid,
                    name: tokenName,
                    type: tokenType,
                    source: 'DOM'
                });
            }
        });
        
        // Method 2: From TokenPlacementManager
        if (window.tokenPlacementManager && window.tokenPlacementManager.placedTokens) {
            console.log(`📋 Found ${window.tokenPlacementManager.placedTokens.size} tokens in TokenPlacementManager`);
            
            for (const [tokenId, tokenInfo] of window.tokenPlacementManager.placedTokens) {
                if (tokenInfo.token) {
                    const existingToken = tokensOnMap.find(t => t.id === tokenId);
                    if (!existingToken) {
                        tokensOnMap.push({
                            id: tokenId,
                            name: tokenInfo.token.name,
                            type: tokenInfo.token.forceType || 'Unknown',
                            source: 'TokenPlacementManager'
                        });
                    }
                }
            }
        }
        
        console.log(`✅ Total unique tokens on map: ${tokensOnMap.length}`);
        console.log('📋 Tokens on map:', tokensOnMap);
        
        // Step 6: Analyze each attack order
        console.log('6️⃣ Analyzing attack orders...');
        const analysis = {
            validOrders: [],
            invalidOrders: [],
            tokenNotFound: []
        };
        
        result.attackOrders.forEach(order => {
            const attackerToken = tokensOnMap.find(t => t.id === order.attackerTokenId);
            const targetToken = tokensOnMap.find(t => t.id === order.targetTokenId);
            
            if (attackerToken && targetToken) {
                analysis.validOrders.push({
                    orderId: order.id,
                    attacker: attackerToken.name,
                    target: targetToken.name,
                    attackerId: order.attackerTokenId,
                    targetId: order.targetTokenId
                });
            } else {
                analysis.invalidOrders.push({
                    orderId: order.id,
                    attackerTokenId: order.attackerTokenId,
                    targetTokenId: order.targetTokenId,
                    attackerFound: !!attackerToken,
                    targetFound: !!targetToken
                });
                
                if (!attackerToken) {
                    analysis.tokenNotFound.push(`Attacker token not found: ${order.attackerTokenId}`);
                }
                if (!targetToken) {
                    analysis.tokenNotFound.push(`Target token not found: ${order.targetTokenId}`);
                }
            }
        });
        
        console.log('📊 Analysis Results:');
        console.log(`✅ Valid orders: ${analysis.validOrders.length}`);
        console.log(`❌ Invalid orders: ${analysis.invalidOrders.length}`);
        console.log(`🔍 Tokens not found:`, analysis.tokenNotFound);
        
        // Step 7: Try to create attack lines manually
        if (analysis.validOrders.length > 0) {
            console.log('7️⃣ Attempting to create attack lines manually...');
            
            let successCount = 0;
            let errorCount = 0;
            
            for (const validOrder of analysis.validOrders) {
                try {
                    console.log(`🎯 Creating attack line: ${validOrder.attacker} -> ${validOrder.target}`);
                    
                    // Get full token data
                    let attackerTokenData = null;
                    let targetTokenData = null;
                    
                    if (window.tokenPlacementManager && window.tokenPlacementManager.placedTokens) {
                        for (const [tokenId, tokenInfo] of window.tokenPlacementManager.placedTokens) {
                            if (tokenInfo.token && tokenInfo.token.id === validOrder.attackerId) {
                                attackerTokenData = tokenInfo.token;
                            }
                            if (tokenInfo.token && tokenInfo.token.id === validOrder.targetId) {
                                targetTokenData = tokenInfo.token;
                            }
                        }
                    }
                    
                    if (attackerTokenData && targetTokenData) {
                        console.log('✅ Found token data:', {
                            attacker: attackerTokenData.name,
                            target: targetTokenData.name
                        });
                        
                        // Get positions
                        const attackerPos = window.attackVisualizationManager.getTokenPosition(attackerTokenData);
                        const targetPos = window.attackVisualizationManager.getTokenPosition(targetTokenData);
                        
                        console.log('📍 Token positions:', {
                            attacker: attackerPos,
                            target: targetPos
                        });
                        
                        if (attackerPos && targetPos) {
                            // Create attack line
                            const attackId = window.attackVisualizationManager.addAttackLine(attackerTokenData, targetTokenData);
                            
                            if (attackId) {
                                successCount++;
                                console.log(`✅ Successfully created attack line: ${attackId}`);
                            } else {
                                errorCount++;
                                console.error(`❌ Failed to create attack line for: ${validOrder.attacker} -> ${validOrder.target}`);
                            }
                        } else {
                            errorCount++;
                            console.error(`❌ Could not get positions for: ${validOrder.attacker} -> ${validOrder.target}`);
                        }
                    } else {
                        errorCount++;
                        console.error(`❌ Could not find token data for: ${validOrder.attacker} -> ${validOrder.target}`);
                    }
                } catch (error) {
                    errorCount++;
                    console.error(`❌ Error creating attack line for order ${validOrder.orderId}:`, error);
                }
            }
            
            console.log(`🎉 Attack line creation results:`);
            console.log(`✅ Successfully created: ${successCount}`);
            console.log(`❌ Failed to create: ${errorCount}`);
        } else {
            console.log('❌ No valid orders to create attack lines from');
        }
        
        // Step 8: Check if any attack lines exist on map
        console.log('8️⃣ Checking existing attack lines on map...');
        const existingLines = window.attackVisualizationManager.attackLines.size;
        console.log(`📊 Existing attack lines in memory: ${existingLines}`);
        
        // Check map layers
        let linesOnMap = 0;
        window.attackVisualizationManager.map.eachLayer((layer) => {
            if (layer instanceof L.Polyline && layer.options.className === 'attack-line-solid') {
                linesOnMap++;
            }
        });
        console.log(`📊 Attack lines on map: ${linesOnMap}`);
        
        // Step 9: Force refresh attack lines
        console.log('9️⃣ Force refreshing attack lines...');
        window.attackVisualizationManager.loadAttackOrdersFromDatabase();
        
        return analysis;
        
    } catch (error) {
        console.error('❌ Error during diagnostic:', error);
    }
};

// Force clear and redraw all attack lines
window.forceRedrawAttackLines = async function() {
    console.log('🔄 FORCE REDRAW: Clearing and redrawing all attack lines...');
    
    try {
        // Step 1: Clear existing attack lines
        console.log('1️⃣ Clearing existing attack lines...');
        window.attackVisualizationManager.clearAllAttackLines();
        
        // Step 2: Wait a moment
        await new Promise(resolve => setTimeout(resolve, 500));
        
        // Step 3: Reload attack orders from database
        console.log('2️⃣ Reloading attack orders from database...');
        await window.attackVisualizationManager.loadAttackOrdersFromDatabase();
        
        // Step 4: Check results
        console.log('3️⃣ Checking results...');
        const linesInMemory = window.attackVisualizationManager.attackLines.size;
        console.log(`📊 Attack lines in memory: ${linesInMemory}`);
        
        let linesOnMap = 0;
        window.attackVisualizationManager.map.eachLayer((layer) => {
            if (layer instanceof L.Polyline && layer.options.className === 'attack-line-solid') {
                linesOnMap++;
            }
        });
        console.log(`📊 Attack lines on map: ${linesOnMap}`);
        
        if (linesOnMap > 0) {
            console.log('✅ Attack lines successfully redrawn!');
        } else {
            console.log('❌ No attack lines visible on map');
            console.log('🔍 Run diagnoseAttackLines() for detailed analysis');
        }
        
    } catch (error) {
        console.error('❌ Error during force redraw:', error);
    }
};

// Function to clean up duplicate attack orders
window.cleanupDuplicateAttackOrders = async function() {
    console.log('🧹 Starting cleanup of duplicate attack orders...');
    
    try {
        // Get attack orders from database
        const response = await fetch('/AttackPlanning/GetAllAttackOrders');
        const result = await response.json();
        
        if (!result.success || !result.attackOrders) {
            console.error('❌ Could not get attack orders from database');
            return;
        }
        
        // Group orders by attacker-target pair
        const orderGroups = new Map();
        
        result.attackOrders.forEach(order => {
            const key = `${order.attackerTokenId}_${order.targetTokenId}`;
            if (!orderGroups.has(key)) {
                orderGroups.set(key, []);
            }
            orderGroups.get(key).push(order);
        });
        
        console.log(`📊 Found ${orderGroups.size} unique attacker-target pairs`);
        
        // Keep only the most recent order for each pair
        const ordersToDelete = [];
        
        for (const [key, orders] of orderGroups) {
            if (orders.length > 1) {
                console.log(`🔄 Found ${orders.length} duplicate orders for ${key}`);
                
                // Sort by creation date (most recent first)
                orders.sort((a, b) => new Date(b.createdDate) - new Date(a.createdDate));
                
                // Keep the first (most recent) order, mark others for deletion
                const keepOrder = orders[0];
                const deleteOrders = orders.slice(1);
                
                console.log(`✅ Keeping order: ${keepOrder.id} (${keepOrder.createdDate})`);
                console.log(`🗑️ Marking for deletion: ${deleteOrders.map(o => o.id).join(', ')}`);
                
                ordersToDelete.push(...deleteOrders);
            }
        }
        
        console.log(`🗑️ Total orders to delete: ${ordersToDelete.length}`);
        
        if (ordersToDelete.length > 0) {
            // Delete duplicate orders
            for (const order of ordersToDelete) {
                try {
                    const deleteResponse = await fetch(`/AttackPlanning/DeleteAttackOrder/${order.id}`, {
                        method: 'DELETE'
                    });
                    
                    if (deleteResponse.ok) {
                        console.log(`✅ Deleted duplicate order: ${order.id}`);
                    } else {
                        console.error(`❌ Failed to delete order: ${order.id}`);
                    }
                } catch (error) {
                    console.error(`❌ Error deleting order ${order.id}:`, error);
                }
            }
            
            console.log('🎉 Cleanup completed!');
            
            // Reload attack orders
            setTimeout(() => {
                window.debugAttackOrdersAndTokens();
            }, 1000);
        } else {
            console.log('✅ No duplicate orders found');
        }
        
    } catch (error) {
        console.error('❌ Error during cleanup:', error);
    }
};

// Debug function to check token availability
window.debugTokens = function() {
    console.log('🔍 DEBUG: Checking token availability...');
    
    console.log('🔍 DEBUG: TokenPlacementManager:', window.tokenPlacementManager);
    console.log('🔍 DEBUG: TokenManager:', window.tokenManager);
    console.log('🔍 DEBUG: Map:', window.attackVisualizationManager?.map);
    
    if (window.tokenPlacementManager && window.tokenPlacementManager.placedTokens) {
        console.log('🔍 DEBUG: Placed tokens:', window.tokenPlacementManager.placedTokens);
        console.log('🔍 DEBUG: Placed tokens count:', window.tokenPlacementManager.placedTokens.size);
        
        console.log('🔍 DEBUG: === PLACED TOKENS DETAILS ===');
        for (const [tokenId, tokenInfo] of window.tokenPlacementManager.placedTokens) {
            console.log(`🔍 DEBUG: Token ID: ${tokenId}`);
            console.log(`🔍 DEBUG: Token Name: ${tokenInfo.token?.name}`);
            console.log(`🔍 DEBUG: Token Data:`, tokenInfo.token);
            console.log('---');
        }
    }
    
    if (window.tokenManager) {
        console.log('🔍 DEBUG: TokenManager methods:', Object.getOwnPropertyNames(window.tokenManager));
        
        // Try to get tokens from token manager
        if (window.tokenManager.getAllTokens) {
            const allTokens = window.tokenManager.getAllTokens();
            console.log('🔍 DEBUG: All tokens from TokenManager:', allTokens);
        }
    }
    
    // Check map layers for tokens
    if (window.attackVisualizationManager?.map) {
        console.log('🔍 DEBUG: === MAP LAYERS ===');
        const layers = window.attackVisualizationManager.map._layers;
        let tokenCount = 0;
        for (const layerId in layers) {
            const layer = layers[layerId];
            if (layer && layer.tokenData) {
                tokenCount++;
                console.log(`🔍 DEBUG: Map Layer Token ${tokenCount}:`, layer.tokenData);
            }
        }
        console.log(`🔍 DEBUG: Total tokens found in map layers: ${tokenCount}`);
    }
};

// Debug function to manually draw a test line
window.debugDrawTestLine = function() {
    console.log('🔍 DEBUG: Drawing test attack line...');
    
    if (!window.attackVisualizationManager || !window.attackVisualizationManager.map) {
        console.error('❌ AttackVisualizationManager or map not available');
        return;
    }
    
    // Create test positions
    const testAttackerPos = L.latLng(40.7128, -74.0060); // New York
    const testTargetPos = L.latLng(40.7589, -73.9851);   // Central Park
    
    // Create test tokens
    const testAttacker = {
        id: 'test-attacker',
        name: 'Test Attacker',
        type: 'Infantry'
    };
    
    const testTarget = {
        id: 'test-target', 
        name: 'Test Target',
        type: 'Enemy'
    };
    
    // Create test attack line data
    const testAttackLineData = {
        id: 'test-attack',
        attacker: testAttacker,
        target: testTarget,
        attackerPos: testAttackerPos,
        targetPos: testTargetPos,
        attackOrder: null,
        created: new Date()
    };
    
    console.log('🔍 DEBUG: Test attack line data:', testAttackLineData);
    
    // Draw the test line
    const testLine = window.attackVisualizationManager.drawAttackLine(testAttackLineData);
    console.log('🔍 DEBUG: Test line drawn:', testLine);
    
    if (testLine) {
        console.log('✅ DEBUG: Test line successfully drawn!');
    } else {
        console.error('❌ DEBUG: Failed to draw test line');
    }
};

// Debug function to create attack lines using actual tokens on map
window.debugCreateAttackLinesWithRealTokens = function() {
    console.log('🔍 DEBUG: Creating attack lines with real tokens...');
    
    if (!window.tokenPlacementManager || !window.tokenPlacementManager.placedTokens) {
        console.error('❌ No placed tokens available');
        return;
    }
    
    const placedTokens = Array.from(window.tokenPlacementManager.placedTokens.values());
    console.log('🔍 DEBUG: Available tokens:', placedTokens.length);
    
    if (placedTokens.length < 2) {
        console.error('❌ Need at least 2 tokens to create attack line');
        return;
    }
    
    // Use first two tokens
    const attackerToken = placedTokens[0].token;
    const targetToken = placedTokens[1].token;
    
    console.log('🔍 DEBUG: Using attacker:', attackerToken);
    console.log('🔍 DEBUG: Using target:', targetToken);
    
    // Create attack line
    const attackId = window.attackVisualizationManager.addAttackLine(attackerToken, targetToken);
    console.log('🔍 DEBUG: Attack line created with ID:', attackId);
    
    if (attackId) {
        console.log('✅ DEBUG: Attack line successfully created with real tokens!');
    } else {
        console.error('❌ DEBUG: Failed to create attack line with real tokens');
    }
};

// Debug function to fix attack orders with correct token IDs
window.debugFixAttackOrders = async function() {
    console.log('🔍 DEBUG: Fixing attack orders with correct token IDs...');
    
    try {
        // Get current attack orders
        const response = await fetch('/AttackPlanning/GetAllAttackOrders');
        const result = await response.json();
        
        if (!result.success || !result.attackOrders) {
            console.error('❌ DEBUG: Could not get attack orders');
            return;
        }
        
        console.log('🔍 DEBUG: Current attack orders:', result.attackOrders);
        
        console.log('🔍 DEBUG: Using token IDs directly from database (no mapping needed)');
        
        // Display attack orders as they are in database
        for (const order of result.attackOrders) {
            console.log(`🔍 DEBUG: Attack order ${order.id}:`);
            console.log(`  Attacker: ${order.attackerTokenId}`);
            console.log(`  Target: ${order.targetTokenId}`);
        }
        
        console.log('✅ DEBUG: Attack order fixing completed');
        
        // Now try to load attack orders again
        console.log('🔍 DEBUG: Reloading attack orders...');
        await window.attackVisualizationManager.loadAttackOrdersFromDatabase();
        
    } catch (error) {
        console.error('❌ DEBUG: Error fixing attack orders:', error);
    }
};

// Debug function to manually create attack lines with correct token IDs
window.debugCreateAttackLinesWithCorrectIds = async function() {
    console.log('🔍 DEBUG: Creating attack lines with correct token IDs...');
    
    try {
        // Get tokens from the API response you provided
        const tokens = [
            { id: '48490164-6d03-4a98-9be5-003a33a97bf7', name: 'Token 01' },
            { id: '4e35e6f7-47d8-449b-a302-062c187e94d8', name: 'Token 02' },
            { id: 'aee69881-ff99-4520-83b7-b207ede8d9d1', name: 'Token 031' }
        ];
        
        console.log('🔍 DEBUG: Available tokens:', tokens);
        
        // Create attack lines between Token 01 and Token 02
        const attackerToken = tokens[0];
        const targetToken = tokens[1];
        
        console.log('🔍 DEBUG: Creating attack line:', attackerToken.name, '->', targetToken.name);
        
        // Try to find the actual token objects from the map
        const attackerTokenObj = await window.attackVisualizationManager.getTokenById(attackerToken.id);
        const targetTokenObj = await window.attackVisualizationManager.getTokenById(targetToken.id);
        
        console.log('🔍 DEBUG: Attacker token object:', attackerTokenObj);
        console.log('🔍 DEBUG: Target token object:', targetTokenObj);
        
        if (attackerTokenObj && targetTokenObj) {
            const attackId = window.attackVisualizationManager.addAttackLine(attackerTokenObj, targetTokenObj);
            console.log('🔍 DEBUG: Attack line created with ID:', attackId);
            
            if (attackId) {
                console.log('✅ DEBUG: Attack line successfully created!');
            } else {
                console.error('❌ DEBUG: Failed to create attack line');
            }
        } else {
            console.error('❌ DEBUG: Could not find token objects on map');
        }
        
    } catch (error) {
        console.error('❌ DEBUG: Error creating attack lines:', error);
    }
};

// Global utility functions for accessing token information
window.getTokenById = async function(tokenId) {
    if (window.attackVisualizationManager) {
        return await window.attackVisualizationManager.getTokenById(tokenId);
    }
    return null;
};

window.getAllTokenMarkers = function() {
    if (window.attackVisualizationManager) {
        return window.attackVisualizationManager.getAllTokenMarkers();
    }
    return [];
};

window.findTokenMarkerById = function(tokenId) {
    if (window.attackVisualizationManager) {
        return window.attackVisualizationManager.findTokenMarkerById(tokenId);
    }
    return null;
};

window.getTokenFromElement = function(element) {
    if (element && element.getAttribute) {
        const tokenId = element.getAttribute('data-token-id');
        if (tokenId) {
            return {
                id: tokenId,
                name: element.getAttribute('data-token-name'),
                type: element.getAttribute('data-token-type')
            };
        }
    }
    return null;
};

window.getAllTokensFromDOM = function() {
    const tokenElements = document.querySelectorAll('[data-token-id]');
    const tokens = [];
    tokenElements.forEach(element => {
        const token = window.getTokenFromElement(element);
        if (token) {
            tokens.push(token);
        }
    });
    return tokens;
};

// Main function to draw attack lines using enhanced token ID functions
window.drawAttackLinesWithTokenIds = async function() {
    console.log('🎯 Drawing attack lines using enhanced token ID functions...');
    
    try {
        // Step 1: Get all tokens currently on the map
        const allTokenMarkers = getAllTokenMarkers();
        console.log('🔍 Found token markers on map:', allTokenMarkers.length);
        
        if (allTokenMarkers.length < 2) {
            console.warn('⚠️ Need at least 2 tokens to create attack lines');
            return;
        }
        
        // Step 2: Display available tokens
        console.log('📋 Available tokens:');
        allTokenMarkers.forEach((tokenMarker, index) => {
            console.log(`  ${index + 1}. ${tokenMarker.tokenData.name} (ID: ${tokenMarker.tokenId})`);
        });
        
        // Step 3: Create attack lines between different tokens
        const attackLinesCreated = [];
        
        // Create attack lines between Token 01 and Token 02
        const token01 = allTokenMarkers.find(tm => tm.tokenId === '48490164-6d03-4a98-9be5-003a33a97bf7');
        const token02 = allTokenMarkers.find(tm => tm.tokenId === '4e35e6f7-47d8-449b-a302-062c187e94d8');
        
        if (token01 && token02) {
            console.log('🎯 Creating attack line: Token 01 -> Token 02');
            const attackId1 = window.attackVisualizationManager.addAttackLine(token01.tokenData, token02.tokenData);
            if (attackId1) {
                attackLinesCreated.push({ from: 'Token 01', to: 'Token 02', id: attackId1 });
                console.log('✅ Attack line created:', attackId1);
            }
        }
        
        // Create attack line between Token 01 and Token 031
        const token031 = allTokenMarkers.find(tm => tm.tokenId === 'aee69881-ff99-4520-83b7-b207ede8d9d1');
        
        if (token01 && token031) {
            console.log('🎯 Creating attack line: Token 01 -> Token 031');
            const attackId2 = window.attackVisualizationManager.addAttackLine(token01.tokenData, token031.tokenData);
            if (attackId2) {
                attackLinesCreated.push({ from: 'Token 01', to: 'Token 031', id: attackId2 });
                console.log('✅ Attack line created:', attackId2);
            }
        }
        
        // Step 4: Summary
        console.log('🎯 Attack Lines Summary:');
        if (attackLinesCreated.length > 0) {
            attackLinesCreated.forEach(line => {
                console.log(`  ✅ ${line.from} -> ${line.to} (ID: ${line.id})`);
            });
            console.log(`🎉 Successfully created ${attackLinesCreated.length} attack lines!`);
        } else {
            console.warn('⚠️ No attack lines were created');
        }
        
        return attackLinesCreated;
        
    } catch (error) {
        console.error('❌ Error drawing attack lines:', error);
        return [];
    }
};

// Function to fix and display existing attack orders
window.fixAndDisplayAttackOrders = async function() {
    console.log('🔧 Fixing and displaying existing attack orders...');
    
    try {
        // Step 1: Get current attack orders from database
        const response = await fetch('/AttackPlanning/GetAllAttackOrders');
        const result = await response.json();
        
        if (!result.success || !result.attackOrders) {
            console.error('❌ Could not get attack orders from database');
            return;
        }
        
        console.log('📋 Found attack orders in database:', result.attackOrders.length);
        
        // Step 2: Get all current tokens on map
        const allTokenMarkers = getAllTokenMarkers();
        console.log('📋 Current tokens on map:', allTokenMarkers.length);
        
        // Step 3: Use token IDs directly from database (no mapping needed)
        
        // Step 4: Process each attack order
        const processedOrders = [];
        
        for (const order of result.attackOrders) {
            console.log(`🔍 Processing attack order: ${order.id}`);
            
            // Use token IDs directly from database
            const attackerTokenId = order.attackerTokenId;
            const targetTokenId = order.targetTokenId;
            
            // Find tokens on map
            const attackerToken = allTokenMarkers.find(tm => tm.tokenId === attackerTokenId);
            const targetToken = allTokenMarkers.find(tm => tm.tokenId === targetTokenId);
            
            if (attackerToken && targetToken) {
                console.log(`  ✅ Found tokens: ${attackerToken.tokenData.name} -> ${targetToken.tokenData.name}`);
                
                // Create attack line
                const attackId = window.attackVisualizationManager.addAttackLine(attackerToken.tokenData, targetToken.tokenData);
                
                if (attackId) {
                    processedOrders.push({
                        orderId: order.id,
                        attacker: attackerToken.tokenData.name,
                        target: targetToken.tokenData.name,
                        attackId: attackId
                    });
                    console.log(`  🎯 Attack line created: ${attackId}`);
                } else {
                    console.warn(`  ⚠️ Failed to create attack line for order: ${order.id}`);
                }
            } else {
                console.warn(`  ⚠️ Could not find tokens for order: ${order.id}`);
                if (!attackerToken) console.warn(`    - Attacker token not found: ${attackerTokenId}`);
                if (!targetToken) console.warn(`    - Target token not found: ${targetTokenId}`);
            }
        }
        
        // Step 5: Summary
        console.log('🎯 Attack Orders Processing Summary:');
        if (processedOrders.length > 0) {
            processedOrders.forEach(order => {
                console.log(`  ✅ ${order.attacker} -> ${order.target} (Order: ${order.orderId}, Line: ${order.attackId})`);
            });
            console.log(`🎉 Successfully processed ${processedOrders.length} attack orders!`);
        } else {
            console.warn('⚠️ No attack orders were processed');
        }
        
        return processedOrders;
        
    } catch (error) {
        console.error('❌ Error fixing and displaying attack orders:', error);
        return [];
    }
};

// Function to test token ID functions
window.testTokenIdFunctions = async function() {
    console.log('🧪 Testing token ID functions...');
    
    try {
        // Test 1: Get all token markers
        console.log('🔍 Test 1: Getting all token markers...');
        const allTokens = getAllTokenMarkers();
        console.log(`✅ Found ${allTokens.length} token markers`);
        
        // Test 2: Get specific token by ID
        console.log('🔍 Test 2: Getting specific token by ID...');
        const token01 = await getTokenById('48490164-6d03-4a98-9be5-003a33a97bf7');
        if (token01) {
            console.log(`✅ Found Token 01: ${token01.name}`);
        } else {
            console.warn('⚠️ Token 01 not found');
        }
        
        // Test 3: Get tokens from DOM
        console.log('🔍 Test 3: Getting tokens from DOM...');
        const domTokens = getAllTokensFromDOM();
        console.log(`✅ Found ${domTokens.length} tokens in DOM`);
        
        // Test 4: Find token marker by ID
        console.log('🔍 Test 4: Finding token marker by ID...');
        const tokenMarker = findTokenMarkerById('48490164-6d03-4a98-9be5-003a33a97bf7');
        if (tokenMarker) {
            console.log('✅ Found token marker on map');
        } else {
            console.warn('⚠️ Token marker not found on map');
        }
        
        console.log('🎉 Token ID function tests completed!');
        
    } catch (error) {
        console.error('❌ Error testing token ID functions:', error);
    }
};

// Manual trigger to force load attack orders
window.forceLoadAttackOrders = async function() {
    console.log('🚀 Force loading attack orders...');
    
    if (window.attackVisualizationManager) {
        await window.attackVisualizationManager.loadAttackOrdersFromDatabase();
    } else {
        console.error('❌ AttackVisualizationManager not available');
    }
};

// Simple test function to verify enhanced token lookup
window.testEnhancedTokenLookup = async function() {
    console.log('🧪 Testing enhanced token lookup...');
    
    try {
        // Get all token markers
        const allTokenMarkers = getAllTokenMarkers();
        console.log(`✅ Found ${allTokenMarkers.length} token markers`);
        
        // Display all tokens
        allTokenMarkers.forEach((tokenMarker, index) => {
            console.log(`  ${index + 1}. ${tokenMarker.tokenData.name} (ID: ${tokenMarker.tokenId})`);
        });
        
        // Test specific token lookup
        const token01 = allTokenMarkers.find(tm => tm.tokenId === '48490164-6d03-4a98-9be5-003a33a97bf7');
        if (token01) {
            console.log('✅ Token 01 found:', token01.tokenData.name);
        } else {
            console.warn('⚠️ Token 01 not found');
        }
        
        return allTokenMarkers;
        
    } catch (error) {
        console.error('❌ Error testing enhanced token lookup:', error);
        return [];
    }
};

// Manual trigger to force load attack orders with enhanced method
window.forceLoadAttackOrdersEnhanced = async function() {
    console.log('🚀 Force loading attack orders with enhanced method...');
    
    try {
        // Get attack orders from database
        const response = await fetch('/AttackPlanning/GetAllAttackOrders');
        const result = await response.json();
        
        if (!result.success || !result.attackOrders) {
            console.error('❌ Could not get attack orders from database');
            return;
        }
        
        console.log(`📋 Found ${result.attackOrders.length} attack orders in database`);
        
        // Get all current tokens on map
        const allTokenMarkers = getAllTokenMarkers();
        console.log(`📋 Current tokens on map: ${allTokenMarkers.length}`);
        
        // Use token IDs directly from database (no mapping needed)
        
        let processedCount = 0;
        
        for (const order of result.attackOrders) {
            console.log(`🔍 Processing attack order: ${order.id}`);
            
            // Use token IDs directly from database
            const attackerTokenId = order.attackerTokenId;
            const targetTokenId = order.targetTokenId;
            
            // Find tokens on map
            const attackerToken = allTokenMarkers.find(tm => tm.tokenId === attackerTokenId);
            const targetToken = allTokenMarkers.find(tm => tm.tokenId === targetTokenId);
            
            if (attackerToken && targetToken) {
                console.log(`  ✅ Found tokens: ${attackerToken.tokenData.name} -> ${targetToken.tokenData.name}`);
                
                // Create attack line
                const attackId = window.attackVisualizationManager.addAttackLine(attackerToken.tokenData, targetToken.tokenData);
                
                if (attackId) {
                    processedCount++;
                    console.log(`  🎯 Attack line created: ${attackId}`);
                } else {
                    console.warn(`  ⚠️ Failed to create attack line for order: ${order.id}`);
                }
            } else {
                console.warn(`  ⚠️ Could not find tokens for order: ${order.id}`);
                if (!attackerToken) console.warn(`    - Attacker token not found: ${attackerTokenId}`);
                if (!targetToken) console.warn(`    - Target token not found: ${targetTokenId}`);
            }
        }
        
        console.log(`🎉 Successfully processed ${processedCount} out of ${result.attackOrders.length} attack orders`);
        
    } catch (error) {
        console.error('❌ Error force loading attack orders:', error);
    }
};

// Function to update existing token markers with data-id attributes
window.updateExistingTokenMarkers = function() {
    console.log('🔧 Updating existing token markers with data-id attributes...');
    
    let updatedCount = 0;
    
    try {
        // Update markers from TokenPlacementManager
        if (window.tokenPlacementManager && window.tokenPlacementManager.placedTokens) {
            console.log('🔍 Updating markers from TokenPlacementManager...');
            for (const [key, tokenInfo] of window.tokenPlacementManager.placedTokens) {
                if (tokenInfo.marker && tokenInfo.token) {
                    const element = tokenInfo.marker.getElement();
                    if (element) {
                        element.setAttribute('data-id', tokenInfo.token.id);
                        element.setAttribute('data-token-id', tokenInfo.token.id);
                        element.setAttribute('data-token-name', tokenInfo.token.name);
                        element.setAttribute('data-token-type', tokenInfo.token.forceType || 'Unknown');
                        element.setAttribute('data-token-guid', tokenInfo.token.id);
                        
                        // Add meaningful title attribute
                        const tooltipParts = [
                            `${tokenInfo.token.name}`,
                            `Force: ${tokenInfo.token.forceType || 'Unknown'}`,
                            tokenInfo.token.tokenGroupName ? `Group: ${tokenInfo.token.tokenGroupName}` : null
                        ].filter(Boolean);
                        element.setAttribute('title', tooltipParts.join(' | '));
                        
                        element.classList.add('token-marker');
                        
                        // Also set the marker properties
                        tokenInfo.marker.tokenData = tokenInfo.token;
                        tokenInfo.marker.tokenId = tokenInfo.token.id;
                        
                        updatedCount++;
                        console.log(`  ✅ Updated marker for ${tokenInfo.token.name}: data-id="${tokenInfo.token.id}"`);
                    }
                }
            }
        }
        
        // Update markers from map layers
        if (window.gamePlayManager && window.gamePlayManager.map) {
            console.log('🔍 Updating markers from map layers...');
            window.gamePlayManager.map.eachLayer((layer) => {
                if (layer instanceof L.Marker && layer.tokenData && !layer.getElement().hasAttribute('data-id')) {
                    const element = layer.getElement();
                    if (element) {
                        element.setAttribute('data-id', layer.tokenData.id);
                        element.setAttribute('data-token-id', layer.tokenData.id);
                        element.setAttribute('data-token-name', layer.tokenData.name);
                        element.setAttribute('data-token-type', layer.tokenData.forceType || 'Unknown');
                        element.setAttribute('data-token-guid', layer.tokenData.id);
                        
                        // Add meaningful title attribute
                        const tooltipParts = [
                            `${layer.tokenData.name}`,
                            `Force: ${layer.tokenData.forceType || 'Unknown'}`,
                            layer.tokenData.tokenGroupName ? `Group: ${layer.tokenData.tokenGroupName}` : null
                        ].filter(Boolean);
                        element.setAttribute('title', tooltipParts.join(' | '));
                        
                        element.classList.add('token-marker');
                        
                        // Also set the marker properties
                        layer.tokenId = layer.tokenData.id;
                        
                        updatedCount++;
                        console.log(`  ✅ Updated map marker for ${layer.tokenData.name}: data-id="${layer.tokenData.id}"`);
                    }
                }
            });
        }
        
        console.log(`🎉 Successfully updated ${updatedCount} token markers with data-id attributes`);
        
        // Test the updated markers
        const testElements = document.querySelectorAll('[data-id]');
        console.log(`🔍 Found ${testElements.length} elements with data-id attributes`);
        
        testElements.forEach((element, index) => {
            const tokenId = element.getAttribute('data-id');
            const tokenName = element.getAttribute('data-token-name');
            console.log(`  ${index + 1}. ${tokenName} (ID: ${tokenId})`);
        });
        
        return updatedCount;
        
    } catch (error) {
        console.error('❌ Error updating token markers:', error);
        return 0;
    }
};

// Function to find tokens by data-id attribute
window.findTokensByDataId = function() {
    console.log('🔍 Finding tokens by data-id attribute...');
    
    const tokenElements = document.querySelectorAll('[data-id]');
    const tokens = [];
    
    tokenElements.forEach(element => {
        const tokenId = element.getAttribute('data-id');
        const tokenName = element.getAttribute('data-token-name');
        const tokenType = element.getAttribute('data-token-type');
        
        if (tokenId && tokenName) {
            tokens.push({
                element: element,
                id: tokenId,
                name: tokenName,
                type: tokenType,
                position: element.getBoundingClientRect()
            });
        }
    });
    
    console.log(`📋 Found ${tokens.length} tokens with data-id attributes:`);
    tokens.forEach((token, index) => {
        console.log(`  ${index + 1}. ${token.name} (ID: ${token.id}) - Type: ${token.type}`);
    });
    
    return tokens;
};

// Add CSS styles for attack lines
const attackStyles = `
<style>
.attack-line {
    stroke: #ff4444;
    stroke-width: 3;
    stroke-dasharray: 10, 5;
    opacity: 0.8;
    transition: all 0.3s ease;
}

.attack-line:hover {
    stroke-width: 5;
    opacity: 1;
}

.attack-arrow-marker {
    background: transparent !important;
    border: none !important;
}

.attack-arrow {
    color: #ff4444;
    font-size: 16px;
    text-shadow: 1px 1px 2px rgba(0,0,0,0.8);
    animation: pulse 2s infinite;
}

@keyframes pulse {
    0% { opacity: 0.8; }
    50% { opacity: 1; }
    100% { opacity: 0.8; }
}

.attack-summary-popup {
    background: #1a1a1a;
    color: #fff;
    border-radius: 8px;
    padding: 15px;
    min-width: 300px;
}

.attack-summary-popup h4 {
    color: #00d4ff;
    margin: 0 0 15px 0;
    font-size: 16px;
}

.attack-participants {
    margin-bottom: 15px;
}

.attacker-info, .target-info {
    margin: 5px 0;
    font-size: 14px;
}

.attack-details {
    margin-bottom: 15px;
}

.attack-details h5 {
    color: #00d4ff;
    margin: 0 0 10px 0;
    font-size: 14px;
}

.detail-item {
    margin: 5px 0;
    font-size: 12px;
}

.attack-actions {
    display: flex;
    gap: 10px;
    justify-content: center;
}

.attack-actions .btn {
    padding: 5px 10px;
    font-size: 12px;
}
</style>
`;

// Inject styles into the page
document.head.insertAdjacentHTML('beforeend', attackStyles);

console.log('🎯 AttackVisualizationManager loaded and ready');
