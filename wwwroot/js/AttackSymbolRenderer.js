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
     * Create attack arrow marker
     * @param {L.LatLng} targetPos - Target position
     * @param {L.LatLng} attackerPos - Attacker position
     * @param {string} attackType - Type of attack
     * @param {Object} options - Additional options
     * @returns {L.Marker} Leaflet marker with attack symbol
     */
    createAttackArrow(targetPos, attackerPos, attackType = 'attack', options = {}) {
        // Calculate arrow direction
        const dx = targetPos.lng - attackerPos.lng;
        const dy = targetPos.lat - attackerPos.lat;
        const angle = Math.atan2(dy, dx) * 180 / Math.PI;
        
        // Get attack symbol configuration
        const symbolConfig = this.createAttackSymbol(attackType, options);
        
        // Create arrow icon with NATO styling
        const arrowIcon = L.divIcon({
            html: `
                <div class="nato-attack-symbol ${symbolConfig.className}" 
                     style="transform: rotate(${angle}deg); color: ${symbolConfig.color}; font-size: ${symbolConfig.size}px;">
                    <div class="attack-symbol-inner">
                        ${symbolConfig.symbol}
                    </div>
                    <div class="attack-symbol-shadow"></div>
                </div>
            `,
            className: 'nato-attack-marker',
            iconSize: [symbolConfig.size + 8, symbolConfig.size + 8],
            iconAnchor: [(symbolConfig.size + 8) / 2, (symbolConfig.size + 8) / 2]
        });
        
        // Position arrow slightly before the target
        const arrowPos = this.calculateArrowPosition(targetPos, attackerPos);
        
        const arrowMarker = L.marker(arrowPos, { 
            icon: arrowIcon,
            attackType: attackType,
            symbolConfig: symbolConfig
        });
        
        return arrowMarker;
    }

    /**
     * Calculate optimal arrow position
     * @param {L.LatLng} targetPos - Target position
     * @param {L.LatLng} attackerPos - Attacker position
     * @returns {L.LatLng} Calculated arrow position
     */
    calculateArrowPosition(targetPos, attackerPos) {
        // Calculate distance and direction
        const dx = targetPos.lng - attackerPos.lng;
        const dy = targetPos.lat - attackerPos.lat;
        const distance = Math.sqrt(dx * dx + dy * dy);
        
        // Position arrow 80% of the way to target (adjustable)
        const ratio = 0.8;
        const offsetLng = dx * (1 - ratio);
        const offsetLat = dy * (1 - ratio);
        
        return L.latLng(
            targetPos.lat - offsetLat,
            targetPos.lng - offsetLng
        );
    }

    /**
     * Create attack line with NATO styling
     * @param {Array} path - Array of LatLng points
     * @param {string} attackType - Type of attack
     * @param {Object} options - Additional options
     * @returns {L.Polyline} Leaflet polyline with attack styling
     */
    createAttackLine(path, attackType = 'attack', options = {}) {
        const symbolConfig = this.createAttackSymbol(attackType, options);
        
        // Determine line style based on attack type
        let lineStyle = {
            color: symbolConfig.color,
            weight: 4,
            opacity: 0.9,
            className: `nato-attack-line ${symbolConfig.className}`
        };

        // Customize line style based on attack type
        switch (attackType) {
            case 'attack-supporting':
                lineStyle.weight = 3;
                lineStyle.opacity = 0.7;
                lineStyle.dashArray = '5, 5';
                break;
            case 'attack-feint':
                lineStyle.weight = 2;
                lineStyle.opacity = 0.6;
                lineStyle.dashArray = '10, 10';
                break;
            case 'attack-envelopment':
                lineStyle.weight = 5;
                lineStyle.opacity = 0.8;
                break;
            case 'attack-ambush':
                lineStyle.weight = 3;
                lineStyle.opacity = 0.8;
                lineStyle.dashArray = '15, 5';
                break;
            default:
                lineStyle.dashArray = null; // Solid line
        }

        return L.polyline(path, lineStyle);
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
