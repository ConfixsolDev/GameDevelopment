/**
 * Terrain Integration Service
 * Integrates NATO terrain classification with existing movement adjudication and simulation systems
 * Provides seamless terrain impact calculations for combat simulations
 */

class TerrainIntegrationService {
    constructor() {
        this.terrainEngine = null;
        this.movementService = null;
        this.simulationEngine = null;
        this.terrainCache = new Map();
        this.integrationStatus = 'disconnected';
        
        this.initializeIntegration();
    }

    /**
     * Initialize terrain integration service
     */
    initializeIntegration() {
        console.log('🔗 Initializing Terrain Integration Service...');
        
        // Check for required services
        this.checkRequiredServices();
        
        // Initialize integration
        this.setupIntegration();
        
        console.log('✅ Terrain Integration Service initialized');
    }

    /**
     * Check for required services
     */
    checkRequiredServices() {
        // Check for NATO Terrain Engine
        if (typeof NatoTerrainEngine !== 'undefined' && window.natoTerrainEngine) {
            this.terrainEngine = window.natoTerrainEngine;
            console.log('✅ NATO Terrain Engine connected');
        } else {
            console.warn('⚠️ NATO Terrain Engine not available');
        }
        
        // Check for Movement Service
        if (window.movementService) {
            this.movementService = window.movementService;
            console.log('✅ Movement Service connected');
        } else {
            console.warn('⚠️ Movement Service not available');
        }
        
        // Check for Simulation Engine
        if (window.combatSimulationEngine) {
            this.simulationEngine = window.combatSimulationEngine;
            console.log('✅ Simulation Engine connected');
        } else {
            console.warn('⚠️ Simulation Engine not available');
        }
    }

    /**
     * Setup integration between services
     */
    setupIntegration() {
        if (this.terrainEngine && this.movementService) {
            this.integrateWithMovementService();
        }
        
        if (this.terrainEngine && this.simulationEngine) {
            this.integrateWithSimulationEngine();
        }
        
        this.integrationStatus = 'connected';
        console.log('🔗 Terrain integration setup complete');
    }

    /**
     * Integrate with movement service
     */
    integrateWithMovementService() {
        console.log('🔗 Integrating terrain with movement service...');
        
        // Override movement calculation methods to include terrain impact
        if (this.movementService.calculateMovementCost) {
            const originalCalculateMovementCost = this.movementService.calculateMovementCost.bind(this.movementService);
            
            this.movementService.calculateMovementCost = (from, to, unitType) => {
                // Get terrain classification for destination
                const terrainClassification = this.terrainEngine.classifyTerrain(to);
                
                // Calculate base movement cost
                const baseCost = originalCalculateMovementCost(from, to, unitType);
                
                // Apply terrain impact
                const terrainImpact = this.terrainEngine.getMovementImpact(unitType, terrainClassification.terrainType);
                
                // Calculate final cost
                const finalCost = baseCost / terrainImpact;
                
                console.log(`🚶 Movement cost calculated: ${baseCost} -> ${finalCost} (terrain impact: ${terrainImpact})`);
                
                return finalCost;
            };
        }
        
        // Override movement validation to include terrain restrictions
        if (this.movementService.validateMovement) {
            const originalValidateMovement = this.movementService.validateMovement.bind(this.movementService);
            
            this.movementService.validateMovement = (from, to, unitType) => {
                // Get terrain classification for destination
                const terrainClassification = this.terrainEngine.classifyTerrain(to);
                
                // Check terrain movement restrictions
                const movementImpact = this.terrainEngine.getMovementImpact(unitType, terrainClassification.terrainType);
                
                // If movement is severely restricted, invalidate
                if (movementImpact < 0.3) {
                    console.log(`🚫 Movement invalidated: terrain too restrictive (impact: ${movementImpact})`);
                    return {
                        valid: false,
                        reason: 'Terrain too restrictive for unit type',
                        terrainImpact: movementImpact
                    };
                }
                
                // Continue with original validation
                const originalResult = originalValidateMovement(from, to, unitType);
                
                // Add terrain information to result
                originalResult.terrainImpact = movementImpact;
                originalResult.terrainClassification = terrainClassification;
                
                return originalResult;
            };
        }
        
        console.log('✅ Movement service integration complete');
    }

    /**
     * Integrate with simulation engine
     */
    integrateWithSimulationEngine() {
        console.log('🔗 Integrating terrain with simulation engine...');
        
        // Override combat resolution to include terrain impact
        if (this.simulationEngine.resolveCombat) {
            const originalResolveCombat = this.simulationEngine.resolveCombat.bind(this.simulationEngine);
            
            this.simulationEngine.resolveCombat = async (engagement) => {
                // Get terrain classification for engagement location
                const terrainClassification = this.terrainEngine.classifyTerrain(engagement.attacker.position);
                
                // Calculate terrain combat impact
                const terrainImpact = this.terrainEngine.calculateTerrainCombatImpact(terrainClassification, engagement);
                
                // Continue with original combat resolution
                const originalResult = await originalResolveCombat(engagement);
                
                // Apply terrain impact to effectiveness
                if (originalResult.effectiveness) {
                    originalResult.effectiveness.terrainImpact = terrainImpact;
                    originalResult.effectiveness.finalEffectiveness *= terrainImpact.relativeAdvantage;
                }
                
                // Add terrain information to result
                originalResult.terrain = terrainClassification;
                originalResult.terrainImpact = terrainImpact;
                
                console.log('⚔️ Combat resolved with terrain impact:', terrainImpact);
                
                return originalResult;
            };
        }
        
        // Override engagement detection to include terrain analysis
        if (this.simulationEngine.findCombatEngagements) {
            const originalFindCombatEngagements = this.simulationEngine.findCombatEngagements.bind(this.simulationEngine);
            
            this.simulationEngine.findCombatEngagements = (combatants) => {
                // Get original engagements
                const engagements = originalFindCombatEngagements(combatants);
                
                // Add terrain analysis to each engagement
                engagements.forEach(engagement => {
                    const terrainClassification = this.terrainEngine.classifyTerrain(engagement.attacker.position);
                    engagement.terrain = terrainClassification;
                    engagement.terrainAdvantage = this.terrainEngine.calculateTerrainCombatImpact(terrainClassification, engagement).terrainAdvantage;
                });
                
                return engagements;
            };
        }
        
        console.log('✅ Simulation engine integration complete');
    }

    /**
     * Analyze terrain impact on attack planning
     * @param {Object} attackPlan - Attack plan data
     * @returns {Object} Terrain impact analysis
     */
    analyzeAttackPlanTerrainImpact(attackPlan) {
        console.log('🎯 Analyzing terrain impact on attack plan...');
        
        if (!this.terrainEngine) {
            console.warn('⚠️ Terrain engine not available');
            return null;
        }
        
        const analysis = {
            attackPlan: attackPlan,
            terrainAnalysis: [],
            overallAssessment: {},
            recommendations: [],
            natoCompliance: true,
            timestamp: new Date().toISOString()
        };
        
        // Analyze each waypoint in the attack plan
        if (attackPlan.waypoints) {
            attackPlan.waypoints.forEach((waypoint, index) => {
                const terrainClassification = this.terrainEngine.classifyTerrain(waypoint.coordinates);
                const terrainImpact = this.terrainEngine.calculateTerrainCombatImpact(terrainClassification, {
                    attacker: attackPlan.attacker,
                    defender: attackPlan.defender
                });
                
                analysis.terrainAnalysis.push({
                    waypoint: index + 1,
                    coordinates: waypoint.coordinates,
                    terrain: terrainClassification,
                    impact: terrainImpact,
                    tacticalValue: terrainClassification.tacticalAssessment.tacticalValue
                });
            });
        }
        
        // Calculate overall assessment
        const terrainAdvantages = analysis.terrainAnalysis.map(t => t.impact.terrainAdvantage);
        const mostCommonAdvantage = this.getMostCommon(terrainAdvantages);
        
        const averageTerrainImpact = analysis.terrainAnalysis.reduce((sum, t) => 
            sum + t.impact.relativeAdvantage, 0) / analysis.terrainAnalysis.length;
        
        analysis.overallAssessment = {
            dominantTerrainAdvantage: mostCommonAdvantage,
            averageTerrainImpact: averageTerrainImpact,
            favorableWaypoints: analysis.terrainAnalysis.filter(t => t.impact.terrainAdvantage === 'attacker').length,
            unfavorableWaypoints: analysis.terrainAnalysis.filter(t => t.impact.terrainAdvantage === 'defender').length,
            neutralWaypoints: analysis.terrainAnalysis.filter(t => t.impact.terrainAdvantage === 'neutral').length
        };
        
        // Generate recommendations
        if (analysis.overallAssessment.averageTerrainImpact > 1.2) {
            analysis.recommendations.push('Terrain favors attack plan execution');
        } else if (analysis.overallAssessment.averageTerrainImpact < 0.8) {
            analysis.recommendations.push('Consider alternative attack routes');
        }
        
        if (analysis.overallAssessment.unfavorableWaypoints > analysis.terrainAnalysis.length / 2) {
            analysis.recommendations.push('Majority of route has unfavorable terrain');
        }
        
        if (analysis.overallAssessment.favorableWaypoints > analysis.terrainAnalysis.length / 2) {
            analysis.recommendations.push('Route has favorable terrain for attack');
        }
        
        console.log('✅ Attack plan terrain analysis complete:', analysis);
        return analysis;
    }

    /**
     * Analyze terrain impact on defense planning
     * @param {Object} defensePlan - Defense plan data
     * @returns {Object} Terrain impact analysis
     */
    analyzeDefensePlanTerrainImpact(defensePlan) {
        console.log('🛡️ Analyzing terrain impact on defense plan...');
        
        if (!this.terrainEngine) {
            console.warn('⚠️ Terrain engine not available');
            return null;
        }
        
        const analysis = {
            defensePlan: defensePlan,
            terrainAnalysis: [],
            overallAssessment: {},
            recommendations: [],
            natoCompliance: true,
            timestamp: new Date().toISOString()
        };
        
        // Analyze each defensive position
        if (defensePlan.positions) {
            defensePlan.positions.forEach((position, index) => {
                const terrainClassification = this.terrainEngine.classifyTerrain(position.coordinates);
                const terrainImpact = this.terrainEngine.calculateTerrainCombatImpact(terrainClassification, {
                    attacker: defensePlan.expectedAttacker,
                    defender: defensePlan.defender
                });
                
                analysis.terrainAnalysis.push({
                    position: index + 1,
                    coordinates: position.coordinates,
                    terrain: terrainClassification,
                    impact: terrainImpact,
                    tacticalValue: terrainClassification.tacticalAssessment.tacticalValue
                });
            });
        }
        
        // Calculate overall assessment
        const terrainAdvantages = analysis.terrainAnalysis.map(t => t.impact.terrainAdvantage);
        const mostCommonAdvantage = this.getMostCommon(terrainAdvantages);
        
        const averageTerrainImpact = analysis.terrainAnalysis.reduce((sum, t) => 
            sum + t.impact.relativeAdvantage, 0) / analysis.terrainAnalysis.length;
        
        analysis.overallAssessment = {
            dominantTerrainAdvantage: mostCommonAdvantage,
            averageTerrainImpact: averageTerrainImpact,
            favorablePositions: analysis.terrainAnalysis.filter(t => t.impact.terrainAdvantage === 'defender').length,
            unfavorablePositions: analysis.terrainAnalysis.filter(t => t.impact.terrainAdvantage === 'attacker').length,
            neutralPositions: analysis.terrainAnalysis.filter(t => t.impact.terrainAdvantage === 'neutral').length
        };
        
        // Generate recommendations
        if (analysis.overallAssessment.averageTerrainImpact > 1.2) {
            analysis.recommendations.push('Terrain favors defense plan execution');
        } else if (analysis.overallAssessment.averageTerrainImpact < 0.8) {
            analysis.recommendations.push('Consider alternative defensive positions');
        }
        
        if (analysis.overallAssessment.favorablePositions > analysis.terrainAnalysis.length / 2) {
            analysis.recommendations.push('Majority of positions have favorable terrain');
        }
        
        if (analysis.overallAssessment.unfavorablePositions > analysis.terrainAnalysis.length / 2) {
            analysis.recommendations.push('Consider repositioning defensive elements');
        }
        
        console.log('✅ Defense plan terrain analysis complete:', analysis);
        return analysis;
    }

    /**
     * Get terrain data for specific coordinates
     * @param {Array} coordinates - [latitude, longitude]
     * @param {Object} terrainData - Optional terrain data from map
     * @returns {Object} Terrain classification
     */
    getTerrainData(coordinates, terrainData = {}) {
        if (!this.terrainEngine) {
            console.warn('⚠️ Terrain engine not available');
            return null;
        }
        
        // Check cache first
        const cacheKey = `${coordinates[0]}_${coordinates[1]}`;
        if (this.terrainCache.has(cacheKey)) {
            return this.terrainCache.get(cacheKey);
        }
        
        // Classify terrain
        const classification = this.terrainEngine.classifyTerrain(coordinates, terrainData);
        
        // Cache result
        this.terrainCache.set(cacheKey, classification);
        
        return classification;
    }

    /**
     * Get terrain impact for movement
     * @param {Array} from - Starting coordinates
     * @param {Array} to - Destination coordinates
     * @param {String} unitType - Unit type
     * @returns {Object} Movement impact analysis
     */
    getMovementTerrainImpact(from, to, unitType) {
        if (!this.terrainEngine) {
            console.warn('⚠️ Terrain engine not available');
            return null;
        }
        
        const terrainClassification = this.terrainEngine.classifyTerrain(to);
        const movementImpact = this.terrainEngine.getMovementImpact(unitType, terrainClassification.terrainType);
        
        return {
            from: from,
            to: to,
            unitType: unitType,
            terrain: terrainClassification,
            movementImpact: movementImpact,
            recommendations: this.generateMovementRecommendations(movementImpact, terrainClassification)
        };
    }

    /**
     * Get terrain impact for combat
     * @param {Array} coordinates - Combat location coordinates
     * @param {Object} attacker - Attacker data
     * @param {Object} defender - Defender data
     * @returns {Object} Combat impact analysis
     */
    getCombatTerrainImpact(coordinates, attacker, defender) {
        if (!this.terrainEngine) {
            console.warn('⚠️ Terrain engine not available');
            return null;
        }
        
        const terrainClassification = this.terrainEngine.classifyTerrain(coordinates);
        const combatImpact = this.terrainEngine.calculateTerrainCombatImpact(terrainClassification, {
            attacker: attacker,
            defender: defender
        });
        
        return combatImpact;
    }

    /**
     * Generate movement recommendations
     */
    generateMovementRecommendations(movementImpact, terrainClassification) {
        const recommendations = [];
        
        if (movementImpact < 0.5) {
            recommendations.push('Terrain severely restricts movement');
            recommendations.push('Consider alternative route or unit type');
        } else if (movementImpact < 0.8) {
            recommendations.push('Terrain moderately restricts movement');
            recommendations.push('Plan for increased movement time');
        } else if (movementImpact > 1.2) {
            recommendations.push('Terrain favors movement');
            recommendations.push('Consider exploiting terrain advantages');
        }
        
        // Add terrain-specific recommendations
        terrainClassification.tacticalAssessment.recommendations.forEach(rec => {
            recommendations.push(rec);
        });
        
        return recommendations;
    }

    /**
     * Clear terrain cache
     */
    clearTerrainCache() {
        this.terrainCache.clear();
        console.log('🗺️ Terrain cache cleared');
    }

    /**
     * Get integration status
     */
    getIntegrationStatus() {
        return {
            status: this.integrationStatus,
            terrainEngine: !!this.terrainEngine,
            movementService: !!this.movementService,
            simulationEngine: !!this.simulationEngine,
            cacheSize: this.terrainCache.size
        };
    }

    /**
     * Utility methods
     */
    getMostCommon(array) {
        const counts = {};
        array.forEach(item => {
            counts[item] = (counts[item] || 0) + 1;
        });
        return Object.keys(counts).reduce((a, b) => counts[a] > counts[b] ? a : b);
    }

    /**
     * Reconnect to services
     */
    reconnect() {
        console.log('🔗 Reconnecting terrain integration service...');
        this.checkRequiredServices();
        this.setupIntegration();
        console.log('✅ Terrain integration service reconnected');
    }
}

// Initialize global instance
window.terrainIntegrationService = new TerrainIntegrationService();
console.log('🔗 Terrain Integration Service initialized');
