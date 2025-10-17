/**
 * Scenario Planning - Core functionality for war game scenario planning
 * Handles movement planning, orders, objectives, and turn management
 */

class ScenarioPlanning {
    constructor() {
        this.currentTurn = 1;
        this.orders = [];
        this.objectives = [];
        this.movementOrders = [];
        this.isPlanningMode = false;
        this.selectedUnit = null;
        this.routeDrawing = null;
        this.undoStack = [];
        
        this.initializeEventListeners();
    }

    /**
     * Initialize event listeners for scenario planning
     */
    initializeEventListeners() {
        // Plan Move button
        const planMoveBtn = document.querySelector('[onclick="planMovement()"]');
        if (planMoveBtn) {
            planMoveBtn.removeAttribute('onclick');
            planMoveBtn.addEventListener('click', () => this.startMovementPlanning());
        }

        // Set Objective button
        const setObjectiveBtn = document.querySelector('[onclick="createObjective()"]');
        if (setObjectiveBtn) {
            setObjectiveBtn.removeAttribute('onclick');
            setObjectiveBtn.addEventListener('click', () => this.startObjectiveSetting());
        }

        // Confirm Orders button
        const confirmOrdersBtn = document.querySelector('[onclick="confirmOrders()"]');
        if (confirmOrdersBtn) {
            confirmOrdersBtn.removeAttribute('onclick');
            confirmOrdersBtn.addEventListener('click', () => this.confirmOrders());
        }

        // Simulate Turn button
        const simulateBtn = document.querySelector('[onclick="simulateTurn()"]');
        if (simulateBtn) {
            simulateBtn.removeAttribute('onclick');
            simulateBtn.addEventListener('click', () => this.simulateTurn());
        }

        // Advance Turn button
        const advanceBtn = document.querySelector('[onclick="advanceTurn()"]');
        if (advanceBtn) {
            advanceBtn.removeAttribute('onclick');
            advanceBtn.addEventListener('click', () => this.advanceTurn());
        }
    }

    /**
     * Start movement planning mode
     */
    startMovementPlanning() {
        if (!window.gameMap) {
            showNotification('Map not available', 'error');
            return;
        }

        this.isPlanningMode = true;
        this.showUnitSelectionModal();
    }

    /**
     * Show unit selection modal for movement planning
     */
    showUnitSelectionModal() {
        const modal = document.createElement('div');
        modal.className = 'gameplay-modal';
        modal.id = 'unitSelectionModal';
        modal.innerHTML = `
            <div class="gameplay-modal-content">
                <div class="gameplay-modal-header">
                    <h3><i class="fas fa-route"></i> Select Unit for Movement</h3>
                    <button class="gameplay-modal-close" onclick="this.closest('.gameplay-modal').remove()">&times;</button>
                </div>
                <div class="gameplay-modal-body">
                    <div class="unit-list">
                        <div class="loading-units">
                            <i class="fas fa-spinner fa-spin"></i>
                            <p>Loading deployed units...</p>
                        </div>
                    </div>
                </div>
                <div class="gameplay-modal-footer">
                    <button class="gameplay-btn" onclick="this.closest('.gameplay-modal').remove()">Cancel</button>
                </div>
            </div>
        `;

        document.body.appendChild(modal);
        this.loadDeployedUnits();
    }

    /**
     * Load deployed units for selection
     */
    async loadDeployedUnits() {
        try {
            const response = await fetch('/GamePlay/GetDeployedUnits');
            const data = await response.json();
            
            if (data.success) {
                this.populateUnitList(data.units);
            } else {
                showNotification('Failed to load units: ' + data.message, 'error');
            }
        } catch (error) {
            console.error('Error loading units:', error);
            showNotification('Error loading units', 'error');
        }
    }

    /**
     * Populate unit list in modal
     */
    populateUnitList(units) {
        const unitList = document.querySelector('#unitSelectionModal .unit-list');
        if (!unitList) return;

        unitList.innerHTML = '';

        if (units.length === 0) {
            unitList.innerHTML = '<p class="no-units">No deployed units found</p>';
            return;
        }

        units.forEach(unit => {
            const unitElement = document.createElement('div');
            unitElement.className = 'unit-option';
            unitElement.innerHTML = `
                <div class="unit-info">
                    <h5>${unit.name}</h5>
                    <p>${unit.type} • Strength: ${unit.strength}</p>
                    <p>Position: ${unit.position}</p>
                </div>
                <div class="unit-actions">
                    <button class="btn-select" onclick="scenarioPlanning.selectUnitForMovement('${unit.id}')">
                        Select
                    </button>
                </div>
            `;
            unitList.appendChild(unitElement);
        });
    }

    /**
     * Select unit for movement planning
     */
    selectUnitForMovement(unitId) {
        this.selectedUnit = unitId;
        document.getElementById('unitSelectionModal').remove();
        this.startRouteDrawing();
    }

    /**
     * Start route drawing on map
     */
    startRouteDrawing() {
        if (!window.gameMap) return;

        this.routeDrawing = new RouteDrawing(window.gameMap);
        this.routeDrawing.startDrawing((route) => {
            this.showMovementDetailsModal(route);
        });

        showNotification('Click on map to start drawing route. Double-click to finish.', 'info');
    }

    /**
     * Show movement details modal
     */
    showMovementDetailsModal(route) {
        const modal = document.createElement('div');
        modal.className = 'gameplay-modal';
        modal.id = 'movementDetailsModal';
        modal.innerHTML = `
            <div class="gameplay-modal-content">
                <div class="gameplay-modal-header">
                    <h3><i class="fas fa-route"></i> Movement Details</h3>
                    <button class="gameplay-modal-close" onclick="this.closest('.gameplay-modal').remove()">&times;</button>
                </div>
                <div class="gameplay-modal-body">
                    <div class="form-group">
                        <label for="movementType">Movement Type</label>
                        <select id="movementType" class="form-control">
                            <option value="march">March</option>
                            <option value="tactical">Tactical</option>
                            <option value="combat">Combat</option>
                        </select>
                    </div>
                    <div class="form-group">
                        <label for="movementSpeed">Speed Modifier (%)</label>
                        <input type="number" id="movementSpeed" class="form-control" value="100" min="10" max="200">
                    </div>
                    <div class="form-group">
                        <label for="movementNotes">Notes</label>
                        <textarea id="movementNotes" class="form-control" rows="3" placeholder="Additional movement instructions..."></textarea>
                    </div>
                    <div class="route-summary">
                        <h6>Route Summary</h6>
                        <p>Distance: <span id="routeDistance">${route.distance.toFixed(2)} km</span></p>
                        <p>Estimated Time: <span id="routeTime">${route.estimatedTime}</span></p>
                    </div>
                </div>
                <div class="gameplay-modal-footer">
                    <button class="gameplay-btn" onclick="this.closest('.gameplay-modal').remove()">Cancel</button>
                    <button class="gameplay-btn primary" onclick="scenarioPlanning.confirmMovement()">Confirm Movement</button>
                </div>
            </div>
        `;

        document.body.appendChild(modal);
    }

    /**
     * Confirm movement order
     */
    confirmMovement() {
        const movementType = document.getElementById('movementType').value;
        const speedModifier = document.getElementById('movementSpeed').value;
        const notes = document.getElementById('movementNotes').value;

        const movementOrder = {
            id: this.generateId(),
            unitId: this.selectedUnit,
            type: 'movement',
            movementType: movementType,
            speedModifier: speedModifier,
            notes: notes,
            route: this.routeDrawing.getRoute(),
            timestamp: new Date()
        };

        this.orders.push(movementOrder);
        this.undoStack.push({ action: 'add', order: movementOrder });

        document.getElementById('movementDetailsModal').remove();
        this.routeDrawing.clear();
        this.routeDrawing = null;
        this.selectedUnit = null;
        this.isPlanningMode = false;

        showNotification('Movement order added successfully', 'success');
        this.updateOrdersDisplay();
    }

    /**
     * Start objective setting mode
     */
    startObjectiveSetting() {
        if (!window.gameMap) {
            showNotification('Map not available', 'error');
            return;
        }

        this.showObjectiveTypeModal();
    }

    /**
     * Show objective type selection modal
     */
    showObjectiveTypeModal() {
        const modal = document.createElement('div');
        modal.className = 'gameplay-modal';
        modal.id = 'objectiveTypeModal';
        modal.innerHTML = `
            <div class="gameplay-modal-content">
                <div class="gameplay-modal-header">
                    <h3><i class="fas fa-flag"></i> Set Objective</h3>
                    <button class="gameplay-modal-close" onclick="this.closest('.gameplay-modal').remove()">&times;</button>
                </div>
                <div class="gameplay-modal-body">
                    <div class="objective-types">
                        <div class="objective-type" onclick="scenarioPlanning.selectObjectiveType('capture')">
                            <i class="fas fa-flag"></i>
                            <h5>Capture</h5>
                            <p>Capture and hold a location</p>
                        </div>
                        <div class="objective-type" onclick="scenarioPlanning.selectObjectiveType('destroy')">
                            <i class="fas fa-bomb"></i>
                            <h5>Destroy</h5>
                            <p>Destroy enemy assets</p>
                        </div>
                        <div class="objective-type" onclick="scenarioPlanning.selectObjectiveType('defend')">
                            <i class="fas fa-shield-alt"></i>
                            <h5>Defend</h5>
                            <p>Defend a location</p>
                        </div>
                        <div class="objective-type" onclick="scenarioPlanning.selectObjectiveType('recon')">
                            <i class="fas fa-eye"></i>
                            <h5>Reconnaissance</h5>
                            <p>Gather intelligence</p>
                        </div>
                    </div>
                </div>
                <div class="gameplay-modal-footer">
                    <button class="gameplay-btn" onclick="this.closest('.gameplay-modal').remove()">Cancel</button>
                </div>
            </div>
        `;

        document.body.appendChild(modal);
    }

    /**
     * Select objective type and start placement
     */
    selectObjectiveType(type) {
        this.selectedObjectiveType = type;
        document.getElementById('objectiveTypeModal').remove();
        this.startObjectivePlacement();
    }

    /**
     * Start objective placement on map
     */
    startObjectivePlacement() {
        if (!window.gameMap) return;

        showNotification('Click on map to place objective marker', 'info');
        
        const map = window.gameMap;
        const marker = L.marker([0, 0], { draggable: true });
        
        map.on('click', (e) => {
            marker.setLatLng(e.latlng);
            if (!map.hasLayer(marker)) {
                marker.addTo(map);
            }
            this.showObjectiveDetailsModal(e.latlng);
        });
    }

    /**
     * Show objective details modal
     */
    showObjectiveDetailsModal(latlng) {
        const modal = document.createElement('div');
        modal.className = 'gameplay-modal';
        modal.id = 'objectiveDetailsModal';
        modal.innerHTML = `
            <div class="gameplay-modal-content">
                <div class="gameplay-modal-header">
                    <h3><i class="fas fa-flag"></i> Objective Details</h3>
                    <button class="gameplay-modal-close" onclick="this.closest('.gameplay-modal').remove()">&times;</button>
                </div>
                <div class="gameplay-modal-body">
                    <div class="form-group">
                        <label for="objectiveName">Objective Name</label>
                        <input type="text" id="objectiveName" class="form-control" required>
                    </div>
                    <div class="form-group">
                        <label for="objectiveDescription">Description</label>
                        <textarea id="objectiveDescription" class="form-control" rows="3"></textarea>
                    </div>
                    <div class="form-group">
                        <label for="objectiveDeadline">Deadline (hours)</label>
                        <input type="number" id="objectiveDeadline" class="form-control" value="24" min="1">
                    </div>
                    <div class="form-group">
                        <label for="objectiveReward">Reward Points</label>
                        <input type="number" id="objectiveReward" class="form-control" value="100" min="0">
                    </div>
                    <div class="form-group">
                        <label for="objectivePriority">Priority</label>
                        <select id="objectivePriority" class="form-control">
                            <option value="low">Low</option>
                            <option value="medium" selected>Medium</option>
                            <option value="high">High</option>
                            <option value="critical">Critical</option>
                        </select>
                    </div>
                </div>
                <div class="gameplay-modal-footer">
                    <button class="gameplay-btn" onclick="this.closest('.gameplay-modal').remove()">Cancel</button>
                    <button class="gameplay-btn primary" onclick="scenarioPlanning.confirmObjective()">Set Objective</button>
                </div>
            </div>
        `;

        document.body.appendChild(modal);
    }

    /**
     * Confirm objective
     */
    confirmObjective() {
        const name = document.getElementById('objectiveName').value;
        const description = document.getElementById('objectiveDescription').value;
        const deadline = document.getElementById('objectiveDeadline').value;
        const reward = document.getElementById('objectiveReward').value;
        const priority = document.getElementById('objectivePriority').value;

        if (!name) {
            showNotification('Please enter objective name', 'error');
            return;
        }

        const objective = {
            id: this.generateId(),
            type: this.selectedObjectiveType,
            name: name,
            description: description,
            deadline: deadline,
            reward: reward,
            priority: priority,
            status: 'active',
            timestamp: new Date()
        };

        this.objectives.push(objective);
        this.undoStack.push({ action: 'add', objective: objective });

        document.getElementById('objectiveDetailsModal').remove();
        this.selectedObjectiveType = null;

        showNotification('Objective set successfully', 'success');
        this.updateObjectivesDisplay();
    }

    /**
     * Confirm all orders for the turn
     */
    confirmOrders() {
        if (this.orders.length === 0) {
            showNotification('No orders to confirm', 'warning');
            return;
        }

        const confirmed = confirm(`Confirm ${this.orders.length} orders for Turn ${this.currentTurn}?`);
        if (confirmed) {
            this.orders.forEach(order => {
                order.status = 'confirmed';
                order.confirmedAt = new Date();
            });

            showNotification(`Confirmed ${this.orders.length} orders for Turn ${this.currentTurn}`, 'success');
            this.updateOrdersDisplay();
        }
    }

    /**
     * Simulate current turn
     */
    async simulateTurn() {
        const confirmedOrders = this.orders.filter(order => order.status === 'confirmed');
        
        if (confirmedOrders.length === 0) {
            showNotification('No confirmed orders to simulate', 'warning');
            return;
        }

        showNotification('Simulating turn...', 'info');

        try {
            const response = await fetch('/GamePlay/SimulateTurn', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    turn: this.currentTurn,
                    orders: confirmedOrders
                })
            });

            const data = await response.json();
            
            if (data.success) {
                showNotification('Turn simulation completed', 'success');
                this.showSimulationResults(data.results);
            } else {
                showNotification('Simulation failed: ' + data.message, 'error');
            }
        } catch (error) {
            console.error('Error simulating turn:', error);
            showNotification('Error simulating turn', 'error');
        }
    }

    /**
     * Advance to next turn
     */
    advanceTurn() {
        const confirmed = confirm(`Advance to Turn ${this.currentTurn + 1}?`);
        if (confirmed) {
            this.currentTurn++;
            this.orders = [];
            this.undoStack = [];
            
            showNotification(`Advanced to Turn ${this.currentTurn}`, 'success');
            this.updateTurnDisplay();
            this.updateOrdersDisplay();
        }
    }

    /**
     * Undo last order
     */
    undoLastOrder() {
        if (this.undoStack.length === 0) {
            showNotification('Nothing to undo', 'warning');
            return;
        }

        const lastAction = this.undoStack.pop();
        
        if (lastAction.action === 'add') {
            if (lastAction.order) {
                const index = this.orders.findIndex(o => o.id === lastAction.order.id);
                if (index !== -1) {
                    this.orders.splice(index, 1);
                }
            }
            if (lastAction.objective) {
                const index = this.objectives.findIndex(o => o.id === lastAction.objective.id);
                if (index !== -1) {
                    this.objectives.splice(index, 1);
                }
            }
        }

        showNotification('Last order undone', 'info');
        this.updateOrdersDisplay();
        this.updateObjectivesDisplay();
    }

    /**
     * Update orders display
     */
    updateOrdersDisplay() {
        // Implementation for updating orders UI
        console.log('Orders updated:', this.orders);
    }

    /**
     * Update objectives display
     */
    updateObjectivesDisplay() {
        // Implementation for updating objectives UI
        console.log('Objectives updated:', this.objectives);
    }

    /**
     * Update turn display
     */
    updateTurnDisplay() {
        const turnDisplay = document.getElementById('currentTurn');
        if (turnDisplay) {
            turnDisplay.textContent = this.currentTurn;
        }
    }

    /**
     * Show simulation results
     */
    showSimulationResults(results) {
        // Implementation for showing simulation results
        console.log('Simulation results:', results);
    }

    /**
     * Generate unique ID
     */
    generateId() {
        return 'order_' + Date.now() + '_' + Math.random().toString(36).substr(2, 9);
    }
}

/**
 * Route Drawing class for drawing movement routes on map
 */
class RouteDrawing {
    constructor(map) {
        this.map = map;
        this.route = [];
        this.polyline = null;
        this.isDrawing = false;
        this.callback = null;
    }

    /**
     * Start drawing route
     */
    startDrawing(callback) {
        this.callback = callback;
        this.isDrawing = true;
        this.route = [];
        
        this.map.on('click', this.onMapClick.bind(this));
        this.map.on('dblclick', this.onMapDoubleClick.bind(this));
        
        showNotification('Click to add waypoints, double-click to finish', 'info');
    }

    /**
     * Handle map click
     */
    onMapClick(e) {
        if (!this.isDrawing) return;
        
        this.route.push(e.latlng);
        this.updatePolyline();
    }

    /**
     * Handle map double click
     */
    onMapDoubleClick(e) {
        if (!this.isDrawing) return;
        
        this.finishDrawing();
    }

    /**
     * Update polyline on map
     */
    updatePolyline() {
        if (this.polyline) {
            this.map.removeLayer(this.polyline);
        }
        
        if (this.route.length > 1) {
            this.polyline = L.polyline(this.route, {
                color: '#ff6b6b',
                weight: 3,
                opacity: 0.8
            }).addTo(this.map);
        }
    }

    /**
     * Finish drawing
     */
    finishDrawing() {
        if (this.route.length < 2) {
            showNotification('Route must have at least 2 points', 'warning');
            return;
        }
        
        this.isDrawing = false;
        this.map.off('click', this.onMapClick);
        this.map.off('dblclick', this.onMapDoubleClick);
        
        const routeData = {
            waypoints: this.route,
            distance: this.calculateDistance(),
            estimatedTime: this.calculateEstimatedTime()
        };
        
        if (this.callback) {
            this.callback(routeData);
        }
    }

    /**
     * Calculate route distance
     */
    calculateDistance() {
        let totalDistance = 0;
        for (let i = 1; i < this.route.length; i++) {
            totalDistance += this.route[i-1].distanceTo(this.route[i]) / 1000; // Convert to km
        }
        return totalDistance;
    }

    /**
     * Calculate estimated travel time
     */
    calculateEstimatedTime() {
        const distance = this.calculateDistance();
        const avgSpeed = 20; // km/h
        const hours = distance / avgSpeed;
        return Math.round(hours * 10) / 10 + ' hours';
    }

    /**
     * Get route data
     */
    getRoute() {
        return {
            waypoints: this.route,
            distance: this.calculateDistance(),
            estimatedTime: this.calculateEstimatedTime()
        };
    }

    /**
     * Clear route
     */
    clear() {
        if (this.polyline) {
            this.map.removeLayer(this.polyline);
            this.polyline = null;
        }
        this.route = [];
        this.isDrawing = false;
    }
}

// Initialize scenario planning when DOM is ready
$(document).ready(function() {
    window.scenarioPlanning = new ScenarioPlanning();
});

// Make functions globally available
window.planMovement = () => window.scenarioPlanning.startMovementPlanning();
window.createObjective = () => window.scenarioPlanning.startObjectiveSetting();
window.confirmOrders = () => window.scenarioPlanning.confirmOrders();
window.simulateTurn = () => window.scenarioPlanning.simulateTurn();
window.advanceTurn = () => window.scenarioPlanning.advanceTurn();
window.undoLastOrder = () => window.scenarioPlanning.undoLastOrder();
