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
        this.gridSize = 1000; // Default grid size in meters (cell size)
        this.gridColor = '#00bcd4'; // Primary cyan
        this.gridOpacity = 0.4;
        this.labelColor = '#00bcd4';
        this.labelOpacity = 0.8;
        // We now render in projected meters (EPSG:3857) so spacing is fixed in meters
        this.gridSpacingMeters = this.gridSize; // meters between grid lines
        
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
            this.updateGrid();
        }
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
        const crs = this.map.options.crs || L.CRS.EPSG3857;

        // Project bounds to meters (Spherical Mercator)
        const sw = crs.project(bounds.getSouthWest());
        const ne = crs.project(bounds.getNorthEast());

        // Align start positions to the grid spacing in meters
        const startX = Math.floor(sw.x / this.gridSpacingMeters) * this.gridSpacingMeters;
        const endX = Math.ceil(ne.x / this.gridSpacingMeters) * this.gridSpacingMeters;
        const startY = Math.floor(sw.y / this.gridSpacingMeters) * this.gridSpacingMeters;
        const endY = Math.ceil(ne.y / this.gridSpacingMeters) * this.gridSpacingMeters;

        // Safety: cap number of lines for performance and maintain visibility at any zoom
        const spanX = endX - startX;
        const spanY = endY - startY;
        const maxLines = 120; // total lines target per axis (keeps perf OK)
        let stepMeters = this.gridSpacingMeters;

        // If too many lines would be drawn, increase step to fit within maxLines
        const neededX = Math.ceil(spanX / stepMeters) + 1;
        const neededY = Math.ceil(spanY / stepMeters) + 1;
        if (neededX > maxLines || neededY > maxLines) {
            const maxSpan = Math.max(spanX, spanY);
            stepMeters = Math.ceil(maxSpan / maxLines);
            // Round stepMeters to nearest 100 meters for neat grid
            stepMeters = Math.max(100, Math.ceil(stepMeters / 100) * 100);
            console.warn(`⚠️ Grid too dense at this zoom. Using coarser spacing: ${stepMeters}m`);
        }

        // Draw vertical lines (constant meter spacing)
        for (let x = startX; x <= endX; x += stepMeters) {
            const southLatLng = crs.unproject(L.point(x, startY));
            const northLatLng = crs.unproject(L.point(x, endY));
            this.drawVerticalLineLatLng(southLatLng, northLatLng);
        }

        // Draw horizontal lines
        for (let y = startY; y <= endY; y += stepMeters) {
            const westLatLng = crs.unproject(L.point(startX, y));
            const eastLatLng = crs.unproject(L.point(endX, y));
            this.drawHorizontalLineLatLng(westLatLng, eastLatLng);
        }
        const xLines = Math.ceil(spanX / stepMeters) + 1;
        const yLines = Math.ceil(spanY / stepMeters) + 1;
        console.log(`🎯 Falcon View grid drawn (meters): ${xLines}x${yLines} cells of ${stepMeters}m`);
    }

    /**
     * Draw vertical grid line (LatLng endpoints)
     */
    drawVerticalLineLatLng(southLatLng, northLatLng) {
        const line = L.polyline(
            [southLatLng, northLatLng],
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
     * Draw horizontal grid line (LatLng endpoints)
     */
    drawHorizontalLineLatLng(westLatLng, eastLatLng) {
        const line = L.polyline(
            [westLatLng, eastLatLng],
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

