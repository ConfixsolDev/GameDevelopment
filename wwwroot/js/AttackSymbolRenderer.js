/**
 * Attack Symbol Renderer
 * Generates NATO-standard attack symbols for military operations
 */

class AttackSymbolRenderer {
    constructor() {
        this.symbols = this.initializeAttackSymbols();
        console.log('🎯 Attack Symbol Renderer initialized');
    }

    /**
     * Initialize NATO attack symbols
     */
    initializeAttackSymbols() {
        return {
            // Basic Attack Symbols
            'attack': {
                symbol: '→',
                color: '#ff4444',
                size: 24,
                className: 'nato-attack-basic'
            },
            'attack-main': {
                symbol: '⟶',
                color: '#ff4444',
                size: 30,
                className: 'nato-attack-main'
            },
            
            // NATO Attack Types (from Attack Intent form)
            'frontal': {
                symbol: '⇨',
                color: '#ff4444',
                size: 26,
                className: 'nato-attack-frontal'
            },
            'flanking': {
                symbol: '↗',
                color: '#ff4444',
                size: 26,
                className: 'nato-attack-flanking'
            },
            'envelopment': {
                symbol: '↻',
                color: '#ff4444',
                size: 28,
                className: 'nato-attack-envelopment'
            },
            'penetration': {
                symbol: '⇉',
                color: '#ff4444',
                size: 28,
                className: 'nato-attack-penetration'
            },
            'raid': {
                symbol: '⚔',
                color: '#ff4444',
                size: 26,
                className: 'nato-attack-raid'
            },
            'ambush': {
                symbol: '⚡',
                color: '#ff6600',
                size: 24,
                className: 'nato-attack-ambush'
            },
            
            // Legacy Attack Types (for backward compatibility)
            'attack-supporting': {
                symbol: '⇢',
                color: '#ff8844',
                size: 24,
                className: 'nato-attack-supporting'
            },
            'attack-feint': {
                symbol: '⇛',
                color: '#ffaa44',
                size: 22,
                className: 'nato-attack-feint'
            }
        };
    }

    /**
     * Create NATO attack symbol
     * @param {string} attackType - Type of attack symbol
     * @param {Object} options - Additional options
     * @returns {Object} Attack symbol configuration
     */
    createAttackSymbol(attackType = 'attack', options = {}) {
        const symbolConfig = this.symbols[attackType] || this.symbols['attack'];
        
        return {
            symbol: symbolConfig.symbol,
            color: options.color || symbolConfig.color,
            size: options.size || symbolConfig.size,
            className: symbolConfig.className,
            attackType: attackType,
            ...options
        };
    }


    /**
     * Get arrow color based on target's placerSide (reverse logic)
     * If target placerSide is "Blue" → RED arrow (Blue attacking Red enemy)
     * If target placerSide is "Red" → BLUE arrow (Red attacking Blue enemy)
     * @param {Object} options - Options containing target token information
     * @returns {string} Arrow color
     */
    getArrowColorForAttacker(options = {}) {
        let placerSide = null;
        
        // Try to get placerSide from target token (suspected token being attacked)
        if (options.targetToken && options.targetToken.placerSide) {
            placerSide = options.targetToken.placerSide;
        }
        // Try from attacker if target not available (shouldn't happen but fallback)
        else if (options.attackerToken && options.attackerToken.placerSide) {
            placerSide = options.attackerToken.placerSide;
        }
        // Try from direct placerSide parameter
        else if (options.placerSide) {
            placerSide = options.placerSide;
        }
        
        // Determine arrow color (REVERSE: Blue side → Red arrow, Red side → Blue arrow)
        if (placerSide) {
            const sideLower = placerSide.toLowerCase();
            
            // Blue side attacking → RED arrow (attacking red enemy)
            if (sideLower === 'blue' || sideLower.includes('blue')) {
                console.log('🎯 Blue force attacking → RED arrow');
                return '#ff4444'; // Red
            }
            // Red side attacking → BLUE arrow (attacking blue enemy)
            else if (sideLower === 'red' || sideLower.includes('red')) {
                console.log('🎯 Red force attacking → BLUE arrow');
                return '#4444ff'; // Blue
            }
        }
        
        // Try to get from current logged-in user as final fallback
        if (window.userObject && window.userObject.forceType) {
            const forceType = window.userObject.forceType.toLowerCase();
            
            if (forceType.includes('blue') || forceType.includes('friendly')) {
                console.log('🎯 User is Blue force → RED arrow (default)');
                return '#ff4444'; // Red
            } else if (forceType.includes('red') || forceType.includes('enemy')) {
                console.log('🎯 User is Red force → BLUE arrow (default)');
                return '#4444ff'; // Blue
            }
        }
        
        // Default: Red arrow
        console.log('⚠️ Could not determine side, using default RED arrow');
        return '#ff4444';
    }

    /**
     * Extract symbol number from token name
     * @param {string} name - Token name (e.g., "Token 01", "Unit 12")
     * @returns {string} Extracted symbol number
     */
    extractSymbolNumber(name) {
        if (!name) return '';
        
        // Try to extract number patterns
        const patterns = [
            /(\d{1,3})$/,           // Numbers at end: "Token 01" -> "01"
            /\s(\d{1,3})\s/,        // Numbers in middle: "Unit 12 Alpha" -> "12"
            /[A-Za-z]\s*(\d{1,3})/, // Letter followed by number: "A 123" -> "123"
        ];
        
        for (const pattern of patterns) {
            const match = name.match(pattern);
            if (match && match[1]) {
                return match[1];
            }
        }
        
        // If no number found, return first 2-3 characters
        return name.substring(0, 3).toUpperCase();
    }


    /**
     * Create attack line with NATO styling (arrowhead at START, lines extend to target)
     * @param {Array} path - Array of LatLng points
     * @param {string} attackType - Type of attack
     * @param {Object} options - Additional options (including attackerSymbol)
     * @returns {L.LayerGroup} Layer group containing arrowhead at start and double lines to target
     */
    createAttackLine(path, attackType = 'attack', options = {}) {
        const symbolConfig = this.createAttackSymbol(attackType, options);
        
        // Determine arrow color based on attacker's team
        // If attacker is BLUE → arrow is RED (attacking red/enemy)
        // If attacker is RED → arrow is BLUE (attacking blue/enemy)
        const arrowColor = this.getArrowColorForAttacker(options);
        
        // Create layer group for the complete arrow
        const lineGroup = L.layerGroup();
        
        // Base line style
        let lineStyle = {
            color: arrowColor,
            weight: 3,
            opacity: 0.9,
            className: `nato-attack-line ${symbolConfig.className}`
        };

        // Customize line style based on attack type
        switch (attackType) {
            case 'attack-supporting':
                lineStyle.weight = 2;
                lineStyle.opacity = 0.7;
                lineStyle.dashArray = '5, 5';
                break;
            case 'attack-feint':
                lineStyle.weight = 2;
                lineStyle.opacity = 0.6;
                lineStyle.dashArray = '10, 10';
                break;
            case 'attack-envelopment':
                lineStyle.weight = 4;
                lineStyle.opacity = 0.8;
                break;
            case 'attack-ambush':
                lineStyle.weight = 2;
                lineStyle.opacity = 0.8;
                lineStyle.dashArray = '15, 5';
                break;
            default:
                lineStyle.dashArray = null; // Solid line
        }

        if (path.length < 2) return lineGroup;

        const spacing = 0.00008; // Spacing between parallel lines (in degrees)
        
        // Split path: arrowhead at START (first 10%), then parallel lines
        const arrowheadLength = 0.10; // 10% of path for starting arrowhead
        const splitIndex = Math.max(1, Math.floor(path.length * arrowheadLength));
        
        const arrowheadPath = path.slice(0, splitIndex + 1);
        const bodyPath = path.slice(splitIndex);
        
        // 1. Create ARROWHEAD at START (near attacker)
        if (arrowheadPath.length >= 2) {
            const arrowhead = this.createStartingArrowhead(
                arrowheadPath,
                spacing,
                arrowColor,  // Use determined arrow color
                options
            );
            
            if (arrowhead.polygon) {
                lineGroup.addLayer(arrowhead.polygon);
                lineGroup.arrowheadPolygon = arrowhead.polygon;
            }
            
            if (arrowhead.label) {
                lineGroup.addLayer(arrowhead.label);
                lineGroup.arrowheadLabel = arrowhead.label;
            }
        }
        
        // 2. Create parallel lines extending FROM arrowhead TO target
        if (bodyPath.length >= 2) {
            const bodyPath1 = this.offsetPath(bodyPath, spacing);
            const bodyPath2 = this.offsetPath(bodyPath, -spacing);
            
            const line1 = L.polyline(bodyPath1, lineStyle);
            const line2 = L.polyline(bodyPath2, lineStyle);
            
            lineGroup.addLayer(line1);
            lineGroup.addLayer(line2);
            
            lineGroup.line1 = line1;
            lineGroup.line2 = line2;
        }
        
        // Make the group respond to events
        lineGroup.setStyle = function(style) {
            if (lineGroup.line1) lineGroup.line1.setStyle(style);
            if (lineGroup.line2) lineGroup.line2.setStyle(style);
            if (lineGroup.arrowheadPolygon) {
                lineGroup.arrowheadPolygon.setStyle({
                    fillColor: style.color || symbolConfig.color,
                    color: style.color || symbolConfig.color
                });
            }
        };
        
        lineGroup.on = function(event, handler) {
            if (lineGroup.line1) lineGroup.line1.on(event, handler);
            if (lineGroup.line2) lineGroup.line2.on(event, handler);
            if (lineGroup.arrowheadPolygon) lineGroup.arrowheadPolygon.on(event, handler);
            return lineGroup;
        };
        
        return lineGroup;
    }

    /**
     * Create arrowhead at the START of attack line (near attacker)
     * @param {Array} path - Path segment for arrowhead at start
     * @param {number} spacing - Line spacing
     * @param {string} color - Arrow color
     * @param {Object} options - Options including attackerSymbol
     * @returns {Object} Object containing polygon and label
     */
    createStartingArrowhead(path, spacing, color, options = {}) {
        if (path.length < 2) return { polygon: null, label: null };
        
        const startPoint = path[0]; // Attacker position
        const endPoint = path[path.length - 1]; // Where arrowhead ends and lines begin
        
        // Calculate direction (from attacker toward target)
        const dx = endPoint.lng - startPoint.lng;
        const dy = endPoint.lat - startPoint.lat;
        const length = Math.sqrt(dx * dx + dy * dy);
        
        if (length === 0) return { polygon: null, label: null };
        
        // Normalized direction
        const dirX = dx / length;
        const dirY = dy / length;
        
        // Perpendicular direction
        const perpX = -dirY;
        const perpY = dirX;
        
        // Arrowhead dimensions (WIDE at start, narrows to parallel lines)
        const arrowWidth = spacing * 4; // Wide base at attacker
        
        // Create arrowhead polygon
        // Wide base at attacker, narrows to parallel line spacing at end
        
        // Base (wide) at attacker position
        const baseLeft = L.latLng(
            startPoint.lat + perpY * arrowWidth,
            startPoint.lng + perpX * arrowWidth
        );
        const baseRight = L.latLng(
            startPoint.lat - perpY * arrowWidth,
            startPoint.lng - perpX * arrowWidth
        );
        
        // Narrow end where parallel lines begin
        const narrowLeft = L.latLng(
            endPoint.lat + perpY * spacing,
            endPoint.lng + perpX * spacing
        );
        const narrowRight = L.latLng(
            endPoint.lat - perpY * spacing,
            endPoint.lng - perpX * spacing
        );
        
        // Arrowhead shape: baseLeft -> baseRight -> narrowRight -> narrowLeft -> back to baseLeft
        const arrowheadPoints = [baseLeft, baseRight, narrowRight, narrowLeft, baseLeft];
        
        const arrowheadPolygon = L.polygon(arrowheadPoints, {
            color: '#000000',
            fillColor: color,
            fillOpacity: 0.9,
            weight: 2,
            className: 'nato-attack-arrowhead-polygon'
        });
        
        // Add symbol label in the center of arrowhead
        const attackerSymbol = options.attackerSymbol || options.attackerName || '';
        const symbolNumber = this.extractSymbolNumber(attackerSymbol);
        
        // Calculate center position for label
        const labelPos = L.latLng(
            (startPoint.lat + endPoint.lat) / 2,
            (startPoint.lng + endPoint.lng) / 2
        );
        
        const labelIcon = L.divIcon({
            html: `<div class="nato-attack-label">${symbolNumber}</div>`,
            className: 'nato-attack-label-marker',
            iconSize: [30, 20],
            iconAnchor: [15, 10]
        });
        
        const labelMarker = L.marker(labelPos, {
            icon: labelIcon,
            zIndexOffset: 2000
        });
        
        return {
            polygon: arrowheadPolygon,
            label: labelMarker
        };
    }


    /**
     * Offset a path perpendicular to its direction
     * @param {Array} path - Array of LatLng points
     * @param {number} offset - Offset distance (in degrees)
     * @returns {Array} Offset path
     */
    offsetPath(path, offset) {
        if (!path || path.length < 2) return path;
        
        const offsetPath = [];
        
        for (let i = 0; i < path.length; i++) {
            const current = path[i];
            let perpX, perpY;
            
            if (i === 0) {
                // First point: use direction to next point
                const next = path[i + 1];
                const dx = next.lng - current.lng;
                const dy = next.lat - current.lat;
                const length = Math.sqrt(dx * dx + dy * dy);
                perpX = -dy / length * offset;
                perpY = dx / length * offset;
            } else if (i === path.length - 1) {
                // Last point: use direction from previous point
                const prev = path[i - 1];
                const dx = current.lng - prev.lng;
                const dy = current.lat - prev.lat;
                const length = Math.sqrt(dx * dx + dy * dy);
                perpX = -dy / length * offset;
                perpY = dx / length * offset;
            } else {
                // Middle points: use average of directions
                const prev = path[i - 1];
                const next = path[i + 1];
                const dx1 = current.lng - prev.lng;
                const dy1 = current.lat - prev.lat;
                const dx2 = next.lng - current.lng;
                const dy2 = next.lat - current.lat;
                const length1 = Math.sqrt(dx1 * dx1 + dy1 * dy1);
                const length2 = Math.sqrt(dx2 * dx2 + dy2 * dy2);
                const perpX1 = -dy1 / length1;
                const perpY1 = dx1 / length1;
                const perpX2 = -dy2 / length2;
                const perpY2 = dx2 / length2;
                perpX = ((perpX1 + perpX2) / 2) * offset;
                perpY = ((perpY1 + perpY2) / 2) * offset;
            }
            
            offsetPath.push(L.latLng(current.lat + perpY, current.lng + perpX));
        }
        
        return offsetPath;
    }

    /**
     * Create attack direction indicator
     * @param {L.LatLng} position - Position for the indicator
     * @param {number} direction - Direction in degrees
     * @param {string} attackType - Type of attack
     * @param {Object} options - Additional options
     * @returns {L.Marker} Direction indicator marker
     */
    createDirectionIndicator(position, direction, attackType = 'attack', options = {}) {
        const symbolConfig = this.createAttackSymbol(attackType, options);
        
        const directionIcon = L.divIcon({
            html: `
                <div class="nato-direction-indicator ${symbolConfig.className}" 
                     style="transform: rotate(${direction}deg); color: ${symbolConfig.color};">
                    <div class="direction-symbol">${symbolConfig.symbol}</div>
                    <div class="direction-line"></div>
                </div>
            `,
            className: 'nato-direction-marker',
            iconSize: [20, 20],
            iconAnchor: [10, 10]
        });
        
        return L.marker(position, { 
            icon: directionIcon,
            attackType: attackType,
            direction: direction
        });
    }

    /**
     * Get available attack types
     * @returns {Array} Array of attack type names
     */
    getAvailableAttackTypes() {
        return Object.keys(this.symbols);
    }

    /**
     * Get attack symbol info
     * @param {string} attackType - Attack type
     * @returns {Object} Symbol information
     */
    getAttackSymbolInfo(attackType) {
        return this.symbols[attackType] || this.symbols['attack'];
    }
}

// Initialize global instance
window.attackSymbolRenderer = new AttackSymbolRenderer();
console.log('🎯 Attack Symbol Renderer loaded');
