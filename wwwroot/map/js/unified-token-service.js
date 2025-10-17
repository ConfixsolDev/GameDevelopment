/**
 * Unified Token Service - Single JavaScript service for all token operations
 * Uses the unified API for consistent behavior across all systems
 */
window.UnifiedTokenService = (function() {
    'use strict';

    const API_BASE_URL = '/api/UnifiedToken';

    /**
     * Identify token from touch points
     * @param {Array} touchPoints - Array of [x, y] coordinates
     * @param {number} confidenceThreshold - Minimum confidence threshold (default: 70)
     * @param {boolean} preferSimplified - Whether to prefer simplified system (default: true)
     * @returns {Promise<Object>} Identification result
     */
    async function identifyToken(touchPoints, confidenceThreshold = 70, preferSimplified = true) {
        try {
            console.log('UnifiedTokenService: Identifying token with', touchPoints.length, 'touch points');
            
            const response = await fetch(`${API_BASE_URL}/identify`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    touchPoints: touchPoints,
                    confidenceThreshold: confidenceThreshold,
                    preferSimplified: preferSimplified
                })
            });

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const result = await response.json();
            console.log('UnifiedTokenService: Identification result:', result);
            return result;
        } catch (error) {
            console.error('UnifiedTokenService: Error identifying token:', error);
            return {
                success: false,
                message: 'Error identifying token: ' + error.message,
                systemUsed: 'error'
            };
        }
    }

    /**
     * Save token (create or update)
     * @param {Object} tokenData - Token data to save
     * @returns {Promise<Object>} Save result
     */
    async function saveToken(tokenData) {
        try {
            console.log('UnifiedTokenService: Saving token:', tokenData.name);
            
            const response = await fetch(`${API_BASE_URL}/save`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(tokenData)
            });

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const result = await response.json();
            console.log('UnifiedTokenService: Save result:', result);
            return result;
        } catch (error) {
            console.error('UnifiedTokenService: Error saving token:', error);
            return {
                success: false,
                message: 'Error saving token: ' + error.message
            };
        }
    }

    /**
     * Get token by ID
     * @param {number} tokenId - Token ID
     * @returns {Promise<Object>} Token information
     */
    async function getToken(tokenId) {
        try {
            console.log('UnifiedTokenService: Getting token:', tokenId);
            
            const response = await fetch(`${API_BASE_URL}/${tokenId}`, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json',
                }
            });

            if (!response.ok) {
                if (response.status === 404) {
                    return null;
                }
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const result = await response.json();
            console.log('UnifiedTokenService: Token info:', result);
            return result;
        } catch (error) {
            console.error('UnifiedTokenService: Error getting token:', error);
            return null;
        }
    }

    /**
     * Get all active tokens
     * @returns {Promise<Array>} Array of token information
     */
    async function getAllTokens() {
        try {
            console.log('UnifiedTokenService: Getting all tokens');
            
            const response = await fetch(`${API_BASE_URL}/all`, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json',
                }
            });

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const result = await response.json();
            console.log('UnifiedTokenService: All tokens:', result);
            return result;
        } catch (error) {
            console.error('UnifiedTokenService: Error getting all tokens:', error);
            return [];
        }
    }

    /**
     * Delete token
     * @param {number} tokenId - Token ID to delete
     * @returns {Promise<Object>} Delete result
     */
    async function deleteToken(tokenId) {
        try {
            console.log('UnifiedTokenService: Deleting token:', tokenId);
            
            const response = await fetch(`${API_BASE_URL}/${tokenId}`, {
                method: 'DELETE',
                headers: {
                    'Content-Type': 'application/json',
                }
            });

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const result = await response.json();
            console.log('UnifiedTokenService: Delete result:', result);
            return result;
        } catch (error) {
            console.error('UnifiedTokenService: Error deleting token:', error);
            return {
                success: false,
                message: 'Error deleting token: ' + error.message
            };
        }
    }

    /**
     * Test token identification with sample data
     * @param {Array} touchPoints - Array of [x, y] coordinates
     * @param {number} confidenceThreshold - Minimum confidence threshold
     * @param {boolean} preferSimplified - Whether to prefer simplified system
     * @returns {Promise<Object>} Test result
     */
    async function testIdentification(touchPoints, confidenceThreshold = 70, preferSimplified = true) {
        try {
            console.log('UnifiedTokenService: Testing identification with', touchPoints.length, 'touch points');
            
            const response = await fetch(`${API_BASE_URL}/test`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    touchPoints: touchPoints,
                    confidenceThreshold: confidenceThreshold,
                    preferSimplified: preferSimplified
                })
            });

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const result = await response.json();
            console.log('UnifiedTokenService: Test result:', result);
            return result;
        } catch (error) {
            console.error('UnifiedTokenService: Error testing identification:', error);
            return {
                success: false,
                message: 'Error testing identification: ' + error.message,
                systemUsed: 'error'
            };
        }
    }

    /**
     * Create a test token for development/testing
     * @param {Object} testData - Test token data
     * @returns {Promise<Object>} Create result
     */
    async function createTestToken(testData) {
        try {
            console.log('UnifiedTokenService: Creating test token:', testData.name);
            
            const response = await fetch(`${API_BASE_URL}/create-test`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(testData)
            });

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const result = await response.json();
            console.log('UnifiedTokenService: Create test result:', result);
            return result;
        } catch (error) {
            console.error('UnifiedTokenService: Error creating test token:', error);
            return {
                success: false,
                message: 'Error creating test token: ' + error.message
            };
        }
    }

    /**
     * Helper function to convert touch events to coordinate array
     * @param {Array} touches - Touch events or coordinate objects
     * @returns {Array} Array of [x, y] coordinates
     */
    function convertTouchesToCoordinates(touches) {
        if (!touches || touches.length === 0) {
            return [];
        }

        return touches.map(touch => {
            if (touch.clientX !== undefined && touch.clientY !== undefined) {
                return [touch.clientX, touch.clientY];
            } else if (touch.x !== undefined && touch.y !== undefined) {
                return [touch.x, touch.y];
            } else if (Array.isArray(touch) && touch.length >= 2) {
                return [touch[0], touch[1]];
            }
            return null;
        }).filter(coord => coord !== null);
    }

    /**
     * Helper function to create a simple test pattern
     * @param {string} patternType - Type of pattern ('triangle', 'square', 'line', 'custom')
     * @param {Object} options - Pattern options (center, size, etc.)
     * @returns {Array} Array of [x, y] coordinates
     */
    function createTestPattern(patternType = 'triangle', options = {}) {
        const center = options.center || { x: 400, y: 300 };
        const size = options.size || 100;
        const points = [];

        switch (patternType.toLowerCase()) {
            case 'triangle':
                points.push([center.x, center.y - size]);
                points.push([center.x - size * 0.866, center.y + size * 0.5]);
                points.push([center.x + size * 0.866, center.y + size * 0.5]);
                break;
            case 'square':
                points.push([center.x - size/2, center.y - size/2]);
                points.push([center.x + size/2, center.y - size/2]);
                points.push([center.x + size/2, center.y + size/2]);
                points.push([center.x - size/2, center.y + size/2]);
                break;
            case 'line':
                points.push([center.x - size/2, center.y]);
                points.push([center.x + size/2, center.y]);
                break;
            case 'pentagon':
                for (let i = 0; i < 5; i++) {
                    const angle = (i * 2 * Math.PI) / 5 - Math.PI / 2;
                    const x = center.x + size * Math.cos(angle);
                    const y = center.y + size * Math.sin(angle);
                    points.push([x, y]);
                }
                break;
            default:
                // Custom pattern - use provided points or default triangle
                if (options.points && Array.isArray(options.points)) {
                    return options.points;
                }
                return createTestPattern('triangle', options);
        }

        return points;
    }

    // Public API
    return {
        identifyToken,
        saveToken,
        getToken,
        getAllTokens,
        deleteToken,
        testIdentification,
        createTestToken,
        convertTouchesToCoordinates,
        createTestPattern
    };
})();

// Make it globally available
window.unifiedTokenService = window.UnifiedTokenService;
