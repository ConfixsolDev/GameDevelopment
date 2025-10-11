/**
 * Interoperability Service
 * Integrates HLA adapter with existing KSA Wargame systems
 * Provides seamless interoperability with external military simulation systems
 * Supports NATO NETN, C-BML, MSDL standards
 */

class InteroperabilityService {
    constructor() {
        this.hlaAdapter = null;
        this.tokenManager = null;
        this.simulationEngine = null;
        this.interoperabilityEnabled = false;
        this.externalSystems = new Map();
        
        this.initializeInteroperability();
    }

    /**
     * Initialize interoperability service
     */
    initializeInteroperability() {
        console.log('🌐 Initializing Interoperability Service...');
        
        // Check for required services
        this.checkRequiredServices();
        
        // Setup integration
        this.setupIntegration();
        
        // Setup event listeners
        this.setupEventListeners();
        
        console.log('✅ Interoperability Service initialized');
    }

    /**
     * Check for required services
     */
    checkRequiredServices() {
        // Check for HLA adapter
        if (typeof HlaIntegrationAdapter !== 'undefined' && window.hlaIntegrationAdapter) {
            this.hlaAdapter = window.hlaIntegrationAdapter;
            console.log('✅ HLA Integration Adapter connected');
        } else {
            console.warn('⚠️ HLA Integration Adapter not available');
        }
        
        // Check for token manager
        if (window.tokenPlacementManager) {
            this.tokenManager = window.tokenPlacementManager;
            console.log('✅ Token Placement Manager connected');
        } else {
            console.warn('⚠️ Token Placement Manager not available');
        }
        
        // Check for simulation engine
        if (window.combatSimulationEngine) {
            this.simulationEngine = window.combatSimulationEngine;
            console.log('✅ Combat Simulation Engine connected');
        } else {
            console.warn('⚠️ Combat Simulation Engine not available');
        }
    }

    /**
     * Setup integration
     */
    setupIntegration() {
        if (this.hlaAdapter && this.tokenManager) {
            this.integrateTokenManagement();
        }
        
        if (this.hlaAdapter && this.simulationEngine) {
            this.integrateSimulationEngine();
        }
    }

    /**
     * Setup event listeners
     */
    setupEventListeners() {
        // Listen for HLA connection status
        window.addEventListener('hla_connection_status', (event) => {
            this.handleConnectionStatusChange(event.detail);
        });

        // Listen for HLA object updates
        window.addEventListener('hla_object_update', (event) => {
            this.handleHlaObjectUpdate(event.detail);
        });

        // Listen for HLA interactions
        window.addEventListener('hla_interaction', (event) => {
            this.handleHlaInteraction(event.detail);
        });

        // Listen for local simulation events
        window.addEventListener('simulation_event', (event) => {
            this.handleLocalSimulationEvent(event.detail);
        });
    }

    /**
     * Integrate with token management
     */
    integrateTokenManagement() {
        console.log('🔗 Integrating HLA with token management...');
        
        // Add external token management methods to token manager
        if (!this.tokenManager.addExternalToken) {
            this.tokenManager.addExternalToken = (tokenData) => {
                console.log('📥 Adding external token from HLA:', tokenData.id);
                
                // Mark token as external
                tokenData.external = true;
                tokenData.editable = false; // External tokens can't be edited locally
                
                // Use existing token placement logic
                if (this.tokenManager.allTokens) {
                    this.tokenManager.allTokens.push(tokenData);
                }
                
                // Visualize on map
                if (this.tokenManager.visualizeToken) {
                    this.tokenManager.visualizeToken(tokenData);
                }
            };
        }
        
        if (!this.tokenManager.updateExternalToken) {
            this.tokenManager.updateExternalToken = (tokenId, updates) => {
                console.log('🔄 Updating external token from HLA:', tokenId);
                
                if (this.tokenManager.allTokens) {
                    const token = this.tokenManager.allTokens.find(t => t.id === tokenId);
                    if (token) {
                        Object.assign(token, updates);
                        
                        // Update visualization
                        if (this.tokenManager.updateTokenVisualization) {
                            this.tokenManager.updateTokenVisualization(token);
                        }
                    }
                }
            };
        }
        
        if (!this.tokenManager.removeExternalToken) {
            this.tokenManager.removeExternalToken = (tokenId) => {
                console.log('🗑️ Removing external token from HLA:', tokenId);
                
                if (this.tokenManager.allTokens) {
                    const index = this.tokenManager.allTokens.findIndex(t => t.id === tokenId);
                    if (index > -1) {
                        this.tokenManager.allTokens.splice(index, 1);
                        
                        // Remove from map
                        if (this.tokenManager.removeTokenFromMap) {
                            this.tokenManager.removeTokenFromMap(tokenId);
                        }
                    }
                }
            };
        }
        
        console.log('✅ Token management integration complete');
    }

    /**
     * Integrate with simulation engine
     */
    integrateSimulationEngine() {
        console.log('🔗 Integrating HLA with simulation engine...');
        
        // Enhance simulation engine with HLA support
        if (!this.simulationEngine.hlaEnabled) {
            this.simulationEngine.hlaEnabled = false;
        }
        
        // Add method to enable HLA for simulation
        this.simulationEngine.enableHla = () => {
            this.simulationEngine.hlaEnabled = true;
            console.log('✅ HLA enabled for simulation engine');
        };
        
        this.simulationEngine.disableHla = () => {
            this.simulationEngine.hlaEnabled = false;
            console.log('✅ HLA disabled for simulation engine');
        };
        
        console.log('✅ Simulation engine integration complete');
    }

    /**
     * Handle connection status change
     */
    handleConnectionStatusChange(status) {
        console.log('🔗 HLA connection status changed:', status.status);
        
        this.interoperabilityEnabled = status.status === 'connected';
        
        // Update UI if status panel exists
        if (document.getElementById('hlaStatusPanel')) {
            this.updateHlaStatusPanel(status);
        }
    }

    /**
     * Handle HLA object update
     */
    handleHlaObjectUpdate(update) {
        console.log('🔄 HLA object update received:', update.className);
        
        // Route to appropriate handler based on object class
        switch (update.className) {
            case 'PhysicalEntity':
                this.handlePhysicalEntityUpdate(update);
                break;
            case 'AggregateEntity':
                this.handleAggregateEntityUpdate(update);
                break;
            case 'MovementOrder':
                this.handleMovementOrderUpdate(update);
                break;
            default:
                console.log('Unhandled object class update:', update.className);
        }
    }

    /**
     * Handle HLA interaction
     */
    handleHlaInteraction(interaction) {
        console.log('⚡ HLA interaction received:', interaction.className);
        
        // Process interaction
        // This is already handled by HLA adapter
    }

    /**
     * Handle local simulation event
     */
    handleLocalSimulationEvent(event) {
        if (!this.interoperabilityEnabled) return;
        
        console.log('📤 Local simulation event to HLA:', event.type);
        
        // HLA adapter already handles this
    }

    /**
     * Handle physical entity update
     */
    handlePhysicalEntityUpdate(update) {
        if (!this.tokenManager) return;
        
        const tokenId = `hla_${update.handle}`;
        
        // Check if token exists
        const existingToken = this.tokenManager.allTokens?.find(t => t.id === tokenId);
        
        if (existingToken) {
            // Update existing token
            this.tokenManager.updateExternalToken(tokenId, {
                position: update.attributes.Spatial.position,
                orientation: update.attributes.Spatial.orientation,
                supply: update.attributes.SupplyLevel,
                ammo: update.attributes.AmmoLevel,
                fuel: update.attributes.FuelLevel
            });
        } else {
            // Add new token
            this.hlaAdapter.integrateExternalToken({
                handle: update.handle,
                className: update.className,
                attributes: update.attributes
            });
        }
    }

    /**
     * Handle aggregate entity update
     */
    handleAggregateEntityUpdate(update) {
        console.log('🔄 Aggregate entity update:', update.handle);
        // Handle formation/group updates
    }

    /**
     * Handle movement order update
     */
    handleMovementOrderUpdate(update) {
        console.log('🚶 Movement order update:', update.handle);
        // Handle movement order from external system
    }

    /**
     * Update HLA status panel
     */
    updateHlaStatusPanel(status) {
        const panel = document.getElementById('hlaStatusPanel');
        if (!panel) return;
        
        const statusClass = status.status === 'connected' ? 'connected' : 'disconnected';
        const statusIcon = status.status === 'connected' ? '🟢' : '🔴';
        
        panel.innerHTML = `
            <div class="hla-status ${statusClass}">
                <span class="status-icon">${statusIcon}</span>
                <span class="status-text">${status.status.toUpperCase()}</span>
                <span class="federation-name">${status.federationName}</span>
            </div>
        `;
    }

    /**
     * Enable interoperability
     */
    async enableInteroperability(federationName, federateType) {
        if (!this.hlaAdapter) {
            throw new Error('HLA adapter not available');
        }
        
        try {
            await this.hlaAdapter.connectToFederation(federationName, federateType);
            this.interoperabilityEnabled = true;
            console.log('✅ Interoperability enabled');
            
            return { success: true };
        } catch (error) {
            console.error('❌ Failed to enable interoperability:', error);
            throw error;
        }
    }

    /**
     * Disable interoperability
     */
    async disableInteroperability() {
        if (!this.hlaAdapter) {
            return;
        }
        
        try {
            await this.hlaAdapter.disconnectFromFederation();
            this.interoperabilityEnabled = false;
            console.log('✅ Interoperability disabled');
            
            return { success: true };
        } catch (error) {
            console.error('❌ Failed to disable interoperability:', error);
            throw error;
        }
    }

    /**
     * Get interoperability status
     */
    getInteroperabilityStatus() {
        return {
            enabled: this.interoperabilityEnabled,
            hlaStatus: this.hlaAdapter ? this.hlaAdapter.getConnectionStatus() : null,
            externalSystems: Array.from(this.externalSystems.keys()),
            timestamp: new Date().toISOString()
        };
    }

    /**
     * Register external system
     */
    registerExternalSystem(systemInfo) {
        const systemId = systemInfo.id || this.generateSystemId();
        
        this.externalSystems.set(systemId, {
            id: systemId,
            name: systemInfo.name,
            type: systemInfo.type,
            protocol: systemInfo.protocol || 'HLA',
            status: 'registered',
            registeredAt: new Date().toISOString()
        });
        
        console.log(`✅ Registered external system: ${systemInfo.name}`);
        return systemId;
    }

    /**
     * Unregister external system
     */
    unregisterExternalSystem(systemId) {
        if (this.externalSystems.has(systemId)) {
            this.externalSystems.delete(systemId);
            console.log(`✅ Unregistered external system: ${systemId}`);
        }
    }

    /**
     * Generate system ID
     */
    generateSystemId() {
        return `sys_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
    }
}

// Initialize global instance
window.interoperabilityService = new InteroperabilityService();
console.log('🌐 Interoperability Service initialized');
