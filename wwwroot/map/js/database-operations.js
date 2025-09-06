/**
 * Database Operations for TouchTokenApp
 * Replaces localStorage operations with database API calls
 */

// Extend the TouchTokenApp class with database operations
if (typeof TouchTokenApp !== 'undefined') {
    
    // ==================== TOKEN OPERATIONS ====================

    /**
     * Load tokens from database instead of localStorage
     */
    TouchTokenApp.prototype.loadTokensFromStorage = async function() {
        try {
            console.log('📡 Loading tokens from database...');
            const dbTokens = await window.apiService.loadTokens();
            
            // Convert database format to JavaScript format
            this.learnedTokens = dbTokens.map(dbToken => 
                window.apiService.convertTokenFromDbFormat(dbToken)
            );
            
            console.log(`📡 Loaded ${this.learnedTokens.length} tokens from database`);

            // Load map markers from database
            await this.loadMapMarkersFromStorage();

            // Try to restore map markers after tokens are loaded
            this.tryRestoreMapMarkers();
        } catch (error) {
            console.error('Error loading tokens from database:', error);
            this.showToast('Failed to load tokens from database', 'error');
            // Fallback to empty array
            this.learnedTokens = [];
        }
    };

    /**
     * Save tokens to database instead of localStorage
     */
    TouchTokenApp.prototype.saveTokensToStorage = async function() {
        try {
            console.log('📡 Saving tokens to database...');
            
            // Convert JavaScript format to database format
            const dbTokens = this.learnedTokens.map(jsToken => 
                window.apiService.convertTokenToDbFormat(jsToken)
            );
            
            // Use bulk import to save all tokens
            const results = await window.apiService.bulkImportTokens(dbTokens);
            
            // Check for any failures
            const failures = results.filter(result => !result.success);
            if (failures.length > 0) {
                console.warn('Some tokens failed to save:', failures);
                this.showToast(`${failures.length} tokens failed to save`, 'warning');
            } else {
                console.log('📡 All tokens saved to database successfully');
            }
        } catch (error) {
            console.error('Error saving tokens to database:', error);
            this.showToast('Failed to save tokens to database', 'error');
        }
    };

    /**
     * Save a single token to database
     */
    TouchTokenApp.prototype.saveTokenToStorage = async function(token) {
        try {
            this.learnedTokens.push(token);
            
            // Convert to database format and save
            const dbToken = window.apiService.convertTokenToDbFormat(token);
            await window.apiService.saveToken(dbToken);
            
            console.log(`📡 Token "${token.name}" saved to database`);
        } catch (error) {
            console.error('Error saving token to database:', error);
            this.showToast('Failed to save token to database', 'error');
            // Remove from local array if save failed
            this.learnedTokens = this.learnedTokens.filter(t => t.id !== token.id);
        }
    };

    /**
     * Delete token from database
     */
    TouchTokenApp.prototype.deleteToken = async function(tokenId) {
        const token = this.learnedTokens.find(t => t.id === tokenId);
        if (!token) return;

        try {
            // Delete from database
            await window.apiService.deleteToken(tokenId);
            
            // Delete associated map markers
            await window.apiService.deleteMapMarkersByToken(tokenId);
            
            // Remove from local array
            this.learnedTokens = this.learnedTokens.filter(t => t.id !== tokenId);
            
            // Remove from map if exists
            this.removeTokenMarker(tokenId);
            
            console.log(`📡 Token ${tokenId} deleted from database`);
            this.updateTokenCount();
            this.showToast(`Token "${token.name}" deleted`, 'success');
        } catch (error) {
            console.error('Error deleting token from database:', error);
            this.showToast('Failed to delete token from database', 'error');
        }
    };

    /**
     * Delete all tokens from database
     */
    TouchTokenApp.prototype.deleteAllTokens = async function() {
        if (confirm('Are you sure you want to delete ALL tokens? This cannot be undone.')) {
            try {
                // Delete all tokens from database
                await window.apiService.deleteAllTokens();
                
                // Clear local array
                this.learnedTokens = [];
                
                // Clear map markers
                this.mapTokenMarkers.clear();
                
                console.log('📡 All tokens deleted from database');
                this.updateTokenCount();
                this.showToast('All tokens deleted', 'success');
            } catch (error) {
                console.error('Error deleting all tokens from database:', error);
                this.showToast('Failed to delete all tokens from database', 'error');
            }
        }
    };

    /**
     * Export tokens (unchanged - still works with local data)
     */
    TouchTokenApp.prototype.exportTokens = function() {
        const dataStr = JSON.stringify(this.learnedTokens, null, 2);
        const dataBlob = new Blob([dataStr], { type: 'application/json' });
        const url = URL.createObjectURL(dataBlob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `tokens_${new Date().toISOString().split('T')[0]}.json`;
        link.click();
        URL.revokeObjectURL(url);
    };

    /**
     * Import tokens from database
     */
    TouchTokenApp.prototype.importTokens = async function(file) {
        const reader = new FileReader();
        reader.onload = async (e) => {
            try {
                const importedTokens = JSON.parse(e.target.result);
                
                // Convert to database format and save
                const dbTokens = importedTokens.map(jsToken => 
                    window.apiService.convertTokenToDbFormat(jsToken)
                );
                
                const results = await window.apiService.bulkImportTokens(dbTokens);
                
                // Check results
                const successful = results.filter(r => r.Success);
                const failed = results.filter(r => !r.Success);
                
                if (successful.length > 0) {
                    // Reload tokens from database
                    await this.loadTokensFromStorage();
                    this.updateTokenCount();
                    this.showToast(`${successful.length} tokens imported successfully`, 'success');
                }
                
                if (failed.length > 0) {
                    console.warn('Some tokens failed to import:', failed);
                    this.showToast(`${failed.length} tokens failed to import`, 'warning');
                }
            } catch (error) {
                console.error('Error importing tokens:', error);
                this.showToast('Failed to import tokens', 'error');
            }
        };
        reader.readAsText(file);
    };

    // ==================== MAP MARKER OPERATIONS ====================

    /**
     * Load map markers from database instead of localStorage
     */
    TouchTokenApp.prototype.loadMapMarkersFromStorage = async function() {
        try {
            console.log('📡 Loading map markers from database...');
            const dbMarkers = await window.apiService.loadMapMarkers();
            
            // Convert database format to JavaScript format
            const markersData = dbMarkers.map(dbMarker => 
                window.apiService.convertMapMarkerFromDbFormat(dbMarker)
            );
            
            console.log(`📡 Loaded ${markersData.length} map markers from database`);
            
            // Markers will be restored when map is initialized
            this.pendingMapMarkers = markersData;
        } catch (error) {
            console.error('Error loading map markers from database:', error);
            this.showToast('Failed to load map markers from database', 'error');
            this.pendingMapMarkers = [];
        }
    };

    /**
     * Save map markers to database instead of localStorage
     */
    TouchTokenApp.prototype.saveMapMarkersToStorage = async function() {
        try {
            console.log('📡 Saving map markers to database...');
            debugger
            const markersData = Array.from(this.mapTokenMarkers.entries()).map(([id, data]) => ({
                id: id,
                tokenId: data.tokenId,
                location: data.location,
                createdAt: data.createdAt,
                tokenName: data.tokenName
            }));
            
            // Convert to database format and save
            const dbMarkers = markersData.map(jsMarker => 
                window.apiService.convertMapMarkerToDbFormat(jsMarker)
            );
            
            const results = await window.apiService.bulkSaveMapMarkers(dbMarkers);
            
            // Check for any failures
            const failures = results.filter(result => !result.success);
            if (failures.length > 0) {
                console.warn('Some map markers failed to save:', failures);
                this.showToast(`${failures.length} map markers failed to save`, 'warning');
            } else {
                console.log(`📡 Saved ${markersData.length} map markers to database`);
            }
        } catch (error) {
            console.error('Error saving map markers to database:', error);
            this.showToast('Failed to save map markers to database', 'error');
        }
    };

    /**
     * Save a single map marker to database
     */
    TouchTokenApp.prototype.saveMapMarkerToStorage = async function(markerData) {
        try {
            const dbMarker = window.apiService.convertMapMarkerToDbFormat(markerData);
            await window.apiService.saveMapMarker(dbMarker);
            console.log(`📡 Map marker ${markerData.id} saved to database`);
        } catch (error) {
            console.error('Error saving map marker to database:', error);
            this.showToast('Failed to save map marker to database', 'error');
        }
    };

    /**
     * Remove token marker from map and database
     */
    TouchTokenApp.prototype.removeTokenMarker = async function(tokenId) {
        const markerEntry = this.mapTokenMarkers.get(`token_${tokenId}`);
        if (markerEntry) {
            try {
                // Remove from map
                this.map.removeLayer(markerEntry.marker);
                this.mapTokenMarkers.delete(`token_${tokenId}`);

                // Remove from database
                await window.apiService.deleteMapMarker(`token_${tokenId}`);
                
                console.log(`📡 Map marker for token ${tokenId} removed from database`);
            } catch (error) {
                console.error('Error removing map marker from database:', error);
                this.showToast('Failed to remove map marker from database', 'error');
            }
        }
    };

    // ==================== INITIALIZATION ====================

    /**
     * Initialize database operations
     */
    TouchTokenApp.prototype.initializeDatabase = async function() {
        try {
            console.log('📡 Initializing database operations...');
            
            // Load tokens from database
            await this.loadTokensFromStorage();
            
            console.log('📡 Database operations initialized successfully');
        } catch (error) {
            console.error('Error initializing database operations:', error);
            this.showToast('Failed to initialize database operations', 'error');
        }
    };

    // ==================== ERROR HANDLING ====================

    /**
     * Enhanced error handling for database operations
     */
    TouchTokenApp.prototype.handleDatabaseError = function(error, operation) {
        console.error(`Database error during ${operation}:`, error);
        
        let message = `Database error during ${operation}`;
        if (error.message.includes('HTTP 404')) {
            message = 'Data not found in database';
        } else if (error.message.includes('HTTP 400')) {
            message = 'Invalid data sent to database';
        } else if (error.message.includes('HTTP 500')) {
            message = 'Database server error';
        } else if (error.message.includes('Failed to fetch')) {
            message = 'Cannot connect to database server';
        }
        
        this.showToast(message, 'error');
    };

    // ==================== MIGRATION HELPERS ====================

    /**
     * Migrate data from localStorage to database (one-time migration)
     */
    TouchTokenApp.prototype.migrateFromLocalStorage = async function() {
        try {
            console.log('🔄 Starting migration from localStorage to database...');
            
            // Check if migration is needed
            const hasLocalTokens = localStorage.getItem('learnedTokens');
            const hasLocalMarkers = localStorage.getItem('mapTokenMarkers');
            
            if (!hasLocalTokens && !hasLocalMarkers) {
                console.log('📡 No localStorage data to migrate');
                return;
            }
            
            // Migrate tokens
            if (hasLocalTokens) {
                const localTokens = JSON.parse(hasLocalTokens);
                console.log(`🔄 Migrating ${localTokens.length} tokens from localStorage...`);
                
                const dbTokens = localTokens.map(jsToken => 
                    window.apiService.convertTokenToDbFormat(jsToken)
                );
                
                const results = await window.apiService.bulkImportTokens(dbTokens);
                const successful = results.filter(r => r.Success).length;
                console.log(`✅ Migrated ${successful} tokens to database`);
            }
            
            // Migrate map markers
            if (hasLocalMarkers) {
                const localMarkers = JSON.parse(hasLocalMarkers);
                console.log(`🔄 Migrating ${localMarkers.length} map markers from localStorage...`);
                
                const dbMarkers = localMarkers.map(jsMarker => 
                    window.apiService.convertMapMarkerToDbFormat(jsMarker)
                );
                
                const results = await window.apiService.bulkSaveMapMarkers(dbMarkers);
                const successful = results.filter(r => r.Success).length;
                console.log(`✅ Migrated ${successful} map markers to database`);
            }
            
            // Clear localStorage after successful migration
            localStorage.removeItem('learnedTokens');
            localStorage.removeItem('mapTokenMarkers');
            
            console.log('✅ Migration completed successfully');
            this.showToast('Data migrated from localStorage to database', 'success');
            
        } catch (error) {
            console.error('Error during migration:', error);
            this.showToast('Migration failed - data remains in localStorage', 'error');
        }
    };

    console.log('📡 Database operations loaded successfully');


}

