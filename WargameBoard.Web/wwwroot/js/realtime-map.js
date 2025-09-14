// Real-time Map JavaScript - Enhanced for Modern Arena
let connection = null;
let currentSessionId = null;
let selectedHexId = null;
let selectedTokenId = null;
let arenaIntegration = null;

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', function() {
    initializeSignalR();
    
    // Wait a bit for all elements to be available
    setTimeout(() => {
        setupEventListeners();
        updateUI();
        initializeArenaIntegration();
    }, 100);
});

// Initialize integration with modern arena
function initializeArenaIntegration() {
    // Wait for arena to be initialized
    const checkArena = setInterval(() => {
        if (window.arenaState && window.arenaState.isInitialized) {
            arenaIntegration = window.arenaState;
            clearInterval(checkArena);
            setupArenaIntegration();
        }
    }, 100);
}

// Setup integration between real-time and arena systems
function setupArenaIntegration() {
    if (!arenaIntegration) return;
    
    // Override arena's hex click handler to include real-time functionality
    const originalHandleHexClick = window.handleHexClick;
    if (originalHandleHexClick) {
        window.handleHexClick = function(hexElement) {
            // Call original arena handler
            originalHandleHexClick(hexElement);
            
            // Add real-time functionality
            if (arenaIntegration.currentRole === 'tokens' && arenaIntegration.selectedToken) {
                // Real-time token placement
                placeTokenRealTime(hexElement);
            }
        };
    }
    
    // Override arena's token placement to include real-time sync
    const originalPlaceTokenOnHex = window.placeTokenOnHex;
    if (originalPlaceTokenOnHex) {
        window.placeTokenOnHex = function(hexElement) {
            // Call original arena handler
            originalPlaceTokenOnHex(hexElement);
            
            // Sync with real-time system
            syncTokenPlacement(hexElement);
        };
    }
}

// Initialize SignalR connection
function initializeSignalR() {
    connection = new signalR.HubConnectionBuilder()
        .withUrl("/hubs/realtime")
        .withAutomaticReconnect()
        .build();

    // Connection event handlers
    connection.onclose(function (error) {
        updateConnectionStatus(false);
        console.log("SignalR connection closed:", error);
    });

    connection.onreconnecting(function (error) {
        updateConnectionStatus(false);
        console.log("SignalR reconnecting:", error);
    });

    connection.onreconnected(function (connectionId) {
        updateConnectionStatus(true);
        console.log("SignalR reconnected:", connectionId);
        if (currentSessionId) {
            connection.invoke("JoinSession", currentSessionId);
        }
    });

    // Game event handlers
    connection.on("SessionState", function (sessionData) {
        updateSessionStatus(sessionData);
    });

    connection.on("TokenPlaced", function (placementData) {
        addTokenToHex(placementData);
        showNotification("Token placed successfully", "success");
    });

    connection.on("TokenMoved", function (moveData) {
        moveTokenOnMap(moveData);
        showNotification("Token moved successfully", "success");
    });

    connection.on("TokenRemoved", function (placementId) {
        removeTokenFromMap(placementId);
        showNotification("Token removed successfully", "success");
    });

    connection.on("HexUpdated", function (hexData) {
        updateHexTerrain(hexData);
        showNotification("Hex terrain updated", "info");
    });

    connection.on("HexFeatureAdded", function (featureData) {
        addFeatureToHex(featureData);
        showNotification("Feature added to hex", "success");
    });

    connection.on("HexFeatureRemoved", function (featureId) {
        removeFeatureFromMap(featureId);
        showNotification("Feature removed from hex", "success");
    });

    connection.on("TurnAdvanced", function (turnData) {
        updateCurrentTurn(turnData);
        showNotification(`Turn advanced to ${turnData.Number}`, "info");
    });

    connection.on("ObjectiveControlUpdated", function (objectiveData) {
        updateObjectiveControl(objectiveData);
        showNotification("Objective control updated", "info");
    });

    // Error handlers
    connection.on("PlacementError", function (message) {
        showNotification(message, "error");
    });

    connection.on("MoveError", function (message) {
        showNotification(message, "error");
    });

    connection.on("RemoveError", function (message) {
        showNotification(message, "error");
    });

    connection.on("UpdateError", function (message) {
        showNotification(message, "error");
    });

    connection.on("TurnError", function (message) {
        showNotification(message, "error");
    });

    // Start connection
    connection.start().then(function () {
        updateConnectionStatus(true);
        console.log("SignalR connection started");
    }).catch(function (err) {
        updateConnectionStatus(false);
        console.error("SignalR connection error:", err);
    });
}

// Setup event listeners
function setupEventListeners() {
    // Session selection
    document.getElementById('sessionSelect').addEventListener('change', function() {
        const sessionId = this.value;
        if (sessionId) {
            currentSessionId = sessionId;
            document.getElementById('joinSessionBtn').disabled = false;
        } else {
            currentSessionId = null;
            document.getElementById('joinSessionBtn').disabled = true;
        }
    });

    // Join session
    document.getElementById('joinSessionBtn').addEventListener('click', function() {
        if (currentSessionId) {
            joinSession(currentSessionId);
        }
    });

    // Leave session
    document.getElementById('leaveSessionBtn').addEventListener('click', function() {
        leaveSession();
    });

    // Advance turn
    document.getElementById('advanceTurnBtn').addEventListener('click', function() {
        if (currentSessionId) {
            advanceTurn(currentSessionId);
        }
    });

    // Token selection - check if element exists
    const tokenSelect = safeGetElement('tokenSelect');
    if (tokenSelect) {
        tokenSelect.addEventListener('change', function() {
            selectedTokenId = this.value;
        });
    }

    // Terrain selection - check if element exists
    const terrainSelect = safeGetElement('terrainSelect');
    if (terrainSelect) {
        terrainSelect.addEventListener('change', function() {
            if (selectedHexId && currentSessionId) {
                updateHexTerrainRealTime(selectedHexId, this.value);
            }
        });
    }

    // Feature type selection - check if element exists
    const featureTypeSelect = safeGetElement('featureTypeSelect');
    if (featureTypeSelect) {
        featureTypeSelect.addEventListener('change', function() {
            if (selectedHexId && currentSessionId) {
                const featureTypeId = this.value;
                const featureKind = this.options[this.selectedIndex].text;
                const sideSelect = safeGetElement('sideSelect');
                const sideId = sideSelect ? sideSelect.value : null;
                
                if (featureTypeId) {
                    addHexFeatureRealTime(selectedHexId, featureTypeId, featureKind, sideId);
                }
            }
        });
    }
}

// Join session
function joinSession(sessionId) {
    if (connection && connection.state === signalR.HubConnectionState.Connected) {
        connection.invoke("JoinSession", sessionId).then(function() {
            updateSessionStatus({ Id: sessionId, Scenario: "Loading...", CurrentSide: "Loading..." });
            
            // Safely update UI elements
            const statusPanel = safeGetElement('statusPanel');
            const gameControls = safeGetElement('gameControls');
            const joinSessionBtn = safeGetElement('joinSessionBtn');
            const leaveSessionBtn = safeGetElement('leaveSessionBtn');
            const advanceTurnBtn = safeGetElement('advanceTurnBtn');
            
            if (statusPanel) statusPanel.classList.remove('d-none');
            if (gameControls) gameControls.classList.remove('d-none');
            if (joinSessionBtn) joinSessionBtn.disabled = true;
            if (leaveSessionBtn) leaveSessionBtn.disabled = false;
            if (advanceTurnBtn) advanceTurnBtn.disabled = false;
            
            showNotification("Joined session successfully", "success");
        }).catch(function(err) {
            console.error("Error joining session:", err);
            showNotification("Failed to join session", "error");
        });
    }
}

// Leave session
function leaveSession() {
    if (connection && connection.state === signalR.HubConnectionState.Connected && currentSessionId) {
        connection.invoke("LeaveSession", currentSessionId).then(function() {
            updateSessionStatus(null);
            
            // Safely update UI elements
            const statusPanel = safeGetElement('statusPanel');
            const gameControls = safeGetElement('gameControls');
            const joinSessionBtn = safeGetElement('joinSessionBtn');
            const leaveSessionBtn = safeGetElement('leaveSessionBtn');
            const advanceTurnBtn = safeGetElement('advanceTurnBtn');
            
            if (statusPanel) statusPanel.classList.add('d-none');
            if (gameControls) gameControls.classList.add('d-none');
            if (joinSessionBtn) joinSessionBtn.disabled = false;
            if (leaveSessionBtn) leaveSessionBtn.disabled = true;
            if (advanceTurnBtn) advanceTurnBtn.disabled = true;
            
            currentSessionId = null;
            showNotification("Left session", "info");
        }).catch(function(err) {
            console.error("Error leaving session:", err);
        });
    }
}

// Advance turn
function advanceTurn(sessionId) {
    if (connection && connection.state === signalR.HubConnectionState.Connected) {
        connection.invoke("AdvanceTurn", sessionId).catch(function(err) {
            console.error("Error advancing turn:", err);
            showNotification("Failed to advance turn", "error");
        });
    }
}

// Place token
function placeToken(hexId) {
    if (selectedTokenId && currentSessionId) {
        if (connection && connection.state === signalR.HubConnectionState.Connected) {
            connection.invoke("PlaceToken", currentSessionId, selectedTokenId, hexId).catch(function(err) {
                console.error("Error placing token:", err);
            });
        }
    } else {
        showNotification("Please select a token first", "warning");
    }
}

// Move token
function moveToken(placementId, newHexId) {
    if (currentSessionId) {
        if (connection && connection.state === signalR.HubConnectionState.Connected) {
            connection.invoke("MoveToken", currentSessionId, placementId, newHexId).catch(function(err) {
                console.error("Error moving token:", err);
            });
        }
    }
}

// Remove token
function removeToken(placementId) {
    if (currentSessionId) {
        if (connection && connection.state === signalR.HubConnectionState.Connected) {
            connection.invoke("RemoveToken", currentSessionId, placementId).catch(function(err) {
                console.error("Error removing token:", err);
            });
        }
    }
}

// Update hex terrain via API
function updateHexTerrainRealTime(hexId, terrainTypeId) {
    if (currentSessionId) {
        fetch('/Map/Map/UpdateHexTerrain', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
            },
            body: `sessionId=${currentSessionId}&hexId=${hexId}&terrainTypeId=${terrainTypeId}`
        }).then(response => response.json())
        .then(data => {
            if (data.success) {
                // The SignalR hub will handle the real-time update
            } else {
                showNotification("Failed to update hex terrain", "error");
            }
        }).catch(err => {
            console.error("Error updating hex terrain:", err);
            showNotification("Failed to update hex terrain", "error");
        });
    }
}

// Add hex feature via API
function addHexFeatureRealTime(hexId, featureTypeId, featureKind, sideId) {
    if (currentSessionId) {
        fetch('/Map/Map/AddHexFeature', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
            },
            body: `sessionId=${currentSessionId}&hexId=${hexId}&featureTypeId=${featureTypeId}&featureKind=${featureKind}&sideId=${sideId}`
        }).then(response => response.json())
        .then(data => {
            if (data.success) {
                // The SignalR hub will handle the real-time update
            } else {
                showNotification("Failed to add hex feature", "error");
            }
        }).catch(err => {
            console.error("Error adding hex feature:", err);
            showNotification("Failed to add hex feature", "error");
        });
    }
}

// UI Update functions
function updateConnectionStatus(connected) {
    const statusElement = document.getElementById('connectionStatus');
    if (connected) {
        statusElement.textContent = 'Connected';
        statusElement.className = 'badge bg-success';
    } else {
        statusElement.textContent = 'Disconnected';
        statusElement.className = 'badge bg-danger';
    }
}

function updateSessionStatus(sessionData) {
    const statusElement = document.getElementById('sessionStatus');
    const turnElement = document.getElementById('currentTurn');
    
    if (sessionData) {
        statusElement.textContent = `Connected to ${sessionData.Scenario || 'Unknown Session'}`;
        if (sessionData.Turns && sessionData.Turns.length > 0) {
            const currentTurn = sessionData.Turns[sessionData.Turns.length - 1];
            turnElement.textContent = currentTurn.Number;
        }
    } else {
        statusElement.textContent = 'Not Connected';
        turnElement.textContent = '-';
    }
}

function updateCurrentTurn(turnData) {
    document.getElementById('currentTurn').textContent = turnData.Number;
}

function addTokenToHex(placementData) {
    const hexElement = document.querySelector(`[data-hexid="${placementData.HexId}"]`);
    if (hexElement) {
        const overlay = hexElement.querySelector('.hex-overlay');
        const tokenBadge = document.createElement('span');
        tokenBadge.className = 'badge bg-primary token-badge';
        tokenBadge.setAttribute('data-placement-id', placementData.Id);
        tokenBadge.setAttribute('data-token-id', placementData.TokenPieceId);
        tokenBadge.textContent = `Token ${placementData.TokenPieceId}`;
        overlay.appendChild(tokenBadge);
    }
}

function moveTokenOnMap(moveData) {
    const tokenBadge = document.querySelector(`[data-placement-id="${moveData.PlacementId}"]`);
    if (tokenBadge) {
        const newHexElement = document.querySelector(`[data-hexid="${moveData.ToHexId}"]`);
        if (newHexElement) {
            const overlay = newHexElement.querySelector('.hex-overlay');
            overlay.appendChild(tokenBadge);
        }
    }
}

function removeTokenFromMap(placementId) {
    const tokenBadge = document.querySelector(`[data-placement-id="${placementId}"]`);
    if (tokenBadge) {
        tokenBadge.remove();
    }
}

function updateHexTerrain(hexData) {
    const hexElement = document.querySelector(`[data-hexid="${hexData.Id}"]`);
    if (hexElement) {
        // Remove existing terrain classes
        hexElement.classList.remove('terrain-clear', 'terrain-forest', 'terrain-mountain', 
                                  'terrain-water', 'terrain-desert', 'terrain-urban', 'terrain-swamp');
        
        // Add new terrain class
        const terrainClasses = {
            1: 'terrain-clear',
            2: 'terrain-forest',
            3: 'terrain-mountain',
            4: 'terrain-water',
            5: 'terrain-desert',
            6: 'terrain-urban',
            7: 'terrain-swamp'
        };
        
        const terrainClass = terrainClasses[hexData.TerrainTypeId] || 'terrain-clear';
        hexElement.classList.add(terrainClass);
    }
}

function addFeatureToHex(featureData) {
    const hexElement = document.querySelector(`[data-hexid="${featureData.HexId}"]`);
    if (hexElement) {
        const overlay = hexElement.querySelector('.hex-overlay');
        const featureBadge = document.createElement('span');
        featureBadge.className = 'badge bg-secondary feature-badge';
        featureBadge.setAttribute('data-feature-id', featureData.Id);
        featureBadge.textContent = featureData.FeatureKind;
        overlay.appendChild(featureBadge);
    }
}

function removeFeatureFromMap(featureId) {
    const featureBadge = document.querySelector(`[data-feature-id="${featureId}"]`);
    if (featureBadge) {
        featureBadge.remove();
    }
}

function updateObjectiveControl(objectiveData) {
    // Implement objective control updates
    console.log("Objective control updated:", objectiveData);
}

// Enhanced hex selection with real-time operations
function selectHex(hexElement) {
    // Remove previous selection
    document.querySelectorAll('.hex').forEach(h => h.classList.remove('selected'));
    
    // Add selection to current hex
    hexElement.classList.add('selected');
    
    // Update selected hex ID
    selectedHexId = hexElement.dataset.hexid;
    
    // Show hex coordinates
    const q = hexElement.dataset.q;
    const r = hexElement.dataset.r;
    showNotification(`Selected hex (${q}, ${r})`, "info");
    
    // Load hex details if available
    if (document.getElementById('hexEditHost')) {
        load(`/Map/Map/EditHex/${selectedHexId}`, '#hexEditHost');
    }
    if (document.getElementById('hexFeaturesHost')) {
        load(`/Map/Map/ListHexFeatures?hexId=${selectedHexId}`, '#hexFeaturesHost');
    }
    
    // Update terrain select to match current hex
    const currentTerrain = hexElement.classList.contains('terrain-clear') ? '1' :
                          hexElement.classList.contains('terrain-forest') ? '2' :
                          hexElement.classList.contains('terrain-mountain') ? '3' :
                          hexElement.classList.contains('terrain-water') ? '4' :
                          hexElement.classList.contains('terrain-desert') ? '5' :
                          hexElement.classList.contains('terrain-urban') ? '6' :
                          hexElement.classList.contains('terrain-swamp') ? '7' : '1';
    
    const terrainSelect = document.getElementById('terrainSelect');
    if (terrainSelect) {
        terrainSelect.value = currentTerrain;
    }
}

// Token placement on hex click
function onHexClick(hexElement) {
    if (selectedTokenId && currentSessionId) {
        const hexId = hexElement.dataset.hexid;
        placeToken(hexId);
    } else {
        selectHex(hexElement);
        // Also show instructions if no token is selected
        if (!selectedTokenId) {
            showNotification("Select a token from the controls above to place it on the map", "warning");
        }
    }
}

// Token interaction
document.addEventListener('click', function(e) {
    if (e.target.classList.contains('token-badge')) {
        e.stopPropagation();
        const placementId = e.target.dataset.placementId;
        
        // Show token options (move, remove, etc.)
        showTokenOptions(placementId, e.target);
    }
});

function showTokenOptions(placementId, tokenElement) {
    // Create context menu or modal for token options
    const options = confirm("Remove this token?");
    if (options) {
        removeToken(placementId);
    }
}

// Notification system
function showNotification(message, type) {
    const alertClass = type === 'error' ? 'alert-danger' : 
                      type === 'warning' ? 'alert-warning' : 
                      type === 'success' ? 'alert-success' : 'alert-info';
    
    const notification = document.createElement('div');
    notification.className = `alert ${alertClass} alert-dismissible fade show position-fixed`;
    notification.style.cssText = 'top: 20px; right: 20px; z-index: 9999; min-width: 300px;';
    notification.innerHTML = `
        ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    `;
    
    document.body.appendChild(notification);
    
    // Auto-remove after 3 seconds
    setTimeout(() => {
        if (notification.parentNode) {
            notification.remove();
        }
    }, 3000);
}

// Update UI based on connection state
function updateUI() {
    const sessionId = document.getElementById('sessionSelect').value;
    if (sessionId) {
        currentSessionId = sessionId;
        document.getElementById('joinSessionBtn').disabled = false;
    }
}

// Helper function to load content via AJAX
function load(url, target) {
    fetch(url)
        .then(response => response.text())
        .then(html => {
            const element = document.querySelector(target);
            if (element) {
                element.innerHTML = html;
            }
        })
        .catch(error => {
            console.error('Error loading content:', error);
        });
}

// Helper function to refresh data
function refreshData(url, target) {
    load(url, target);
}

// Helper function to post form data
function postForm(form, target) {
    const formData = new FormData(form);
    fetch(form.action, {
        method: 'POST',
        body: formData
    }).then(response => response.text())
    .then(html => {
        const element = document.querySelector(target);
        if (element) {
            element.innerHTML = html;
        }
    }).catch(error => {
        console.error('Error posting form:', error);
    });
}

// Initialize map editor functionality
function initializeMapEditor() {
    // Add double-click handler for hex editing
    document.addEventListener('dblclick', function(e) {
        if (e.target.classList.contains('hex') || e.target.closest('.hex')) {
            const hexElement = e.target.classList.contains('hex') ? e.target : e.target.closest('.hex');
            const hexId = hexElement.dataset.hexid;
            
            if (hexId) {
                // Load hex editor modal
                load(`/Map/Map/EditHex/${hexId}`, '#hexEditHost');
                
                // Show modal
                const modal = new bootstrap.Modal(document.getElementById('editHexModal'));
                modal.show();
            }
        }
    });
    
    // Add right-click handler for context menu (future feature)
    document.addEventListener('contextmenu', function(e) {
        if (e.target.classList.contains('hex') || e.target.closest('.hex')) {
            e.preventDefault();
            const hexElement = e.target.classList.contains('hex') ? e.target : e.target.closest('.hex');
            selectHex(hexElement);
        }
    });
}

// Token selection functionality
function selectToken(tokenId) {
    selectedTokenId = tokenId;
    const tokenSelect = document.getElementById('tokenSelect');
    const selectedDisplay = document.getElementById('selectedTokenDisplay');
    
    if (tokenId) {
        const selectedOption = tokenSelect.querySelector(`option[value="${tokenId}"]`);
        const serial = selectedOption ? selectedOption.dataset.serial : 'Unknown';
        
        selectedDisplay.innerHTML = `
            <span class="badge bg-primary">
                <i class="bi bi-shapes"></i> ${serial}
            </span>
        `;
        
        showNotification(`Selected token: ${serial}. Click on a hex to place it.`, 'success');
    } else {
        selectedDisplay.innerHTML = '<span class="text-muted">No token selected</span>';
        showNotification('Token selection cleared', 'info');
    }
}

function clearTokenSelection() {
    document.getElementById('tokenSelect').value = '';
    selectToken('');
}

function showTokenInfo() {
    if (selectedTokenId) {
        showNotification('Token info functionality coming soon', 'info');
    } else {
        showNotification('Please select a token first', 'warning');
    }
}

// Safe element getter with error handling
function safeGetElement(id) {
    const element = document.getElementById(id);
    if (!element) {
        console.warn(`Element with id '${id}' not found`);
    }
    return element;
}

// Notification system
function showNotification(message, type = 'info') {
    // Remove existing notifications
    const existingNotifications = document.querySelectorAll('.notification-toast');
    existingNotifications.forEach(n => n.remove());
    
    // Create notification element
    const notification = document.createElement('div');
    notification.className = `notification-toast alert alert-${type === 'error' ? 'danger' : type} alert-dismissible fade show`;
    notification.style.cssText = `
        position: fixed;
        top: 20px;
        right: 20px;
        z-index: 9999;
        min-width: 300px;
        box-shadow: 0 4px 6px rgba(0,0,0,0.1);
    `;
    
    notification.innerHTML = `
        ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    `;
    
    // Add to page
    document.body.appendChild(notification);
    
    // Auto-remove after 5 seconds
    setTimeout(() => {
        if (notification.parentNode) {
            notification.remove();
        }
    }, 5000);
}

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', function() {
    initializeMapEditor();
});

// Enhanced Real-time Functions for Arena Integration

// Real-time token placement with arena integration
function placeTokenRealTime(hexElement) {
    if (!arenaIntegration || !arenaIntegration.selectedToken || !currentSessionId) return;
    
    const hexId = hexElement.dataset.hexid;
    const tokenId = arenaIntegration.selectedToken;
    
    if (connection && connection.state === signalR.HubConnectionState.Connected) {
        connection.invoke("PlaceToken", currentSessionId, tokenId, hexId)
            .then(() => {
                // Enhanced visual feedback
                showArenaNotification('Token placed in real-time!', 'success');
                
                // Add visual effect
                addPlacementEffect(hexElement);
            })
            .catch(err => {
                console.error("Error placing token:", err);
                showArenaNotification('Failed to place token', 'error');
            });
    }
}

// Sync token placement with arena system
function syncTokenPlacement(hexElement) {
    if (!currentSessionId) return;
    
    // Get the last placed token
    const lastToken = hexElement.querySelector('.token-modern:last-child');
    if (!lastToken) return;
    
    const tokenData = {
        hexId: hexElement.dataset.hexid,
        tokenId: lastToken.dataset.tokenId,
        placementId: lastToken.dataset.placementId,
        serial: lastToken.textContent,
        q: hexElement.dataset.q,
        r: hexElement.dataset.r
    };
    
    // Broadcast to other players
    if (connection && connection.state === signalR.HubConnectionState.Connected) {
        connection.invoke("BroadcastTokenPlacement", currentSessionId, tokenData);
    }
}

// Add visual effect for token placement
function addPlacementEffect(hexElement) {
    // Create ripple effect
    const ripple = document.createElement('div');
    ripple.style.cssText = `
        position: absolute;
        top: 50%;
        left: 50%;
        width: 0;
        height: 0;
        border: 2px solid #3498db;
        border-radius: 50%;
        transform: translate(-50%, -50%);
        animation: ripple 0.6s ease-out;
        pointer-events: none;
        z-index: 20;
    `;
    
    hexElement.appendChild(ripple);
    
    // Remove after animation
    setTimeout(() => {
        if (ripple.parentNode) {
            ripple.remove();
        }
    }, 600);
}

// Enhanced notification system for arena
function showArenaNotification(message, type = 'info') {
    // Use arena's notification system if available
    if (window.showNotification) {
        window.showNotification(message, type);
    } else {
        // Fallback to original notification
        showNotification(message, type);
    }
}

// Enhanced token movement with arena integration
function moveTokenRealTime(placementId, newHexId) {
    if (!currentSessionId) return;
    
    if (connection && connection.state === signalR.HubConnectionState.Connected) {
        connection.invoke("MoveToken", currentSessionId, placementId, newHexId)
            .then(() => {
                showArenaNotification('Token moved in real-time!', 'success');
                
                // Add movement effect
                const newHex = document.querySelector(`[data-hexid="${newHexId}"]`);
                if (newHex) {
                    addMovementEffect(newHex);
                }
            })
            .catch(err => {
                console.error("Error moving token:", err);
                showArenaNotification('Failed to move token', 'error');
            });
    }
}

// Add movement effect
function addMovementEffect(hexElement) {
    // Create movement trail effect
    const trail = document.createElement('div');
    trail.style.cssText = `
        position: absolute;
        top: 0;
        left: 0;
        right: 0;
        bottom: 0;
        background: linear-gradient(45deg, transparent, rgba(52, 152, 219, 0.3), transparent);
        animation: movementTrail 0.8s ease-out;
        pointer-events: none;
        z-index: 15;
    `;
    
    hexElement.appendChild(trail);
    
    // Remove after animation
    setTimeout(() => {
        if (trail.parentNode) {
            trail.remove();
        }
    }, 800);
}

// Enhanced terrain update with arena integration
function updateTerrainRealTime(hexElement, terrainType) {
    if (!currentSessionId) return;
    
    const hexId = hexElement.dataset.hexid;
    
    if (connection && connection.state === signalR.HubConnectionState.Connected) {
        connection.invoke("UpdateHexTerrain", currentSessionId, hexId, terrainType)
            .then(() => {
                showArenaNotification('Terrain updated in real-time!', 'success');
                
                // Add terrain change effect
                addTerrainChangeEffect(hexElement);
            })
            .catch(err => {
                console.error("Error updating terrain:", err);
                showArenaNotification('Failed to update terrain', 'error');
            });
    }
}

// Add terrain change effect
function addTerrainChangeEffect(hexElement) {
    // Create terrain change effect
    const effect = document.createElement('div');
    effect.style.cssText = `
        position: absolute;
        top: 0;
        left: 0;
        right: 0;
        bottom: 0;
        background: radial-gradient(circle, rgba(46, 204, 113, 0.4) 0%, transparent 70%);
        animation: terrainChange 1s ease-out;
        pointer-events: none;
        z-index: 15;
    `;
    
    hexElement.appendChild(effect);
    
    // Remove after animation
    setTimeout(() => {
        if (effect.parentNode) {
            effect.remove();
        }
    }, 1000);
}

// Enhanced turn advancement with arena integration
function advanceTurnRealTime() {
    if (!currentSessionId) return;
    
    if (connection && connection.state === signalR.HubConnectionState.Connected) {
        connection.invoke("AdvanceTurn", currentSessionId)
            .then(() => {
                showArenaNotification('Turn advanced in real-time!', 'success');
                
                // Add turn change effect
                addTurnChangeEffect();
            })
            .catch(err => {
                console.error("Error advancing turn:", err);
                showArenaNotification('Failed to advance turn', 'error');
            });
    }
}

// Add turn change effect
function addTurnChangeEffect() {
    // Create turn change overlay
    const overlay = document.createElement('div');
    overlay.style.cssText = `
        position: fixed;
        top: 0;
        left: 0;
        width: 100%;
        height: 100%;
        background: linear-gradient(45deg, rgba(52, 152, 219, 0.1), rgba(46, 204, 113, 0.1));
        animation: turnChange 2s ease-out;
        pointer-events: none;
        z-index: 1000;
    `;
    
    document.body.appendChild(overlay);
    
    // Remove after animation
    setTimeout(() => {
        if (overlay.parentNode) {
            overlay.remove();
        }
    }, 2000);
}

// Enhanced session joining with arena integration
function joinSessionRealTime(sessionId) {
    if (connection && connection.state === signalR.HubConnectionState.Connected) {
        connection.invoke("JoinSession", sessionId)
            .then(() => {
                currentSessionId = sessionId;
                
                // Update arena state
                if (arenaIntegration) {
                    arenaIntegration.currentSession = sessionId;
                }
                
                showArenaNotification('Joined session in real-time!', 'success');
                
                // Add join effect
                addJoinEffect();
            })
            .catch(err => {
                console.error("Error joining session:", err);
                showArenaNotification('Failed to join session', 'error');
            });
    }
}

// Add join effect
function addJoinEffect() {
    // Create join effect
    const effect = document.createElement('div');
    effect.style.cssText = `
        position: fixed;
        top: 50%;
        left: 50%;
        width: 200px;
        height: 200px;
        background: radial-gradient(circle, rgba(52, 152, 219, 0.3) 0%, transparent 70%);
        transform: translate(-50%, -50%) scale(0);
        animation: joinEffect 1.5s ease-out;
        pointer-events: none;
        z-index: 1000;
    `;
    
    document.body.appendChild(effect);
    
    // Remove after animation
    setTimeout(() => {
        if (effect.parentNode) {
            effect.remove();
        }
    }, 1500);
}

// Add CSS animations for effects
const style = document.createElement('style');
style.textContent = `
    @keyframes ripple {
        0% {
            width: 0;
            height: 0;
            opacity: 1;
        }
        100% {
            width: 100px;
            height: 100px;
            opacity: 0;
        }
    }
    
    @keyframes movementTrail {
        0% {
            transform: translateX(-100%);
            opacity: 0;
        }
        50% {
            opacity: 1;
        }
        100% {
            transform: translateX(100%);
            opacity: 0;
        }
    }
    
    @keyframes terrainChange {
        0% {
            transform: scale(0);
            opacity: 1;
        }
        50% {
            transform: scale(1.2);
        }
        100% {
            transform: scale(1);
            opacity: 0;
        }
    }
    
    @keyframes turnChange {
        0% {
            opacity: 0;
        }
        50% {
            opacity: 1;
        }
        100% {
            opacity: 0;
        }
    }
    
    @keyframes joinEffect {
        0% {
            transform: translate(-50%, -50%) scale(0);
            opacity: 1;
        }
        50% {
            transform: translate(-50%, -50%) scale(1.5);
        }
        100% {
            transform: translate(-50%, -50%) scale(2);
            opacity: 0;
        }
    }
`;

document.head.appendChild(style);

// Export enhanced functions
window.placeTokenRealTime = placeTokenRealTime;
window.moveTokenRealTime = moveTokenRealTime;
window.updateTerrainRealTime = updateTerrainRealTime;
window.advanceTurnRealTime = advanceTurnRealTime;
window.joinSessionRealTime = joinSessionRealTime;
