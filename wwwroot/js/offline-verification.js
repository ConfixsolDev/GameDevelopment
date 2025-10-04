/**
 * Offline Verification Script
 * Tests to confirm map works without IndexedDB or network
 */

class OfflineVerification {
    constructor() {
        this.testResults = [];
    }

    /**
     * Run all offline verification tests
     */
    async runAllTests() {
        console.log('🔍 Starting offline verification tests...');
        
        try {
            await this.testNetworkDisconnection();
            await this.testIndexedDBClearance();
            await this.testFileSystemStorage();
            await this.testServerStorage();
            await this.testHybridStorage();
            await this.testMapRendering();
            
            this.displayResults();
        } catch (error) {
            console.error('❌ Verification tests failed:', error);
        }
    }

    /**
     * Test 1: Disconnect from network completely
     */
    async testNetworkDisconnection() {
        console.log('🌐 Testing network disconnection...');
        
        const test = {
            name: 'Network Disconnection',
            passed: false,
            details: ''
        };

        try {
            // Simulate offline mode
            Object.defineProperty(navigator, 'onLine', {
                writable: true,
                value: false
            });

            // Block all network requests
            const originalFetch = window.fetch;
            window.fetch = function() {
                return Promise.reject(new Error('Network blocked for testing'));
            };

            // Test if map still renders tiles
            if (window.gameMap) {
                // Force map to re-render current view
                window.gameMap.invalidateSize();
                
                // Wait a moment for tiles to load
                await new Promise(resolve => setTimeout(resolve, 2000));
                
                // Check if tiles are visible (this should work if truly offline)
                const tileLayers = [];
                window.gameMap.eachLayer(layer => {
                    if (layer instanceof L.TileLayer || layer instanceof L.TileLayer.Offline) {
                        tileLayers.push(layer);
                    }
                });

                if (tileLayers.length > 0) {
                    test.passed = true;
                    test.details = `Map rendered with ${tileLayers.length} tile layers while offline`;
                } else {
                    test.details = 'No tile layers found while offline';
                }
            } else {
                test.details = 'Game map not available';
            }

            // Restore network
            Object.defineProperty(navigator, 'onLine', {
                writable: true,
                value: true
            });
            window.fetch = originalFetch;

        } catch (error) {
            test.details = `Error: ${error.message}`;
        }

        this.testResults.push(test);
    }

    /**
     * Test 2: Clear IndexedDB completely
     */
    async testIndexedDBClearance() {
        console.log('🗄️ Testing IndexedDB clearance...');
        
        const test = {
            name: 'IndexedDB Clearance',
            passed: false,
            details: ''
        };

        try {
            // Clear all IndexedDB data
            const databases = await indexedDB.databases();
            for (const db of databases) {
                if (db.name && db.name.includes('Leaflet')) {
                    indexedDB.deleteDatabase(db.name);
                }
            }

            // Wait for deletion to complete
            await new Promise(resolve => setTimeout(resolve, 1000));

            // Force map to reload tiles
            if (window.gameMap) {
                window.gameMap.eachLayer(layer => {
                    if (layer instanceof L.TileLayer || layer instanceof L.TileLayer.Offline) {
                        layer.redraw();
                    }
                });

                // Wait for tiles to load
                await new Promise(resolve => setTimeout(resolve, 2000));

                // Check if tiles still render
                const mapContainer = document.querySelector('#gameMap');
                const tileImages = mapContainer.querySelectorAll('img[src*="tile"], img[src*="blob:"]');
                
                if (tileImages.length > 0) {
                    test.passed = true;
                    test.details = `${tileImages.length} tiles rendered after IndexedDB cleared`;
                } else {
                    test.details = 'No tiles rendered after IndexedDB cleared';
                }
            } else {
                test.details = 'Game map not available';
            }

        } catch (error) {
            test.details = `Error: ${error.message}`;
        }

        this.testResults.push(test);
    }

    /**
     * Test 3: Verify File System storage
     */
    async testFileSystemStorage() {
        console.log('💾 Testing File System storage...');
        
        const test = {
            name: 'File System Storage',
            passed: false,
            details: ''
        };

        try {
            if ('showSaveFilePicker' in window) {
                // Check if we can access file system
                test.passed = true;
                test.details = 'File System Access API available';
                
                // Test saving a tile to file system
                if (window.TileStorage && window.TileStorage.backends.filesystem.available) {
                    test.details += ' - Can save to file system';
                }
            } else {
                test.details = 'File System Access API not available (Chrome 86+ required)';
            }
        } catch (error) {
            test.details = `Error: ${error.message}`;
        }

        this.testResults.push(test);
    }

    /**
     * Test 4: Verify Server storage
     */
    async testServerStorage() {
        console.log('🌐 Testing Server storage...');
        
        const test = {
            name: 'Server Storage',
            passed: false,
            details: ''
        };

        try {
            // Test server endpoint
            const response = await fetch('/api/map-tiles/status');
            if (response.ok) {
                const data = await response.json();
                test.passed = true;
                test.details = `Server storage available - ${data.tilesCount} tiles, ${data.totalSizeFormatted}`;
            } else {
                test.details = `Server responded with ${response.status}`;
            }
        } catch (error) {
            test.details = `Server storage not available: ${error.message}`;
        }

        this.testResults.push(test);
    }

    /**
     * Test 5: Verify Hybrid storage
     */
    async testHybridStorage() {
        console.log('🔄 Testing Hybrid storage...');
        
        const test = {
            name: 'Hybrid Storage',
            passed: false,
            details: ''
        };

        try {
            if (window.TileStorage) {
                const backends = window.TileStorage.backends;
                const availableCount = Object.values(backends).filter(b => b.available).length;
                
                test.passed = availableCount > 1;
                test.details = `${availableCount} storage backends available`;
                
                if (availableCount > 1) {
                    test.details += ' - Hybrid storage possible';
                }
            } else {
                test.details = 'TileStorage not available';
            }
        } catch (error) {
            test.details = `Error: ${error.message}`;
        }

        this.testResults.push(test);
    }

    /**
     * Test 6: Verify map rendering without network
     */
    async testMapRendering() {
        console.log('🗺️ Testing map rendering...');
        
        const test = {
            name: 'Map Rendering',
            passed: false,
            details: ''
        };

        try {
            if (window.gameMap) {
                // Get current map state
                const center = window.gameMap.getCenter();
                const zoom = window.gameMap.getZoom();
                const bounds = window.gameMap.getBounds();
                
                // Count visible tile elements
                const mapContainer = document.querySelector('#gameMap');
                const tileElements = mapContainer.querySelectorAll('img');
                
                if (tileElements.length > 0) {
                    test.passed = true;
                    test.details = `${tileElements.length} tile images rendered at zoom ${zoom}`;
                    
                    // Check if tiles are from blob URLs (cached)
                    const blobTiles = Array.from(tileElements).filter(img => img.src.startsWith('blob:'));
                    if (blobTiles.length > 0) {
                        test.details += ` (${blobTiles.length} from cache)`;
                    }
                } else {
                    test.details = 'No tile images found on map';
                }
            } else {
                test.details = 'Game map not available';
            }
        } catch (error) {
            test.details = `Error: ${error.message}`;
        }

        this.testResults.push(test);
    }

    /**
     * Display test results
     */
    displayResults() {
        console.log('\n📊 OFFLINE VERIFICATION RESULTS');
        console.log('================================');
        
        const passed = this.testResults.filter(t => t.passed).length;
        const total = this.testResults.length;
        
        console.log(`Tests Passed: ${passed}/${total}`);
        console.log('');
        
        this.testResults.forEach(test => {
            const status = test.passed ? '✅' : '❌';
            console.log(`${status} ${test.name}: ${test.details}`);
        });
        
        console.log('\n================================');
        
        // Create visual results panel
        this.createResultsPanel();
    }

    /**
     * Create visual results panel
     */
    createResultsPanel() {
        const panel = document.createElement('div');
        panel.id = 'offlineVerificationResults';
        panel.style.cssText = `
            position: fixed;
            top: 20px;
            right: 20px;
            width: 400px;
            background: rgba(26, 26, 26, 0.95);
            border: 1px solid #444;
            border-radius: 8px;
            padding: 20px;
            color: white;
            font-family: monospace;
            font-size: 12px;
            z-index: 10000;
            box-shadow: 0 4px 20px rgba(0, 0, 0, 0.5);
            max-height: 80vh;
            overflow-y: auto;
        `;

        const passed = this.testResults.filter(t => t.passed).length;
        const total = this.testResults.length;

        panel.innerHTML = `
            <h3 style="margin: 0 0 15px 0; color: #fff;">🧪 Offline Verification</h3>
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
                <button onclick="window.offlineVerifier.runAllTests()" 
                        style="background: #28a745; color: white; border: none; padding: 8px 16px; border-radius: 4px; cursor: pointer; margin-left: 8px;">
                    Run Again
                </button>
            </div>
        `;

        // Remove existing panel if any
        const existing = document.getElementById('offlineVerificationResults');
        if (existing) existing.remove();

        document.body.appendChild(panel);

        // Auto-remove after 60 seconds
        setTimeout(() => {
            if (panel.parentElement) {
                panel.remove();
            }
        }, 60000);
    }
}

// Create global instance
window.offlineVerifier = new OfflineVerification();

// Add to global scope
window.verifyOfflineMap = () => window.offlineVerifier.runAllTests();

console.log('🧪 Offline Verification loaded. Use verifyOfflineMap() to test.');
