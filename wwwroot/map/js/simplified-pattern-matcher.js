/**
 * Simplified Pattern Matcher - Frontend integration for simplified geometric pattern matching
 * Only uses distances, angles, and center point for token identification
 */
class SimplifiedPatternMatcher {
    constructor() {
        this.baseUrl = '/api/simplifiedtoken';
        this.identifyUrl = `${this.baseUrl}/identify`;
        this.calculateUrl = `${this.baseUrl}/calculate-geometric-data`;
        this.similarityUrl = `${this.baseUrl}/similarity`;
        this.consistencyUrl = `${this.baseUrl}/validate-consistency`;
    }

    /**
     * Identify token from touch points using simplified geometric matching
     */
    async identifyToken(touches) {
        try {
            console.log(`🔍 Simplified Pattern Matching: Processing ${touches.length} touches`);
            
            // Convert touches to coordinate array
            const touchPoints = touches.map(touch => [touch.clientX, touch.clientY]);
            
            // Calculate geometric data
            const geometricData = await this.calculateGeometricData(touchPoints);
            
            // Send to backend for pattern matching
            const response = await fetch(this.identifyUrl, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    geometricData: geometricData,
                    confidenceThreshold: 70.0
                })
            });

            if (!response.ok) {
                const errorText = await response.text();
                throw new Error(`HTTP ${response.status}: ${errorText}`);
            }

            const result = await response.json();
            
            console.log(`📊 Simplified identification result:`, {
                success: result.success,
                confidence: result.confidence,
                tokenName: result.matchedToken?.name,
                totalMatches: result.allMatches?.length || 0
            });

            return result;
        } catch (error) {
            console.error('❌ Error identifying token via simplified backend:', error);
            return {
                success: false,
                message: 'Failed to identify token via simplified backend',
                error: error.message
            };
        }
    }

    /**
     * Calculate geometric data from touch points
     */
    async calculateGeometricData(touchPoints) {
        try {
            const response = await fetch(this.calculateUrl, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    touchPoints: touchPoints
                })
            });

            if (!response.ok) {
                const errorText = await response.text();
                throw new Error(`HTTP ${response.status}: ${errorText}`);
            }

            return await response.json();
        } catch (error) {
            console.error('❌ Error calculating geometric data:', error);
            return null;
        }
    }

    /**
     * Calculate similarity between two geometric data sets
     */
    async calculateSimilarity(data1, data2) {
        try {
            const response = await fetch(this.similarityUrl, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    data1: data1,
                    data2: data2
                })
            });

            if (!response.ok) {
                const errorText = await response.text();
                throw new Error(`HTTP ${response.status}: ${errorText}`);
            }

            return await response.json();
        } catch (error) {
            console.error('❌ Error calculating similarity:', error);
            return null;
        }
    }

    /**
     * Validate pattern consistency for training
     */
    async validateConsistency(geometricDataArray) {
        try {
            const response = await fetch(this.consistencyUrl, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(geometricDataArray)
            });

            if (!response.ok) {
                const errorText = await response.text();
                throw new Error(`HTTP ${response.status}: ${errorText}`);
            }

            return await response.json();
        } catch (error) {
            console.error('❌ Error validating consistency:', error);
            return false;
        }
    }

    /**
     * Display simplified identification results
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
                console.log(`     Distance: ${match.distanceSimilarity}%, Angle: ${match.angleSimilarity}%, Center: ${match.centerSimilarity}%`);
                if (match.matchFactors.length > 0) {
                    console.log(`     Factors: ${match.matchFactors.join(', ')}`);
                }
            });
        }
    }

    /**
     * Create a token with simplified geometric data
     */
    async createToken(tokenName, touches) {
        try {
            console.log(`🔧 Creating simplified token: ${tokenName}`);
            
            // Convert touches to coordinate array
            const touchPoints = touches.map(touch => [touch.clientX, touch.clientY]);
            
            // Calculate geometric data
            const geometricData = await this.calculateGeometricData(touchPoints);
            
            if (!geometricData) {
                throw new Error('Failed to calculate geometric data');
            }

            // Create token signature
            const tokenSignature = {
                touchCount: touches.length,
                distances: JSON.stringify(geometricData.distances),
                angles: JSON.stringify(geometricData.angles),
                center: JSON.stringify(geometricData.center),
                createdAt: new Date().toISOString()
            };

            console.log(`📊 Token signature created:`, {
                touchCount: tokenSignature.touchCount,
                distances: geometricData.distances,
                angles: geometricData.angles,
                center: geometricData.center
            });

            return {
                name: tokenName,
                signature: tokenSignature,
                geometricData: geometricData
            };
        } catch (error) {
            console.error('❌ Error creating simplified token:', error);
            return null;
        }
    }

    /**
     * Validate touch points for token creation
     */
    validateTouchPoints(touches) {
        if (!touches || touches.length < 2) {
            return { valid: false, message: 'At least 2 touch points are required' };
        }

        if (touches.length > 5) {
            return { valid: false, message: 'Maximum 5 touch points allowed' };
        }

        // Check if all touches have valid coordinates
        for (let i = 0; i < touches.length; i++) {
            const touch = touches[i];
            if (typeof touch.clientX !== 'number' || typeof touch.clientY !== 'number' ||
                isNaN(touch.clientX) || isNaN(touch.clientY)) {
                return { valid: false, message: `Touch ${i + 1} has invalid coordinates` };
            }
        }

        return { valid: true, message: 'Touch points are valid' };
    }
}

// Create global instance
window.simplifiedPatternMatcher = new SimplifiedPatternMatcher();
