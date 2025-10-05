/**
 * Suspected Token Manager - Handles fog of war intelligence system
 * Allows teams to place and manage suspected enemy contacts
 */
class SuspectedTokenManager {
    constructor(map, notificationCallback) {
        this.map = map;
        this.notificationCallback = notificationCallback || this.defaultNotification;
        this.suspectedTokens = new Map(); // tokenId -> { marker, data }
        this.isPlacementMode = false;
        this.placementData = null;
        this.tempMarker = null;
    }

    /**
     * Load suspected tokens for current team
     */
    async loadSuspectedTokens() {
        try {
            const response = await fetch('/SuspectedToken/GetSuspectedTokens');
            const result = await response.json();

            if (result.success && result.suspectedTokens) {
                console.log(`📍 Loaded ${result.suspectedTokens.length} suspected tokens`);
                
                // Clear existing markers
                this.suspectedTokens.forEach((data, tokenId) => {
                    if (data.marker) {
                        this.map.removeLayer(data.marker);
                    }
                });
                this.suspectedTokens.clear();

                // Add markers for each suspected token
                result.suspectedTokens.forEach(token => {
                    this.createSuspectedTokenMarker(token);
                });

                return result.suspectedTokens;
            }
        } catch (error) {
            console.error('Error loading suspected tokens:', error);
            this.notificationCallback('Error loading suspected contacts', 'error');
        }
        return [];
    }

    /**
     * Create marker for suspected token
     */
    createSuspectedTokenMarker(tokenData) {
        const lat = typeof tokenData.position.lat === 'string' ? parseFloat(tokenData.position.lat) : tokenData.position.lat;
        const lng = typeof tokenData.position.lng === 'string' ? parseFloat(tokenData.position.lng) : tokenData.position.lng;

        // Determine styling based on confidence level
        let markerColor = '#ffa500'; // Default orange
        let fillOpacity = 0.3;
        if (tokenData.confidence >= 70) {
            markerColor = '#ff4444'; // High confidence: red
            fillOpacity = 0.5;
        } else if (tokenData.confidence <= 30) {
            markerColor = '#ffff00'; // Low confidence: yellow
            fillOpacity = 0.2;
        }

        // Create icon with question mark
        const iconHtml = `
            <div class="suspected-token-marker" style="border-color: ${markerColor};">
                <i class="fas fa-question"></i>
                <div class="confidence-indicator" style="background-color: ${markerColor}; opacity: ${fillOpacity};">
                    ${Math.round(tokenData.confidence)}%
                </div>
            </div>
        `;

        const icon = L.divIcon({
            className: 'suspected-token-container',
            html: iconHtml,
            iconSize: [45, 45],
            iconAnchor: [22.5, 22.5],
            popupAnchor: [0, -22.5]
        });

        const marker = L.marker([lat, lng], { 
            icon: icon,
            draggable: true 
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
                    element.setAttribute('data-token-type', 'Suspected');
                    element.setAttribute('data-token-guid', tokenData.id);
                    element.classList.add('suspected-token-marker');
                    
                    // Add title attribute to show token GUID on hover
                    element.setAttribute('title', `Suspected Token: ${tokenData.name} (ID: ${tokenData.id})`);
                    
                    console.log(`✅ Suspected token marker DOM attributes set for ${tokenData.name}: data-id="${tokenData.id}"`);
                }
            }
        });

        // Add popup
        marker.bindPopup(this.createPopupContent(tokenData));

        // Handle drag event
        marker.on('dragend', async (e) => {
            const newPos = e.target.getLatLng();
            await this.updateSuspectedTokenPosition(tokenData.id, newPos.lat, newPos.lng);
        });

        // Click event - Generic token click handler
        marker.on('click', () => {
            console.log('Suspected token clicked:', tokenData.name);
            
            // Use the generic token click handler from TokenActionModeManager
            if (window.tokenActionModeManager) {
                const currentMode = window.tokenActionModeManager.getCurrentMode();
                console.log('🔍 Current mode:', currentMode);
                
                if (currentMode === 'attack') {
                    // In attack mode - check if we have an attacker selected
                    const attackerToken = window.tokenActionModeManager.getSelectedAttacker();
                    if (attackerToken) {
                        console.log('🎯 Suspected token selected as target for attack planning');
                        // Trigger attack planning with suspected token as target
                        window.tokenActionModeManager.openAttackDataEntry(attackerToken, tokenData);
                    } else {
                        console.log('❌ No attacker selected - select attacker first');
                        if (window.tokenActionModeManager.notificationCallback) {
                            window.tokenActionModeManager.notificationCallback('Please select an attacker token first', 'warning');
                        }
                    }
                } else {
                    // In other modes - show token info (default behavior)
                    console.log('🔍 Showing suspected token info');
                    this.showSuspectedTokenInfo(tokenData);
                }
            } else {
                console.log('❌ TokenActionModeManager not available');
            }
        });

        marker.addTo(this.map);

        // Store reference
        this.suspectedTokens.set(tokenData.id, {
            marker: marker,
            data: tokenData
        });

        console.log(`✅ Suspected token marker created: ${tokenData.name} (${tokenData.confidence}% confidence)`);
    }

    /**
     * Create popup content for suspected token
     */
    createPopupContent(tokenData) {
        // Determine confidence color
        let confidenceColor = '#ffa500'; // Medium
        if (tokenData.confidence >= 70) {
            confidenceColor = '#ff4444'; // High - Red
        } else if (tokenData.confidence <= 30) {
            confidenceColor = '#ffff00'; // Low - Yellow
        }

        return `
            <div class="suspected-token-popup">
                <div class="popup-header">
                    <h4><i class="fas fa-question-circle"></i> ${tokenData.name}</h4>
                    <span class="enemy-badge">ENEMY TOKEN</span>
                </div>
                
                <div class="popup-section">
                    <div class="popup-row">
                        <span class="popup-label">Intelligence Source:</span>
                        <span class="popup-value">${this.formatSource(tokenData.source)}</span>
                    </div>
                    <div class="popup-row">
                        <span class="popup-label">Confidence Level:</span>
                        <span class="popup-value">
                            <span class="confidence-badge" style="background-color: ${confidenceColor};">
                                ${Math.round(tokenData.confidence)}%
                            </span>
                        </span>
                    </div>
                    <div class="popup-row">
                        <span class="popup-label">Status:</span>
                        <span class="popup-value status-${tokenData.status}">${this.formatStatus(tokenData.status)}</span>
                    </div>
                    ${tokenData.suspectedType ? `
                    <div class="popup-row">
                        <span class="popup-label">Unit Type:</span>
                        <span class="popup-value">${this.formatUnitType(tokenData.suspectedType)}</span>
                    </div>` : ''}
                    <div class="popup-row">
                        <span class="popup-label">First Detected:</span>
                        <span class="popup-value">${tokenData.firstDetectedAt ? new Date(tokenData.firstDetectedAt).toLocaleString() : 'Unknown'}</span>
                    </div>
                    ${tokenData.isrMissionsCount > 0 ? `
                    <div class="popup-row">
                        <span class="popup-label">ISR Missions:</span>
                        <span class="popup-value"><span class="badge-count">${tokenData.isrMissionsCount}</span></span>
                    </div>` : ''}
                    ${tokenData.notes ? `
                    <div class="popup-row popup-notes">
                        <span class="popup-label">Intel Notes:</span>
                        <span class="popup-value">${tokenData.notes}</span>
                    </div>` : ''}
                </div>
                
                <div class="popup-actions">
                    <button class="btn btn-sm btn-danger" onclick="suspectedTokenManager.removeSuspectedToken('${tokenData.id}'); event.stopPropagation();">
                        <i class="fas fa-trash-alt"></i> Remove
                    </button>
                </div>
            </div>
        `;
    }

    /**
     * Format intelligence source for display
     */
    formatSource(source) {
        const sourceMap = {
            'human': 'Human Intel (HUMINT)',
            'radar': 'Radar Detection',
            'uav': 'UAV Observation',
            'satellite': 'Satellite Imagery',
            'signals': 'Signals Intel (SIGINT)',
            'patrol': 'Patrol Report',
            'other': 'Other Source'
        };
        return sourceMap[source] || source || 'Unknown';
    }

    /**
     * Format status for display
     */
    formatStatus(status) {
        const statusMap = {
            'suspected': 'Suspected',
            'confirmed': 'Confirmed',
            'dismissed': 'Dismissed'
        };
        return statusMap[status] || status;
    }

    /**
     * Format unit type for display
     */
    formatUnitType(type) {
        const typeMap = {
            'infantry': 'Infantry Unit',
            'armor': 'Armor/Tank Unit',
            'artillery': 'Artillery Unit',
            'mechanized': 'Mechanized Infantry',
            'logistics': 'Logistics/Supply',
            'command': 'Command Post',
            'air_defense': 'Air Defense',
            'other': 'Other'
        };
        return typeMap[type] || type || 'Unknown';
    }

    /**
     * Start placement mode for suspected token
     */
    startPlacementMode(data) {
        this.isPlacementMode = true;
        this.placementData = data;
        this.map.getContainer().style.cursor = 'crosshair';
        
        // Add click handler to map
        this.mapClickHandler = (e) => {
            this.placeSuspectedToken(e.latlng);
        };
        this.map.on('click', this.mapClickHandler);

        this.notificationCallback('Click on the map to place suspected contact', 'info');
    }

    /**
     * Cancel placement mode
     */
    cancelPlacementMode() {
        this.isPlacementMode = false;
        this.placementData = null;
        this.map.getContainer().style.cursor = 'default';
        
        if (this.mapClickHandler) {
            this.map.off('click', this.mapClickHandler);
            this.mapClickHandler = null;
        }

        if (this.tempMarker) {
            this.map.removeLayer(this.tempMarker);
            this.tempMarker = null;
        }
    }

    /**
     * Place suspected token at location
     */
    async placeSuspectedToken(latlng) {
        if (!this.isPlacementMode || !this.placementData) return;

        try {
            const response = await fetch('/SuspectedToken/PlaceSuspectedToken', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    name: this.placementData.name,
                    latitude: latlng.lat,
                    longitude: latlng.lng,
                    source: this.placementData.source,
                    confidence: this.placementData.confidence,
                    suspectedType: this.placementData.suspectedType,
                    notes: this.placementData.notes
                })
            });

            const result = await response.json();

            if (result.success) {
                this.notificationCallback(result.message, 'success');
                
                // Create marker for the new suspected token
                this.createSuspectedTokenMarker(result.suspectedToken);
                
                // Cancel placement mode
                this.cancelPlacementMode();
                
                // Close modal
                if (typeof closeSuspectedTokenModal === 'function') {
                    closeSuspectedTokenModal();
                }
            } else {
                this.notificationCallback(result.message, 'error');
            }
        } catch (error) {
            console.error('Error placing suspected token:', error);
            this.notificationCallback('Error placing suspected contact', 'error');
        }
    }

    /**
     * Show suspected token information
     */
    showSuspectedTokenInfo(tokenData) {
        // Create a popup with suspected token information
        const popup = L.popup()
            .setLatLng([tokenData.latitude, tokenData.longitude])
            .setContent(`
                <div class="suspected-token-info">
                    <h4><i class="fas fa-question-circle"></i> Suspected Contact</h4>
                    <p><strong>Name:</strong> ${tokenData.name}</p>
                    <p><strong>Confidence:</strong> ${tokenData.confidence}%</p>
                    <p><strong>Type:</strong> ${tokenData.type || 'Unknown'}</p>
                    <p><strong>Last Updated:</strong> ${new Date(tokenData.lastUpdated).toLocaleString()}</p>
                    <p><strong>Notes:</strong> ${tokenData.notes || 'No additional information'}</p>
                </div>
            `)
            .openOn(this.map);
    }

    /**
     * Update suspected token position
     */
    async updateSuspectedTokenPosition(tokenId, lat, lng) {
        try {
            const response = await fetch('/SuspectedToken/UpdateSuspectedToken', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    tokenId: tokenId,
                    latitude: lat,
                    longitude: lng
                })
            });

            const result = await response.json();

            if (result.success) {
                this.notificationCallback('Suspected contact position updated', 'success');
            } else {
                this.notificationCallback(result.message, 'error');
            }
        } catch (error) {
            console.error('Error updating suspected token position:', error);
            this.notificationCallback('Error updating position', 'error');
        }
    }

    /**
     * Remove suspected token
     */
    async removeSuspectedToken(tokenId) {
        if (!confirm('Are you sure you want to remove this suspected contact?')) {
            return;
        }

        try {
            const response = await fetch('/SuspectedToken/RemoveSuspectedToken', {
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
                const tokenData = this.suspectedTokens.get(tokenId);
                if (tokenData && tokenData.marker) {
                    this.map.removeLayer(tokenData.marker);
                }
                this.suspectedTokens.delete(tokenId);

                this.notificationCallback(result.message, 'success');
            } else {
                this.notificationCallback(result.message, 'error');
            }
        } catch (error) {
            console.error('Error removing suspected token:', error);
            this.notificationCallback('Error removing suspected contact', 'error');
        }
    }

    /**
     * Plan ISR mission for suspected token
     */
    planISRMission(tokenId) {
        console.log('Planning ISR mission for token:', tokenId);
        // TODO: Implement ISR mission planning UI
        this.notificationCallback('ISR mission planning coming soon', 'info');
    }

    /**
     * Edit suspected token
     */
    editSuspectedToken(tokenId) {
        console.log('Editing suspected token:', tokenId);
        // TODO: Implement edit UI
        this.notificationCallback('Edit functionality coming soon', 'info');
    }

    /**
     * Default notification callback
     */
    defaultNotification(message, type) {
        console.log(`[${type.toUpperCase()}] ${message}`);
    }
}

// Global functions for modal interactions
function openSuspectedTokenModal() {
    if (typeof lazyLoader !== 'undefined') {
        lazyLoader.loadPartial('suspected-token-modal', '#modalsContainer', {
            onLoaded: () => {
                document.getElementById('suspectedTokenModal').style.display = 'block';
            }
        });
    }
}

function closeSuspectedTokenModal() {
    const modal = document.getElementById('suspectedTokenModal');
    if (modal) {
        modal.style.display = 'none';
    }
}

function openSuspectedTokensListModal() {
    if (typeof lazyLoader !== 'undefined') {
        lazyLoader.loadPartial('suspected-token-modal', '#modalsContainer', {
            onLoaded: async () => {
                document.getElementById('suspectedTokensListModal').style.display = 'block';
                
                // Load and display suspected tokens
                if (window.suspectedTokenManager) {
                    const tokens = await window.suspectedTokenManager.loadSuspectedTokens();
                    displaySuspectedTokensList(tokens);
                }
            }
        });
    }
}

function closeSuspectedTokensListModal() {
    const modal = document.getElementById('suspectedTokensListModal');
    if (modal) {
        modal.style.display = 'none';
    }
}

function startSuspectedTokenPlacement() {
    const data = {
        name: document.getElementById('suspectedTokenName').value || `Contact-${Date.now()}`,
        source: document.getElementById('suspectedTokenSource').value,
        confidence: parseFloat(document.getElementById('suspectedTokenConfidence').value),
        suspectedType: document.getElementById('suspectedTokenType').value,
        notes: document.getElementById('suspectedTokenNotes').value
    };

    if (window.suspectedTokenManager) {
        window.suspectedTokenManager.startPlacementMode(data);
    }
}

function displaySuspectedTokensList(tokens) {
    const container = document.getElementById('suspectedTokensList');
    if (!container) return;

    if (!tokens || tokens.length === 0) {
        container.innerHTML = '<div class="no-tokens"><i class="fas fa-info-circle"></i><p>No enemy tokens found</p></div>';
        return;
    }

    const html = tokens.map(token => {
        // Determine confidence color
        let confidenceColor = '#ffa500';
        let confidenceLevel = 'Medium';
        if (token.confidence >= 70) {
            confidenceColor = '#ff4444';
            confidenceLevel = 'High';
        } else if (token.confidence <= 30) {
            confidenceColor = '#ffff00';
            confidenceLevel = 'Low';
        }

        return `
        <div class="suspected-token-item">
            <div class="token-info-full">
                <div class="token-header-row">
                    <h5><i class="fas fa-crosshairs"></i> ${token.name}</h5>
                    <span class="confidence-badge-large" style="background-color: ${confidenceColor};">
                        ${Math.round(token.confidence)}% ${confidenceLevel}
                    </span>
                </div>
                <div class="token-details-grid">
                    <div class="detail-item">
                        <span class="detail-label">Source:</span>
                        <span class="detail-value">${suspectedTokenManager.formatSource(token.source)}</span>
                    </div>
                    <div class="detail-item">
                        <span class="detail-label">Status:</span>
                        <span class="detail-value status-${token.status}">${suspectedTokenManager.formatStatus(token.status)}</span>
                    </div>
                    ${token.suspectedType ? `
                    <div class="detail-item">
                        <span class="detail-label">Unit Type:</span>
                        <span class="detail-value">${suspectedTokenManager.formatUnitType(token.suspectedType)}</span>
                    </div>` : ''}
                    <div class="detail-item">
                        <span class="detail-label">Detected:</span>
                        <span class="detail-value">${token.firstDetectedAt ? new Date(token.firstDetectedAt).toLocaleString() : 'Unknown'}</span>
                    </div>
                    ${token.isrMissionsCount > 0 ? `
                    <div class="detail-item">
                        <span class="detail-label">ISR Missions:</span>
                        <span class="detail-value"><span class="badge-count">${token.isrMissionsCount}</span></span>
                    </div>` : ''}
                </div>
            </div>
            <div class="token-actions-column">
                <button class="btn btn-sm btn-info" onclick="suspectedTokenManager.planISRMission('${token.id}')">
                    <i class="fas fa-satellite"></i> Plan ISR
                </button>
                <button class="btn btn-sm btn-warning" onclick="suspectedTokenManager.editSuspectedToken('${token.id}')">
                    <i class="fas fa-edit"></i> Edit
                </button>
                <button class="btn btn-sm btn-danger" onclick="suspectedTokenManager.removeSuspectedToken('${token.id}')">
                    <i class="fas fa-trash-alt"></i> Remove
                </button>
            </div>
        </div>
    `}).join('');

    container.innerHTML = html;
}

