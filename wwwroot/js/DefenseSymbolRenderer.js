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

            // Minefields (NATO APP-6 Standard - Circle with dot in center)
            minefields: {
                'antipersonnel': {
                    symbol: '●', // Dot in center
                    color: '#cc0000',
                    name: 'Anti-Personnel Minefield',
                    radius: 8,
                    strokeWidth: 2
                },
                'antitank': {
                    symbol: '●', // Dot in center
                    color: '#990000',
                    name: 'Anti-Tank Minefield',
                    radius: 10,
                    strokeWidth: 2
                },
                'mixed': {
                    symbol: '●', // Dot in center
                    color: '#660000',
                    name: 'Mixed Minefield',
                    radius: 12,
                    strokeWidth: 3
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
     * Create minefield markers on map (NATO APP-6 Standard - Individual markers, not polygons)
     */
    createMinefield(coordinates, type = 'mixed', options = {}) {
        const minefieldConfig = this.symbols.minefields[type];
        
        // Create individual mine markers instead of a polygon
        const markers = this.createMinefieldMarkers(coordinates, minefieldConfig);
        
        return { markers };
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
     * Create minefield markers (NATO APP-6 Standard - Circle with dot in center)
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
            
            const html = `
                <div class="nato-minefield-marker" style="width: ${config.radius * 2}px; height: ${config.radius * 2}px;">
                    <div class="minefield-circle" style="border-color: ${config.color}; border-width: ${config.strokeWidth}px;">
                        <div class="minefield-dot" style="background-color: ${config.color};"></div>
                    </div>
                </div>
            `;

            const icon = L.divIcon({
                html: html,
                className: 'nato-minefield-marker-icon',
                iconSize: [config.radius * 2, config.radius * 2],
                iconAnchor: [config.radius, config.radius]
            });

            markers.push(L.marker(latlng, { icon }));
        }
        
        return markers;
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
     * Get defense symbol information
     */
    getDefenseSymbolInfo(type, category) {
        const categoryMap = {
            'killzone': this.symbols.killZones,
            'minefield': this.symbols.minefields,
            'obstacle': this.symbols.obstacles,
            'position': this.symbols.positions,
            'withdrawal': this.symbols.withdrawalRoutes,
            'line': this.symbols.defensiveLines
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

