/**
 * API Service for database operations
 * Replaces localStorage operations with database API calls
 */
class ApiService {
    constructor() {
        this.baseUrl = '/api';
        this.tokensUrl = `${this.baseUrl}/tokens`;
        this.tokenManagementUrl = `${this.baseUrl}/tokenmanagement`;
        this.mapMarkersUrl = `${this.baseUrl}/mapmarker`;
    }

    /**
     * Generic API call method
     */
    async apiCall(url, options = {}) {
        const defaultOptions = {
            headers: {
                'Content-Type': 'application/json',
            },
        };

        const config = { ...defaultOptions, ...options };

        try {
            const response = await fetch(url, config);

            if (!response.ok) {
                const errorText = await response.text();
                throw new Error(`HTTP ${response.status}: ${errorText}`);
            }

            // Handle empty responses (like DELETE operations)
            if (response.status === 204) {
                return null;
            }

            return await response.json();
        } catch (error) {
            console.error(`API call failed for ${url}:`, error);
            throw error;
        }
    }

    // ==================== TOKEN OPERATIONS ====================

    /**
     * Load all tokens from database
     */
    async loadTokens() {
        try {
            const tokens = await this.apiCall(this.tokenManagementUrl);
            console.log(`📡 Loaded ${tokens.length} tokens from database`);
            return tokens;
        } catch (error) {
            console.error('Error loading tokens from database:', error);
            return [];
        }
    }

    /**
     * Save a single token to database
     */
    async saveToken(token) {
        try {
            // Convert to complete token request format
            const completeTokenRequest = this.convertToCompleteTokenRequest(token);

            const savedToken = await this.apiCall(`${this.tokenManagementUrl}/create-complete`, {
                method: 'POST',
                body: JSON.stringify(completeTokenRequest)
            });
            console.log(`📡 Saved token "${token.name}" to database`);
            return savedToken;
        } catch (error) {
            console.error('Error saving token to database:', error);
            throw error;
        }
    }

    /**
     * Update an existing token
     */
    async updateToken(token) {
        try {
            await this.apiCall(`${this.tokensUrl}/${token.id}`, {
                method: 'PUT',
                body: JSON.stringify(token)
            });
            console.log(`📡 Updated token "${token.name}" in database`);
        } catch (error) {
            console.error('Error updating token in database:', error);
            throw error;
        }
    }

    /**
     * Delete a token from database
     */
    async deleteToken(tokenId) {
        try {
            await this.apiCall(`${this.tokenManagementUrl}/${tokenId}`, {
                method: 'DELETE'
            });
            console.log(`📡 Deleted token ${tokenId} from database`);
        } catch (error) {
            console.error('Error deleting token from database:', error);
            throw error;
        }
    }

    /**
     * Delete all tokens from database
     */
    async deleteAllTokens() {
        try {
            const tokens = await this.loadTokens();
            const deletePromises = tokens.map(token => this.deleteToken(token.id));
            await Promise.all(deletePromises);
            console.log(`📡 Deleted all ${tokens.length} tokens from database`);
        } catch (error) {
            console.error('Error deleting all tokens from database:', error);
            throw error;
        }
    }

    /**
     * Bulk import tokens to database
     */
    async bulkImportTokens(tokens) {
        try {
            const results = [];

            // Process each token individually since we need to create complete token requests
            for (const token of tokens) {
                try {
                    const completeTokenRequest = this.convertToCompleteTokenRequest(token);
                    const savedToken = await this.apiCall(`${this.tokenManagementUrl}/create-complete`, {
                        method: 'POST',
                        body: JSON.stringify(completeTokenRequest)
                    });
                    results.push({ token: token.name, Success: true, Id: savedToken.id });
                } catch (error) {
                    results.push({ token: token.name, Success: false, Error: error.message });
                }
            }

            console.log(`📡 Bulk imported tokens to database:`, results);
            return results;
        } catch (error) {
            console.error('Error bulk importing tokens to database:', error);
            throw error;
        }
    }

    /**
     * Identify token from signature
     */
    async identifyToken(signature, confidenceThreshold = 70.0) {
        try {
            const result = await this.apiCall(`${this.tokensUrl}/identify?confidenceThreshold=${confidenceThreshold}`, {
                method: 'POST',
                body: JSON.stringify(signature)
            });
            return result;
        } catch (error) {
            console.error('Error identifying token:', error);
            return null;
        }
    }

    // ==================== MAP MARKER OPERATIONS ====================

    /**
     * Load all map markers from database
     */
    async loadMapMarkers() {
        try {
            debugger
            const markers = await this.apiCall(this.mapMarkersUrl);
            console.log(`📡 Loaded ${markers.length} map markers from database`);
            return markers;
        } catch (error) {
            console.error('Error loading map markers from database:', error);
            return [];
        }
    }

    /**
     * Load map markers by token ID
     */
    async loadMapMarkersByToken(tokenId) {
        try {
            const markers = await this.apiCall(`${this.mapMarkersUrl}/by-token/${tokenId}`);
            console.log(`📡 Loaded ${markers.length} map markers for token ${tokenId} from database`);
            return markers;
        } catch (error) {
            console.error('Error loading map markers for token from database:', error);
            return [];
        }
    }

    /**
     * Save a single map marker to database
     */
    async saveMapMarker(marker) {
        try {
            const savedMarker = await this.apiCall(this.mapMarkersUrl, {
                method: 'POST',
                body: JSON.stringify(marker)
            });
            console.log(`📡 Saved map marker ${marker.id} to database`);
            return savedMarker;
        } catch (error) {
            console.error('Error saving map marker to database:', error);
            throw error;
        }
    }

    /**
     * Update an existing map marker
     */
    async updateMapMarker(marker) {
        try {
            await this.apiCall(`${this.mapMarkersUrl}/${marker.id}`, {
                method: 'PUT',
                body: JSON.stringify(marker)
            });
            console.log(`📡 Updated map marker ${marker.id} in database`);
        } catch (error) {
            console.error('Error updating map marker in database:', error);
            throw error;
        }
    }

    /**
     * Delete a map marker from database
     */
    async deleteMapMarker(markerId) {
        try {
            await this.apiCall(`${this.mapMarkersUrl}/${markerId}`, {
                method: 'DELETE'
            });
            console.log(`📡 Deleted map marker ${markerId} from database`);
        } catch (error) {
            console.error('Error deleting map marker from database:', error);
            throw error;
        }
    }

    /**
     * Delete all map markers for a specific token
     */
    async deleteMapMarkersByToken(tokenId) {
        try {
            const result = await this.apiCall(`${this.mapMarkersUrl}/by-token/${tokenId}`, {
                method: 'DELETE'
            });
            console.log(`📡 Deleted ${result.deletedCount} map markers for token ${tokenId} from database`);
            return result;
        } catch (error) {
            console.error('Error deleting map markers for token from database:', error);
            throw error;
        }
    }

    /**
     * Bulk save map markers to database
     */
    async bulkSaveMapMarkers(markers) {
        try {
            const results = await this.apiCall(`${this.mapMarkersUrl}/bulk`, {
                method: 'POST',
                body: JSON.stringify(markers)
            });
            console.log(`📡 Bulk saved map markers to database:`, results);
            return results;
        } catch (error) {
            console.error('Error bulk saving map markers to database:', error);
            throw error;
        }
    }

    // ==================== UTILITY METHODS ====================
    isJSON(value) {
        if (typeof value !== "string") {
            return false;
        }
        try {
            const parsed = JSON.parse(value);
            // Optional: ensure it’s an object/array, not just a primitive
            return typeof parsed === "object" && parsed !== null;
        } catch (e) {
            return false;
        }
    }
    ensureJSONString(value) {
        if (typeof value === "string" && this.isJSON(value)) {
            // already a valid JSON string (array or object)
            return value;
        }
        if (Array.isArray(value) || (typeof value === "object" && value !== null)) {
            // raw object or array, needs stringifying
            return JSON.stringify(value);
        }
        return null; // not valid input
    }
    toJSONObject(value) {
        if (typeof value === "string" && this.isJSON(value)) {
            return JSON.parse(value); // parse string into object/array
        }
        if (Array.isArray(value) || (typeof value === "object" && value !== null)) {
            return value; // already parsed
        }
        return null;
    }

    /**
     * Convert JavaScript token to complete token request format
     */
    convertToCompleteTokenRequest(jsToken) {
        const dbToken = this.convertTokenToDbFormat(jsToken);
        const dbSignature = jsToken.signature ? this.convertSignatureToDbFormat(jsToken.signature, jsToken.id) : null;

        return {
            token: dbToken,
            signature: dbSignature,
            stability: dbSignature?.stability,
            touchGeometry: dbSignature?.touchProperties,
            touchPattern: dbSignature?.touchPattern,
            multiTouchGeometry: dbSignature?.multiTouchGeometry
        };
    }

    /**
     * Convert JavaScript token format to database format
     */
    convertTokenToDbFormat(jsToken) {
        return {
            id: jsToken.id,
            name: jsToken.name,
            createdAt: jsToken.createdAt,
            trainingConsistency: jsToken.trainingConsistency?.avg || 0,
            description: jsToken.description || null,
            category: jsToken.category || null,
            isActive: true,
            usageCount: 0,
            lastUsed: null,
            createdBy: null,
            notes: jsToken.notes || null,
            signature: jsToken.signature ? this.convertSignatureToDbFormat(jsToken.signature, jsToken.id) : null
        };
    }

    /**
     * Convert JavaScript signature format to database format
     */
    convertSignatureToDbFormat(jsSignature, tokenId) {

        debugger
        let originalTouches = this.ensureJSONString(jsSignature.originalTouches)



        return {
            tokenId: tokenId,
            touchCount: jsSignature.touchCount || 0,
            timestamp: jsSignature.timestamp || Date.now(),
            tokenHash: jsSignature.tokenHash || null,
            originalTouches: originalTouches,
            stability: jsSignature.stability ? this.convertStabilityToDbFormat(jsSignature.stability) : null,
            touchProperties: jsSignature.touchProperties ? this.convertTouchGeometryToDbFormat(jsSignature.touchProperties) : null,
            touchPattern: jsSignature.touchPattern ? this.convertTouchPatternToDbFormat(jsSignature.touchPattern) : null,
            multiTouchGeometry: jsSignature.multiTouchGeometry ? this.convertMultiTouchGeometryToDbFormat(jsSignature.multiTouchGeometry) : null
        };
    }

    /**
     * Convert stability info to database format
     */
    convertStabilityToDbFormat(jsStability) {
        return {
            isStabilized: jsStability.isStabilized || false,
            generatedAt: jsStability.generatedAt || Date.now(),
            sampleCount: jsStability.sampleCount || 0
        };
    }

    /**
     * Convert touch geometry to database format
     */
    convertTouchGeometryToDbFormat(jsTouchGeometry) {

        let radiusValues = this.ensureJSONString(jsTouchGeometry.radiusValues)
        let rotationValues = this.ensureJSONString(jsTouchGeometry.rotationValues)

        return {
            hasRadius: jsTouchGeometry.hasRadius || false,
            hasRotation: jsTouchGeometry.hasRotation || false,
            radiusValues: radiusValues,
            rotationValues: rotationValues,
            avgRadius: jsTouchGeometry.avgRadius || 0,
            avgRotation: jsTouchGeometry.avgRotation || 0,
            radiusVariance: jsTouchGeometry.radiusVariance || 0
        };
    }

    /**
     * Convert touch pattern to database format
     */
    convertTouchPatternToDbFormat(jsTouchPattern) {

        let distances = this.ensureJSONString(jsTouchPattern.distances)
        let distancePairs = this.ensureJSONString(jsTouchPattern.distancePairs)
        let geometricCenter = this.ensureJSONString(jsTouchPattern.geometricCenter)


        return {
            type: jsTouchPattern.type || 'single',
            complexity: jsTouchPattern.complexity || 0,
            distances: distances,
            distancePairs: distancePairs,
            avgDistance: jsTouchPattern.avgDistance || 0,
            minDistance: jsTouchPattern.minDistance || 0,
            maxDistance: jsTouchPattern.maxDistance || 0,
            distanceRange: jsTouchPattern.distanceRange || 0,
            distanceVariance: jsTouchPattern.distanceVariance || 0,
            distanceSignature: jsTouchPattern.distanceSignature || null,
            angleSpread: jsTouchPattern.angleSpread || 0,
            geometricCenter: geometricCenter
        };
    }

    /**
     * Convert multi-touch geometry to database format
     */
    convertMultiTouchGeometryToDbFormat(jsMultiTouchGeometry) {
        return {
            aspectRatio: jsMultiTouchGeometry.aspectRatio || 0,
            boundingBoxWidth: jsMultiTouchGeometry.boundingBoxWidth || 0,
            boundingBoxHeight: jsMultiTouchGeometry.boundingBoxHeight || 0,
            boundingBoxArea: jsMultiTouchGeometry.boundingBoxArea || 0,
            centerX: jsMultiTouchGeometry.centerX || 0,
            centerY: jsMultiTouchGeometry.centerY || 0,
            spread: jsMultiTouchGeometry.spread || 0,
            density: jsMultiTouchGeometry.density || 0
        };
    }

    /**
     * Convert database token format to JavaScript format
     */
    convertTokenFromDbFormat(dbToken) {
        return {
            id: dbToken.id,
            name: dbToken.name,
            createdAt: dbToken.createdAt,
            trainingConsistency: {
                avg: dbToken.trainingConsistency || 0,
                min: 0, // These would need to be calculated or stored separately
                max: 0
            },
            description: dbToken.description,
            category: dbToken.category,
            notes: dbToken.notes,
            signature: dbToken.signature ? this.convertSignatureFromDbFormat(dbToken.signature) : null
        };
    }

    /**
     * Convert database signature format to JavaScript format
     */

  

    convertSignatureFromDbFormat(dbSignature) {

        debugger
        let originalTouches = this.toJSONObject(dbSignature.originalTouches); 

        return {
            touchCount: dbSignature.touchCount || 0,
            timestamp: dbSignature.timestamp || Date.now(),
            tokenHash: dbSignature.tokenHash,
            originalTouches: originalTouches,
            stability: dbSignature.stability ? this.convertStabilityFromDbFormat(dbSignature.stability) : null,
            touchProperties: dbSignature.touchProperties ? this.convertTouchGeometryFromDbFormat(dbSignature.touchProperties) : null,
            touchPattern: dbSignature.touchPattern ? this.convertTouchPatternFromDbFormat(dbSignature.touchPattern) : null,
            multiTouchGeometry: dbSignature.multiTouchGeometry ? this.convertMultiTouchGeometryFromDbFormat(dbSignature.multiTouchGeometry) : null
        };
    }

    /**
     * Convert stability info from database format
     */
    convertStabilityFromDbFormat(dbStability) {
        return {
            isStabilized: dbStability.isStabilized || false,
            generatedAt: dbStability.generatedAt || Date.now(),
            sampleCount: dbStability.sampleCount || 0
        };
    }

    /**
     * Convert touch geometry from database format
     */
    convertTouchGeometryFromDbFormat(dbTouchGeometry) {

        let radiusValues = this.toJSONObject(dbTouchGeometry.radiusValues); 
        let rotationValues = this.toJSONObject(dbTouchGeometry.rotationValues); 
        return {
            hasRadius: dbTouchGeometry.hasRadius || false,
            hasRotation: dbTouchGeometry.hasRotation || false,
            radiusValues: radiusValues,
            rotationValues: rotationValues,
            avgRadius: dbTouchGeometry.avgRadius || 0,
            avgRotation: dbTouchGeometry.avgRotation || 0,
            radiusVariance: dbTouchGeometry.radiusVariance || 0
        };
    }

    /**
     * Convert touch pattern from database format
     */
    convertTouchPatternFromDbFormat(dbTouchPattern) {

      
        let distances = this.toJSONObject(dbTouchPattern.distances);
        let distancePairs = this.toJSONObject(dbTouchPattern.distancePairs); 
        let geometricCenter = this.toJSONObject(dbTouchPattern.geometricCenter); 


        return {
            type: dbTouchPattern.type || 'single',
            complexity: dbTouchPattern.complexity || 0,
            distances: distances,
            distancePairs: distancePairs,
            avgDistance: dbTouchPattern.avgDistance || 0,
            minDistance: dbTouchPattern.minDistance || 0,
            maxDistance: dbTouchPattern.maxDistance || 0,
            distanceRange: dbTouchPattern.distanceRange || 0,
            distanceVariance: dbTouchPattern.distanceVariance || 0,
            distanceSignature: dbTouchPattern.distanceSignature,
            angleSpread: dbTouchPattern.angleSpread || 0,
            geometricCenter: geometricCenter
        };
    }

    /**
     * Convert multi-touch geometry from database format
     */
    convertMultiTouchGeometryFromDbFormat(dbMultiTouchGeometry) {
        return {
            aspectRatio: dbMultiTouchGeometry.aspectRatio || 0,
            boundingBoxWidth: dbMultiTouchGeometry.boundingBoxWidth || 0,
            boundingBoxHeight: dbMultiTouchGeometry.boundingBoxHeight || 0,
            boundingBoxArea: dbMultiTouchGeometry.boundingBoxArea || 0,
            centerX: dbMultiTouchGeometry.centerX || 0,
            centerY: dbMultiTouchGeometry.centerY || 0,
            spread: dbMultiTouchGeometry.spread || 0,
            density: dbMultiTouchGeometry.density || 0
        };
    }

    /**
     * Convert JavaScript map marker format to database format
     */
    convertMapMarkerToDbFormat(jsMarker) {
        return {
            id: jsMarker.id,
            tokenId: jsMarker.tokenId,
            location: JSON.stringify(jsMarker.location),
            createdAt: jsMarker.createdAt,
            tokenName: jsMarker.tokenName
        };
    }

    /**
     * Convert database map marker format to JavaScript format
     */
    convertMapMarkerFromDbFormat(dbMarker) {
        return {
            id: dbMarker.id,
            tokenId: dbMarker.tokenId,
            location: JSON.parse(dbMarker.location || '{}'),
            createdAt: dbMarker.createdAt,
            tokenName: dbMarker.tokenName
        };
    }



   
}

// Create global instance
window.apiService = new ApiService();

