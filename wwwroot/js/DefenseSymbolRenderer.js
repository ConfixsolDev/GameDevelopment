/**
 * Defense Symbol Renderer
 * Renders NATO-style defense symbols on Leaflet maps
 * Supports kill zones, minefields, obstacles, withdrawal routes, etc.
 */

class DefenseSymbolRenderer {
    constructor() {
        this.symbols = this.initializeDefenseSymbols();
    }

    /**
     * Initialize NATO APP-6 defense symbol mappings
     */
    initializeDefenseSymbols() {
        return {
            // Kill Zones
            killZones: {
                'primary': {
                    symbol: '⊗',
                    color: '#cc0000',
                    name: 'Primary Kill Zone',
                    fillOpacity: 0.3,
                    strokeWidth: 3,
                    strokeDashArray: '10, 5'
                },
                'secondary': {
                    symbol: '⊙',
                    color: '#ff6600',
                    name: 'Secondary Kill Zone',
                    fillOpacity: 0.2,
                    strokeWidth: 2,
                    strokeDashArray: '5, 5'
                },
                'engagement': {
                    symbol: '◎',
                    color: '#ff9900',
                    name: 'Engagement Area',
                    fillOpacity: 0.15,
                    strokeWidth: 2,
                    strokeDashArray: '8, 4'
                }
            },

            // Minefields (Military mine symbols - larger sizes)
            minefields: {
                'antipersonnel': {
                    symbol: '⊗', // Circle with X (standard mine symbol)
                    color: '#cc0000',
                    name: 'Anti-Personnel Minefield',
                    radius: 16,
                    strokeWidth: 2,
                    iconSize: 24
                },
                'antitank': {
                    symbol: '⧉', // Diamond with X for larger mines
                    color: '#990000',
                    name: 'Anti-Tank Minefield',
                    radius: 18,
                    strokeWidth: 2,
                    iconSize: 28
                },
                'mixed': {
                    symbol: '☠', // Skull for mixed/dangerous minefields
                    color: '#660000',
                    name: 'Mixed Minefield',
                    radius: 20,
                    strokeWidth: 3,
                    iconSize: 32
                }
            },

            // Obstacles
            obstacles: {
                'wire': {
                    symbol: '▬▬▬',
                    color: '#666666',
                    name: 'Wire Obstacle',
                    strokeWidth: 3,
                    strokeDashArray: '15, 5, 5, 5'
                },
                'abatis': {
                    symbol: '🌲',
                    color: '#663300',
                    name: 'Abatis',
                    strokeWidth: 4,
                    strokeDashArray: '10, 10'
                },
                'trench': {
                    symbol: '━━━',
                    color: '#996633',
                    name: 'Trench',
                    strokeWidth: 4,
                    strokeDashArray: '20, 5'
                },
                'tankditch': {
                    symbol: '▭▭▭',
                    color: '#8B4513',
                    name: 'Tank Ditch',
                    strokeWidth: 5,
                    strokeDashArray: '25, 5'
                }
            },

            // Defensive Positions
            positions: {
                'bunker': {
                    symbol: '⌂',
                    color: '#333333',
                    name: 'Bunker',
                    size: 24
                },
                'foxhole': {
                    symbol: '○',
                    color: '#666666',
                    name: 'Foxhole',
                    size: 16
                },
                'strongpoint': {
                    symbol: '⬟',
                    color: '#000000',
                    name: 'Strongpoint',
                    size: 28
                },
                'checkpoint': {
                    symbol: '▣',
                    color: '#0066cc',
                    name: 'Checkpoint',
                    size: 20
                }
            },

            // Defense Zones (New Implementation)
            defenseZones: {
                'primary': {
                    symbol: '🛡️',
                    color: '#0066cc', // Blue color coding
                    name: 'Primary Defense Zone',
                    shape: 'oval',
                    fillOpacity: 0.2,
                    strokeWidth: 3,
                    strokeDashArray: '8, 4',
                    parallelLines: true,
                    tokenSize: 24
                },
                'secondary': {
                    symbol: '🛡️',
                    color: '#0088dd', // Lighter blue
                    name: 'Secondary Defense Zone',
                    shape: 'oval',
                    fillOpacity: 0.15,
                    strokeWidth: 2,
                    strokeDashArray: '6, 3',
                    parallelLines: true,
                    tokenSize: 20
                },
                'support': {
                    symbol: '🛡️',
                    color: '#004499', // Darker blue
                    name: 'Support Defense Zone',
                    shape: 'circle',
                    fillOpacity: 0.25,
                    strokeWidth: 2,
                    strokeDashArray: '5, 5',
                    parallelLines: true,
                    tokenSize: 18
                }
            },

            // Withdrawal Routes
            withdrawalRoutes: {
                'primary': {
                    symbol: '⇐',
                    color: '#0066cc',
                    name: 'Primary Withdrawal Route',
                    strokeWidth: 4,
                    strokeDashArray: '15, 10',
                    arrow: true
                },
                'alternate': {
                    symbol: '⇐',
                    color: '#00cc00',
                    name: 'Alternate Withdrawal Route',
                    strokeWidth: 3,
                    strokeDashArray: '10, 10',
                    arrow: true
                },
                'emergency': {
                    symbol: '⇐',
                    color: '#ff6600',
                    name: 'Emergency Withdrawal Route',
                    strokeWidth: 3,
                    strokeDashArray: '5, 5',
                    arrow: true
                }
            },

            // Defensive Lines
            defensiveLines: {
                'feba': {
                    symbol: '━━━',
                    color: '#cc0000',
                    name: 'Forward Edge of Battle Area (FEBA)',
                    strokeWidth: 5,
                    strokeDashArray: 'none'
                },
                'mainline': {
                    symbol: '━━━',
                    color: '#990000',
                    name: 'Main Line of Defense',
                    strokeWidth: 4,
                    strokeDashArray: 'none'
                },
                'secondary': {
                    symbol: '- - -',
                    color: '#660000',
                    name: 'Secondary Defense Line',
                    strokeWidth: 3,
                    strokeDashArray: '20, 10'
                }
            }
        };
    }

    /**
     * Create kill zone polygon on map
     */
    createKillZone(coordinates, type = 'primary', options = {}) {
        const killZoneConfig = this.symbols.killZones[type];
        
        const polygon = L.polygon(coordinates, {
            color: killZoneConfig.color,
            fillColor: killZoneConfig.color,
            fillOpacity: killZoneConfig.fillOpacity,
            weight: killZoneConfig.strokeWidth,
            dashArray: killZoneConfig.strokeDashArray,
            className: `defense-killzone killzone-${type}`,
            ...options
        });

        // Add label marker at center
        const center = this.getPolygonCenter(coordinates);
        const label = this.createDefenseLabel(center, killZoneConfig.symbol, killZoneConfig.name);

        return { polygon, label };
    }

    /**
     * Create minefield markers on map with mine icons
     */
    createMinefield(coordinates, type = 'mixed', options = {}) {
        const minefieldConfig = this.symbols.minefields[type];
        
        // Add type to config for marker styling
        minefieldConfig.type = type;
        
        // Create individual mine markers with icons
        const markers = this.createMinefieldMarkers(coordinates, minefieldConfig);
        
        // Create central mine icon at the center of the minefield
        const centerMineIcon = this.createCentralMineIcon(coordinates, minefieldConfig);
        
        return { markers, centerIcon: centerMineIcon };
    }

    /**
     * Create obstacle line on map
     */
    createObstacle(coordinates, type = 'wire', options = {}) {
        const obstacleConfig = this.symbols.obstacles[type];
        
        const polyline = L.polyline(coordinates, {
            color: obstacleConfig.color,
            weight: obstacleConfig.strokeWidth,
            dashArray: obstacleConfig.strokeDashArray,
            className: `defense-obstacle obstacle-${type}`,
            ...options
        });

        return polyline;
    }

    /**
     * Create defensive position marker
     */
    createDefensivePosition(latlng, type = 'foxhole', options = {}) {
        const positionConfig = this.symbols.positions[type];
        
        const html = `
            <div class="defense-position position-${type}">
                <div class="position-symbol" style="font-size: ${positionConfig.size}px; color: ${positionConfig.color};">
                    ${positionConfig.symbol}
                </div>
            </div>
        `;

        const icon = L.divIcon({
            html: html,
            className: 'defense-position-marker',
            iconSize: [positionConfig.size + 10, positionConfig.size + 10],
            iconAnchor: [(positionConfig.size + 10) / 2, (positionConfig.size + 10) / 2]
        });

        return L.marker(latlng, { icon, ...options });
    }

    /**
     * Create withdrawal route polyline with arrows
     */
    createWithdrawalRoute(coordinates, type = 'primary', options = {}) {
        const routeConfig = this.symbols.withdrawalRoutes[type];
        
        const polyline = L.polyline(coordinates, {
            color: routeConfig.color,
            weight: routeConfig.strokeWidth,
            dashArray: routeConfig.strokeDashArray,
            className: `defense-withdrawal withdrawal-${type}`,
            ...options
        });

        // Add arrow markers along the route
        const arrows = this.createRouteArrows(coordinates, routeConfig);

        return { polyline, arrows };
    }

    /**
     * Create defensive line polyline
     */
    createDefensiveLine(coordinates, type = 'feba', options = {}) {
        const lineConfig = this.symbols.defensiveLines[type];
        
        const polyline = L.polyline(coordinates, {
            color: lineConfig.color,
            weight: lineConfig.strokeWidth,
            dashArray: lineConfig.strokeDashArray === 'none' ? null : lineConfig.strokeDashArray,
            className: `defense-line line-${type}`,
            ...options
        });

        return polyline;
    }

    /**
     * Create defense label marker
     */
    createDefenseLabel(latlng, symbol, name) {
        const html = `
            <div class="defense-label">
                <div class="defense-symbol">${symbol}</div>
                <div class="defense-name">${name}</div>
            </div>
        `;

        const icon = L.divIcon({
            html: html,
            className: 'defense-label-marker',
            iconSize: [60, 40],
            iconAnchor: [30, 20]
        });

        return L.marker(latlng, { icon });
    }

    /**
     * Create minefield markers with mine icons
     */
    createMinefieldMarkers(coordinates, config) {
        const markers = [];
        
        // Create markers along the polygon perimeter and inside
        const numMarkers = Math.max(5, Math.floor(coordinates.length * 2)); // More markers for better coverage
        
        for (let i = 0; i < numMarkers; i++) {
            let latlng;
            
            if (i < coordinates.length) {
                // Place markers on polygon vertices
                latlng = L.latLng(coordinates[i][0], coordinates[i][1]);
            } else {
                // Place additional markers inside the polygon
                const center = this.getPolygonCenter(coordinates);
                const randomOffset = 0.001; // Small random offset for visual variety
                latlng = L.latLng(
                    center[0] + (Math.random() - 0.5) * randomOffset,
                    center[1] + (Math.random() - 0.5) * randomOffset
                );
            }
            
            // Create simple, visible mine icons
            let mineSymbol = '';
            let backgroundColor = '';
            
            if (config.type === 'antipersonnel') {
                mineSymbol = '●';
                backgroundColor = config.color;
            } else if (config.type === 'antitank') {
                mineSymbol = '◆';
                backgroundColor = config.color;
            } else {
                mineSymbol = '!';
                backgroundColor = config.color;
            }

            const html = `
                <div style="
                    width: ${config.iconSize}px;
                    height: ${config.iconSize}px;
                    background: ${backgroundColor};
                    border: 3px solid #000;
                    border-radius: 50%;
                    display: flex;
                    align-items: center;
                    justify-content: center;
                    color: #000;
                    font-weight: bold;
                    font-size: ${config.iconSize * 0.6}px;
                    text-shadow: 1px 1px 0px #fff;
                    box-shadow: 0 3px 8px rgba(0, 0, 0, 0.7);
                    cursor: pointer;
                ">
                    ${mineSymbol}
                </div>
            `;

            const icon = L.divIcon({
                html: html,
                className: 'minefield-icon-marker-container',
                iconSize: [config.iconSize + 4, config.iconSize + 4],
                iconAnchor: [(config.iconSize + 4) / 2, (config.iconSize + 4) / 2]
            });

            const marker = L.marker(latlng, { 
                icon: icon,
                zIndexOffset: 1500 // Higher z-index to ensure minefields are visible
            });

            // Add click handler for minefield info
            marker.on('click', () => {
                console.log(`💣 Minefield clicked: ${config.name}`);
                // TODO: Show minefield info panel
            });

            markers.push(marker);
        }
        
        return markers;
    }

    /**
     * Create central mine icon at the center of the minefield
     */
    createCentralMineIcon(coordinates, config) {
        const center = this.getPolygonCenter(coordinates);
        const centerLatLng = L.latLng(center[0], center[1]);
        
        // Create a larger, more prominent central mine icon
        const centerIconSize = config.iconSize + 8; // Make central icon larger
        
        let mineSymbol = '';
        let backgroundColor = '';
        
        if (config.type === 'antipersonnel') {
            mineSymbol = '●';
            backgroundColor = config.color;
        } else if (config.type === 'antitank') {
            mineSymbol = '◆';
            backgroundColor = config.color;
        } else {
            mineSymbol = '!';
            backgroundColor = config.color;
        }

        const html = `
            <div style="
                width: ${centerIconSize}px;
                height: ${centerIconSize}px;
                background: ${backgroundColor};
                border: 4px solid #000;
                border-radius: 50%;
                display: flex;
                align-items: center;
                justify-content: center;
                color: #000;
                font-weight: bold;
                font-size: ${centerIconSize * 0.6}px;
                text-shadow: 2px 2px 0px #fff;
                box-shadow: 0 4px 12px rgba(0, 0, 0, 0.8);
                cursor: pointer;
            ">
                ${mineSymbol}
            </div>
        `;

        const icon = L.divIcon({
            html: html,
            className: 'minefield-central-icon-container',
            iconSize: [centerIconSize + 6, centerIconSize + 6],
            iconAnchor: [(centerIconSize + 6) / 2, (centerIconSize + 6) / 2]
        });

        const marker = L.marker(centerLatLng, { 
            icon: icon,
            zIndexOffset: 2000 // Higher z-index to ensure it's visible above other markers
        });

        // Add click handler for central mine icon
        marker.on('click', () => {
            console.log(`💣 Central minefield icon clicked: ${config.name}`);
            // TODO: Show minefield info panel
        });

        return marker;
    }

    /**
     * Create arrow markers for withdrawal routes
     */
    createRouteArrows(coordinates, config) {
        const arrows = [];
        const numArrows = Math.floor(coordinates.length / 3);
        
        for (let i = 1; i <= numArrows; i++) {
            const idx = Math.floor((coordinates.length - 1) * i / (numArrows + 1));
            const point = coordinates[idx];
            const prevPoint = coordinates[idx - 1];
            
            const angle = this.calculateAngle(prevPoint, point);
            
            const html = `
                <div class="withdrawal-arrow" style="transform: rotate(${angle}deg); color: ${config.color};">
                    ${config.symbol}
                </div>
            `;

            const icon = L.divIcon({
                html: html,
                className: 'withdrawal-arrow-marker',
                iconSize: [24, 24],
                iconAnchor: [12, 12]
            });

            arrows.push(L.marker(point, { icon }));
        }
        
        return arrows;
    }

    /**
     * Get center of polygon
     */
    getPolygonCenter(coordinates) {
        let lat = 0, lng = 0;
        coordinates.forEach(coord => {
            lat += coord[0];
            lng += coord[1];
        });
        return [lat / coordinates.length, lng / coordinates.length];
    }

    /**
     * Calculate angle between two points
     */
    calculateAngle(point1, point2) {
        const dx = point2[1] - point1[1];
        const dy = point2[0] - point1[0];
        return Math.atan2(dy, dx) * 180 / Math.PI;
    }

    /**
     * Create Defense Zone with oval/circular shape and parallel lines
     * @param {Array} coordinates - Array of [lat, lng] coordinates defining the zone boundary
     * @param {string} type - Defense zone type ('primary', 'secondary', 'support')
     * @param {Object} options - Additional options (tokenId, tokenName, etc.)
     * @returns {Object} Defense zone group containing shape, lines, and token
     */
    createDefenseZone(coordinates, type = 'primary', options = {}) {
        const config = this.symbols.defenseZones[type] || this.symbols.defenseZones.primary;
        
        // Create defense zone group
        const defenseZoneGroup = L.layerGroup();
        
        // Convert coordinates to LatLng points
        const points = coordinates.map(coord => L.latLng(coord[0], coord[1]));
        
        // Calculate center point for token placement
        const center = this.getPolygonCenter(coordinates);
        const centerLatLng = L.latLng(center[0], center[1]);
        
        // 1. Create the main shape (oval or circle)
        let shape;
        if (config.shape === 'circle') {
            // Create circle from center and radius
            const radius = this.calculateCircleRadius(points);
            shape = L.circle(centerLatLng, {
                radius: radius,
                color: config.color,
                weight: config.strokeWidth,
                opacity: 0.8,
                fillColor: config.color,
                fillOpacity: config.fillOpacity,
                dashArray: config.strokeDashArray,
                className: 'defense-zone-shape'
            });
        } else {
            // Create oval/polygon shape
            shape = L.polygon(points, {
                color: config.color,
                weight: config.strokeWidth,
                opacity: 0.8,
                fillColor: config.color,
                fillOpacity: config.fillOpacity,
                dashArray: config.strokeDashArray,
                className: 'defense-zone-shape'
            });
        }
        
        defenseZoneGroup.addLayer(shape);
        
        // 2. Add parallel lines across the shape if enabled
        if (config.parallelLines) {
            const parallelLines = this.createParallelLines(points, config);
            parallelLines.forEach(line => {
                defenseZoneGroup.addLayer(line);
            });
        }
        
        // 3. Add defense token at exact center
        const tokenIcon = L.divIcon({
            html: `
                <div class="defense-zone-token" style="
                    background: ${config.color};
                    color: white;
                    width: ${config.tokenSize}px;
                    height: ${config.tokenSize}px;
                    border-radius: 50%;
                    display: flex;
                    align-items: center;
                    justify-content: center;
                    font-size: ${config.tokenSize * 0.6}px;
                    font-weight: bold;
                    border: 2px solid white;
                    box-shadow: 0 2px 6px rgba(0,0,0,0.3);
                ">
                    ${config.symbol}
                </div>
            `,
            className: 'defense-zone-token-marker',
            iconSize: [config.tokenSize + 4, config.tokenSize + 4],
            iconAnchor: [(config.tokenSize + 4) / 2, (config.tokenSize + 4) / 2]
        });
        
        const tokenMarker = L.marker(centerLatLng, {
            icon: tokenIcon,
            zIndexOffset: 2000
        });
        
        defenseZoneGroup.addLayer(tokenMarker);
        
        // Store references for easy access
        defenseZoneGroup.shape = shape;
        defenseZoneGroup.tokenMarker = tokenMarker;
        defenseZoneGroup.config = config;
        defenseZoneGroup.type = type;
        
        // Add click handler for defense zone info
        defenseZoneGroup.on('click', (e) => {
            console.log(`🛡️ Defense Zone clicked: ${config.name} (${type})`);
            // TODO: Show defense zone info panel
        });
        
        return defenseZoneGroup;
    }
    
    /**
     * Create two short parallel lines across the defense zone
     * @param {Array} points - LatLng points defining the zone
     * @param {Object} config - Defense zone configuration
     * @returns {Array} Array of L.polyline objects
     */
    createParallelLines(points, config) {
        const lines = [];
        
        // Calculate zone bounds
        const bounds = L.latLngBounds(points);
        const center = bounds.getCenter();
        const width = bounds.getEast() - bounds.getWest();
        const height = bounds.getNorth() - bounds.getSouth();
        
        // Calculate line length (shorter than zone width)
        const lineLength = Math.min(width, height) * 0.6; // 60% of smaller dimension
        const lineSpacing = Math.min(width, height) * 0.15; // 15% spacing between lines
        
        // Create two short parallel lines across the center of the zone
        const line1Offset = lineSpacing / 2;
        const line2Offset = -lineSpacing / 2;
        
        // Line 1 (top) - short line across center
        const line1Start = L.latLng(center.lat + line1Offset, center.lng - lineLength/2);
        const line1End = L.latLng(center.lat + line1Offset, center.lng + lineLength/2);
        const line1 = L.polyline([line1Start, line1End], {
            color: config.color,
            weight: 3,
            opacity: 0.8,
            className: 'defense-zone-parallel-line'
        });
        
        // Line 2 (bottom) - short line across center
        const line2Start = L.latLng(center.lat + line2Offset, center.lng - lineLength/2);
        const line2End = L.latLng(center.lat + line2Offset, center.lng + lineLength/2);
        const line2 = L.polyline([line2Start, line2End], {
            color: config.color,
            weight: 3,
            opacity: 0.8,
            className: 'defense-zone-parallel-line'
        });
        
        lines.push(line1, line2);
        
        return lines;
    }
    
    /**
     * Calculate radius for circular defense zone
     * @param {Array} points - LatLng points
     * @returns {number} Radius in meters
     */
    calculateCircleRadius(points) {
        if (points.length < 2) return 1000; // Default 1km radius
        
        const bounds = L.latLngBounds(points);
        const center = bounds.getCenter();
        
        // Find the maximum distance from center to any point
        let maxDistance = 0;
        points.forEach(point => {
            const distance = center.distanceTo(point);
            maxDistance = Math.max(maxDistance, distance);
        });
        
        return maxDistance;
    }

    /**
     * Get defense symbol information
     */
    getDefenseSymbolInfo(type, category) {
        const categoryMap = {
            'killzone': this.symbols.killZones,
            'minefield': this.symbols.minefields,
            'obstacle': this.symbols.obstacles,
            'position': this.symbols.positions,
            'withdrawal': this.symbols.withdrawalRoutes,
            'line': this.symbols.defensiveLines,
            'defensezone': this.symbols.defenseZones
        };

        const symbols = categoryMap[category];
        return symbols ? symbols[type] : null;
    }
}

// Create global instance
window.defenseSymbolRenderer = new DefenseSymbolRenderer();

// Export for module use
if (typeof module !== 'undefined' && module.exports) {
    module.exports = DefenseSymbolRenderer;
}

console.log('✅ Defense Symbol Renderer loaded');

