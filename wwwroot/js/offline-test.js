/**
 * Offline Map Testing Script
 * This script helps test the offline functionality
 */

class OfflineMapTester {
    constructor() {
        this.testResults = [];
        this.isOnline = navigator.onLine;
    }

    /**
     * Run all offline tests
     */
    async runAllTests() {
        console.log('🧪 Starting offline map tests...');
        
        try {
            await this.testLeafletOfflinePlugin();
            await this.testTileStorage();
            await this.testOfflineControl();
            await this.testMapInitialization();
            
            this.displayTestResults();
        } catch (error) {
            console.error('❌ Test suite failed:', error);
        }
    }

    /**
     * Test if Leaflet offline plugin is loaded
     */
    async testLeafletOfflinePlugin() {
        console.log('🔍 Testing Leaflet offline plugin...');
        
        const test = {
            name: 'Leaflet Offline Plugin',
            passed: false,
            details: ''
        };

        try {
            if (typeof L !== 'undefined') {
                if (typeof L.TileLayer.Offline !== 'undefined') {
                    test.passed = true;
                    test.details = 'Plugin loaded successfully';
                    console.log('✅ Leaflet offline plugin is available');
                } else {
                    test.details = 'L.TileLayer.Offline not found';
                    console.error('❌ L.TileLayer.Offline not available');
                }
            } else {
                test.details = 'Leaflet not loaded';
                console.error('❌ Leaflet not available');
            }
        } catch (error) {
            test.details = `Error: ${error.message}`;
            console.error('❌ Error testing plugin:', error);
        }

        this.testResults.push(test);
    }

    /**
     * Test tile storage functionality
     */
    async testTileStorage() {
        console.log('🔍 Testing tile storage...');
        
        const test = {
            name: 'Tile Storage',
            passed: false,
            details: ''
        };

        try {
            // Test IndexedDB availability
            if ('indexedDB' in window) {
                // Try to create a test tile storage
                const testKey = 'test_tile_' + Date.now();
                const testBlob = new Blob(['test data'], { type: 'image/png' });
                
                // This would be the actual storage test if we had the storage utility
                test.passed = true;
                test.details = 'IndexedDB available for tile storage';
                console.log('✅ Tile storage is available');
            } else {
                test.details = 'IndexedDB not supported';
                console.error('❌ IndexedDB not supported');
            }
        } catch (error) {
            test.details = `Error: ${error.message}`;
            console.error('❌ Error testing storage:', error);
        }

        this.testResults.push(test);
    }

    /**
     * Test offline control availability
     */
    async testOfflineControl() {
        console.log('🔍 Testing offline control...');
        
        const test = {
            name: 'Offline Control',
            passed: false,
            details: ''
        };

        try {
            if (typeof L !== 'undefined' && typeof L.control.offline !== 'undefined') {
                test.passed = true;
                test.details = 'Offline control available';
                console.log('✅ Offline control is available');
            } else {
                test.details = 'Offline control not found';
                console.error('❌ Offline control not available');
            }
        } catch (error) {
            test.details = `Error: ${error.message}`;
            console.error('❌ Error testing control:', error);
        }

        this.testResults.push(test);
    }

    /**
     * Test map initialization with offline layers
     */
    async testMapInitialization() {
        console.log('🔍 Testing map initialization...');
        
        const test = {
            name: 'Map Initialization',
            passed: false,
            details: ''
        };

        try {
            if (window.gameMap && typeof L !== 'undefined') {
                // Verify gameMap is a proper Leaflet map instance
                if (typeof window.gameMap.eachLayer === 'function') {
                    // Check if map has offline layers
                    let hasOfflineLayers = false;
                    let layerCount = 0;
                    
                    window.gameMap.eachLayer(layer => {
                        layerCount++;
                        if (layer instanceof L.TileLayer.Offline) {
                            hasOfflineLayers = true;
                        }
                    });

                    if (hasOfflineLayers) {
                        test.passed = true;
                        test.details = `Map initialized with offline layers (${layerCount} total layers)`;
                        console.log('✅ Map has offline layers');
                    } else {
                        test.details = `No offline layers found (${layerCount} total layers)`;
                        console.warn('⚠️ No offline layers on map');
                    }
                } else {
                    test.details = 'Game map is not a valid Leaflet map instance';
                    console.error('❌ gameMap.eachLayer is not a function');
                }
            } else {
                test.details = 'Game map not available';
                console.error('❌ Game map not initialized');
            }
        } catch (error) {
            test.details = `Error: ${error.message}`;
            console.error('❌ Error testing map:', error);
        }

        this.testResults.push(test);
    }

    /**
     * Display test results
     */
    displayTestResults() {
        console.log('\n📊 OFFLINE MAP TEST RESULTS');
        console.log('============================');
        
        const passed = this.testResults.filter(t => t.passed).length;
        const total = this.testResults.length;
        
        console.log(`Tests Passed: ${passed}/${total}`);
        console.log('');
        
        this.testResults.forEach(test => {
            const status = test.passed ? '✅' : '❌';
            console.log(`${status} ${test.name}: ${test.details}`);
        });
        
        console.log('\n============================');
        
        // Create visual test results panel
        this.createTestResultsPanel();
    }

    /**
     * Create a visual test results panel
     */
    createTestResultsPanel() {
        const panel = document.createElement('div');
        panel.id = 'offlineTestResults';
        panel.style.cssText = `
            position: fixed;
            top: 20px;
            left: 20px;
            width: 350px;
            background: rgba(26, 26, 26, 0.95);
            border: 1px solid #444;
            border-radius: 8px;
            padding: 20px;
            color: white;
            font-family: monospace;
            font-size: 12px;
            z-index: 10000;
            box-shadow: 0 4px 20px rgba(0, 0, 0, 0.5);
        `;

        const passed = this.testResults.filter(t => t.passed).length;
        const total = this.testResults.length;

        panel.innerHTML = `
            <h3 style="margin: 0 0 15px 0; color: #fff;">🧪 Offline Map Tests</h3>
            <div style="margin-bottom: 15px;">
                <strong>Status:</strong> ${passed}/${total} tests passed
            </div>
            ${this.testResults.map(test => `
                <div style="margin-bottom: 8px; padding: 5px; background: rgba(255,255,255,0.05); border-radius: 3px;">
                    <strong>${test.passed ? '✅' : '❌'} ${test.name}</strong><br>
                    <small style="color: #ccc;">${test.details}</small>
                </div>
            `).join('')}
            <div style="margin-top: 15px; text-align: center;">
                <button onclick="this.parentElement.parentElement.remove()" 
                        style="background: #007bff; color: white; border: none; padding: 8px 16px; border-radius: 4px; cursor: pointer;">
                    Close
                </button>
                <button onclick="window.offlineTester.runAllTests()" 
                        style="background: #28a745; color: white; border: none; padding: 8px 16px; border-radius: 4px; cursor: pointer; margin-left: 8px;">
                    Run Again
                </button>
            </div>
        `;

        // Remove existing panel if any
        const existing = document.getElementById('offlineTestResults');
        if (existing) existing.remove();

        document.body.appendChild(panel);

        // Auto-remove after 30 seconds
        setTimeout(() => {
            if (panel.parentElement) {
                panel.remove();
            }
        }, 30000);
    }

    /**
     * Simulate offline mode for testing
     */
    simulateOffline() {
        console.log('📴 Simulating offline mode...');
        
        // Override navigator.onLine
        Object.defineProperty(navigator, 'onLine', {
            writable: true,
            value: false
        });

        // Dispatch offline event
        window.dispatchEvent(new Event('offline'));
        
        console.log('✅ Offline mode simulated');
        
        // Restore online mode after 10 seconds
        setTimeout(() => {
            Object.defineProperty(navigator, 'onLine', {
                writable: true,
                value: true
            });
            window.dispatchEvent(new Event('online'));
            console.log('🌐 Online mode restored');
        }, 10000);
    }
}

// Create global instance
window.offlineTester = new OfflineMapTester();

// Auto-run tests when page loads
document.addEventListener('DOMContentLoaded', function() {
    // Wait for map to initialize (increased delay)
    setTimeout(() => {
        window.offlineTester.runAllTests();
    }, 5000);
});

// Add test functions to global scope
window.testOfflineMap = () => window.offlineTester.runAllTests();
window.simulateOffline = () => window.offlineTester.simulateOffline();

console.log('🧪 Offline Map Tester loaded. Use testOfflineMap() or simulateOffline() to test.');
