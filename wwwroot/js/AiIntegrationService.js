/**
 * AI Integration Service
 * Integrates NATO AI Analysis Engine with existing military systems
 * Provides seamless AI analysis for attack planning, defense planning, and simulation
 */

class AiIntegrationService {
    constructor() {
        this.aiEngine = null;
        this.attackPlanningService = null;
        this.defensePlanningService = null;
        this.simulationService = null;
        this.tokenManagementService = null;
        this.integrationStatus = 'disconnected';
        
        this.initializeIntegration();
    }

    /**
     * Initialize AI integration service
     */
    initializeIntegration() {
        console.log('🔗 Initializing AI Integration Service...');
        
        // Check for required services
        this.checkRequiredServices();
        
        // Initialize integration
        this.setupIntegration();
        
        console.log('✅ AI Integration Service initialized');
    }

    /**
     * Check for required services
     */
    checkRequiredServices() {
        // Check for NATO AI Analysis Engine
        if (typeof NatoAiAnalysisEngine !== 'undefined' && window.natoAiAnalysisEngine) {
            this.aiEngine = window.natoAiAnalysisEngine;
            console.log('✅ NATO AI Analysis Engine connected');
        } else {
            console.warn('⚠️ NATO AI Analysis Engine not available');
        }
        
        // Check for Attack Planning Service
        if (window.attackPlanningService) {
            this.attackPlanningService = window.attackPlanningService;
            console.log('✅ Attack Planning Service connected');
        } else {
            console.warn('⚠️ Attack Planning Service not available');
        }
        
        // Check for Defense Planning Service
        if (window.defensePlanningService) {
            this.defensePlanningService = window.defensePlanningService;
            console.log('✅ Defense Planning Service connected');
        } else {
            console.warn('⚠️ Defense Planning Service not available');
        }
        
        // Check for Simulation Service
        if (window.combatSimulationEngine) {
            this.simulationService = window.combatSimulationEngine;
            console.log('✅ Simulation Service connected');
        } else {
            console.warn('⚠️ Simulation Service not available');
        }
        
        // Check for Token Management Service
        if (window.tokenPlacementManager) {
            this.tokenManagementService = window.tokenPlacementManager;
            console.log('✅ Token Management Service connected');
        } else {
            console.warn('⚠️ Token Management Service not available');
        }
    }

    /**
     * Setup integration between services
     */
    setupIntegration() {
        if (this.aiEngine && this.attackPlanningService) {
            this.integrateWithAttackPlanning();
        }
        
        if (this.aiEngine && this.defensePlanningService) {
            this.integrateWithDefensePlanning();
        }
        
        if (this.aiEngine && this.simulationService) {
            this.integrateWithSimulation();
        }
        
        if (this.aiEngine && this.tokenManagementService) {
            this.integrateWithTokenManagement();
        }
        
        this.integrationStatus = 'connected';
        console.log('🔗 AI integration setup complete');
    }

    /**
     * Integrate with attack planning service
     */
    integrateWithAttackPlanning() {
        console.log('🔗 Integrating AI with attack planning...');
        
        // Override attack planning methods to include AI analysis
        if (this.attackPlanningService.planAttack) {
            const originalPlanAttack = this.attackPlanningService.planAttack.bind(this.attackPlanningService);
            
            this.attackPlanningService.planAttack = async (attackData) => {
                // Perform AI analysis on attack plan
                const aiAnalysis = await this.analyzeAttackPlan(attackData);
                
                // Continue with original attack planning
                const originalResult = await originalPlanAttack(attackData);
                
                // Add AI analysis to result
                originalResult.aiAnalysis = aiAnalysis;
                originalResult.aiRecommendations = aiAnalysis.recommendations;
                originalResult.aiRiskAssessment = aiAnalysis.riskAssessment;
                
                console.log('🎯 Attack plan enhanced with AI analysis:', aiAnalysis);
                
                return originalResult;
            };
        }
        
        console.log('✅ Attack planning integration complete');
    }

    /**
     * Integrate with defense planning service
     */
    integrateWithDefensePlanning() {
        console.log('🔗 Integrating AI with defense planning...');
        
        // Override defense planning methods to include AI analysis
        if (this.defensePlanningService.planDefense) {
            const originalPlanDefense = this.defensePlanningService.planDefense.bind(this.defensePlanningService);
            
            this.defensePlanningService.planDefense = async (defenseData) => {
                // Perform AI analysis on defense plan
                const aiAnalysis = await this.analyzeDefensePlan(defenseData);
                
                // Continue with original defense planning
                const originalResult = await originalPlanDefense(defenseData);
                
                // Add AI analysis to result
                originalResult.aiAnalysis = aiAnalysis;
                originalResult.aiRecommendations = aiAnalysis.recommendations;
                originalResult.aiRiskAssessment = aiAnalysis.riskAssessment;
                
                console.log('🛡️ Defense plan enhanced with AI analysis:', aiAnalysis);
                
                return originalResult;
            };
        }
        
        console.log('✅ Defense planning integration complete');
    }

    /**
     * Integrate with simulation service
     */
    integrateWithSimulation() {
        console.log('🔗 Integrating AI with simulation...');
        
        // Override simulation methods to include AI analysis
        if (this.simulationService.runSimulation) {
            const originalRunSimulation = this.simulationService.runSimulation.bind(this.simulationService);
            
            this.simulationService.runSimulation = async (simulationData) => {
                // Perform AI analysis on simulation
                const aiAnalysis = await this.analyzeSimulation(simulationData);
                
                // Continue with original simulation
                const originalResult = await originalRunSimulation(simulationData);
                
                // Add AI analysis to result
                originalResult.aiAnalysis = aiAnalysis;
                originalResult.aiRecommendations = aiAnalysis.recommendations;
                originalResult.aiRiskAssessment = aiAnalysis.riskAssessment;
                
                console.log('⚔️ Simulation enhanced with AI analysis:', aiAnalysis);
                
                return originalResult;
            };
        }
        
        console.log('✅ Simulation integration complete');
    }

    /**
     * Integrate with token management service
     */
    integrateWithTokenManagement() {
        console.log('🔗 Integrating AI with token management...');
        
        // Override token management methods to include AI analysis
        if (this.tokenManagementService.placeToken) {
            const originalPlaceToken = this.tokenManagementService.placeToken.bind(this.tokenManagementService);
            
            this.tokenManagementService.placeToken = async (tokenData) => {
                // Perform AI analysis on token placement
                const aiAnalysis = await this.analyzeTokenPlacement(tokenData);
                
                // Continue with original token placement
                const originalResult = await originalPlaceToken(tokenData);
                
                // Add AI analysis to result
                originalResult.aiAnalysis = aiAnalysis;
                originalResult.aiRecommendations = aiAnalysis.recommendations;
                
                console.log('🎯 Token placement enhanced with AI analysis:', aiAnalysis);
                
                return originalResult;
            };
        }
        
        console.log('✅ Token management integration complete');
    }

    /**
     * Analyze attack plan using AI
     * @param {Object} attackData - Attack plan data
     * @returns {Object} AI analysis results
     */
    async analyzeAttackPlan(attackData) {
        console.log('🤖 Analyzing attack plan with AI...');
        
        if (!this.aiEngine) {
            console.warn('⚠️ AI engine not available');
            return null;
        }
        
        const analysisRequest = {
            type: 'attack_planning',
            scope: 'tactical',
            timeframe: 'immediate',
            objectives: attackData.objectives || [],
            constraints: attackData.constraints || [],
            resources: {
                forces: attackData.forces || []
            },
            threats: attackData.threats || [],
            opportunities: attackData.opportunities || [],
            terrain: attackData.terrain || {},
            attackPlan: attackData
        };
        
        try {
            const analysis = await this.aiEngine.performAnalysis(analysisRequest);
            console.log('✅ Attack plan AI analysis complete:', analysis);
            return analysis;
        } catch (error) {
            console.error('❌ Error analyzing attack plan:', error);
            return null;
        }
    }

    /**
     * Analyze defense plan using AI
     * @param {Object} defenseData - Defense plan data
     * @returns {Object} AI analysis results
     */
    async analyzeDefensePlan(defenseData) {
        console.log('🤖 Analyzing defense plan with AI...');
        
        if (!this.aiEngine) {
            console.warn('⚠️ AI engine not available');
            return null;
        }
        
        const analysisRequest = {
            type: 'defense_planning',
            scope: 'tactical',
            timeframe: 'immediate',
            objectives: defenseData.objectives || [],
            constraints: defenseData.constraints || [],
            resources: {
                forces: defenseData.forces || []
            },
            threats: defenseData.threats || [],
            opportunities: defenseData.opportunities || [],
            terrain: defenseData.terrain || {},
            defensePlan: defenseData
        };
        
        try {
            const analysis = await this.aiEngine.performAnalysis(analysisRequest);
            console.log('✅ Defense plan AI analysis complete:', analysis);
            return analysis;
        } catch (error) {
            console.error('❌ Error analyzing defense plan:', error);
            return null;
        }
    }

    /**
     * Analyze simulation using AI
     * @param {Object} simulationData - Simulation data
     * @returns {Object} AI analysis results
     */
    async analyzeSimulation(simulationData) {
        console.log('🤖 Analyzing simulation with AI...');
        
        if (!this.aiEngine) {
            console.warn('⚠️ AI engine not available');
            return null;
        }
        
        const analysisRequest = {
            type: 'simulation',
            scope: 'operational',
            timeframe: 'mediumTerm',
            objectives: simulationData.objectives || [],
            constraints: simulationData.constraints || [],
            resources: {
                forces: simulationData.forces || []
            },
            threats: simulationData.threats || [],
            opportunities: simulationData.opportunities || [],
            terrain: simulationData.terrain || {},
            simulationData: simulationData
        };
        
        try {
            const analysis = await this.aiEngine.performAnalysis(analysisRequest);
            console.log('✅ Simulation AI analysis complete:', analysis);
            return analysis;
        } catch (error) {
            console.error('❌ Error analyzing simulation:', error);
            return null;
        }
    }

    /**
     * Analyze token placement using AI
     * @param {Object} tokenData - Token placement data
     * @returns {Object} AI analysis results
     */
    async analyzeTokenPlacement(tokenData) {
        console.log('🤖 Analyzing token placement with AI...');
        
        if (!this.aiEngine) {
            console.warn('⚠️ AI engine not available');
            return null;
        }
        
        const analysisRequest = {
            type: 'token_placement',
            scope: 'tactical',
            timeframe: 'immediate',
            objectives: ['Optimize token placement'],
            constraints: tokenData.constraints || [],
            resources: {
                forces: [tokenData]
            },
            threats: tokenData.threats || [],
            opportunities: tokenData.opportunities || [],
            terrain: tokenData.terrain || {},
            tokenData: tokenData
        };
        
        try {
            const analysis = await this.aiEngine.performAnalysis(analysisRequest);
            console.log('✅ Token placement AI analysis complete:', analysis);
            return analysis;
        } catch (error) {
            console.error('❌ Error analyzing token placement:', error);
            return null;
        }
    }

    /**
     * Get AI recommendations for specific scenario
     * @param {Object} scenario - Scenario data
     * @returns {Object} AI recommendations
     */
    async getAiRecommendations(scenario) {
        console.log('🤖 Getting AI recommendations...');
        
        if (!this.aiEngine) {
            console.warn('⚠️ AI engine not available');
            return null;
        }
        
        const analysisRequest = {
            type: 'recommendations',
            scope: scenario.scope || 'tactical',
            timeframe: scenario.timeframe || 'immediate',
            objectives: scenario.objectives || [],
            constraints: scenario.constraints || [],
            resources: scenario.resources || {},
            threats: scenario.threats || [],
            opportunities: scenario.opportunities || [],
            terrain: scenario.terrain || {},
            scenario: scenario
        };
        
        try {
            const analysis = await this.aiEngine.performAnalysis(analysisRequest);
            const recommendations = analysis.recommendations;
            
            console.log('✅ AI recommendations generated:', recommendations);
            return recommendations;
        } catch (error) {
            console.error('❌ Error getting AI recommendations:', error);
            return null;
        }
    }

    /**
     * Perform real-time AI analysis
     * @param {Object} context - Analysis context
     * @returns {Object} Real-time analysis results
     */
    async performRealTimeAnalysis(context) {
        console.log('🤖 Performing real-time AI analysis...');
        
        if (!this.aiEngine) {
            console.warn('⚠️ AI engine not available');
            return null;
        }
        
        const analysisRequest = {
            type: 'real_time',
            scope: context.scope || 'tactical',
            timeframe: 'immediate',
            objectives: context.objectives || [],
            constraints: context.constraints || [],
            resources: context.resources || {},
            threats: context.threats || [],
            opportunities: context.opportunities || [],
            terrain: context.terrain || {},
            context: context
        };
        
        try {
            const analysis = await this.aiEngine.performAnalysis(analysisRequest);
            console.log('✅ Real-time AI analysis complete:', analysis);
            return analysis;
        } catch (error) {
            console.error('❌ Error performing real-time analysis:', error);
            return null;
        }
    }

    /**
     * Get AI analysis history
     * @returns {Array} Analysis history
     */
    getAnalysisHistory() {
        if (!this.aiEngine) {
            console.warn('⚠️ AI engine not available');
            return [];
        }
        
        return this.aiEngine.getAnalysisHistory();
    }

    /**
     * Get specific AI analysis
     * @param {String} analysisId - Analysis ID
     * @returns {Object} Analysis results
     */
    getAnalysis(analysisId) {
        if (!this.aiEngine) {
            console.warn('⚠️ AI engine not available');
            return null;
        }
        
        return this.aiEngine.getAnalysis(analysisId);
    }

    /**
     * Clear AI analysis history
     */
    clearAnalysisHistory() {
        if (!this.aiEngine) {
            console.warn('⚠️ AI engine not available');
            return;
        }
        
        this.aiEngine.clearAnalysisHistory();
        console.log('🤖 AI analysis history cleared');
    }

    /**
     * Get integration status
     */
    getIntegrationStatus() {
        return {
            status: this.integrationStatus,
            aiEngine: !!this.aiEngine,
            attackPlanningService: !!this.attackPlanningService,
            defensePlanningService: !!this.defensePlanningService,
            simulationService: !!this.simulationService,
            tokenManagementService: !!this.tokenManagementService
        };
    }

    /**
     * Reconnect to services
     */
    reconnect() {
        console.log('🔗 Reconnecting AI integration service...');
        this.checkRequiredServices();
        this.setupIntegration();
        console.log('✅ AI integration service reconnected');
    }

    /**
     * Enable/disable AI analysis
     * @param {Boolean} enabled - Whether to enable AI analysis
     */
    setAiAnalysisEnabled(enabled) {
        this.aiAnalysisEnabled = enabled;
        console.log(`🤖 AI analysis ${enabled ? 'enabled' : 'disabled'}`);
    }

    /**
     * Check if AI analysis is enabled
     * @returns {Boolean} Whether AI analysis is enabled
     */
    isAiAnalysisEnabled() {
        return this.aiAnalysisEnabled !== false;
    }
}

// Initialize global instance
window.aiIntegrationService = new AiIntegrationService();
console.log('🔗 AI Integration Service initialized');
