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
                color: '#ff0000',
                size: 24,
                className: 'nato-attack-basic'
            },
            'attack-main': {
                symbol: '⟶',
                color: '#ff0000',
                size: 30,
                className: 'nato-attack-main'
            },
            
            // NATO Attack Types (from Attack Intent form)
            'frontal': {
                symbol: '⇨',
                color: '#ff0000',
                size: 26,
                className: 'nato-attack-frontal'
            },
            'flanking': {
                symbol: '↗',
                color: '#ff0000',
                size: 26,
                className: 'nato-attack-flanking'
            },
            'envelopment': {
                symbol: '↻',
                color: '#ff0000',
                size: 28,
                className: 'nato-attack-envelopment'
            },
            'penetration': {
                symbol: '⇉',
                color: '#ff0000',
                size: 28,
                className: 'nato-attack-penetration'
            },
            'raid': {
                symbol: '⚔',
                color: '#ff0000',
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
        
        // Determine arrow color (SAME as force: Blue force → Blue arrow, Red force → Red arrow)
        // This matches defense element coloring
        if (placerSide) {
            const sideLower = placerSide.toLowerCase();
            
            // Blue side attacking → BLUE arrow (matches Blue Land defense elements)
            if (sideLower === 'blue' || sideLower.includes('blue')) {
                console.log('🎯 Blue force attacking → BLUE arrow (matches defense elements)');
                return '#0000ff'; // Blue (same as Blue Land defense)
            }
            // Red/Fox side attacking → RED arrow (matches Fox Land defense elements)
            else if (sideLower === 'red' || sideLower.includes('red') || sideLower.includes('fox')) {
                console.log('🎯 Red/Fox force attacking → RED arrow (matches defense elements)');
                return '#ff0000'; // Red (same as Fox Land defense)
            }
        }
        
        // Try to get from current logged-in user as final fallback
        if (window.userObject && window.userObject.forceType) {
            const forceType = window.userObject.forceType.toLowerCase();
            
            if (forceType.includes('blue') || forceType.includes('friendly')) {
                console.log('🎯 User is Blue force → BLUE arrow (matches defense elements)');
                return '#0000ff'; // Blue (matches Blue Land defense)
            } else if (forceType.includes('red') || forceType.includes('fox') || forceType.includes('enemy')) {
                console.log('🎯 User is Red/Fox force → RED arrow (matches defense elements)');
                return '#ff0000'; // Red (matches Fox Land defense)
            }
        }
        
        // Default: Red arrow
        console.log('⚠️ Could not determine side, using default RED arrow');
        return '#ff0000';
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
     * Get token insignia based on unit type symbol (center symbol)
     * @param {Object} token - Token object with unitType property
     * @returns {string} Unit type symbol (e.g., "O" for Infantry, "X" for Armored)
     */
    getTokenInsignia(token) {
        if (!token || !token.unitType) {
            return 'O'; // Default to Infantry (circle)
        }

        // Map unit types to their symbols (matching what's displayed in center of token)
        const unitTypeSymbols = {
            'Infantry': 'O',           // Circle
            'Armoured': 'X',           // X symbol  
            'Mechanized': 'X',         // X symbol
            'Artillery': '◊',          // Diamond
            'Aviation': '△',           // Triangle
            'AirDefense': '◊',         // Diamond
            'Engineers': '⚒',         // Hammer/pick
            'Signals': '◊',            // Diamond
            'Logistics': '◊',          // Diamond
            'Medical': '✚',           // Cross
            'Reconnaissance': '◊',     // Diamond
            'SpecialForces': '◊',      // Diamond
            'AirborneParatroop': '△',  // Triangle
            'Marines': 'O',            // Circle
            'Cavalry': 'X',            // X symbol
            'HeadquartersCommand': '◊', // Diamond
            'Intelligence': '◊',       // Diamond
            'MilitaryPolice': '✚',    // Cross
            'CBRN': '◊',              // Diamond
            'Maintenance': '⚒',       // Hammer/pick
            'Cyber': '◊'              // Diamond
        };

        // Handle both string and numeric unit types
        let unitType = token.unitType;
        if (typeof unitType === 'number') {
            // Convert numeric unit type to string
            const typeNames = ['Infantry', 'Armoured', 'Mechanized', 'Artillery', 'Aviation', 'AirDefense', 'Engineers', 'Signals', 'Logistics', 'Medical', 'Reconnaissance', 'SpecialForces', 'AirborneParatroop', 'Marines', 'Cavalry', 'HeadquartersCommand', 'Intelligence', 'MilitaryPolice', 'CBRN', 'Maintenance', 'Cyber'];
            unitType = typeNames[unitType] || 'Infantry';
        }

        return unitTypeSymbols[unitType] || 'O';
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
        
        // Base line style with canvas rendering for better performance and fixed positioning
        let lineStyle = {
            color: arrowColor,
            weight: 6,
            opacity: 0.9,
            className: `nato-attack-line ${symbolConfig.className}`,
            renderer: L.canvas() // Use canvas renderer for better performance and fixed positioning
        };

        // Customize line style based on attack type
        switch (attackType) {
            case 'attack-supporting':
                lineStyle.weight = 4;
                lineStyle.opacity = 0.7;
                lineStyle.dashArray = '5, 5';
                break;
            case 'attack-feint':
                lineStyle.weight = 4;
                lineStyle.opacity = 0.6;
                lineStyle.dashArray = '10, 10';
                break;
            case 'attack-envelopment':
                lineStyle.weight = 8;
                lineStyle.opacity = 0.8;
                break;
            case 'attack-ambush':
                lineStyle.weight = 4;
                lineStyle.opacity = 0.8;
                lineStyle.dashArray = '15, 5';
                break;
            default:
                lineStyle.dashArray = null; // Solid line
        }

        if (path.length < 2) return lineGroup;

        const spacing = 0.0003; // Maximum spacing between parallel lines (in degrees) for clear separation
        
        // Split path: parallel lines with gap before target, then arrowhead at END (near target)
        const endGap = 0.1; // 10% gap from target token
        const arrowheadLength = 0.01; // Only 1% of path for very short ending arrowhead
        const splitIndex = Math.max(1, Math.floor(path.length * (1 - arrowheadLength - endGap)));
        
        const bodyPath = path.slice(0, splitIndex + 1);
        const arrowheadPath = path.slice(splitIndex);
        
        // 1. Create TWO PARALLEL LINES from attacker toward target
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
        
        // 2. Create ARROWHEAD at END (near target)
        if (arrowheadPath.length >= 2) {
            const arrowhead = this.createEndingArrowhead(
                arrowheadPath,
                spacing,
                arrowColor,  // Use determined arrow color
                options
            );
            
            if (arrowhead.polygon) {
                lineGroup.addLayer(arrowhead.polygon);
                lineGroup.arrowheadPolygon = arrowhead.polygon;
            }
        }
        
        // 3. Add attack number label ON the main attack line (midpoint of full path)
        const fullPathMidpoint = Math.floor(path.length / 2);
        const labelPos = path[fullPathMidpoint];
        
        // Use token insignia and unit number instead of sequential numbers
        let labelText = '';
        if (options.attackerToken) {
            // Extract insignia and unit number from attacker token
            const insignia = this.getTokenInsignia(options.attackerToken);
            const unitNumber = options.attackerToken.unitDesignation || '';
            labelText = `${insignia} ${unitNumber}`.trim();
            console.log('🎯 Creating attack label with token insignia and number:', labelText);
        } else {
            // Fallback to sequential number if no token data available
            labelText = options.attackNumber ? options.attackNumber.toString() : '1';
            console.log('🎯 Creating attack label with fallback sequential number:', labelText);
        }
        
        console.log('🎯 Label position:', labelPos);
        console.log('🎯 Full path length:', path.length, 'Midpoint index:', fullPathMidpoint);
        
        const labelIcon = L.divIcon({
            html: `<div class="nato-attack-label">${labelText}</div>`,
            className: 'nato-attack-label-marker',
            iconSize: [30, 20],
            iconAnchor: [15, 10]
        });
        
        const labelMarker = L.marker(labelPos, {
            icon: labelIcon,
            zIndexOffset: 2000
        });
        
        lineGroup.addLayer(labelMarker);
        lineGroup.arrowheadLabel = labelMarker;
        
        console.log('✅ Attack label added to line group:', labelText, 'at position:', labelPos);
        
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
            className: 'nato-attack-arrowhead-polygon',
            renderer: L.canvas() // Use canvas renderer for better performance and fixed positioning
        });
        
        return {
            polygon: arrowheadPolygon,
            label: null
        };
    }

    /**
     * Create a simple triangular arrowhead at the END of the attack line (near target)
     * Creates a clean, solid triangular arrowhead like the attached image
     * @param {Array} path - Path segment for the arrowhead
     * @param {number} spacing - Distance between parallel lines
     * @param {string} color - Arrow color
     * @param {Object} options - Additional options
     * @returns {Object} Object containing arrowhead polygon
     */
    createEndingArrowhead(path, spacing, color, options = {}) {
        if (path.length < 2) return { polygon: null };
        
        const startPoint = path[0]; // Where parallel lines end
        const endPoint = path[path.length - 1]; // Target position (arrow tip)
        
        // Calculate direction (from parallel lines toward target)
        const dx = endPoint.lng - startPoint.lng;
        const dy = endPoint.lat - startPoint.lat;
        const length = Math.sqrt(dx * dx + dy * dy);
        
        if (length === 0) return { polygon: null };
        
        // Normalized direction
        const dirX = dx / length;
        const dirY = dy / length;
        
        // Perpendicular direction
        const perpX = -dirY;
        const perpY = dirX;
        
        // Create a simple triangular arrowhead
        // Base width proportional to spacing, but keep it small - only 10px wider than lines
        const arrowBaseWidth = spacing * 2; // Make arrowhead only 2x wider than line spacing for smaller size
        
        // Gap size to prevent overlap with parallel lines
        const gapSize = spacing * 0.2; // 20% of line spacing for smaller gap
        
        // Base points (back of arrow) with gap so lines don't touch arrowhead
        const baseLeft = L.latLng(
            startPoint.lat + perpY * (arrowBaseWidth + gapSize),
            startPoint.lng + perpX * (arrowBaseWidth + gapSize)
        );
        const baseRight = L.latLng(
            startPoint.lat - perpY * (arrowBaseWidth + gapSize),
            startPoint.lng - perpX * (arrowBaseWidth + gapSize)
        );
        
        // Tip point (front of arrow) - very short arrowhead for minimal appearance
        const fixedTipExtension = 0.00001; // Very small distance in degrees for very short arrowhead
        const tip = L.latLng(
            endPoint.lat + dirY * fixedTipExtension,
            endPoint.lng + dirX * fixedTipExtension
        );
        
        // Arrowhead with hidden base line: just two sides converging to tip (no bottom base line)
        const arrowheadPoints = [baseLeft, tip, baseRight];
        
        const arrowheadPolygon = L.polygon(arrowheadPoints, {
            color: color, // Outline color same as lines
            fillColor: color, // Fill with same color as lines
            fillOpacity: 0.9, // Solid fill
            weight: 2, // Thinner border for smaller appearance
            className: 'nato-attack-arrowhead-filled',
            renderer: L.canvas() // Use canvas renderer for better performance and fixed positioning
        });
        
        return {
            polygon: arrowheadPolygon
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
