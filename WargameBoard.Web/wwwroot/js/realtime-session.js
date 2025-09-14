// Real-time Session JavaScript
let connection = null;
let currentSessionId = null;

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', function() {
    // Get session ID from URL or data attribute
    currentSessionId = getSessionIdFromUrl();
    if (currentSessionId) {
        initializeSignalR();
        setupEventListeners();
        updateUI();
    }
});

// Get session ID from URL
function getSessionIdFromUrl() {
    const path = window.location.pathname;
    const match = path.match(/\/GameSessions\/Details\/(\d+)/);
    return match ? match[1] : null;
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
        addActivityToFeed("Token placed", "placement", placementData);
        updateMiniHexGrid();
    });

    connection.on("TokenMoved", function (moveData) {
        addActivityToFeed("Token moved", "movement", moveData);
        updateMiniHexGrid();
    });

    connection.on("TokenRemoved", function (placementId) {
        addActivityToFeed("Token removed", "placement", { placementId });
        updateMiniHexGrid();
    });

    connection.on("HexUpdated", function (hexData) {
        addActivityToFeed("Hex terrain updated", "placement", hexData);
        updateMiniHexGrid();
    });

    connection.on("HexFeatureAdded", function (featureData) {
        addActivityToFeed("Feature added to hex", "placement", featureData);
        updateMiniHexGrid();
    });

    connection.on("HexFeatureRemoved", function (featureId) {
        addActivityToFeed("Feature removed from hex", "placement", { featureId });
        updateMiniHexGrid();
    });

    connection.on("TurnAdvanced", function (turnData) {
        addActivityToFeed(`Turn advanced to ${turnData.Number}`, "turn", turnData);
        updateCurrentTurn(turnData);
        updateTurnHistory(turnData);
    });

    connection.on("ObjectiveControlUpdated", function (objectiveData) {
        addActivityToFeed("Objective control updated", "placement", objectiveData);
    });

    // Error handlers
    connection.on("PlacementError", function (message) {
        addActivityToFeed(`Error: ${message}`, "error", { message });
    });

    connection.on("MoveError", function (message) {
        addActivityToFeed(`Error: ${message}`, "error", { message });
    });

    connection.on("RemoveError", function (message) {
        addActivityToFeed(`Error: ${message}`, "error", { message });
    });

    connection.on("UpdateError", function (message) {
        addActivityToFeed(`Error: ${message}`, "error", { message });
    });

    connection.on("TurnError", function (message) {
        addActivityToFeed(`Error: ${message}`, "error", { message });
    });

    // Start connection
    connection.start().then(function () {
        updateConnectionStatus(true);
        console.log("SignalR connection started");
        
        // Auto-join session if available
        if (currentSessionId) {
            joinSession(currentSessionId);
        }
    }).catch(function (err) {
        updateConnectionStatus(false);
        console.error("SignalR connection error:", err);
    });
}

// Setup event listeners
function setupEventListeners() {
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
}

// Join session
function joinSession(sessionId) {
    if (connection && connection.state === signalR.HubConnectionState.Connected) {
        connection.invoke("JoinSession", sessionId).then(function() {
            updateSessionStatus({ Id: sessionId, Scenario: "Loading...", CurrentSide: "Loading..." });
            document.getElementById('joinSessionBtn').disabled = true;
            document.getElementById('leaveSessionBtn').disabled = false;
            document.getElementById('advanceTurnBtn').disabled = false;
            addActivityToFeed("Joined session", "info", { sessionId });
        }).catch(function(err) {
            console.error("Error joining session:", err);
            addActivityToFeed("Failed to join session", "error", { error: err });
        });
    }
}

// Leave session
function leaveSession() {
    if (connection && connection.state === signalR.HubConnectionState.Connected && currentSessionId) {
        connection.invoke("LeaveSession", currentSessionId).then(function() {
            updateSessionStatus(null);
            document.getElementById('joinSessionBtn').disabled = false;
            document.getElementById('leaveSessionBtn').disabled = true;
            document.getElementById('advanceTurnBtn').disabled = true;
            addActivityToFeed("Left session", "info", {});
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
            addActivityToFeed("Failed to advance turn", "error", { error: err });
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
    const sideElement = document.getElementById('currentSide');
    
    if (sessionData) {
        statusElement.textContent = `Connected to ${sessionData.Scenario || 'Unknown Session'}`;
        if (sessionData.Turns && sessionData.Turns.length > 0) {
            const currentTurn = sessionData.Turns[sessionData.Turns.length - 1];
            turnElement.textContent = currentTurn.Number;
        }
        if (sessionData.CurrentSide) {
            sideElement.textContent = sessionData.CurrentSide;
        }
    } else {
        statusElement.textContent = 'Not Connected';
        turnElement.textContent = '-';
        sideElement.textContent = '-';
    }
}

function updateCurrentTurn(turnData) {
    document.getElementById('currentTurn').textContent = turnData.Number;
}

function updateTurnHistory(turnData) {
    const turnHistory = document.getElementById('turnHistory');
    const turnItem = document.createElement('div');
    turnItem.className = 'd-flex justify-content-between align-items-center mb-2';
    turnItem.innerHTML = `
        <span>Turn ${turnData.Number}</span>
        <small class="text-muted">
            ${new Date(turnData.StartedAt).toLocaleTimeString()}
            ${turnData.EndedAt ? ` - ${new Date(turnData.EndedAt).toLocaleTimeString()}` : ''}
        </small>
    `;
    turnHistory.appendChild(turnItem);
}

function addActivityToFeed(message, type, data) {
    const activityFeed = document.getElementById('activityFeed');
    
    // Clear "no activity" message if present
    const noActivityMsg = activityFeed.querySelector('.text-muted.text-center');
    if (noActivityMsg) {
        noActivityMsg.remove();
    }
    
    const activityItem = document.createElement('div');
    activityItem.className = `activity-item ${type}`;
    activityItem.innerHTML = `
        <div class="d-flex justify-content-between align-items-start">
            <div>
                <strong>${message}</strong>
                <small class="text-muted d-block">${new Date().toLocaleTimeString()}</small>
            </div>
            <button type="button" class="btn-close btn-close-sm" onclick="this.parentElement.parentElement.remove()"></button>
        </div>
    `;
    
    // Add to top of feed
    activityFeed.insertBefore(activityItem, activityFeed.firstChild);
    
    // Limit to 50 items
    const items = activityFeed.querySelectorAll('.activity-item');
    if (items.length > 50) {
        items[items.length - 1].remove();
    }
    
    // Auto-scroll to top
    activityFeed.scrollTop = 0;
}

function updateMiniHexGrid() {
    // This would update a mini hex grid preview
    // For now, just show a placeholder
    const miniGrid = document.getElementById('miniHexGrid');
    if (miniGrid && !miniGrid.querySelector('.mini-hex')) {
        // Create a simple 6x6 grid for preview
        for (let i = 0; i < 36; i++) {
            const hex = document.createElement('div');
            hex.className = 'mini-hex terrain-clear';
            hex.title = `Hex ${i}`;
            miniGrid.appendChild(hex);
        }
    }
}

function updateUI() {
    // Initialize UI state
    if (currentSessionId) {
        document.getElementById('joinSessionBtn').disabled = false;
    }
}

// Export functions for global access
window.joinSession = joinSession;
window.leaveSession = leaveSession;
window.advanceTurn = advanceTurn;
