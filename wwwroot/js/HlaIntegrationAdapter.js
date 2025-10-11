/**
 * HLA (High Level Architecture) Integration Adapter
 * Implements NATO HLA/NETN standards for distributed simulation
 * Enables interoperability with other HLA-compliant military simulation systems
 * Based on NATO NETN-FOM (NATO Education and Training Network - Federation Object Model)
 */

class HlaIntegrationAdapter {
    constructor() {
        this.federationName = 'KSA_WARGAME_FEDERATION';
        this.federateType = 'WARGAME_CLIENT';
        this.connected = false;
        this.federationExecutionHandle = null;
        this.objectClasses = new Map();
        this.interactionClasses = new Map();
        this.registeredObjects = new Map();
        this.subscriptions = new Map();
        
        this.initializeHlaAdapter();
    }

    /**
     * Initialize HLA adapter
     */
    initializeHlaAdapter() {
        console.log('🔗 Initializing HLA Integration Adapter...');
        
        // Initialize NATO NETN object classes
        this.initializeNetnObjectClasses();
        
        // Initialize interaction classes
        this.initializeInteractionClasses();
        
        // Setup message handlers
        this.setupMessageHandlers();
        
        console.log('✅ HLA Integration Adapter initialized');
    }

    /**
     * Initialize NATO NETN object classes
     */
    initializeNetnObjectClasses() {
        // NETN-Physical: Physical entities (units, equipment)
        this.objectClasses.set('PhysicalEntity', {
            class: 'HLAobjectRoot.BaseEntity.PhysicalEntity',
            attributes: [
                'EntityIdentifier',
                'EntityType',
                'Spatial',
                'ForceIdentifier',
                'Marking',
                'DamageState',
                'SupplyLevel',
                'AmmoLevel',
                'FuelLevel'
            ],
            publishable: true,
            subscribable: true
        });

        // NETN-Aggregate: Aggregated units (formations)
        this.objectClasses.set('AggregateEntity', {
            class: 'HLAobjectRoot.BaseEntity.AggregateEntity',
            attributes: [
                'AggregateIdentifier',
                'AggregateType',
                'ForceIdentifier',
                'Formation',
                'AggregateState',
                'SubordinateList',
                'Location',
                'Orientation'
            ],
            publishable: true,
            subscribable: true
        });

        // NETN-MRM: Maneuver/Movement
        this.objectClasses.set('MovementOrder', {
            class: 'HLAobjectRoot.Order.MovementOrder',
            attributes: [
                'OrderIdentifier',
                'IssuingCommander',
                'ReceivingUnit',
                'Waypoints',
                'FormationType',
                'Speed',
                'Priority'
            ],
            publishable: true,
            subscribable: true
        });

        // NETN-LOG: Logistics
        this.objectClasses.set('SupplyPoint', {
            class: 'HLAobjectRoot.BaseEntity.SupplyPoint',
            attributes: [
                'SupplyPointIdentifier',
                'Location',
                'SupplyTypes',
                'CapacityLevel',
                'SupplyRate'
            ],
            publishable: true,
            subscribable: true
        });

        // NETN-CBRN: CBRN (Chemical, Biological, Radiological, Nuclear)
        this.objectClasses.set('CBRNEvent', {
            class: 'HLAobjectRoot.Event.CBRNEvent',
            attributes: [
                'EventIdentifier',
                'EventType',
                'Location',
                'Severity',
                'Timestamp',
                'AffectedArea'
            ],
            publishable: true,
            subscribable: false
        });

        // NETN-TMR: Task/Mission/Request
        this.objectClasses.set('MissionTask', {
            class: 'HLAobjectRoot.Task.MissionTask',
            attributes: [
                'TaskIdentifier',
                'TaskType',
                'AssignedUnit',
                'Objective',
                'Priority',
                'Status',
                'StartTime',
                'Deadline'
            ],
            publishable: true,
            subscribable: true
        });

        console.log(`📋 Initialized ${this.objectClasses.size} NETN object classes`);
    }

    /**
     * Initialize interaction classes
     */
    initializeInteractionClasses() {
        // Weapon Fire interaction
        this.interactionClasses.set('WeaponFire', {
            class: 'HLAinteractionRoot.Warfare.WeaponFire',
            parameters: [
                'FiringEntity',
                'TargetEntity',
                'MunitionType',
                'WarheadType',
                'FuseType',
                'Quantity',
                'FireMissionIndex'
            ],
            publishable: true,
            subscribable: true
        });

        // Detonation interaction
        this.interactionClasses.set('Detonation', {
            class: 'HLAinteractionRoot.Warfare.Detonation',
            parameters: [
                'DetonationLocation',
                'MunitionType',
                'WarheadType',
                'FiringEntity',
                'TargetEntity',
                'DetonationResult',
                'Casualties'
            ],
            publishable: true,
            subscribable: true
        });

        // Communication interaction
        this.interactionClasses.set('RadioTransmission', {
            class: 'HLAinteractionRoot.Communication.RadioTransmission',
            parameters: [
                'TransmittingEntity',
                'ReceivingEntities',
                'RadioFrequency',
                'MessageContent',
                'Timestamp',
                'Priority'
            ],
            publishable: true,
            subscribable: true
        });

        // Command interaction
        this.interactionClasses.set('CommandIssued', {
            class: 'HLAinteractionRoot.Command.CommandIssued',
            parameters: [
                'CommandIdentifier',
                'IssuingCommander',
                'ReceivingUnits',
                'CommandType',
                'CommandContent',
                'Priority',
                'Timestamp'
            ],
            publishable: true,
            subscribable: true
        });

        // Simulation Control
        this.interactionClasses.set('SimulationControl', {
            class: 'HLAinteractionRoot.Manager.SimulationControl',
            parameters: [
                'ControlType',
                'FederateName',
                'Timestamp',
                'Parameters'
            ],
            publishable: true,
            subscribable: true
        });

        console.log(`📋 Initialized ${this.interactionClasses.size} interaction classes`);
    }

    /**
     * Setup message handlers
     */
    setupMessageHandlers() {
        // Listen for HLA messages from other systems
        window.addEventListener('hla_message', (event) => {
            this.handleHlaMessage(event.detail);
        });

        // Listen for simulation events to publish
        window.addEventListener('simulation_event', (event) => {
            this.handleSimulationEvent(event.detail);
        });
    }

    /**
     * Connect to HLA federation
     * @param {String} federationName - Name of the federation to join
     * @param {String} federateType - Type of federate
     * @returns {Promise} Connection promise
     */
    async connectToFederation(federationName, federateType) {
        console.log(`🔗 Connecting to HLA federation: ${federationName} as ${federateType}...`);
        
        try {
            // In a real implementation, this would connect to RTI (Runtime Infrastructure)
            // For now, we simulate the connection
            this.federationName = federationName || this.federationName;
            this.federateType = federateType || this.federateType;
            
            // Simulate connection delay
            await this.delay(1000);
            
            this.connected = true;
            this.federationExecutionHandle = this.generateHandle();
            
            // Publish and subscribe to object classes
            await this.publishAndSubscribe();
            
            console.log('✅ Connected to HLA federation');
            
            // Notify listeners
            this.notifyConnectionStatus('connected');
            
            return {
                success: true,
                federationName: this.federationName,
                federateType: this.federateType,
                handle: this.federationExecutionHandle
            };
            
        } catch (error) {
            console.error('❌ Failed to connect to HLA federation:', error);
            this.connected = false;
            this.notifyConnectionStatus('error', error.message);
            throw error;
        }
    }

    /**
     * Disconnect from HLA federation
     */
    async disconnectFromFederation() {
        console.log('🔗 Disconnecting from HLA federation...');
        
        try {
            // Unpublish all objects
            await this.unpublishAllObjects();
            
            // Unsubscribe from all classes
            this.subscriptions.clear();
            
            this.connected = false;
            this.federationExecutionHandle = null;
            
            console.log('✅ Disconnected from HLA federation');
            this.notifyConnectionStatus('disconnected');
            
            return { success: true };
            
        } catch (error) {
            console.error('❌ Failed to disconnect from HLA federation:', error);
            throw error;
        }
    }

    /**
     * Publish and subscribe to object classes
     */
    async publishAndSubscribe() {
        console.log('📡 Publishing and subscribing to object classes...');
        
        // Publish object classes we can create
        for (const [className, classInfo] of this.objectClasses) {
            if (classInfo.publishable) {
                await this.publishObjectClass(className);
            }
        }
        
        // Subscribe to object classes we want to receive
        for (const [className, classInfo] of this.objectClasses) {
            if (classInfo.subscribable) {
                await this.subscribeObjectClass(className);
            }
        }
        
        // Publish interaction classes
        for (const [className, classInfo] of this.interactionClasses) {
            if (classInfo.publishable) {
                await this.publishInteractionClass(className);
            }
        }
        
        // Subscribe to interaction classes
        for (const [className, classInfo] of this.interactionClasses) {
            if (classInfo.subscribable) {
                await this.subscribeInteractionClass(className);
            }
        }
        
        console.log('✅ Publish and subscribe complete');
    }

    /**
     * Publish object class
     */
    async publishObjectClass(className) {
        console.log(`📤 Publishing object class: ${className}`);
        // In a real implementation, this would call RTI publishObjectClassAttributes
        return { success: true, className: className };
    }

    /**
     * Subscribe to object class
     */
    async subscribeObjectClass(className) {
        console.log(`📥 Subscribing to object class: ${className}`);
        
        if (!this.subscriptions.has(className)) {
            this.subscriptions.set(className, {
                className: className,
                handlers: [],
                timestamp: new Date().toISOString()
            });
        }
        
        // In a real implementation, this would call RTI subscribeObjectClassAttributes
        return { success: true, className: className };
    }

    /**
     * Publish interaction class
     */
    async publishInteractionClass(className) {
        console.log(`📤 Publishing interaction class: ${className}`);
        // In a real implementation, this would call RTI publishInteractionClass
        return { success: true, className: className };
    }

    /**
     * Subscribe to interaction class
     */
    async subscribeInteractionClass(className) {
        console.log(`📥 Subscribing to interaction class: ${className}`);
        
        if (!this.subscriptions.has(className)) {
            this.subscriptions.set(className, {
                className: className,
                handlers: [],
                timestamp: new Date().toISOString()
            });
        }
        
        // In a real implementation, this would call RTI subscribeInteractionClass
        return { success: true, className: className };
    }

    /**
     * Register object instance
     * @param {String} className - Object class name
     * @param {Object} attributes - Object attributes
     * @returns {String} Object instance handle
     */
    async registerObjectInstance(className, attributes) {
        console.log(`📝 Registering object instance: ${className}`);
        
        if (!this.connected) {
            throw new Error('Not connected to HLA federation');
        }
        
        const classInfo = this.objectClasses.get(className);
        if (!classInfo) {
            throw new Error(`Unknown object class: ${className}`);
        }
        
        const instanceHandle = this.generateHandle();
        
        const instance = {
            handle: instanceHandle,
            className: className,
            attributes: attributes,
            timestamp: new Date().toISOString(),
            lastUpdate: new Date().toISOString()
        };
        
        this.registeredObjects.set(instanceHandle, instance);
        
        console.log(`✅ Registered object instance: ${instanceHandle}`);
        return instanceHandle;
    }

    /**
     * Update object attributes
     * @param {String} instanceHandle - Object instance handle
     * @param {Object} attributes - Updated attributes
     */
    async updateObjectAttributes(instanceHandle, attributes) {
        const instance = this.registeredObjects.get(instanceHandle);
        if (!instance) {
            throw new Error(`Object instance not found: ${instanceHandle}`);
        }
        
        // Merge updated attributes
        Object.assign(instance.attributes, attributes);
        instance.lastUpdate = new Date().toISOString();
        
        // In a real implementation, this would call RTI updateAttributeValues
        this.broadcastObjectUpdate(instance);
        
        console.log(`✅ Updated object instance: ${instanceHandle}`);
        return { success: true, handle: instanceHandle };
    }

    /**
     * Send interaction
     * @param {String} className - Interaction class name
     * @param {Object} parameters - Interaction parameters
     */
    async sendInteraction(className, parameters) {
        console.log(`📡 Sending interaction: ${className}`);
        
        if (!this.connected) {
            throw new Error('Not connected to HLA federation');
        }
        
        const classInfo = this.interactionClasses.get(className);
        if (!classInfo) {
            throw new Error(`Unknown interaction class: ${className}`);
        }
        
        const interaction = {
            className: className,
            parameters: parameters,
            timestamp: new Date().toISOString(),
            sender: this.federateType
        };
        
        // In a real implementation, this would call RTI sendInteraction
        this.broadcastInteraction(interaction);
        
        console.log(`✅ Sent interaction: ${className}`);
        return { success: true, className: className };
    }

    /**
     * Handle incoming HLA message
     */
    handleHlaMessage(message) {
        console.log('📨 Received HLA message:', message.type);
        
        switch (message.type) {
            case 'object_discovered':
                this.handleObjectDiscovered(message.data);
                break;
            case 'object_updated':
                this.handleObjectUpdated(message.data);
                break;
            case 'object_removed':
                this.handleObjectRemoved(message.data);
                break;
            case 'interaction_received':
                this.handleInteractionReceived(message.data);
                break;
            default:
                console.warn('Unknown HLA message type:', message.type);
        }
    }

    /**
     * Handle object discovered
     */
    handleObjectDiscovered(data) {
        console.log('🔍 Object discovered:', data.className);
        
        // Integrate with existing token management
        if (window.tokenPlacementManager && data.className === 'PhysicalEntity') {
            this.integrateExternalToken(data);
        }
    }

    /**
     * Handle object updated
     */
    handleObjectUpdated(data) {
        console.log('🔄 Object updated:', data.handle);
        
        // Update existing token if it's from external federation
        if (window.tokenPlacementManager) {
            this.updateExternalToken(data);
        }
    }

    /**
     * Handle object removed
     */
    handleObjectRemoved(data) {
        console.log('🗑️ Object removed:', data.handle);
        
        // Remove token if it's from external federation
        if (window.tokenPlacementManager) {
            this.removeExternalToken(data);
        }
    }

    /**
     * Handle interaction received
     */
    handleInteractionReceived(data) {
        console.log('⚡ Interaction received:', data.className);
        
        // Process interaction based on type
        switch (data.className) {
            case 'WeaponFire':
                this.handleWeaponFire(data);
                break;
            case 'Detonation':
                this.handleDetonation(data);
                break;
            case 'CommandIssued':
                this.handleCommandIssued(data);
                break;
            default:
                console.log('Unhandled interaction:', data.className);
        }
    }

    /**
     * Handle simulation event from local system
     */
    handleSimulationEvent(event) {
        if (!this.connected) return;
        
        console.log('📤 Publishing simulation event to HLA:', event.type);
        
        // Map simulation events to HLA interactions/updates
        switch (event.type) {
            case 'token_placed':
                this.publishTokenPlacement(event.data);
                break;
            case 'token_moved':
                this.publishTokenMovement(event.data);
                break;
            case 'attack_executed':
                this.publishAttackExecution(event.data);
                break;
            case 'combat_resolved':
                this.publishCombatResolution(event.data);
                break;
            default:
                // Not all events need to be published
                break;
        }
    }

    /**
     * Publish token placement to HLA
     */
    async publishTokenPlacement(tokenData) {
        const attributes = this.mapTokenToPhysicalEntity(tokenData);
        const handle = await this.registerObjectInstance('PhysicalEntity', attributes);
        
        // Store mapping for future updates
        if (!this.tokenToHlaMap) {
            this.tokenToHlaMap = new Map();
        }
        this.tokenToHlaMap.set(tokenData.id, handle);
    }

    /**
     * Publish token movement to HLA
     */
    async publishTokenMovement(tokenData) {
        if (!this.tokenToHlaMap || !this.tokenToHlaMap.has(tokenData.id)) {
            return;
        }
        
        const handle = this.tokenToHlaMap.get(tokenData.id);
        const attributes = {
            Spatial: {
                position: tokenData.position,
                orientation: tokenData.orientation || 0
            }
        };
        
        await this.updateObjectAttributes(handle, attributes);
    }

    /**
     * Publish attack execution to HLA
     */
    async publishAttackExecution(attackData) {
        const parameters = {
            FiringEntity: attackData.attackerId,
            TargetEntity: attackData.targetId,
            MunitionType: attackData.munitionType || 'STANDARD',
            WarheadType: attackData.warheadType || 'HE',
            FuseType: 'IMPACT',
            Quantity: 1,
            FireMissionIndex: attackData.attackId
        };
        
        await this.sendInteraction('WeaponFire', parameters);
    }

    /**
     * Publish combat resolution to HLA
     */
    async publishCombatResolution(combatData) {
        const parameters = {
            DetonationLocation: combatData.location,
            MunitionType: 'STANDARD',
            WarheadType: 'HE',
            FiringEntity: combatData.attackerId,
            TargetEntity: combatData.targetId,
            DetonationResult: combatData.result,
            Casualties: combatData.casualties || 0
        };
        
        await this.sendInteraction('Detonation', parameters);
    }

    /**
     * Map token data to NETN PhysicalEntity
     */
    mapTokenToPhysicalEntity(tokenData) {
        return {
            EntityIdentifier: tokenData.id,
            EntityType: {
                kind: 1, // Platform
                domain: 1, // Land
                country: 0,
                category: this.mapUnitTypeToCategory(tokenData.unitType),
                subcategory: 0
            },
            Spatial: {
                position: tokenData.position,
                orientation: tokenData.orientation || 0,
                velocity: [0, 0, 0]
            },
            ForceIdentifier: this.mapForceType(tokenData.forceType),
            Marking: tokenData.name || 'UNKNOWN',
            DamageState: 0, // No damage
            SupplyLevel: tokenData.supply || 100,
            AmmoLevel: tokenData.ammo || 100,
            FuelLevel: tokenData.fuel || 100
        };
    }

    /**
     * Map unit type to DIS category
     */
    mapUnitTypeToCategory(unitType) {
        const mapping = {
            'Infantry': 11,
            'Armoured': 1,
            'Artillery': 2,
            'Engineers': 10,
            'Signals': 13,
            'Medical': 12
        };
        return mapping[unitType] || 0;
    }

    /**
     * Map force type to DIS force identifier
     */
    mapForceType(forceType) {
        const mapping = {
            'Friendly': 1,
            'Hostile': 2,
            'Neutral': 3,
            'Unknown': 0
        };
        return mapping[forceType] || 0;
    }

    /**
     * Integrate external token from HLA federation
     */
    integrateExternalToken(data) {
        console.log('🔗 Integrating external token from HLA');
        
        // Map NETN PhysicalEntity to local token format
        const tokenData = {
            id: `hla_${data.attributes.EntityIdentifier}`,
            name: data.attributes.Marking,
            unitType: this.mapCategoryToUnitType(data.attributes.EntityType.category),
            forceType: this.mapForceIdentifier(data.attributes.ForceIdentifier),
            position: data.attributes.Spatial.position,
            orientation: data.attributes.Spatial.orientation,
            strength: 100, // Default strength
            external: true, // Mark as external
            federationSource: this.federationName
        };
        
        // Add to local system (if token manager is available)
        if (window.tokenPlacementManager && window.tokenPlacementManager.addExternalToken) {
            window.tokenPlacementManager.addExternalToken(tokenData);
        }
    }

    /**
     * Update external token
     */
    updateExternalToken(data) {
        const tokenId = `hla_${data.handle}`;
        
        if (window.tokenPlacementManager && window.tokenPlacementManager.updateExternalToken) {
            window.tokenPlacementManager.updateExternalToken(tokenId, {
                position: data.attributes.Spatial.position,
                orientation: data.attributes.Spatial.orientation
            });
        }
    }

    /**
     * Remove external token
     */
    removeExternalToken(data) {
        const tokenId = `hla_${data.handle}`;
        
        if (window.tokenPlacementManager && window.tokenPlacementManager.removeExternalToken) {
            window.tokenPlacementManager.removeExternalToken(tokenId);
        }
    }

    /**
     * Map DIS category to unit type
     */
    mapCategoryToUnitType(category) {
        const mapping = {
            1: 'Armoured',
            2: 'Artillery',
            10: 'Engineers',
            11: 'Infantry',
            12: 'Medical',
            13: 'Signals'
        };
        return mapping[category] || 'Infantry';
    }

    /**
     * Map DIS force identifier to force type
     */
    mapForceIdentifier(forceId) {
        const mapping = {
            1: 'Friendly',
            2: 'Hostile',
            3: 'Neutral',
            0: 'Unknown'
        };
        return mapping[forceId] || 'Unknown';
    }

    /**
     * Handle weapon fire interaction
     */
    handleWeaponFire(data) {
        console.log('🔫 Weapon fire:', data.parameters);
        
        // Visualize weapon fire if visualization manager is available
        if (window.attackVisualizationManager) {
            // Map to local attack visualization
        }
    }

    /**
     * Handle detonation interaction
     */
    handleDetonation(data) {
        console.log('💥 Detonation:', data.parameters);
        
        // Process casualties and damage
        if (window.combatSimulationEngine) {
            // Update local simulation with combat results
        }
    }

    /**
     * Handle command issued interaction
     */
    handleCommandIssued(data) {
        console.log('📋 Command issued:', data.parameters);
        
        // Display command notification
    }

    /**
     * Broadcast object update
     */
    broadcastObjectUpdate(instance) {
        const event = new CustomEvent('hla_object_update', {
            detail: {
                handle: instance.handle,
                className: instance.className,
                attributes: instance.attributes,
                timestamp: instance.lastUpdate
            }
        });
        window.dispatchEvent(event);
    }

    /**
     * Broadcast interaction
     */
    broadcastInteraction(interaction) {
        const event = new CustomEvent('hla_interaction', {
            detail: interaction
        });
        window.dispatchEvent(event);
    }

    /**
     * Unpublish all objects
     */
    async unpublishAllObjects() {
        console.log('📤 Unpublishing all objects...');
        
        for (const [handle, instance] of this.registeredObjects) {
            // In a real implementation, this would call RTI deleteObjectInstance
            console.log(`Unpublishing object: ${handle}`);
        }
        
        this.registeredObjects.clear();
        
        if (this.tokenToHlaMap) {
            this.tokenToHlaMap.clear();
        }
    }

    /**
     * Notify connection status
     */
    notifyConnectionStatus(status, message = '') {
        const event = new CustomEvent('hla_connection_status', {
            detail: {
                status: status,
                federationName: this.federationName,
                federateType: this.federateType,
                message: message,
                timestamp: new Date().toISOString()
            }
        });
        window.dispatchEvent(event);
    }

    /**
     * Get connection status
     */
    getConnectionStatus() {
        return {
            connected: this.connected,
            federationName: this.federationName,
            federateType: this.federateType,
            handle: this.federationExecutionHandle,
            registeredObjects: this.registeredObjects.size,
            subscriptions: this.subscriptions.size
        };
    }

    /**
     * Utility methods
     */
    generateHandle() {
        return `hla_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
    }

    delay(ms) {
        return new Promise(resolve => setTimeout(resolve, ms));
    }
}

// Initialize global instance
window.hlaIntegrationAdapter = new HlaIntegrationAdapter();
console.log('🔗 HLA Integration Adapter initialized');
