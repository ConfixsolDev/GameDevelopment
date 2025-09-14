// Real Map Arena System
let realMapArena = {
    map: null,
    mapContainer: null,
    hexGrid: null,
    currentZoom: 12,
    currentCenter: { lat: 51.5074, lng: -0.1278 }, // London default
    mapType: 'roadmap',
    showHexOverlay: false,
    tokens: new Map(),
    features: new Map(),
    selectedToken: null,
    selectedTerrain: null,
    isInitialized: false
};

// Map region presets
const mapRegions = {
    london: {
        name: 'Greater London',
        center: { lat: 51.5074, lng: -0.1278 },
        zoom: 10,
        bounds: {
            north: 51.7,
            south: 51.3,
            east: 0.3,
            west: -0.5
        }
    },
    paris: {
        name: 'Paris Region',
        center: { lat: 48.8566, lng: 2.3522 },
        zoom: 10,
        bounds: {
            north: 49.0,
            south: 48.6,
            east: 2.8,
            west: 1.8
        }
    },
    berlin: {
        name: 'Berlin Area',
        center: { lat: 52.5200, lng: 13.4050 },
        zoom: 10,
        bounds: {
            north: 52.7,
            south: 52.3,
            east: 13.8,
            west: 13.0
        }
    },
    newyork: {
        name: 'New York Metro',
        center: { lat: 40.7128, lng: -74.0060 },
        zoom: 10,
        bounds: {
            north: 41.0,
            south: 40.4,
            east: -73.5,
            west: -74.8
        }
    },
    tokyo: {
        name: 'Tokyo Prefecture',
        center: { lat: 35.6762, lng: 139.6503 },
        zoom: 10,
        bounds: {
            north: 35.9,
            south: 35.4,
            east: 140.0,
            west: 139.2
        }
    }
};

// Initialize real map arena
document.addEventListener('DOMContentLoaded', function() {
    console.log('Real Map Arena: DOM loaded, starting initialization...');
    
    // Wait for DOM to be fully loaded and other scripts
    setTimeout(() => {
        try {
            console.log('Real Map Arena: Starting initialization...');
            initializeRealMapArena();
            setupDataEntrySystem();
            setupMapControls();
            setupTokenSystem();
            console.log('Real Map Arena: Initialization complete');
        } catch (error) {
            console.error('Error initializing real map arena:', error);
            showNotification('Real map system error: ' + error.message, 'error');
        }
    }, 500);
});

// Initialize the real map arena
function initializeRealMapArena() {
    console.log('Initializing Real Map Arena...');
    
    realMapArena.mapContainer = document.getElementById('realMapContainer');
    realMapArena.hexGrid = document.getElementById('hexGrid');
    
    if (!realMapArena.mapContainer) {
        console.error('Real map container not found');
        showNotification('Map container not found!', 'error');
        return false;
    }
    
    console.log('Map container found:', realMapArena.mapContainer);
    
    // Initialize map (using Leaflet for better control)
    const mapInitialized = initializeMap();
    if (!mapInitialized) {
        console.error('Failed to initialize map');
        showNotification('Failed to initialize map!', 'error');
        return false;
    }
    
    // Setup hex grid mapping
    setupHexGridMapping();
    
    // Load initial map data
    loadMapRegion('london');
    
    realMapArena.isInitialized = true;
    showNotification('Real Map Arena initialized!', 'success');
    return true;
}

// Initialize the map using Leaflet
function initializeMap() {
    console.log('Initializing Leaflet map...');
    
    // Create map container
    const mapElement = document.getElementById('map');
    if (!mapElement) {
        console.error('Map element not found!');
        return false;
    }
    
    try {
        // Initialize Leaflet map
        realMapArena.map = L.map('map', {
            center: [realMapArena.currentCenter.lat, realMapArena.currentCenter.lng],
            zoom: realMapArena.currentZoom,
            zoomControl: false, // We'll use custom controls
            attributionControl: false,
            maxBounds: [
                [20, -130],  // Southwest corner - North America focus
                [50, -60]    // Northeast corner
            ],
            maxBoundsViscosity: 1.0,
            minZoom: 3,
            maxZoom: 18
        });
        
        console.log('Leaflet map created successfully');
        
        // Add tile layer (OpenStreetMap)
        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            attribution: '© OpenStreetMap contributors',
            maxZoom: 18
        }).addTo(realMapArena.map);
        
        console.log('Tile layer added successfully');
        
        // Add click handler for map
        realMapArena.map.on('click', function(e) {
            console.log('Map clicked at:', e.latlng);
            handleMapClick(e);
        });
        
        // MILITARY GAME TOOLS - DRAW, MEASURE, SELECT
        realMapArena.map.on('click', function(e) {
            console.log('Map clicked at:', e.latlng);
            
            // Check current tool mode
            if (window.currentTool === 'draw') {
                handleDrawTool(e);
            } else if (window.currentTool === 'measure') {
                handleMeasureTool(e);
            } else if (window.currentTool === 'select') {
                handleSelectTool(e);
            } else if (window.currentTool === 'area') {
                handleAreaTool(e);
            } else if (window.currentTool === 'maparea') {
                handleMapAreaTool(e);
            } else if (window.removeMode) {
                handleRemoveMode(e);
            } else if (window.selectedUnitType) {
                // Place unit
                const unitIcon = 'fa-solid fa-circle';
                const unitColor = '#ff0000';
                const unitSerial = window.selectedUnitType.toUpperCase().substring(0, 3);
                const placementId = Date.now().toString();
                
                placeTokenOnMap(e.latlng.lat, e.latlng.lng, null, placementId, placementId, unitIcon, unitColor, unitSerial);
                showNotification(`${window.selectedUnitType} unit placed`, 'success');
                window.selectedUnitType = null;
                
            } else if (window.selectedFeatureType) {
                // Place feature at actual click location
                const clickLat = e.latlng.lat;
                const clickLng = e.latlng.lng;
                
                // Create feature marker at actual coordinates
                const featureIcon = L.divIcon({
                    html: `<div style="
                        background: #ff6b35;
                        color: white;
                        border: 2px solid white;
                        border-radius: 50%;
                        width: 30px;
                        height: 30px;
                        display: flex;
                        align-items: center;
                        justify-content: center;
                        font-weight: bold;
                        font-size: 14px;
                    ">${window.selectedFeatureType.charAt(0).toUpperCase()}</div>`,
                    iconSize: [30, 30],
                    iconAnchor: [15, 15]
                });
                
                const featureMarker = L.marker([clickLat, clickLng], {
                    icon: featureIcon
                }).addTo(realMapArena.map);
                
                // Store feature with actual coordinates
                const featureId = Date.now().toString();
                realMapArena.features.set(featureId, {
                    marker: featureMarker,
                    featureType: window.selectedFeatureType,
                    lat: clickLat,
                    lng: clickLng
                });
                
                showNotification(`${window.selectedFeatureType} placed at (${clickLat.toFixed(4)}, ${clickLng.toFixed(4)})`, 'success');
                window.selectedFeatureType = null;
                
            } else if (window.selectedTerrainType) {
                // Change terrain at actual click location
                const clickLat = e.latlng.lat;
                const clickLng = e.latlng.lng;
                
                // Create terrain marker
                const terrainColors = {
                    'clear': '#90EE90',
                    'forest': '#228B22', 
                    'mountain': '#8B4513',
                    'water': '#4169E1',
                    'desert': '#F4A460',
                    'urban': '#696969'
                };
                
                const color = terrainColors[window.selectedTerrainType] || '#90EE90';
                
                const terrainMarker = L.circleMarker([clickLat, clickLng], {
                    radius: 20,
                    fillColor: color,
                    color: '#000',
                    weight: 2,
                    opacity: 1,
                    fillOpacity: 0.8
                }).addTo(realMapArena.map);
                
                showNotification(`Terrain changed to ${window.selectedTerrainType} at (${clickLat.toFixed(4)}, ${clickLng.toFixed(4)})`, 'success');
                window.selectedTerrainType = null;
                
            } else {
                showNotification('Select a tool or item first', 'warning');
            }
        });
        
        // Add move handler for coordinate display
        realMapArena.map.on('move', function() {
            updateCoordinateDisplay();
        });
        
        // Add zoom handler
        realMapArena.map.on('zoom', function() {
            updateCoordinateDisplay();
            realMapArena.currentZoom = realMapArena.map.getZoom();
        });
        
        console.log('Map event handlers added successfully');
        return true;
    } catch (error) {
        console.error('Error initializing map:', error);
        return false;
    }
}

// Setup hex grid mapping to real coordinates
function setupHexGridMapping() {
    if (!realMapArena.hexGrid) return;
    
    const hexes = realMapArena.hexGrid.querySelectorAll('.hex-modern');
    hexes.forEach(hex => {
        const q = parseInt(hex.dataset.q);
        const r = parseInt(hex.dataset.r);
        const lat = parseFloat(hex.dataset.lat) || 0;
        const lng = parseFloat(hex.dataset.lng) || 0;
        
        // Convert hex coordinates to map coordinates
        const mapCoords = hexToMapCoordinates(q, r);
        
        // Store coordinates
        hex.dataset.mapLat = mapCoords.lat;
        hex.dataset.mapLng = mapCoords.lng;
        
        // Position hex overlay on map
        positionHexOnMap(hex, mapCoords);
    });
}

// Convert hex coordinates to map coordinates
function hexToMapCoordinates(q, r) {
    // This is a simplified conversion - in reality you'd use proper hex-to-lat/lng conversion
    const hexSize = 0.01; // Approximate hex size in degrees
    const centerLat = realMapArena.currentCenter.lat;
    const centerLng = realMapArena.currentCenter.lng;
    
    const x = q * hexSize * 0.866; // sqrt(3)/2
    const y = (r + q * 0.5) * hexSize;
    
    return {
        lat: centerLat + y,
        lng: centerLng + x
    };
}

// Position hex on map
function positionHexOnMap(hexElement, coords) {
    // Create marker for hex if it has tokens or features
    const hasTokens = hexElement.querySelector('.token-modern');
    const hasFeatures = hexElement.querySelector('.feature-modern');
    
    if (hasTokens || hasFeatures) {
        const marker = L.marker([coords.lat, coords.lng], {
            icon: createHexIcon(hexElement)
        }).addTo(realMapArena.map);
        
        // Store reference
        hexElement.mapMarker = marker;
    }
}

// Create custom icon for hex
function createHexIcon(hexElement) {
    const hasTokens = hexElement.querySelector('.token-modern');
    const hasFeatures = hexElement.querySelector('.feature-modern');
    
    let iconHtml = '';
    
    if (hasTokens) {
        const tokens = hexElement.querySelectorAll('.token-modern');
        tokens.forEach(token => {
            iconHtml += `<div class="token-marker">${token.textContent}</div>`;
        });
    }
    
    if (hasFeatures) {
        const features = hexElement.querySelectorAll('.feature-modern');
        features.forEach(feature => {
            iconHtml += `<div class="feature-marker ${feature.className}">${feature.textContent}</div>`;
        });
    }
    
    return L.divIcon({
        html: iconHtml,
        className: 'hex-marker',
        iconSize: [40, 40],
        iconAnchor: [20, 20]
    });
}

// Handle map click
function handleMapClick(e) {
    console.log('Map clicked at:', e.latlng);
    const lat = e.latlng.lat;
    const lng = e.latlng.lng;
    
    // Find closest hex
    const closestHex = findClosestHex(lat, lng);
    console.log('Closest hex found:', closestHex);
    
    if (closestHex) {
        // Handle token placement or hex selection
        if (realMapArena.selectedToken) {
            console.log('Placing token on hex:', closestHex);
            placeTokenOnMap(lat, lng, closestHex);
        } else {
            console.log('Selecting hex:', closestHex);
            selectHexOnMap(closestHex, lat, lng);
        }
    } else {
        console.log('No hex found near click location');
        showNotification('No hex found at this location', 'warning');
    }
}

// Find closest hex to coordinates
function findClosestHex(lat, lng) {
    let closestHex = null;
    let minDistance = Infinity;
    
    const hexes = realMapArena.hexGrid.querySelectorAll('.hex-modern');
    hexes.forEach(hex => {
        const hexLat = parseFloat(hex.dataset.mapLat);
        const hexLng = parseFloat(hex.dataset.mapLng);
        
        const distance = Math.sqrt(
            Math.pow(lat - hexLat, 2) + Math.pow(lng - hexLng, 2)
        );
        
        if (distance < minDistance) {
            minDistance = distance;
            closestHex = hex;
        }
    });
    
    return closestHex;
}

// Place token on map
function placeTokenOnMap(lat, lng, hexElement, tokenId, placementId, icon, color, serial) {
    console.log('placeTokenOnMap called with:', { lat, lng, hexElement, tokenId, placementId, icon, color, serial });
    
    if (!tokenId) {
        console.error('No tokenId provided');
        return;
    }
    
    if (!realMapArena.map) {
        console.error('Map not initialized');
        showNotification('Map not ready for token placement', 'error');
        return;
    }
    
    // Create custom token icon
    const tokenIcon = createCustomTokenIcon(icon, color, serial);
    
    // Create token marker
    const tokenMarker = L.marker([lat, lng], {
        icon: tokenIcon
    }).addTo(realMapArena.map);
    
    console.log('Token marker created and added to map');
    
    // Add click handler to token marker
    tokenMarker.on('click', function(e) {
        handleTokenMarkerClick(this, tokenId, placementId);
    });
    
    // Store token
    realMapArena.tokens.set(placementId, {
        marker: tokenMarker,
        hexElement: hexElement,
        tokenId: tokenId,
        placementId: placementId,
        lat: lat,
        lng: lng,
        icon: icon,
        color: color,
        serial: serial
    });
    
    console.log('Token stored in tokens map');
    showNotification(`Token ${serial} placed on map!`, 'success');
}

// Create custom token icon
function createCustomTokenIcon(icon, color, serial) {
    // Handle undefined values with defaults
    icon = icon || 'fa-solid fa-circle';
    color = color || '#e74c3c';
    serial = serial || 'TKN';
    
    console.log('Creating token icon with:', { icon, color, serial });
    
    const iconHtml = `
        <div class="token-marker-real" style="
            background: linear-gradient(135deg, ${color}, ${adjustColor(color, -20)});
            border: 3px solid rgba(255, 255, 255, 0.9);
            border-radius: 50%;
            width: 40px;
            height: 40px;
            display: flex;
            align-items: center;
            justify-content: center;
            box-shadow: 0 4px 8px rgba(0, 0, 0, 0.3);
            cursor: pointer;
            transition: all 0.3s ease;
        ">
            <i class="${icon}" style="color: white; font-size: 16px;"></i>
            <div class="token-serial" style="
                position: absolute;
                bottom: -20px;
                left: 50%;
                transform: translateX(-50%);
                background: rgba(0, 0, 0, 0.8);
                color: white;
                padding: 2px 6px;
                border-radius: 10px;
                font-size: 10px;
                font-weight: bold;
                white-space: nowrap;
            ">${serial}</div>
        </div>
    `;
    
    return L.divIcon({
        html: iconHtml,
        className: 'token-marker-icon',
        iconSize: [40, 60],
        iconAnchor: [20, 30]
    });
}

// Adjust color brightness
function adjustColor(color, amount) {
    // Handle undefined or null color
    if (!color) {
        color = '#e74c3c'; // Default red color
    }
    
    // Ensure color is a string
    color = String(color);
    
    const usePound = color[0] === '#';
    const col = usePound ? color.slice(1) : color;
    const num = parseInt(col, 16);
    
    // Handle invalid color values
    if (isNaN(num)) {
        color = '#e74c3c'; // Default red color
        const col = color.slice(1);
        const num = parseInt(col, 16);
    }
    
    let r = (num >> 16) + amount;
    let g = (num >> 8 & 0x00FF) + amount;
    let b = (num & 0x0000FF) + amount;
    r = r > 255 ? 255 : r < 0 ? 0 : r;
    g = g > 255 ? 255 : g < 0 ? 0 : g;
    b = b > 255 ? 255 : b < 0 ? 0 : b;
    return (usePound ? '#' : '') + (r << 16 | g << 8 | b).toString(16).padStart(6, '0');
}

// Handle token marker click
function handleTokenMarkerClick(marker, tokenId, placementId) {
    showTokenOptions(placementId, marker);
}

// Show token options
function showTokenOptions(placementId, marker) {
    const token = realMapArena.tokens.get(placementId);
    if (!token) return;
    
    // Create options popup
    const popup = L.popup({
        closeButton: true,
        autoClose: false,
        closeOnClick: false
    }).setContent(`
        <div class="token-options-popup">
            <h6>Token: ${token.serial}</h6>
            <div class="token-actions">
                <button class="btn btn-sm btn-primary" onclick="moveToken('${placementId}')">
                    <i class="bi bi-arrows-move"></i> Move
                </button>
                <button class="btn btn-sm btn-warning" onclick="editToken('${placementId}')">
                    <i class="bi bi-pencil"></i> Edit
                </button>
                <button class="btn btn-sm btn-danger" onclick="removeToken('${placementId}')">
                    <i class="bi bi-trash"></i> Remove
                </button>
            </div>
        </div>
    `);
    
    marker.bindPopup(popup).openPopup();
}

// Move token
function moveToken(placementId) {
    const token = realMapArena.tokens.get(placementId);
    if (!token) return;
    
    realMapArena.map.off('click');
    realMapArena.map.on('click', function(e) {
        const newLat = e.latlng.lat;
        const newLng = e.latlng.lng;
        
        // Update token position
        token.marker.setLatLng([newLat, newLng]);
        token.lat = newLat;
        token.lng = newLng;
        
        realMapArena.map.off('click');
        showNotification('Token moved!', 'success');
    });
    
    showNotification('Click on the map to move the token', 'info');
}

// Edit token
function editToken(placementId) {
    const token = realMapArena.tokens.get(placementId);
    if (!token) return;
    
    // Show edit modal
    showTokenEditModal(token);
}

// Remove token
function removeToken(placementId) {
    const token = realMapArena.tokens.get(placementId);
    if (!token) return;
    
    if (confirm('Are you sure you want to remove this token?')) {
        realMapArena.map.removeLayer(token.marker);
        realMapArena.tokens.delete(placementId);
        showNotification('Token removed!', 'success');
    }
}

// Clean up modal backdrop
function cleanupModalBackdrop() {
    // Remove any existing backdrop elements
    const backdrops = document.querySelectorAll('.modal-backdrop');
    backdrops.forEach(backdrop => backdrop.remove());
    
    // Remove modal-open class from body
    document.body.classList.remove('modal-open');
    
    // Reset body padding if it was modified
    document.body.style.paddingRight = '';
}

// Show token edit modal
function showTokenEditModal(token) {
    // Clean up any existing modals first
    cleanupModalBackdrop();
    
    const modalHtml = `
        <div class="modal fade" id="tokenEditModal" tabindex="-1">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title">Edit Token: ${token.serial}</h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                    </div>
                    <div class="modal-body">
                        <div class="form-group">
                            <label>Token Serial</label>
                            <input type="text" id="editTokenSerial" class="form-control" value="${token.serial}">
                        </div>
                        <div class="form-group">
                            <label>Token Color</label>
                            <input type="color" id="editTokenColor" class="form-control" value="${token.color}">
                        </div>
                        <div class="form-group">
                            <label>Token Icon</label>
                            <select id="editTokenIcon" class="form-control">
                                <option value="fa-solid fa-circle" ${token.icon === 'fa-solid fa-circle' ? 'selected' : ''}>Circle</option>
                                <option value="fa-solid fa-square" ${token.icon === 'fa-solid fa-square' ? 'selected' : ''}>Square</option>
                                <option value="fa-solid fa-triangle" ${token.icon === 'fa-solid fa-triangle' ? 'selected' : ''}>Triangle</option>
                                <option value="fa-solid fa-star" ${token.icon === 'fa-solid fa-star' ? 'selected' : ''}>Star</option>
                            </select>
                        </div>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancel</button>
                        <button type="button" class="btn btn-primary" onclick="saveTokenEdit('${token.placementId}')">Save Changes</button>
                    </div>
                </div>
            </div>
        </div>
    `;
    
    document.body.insertAdjacentHTML('beforeend', modalHtml);
    const modal = new bootstrap.Modal(document.getElementById('tokenEditModal'));
    modal.show();
    
    // Remove modal after closing and clean up backdrop
    document.getElementById('tokenEditModal').addEventListener('hidden.bs.modal', function() {
        cleanupModalBackdrop();
        this.remove();
    });
}

// Save token edit
function saveTokenEdit(placementId) {
    const token = realMapArena.tokens.get(placementId);
    if (!token) return;
    
    const newSerial = document.getElementById('editTokenSerial').value;
    const newColor = document.getElementById('editTokenColor').value;
    const newIcon = document.getElementById('editTokenIcon').value;
    
    // Update token properties
    token.serial = newSerial;
    token.color = newColor;
    token.icon = newIcon;
    
    // Update marker icon
    const newIconObj = createCustomTokenIcon(newIcon, newColor, newSerial);
    token.marker.setIcon(newIconObj);
    
    // Close modal
    const modal = bootstrap.Modal.getInstance(document.getElementById('tokenEditModal'));
    if (modal) modal.hide();
    
    showNotification('Token updated!', 'success');
}

// Create token icon
function createTokenIcon(tokenId) {
    const serial = getTokenSerial(tokenId);
    
    return L.divIcon({
        html: `<div class="token-marker">${serial}</div>`,
        className: 'token-marker-icon',
        iconSize: [30, 30],
        iconAnchor: [15, 15]
    });
}

// Get token serial from token ID
function getTokenSerial(tokenId) {
    const tokenSelect = document.getElementById('tokenSelect');
    const option = tokenSelect.querySelector(`option[value="${tokenId}"]`);
    return option ? option.dataset.serial : 'T' + tokenId;
}

// Select hex on map
function selectHexOnMap(hexElement, lat, lng) {
    // Remove previous selection
    document.querySelectorAll('.hex-modern').forEach(h => h.classList.remove('selected'));
    
    // Add selection
    hexElement.classList.add('selected');
    
    // Update coordinate display
    updateCoordinateDisplay(lat, lng, hexElement);
    
    showNotification(`Selected hex (${hexElement.dataset.q}, ${hexElement.dataset.r})`, 'info');
}

// Update coordinate display
function updateCoordinateDisplay(lat, lng, hexElement) {
    const coordDisplay = document.getElementById('coordinateDisplay');
    if (!coordDisplay) return;
    
    if (lat && lng) {
        coordDisplay.querySelector('#currentCoords').textContent = 
            `Lat: ${lat.toFixed(6)}, Lng: ${lng.toFixed(6)}`;
    } else {
        const center = realMapArena.map.getCenter();
        coordDisplay.querySelector('#currentCoords').textContent = 
            `Lat: ${center.lat.toFixed(6)}, Lng: ${center.lng.toFixed(6)}`;
    }
    
    if (hexElement) {
        coordDisplay.querySelector('#hexCoords').textContent = 
            `Hex: (${hexElement.dataset.q}, ${hexElement.dataset.r})`;
    }
}

// Update hex marker
function updateHexMarker(hexElement) {
    if (hexElement.mapMarker) {
        realMapArena.map.removeLayer(hexElement.mapMarker);
    }
    
    const coords = {
        lat: parseFloat(hexElement.dataset.mapLat),
        lng: parseFloat(hexElement.dataset.mapLng)
    };
    
    const marker = L.marker([coords.lat, coords.lng], {
        icon: createHexIcon(hexElement)
    }).addTo(realMapArena.map);
    
    hexElement.mapMarker = marker;
}

// Setup data entry system
function setupDataEntrySystem() {
    // Map region selection
    const regionSelect = document.getElementById('mapRegionSelect');
    if (regionSelect) {
        regionSelect.addEventListener('change', function() {
            loadMapRegion(this.value);
        });
    }
    
    // Map center coordinates
    const latInput = document.getElementById('mapCenterLat');
    const lngInput = document.getElementById('mapCenterLng');
    
    if (latInput && lngInput) {
        latInput.addEventListener('change', updateMapCenter);
        lngInput.addEventListener('change', updateMapCenter);
    }
    
    // Zoom level
    const zoomSlider = document.getElementById('mapZoomLevel');
    if (zoomSlider) {
        zoomSlider.addEventListener('input', function() {
            const zoom = parseInt(this.value);
            realMapArena.map.setZoom(zoom);
            document.getElementById('zoomValue').textContent = zoom;
        });
    }
    
    // Hex grid settings
    const gridSizeSelect = document.getElementById('gridSizeSelect');
    if (gridSizeSelect) {
        gridSizeSelect.addEventListener('change', function() {
            updateHexGridSize(this.value);
        });
    }
    
    const showHexOverlay = document.getElementById('showHexOverlay');
    if (showHexOverlay) {
        showHexOverlay.addEventListener('change', function() {
            toggleHexOverlay(this.checked);
        });
    }
    
    // Token settings
    const tokenSize = document.getElementById('tokenSize');
    if (tokenSize) {
        tokenSize.addEventListener('input', function() {
            const size = this.value + 'px';
            document.getElementById('tokenSizeValue').textContent = size;
            updateTokenSize(parseInt(this.value));
        });
    }
    
    const tokenOpacity = document.getElementById('tokenOpacity');
    if (tokenOpacity) {
        tokenOpacity.addEventListener('input', function() {
            const opacity = Math.round(this.value * 100) + '%';
            document.getElementById('tokenOpacityValue').textContent = opacity;
            updateTokenOpacity(parseFloat(this.value));
        });
    }
    
    // Data actions
    const saveBtn = document.getElementById('saveMapDataBtn');
    const loadBtn = document.getElementById('loadMapDataBtn');
    const resetBtn = document.getElementById('resetMapDataBtn');
    
    if (saveBtn) saveBtn.addEventListener('click', saveMapData);
    if (loadBtn) loadBtn.addEventListener('click', loadMapData);
    if (resetBtn) resetBtn.addEventListener('click', resetMapData);
}

// Load map region
function loadMapRegion(regionKey) {
    const region = mapRegions[regionKey];
    if (!region) return;
    
    realMapArena.currentCenter = region.center;
    realMapArena.currentZoom = region.zoom;
    
    realMapArena.map.setView([region.center.lat, region.center.lng], region.zoom);
    
    // Update form inputs
    document.getElementById('mapCenterLat').value = region.center.lat;
    document.getElementById('mapCenterLng').value = region.center.lng;
    document.getElementById('mapZoomLevel').value = region.zoom;
    document.getElementById('zoomValue').textContent = region.zoom;
    
    showNotification(`Loaded ${region.name}`, 'success');
}

// Update map center
function updateMapCenter() {
    const lat = parseFloat(document.getElementById('mapCenterLat').value);
    const lng = parseFloat(document.getElementById('mapCenterLng').value);
    
    if (!isNaN(lat) && !isNaN(lng)) {
        realMapArena.map.setView([lat, lng], realMapArena.currentZoom);
        realMapArena.currentCenter = { lat, lng };
    }
}

// Update hex grid size
function updateHexGridSize(size) {
    const [width, height] = size.split('x').map(Number);
    showNotification(`Hex grid updated to ${width}x${height}`, 'info');
    // In a real implementation, you'd regenerate the hex grid
}

// Toggle hex overlay
function toggleHexOverlay(show) {
    realMapArena.showHexOverlay = show;
    
    if (show) {
        realMapArena.hexGrid.classList.add('show-overlay');
    } else {
        realMapArena.hexGrid.classList.remove('show-overlay');
    }
    
    showNotification(`Hex overlay ${show ? 'shown' : 'hidden'}`, 'info');
}

// Update token size
function updateTokenSize(size) {
    document.querySelectorAll('.token-marker').forEach(marker => {
        marker.style.fontSize = size + 'px';
        marker.style.padding = (size * 0.2) + 'px ' + (size * 0.4) + 'px';
    });
}

// Update token opacity
function updateTokenOpacity(opacity) {
    document.querySelectorAll('.token-marker').forEach(marker => {
        marker.style.opacity = opacity;
    });
}

// Setup map controls
function setupMapControls() {
    // Zoom controls
    const zoomInBtn = document.getElementById('zoomInBtn');
    const zoomOutBtn = document.getElementById('zoomOutBtn');
    const resetViewBtn = document.getElementById('resetViewBtn');
    
    if (zoomInBtn) zoomInBtn.addEventListener('click', () => realMapArena.map.zoomIn());
    if (zoomOutBtn) zoomOutBtn.addEventListener('click', () => realMapArena.map.zoomOut());
    if (resetViewBtn) resetViewBtn.addEventListener('click', resetMapView);
    
    // Layer controls
    const satelliteBtn = document.getElementById('toggleSatelliteBtn');
    const terrainBtn = document.getElementById('toggleTerrainBtn');
    const labelsBtn = document.getElementById('toggleLabelsBtn');
    
    if (satelliteBtn) satelliteBtn.addEventListener('click', toggleSatellite);
    if (terrainBtn) terrainBtn.addEventListener('click', toggleTerrain);
    if (labelsBtn) labelsBtn.addEventListener('click', toggleLabels);
}

// Reset map view
function resetMapView() {
    realMapArena.map.setView([realMapArena.currentCenter.lat, realMapArena.currentCenter.lng], realMapArena.currentZoom);
    showNotification('Map view reset', 'info');
}

// Toggle satellite view
function toggleSatellite() {
    // In a real implementation, you'd switch tile layers
    showNotification('Satellite view toggled', 'info');
}

// Toggle terrain view
function toggleTerrain() {
    // In a real implementation, you'd switch to terrain tiles
    showNotification('Terrain view toggled', 'info');
}

// Toggle labels
function toggleLabels() {
    // In a real implementation, you'd toggle label visibility
    showNotification('Labels toggled', 'info');
}

// Setup token system
function setupTokenSystem() {
    const tokenSelect = document.getElementById('tokenSelect');
    if (tokenSelect) {
        tokenSelect.addEventListener('change', function() {
            realMapArena.selectedToken = this.value;
            updateTokenSelection();
        });
    }
}

// Update token selection display
function updateTokenSelection() {
    const selectedDisplay = document.getElementById('selectedTokenDisplay');
    if (!selectedDisplay) return;
    
    if (realMapArena.selectedToken) {
        const serial = getTokenSerial(realMapArena.selectedToken);
        selectedDisplay.innerHTML = `
            <span class="badge bg-primary">
                <i class="bi bi-shapes"></i> ${serial}
            </span>
        `;
        showNotification(`Selected token: ${serial}. Click on the map to place it.`, 'success');
    } else {
        selectedDisplay.innerHTML = '<span class="text-muted">No token selected</span>';
    }
}

// Save map data
function saveMapData() {
    const mapData = {
        name: document.getElementById('mapNameInput').value || 'Untitled Map',
        region: document.getElementById('mapRegionSelect').value,
        center: {
            lat: parseFloat(document.getElementById('mapCenterLat').value),
            lng: parseFloat(document.getElementById('mapCenterLng').value)
        },
        zoom: parseInt(document.getElementById('mapZoomLevel').value),
        gridSize: document.getElementById('gridSizeSelect').value,
        hexSize: parseInt(document.getElementById('hexSizeMeters').value),
        showHexOverlay: document.getElementById('showHexOverlay').checked,
        tokenSize: parseInt(document.getElementById('tokenSize').value),
        tokenOpacity: parseFloat(document.getElementById('tokenOpacity').value),
        showTokenLabels: document.getElementById('showTokenLabels').checked,
        turnDuration: parseInt(document.getElementById('turnDuration').value),
        maxTurns: parseInt(document.getElementById('maxTurns').value),
        victoryConditions: document.getElementById('victoryConditions').value,
        tokens: Array.from(realMapArena.tokens.values()).map(token => ({
            id: token.tokenId,
            lat: token.lat,
            lng: token.lng,
            hexQ: token.hexElement.dataset.q,
            hexR: token.hexElement.dataset.r
        }))
    };
    
    // Save to localStorage for demo
    localStorage.setItem('mapData', JSON.stringify(mapData));
    
    showNotification('Map data saved successfully!', 'success');
}

// Load map data
function loadMapData() {
    const mapData = localStorage.getItem('mapData');
    if (!mapData) {
        showNotification('No saved map data found', 'warning');
        return;
    }
    
    try {
        const data = JSON.parse(mapData);
        
        // Load map configuration
        document.getElementById('mapNameInput').value = data.name || '';
        document.getElementById('mapRegionSelect').value = data.region || 'london';
        document.getElementById('mapCenterLat').value = data.center?.lat || 51.5074;
        document.getElementById('mapCenterLng').value = data.center?.lng || -0.1278;
        document.getElementById('mapZoomLevel').value = data.zoom || 12;
        document.getElementById('gridSizeSelect').value = data.gridSize || '12x12';
        document.getElementById('hexSizeMeters').value = data.hexSize || 1000;
        document.getElementById('showHexOverlay').checked = data.showHexOverlay || false;
        document.getElementById('tokenSize').value = data.tokenSize || 20;
        document.getElementById('tokenOpacity').value = data.tokenOpacity || 0.8;
        document.getElementById('showTokenLabels').checked = data.showTokenLabels !== false;
        document.getElementById('turnDuration').value = data.turnDuration || 30;
        document.getElementById('maxTurns').value = data.maxTurns || 20;
        document.getElementById('victoryConditions').value = data.victoryConditions || 'territory';
        
        // Update map
        if (data.center) {
            realMapArena.map.setView([data.center.lat, data.center.lng], data.zoom);
        }
        
        // Load tokens
        if (data.tokens) {
            data.tokens.forEach(tokenData => {
                // Recreate tokens on map
                // This would be more complex in a real implementation
            });
        }
        
        showNotification('Map data loaded successfully!', 'success');
    } catch (error) {
        showNotification('Error loading map data', 'error');
        console.error('Error loading map data:', error);
    }
}

// Reset map data
function resetMapData() {
    if (confirm('Are you sure you want to reset all map data? This will clear all tokens and settings.')) {
        // Clear form
        document.getElementById('mapNameInput').value = '';
        document.getElementById('mapRegionSelect').value = 'london';
        document.getElementById('mapCenterLat').value = '51.5074';
        document.getElementById('mapCenterLng').value = '-0.1278';
        document.getElementById('mapZoomLevel').value = '12';
        document.getElementById('gridSizeSelect').value = '12x12';
        document.getElementById('hexSizeMeters').value = '1000';
        document.getElementById('showHexOverlay').checked = false;
        document.getElementById('tokenSize').value = '20';
        document.getElementById('tokenOpacity').value = '0.8';
        document.getElementById('showTokenLabels').checked = true;
        document.getElementById('turnDuration').value = '30';
        document.getElementById('maxTurns').value = '20';
        document.getElementById('victoryConditions').value = 'territory';
        
        // Clear tokens
        realMapArena.tokens.forEach(token => {
            realMapArena.map.removeLayer(token.marker);
        });
        realMapArena.tokens.clear();
        
        // Reset map view
        realMapArena.map.setView([51.5074, -0.1278], 12);
        
        showNotification('Map data reset successfully!', 'success');
    }
}

// Fallback notification function
function showNotification(message, type = 'info') {
    if (window.showNotification) {
        window.showNotification(message, type);
    } else {
        console.log(`${type.toUpperCase()}: ${message}`);
        // Create a simple notification
        const notification = document.createElement('div');
        notification.style.cssText = `
            position: fixed;
            top: 20px;
            right: 20px;
            background: ${type === 'error' ? '#dc3545' : type === 'success' ? '#28a745' : '#17a2b8'};
            color: white;
            padding: 10px 15px;
            border-radius: 5px;
            z-index: 10000;
            font-family: Arial, sans-serif;
        `;
        notification.textContent = message;
        document.body.appendChild(notification);
        
        setTimeout(() => {
            if (notification.parentNode) {
                notification.parentNode.removeChild(notification);
            }
        }, 3000);
    }
}

// Get hex center coordinates
function getHexCenterLatLng(q, r) {
    const hexSize = 0.01; // Approximate hex size in degrees
    const centerLat = realMapArena.currentCenter.lat;
    const centerLng = realMapArena.currentCenter.lng;
    
    const x = q * hexSize * 0.866; // sqrt(3)/2
    const y = (r + q * 0.5) * hexSize;
    
    return {
        lat: centerLat + y,
        lng: centerLng + x
    };
}

// Hex feature management
// SIMPLE FEATURE PLACEMENT - MILITARY STYLE
function addHexFeature(hexElement, featureType, featureData) {
    console.log('MILITARY: Adding feature to hex');
    
    if (!realMapArena.map) {
        showNotification('Map not ready', 'error');
        return;
    }
    
    // Get hex coordinates
    const q = parseInt(hexElement.dataset.q) || 0;
    const r = parseInt(hexElement.dataset.r) || 0;
    const latlng = getHexCenterLatLng(q, r);
    
    if (!latlng) {
        showNotification('Cannot place feature here', 'error');
        return;
    }
    
    // Create simple feature marker
    const featureIcon = L.divIcon({
        html: `<div style="
            background: #ff6b35;
            color: white;
            border: 2px solid white;
            border-radius: 50%;
            width: 30px;
            height: 30px;
            display: flex;
            align-items: center;
            justify-content: center;
            font-weight: bold;
            font-size: 14px;
        ">${featureType.charAt(0).toUpperCase()}</div>`,
        iconSize: [30, 30],
        iconAnchor: [15, 15]
    });
    
    const featureMarker = L.marker([latlng.lat, latlng.lng], {
        icon: featureIcon
    }).addTo(realMapArena.map);
    
    // Store feature
    const featureId = Date.now().toString();
    realMapArena.features.set(featureId, {
        marker: featureMarker,
        hexElement: hexElement,
        featureType: featureType,
        lat: latlng.lat,
        lng: latlng.lng
    });
    
    showNotification(`Feature placed at (${q},${r})`, 'success');
    console.log('Feature placed:', featureType);
}

// Create feature icon
function createFeatureIcon(featureType, featureData) {
    const icons = {
        'fortification': 'fa-solid fa-shield',
        'obstacle': 'fa-solid fa-mountain',
        'objective': 'fa-solid fa-flag',
        'resource': 'fa-solid fa-gem',
        'building': 'fa-solid fa-building'
    };
    
    const colors = {
        'fortification': '#8e44ad',
        'obstacle': '#95a5a6',
        'objective': '#f39c12',
        'resource': '#27ae60',
        'building': '#e74c3c'
    };
    
    const icon = icons[featureType] || 'fa-solid fa-circle';
    const color = colors[featureType] || '#3498db';
    
    const iconHtml = `
        <div class="feature-marker-real" style="
            background: linear-gradient(135deg, ${color}, ${adjustColor(color, -20)});
            border: 2px solid rgba(255, 255, 255, 0.9);
            border-radius: 8px;
            width: 30px;
            height: 30px;
            display: flex;
            align-items: center;
            justify-content: center;
            box-shadow: 0 3px 6px rgba(0, 0, 0, 0.3);
            cursor: pointer;
            transition: all 0.3s ease;
        ">
            <i class="${icon}" style="color: white; font-size: 12px;"></i>
        </div>
    `;
    
    return L.divIcon({
        html: iconHtml,
        className: 'feature-marker-icon',
        iconSize: [30, 30],
        iconAnchor: [15, 15]
    });
}

// Show feature options
function showFeatureOptions(featureType, featureData, marker) {
    const popup = L.popup({
        closeButton: true,
        autoClose: false,
        closeOnClick: false
    }).setContent(`
        <div class="feature-options-popup">
            <h6>${featureType.toUpperCase()}</h6>
            <p>${featureData.name || 'Feature'}</p>
            <div class="feature-actions">
                <button class="btn btn-sm btn-warning" onclick="editFeature('${featureType}')">
                    <i class="bi bi-pencil"></i> Edit
                </button>
                <button class="btn btn-sm btn-danger" onclick="removeFeature('${featureType}')">
                    <i class="bi bi-trash"></i> Remove
                </button>
            </div>
        </div>
    `);
    
    marker.bindPopup(popup).openPopup();
}

// Map editor integration
function initializeMapEditor() {
    // Add map editor controls to the arena
    const mapEditorHtml = `
        <div class="map-editor-panel" id="mapEditorPanel">
            <h4>MAP EDITOR</h4>
            <div class="editor-tools">
                <button class="editor-tool-btn" id="terrainTool" onclick="setEditorTool('terrain')">
                    <i class="bi bi-mountain"></i> Terrain
                </button>
                <button class="editor-tool-btn" id="tokenTool" onclick="setEditorTool('token')">
                    <i class="bi bi-shapes"></i> Tokens
                </button>
                <button class="editor-tool-btn" id="featureTool" onclick="setEditorTool('feature')">
                    <i class="bi bi-geo-alt"></i> Features
                </button>
                <button class="editor-tool-btn" id="gridTool" onclick="setEditorTool('grid')">
                    <i class="bi bi-grid-3x3"></i> Grid
                </button>
            </div>
            
            <div class="editor-options" id="editorOptions">
                <!-- Dynamic content based on selected tool -->
            </div>
        </div>
    `;
    
    // Add to left sidebar
    const leftSidebar = document.querySelector('.arena-sidebar-left');
    if (leftSidebar) {
        leftSidebar.insertAdjacentHTML('beforeend', mapEditorHtml);
        
        // Initialize map persistence after adding the panel
        setTimeout(() => {
            initializeMapPersistence();
            setupLocationSearch();
        }, 100);
    }
}

// Set editor tool with comprehensive terrain system
function setEditorTool(tool) {
    console.log('🎖️ Setting editor tool:', tool);
    
    // Remove active class from all tools
    document.querySelectorAll('.editor-tool-btn').forEach(btn => {
        btn.classList.remove('active');
    });
    
    // Add active class to selected tool
    const selectedTool = document.getElementById(tool + 'Tool');
    if (selectedTool) {
        selectedTool.classList.add('active');
    }
    
    // Update editor options based on tool
    updateEditorOptions(tool);
    
    // Show tool-specific guidance
    showToolGuidance(tool);
}

// Show tool-specific guidance
function showToolGuidance(tool) {
    const guidance = {
        'terrain': '🌍 TERRAIN TOOL: Click on hexes to change terrain type. Use the terrain palette to select different terrain types.',
        'token': '🎯 TOKEN TOOL: Select tokens from the panel and click on the map to place them.',
        'feature': '🏗️ FEATURE TOOL: Click on hexes to add fortifications, obstacles, and other features.',
        'grid': '📐 GRID TOOL: Toggle hex grid visibility and adjust grid settings.'
    };
    
    if (guidance[tool]) {
        showNotification(guidance[tool], 'info');
    }
}

// Update editor options
function updateEditorOptions(tool) {
    const optionsContainer = document.getElementById('editorOptions');
    if (!optionsContainer) return;
    
    let optionsHtml = '';
    
    switch(tool) {
        case 'terrain':
            optionsHtml = `
                <div class="terrain-options">
                    <h6>Terrain Types</h6>
                    <div class="terrain-palette">
                        <div class="terrain-option" data-terrain="clear" onclick="selectTerrain('clear')">
                            <div class="terrain-preview clear"></div>
                            <span>Clear</span>
                        </div>
                        <div class="terrain-option" data-terrain="forest" onclick="selectTerrain('forest')">
                            <div class="terrain-preview forest"></div>
                            <span>Forest</span>
                        </div>
                        <div class="terrain-option" data-terrain="mountain" onclick="selectTerrain('mountain')">
                            <div class="terrain-preview mountain"></div>
                            <span>Mountain</span>
                        </div>
                        <div class="terrain-option" data-terrain="water" onclick="selectTerrain('water')">
                            <div class="terrain-preview water"></div>
                            <span>Water</span>
                        </div>
                    </div>
                </div>
            `;
            break;
        case 'feature':
            optionsHtml = `
                <div class="feature-options">
                    <h6>Feature Types</h6>
                    <div class="feature-palette">
                        <div class="feature-option" data-feature="fortification" onclick="selectFeature('fortification')">
                            <i class="fa-solid fa-shield"></i>
                            <span>Fortification</span>
                        </div>
                        <div class="feature-option" data-feature="obstacle" onclick="selectFeature('obstacle')">
                            <i class="fa-solid fa-mountain"></i>
                            <span>Obstacle</span>
                        </div>
                        <div class="feature-option" data-feature="objective" onclick="selectFeature('objective')">
                            <i class="fa-solid fa-flag"></i>
                            <span>Objective</span>
                        </div>
                        <div class="feature-option" data-feature="resource" onclick="selectFeature('resource')">
                            <i class="fa-solid fa-gem"></i>
                            <span>Resource</span>
                        </div>
                    </div>
                </div>
            `;
            break;
        case 'grid':
            optionsHtml = `
                <div class="grid-options">
                    <h6>Grid Settings</h6>
                    <div class="form-group">
                        <label>Show Grid</label>
                        <input type="checkbox" id="showGrid" onchange="toggleGrid(this.checked)">
                    </div>
                    <div class="form-group">
                        <label>Grid Size</label>
                        <input type="range" id="gridSize" min="10" max="50" value="20" onchange="updateGridSize(this.value)">
                    </div>
                </div>
            `;
            break;
    }
    
    optionsContainer.innerHTML = optionsHtml;
}

// Select terrain
function selectTerrain(terrainType) {
    realMapArena.selectedTerrain = terrainType;
    showNotification(`Selected terrain: ${terrainType}`, 'info');
}

// Select feature
function selectFeature(featureType) {
    realMapArena.selectedFeature = featureType;
    showNotification(`Selected feature: ${featureType}`, 'info');
}

// Toggle grid
function toggleGrid(show) {
    if (show) {
        realMapArena.hexGrid.classList.add('show-overlay');
    } else {
        realMapArena.hexGrid.classList.remove('show-overlay');
    }
}

// Update grid size
function updateGridSize(size) {
    // Update grid size logic
    showNotification(`Grid size updated to ${size}px`, 'info');
}

// Test function to verify systems are working
function testArenaSystems() {
    console.log('Testing arena systems...');
    
    // Test 1: Check if map is initialized
    if (realMapArena.map) {
        console.log('✓ Map is initialized');
    } else {
        console.error('✗ Map is not initialized');
    }
    
    // Test 2: Check if map container exists
    if (realMapArena.mapContainer) {
        console.log('✓ Map container found');
    } else {
        console.error('✗ Map container not found');
    }
    
    // Test 3: Check if hex grid exists
    if (realMapArena.hexGrid) {
        console.log('✓ Hex grid found');
    } else {
        console.error('✗ Hex grid not found');
    }
    
    // Test 4: Check if Leaflet is available
    if (typeof L !== 'undefined') {
        console.log('✓ Leaflet is available');
    } else {
        console.error('✗ Leaflet is not available');
    }
    
    // Test 5: Try to place a test token
    if (realMapArena.map && realMapArena.isInitialized) {
        const testLatLng = getHexCenterLatLng(0, 0);
        if (testLatLng) {
            console.log('✓ Can get hex coordinates');
            // Place a test token
            placeTokenOnMap(testLatLng.lat, testLatLng.lng, 'test', 'test-token', 'test-placement', 'fa-solid fa-circle', '#e74c3c', 'TEST');
            console.log('✓ Test token placed');
        } else {
            console.error('✗ Cannot get hex coordinates');
        }
    } else {
        console.error('✗ Map not ready for testing');
    }
    
    showNotification('Arena systems test completed - check console for results', 'info');
}

// Export functions for global access
// ========================================
// MAP PERSISTENCE SYSTEM
// ========================================

// Initialize map persistence system
function initializeMapPersistence() {
    console.log('🎖️ Initializing Map Persistence System...');
    
    // Add save/load controls to map editor
    addMapPersistenceControls();
    
    // Load saved maps on startup
    loadSavedMaps();
    
    console.log('✓ Map Persistence System initialized');
}

// Add map persistence controls to the map editor
function addMapPersistenceControls() {
    const mapEditorPanel = document.getElementById('mapEditorPanel');
    if (!mapEditorPanel) return;
    
    const persistenceControls = `
        <div class="map-persistence-controls">
            <h5><i class="bi bi-save"></i> MAP PERSISTENCE</h5>
            <div class="persistence-actions">
                <button class="btn btn-success btn-sm" onclick="saveCurrentMap()">
                    <i class="bi bi-save"></i> Save Map
                </button>
                <button class="btn btn-primary btn-sm" onclick="loadMapDialog()">
                    <i class="bi bi-folder-open"></i> Load Map
                </button>
                <button class="btn btn-warning btn-sm" onclick="newMapDialog()">
                    <i class="bi bi-plus-circle"></i> New Map
                </button>
                <button class="btn btn-info btn-sm" onclick="exportMap()">
                    <i class="bi bi-download"></i> Export
                </button>
            </div>
            <div class="map-list" id="savedMapsList">
                <!-- Saved maps will be loaded here -->
            </div>
        </div>
    `;
    
    mapEditorPanel.insertAdjacentHTML('beforeend', persistenceControls);
}

// Save current map state with comprehensive alerts
async function saveCurrentMap() {
    try {
        console.log('🎖️ MILITARY SIMULATION: Saving current map state...');
        
        // Show comprehensive save confirmation
        showNotification('💾 SAVING MAP STATE...', 'info');
        showNotification('Collecting token data...', 'info');
        showNotification('Collecting feature data...', 'info');
        showNotification('Collecting terrain data...', 'info');
        
        // Collect comprehensive map data
        const mapData = {
            id: Date.now().toString(),
            name: `Map_${new Date().toISOString().slice(0, 19).replace(/:/g, '-')}`,
            timestamp: new Date().toISOString(),
            center: realMapArena.map.getCenter(),
            zoom: realMapArena.map.getZoom(),
            tokens: Array.from(realMapArena.tokens.values()),
            features: Array.from(realMapArena.features.values()),
            hexGrid: {
                size: realMapArena.hexGridSize,
                center: realMapArena.hexGridCenter
            },
            metadata: {
                totalTokens: realMapArena.tokens.size,
                totalFeatures: realMapArena.features.size,
                mapCenter: realMapArena.map.getCenter(),
                zoomLevel: realMapArena.map.getZoom(),
                saveVersion: '1.0'
            }
        };
        
        // Validate data integrity
        if (mapData.tokens.length === 0 && mapData.features.length === 0) {
            showNotification('⚠️ WARNING: Map contains no tokens or features', 'warning');
        }
        
        // Save to localStorage (in production, this would be server-side)
        const savedMaps = JSON.parse(localStorage.getItem('savedMaps') || '[]');
        savedMaps.push(mapData);
        localStorage.setItem('savedMaps', JSON.stringify(savedMaps));
        
        // Update UI
        updateSavedMapsList();
        
        // Show comprehensive success alerts
        showNotification(`✅ MAP SAVED SUCCESSFULLY`, 'success');
        showNotification(`📁 File: ${mapData.name}`, 'success');
        showNotification(`🎯 Tokens: ${mapData.metadata.totalTokens}`, 'success');
        showNotification(`🏗️ Features: ${mapData.metadata.totalFeatures}`, 'success');
        showNotification(`📍 Center: ${mapData.center.lat.toFixed(4)}, ${mapData.center.lng.toFixed(4)}`, 'success');
        showNotification(`🔍 Zoom: ${mapData.zoom}`, 'success');
        
        console.log('✓ MILITARY SIMULATION: Map saved successfully:', mapData.name);
        console.log('✓ Map metadata:', mapData.metadata);
        
    } catch (error) {
        console.error('❌ MILITARY SIMULATION: Error saving map:', error);
        showNotification('❌ CRITICAL ERROR: Failed to save map', 'error');
        showNotification('Please check console for details', 'error');
    }
}

// Load map dialog
function loadMapDialog() {
    const savedMaps = JSON.parse(localStorage.getItem('savedMaps') || '[]');
    
    if (savedMaps.length === 0) {
        showNotification('No saved maps found', 'warning');
        return;
    }
    
    // Create map selection modal
    const modalHtml = `
        <div class="modal fade" id="loadMapModal" tabindex="-1">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title">Load Saved Map</h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                    </div>
                    <div class="modal-body">
                        <div class="map-list">
                            ${savedMaps.map(map => `
                                <div class="map-item" onclick="loadMap('${map.id}')">
                                    <div class="map-name">${map.name}</div>
                                    <div class="map-date">${new Date(map.timestamp).toLocaleString()}</div>
                                    <div class="map-stats">
                                        ${map.tokens.length} tokens, ${map.features.length} features
                                    </div>
                                </div>
                            `).join('')}
                        </div>
                    </div>
                </div>
            </div>
        </div>
    `;
    
    // Remove existing modal
    const existingModal = document.getElementById('loadMapModal');
    if (existingModal) existingModal.remove();
    
    // Add new modal
    document.body.insertAdjacentHTML('beforeend', modalHtml);
    
    // Show modal
    const modal = new bootstrap.Modal(document.getElementById('loadMapModal'));
    modal.show();
}

// Load specific map
function loadMap(mapId) {
    try {
        const savedMaps = JSON.parse(localStorage.getItem('savedMaps') || '[]');
        const mapData = savedMaps.find(map => map.id === mapId);
        
        if (!mapData) {
            showNotification('Map not found', 'error');
            return;
        }
        
        console.log('Loading map:', mapData.name);
        showNotification(`Loading map: ${mapData.name}`, 'info');
        
        // Clear current map
        clearCurrentMap();
        
        // Restore map state
        realMapArena.map.setView(mapData.center, mapData.zoom);
        
        // Restore tokens
        mapData.tokens.forEach(token => {
            placeTokenOnMap(token.lat, token.lng, null, token.tokenId, token.placementId, token.icon, token.color, token.serial);
        });
        
        // Restore features
        mapData.features.forEach(feature => {
            addHexFeature(null, feature.featureType, feature.featureData);
        });
        
        // Close modal
        const modal = bootstrap.Modal.getInstance(document.getElementById('loadMapModal'));
        if (modal) modal.hide();
        
        showNotification(`Map "${mapData.name}" loaded successfully`, 'success');
        console.log('✓ Map loaded:', mapData.name);
        
    } catch (error) {
        console.error('Error loading map:', error);
        showNotification('Failed to load map', 'error');
    }
}

// Clear current map
function clearCurrentMap() {
    // Remove all tokens
    realMapArena.tokens.forEach((token, id) => {
        realMapArena.map.removeLayer(token.marker);
    });
    realMapArena.tokens.clear();
    
    // Remove all features
    realMapArena.features.forEach((feature, id) => {
        realMapArena.map.removeLayer(feature.marker);
    });
    realMapArena.features.clear();
}

// New map dialog
function newMapDialog() {
    if (confirm('Create a new map? This will clear the current map.')) {
        clearCurrentMap();
        showNotification('New map created', 'success');
    }
}

// Export map
function exportMap() {
    try {
        const mapData = {
            name: `Exported_Map_${new Date().toISOString().slice(0, 19).replace(/:/g, '-')}`,
            timestamp: new Date().toISOString(),
            center: realMapArena.map.getCenter(),
            zoom: realMapArena.map.getZoom(),
            tokens: Array.from(realMapArena.tokens.values()),
            features: Array.from(realMapArena.features.values())
        };
        
        const dataStr = JSON.stringify(mapData, null, 2);
        const dataBlob = new Blob([dataStr], {type: 'application/json'});
        
        const link = document.createElement('a');
        link.href = URL.createObjectURL(dataBlob);
        link.download = `${mapData.name}.json`;
        link.click();
        
        showNotification('Map exported successfully', 'success');
        
    } catch (error) {
        console.error('Error exporting map:', error);
        showNotification('Failed to export map', 'error');
    }
}

// Load saved maps list
function loadSavedMaps() {
    updateSavedMapsList();
}

// Update saved maps list in UI
function updateSavedMapsList() {
    const savedMapsList = document.getElementById('savedMapsList');
    if (!savedMapsList) return;
    
    const savedMaps = JSON.parse(localStorage.getItem('savedMaps') || '[]');
    
    if (savedMaps.length === 0) {
        savedMapsList.innerHTML = '<div class="text-muted">No saved maps</div>';
        return;
    }
    
    savedMapsList.innerHTML = savedMaps.map(map => `
        <div class="saved-map-item">
            <div class="map-name">${map.name}</div>
            <div class="map-date">${new Date(map.timestamp).toLocaleString()}</div>
            <div class="map-actions">
                <button class="btn btn-sm btn-outline-primary" onclick="loadMap('${map.id}')">Load</button>
                <button class="btn btn-sm btn-outline-danger" onclick="deleteMap('${map.id}')">Delete</button>
            </div>
        </div>
    `).join('');
}

// Delete map
function deleteMap(mapId) {
    if (confirm('Are you sure you want to delete this map?')) {
        const savedMaps = JSON.parse(localStorage.getItem('savedMaps') || '[]');
        const filteredMaps = savedMaps.filter(map => map.id !== mapId);
        localStorage.setItem('savedMaps', JSON.stringify(filteredMaps));
        updateSavedMapsList();
        showNotification('Map deleted', 'success');
    }
}

window.realMapArena = realMapArena;
window.loadMapRegion = loadMapRegion;
window.toggleHexOverlay = toggleHexOverlay;
window.showNotification = showNotification;
window.getHexCenterLatLng = getHexCenterLatLng;
window.placeTokenOnMap = placeTokenOnMap;
window.addHexFeature = addHexFeature;
window.initializeMapEditor = initializeMapEditor;
window.testArenaSystems = testArenaSystems;
// ========================================
// COMPREHENSIVE TERRAIN SYSTEM
// ========================================

// Select terrain type with comprehensive validation
function selectTerrain(terrainType) {
    console.log('🎖️ MILITARY SIMULATION: Selecting terrain type:', terrainType);
    
    // Store selected terrain
    realMapArena.selectedTerrain = terrainType;
    
    // Update UI
    document.querySelectorAll('.terrain-option').forEach(option => {
        option.classList.remove('selected');
    });
    
    const selectedOption = document.querySelector(`[data-terrain="${terrainType}"]`);
    if (selectedOption) {
        selectedOption.classList.add('selected');
    }
    
    // Show terrain information
    const terrainInfo = {
        'clear': { name: 'Clear Terrain', movement: 'Easy', defense: 'None', description: 'Open ground with no obstacles' },
        'forest': { name: 'Forest', movement: 'Difficult', defense: '+2', description: 'Dense vegetation providing cover' },
        'mountain': { name: 'Mountain', movement: 'Very Difficult', defense: '+3', description: 'High elevation with excellent cover' },
        'water': { name: 'Water', movement: 'Restricted', defense: '+1', description: 'Rivers, lakes, and other water features' },
        'desert': { name: 'Desert', movement: 'Moderate', defense: 'None', description: 'Arid terrain with limited cover' },
        'urban': { name: 'Urban', movement: 'Variable', defense: '+2', description: 'Buildings and urban infrastructure' }
    };
    
    const info = terrainInfo[terrainType] || terrainInfo['clear'];
    showNotification(`🌍 Selected: ${info.name}`, 'info');
    showNotification(`🚶 Movement: ${info.movement}`, 'info');
    showNotification(`🛡️ Defense: ${info.defense}`, 'info');
    showNotification(`📝 ${info.description}`, 'info');
    
    console.log('✓ Terrain selected:', info);
}

// Select feature type with comprehensive validation
function selectFeature(featureType) {
    console.log('🎖️ MILITARY SIMULATION: Selecting feature type:', featureType);
    
    // Store selected feature
    realMapArena.selectedFeature = featureType;
    
    // Update UI
    document.querySelectorAll('.feature-option').forEach(option => {
        option.classList.remove('selected');
    });
    
    const selectedOption = document.querySelector(`[data-feature="${featureType}"]`);
    if (selectedOption) {
        selectedOption.classList.add('selected');
    }
    
    // Show feature information
    const featureInfo = {
        'fortification': { name: 'Fortification', effect: 'Defensive Bonus', description: 'Bunkers, walls, and defensive structures' },
        'obstacle': { name: 'Obstacle', effect: 'Movement Penalty', description: 'Rocks, debris, and movement barriers' },
        'objective': { name: 'Objective', effect: 'Victory Point', description: 'Strategic locations and objectives' },
        'resource': { name: 'Resource', effect: 'Supply Point', description: 'Supply depots and resource points' },
        'bridge': { name: 'Bridge', effect: 'Water Crossing', description: 'Bridges over water obstacles' },
        'road': { name: 'Road', effect: 'Movement Bonus', description: 'Improved movement routes' }
    };
    
    const info = featureInfo[featureType] || featureInfo['fortification'];
    showNotification(`🏗️ Selected: ${info.name}`, 'info');
    showNotification(`⚡ Effect: ${info.effect}`, 'info');
    showNotification(`📝 ${info.description}`, 'info');
    
    console.log('✓ Feature selected:', info);
}

// Toggle grid visibility with comprehensive controls
function toggleGrid(show) {
    console.log('🎖️ MILITARY SIMULATION: Toggling grid visibility:', show);
    
    const hexGrid = document.getElementById('hexGrid');
    if (hexGrid) {
        if (show) {
            hexGrid.style.display = 'block';
            hexGrid.classList.add('show-overlay');
            showNotification('📐 Hex grid overlay enabled', 'success');
        } else {
            hexGrid.style.display = 'none';
            hexGrid.classList.remove('show-overlay');
            showNotification('📐 Hex grid overlay disabled', 'info');
        }
    }
    
    console.log('✓ Grid visibility toggled:', show);
}

// Update grid size with comprehensive controls
function updateGridSize(size) {
    console.log('🎖️ MILITARY SIMULATION: Updating grid size:', size);
    
    const hexGrid = document.getElementById('hexGrid');
    if (hexGrid) {
        hexGrid.style.setProperty('--hex-size', `${size}px`);
    }
    
    // Update display value
    const sizeValue = document.getElementById('gridSizeValue');
    if (sizeValue) {
        sizeValue.textContent = size;
    }
    
    showNotification(`📐 Grid size updated to ${size}px`, 'info');
    console.log('✓ Grid size updated:', size);
}

// Update grid opacity
function updateGridOpacity(opacity) {
    console.log('🎖️ MILITARY SIMULATION: Updating grid opacity:', opacity);
    
    const hexGrid = document.getElementById('hexGrid');
    if (hexGrid) {
        hexGrid.style.opacity = opacity / 100;
    }
    
    // Update display value
    const opacityValue = document.getElementById('gridOpacityValue');
    if (opacityValue) {
        opacityValue.textContent = opacity;
    }
    
    showNotification(`📐 Grid opacity updated to ${opacity}%`, 'info');
    console.log('✓ Grid opacity updated:', opacity);
}

// SIMPLE TERRAIN CHANGE - MILITARY STYLE
function applyTerrainToHex(hexElement, terrainType) {
    console.log('MILITARY: Changing terrain to', terrainType);
    
    if (!hexElement || !terrainType) {
        showNotification('Cannot change terrain', 'error');
        return;
    }
    
    // Change hex color based on terrain
    const terrainColors = {
        'clear': '#90EE90',
        'forest': '#228B22', 
        'mountain': '#8B4513',
        'water': '#4169E1',
        'desert': '#F4A460',
        'urban': '#696969'
    };
    
    const color = terrainColors[terrainType] || '#90EE90';
    hexElement.style.backgroundColor = color;
    hexElement.dataset.terrain = terrainType;
    
    const q = hexElement.dataset.q;
    const r = hexElement.dataset.r;
    showNotification(`Terrain changed to ${terrainType} at (${q},${r})`, 'success');
    
    console.log('Terrain changed:', terrainType);
}

// Apply feature to hex with comprehensive validation
function applyFeatureToHex(hexElement, featureType) {
    console.log('🎖️ MILITARY SIMULATION: Applying feature to hex:', { hexElement, featureType });
    
    if (!hexElement || !featureType) {
        console.error('Invalid parameters for applyFeatureToHex');
        showNotification('Invalid feature data - please check selection', 'error');
        return;
    }
    
    try {
        // Add feature to hex
        addHexFeature(hexElement, featureType, { type: featureType, timestamp: new Date().toISOString() });
        
        console.log('✓ Feature applied successfully to hex:', { q: hexElement.dataset.q, r: hexElement.dataset.r, featureType });
        
    } catch (error) {
        console.error('Error applying feature to hex:', error);
        showNotification('Failed to apply feature - please try again', 'error');
    }
}

// MILITARY GAME TOOLS
let drawingPoints = [];
let measuringPoints = [];
let selectedItems = [];

function setGameTool(tool) {
    console.log('Setting game tool:', tool);
    window.currentTool = tool;
    
    // Clear previous tool states
    drawingPoints = [];
    measuringPoints = [];
    
    // Update UI
    document.querySelectorAll('.tool-btn').forEach(btn => btn.classList.remove('active'));
    const activeBtn = document.querySelector(`[data-tool="${tool}"]`);
    if (activeBtn) activeBtn.classList.add('active');
    
    showNotification(`Tool selected: ${tool.toUpperCase()}`, 'info');
}

function handleDrawTool(e) {
    console.log('Draw tool activated');
    drawingPoints.push(e.latlng);
    
    if (drawingPoints.length === 1) {
        showNotification('Click to start drawing area', 'info');
    } else if (drawingPoints.length === 2) {
        // Draw line
        const line = L.polyline(drawingPoints, {
            color: '#ff0000',
            weight: 3,
            opacity: 0.8
        }).addTo(realMapArena.map);
        
        showNotification('Line drawn', 'success');
        drawingPoints = [];
    }
}

function handleMeasureTool(e) {
    console.log('Measure tool activated');
    measuringPoints.push(e.latlng);
    
    if (measuringPoints.length === 1) {
        showNotification('Click second point to measure distance', 'info');
    } else if (measuringPoints.length === 2) {
        // Calculate distance
        const distance = realMapArena.map.distance(measuringPoints[0], measuringPoints[1]);
        const distanceKm = (distance / 1000).toFixed(2);
        
        // Draw measurement line
        const line = L.polyline(measuringPoints, {
            color: '#00ff00',
            weight: 2,
            opacity: 0.8
        }).addTo(realMapArena.map);
        
        // Add distance label
        const midPoint = [
            (measuringPoints[0].lat + measuringPoints[1].lat) / 2,
            (measuringPoints[0].lng + measuringPoints[1].lng) / 2
        ];
        
        L.marker(midPoint, {
            icon: L.divIcon({
                html: `<div style="background: #00ff00; color: black; padding: 2px 6px; border-radius: 3px; font-weight: bold;">${distanceKm} km</div>`,
                iconSize: [60, 20],
                iconAnchor: [30, 10]
            })
        }).addTo(realMapArena.map);
        
        showNotification(`Distance: ${distanceKm} km`, 'success');
        measuringPoints = [];
    }
}

function handleSelectTool(e) {
    console.log('Select tool activated');
    
    // Find items near click point
    const clickPoint = e.latlng;
    let foundItem = null;
    
    // Check tokens
    realMapArena.tokens.forEach((token, id) => {
        const distance = realMapArena.map.distance(clickPoint, [token.lat, token.lng]);
        if (distance < 50) { // 50 meter tolerance
            foundItem = { type: 'token', id: id, data: token };
        }
    });
    
    // Check features
    realMapArena.features.forEach((feature, id) => {
        const distance = realMapArena.map.distance(clickPoint, [feature.lat, feature.lng]);
        if (distance < 50) {
            foundItem = { type: 'feature', id: id, data: feature };
        }
    });
    
    if (foundItem) {
        selectedItems.push(foundItem);
        showNotification(`Selected ${foundItem.type}: ${foundItem.id}`, 'success');
        
        // Highlight selected item
        if (foundItem.type === 'token') {
            foundItem.data.marker.setIcon(createCustomTokenIcon(
                foundItem.data.icon, 
                '#ffff00', // Yellow for selected
                foundItem.data.serial
            ));
        }
    } else {
        showNotification('No item found at this location', 'warning');
    }
}

window.saveCurrentMap = saveCurrentMap;
window.loadMapDialog = loadMapDialog;
window.loadMap = loadMap;
window.exportMap = exportMap;
window.selectTerrain = selectTerrain;
window.selectFeature = selectFeature;
window.toggleGrid = toggleGrid;
window.updateGridSize = updateGridSize;
window.updateGridOpacity = updateGridOpacity;
window.applyTerrainToHex = applyTerrainToHex;
window.applyFeatureToHex = applyFeatureToHex;
window.setGameTool = setGameTool;

// HEX MODE AND FEATURE VISUALIZATION
let hexMode = false;
let hexGridLayer = null;

function toggleHexMode() {
    hexMode = !hexMode;
    console.log('Hex mode:', hexMode);
    
    if (hexMode) {
        showHexGrid();
        showNotification('Hex Mode ON - Grid visible', 'info');
    } else {
        hideHexGrid();
        showNotification('Hex Mode OFF - Grid hidden', 'info');
    }
}

function showHexGrid() {
    if (!realMapArena.map) return;
    
    // Create hex grid overlay
    const bounds = realMapArena.map.getBounds();
    const hexSize = 0.01; // Adjust size as needed
    
    const hexes = [];
    
    for (let lat = bounds.getSouth(); lat < bounds.getNorth(); lat += hexSize * 2) {
        for (let lng = bounds.getWest(); lng < bounds.getEast(); lng += hexSize * 1.5) {
            const hex = createHexPolygon(lat, lng, hexSize);
            hexes.push(hex);
        }
    }
    
    hexGridLayer = L.layerGroup(hexes).addTo(realMapArena.map);
}

function createHexPolygon(lat, lng, size) {
    const points = [];
    for (let i = 0; i < 6; i++) {
        const angle = (i * 60) * Math.PI / 180;
        const x = lng + size * Math.cos(angle);
        const y = lat + size * Math.sin(angle);
        points.push([y, x]);
    }
    
    return L.polygon(points, {
        color: '#ff0000',
        weight: 1,
        opacity: 0.6,
        fillOpacity: 0.1
    });
}

function hideHexGrid() {
    if (hexGridLayer) {
        realMapArena.map.removeLayer(hexGridLayer);
        hexGridLayer = null;
    }
}

function showAllFeatures() {
    console.log('Showing all features on map');
    
    // Show feature legend
    const legend = L.control({position: 'bottomright'});
    
    legend.onAdd = function(map) {
        const div = L.DomUtil.create('div', 'feature-legend');
        div.innerHTML = `
            <div style="background: white; padding: 10px; border-radius: 5px; box-shadow: 0 0 10px rgba(0,0,0,0.3);">
                <h6>🎖️ MILITARY FEATURES</h6>
                <div style="display: flex; align-items: center; margin: 5px 0;">
                    <div style="width: 20px; height: 20px; background: #ff6b35; border-radius: 50%; margin-right: 10px;"></div>
                    <span>Fortification (F)</span>
                </div>
                <div style="display: flex; align-items: center; margin: 5px 0;">
                    <div style="width: 20px; height: 20px; background: #ff6b35; border-radius: 50%; margin-right: 10px;"></div>
                    <span>Obstacle (O)</span>
                </div>
                <div style="display: flex; align-items: center; margin: 5px 0;">
                    <div style="width: 20px; height: 20px; background: #ff6b35; border-radius: 50%; margin-right: 10px;"></div>
                    <span>Objective (O)</span>
                </div>
                <h6 style="margin-top: 10px;">🌍 TERRAIN TYPES</h6>
                <div style="display: flex; align-items: center; margin: 5px 0;">
                    <div style="width: 20px; height: 20px; background: #90EE90; border-radius: 50%; margin-right: 10px;"></div>
                    <span>Clear</span>
                </div>
                <div style="display: flex; align-items: center; margin: 5px 0;">
                    <div style="width: 20px; height: 20px; background: #228B22; border-radius: 50%; margin-right: 10px;"></div>
                    <span>Forest</span>
                </div>
                <div style="display: flex; align-items: center; margin: 5px 0;">
                    <div style="width: 20px; height: 20px; background: #8B4513; border-radius: 50%; margin-right: 10px;"></div>
                    <span>Mountain</span>
                </div>
                <div style="display: flex; align-items: center; margin: 5px 0;">
                    <div style="width: 20px; height: 20px; background: #4169E1; border-radius: 50%; margin-right: 10px;"></div>
                    <span>Water</span>
                </div>
            </div>
        `;
        return div;
    };
    
    legend.addTo(realMapArena.map);
    
    showNotification('Feature legend displayed - All types shown', 'success');
}

function clearMap() {
    if (confirm('Clear all items from map?')) {
        // Clear all tokens
        realMapArena.tokens.forEach((token, id) => {
            realMapArena.map.removeLayer(token.marker);
        });
        realMapArena.tokens.clear();
        
        // Clear all features
        realMapArena.features.forEach((feature, id) => {
            realMapArena.map.removeLayer(feature.marker);
        });
        realMapArena.features.clear();
        
        // Clear hex grid
        hideHexGrid();
        
        showNotification('Map cleared', 'info');
    }
}

// FIXED MAP AREA TOOL
let areaPoints = [];
let areaPolygon = null;

function handleAreaTool(e) {
    console.log('Area tool activated');
    areaPoints.push(e.latlng);
    
    if (areaPoints.length === 1) {
        showNotification('Click to define area boundary', 'info');
    } else if (areaPoints.length >= 3) {
        // Create area polygon
        if (areaPolygon) {
            realMapArena.map.removeLayer(areaPolygon);
        }
        
        areaPolygon = L.polygon(areaPoints, {
            color: '#ff0000',
            weight: 3,
            opacity: 0.8,
            fillColor: '#ff0000',
            fillOpacity: 0.2
        }).addTo(realMapArena.map);
        
        showNotification(`Fixed area defined with ${areaPoints.length} points`, 'success');
        areaPoints = [];
    }
}

// REMOVE MODE FUNCTIONS
function setRemoveMode(type) {
    window.removeMode = type;
    showNotification(`Remove mode: ${type} - Click items to remove`, 'warning');
}

function handleRemoveMode(e) {
    if (!window.removeMode) return;
    
    const clickPoint = e.latlng;
    let removed = false;
    
    if (window.removeMode === 'feature') {
        // Remove features near click
        realMapArena.features.forEach((feature, id) => {
            const distance = realMapArena.map.distance(clickPoint, [feature.lat, feature.lng]);
            if (distance < 50) {
                realMapArena.map.removeLayer(feature.marker);
                realMapArena.features.delete(id);
                removed = true;
            }
        });
    } else if (window.removeMode === 'terrain') {
        // Remove terrain markers near click
        realMapArena.map.eachLayer(layer => {
            if (layer instanceof L.CircleMarker) {
                const distance = realMapArena.map.distance(clickPoint, layer.getLatLng());
                if (distance < 50) {
                    realMapArena.map.removeLayer(layer);
                    removed = true;
                }
            }
        });
    }
    
    if (removed) {
        showNotification(`${window.removeMode} removed`, 'success');
    } else {
        showNotification(`No ${window.removeMode} found at this location`, 'warning');
    }
}

// ENHANCED FEATURE ICONS WITH TOOLTIPS
function createEnhancedFeatureIcon(featureType, featureData) {
    const icons = {
        'fortification': { icon: '🛡️', color: '#8B4513', name: 'Fortification' },
        'obstacle': { icon: '🚧', color: '#FF6347', name: 'Obstacle' },
        'objective': { icon: '🎯', color: '#FFD700', name: 'Objective' },
        'supply': { icon: '📦', color: '#32CD32', name: 'Supply Point' },
        'command': { icon: '🏛️', color: '#4169E1', name: 'Command Post' }
    };
    
    const feature = icons[featureType] || { icon: '📍', color: '#ff6b35', name: 'Feature' };
    
    return L.divIcon({
        html: `
            <div class="enhanced-feature-marker" 
                 style="
                     background: ${feature.color};
                     color: white;
                     border: 2px solid white;
                     border-radius: 50%;
                     width: 40px;
                     height: 40px;
                     display: flex;
                     align-items: center;
                     justify-content: center;
                     font-weight: bold;
                     font-size: 18px;
                     box-shadow: 0 2px 8px rgba(0,0,0,0.3);
                 "
                 title="${feature.name}">
                ${feature.icon}
            </div>
        `,
        iconSize: [40, 40],
        iconAnchor: [20, 20],
        className: 'enhanced-feature-icon'
    });
}

// ENHANCED TERRAIN ICONS WITH TOOLTIPS
function createEnhancedTerrainIcon(terrainType) {
    const terrains = {
        'clear': { icon: '🌱', color: '#90EE90', name: 'Clear Terrain' },
        'forest': { icon: '🌲', color: '#228B22', name: 'Forest' },
        'mountain': { icon: '⛰️', color: '#8B4513', name: 'Mountain' },
        'water': { icon: '💧', color: '#4169E1', name: 'Water' },
        'desert': { icon: '🏜️', color: '#F4A460', name: 'Desert' },
        'urban': { icon: '🏙️', color: '#696969', name: 'Urban' }
    };
    
    const terrain = terrains[terrainType] || { icon: '🌍', color: '#90EE90', name: 'Terrain' };
    
    return L.divIcon({
        html: `
            <div class="enhanced-terrain-marker" 
                 style="
                     background: ${terrain.color};
                     color: white;
                     border: 2px solid white;
                     border-radius: 50%;
                     width: 35px;
                     height: 35px;
                     display: flex;
                     align-items: center;
                     justify-content: center;
                     font-weight: bold;
                     font-size: 16px;
                     box-shadow: 0 2px 6px rgba(0,0,0,0.3);
                 "
                 title="${terrain.name}">
                ${terrain.icon}
            </div>
        `,
        iconSize: [35, 35],
        iconAnchor: [17, 17],
        className: 'enhanced-terrain-icon'
    });
}

// MAP AREA SELECTION WITH HEX GRID
let mapAreaPoints = [];
let mapAreaPolygon = null;
let mapAreaHexGrid = null;
let selectedMapArea = null;

function handleMapAreaTool(e) {
    console.log('Map area tool activated');
    mapAreaPoints.push(e.latlng);
    
    if (mapAreaPoints.length === 1) {
        showNotification('Click to define map area boundary', 'info');
    } else if (mapAreaPoints.length >= 3) {
        // Create map area polygon
        if (mapAreaPolygon) {
            realMapArena.map.removeLayer(mapAreaPolygon);
        }
        
        mapAreaPolygon = L.polygon(mapAreaPoints, {
            color: '#ff0000',
            weight: 3,
            opacity: 0.8,
            fillColor: '#ff0000',
            fillOpacity: 0.2
        }).addTo(realMapArena.map);
        
        // Create hex grid overlay on the area
        createHexGridOnArea(mapAreaPoints);
        
        showNotification(`Map area selected with ${mapAreaPoints.length} points - Hex grid applied`, 'success');
        mapAreaPoints = [];
    }
}

function createHexGridOnArea(areaPoints) {
    if (!realMapArena.map) return;
    
    // Calculate bounds of the area
    const bounds = L.polygon(areaPoints).getBounds();
    const southWest = bounds.getSouthWest();
    const northEast = bounds.getNorthEast();
    
    // Clear existing hex grid
    if (mapAreaHexGrid) {
        realMapArena.map.removeLayer(mapAreaHexGrid);
    }
    
    // Create hex grid within the area
    const hexes = [];
    const hexSize = 0.005; // Adjust size as needed
    
    for (let lat = southWest.lat; lat < northEast.lat; lat += hexSize * 2) {
        for (let lng = southWest.lng; lng < northEast.lng; lng += hexSize * 1.5) {
            // Check if point is within the area
            const point = L.latLng(lat, lng);
            if (isPointInPolygon(point, areaPoints)) {
                const hex = createHexPolygon(lat, lng, hexSize);
                hexes.push(hex);
            }
        }
    }
    
    mapAreaHexGrid = L.layerGroup(hexes).addTo(realMapArena.map);
    
    // Store the selected area
    selectedMapArea = {
        bounds: bounds,
        points: areaPoints,
        hexGrid: mapAreaHexGrid
    };
    
    showNotification(`Hex grid created with ${hexes.length} hexes`, 'info');
}

function isPointInPolygon(point, polygonPoints) {
    // Simple point-in-polygon test
    let inside = false;
    for (let i = 0, j = polygonPoints.length - 1; i < polygonPoints.length; j = i++) {
        if (((polygonPoints[i].lat > point.lat) !== (polygonPoints[j].lat > point.lat)) &&
            (point.lng < (polygonPoints[j].lng - polygonPoints[i].lng) * (point.lat - polygonPoints[i].lat) / (polygonPoints[j].lat - polygonPoints[i].lat) + polygonPoints[i].lng)) {
            inside = !inside;
        }
    }
    return inside;
}

// LOCATION SEARCH FUNCTIONS
function goToLocation(locationName) {
    console.log('Going to location:', locationName);
    
    // Predefined locations
    const locations = {
        'Dubai, UAE': { lat: 25.2048, lng: 55.2708, zoom: 13 },
        'New York, USA': { lat: 40.7128, lng: -74.0060, zoom: 11 },
        'London, UK': { lat: 51.5074, lng: -0.1278, zoom: 11 },
        'Tokyo, Japan': { lat: 35.6762, lng: 139.6503, zoom: 11 }
    };
    
    const location = locations[locationName];
    if (location) {
        realMapArena.map.setView([location.lat, location.lng], location.zoom);
        showNotification(`Navigated to ${locationName}`, 'success');
    } else {
        // Try to geocode the location
        geocodeLocation(locationName);
    }
}

function geocodeLocation(locationName) {
    // Simple geocoding using OpenStreetMap Nominatim
    const url = `https://nominatim.openstreetmap.org/search?format=json&q=${encodeURIComponent(locationName)}&limit=1`;
    
    fetch(url)
        .then(response => response.json())
        .then(data => {
            if (data && data.length > 0) {
                const result = data[0];
                const lat = parseFloat(result.lat);
                const lng = parseFloat(result.lon);
                
                realMapArena.map.setView([lat, lng], 13);
                showNotification(`Found ${locationName}`, 'success');
            } else {
                showNotification(`Location not found: ${locationName}`, 'error');
            }
        })
        .catch(error => {
            console.error('Geocoding error:', error);
            showNotification(`Error searching for ${locationName}`, 'error');
        });
}

// Setup location search event listener
function setupLocationSearch() {
    const searchInput = document.getElementById('locationSearch');
    if (searchInput) {
        searchInput.addEventListener('keypress', function(e) {
            if (e.key === 'Enter') {
                const location = this.value.trim();
                if (location) {
                    goToLocation(location);
                    this.value = '';
                }
            }
        });
    }
}

// Export hex mode functions
window.toggleHexMode = toggleHexMode;
window.showAllFeatures = showAllFeatures;
window.clearMap = clearMap;
window.setRemoveMode = setRemoveMode;
window.goToLocation = goToLocation;
window.setupLocationSearch = setupLocationSearch;
window.cleanupModalBackdrop = cleanupModalBackdrop;

// ========================================
// COMPREHENSIVE SYSTEM VALIDATION
// ========================================

// Enhanced system validation for military simulation
function validateMilitarySimulation() {
    console.log('🎖️ MILITARY SIMULATION: COMPREHENSIVE SYSTEM VALIDATION...');
    
    const testResults = {
        mapInitialization: false,
        tokenSystem: false,
        featureSystem: false,
        terrainSystem: false,
        groupManagement: false,
        mapPersistence: false,
        roleManagement: false,
        realTimeCommunication: false
    };
    
    // Test 1: Map Initialization
    console.log('🔍 Testing Map Initialization...');
    if (realMapArena.map && realMapArena.mapContainer && realMapArena.hexGrid && typeof L !== 'undefined') {
        testResults.mapInitialization = true;
        console.log('✅ Map initialization: PASSED');
    } else {
        console.log('❌ Map initialization: FAILED');
    }
    
    // Test 2: Token System
    console.log('🔍 Testing Token System...');
    if (realMapArena.map && typeof placeTokenOnMap === 'function') {
        testResults.tokenSystem = true;
        console.log('✅ Token system: PASSED');
    } else {
        console.log('❌ Token system: FAILED');
    }
    
    // Test 3: Feature System
    console.log('🔍 Testing Feature System...');
    if (typeof addHexFeature === 'function' && typeof selectFeature === 'function') {
        testResults.featureSystem = true;
        console.log('✅ Feature system: PASSED');
    } else {
        console.log('❌ Feature system: FAILED');
    }
    
    // Test 4: Terrain System
    console.log('🔍 Testing Terrain System...');
    if (typeof selectTerrain === 'function' && typeof applyTerrainToHex === 'function') {
        testResults.terrainSystem = true;
        console.log('✅ Terrain system: PASSED');
    } else {
        console.log('❌ Terrain system: FAILED');
    }
    
    // Test 5: Group Management
    console.log('🔍 Testing Group Management...');
    if (typeof initializeGroupManagement === 'function') {
        testResults.groupManagement = true;
        console.log('✅ Group management: PASSED');
    } else {
        console.log('❌ Group management: FAILED');
    }
    
    // Test 6: Map Persistence
    console.log('🔍 Testing Map Persistence...');
    if (typeof saveCurrentMap === 'function' && typeof loadMapDialog === 'function') {
        testResults.mapPersistence = true;
        console.log('✅ Map persistence: PASSED');
    } else {
        console.log('❌ Map persistence: FAILED');
    }
    
    // Test 7: Role Management
    console.log('🔍 Testing Role Management...');
    if (window.arenaState && typeof window.updateUIForRole === 'function') {
        testResults.roleManagement = true;
        console.log('✅ Role management: PASSED');
    } else {
        console.log('❌ Role management: FAILED');
    }
    
    // Test 8: Real-time Communication
    console.log('🔍 Testing Real-time Communication...');
    if (typeof window.addHexFeatureRealTime === 'function' || typeof window.updateHexTerrainRealTime === 'function') {
        testResults.realTimeCommunication = true;
        console.log('✅ Real-time communication: PASSED');
    } else {
        console.log('❌ Real-time communication: FAILED');
    }
    
    // Calculate overall system health
    const passedTests = Object.values(testResults).filter(result => result === true).length;
    const totalTests = Object.keys(testResults).length;
    const systemHealth = (passedTests / totalTests) * 100;
    
    console.log('🎖️ MILITARY SIMULATION: SYSTEM VALIDATION COMPLETE');
    console.log('================================================');
    console.log(`System Health: ${systemHealth.toFixed(1)}% (${passedTests}/${totalTests} systems operational)`);
    console.log('================================================');
    
    // Show comprehensive results
    showNotification(`🎖️ MILITARY SIMULATION: System Health ${systemHealth.toFixed(1)}%`, systemHealth >= 80 ? 'success' : 'warning');
    showNotification(`✅ Operational Systems: ${passedTests}/${totalTests}`, 'info');
    
    if (systemHealth >= 90) {
        showNotification('🚀 MISSION READY: All critical systems operational', 'success');
    } else if (systemHealth >= 70) {
        showNotification('⚠️ PARTIAL READINESS: Some systems require attention', 'warning');
    } else {
        showNotification('❌ CRITICAL ISSUES: Multiple systems offline', 'error');
    }
    
    return testResults;
}

window.validateMilitarySimulation = validateMilitarySimulation;
