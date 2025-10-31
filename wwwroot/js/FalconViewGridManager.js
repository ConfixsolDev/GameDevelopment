/**
 * Falcon View Grid Manager
 * Military Grid Reference System (MGRS) overlay for tactical mapping
 * Mimics Falcon View GIS grid system
 */

class FalconViewGridManager {
    constructor(map) {
        this.map = map;
        this.gridLayer = null;
        this.gridLines = [];
        this.gridLabels = [];
        this.enabled = false;
        this.gridSize = 1000; // Grid size in meters (adjustable)
        this.gridColor = '#00bcd4'; // Primary cyan
        this.gridOpacity = 0.4;
        this.labelColor = '#00bcd4';
        this.labelOpacity = 0.8;
        this.gridSpacing = 0.01; // Default spacing in degrees
        
        console.log('🎯 Falcon View Grid Manager initialized');
    }

    /**
     * Enable/disable Falcon View grid overlay
     */
    toggle() {
        if (this.enabled) {
            this.disable();
        } else {
            this.enable();
        }
    }

    /**
     * Enable Falcon View grid
     */
    enable() {
        if (this.enabled) {
            console.log('⚠️ Falcon View grid already enabled');
            return;
        }

        console.log('🎯 Enabling Falcon View grid...');
        this.enabled = true;
        
        // Create grid layer if it doesn't exist
        if (!this.gridLayer) {
            this.gridLayer = L.featureGroup();
        }

        // Draw the grid
        this.drawGrid();

        // Add grid layer to map
        this.gridLayer.addTo(this.map);

        // Update grid on map move/zoom
        this.map.on('moveend', this.onMapMove, this);
        this.map.on('zoomend', this.onMapZoom, this);

        console.log('✅ Falcon View grid enabled');
        
        // Show notification
        if (typeof showNotification === 'function') {
            showNotification('Falcon View Grid: ON', 'success');
        }
    }

    /**
     * Disable Falcon View grid
     */
    disable() {
        if (!this.enabled) {
            console.log('⚠️ Falcon View grid already disabled');
            return;
        }

        console.log('🎯 Disabling Falcon View grid...');
        this.enabled = false;

        // Remove grid layer from map
        if (this.gridLayer && this.map.hasLayer(this.gridLayer)) {
            this.map.removeLayer(this.gridLayer);
        }

        // Clear grid elements
        this.clearGrid();

        // Remove event listeners
        this.map.off('moveend', this.onMapMove, this);
        this.map.off('zoomend', this.onMapZoom, this);

        console.log('✅ Falcon View grid disabled');
        
        // Show notification
        if (typeof showNotification === 'function') {
            showNotification('Falcon View Grid: OFF', 'info');
        }
    }

    /**
     * Handle map move event
     */
    onMapMove() {
        if (this.enabled) {
            this.updateGrid();
        }
    }

    /**
     * Handle map zoom event
     */
    onMapZoom() {
        if (this.enabled) {
            // Adjust grid spacing based on zoom level
            this.adjustGridSpacing();
            this.updateGrid();
        }
    }

    /**
     * Adjust grid spacing based on zoom level
     */
    adjustGridSpacing() {
        const zoom = this.map.getZoom();
        
        // Adjust grid spacing based on zoom level for optimal visibility
        if (zoom <= 8) {
            this.gridSpacing = 1.0;   // Large grid for world view
        } else if (zoom <= 10) {
            this.gridSpacing = 0.5;   // Medium-large grid
        } else if (zoom <= 12) {
            this.gridSpacing = 0.1;   // Medium grid
        } else if (zoom <= 14) {
            this.gridSpacing = 0.05;  // Small-medium grid
        } else if (zoom <= 16) {
            this.gridSpacing = 0.01;  // Small grid
        } else {
            this.gridSpacing = 0.005; // Very small grid for detailed view
        }

        console.log(`🎯 Grid spacing adjusted to ${this.gridSpacing} degrees for zoom level ${zoom}`);
    }

    /**
     * Update grid (clear and redraw)
     */
    updateGrid() {
        this.clearGrid();
        this.drawGrid();
    }

    /**
     * Clear all grid elements
     */
    clearGrid() {
        if (this.gridLayer) {
            this.gridLayer.clearLayers();
        }
        this.gridLines = [];
        this.gridLabels = [];
    }

    /**
     * Draw Falcon View grid
     */
    drawGrid() {
        const bounds = this.map.getBounds();
        const zoom = this.map.getZoom();

        // Calculate grid boundaries
        const south = Math.floor(bounds.getSouth() / this.gridSpacing) * this.gridSpacing;
        const north = Math.ceil(bounds.getNorth() / this.gridSpacing) * this.gridSpacing;
        const west = Math.floor(bounds.getWest() / this.gridSpacing) * this.gridSpacing;
        const east = Math.ceil(bounds.getEast() / this.gridSpacing) * this.gridSpacing;

        // Limit number of grid lines for performance
        const maxLines = 50;
        const latLines = Math.ceil((north - south) / this.gridSpacing);
        const lngLines = Math.ceil((east - west) / this.gridSpacing);

        if (latLines > maxLines || lngLines > maxLines) {
            console.warn('⚠️ Too many grid lines, increasing spacing...');
            this.gridSpacing *= 2;
            return this.drawGrid();
        }

        // Draw vertical grid lines (longitude)
        for (let lng = west; lng <= east; lng += this.gridSpacing) {
            this.drawVerticalLine(lng, south, north);
        }

        // Draw horizontal grid lines (latitude)
        for (let lat = south; lat <= north; lat += this.gridSpacing) {
            this.drawHorizontalLine(lat, west, east);
        }

        // Draw grid labels
        this.drawGridLabels(south, north, west, east);

        console.log(`🎯 Falcon View grid drawn: ${latLines}x${lngLines} cells`);
    }

    /**
     * Draw vertical grid line
     */
    drawVerticalLine(lng, south, north) {
        const line = L.polyline(
            [[south, lng], [north, lng]],
            {
                color: this.gridColor,
                weight: 1,
                opacity: this.gridOpacity,
                interactive: false,
                className: 'falcon-grid-line falcon-grid-vertical'
            }
        );

        line.addTo(this.gridLayer);
        this.gridLines.push(line);
    }

    /**
     * Draw horizontal grid line
     */
    drawHorizontalLine(lat, west, east) {
        const line = L.polyline(
            [[lat, west], [lat, east]],
            {
                color: this.gridColor,
                weight: 1,
                opacity: this.gridOpacity,
                interactive: false,
                className: 'falcon-grid-line falcon-grid-horizontal'
            }
        );

        line.addTo(this.gridLayer);
        this.gridLines.push(line);
    }

    /**
     * Draw grid labels with coordinates
     */
    drawGridLabels(south, north, west, east) {
        const bounds = this.map.getBounds();
        const zoom = this.map.getZoom();

        // Only show labels at appropriate zoom levels
        if (zoom < 10) {
            return; // Too zoomed out for labels
        }

        // Calculate label spacing (show fewer labels at lower zoom)
        const labelSpacing = zoom < 12 ? this.gridSpacing * 2 : this.gridSpacing;

        // Draw longitude labels (top and bottom)
        for (let lng = west; lng <= east; lng += labelSpacing) {
            // Top label
            const topLabel = this.createGridLabel(
                [north - this.gridSpacing * 0.1, lng],
                this.formatCoordinate(lng, 'lng')
            );
            topLabel.addTo(this.gridLayer);
            this.gridLabels.push(topLabel);

            // Bottom label
            const bottomLabel = this.createGridLabel(
                [south + this.gridSpacing * 0.1, lng],
                this.formatCoordinate(lng, 'lng')
            );
            bottomLabel.addTo(this.gridLayer);
            this.gridLabels.push(bottomLabel);
        }

        // Draw latitude labels (left and right)
        for (let lat = south; lat <= north; lat += labelSpacing) {
            // Left label
            const leftLabel = this.createGridLabel(
                [lat, west + this.gridSpacing * 0.1],
                this.formatCoordinate(lat, 'lat')
            );
            leftLabel.addTo(this.gridLayer);
            this.gridLabels.push(leftLabel);

            // Right label
            const rightLabel = this.createGridLabel(
                [lat, east - this.gridSpacing * 0.1],
                this.formatCoordinate(lat, 'lat')
            );
            rightLabel.addTo(this.gridLayer);
            this.gridLabels.push(rightLabel);
        }
    }

    /**
     * Create a grid label marker
     */
    createGridLabel(latlng, text) {
        const icon = L.divIcon({
            className: 'falcon-grid-label',
            html: `<div class="falcon-grid-label-text">${text}</div>`,
            iconSize: [60, 20],
            iconAnchor: [30, 10]
        });

        return L.marker(latlng, {
            icon: icon,
            interactive: false
        });
    }

    /**
     * Format coordinate for display
     */
    formatCoordinate(value, type) {
        const absValue = Math.abs(value);
        const degrees = Math.floor(absValue);
        const minutes = Math.floor((absValue - degrees) * 60);
        const seconds = Math.floor(((absValue - degrees) * 60 - minutes) * 60);

        let direction = '';
        if (type === 'lat') {
            direction = value >= 0 ? 'N' : 'S';
        } else {
            direction = value >= 0 ? 'E' : 'W';
        }

        // Use decimal degrees for simplicity at different zoom levels
        if (this.gridSpacing >= 0.1) {
            return `${value.toFixed(1)}°${direction}`;
        } else if (this.gridSpacing >= 0.01) {
            return `${value.toFixed(2)}°${direction}`;
        } else {
            return `${value.toFixed(3)}°${direction}`;
        }
    }

    /**
     * Convert lat/lng to MGRS format (simplified version)
     */
    toMGRS(lat, lng) {
        // This is a simplified MGRS representation
        // Full MGRS would require UTM zone calculation
        const latBand = this.getMGRSLatBand(lat);
        const zone = this.getUTMZone(lng);
        
        return `${zone}${latBand}`;
    }

    /**
     * Get MGRS latitude band letter
     */
    getMGRSLatBand(lat) {
        const bands = 'CDEFGHJKLMNPQRSTUVWX';
        const index = Math.floor((lat + 80) / 8);
        return bands[Math.max(0, Math.min(index, bands.length - 1))];
    }

    /**
     * Get UTM zone number
     */
    getUTMZone(lng) {
        return Math.floor((lng + 180) / 6) + 1;
    }

    /**
     * Set grid color
     */
    setGridColor(color) {
        this.gridColor = color;
        if (this.enabled) {
            this.updateGrid();
        }
        console.log(`🎯 Grid color set to: ${color}`);
    }

    /**
     * Set grid opacity
     */
    setGridOpacity(opacity) {
        this.gridOpacity = opacity;
        if (this.enabled) {
            this.updateGrid();
        }
        console.log(`🎯 Grid opacity set to: ${opacity}`);
    }

    /**
     * Get grid status
     */
    isEnabled() {
        return this.enabled;
    }
}

// Make globally available
window.FalconViewGridManager = FalconViewGridManager;

console.log('✅ Falcon View Grid Manager loaded');

