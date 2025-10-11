/**
 * Simulation Results Manager
 * Manages simulation results storage, analysis, and reporting
 * Provides comprehensive analysis dashboard for combat simulation outcomes
 */

class SimulationResultsManager {
    constructor() {
        this.results = new Map();
        this.analysisCache = new Map();
        this.reportTemplates = new Map();
        this.twoPlayerResults = new Map(); // Store two-player specific results
        this.turnResults = new Map(); // Store turn-by-turn results
        this.playerAnalysis = new Map(); // Store player-specific analysis
        
        this.initializeResultsManager();
    }

    /**
     * Initialize results manager
     */
    initializeResultsManager() {
        console.log('📊 Initializing Simulation Results Manager...');
        
        // Initialize report templates
        this.initializeReportTemplates();
        
        // Load existing results from storage
        this.loadStoredResults();
        
        console.log('✅ Simulation Results Manager initialized');
    }

    /**
     * Initialize report templates
     */
    initializeReportTemplates() {
        this.reportTemplates.set('nato_combat_report', {
            name: 'NATO Combat Report',
            sections: [
                'executive_summary',
                'force_composition',
                'tactical_situation',
                'combat_analysis',
                'casualty_assessment',
                'terrain_effects',
                'recommendations',
                'lessons_learned'
            ],
            format: 'military_standard'
        });

        this.reportTemplates.set('detailed_analysis', {
            name: 'Detailed Combat Analysis',
            sections: [
                'turn_by_turn_analysis',
                'effectiveness_calculations',
                'force_ratio_analysis',
                'terrain_impact_assessment',
                'supply_line_analysis',
                'morale_impact_assessment',
                'tactical_recommendations',
                'strategic_implications'
            ],
            format: 'detailed'
        });

        this.reportTemplates.set('summary_report', {
            name: 'Executive Summary',
            sections: [
                'key_findings',
                'casualty_summary',
                'victory_conditions',
                'tactical_lessons',
                'recommendations'
            ],
            format: 'executive'
        });
    }

    /**
     * Store simulation results
     */
    storeResults(simulationId, results) {
        console.log(`📊 Storing results for simulation ${simulationId}...`);
        
        const storedResults = {
            simulationId: simulationId,
            timestamp: new Date().toISOString(),
            results: results,
            metadata: {
                totalTurns: results.totalTurns,
                totalCasualties: results.summary?.totalCasualties || 0,
                totalCombatActions: results.summary?.totalCombatActions || 0,
                duration: results.endTime ? new Date(results.endTime).getTime() - new Date(results.startTime || results.timestamp).getTime() : 0,
                participants: this.extractParticipants(results),
                terrain: this.extractTerrainData(results),
                defenseElements: this.extractDefenseElements(results)
            },
            analysis: null // Will be generated on demand
        };
        
        this.results.set(simulationId, storedResults);
        
        // Store in browser storage
        this.saveToStorage();
        
        console.log('✅ Results stored successfully');
        return storedResults;
    }

    /**
     * Retrieve simulation results
     */
    getResults(simulationId) {
        return this.results.get(simulationId);
    }

    /**
     * Get all simulation results
     */
    getAllResults() {
        return Array.from(this.results.values());
    }

    /**
     * Generate comprehensive analysis
     */
    generateAnalysis(simulationId, analysisType = 'nato_combat_report') {
        console.log(`📈 Generating ${analysisType} analysis for simulation ${simulationId}...`);
        
        const results = this.getResults(simulationId);
        if (!results) {
            throw new Error(`Simulation results not found: ${simulationId}`);
        }
        
        // Check cache first
        const cacheKey = `${simulationId}_${analysisType}`;
        if (this.analysisCache.has(cacheKey)) {
            return this.analysisCache.get(cacheKey);
        }
        
        const template = this.reportTemplates.get(analysisType);
        if (!template) {
            throw new Error(`Analysis template not found: ${analysisType}`);
        }
        
        const analysis = {
            simulationId: simulationId,
            analysisType: analysisType,
            generatedAt: new Date().toISOString(),
            template: template,
            sections: {}
        };
        
        // Generate each section
        template.sections.forEach(section => {
            analysis.sections[section] = this.generateSection(section, results);
        });
        
        // Store in cache
        this.analysisCache.set(cacheKey, analysis);
        
        console.log('✅ Analysis generated successfully');
        return analysis;
    }

    /**
     * Generate specific analysis section
     */
    generateSection(sectionName, results) {
        switch (sectionName) {
            case 'executive_summary':
                return this.generateExecutiveSummary(results);
            case 'force_composition':
                return this.generateForceComposition(results);
            case 'tactical_situation':
                return this.generateTacticalSituation(results);
            case 'combat_analysis':
                return this.generateCombatAnalysis(results);
            case 'casualty_assessment':
                return this.generateCasualtyAssessment(results);
            case 'terrain_effects':
                return this.generateTerrainEffects(results);
            case 'recommendations':
                return this.generateRecommendations(results);
            case 'lessons_learned':
                return this.generateLessonsLearned(results);
            case 'turn_by_turn_analysis':
                return this.generateTurnByTurnAnalysis(results);
            case 'effectiveness_calculations':
                return this.generateEffectivenessCalculations(results);
            case 'force_ratio_analysis':
                return this.generateForceRatioAnalysis(results);
            case 'terrain_impact_assessment':
                return this.generateTerrainImpactAssessment(results);
            case 'supply_line_analysis':
                return this.generateSupplyLineAnalysis(results);
            case 'morale_impact_assessment':
                return this.generateMoraleImpactAssessment(results);
            case 'tactical_recommendations':
                return this.generateTacticalRecommendations(results);
            case 'strategic_implications':
                return this.generateStrategicImplications(results);
            case 'key_findings':
                return this.generateKeyFindings(results);
            case 'casualty_summary':
                return this.generateCasualtySummary(results);
            case 'victory_conditions':
                return this.generateVictoryConditions(results);
            case 'tactical_lessons':
                return this.generateTacticalLessons(results);
            default:
                return { error: `Unknown section: ${sectionName}` };
        }
    }

    /**
     * Generate executive summary
     */
    generateExecutiveSummary(results) {
        const summary = results.summary || {};
        const victory = results.victoryConditions || {};
        
        return {
            title: 'Executive Summary',
            content: {
                simulationOverview: {
                    totalTurns: summary.totalTurns || 0,
                    duration: summary.averageTurnDuration || 0,
                    totalCombatActions: summary.totalCombatActions || 0
                },
                outcome: {
                    winner: victory.winner || 'undetermined',
                    condition: victory.condition || 'ongoing',
                    finalSituation: summary.finalSituation || {}
                },
                keyMetrics: {
                    totalCasualties: summary.totalCasualties || 0,
                    averageCasualtiesPerTurn: summary.totalCasualties / (summary.totalTurns || 1),
                    combatIntensity: summary.totalCombatActions / (summary.totalTurns || 1)
                },
                tacticalAssessment: this.assessTacticalOutcome(results)
            }
        };
    }

    /**
     * Generate force composition analysis
     */
    generateForceComposition(results) {
        const participants = this.extractParticipants(results);
        const attackers = participants.filter(p => p.type === 'attacker');
        const defenders = participants.filter(p => p.type === 'defender');
        
        return {
            title: 'Force Composition',
            content: {
                totalForces: {
                    attackers: attackers.length,
                    defenders: defenders.length,
                    total: participants.length
                },
                unitTypes: this.analyzeUnitTypes(participants),
                organizationLevels: this.analyzeOrganizationLevels(participants),
                initialStrength: {
                    attackers: attackers.reduce((sum, p) => sum + (p.strength || 0), 0),
                    defenders: defenders.reduce((sum, p) => sum + (p.strength || 0), 0)
                },
                finalStrength: this.calculateFinalStrength(results)
            }
        };
    }

    /**
     * Generate tactical situation analysis
     */
    generateTacticalSituation(results) {
        const finalSituation = results.summary?.finalSituation || {};
        
        return {
            title: 'Tactical Situation',
            content: {
                finalSituation: finalSituation,
                strengthRatio: finalSituation.strengthRatio || 1.0,
                tacticalAdvantage: this.determineTacticalAdvantage(finalSituation),
                keyTerrain: this.analyzeKeyTerrain(results),
                defensivePositions: this.analyzeDefensivePositions(results),
                offensiveCapabilities: this.analyzeOffensiveCapabilities(results)
            }
        };
    }

    /**
     * Generate combat analysis
     */
    generateCombatAnalysis(results) {
        const combatActions = this.extractCombatActions(results);
        
        return {
            title: 'Combat Analysis',
            content: {
                totalEngagements: combatActions.length,
                engagementTypes: this.analyzeEngagementTypes(combatActions),
                effectivenessAnalysis: this.analyzeCombatEffectiveness(combatActions),
                rangeAnalysis: this.analyzeCombatRanges(combatActions),
                terrainImpact: this.analyzeTerrainImpactOnCombat(combatActions),
                tacticalTrends: this.analyzeTacticalTrends(combatActions)
            }
        };
    }

    /**
     * Generate casualty assessment
     */
    generateCasualtyAssessment(results) {
        const casualties = this.extractCasualties(results);
        
        return {
            title: 'Casualty Assessment',
            content: {
                totalCasualties: casualties.total,
                casualtiesByTurn: casualties.byTurn,
                casualtiesByUnit: casualties.byUnit,
                casualtyRates: this.calculateCasualtyRates(casualties),
                lossAnalysis: this.analyzeLossPatterns(casualties),
                medicalAssessment: this.generateMedicalAssessment(casualties)
            }
        };
    }

    /**
     * Generate terrain effects analysis
     */
    generateTerrainEffects(results) {
        const terrainData = this.extractTerrainData(results);
        
        return {
            title: 'Terrain Effects',
            content: {
                terrainTypes: terrainData.types,
                terrainImpact: terrainData.impact,
                movementAnalysis: terrainData.movement,
                coverAnalysis: terrainData.cover,
                concealmentAnalysis: terrainData.concealment,
                tacticalImplications: terrainData.implications
            }
        };
    }

    /**
     * Generate recommendations
     */
    generateRecommendations(results) {
        const analysis = this.analyzeSimulationResults(results);
        
        return {
            title: 'Tactical Recommendations',
            content: {
                immediateActions: analysis.immediateActions,
                tacticalAdjustments: analysis.tacticalAdjustments,
                forceComposition: analysis.forceComposition,
                terrainUtilization: analysis.terrainUtilization,
                supplyConsiderations: analysis.supplyConsiderations,
                moraleManagement: analysis.moraleManagement
            }
        };
    }

    /**
     * Generate lessons learned
     */
    generateLessonsLearned(results) {
        const lessons = this.extractLessonsLearned(results);
        
        return {
            title: 'Lessons Learned',
            content: {
                tacticalLessons: lessons.tactical,
                operationalLessons: lessons.operational,
                technicalLessons: lessons.technical,
                leadershipLessons: lessons.leadership,
                trainingImplications: lessons.training,
                doctrineImplications: lessons.doctrine
            }
        };
    }

    /**
     * Generate turn-by-turn analysis
     */
    generateTurnByTurnAnalysis(results) {
        const turns = results.finalResults || [];
        
        return {
            title: 'Turn-by-Turn Analysis',
            content: {
                turnSummaries: turns.map(turn => ({
                    turn: turn.turn,
                    summary: turn.turnSummary,
                    casualties: turn.casualties,
                    combatActions: turn.combatActions?.length || 0,
                    keyEvents: turn.turnSummary?.majorEvents || []
                })),
                trends: this.analyzeTurnTrends(turns),
                criticalTurns: this.identifyCriticalTurns(turns),
                turningPoints: this.identifyTurningPoints(turns)
            }
        };
    }

    /**
     * Generate effectiveness calculations
     */
    generateEffectivenessCalculations(results) {
        const combatActions = this.extractCombatActions(results);
        
        return {
            title: 'Effectiveness Calculations',
            content: {
                natoCompliance: this.verifyNatoCompliance(combatActions),
                forceRatioAnalysis: this.analyzeForceRatios(combatActions),
                terrainCoefficients: this.analyzeTerrainCoefficients(combatActions),
                unitEffectiveness: this.analyzeUnitEffectiveness(combatActions),
                moraleImpact: this.analyzeMoraleImpact(combatActions),
                supplyImpact: this.analyzeSupplyImpact(combatActions)
            }
        };
    }

    /**
     * Generate force ratio analysis
     */
    generateForceRatioAnalysis(results) {
        const combatActions = this.extractCombatActions(results);
        
        return {
            title: 'Force Ratio Analysis',
            content: {
                natoStandards: this.getNatoForceRatioStandards(),
                actualRatios: this.calculateActualRatios(combatActions),
                adequacyAssessment: this.assessForceRatioAdequacy(combatActions),
                recommendations: this.generateForceRatioRecommendations(combatActions)
            }
        };
    }

    /**
     * Generate terrain impact assessment
     */
    generateTerrainImpactAssessment(results) {
        const terrainData = this.extractTerrainData(results);
        
        return {
            title: 'Terrain Impact Assessment',
            content: {
                terrainTypes: terrainData.types,
                impactCoefficients: terrainData.coefficients,
                tacticalAdvantages: terrainData.advantages,
                tacticalDisadvantages: terrainData.disadvantages,
                recommendations: terrainData.recommendations
            }
        };
    }

    /**
     * Generate supply line analysis
     */
    generateSupplyLineAnalysis(results) {
        const supplyData = this.extractSupplyData(results);
        
        return {
            title: 'Supply Line Analysis',
            content: {
                supplyStatus: supplyData.status,
                supplyEffects: supplyData.effects,
                supplyVulnerabilities: supplyData.vulnerabilities,
                supplyRecommendations: supplyData.recommendations
            }
        };
    }

    /**
     * Generate morale impact assessment
     */
    generateMoraleImpactAssessment(results) {
        const moraleData = this.extractMoraleData(results);
        
        return {
            title: 'Morale Impact Assessment',
            content: {
                moraleTrends: moraleData.trends,
                moraleEffects: moraleData.effects,
                moraleFactors: moraleData.factors,
                moraleRecommendations: moraleData.recommendations
            }
        };
    }

    /**
     * Generate tactical recommendations
     */
    generateTacticalRecommendations(results) {
        const analysis = this.analyzeSimulationResults(results);
        
        return {
            title: 'Tactical Recommendations',
            content: {
                immediateActions: analysis.immediateActions,
                tacticalAdjustments: analysis.tacticalAdjustments,
                forceComposition: analysis.forceComposition,
                terrainUtilization: analysis.terrainUtilization,
                supplyConsiderations: analysis.supplyConsiderations,
                moraleManagement: analysis.moraleManagement
            }
        };
    }

    /**
     * Generate strategic implications
     */
    generateStrategicImplications(results) {
        const analysis = this.analyzeSimulationResults(results);
        
        return {
            title: 'Strategic Implications',
            content: {
                strategicImpact: analysis.strategicImpact,
                operationalImplications: analysis.operationalImplications,
                tacticalImplications: analysis.tacticalImplications,
                resourceImplications: analysis.resourceImplications,
                timeImplications: analysis.timeImplications
            }
        };
    }

    /**
     * Generate key findings
     */
    generateKeyFindings(results) {
        const analysis = this.analyzeSimulationResults(results);
        
        return {
            title: 'Key Findings',
            content: {
                primaryFindings: analysis.primaryFindings,
                secondaryFindings: analysis.secondaryFindings,
                unexpectedFindings: analysis.unexpectedFindings,
                criticalFindings: analysis.criticalFindings
            }
        };
    }

    /**
     * Generate casualty summary
     */
    generateCasualtySummary(results) {
        const casualties = this.extractCasualties(results);
        
        return {
            title: 'Casualty Summary',
            content: {
                totalCasualties: casualties.total,
                casualtiesBySide: casualties.bySide,
                casualtyRates: casualties.rates,
                medicalImplications: casualties.medical
            }
        };
    }

    /**
     * Generate victory conditions analysis
     */
    generateVictoryConditions(results) {
        const victory = results.victoryConditions || {};
        
        return {
            title: 'Victory Conditions',
            content: {
                outcome: victory,
                conditions: this.analyzeVictoryConditions(results),
                factors: this.analyzeVictoryFactors(results),
                implications: this.analyzeVictoryImplications(results)
            }
        };
    }

    /**
     * Generate tactical lessons
     */
    generateTacticalLessons(results) {
        const lessons = this.extractLessonsLearned(results);
        
        return {
            title: 'Tactical Lessons',
            content: {
                lessons: lessons.tactical,
                applications: lessons.applications,
                training: lessons.training,
                doctrine: lessons.doctrine
            }
        };
    }

    /**
     * Export results to various formats
     */
    exportResults(simulationId, format = 'json') {
        const results = this.getResults(simulationId);
        if (!results) {
            throw new Error(`Simulation results not found: ${simulationId}`);
        }
        
        switch (format.toLowerCase()) {
            case 'json':
                return JSON.stringify(results, null, 2);
            case 'csv':
                return this.exportToCSV(results);
            case 'xml':
                return this.exportToXML(results);
            case 'pdf':
                return this.exportToPDF(results);
            default:
                throw new Error(`Unsupported export format: ${format}`);
        }
    }

    /**
     * Export to CSV format
     */
    exportToCSV(results) {
        const csvData = [];
        
        // Add metadata
        csvData.push(['Simulation ID', results.simulationId]);
        csvData.push(['Timestamp', results.timestamp]);
        csvData.push(['Total Turns', results.metadata.totalTurns]);
        csvData.push(['Total Casualties', results.metadata.totalCasualties]);
        csvData.push(['Total Combat Actions', results.metadata.totalCombatActions]);
        csvData.push([]);
        
        // Add turn data
        csvData.push(['Turn', 'Casualties', 'Combat Actions', 'Duration']);
        if (results.results.finalResults) {
            results.results.finalResults.forEach(turn => {
                csvData.push([
                    turn.turn,
                    turn.casualties?.total || 0,
                    turn.combatActions?.length || 0,
                    turn.duration || 0
                ]);
            });
        }
        
        return csvData.map(row => row.join(',')).join('\n');
    }

    /**
     * Export to XML format
     */
    exportToXML(results) {
        let xml = '<?xml version="1.0" encoding="UTF-8"?>\n';
        xml += '<simulation_results>\n';
        xml += `  <simulation_id>${results.simulationId}</simulation_id>\n`;
        xml += `  <timestamp>${results.timestamp}</timestamp>\n`;
        xml += `  <total_turns>${results.metadata.totalTurns}</total_turns>\n`;
        xml += `  <total_casualties>${results.metadata.totalCasualties}</total_casualties>\n`;
        xml += `  <total_combat_actions>${results.metadata.totalCombatActions}</total_combat_actions>\n`;
        xml += '</simulation_results>';
        
        return xml;
    }

    /**
     * Export to PDF format (placeholder)
     */
    exportToPDF(results) {
        // This would integrate with a PDF generation library
        return {
            message: 'PDF export not yet implemented',
            data: results
        };
    }

    /**
     * Utility methods for data extraction and analysis
     */
    extractParticipants(results) {
        // Extract participant data from results
        return results.metadata?.participants || [];
    }

    extractTerrainData(results) {
        // Extract terrain data from results
        return results.metadata?.terrain || {};
    }

    extractDefenseElements(results) {
        // Extract defense elements from results
        return results.metadata?.defenseElements || [];
    }

    extractCombatActions(results) {
        // Extract all combat actions from all turns
        const combatActions = [];
        if (results.results.finalResults) {
            results.results.finalResults.forEach(turn => {
                if (turn.combatActions) {
                    combatActions.push(...turn.combatActions);
                }
            });
        }
        return combatActions;
    }

    extractCasualties(results) {
        // Extract casualty data from all turns
        const casualties = {
            total: 0,
            byTurn: [],
            byUnit: {},
            bySide: { attacker: 0, defender: 0 }
        };
        
        if (results.results.finalResults) {
            results.results.finalResults.forEach(turn => {
                if (turn.casualties) {
                    casualties.total += turn.casualties.total || 0;
                    casualties.byTurn.push({
                        turn: turn.turn,
                        casualties: turn.casualties.total || 0
                    });
                }
            });
        }
        
        return casualties;
    }

    extractSupplyData(results) {
        // Extract supply data from results
        return {
            status: 'adequate',
            effects: {},
            vulnerabilities: [],
            recommendations: []
        };
    }

    extractMoraleData(results) {
        // Extract morale data from results
        return {
            trends: [],
            effects: {},
            factors: [],
            recommendations: []
        };
    }

    extractLessonsLearned(results) {
        // Extract lessons learned from results
        return {
            tactical: [],
            operational: [],
            technical: [],
            leadership: [],
            training: [],
            doctrine: []
        };
    }

    /**
     * Analysis methods
     */
    analyzeSimulationResults(results) {
        // Comprehensive analysis of simulation results
        return {
            immediateActions: [],
            tacticalAdjustments: [],
            forceComposition: [],
            terrainUtilization: [],
            supplyConsiderations: [],
            moraleManagement: [],
            strategicImpact: {},
            operationalImplications: {},
            tacticalImplications: {},
            resourceImplications: {},
            timeImplications: {},
            primaryFindings: [],
            secondaryFindings: [],
            unexpectedFindings: [],
            criticalFindings: []
        };
    }

    assessTacticalOutcome(results) {
        // Assess tactical outcome of simulation
        return {
            outcome: 'undetermined',
            confidence: 0.5,
            factors: [],
            implications: []
        };
    }

    analyzeUnitTypes(participants) {
        // Analyze unit types in simulation
        const unitTypes = {};
        participants.forEach(participant => {
            const unitType = participant.unit?.unitType || 'Unknown';
            unitTypes[unitType] = (unitTypes[unitType] || 0) + 1;
        });
        return unitTypes;
    }

    analyzeOrganizationLevels(participants) {
        // Analyze organization levels in simulation
        const orgLevels = {};
        participants.forEach(participant => {
            const orgLevel = participant.unit?.organizationLevel || 'Unknown';
            orgLevels[orgLevel] = (orgLevels[orgLevel] || 0) + 1;
        });
        return orgLevels;
    }

    calculateFinalStrength(results) {
        // Calculate final strength of forces
        return {
            attackers: 0,
            defenders: 0,
            total: 0
        };
    }

    determineTacticalAdvantage(situation) {
        // Determine tactical advantage from situation
        if (situation.strengthRatio >= 2.0) {
            return 'attacker_advantage';
        } else if (situation.strengthRatio <= 0.5) {
            return 'defender_advantage';
        } else {
            return 'balanced';
        }
    }

    analyzeKeyTerrain(results) {
        // Analyze key terrain features
        return {
            features: [],
            advantages: [],
            disadvantages: []
        };
    }

    analyzeDefensivePositions(results) {
        // Analyze defensive positions
        return {
            positions: [],
            effectiveness: [],
            vulnerabilities: []
        };
    }

    analyzeOffensiveCapabilities(results) {
        // Analyze offensive capabilities
        return {
            capabilities: [],
            limitations: [],
            opportunities: []
        };
    }

    analyzeEngagementTypes(combatActions) {
        // Analyze types of combat engagements
        const engagementTypes = {};
        combatActions.forEach(action => {
            const engagement = action.engagement;
            if (engagement) {
                const type = engagement.attacker.unit?.unitType || 'Unknown';
                engagementTypes[type] = (engagementTypes[type] || 0) + 1;
            }
        });
        return engagementTypes;
    }

    analyzeCombatEffectiveness(combatActions) {
        // Analyze combat effectiveness
        return {
            averageEffectiveness: 0,
            effectivenessTrends: [],
            factors: []
        };
    }

    analyzeCombatRanges(combatActions) {
        // Analyze combat ranges
        return {
            averageRange: 0,
            rangeDistribution: {},
            rangeEffectiveness: {}
        };
    }

    analyzeTerrainImpactOnCombat(combatActions) {
        // Analyze terrain impact on combat
        return {
            terrainTypes: {},
            impactFactors: {},
            recommendations: []
        };
    }

    analyzeTacticalTrends(combatActions) {
        // Analyze tactical trends
        return {
            trends: [],
            patterns: [],
            implications: []
        };
    }

    calculateCasualtyRates(casualties) {
        // Calculate casualty rates
        return {
            averagePerTurn: 0,
            peakRate: 0,
            trend: 'stable'
        };
    }

    analyzeLossPatterns(casualties) {
        // Analyze loss patterns
        return {
            patterns: [],
            factors: [],
            implications: []
        };
    }

    generateMedicalAssessment(casualties) {
        // Generate medical assessment
        return {
            medicalRequirements: [],
            evacuationNeeds: [],
            treatmentCapabilities: []
        };
    }

    analyzeTurnTrends(turns) {
        // Analyze trends across turns
        return {
            casualtyTrends: [],
            combatTrends: [],
            tacticalTrends: []
        };
    }

    identifyCriticalTurns(turns) {
        // Identify critical turns
        return turns.filter(turn => {
            const casualties = turn.casualties?.total || 0;
            const combatActions = turn.combatActions?.length || 0;
            return casualties > 50 || combatActions > 3;
        });
    }

    identifyTurningPoints(turns) {
        // Identify turning points in simulation
        return [];
    }

    verifyNatoCompliance(combatActions) {
        // Verify NATO compliance
        return {
            compliant: true,
            violations: [],
            recommendations: []
        };
    }

    analyzeForceRatios(combatActions) {
        // Analyze force ratios
        return {
            ratios: [],
            adequacy: [],
            recommendations: []
        };
    }

    analyzeTerrainCoefficients(combatActions) {
        // Analyze terrain coefficients
        return {
            coefficients: [],
            effectiveness: [],
            recommendations: []
        };
    }

    analyzeUnitEffectiveness(combatActions) {
        // Analyze unit effectiveness
        return {
            effectiveness: [],
            factors: [],
            recommendations: []
        };
    }

    analyzeMoraleImpact(combatActions) {
        // Analyze morale impact
        return {
            impact: [],
            factors: [],
            recommendations: []
        };
    }

    analyzeSupplyImpact(combatActions) {
        // Analyze supply impact
        return {
            impact: [],
            factors: [],
            recommendations: []
        };
    }

    getNatoForceRatioStandards() {
        // Get NATO force ratio standards
        return {
            offensive: {
                frontal: 3.0,
                flanking: 2.0,
                envelopment: 1.5,
                penetration: 2.5,
                raid: 1.0,
                ambush: 0.5
            },
            defensive: {
                prepared: 0.33,
                hasty: 0.5,
                mobile: 0.67
            }
        };
    }

    calculateActualRatios(combatActions) {
        // Calculate actual force ratios
        return [];
    }

    assessForceRatioAdequacy(combatActions) {
        // Assess force ratio adequacy
        return {
            adequate: [],
            inadequate: [],
            recommendations: []
        };
    }

    generateForceRatioRecommendations(combatActions) {
        // Generate force ratio recommendations
        return [];
    }

    analyzeVictoryConditions(results) {
        // Analyze victory conditions
        return {
            conditions: [],
            factors: [],
            implications: []
        };
    }

    analyzeVictoryFactors(results) {
        // Analyze victory factors
        return {
            factors: [],
            importance: [],
            recommendations: []
        };
    }

    analyzeVictoryImplications(results) {
        // Analyze victory implications
        return {
            implications: [],
            consequences: [],
            recommendations: []
        };
    }

    /**
     * Storage methods
     */
    saveToStorage() {
        try {
            const data = Array.from(this.results.entries());
            localStorage.setItem('simulation_results', JSON.stringify(data));
        } catch (error) {
            console.warn('Failed to save results to storage:', error);
        }
    }

    loadStoredResults() {
        try {
            const stored = localStorage.getItem('simulation_results');
            if (stored) {
                const data = JSON.parse(stored);
                this.results = new Map(data);
                console.log(`📊 Loaded ${this.results.size} stored results`);
            }
        } catch (error) {
            console.warn('Failed to load results from storage:', error);
        }
    }

    clearStoredResults() {
        try {
            localStorage.removeItem('simulation_results');
            this.results.clear();
            this.analysisCache.clear();
            console.log('📊 Cleared all stored results');
        } catch (error) {
            console.warn('Failed to clear stored results:', error);
        }
    }

    /**
     * Two-player simulation methods
     */

    /**
     * Store two-player turn result
     * @param {String} simulationId - Simulation ID
     * @param {Number} turn - Turn number
     * @param {Object} turnData - Turn data
     */
    storeTwoPlayerTurnResult(simulationId, turn, turnData) {
        console.log(`📊 Storing two-player turn result: ${simulationId}, turn ${turn}`);
        
        if (!this.turnResults.has(simulationId)) {
            this.turnResults.set(simulationId, new Map());
        }
        
        const simTurns = this.turnResults.get(simulationId);
        simTurns.set(turn, {
            turn: turn,
            timestamp: new Date().toISOString(),
            phase: turnData.phase || 'completed',
            player1Actions: turnData.player1Actions || [],
            player2Actions: turnData.player2Actions || [],
            combatResults: turnData.combatResults || [],
            casualties: turnData.casualties || {},
            terrainEffects: turnData.terrainEffects || {},
            aiAnalysis: turnData.aiAnalysis || null,
            visibility: turnData.visibility || {}
        });
        
        // Update two-player results
        this.updateTwoPlayerResults(simulationId);
        
        console.log('✅ Two-player turn result stored');
    }

    /**
     * Update two-player results summary
     */
    updateTwoPlayerResults(simulationId) {
        const turns = this.turnResults.get(simulationId);
        if (!turns) return;
        
        const twoPlayerResults = {
            simulationId: simulationId,
            totalTurns: turns.size,
            turns: Array.from(turns.values()),
            player1Summary: this.generatePlayerSummary(simulationId, 'player1'),
            player2Summary: this.generatePlayerSummary(simulationId, 'player2'),
            overallSummary: this.generateOverallSummary(simulationId),
            timestamp: new Date().toISOString()
        };
        
        this.twoPlayerResults.set(simulationId, twoPlayerResults);
    }

    /**
     * Generate player-specific summary
     */
    generatePlayerSummary(simulationId, playerId) {
        const turns = this.turnResults.get(simulationId);
        if (!turns) return null;
        
        const summary = {
            playerId: playerId,
            totalActions: 0,
            combatActions: 0,
            movementActions: 0,
            casualties: 0,
            enemyCasualtiesInflicted: 0,
            unitsLost: 0,
            unitsRemaining: 0,
            tacticalVictories: 0,
            tacticalDefeats: 0,
            performance: {},
            aiRecommendationsFollowed: 0
        };
        
        turns.forEach(turn => {
            const playerActions = playerId === 'player1' ? turn.player1Actions : turn.player2Actions;
            summary.totalActions += playerActions.length;
            
            // Count action types
            playerActions.forEach(action => {
                if (action.type === 'combat') summary.combatActions++;
                if (action.type === 'movement') summary.movementActions++;
            });
            
            // Count casualties
            if (turn.casualties && turn.casualties[playerId]) {
                summary.casualties += turn.casualties[playerId];
            }
        });
        
        // Calculate performance metrics
        summary.performance = {
            aggressiveness: summary.combatActions / (summary.totalActions || 1),
            mobility: summary.movementActions / (summary.totalActions || 1),
            effectiveness: (summary.enemyCasualtiesInflicted / (summary.casualties || 1)) || 0,
            survivalRate: (summary.unitsRemaining / (summary.unitsRemaining + summary.unitsLost || 1)) || 1
        };
        
        return summary;
    }

    /**
     * Generate overall summary for two-player simulation
     */
    generateOverallSummary(simulationId) {
        const turns = this.turnResults.get(simulationId);
        if (!turns) return null;
        
        const summary = {
            totalTurns: turns.size,
            totalCombatActions: 0,
            totalCasualties: 0,
            player1Casualties: 0,
            player2Casualties: 0,
            tacticalTrends: [],
            criticalMoments: [],
            turningPoints: []
        };
        
        turns.forEach(turn => {
            summary.totalCombatActions += turn.combatResults.length;
            
            if (turn.casualties) {
                summary.player1Casualties += turn.casualties.player1 || 0;
                summary.player2Casualties += turn.casualties.player2 || 0;
                summary.totalCasualties += (turn.casualties.player1 || 0) + (turn.casualties.player2 || 0);
            }
        });
        
        // Identify critical moments (high casualty turns)
        turns.forEach(turn => {
            const turnCasualties = (turn.casualties.player1 || 0) + (turn.casualties.player2 || 0);
            if (turnCasualties > summary.totalCasualties / turns.size * 2) {
                summary.criticalMoments.push({
                    turn: turn.turn,
                    casualties: turnCasualties,
                    significance: 'high_casualty_turn'
                });
            }
        });
        
        return summary;
    }

    /**
     * Get player-specific view of results
     */
    getPlayerResults(simulationId, playerId) {
        const twoPlayerResults = this.twoPlayerResults.get(simulationId);
        if (!twoPlayerResults) return null;
        
        const playerSummary = playerId === 'player1' ? 
            twoPlayerResults.player1Summary : 
            twoPlayerResults.player2Summary;
        
        return {
            simulationId: simulationId,
            playerId: playerId,
            summary: playerSummary,
            turns: twoPlayerResults.turns.map(turn => ({
                turn: turn.turn,
                timestamp: turn.timestamp,
                phase: turn.phase,
                myActions: playerId === 'player1' ? turn.player1Actions : turn.player2Actions,
                casualties: turn.casualties[playerId] || 0,
                aiAnalysis: turn.aiAnalysis
            })),
            overallRanking: this.calculatePlayerRanking(simulationId, playerId),
            timestamp: new Date().toISOString()
        };
    }

    /**
     * Calculate player ranking/score
     */
    calculatePlayerRanking(simulationId, playerId) {
        const summary = this.generatePlayerSummary(simulationId, playerId);
        if (!summary) return null;
        
        const score = {
            tacticalScore: 0,
            performanceScore: 0,
            efficiencyScore: 0,
            overallScore: 0,
            rank: 'Pending'
        };
        
        // Calculate tactical score (0-100)
        score.tacticalScore = Math.min(100, 
            (summary.tacticalVictories * 10) + 
            (summary.enemyCasualtiesInflicted / 10)
        );
        
        // Calculate performance score (0-100)
        score.performanceScore = Math.min(100,
            (summary.performance.effectiveness * 30) +
            (summary.performance.survivalRate * 40) +
            (summary.performance.aggressiveness * 15) +
            (summary.performance.mobility * 15)
        );
        
        // Calculate efficiency score (0-100)
        score.efficiencyScore = Math.min(100,
            ((summary.totalActions / summary.totalTurns) * 10) +
            (summary.aiRecommendationsFollowed * 5)
        );
        
        // Calculate overall score
        score.overallScore = (score.tacticalScore + score.performanceScore + score.efficiencyScore) / 3;
        
        // Determine rank
        if (score.overallScore >= 90) score.rank = 'Exceptional';
        else if (score.overallScore >= 75) score.rank = 'Excellent';
        else if (score.overallScore >= 60) score.rank = 'Good';
        else if (score.overallScore >= 45) score.rank = 'Average';
        else score.rank = 'Needs Improvement';
        
        return score;
    }

    /**
     * Generate comparative analysis for two players
     */
    generateComparativeAnalysis(simulationId) {
        console.log(`📊 Generating comparative analysis for ${simulationId}`);
        
        const twoPlayerResults = this.twoPlayerResults.get(simulationId);
        if (!twoPlayerResults) {
            throw new Error('Two-player results not found');
        }
        
        const player1Summary = twoPlayerResults.player1Summary;
        const player2Summary = twoPlayerResults.player2Summary;
        
        const comparison = {
            simulationId: simulationId,
            timestamp: new Date().toISOString(),
            player1: {
                summary: player1Summary,
                ranking: this.calculatePlayerRanking(simulationId, 'player1'),
                strengths: this.identifyPlayerStrengths(player1Summary),
                weaknesses: this.identifyPlayerWeaknesses(player1Summary)
            },
            player2: {
                summary: player2Summary,
                ranking: this.calculatePlayerRanking(simulationId, 'player2'),
                strengths: this.identifyPlayerStrengths(player2Summary),
                weaknesses: this.identifyPlayerWeaknesses(player2Summary)
            },
            comparison: {
                casualtyRatio: (player2Summary.casualties / (player1Summary.casualties || 1)),
                actionRatio: (player1Summary.totalActions / (player2Summary.totalActions || 1)),
                effectivenessComparison: this.compareEffectiveness(player1Summary, player2Summary),
                tacticalComparison: this.compareTacticalApproach(player1Summary, player2Summary)
            },
            winner: this.determineWinner(player1Summary, player2Summary),
            recommendations: {
                player1: this.generatePlayerRecommendations(player1Summary),
                player2: this.generatePlayerRecommendations(player2Summary)
            }
        };
        
        console.log('✅ Comparative analysis generated');
        return comparison;
    }

    /**
     * Identify player strengths
     */
    identifyPlayerStrengths(playerSummary) {
        const strengths = [];
        
        if (playerSummary.performance.effectiveness > 1.5) {
            strengths.push('Highly effective combat operations');
        }
        if (playerSummary.performance.survivalRate > 0.8) {
            strengths.push('Excellent force preservation');
        }
        if (playerSummary.performance.aggressiveness > 0.6) {
            strengths.push('Aggressive tactical approach');
        }
        if (playerSummary.performance.mobility > 0.5) {
            strengths.push('Good mobility and maneuver');
        }
        if (playerSummary.tacticalVictories > playerSummary.tacticalDefeats * 2) {
            strengths.push('Consistent tactical success');
        }
        
        return strengths;
    }

    /**
     * Identify player weaknesses
     */
    identifyPlayerWeaknesses(playerSummary) {
        const weaknesses = [];
        
        if (playerSummary.performance.effectiveness < 0.7) {
            weaknesses.push('Low combat effectiveness');
        }
        if (playerSummary.performance.survivalRate < 0.5) {
            weaknesses.push('Poor force preservation');
        }
        if (playerSummary.performance.aggressiveness < 0.3) {
            weaknesses.push('Overly passive approach');
        }
        if (playerSummary.performance.mobility < 0.3) {
            weaknesses.push('Limited mobility and maneuver');
        }
        if (playerSummary.tacticalDefeats > playerSummary.tacticalVictories) {
            weaknesses.push('More defeats than victories');
        }
        
        return weaknesses;
    }

    /**
     * Compare effectiveness between players
     */
    compareEffectiveness(player1Summary, player2Summary) {
        const p1Effectiveness = player1Summary.performance.effectiveness;
        const p2Effectiveness = player2Summary.performance.effectiveness;
        
        if (p1Effectiveness > p2Effectiveness * 1.2) {
            return 'Player 1 significantly more effective';
        } else if (p2Effectiveness > p1Effectiveness * 1.2) {
            return 'Player 2 significantly more effective';
        } else {
            return 'Both players equally effective';
        }
    }

    /**
     * Compare tactical approaches
     */
    compareTacticalApproach(player1Summary, player2Summary) {
        const p1Aggressiveness = player1Summary.performance.aggressiveness;
        const p2Aggressiveness = player2Summary.performance.aggressiveness;
        
        let comparison = '';
        
        if (p1Aggressiveness > 0.6) {
            comparison += 'Player 1: Aggressive. ';
        } else if (p1Aggressiveness > 0.4) {
            comparison += 'Player 1: Balanced. ';
        } else {
            comparison += 'Player 1: Defensive. ';
        }
        
        if (p2Aggressiveness > 0.6) {
            comparison += 'Player 2: Aggressive.';
        } else if (p2Aggressiveness > 0.4) {
            comparison += 'Player 2: Balanced.';
        } else {
            comparison += 'Player 2: Defensive.';
        }
        
        return comparison;
    }

    /**
     * Determine winner
     */
    determineWinner(player1Summary, player2Summary) {
        const p1Score = (
            player1Summary.performance.effectiveness * 0.4 +
            player1Summary.performance.survivalRate * 0.3 +
            (player1Summary.tacticalVictories / (player1Summary.tacticalVictories + player1Summary.tacticalDefeats || 1)) * 0.3
        );
        
        const p2Score = (
            player2Summary.performance.effectiveness * 0.4 +
            player2Summary.performance.survivalRate * 0.3 +
            (player2Summary.tacticalVictories / (player2Summary.tacticalVictories + player2Summary.tacticalDefeats || 1)) * 0.3
        );
        
        if (p1Score > p2Score * 1.1) {
            return {
                winner: 'player1',
                margin: 'decisive',
                score: { player1: p1Score, player2: p2Score }
            };
        } else if (p2Score > p1Score * 1.1) {
            return {
                winner: 'player2',
                margin: 'decisive',
                score: { player1: p1Score, player2: p2Score }
            };
        } else if (p1Score > p2Score) {
            return {
                winner: 'player1',
                margin: 'narrow',
                score: { player1: p1Score, player2: p2Score }
            };
        } else if (p2Score > p1Score) {
            return {
                winner: 'player2',
                margin: 'narrow',
                score: { player1: p1Score, player2: p2Score }
            };
        } else {
            return {
                winner: 'draw',
                margin: 'none',
                score: { player1: p1Score, player2: p2Score }
            };
        }
    }

    /**
     * Generate player-specific recommendations
     */
    generatePlayerRecommendations(playerSummary) {
        const recommendations = [];
        
        if (playerSummary.performance.effectiveness < 1.0) {
            recommendations.push('Improve combat effectiveness through better target selection');
        }
        if (playerSummary.performance.survivalRate < 0.7) {
            recommendations.push('Focus on force preservation and tactical withdrawals');
        }
        if (playerSummary.performance.mobility < 0.4) {
            recommendations.push('Increase mobility and maneuver to gain tactical advantages');
        }
        if (playerSummary.performance.aggressiveness < 0.3) {
            recommendations.push('Consider more aggressive tactics when appropriate');
        }
        if (playerSummary.aiRecommendationsFollowed / playerSummary.totalActions < 0.3) {
            recommendations.push('Pay more attention to AI analysis and recommendations');
        }
        
        return recommendations;
    }

    /**
     * Real-time update methods
     */

    /**
     * Subscribe to simulation updates
     */
    subscribeToSimulation(simulationId, callback) {
        if (!this.simulationSubscribers) {
            this.simulationSubscribers = new Map();
        }
        
        if (!this.simulationSubscribers.has(simulationId)) {
            this.simulationSubscribers.set(simulationId, []);
        }
        
        this.simulationSubscribers.get(simulationId).push(callback);
        
        console.log(`📊 Subscribed to simulation ${simulationId}`);
        return () => this.unsubscribeFromSimulation(simulationId, callback);
    }

    /**
     * Unsubscribe from simulation updates
     */
    unsubscribeFromSimulation(simulationId, callback) {
        if (!this.simulationSubscribers || !this.simulationSubscribers.has(simulationId)) {
            return;
        }
        
        const subscribers = this.simulationSubscribers.get(simulationId);
        const index = subscribers.indexOf(callback);
        if (index > -1) {
            subscribers.splice(index, 1);
        }
        
        console.log(`📊 Unsubscribed from simulation ${simulationId}`);
    }

    /**
     * Notify subscribers of simulation update
     */
    notifySubscribers(simulationId, updateData) {
        if (!this.simulationSubscribers || !this.simulationSubscribers.has(simulationId)) {
            return;
        }
        
        const subscribers = this.simulationSubscribers.get(simulationId);
        subscribers.forEach(callback => {
            try {
                callback(updateData);
            } catch (error) {
                console.error('Error in simulation subscriber callback:', error);
            }
        });
    }

    /**
     * Get real-time simulation status
     */
    getRealTimeStatus(simulationId) {
        const twoPlayerResults = this.twoPlayerResults.get(simulationId);
        if (!twoPlayerResults) return null;
        
        return {
            simulationId: simulationId,
            currentTurn: twoPlayerResults.totalTurns,
            player1Status: {
                casualties: twoPlayerResults.player1Summary.casualties,
                unitsRemaining: twoPlayerResults.player1Summary.unitsRemaining,
                performance: twoPlayerResults.player1Summary.performance
            },
            player2Status: {
                casualties: twoPlayerResults.player2Summary.casualties,
                unitsRemaining: twoPlayerResults.player2Summary.unitsRemaining,
                performance: twoPlayerResults.player2Summary.performance
            },
            overallStatus: twoPlayerResults.overallSummary,
            timestamp: new Date().toISOString()
        };
    }
}

// Initialize global instance
window.simulationResultsManager = new SimulationResultsManager();
console.log('📊 Simulation Results Manager initialized');
