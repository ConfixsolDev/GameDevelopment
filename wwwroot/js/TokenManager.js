/**
 * TokenManager - Centralized token management system
 * Handles all token-related activities: placement, movement, deletion, and UI management
 */
class TokenManager {
    constructor() {
        this.tokens = [];
        this.placedTokens = new Map(); // tokenId -> { marker, coverageAreas, token }
        this.tokenPlacementManager = null;
        this.isInitialized = false;
        // No client-side cache
        
        // Bind methods to preserve context
        this.initialize = this.initialize.bind(this);
        this.openTokenPlacement = this.openTokenPlacement.bind(this);
        this.cancelTokenPlacement = this.cancelTokenPlacement.bind(this);
        this.placeTokenOnMap = this.placeTokenOnMap.bind(this);
        this.refreshTokenList = this.refreshTokenList.bind(this);
        this.updateTokenCounts = this.updateTokenCounts.bind(this);
        this.showTokenDetails = this.showTokenDetails.bind(this);
        this.closeTokenDetails = this.closeTokenDetails.bind(this);
        this.deleteTokenById = this.deleteTokenById.bind(this);
        this.openTokenDataEntryModal = this.openTokenDataEntryModal.bind(this);
        this.validateTokenData = this.validateTokenData.bind(this);
    }

    /**
     * Initialize the token manager
     */
    async initialize(map, notificationCallback) {
        if (this.isInitialized) {
            console.log('TokenManager already initialized');
            return;
        }

        try {
            // Initialize TokenPlacementManager if available
            if (typeof TokenPlacementManager !== 'undefined' && map) {
                this.tokenPlacementManager = new TokenPlacementManager(map, notificationCallback);
                console.log('TokenPlacementManager initialized');
            }

            // Load tokens from database
            await this.loadTokensFromDatabase();

            // Make functions globally available
            this.makeFunctionsGlobal();

            this.isInitialized = true;
            console.log('TokenManager initialized successfully');
        } catch (error) {
            console.error('Error initializing TokenManager:', error);
        }
    }

    /**
     * Make all functions globally available
     */
    makeFunctionsGlobal() {
        window.openTokenPlacement = this.openTokenPlacement;
        window.cancelTokenPlacement = this.cancelTokenPlacement;
        window.placeTokenOnMap = this.placeTokenOnMap;
        window.refreshTokenList = this.refreshTokenList;
        window.updateTokenCounts = this.updateTokenCounts;
        window.showTokenDetails = this.showTokenDetails;
        window.closeTokenDetails = this.closeTokenDetails;
        window.deleteTokenById = this.deleteTokenById;
        window.openTokenDataEntryModal = this.openTokenDataEntryModal;
        window.validateTokenData = this.validateTokenData;
        
        // Token selection modal functions
        window.openTokenSelectionModal = this.openTokenSelectionModal.bind(this);
        window.closeTokenSelectionModal = this.closeTokenSelectionModal.bind(this);
        window.loadAvailableTokens = this.loadAvailableTokens.bind(this);
        window.populateTokenSelection = this.populateTokenSelection.bind(this);
        window.selectTokenForPlacement = this.selectTokenForPlacement.bind(this);
        window.showTokenError = this.showTokenError.bind(this);

        // Tab handlers
        window.switchTokenTab = this.switchTokenTab?.bind(this) || (() => {});
    }

    /**
     * Load tokens from database
     */
    async loadTokensFromDatabase() {
        try {
            const response = await fetch('/GamePlay/GetTeamTokens');
            const result = await response.json();

            if (result.success) {
                this.tokens = result.tokens.map(token => ({
                    ...token,
                    status: 'created', // Default status
                    position: null,
                    marker: null
                }));

                // Load positions for placed tokens
                for (const token of this.tokens) {
                    try {
                        const positionResponse = await fetch(`/GamePlay/GetTokenPosition?tokenId=${token.id}`);
                        const positionResult = await positionResponse.json();
                        
                        if (positionResult.success) {
                            token.position = positionResult.position;
                            token.status = 'placed';
                        }
                    } catch (error) {
                        console.log(`No position found for token ${token.id}:`, error.message);
                    }
                }

                console.log(`Loaded ${this.tokens.length} tokens from database`);
                return this.tokens;
            } else {
                console.error('Failed to load tokens:', result.message);
                return [];
            }
        } catch (error) {
            console.error('Error loading tokens from database:', error);
            return [];
        }
    }

    /**
     * Open token placement modal
     */
    openTokenPlacement() {
        if (typeof openModal === 'function') {
            openModal('tokenManagementModal');
            this.refreshTokenList();
        } else {
            console.error('openModal function not available');
        }
    }

    /**
     * Cancel token placement
     */
    cancelTokenPlacement() {
        if (this.tokenPlacementManager) {
            this.tokenPlacementManager.cancelPlacementMode();
        }
        if (typeof showNotification === 'function') {
            showNotification('Token placement cancelled', 'info');
        }
    }

    /**
     * Place token on map using new system
     */
    async placeTokenOnMap(tokenId) {
        console.log('placeTokenOnMap called with tokenId:', tokenId);
        const token = this.tokens.find(t => t.id === tokenId);
        if (!token) {
            console.error('Token not found:', tokenId);
            if (typeof showNotification === 'function') {
                showNotification('Token not found', 'error');
            }
            return;
        }
        
        console.log('Found token:', token);
        
        // Check if token is already placed
        if (this.tokenPlacementManager && this.tokenPlacementManager.isTokenPlaced(tokenId)) {
            if (typeof showNotification === 'function') {
                showNotification('Token is already placed on the map', 'info');
            }
            return;
        }
        
        // Validate that token has required data before placement
        console.log('Validating token data...');
        const isValid = await this.validateTokenData(tokenId);
        console.log('Token validation result:', isValid);
        if (!isValid) {
            if (typeof showNotification === 'function') {
                showNotification('Token must have at least one Brigade before placement', 'error');
            }
            return;
        }
        
        // Start placement mode
        if (this.tokenPlacementManager) {
            this.tokenPlacementManager.startPlacementMode(token);
            if (typeof closeModal === 'function') {
                closeModal('tokenManagementModal');
            }
        } else {
            if (typeof showNotification === 'function') {
                showNotification('Token placement system not initialized', 'error');
            }
        }
    }

    /**
     * Validate token data
     */
    async validateTokenData(tokenId) {
        try {
            // Check if token has brigades from database (required for military units)
            const response = await fetch(`/DataManagement/GetTokenBrigades?tokenId=${tokenId}`);
            const result = await response.json();
            
            if (result.success && result.data && result.data.length > 0) {
                return true;
            }
            
            // For now, allow placement even without brigades
            // This can be made more strict based on requirements
            return true;
        } catch (error) {
            console.error('Error validating token data:', error);
            return true; // Allow placement on validation error
        }
    }

    /**
     * Refresh token list in the modal
     */
    async refreshTokenList() {
        const q = ($('#tokenSearch').val() || '').toLowerCase();
        const list = $('#tokenList');

        try {
            // Reload tokens from database
            await this.loadTokensFromDatabase();

            // Filter tokens based on search query
            const filteredTokens = this.tokens.filter(t => 
                t.name.toLowerCase().includes(q) ||
                (t.tokenGroupName && t.tokenGroupName.toLowerCase().includes(q))
            );

            // Generate token list HTML
            const rows = filteredTokens.map(t => {
                const status = t.status || 'created';
                const statusColor = status === 'placed' ? '#28a745' : '#6c757d';
                const positionText = t.position ? 
                    `${t.position.lat.toFixed(4)}, ${t.position.lng.toFixed(4)}` : 
                    'Not placed';

                return `
                <div class="deployed-unit-item" data-token-id="${t.id}">
                    <div class="deployed-unit-info">
                        <div class="deployed-unit-name">${t.name}</div>
                        <div class="deployed-unit-status">
                            <span style="color: ${statusColor};">●</span> ${status.toUpperCase()} | ${t.type || 'Token'} | ${positionText}
                        </div>
                        ${t.tokenGroupName ? `<div class="deployed-unit-group">Group: ${t.tokenGroupName}</div>` : ''}
                    </div>
                    <div class="deployed-unit-actions">
                        ${status === 'created' ? `<button class="btn btn-sm btn-primary" onclick="placeTokenOnMap('${t.id}')">Place on Map</button>` : ''}
                        ${status === 'placed' ? `<button class="btn btn-sm btn-info" onclick="tokenManager.tokenPlacementManager.startMoveMode('${t.id}')">Move</button>` : ''}
                        ${status === 'placed' ? `<button class="btn btn-sm btn-warning" onclick="tokenManager.tokenPlacementManager.removeTokenFromMap('${t.id}'); refreshTokenList()">Remove from Map</button>` : ''}
                        ${status === 'created' ? `<button class="btn btn-sm btn-warning" onclick="openTokenDataEntryModal('${t.id}')">Data Entry</button>` : ''}
                        <button class="btn btn-sm btn-danger" onclick="deleteTokenById('${t.id}'); refreshTokenList()">Delete</button>
                    </div>
                </div>`;
            });

            list.html(rows.join('') || '<p>No tokens found.</p>');
            
            // Update token counts
            this.updateTokenCounts();
        } catch (error) {
            console.error('Error loading tokens:', error);
            list.html('<div class="error">Error loading tokens</div>');
        }
    }

    /**
     * Update token counts in the modal
     */
    updateTokenCounts() {
        const totalCount = this.tokens.length;
        const placedCount = this.tokenPlacementManager ? this.tokenPlacementManager.getPlacedTokens().length : 0;
        
        const totalElement = document.getElementById('totalTokenCount');
        const placedElement = document.getElementById('placedTokenCount');
        
        if (totalElement) totalElement.textContent = totalCount;
        if (placedElement) placedElement.textContent = placedCount;
    }

    /**
     * Show token details
     */
    async showTokenDetails(token) {
        console.log('🎯 Loading token summary modal for:', token.name, token.id);
        
        $("#loading").show();
        
        // Load the token summary directly from GetTokenSummary
        $.ajax({
            url: '/DataManagement/GetTokenSummary',
            type: 'GET',
            data: { tokenId: token.id },
            success: function(modalHtml) {
                console.log('✅ Token summary loaded');
                
                // Remove any existing modal and add the new one
                $('#tokenSummaryModal').remove();
                $('body').append(modalHtml);
                
                // Show the modal
                const modal = document.getElementById('tokenSummaryModal');
                if (modal) {
                    modal.style.display = 'flex';
                    window.currentTokenId = token.id;
                } else {
                    console.error('❌ Token summary modal element not found');
                    alert('Error: Could not display token summary modal');
                }
            },
            error: function(xhr, status, error) {
                console.error('❌ Error loading token summary:', error);
                alert('Failed to load token summary');
            },
            complete: function() {
                $("#loading").hide();
            }
        });
        return; // Exit early - rest of method is legacy code now commented out
        
        try {
            // Load all token-related data in a single request
            const summaryResponse = await fetch(`/DataManagement/GetTokenSummary?tokenId=${token.id}`);
            const summary = await summaryResponse.json();
            const data = summary && summary.success ? summary.data : {};
            const brigades = data.brigades || [];
            const infantryBattalions = data.infantryBattalions || [];
            const armouredRegiments = data.armouredRegiments || [];
            const artilleryRegiments = data.artilleryRegiments || [];
            const intelligence = data.intelligence || [];
            const recon = data.recon || [];

            // Build table-based details UI
            let detailsHtml = `
                <div class="token-details-panel">
                    <div class="token-details-header">
                        <h3><i class="fas fa-map-marker-alt"></i> ${token.name || 'Unnamed Token'}</h3>
                        <div>
                            <button class="btn btn-sm btn-primary" onclick="tokenManager.tokenPlacementManager && tokenManager.tokenPlacementManager.startMoveMode && tokenManager.tokenPlacementManager.startMoveMode('${token.id}')">
                                <i class="fas fa-arrows-alt"></i> Move Token
                            </button>
                            <button class="btn btn-sm btn-secondary" onclick="closeTokenDetails()">Close</button>
                        </div>
                    </div>
                    <div class="token-details-content">
                        <table class="table table-sm table-dark" style="width:100%; margin-bottom:16px;">
                            <tbody>
                                <tr><th style="width:180px;">Token ID</th><td>${token.id}</td></tr>
                                <tr><th>Name</th><td>${token.name || ''}</td></tr>
                                <tr><th>Status</th><td><span class="status-badge ${token.status}">${token.status}</span></td></tr>
                                <tr><th>Category</th><td>${token.category || 'Generic'}</td></tr>
                                <tr><th>Position</th><td>${token.position ? `${token.position.lat.toFixed(6)}, ${token.position.lng.toFixed(6)}` : 'Not placed'}</td></tr>
                            </tbody>
                        </table>`;

            // Military Units Section
            if (brigades.length > 0) {
                detailsHtml += `
                    <div class="token-data-section">
                        <h4><i class="fas fa-flag"></i> Military Units (${brigades.length} Brigades)</h4>
                        <table class="table table-sm table-striped table-dark" style="width:100%;">
                            <thead>
                                <tr>
                                    <th>Brigade</th>
                                    <th>Code</th>
                                    <th>Strength</th>
                                    <th>Companies</th>
                                    <th>Squadrons</th>
                                    <th>Batteries</th>
                                </tr>
                            </thead>
                            <tbody>`;

                brigades.forEach(brigade => {
                    const brigadeUnits = [
                        ...infantryBattalions.filter(unit => unit.brigadeId === brigade.id),
                        ...armouredRegiments.filter(unit => unit.brigadeId === brigade.id),
                        ...artilleryRegiments.filter(unit => unit.brigadeId === brigade.id)
                    ];
                    
                    detailsHtml += `
                                <tr>
                                    <td>${brigade.name}</td>
                                    <td>${brigade.unitCode}</td>
                                    <td>${brigade.strength}</td>
                                    <td>${brigade.companies || 0}</td>
                                    <td>${brigade.squadrons || 0}</td>
                                    <td>${brigade.batteries || 0}</td>
                                </tr>`;
                    
                    if (brigadeUnits.length > 0) {
                        detailsHtml += `
                            <tr>
                                <td colspan="6" style="padding:0;">
                                    <table class="table table-sm table-bordered table-dark" style="margin:0;">
                                        <thead>
                                            <tr>
                                                <th>Unit</th>
                                                <th>Type</th>
                                                <th>Code</th>
                                                <th>Strength</th>
                                                <th>Drones</th>
                                            </tr>
                                        </thead>
                                        <tbody>`;
                        brigadeUnits.forEach(unit => {
                            const unitType = unit.unitType || (unit.id && unit.id.indexOf('armoured') >= 0 ? 'Armoured' : (unit.id && unit.id.indexOf('artillery') >= 0 ? 'Artillery' : 'Infantry'));
                            detailsHtml += `
                                            <tr>
                                                <td>${unit.name}</td>
                                                <td>${unitType}</td>
                                                <td>${unit.unitCode}</td>
                                                <td>${unit.strength}</td>
                                                <td>${unit.drones || 0}</td>
                                            </tr>`;
                        });
                        detailsHtml += `
                                        </tbody>
                                    </table>
                                </td>
                            </tr>`;
                    }
                });
                detailsHtml += `
                            </tbody>
                        </table>
                    </div>`;
            } else {
                detailsHtml += `
                    <div class="token-data-section">
                        <h4><i class="fas fa-flag"></i> Military Units</h4>
                        <p>No military units assigned to this token.</p>
                    </div>`;
            }

            // Intelligence Section
            if (intelligence.length > 0) {
                detailsHtml += `
                    <div class="token-data-section">
                        <h4><i class="fas fa-eye"></i> Intelligence Reports (${intelligence.length})</h4>
                        <table class="table table-sm table-striped table-dark" style="width:100%;">
                            <thead>
                                <tr>
                                    <th>Title</th>
                                    <th>Source</th>
                                    <th>Priority</th>
                                </tr>
                            </thead>
                            <tbody>`;
                intelligence.forEach(intel => {
                    detailsHtml += `
                                <tr>
                                    <td>${intel.title}</td>
                                    <td>${intel.source || 'Unknown'}</td>
                                    <td>${intel.priority || 'Medium'}</td>
                                </tr>`;
                });
                detailsHtml += `
                            </tbody>
                        </table>
                    </div>`;
            } else {
                detailsHtml += `
                    <div class="token-data-section">
                        <h4><i class="fas fa-eye"></i> Intelligence Reports</h4>
                        <p>No intelligence reports available.</p>
                    </div>`;
            }

            // Recon Section
            if (recon.length > 0) {
                detailsHtml += `
                    <div class="token-data-section">
                        <h4><i class="fas fa-binoculars"></i> Recon Data (${recon.length})</h4>
                        <table class="table table-sm table-striped table-dark" style="width:100%;">
                            <thead>
                                <tr>
                                    <th>Type</th>
                                    <th>Location</th>
                                    <th>Confidence</th>
                                </tr>
                            </thead>
                            <tbody>`;
                recon.forEach(reconItem => {
                    detailsHtml += `
                                <tr>
                                    <td>${reconItem.reconType}</td>
                                    <td>${reconItem.location || 'Unknown'}</td>
                                    <td>${reconItem.confidence || 'Medium'}</td>
                                </tr>`;
                });
                detailsHtml += `
                            </tbody>
                        </table>
                    </div>`;
            } else {
                detailsHtml += `
                    <div class="token-data-section">
                        <h4><i class="fas fa-binoculars"></i> Recon Data</h4>
                        <p>No reconnaissance data available.</p>
                    </div>`;
            }
            
            detailsHtml += `
                        <div class="token-actions">
                            <button class="btn btn-primary" onclick="closeTokenDetails(); openTokenDataEntryModal('${token.id}')">
                                <i class="fas fa-edit"></i> Edit Data
                            </button>
                        </div>
                    </div>
                </div>
            `;
            
            // Show the details panel
            $('#tokenDetailsPanel').html(detailsHtml);
            
        } catch (error) {
            console.error('Error loading token details:', error);
            $('#tokenDetailsPanel').html(`
                <div class="token-details-panel">
                    <div class="token-details-header">
                        <h3><i class="fas fa-map-marker-alt"></i> ${token.name || 'Unnamed Token'}</h3>
                        <button class="btn btn-sm btn-secondary" onclick="closeTokenDetails()">Close</button>
                    </div>
                    <div class="token-details-content">
                        <div class="error">Error loading token data: ${error.message}</div>
                    </div>
                </div>
            `);
        }
    }


    /**
     * Close token details
     */
    closeTokenDetails() {
        $('#tokenDetailsPanel').hide();
    }

    /**
     * Delete token by ID
     */
    async deleteTokenById(tokenId) {
        if (!confirm('Are you sure you want to delete this token?')) {
            return;
        }

        try {
            const response = await fetch(`/AdminToken/DeleteToken/${tokenId}`, {
                method: 'DELETE'
            });

            const result = await response.json();

            if (result.success) {
                // Remove from local tokens array
                this.tokens = this.tokens.filter(t => t.id !== tokenId);
                
                if (typeof showNotification === 'function') {
                    showNotification('Token deleted successfully', 'success');
                }
                
                // Refresh the token list
                this.refreshTokenList();
            } else {
                if (typeof showNotification === 'function') {
                    showNotification(result.message || 'Error deleting token', 'error');
                }
            }
        } catch (error) {
            console.error('Error deleting token:', error);
            if (typeof showNotification === 'function') {
                showNotification('Error deleting token', 'error');
            }
        }
    }

    /**
     * Open token data entry modal
     * Uses the single master data entry system
     */
    openTokenDataEntryModal(tokenId) {
        console.log('🎯 TokenManager - Using master data entry system');
        
        // Single function call - no legacy, no fallbacks
        if (typeof window.openDataEntry === 'function') {
            window.openDataEntry();
        } else {
            console.error('❌ Master data entry system not available');
        }
    }

    /**
     * Get all tokens
     */
    getTokens() {
        return this.tokens;
    }

    /**
     * Get placed tokens
     */
    getPlacedTokens() {
        return this.tokenPlacementManager ? this.tokenPlacementManager.getPlacedTokens() : [];
    }

    /**
     * Check if token is placed
     */
    isTokenPlaced(tokenId) {
        return this.tokenPlacementManager ? this.tokenPlacementManager.isTokenPlaced(tokenId) : false;
    }

    /**
     * Token Selection Modal Functions
     */

    /**
     * Open token selection modal for placement
     */
    async openTokenSelectionModal() {
        console.log('🎯 Opening token selection modal...');
        
        try {
            // Check if modal already exists
            const existingModal = document.getElementById('tokenSelectionModal');
            if (existingModal) {
                console.log('Modal already exists, opening directly...');
                this.openModal('tokenSelectionModal');
                this.attachTabHandlers();
                if (!this.tokensLoaded) {
                    this.tokensLoaded = true;
                    this.loadAvailableTokens();
                }
                return;
            }
            
            // Load the modal using lazy loader
            if (typeof lazyLoader !== 'undefined') {
                console.log('Loading modal via lazy loader...');
                await lazyLoader.loadPartial('token-selection-modal', '#modalsContainer', {
                    onLoaded: () => {
                        console.log('Modal loaded successfully');
                        this.attachTabHandlers();
                        setTimeout(() => {
                            this.openModal('tokenSelectionModal');
                            if (!this.tokensLoaded) {
                                this.tokensLoaded = true;
                                this.loadAvailableTokens();
                            }
                        }, 100);
                    }
                });
            } else {
                console.error('LazyLoader not available');
                this.showNotification('LazyLoader not available', 'error');
            }
        } catch (error) {
            console.error('Failed to load token selection modal:', error);
            this.showNotification('Failed to load token selection modal', 'error');
        }
    }

    /**
     * Close token selection modal
     */
    closeTokenSelectionModal() {
        this.closeModal('tokenSelectionModal');
        
        // Reset map cursor if in placement mode
        if (window.gameMap) {
            window.gameMap.getContainer().style.cursor = 'default';
        }
    }

    /**
     * Load available tokens for selection
     */
    async loadAvailableTokens() {
        console.log('🎯 Loading available tokens...');
        
        // Prevent multiple simultaneous requests
        if (this.isLoadingTokens) {
            console.log('⏳ Already loading tokens, skipping...');
            return;
        }
        
        this.isLoadingTokens = true;
        
        try {
            const response = await fetch('/GamePlay/GetTeamTokens');
            const data = await response.json();
            
            console.log('📋 Token data received:', data);
            
            if (data.success) {
                let tokens = Array.isArray(data.tokens) ? data.tokens : [];
                // Build exclusion set from multiple sources (server + client state)
                let placedIds = new Set();
                try {
                    const placed = await this.getAllPlacedTokens();
                    (placed || []).forEach(p => placedIds.add(p.id || p.Id));
                } catch {}
                if (this.tokenPlacementManager && this.tokenPlacementManager.placedTokens) {
                    for (const [pid] of this.tokenPlacementManager.placedTokens) placedIds.add(pid);
                }
                // no cache

                // Exclude any token that is placed by id or by coordinates/status
                tokens = tokens.filter(t => {
                    const tid = t.id || t.Id;
                    if (placedIds.has(tid)) return false;
                    return !this.isTokenAlreadyPlaced(t);
                });

                this.populateTokenSelection(tokens);
                console.log('✅ Tokens populated successfully');
            } else {
                console.error('❌ Failed to load tokens:', data.message);
                this.showTokenError('Failed to load tokens: ' + data.message);
            }
        } catch (error) {
            console.error('❌ Error loading tokens:', error);
            this.showTokenError('Error connecting to server');
        } finally {
            this.isLoadingTokens = false;
            console.log('🏁 Token loading process complete');
        }
    }

    /**
     * Determine whether a token is already placed on the map
     */
    isTokenAlreadyPlaced(token) {
        // Consider placed ONLY if server provided a non-null position
        const statusIsPlaced = false;
        const hasPosition = (token.position && (token.position.lat != null && token.position.lng != null));

        const placedByManager = this.tokenPlacementManager && this.tokenPlacementManager.isTokenPlaced && token.id && this.tokenPlacementManager.isTokenPlaced(token.id);

        return hasPosition || placedByManager;
    }

    /**
     * Populate token selection grid
     */
    populateTokenSelection(tokens) {
        console.log(`🔢 Populating ${tokens.length} tokens`);
        const container = document.getElementById('tokenSelectionGrid');
        
        if (!container) {
            console.error('❌ Token selection grid container not found');
            return;
        }
        
        if (!Array.isArray(tokens) || tokens.length === 0) {
            container.innerHTML = '<div class="no-tokens">No tokens available for placement</div>';
            return;
        }
        
        const tokenCards = tokens.map((token, index) => this.createTokenCard(token, index)).join('');
        container.innerHTML = tokenCards;
        
        console.log(`✅ ${tokens.length} tokens displayed in selection grid`);
    }

    /**
     * Create token card HTML
     */
    createTokenCard(token, index) {
        const hasImage = token.assetImagePath && token.assetImagePath.trim() !== '';
        const status = token.status || 'created';
        const isPlaced = this.isTokenAlreadyPlaced(token);
        
        // Image thumbnail with fallback and name overlay
        const imageThumbnail = hasImage 
            ? `<img src="${token.assetImagePath}" alt="${token.name}" class="token-card-image" title="${token.name}" onerror="this.style.display='none'; this.nextElementSibling.style.display='flex';">
               <div class="token-placeholder" style="display:none;">
                   <i class="${this.getTokenIcon(token)}"></i>
               </div>
               <div class="token-name-overlay">${token.name || 'Unnamed Token'}</div>`
            : `<div class="token-placeholder">
                   <i class="${this.getTokenIcon(token)}"></i>
               </div>
               <div class="token-name-overlay">${token.name || 'Unnamed Token'}</div>`;
        
        const cardHtml = `
            <div class="token-selection-card" ${!isPlaced ? `onclick=\"tokenManager.selectTokenForPlacement(${JSON.stringify(token).replace(/"/g, '&quot;')})\"` : ''} data-token-id="${token.id}">
                <div class="token-status-indicator ${status}"></div>
                ${imageThumbnail}
            </div>`;

        if (!isPlaced) {
            return cardHtml;
        }

        // For placed tokens, append a quick action button below the card
        return `
            <div>
                ${cardHtml}
                <div style="margin-top:8px; text-align:center;">
                    <button type="button" class="gameplay-btn" onclick="tokenManager.quickRemoveFromMap('${token.id}')">Remove from map</button>
                </div>
            </div>
        `;
    }

    /**
     * Get fallback background for tokens without images
     */
    getFallbackBackground(token, index) {
        const gradients = [
            'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
            'linear-gradient(135deg, #f093fb 0%, #f5576c 100%)',
            'linear-gradient(135deg, #4facfe 0%, #00f2fe 100%)',
            'linear-gradient(135deg, #43e97b 0%, #38f9d7 100%)',
            'linear-gradient(135deg, #fa709a 0%, #fee140 100%)',
            'linear-gradient(135deg, #a8edea 0%, #fed6e3 100%)',
            'linear-gradient(135deg, #ff9a9e 0%, #fecfef 100%)',
            'linear-gradient(135deg, #a1c4fd 0%, #c2e9fb 100%)'
        ];
        
        const gradientIndex = index % gradients.length;
        return `background: ${gradients[gradientIndex]};`;
    }

    /**
     * Get token icon based on type/category
     */
    getTokenIcon(token) {
        // Map token types to icons
        const iconMap = {
            'infantry': 'fas fa-running',
            'armor': 'fas fa-shield-alt',
            'artillery': 'fas fa-crosshairs',
            'air': 'fas fa-plane',
            'naval': 'fas fa-ship',
            'default': 'fas fa-map-marker-alt'
        };
        
        const type = (token.type || token.tokenGroupName || 'default').toLowerCase();
        return iconMap[type] || iconMap.default;
    }

    /**
     * Select token for placement
     */
    selectTokenForPlacement(token) {
        console.log('🎯 Selected token for placement:', token.name);
        
        // Store the selected token for placement
        this.selectedTokenForPlacement = token;
        
        // Close the modal
        this.closeTokenSelectionModal();
        
        // Start placement mode using TokenPlacementManager
        if (this.tokenPlacementManager) {
            this.tokenPlacementManager.startPlacementMode(token);
            console.log('💡 Using TokenPlacementManager for token placement');
        } else {
            console.warn('⚠️ TokenPlacementManager not available, using fallback');
            // Fallback to basic placement mode
            if (window.gameMap) {
                window.gameMap.getContainer().style.cursor = 'crosshair';
                this.showNotification(`📢 Click on the map to place ${token.name}`, 'info');
            }
        }
    }

    /**
     * Show token error message
     */
    showTokenError(message) {
        const container = document.getElementById('tokenSelectionGrid');
        if (container) {
            container.innerHTML = `<div class="token-error">${message}</div>`;
        }
        console.error('Token Error:', message);
    }

    /**
     * Quick remove a placed token from the map and refresh lists
     */
    async quickRemoveFromMap(tokenId) {
        try {
            if (this.tokenPlacementManager) {
                await this.tokenPlacementManager.removeTokenFromMap(tokenId);
            }
            // no cache

            // Refresh current tab
            const placedBtn = document.getElementById('tokenTabPlaced');
            const availableBtn = document.getElementById('tokenTabAvailable');
            if (placedBtn && placedBtn.classList.contains('active')) {
                const tokens = await this.getAllPlacedTokens();
                this.populateTokenSelection((tokens || []).filter(t => t.position && t.position.lat != null && t.position.lng != null));
            } else if (availableBtn && availableBtn.classList.contains('active')) {
                await this.loadAvailableTokens();
            } else {
                // Default to refreshing available list
                await this.loadAvailableTokens();
            }

            if (typeof showNotification === 'function') {
                showNotification('Token removed from map', 'success');
            }
        } catch (error) {
            console.error('Error removing token from map:', error);
            if (typeof showNotification === 'function') {
                showNotification('Error removing token from map', 'error');
            }
        }
    }

    /**
     * Open modal utility
     */
    openModal(modalId) {
        const modal = document.getElementById(modalId);
        if (modal) {
            modal.style.display = 'block';
            document.body.classList.add('modal-open');
            console.log(`✅ Modal opened: ${modalId}`);
        } else {
            // Try camelCase version
            const camelCaseId = modalId.replace(/-([a-z])/g, (g) => g[1].toUpperCase());
            const camelModal = document.getElementById(camelCaseId);
            if (camelModal) {
                camelModal.style.display = 'block';
                document.body.classList.add('modal-open');
                console.log(`✅ Modal opened: ${camelCaseId}`);
            } else {
                console.error(`❌ Modal not found: ${modalId} or ${camelCaseId}`);
            }
        }
    }

    /**
     * Close modal utility
     */
    closeModal(modalId) {
        const modal = document.getElementById(modalId);
        if (modal) {
            modal.style.display = 'none';
            document.body.classList.remove('modal-open');
            console.log(`✅ Modal closed: ${modalId}`);
        } else {
            // Try camelCase version
            const camelCaseId = modalId.replace(/-([a-z])/g, (g) => g[1].toUpperCase());
            const camelModal = document.getElementById(camelCaseId);
            if (camelModal) {
                camelModal.style.display = 'none';
                document.body.classList.remove('modal-open');
                console.log(`✅ Modal closed: ${camelCaseId}`);
            } else {
                console.error(`❌ Modal not found: ${modalId} or ${camelCaseId}`);
            }
        }
    }

    /**
     * Show notification utility
     */
    showNotification(message, type = 'info') {
        console.log(`📢 ${type.toUpperCase()}: ${message}`);
        
        // Use external notification system if available
        if (typeof showNotification === 'function') {
            showNotification(message, type);
        }
        
        // You can also implement a simple notification display here
        // For now, we'll just log to console
    }

    /**
     * Save placed tokens to localStorage
     */
    savePlacedTokensToCache(tokenData, position) {
        try {
            const placedToken = {
                ...tokenData,
                latitude: position.lat,
                longitude: position.lng,
                placedAt: new Date().toISOString()
            };
            
            // Add to cache
            const existingIndex = this.placedTokensCache.findIndex(t => t.id === tokenData.id);
            if (existingIndex >= 0) {
                this.placedTokensCache[existingIndex] = placedToken;
            } else {
                this.placedTokensCache.push(placedToken);
            }
            
            // Save to localStorage
            localStorage.setItem('gamePlay_placedTokens', JSON.stringify(this.placedTokensCache));
            console.log(`💾 Saved token ${tokenData.name} to cache`);
        } catch (error) {
            console.error('Error saving placed token to cache:', error);
        }
    }

    /**
     * Load placed tokens from localStorage
     */
    loadPlacedTokensFromCache() {
        try {
            const cached = localStorage.getItem('gamePlay_placedTokens');
            if (cached) {
                this.placedTokensCache = JSON.parse(cached);
                console.log(`📂 Loaded ${this.placedTokensCache.length} placed tokens from cache`);
                return this.placedTokensCache;
            }
        } catch (error) {
            console.error('Error loading placed tokens from cache:', error);
        }
        return [];
    }

    /**
     * Attach tab click handlers
     */
    attachTabHandlers() {
        const availableBtn = document.getElementById('tokenTabAvailable');
        const placedBtn = document.getElementById('tokenTabPlaced');
        if (!availableBtn || !placedBtn) return;

        availableBtn.onclick = async () => {
            availableBtn.classList.add('active');
            placedBtn.classList.remove('active');
            await this.loadAvailableTokens();
        };

        placedBtn.onclick = async () => {
            placedBtn.classList.add('active');
            availableBtn.classList.remove('active');
            const tokens = await this.getAllPlacedTokens();
            this.populateTokenSelection((tokens || []).filter(t => t.position && t.position.lat != null && t.position.lng != null));
        };
    }

    /**
     * Remove token from cache
     */
    removeTokenFromCache(tokenId) {
        try {
            this.placedTokensCache = this.placedTokensCache.filter(t => t.id !== tokenId);
            localStorage.setItem('gamePlay_placedTokens', JSON.stringify(this.placedTokensCache));
            console.log(`🗑️ Removed token ${tokenId} from cache`);
        } catch (error) {
            console.error('Error removing token from cache:', error);
        }
    }

    /**
     * Enable token movement (dragging)
     */
    enableTokenMovement() {
        if (this.tokenPlacementManager) {
            this.tokenPlacementManager.enableTokenDragging();
        }
        console.log('✅ Token movement enabled');
    }

    /**
     * Disable token movement (dragging)
     */
    disableTokenMovement() {
        if (this.tokenPlacementManager) {
            this.tokenPlacementManager.disableTokenDragging();
        }
        console.log('🚫 Token movement disabled');
    }

    /**
     * Enable token selection only
     */
    enableTokenSelection() {
        if (this.tokenPlacementManager) {
            this.tokenPlacementManager.enableTokenSelection();
        }
        console.log('✅ Token selection enabled');
    }

    /**
     * Find token at location
     */
    findTokenAtLocation(latlng) {
        console.log('🔍 TokenManager.findTokenAtLocation called with:', latlng);
        console.log('🔍 TokenManager has tokenPlacementManager:', !!this.tokenPlacementManager);
        
        if (this.tokenPlacementManager) {
            const result = this.tokenPlacementManager.findTokenAtLocation(latlng);
            console.log('🔍 TokenManager returning result:', result);
            return result;
        }
        
        console.log('❌ TokenManager has no tokenPlacementManager');
        return null;
    }

    /**
     * Get all placed tokens (from cache and server)
     */
    async getAllPlacedTokens() {
        try {
            // First try to get from server
            const response = await fetch('/GamePlay/GetPlacedTokens');
            const result = await response.json();
            
            if (result.success && result.tokens) {
                return result.tokens;
            }
        } catch (error) {
            console.warn('Could not load placed tokens from server, using cache:', error);
        }
        
        // Fallback to cache
        return this.loadPlacedTokensFromCache();
    }

    /**
     * Clean up resources
     */
    destroy() {
        if (this.tokenPlacementManager) {
            this.tokenPlacementManager.destroy();
        }
        this.tokens = [];
        this.placedTokens.clear();
        this.isInitialized = false;
        this.isLoadingTokens = false;
        this.tokensLoaded = false;
        this.selectedTokenForPlacement = null;
        this.placedTokensCache = [];
    }
}

// Create global instance
const tokenManager = new TokenManager();

// Export for use in other modules
if (typeof module !== 'undefined' && module.exports) {
    module.exports = TokenManager;
}
