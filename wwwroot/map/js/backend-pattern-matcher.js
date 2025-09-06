/**
 * Backend Pattern Matcher - Frontend integration for backend pattern matching
 * This replaces the frontend pattern matching with API calls to the backend
 */
class BackendPatternMatcher {
    constructor() {
        this.baseUrl = '/api/tokenidentification';
        this.identifyUrl = `${this.baseUrl}/identify`;
        this.analyzeUrl = `${this.baseUrl}/analyze`;
        this.similarityUrl = `${this.baseUrl}/similarity`;
        this.consistencyUrl = `${this.baseUrl}/validate-consistency`;
        this.statisticsUrl = `${this.baseUrl}/statistics`;
    }

    /**
     * Identify token from touch signature using backend pattern matching
     */
    async identifyToken(touches) {
        try {
            console.log(`🔍 Backend Pattern Matching: Processing ${touches.length} touches`);
            
            // Generate lightweight signature on frontend
            const signature = this.generateSignature(touches);
            
            // Send to backend for pattern matching
            const response = await fetch(this.identifyUrl, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    signature: signature,
                    confidenceThreshold: 70.0
                })
            });

            if (!response.ok) {
                const errorText = await response.text();
                throw new Error(`HTTP ${response.status}: ${errorText}`);
            }

            const result = await response.json();
            
            console.log(`📊 Backend identification result:`, {
                success: result.success,
                confidence: result.confidence,
                tokenName: result.matchedToken?.name,
                totalMatches: result.allMatches?.length || 0
            });

            return result;
        } catch (error) {
            console.error('❌ Error identifying token via backend:', error);
            return {
                success: false,
                message: 'Failed to identify token via backend',
                error: error.message
            };
        }
    }

    /**
     * Analyze pattern characteristics using backend
     */
    async analyzePattern(touches) {
        try {
            const signature = this.generateSignature(touches);
            
            const response = await fetch(this.analyzeUrl, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(signature)
            });

            if (!response.ok) {
                const errorText = await response.text();
                throw new Error(`HTTP ${response.status}: ${errorText}`);
            }

            return await response.json();
        } catch (error) {
            console.error('❌ Error analyzing pattern via backend:', error);
            return null;
        }
    }

    /**
     * Calculate similarity between two patterns using backend
     */
    async calculateSimilarity(signature1, signature2) {
        try {
            const response = await fetch(this.similarityUrl, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    signature1: signature1,
                    signature2: signature2
                })
            });

            if (!response.ok) {
                const errorText = await response.text();
                throw new Error(`HTTP ${response.status}: ${errorText}`);
            }

            return await response.json();
        } catch (error) {
            console.error('❌ Error calculating similarity via backend:', error);
            return null;
        }
    }

    /**
     * Validate pattern consistency for training using backend
     */
    async validateConsistency(signatures) {
        try {
            const response = await fetch(this.consistencyUrl, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(signatures)
            });

            if (!response.ok) {
                const errorText = await response.text();
                throw new Error(`HTTP ${response.status}: ${errorText}`);
            }

            return await response.json();
        } catch (error) {
            console.error('❌ Error validating consistency via backend:', error);
            return false;
        }
    }

    /**
     * Get pattern statistics for a token using backend
     */
    async getStatistics(tokenId) {
        try {
            const response = await fetch(`${this.statisticsUrl}/${tokenId}`, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json',
                }
            });

            if (!response.ok) {
                const errorText = await response.text();
                throw new Error(`HTTP ${response.status}: ${errorText}`);
            }

            return await response.json();
        } catch (error) {
            console.error('❌ Error getting statistics via backend:', error);
            return null;
        }
    }

    /**
     * Generate lightweight signature for backend processing
     * This only includes essential data, complex calculations are done on backend
     */
    generateSignature(touches) {
        try {
            // Basic touch data
            const touchData = touches.map(touch => ({
                clientX: touch.clientX,
                clientY: touch.clientY,
                identifier: touch.identifier,
                timestamp: touch.timestamp || Date.now()
            }));

            // Calculate basic distances for backend processing
            const distances = this.calculateBasicDistances(touches);
            
            // Create touch pattern data
            const touchPattern = {
                type: touches.length === 1 ? 'single' : 'multi',
                complexity: touches.length,
                distances: JSON.stringify(distances),
                avgDistance: distances.length > 0 ? distances.reduce((a, b) => a + b, 0) / distances.length : 0,
                minDistance: distances.length > 0 ? Math.min(...distances) : 0,
                maxDistance: distances.length > 0 ? Math.max(...distances) : 0
            };

            // Create multi-touch geometry data
            const multiTouchGeometry = touches.length > 1 ? this.calculateBasicGeometry(touches) : null;

            // Create touch properties data
            const touchProperties = this.calculateBasicProperties(touches);

            return {
                touchCount: touches.length,
                timestamp: Date.now(),
                originalTouches: JSON.stringify(touchData),
                touchPattern: touchPattern,
                multiTouchGeometry: multiTouchGeometry,
                touchProperties: touchProperties
            };
        } catch (error) {
            console.error('❌ Error generating signature:', error);
            return null;
        }
    }

    /**
     * Calculate basic distances between touch points
     */
    calculateBasicDistances(touches) {
        const distances = [];
        
        for (let i = 0; i < touches.length; i++) {
            for (let j = i + 1; j < touches.length; j++) {
                const dx = touches[j].clientX - touches[i].clientX;
                const dy = touches[j].clientY - touches[i].clientY;
                const distance = Math.sqrt(dx * dx + dy * dy);
                distances.push(distance);
            }
        }
        
        return distances;
    }

    /**
     * Calculate basic geometry for multi-touch patterns
     */
    calculateBasicGeometry(touches) {
        if (touches.length < 2) return null;

        const points = touches.map(t => ({ x: t.clientX, y: t.clientY }));
        
        // Calculate bounding box
        const minX = Math.min(...points.map(p => p.x));
        const maxX = Math.max(...points.map(p => p.x));
        const minY = Math.min(...points.map(p => p.y));
        const maxY = Math.max(...points.map(p => p.y));
        
        const width = maxX - minX;
        const height = maxY - minY;
        const area = width * height;
        
        // Calculate center
        const centerX = (minX + maxX) / 2;
        const centerY = (minY + maxY) / 2;
        
        // Calculate spread (average distance from center)
        const spread = points.reduce((sum, p) => {
            const dx = p.x - centerX;
            const dy = p.y - centerY;
            return sum + Math.sqrt(dx * dx + dy * dy);
        }, 0) / points.length;
        
        // Calculate density (points per unit area)
        const density = points.length / Math.max(area, 1);
        
        return {
            aspectRatio: height > 0 ? width / height : 1,
            boundingBoxWidth: width,
            boundingBoxHeight: height,
            boundingBoxArea: area,
            centerX: centerX,
            centerY: centerY,
            spread: spread,
            density: density
        };
    }

    /**
     * Calculate basic touch properties
     */
    calculateBasicProperties(touches) {
        const hasRadius = touches.some(t => t.radiusX !== undefined || t.radiusY !== undefined);
        const hasRotation = touches.some(t => t.rotationAngle !== undefined);
        
        let avgRadius = 0;
        let avgRotation = 0;
        let radiusVariance = 0;
        
        if (hasRadius) {
            const radii = touches.map(t => {
                const radiusX = t.radiusX || 0;
                const radiusY = t.radiusY || 0;
                return (radiusX + radiusY) / 2;
            }).filter(r => r > 0);
            
            if (radii.length > 0) {
                avgRadius = radii.reduce((a, b) => a + b, 0) / radii.length;
                radiusVariance = this.calculateVariance(radii);
            }
        }
        
        if (hasRotation) {
            const rotations = touches.map(t => t.rotationAngle || 0).filter(r => r !== 0);
            if (rotations.length > 0) {
                avgRotation = rotations.reduce((a, b) => a + b, 0) / rotations.length;
            }
        }
        
        return {
            hasRadius: hasRadius,
            hasRotation: hasRotation,
            avgRadius: avgRadius,
            avgRotation: avgRotation,
            radiusVariance: radiusVariance
        };
    }

    /**
     * Calculate variance for a set of values
     */
    calculateVariance(values) {
        if (values.length === 0) return 0;
        
        const mean = values.reduce((a, b) => a + b, 0) / values.length;
        const squaredDiffs = values.map(v => Math.pow(v - mean, 2));
        return squaredDiffs.reduce((a, b) => a + b, 0) / values.length;
    }

    /**
     * Display backend identification results
     */
    displayIdentificationResults(result) {
        if (!result.success) {
            console.log(`❌ Identification failed: ${result.message}`);
            return;
        }

        console.log(`✅ Token identified: ${result.matchedToken.name}`);
        console.log(`📊 Confidence: ${result.confidence}%`);
        console.log(`🔍 Total matches analyzed: ${result.allMatches.length}`);
        
        if (result.allMatches.length > 1) {
            console.log(`📋 All matches:`);
            result.allMatches.forEach((match, index) => {
                console.log(`  ${index + 1}. ${match.tokenName}: ${match.confidence}% confidence`);
                if (match.matchFactors.length > 0) {
                    console.log(`     Factors: ${match.matchFactors.join(', ')}`);
                }
            });
        }
    }
}

// Create global instance
window.backendPatternMatcher = new BackendPatternMatcher();
