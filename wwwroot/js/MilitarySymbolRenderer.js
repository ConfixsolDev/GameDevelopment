/**
 * Military Symbol Renderer
 * Renders NATO-style military unit symbols on Leaflet maps
 * Replaces image-based tokens with standardized military symbols
 */

class MilitarySymbolRenderer {
    constructor() {
        this.symbols = this.initializeSymbols();
    }

    /**
     * Initialize symbol mappings
     */
    initializeSymbols() {
        return {
            // Organization Level Symbols
            orgLevelSymbols: {
                1: '●',           // Squad
                2: '●●',          // Platoon
                3: '●●●',         // Company
                4: '╎',           // Battalion
                5: '╎╎',          // Regiment
                6: '✕',           // Brigade
                7: '✕✕',          // Division
                8: '✕✕✕',         // Corps
                9: '✕✕✕✕'         // Army
            },
            
            // Organization Level Abbreviations
            orgLevelAbbr: {
                1: 'Sqd', 2: 'Plt', 3: 'Coy', 4: 'Bn', 5: 'Regt',
                6: 'Brig', 7: 'Div', 8: 'Corps', 9: 'Army'
            },
            
            // Unit Type Icons (NATO Standard Symbols)
            unitTypeIcons: {
                0: 'nato-infantry',        // Infantry - Crossed swords (X pattern)
                1: 'nato-armour',          // Armoured - Track/oval shape
                2: 'nato-mechanized',       // Mechanized - Truck symbol
                3: 'nato-artillery',        // Artillery - Circle/dot
                4: 'nato-aviation',         // Aviation - Helicopter
                5: 'nato-airdefense',       // Air Defense - Umbrella arc
                6: 'nato-engineer',         // Engineers - Bridge/E symbol
                7: 'nato-signals',          // Signals - Lightning bolt
                8: 'nato-logistics',        // Logistics - Boxes
                9: 'nato-medical',          // Medical - Cross
                10: 'nato-reconnaissance',  // Reconnaissance - Quatrefoil with diagonal line
                11: 'nato-specialforces',   // Special Forces - Special symbol
                12: 'nato-airborne',        // Airborne - Parachute
                13: 'nato-marines',         // Marines - Anchor
                14: 'nato-cavalry',         // Cavalry - Horse head
                15: 'nato-hq',              // HQ/Command - Flag
                16: 'nato-intelligence',    // Intelligence - Eye
                17: 'nato-militarypolice',  // Military Police - Shield
                18: 'nato-cbrn',            // CBRN - Radiation symbol
                19: 'nato-maintenance',     // Maintenance - Wrench
                20: 'nato-cyber'            // Cyber - Computer
            },
            
            // Unit Type Short Names
            unitTypeNames: {
                0: 'Inf', 1: 'Armoured', 2: 'Mech', 3: 'Arty', 4: 'Avn',
                5: 'ADA', 6: 'Eng', 7: 'Sig', 8: 'Log', 9: 'Med',
                10: 'Recce', 11: 'SF', 12: 'Airborne', 13: 'Marines', 14: 'Cav',
                15: 'HQ', 16: 'Intel', 17: 'MP', 18: 'CBRN', 19: 'Maint', 20: 'Cyber'
            }
        };
    }

    /**
     * Create a military symbol marker for Leaflet
     * @param {Object} tokenData - Token data including organizationLevel, unitType, unitDesignation, forceType
     * @param {L.LatLng} latlng - Position for the marker
     * @returns {L.Marker} Leaflet marker with military symbol
     */
    createMilitaryMarker(tokenData, latlng) {
        // If token doesn't have military classification, fall back to simple marker
        if (!tokenData.organizationLevel || !tokenData.unitType) {
            return this.createFallbackMarker(tokenData, latlng);
        }

        const icon = this.createMilitaryIcon(tokenData);
        const marker = L.marker(latlng, {
            icon: icon,
            draggable: true,
            riseOnHover: true
        });

        // Store token data on marker
        marker.tokenData = tokenData;
        
        // Add popup with military unit info
        marker.bindPopup(this.createPopupContent(tokenData), {
            maxWidth: 300,
            className: 'military-token-popup'
        });

        return marker;
    }

    /**
     * Create a Leaflet DivIcon with professional military symbol
     */
    createMilitaryIcon(tokenData) {
        const { organizationLevel, unitType, unitDesignation, forceType, name } = tokenData;
        
        // Convert string values to numbers if needed for organization level
        const orgLevelNum = this.convertOrgLevelToNumber(organizationLevel);
        
        const orgSymbol = this.symbols.orgLevelSymbols[orgLevelNum] || '';
        const orgAbbr = this.symbols.orgLevelAbbr[orgLevelNum] || '';
        
        // Determine force type CSS class
        const forceClass = this.getForceTypeCssClass(forceType);
        
        // Get NATO-style frame based on organization level
        const frameType = this.getNATOFrameType(organizationLevel);
        
        // Debug logging
        console.log('🎖️ Creating military symbol with data:', {
            originalData: { organizationLevel, unitType, unitDesignation, forceType, name },
            orgLevelNum,
            orgSymbol,
            orgAbbr,
            forceClass
        });
        
        // Build HTML for the professional NATO-style military symbol
        const html = `
            <div class="military-token nato-standard ${forceClass}">
                <div class="nato-frame ${frameType}">
                    <!-- Organization Level at Top -->
                    <div class="org-level-top">
                        <div class="org-symbol">${orgSymbol}</div>
                    </div>
                    
                    <!-- Unit Type Symbol in Center with Designation -->
                    <div class="unit-symbol-center">
                        <div class="unit-symbol ${unitType || 'Infantry'}"></div>
                        <div class="unit-designation-right">
                            <div class="unit-designation">${unitDesignation || 'N/A'}</div>
                        </div>
                    </div>
                </div>
            </div>
        `;

        return L.divIcon({
            html: html,
            className: 'military-marker professional',
            iconSize: [100, 100],
            iconAnchor: [50, 50],
            popupAnchor: [0, -50]
        });
    }

    /**
     * Create fallback marker for tokens without military classification
     */
    createFallbackMarker(tokenData, latlng) {
        const iconUrl = tokenData.assetImagePath || '/images/default-token.png';
        const icon = L.icon({
            iconUrl: iconUrl,
            iconSize: [40, 40],
            iconAnchor: [20, 20],
            popupAnchor: [0, -20]
        });

        const marker = L.marker(latlng, {
            icon: icon,
            draggable: true,
            riseOnHover: true
        });

        marker.tokenData = tokenData;
        marker.bindPopup(this.createPopupContent(tokenData));
        
        return marker;
    }

    /**
     * Get force type CSS class
     */
    getForceTypeCssClass(forceType) {
        if (!forceType) return 'force-friendly';
        
        const type = forceType.toLowerCase();
        if (type === 'friendly' || type === 'blue') return 'force-friendly';
        if (type === 'hostile' || type === 'red' || type === 'enemy') return 'force-hostile';
        if (type === 'neutral' || type === 'green') return 'force-neutral';
        if (type === 'unknown' || type === 'yellow') return 'force-unknown';
        
        return 'force-friendly';
    }

    /**
     * Get size class based on organization level
     */
    getSizeClass(orgLevel) {
        if (orgLevel <= 2) return 'size-squad';
        if (orgLevel <= 4) return 'size-company';
        if (orgLevel <= 6) return 'size-brigade';
        return 'size-division';
    }

    /**
     * Convert organization level string to number
     */
    convertOrgLevelToNumber(orgLevel) {
        if (typeof orgLevel === 'number') return orgLevel;
        
        const levelMap = {
            'Squad': 1, 'Platoon': 2, 'Company': 3, 'Battalion': 4, 'Regiment': 5,
            'Brigade': 6, 'Division': 7, 'Corps': 8, 'Army': 9
        };
        
        return levelMap[orgLevel] || 6; // Default to Brigade
    }

    /**
     * Convert unit type string to number
     */
    convertUnitTypeToNumber(unitType) {
        if (typeof unitType === 'number') return unitType;
        
        const typeMap = {
            'Infantry': 0, 'Armoured': 1, 'Mechanized': 2, 'Artillery': 3, 'Aviation': 4,
            'Air Defense': 5, 'Engineers': 6, 'Signals': 7, 'Logistics': 8, 'Medical': 9,
            'Reconnaissance': 10, 'Special Forces': 11, 'Airborne': 12, 'Marines': 13,
            'Cavalry': 14, 'Headquarters': 15, 'Intelligence': 16, 'Military Police': 17,
            'CBRN': 18, 'Maintenance': 19, 'Cyber': 20
        };
        
        return typeMap[unitType] || 0; // Default to Infantry
    }

    /**
     * Get NATO-style frame type based on organization level
     */
    getNATOFrameType(orgLevel) {
        if (orgLevel <= 2) return 'frame-squad';      // Small units - rectangle
        if (orgLevel <= 4) return 'frame-company';    // Medium units - rectangle with notch
        if (orgLevel <= 6) return 'frame-brigade';    // Large units - hexagon
        return 'frame-division';                      // Very large units - octagon
    }

    /**
     * Create popup content for military unit
     */
    createPopupContent(tokenData) {
        const { name, organizationLevel, unitType, unitDesignation, forceType } = tokenData;
        
        let html = `<div class="military-info">`;
        html += `<h6 style="margin: 0 0 10px 0; padding-bottom: 5px; border-bottom: 2px solid #333;">${name}</h6>`;
        
        if (organizationLevel && unitType) {
            const orgName = this.symbols.orgLevelAbbr[organizationLevel];
            const typeName = this.symbols.unitTypeNames[unitType];
            
            html += `<div class="info-row">
                <span class="info-label">Unit:</span>
                <span class="info-value">${orgName} ${typeName} ${unitDesignation || ''}</span>
            </div>`;
        }
        
        if (forceType) {
            html += `<div class="info-row">
                <span class="info-label">Force:</span>
                <span class="info-value">${forceType}</span>
            </div>`;
        }
        
        html += `<div class="info-row">
            <span class="info-label">Token ID:</span>
            <span class="info-value" style="font-family: monospace; font-size: 10px;">${tokenData.id}</span>
        </div>`;
        
        html += `</div>`;
        
        return html;
    }

    /**
     * Update existing marker with new military symbol
     */
    updateMarkerSymbol(marker, tokenData) {
        if (!marker || !tokenData) return;
        
        const newIcon = this.createMilitaryIcon(tokenData);
        marker.setIcon(newIcon);
        marker.tokenData = tokenData;
        
        // Update popup
        marker.setPopupContent(this.createPopupContent(tokenData));
    }

    /**
     * Get unit label for display
     */
    getUnitLabel(organizationLevel, unitType, unitDesignation) {
        const orgAbbr = this.symbols.orgLevelAbbr[organizationLevel] || '';
        const unitName = this.symbols.unitTypeNames[unitType] || '';
        return `${orgAbbr} ${unitName} ${unitDesignation || ''}`.trim();
    }
}

// Create global instance
window.militarySymbolRenderer = new MilitarySymbolRenderer();

// Export for module use
if (typeof module !== 'undefined' && module.exports) {
    module.exports = MilitarySymbolRenderer;
}

console.log('✅ Military Symbol Renderer loaded');

