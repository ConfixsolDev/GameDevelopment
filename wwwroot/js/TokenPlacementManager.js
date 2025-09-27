/**
 * TokenPlacementManager - Handles token placement, movement, and deletion on the map
 */
class TokenPlacementManager {
    constructor(map, notificationCallback) {
        this.map = map;
        this.notificationCallback = notificationCallback || this.defaultNotification;
        this.placedTokens = new Map(); // tokenId -> { marker, coverageAreas, token }
        this.isPlacementMode = false;
        this.selectedTokenForPlacement = null;
        this.isMovingToken = false;
        this.movingToken = null;
        this.tempMarker = null;
        
        this.setupEventListeners();
    }

    /**
     * Setup event listeners for map interactions
     */
    setupEventListeners() {
        // Map click events
        this.map.on('click', (e) => {
            if (this.isPlacementMode && this.selectedTokenForPlacement) {
                this.placeTokenAtLocation(e.latlng);
            } else if (this.isMovingToken && this.movingToken) {
                this.moveTokenToLocation(e.latlng);
            }
        });

        // Map mouse move events for preview
        this.map.on('mousemove', (e) => {
            if (this.isPlacementMode && this.selectedTokenForPlacement) {
                this.updatePlacementPreview(e.latlng);
            }
        });

        // Map mouse leave events
        this.map.on('mouseout', () => {
            if (this.tempMarker) {
                this.map.removeLayer(this.tempMarker);
                this.tempMarker = null;
            }
        });
    }

    /**
     * Start token placement mode
     */
    startPlacementMode(token) {
        this.isPlacementMode = true;
        this.selectedTokenForPlacement = token;
        this.map.getContainer().style.cursor = 'crosshair';
        this.notificationCallback(`Click on the map to place "${token.name}"`, 'info');
    }

    /**
     * Cancel placement mode
     */
    cancelPlacementMode() {
        this.isPlacementMode = false;
        this.selectedTokenForPlacement = null;
        this.map.getContainer().style.cursor = 'default';
        
        if (this.tempMarker) {
            this.map.removeLayer(this.tempMarker);
            this.tempMarker = null;
        }
    }

    /**
     * Update placement preview marker
     */
    updatePlacementPreview(latlng) {
        if (!this.selectedTokenForPlacement) return;

        // Remove existing temp marker
        if (this.tempMarker) {
            this.map.removeLayer(this.tempMarker);
        }

        // Create preview marker
        const icon = this.createTokenIcon(this.selectedTokenForPlacement);
        this.tempMarker = L.marker(latlng, { icon: icon })
            .addTo(this.map);

        // Add coverage area preview if token has radius
        if (this.selectedTokenForPlacement.coverageRadius && this.selectedTokenForPlacement.coverageRadius > 0) {
            const circle = L.circle(latlng, {
                radius: this.selectedTokenForPlacement.coverageRadius * 1000, // Convert km to meters
                color: '#3388ff',
                fillColor: '#3388ff',
                fillOpacity: 0.2,
                weight: 2,
                dashArray: '5, 5'
            }).addTo(this.map);

            this.tempMarker.coveragePreview = circle;
        }
    }

    /**
     * Place token at specified location
     */
    async placeTokenAtLocation(latlng) {
        if (!this.selectedTokenForPlacement) return;

        try {
            const response = await fetch('/GamePlay/PlaceTokenOnMap', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    tokenId: this.selectedTokenForPlacement.id,
                    latitude: latlng.lat,
                    longitude: latlng.lng
                })
            });

            const result = await response.json();

            if (result.success) {
                // Create permanent marker
                const marker = this.createTokenMarker(this.selectedTokenForPlacement, latlng);
                this.map.addLayer(marker);

                // Create coverage areas
                if (result.areaCoverages && result.areaCoverages.length > 0) {
                    this.createCoverageAreas(result.areaCoverages, latlng);
                }

                // Store token info
                this.placedTokens.set(this.selectedTokenForPlacement.id, {
                    marker: marker,
                    token: this.selectedTokenForPlacement,
                    coverageAreas: result.areaCoverages || []
                });

                // Save to cache via TokenManager if available
                if (typeof tokenManager !== 'undefined' && tokenManager.savePlacedTokensToCache) {
                    tokenManager.savePlacedTokensToCache(this.selectedTokenForPlacement, latlng);
                }

                // Clean up
                this.cancelPlacementMode();
                this.notificationCallback(result.message, 'success');
            } else {
                this.notificationCallback(result.message, 'error');
            }
        } catch (error) {
            console.error('Error placing token:', error);
            this.notificationCallback('Error placing token', 'error');
        }
    }

    /**
     * Start token movement mode
     */
    startMoveMode(tokenId) {
        const tokenInfo = this.placedTokens.get(tokenId);
        if (!tokenInfo) return;

        this.isMovingToken = true;
        this.movingToken = tokenInfo;
        this.map.getContainer().style.cursor = 'move';
        this.notificationCallback(`Click on the map to move "${tokenInfo.token.name}"`, 'info');
    }

    /**
     * Cancel move mode
     */
    cancelMoveMode() {
        this.isMovingToken = false;
        this.movingToken = null;
        this.map.getContainer().style.cursor = 'default';
    }

    /**
     * Move token to new location
     */
    async moveTokenToLocation(latlng) {
        if (!this.movingToken) return;

        try {
            const response = await fetch('/GamePlay/MoveToken', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    tokenId: this.movingToken.token.id,
                    latitude: latlng.lat,
                    longitude: latlng.lng
                })
            });

            const result = await response.json();

            if (result.success) {
                // Update marker position
                this.movingToken.marker.setLatLng(latlng);

                // Update coverage areas
                if (result.areaCoverages && result.areaCoverages.length > 0) {
                    this.updateCoverageAreas(this.movingToken.token.id, result.areaCoverages, latlng);
                }

                this.notificationCallback(result.message, 'success');
            } else {
                this.notificationCallback(result.message, 'error');
            }
        } catch (error) {
            console.error('Error moving token:', error);
            this.notificationCallback('Error moving token', 'error');
        } finally {
            this.cancelMoveMode();
        }
    }

    /**
     * Remove token from map permanently
     */
    async removeTokenFromMap(tokenId) {
        const tokenInfo = this.placedTokens.get(tokenId);
        if (!tokenInfo) return;

        if (!confirm(`Are you sure you want to remove "${tokenInfo.token.name}" from the map?`)) {
            return;
        }

        try {
            const response = await fetch('/GamePlay/RemoveTokenFromMap', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    tokenId: tokenId
                })
            });

            const result = await response.json();

            if (result.success) {
                // Remove marker from map
                this.map.removeLayer(tokenInfo.marker);

                // Remove coverage areas
                this.removeCoverageAreas(tokenId);

                // Remove from tracking
                this.placedTokens.delete(tokenId);

                this.notificationCallback(result.message, 'success');
            } else {
                this.notificationCallback(result.message, 'error');
            }
        } catch (error) {
            console.error('Error removing token:', error);
            this.notificationCallback('Error removing token', 'error');
        }
    }

    /**
     * Create token marker
     */
    createTokenMarker(token, latlng) {
        const icon = this.createTokenIcon(token);
        const marker = L.marker(latlng, { icon: icon });

        // Add click event for token details
        marker.on('click', () => {
            this.showTokenDetails(token);
        });

        // Add context menu for token actions
        marker.on('contextmenu', (e) => {
            this.showTokenContextMenu(e, token);
        });

        return marker;
    }

    /**
     * Create token icon
     */
    createTokenIcon(token) {
        const iconHtml = token.assetImagePath ? 
            `<img src="${token.assetImagePath}" class="token-image" onerror="this.style.display='none'; this.nextElementSibling.style.display='inline';"><i class="fas fa-crosshairs token-fallback-icon" style="display:none;"></i>` :
            `<i class="fas fa-crosshairs"></i>`;

        return L.divIcon({
            className: 'token-marker',
            html: iconHtml,
            iconSize: [24, 24],
            iconAnchor: [12, 12]
        });
    }

    /**
     * Create coverage areas
     */
    createCoverageAreas(areaCoverages, centerLatLng) {
        areaCoverages.forEach(coverage => {
            if (coverage.shapeType === 'Circle' && coverage.radiusKm) {
                const circle = L.circle(centerLatLng, {
                    radius: coverage.radiusKm * 1000, // Convert km to meters
                    color: this.getCoverageColor(coverage.coverageType),
                    fillColor: this.getCoverageColor(coverage.coverageType),
                    fillOpacity: 0.3,
                    weight: 2
                }).addTo(this.map);

                // Store reference for later updates
                if (!this.coverageAreas) this.coverageAreas = new Map();
                this.coverageAreas.set(coverage.id, circle);
            }
        });
    }

    /**
     * Update coverage areas
     */
    updateCoverageAreas(tokenId, areaCoverages, newCenterLatLng) {
        // Remove existing coverage areas for this token
        this.removeCoverageAreas(tokenId);

        // Create new coverage areas
        this.createCoverageAreas(areaCoverages, newCenterLatLng);
    }

    /**
     * Remove coverage areas
     */
    removeCoverageAreas(tokenId) {
        if (!this.coverageAreas) return;

        // Find and remove coverage areas for this token
        for (const [coverageId, circle] of this.coverageAreas) {
            this.map.removeLayer(circle);
            this.coverageAreas.delete(coverageId);
        }
    }

    /**
     * Get coverage color based on type
     */
    getCoverageColor(coverageType) {
        const colors = {
            'Operational': '#3388ff',
            'Surveillance': '#ff6b35',
            'Defensive': '#ff4757',
            'Support': '#2ed573',
            'Patrol': '#ffa502',
            'Combat': '#ff3838',
            'Reconnaissance': '#9c88ff'
        };
        return colors[coverageType] || '#3388ff';
    }

    /**
     * Show token details
     */
    showTokenDetails(token) {
        // This should be implemented to show token details in a panel or modal
        console.log('Show token details for:', token);
        // You can integrate this with your existing token details functionality
    }

    /**
     * Show token context menu
     */
    showTokenContextMenu(e, token) {
        // Create context menu
        const contextMenu = L.popup()
            .setLatLng(e.latlng)
            .setContent(`
                <div class="token-context-menu">
                    <h6>${token.name}</h6>
                    <button class="btn btn-sm btn-primary" onclick="tokenPlacementManager.startMoveMode('${token.id}')">
                        <i class="fas fa-arrows-alt"></i> Move
                    </button>
                    <button class="btn btn-sm btn-info" onclick="tokenPlacementManager.showTokenDetails(${JSON.stringify(token).replace(/"/g, '&quot;')})">
                        <i class="fas fa-info-circle"></i> Details
                    </button>
                    <button class="btn btn-sm btn-danger" onclick="tokenPlacementManager.removeTokenFromMap('${token.id}')">
                        <i class="fas fa-trash"></i> Remove
                    </button>
                </div>
            `)
            .openOn(this.map);
    }

    /**
     * Load existing placed tokens
     */
    async loadPlacedTokens() {
        try {
            // This would typically fetch from an endpoint that returns all placed tokens
            // For now, we'll implement a basic version
            console.log('Loading placed tokens...');
        } catch (error) {
            console.error('Error loading placed tokens:', error);
        }
    }

    /**
     * Get all placed tokens
     */
    getPlacedTokens() {
        return Array.from(this.placedTokens.values()).map(info => info.token);
    }

    /**
     * Check if token is placed
     */
    isTokenPlaced(tokenId) {
        return this.placedTokens.has(tokenId);
    }

    /**
     * Default notification callback
     */
    defaultNotification(message, type) {
        console.log(`[${type.toUpperCase()}] ${message}`);
    }

    /**
     * Clean up resources
     */
    destroy() {
        this.cancelPlacementMode();
        this.cancelMoveMode();
        
        if (this.tempMarker) {
            this.map.removeLayer(this.tempMarker);
        }

        // Remove all placed tokens
        for (const [tokenId, tokenInfo] of this.placedTokens) {
            this.map.removeLayer(tokenInfo.marker);
        }
        
        this.placedTokens.clear();
        
        if (this.coverageAreas) {
            for (const [coverageId, circle] of this.coverageAreas) {
                this.map.removeLayer(circle);
            }
            this.coverageAreas.clear();
        }
    }
}

// Export for use in other modules
if (typeof module !== 'undefined' && module.exports) {
    module.exports = TokenPlacementManager;
}
