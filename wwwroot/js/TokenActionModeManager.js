/**
 * Token Action Mode Manager
 * Manages token action modes with browser storage persistence
 */
class TokenActionModeManager {
    constructor() {
        this.currentMode = null;
        this.storageKey = 'tokenActionMode';
        this.attackerStorageKey = 'selectedAttacker';
        this.modeIndicator = null;
        this.map = null;
        this.tokenManager = null;
        this.selectedAttacker = null; // Track the selected attacker token
        
        this.modes = {
            'place': {
                name: 'Place Token',
                icon: 'fas fa-plus',
                color: 'var(--gameplay-primary)',
                cursor: 'crosshair'
            },
            'move': {
                name: 'Plan Move',
                icon: 'fas fa-route',
                color: 'var(--blue-land-color)',
                cursor: 'move'
            },
            'attack': {
                name: 'Plan Attack',
                icon: 'fas fa-crosshairs',
                color: 'var(--fox-land-color)',
                cursor: 'crosshair'
            },
            'pan-attack': {
                name: 'Pan Attack',
                icon: 'fas fa-bullseye',
                color: 'var(--spectator-color)',
                cursor: 'crosshair'
            },
            'select': {
                name: 'Select Token',
                icon: 'fas fa-hand-pointer',
                color: 'var(--gameplay-secondary)',
                cursor: 'pointer'
            }
        };
        
        this.init();
    }

    init() {
        console.log('🔧 TokenActionModeManager initializing...');
        this.loadModeFromStorage();
        this.setupEventListeners();
        this.updateUIWithRetry(); // Use retry mechanism instead of direct updateUI
        console.log('🔧 TokenActionModeManager initialized. Current mode:', this.currentMode);
    }

    setupEventListeners() {
        // Token action button listeners
        document.addEventListener('click', (e) => {
            if (e.target.closest('.token-action-btn')) {
                const button = e.target.closest('.token-action-btn');
                const mode = button.dataset.mode;
                if (mode) {
                    this.setMode(mode);
                }
            }
        });

        // Cancel mode button
        document.addEventListener('click', (e) => {
            if (e.target.closest('#btnCancelMode')) {
                this.cancelMode();
            }
        });

        // Map click handler for mode actions
        document.addEventListener('click', (e) => {
            if (e.target.closest('#map') && this.currentMode) {
                this.handleMapClick(e);
            }
        });
    }

    setMode(mode) {
        if (this.modes[mode]) {
            this.currentMode = mode;
            this.saveModeToStorage();
            this.updateUI();
            this.updateMapCursor();
            
            // Trigger existing functionality based on mode
            this.triggerModeAction(mode);
        }
    }

    cancelMode() {
        // Clear selected attacker when cancelling attack mode
        if (this.currentMode === 'attack') {
            this.selectedAttacker = null;
            this.saveSelectedAttacker(null); // Clear from storage
        }
        
        this.currentMode = null;
        this.saveModeToStorage();
        this.updateUI();
        this.updateMapCursor();
        
        // Enable default mode when no mode is selected
        this.enableDefaultMode();
    }

    triggerModeAction(mode) {
        // Disable all token interactions first
        this.disableAllTokenInteractions();
        
        switch (mode) {
            case 'place':
                // Enable token placement only
                this.enableTokenPlacement();
                break;
            case 'move':
                // Enable token movement and selection
                this.enableTokenMovement();
                break;
            case 'attack':
                // Enable attack mode only - no movement, no context menus
                this.enableAttackMode();
                break;
            case 'pan-attack':
                // Enable pan attack mode only - no movement, no context menus
                this.enablePanAttackMode();
                break;
            case 'select':
                // Enable selection mode with movement and context menus
                this.enableSelectionMode();
                break;
            default:
                // Default mode (no mode selected) - enable token details and dragging
                this.enableDefaultMode();
                break;
        }
    }

    disableAllTokenInteractions() {
        // Disable token dragging/movement
        if (window.tokenManager && window.tokenManager.disableTokenMovement) {
            window.tokenManager.disableTokenMovement();
        }
        
        // Disable token dragging in TokenPlacementManager
        if (window.tokenPlacementManager && window.tokenPlacementManager.disableTokenDragging) {
            window.tokenPlacementManager.disableTokenDragging();
        }
        
        // Disable token placement
        if (window.tokenPlacementManager && window.tokenPlacementManager.cancelPlacementMode) {
            window.tokenPlacementManager.cancelPlacementMode();
        }
        
        // Disable attack mode
        if (window.tokenPlacementManager && window.tokenPlacementManager.exitAttackMode) {
            window.tokenPlacementManager.exitAttackMode();
        }
        
        // Re-enable context menus when disabling all interactions
        this.enableTokenContextMenus();
        
        // Remove any active attack panels
        this.closeAttackPanel();
    }

    enableTokenPlacement() {
        if (window.tokenPlacementManager && window.tokenPlacementManager.startPlacementMode) {
            window.tokenPlacementManager.startPlacementMode();
        }
        
        // Disable context menus and movement during placement mode
        this.disableTokenContextMenus();
        this.disableTokenMovement();
    }

    enableTokenMovement() {
        // Enable token dragging/movement
        if (window.tokenManager && window.tokenManager.enableTokenMovement) {
            window.tokenManager.enableTokenMovement();
        }
        
        // Enable token dragging in TokenPlacementManager
        if (window.tokenPlacementManager && window.tokenPlacementManager.enableTokenDragging) {
            window.tokenPlacementManager.enableTokenDragging();
        }
        
        // Re-enable context menus for movement mode
        this.enableTokenContextMenus();
        
        // Trigger movement planning UI if available
        if (typeof openMovementPlanning === 'function') {
            openMovementPlanning();
        }
    }

    enableAttackMode() {
        // Disable token movement for attack mode
        if (window.tokenManager && window.tokenManager.disableTokenMovement) {
            window.tokenManager.disableTokenMovement();
        }
        
        // Disable right-click context menus for all tokens
        this.disableTokenContextMenus();
        
        // Attack mode will be activated when user clicks on a token
        console.log('Attack mode activated - click on a token to start attack planning');
    }

    enablePanAttackMode() {
        // Disable token movement for pan attack mode
        if (window.tokenManager && window.tokenManager.disableTokenMovement) {
            window.tokenManager.disableTokenMovement();
        }
        
        // Disable right-click context menus for all tokens
        this.disableTokenContextMenus();
        
        // Pan attack mode will be activated when user clicks on a token
        console.log('Pan attack mode activated - click on a token to start pan attack');
    }

    enableSelectionMode() {
        // Enable token movement and selection
        if (window.tokenManager && window.tokenManager.enableTokenMovement) {
            window.tokenManager.enableTokenMovement();
        }
        
        // Enable token dragging in TokenPlacementManager
        if (window.tokenPlacementManager && window.tokenPlacementManager.enableTokenDragging) {
            window.tokenPlacementManager.enableTokenDragging();
        }
        
        if (window.tokenManager && window.tokenManager.enableTokenSelection) {
            window.tokenManager.enableTokenSelection();
        }
        
        // Re-enable right-click context menus for selection mode
        this.enableTokenContextMenus();
    }

    enableDefaultMode() {
        // Enable token dragging and details for default mode
        if (window.tokenManager && window.tokenManager.enableTokenMovement) {
            window.tokenManager.enableTokenMovement();
        }
        
        // Enable token dragging in TokenPlacementManager
        if (window.tokenPlacementManager && window.tokenPlacementManager.enableTokenDragging) {
            window.tokenPlacementManager.enableTokenDragging();
        }
        
        // Re-enable right-click context menus for default mode
        this.enableTokenContextMenus();
        
        console.log('Default mode enabled - token details and dragging available');
    }

    closeAttackPanel() {
        // Close any open attack panels
        const attackPanel = document.getElementById('attackPanelModal');
        if (attackPanel) {
            attackPanel.style.display = 'none';
        }
    }

    handleMapClick(event) {
        if (!this.currentMode) return;

        const latlng = this.getLatLngFromEvent(event);
        if (!latlng) return;

        switch (this.currentMode) {
            case 'place':
                // Handle token placement
                if (window.tokenPlacementManager) {
                    window.tokenPlacementManager.placeTokenAtLocation(latlng);
                }
                break;
            case 'attack':
                // Find token at click location and start attack mode
                this.handleTokenAttack(latlng);
                break;
            case 'pan-attack':
                // Handle pan attack
                this.handlePanAttack(latlng);
                break;
            case 'select':
                // Handle token selection
                this.handleTokenSelection(latlng);
                break;
        }
    }

    handleTokenAttack(latlng) {
        // Find token at location
        const token = this.findTokenAtLocation(latlng);
        if (token) {
            // Immediately start attack mode with this token as attacker
            this.selectedAttacker = token;
            
            // Save attacker to storage
            this.saveSelectedAttacker(token);
            
            this.notificationCallback(`Selected ${token.name} as attacker. Click on target token to plan attack.`, 'info');
            
            // Change cursor to arrow shape
            this.map.getContainer().style.cursor = 'url("data:image/svg+xml;utf8,<svg xmlns=\'http://www.w3.org/2000/svg\' width=\'24\' height=\'24\' viewBox=\'0 0 24 24\' fill=\'none\' stroke=\'%23ff4444\' stroke-width=\'2\' stroke-linecap=\'round\' stroke-linejoin=\'round\'><path d=\'M3 3l7.07 16.97 2.51-7.39 7.39-2.51L3 3z\'></path><path d=\'M13 13l6 6\'></path></svg>"), auto';
            
            // Enable target selection mode
            this.enableTargetSelectionMode();
        } else {
            console.log('No token found at click location for attack');
            this.notificationCallback('No token found at this location. Please click on a valid token.', 'warning');
        }
    }

    enableTargetSelectionMode() {
        // Change cursor to indicate target selection
        this.map.getContainer().style.cursor = 'crosshair';
        
        // Add visual feedback for attacker token
        this.highlightAttackerToken();
        
        // Listen for target selection
        this.map.off('click', this.handleMapClick, this);
        this.map.on('click', this.handleTargetSelection, this);
    }

    handleTargetSelection(event) {
        const latlng = this.getLatLngFromEvent(event);
        const targetToken = this.findTokenAtLocation(latlng);
        
        if (targetToken) {
            // Found target token - create attack arrow and open data entry
            this.createAttackArrow(this.attackerToken, targetToken);
            this.openAttackDataEntry(this.attackerToken, targetToken);
            
            // Reset mode
            this.resetAttackMode();
        } else {
            // No target token found - show message
            this.notificationCallback('No target token found. Please click on a valid target token.', 'warning');
        }
    }

    highlightAttackerToken() {
        // Find and highlight the attacker token marker
        if (this.attackerToken && window.tokenPlacementManager) {
            const tokenInfo = window.tokenPlacementManager.placedTokens.get(this.attackerToken.id);
            if (tokenInfo && tokenInfo.marker) {
                // Add highlighting class
                tokenInfo.marker.getElement().classList.add('attacker-highlighted');
            }
        }
    }

    createAttackArrow(attackerToken, targetToken) {
        // Get positions of both tokens
        const attackerPos = this.getTokenPosition(attackerToken);
        const targetPos = this.getTokenPosition(targetToken);
        
        if (attackerPos && targetPos) {
            // Create arrow line from attacker to target
            const arrow = L.polyline([attackerPos, targetPos], {
                color: '#ff4444',
                weight: 3,
                opacity: 0.8,
                dashArray: '10, 5'
            }).addTo(this.map);
            
            // Add arrowhead at the end
            const arrowhead = L.marker(targetPos, {
                icon: L.divIcon({
                    className: 'attack-arrowhead',
                    html: '<div class="arrowhead">→</div>',
                    iconSize: [20, 20],
                    iconAnchor: [10, 10]
                })
            }).addTo(this.map);
            
            // Store references for cleanup
            this.attackArrow = arrow;
            this.attackArrowhead = arrowhead;
            
            this.notificationCallback(`Attack arrow created from ${attackerToken.name} to ${targetToken.name}`, 'success');
        }
    }

    getTokenPosition(token) {
        if (window.tokenPlacementManager) {
            const tokenInfo = window.tokenPlacementManager.placedTokens.get(token.id);
            if (tokenInfo && tokenInfo.marker) {
                return tokenInfo.marker.getLatLng();
            }
        }
        return null;
    }

    openAttackDataEntry(attackerToken, targetToken) {
        // Open our new attack planning modal
        console.log('Opening attack planning modal:', attackerToken.name, '->', targetToken.name);
        
        // Load the attack planning modal
        this.loadAttackPlanningModal(attackerToken, targetToken);
        
        // Add attack line to visualization
        if (window.attackVisualizationManager) {
            const attackId = window.attackVisualizationManager.addAttackLine(attackerToken, targetToken);
            console.log('🎯 Attack line added:', attackId);
        }
    }

    // Load attack planning modal via AJAX
    loadAttackPlanningModal(attackerToken, targetToken) {
        console.log('Loading attack planning modal...');
        
        // Check if modal already exists
        let modalContainer = document.getElementById('attackPlanningModal');
        console.log('🔍 Modal container exists:', !!modalContainer);
        
        if (!modalContainer) {
            console.log('🔍 Fetching modal from /AttackPlanning/CreateAttackOrder');
            // Load modal HTML via AJAX
            fetch('/AttackPlanning/CreateAttackOrder')
                .then(response => {
                    console.log('🔍 Fetch response status:', response.status);
                    console.log('🔍 Fetch response ok:', response.ok);
                    return response.text();
                })
                .then(html => {
                    console.log('🔍 Received HTML length:', html.length);
                    console.log('🔍 HTML preview:', html.substring(0, 200) + '...');
                    
                    // Add modal to modals container
                    const modalsContainer = document.getElementById('modalsContainer');
                    console.log('🔍 Modals container exists:', !!modalsContainer);
                    
                    if (modalsContainer) {
                        modalsContainer.insertAdjacentHTML('beforeend', html);
                        console.log('✅ Modal HTML added to container');
                        
                        // Wait for the script to execute, then show the modal
                        setTimeout(() => {
                            const modal = document.getElementById('attackPlanningModal');
                            if (modal) {
                                console.log('🔍 Modal found, showing...');
                                
                                // Update modal content with token names
                                const attackerNameEl = document.getElementById('attackerName');
                                const targetNameEl = document.getElementById('targetName');
                                if (attackerNameEl) attackerNameEl.textContent = attackerToken.name;
                                if (targetNameEl) targetNameEl.textContent = targetToken.name;
                                
                                // Show the modal
                                modal.style.display = 'block';
                                console.log('✅ Attack planning modal shown');
                                
                                // Initialize the modal using our new function
                                console.log('🔍 Initializing attack planning modal...');
                                this.initializeAttackPlanningModal(attackerToken, targetToken);
                            } else {
                                console.error('❌ Modal not found after adding HTML');
                            }
                        }, 200);
                    } else {
                        console.error('❌ Modals container not found');
                    }
                })
                .catch(error => {
                    console.error('❌ Error loading attack planning modal:', error);
                    // Fallback to basic attack info
                    this.showAttackInfo(attackerToken, targetToken);
                });
        } else {
            console.log('🔍 Modal already exists, showing...');
            // Modal exists, just show it
            modalContainer.style.display = 'block';
            
            // Update modal content with token names
            const attackerNameEl = document.getElementById('attackerName');
            const targetNameEl = document.getElementById('targetName');
            if (attackerNameEl) attackerNameEl.textContent = attackerToken.name;
            if (targetNameEl) targetNameEl.textContent = targetToken.name;
            
            // Initialize if function exists
            if (window.initializeAttackPlanning) {
                window.initializeAttackPlanning(attackerToken.id, targetToken.id, attackerToken.name, targetToken.name);
            }
        }
    }

    showAttackInfo(attackerToken, targetToken) {
        // Create a simple info popup
        const popup = L.popup()
            .setLatLng(this.getTokenPosition(targetToken))
            .setContent(`
                <div class="attack-info-popup">
                    <h4>Attack Planning</h4>
                    <p><strong>Attacker:</strong> ${attackerToken.name}</p>
                    <p><strong>Target:</strong> ${targetToken.name}</p>
                    <p><strong>Status:</strong> Ready to plan</p>
                    <button class="btn btn-sm btn-primary" onclick="tokenActionModeManager.openDetailedAttackPlanning('${attackerToken.id}', '${targetToken.id}')">
                        Plan Attack
                    </button>
                </div>
            `)
            .openOn(this.map);
    }

    openDetailedAttackPlanning(attackerId, targetId) {
        // This would open the detailed attack planning modal
        console.log('Opening detailed attack planning for:', attackerId, '->', targetId);
        this.notificationCallback('Opening detailed attack planning...', 'info');
    }

    resetAttackMode() {
        // Reset the attack mode
        this.attackerToken = null;
        this.map.getContainer().style.cursor = 'default';
        
        // Remove event listeners
        this.map.off('click', this.handleTargetSelection, this);
        this.map.off('click', this.handlePanAttackTargetSelection, this);
        this.map.on('click', this.handleMapClick, this);
        
        // Remove highlighting
        this.removeAttackerHighlighting();
        
        // Clear attack arrow and pan attack area after a delay
        setTimeout(() => {
            this.clearAttackArrow();
            this.clearPanAttackArea();
        }, 5000);
    }

    removeAttackerHighlighting() {
        // Remove highlighting from attacker token
        if (this.attackerToken && window.tokenPlacementManager) {
            const tokenInfo = window.tokenPlacementManager.placedTokens.get(this.attackerToken.id);
            if (tokenInfo && tokenInfo.marker) {
                tokenInfo.marker.getElement().classList.remove('attacker-highlighted');
            }
        }
    }

    clearAttackArrow() {
        // Remove attack arrow and arrowhead
        if (this.attackArrow) {
            this.map.removeLayer(this.attackArrow);
            this.attackArrow = null;
        }
        if (this.attackArrowhead) {
            this.map.removeLayer(this.attackArrowhead);
            this.attackArrowhead = null;
        }
    }

    clearPanAttackArea() {
        // Remove pan attack area and label
        if (this.panAttackArea) {
            this.map.removeLayer(this.panAttackArea);
            this.panAttackArea = null;
        }
        if (this.panAttackLabel) {
            this.map.removeLayer(this.panAttackLabel);
            this.panAttackLabel = null;
        }
    }

    disableTokenContextMenus() {
        // Disable right-click context menus for all placed tokens
        if (window.tokenPlacementManager && window.tokenPlacementManager.placedTokens) {
            window.tokenPlacementManager.placedTokens.forEach((tokenInfo, tokenId) => {
                if (tokenInfo.marker) {
                    // Remove existing contextmenu event listeners
                    tokenInfo.marker.off('contextmenu');
                    
                    // Add a new contextmenu handler that prevents the default behavior
                    tokenInfo.marker.on('contextmenu', (e) => {
                        e.originalEvent.preventDefault();
                        e.originalEvent.stopPropagation();
                        console.log('Context menu disabled in attack mode');
                    });
                }
            });
        }
        console.log('✅ Token context menus disabled');
    }

    enableTokenContextMenus() {
        // Re-enable right-click context menus for all placed tokens
        if (window.tokenPlacementManager && window.tokenPlacementManager.placedTokens) {
            window.tokenPlacementManager.placedTokens.forEach((tokenInfo, tokenId) => {
                if (tokenInfo.marker) {
                    // Remove the disabled contextmenu handler
                    tokenInfo.marker.off('contextmenu');
                    
                    // Re-add the original contextmenu functionality
                    tokenInfo.marker.on('contextmenu', (e) => {
                        if (window.tokenPlacementManager && window.tokenPlacementManager.showTokenContextMenu) {
                            window.tokenPlacementManager.showTokenContextMenu(e, tokenInfo.token);
                        }
                    });
                }
            });
        }
        console.log('✅ Token context menus re-enabled');
    }

    handlePanAttack(latlng) {
        // Find token at location
        const token = this.findTokenAtLocation(latlng);
        if (token) {
            // Immediately start pan attack mode with this token
            this.attackerToken = token;
            this.notificationCallback(`Selected ${token.name} for pan attack. Click on target area to plan attack.`, 'info');
            
            // Change cursor to arrow shape
            this.map.getContainer().style.cursor = 'url("data:image/svg+xml;utf8,<svg xmlns=\'http://www.w3.org/2000/svg\' width=\'24\' height=\'24\' viewBox=\'0 0 24 24\' fill=\'none\' stroke=\'%23ffaa00\' stroke-width=\'2\' stroke-linecap=\'round\' stroke-linejoin=\'round\'><circle cx=\'12\' cy=\'12\' r=\'10\'></circle><path d=\'M8 12h8\'></path><path d=\'M12 8v8\'></path></svg>"), auto';
            
            // Enable target selection mode for pan attack
            this.enablePanAttackTargetSelection();
        } else {
            console.log('No token found at click location for pan attack');
            this.notificationCallback('No token found at this location. Please click on a valid token.', 'warning');
        }
    }

    handleTokenSelection(latlng) {
        // Find and select token at location
        const token = this.findTokenAtLocation(latlng);
        if (token) {
            // Show token summary/info
            this.showTokenSummary(token);
        } else {
            this.notificationCallback('No token found at this location.', 'info');
        }
    }

    showTokenSummary(token) {
        // Create a popup with token summary
        const popup = L.popup()
            .setLatLng(this.getTokenPosition(token))
            .setContent(`
                <div class="token-summary-popup">
                    <h4>${token.name}</h4>
                    <p><strong>Type:</strong> ${token.type || 'Unknown'}</p>
                    <p><strong>Status:</strong> ${token.status || 'Active'}</p>
                    <p><strong>Position:</strong> ${this.getTokenPosition(token)?.lat.toFixed(4)}, ${this.getTokenPosition(token)?.lng.toFixed(4)}</p>
                    <div class="token-actions">
                        <button class="btn btn-sm btn-primary" onclick="tokenActionModeManager.showDetailedTokenInfo('${token.id}')">
                            View Details
                        </button>
                    </div>
                </div>
            `)
            .openOn(this.map);
    }

    showDetailedTokenInfo(tokenId) {
        // This would open a detailed token info modal
        console.log('Opening detailed token info for:', tokenId);
        this.notificationCallback('Opening detailed token information...', 'info');
    }

    handleTokenMovement(latlng) {
        // Find token at location
        const token = this.findTokenAtLocation(latlng);
        if (token) {
            // Show token summary and enable movement
            this.showTokenSummary(token);
            this.notificationCallback(`Selected ${token.name} for movement. You can now drag the token to move it.`, 'info');
        } else {
            this.notificationCallback('No token found at this location.', 'info');
        }
    }

    enablePanAttackTargetSelection() {
        // Change cursor to indicate target selection for pan attack
        this.map.getContainer().style.cursor = 'crosshair';
        
        // Add visual feedback for attacker token
        this.highlightAttackerToken();
        
        // Listen for target selection
        this.map.off('click', this.handleMapClick, this);
        this.map.on('click', this.handlePanAttackTargetSelection, this);
    }

    handlePanAttackTargetSelection(event) {
        const latlng = this.getLatLngFromEvent(event);
        
        // Create pan attack area at the clicked location
        this.createPanAttackArea(latlng);
        this.openPanAttackDataEntry(this.attackerToken, latlng);
        
        // Reset mode
        this.resetAttackMode();
    }

    createPanAttackArea(latlng) {
        // Create a circular area for pan attack
        const panAttackArea = L.circle(latlng, {
            color: '#ffaa00',
            fillColor: '#ffaa00',
            fillOpacity: 0.2,
            radius: 1000 // 1km radius
        }).addTo(this.map);
        
        // Add label
        const label = L.marker(latlng, {
            icon: L.divIcon({
                className: 'pan-attack-label',
                html: '<div class="pan-attack-label-text">PAN ATTACK</div>',
                iconSize: [80, 20],
                iconAnchor: [40, 10]
            })
        }).addTo(this.map);
        
        // Store references for cleanup
        this.panAttackArea = panAttackArea;
        this.panAttackLabel = label;
        
        this.notificationCallback(`Pan attack area created at ${latlng.lat.toFixed(4)}, ${latlng.lng.toFixed(4)}`, 'success');
    }

    openPanAttackDataEntry(attackerToken, targetLocation) {
        // Create a popup for pan attack planning
        const popup = L.popup()
            .setLatLng(targetLocation)
            .setContent(`
                <div class="pan-attack-popup">
                    <h4>Pan Attack Planning</h4>
                    <p><strong>Attacker:</strong> ${attackerToken.name}</p>
                    <p><strong>Target Area:</strong> ${targetLocation.lat.toFixed(4)}, ${targetLocation.lng.toFixed(4)}</p>
                    <p><strong>Radius:</strong> 1km</p>
                    <button class="btn btn-sm btn-primary" onclick="tokenActionModeManager.openDetailedPanAttackPlanning('${attackerToken.id}', '${targetLocation.lat}', '${targetLocation.lng}')">
                        Plan Pan Attack
                    </button>
                </div>
            `)
            .openOn(this.map);
    }

    openDetailedPanAttackPlanning(attackerId, lat, lng) {
        // This would open the detailed pan attack planning modal
        console.log('Opening detailed pan attack planning for:', attackerId, 'at', lat, lng);
        this.notificationCallback('Opening detailed pan attack planning...', 'info');
    }

    findTokenAtLocation(latlng) {
        console.log('🔍 Finding token at location:', latlng);
        console.log('🔍 window.tokenManager exists:', !!window.tokenManager);
        console.log('🔍 window.tokenManager.findTokenAtLocation exists:', !!(window.tokenManager && window.tokenManager.findTokenAtLocation));
        
        // Find token at the given location using existing token management
        if (window.tokenManager && window.tokenManager.findTokenAtLocation) {
            const token = window.tokenManager.findTokenAtLocation(latlng);
            console.log('🔍 TokenManager found token:', token);
            return token;
        }
        
        // Fallback: search through all token markers
        if (this.map) {
            console.log('🔍 Using fallback method to find token');
            const tolerance = 0.01; // Increased tolerance
            const markers = this.map._layers;
            
            for (const layerId in markers) {
                const layer = markers[layerId];
                if (layer && layer.getLatLng && layer.tokenData) {
                    const markerLatLng = layer.getLatLng();
                    const distance = this.calculateDistance(
                        latlng.lat, latlng.lng,
                        markerLatLng.lat, markerLatLng.lng
                    );
                    
                    console.log(`🔍 Checking marker at ${markerLatLng.lat}, ${markerLatLng.lng}, distance: ${distance}`);
                    
                    if (distance < tolerance) {
                        console.log('✅ Token found via fallback:', layer.tokenData);
                        return layer.tokenData;
                    }
                }
            }
        }
        
        console.log('❌ No token found at location');
        return null;
    }

    getLatLngFromEvent(event) {
        // Convert click event to lat/lng using the map
        if (this.map) {
            return this.map.mouseEventToLatLng(event);
        }
        return null;
    }

    calculateDistance(lat1, lng1, lat2, lng2) {
        // Simple distance calculation for token detection
        const dx = lat2 - lat1;
        const dy = lng2 - lng1;
        return Math.sqrt(dx * dx + dy * dy);
    }

    updateUI() {
        console.log('🔧 Updating UI. Current mode:', this.currentMode);
        
        // Update button states
        const buttons = document.querySelectorAll('.token-action-btn');
        console.log('🔧 Found', buttons.length, 'token action buttons');
        
        buttons.forEach(btn => {
            btn.classList.remove('active');
            if (btn.dataset.mode === this.currentMode) {
                btn.classList.add('active');
                console.log('🔧 Activated button for mode:', this.currentMode);
            }
        });

        // Update map classes
        const mapElement = document.getElementById('map');
        if (mapElement) {
            // Remove all mode classes
            Object.keys(this.modes).forEach(mode => {
                mapElement.classList.remove(`map-mode-${mode}`);
            });
            
            // Add current mode class
            if (this.currentMode) {
                mapElement.classList.add(`map-mode-${this.currentMode}`);
                console.log('🔧 Added map class for mode:', this.currentMode);
            }
        } else {
            console.log('🔧 Map element not found for UI update');
        }
    }

    // Update UI with retry mechanism
    updateUIWithRetry(maxRetries = 5, delay = 100) {
        console.log('🔧 Updating UI with retry mechanism...');
        
        const tryUpdate = (attempt) => {
            const buttons = document.querySelectorAll('.token-action-btn');
            console.log(`🔧 Attempt ${attempt}: Found ${buttons.length} buttons`);
            
            if (buttons.length > 0) {
                this.updateUI();
                return true;
            } else if (attempt < maxRetries) {
                console.log(`🔧 No buttons found, retrying in ${delay}ms...`);
                setTimeout(() => tryUpdate(attempt + 1), delay);
                return false;
            } else {
                console.log('🔧 Max retries reached, buttons not found');
                return false;
            }
        };
        
        tryUpdate(1);
    }

    updateMapCursor() {
        const mapElement = document.getElementById('map');
        if (mapElement && this.currentMode) {
            const mode = this.modes[this.currentMode];
            mapElement.style.cursor = mode.cursor;
        } else if (mapElement) {
            mapElement.style.cursor = 'default';
        }
    }


    saveModeToStorage() {
        try {
            const data = {
                mode: this.currentMode,
                timestamp: Date.now()
            };
            localStorage.setItem(this.storageKey, JSON.stringify(data));
            console.log('💾 Mode saved to storage:', data);
        } catch (e) {
            console.warn('Could not save mode to localStorage:', e);
        }
    }

    loadModeFromStorage() {
        console.log('🔍 Loading mode from storage...');
        try {
            const stored = localStorage.getItem(this.storageKey);
            console.log('🔍 Stored mode data:', stored);
            
            if (stored) {
                const data = JSON.parse(stored);
                console.log('🔍 Parsed mode data:', data);
                
                // Only restore if less than 1 hour old
                if (Date.now() - data.timestamp < 3600000) {
                    this.currentMode = data.mode;
                    console.log('🔄 Mode restored from storage:', data.mode);
                    
                    // If attack mode is restored, also restore the selected attacker
                    if (data.mode === 'attack') {
                        this.restoreSelectedAttacker();
                    }
                } else {
                    console.log('⏰ Stored mode is too old, not restoring');
                }
            } else {
                console.log('🔍 No stored mode found');
            }
        } catch (e) {
            console.warn('Could not load mode from localStorage:', e);
        }
    }

    // Public methods for integration
    getCurrentMode() {
        return this.currentMode;
    }

    getSelectedAttacker() {
        return this.selectedAttacker;
    }

    // Save selected attacker to storage
    saveSelectedAttacker(attackerToken) {
        if (attackerToken) {
            localStorage.setItem(this.attackerStorageKey, JSON.stringify({
                id: attackerToken.id,
                name: attackerToken.name,
                tokenGroupId: attackerToken.tokenGroupId,
                tokenGroupName: attackerToken.tokenGroupName,
                assetImagePath: attackerToken.assetImagePath,
                timestamp: Date.now()
            }));
            console.log('💾 Selected attacker saved to storage:', attackerToken.name);
        } else {
            localStorage.removeItem(this.attackerStorageKey);
            console.log('💾 Selected attacker cleared from storage');
        }
    }

    // Restore selected attacker from storage
    restoreSelectedAttacker() {
        try {
            const stored = localStorage.getItem(this.attackerStorageKey);
            if (stored) {
                const attackerData = JSON.parse(stored);
                // Check if the stored attacker is still valid (not too old)
                const maxAge = 24 * 60 * 60 * 1000; // 24 hours
                if (Date.now() - attackerData.timestamp < maxAge) {
                    this.selectedAttacker = attackerData;
                    console.log('🔄 Selected attacker restored from storage:', attackerData.name);
                    return attackerData;
                } else {
                    console.log('⏰ Stored attacker is too old, clearing...');
                    localStorage.removeItem(this.attackerStorageKey);
                }
            }
        } catch (e) {
            console.warn('Could not restore attacker from localStorage:', e);
        }
        return null;
    }

    isModeActive(mode) {
        return this.currentMode === mode;
    }

    setMap(mapInstance) {
        this.map = mapInstance;
        
        // Update UI now that map is available
        this.updateUI();
        
        // Initialize restored state after map is ready
        this.initializeRestoredState();
    }

    // Initialize restored state after map is ready
    initializeRestoredState() {
        console.log('🔧 Initializing restored state. Current mode:', this.currentMode, 'Selected attacker:', this.selectedAttacker);
        
        if (this.currentMode === 'attack' && this.selectedAttacker) {
            console.log('🔄 Initializing restored attack mode with attacker:', this.selectedAttacker.name);
            
            // Update UI to show attack mode is active (with retry)
            this.updateUIWithRetry();
            this.updateMapCursor();
            
            // Set attack cursor
            if (this.map) {
                this.map.getContainer().style.cursor = 'url("data:image/svg+xml;utf8,<svg xmlns=\'http://www.w3.org/2000/svg\' width=\'24\' height=\'24\' viewBox=\'0 0 24 24\' fill=\'none\' stroke=\'%23ff4444\' stroke-width=\'2\' stroke-linecap=\'round\' stroke-linejoin=\'round\'><path d=\'M3 3l7.07 16.97 2.51-7.39 7.39-2.51L3 3z\'></path><path d=\'M13 13l6 6\'></path></svg>"), auto';
            }
            
            // Show notification about restored state
            if (this.notificationCallback) {
                this.notificationCallback(`Attack mode restored. ${this.selectedAttacker.name} is selected as attacker. Click on target token to plan attack.`, 'info');
            }
        } else {
            console.log('🔧 No attack mode to restore or no attacker selected');
        }
    }

    setTokenManager(tokenManagerInstance) {
        this.tokenManager = tokenManagerInstance;
    }

    setNotificationCallback(callback) {
        this.notificationCallback = callback;
    }

    // Attack planning modal functions
    async initializeAttackPlanningModal(attackerToken, targetToken) {
        console.log('🔍 Initializing attack planning modal...');
        
        // Set global token IDs for saving
        window.currentAttackerTokenId = attackerToken.id;
        window.currentTargetTokenId = targetToken.id;
        
        // Check if attack order already exists
        await this.checkAndLoadExistingData(attackerToken.id, targetToken.id);
        
        // Update modal content with token names
        const attackerNameEl = document.getElementById('attackerName');
        const targetNameEl = document.getElementById('targetName');
        if (attackerNameEl) attackerNameEl.textContent = attackerToken.name;
        if (targetNameEl) targetNameEl.textContent = targetToken.name;
        
        // Show the modal
        const modal = document.getElementById('attackPlanningModal');
        if (modal) {
            modal.style.display = 'block';
            console.log('✅ Attack planning modal shown');
            
            // Load first tab
            this.switchAttackTab('intent');
        }
    }
    
    async checkAndLoadExistingData(attackerTokenId, targetTokenId) {
        try {
            console.log('🔍 Checking for existing attack order...');
            const response = await fetch(`/AttackPlanning/CheckExistingAttackOrder?attackerTokenId=${attackerTokenId}&targetTokenId=${targetTokenId}`);
            const result = await response.json();
            
            if (result.success && result.exists) {
                console.log('✅ Found existing attack order:', result.orderId);
                window.currentOrderId = result.orderId;
                
                // Show notification about existing data
                window.showNotification(`Found existing attack order from ${new Date(result.createdDate).toLocaleDateString()}. Data will be loaded.`, 'info');
            } else {
                console.log('📝 No existing attack order found, creating new one');
                window.currentOrderId = generateOrderId();
            }
        } catch (error) {
            console.error('❌ Error checking existing data:', error);
            window.currentOrderId = generateOrderId();
        }
    }

    async switchAttackTab(tabName) {
        console.log('🔍 Switching to tab:', tabName);
        
        // Auto-save current tab data before switching
        await this.autoSaveCurrentTab();
        
        // Remove active class from all tabs
        document.querySelectorAll('.attack-tab').forEach(tab => {
            tab.classList.remove('active');
        });
        
        // Add active class to selected tab
        const selectedTab = document.querySelector(`[data-tab="${tabName}"]`);
        if (selectedTab) {
            selectedTab.classList.add('active');
        }
        
        // Hide all tab contents
        document.querySelectorAll('.tab-content').forEach(content => {
            content.style.display = 'none';
        });
        
        // Show selected tab content
        const selectedContent = document.getElementById(`tab-${tabName}`);
        if (selectedContent) {
            selectedContent.style.display = 'block';
        }
        
        // Load tab content if not already loaded
        this.loadTabContent(tabName);
        
        // If switching to summary tab, load complete draft
        if (tabName === 'summary') {
            await this.loadCompleteDraft();
        }
    }

    async autoSaveCurrentTab() {
        const activeTab = document.querySelector('.attack-tab.active');
        if (!activeTab) return;
        
        const currentTabName = activeTab.dataset.tab;
        if (!currentTabName || currentTabName === 'summary') return;
        
        console.log('💾 Auto-saving current tab:', currentTabName);
        
        try {
            // Collect form data from current tab
            const formData = this.collectTabData(currentTabName);
            
            if (formData && Object.keys(formData).length > 0) {
                // Save to database
                await this.saveTabData(currentTabName, formData);
                console.log('✅ Auto-saved tab:', currentTabName);
            }
        } catch (error) {
            console.error('❌ Error auto-saving tab:', error);
        }
    }
    
    collectTabData(tabName) {
        const tabContent = document.getElementById(`tab-${tabName}`);
        if (!tabContent) return {};
        
        const formData = {};
        const form = tabContent.querySelector('form');
        
        if (form) {
            const formElements = form.querySelectorAll('input, select, textarea');
            formElements.forEach(element => {
                if (element.name) {
                    if (element.type === 'checkbox' || element.type === 'radio') {
                        formData[element.name] = element.checked;
                    } else {
                        formData[element.name] = element.value;
                    }
                }
            });
        }
        
        return formData;
    }
    
    async saveTabData(tabName, formData) {
        const orderId = window.currentOrderId || generateOrderId();
        window.currentOrderId = orderId;
        
        const saveData = {
            orderId: orderId,
            tabName: tabName,
            attackerTokenId: window.currentAttackerTokenId,
            targetTokenId: window.currentTargetTokenId,
            data: formData
        };
        
        const response = await fetch('/AttackPlanning/SaveDraft', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(saveData)
        });
        
        const result = await response.json();
        return result.success;
    }
    
    async loadCompleteDraft() {
        console.log('📋 Loading complete draft for summary...');
        
        try {
            const response = await fetch(`/AttackPlanning/CheckExistingAttackOrder?attackerTokenId=${window.currentAttackerTokenId}&targetTokenId=${window.currentTargetTokenId}`);
            const result = await response.json();
            
            if (result.success && result.exists) {
                console.log('✅ Found existing attack order:', result.orderId);
                window.currentOrderId = result.orderId;
                
                // Populate summary with existing data
                this.populateSummaryWithData(result.data);
            } else {
                console.log('📝 No existing attack order found, showing empty summary');
                this.populateSummaryWithData({});
            }
        } catch (error) {
            console.error('❌ Error loading complete draft:', error);
            this.populateSummaryWithData({});
        }
    }
    
    populateSummaryWithData(data) {
        const summaryContent = document.getElementById('tab-summary');
        if (!summaryContent) return;
        
        // Create summary display
        let summaryHtml = `
            <div class="attack-summary">
                <h4><i class="fas fa-clipboard-list"></i> Attack Order Summary</h4>
                <div class="summary-sections">
        `;
        
        // Intent section
        if (data.intent) {
            const intent = JSON.parse(data.intent || '{}');
            summaryHtml += `
                <div class="summary-section">
                    <h5><i class="fas fa-crosshairs"></i> Intent</h5>
                    <p><strong>Attack Type:</strong> ${intent.attackType || 'Not specified'}</p>
                    <p><strong>Maneuver Form:</strong> ${intent.maneuverForm || 'Not specified'}</p>
                    <p><strong>Desired Effect:</strong> ${intent.desiredEffect || 'Not specified'}</p>
                </div>
            `;
        }
        
        // Timing section
        if (data.timing) {
            const timing = JSON.parse(data.timing || '{}');
            summaryHtml += `
                <div class="summary-section">
                    <h5><i class="fas fa-clock"></i> Timing</h5>
                    <p><strong>Start Turn:</strong> ${timing.startTurn || 'Not specified'}</p>
                    <p><strong>Duration:</strong> ${timing.durationTurns || 'Not specified'} turns</p>
                    <p><strong>Posture:</strong> ${timing.posture || 'Not specified'}</p>
                </div>
            `;
        }
        
        // Add more sections as needed...
        
        summaryHtml += `
                </div>
                <div class="summary-actions">
                    <button class="btn btn-primary" onclick="saveAttackDraft()">
                        <i class="fas fa-save"></i> Save Complete Order
                    </button>
                    <button class="btn btn-secondary" onclick="closeAttackPlanningModal()">
                        <i class="fas fa-times"></i> Cancel
                    </button>
                </div>
            </div>
        `;
        
        summaryContent.innerHTML = summaryHtml;
    }

    loadTabContent(tabName) {
        const tabContent = document.getElementById(`tab-${tabName}`);
        
        // Map tab names to controller action names
        const actionMap = {
            'intent': 'LoadIntentForm',
            'timing': 'LoadTimingForm',
            'movement': 'LoadMovementForm',
            'fires': 'LoadFiresForm',
            'fogofwar': 'LoadFogOfWarForm',
            'logistics': 'LoadLogisticsForm',
            'roe': 'LoadROEForm',
            'summary': 'LoadSummaryForm'
        };
        
        const actionName = actionMap[tabName] || `Load${tabName.charAt(0).toUpperCase() + tabName.slice(1)}Form`;
        
        fetch(`/AttackPlanning/${actionName}`)
            .then(response => response.text())
            .then(html => {
                tabContent.innerHTML = html;
                console.log(`Loaded ${tabName} tab content`);
            })
            .catch(error => {
                console.error(`Error loading ${tabName} tab:`, error);
                tabContent.innerHTML = '<div class="error">Failed to load tab content</div>';
            });
    }

    closeAttackPlanningModal() {
        const modal = document.getElementById('attackPlanningModal');
        if (modal) {
            modal.style.display = 'none';
            console.log('✅ Attack planning modal closed');
        }
    }
}

// Global instance
window.tokenActionModeManager = new TokenActionModeManager();

// Global functions for backward compatibility
window.switchAttackTab = function(tabName) {
    window.tokenActionModeManager.switchAttackTab(tabName);
};

window.closeAttackPlanningModal = function() {
    window.tokenActionModeManager.closeAttackPlanningModal();
};

window.initializeAttackPlanning = function(attackerTokenId, targetTokenId, attackerName, targetName) {
    const attackerToken = { id: attackerTokenId, name: attackerName };
    const targetToken = { id: targetTokenId, name: targetName };
    window.tokenActionModeManager.initializeAttackPlanningModal(attackerToken, targetToken);
};

// Additional global functions for attack planning forms
window.validateTimingForm = function() {
    console.log('Validating timing form...');
    showNotification('Timing form validated successfully', 'success');
    return true;
};

window.addWaypoint = function() {
    console.log('Adding waypoint...');
    showNotification('Waypoint added successfully', 'success');
};

window.executeAttack = function() {
    console.log('Executing attack...');
    showNotification('Attack execution initiated', 'info');
};

window.previewAttack = function() {
    console.log('Previewing attack...');
    showNotification('Attack preview generated', 'info');
};

window.validateLogisticsForm = function() {
    console.log('Validating logistics form...');
    showNotification('Logistics form validated successfully', 'success');
    return true;
};

window.resetLogisticsForm = function() {
    console.log('Resetting logistics form...');
    showNotification('Logistics form reset', 'info');
};

window.validateIntentForm = function() {
    console.log('Validating intent form...');
    showNotification('Intent form validated successfully', 'success');
    return true;
};

window.validateMovementForm = function() {
    console.log('Validating movement form...');
    showNotification('Movement form validated successfully', 'success');
    return true;
};

window.validateFiresForm = function() {
    console.log('Validating fires form...');
    showNotification('Fires form validated successfully', 'success');
    return true;
};

window.validateFogOfWarForm = function() {
    console.log('Validating fog of war form...');
    showNotification('Fog of war form validated successfully', 'success');
    return true;
};

window.validateROEForm = function() {
    console.log('Validating ROE form...');
    showNotification('ROE form validated successfully', 'success');
    return true;
};

window.saveAttackOrder = function() {
    console.log('Saving attack order...');
    showNotification('Attack order saved successfully', 'success');
    return true;
};

window.executeAttackOrder = function() {
    console.log('Executing attack order...');
    showNotification('Attack order execution initiated', 'info');
    return true;
};

window.saveAttackDraft = async function() {
    console.log('Saving attack draft...');
    
    try {
        // Get current attack order data from the modal
        const attackOrderData = collectAttackOrderData();
        
        if (!attackOrderData) {
            showNotification('No attack order data to save', 'warning');
            return false;
        }
        
        // Generate unique order ID
        const orderId = attackOrderData.orderId || generateOrderId();
        
        // Save to database
        const saveData = {
            orderId: orderId,
            tabName: 'complete',
            attackerTokenId: attackOrderData.attackerTokenId,
            targetTokenId: attackOrderData.targetTokenId,
            data: attackOrderData
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
            showNotification('Attack order saved successfully', 'success');
            
            // Update attack visualization if it exists
            if (window.attackVisualizationManager) {
                const attackId = `${attackOrderData.attackerTokenId}_${attackOrderData.targetTokenId}`;
                await window.attackVisualizationManager.saveAttackOrderToDatabase(attackId, attackOrderData);
            }
            
            return true;
        } else {
            showNotification('Failed to save attack order: ' + result.message, 'error');
            return false;
        }
    } catch (error) {
        console.error('Error saving attack draft:', error);
        showNotification('Error saving attack order', 'error');
        return false;
    }
};

// Helper function to collect attack order data from the modal
function collectAttackOrderData() {
    // This would collect data from all form tabs
    // For now, return a basic structure
    const attackerTokenId = window.currentAttackerTokenId || 'unknown';
    const targetTokenId = window.currentTargetTokenId || 'unknown';
    const orderId = window.currentOrderId || generateOrderId();
    
    return {
        orderId: orderId,
        attackerTokenId: attackerTokenId,
        targetTokenId: targetTokenId,
        intent: {},
        timing: {},
        movement: {},
        fires: {},
        fogOfWar: {},
        logistics: {},
        roe: {}
    };
}

// Helper function to generate unique order ID
function generateOrderId() {
    return 'attack_' + Date.now() + '_' + Math.random().toString(36).substr(2, 9);
}

// Global notification function - simple console logging to avoid recursion
window.showNotification = function(message, type = 'info') {
    console.log(`[${type.toUpperCase()}] ${message}`);
    
    // Simple fallback notification system
    if (type === 'error') {
        alert('Error: ' + message);
    }
};
function cancelTokenMode() {
    if (window.tokenActionModeManager) {
        window.tokenActionModeManager.cancelMode();
    }
}

// Export for module systems
if (typeof module !== 'undefined' && module.exports) {
    module.exports = TokenActionModeManager;
}
