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
            
            // Unit Type Icons (Font Awesome classes)
            unitTypeIcons: {
                0: 'fa-person-rifle',      // Infantry
                1: 'fa-shield',            // Armoured
                2: 'fa-truck-pickup',      // Mechanized
                3: 'fa-bullseye',          // Artillery
                4: 'fa-helicopter',        // Aviation
                5: 'fa-shield-halved',     // Air Defense
                6: 'fa-hammer',            // Engineers
                7: 'fa-satellite-dish',    // Signals
                8: 'fa-boxes-stacked',     // Logistics
                9: 'fa-staff-snake',       // Medical
                10: 'fa-binoculars',       // Reconnaissance
                11: 'fa-user-secret',      // Special Forces
                12: 'fa-parachute-box',    // Airborne
                13: 'fa-anchor',           // Marines
                14: 'fa-horse-head',       // Cavalry
                15: 'fa-flag',             // HQ/Command
                16: 'fa-eye',              // Intelligence
                17: 'fa-shield-halved',    // Military Police
                18: 'fa-radiation',        // CBRN
                19: 'fa-wrench',           // Maintenance
                20: 'fa-laptop-code'       // Cyber
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
     * Create a Leaflet DivIcon with military symbol
     */
    createMilitaryIcon(tokenData) {
        const { organizationLevel, unitType, unitDesignation, forceType, name } = tokenData;
        
        const orgSymbol = this.symbols.orgLevelSymbols[organizationLevel] || '';
        const orgAbbr = this.symbols.orgLevelAbbr[organizationLevel] || '';
        const unitIcon = this.symbols.unitTypeIcons[unitType] || 'fa-circle';
        const unitName = this.symbols.unitTypeNames[unitType] || '';
        
        // Determine force type CSS class
        const forceClass = this.getForceTypeCssClass(forceType);
        
        // Create unit label (e.g., "Brig Armoured 29")
        const unitLabel = `${orgAbbr} ${unitName} ${unitDesignation || ''}`.trim();
        
        // Size class based on organization level
        const sizeClass = this.getSizeClass(organizationLevel);
        
        // Build HTML for the military symbol
        const html = `
            <div class="military-token">
                <div class="military-symbol ${forceClass} ${sizeClass}">
                    <div class="org-level-marker">${orgSymbol}</div>
                    <i class="fas ${unitIcon} unit-type-icon"></i>
                </div>
                <div class="unit-label">${unitLabel}</div>
            </div>
        `;

        return L.divIcon({
            html: html,
            className: 'military-marker',
            iconSize: [80, 80],
            iconAnchor: [40, 40],
            popupAnchor: [0, -40]
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

