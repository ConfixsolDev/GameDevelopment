/**
 * NATO Terrain Classification Engine
 * Implements NATO-compliant terrain classification and impact calculations
 * Based on NATO ATP-3-90.3 (Defensive Operations) and ATP-3-90.4 (Terrain Analysis)
 */

class NatoTerrainEngine {
    constructor() {
        this.initializeNatoTerrainStandards();
        this.initializeTerrainImpactCalculations();
        this.terrainData = new Map();
        this.terrainOverlays = new Map();
    }

    /**
     * Initialize NATO terrain classification standards
     */
    initializeNatoTerrainStandards() {
        this.natoTerrainStandards = {
            // NATO Terrain Types (ATP-3-90.4)
            terrainTypes: {
                'open': {
                    name: 'Open Terrain',
                    description: 'Flat, unobstructed terrain with minimal cover',
                    characteristics: {
                        cover: 'minimal',
                        concealment: 'minimal',
                        observation: 'excellent',
                        fieldsOfFire: 'excellent',
                        movement: 'unrestricted',
                        obstacles: 'minimal'
                    },
                    natoCode: 'OPEN',
                    classification: 'A'
                },
                'urban': {
                    name: 'Urban Terrain',
                    description: 'Built-up areas with buildings, streets, and infrastructure',
                    characteristics: {
                        cover: 'extensive',
                        concealment: 'extensive',
                        observation: 'limited',
                        fieldsOfFire: 'restricted',
                        movement: 'restricted',
                        obstacles: 'extensive'
                    },
                    natoCode: 'URBAN',
                    classification: 'B'
                },
                'forest': {
                    name: 'Forest Terrain',
                    description: 'Wooded areas with dense vegetation',
                    characteristics: {
                        cover: 'moderate',
                        concealment: 'extensive',
                        observation: 'limited',
                        fieldsOfFire: 'restricted',
                        movement: 'restricted',
                        obstacles: 'moderate'
                    },
                    natoCode: 'FOREST',
                    classification: 'B'
                },
                'hills': {
                    name: 'Hilly Terrain',
                    description: 'Elevated terrain with varying slopes',
                    characteristics: {
                        cover: 'moderate',
                        concealment: 'moderate',
                        observation: 'good',
                        fieldsOfFire: 'good',
                        movement: 'moderate',
                        obstacles: 'moderate'
                    },
                    natoCode: 'HILLS',
                    classification: 'B'
                },
                'swamp': {
                    name: 'Swamp/Marsh',
                    description: 'Wetland areas with poor drainage',
                    characteristics: {
                        cover: 'minimal',
                        concealment: 'moderate',
                        observation: 'limited',
                        fieldsOfFire: 'restricted',
                        movement: 'severely_restricted',
                        obstacles: 'extensive'
                    },
                    natoCode: 'SWAMP',
                    classification: 'C'
                },
                'desert': {
                    name: 'Desert Terrain',
                    description: 'Arid terrain with minimal vegetation',
                    characteristics: {
                        cover: 'minimal',
                        concealment: 'minimal',
                        observation: 'excellent',
                        fieldsOfFire: 'excellent',
                        movement: 'unrestricted',
                        obstacles: 'minimal'
                    },
                    natoCode: 'DESERT',
                    classification: 'A'
                },
                'mountain': {
                    name: 'Mountain Terrain',
                    description: 'High elevation terrain with steep slopes',
                    characteristics: {
                        cover: 'extensive',
                        concealment: 'extensive',
                        observation: 'excellent',
                        fieldsOfFire: 'excellent',
                        movement: 'severely_restricted',
                        obstacles: 'extensive'
                    },
                    natoCode: 'MOUNTAIN',
                    classification: 'C'
                },
                'water': {
                    name: 'Water Obstacle',
                    description: 'Rivers, lakes, and other water features',
                    characteristics: {
                        cover: 'none',
                        concealment: 'none',
                        observation: 'excellent',
                        fieldsOfFire: 'excellent',
                        movement: 'blocked',
                        obstacles: 'extensive'
                    },
                    natoCode: 'WATER',
                    classification: 'C'
                }
            },

            // NATO Terrain Impact Coefficients (ATP-3-90.3)
            terrainImpactCoefficients: {
                'open': {
                    attack: 1.1,
                    defense: 0.9,
                    movement: 1.0,
                    observation: 1.2,
                    fieldsOfFire: 1.2,
                    cover: 0.3,
                    concealment: 0.3
                },
                'urban': {
                    attack: 0.7,
                    defense: 1.3,
                    movement: 0.6,
                    observation: 0.5,
                    fieldsOfFire: 0.4,
                    cover: 1.5,
                    concealment: 1.5
                },
                'forest': {
                    attack: 0.8,
                    defense: 1.2,
                    movement: 0.7,
                    observation: 0.6,
                    fieldsOfFire: 0.5,
                    cover: 1.2,
                    concealment: 1.4
                },
                'hills': {
                    attack: 0.9,
                    defense: 1.1,
                    movement: 0.8,
                    observation: 1.1,
                    fieldsOfFire: 1.1,
                    cover: 1.0,
                    concealment: 1.0
                },
                'swamp': {
                    attack: 0.6,
                    defense: 1.4,
                    movement: 0.3,
                    observation: 0.7,
                    fieldsOfFire: 0.6,
                    cover: 0.8,
                    concealment: 1.2
                },
                'desert': {
                    attack: 1.0,
                    defense: 1.0,
                    movement: 1.0,
                    observation: 1.1,
                    fieldsOfFire: 1.1,
                    cover: 0.2,
                    concealment: 0.2
                },
                'mountain': {
                    attack: 0.5,
                    defense: 1.5,
                    movement: 0.2,
                    observation: 1.3,
                    fieldsOfFire: 1.3,
                    cover: 1.3,
                    concealment: 1.3
                },
                'water': {
                    attack: 0.4,
                    defense: 1.6,
                    movement: 0.1,
                    observation: 1.0,
                    fieldsOfFire: 1.0,
                    cover: 0.0,
                    concealment: 0.0
                }
            },

            // NATO Terrain Analysis Factors
            terrainAnalysisFactors: {
                'elevation': {
                    'low': 0.9,
                    'moderate': 1.0,
                    'high': 1.1,
                    'very_high': 1.2
                },
                'slope': {
                    'flat': 1.0,
                    'gentle': 0.95,
                    'moderate': 0.9,
                    'steep': 0.8,
                    'very_steep': 0.6
                },
                'vegetation': {
                    'none': 1.0,
                    'sparse': 0.95,
                    'moderate': 0.9,
                    'dense': 0.8,
                    'very_dense': 0.7
                },
                'drainage': {
                    'excellent': 1.0,
                    'good': 0.95,
                    'fair': 0.9,
                    'poor': 0.8,
                    'very_poor': 0.7
                },
                'soil': {
                    'firm': 1.0,
                    'moderate': 0.95,
                    'soft': 0.9,
                    'muddy': 0.8,
                    'quicksand': 0.6
                }
            }
        };
    }

    /**
     * Initialize terrain impact calculations
     */
    initializeTerrainImpactCalculations() {
        this.terrainCalculations = {
            // Movement Impact Calculations
            movementImpact: {
                'foot': {
                    'open': 1.0,
                    'urban': 0.8,
                    'forest': 0.7,
                    'hills': 0.8,
                    'swamp': 0.4,
                    'desert': 0.9,
                    'mountain': 0.3,
                    'water': 0.0
                },
                'wheeled': {
                    'open': 1.0,
                    'urban': 0.6,
                    'forest': 0.4,
                    'hills': 0.7,
                    'swamp': 0.2,
                    'desert': 0.8,
                    'mountain': 0.1,
                    'water': 0.0
                },
                'tracked': {
                    'open': 1.0,
                    'urban': 0.7,
                    'forest': 0.6,
                    'hills': 0.8,
                    'swamp': 0.5,
                    'desert': 0.9,
                    'mountain': 0.4,
                    'water': 0.3
                }
            },

            // Cover and Concealment Calculations
            coverConcealment: {
                'open': { cover: 0.1, concealment: 0.1 },
                'urban': { cover: 0.8, concealment: 0.7 },
                'forest': { cover: 0.6, concealment: 0.8 },
                'hills': { cover: 0.4, concealment: 0.5 },
                'swamp': { cover: 0.2, concealment: 0.6 },
                'desert': { cover: 0.05, concealment: 0.05 },
                'mountain': { cover: 0.7, concealment: 0.7 },
                'water': { cover: 0.0, concealment: 0.0 }
            },

            // Observation and Fields of Fire Calculations
            observationFieldsOfFire: {
                'open': { observation: 1.0, fieldsOfFire: 1.0 },
                'urban': { observation: 0.3, fieldsOfFire: 0.2 },
                'forest': { observation: 0.4, fieldsOfFire: 0.3 },
                'hills': { observation: 1.2, fieldsOfFire: 1.1 },
                'swamp': { observation: 0.6, fieldsOfFire: 0.5 },
                'desert': { observation: 1.1, fieldsOfFire: 1.0 },
                'mountain': { observation: 1.3, fieldsOfFire: 1.2 },
                'water': { observation: 1.0, fieldsOfFire: 1.0 }
            }
        };
    }

    /**
     * Classify terrain at specific coordinates
     * @param {Array} coordinates - [latitude, longitude]
     * @param {Object} terrainData - Terrain data from map
     * @returns {Object} Terrain classification
     */
    classifyTerrain(coordinates, terrainData = {}) {
        console.log('🗺️ Classifying terrain at coordinates:', coordinates);
        
        // Extract terrain information
        const terrainType = terrainData.type || 'open';
        const elevation = terrainData.elevation || 0;
        const slope = terrainData.slope || 'flat';
        const vegetation = terrainData.vegetation || 'none';
        const drainage = terrainData.drainage || 'good';
        const soil = terrainData.soil || 'firm';
        
        // Get base terrain classification
        const baseTerrain = this.natoTerrainStandards.terrainTypes[terrainType];
        if (!baseTerrain) {
            console.warn('Unknown terrain type:', terrainType);
            return this.getDefaultTerrainClassification();
        }
        
        // Apply terrain analysis factors
        const elevationFactor = this.natoTerrainStandards.terrainAnalysisFactors.elevation[this.categorizeElevation(elevation)] || 1.0;
        const slopeFactor = this.natoTerrainStandards.terrainAnalysisFactors.slope[slope] || 1.0;
        const vegetationFactor = this.natoTerrainStandards.terrainAnalysisFactors.vegetation[vegetation] || 1.0;
        const drainageFactor = this.natoTerrainStandards.terrainAnalysisFactors.drainage[drainage] || 1.0;
        const soilFactor = this.natoTerrainStandards.terrainAnalysisFactors.soil[soil] || 1.0;
        
        // Calculate combined impact
        const combinedFactor = elevationFactor * slopeFactor * vegetationFactor * drainageFactor * soilFactor;
        
        // Get terrain impact coefficients
        const impactCoefficients = this.natoTerrainStandards.terrainImpactCoefficients[terrainType];
        
        // Create comprehensive terrain classification
        const classification = {
            coordinates: coordinates,
            terrainType: terrainType,
            natoCode: baseTerrain.natoCode,
            classification: baseTerrain.classification,
            name: baseTerrain.name,
            description: baseTerrain.description,
            characteristics: baseTerrain.characteristics,
            elevation: elevation,
            slope: slope,
            vegetation: vegetation,
            drainage: drainage,
            soil: soil,
            factors: {
                elevation: elevationFactor,
                slope: slopeFactor,
                vegetation: vegetationFactor,
                drainage: drainageFactor,
                soil: soilFactor,
                combined: combinedFactor
            },
            impactCoefficients: {
                attack: impactCoefficients.attack * combinedFactor,
                defense: impactCoefficients.defense * combinedFactor,
                movement: impactCoefficients.movement * combinedFactor,
                observation: impactCoefficients.observation * combinedFactor,
                fieldsOfFire: impactCoefficients.fieldsOfFire * combinedFactor,
                cover: impactCoefficients.cover * combinedFactor,
                concealment: impactCoefficients.concealment * combinedFactor
            },
            movementImpact: this.calculateMovementImpact(terrainType, combinedFactor),
            coverConcealment: this.calculateCoverConcealment(terrainType, combinedFactor),
            observationFieldsOfFire: this.calculateObservationFieldsOfFire(terrainType, combinedFactor),
            tacticalAssessment: this.generateTacticalAssessment(terrainType, impactCoefficients, combinedFactor),
            natoCompliance: true,
            timestamp: new Date().toISOString()
        };
        
        // Store terrain data
        const terrainId = this.generateTerrainId(coordinates);
        this.terrainData.set(terrainId, classification);
        
        console.log('✅ Terrain classified:', classification);
        return classification;
    }

    /**
     * Calculate movement impact for different unit types
     */
    calculateMovementImpact(terrainType, combinedFactor) {
        const movementImpact = {};
        
        Object.entries(this.terrainCalculations.movementImpact).forEach(([unitType, impacts]) => {
            movementImpact[unitType] = impacts[terrainType] * combinedFactor;
        });
        
        return movementImpact;
    }

    /**
     * Calculate cover and concealment values
     */
    calculateCoverConcealment(terrainType, combinedFactor) {
        const baseValues = this.terrainCalculations.coverConcealment[terrainType];
        return {
            cover: baseValues.cover * combinedFactor,
            concealment: baseValues.concealment * combinedFactor
        };
    }

    /**
     * Calculate observation and fields of fire
     */
    calculateObservationFieldsOfFire(terrainType, combinedFactor) {
        const baseValues = this.terrainCalculations.observationFieldsOfFire[terrainType];
        return {
            observation: baseValues.observation * combinedFactor,
            fieldsOfFire: baseValues.fieldsOfFire * combinedFactor
        };
    }

    /**
     * Generate tactical assessment
     */
    generateTacticalAssessment(terrainType, impactCoefficients, combinedFactor) {
        const assessment = {
            advantages: [],
            disadvantages: [],
            recommendations: [],
            tacticalValue: 'moderate'
        };
        
        // Analyze terrain advantages
        if (impactCoefficients.defense > 1.2) {
            assessment.advantages.push('Excellent defensive terrain');
        }
        if (impactCoefficients.observation > 1.1) {
            assessment.advantages.push('Good observation positions');
        }
        if (impactCoefficients.fieldsOfFire > 1.1) {
            assessment.advantages.push('Excellent fields of fire');
        }
        if (impactCoefficients.concealment > 1.2) {
            assessment.advantages.push('Good concealment available');
        }
        
        // Analyze terrain disadvantages
        if (impactCoefficients.movement < 0.8) {
            assessment.disadvantages.push('Restricted movement');
        }
        if (impactCoefficients.attack < 0.8) {
            assessment.disadvantages.push('Difficult for offensive operations');
        }
        if (impactCoefficients.observation < 0.7) {
            assessment.disadvantages.push('Limited observation');
        }
        if (impactCoefficients.fieldsOfFire < 0.7) {
            assessment.disadvantages.push('Restricted fields of fire');
        }
        
        // Generate recommendations
        if (impactCoefficients.defense > 1.2) {
            assessment.recommendations.push('Utilize for defensive positions');
        }
        if (impactCoefficients.attack > 1.1) {
            assessment.recommendations.push('Suitable for offensive operations');
        }
        if (impactCoefficients.movement < 0.8) {
            assessment.recommendations.push('Plan alternative routes');
        }
        if (impactCoefficients.concealment > 1.2) {
            assessment.recommendations.push('Exploit concealment for surprise');
        }
        
        // Determine tactical value
        const tacticalScore = (impactCoefficients.defense + impactCoefficients.attack + 
                              impactCoefficients.observation + impactCoefficients.fieldsOfFire) / 4;
        
        if (tacticalScore > 1.2) {
            assessment.tacticalValue = 'high';
        } else if (tacticalScore < 0.8) {
            assessment.tacticalValue = 'low';
        } else {
            assessment.tacticalValue = 'moderate';
        }
        
        return assessment;
    }

    /**
     * Calculate terrain impact on combat effectiveness
     * @param {Object} terrainClassification - Terrain classification
     * @param {Object} combatData - Combat data (attacker, defender, unit types)
     * @returns {Object} Terrain impact on combat
     */
    calculateTerrainCombatImpact(terrainClassification, combatData) {
        console.log('⚔️ Calculating terrain combat impact...');
        
        const { attacker, defender } = combatData;
        const terrain = terrainClassification;
        
        // Calculate attacker impact
        const attackerImpact = {
            movement: this.getMovementImpact(attacker.unitType, terrain.terrainType),
            observation: terrain.impactCoefficients.observation,
            fieldsOfFire: terrain.impactCoefficients.fieldsOfFire,
            cover: terrain.impactCoefficients.cover,
            concealment: terrain.impactCoefficients.concealment,
            overall: terrain.impactCoefficients.attack
        };
        
        // Calculate defender impact
        const defenderImpact = {
            movement: this.getMovementImpact(defender.unitType, terrain.terrainType),
            observation: terrain.impactCoefficients.observation,
            fieldsOfFire: terrain.impactCoefficients.fieldsOfFire,
            cover: terrain.impactCoefficients.cover,
            concealment: terrain.impactCoefficients.concealment,
            overall: terrain.impactCoefficients.defense
        };
        
        // Calculate relative advantage
        const relativeAdvantage = attackerImpact.overall / defenderImpact.overall;
        
        // Determine terrain advantage
        let terrainAdvantage;
        if (relativeAdvantage > 1.2) {
            terrainAdvantage = 'attacker';
        } else if (relativeAdvantage < 0.8) {
            terrainAdvantage = 'defender';
        } else {
            terrainAdvantage = 'neutral';
        }
        
        const result = {
            terrain: terrain,
            attackerImpact: attackerImpact,
            defenderImpact: defenderImpact,
            relativeAdvantage: relativeAdvantage,
            terrainAdvantage: terrainAdvantage,
            tacticalImplications: this.generateTacticalImplications(terrain, attackerImpact, defenderImpact),
            natoCompliance: true,
            timestamp: new Date().toISOString()
        };
        
        console.log('✅ Terrain combat impact calculated:', result);
        return result;
    }

    /**
     * Get movement impact for specific unit type
     */
    getMovementImpact(unitType, terrainType) {
        const unitTypeMap = {
            'Infantry': 'foot',
            'Armoured': 'tracked',
            'Artillery': 'wheeled',
            'Engineers': 'foot',
            'Signals': 'wheeled',
            'Medical': 'wheeled'
        };
        
        const movementType = unitTypeMap[unitType] || 'foot';
        return this.terrainCalculations.movementImpact[movementType][terrainType] || 1.0;
    }

    /**
     * Generate tactical implications
     */
    generateTacticalImplications(terrain, attackerImpact, defenderImpact) {
        const implications = {
            attacker: [],
            defender: [],
            general: []
        };
        
        // Attacker implications
        if (attackerImpact.overall > 1.1) {
            implications.attacker.push('Terrain favors offensive operations');
        } else if (attackerImpact.overall < 0.9) {
            implications.attacker.push('Terrain hinders offensive operations');
        }
        
        if (attackerImpact.movement < 0.8) {
            implications.attacker.push('Consider alternative approach routes');
        }
        
        if (attackerImpact.concealment > 1.2) {
            implications.attacker.push('Exploit concealment for surprise');
        }
        
        // Defender implications
        if (defenderImpact.overall > 1.1) {
            implications.defender.push('Terrain favors defensive operations');
        } else if (defenderImpact.overall < 0.9) {
            implications.defender.push('Terrain hinders defensive operations');
        }
        
        if (defenderImpact.cover > 1.2) {
            implications.defender.push('Utilize available cover');
        }
        
        if (defenderImpact.observation > 1.1) {
            implications.defender.push('Exploit observation advantages');
        }
        
        // General implications
        if (terrain.impactCoefficients.movement < 0.8) {
            implications.general.push('Plan for restricted movement');
        }
        
        if (terrain.impactCoefficients.observation < 0.7) {
            implications.general.push('Limited observation capabilities');
        }
        
        if (terrain.impactCoefficients.fieldsOfFire < 0.7) {
            implications.general.push('Restricted fields of fire');
        }
        
        return implications;
    }

    /**
     * Analyze terrain corridor for movement
     * @param {Array} routeCoordinates - Array of coordinate pairs
     * @param {Object} unitType - Unit type for movement analysis
     * @returns {Object} Terrain corridor analysis
     */
    analyzeTerrainCorridor(routeCoordinates, unitType) {
        console.log('🛣️ Analyzing terrain corridor...');
        
        const corridorAnalysis = {
            route: routeCoordinates,
            unitType: unitType,
            segments: [],
            overallAssessment: {},
            recommendations: [],
            natoCompliance: true,
            timestamp: new Date().toISOString()
        };
        
        // Analyze each segment
        for (let i = 0; i < routeCoordinates.length - 1; i++) {
            const start = routeCoordinates[i];
            const end = routeCoordinates[i + 1];
            
            // Get terrain classification for segment
            const terrainClassification = this.classifyTerrain(start);
            
            // Calculate segment analysis
            const segmentAnalysis = {
                segment: i + 1,
                start: start,
                end: end,
                terrain: terrainClassification,
                movementDifficulty: this.getMovementImpact(unitType, terrainClassification.terrainType),
                tacticalValue: terrainClassification.tacticalAssessment.tacticalValue,
                recommendations: terrainClassification.tacticalAssessment.recommendations
            };
            
            corridorAnalysis.segments.push(segmentAnalysis);
        }
        
        // Calculate overall assessment
        const averageMovementDifficulty = corridorAnalysis.segments.reduce((sum, segment) => 
            sum + segment.movementDifficulty, 0) / corridorAnalysis.segments.length;
        
        const tacticalValues = corridorAnalysis.segments.map(segment => segment.tacticalValue);
        const mostCommonTacticalValue = this.getMostCommon(tacticalValues);
        
        corridorAnalysis.overallAssessment = {
            averageMovementDifficulty: averageMovementDifficulty,
            dominantTacticalValue: mostCommonTacticalValue,
            totalSegments: corridorAnalysis.segments.length,
            difficultSegments: corridorAnalysis.segments.filter(s => s.movementDifficulty < 0.8).length,
            favorableSegments: corridorAnalysis.segments.filter(s => s.movementDifficulty > 1.1).length
        };
        
        // Generate recommendations
        if (averageMovementDifficulty < 0.8) {
            corridorAnalysis.recommendations.push('Route has significant movement restrictions');
        }
        
        if (corridorAnalysis.overallAssessment.difficultSegments > corridorAnalysis.segments.length / 2) {
            corridorAnalysis.recommendations.push('Consider alternative route');
        }
        
        if (corridorAnalysis.overallAssessment.favorableSegments > corridorAnalysis.segments.length / 2) {
            corridorAnalysis.recommendations.push('Route is favorable for movement');
        }
        
        console.log('✅ Terrain corridor analyzed:', corridorAnalysis);
        return corridorAnalysis;
    }

    /**
     * Create terrain overlay for map visualization
     * @param {Object} map - Leaflet map instance
     * @param {Object} terrainData - Terrain data to visualize
     * @returns {Object} Terrain overlay
     */
    createTerrainOverlay(map, terrainData) {
        console.log('🗺️ Creating terrain overlay...');
        
        const overlayId = this.generateOverlayId();
        const overlay = {
            id: overlayId,
            map: map,
            layers: new Map(),
            terrainData: terrainData,
            visible: true,
            opacity: 0.7
        };
        
        // Create terrain visualization layers
        Object.entries(terrainData).forEach(([terrainId, terrain]) => {
            const layer = this.createTerrainLayer(terrain);
            if (layer) {
                overlay.layers.set(terrainId, layer);
                if (overlay.visible) {
                    layer.addTo(map);
                }
            }
        });
        
        this.terrainOverlays.set(overlayId, overlay);
        
        console.log('✅ Terrain overlay created:', overlay);
        return overlay;
    }

    /**
     * Create individual terrain layer
     */
    createTerrainLayer(terrain) {
        // This would create Leaflet layers based on terrain type
        // For now, return a placeholder
        return {
            addTo: (map) => console.log('Adding terrain layer to map'),
            remove: () => console.log('Removing terrain layer'),
            setOpacity: (opacity) => console.log('Setting terrain layer opacity:', opacity)
        };
    }

    /**
     * Utility methods
     */
    categorizeElevation(elevation) {
        if (elevation < 100) return 'low';
        if (elevation < 500) return 'moderate';
        if (elevation < 1000) return 'high';
        return 'very_high';
    }

    getMostCommon(array) {
        const counts = {};
        array.forEach(item => {
            counts[item] = (counts[item] || 0) + 1;
        });
        return Object.keys(counts).reduce((a, b) => counts[a] > counts[b] ? a : b);
    }

    getDefaultTerrainClassification() {
        return {
            terrainType: 'open',
            natoCode: 'OPEN',
            classification: 'A',
            name: 'Open Terrain',
            description: 'Default terrain classification',
            impactCoefficients: this.natoTerrainStandards.terrainImpactCoefficients.open,
            natoCompliance: true
        };
    }

    generateTerrainId(coordinates) {
        return `terrain_${coordinates[0]}_${coordinates[1]}`;
    }

    generateOverlayId() {
        return `overlay_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
    }

    /**
     * Get terrain data for specific coordinates
     */
    getTerrainData(coordinates) {
        const terrainId = this.generateTerrainId(coordinates);
        return this.terrainData.get(terrainId);
    }

    /**
     * Get all terrain data
     */
    getAllTerrainData() {
        return Array.from(this.terrainData.values());
    }

    /**
     * Clear terrain data
     */
    clearTerrainData() {
        this.terrainData.clear();
        this.terrainOverlays.clear();
        console.log('🗺️ Terrain data cleared');
    }
}

// Initialize global instance
window.natoTerrainEngine = new NatoTerrainEngine();
console.log('🗺️ NATO Terrain Engine initialized');
