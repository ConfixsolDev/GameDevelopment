/**
 * RegionManager - Handles polygon-based region creation and management
 * Provides tools for creating labeled regions on the map for better understanding
 */
class RegionManager {
    constructor(map, notificationCallback) {
        this.map = map;
        this.notificationCallback = notificationCallback || this.defaultNotification;
        this.regions = new Map(); // regionId -> { polygon, label, data }
        this.isDrawingMode = false;
        this.currentPolygon = null;
        this.currentPoints = [];
        this.drawingLayer = null;
        this.regionsLayer = null;
        this.isVisible = true;
        this.selectedForce = 'blue'; // 'blue' or 'fox'
        
        this.setupDrawingLayer();
        this.setupRegionsLayer();
        this.setupEventListeners();
    }

    /**
     * Setup drawing layer for temporary polygons
     */
    setupDrawingLayer() {
        this.drawingLayer = L.layerGroup().addTo(this.map);
    }

    /**
     * Setup regions layer for all regions
     */
    setupRegionsLayer() {
        this.regionsLayer = L.layerGroup().addTo(this.map);
    }

    /**
     * Setup event listeners
     */
    setupEventListeners() {
        // Map click events for drawing
        this.map.on('click', (e) => {
            if (this.isDrawingMode) {
                this.addPointToPolygon(e.latlng);
            }
        });

        // Double-click to finish polygon
        this.map.on('dblclick', (e) => {
            if (this.isDrawingMode && this.currentPoints.length >= 3) {
                this.finishPolygon();
            }
        });

        // Escape key to cancel drawing
        document.addEventListener('keydown', (e) => {
            if (e.key === 'Escape' && this.isDrawingMode) {
                this.cancelDrawing();
            }
        });
    }

    /**
     * Start drawing a new region
     */
    startDrawingRegion() {
        if (this.isDrawingMode) {
            this.cancelDrawing();
        }

        this.isDrawingMode = true;
        this.currentPoints = [];
        this.currentPolygon = null;
        
        // Change cursor
        this.map.getContainer().style.cursor = 'crosshair';
        
        const forceName = this.selectedForce === 'blue' ? 'Blue Land' : 'Fox Land';
        this.notificationCallback(`Click on the map to start drawing a ${forceName} region. Double-click to finish.`, 'info');
    }

    /**
     * Add point to current polygon
     */
    addPointToPolygon(latlng) {
        this.currentPoints.push(latlng);
        
        // Remove previous temporary polygon
        if (this.currentPolygon) {
            this.drawingLayer.removeLayer(this.currentPolygon);
        }
        
        // Get color based on selected force
        const color = this.selectedForce === 'blue' ? '#00d4ff' : '#ff6b6b';
        
        // Create temporary polygon
        if (this.currentPoints.length >= 3) {
            this.currentPolygon = L.polygon(this.currentPoints, {
                color: color,
                weight: 2,
                opacity: 0.7,
                fillColor: color,
                fillOpacity: 0.2,
                dashArray: '5, 5'
            }).addTo(this.drawingLayer);
        }
        
        // Add temporary markers for points
        L.circleMarker(latlng, {
            radius: 4,
            color: color,
            fillColor: color,
            fillOpacity: 0.8
        }).addTo(this.drawingLayer);
    }

    /**
     * Finish drawing the polygon
     */
    finishPolygon() {
        if (this.currentPoints.length < 3) {
            this.notificationCallback('A region must have at least 3 points', 'error');
            return;
        }

        // Show region creation modal
        this.showRegionCreationModal();
    }

    /**
     * Show region creation modal
     */
    showRegionCreationModal() {
        const existingModal = document.getElementById('regionCreationModal');
        if (existingModal) {
            existingModal.remove();
        }

        const modal = document.createElement('div');
        modal.className = 'gameplay-modal';
        modal.id = 'regionCreationModal';
        modal.innerHTML = `
            <div class="gameplay-modal-content">
                <div class="gameplay-modal-header">
                    <h3><i class="fas fa-map-marked-alt"></i> Create Map Region</h3>
                    <button class="gameplay-modal-close" onclick="this.closest('.gameplay-modal').remove()">&times;</button>
                </div>
                <div class="gameplay-modal-body">
                    <div class="data-entry-container">
                        <div class="data-entry-form-section">
                            <!-- Region Header -->
                            <div class="brigade-header-section">
                                <div class="brigade-name-display">
                                    <h5><i class="fas fa-map-marked-alt"></i> Region Information</h5>
                                </div>
                            </div>
                            
                            <!-- Region Form -->
                            <div class="brigade-data-form">
                                <div class="data-tab-content active">
                                    <div class="tab-content-header">
                                        <h6><i class="fas fa-info-circle"></i> Region Details</h6>
                                        <p class="text-muted">Define the region name and properties</p>
                                    </div>
                                    
                                    <!-- Region Name -->
                                    <div class="form-group">
                                        <label for="regionName" style="color: #ccc; font-size: 12px; margin-bottom: 5px;">Region Name *</label>
                                        <input type="text" class="form-control" id="regionName" placeholder="Enter region name" required>
                                    </div>
                                    
                                    <!-- Force Type (readonly) -->
                                    <div class="form-group">
                                        <label for="regionForce" style="color: #ccc; font-size: 12px; margin-bottom: 5px;">Force Type</label>
                                        <input type="text" class="form-control" id="regionForce" readonly style="background: #333; color: #ccc;">
                                    </div>
                                    
                                    <!-- Region Type -->
                                    <div class="form-group">
                                        <label for="regionType" style="color: #ccc; font-size: 12px; margin-bottom: 5px;">Region Type</label>
                                        <select class="form-control" id="regionType">
                                            <option value="terrain">Terrain</option>
                                            <option value="zone">Zone</option>
                                            <option value="area">Area</option>
                                            <option value="sector">Sector</option>
                                            <option value="boundary">Boundary</option>
                                        </select>
                                    </div>
                                    
                                    <!-- Description -->
                                    <div class="form-group">
                                        <label for="regionDescription" style="color: #ccc; font-size: 12px; margin-bottom: 5px;">Description</label>
                                        <textarea class="form-control" id="regionDescription" rows="3" placeholder="Region description..."></textarea>
                                    </div>
                                </div>
                            </div>
                            
                            <!-- Action Buttons -->
                            <div class="add-unit-section">
                                <button type="button" class="btn btn-outline-secondary" onclick="this.closest('.gameplay-modal').remove()" style="margin-right: 10px;">
                                    <i class="fas fa-times"></i> Cancel
                                </button>
                                <button type="button" class="btn btn-primary" onclick="window.regionManager.saveRegion()">
                                    <i class="fas fa-save"></i> Create Region
                                </button>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
            
            <style>
            /* Modal Centering */
            .gameplay-modal {
                display: flex !important;
                align-items: center;
                justify-content: center;
            }

            .gameplay-modal-content {
                margin: 0;
                max-width: 600px;
                width: 90%;
            }

            /* Dark Theme Data Entry Form */
            .data-entry-container {
                background: #1a1a1a;
                border-radius: 8px;
                padding: 20px;
            }

            .brigade-header-section {
                margin-bottom: 20px;
                padding: 15px;
                background: #2a2a2a;
                border-radius: 8px;
                border-left: 4px solid #00d4ff;
            }

            .brigade-name-display h5 {
                color: #00d4ff;
                margin: 0;
                font-weight: 600;
            }

            .data-tab-content {
                display: block;
                padding: 20px;
                border: 1px solid #333;
                border-radius: 8px;
                background: #2a2a2a;
                margin-top: 10px;
            }

            .tab-content-header {
                margin-bottom: 15px;
                padding-bottom: 10px;
                border-bottom: 1px solid #444;
            }

            .tab-content-header h6 {
                color: #00d4ff;
                margin: 0 0 5px 0;
                font-weight: 600;
            }

            .tab-content-header .text-muted {
                color: #888 !important;
                font-size: 13px;
            }

            .btn {
                padding: 10px 20px;
                border-radius: 6px;
                font-weight: 600;
                transition: all 0.3s ease;
            }

            .btn-primary {
                background: #00d4ff;
                border-color: #00d4ff;
                color: #000;
            }

            .btn-primary:hover {
                background: #00b8e6;
                border-color: #00b8e6;
                color: #000;
            }

            .btn-outline-secondary {
                background: transparent;
                border-color: #666;
                color: #ccc;
            }

            .btn-outline-secondary:hover {
                background: #666;
                border-color: #666;
                color: #fff;
            }

            .form-control {
                background: #1a1a1a;
                border: 1px solid #444;
                color: #fff;
                border-radius: 6px;
                padding: 10px 12px;
            }

            .form-control:focus {
                background: #2a2a2a;
                border-color: #00d4ff;
                color: #fff;
                box-shadow: 0 0 0 0.2rem rgba(0, 212, 255, 0.25);
            }

            .text-muted {
                color: #888 !important;
            }

            .add-unit-section {
                text-align: center;
                padding: 15px;
                border-top: 1px solid #444;
                margin-top: 10px;
            }

            .form-group {
                margin-bottom: 15px;
            }

            .form-group label {
                font-weight: 600;
                color: #ccc;
                margin-bottom: 5px;
                font-size: 12px;
            }
            </style>
        `;
        
        document.body.appendChild(modal);
        window.regionManager = this;
        
        // Set force type in the form
        const forceName = this.selectedForce === 'blue' ? 'Blue Land' : 'Fox Land';
        document.getElementById('regionForce').value = forceName;
    }

    /**
     * Save the region to localStorage
     */
    saveRegion() {
        const name = document.getElementById('regionName').value.trim();
        const type = document.getElementById('regionType').value;
        const description = document.getElementById('regionDescription').value.trim();
        const color = this.selectedForce === 'blue' ? '#00d4ff' : '#ff6b6b';

        if (!name) {
            this.notificationCallback('Please enter a region name', 'error');
            return;
        }

        try {
            // Calculate area and center
            const area = this.calculatePolygonArea(this.currentPoints);
            const center = this.calculatePolygonCenter(this.currentPoints);

            // Create GeoJSON geometry
            const geometry = {
                type: "Polygon",
                coordinates: [this.currentPoints.map(p => [p.lng, p.lat])]
            };

            // Generate unique ID
            const regionId = 'region_' + Date.now() + '_' + Math.random().toString(36).substr(2, 9);

            // Create region data
            const regionData = {
                id: regionId,
                name: name,
                geometry: JSON.stringify(geometry),
                regionType: type,
                description: description,
                areaM2: area,
                centerLat: center.lat,
                centerLng: center.lng,
                properties: JSON.stringify({ 
                    color: color, 
                    force: this.selectedForce,
                    forceName: this.selectedForce === 'blue' ? 'Blue Land' : 'Fox Land'
                }),
                isLocked: false,
                createdDate: new Date().toISOString()
            };

            // Save to localStorage
            this.saveRegionToLocalStorage(regionData);

            // Create permanent polygon on map
            const polygon = L.polygon(this.currentPoints, {
                color: color,
                weight: 2,
                opacity: 0.8,
                fillColor: color,
                fillOpacity: 0.3
            }).addTo(this.regionsLayer);

            // Create label at center
            const label = L.marker(center, {
                icon: L.divIcon({
                    className: 'region-label',
                    html: `<div style="
                        background: rgba(0,0,0,0.8);
                        color: white;
                        padding: 4px 8px;
                        border-radius: 4px;
                        font-size: 12px;
                        font-weight: bold;
                        text-align: center;
                        border: 1px solid ${color};
                    ">${name}</div>`,
                    iconSize: [100, 20],
                    iconAnchor: [50, 10]
                })
            }).addTo(this.regionsLayer);

            // Store region data
            this.regions.set(regionId, {
                polygon: polygon,
                label: label,
                data: regionData
            });

            // Clean up and reset for next region
            this.cleanupDrawing();
            document.getElementById('regionCreationModal').remove();
            
            this.notificationCallback(`Region "${name}" created successfully. You can now create another region.`, 'success');
            
            // Reset drawing mode to allow creating another region
            this.isDrawingMode = false;
            this.map.getContainer().style.cursor = '';
            
            // Update region count in UI
            this.updateRegionCount();
            
        } catch (error) {
            console.error('Error saving region:', error);
            this.notificationCallback('Error creating region', 'error');
        }
    }

    /**
     * Cancel drawing mode
     */
    cancelDrawing() {
        this.isDrawingMode = false;
        this.currentPoints = [];
        this.currentPolygon = null;
        this.cleanupDrawing();
        this.map.getContainer().style.cursor = '';
        this.notificationCallback('Region drawing cancelled', 'info');
    }

    /**
     * Clean up drawing elements
     */
    cleanupDrawing() {
        if (this.drawingLayer) {
            this.drawingLayer.clearLayers();
        }
    }

    /**
     * Calculate polygon area in square meters
     */
    calculatePolygonArea(points) {
        if (points.length < 3) return 0;
        
        let area = 0;
        for (let i = 0; i < points.length; i++) {
            const j = (i + 1) % points.length;
            area += points[i].lng * points[j].lat;
            area -= points[j].lng * points[i].lat;
        }
        area = Math.abs(area) / 2;
        
        // Convert to square meters (rough approximation)
        return area * 111000 * 111000;
    }

    /**
     * Calculate polygon center
     */
    calculatePolygonCenter(points) {
        let lat = 0, lng = 0;
        points.forEach(point => {
            lat += point.lat;
            lng += point.lng;
        });
        return {
            lat: lat / points.length,
            lng: lng / points.length
        };
    }

    /**
     * Save region to localStorage
     */
    saveRegionToLocalStorage(regionData) {
        try {
            // Get existing regions from localStorage
            const existingRegions = this.loadRegionsFromLocalStorage();
            
            // Add new region
            existingRegions.push(regionData);
            
            // Save back to localStorage
            localStorage.setItem('gamePlay_regions', JSON.stringify(existingRegions));
            console.log(`💾 Saved region "${regionData.name}" to localStorage`);
        } catch (error) {
            console.error('Error saving region to localStorage:', error);
        }
    }

    /**
     * Load existing regions from localStorage
     */
    loadRegionsFromLocalStorage() {
        try {
            const stored = localStorage.getItem('gamePlay_regions');
            if (stored) {
                return JSON.parse(stored);
            }
            return [];
        } catch (error) {
            console.error('Error loading regions from localStorage:', error);
            return [];
        }
    }

    /**
     * Load existing regions from localStorage
     */
    loadRegions() {
        try {
            const regions = this.loadRegionsFromLocalStorage();
            
            regions.forEach(regionData => {
                this.createRegionFromData(regionData);
            });
            
            console.log(`📂 Loaded ${regions.length} regions from localStorage`);
        } catch (error) {
            console.error('Error loading regions:', error);
        }
    }

    /**
     * Create region from database data
     */
    createRegionFromData(regionData) {
        try {
            const geometry = JSON.parse(regionData.geometry);
            const properties = regionData.properties ? JSON.parse(regionData.properties) : {};
            const color = properties.color || '#00d4ff';
            
            // Convert coordinates to LatLng
            const points = geometry.coordinates[0].map(coord => 
                L.latLng(coord[1], coord[0])
            );
            
            // Create polygon
            const polygon = L.polygon(points, {
                color: color,
                weight: 2,
                opacity: 0.8,
                fillColor: color,
                fillOpacity: 0.3
            }).addTo(this.regionsLayer);

            // Create label
            const center = L.latLng(regionData.centerLat, regionData.centerLng);
            const label = L.marker(center, {
                icon: L.divIcon({
                    className: 'region-label',
                    html: `<div style="
                        background: rgba(0,0,0,0.8);
                        color: white;
                        padding: 4px 8px;
                        border-radius: 4px;
                        font-size: 12px;
                        font-weight: bold;
                        text-align: center;
                        border: 1px solid ${color};
                    ">${regionData.name}</div>`,
                    iconSize: [100, 20],
                    iconAnchor: [50, 10]
                })
            }).addTo(this.regionsLayer);

            // Store region data
            this.regions.set(regionData.id, {
                polygon: polygon,
                label: label,
                data: regionData
            });
        } catch (error) {
            console.error('Error creating region from data:', error);
        }
    }

    /**
     * Delete a region from localStorage
     */
    deleteRegion(regionId) {
        try {
            // Remove from map
            const region = this.regions.get(regionId);
            if (region) {
                this.regionsLayer.removeLayer(region.polygon);
                this.regionsLayer.removeLayer(region.label);
                this.regions.delete(regionId);
            }
            
            // Remove from localStorage
            const existingRegions = this.loadRegionsFromLocalStorage();
            const updatedRegions = existingRegions.filter(r => r.id !== regionId);
            localStorage.setItem('gamePlay_regions', JSON.stringify(updatedRegions));
            
            this.notificationCallback('Region deleted successfully', 'success');
        } catch (error) {
            console.error('Error deleting region:', error);
            this.notificationCallback('Error deleting region', 'error');
        }
    }

    /**
     * Toggle regions layer visibility
     */
    toggleVisibility() {
        this.isVisible = !this.isVisible;
        
        if (this.isVisible) {
            this.map.addLayer(this.regionsLayer);
        } else {
            this.map.removeLayer(this.regionsLayer);
        }
        
        const checkbox = document.getElementById('chkShowRegions');
        if (checkbox) {
            checkbox.checked = this.isVisible;
        }
        
        console.log(`Regions layer ${this.isVisible ? 'shown' : 'hidden'}`);
    }

    /**
     * Clear all regions from localStorage
     */
    clearAllRegions() {
        if (this.regions.size === 0) {
            this.notificationCallback('No regions to clear', 'info');
            return;
        }

        if (confirm(`Are you sure you want to delete all ${this.regions.size} regions?`)) {
            try {
                // Remove all regions from map
                for (const [regionId, region] of this.regions) {
                    this.regionsLayer.removeLayer(region.polygon);
                    this.regionsLayer.removeLayer(region.label);
                }
                
                // Clear regions map
                this.regions.clear();
                
                // Clear localStorage
                localStorage.removeItem('gamePlay_regions');
                
                this.notificationCallback('All regions cleared successfully', 'success');
            } catch (error) {
                console.error('Error clearing regions:', error);
                this.notificationCallback('Error clearing regions', 'error');
            }
        }
    }

    /**
     * Select force for region creation
     */
    selectForce(force) {
        this.selectedForce = force;
        
        // Update button states
        const blueBtn = document.getElementById('btnSelectBlue');
        const foxBtn = document.getElementById('btnSelectFox');
        
        if (blueBtn && foxBtn) {
            if (force === 'blue') {
                blueBtn.classList.add('active');
                foxBtn.classList.remove('active');
            } else {
                foxBtn.classList.add('active');
                blueBtn.classList.remove('active');
            }
        }
        
        const forceName = force === 'blue' ? 'Blue Land' : 'Fox Land';
        this.notificationCallback(`Selected ${forceName} for region creation`, 'info');
        
        // Reset drawing mode if active to allow new region creation
        if (this.isDrawingMode) {
            this.cancelDrawing();
        }
    }

    /**
     * Update region count in UI
     */
    updateRegionCount() {
        const countElement = document.getElementById('metric-sectors');
        if (countElement) {
            countElement.textContent = this.regions.size;
        }
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
        this.cancelDrawing();
        
        // Remove all regions
        for (const [regionId, region] of this.regions) {
            this.regionsLayer.removeLayer(region.polygon);
            this.regionsLayer.removeLayer(region.label);
        }
        
        this.regions.clear();
        
        if (this.drawingLayer) {
            this.map.removeLayer(this.drawingLayer);
        }
        
        if (this.regionsLayer) {
            this.map.removeLayer(this.regionsLayer);
        }
    }
}

// Export for use in other modules
if (typeof module !== 'undefined' && module.exports) {
    module.exports = RegionManager;
}
