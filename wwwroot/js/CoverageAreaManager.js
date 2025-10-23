/**
 * Coverage Area Manager
 * Handles interactive coverage area mapping and organizational sign placement
 */
class CoverageAreaManager {
    constructor(map, tokenManager) {
        this.map = map;
        this.tokenManager = tokenManager;
        this.isMappingMode = false;
        this.currentToken = null;
        this.currentCoverageType = null;
        this.currentPoints = [];
        this.tempPolygon = null;
        this.tempMarkers = [];
        this.organizationalSigns = new Map(); // Store organizational signs by token ID
        
        this.init();
    }

    init() {
        console.log('🗺️ CoverageAreaManager initialized');
        this.setupEventListeners();
    }

    setupEventListeners() {
        // Listen for map clicks when in mapping mode
        this.map.on('click', (e) => {
            if (this.isMappingMode) {
                this.handleMapClick(e);
            }
        });

        // Listen for escape key to cancel mapping
        document.addEventListener('keydown', (e) => {
            if (e.key === 'Escape' && this.isMappingMode) {
                this.cancelMapping();
            }
        });
    }

    /**
     * Start coverage area mapping for a token
     */
    startCoverageMapping(token, coverageType = 'Operational') {
        console.log(`🗺️ Starting coverage mapping for token ${token.name} (ID: ${token.id}), type: ${coverageType}`);
        
        // Check if token already has a coverage area
        if (token.areaCoverages && token.areaCoverages.length > 0) {
            const confirmed = confirm(`Token "${token.name}" already has a coverage area. Do you want to replace it?`);
            if (!confirmed) {
                console.log('🗺️ User cancelled - token already has coverage area');
                return;
            }
        }
        
        // Deep copy token data to prevent reference issues
        this.currentToken = {
            id: token.id,
            name: token.name,
            position: token.position,
            forceType: token.forceType,
            areaCoverages: token.areaCoverages ? [...token.areaCoverages] : [],
            tokenId: token.id // Add explicit tokenId for reference
        };
        
        console.log('🗺️ Token data deep copied:', this.currentToken);
        
        // Store token data in multiple places to prevent loss
        this.currentCoverageType = coverageType;
        this.isMappingMode = true;
        this.currentPoints = [];
        
        // Store token data in window object as backup
        window.currentMappingToken = this.currentToken;
        console.log('🗺️ Token data backed up to window.currentMappingToken');
        
        console.log('🗺️ Mapping mode activated for token:', this.currentToken.name);
        console.log('🗺️ Token data preserved:', this.currentToken);
        
        // Clear any existing temp elements
        this.clearTempElements();
        
        // Show instructions
        this.showMappingInstructions();
        
        // Change cursor
        if (this.map && this.map.getContainer) {
            this.map.getContainer().style.cursor = 'crosshair';
            console.log('🗺️ Cursor changed to crosshair');
        } else {
            console.error('❌ Map or getContainer not available');
        }
        
        // Add mapping controls
        this.addMappingControls();
        
        // Set up map click handler for coverage mapping
        if (this.map) {
            // Remove any existing click handlers
            this.map.off('click', this.handleMapClick);
            
            // Add new click handler
            this.map.on('click', (e) => {
                console.log('🗺️ Map clicked during mapping mode');
                if (this.isMappingMode) {
                    this.handleMapClick(e);
                }
            });
            console.log('🗺️ Map click handler set up for coverage mapping');
        } else {
            console.error('❌ Map not available for click handler');
        }
        
        console.log('✅ Coverage mapping setup complete for token:', token.name);
    }

    /**
     * Handle map clicks during coverage mapping
     */
    handleMapClick(e) {
        if (!this.isMappingMode || !this.currentToken) return;
        
        const latlng = e.latlng;
        this.currentPoints.push([latlng.lat, latlng.lng]);
        
        const pointNumber = this.currentPoints.length;
        console.log(`🗺️ Added point ${pointNumber}: ${latlng.lat.toFixed(6)}, ${latlng.lng.toFixed(6)}`);
        
        // Add visual marker for the point with sequential numbering starting from 1
        const marker = L.marker(latlng, {
            icon: L.divIcon({
                className: 'coverage-point-marker',
                html: `<div class="coverage-point">${pointNumber}</div>`,
                iconSize: [20, 20],
                iconAnchor: [10, 10]
            })
        }).addTo(this.map);
        
        this.tempMarkers.push(marker);
        
        // Update polygon if we have at least 3 points
        if (this.currentPoints.length >= 3) {
            this.updateTempPolygon();
        }
        
        // Update instructions
        this.updateMappingInstructions();
    }

    /**
     * Update temporary polygon as user adds points
     */
    updateTempPolygon() {
        // Remove existing temp polygon
        if (this.tempPolygon) {
            this.map.removeLayer(this.tempPolygon);
        }
        
        // Create new polygon with current points
        const polygon = L.polygon(this.currentPoints, {
            color: this.getCoverageColor(this.currentCoverageType),
            fillColor: this.getCoverageColor(this.currentCoverageType),
            fillOpacity: 0.2,
            opacity: 0.6,
            weight: 2,
            dashArray: '5, 5'
        }).addTo(this.map);
        
        this.tempPolygon = polygon;
    }

    /**
     * Complete the coverage area mapping
     */
    completeMapping() {
        console.log('🗺️ Complete mapping called - checking token data...');
        console.log('🗺️ Current token:', this.currentToken);
        console.log('🗺️ Current points:', this.currentPoints.length);
        
        if (this.currentPoints.length < 4) {
            this.showNotification('Please add at least 4 points to create a coverage area', 'warning');
            return;
        }
        
        // Check if token data is still available
        if (!this.currentToken || !this.currentToken.name) {
            console.error('❌ Token data lost during mapping');
            console.error('❌ Current token object:', this.currentToken);
            
            // Try to restore from backup
            if (window.currentMappingToken) {
                console.log('🔄 Attempting to restore token data from backup...');
                this.currentToken = window.currentMappingToken;
                console.log('🔄 Restored token data:', this.currentToken);
            } else {
                this.showNotification('Error: Token data lost. Please try again.', 'error');
                this.finishMapping();
                return;
            }
        }
        
        console.log('🗺️ Token data verified - proceeding with coverage area creation');
        
        // Double-check token data is still available
        if (!this.currentToken || !this.currentToken.id || !this.currentToken.name) {
            console.error('❌ Token data lost during coverage area creation');
            this.showNotification('Error: Token data lost during creation. Please try again.', 'error');
            this.finishMapping();
            return;
        }
        
        // Create coverage area object
        const coverageArea = {
            id: this.generateId(),
            shapeType: 'Custom',
            coverageType: this.currentCoverageType,
            geometry: {
                type: 'Polygon',
                coordinates: [this.currentPoints]
            },
            createdDate: new Date().toISOString(),
            tokenId: this.currentToken.id
        };
        
        console.log('🗺️ Completed coverage area:', coverageArea);
        console.log('🗺️ About to save coverage area for token:', this.currentToken.name);
        console.log('🗺️ Token ID for coverage area:', this.currentToken.id);
        
        // Save to token
        this.saveCoverageArea(coverageArea);
        
        // Store token name before cleanup to prevent loss
        const tokenName = this.currentToken ? this.currentToken.name : 'Unknown';
        
        // Clean up
        this.finishMapping();
        
        this.showNotification(`Coverage area created for ${tokenName}`, 'success');
        
        // Ask for organizational level placement only if save was successful
        // The save operation will handle this in the success callback
        console.log('🗺️ Coverage area creation completed, save operation in progress...');
    }

    /**
     * Save coverage area to token
     */
    async saveCoverageArea(coverageArea) {
        try {
            console.log('🗺️ Save coverage area called');
            console.log('🗺️ Current token in save method:', this.currentToken);
            console.log('🗺️ Coverage area to save:', coverageArea);
            
            // Check if token data is still available
            if (!this.currentToken || !this.currentToken.name || !this.currentToken.id) {
                console.error('❌ Token data not available during save');
                console.error('❌ Current token object:', this.currentToken);
                
                // Try to restore from backup
                if (window.currentMappingToken) {
                    console.log('🔄 Attempting to restore token data from backup during save...');
                    this.currentToken = window.currentMappingToken;
                    console.log('🔄 Restored token data:', this.currentToken);
                } else {
                    this.showNotification('Error: Token data not available', 'error');
                    return;
                }
            }
            
            // Additional validation
            if (!coverageArea || !coverageArea.id || !coverageArea.tokenId) {
                console.error('❌ Invalid coverage area data');
                this.showNotification('Error: Invalid coverage area data', 'error');
                return;
            }
            
            console.log('🗺️ Saving coverage area for token:', this.currentToken.name, 'ID:', this.currentToken.id);
            
            // Create coverage area JSON for storage in token
            const coverageAreaJson = {
                id: coverageArea.id,
                shapeType: coverageArea.shapeType,
                coverageType: coverageArea.coverageType,
                geometry: coverageArea.geometry,
                createdDate: coverageArea.createdDate,
                tokenId: coverageArea.tokenId
            };
            
            console.log('🗺️ Coverage area JSON to save:', coverageAreaJson);
            
            // Save to server with the coverage area JSON
            const response = await fetch('/AdminToken/UpdateTokenCoverage', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    TokenId: this.currentToken.id,
                    CoverageArea: coverageAreaJson
                })
            });
            
            if (response.ok) {
                console.log('✅ Coverage area saved successfully for token:', this.currentToken.name);
                
                // Update the current token's areaCoverages with the new coverage area
                this.currentToken.areaCoverages = [coverageAreaJson];
                
                // Refresh coverage areas on map
                if (window.tokenPlacementManager) {
                    const tokenPosition = this.currentToken.position || { lat: 0, lng: 0 };
                    window.tokenPlacementManager.createCoverageAreas(
                        this.currentToken.areaCoverages,
                        L.latLng(tokenPosition.lat, tokenPosition.lng),
                        this.currentToken.forceType,
                        this.currentToken
                    );
                }
                
                // Keep the polygon visible on map - don't clear UI
                console.log('🗺️ Save operation completed successfully, keeping polygon visible');
                
                // Convert temporary polygon to permanent coverage area
                this.convertTempPolygonToPermanent();
                
                // Ask for organizational level placement after successful save
                console.log('🗺️ Asking for organizational sign placement');
                this.askForOrganizationalLevelPlacement();
                
                // Don't clear token data immediately - keep for organizational sign placement
                console.log('🗺️ Token data preserved for organizational sign placement');
            } else {
                const errorText = await response.text();
                console.error('❌ Server error:', response.status, errorText);
                throw new Error(`Failed to save coverage area: ${response.status}`);
            }
        } catch (error) {
            console.error('❌ Error saving coverage area:', error);
            this.showNotification('Failed to save coverage area: ' + error.message, 'error');
        }
    }

    /**
     * Cancel coverage area mapping
     */
    cancelMapping() {
        console.log('🗺️ Cancelling coverage mapping');
        this.finishMapping();
    }

    /**
     * Finish mapping and clean up
     */
    finishMapping() {
        this.isMappingMode = false;
        // Don't clear currentToken immediately - let it be cleared after save is complete
        this.currentCoverageType = null;
        this.currentPoints = [];
        
        this.clearTempElements();
        this.removeMappingControls();
        this.hideMappingInstructions();
        
        // Reset cursor
        this.map.getContainer().style.cursor = '';
        
        // Don't clear token data automatically - let save operation handle it
        console.log('🗺️ Mapping finished, token data preserved for save operation');
    }

    /**
     * Convert temporary polygon to permanent coverage area
     */
    convertTempPolygonToPermanent() {
        if (this.tempPolygon && this.currentToken) {
            console.log('🗺️ Converting temporary polygon to permanent coverage area');
            
            // Remove temporary styling and make it permanent
            this.tempPolygon.setStyle({
                color: '#0000ff', // Blue for Blue Land
                weight: 2,
                opacity: 0.8,
                fillColor: '#0000ff',
                fillOpacity: 0.2,
                className: 'coverage-area-polygon'
            });
            
            // Add permanent popup
            const popupContent = `
                <div class="coverage-popup">
                    <h6><i class="fas fa-draw-polygon"></i> Coverage Area - ${this.currentToken.name}</h6>
                    <p><strong>Type:</strong> Operational</p>
                    <p><strong>Shape:</strong> Custom Polygon</p>
                    <p><strong>Points:</strong> ${this.currentPoints.length}</p>
                    <p><strong>Status:</strong> <span class="text-success">Saved</span></p>
                </div>
            `;
            this.tempPolygon.bindPopup(popupContent);
            
            // Store as permanent coverage area
            if (!this.coverageAreas) {
                this.coverageAreas = new Map();
            }
            const coverageId = `coverage_${this.currentToken.id}_${Date.now()}`;
            this.coverageAreas.set(coverageId, this.tempPolygon);
            
            // Clear temporary references
            this.tempPolygon = null;
            this.tempMarkers = [];
            
            console.log('✅ Temporary polygon converted to permanent coverage area');
        }
    }

    /**
     * Clear token data after successful save
     */
    clearTokenData() {
        this.currentToken = null;
        window.currentMappingToken = null;
        console.log('🗺️ Token data cleared after successful save');
    }

    /**
     * Clear temporary elements
     */
    clearTempElements() {
        // Remove temp markers
        this.tempMarkers.forEach(marker => {
            this.map.removeLayer(marker);
        });
        this.tempMarkers = [];
        
        // Don't remove temp polygon if it's being converted to permanent
        // The convertTempPolygonToPermanent method will handle this
        console.log('🗺️ Temporary markers cleared, polygon preserved for conversion');
    }

    /**
     * Show mapping instructions
     */
    showMappingInstructions() {
        console.log('🗺️ Showing mapping instructions...');
        debugger; // DEBUG: Check if method is called
        
        // Remove any existing instructions
        const existing = document.getElementById('mapping-instructions');
        console.log('🗺️ Existing instructions found:', !!existing);
        debugger; // DEBUG: Check existing instructions
        
        if (existing) {
            existing.remove();
            console.log('🗺️ Removed existing instructions');
        }
        
        const instructions = document.createElement('div');
        instructions.id = 'mapping-instructions';
        instructions.className = 'mapping-instructions';
        console.log('🗺️ Created instructions element');
        debugger; // DEBUG: Check element creation
        
        instructions.innerHTML = `
            <div class="mapping-instructions-content">
                <h4><i class="fas fa-mouse-pointer"></i> Coverage Area Mapping - ${this.currentToken.name}</h4>
                <p>Click on the map to add points for your coverage area.</p>
                <p><strong>Minimum 4 points required</strong></p>
                <p>Points added: <span id="point-count">0</span></p>
                <div class="mapping-actions">
                    <button class="btn btn-success" onclick="window.coverageAreaManager.completeMapping()">
                        <i class="fas fa-check"></i> Complete (4+ points)
                    </button>
                    <button class="btn btn-secondary" onclick="window.coverageAreaManager.cancelMapping()">
                        <i class="fas fa-times"></i> Cancel
                    </button>
                </div>
            </div>
        `;
        console.log('🗺️ Set instructions HTML');
        debugger; // DEBUG: Check HTML content
        
        document.body.appendChild(instructions);
        console.log('✅ Mapping instructions added to DOM');
        debugger; // DEBUG: Check DOM addition
        
        // Verify the element is in the DOM
        const addedElement = document.getElementById('mapping-instructions');
        console.log('🗺️ Instructions element in DOM:', !!addedElement);
        console.log('🗺️ Instructions element visible:', addedElement ? addedElement.offsetHeight > 0 : false);
        debugger; // DEBUG: Check final verification
    }

    /**
     * Update mapping instructions
     */
    updateMappingInstructions() {
        const pointCount = document.getElementById('point-count');
        if (pointCount) {
            pointCount.textContent = this.currentPoints.length;
        }
    }

    /**
     * Hide mapping instructions
     */
    hideMappingInstructions() {
        const instructions = document.getElementById('mapping-instructions');
        if (instructions) {
            instructions.remove();
        }
    }

    /**
     * Add mapping controls
     */
    addMappingControls() {
        console.log('🗺️ Adding mapping controls...');
        
        // Remove any existing controls first
        this.removeMappingControls();
        
        // Add controls to map
        const controls = L.control({position: 'topright'});
        controls.onAdd = () => {
            const div = L.DomUtil.create('div', 'mapping-controls');
            div.innerHTML = `
                <div class="mapping-control-panel">
                    <h6><i class="fas fa-draw-polygon"></i> Coverage Mapping</h6>
                    <p>Click to add points</p>
                    <button class="btn btn-sm btn-success" onclick="window.coverageAreaManager.completeMapping()">
                        <i class="fas fa-check"></i> Complete
                    </button>
                    <button class="btn btn-sm btn-secondary" onclick="window.coverageAreaManager.cancelMapping()">
                        <i class="fas fa-times"></i> Cancel
                    </button>
                </div>
            `;
            return div;
        };
        
        if (this.map) {
            controls.addTo(this.map);
            console.log('✅ Mapping controls added to map');
        } else {
            console.error('❌ Map not available for controls');
        }
    }

    /**
     * Remove mapping controls
     */
    removeMappingControls() {
        // Remove controls from map
        this.map.eachLayer(layer => {
            if (layer._container && layer._container.classList && layer._container.classList.contains('mapping-controls')) {
            }
        });
    }

    /**
     * Start organizational sign mapping
     */
    startOrganizationalSignMapping(token) {
        console.log(`🏷️ Starting organizational sign mapping for token ${token.name}`);
        
        this.currentToken = token;
        this.isMappingMode = true;
        
        // Show instructions
        this.showOrganizationalSignInstructions();
        
        // Change cursor
        this.map.getContainer().style.cursor = 'crosshair';
        
        // Set up map click handler for organizational sign placement
        this.map.off('click', this.handleOrganizationalSignClick);
        this.map.on('click', (e) => {
            if (this.isMappingMode) {
                this.handleOrganizationalSignClick(e);
            }
        });
    }

    /**
     * Handle organizational sign placement
     */
    handleOrganizationalSignClick(e) {
        if (!this.isMappingMode || !this.currentToken) return;
        
        const latlng = e.latlng;
        
        // Create organizational sign
        const sign = {
            id: this.generateId(),
            tokenId: this.currentToken.id,
            position: {
                lat: latlng.lat,
                lng: latlng.lng
            },
            createdDate: new Date().toISOString()
        };
        
        // Add to token
        if (!this.currentToken.organizationalSigns) {
            this.currentToken.organizationalSigns = [];
        }
        this.currentToken.organizationalSigns.push(sign);
        
        // Create marker on map
        const marker = L.marker(latlng, {
            icon: L.divIcon({
                className: 'organizational-sign-marker',
                html: `<div class="org-sign">${this.currentToken.organizationLevel || 'ORG'}</div>`,
                iconSize: [30, 30],
                iconAnchor: [15, 15]
            })
        }).addTo(this.map);
        
        marker.organizationalSign = sign;
        
        // Store in our map
        this.organizationalSigns.set(sign.id, marker);
        
        console.log('🏷️ Organizational sign placed:', sign);
        
        // Finish mapping
        this.finishMapping();
        this.hideOrganizationalSignInstructions();
        
        this.showNotification(`Organizational sign placed for ${this.currentToken.name}`, 'success');
    }

    /**
     * Show organizational sign instructions
     */
    showOrganizationalSignInstructions() {
        const instructions = document.createElement('div');
        instructions.id = 'org-sign-instructions';
        instructions.className = 'mapping-instructions';
        instructions.innerHTML = `
            <div class="mapping-instructions-content">
                <h4><i class="fas fa-map-marker-alt"></i> Organizational Sign Placement</h4>
                <p>Click on the map to place the organizational sign for ${this.currentToken.name}.</p>
                <div class="mapping-actions">
                    <button class="btn btn-secondary" onclick="window.coverageAreaManager.cancelMapping()">
                        <i class="fas fa-times"></i> Cancel
                    </button>
                </div>
            </div>
        `;
        
        document.body.appendChild(instructions);
    }

    /**
     * Hide organizational sign instructions
     */
    hideOrganizationalSignInstructions() {
        const instructions = document.getElementById('org-sign-instructions');
        if (instructions) {
            instructions.remove();
        }
    }

    /**
     * Get coverage color based on type
     */
    getCoverageColor(coverageType) {
        const colors = {
            'Operational': '#3388ff',
            'Tactical': '#ff6b35',
            'Strategic': '#28a745',
            'Defensive': '#dc3545',
            'Reconnaissance': '#6f42c1'
        };
        return colors[coverageType] || '#3388ff';
    }

    /**
     * Generate unique ID
     */
    generateId() {
        return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {
            const r = Math.random() * 16 | 0;
            const v = c == 'x' ? r : (r & 0x3 | 0x8);
            return v.toString(16);
        });
    }

    /**
     * Ask for organizational level placement after coverage area completion
     */
    askForOrganizationalLevelPlacement() {
        console.log('🏷️ Asking for organizational level placement');
        
        // Show confirmation dialog
        const confirmed = confirm(`Coverage area created successfully!\n\nWould you like to place an organizational sign for ${this.currentToken.name}?`);
        
        if (confirmed) {
            console.log('🏷️ User confirmed organizational sign placement');
            this.startOrganizationalSignMapping(this.currentToken);
        } else {
            console.log('🏷️ User declined organizational sign placement');
            this.showNotification('Coverage area mapping completed', 'success');
        }
    }

    /**
     * Show notification
     */
    showNotification(message, type = 'info') {
        if (window.showNotification) {
            window.showNotification(message, type);
        } else {
            console.log(`${type.toUpperCase()}: ${message}`);
        }
    }
}

// Make it globally available
window.CoverageAreaManager = CoverageAreaManager;
