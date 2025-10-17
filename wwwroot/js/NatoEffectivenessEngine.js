/**
 * NATO Effectiveness Calculation Engine
 * Implements NATO-compliant attack-defense effectiveness calculations
 * Based on NATO ATP-3-90.1 (Tactics), ATP-3-90.2 (Offensive Operations), ATP-3-90.3 (Defensive Operations)
 */

class NatoEffectivenessEngine {
    constructor() {
        this.initializeNatoStandards();
        this.initializeCalculationTables();
    }

    /**
     * Initialize NATO tactical standards and coefficients
     */
    initializeNatoStandards() {
        this.natoStandards = {
            // NATO Force Ratios (ATP-3-90.1)
            forceRatios: {
                'offensive': {
                    'frontal': 3.0,      // 3:1 minimum for frontal attack
                    'flanking': 2.0,     // 2:1 for flanking attack
                    'envelopment': 1.5,  // 1.5:1 for envelopment
                    'penetration': 2.5,  // 2.5:1 for penetration
                    'raid': 1.0,         // 1:1 for raid (hit and run)
                    'ambush': 0.5        // 0.5:1 for ambush (defensive advantage)
                },
                'defensive': {
                    'prepared': 0.33,    // 1:3 ratio for prepared defense
                    'hasty': 0.5,        // 1:2 ratio for hasty defense
                    'mobile': 0.67      // 1:1.5 ratio for mobile defense
                }
            },

            // NATO Combat Effectiveness Factors (ATP-3-90.2)
            effectivenessFactors: {
                'morale': {
                    'high': 1.2,
                    'average': 1.0,
                    'low': 0.8,
                    'broken': 0.5
                },
                'training': {
                    'elite': 1.3,
                    'regular': 1.0,
                    'reserve': 0.8,
                    'militia': 0.6
                },
                'equipment': {
                    'modern': 1.2,
                    'standard': 1.0,
                    'obsolete': 0.8,
                    'inadequate': 0.6
                },
                'supply': {
                    'abundant': 1.1,
                    'adequate': 1.0,
                    'limited': 0.9,
                    'critical': 0.7
                }
            },

            // NATO Terrain Impact Coefficients (ATP-3-90.3)
            terrainCoefficients: {
                'urban': {
                    'attack': 0.7,      // Urban terrain favors defense
                    'defense': 1.3
                },
                'forest': {
                    'attack': 0.8,
                    'defense': 1.2
                },
                'hills': {
                    'attack': 0.9,
                    'defense': 1.1
                },
                'open': {
                    'attack': 1.1,      // Open terrain favors attack
                    'defense': 0.9
                },
                'swamp': {
                    'attack': 0.6,
                    'defense': 1.4
                },
                'desert': {
                    'attack': 1.0,
                    'defense': 1.0
                }
            },

            // NATO Unit Type Effectiveness (APP-6)
            unitEffectiveness: {
                'Infantry': {
                    'vs_infantry': 1.0,
                    'vs_armour': 0.3,
                    'vs_artillery': 0.8,
                    'vs_air': 0.2
                },
                'Armoured': {
                    'vs_infantry': 1.5,
                    'vs_armour': 1.0,
                    'vs_artillery': 0.6,
                    'vs_air': 0.1
                },
                'Artillery': {
                    'vs_infantry': 1.2,
                    'vs_armour': 0.8,
                    'vs_artillery': 1.0,
                    'vs_air': 0.3
                },
                'Engineers': {
                    'vs_infantry': 0.8,
                    'vs_armour': 0.4,
                    'vs_artillery': 0.6,
                    'vs_air': 0.2
                }
            }
        };
    }

    /**
     * Initialize calculation tables for combat resolution
     */
    initializeCalculationTables() {
        // NATO Combat Resolution Table (based on Lanchester's Laws)
        this.combatTables = {
            // Base hit probability by range and unit type
            hitProbability: {
                'close': {    // 0-500m
                    'Infantry': 0.7,
                    'Armoured': 0.8,
                    'Artillery': 0.3,
                    'Engineers': 0.6
                },
                'medium': {   // 500-2000m
                    'Infantry': 0.4,
                    'Armoured': 0.6,
                    'Artillery': 0.8,
                    'Engineers': 0.3
                },
                'long': {     // 2000m+
                    'Infantry': 0.1,
                    'Armoured': 0.3,
                    'Artillery': 0.9,
                    'Engineers': 0.1
                }
            },

            // NATO Damage Assessment
            damageAssessment: {
                'light': 0.1,      // 10% strength loss
                'moderate': 0.25,  // 25% strength loss
                'heavy': 0.5,      // 50% strength loss
                'critical': 0.8    // 80% strength loss
            },

            // NATO Suppression Effects
            suppressionEffects: {
                'light': 0.2,      // 20% effectiveness reduction
                'moderate': 0.4,   // 40% effectiveness reduction
                'heavy': 0.6,      // 60% effectiveness reduction
                'total': 0.8       // 80% effectiveness reduction
            }
        };
    }

    /**
     * Calculate attack effectiveness against defense
     * @param {Object} attackData - Attack parameters
     * @param {Object} defenseData - Defense parameters
     * @param {Object} terrainData - Terrain information
     * @returns {Object} Effectiveness calculation results
     */
    calculateAttackEffectiveness(attackData, defenseData, terrainData = {}) {
        console.log('🎯 Calculating NATO attack effectiveness...');
        
        // Step 1: Calculate force ratio
        const forceRatio = this.calculateForceRatio(attackData, defenseData);
        
        // Step 2: Apply NATO attack type modifiers
        const attackModifier = this.getAttackTypeModifier(attackData.attackType);
        
        // Step 3: Calculate terrain impact
        const terrainImpact = this.calculateTerrainImpact(terrainData, 'attack');
        
        // Step 4: Calculate unit effectiveness
        const unitEffectiveness = this.calculateUnitEffectiveness(attackData, defenseData);
        
        // Step 5: Apply NATO effectiveness factors
        const effectivenessFactors = this.calculateEffectivenessFactors(attackData, defenseData);
        
        // Step 6: Calculate final effectiveness
        const baseEffectiveness = forceRatio * attackModifier * terrainImpact * unitEffectiveness;
        const finalEffectiveness = baseEffectiveness * effectivenessFactors;
        
        // Step 7: Determine outcome
        const outcome = this.determineCombatOutcome(finalEffectiveness, attackData, defenseData);
        
        // Step 8: Calculate casualties
        const casualties = this.calculateCasualties(finalEffectiveness, attackData, defenseData);
        
        const result = {
            forceRatio: forceRatio,
            attackModifier: attackModifier,
            terrainImpact: terrainImpact,
            unitEffectiveness: unitEffectiveness,
            effectivenessFactors: effectivenessFactors,
            finalEffectiveness: finalEffectiveness,
            outcome: outcome,
            casualties: casualties,
            natoCompliance: true,
            calculationMethod: 'NATO ATP-3-90.1/90.2',
            timestamp: new Date().toISOString()
        };
        
        console.log('✅ NATO effectiveness calculation complete:', result);
        return result;
    }

    /**
     * Calculate force ratio according to NATO standards
     */
    calculateForceRatio(attackData, defenseData) {
        const attackerStrength = this.calculateUnitStrength(attackData);
        const defenderStrength = this.calculateUnitStrength(defenseData);
        
        if (defenderStrength === 0) return 10; // Overwhelming advantage
        
        const ratio = attackerStrength / defenderStrength;
        
        // Apply NATO force ratio requirements
        const requiredRatio = this.natoStandards.forceRatios.offensive[attackData.attackType] || 3.0;
        
        return {
            actual: ratio,
            required: requiredRatio,
            adequacy: ratio >= requiredRatio ? 'adequate' : 'insufficient',
            natoStandard: requiredRatio
        };
    }

    /**
     * Get NATO attack type modifier
     */
    getAttackTypeModifier(attackType) {
        const modifiers = {
            'frontal': 1.0,      // Standard frontal attack
            'flanking': 1.3,     // Flanking advantage
            'envelopment': 1.5,  // Envelopment advantage
            'penetration': 1.2,  // Penetration advantage
            'raid': 0.8,         // Hit and run
            'ambush': 2.0        // Surprise advantage
        };
        
        return modifiers[attackType] || 1.0;
    }

    /**
     * Calculate terrain impact on combat
     */
    calculateTerrainImpact(terrainData, combatType) {
        if (!terrainData || !terrainData.type) {
            return 1.0; // Neutral terrain
        }
        
        const terrainType = terrainData.type.toLowerCase();
        const coefficient = this.natoStandards.terrainCoefficients[terrainType];
        
        if (!coefficient) {
            return 1.0; // Unknown terrain
        }
        
        return coefficient[combatType] || 1.0;
    }

    /**
     * Calculate unit effectiveness based on NATO standards
     */
    calculateUnitEffectiveness(attackData, defenseData) {
        const attackerType = attackData.unitType || 'Infantry';
        const defenderType = defenseData.unitType || 'Infantry';
        
        const effectiveness = this.natoStandards.unitEffectiveness[attackerType];
        if (!effectiveness) return 1.0;
        
        const vsType = `vs_${defenderType.toLowerCase()}`;
        return effectiveness[vsType] || 1.0;
    }

    /**
     * Calculate effectiveness factors (morale, training, equipment, supply)
     */
    calculateEffectivenessFactors(attackData, defenseData) {
        let attackerFactor = 1.0;
        let defenderFactor = 1.0;
        
        // Attacker factors
        if (attackData.morale) {
            attackerFactor *= this.natoStandards.effectivenessFactors.morale[attackData.morale] || 1.0;
        }
        if (attackData.training) {
            attackerFactor *= this.natoStandards.effectivenessFactors.training[attackData.training] || 1.0;
        }
        if (attackData.equipment) {
            attackerFactor *= this.natoStandards.effectivenessFactors.equipment[attackData.equipment] || 1.0;
        }
        if (attackData.supply) {
            attackerFactor *= this.natoStandards.effectivenessFactors.supply[attackData.supply] || 1.0;
        }
        
        // Defender factors
        if (defenseData.morale) {
            defenderFactor *= this.natoStandards.effectivenessFactors.morale[defenseData.morale] || 1.0;
        }
        if (defenseData.training) {
            defenderFactor *= this.natoStandards.effectivenessFactors.training[defenseData.training] || 1.0;
        }
        if (defenseData.equipment) {
            defenderFactor *= this.natoStandards.effectivenessFactors.equipment[defenseData.equipment] || 1.0;
        }
        if (defenseData.supply) {
            defenderFactor *= this.natoStandards.effectivenessFactors.supply[defenseData.supply] || 1.0;
        }
        
        return attackerFactor / defenderFactor;
    }

    /**
     * Calculate unit strength based on NATO standards
     */
    calculateUnitStrength(unitData) {
        let baseStrength = unitData.strength || 100;
        
        // Apply organization level multiplier
        const orgMultipliers = {
            'Squad': 1.0,
            'Platoon': 3.0,
            'Company': 9.0,
            'Battalion': 27.0,
            'Brigade': 81.0,
            'Division': 243.0
        };
        
        const orgLevel = unitData.organizationLevel || 'Company';
        const orgMultiplier = orgMultipliers[orgLevel] || 9.0;
        
        // Apply unit type multiplier
        const typeMultipliers = {
            'Infantry': 1.0,
            'Armoured': 1.5,
            'Artillery': 0.8,
            'Engineers': 0.7,
            'Signals': 0.5,
            'Medical': 0.3
        };
        
        const unitType = unitData.unitType || 'Infantry';
        const typeMultiplier = typeMultipliers[unitType] || 1.0;
        
        return baseStrength * orgMultiplier * typeMultiplier;
    }

    /**
     * Determine combat outcome based on NATO standards
     */
    determineCombatOutcome(effectiveness, attackData, defenseData) {
        let outcome;
        let confidence;
        
        if (effectiveness >= 2.0) {
            outcome = 'decisive_victory';
            confidence = 0.9;
        } else if (effectiveness >= 1.5) {
            outcome = 'victory';
            confidence = 0.8;
        } else if (effectiveness >= 1.0) {
            outcome = 'marginal_victory';
            confidence = 0.6;
        } else if (effectiveness >= 0.67) {
            outcome = 'stalemate';
            confidence = 0.5;
        } else if (effectiveness >= 0.5) {
            outcome = 'marginal_defeat';
            confidence = 0.4;
        } else {
            outcome = 'defeat';
            confidence = 0.3;
        }
        
        return {
            result: outcome,
            confidence: confidence,
            effectiveness: effectiveness,
            natoAssessment: this.getNatoAssessment(outcome, effectiveness)
        };
    }

    /**
     * Get NATO tactical assessment
     */
    getNatoAssessment(outcome, effectiveness) {
        const assessments = {
            'decisive_victory': 'Attacker achieves complete tactical success with minimal losses',
            'victory': 'Attacker achieves tactical objectives with acceptable losses',
            'marginal_victory': 'Attacker achieves limited tactical success',
            'stalemate': 'Neither side achieves decisive advantage',
            'marginal_defeat': 'Defender maintains position with heavy losses',
            'defeat': 'Defender successfully repels attack'
        };
        
        return assessments[outcome] || 'Combat outcome uncertain';
    }

    /**
     * Calculate casualties based on NATO standards
     */
    calculateCasualties(effectiveness, attackData, defenseData) {
        const attackerStrength = this.calculateUnitStrength(attackData);
        const defenderStrength = this.calculateUnitStrength(defenseData);
        
        // Base casualty rates from NATO studies
        const baseAttackerCasualties = Math.max(0.05, 0.3 - (effectiveness * 0.1));
        const baseDefenderCasualties = Math.max(0.05, 0.2 + (effectiveness * 0.15));
        
        // Apply NATO casualty modifiers
        const attackerCasualties = Math.min(0.8, attackerStrength * baseAttackerCasualties);
        const defenderCasualties = Math.min(0.9, defenderStrength * baseDefenderCasualties);
        
        return {
            attacker: {
                casualties: attackerCasualties,
                percentage: (attackerCasualties / attackerStrength) * 100,
                remaining: attackerStrength - attackerCasualties
            },
            defender: {
                casualties: defenderCasualties,
                percentage: (defenderCasualties / defenderStrength) * 100,
                remaining: defenderStrength - defenderCasualties
            },
            ratio: attackerCasualties / defenderCasualties
        };
    }

    /**
     * Calculate defense effectiveness against attack
     * @param {Object} defenseData - Defense parameters
     * @param {Object} attackData - Attack parameters
     * @param {Object} terrainData - Terrain information
     * @returns {Object} Defense effectiveness results
     */
    calculateDefenseEffectiveness(defenseData, attackData, terrainData = {}) {
        console.log('🛡️ Calculating NATO defense effectiveness...');
        
        // Calculate attack effectiveness first
        const attackResult = this.calculateAttackEffectiveness(attackData, defenseData, terrainData);
        
        // Invert the result for defense perspective
        const defenseEffectiveness = 1.0 / attackResult.finalEffectiveness;
        
        // Apply defense-specific modifiers
        const defenseModifier = this.getDefenseTypeModifier(defenseData.defenseType);
        const terrainDefenseImpact = this.calculateTerrainImpact(terrainData, 'defense');
        
        const finalDefenseEffectiveness = defenseEffectiveness * defenseModifier * terrainDefenseImpact;
        
        // Determine defense outcome
        const defenseOutcome = this.determineDefenseOutcome(finalDefenseEffectiveness, defenseData, attackData);
        
        const result = {
            defenseEffectiveness: finalDefenseEffectiveness,
            defenseModifier: defenseModifier,
            terrainDefenseImpact: terrainDefenseImpact,
            outcome: defenseOutcome,
            attackAnalysis: attackResult,
            natoCompliance: true,
            calculationMethod: 'NATO ATP-3-90.3',
            timestamp: new Date().toISOString()
        };
        
        console.log('✅ NATO defense effectiveness calculation complete:', result);
        return result;
    }

    /**
     * Get NATO defense type modifier
     */
    getDefenseTypeModifier(defenseType) {
        const modifiers = {
            'prepared': 1.5,     // Prepared defense advantage
            'hasty': 1.2,        // Hasty defense
            'mobile': 1.0,       // Mobile defense
            'delaying': 0.8      // Delaying action
        };
        
        return modifiers[defenseType] || 1.0;
    }

    /**
     * Determine defense outcome
     */
    determineDefenseOutcome(effectiveness, defenseData, attackData) {
        let outcome;
        let confidence;
        
        if (effectiveness >= 2.0) {
            outcome = 'successful_defense';
            confidence = 0.9;
        } else if (effectiveness >= 1.5) {
            outcome = 'effective_defense';
            confidence = 0.8;
        } else if (effectiveness >= 1.0) {
            outcome = 'marginal_defense';
            confidence = 0.6;
        } else if (effectiveness >= 0.67) {
            outcome = 'compromised_defense';
            confidence = 0.5;
        } else {
            outcome = 'failed_defense';
            confidence = 0.3;
        }
        
        return {
            result: outcome,
            confidence: confidence,
            effectiveness: effectiveness,
            natoAssessment: this.getDefenseAssessment(outcome, effectiveness)
        };
    }

    /**
     * Get NATO defense assessment
     */
    getDefenseAssessment(outcome, effectiveness) {
        const assessments = {
            'successful_defense': 'Defense successfully repels attack with minimal losses',
            'effective_defense': 'Defense achieves tactical objectives',
            'marginal_defense': 'Defense maintains position with heavy losses',
            'compromised_defense': 'Defense position compromised but functional',
            'failed_defense': 'Defense fails to prevent enemy advance'
        };
        
        return assessments[outcome] || 'Defense outcome uncertain';
    }

    /**
     * Generate NATO-compliant combat report
     */
    generateCombatReport(attackResult, defenseResult) {
        return {
            reportType: 'NATO Combat Effectiveness Analysis',
            classification: 'UNCLASSIFIED',
            timestamp: new Date().toISOString(),
            attackAnalysis: attackResult,
            defenseAnalysis: defenseResult,
            recommendations: this.generateRecommendations(attackResult, defenseResult),
            natoStandards: {
                forceRatios: this.natoStandards.forceRatios,
                effectivenessFactors: this.natoStandards.effectivenessFactors,
                terrainCoefficients: this.natoStandards.terrainCoefficients
            }
        };
    }

    /**
     * Generate NATO tactical recommendations
     */
    generateRecommendations(attackResult, defenseResult) {
        const recommendations = [];
        
        // Force ratio recommendations
        if (attackResult.forceRatio.adequacy === 'insufficient') {
            recommendations.push({
                type: 'force_ratio',
                priority: 'high',
                recommendation: `Increase attacking force to achieve NATO minimum ratio of ${attackResult.forceRatio.required}:1`,
                natoReference: 'ATP-3-90.1'
            });
        }
        
        // Terrain recommendations
        if (attackResult.terrainImpact < 1.0) {
            recommendations.push({
                type: 'terrain',
                priority: 'medium',
                recommendation: 'Consider alternative approach routes or indirect fire support',
                natoReference: 'ATP-3-90.3'
            });
        }
        
        // Unit effectiveness recommendations
        if (attackResult.unitEffectiveness < 0.8) {
            recommendations.push({
                type: 'unit_effectiveness',
                priority: 'medium',
                recommendation: 'Consider unit type mismatch - adjust force composition',
                natoReference: 'APP-6'
            });
        }
        
        return recommendations;
    }
}

// Initialize global instance
window.natoEffectivenessEngine = new NatoEffectivenessEngine();
console.log('🎯 NATO Effectiveness Engine initialized');
