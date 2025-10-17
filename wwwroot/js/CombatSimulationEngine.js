/**
 * Combat Simulation Engine
 * Implements turn-based combat simulation with NATO-compliant calculations
 * Integrates attack, defense, and terrain effects for realistic combat outcomes
 */

class CombatSimulationEngine {
    constructor() {
        this.currentTurn = 1;
        this.maxTurns = 20;
        this.simulationState = 'idle'; // idle, running, paused, completed
        this.simulationResults = [];
        this.activeCombatants = new Map();
        this.terrainData = new Map();
        this.defenseElements = new Map();
        
        // Turn-based player management
        this.players = new Map();
        this.currentPlayer = null;
        this.playerActions = new Map();
        this.playerVisibility = new Map();
        this.fogOfWarEnabled = true;
        this.turnPhase = 'planning'; // planning, movement, combat, resolution, assessment
        
        this.initializeSimulation();
    }

    /**
     * Initialize simulation engine
     */
    initializeSimulation() {
        console.log('🎮 Initializing Combat Simulation Engine...');
        
        // Initialize NATO effectiveness engine
        if (typeof NatoEffectivenessEngine !== 'undefined') {
            this.effectivenessEngine = window.natoEffectivenessEngine;
        } else {
            console.warn('⚠️ NATO Effectiveness Engine not available');
        }
        
        // Initialize simulation parameters
        this.simulationParameters = {
            turnDuration: 30, // seconds per turn
            maxCombatRounds: 5, // max combat rounds per turn
            fogOfWar: true,
            supplyLines: true,
            moraleEffects: true,
            terrainEffects: true
        };
        
        console.log('✅ Combat Simulation Engine initialized');
    }

    /**
     * Start new simulation
     * @param {Object} simulationConfig - Simulation configuration
     */
    startSimulation(simulationConfig) {
        console.log('🚀 Starting combat simulation...');
        
        this.simulationConfig = {
            name: simulationConfig.name || 'Combat Simulation',
            description: simulationConfig.description || '',
            maxTurns: simulationConfig.maxTurns || 20,
            turnDuration: simulationConfig.turnDuration || 30,
            participants: simulationConfig.participants || [],
            terrain: simulationConfig.terrain || {},
            defenseElements: simulationConfig.defenseElements || [],
            players: simulationConfig.players || ['player1', 'player2'],
            twoPlayerMode: simulationConfig.twoPlayerMode !== false,
            ...simulationConfig
        };
        
        // Reset simulation state
        this.currentTurn = 1;
        this.simulationState = 'running';
        this.simulationResults = [];
        this.activeCombatants.clear();
        this.terrainData.clear();
        this.defenseElements.clear();
        
        // Initialize two-player mode
        if (this.simulationConfig.twoPlayerMode) {
            this.initializePlayers(this.simulationConfig.players);
        }
        
        // Load initial data
        this.loadCombatants(simulationConfig.participants);
        this.loadTerrain(simulationConfig.terrain);
        this.loadDefenseElements(simulationConfig.defenseElements);
        
        // Initialize player visibility (fog of war)
        this.initializePlayerVisibility();
        
        // Start first turn
        if (this.simulationConfig.twoPlayerMode) {
            console.log(`🎮 Player ${this.currentPlayer}'s turn - Phase: ${this.turnPhase}`);
        } else {
            this.executeTurn();
        }
        
        console.log('✅ Simulation started');
        return {
            success: true,
            simulationId: this.generateSimulationId(),
            currentTurn: this.currentTurn,
            currentPlayer: this.currentPlayer,
            turnPhase: this.turnPhase,
            state: this.simulationState
        };
    }
    
    /**
     * Initialize players for two-player mode
     */
    initializePlayers(playerIds) {
        playerIds.forEach((playerId, index) => {
            this.players.set(playerId, {
                id: playerId,
                name: `Player ${index + 1}`,
                side: index === 0 ? 'friendly' : 'hostile',
                color: index === 0 ? '#3498db' : '#e74c3c',
                controlType: 'force',
                isActive: index === 0, // Player 1 starts
                actions: [],
                visibleUnits: [],
                intelligence: []
            });
        });
        
        this.currentPlayer = playerIds[0];
        console.log(`🎮 Initialized ${playerIds.length} players - Starting with: ${this.currentPlayer}`);
    }
    
    /**
     * Initialize player visibility (Fog of War)
     */
    initializePlayerVisibility() {
        if (!this.fogOfWarEnabled) {
            return;
        }
        
        this.players.forEach((player, playerId) => {
            const visibleUnits = [];
            
            // Each player can only see their own units and detected enemy units
            this.activeCombatants.forEach((combatant, combatantId) => {
                if (combatant.type === player.side) {
                    // Always see own units
                    visibleUnits.push(combatantId);
                } else {
                    // Enemy units must be detected
                    if (this.isUnitDetected(combatant, player)) {
                        visibleUnits.push(combatantId);
                    }
                }
            });
            
            this.playerVisibility.set(playerId, {
                visibleUnits: visibleUnits,
                detectedPositions: [],
                intelligence: []
            });
        });
        
        console.log('👁️ Player visibility initialized (Fog of War enabled)');
    }
    
    /**
     * Check if unit is detected by player
     */
    isUnitDetected(unit, player) {
        // Placeholder detection logic - would integrate with terrain, distance, etc.
        const detectionProbability = 0.7;
        return Math.random() < detectionProbability;
    }
    
    /**
     * Submit player action for current turn
     */
    submitPlayerAction(playerId, actionData) {
        if (playerId !== this.currentPlayer) {
            throw new Error('Not your turn');
        }
        
        if (!this.playerActions.has(this.currentTurn)) {
            this.playerActions.set(this.currentTurn, new Map());
        }
        
        this.playerActions.get(this.currentTurn).set(playerId, actionData);
        
        console.log(`✅ Player ${playerId} action submitted for turn ${this.currentTurn}`);
        return { success: true, phase: this.turnPhase };
    }
    
    /**
     * End current player's turn
     */
    endPlayerTurn(playerId) {
        if (playerId !== this.currentPlayer) {
            throw new Error('Not your turn');
        }
        
        // Move to next player or next phase
        const playerList = Array.from(this.players.keys());
        const currentIndex = playerList.indexOf(this.currentPlayer);
        const nextIndex = (currentIndex + 1) % playerList.length;
        
        if (nextIndex === 0) {
            // All players have acted, move to next phase
            this.advancePhase();
        } else {
            // Move to next player in same phase
            this.currentPlayer = playerList[nextIndex];
            console.log(`🎮 Now ${this.currentPlayer}'s turn - Phase: ${this.turnPhase}`);
        }
        
        return {
            success: true,
            nextPlayer: this.currentPlayer,
            phase: this.turnPhase,
            turn: this.currentTurn
        };
    }
    
    /**
     * Advance to next phase
     */
    advancePhase() {
        const phases = ['planning', 'movement', 'combat', 'resolution', 'assessment'];
        const currentIndex = phases.indexOf(this.turnPhase);
        
        if (currentIndex < phases.length - 1) {
            this.turnPhase = phases[currentIndex + 1];
            this.currentPlayer = Array.from(this.players.keys())[0]; // Reset to first player
            console.log(`📋 Advanced to phase: ${this.turnPhase}`);
        } else {
            // All phases complete, advance turn
            this.advanceTurnInPlayerMode();
        }
    }
    
    /**
     * Advance turn in player mode
     */
    advanceTurnInPlayerMode() {
        console.log(`✅ Turn ${this.currentTurn} complete`);
        
        // Process turn results
        this.processTurnResults();
        
        // Check for end conditions
        if (this.checkSimulationEndConditions()) {
            this.endSimulation();
            return;
        }
        
        // Advance to next turn
        this.currentTurn++;
        this.turnPhase = 'planning';
        this.currentPlayer = Array.from(this.players.keys())[0];
        
        // Update player visibility
        this.initializePlayerVisibility();
        
        console.log(`🎮 Starting turn ${this.currentTurn} - ${this.currentPlayer}'s phase: ${this.turnPhase}`);
    }
    
    /**
     * Process results for completed turn
     */
    processTurnResults() {
        const turnActions = this.playerActions.get(this.currentTurn);
        if (!turnActions) {
            return;
        }
        
        const turnResults = {
            turn: this.currentTurn,
            timestamp: new Date().toISOString(),
            actions: Array.from(turnActions.entries()),
            combatResults: [],
            casualties: {},
            aiAnalysis: null
        };
        
        // Integrate AI analysis if available
        if (window.natoAiAnalysisEngine) {
            turnResults.aiAnalysis = this.generateAiTurnAnalysis(turnResults);
        }
        
        this.simulationResults.push(turnResults);
    }
    
    /**
     * Generate AI analysis for turn
     */
    async generateAiTurnAnalysis(turnResults) {
        if (!window.natoAiAnalysisEngine) {
            return null;
        }
        
        const analysis = await window.natoAiAnalysisEngine.performAnalysis({
            type: 'turn_analysis',
            scope: 'tactical',
            timeframe: 'immediate',
            turn: turnResults.turn,
            actions: turnResults.actions,
            combatResults: turnResults.combatResults,
            terrain: this.simulationConfig.terrain
        });
        
        return analysis;
    }
    
    /**
     * Get player-specific view of simulation
     */
    getPlayerView(playerId) {
        const player = this.players.get(playerId);
        if (!player) {
            throw new Error('Player not found');
        }
        
        const visibility = this.playerVisibility.get(playerId) || { visibleUnits: [] };
        
        // Filter combatants based on visibility
        const visibleCombatants = [];
        this.activeCombatants.forEach((combatant, combatantId) => {
            if (visibility.visibleUnits.includes(combatantId)) {
                visibleCombatants.push({
                    id: combatantId,
                    ...combatant,
                    isOwn: combatant.type === player.side
                });
            }
        });
        
        return {
            player: player,
            currentTurn: this.currentTurn,
            currentPhase: this.turnPhase,
            isYourTurn: this.currentPlayer === playerId,
            visibleCombatants: visibleCombatants,
            terrain: this.terrainData,
            defenseElements: this.getVisibleDefenseElements(playerId)
        };
    }
    
    /**
     * Get visible defense elements for player
     */
    getVisibleDefenseElements(playerId) {
        const player = this.players.get(playerId);
        const visibleElements = [];
        
        this.defenseElements.forEach((element, elementId) => {
            // Player can only see their own defense elements unless detected
            if (element.owner === player.side || this.isElementDetected(element, player)) {
                visibleElements.push(element);
            }
        });
        
        return visibleElements;
    }
    
    /**
     * Check if defense element is detected
     */
    isElementDetected(element, player) {
        // Placeholder detection logic
        return Math.random() < 0.5;
    }

    /**
     * Load combatants into simulation
     */
    loadCombatants(participants) {
        participants.forEach(participant => {
            const combatantId = this.generateCombatantId();
            const combatant = {
                id: combatantId,
                name: participant.name,
                type: participant.type, // 'attacker' or 'defender'
                unit: participant.unit,
                position: participant.position,
                strength: participant.strength || 100,
                morale: participant.morale || 'average',
                training: participant.training || 'regular',
                equipment: participant.equipment || 'standard',
                supply: participant.supply || 'adequate',
                actions: [],
                casualties: 0,
                status: 'active'
            };
            
            this.activeCombatants.set(combatantId, combatant);
        });
        
        console.log(`📋 Loaded ${participants.length} combatants`);
    }

    /**
     * Load terrain data
     */
    loadTerrain(terrain) {
        if (terrain && terrain.type) {
            this.terrainData.set('primary', {
                type: terrain.type,
                elevation: terrain.elevation || 0,
                cover: terrain.cover || 'none',
                concealment: terrain.concealment || 'none',
                obstacles: terrain.obstacles || []
            });
        }
        
        console.log('🗺️ Terrain data loaded');
    }

    /**
     * Load defense elements
     */
    loadDefenseElements(defenseElements) {
        defenseElements.forEach(element => {
            const elementId = this.generateElementId();
            this.defenseElements.set(elementId, {
                id: elementId,
                type: element.type,
                category: element.category,
                position: element.position,
                strength: element.strength || 50,
                effectiveness: element.effectiveness || 1.0,
                status: 'active'
            });
        });
        
        console.log(`🛡️ Loaded ${defenseElements.length} defense elements`);
    }

    /**
     * Execute current turn
     */
    async executeTurn() {
        if (this.simulationState !== 'running') {
            return;
        }
        
        console.log(`🎯 Executing turn ${this.currentTurn}...`);
        
        const turnStartTime = Date.now();
        const turnResults = {
            turn: this.currentTurn,
            startTime: new Date().toISOString(),
            combatActions: [],
            casualties: {},
            terrainEffects: {},
            supplyEffects: {},
            moraleEffects: {},
            turnSummary: {}
        };
        
        // Phase 1: Planning Phase
        await this.executePlanningPhase(turnResults);
        
        // Phase 2: Movement Phase
        await this.executeMovementPhase(turnResults);
        
        // Phase 3: Combat Phase
        await this.executeCombatPhase(turnResults);
        
        // Phase 4: Resolution Phase
        await this.executeResolutionPhase(turnResults);
        
        // Phase 5: Assessment Phase
        await this.executeAssessmentPhase(turnResults);
        
        turnResults.endTime = new Date().toISOString();
        turnResults.duration = Date.now() - turnStartTime;
        
        this.simulationResults.push(turnResults);
        
        // Check for simulation end conditions
        if (this.checkSimulationEndConditions()) {
            this.endSimulation();
        } else {
            this.currentTurn++;
            // Continue to next turn
            setTimeout(() => this.executeTurn(), this.simulationParameters.turnDuration * 1000);
        }
        
        console.log(`✅ Turn ${this.currentTurn - 1} completed`);
        return turnResults;
    }

    /**
     * Execute planning phase
     */
    async executePlanningPhase(turnResults) {
        console.log(`📋 Planning phase - Turn ${this.currentTurn}`);
        
        // Simulate planning time and decision making
        const planningActions = [];
        
        this.activeCombatants.forEach(combatant => {
            if (combatant.status === 'active') {
                const planningAction = this.generatePlanningAction(combatant);
                planningActions.push(planningAction);
                combatant.actions.push(planningAction);
            }
        });
        
        turnResults.planningActions = planningActions;
    }

    /**
     * Execute movement phase
     */
    async executeMovementPhase(turnResults) {
        console.log(`🚶 Movement phase - Turn ${this.currentTurn}`);
        
        const movementActions = [];
        
        this.activeCombatants.forEach(combatant => {
            if (combatant.status === 'active') {
                const movementAction = this.generateMovementAction(combatant);
                movementActions.push(movementAction);
                combatant.actions.push(movementAction);
            }
        });
        
        turnResults.movementActions = movementActions;
    }

    /**
     * Execute combat phase
     */
    async executeCombatPhase(turnResults) {
        console.log(`⚔️ Combat phase - Turn ${this.currentTurn}`);
        
        const combatActions = [];
        const combatants = Array.from(this.activeCombatants.values());
        
        // Find combat engagements
        const engagements = this.findCombatEngagements(combatants);
        
        for (const engagement of engagements) {
            const combatResult = await this.resolveCombat(engagement);
            combatActions.push(combatResult);
            turnResults.combatActions.push(combatResult);
        }
        
        turnResults.combatActions = combatActions;
    }

    /**
     * Execute resolution phase
     */
    async executeResolutionPhase(turnResults) {
        console.log(`📊 Resolution phase - Turn ${this.currentTurn}`);
        
        // Calculate casualties
        const casualties = this.calculateTurnCasualties();
        turnResults.casualties = casualties;
        
        // Apply terrain effects
        const terrainEffects = this.calculateTerrainEffects();
        turnResults.terrainEffects = terrainEffects;
        
        // Apply supply effects
        const supplyEffects = this.calculateSupplyEffects();
        turnResults.supplyEffects = supplyEffects;
        
        // Apply morale effects
        const moraleEffects = this.calculateMoraleEffects();
        turnResults.moraleEffects = moraleEffects;
    }

    /**
     * Execute assessment phase
     */
    async executeAssessmentPhase(turnResults) {
        console.log(`📈 Assessment phase - Turn ${this.currentTurn}`);
        
        // Generate turn summary
        const turnSummary = this.generateTurnSummary(turnResults);
        turnResults.turnSummary = turnSummary;
        
        // Update combatant status
        this.updateCombatantStatus();
        
        // Check for victory conditions
        const victoryCheck = this.checkVictoryConditions();
        turnResults.victoryCheck = victoryCheck;
    }

    /**
     * Find combat engagements between combatants
     */
    findCombatEngagements(combatants) {
        const engagements = [];
        
        for (let i = 0; i < combatants.length; i++) {
            for (let j = i + 1; j < combatants.length; j++) {
                const attacker = combatants[i];
                const defender = combatants[j];
                
                // Check if they are in combat range
                if (this.isInCombatRange(attacker, defender)) {
                    engagements.push({
                        attacker: attacker,
                        defender: defender,
                        range: this.calculateDistance(attacker.position, defender.position),
                        terrain: this.getTerrainAtPosition(attacker.position)
                    });
                }
            }
        }
        
        return engagements;
    }

    /**
     * Resolve individual combat engagement
     */
    async resolveCombat(engagement) {
        const { attacker, defender, range, terrain } = engagement;
        
        console.log(`⚔️ Resolving combat: ${attacker.name} vs ${defender.name}`);
        
        // Prepare combat data for NATO effectiveness engine
        const attackData = {
            unitType: attacker.unit.unitType,
            organizationLevel: attacker.unit.organizationLevel,
            strength: attacker.strength,
            attackType: attacker.actions.find(a => a.type === 'attack')?.attackType || 'frontal',
            morale: attacker.morale,
            training: attacker.training,
            equipment: attacker.equipment,
            supply: attacker.supply
        };
        
        const defenseData = {
            unitType: defender.unit.unitType,
            organizationLevel: defender.unit.organizationLevel,
            strength: defender.strength,
            defenseType: defender.actions.find(a => a.type === 'defend')?.defenseType || 'prepared',
            morale: defender.morale,
            training: defender.training,
            equipment: defender.equipment,
            supply: defender.supply
        };
        
        const terrainData = {
            type: terrain?.type || 'open',
            elevation: terrain?.elevation || 0,
            cover: terrain?.cover || 'none'
        };
        
        // Calculate effectiveness
        const effectiveness = this.effectivenessEngine.calculateAttackEffectiveness(
            attackData, defenseData, terrainData
        );
        
        // Apply combat results
        this.applyCombatResults(attacker, defender, effectiveness);
        
        return {
            engagement: engagement,
            effectiveness: effectiveness,
            timestamp: new Date().toISOString()
        };
    }

    /**
     * Apply combat results to combatants
     */
    applyCombatResults(attacker, defender, effectiveness) {
        // Apply casualties
        attacker.casualties += effectiveness.casualties.attacker.casualties;
        defender.casualties += effectiveness.casualties.defender.casualties;
        
        // Update strength
        attacker.strength = Math.max(0, attacker.strength - effectiveness.casualties.attacker.casualties);
        defender.strength = Math.max(0, defender.strength - effectiveness.casualties.defender.casualties);
        
        // Update morale based on results
        if (effectiveness.outcome.result === 'defeat') {
            attacker.morale = this.degradeMorale(attacker.morale);
        } else if (effectiveness.outcome.result === 'victory') {
            attacker.morale = this.improveMorale(attacker.morale);
        }
        
        if (effectiveness.outcome.result === 'victory') {
            defender.morale = this.degradeMorale(defender.morale);
        } else if (effectiveness.outcome.result === 'defeat') {
            defender.morale = this.improveMorale(defender.morale);
        }
        
        // Check for unit destruction
        if (attacker.strength <= 0) {
            attacker.status = 'destroyed';
        }
        if (defender.strength <= 0) {
            defender.status = 'destroyed';
        }
    }

    /**
     * Check if combatants are in combat range
     */
    isInCombatRange(attacker, defender) {
        const distance = this.calculateDistance(attacker.position, defender.position);
        const maxRange = 2000; // 2km maximum combat range
        return distance <= maxRange;
    }

    /**
     * Calculate distance between two positions
     */
    calculateDistance(pos1, pos2) {
        const R = 6371000; // Earth's radius in meters
        const lat1 = pos1[0] * Math.PI / 180;
        const lat2 = pos2[0] * Math.PI / 180;
        const deltaLat = (pos2[0] - pos1[0]) * Math.PI / 180;
        const deltaLon = (pos2[1] - pos1[1]) * Math.PI / 180;
        
        const a = Math.sin(deltaLat / 2) * Math.sin(deltaLat / 2) +
                  Math.cos(lat1) * Math.cos(lat2) *
                  Math.sin(deltaLon / 2) * Math.sin(deltaLon / 2);
        const c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a));
        
        return R * c;
    }

    /**
     * Get terrain at position
     */
    getTerrainAtPosition(position) {
        // For now, return primary terrain data
        // In a full implementation, this would query terrain data at specific coordinates
        return this.terrainData.get('primary') || { type: 'open' };
    }

    /**
     * Generate planning action for combatant
     */
    generatePlanningAction(combatant) {
        const actionTypes = ['attack', 'defend', 'move', 'support', 'reconnaissance'];
        const selectedAction = actionTypes[Math.floor(Math.random() * actionTypes.length)];
        
        return {
            type: 'planning',
            action: selectedAction,
            combatant: combatant.id,
            timestamp: new Date().toISOString(),
            details: this.generateActionDetails(selectedAction)
        };
    }

    /**
     * Generate movement action for combatant
     */
    generateMovementAction(combatant) {
        return {
            type: 'movement',
            combatant: combatant.id,
            from: combatant.position,
            to: this.calculateNewPosition(combatant.position),
            timestamp: new Date().toISOString(),
            distance: 0 // Will be calculated
        };
    }

    /**
     * Generate action details
     */
    generateActionDetails(actionType) {
        const details = {
            'attack': {
                attackType: ['frontal', 'flanking', 'envelopment'][Math.floor(Math.random() * 3)],
                intensity: ['light', 'standard', 'heavy'][Math.floor(Math.random() * 3)]
            },
            'defend': {
                defenseType: ['prepared', 'hasty', 'mobile'][Math.floor(Math.random() * 3)],
                position: 'defensive'
            },
            'move': {
                speed: ['slow', 'normal', 'fast'][Math.floor(Math.random() * 3)],
                formation: ['column', 'line', 'wedge'][Math.floor(Math.random() * 3)]
            }
        };
        
        return details[actionType] || {};
    }

    /**
     * Calculate new position for movement
     */
    calculateNewPosition(currentPosition) {
        // Simple random movement for simulation
        const latOffset = (Math.random() - 0.5) * 0.001;
        const lonOffset = (Math.random() - 0.5) * 0.001;
        
        return [
            currentPosition[0] + latOffset,
            currentPosition[1] + lonOffset
        ];
    }

    /**
     * Calculate turn casualties
     */
    calculateTurnCasualties() {
        const casualties = {
            total: 0,
            byType: {},
            byUnit: {}
        };
        
        this.activeCombatants.forEach(combatant => {
            casualties.total += combatant.casualties;
            casualties.byUnit[combatant.name] = combatant.casualties;
        });
        
        return casualties;
    }

    /**
     * Calculate terrain effects
     */
    calculateTerrainEffects() {
        const effects = {};
        
        this.activeCombatants.forEach(combatant => {
            const terrain = this.getTerrainAtPosition(combatant.position);
            effects[combatant.id] = {
                terrain: terrain.type,
                effects: this.getTerrainEffects(terrain)
            };
        });
        
        return effects;
    }

    /**
     * Calculate supply effects
     */
    calculateSupplyEffects() {
        const effects = {};
        
        this.activeCombatants.forEach(combatant => {
            effects[combatant.id] = {
                supply: combatant.supply,
                effects: this.getSupplyEffects(combatant.supply)
            };
        });
        
        return effects;
    }

    /**
     * Calculate morale effects
     */
    calculateMoraleEffects() {
        const effects = {};
        
        this.activeCombatants.forEach(combatant => {
            effects[combatant.id] = {
                morale: combatant.morale,
                effects: this.getMoraleEffects(combatant.morale)
            };
        });
        
        return effects;
    }

    /**
     * Generate turn summary
     */
    generateTurnSummary(turnResults) {
        const summary = {
            turn: this.currentTurn,
            activeCombatants: this.getActiveCombatantCount(),
            totalCasualties: turnResults.casualties.total,
            majorEvents: this.getMajorEvents(turnResults),
            tacticalSituation: this.assessTacticalSituation()
        };
        
        return summary;
    }

    /**
     * Check simulation end conditions
     */
    checkSimulationEndConditions() {
        // Check if max turns reached
        if (this.currentTurn >= this.simulationConfig.maxTurns) {
            return true;
        }
        
        // Check if all combatants of one side are destroyed
        const attackers = Array.from(this.activeCombatants.values()).filter(c => c.type === 'attacker');
        const defenders = Array.from(this.activeCombatants.values()).filter(c => c.type === 'defender');
        
        const activeAttackers = attackers.filter(c => c.status === 'active');
        const activeDefenders = defenders.filter(c => c.status === 'active');
        
        if (activeAttackers.length === 0 || activeDefenders.length === 0) {
            return true;
        }
        
        return false;
    }

    /**
     * Check victory conditions
     */
    checkVictoryConditions() {
        const attackers = Array.from(this.activeCombatants.values()).filter(c => c.type === 'attacker');
        const defenders = Array.from(this.activeCombatants.values()).filter(c => c.type === 'defender');
        
        const activeAttackers = attackers.filter(c => c.status === 'active');
        const activeDefenders = defenders.filter(c => c.status === 'active');
        
        if (activeAttackers.length === 0) {
            return { winner: 'defender', condition: 'attacker_destroyed' };
        } else if (activeDefenders.length === 0) {
            return { winner: 'attacker', condition: 'defender_destroyed' };
        } else {
            return { winner: null, condition: 'ongoing' };
        }
    }

    /**
     * End simulation
     */
    endSimulation() {
        console.log('🏁 Ending simulation...');
        
        this.simulationState = 'completed';
        
        const finalResults = {
            simulationId: this.generateSimulationId(),
            endTime: new Date().toISOString(),
            totalTurns: this.currentTurn,
            finalResults: this.simulationResults,
            victoryConditions: this.checkVictoryConditions(),
            summary: this.generateSimulationSummary()
        };
        
        console.log('✅ Simulation completed:', finalResults);
        return finalResults;
    }

    /**
     * Generate simulation summary
     */
    generateSimulationSummary() {
        const totalCasualties = this.simulationResults.reduce((sum, turn) => {
            return sum + (turn.casualties?.total || 0);
        }, 0);
        
        const totalCombatActions = this.simulationResults.reduce((sum, turn) => {
            return sum + (turn.combatActions?.length || 0);
        }, 0);
        
        return {
            totalTurns: this.currentTurn,
            totalCasualties: totalCasualties,
            totalCombatActions: totalCombatActions,
            averageTurnDuration: this.simulationResults.reduce((sum, turn) => {
                return sum + (turn.duration || 0);
            }, 0) / this.simulationResults.length,
            finalSituation: this.assessTacticalSituation()
        };
    }

    /**
     * Assess tactical situation
     */
    assessTacticalSituation() {
        const attackers = Array.from(this.activeCombatants.values()).filter(c => c.type === 'attacker');
        const defenders = Array.from(this.activeCombatants.values()).filter(c => c.type === 'defender');
        
        const attackerStrength = attackers.reduce((sum, c) => sum + c.strength, 0);
        const defenderStrength = defenders.reduce((sum, c) => sum + c.strength, 0);
        
        return {
            attackerStrength: attackerStrength,
            defenderStrength: defenderStrength,
            strengthRatio: attackerStrength / defenderStrength,
            situation: this.getSituationDescription(attackerStrength, defenderStrength)
        };
    }

    /**
     * Get situation description
     */
    getSituationDescription(attackerStrength, defenderStrength) {
        const ratio = attackerStrength / defenderStrength;
        
        if (ratio >= 3.0) {
            return 'Attacker has overwhelming superiority';
        } else if (ratio >= 2.0) {
            return 'Attacker has significant advantage';
        } else if (ratio >= 1.5) {
            return 'Attacker has moderate advantage';
        } else if (ratio >= 1.0) {
            return 'Forces are roughly equal';
        } else if (ratio >= 0.67) {
            return 'Defender has moderate advantage';
        } else {
            return 'Defender has significant advantage';
        }
    }

    /**
     * Utility methods
     */
    generateSimulationId() {
        return 'sim_' + Date.now() + '_' + Math.random().toString(36).substr(2, 9);
    }

    generateCombatantId() {
        return 'combatant_' + Date.now() + '_' + Math.random().toString(36).substr(2, 9);
    }

    generateElementId() {
        return 'element_' + Date.now() + '_' + Math.random().toString(36).substr(2, 9);
    }

    getActiveCombatantCount() {
        return Array.from(this.activeCombatants.values()).filter(c => c.status === 'active').length;
    }

    getMajorEvents(turnResults) {
        const events = [];
        
        if (turnResults.combatActions && turnResults.combatActions.length > 0) {
            events.push(`${turnResults.combatActions.length} combat engagements`);
        }
        
        if (turnResults.casualties && turnResults.casualties.total > 0) {
            events.push(`${turnResults.casualties.total} total casualties`);
        }
        
        return events;
    }

    degradeMorale(currentMorale) {
        const moraleLevels = ['broken', 'low', 'average', 'high'];
        const currentIndex = moraleLevels.indexOf(currentMorale);
        return moraleLevels[Math.max(0, currentIndex - 1)];
    }

    improveMorale(currentMorale) {
        const moraleLevels = ['broken', 'low', 'average', 'high'];
        const currentIndex = moraleLevels.indexOf(currentMorale);
        return moraleLevels[Math.min(moraleLevels.length - 1, currentIndex + 1)];
    }

    getTerrainEffects(terrain) {
        const effects = {
            'urban': { cover: 'high', concealment: 'high', movement: 'slow' },
            'forest': { cover: 'medium', concealment: 'high', movement: 'slow' },
            'hills': { cover: 'medium', concealment: 'medium', movement: 'normal' },
            'open': { cover: 'low', concealment: 'low', movement: 'fast' },
            'swamp': { cover: 'low', concealment: 'medium', movement: 'very_slow' }
        };
        
        return effects[terrain.type] || { cover: 'none', concealment: 'none', movement: 'normal' };
    }

    getSupplyEffects(supply) {
        const effects = {
            'abundant': { effectiveness: 1.1, morale: 1.05 },
            'adequate': { effectiveness: 1.0, morale: 1.0 },
            'limited': { effectiveness: 0.9, morale: 0.95 },
            'critical': { effectiveness: 0.7, morale: 0.8 }
        };
        
        return effects[supply] || { effectiveness: 1.0, morale: 1.0 };
    }

    getMoraleEffects(morale) {
        const effects = {
            'high': { effectiveness: 1.2, cohesion: 1.1 },
            'average': { effectiveness: 1.0, cohesion: 1.0 },
            'low': { effectiveness: 0.8, cohesion: 0.9 },
            'broken': { effectiveness: 0.5, cohesion: 0.7 }
        };
        
        return effects[morale] || { effectiveness: 1.0, cohesion: 1.0 };
    }

    updateCombatantStatus() {
        this.activeCombatants.forEach(combatant => {
            if (combatant.strength <= 0) {
                combatant.status = 'destroyed';
            } else if (combatant.strength < 25) {
                combatant.status = 'critical';
            } else if (combatant.strength < 50) {
                combatant.status = 'damaged';
            } else {
                combatant.status = 'active';
            }
        });
    }
}

// Initialize global instance
window.combatSimulationEngine = new CombatSimulationEngine();
console.log('🎮 Combat Simulation Engine initialized');
