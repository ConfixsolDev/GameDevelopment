/**
 * Defense Symbol Renderer
 * Renders NATO-style defense symbols on Leaflet maps
 * Supports kill zones, minefields, obstacles, withdrawal routes, etc.
 */

class DefenseSymbolRenderer {
    constructor(map = null) {
        this.map = map;
        this.symbols = this.initializeDefenseSymbols();
    }
    
    /**
     * Set map reference for zoom-responsive sizing
     */
    setMap(map) {
        this.map = map;
    }
    
    /**
     * Get color based on force type
     * Blue Land = Blue, Fox Land = Red
     */
    getForceColor(forceType) {
        console.log(`🎨 getForceColor called with: "${forceType}"`);
        
        if (!forceType) {
            console.warn('⚠️ Force type is null/undefined - using default NATO colors');
            return null;
        }
        
        const forceTypeLower = forceType.toLowerCase();
        console.log(`🎨 Force type lowercase: "${forceTypeLower}"`);
        
        if (forceTypeLower.includes('blue')) {
            console.log('✅ Detected Blueland - using BLUE color (#0000ff)');
            return '#0000ff'; // Blue for Blue Land
        } else if (forceTypeLower.includes('fox') || forceTypeLower.includes('red')) {
            console.log('✅ Detected Foxland - using RED color (#ff0000)');
            return '#ff0000'; // Red for Fox Land
        }
        
        // Default to null to use original colors
        console.warn(`⚠️ Force type "${forceType}" not recognized - using default NATO colors`);
        return null;
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

            // Minefields (Modern grid-based design with NATO standard mine icons)
            minefields: {
                'antipersonnel': {
                    symbol: '💣', // Modern mine emoji for fallback
                    color: '#cc0000',
                    name: 'Anti-Personnel Minefield',
                    radius: 16,
                    strokeWidth: 2,
                    iconSize: 28,
                    cellSize: 28
                },
                'antitank': {
                    symbol: '💣', // Modern mine emoji for fallback
                    color: '#990000',
                    name: 'Anti-Tank Minefield',
                    radius: 18,
                    strokeWidth: 2,
                    iconSize: 28,
                    cellSize: 28
                },
                'mixed': {
                    symbol: '💣', // Modern mine emoji for fallback
                    color: '#660000',
                    name: 'Mixed Minefield',
                    radius: 20,
                    strokeWidth: 3,
                    iconSize: 28,
                    cellSize: 28
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
        
        // Get force-based color
        const forceColor = this.getForceColor(options.forceType);
        const color = forceColor || killZoneConfig.color;
        
        console.log(`🎨 Kill zone color: ${color} (force: ${options.forceType})`);
        
        const polygon = L.polygon(coordinates, {
            color: color,
            fillColor: color,
            fillOpacity: killZoneConfig.fillOpacity,
            weight: killZoneConfig.strokeWidth,
            dashArray: killZoneConfig.strokeDashArray,
            className: `defense-killzone killzone-${type}`,
            ...options
        });

        // Add label marker at center
        const center = this.getPolygonCenter(coordinates);
        const label = this.createDefenseLabel(center, killZoneConfig.symbol, killZoneConfig.name, color);

        return { polygon, label };
    }

    /**
     * Create minefield markers on map with mine icons
     */
    createMinefield(coordinates, type = 'mixed', options = {}) {
        const minefieldConfig = this.symbols.minefields[type];
        
        // Get force-based color
        const forceColor = this.getForceColor(options.forceType);
        const color = forceColor || minefieldConfig.color;
        
        console.log(`🎨 Minefield color: ${color} (force: ${options.forceType})`);
        
        // Add type and color to config for marker styling
        minefieldConfig.type = type;
        minefieldConfig.forceColor = color;
        
        // Create individual mine markers with icons
        const markers = this.createMinefieldMarkers(coordinates, minefieldConfig);
        
        // Return only markers, no central icon
        return { markers };
    }

    /**
     * Create obstacle line on map
     */
    createObstacle(coordinates, type = 'wire', options = {}) {
        const obstacleConfig = this.symbols.obstacles[type];
        
        // Get force-based color
        const forceColor = this.getForceColor(options.forceType);
        const color = forceColor || obstacleConfig.color;
        
        console.log(`🎨 Obstacle color: ${color} (force: ${options.forceType})`);
        
        const polyline = L.polyline(coordinates, {
            color: color,
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
        
        // Get force-based color
        const forceColor = this.getForceColor(options.forceType);
        const color = forceColor || positionConfig.color;
        
        console.log(`🎨 Defensive position color: ${color} (force: ${options.forceType})`);
        
        const html = `
            <div class="defense-position position-${type}">
                <div class="position-symbol" style="font-size: ${positionConfig.size}px; color: ${color};">
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
        
        // Get force-based color
        const forceColor = this.getForceColor(options.forceType);
        const color = forceColor || routeConfig.color;
        
        console.log(`🎨 Withdrawal route color: ${color} (force: ${options.forceType})`);
        
        const polyline = L.polyline(coordinates, {
            color: color,
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
        
        // Get force-based color
        const forceColor = this.getForceColor(options.forceType);
        const color = forceColor || lineConfig.color;
        
        console.log(`🎨 Defensive line color: ${color} (force: ${options.forceType})`);
        
        const polyline = L.polyline(coordinates, {
            color: color,
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
    createDefenseLabel(latlng, symbol, name, color = null) {
        // Clean white text and icon with no background
        const textStyle = 'color: white; text-shadow: 2px 2px 4px rgba(0,0,0,0.8);';
        
        const html = `
            <div class="defense-label" style="background: transparent; border: none; padding: 4px;">
                <div class="defense-symbol" style="${textStyle} font-size: 18px; font-weight: bold; text-align: center; margin-bottom: 2px;">${symbol}</div>
                <div class="defense-name" style="${textStyle} font-size: 12px; font-weight: bold; text-align: center; text-transform: uppercase; letter-spacing: 0.5px;">${name}</div>
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
     * Create minefield markers with proper grid line layout
     */
    createMinefieldMarkers(coordinates, config) {
        const markers = [];
        
        // Use force color if available, otherwise use default config color
        const mineColor = config.forceColor || config.color;
        
        // Calculate polygon bounds
        const bounds = L.latLngBounds(coordinates);
        const center = bounds.getCenter();
        
        // Calculate polygon dimensions
        const polygonWidth = bounds.getEast() - bounds.getWest();
        const polygonHeight = bounds.getNorth() - bounds.getSouth();
        
        // Calculate appropriate number of mines based on polygon size
        // Increased minimum spacing between mines (in degrees) for better visibility
        const minSpacing = 0.0015; // Increased spacing for better separation
        
        // Calculate how many mines can fit horizontally
        const maxMinesHorizontal = Math.floor(polygonWidth / minSpacing);
        
        // Use reasonable limits: minimum 3 mines, maximum 12 mines (reduced for better spacing)
        const numMines = Math.max(3, Math.min(12, maxMinesHorizontal));
        
        // Get current map zoom level for responsive sizing
        const currentZoom = this.map ? this.map.getZoom() : 14;
        const cellSize = Math.max(16, Math.min(48, 20 + (currentZoom - 10) * 2)); // Responsive size based on zoom
        const gridRows = 1;
        const gridCols = numMines;
        
        // Calculate spacing between mines
        const horizontalSpacing = polygonWidth / (gridCols + 1);
        const verticalSpacing = polygonHeight / (gridRows + 1);
        
        // Start position (top-left of grid)
        const startLat = bounds.getNorth() - verticalSpacing;
        const startLng = bounds.getWest() + horizontalSpacing;
        
        // Create different mine variations for visual variety
        const mineTypes = ['standard', 'cross', 'diamond', 'dots'];
        
        // Create grid of mines in a straight line
        for (let row = 0; row < gridRows; row++) {
            for (let col = 0; col < gridCols; col++) {
                // Calculate position for this mine
                const lat = startLat - (row * verticalSpacing);
                const lng = startLng + (col * horizontalSpacing);
                
                // Check if this position is within the polygon
                const point = L.latLng(lat, lng);
                if (this.isPointInPolygon(point, coordinates)) {
                    const latlng = L.latLng(lat, lng);
                    
                    // Select different mine type for variety
                    const mineType = mineTypes[col % mineTypes.length];
                    const mineSvg = this.createMineSvg(mineType, mineColor);
                    
                    // Create mine cell with proper styling
            const html = `
                        <div class="minefield-grid-cell" style="
                            width: ${cellSize}px;
                            height: ${cellSize}px;
                            background: #FFFFFF;
                            border: 2px solid #000000;
                            border-radius: 4px;
                    display: flex;
                    align-items: center;
                    justify-content: center;
                            box-shadow: 0 2px 6px rgba(0, 0, 0, 0.3);
                    cursor: pointer;
                            transition: all 0.2s ease;
                            transform-origin: center center;
                        ">
                            <div class="mine-icon-container" style="
                                width: ${cellSize * 0.9}px;
                                height: ${cellSize * 0.9}px;
                                border-radius: 50%;
                                border: 2px solid ${mineColor};
                                background: rgba(255, 255, 255, 0.95);
                                display: flex;
                                align-items: center;
                                justify-content: center;
                                padding: 4px;
                            ">
                                ${mineSvg}
                            </div>
                </div>
            `;

            const icon = L.divIcon({
                html: html,
                        className: 'minefield-modern-marker',
                        iconSize: [cellSize + 4, cellSize + 4],
                        iconAnchor: [(cellSize + 4) / 2, (cellSize + 4) / 2]
            });

            const marker = L.marker(latlng, { 
                icon: icon,
                        zIndexOffset: 1500
                    });

                    // Hover effects disabled to prevent positioning issues
                    // marker.on('mouseover', function(e) { ... });
                    // marker.on('mouseout', function(e) { ... });

            // Add click handler for minefield info
            marker.on('click', () => {
                console.log(`💣 Minefield clicked: ${config.name}`);
                        // Show minefield details
            });

            markers.push(marker);
                }
            }
        }
        
        return markers;
    }
    
    /**
     * Check if a point is inside a polygon
     */
    isPointInPolygon(point, polygon) {
        const x = point.lng;
        const y = point.lat;
        let inside = false;
        
        for (let i = 0, j = polygon.length - 1; i < polygon.length; j = i++) {
            const xi = polygon[i][1];
            const yi = polygon[i][0];
            const xj = polygon[j][1];
            const yj = polygon[j][0];
            
            if (((yi > y) !== (yj > y)) && (x < (xj - xi) * (y - yi) / (yj - yi) + xi)) {
                inside = !inside;
            }
        }
        
        return inside;
    }
    
    /**
     * Create different mine SVG variations
     */
    createMineSvg(mineType, color) {
        const variations = {
            'standard': `
                <svg viewBox='0 0 24 24' xmlns='http://www.w3.org/2000/svg' width='100%' height='100%'>
                    <circle cx='12' cy='12' r='5' fill='${color}'/>
                    <line x1='12' y1='1.5' x2='12' y2='5' stroke='${color}' stroke-width='2'/>
                    <line x1='12' y1='19' x2='12' y2='22.5' stroke='${color}' stroke-width='2'/>
                    <line x1='1.5' y1='12' x2='5' y2='12' stroke='${color}' stroke-width='2'/>
                    <line x1='19' y1='12' x2='22.5' y2='12' stroke='${color}' stroke-width='2'/>
                    <line x1='4.2' y1='4.2' x2='6.6' y2='6.6' stroke='${color}' stroke-width='2'/>
                    <line x1='17.4' y1='17.4' x2='19.8' y2='19.8' stroke='${color}' stroke-width='2'/>
                    <line x1='4.2' y1='19.8' x2='6.6' y2='17.4' stroke='${color}' stroke-width='2'/>
                    <line x1='17.4' y1='6.6' x2='19.8' y2='4.2' stroke='${color}' stroke-width='2'/>
                    <circle cx='15' cy='9' r='1.2' fill='white'/>
                </svg>
            `,
            'cross': `
                <svg viewBox='0 0 24 24' xmlns='http://www.w3.org/2000/svg' width='100%' height='100%'>
                    <circle cx='12' cy='12' r='5' fill='${color}'/>
                    <line x1='12' y1='2' x2='12' y2='22' stroke='${color}' stroke-width='3'/>
                    <line x1='2' y1='12' x2='22' y2='12' stroke='${color}' stroke-width='3'/>
                    <circle cx='12' cy='12' r='2' fill='white'/>
                </svg>
            `,
            'diamond': `
                <svg viewBox='0 0 24 24' xmlns='http://www.w3.org/2000/svg' width='100%' height='100%'>
                    <circle cx='12' cy='12' r='5' fill='${color}'/>
                    <polygon points='12,2 18,8 12,14 6,8' fill='${color}' stroke='${color}' stroke-width='2'/>
                    <circle cx='12' cy='12' r='2' fill='white'/>
                </svg>
            `,
            'dots': `
                <svg viewBox='0 0 24 24' xmlns='http://www.w3.org/2000/svg' width='100%' height='100%'>
                    <circle cx='12' cy='12' r='5' fill='${color}'/>
                    <circle cx='8' cy='8' r='1.5' fill='white'/>
                    <circle cx='16' cy='8' r='1.5' fill='white'/>
                    <circle cx='8' cy='16' r='1.5' fill='white'/>
                    <circle cx='16' cy='16' r='1.5' fill='white'/>
                    <circle cx='12' cy='12' r='2' fill='white'/>
                </svg>
            `
        };
        return variations[mineType] || variations['standard'];
    }

    /**
     * Create central mine icon at the center of the minefield
     * Enhanced with modern minimal design
     */
    createCentralMineIcon(coordinates, config) {
        const center = this.getPolygonCenter(coordinates);
        const centerLatLng = L.latLng(center[0], center[1]);
        
        // Create a larger, more prominent central mine icon with modern design
        const centerCellSize = 40; // Larger than regular cells
        
        // Get minefield type label
        let typeLabel = '';
        if (config.type === 'antipersonnel') {
            typeLabel = ''; // Remove AP label
        } else if (config.type === 'antitank') {
            typeLabel = ''; // Remove AT label
        } else {
            typeLabel = ''; // Remove MX label
        }
        
        // SVG mine icon (NATO standard with spikes)
        const mineSvg = `
            <svg viewBox='0 0 24 24' xmlns='http://www.w3.org/2000/svg' width='100%' height='100%'>
                <circle cx='12' cy='12' r='5' fill='${config.color}'/>
                <line x1='12' y1='1.5' x2='12' y2='5' stroke='${config.color}' stroke-width='2'/>
                <line x1='12' y1='19' x2='12' y2='22.5' stroke='${config.color}' stroke-width='2'/>
                <line x1='1.5' y1='12' x2='5' y2='12' stroke='${config.color}' stroke-width='2'/>
                <line x1='19' y1='12' x2='22.5' y2='12' stroke='${config.color}' stroke-width='2'/>
                <line x1='4.2' y1='4.2' x2='6.6' y2='6.6' stroke='${config.color}' stroke-width='2'/>
                <line x1='17.4' y1='17.4' x2='19.8' y2='19.8' stroke='${config.color}' stroke-width='2'/>
                <line x1='4.2' y1='19.8' x2='6.6' y2='17.4' stroke='${config.color}' stroke-width='2'/>
                <line x1='17.4' y1='6.6' x2='19.8' y2='4.2' stroke='${config.color}' stroke-width='2'/>
                <circle cx='15' cy='9' r='1.2' fill='white'/>
            </svg>
        `;

        const html = `
            <div class="minefield-central-cell" style="
                width: ${centerCellSize}px;
                height: ${centerCellSize}px;
                background: #FFFFFF;
                border: 3px solid #000000;
                border-radius: 6px;
                display: flex;
                flex-direction: column;
                align-items: center;
                justify-content: center;
                box-shadow: 0 4px 12px rgba(0, 0, 0, 0.4);
                cursor: pointer;
                transition: all 0.3s ease;
                position: relative;
            ">
                <div class="mine-icon-container" style="
                    width: ${centerCellSize * 0.9}px;
                    height: ${centerCellSize * 0.9}px;
                    border-radius: 50%;
                    border: 2px solid ${config.color};
                    background: rgba(255, 255, 255, 0.95);
                    display: flex;
                    align-items: center;
                    justify-content: center;
                    padding: 4px;
                ">
                    ${mineSvg}
                </div>
            </div>
        `;

        const icon = L.divIcon({
            html: html,
            className: 'minefield-central-modern-marker',
            iconSize: [centerCellSize + 6, centerCellSize + 6],
            iconAnchor: [(centerCellSize + 6) / 2, (centerCellSize + 6) / 2]
        });

        const marker = L.marker(centerLatLng, { 
            icon: icon,
            zIndexOffset: 2000
        });
        
        // Hover effects disabled to prevent positioning issues
        // marker.on('mouseover', function(e) { ... });
        // marker.on('mouseout', function(e) { ... });

        // Add click handler for central mine icon
        marker.on('click', () => {
            console.log(`💣 Central minefield icon clicked: ${config.name}`);
            // Show minefield info panel with details
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
     * Get display text for defense zone type
     */
    getDefenseZoneTypeText(type) {
        const typeTexts = {
            'primary': 'Primary Defense Zone',
            'secondary': 'Secondary Defense Zone', 
            'support': 'Support Defense Zone'
        };
        return typeTexts[type] || 'Defense Zone';
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
        
        // Get force-based color
        const forceColor = this.getForceColor(options.forceType);
        const zoneColor = forceColor || config.color;
        
        console.log(`🎨 Defense zone color: ${zoneColor} (force: ${options.forceType})`);
        
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
                color: zoneColor,
                weight: config.strokeWidth,
                opacity: 0.8,
                fillColor: zoneColor,
                fillOpacity: config.fillOpacity,
                dashArray: config.strokeDashArray,
                className: 'defense-zone-shape',
                interactive: true // Make sure it's interactive
            });
        } else {
            // Create oval/polygon shape
            shape = L.polygon(points, {
                color: zoneColor,
                weight: config.strokeWidth,
                opacity: 0.8,
                fillColor: zoneColor,
                fillOpacity: config.fillOpacity,
                dashArray: config.strokeDashArray,
                className: 'defense-zone-shape',
                interactive: true // Make sure it's interactive
            });
        }
        
        defenseZoneGroup.addLayer(shape);
        
        // 2. Add parallel lines across the shape if enabled
        if (config.parallelLines) {
            const parallelLines = this.createParallelLines(points, config, zoneColor);
            parallelLines.forEach(line => {
                defenseZoneGroup.addLayer(line);
            });
        }
        
        // 3. Add defense token with zone type text at exact center
        const zoneTypeText = this.getDefenseZoneTypeText(type);
        const tokenIcon = L.divIcon({
            html: `
                <div class="defense-zone-container" style="
                    display: flex;
                    flex-direction: column;
                    align-items: center;
                    justify-content: center;
                ">
                    <div class="defense-zone-token" style="
                        background: ${zoneColor};
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
                        margin-bottom: 2px;
                    ">
                        ${config.symbol}
                    </div>
                    <div class="defense-zone-label" style="
                        background: transparent;
                        border: none;
                        color: white;
                        font-size: 12px;
                        font-weight: bold;
                        text-align: center;
                        text-transform: uppercase;
                        letter-spacing: 0.5px;
                        white-space: nowrap;
                        text-shadow: 2px 2px 4px rgba(0,0,0,0.8);
                    ">
                        ${zoneTypeText}
                    </div>
                </div>
            `,
            className: 'defense-zone-token-marker',
            iconSize: [config.tokenSize + 20, config.tokenSize + 20],
            iconAnchor: [(config.tokenSize + 20) / 2, (config.tokenSize + 20) / 2]
        });
        
        const tokenMarker = L.marker(centerLatLng, {
            icon: tokenIcon,
            zIndexOffset: 2000,
            interactive: true
        });
        
        defenseZoneGroup.addLayer(tokenMarker);
        
        // Store references for easy access
        defenseZoneGroup.shape = shape;
        defenseZoneGroup.tokenMarker = tokenMarker;
        defenseZoneGroup.config = config;
        defenseZoneGroup.type = type;
        
        // Note: Click and contextmenu handlers are added by DefensePlanningManager
        
        return defenseZoneGroup;
    }
    
    /**
     * Create two short parallel lines across the defense zone
     * @param {Array} points - LatLng points defining the zone
     * @param {Object} config - Defense zone configuration
     * @param {string} lineColor - Optional color override for force-based coloring
     * @returns {Array} Array of L.polyline objects
     */
    createParallelLines(points, config, lineColor = null) {
        const lines = [];
        
        // Use provided color or config color
        const color = lineColor || config.color;
        
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
            color: color,
            weight: 3,
            opacity: 0.8,
            className: 'defense-zone-parallel-line'
        });
        
        // Line 2 (bottom) - short line across center
        const line2Start = L.latLng(center.lat + line2Offset, center.lng - lineLength/2);
        const line2End = L.latLng(center.lat + line2Offset, center.lng + lineLength/2);
        const line2 = L.polyline([line2Start, line2End], {
            color: color,
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

