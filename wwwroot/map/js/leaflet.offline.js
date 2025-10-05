/**
 * Leaflet.offline - Offline map tiles
 * https://github.com/allartk/leaflet.offline
 * 
 * This is a simplified version for offline tile caching
 */

(function (root, factory) {
    if (typeof define === 'function' && define.amd) {
        define(['leaflet'], factory);
    } else if (typeof module === 'object' && module.exports) {
        module.exports = factory(require('leaflet'));
    } else {
        // Browser global - try immediate execution, fallback to DOMContentLoaded
        if (typeof root.L !== 'undefined') {
            factory(root.L);
        } else {
            // Try again after a short delay
            setTimeout(function() {
                if (typeof root.L !== 'undefined') {
                    factory(root.L);
                } else {
                    // Final fallback to DOMContentLoaded
                    document.addEventListener('DOMContentLoaded', function() {
                        if (typeof root.L !== 'undefined') {
                            factory(root.L);
                        } else {
                            console.error('Leaflet must be loaded before leaflet.offline');
                        }
                    });
                }
            }, 100);
        }
    }
}(typeof self !== 'undefined' ? self : this, function (L) {
    'use strict';

    if (!L) {
        console.error('Leaflet not available for offline plugin');
        return;
    }

    // Multi-backend storage utility
    const TileStorage = {
        dbName: 'LeafletOffline',
        version: 1,
        storeName: 'tiles',
        storageType: 'indexeddb', // Options: 'indexeddb', 'filesystem', 'server', 'hybrid'
        maxIndexedDBSize: 500 * 1024 * 1024, // 500MB limit for IndexedDB
        
        // Storage backends
        backends: {
            indexeddb: {
                name: 'IndexedDB',
                available: false,
                priority: 1
            },
            filesystem: {
                name: 'File System',
                available: false,
                priority: 2
            },
            server: {
                name: 'Server Storage',
                available: false,
                priority: 3
            }
        },
        
        // Initialize storage backends
        initialize: async function() {
            console.log('🔧 Initializing tile storage backends...');
            
            // Check IndexedDB availability
            if ('indexedDB' in window) {
                try {
                    await this.testIndexedDB();
                    this.backends.indexeddb.available = true;
                    console.log('✅ IndexedDB storage available');
                } catch (error) {
                    console.warn('⚠️ IndexedDB not available:', error.message);
                }
            }
            
            // Check File System Access API availability
            if ('showSaveFilePicker' in window && 'showDirectoryPicker' in window) {
                this.backends.filesystem.available = true;
                console.log('✅ File System Access API available');
            } else {
                console.warn('⚠️ File System Access API not available (Chrome 86+)');
            }
            
            // Check server storage availability (assume available if we have API endpoint)
            try {
                // Test if our server endpoint exists
                const response = await fetch('/api/map-tiles/test', { method: 'HEAD' });
                if (response.ok || response.status === 404) { // 404 is ok, means endpoint exists
                    this.backends.server.available = true;
                    console.log('✅ Server storage available');
                }
            } catch (error) {
                console.warn('⚠️ Server storage not available:', error.message);
            }
            
            // Set storage type based on availability
            this.selectBestStorageType();
            
            console.log(`🎯 Selected storage type: ${this.storageType}`);
        },
        
        // Select the best available storage type
        selectBestStorageType: function() {
            const available = Object.entries(this.backends)
                .filter(([key, backend]) => backend.available)
                .sort((a, b) => a[1].priority - b[1].priority);
            
            if (available.length > 0) {
                this.storageType = available[0][0];
            } else {
                this.storageType = 'indexeddb'; // Fallback
                console.warn('⚠️ No storage backends available, using IndexedDB fallback');
            }
        },
        
        // Test IndexedDB functionality
        testIndexedDB: async function() {
            return new Promise((resolve, reject) => {
                const request = indexedDB.open('test-db', 1);
                request.onerror = () => reject(new Error('IndexedDB test failed'));
                request.onsuccess = () => {
                    const db = request.result;
                    db.close();
                    indexedDB.deleteDatabase('test-db');
                    resolve();
                };
                request.onupgradeneeded = (event) => {
                    event.target.result.createObjectStore('test');
                };
            });
        },
        
        openDB: function() {
            return new Promise((resolve, reject) => {
                const request = indexedDB.open(this.dbName, this.version);
                
                request.onerror = () => reject(request.error);
                request.onsuccess = () => resolve(request.result);
                
                request.onupgradeneeded = (event) => {
                    const db = event.target.result;
                    if (!db.objectStoreNames.contains(this.storeName)) {
                        db.createObjectStore(this.storeName, { keyPath: 'key' });
                    }
                };
            });
        },
        
        saveTile: async function(key, blob) {
            const tileData = {
                key: key,
                blob: blob,
                timestamp: Date.now(),
                size: blob.size
            };
            
            try {
                switch (this.storageType) {
                    case 'filesystem':
                        return await this.saveToFileSystem(key, blob);
                    case 'server':
                        return await this.saveToServer(key, blob);
                    case 'hybrid':
                        return await this.saveToMultiple(key, blob);
                    default:
                        return await this.saveToIndexedDB(key, blob);
                }
            } catch (error) {
                console.error('Failed to save tile:', error);
                // Fallback to IndexedDB if other methods fail
                if (this.storageType !== 'indexeddb') {
                    console.warn('Falling back to IndexedDB storage');
                    return await this.saveToIndexedDB(key, blob);
                }
                throw error;
            }
        },
        
        // Save to IndexedDB
        saveToIndexedDB: function(key, blob) {
            return this.openDB().then(db => {
                return new Promise((resolve, reject) => {
                    const transaction = db.transaction([this.storeName], 'readwrite');
                    const store = transaction.objectStore(this.storeName);
                    const request = store.put({ 
                        key: key, 
                        blob: blob, 
                        timestamp: Date.now(),
                        size: blob.size 
                    });
                    request.onsuccess = () => resolve();
                    request.onerror = () => reject(request.error);
                });
            });
        },
        
        // Save to File System (requires user permission)
        saveToFileSystem: async function(key, blob) {
            if (!this.backends.filesystem.available) {
                throw new Error('File System Access API not available');
            }
            
            try {
                // Create a directory picker for tiles
                const dirHandle = await window.showDirectoryPicker({
                    mode: 'readwrite',
                    startIn: 'documents'
                });
                
                // Create tiles subdirectory
                const tilesDir = await dirHandle.getDirectoryHandle('map-tiles', { create: true });
                
                // Parse tile coordinates from key
                const [layerName, z, x, y] = key.split('_');
                const zDir = await tilesDir.getDirectoryHandle(z, { create: true });
                const xDir = await zDir.getDirectoryHandle(x, { create: true });
                
                // Save tile file
                const fileName = `${y}.png`;
                const fileHandle = await xDir.getFileHandle(fileName, { create: true });
                const writable = await fileHandle.createWritable();
                await writable.write(blob);
                await writable.close();
                
                console.log(`💾 Tile saved to file system: ${key}`);
                return Promise.resolve();
            } catch (error) {
                console.error('File system save failed:', error);
                throw error;
            }
        },
        
        // Save to Server
        saveToServer: async function(key, blob) {
            if (!this.backends.server.available) {
                throw new Error('Server storage not available');
            }
            
            const formData = new FormData();
            formData.append('tile', blob);
            formData.append('key', key);
            formData.append('timestamp', Date.now().toString());
            
            try {
                const response = await fetch('/api/map-tiles/save', {
                    method: 'POST',
                    body: formData
                });
                
                if (!response.ok) {
                    throw new Error(`Server error: ${response.status}`);
                }
                
                console.log(`🌐 Tile saved to server: ${key}`);
                return Promise.resolve();
            } catch (error) {
                console.error('Server save failed:', error);
                throw error;
            }
        },
        
        // Save to multiple backends
        saveToMultiple: async function(key, blob) {
            const results = [];
            
            // Save to IndexedDB (always)
            try {
                await this.saveToIndexedDB(key, blob);
                results.push('IndexedDB');
            } catch (error) {
                console.warn('IndexedDB save failed:', error);
            }
            
            // Save to File System if available
            if (this.backends.filesystem.available) {
                try {
                    await this.saveToFileSystem(key, blob);
                    results.push('File System');
                } catch (error) {
                    console.warn('File System save failed:', error);
                }
            }
            
            // Save to Server if available
            if (this.backends.server.available) {
                try {
                    await this.saveToServer(key, blob);
                    results.push('Server');
                } catch (error) {
                    console.warn('Server save failed:', error);
                }
            }
            
            if (results.length === 0) {
                throw new Error('All storage backends failed');
            }
            
            console.log(`💾 Tile saved to multiple backends: ${results.join(', ')}`);
            return Promise.resolve();
        },
        
        getTile: function(key) {
            return this.openDB().then(db => {
                return new Promise((resolve, reject) => {
                    const transaction = db.transaction([this.storeName], 'readonly');
                    const store = transaction.objectStore(this.storeName);
                    const request = store.get(key);
                    request.onsuccess = () => {
                        if (request.result) {
                            resolve(request.result.blob);
                        } else {
                            reject(new Error('Tile not found'));
                        }
                    };
                    request.onerror = () => reject(request.error);
                });
            });
        },
        
        removeTile: function(key) {
            return this.openDB().then(db => {
                return new Promise((resolve, reject) => {
                    const transaction = db.transaction([this.storeName], 'readwrite');
                    const store = transaction.objectStore(this.storeName);
                    const request = store.delete(key);
                    request.onsuccess = () => resolve();
                    request.onerror = () => reject(request.error);
                });
            });
        },
        
        clear: function() {
            return this.openDB().then(db => {
                return new Promise((resolve, reject) => {
                    const transaction = db.transaction([this.storeName], 'readwrite');
                    const store = transaction.objectStore(this.storeName);
                    const request = store.clear();
                    request.onsuccess = () => resolve();
                    request.onerror = () => reject(request.error);
                });
            });
        },
        
        getCount: function() {
            return this.openDB().then(db => {
                return new Promise((resolve, reject) => {
                    const transaction = db.transaction([this.storeName], 'readonly');
                    const store = transaction.objectStore(this.storeName);
                    const request = store.count();
                    request.onsuccess = () => resolve(request.result);
                    request.onerror = () => reject(request.error);
                });
            });
        }
    };

    // Offline Tile Layer
    L.TileLayer.Offline = L.TileLayer.extend({
        options: {
            offline: true,
            saveTiles: false
        },

        initialize: function(url, options) {
            L.TileLayer.prototype.initialize.call(this, url, options);
            this._offlineTiles = new Map();
        },

        createTile: function(coords, done) {
            const tile = L.TileLayer.prototype.createTile.call(this, coords, done);
            const key = this._getTileKey(coords);
            
            // Try to load from cache first
            TileStorage.getTile(key).then(blob => {
                const url = URL.createObjectURL(blob);
                tile.src = url;
                tile._offlineUrl = url;
                done(null, tile);
            }).catch(() => {
                // Load from network
                tile.onload = () => {
                    if (this.options.saveTiles) {
                        this._saveTile(key, tile);
                    }
                    done(null, tile);
                };
                tile.onerror = () => {
                    done(new Error('Tile load error'), tile);
                };
            });
            
            return tile;
        },

        _getTileKey: function(coords) {
            return `${this.options.layerName || 'default'}_${coords.z}_${coords.x}_${coords.y}`;
        },

        _saveTile: function(key, tile) {
            if (tile.crossOrigin) {
                tile.crossOrigin = 'anonymous';
            }
            
            // Convert tile to blob
            const canvas = document.createElement('canvas');
            const ctx = canvas.getContext('2d');
            canvas.width = tile.naturalWidth || 256;
            canvas.height = tile.naturalHeight || 256;
            
            ctx.drawImage(tile, 0, 0);
            canvas.toBlob(blob => {
                if (blob) {
                    TileStorage.saveTile(key, blob).catch(console.error);
                }
            }, 'image/png');
        },

        saveVisibleTiles: function() {
            const tiles = [];
            const bounds = this.getBounds();
            const zoom = this.getZoom();
            
            for (let x = Math.floor(bounds.getWest()); x <= Math.ceil(bounds.getEast()); x++) {
                for (let y = Math.floor(bounds.getSouth()); y <= Math.ceil(bounds.getNorth()); y++) {
                    tiles.push({ z: zoom, x: x, y: y });
                }
            }
            
            return Promise.all(tiles.map(coords => {
                const key = this._getTileKey(coords);
                return this._downloadAndSaveTile(key, coords);
            }));
        },

        _downloadAndSaveTile: function(key, coords) {
            return new Promise((resolve, reject) => {
                const img = new Image();
                img.crossOrigin = 'anonymous';
                
                img.onload = () => {
                    const canvas = document.createElement('canvas');
                    const ctx = canvas.getContext('2d');
                    canvas.width = img.width;
                    canvas.height = img.height;
                    
                    ctx.drawImage(img, 0, 0);
                    canvas.toBlob(blob => {
                        if (blob) {
                            TileStorage.saveTile(key, blob).then(resolve).catch(reject);
                        } else {
                            reject(new Error('Failed to create blob'));
                        }
                    }, 'image/png');
                };
                
                img.onerror = () => reject(new Error('Failed to load tile'));
                img.src = this.getTileUrl(coords);
            });
        },

        clearOfflineTiles: function() {
            return TileStorage.clear();
        },

        getOfflineTileCount: function() {
            return TileStorage.getCount();
        },
        
        // Export tiles as ZIP file
        exportTilesAsZip: async function() {
            try {
                console.log('📦 Preparing tile export...');
                
                // Get all tiles from IndexedDB
                const db = await TileStorage.openDB();
                const transaction = db.transaction([TileStorage.storeName], 'readonly');
                const store = transaction.objectStore(this.storeName);
                const request = store.getAll();
                
                const tiles = await new Promise((resolve, reject) => {
                    request.onsuccess = () => resolve(request.result);
                    request.onerror = () => reject(request.error);
                });
                
                if (tiles.length === 0) {
                    alert('No tiles to export');
                    return;
                }
                
                // Create ZIP file using JSZip (if available) or simple download
                if (typeof JSZip !== 'undefined') {
                    await this.createZipFile(tiles);
                } else {
                    await this.downloadTilesIndividually(tiles);
                }
                
            } catch (error) {
                console.error('Export failed:', error);
                alert('Export failed: ' + error.message);
            }
        },
        
        // Create ZIP file using JSZip
        createZipFile: async function(tiles) {
            const zip = new JSZip();
            
            // Add tiles to ZIP with proper folder structure
            tiles.forEach(tile => {
                const [layerName, z, x, y] = tile.key.split('_');
                const path = `${layerName}/${z}/${x}/${y}.png`;
                zip.file(path, tile.blob);
            });
            
            // Generate and download ZIP
            const content = await zip.generateAsync({ type: 'blob' });
            const url = URL.createObjectURL(content);
            const a = document.createElement('a');
            a.href = url;
            a.download = `map-tiles-${new Date().toISOString().split('T')[0]}.zip`;
            document.body.appendChild(a);
            a.click();
            document.body.removeChild(a);
            URL.revokeObjectURL(url);
            
            console.log(`📦 Exported ${tiles.length} tiles as ZIP file`);
        },
        
        // Download tiles individually (fallback)
        downloadTilesIndividually: async function(tiles) {
            console.log(`📥 Downloading ${tiles.length} tiles individually...`);
            
            for (let i = 0; i < tiles.length; i++) {
                const tile = tiles[i];
                const [layerName, z, x, y] = tile.key.split('_');
                const url = URL.createObjectURL(tile.blob);
                const a = document.createElement('a');
                a.href = url;
                a.download = `${layerName}_${z}_${x}_${y}.png`;
                a.style.display = 'none';
                document.body.appendChild(a);
                a.click();
                document.body.removeChild(a);
                URL.revokeObjectURL(url);
                
                // Small delay to prevent browser blocking
                await new Promise(resolve => setTimeout(resolve, 100));
            }
        }
    });

    // Offline Control
    L.Control.Offline = L.Control.extend({
        options: {
            position: 'topright',
            saveButtonHtml: '<i class="fas fa-download"></i> Save Tiles',
            removeButtonHtml: '<i class="fas fa-trash"></i> Clear Cache',
            statusHtml: '<i class="fas fa-info-circle"></i> Status',
            exportButtonHtml: '<i class="fas fa-file-archive"></i> Export',
            settingsButtonHtml: '<i class="fas fa-cog"></i> Storage',
            confirmSavingCallback: function(nTilesToSave, continueSaveTiles) {
                if (window.confirm(`Save ${nTilesToSave} tiles for offline use?`)) {
                    continueSaveTiles();
                }
            },
            confirmRemovalCallback: function(continueRemoveTiles) {
                if (window.confirm('Remove all cached tiles?')) {
                    continueRemoveTiles();
                }
            }
        },

        onAdd: function(map) {
            const container = L.DomUtil.create('div', 'leaflet-control-offline leaflet-bar leaflet-control');
            
            // Save button
            this._saveButton = this._createButton(
                this.options.saveButtonHtml,
                'Save visible tiles for offline use',
                container,
                this._saveTiles,
                this
            );
            
            // Export button
            this._exportButton = this._createButton(
                this.options.exportButtonHtml,
                'Export tiles as ZIP file',
                container,
                this._exportTiles,
                this
            );
            
            // Settings button
            this._settingsButton = this._createButton(
                this.options.settingsButtonHtml,
                'Storage settings',
                container,
                this._showStorageSettings,
                this
            );
            
            // Remove button
            this._removeButton = this._createButton(
                this.options.removeButtonHtml,
                'Clear all cached tiles',
                container,
                this._removeTiles,
                this
            );
            
            // Status button
            this._statusButton = this._createButton(
                this.options.statusHtml,
                'Show cache status',
                container,
                this._showStatus,
                this
            );
            
            // Initialize storage backends
            TileStorage.initialize().catch(error => {
                console.error('Storage initialization failed:', error);
            });
            
            return container;
        },

        _createButton: function(html, title, container, fn, context) {
            const button = L.DomUtil.create('button', '', container);
            button.innerHTML = html;
            button.title = title;
            button.type = 'button';
            
            L.DomEvent.disableClickPropagation(button);
            L.DomEvent.on(button, 'click', fn, context);
            
            return button;
        },

        _saveTiles: function() {
            const offlineLayers = this._getOfflineLayers();
            let totalTiles = 0;
            
            offlineLayers.forEach(layer => {
                const bounds = this._map.getBounds();
                const zoom = this._map.getZoom();
                const tiles = this._calculateTilesInBounds(bounds, zoom);
                totalTiles += tiles.length;
            });
            
            if (totalTiles > 0) {
                this.options.confirmSavingCallback(totalTiles, () => {
                    this._performSave();
                });
            } else {
                alert('No tiles to save');
            }
        },

        _performSave: function() {
            const offlineLayers = this._getOfflineLayers();
            let savedCount = 0;
            let totalCount = 0;
            
            offlineLayers.forEach(layer => {
                layer.saveVisibleTiles().then(saved => {
                    savedCount += saved.length;
                    totalCount += saved.length;
                    this._updateStatus();
                }).catch(error => {
                    console.error('Error saving tiles:', error);
                });
            });
        },

        _removeTiles: function() {
            this.options.confirmRemovalCallback(() => {
                const offlineLayers = this._getOfflineLayers();
                Promise.all(offlineLayers.map(layer => layer.clearOfflineTiles())).then(() => {
                    this._updateStatus();
                    alert('All cached tiles cleared');
                }).catch(error => {
                    console.error('Error clearing tiles:', error);
                    alert('Error clearing tiles');
                });
            });
        },

        _showStatus: function() {
            const offlineLayers = this._getOfflineLayers();
            Promise.all(offlineLayers.map(layer => layer.getOfflineTileCount())).then(counts => {
                const total = counts.reduce((sum, count) => sum + count, 0);
                alert(`Total cached tiles: ${total}`);
            }).catch(error => {
                console.error('Error getting status:', error);
            });
        },

        _getOfflineLayers: function() {
            const layers = [];
            this._map.eachLayer(layer => {
                if (layer instanceof L.TileLayer.Offline) {
                    layers.push(layer);
                }
            });
            return layers;
        },

        _calculateTilesInBounds: function(bounds, zoom) {
            const tiles = [];
            const west = Math.floor(bounds.getWest());
            const east = Math.ceil(bounds.getEast());
            const south = Math.floor(bounds.getSouth());
            const north = Math.ceil(bounds.getNorth());
            
            for (let x = west; x <= east; x++) {
                for (let y = south; y <= north; y++) {
                    tiles.push({ z: zoom, x: x, y: y });
                }
            }
            
            return tiles;
        },

        _updateStatus: function() {
            // Update status display if needed
        },
        
        // Export tiles as ZIP
        _exportTiles: function() {
            const offlineLayers = this._getOfflineLayers();
            if (offlineLayers.length > 0) {
                offlineLayers[0].exportTilesAsZip();
            } else {
                alert('No offline layers found');
            }
        },
        
        // Show storage settings
        _showStorageSettings: function() {
            const modal = this._createStorageModal();
            document.body.appendChild(modal);
        },
        
        // Create storage settings modal
        _createStorageModal: function() {
            const modal = document.createElement('div');
            modal.className = 'offline-storage-modal';
            modal.style.cssText = `
                position: fixed;
                top: 0;
                left: 0;
                width: 100%;
                height: 100%;
                background: rgba(0, 0, 0, 0.8);
                z-index: 10000;
                display: flex;
                align-items: center;
                justify-content: center;
                color: white;
                font-family: Arial, sans-serif;
            `;
            
            const availableBackends = Object.entries(TileStorage.backends)
                .filter(([key, backend]) => backend.available)
                .map(([key, backend]) => ({ key, ...backend }));
            
            modal.innerHTML = `
                <div style="background: #2a2a2a; padding: 30px; border-radius: 10px; max-width: 500px; width: 90%;">
                    <h2 style="margin-top: 0; color: #fff;">🗄️ Storage Settings</h2>
                    
                    <div style="margin-bottom: 20px;">
                        <h3>Current Storage: ${TileStorage.backends[TileStorage.storageType]?.name || 'Unknown'}</h3>
                        <p style="color: #ccc;">Available storage options:</p>
                    </div>
                    
                    <div style="margin-bottom: 20px;">
                        ${availableBackends.map(backend => `
                            <label style="display: block; margin-bottom: 10px; padding: 10px; background: ${backend.key === TileStorage.storageType ? '#007bff' : '#444'}; border-radius: 5px; cursor: pointer;">
                                <input type="radio" name="storage" value="${backend.key}" ${backend.key === TileStorage.storageType ? 'checked' : ''} 
                                       style="margin-right: 10px;">
                                <strong>${backend.name}</strong>
                                <div style="font-size: 12px; color: #ccc; margin-top: 5px;">
                                    ${this._getStorageDescription(backend.key)}
                                </div>
                            </label>
                        `).join('')}
                    </div>
                    
                    <div style="margin-bottom: 20px;">
                        <h4>Storage Info:</h4>
                        <div style="background: #333; padding: 10px; border-radius: 5px; font-size: 12px;">
                            <div>IndexedDB: ${TileStorage.backends.indexeddb.available ? '✅ Available' : '❌ Not Available'}</div>
                            <div>File System: ${TileStorage.backends.filesystem.available ? '✅ Available' : '❌ Not Available'}</div>
                            <div>Server: ${TileStorage.backends.server.available ? '✅ Available' : '❌ Not Available'}</div>
                        </div>
                    </div>
                    
                    <div style="text-align: right;">
                        <button id="saveStorageSettings" style="background: #28a745; color: white; border: none; padding: 10px 20px; border-radius: 5px; margin-right: 10px; cursor: pointer;">
                            Save Settings
                        </button>
                        <button id="closeStorageModal" style="background: #6c757d; color: white; border: none; padding: 10px 20px; border-radius: 5px; cursor: pointer;">
                            Close
                        </button>
                    </div>
                </div>
            `;
            
            // Add event listeners
            modal.querySelector('#closeStorageModal').onclick = () => modal.remove();
            modal.querySelector('#saveStorageSettings').onclick = () => {
                const selected = modal.querySelector('input[name="storage"]:checked');
                if (selected) {
                    TileStorage.storageType = selected.value;
                    console.log(`Storage type changed to: ${TileStorage.storageType}`);
                    alert(`Storage type changed to: ${TileStorage.backends[TileStorage.storageType].name}`);
                }
                modal.remove();
            };
            
            // Close on background click
            modal.onclick = (e) => {
                if (e.target === modal) modal.remove();
            };
            
            return modal;
        },
        
        // Get storage description
        _getStorageDescription: function(storageType) {
            const descriptions = {
                'indexeddb': 'Fast browser storage, limited by quota (~500MB-2GB)',
                'filesystem': 'Saves to your computer, permanent storage, requires permission',
                'server': 'Saves to game server, accessible from any device',
                'hybrid': 'Saves to multiple locations for maximum reliability'
            };
            return descriptions[storageType] || 'Unknown storage type';
        }
    });

    // Factory functions
    L.tileLayer.offline = function(url, options) {
        return new L.TileLayer.Offline(url, options);
    };

    L.control.offline = function(options) {
        return new L.Control.Offline(options);
    };

    // Log successful initialization
    console.log('✅ Leaflet.offline plugin loaded successfully');
    console.log('   - L.TileLayer.Offline:', typeof L.TileLayer.Offline);
    console.log('   - L.tileLayer.offline:', typeof L.tileLayer.offline);
    console.log('   - L.Control.Offline:', typeof L.Control.Offline);
    console.log('   - L.control.offline:', typeof L.control.offline);

    return L;
}));
