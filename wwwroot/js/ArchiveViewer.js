/**
 * Archive Viewer - Loads and displays pre-saved war game data
 * Read-only viewer - no AJAX calls, everything is pre-saved
 */
class ArchiveViewer {
    constructor(map) {
        this.map = map;
        this.tokens = [];
        this.attacks = [];
        this.defenseElements = [];
        this.mapOverlays = {};
        this.isReadOnly = true;
        
        console.log('📦 ArchiveViewer initialized (read-only mode)');
    }

    /**
     * Load archive data and display on map
     */
    loadArchiveData(archiveData) {
        try {
            console.log('📥 Loading archive data...', archiveData);
            
            // Clear existing data
            this.clearAll();
            
            // Parse JSON strings
            const forces = this.parseJson(archiveData.forcesJson);
            const attacks = this.parseJson(archiveData.attacksJson);
            const defenseElements = this.parseJson(archiveData.defenseElementsJson);
            const mapOverlays = this.parseJson(archiveData.mapOverlaysJson);
            
            // Load forces (tokens)
            if (forces && Array.isArray(forces)) {
                this.loadForces(forces);
            }
            
            // Load attacks
            if (attacks) {
                this.loadAttacks(attacks);
            }
            
            // Load defense elements
            if (defenseElements && Array.isArray(defenseElements)) {
                this.loadDefenseElements(defenseElements);
            }
            
            // Load map overlays
            if (mapOverlays) {
                this.loadMapOverlays(mapOverlays);
            }
            
            console.log('✅ Archive data loaded successfully');
        } catch (error) {
            console.error('❌ Error loading archive data:', error);
        }
    }

    /**
     * Load forces (tokens) from archive data
     */
    loadForces(forces) {
        try {
            console.log(`📥 Loading ${forces.length} forces from archive...`);
            
            if (!this.map || !window.tokenManager) {
                console.error('❌ Map or TokenManager not available');
                return;
            }
            
            forces.forEach(force => {
                try {
                    // Get the last active marker for position
                    const markers = force.markers || [];
                    const activeMarkers = markers.filter(m => m.isActive);
                    const lastMarker = activeMarkers.length > 0 
                        ? activeMarkers[activeMarkers.length - 1] 
                        : (markers.length > 0 ? markers[markers.length - 1] : null);
                    
                    if (!lastMarker) {
                        console.warn(`⚠️ Force ${force.name} has no markers, skipping`);
                        return;
                    }
                    
                    const position = {
                        lat: parseFloat(lastMarker.latitude),
                        lng: parseFloat(lastMarker.longitude)
                    };
                    
                    // Create token marker (read-only)
                    const tokenData = {
                        id: force.id,
                        name: force.name,
                        forceType: force.forceType,
                        tokenGroup: force.tokenGroup,
                        position: position,
                        isReadOnly: true
                    };
                    
                    // Use TokenManager to place token (read-only)
                    if (window.tokenManager && typeof window.tokenManager.placeToken === 'function') {
                        window.tokenManager.placeToken(tokenData);
                        this.tokens.push(tokenData);
                    } else {
                        // Fallback: create marker directly
                        this.createReadOnlyTokenMarker(tokenData, position);
                    }
                } catch (error) {
                    console.error(`❌ Error loading force ${force.name}:`, error);
                }
            });
            
            console.log(`✅ Loaded ${this.tokens.length} forces`);
        } catch (error) {
            console.error('❌ Error loading forces:', error);
        }
    }

    /**
     * Create a read-only token marker
     */
    createReadOnlyTokenMarker(tokenData, position) {
        try {
            // Create a simple marker for archived tokens
            const marker = L.marker([position.lat, position.lng], {
                draggable: false, // Read-only
                icon: L.icon({
                    iconUrl: '/assets/images/marker-icon.png',
                    iconSize: [25, 41],
                    iconAnchor: [12, 41]
                })
            });
            
            // Add popup with token info (read-only)
            const popupContent = `
                <div class="archive-token-popup">
                    <h6>${tokenData.name}</h6>
                    <p><strong>Type:</strong> ${tokenData.tokenGroup || 'Unknown'}</p>
                    <p><strong>Force:</strong> ${tokenData.forceType || 'Unknown'}</p>
                    <p class="text-muted"><small><i class="fas fa-lock"></i> Read-Only Archive View</small></p>
                    <button class="btn btn-sm btn-primary mt-2" onclick="showArchiveTokenSummary('${tokenData.id}')">
                        <i class="fas fa-info-circle"></i> View Details
                    </button>
                </div>
            `;
            
            marker.bindPopup(popupContent);
            marker.addTo(this.map);
            
            // Store reference
            marker.tokenData = tokenData;
            this.tokens.push({ marker, tokenData });
        } catch (error) {
            console.error('❌ Error creating read-only token marker:', error);
        }
    }

    /**
     * Load attacks from archive data
     */
    loadAttacks(attacks) {
        try {
            console.log('📥 Loading attacks from archive...', attacks);
            
            if (!this.map) {
                console.error('❌ Map not available');
                return;
            }
            
            // Load attack orders
            if (attacks.attackOrders && Array.isArray(attacks.attackOrders)) {
                attacks.attackOrders.forEach(attack => {
                    this.createAttackVisualization(attack);
                });
            }
            
            // Load enhanced attack orders
            if (attacks.enhancedAttackOrders && Array.isArray(attacks.enhancedAttackOrders)) {
                attacks.enhancedAttackOrders.forEach(attack => {
                    this.createAttackVisualization(attack);
                });
            }
            
            console.log(`✅ Loaded ${this.attacks.length} attacks`);
        } catch (error) {
            console.error('❌ Error loading attacks:', error);
        }
    }

    /**
     * Create attack visualization (read-only)
     */
    createAttackVisualization(attack) {
        try {
            // Find attacker and target tokens
            const attackerToken = this.tokens.find(t => 
                t.tokenData && t.tokenData.id === attack.attackerTokenId
            );
            const targetToken = this.tokens.find(t => 
                t.tokenData && t.tokenData.id === attack.targetTokenId
            );
            
            if (!attackerToken || !targetToken) {
                console.warn(`⚠️ Attack references missing tokens: ${attack.id}`);
                return;
            }
            
            const attackerPos = attackerToken.marker.getLatLng();
            const targetPos = targetToken.marker.getLatLng();
            
            // Create attack line (read-only)
            const attackLine = L.polyline(
                [[attackerPos.lat, attackerPos.lng], [targetPos.lat, targetPos.lng]],
                {
                    color: '#ff0000',
                    weight: 3,
                    opacity: 0.7,
                    dashArray: '10, 5'
                }
            );
            
            // Add popup with attack details
            const popupContent = `
                <div class="archive-attack-popup">
                    <h6>Attack Order</h6>
                    <p><strong>From:</strong> ${attack.attackerTokenName || 'Unknown'}</p>
                    <p><strong>To:</strong> ${attack.targetTokenName || 'Unknown'}</p>
                    <p><strong>Status:</strong> ${attack.status || 'Unknown'}</p>
                    <p class="text-muted"><small><i class="fas fa-lock"></i> Read-Only Archive View</small></p>
                    <button class="btn btn-sm btn-primary mt-2" onclick="showArchiveAttackDetails('${attack.id}')">
                        <i class="fas fa-info-circle"></i> View Details
                    </button>
                </div>
            `;
            
            attackLine.bindPopup(popupContent);
            attackLine.addTo(this.map);
            
            // Store reference
            this.attacks.push({ line: attackLine, attack });
        } catch (error) {
            console.error('❌ Error creating attack visualization:', error);
        }
    }

    /**
     * Load defense elements from archive data
     */
    loadDefenseElements(elements) {
        try {
            console.log(`📥 Loading ${elements.length} defense elements from archive...`);
            
            if (!this.map) {
                console.error('❌ Map not available');
                return;
            }
            
            elements.forEach(element => {
                try {
                    const coordinates = this.parseJson(element.coordinates);
                    if (!coordinates || !Array.isArray(coordinates)) {
                        console.warn(`⚠️ Defense element ${element.id} has invalid coordinates`);
                        return;
                    }
                    
                    // Create defense element on map (read-only)
                    this.createDefenseElement(element, coordinates);
                } catch (error) {
                    console.error(`❌ Error loading defense element ${element.id}:`, error);
                }
            });
            
            console.log(`✅ Loaded ${this.defenseElements.length} defense elements`);
        } catch (error) {
            console.error('❌ Error loading defense elements:', error);
        }
    }

    /**
     * Create defense element on map (read-only)
     */
    createDefenseElement(element, coordinates) {
        try {
            // Convert coordinates to Leaflet format
            const latlngs = coordinates.map(coord => [coord.lat || coord[0], coord.lng || coord[1]]);
            
            // Create polygon/polyline based on element type
            let layer;
            if (latlngs.length >= 3) {
                layer = L.polygon(latlngs, {
                    color: this.getDefenseColor(element.category),
                    fillColor: this.getDefenseFillColor(element.category),
                    fillOpacity: 0.3,
                    weight: 2,
                    opacity: 0.7
                });
            } else if (latlngs.length === 2) {
                layer = L.polyline(latlngs, {
                    color: this.getDefenseColor(element.category),
                    weight: 3,
                    opacity: 0.7
                });
            } else {
                // Single point - create marker
                layer = L.marker(latlngs[0], {
                    icon: L.icon({
                        iconUrl: this.getDefenseIcon(element.category),
                        iconSize: [25, 25]
                    })
                });
            }
            
            // Add popup
            const popupContent = `
                <div class="archive-defense-popup">
                    <h6>${element.category} - ${element.type}</h6>
                    <p><strong>Strength:</strong> ${element.strength || 'N/A'}</p>
                    <p class="text-muted"><small><i class="fas fa-lock"></i> Read-Only Archive View</small></p>
                </div>
            `;
            
            layer.bindPopup(popupContent);
            layer.addTo(this.map);
            
            // Store reference
            this.defenseElements.push({ layer, element });
        } catch (error) {
            console.error('❌ Error creating defense element:', error);
        }
    }

    /**
     * Load map overlays from archive data
     */
    loadMapOverlays(overlays) {
        try {
            console.log('📥 Loading map overlays from archive...');
            
            if (!this.map) {
                console.error('❌ Map not available');
                return;
            }
            
            // Load regions
            if (overlays.regions && Array.isArray(overlays.regions)) {
                overlays.regions.forEach(region => {
                    try {
                        const geometry = this.parseJson(region.geometry);
                        if (geometry) {
                            this.createMapOverlay(geometry, region.name, 'region');
                        }
                    } catch (error) {
                        console.error(`❌ Error loading region ${region.name}:`, error);
                    }
                });
            }
            
            // Load sectors
            if (overlays.sectors && Array.isArray(overlays.sectors)) {
                overlays.sectors.forEach(sector => {
                    try {
                        const geometry = this.parseJson(sector.geometry);
                        if (geometry) {
                            this.createMapOverlay(geometry, sector.name, 'sector');
                        }
                    } catch (error) {
                        console.error(`❌ Error loading sector ${sector.name}:`, error);
                    }
                });
            }
            
            // Load labels
            if (overlays.labels && Array.isArray(overlays.labels)) {
                overlays.labels.forEach(label => {
                    try {
                        const marker = L.marker([label.latitude, label.longitude], {
                            icon: L.icon({
                                iconUrl: '/assets/images/label-icon.png',
                                iconSize: [20, 20]
                            })
                        });
                        
                        marker.bindPopup(`<strong>${label.name}</strong>`);
                        marker.addTo(this.map);
                    } catch (error) {
                        console.error(`❌ Error loading label ${label.name}:`, error);
                    }
                });
            }
            
            console.log('✅ Map overlays loaded');
        } catch (error) {
            console.error('❌ Error loading map overlays:', error);
        }
    }

    /**
     * Create map overlay from GeoJSON geometry
     */
    createMapOverlay(geometry, name, type) {
        try {
            const layer = L.geoJSON(geometry, {
                style: {
                    color: type === 'region' ? '#0066cc' : '#cc6600',
                    weight: 2,
                    fillOpacity: 0.1
                }
            });
            
            layer.bindPopup(`<strong>${name}</strong><br/>Type: ${type}`);
            layer.addTo(this.map);
        } catch (error) {
            console.error(`❌ Error creating map overlay ${name}:`, error);
        }
    }

    /**
     * Clear all loaded data
     */
    clearAll() {
        // Clear tokens
        this.tokens.forEach(token => {
            if (token.marker && this.map) {
                this.map.removeLayer(token.marker);
            }
        });
        this.tokens = [];
        
        // Clear attacks
        this.attacks.forEach(attack => {
            if (attack.line && this.map) {
                this.map.removeLayer(attack.line);
            }
        });
        this.attacks = [];
        
        // Clear defense elements
        this.defenseElements.forEach(element => {
            if (element.layer && this.map) {
                this.map.removeLayer(element.layer);
            }
        });
        this.defenseElements = [];
        
        console.log('🧹 Cleared all archive data');
    }

    /**
     * Parse JSON string safely
     */
    parseJson(jsonString) {
        if (!jsonString || jsonString === '{}' || jsonString === '[]') {
            return null;
        }
        
        try {
            if (typeof jsonString === 'string') {
                return JSON.parse(jsonString);
            }
            return jsonString;
        } catch (error) {
            console.error('❌ Error parsing JSON:', error, jsonString);
            return null;
        }
    }

    /**
     * Get defense element color by category
     */
    getDefenseColor(category) {
        const colors = {
            killzone: '#ff0000',
            minefield: '#ff9900',
            obstacle: '#9900ff',
            position: '#00ff00',
            route: '#0000ff',
            line: '#00ffff'
        };
        return colors[category.toLowerCase()] || '#666666';
    }

    /**
     * Get defense element fill color
     */
    getDefenseFillColor(category) {
        return this.getDefenseColor(category);
    }

    /**
     * Get defense element icon
     */
    getDefenseIcon(category) {
        const icons = {
            killzone: '/assets/images/killzone-icon.png',
            minefield: '/assets/images/minefield-icon.png',
            obstacle: '/assets/images/obstacle-icon.png',
            position: '/assets/images/position-icon.png'
        };
        return icons[category.toLowerCase()] || '/assets/images/marker-icon.png';
    }
}

// Global functions for archive viewer
window.showArchiveTokenSummary = function(tokenId) {
    // Find token data
    if (window.archiveViewer && window.archiveViewer.tokens) {
        const token = window.archiveViewer.tokens.find(t => 
            t.tokenData && t.tokenData.id === tokenId
        );
        
        if (token) {
            // Show token summary modal (read-only)
            $('#archiveTokenSummaryModal').modal('show');
            // TODO: Populate modal with token data
        }
    }
};

window.showArchiveAttackDetails = function(attackId) {
    // Find attack data
    if (window.archiveViewer && window.archiveViewer.attacks) {
        const attack = window.archiveViewer.attacks.find(a => 
            a.attack && a.attack.id === attackId
        );
        
        if (attack) {
            // Show attack details modal (read-only)
            $('#archiveAttackDetailsModal').modal('show');
            // TODO: Populate modal with attack data
        }
    }
};

