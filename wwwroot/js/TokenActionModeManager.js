/**
 * Token Action Mode Manager
 * Manages token action modes with browser storage persistence
 */
class TokenActionModeManager {
    constructor() {
        this.currentMode = null;
        this.storageKey = 'tokenActionMode';
        this.modeIndicator = null;
        this.map = null;
        this.tokenManager = null;
        
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
        this.loadModeFromStorage();
        this.setupEventListeners();
        this.updateUI();
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
        this.currentMode = null;
        this.saveModeToStorage();
        this.updateUI();
        this.updateMapCursor();
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
                // Enable token movement only
                this.enableTokenMovement();
                break;
            case 'attack':
                // Enable attack mode only
                this.enableAttackMode();
                break;
            case 'pan-attack':
                // Enable pan attack mode only
                this.enablePanAttackMode();
                break;
            case 'select':
                // Enable selection mode only
                this.enableSelectionMode();
                break;
        }
    }

    disableAllTokenInteractions() {
        // Disable token dragging/movement
        if (window.tokenManager && window.tokenManager.disableTokenMovement) {
            window.tokenManager.disableTokenMovement();
        }
        
        // Disable token placement
        if (window.tokenPlacementManager && window.tokenPlacementManager.cancelPlacementMode) {
            window.tokenPlacementManager.cancelPlacementMode();
        }
        
        // Disable attack mode
        if (window.tokenPlacementManager && window.tokenPlacementManager.exitAttackMode) {
            window.tokenPlacementManager.exitAttackMode();
        }
        
        // Remove any active attack panels
        this.closeAttackPanel();
    }

    enableTokenPlacement() {
        if (window.tokenPlacementManager && window.tokenPlacementManager.startPlacementMode) {
            window.tokenPlacementManager.startPlacementMode();
        }
    }

    enableTokenMovement() {
        // Enable token dragging/movement
        if (window.tokenManager && window.tokenManager.enableTokenMovement) {
            window.tokenManager.enableTokenMovement();
        }
        
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
        
        // Attack mode will be activated when user clicks on a token
        console.log('Attack mode activated - click on a token to start attack planning');
    }

    enablePanAttackMode() {
        // Disable token movement for pan attack mode
        if (window.tokenManager && window.tokenManager.disableTokenMovement) {
            window.tokenManager.disableTokenMovement();
        }
        
        // Pan attack mode will be activated when user clicks on a token
        console.log('Pan attack mode activated - click on a token to start pan attack');
    }

    enableSelectionMode() {
        // Enable basic token selection without movement
        if (window.tokenManager && window.tokenManager.enableTokenSelection) {
            window.tokenManager.enableTokenSelection();
        }
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
            // Start attack mode with this token as attacker
            if (window.tokenPlacementManager && window.tokenPlacementManager.startAttackMode) {
                window.tokenPlacementManager.startAttackMode(token.id);
            }
        } else {
            console.log('No token found at click location for attack');
        }
    }

    handlePanAttack(latlng) {
        // Find token at location
        const token = this.findTokenAtLocation(latlng);
        if (token) {
            // Start pan attack mode with this token
            console.log('Starting pan attack with token:', token);
            // This would integrate with existing pan attack functionality
            // For now, we'll trigger the pan attack UI
            if (typeof openPanAttackPlanning === 'function') {
                openPanAttackPlanning(token);
            }
        } else {
            console.log('No token found at click location for pan attack');
        }
    }

    handleTokenSelection(latlng) {
        // Find and select token at location
        const token = this.findTokenAtLocation(latlng);
        if (token) {
            // Show token info or selection
            console.log('Selected token:', token);
        }
    }

    findTokenAtLocation(latlng) {
        // Find token at the given location using existing token management
        if (window.tokenManager && window.tokenManager.findTokenAtLocation) {
            return window.tokenManager.findTokenAtLocation(latlng);
        }
        
        // Fallback: search through all token markers
        if (this.map) {
            const tolerance = 0.001; // Small tolerance for click detection
            const markers = this.map._layers;
            
            for (const layerId in markers) {
                const layer = markers[layerId];
                if (layer && layer.getLatLng && layer.tokenData) {
                    const markerLatLng = layer.getLatLng();
                    const distance = this.calculateDistance(
                        latlng.lat, latlng.lng,
                        markerLatLng.lat, markerLatLng.lng
                    );
                    
                    if (distance < tolerance) {
                        return layer.tokenData;
                    }
                }
            }
        }
        
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
        // Update button states
        document.querySelectorAll('.token-action-btn').forEach(btn => {
            btn.classList.remove('active');
            if (btn.dataset.mode === this.currentMode) {
                btn.classList.add('active');
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
            }
        }
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
            localStorage.setItem(this.storageKey, JSON.stringify({
                mode: this.currentMode,
                timestamp: Date.now()
            }));
        } catch (e) {
            console.warn('Could not save mode to localStorage:', e);
        }
    }

    loadModeFromStorage() {
        try {
            const stored = localStorage.getItem(this.storageKey);
            if (stored) {
                const data = JSON.parse(stored);
                // Only restore if less than 1 hour old
                if (Date.now() - data.timestamp < 3600000) {
                    this.currentMode = data.mode;
                }
            }
        } catch (e) {
            console.warn('Could not load mode from localStorage:', e);
        }
    }

    // Public methods for integration
    getCurrentMode() {
        return this.currentMode;
    }

    isModeActive(mode) {
        return this.currentMode === mode;
    }

    setMap(mapInstance) {
        this.map = mapInstance;
    }

    setTokenManager(tokenManagerInstance) {
        this.tokenManager = tokenManagerInstance;
    }
}

// Global instance
window.tokenActionModeManager = new TokenActionModeManager();

// Global functions for backward compatibility
function cancelTokenMode() {
    if (window.tokenActionModeManager) {
        window.tokenActionModeManager.cancelMode();
    }
}

// Export for module systems
if (typeof module !== 'undefined' && module.exports) {
    module.exports = TokenActionModeManager;
}
