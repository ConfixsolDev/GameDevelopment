// Token Training & Identification System - Unified Web Version
class TokenSystem {
    constructor() {
        try {
            console.log('Initializing TokenSystem...');
            this.mode = 'main';
            this.canvas = document.getElementById('touchCanvas');
            if (!this.canvas) {
                throw new Error('Canvas element not found');
            }
            console.log('✅ Canvas element found:', this.canvas);
            this.ctx = this.canvas.getContext('2d');
            if (!this.ctx) {
                throw new Error('Canvas context not available');
            }
            console.log('✅ Canvas context created successfully');
            console.log('Canvas initialized successfully');

            this.activeTouches = new Map();
            this.learnedTokens = [];
            this.currentTrainingSession = null;
            this.lastIdentifiedToken = null;
            this.permanentLabels = [];
            this.labelIdCounter = 1;
            this.settings = {
                detectionSize: 144,
                confidenceThreshold: 70,
                showGrid: true,
                touchTimeout: 500,
                fixIdenticalCoordinates: true,
                minTouchDistance: 10,
                trainingTolerance: 200,
                minConfidence: 50
            };

            this.debugMode = true;
            this.lastProcessTime = 0;
            this.processingThrottle = 100;
            this.lastTouchHash = '';

            // Touch tracking with stability and averaging
            this.stableTouchDetection = {
                isMonitoring: false,
                stableTouches: [],
                stabilityStartTime: 0,
                stabilityThreshold: 2000, // 2 seconds for stable display
                positionThreshold: 5,
                lastStablePattern: null,
                hasIdentified: false,
                identificationResult: null
            };

            // Touch averaging system
            this.touchAveraging = {
                isActive: false,
                touchSamples: [],
                startTime: 0,
                updateInterval: null,
                displayUpdateDelay: 2000, // 2 seconds before showing stable data
                sampleInterval: 100 // Sample every 100ms for averaging
            };

            // Map view functionality
            this.map = null;
            this.mapViewActive = false;
            this.mapTouchMarkers = [];
            this.mapTokenMarkers = new Map(); // Store token markers by token ID
            this.mapMarkerCounter = 1; // Counter for unique marker IDs
            this.pendingMapMarkers = null; // Store pending markers to restore
            this.pendingTokenPlacement = null; // Store token waiting to be placed
            this.currentMapTouchData = null; // 🎯 NEW: Store current map touch data for processing

            this.init();
            this.initializeDatabase();
            this.setupEventListeners();
            this.startAnimationLoop();
            console.log('TokenSystem initialized successfully');
        } catch (error) {
            console.error('Error initializing TokenSystem:', error);
            alert('Error initializing system: ' + error.message);
        }
    }

    init() {
        this.updateModeDisplay();
        this.updateTokenCount();
        this.showToast('System initialized successfully!', 'success');
        this.resizeCanvas();
        window.addEventListener('resize', () => this.resizeCanvas());
        
        // 🎯 NEW: Activate distance monitoring
        this.monitorDistanceCalculations();
        
        // 🗺️ NEW: Try to restore map markers if everything is ready
        
        // 🧭 NEW: Initialize navigation active states
        this.initializeNavigation();
        setTimeout(() => this.tryRestoreMapMarkers(), 100);
        
        // Ensure grid is drawn after canvas is properly initialized
        setTimeout(() => {
            this.draw();
        }, 100);
        
        // Test touch functionality
        this.testTouchFunctionality();
    }
    
    initializeNavigation() {
        try {
            // Set default active state to first navigation item
            const navItems = document.querySelectorAll('.header-nav a, .navbar-nav a, .nav-item a, .nav-btn, .menu-btn, .header-btn');
            
            if (navItems.length > 0) {
                // Remove any existing active classes from ALL items
                navItems.forEach(item => {
                    item.classList.remove('nav-active', 'active-nav', 'current-page');
                });
                
                // Set first item as active by default
                navItems[0].classList.add('nav-active');
                console.log('✅ Navigation initialized with active state:', navItems[0].textContent.trim());
            }
            
            // Add click handlers to all navigation items
            navItems.forEach((item, index) => {
                item.addEventListener('click', (e) => {
                    e.preventDefault();
                    this.setActiveNavigation(item);
                });
            });
            
        } catch (error) {
            console.error('Error initializing navigation:', error);
        }
    }
    
    setActiveNavigation(activeElement) {
        try {
            // Get ALL navigation items
            const navItems = document.querySelectorAll('.header-nav a, .navbar-nav a, .nav-item a, .nav-btn, .menu-btn, .header-btn');
            
            // Remove active class from ALL navigation items first
            navItems.forEach(item => {
                item.classList.remove('nav-active', 'active-nav', 'current-page');
            });
            
            // Add active class ONLY to the clicked element
            activeElement.classList.add('nav-active');
            
            console.log('✅ Active navigation updated:', activeElement.textContent.trim());
            console.log('📊 Total navigation items:', navItems.length);
            
            // Verify only one item is active
            const activeItems = document.querySelectorAll('.nav-active, .active-nav, .current-page');
            console.log('🎯 Currently active items:', activeItems.length);
            
            // Optional: Update page content based on active navigation
            this.updatePageContent(activeElement);
            
        } catch (error) {
            console.error('Error setting active navigation:', error);
        }
    }
    
    updatePageContent(activeElement) {
        try {
            const text = activeElement.textContent.trim().toLowerCase();
            console.log('🔄 Updating page content for:', text);
            
            // Add your page content update logic here
            // This is where you would show/hide different sections based on the active navigation
            
        } catch (error) {
            console.error('Error updating page content:', error);
        }
    }
    
    testTouchFunctionality() {
        console.log('🧪 Testing touch functionality...');
        console.log('Canvas element:', this.canvas);
        console.log('Canvas dimensions:', this.canvas.width, 'x', this.canvas.height);
        console.log('Canvas style dimensions:', this.canvas.style.width, 'x', this.canvas.style.height);
        
        // Test if touch events are attached
        const touchStartHandler = this.canvas.ontouchstart;
        console.log('Touch start handler attached:', !!touchStartHandler);
        
        // Test canvas click events
        this.canvas.addEventListener('click', (e) => {
            console.log('🖱️ Canvas clicked at:', e.clientX, e.clientY);
        });
        
        // Test if canvas is visible and interactive
        const rect = this.canvas.getBoundingClientRect();
        console.log('Canvas position and size:', rect);
        console.log('Canvas computed style:', window.getComputedStyle(this.canvas));
        
        // Test if canvas is covered by other elements
        const elementAtCenter = document.elementFromPoint(rect.left + rect.width/2, rect.top + rect.height/2);
        console.log('Element at canvas center:', elementAtCenter);
        console.log('Is canvas at center?', elementAtCenter === this.canvas);
        
        // Add a simple test - draw a test pattern
        this.ctx.fillStyle = '#00FF00';
        this.ctx.fillRect(10, 10, 50, 50);
        console.log('Drew green test rectangle');
    }

    resizeCanvas() {
        const container = this.canvas.parentElement;
        const rect = container.getBoundingClientRect();

        const dpr = window.devicePixelRatio || 1;
        // Use full available space instead of limiting size
        const displayWidth = rect.width;
        const displayHeight = rect.height;

        this.canvas.width = displayWidth * dpr;
        this.canvas.height = displayHeight * dpr;
        this.canvas.style.width = displayWidth + 'px';
        this.canvas.style.height = displayHeight + 'px';

        this.ctx.scale(dpr, dpr);
        this.canvasRect = this.canvas.getBoundingClientRect();
        
        // Redraw the canvas after resize to ensure grid covers full area
        this.draw();
    }

    setupEventListeners() {
        // Button event listeners - with null checks
        const addTokenBtn = document.getElementById('addTokenBtn');
        if (addTokenBtn) {
            addTokenBtn.addEventListener('click', () => {
                console.log('Add Token button clicked');
                this.startTrainingMode();
            });
        }

        const testTokenBtn = document.getElementById('testTokenBtn');
        if (testTokenBtn) {
            testTokenBtn.addEventListener('click', () => {
                console.log('Test Token button clicked');
                this.startTestMode();
            });
        }

        const identifyBtn = document.getElementById('identifyBtn');
        if (identifyBtn) {
            identifyBtn.addEventListener('click', () => {
                console.log('Identify button clicked');
                this.startIdentificationMode();
            });
        }

        const manageTokensBtn = document.getElementById('manageTokensBtn');
        if (manageTokensBtn) {
            manageTokensBtn.addEventListener('click', () => {
                console.log('Manage Tokens button clicked');
                this.showTokenManagement();
            });
        }

        const clearScreenBtn = document.getElementById('clearScreenBtn');
        if (clearScreenBtn) {
            clearScreenBtn.addEventListener('click', () => {
                console.log('Clear Screen button clicked');
                this.clearAllPermanentLabels();
            });
        }

        const cancelTrainingBtn = document.getElementById('cancelTrainingBtn');
        if (cancelTrainingBtn) {
            cancelTrainingBtn.addEventListener('click', () => this.cancelTraining());
        }

        // Add reset training button functionality if it exists
        const resetBtn = document.getElementById('resetTrainingBtn');
        if (resetBtn) {
            resetBtn.addEventListener('click', () => this.resetTraining());
        }
        document.getElementById('saveTokenBtn').addEventListener('click', () => this.showTokenNameModal());



        // Add map view toggle button functionality
        const toggleMapViewBtn = document.getElementById('toggleMapViewBtn');
        if (toggleMapViewBtn) {
            toggleMapViewBtn.addEventListener('click', () => this.toggleMapView());
        }

        // Add map location control functionality
        const goToLocationBtn = document.getElementById('goToLocationBtn');
        const useCurrentLocationBtn = document.getElementById('useCurrentLocationBtn');

        if (goToLocationBtn) {
            goToLocationBtn.addEventListener('click', () => this.goToMapLocation());
        }

        if (useCurrentLocationBtn) {
            useCurrentLocationBtn.addEventListener('click', () => this.useCurrentLocation());
        }

        // Add fullscreen map button functionality
        const fullscreenMapBtn = document.getElementById('fullscreenMapBtn');
        if (fullscreenMapBtn) {
            fullscreenMapBtn.addEventListener('click', () => this.toggleMapFullscreen());
        }

        // Add place token marker button functionality
        const placeTokenMarkerBtn = document.getElementById('placeTokenMarkerBtn');
        if (placeTokenMarkerBtn) {
            placeTokenMarkerBtn.addEventListener('click', () => this.showPlaceTokenMarkerDialog());
        }

        // Add map controls button functionality
        const mapControlsBtn = document.getElementById('mapControlsBtn');
        if (mapControlsBtn) {
            mapControlsBtn.addEventListener('click', () => this.showMapControlsInfo());
        }





        // Modal event listeners - with null checks
        const closeTokenModal = document.getElementById('closeTokenModal');
        if (closeTokenModal) {
            closeTokenModal.addEventListener('click', () => this.hideTokenNameModal());
        }

        const confirmSave = document.getElementById('confirmSave');
        if (confirmSave) {
            confirmSave.addEventListener('click', () => this.saveToken());
        }

        const cancelSave = document.getElementById('cancelSave');
        if (cancelSave) {
            cancelSave.addEventListener('click', () => this.hideTokenNameModal());
        }

        const closeManagementModal = document.getElementById('closeManagementModal');
        if (closeManagementModal) {
            closeManagementModal.addEventListener('click', () => this.hideTokenManagement());
        }

        const closeTokenManagement = document.getElementById('closeTokenManagement');
        if (closeTokenManagement) {
            closeTokenManagement.addEventListener('click', () => this.hideTokenManagement());
        }

        const deleteAllTokens = document.getElementById('deleteAllTokens');
        if (deleteAllTokens) {
            deleteAllTokens.addEventListener('click', () => this.deleteAllTokens());
        }

        const exportTokens = document.getElementById('exportTokens');
        if (exportTokens) {
            exportTokens.addEventListener('click', () => this.exportTokens());
        }

        const importTokens = document.getElementById('importTokens');
        if (importTokens) {
            importTokens.addEventListener('click', () => {
                const importFile = document.getElementById('importFile');
                if (importFile) importFile.click();
            });
        }

        const importFile = document.getElementById('importFile');
        if (importFile) {
            importFile.addEventListener('change', (e) => this.importTokens(e));
        }

        const closeDeleteModal = document.getElementById('closeDeleteModal');
        if (closeDeleteModal) {
            closeDeleteModal.addEventListener('click', () => this.hideDeleteConfirmModal());
        }

        const cancelDelete = document.getElementById('cancelDelete');
        if (cancelDelete) {
            cancelDelete.addEventListener('click', () => this.hideDeleteConfirmModal());
        }

        const confirmDelete = document.getElementById('confirmDelete');
        if (confirmDelete) {
            confirmDelete.addEventListener('click', () => this.confirmDeleteToken());
        }

        // Canvas event listeners
        console.log('🎯 Setting up canvas event listeners...');
        this.canvas.addEventListener('touchstart', (e) => this.handleTouchStart(e));
        this.canvas.addEventListener('touchmove', (e) => this.handleTouchMove(e));
        this.canvas.addEventListener('touchend', (e) => this.handleTouchEnd(e));
        this.canvas.addEventListener('mousedown', (e) => this.handleMouseDown(e));
        this.canvas.addEventListener('mousemove', (e) => this.handleMouseMove(e));
        this.canvas.addEventListener('mouseup', (e) => this.handleMouseUp(e));
        console.log('✅ Canvas event listeners set up successfully');

        // Keyboard shortcuts
        document.addEventListener('keydown', (e) => this.handleKeyDown(e));


    }

    handleKeyDown(e) {
        switch (e.key.toLowerCase()) {
            case 't':
                if (this.mode === 'main') this.startTrainingMode();
                break;
            case 'e':
                if (this.mode === 'main') this.startTestMode();
                break;
            case 'i':
                if (this.mode === 'main') this.startIdentificationMode();
                break;
            case 'l':
                if (this.mode === 'main') this.showTokenManagement();
                break;
            case 'c':
                this.clearAllPermanentLabels();
                break;
            case 'm':
                if (this.mode !== 'main') this.returnToMainMenu();
                break;

        }
    }





    startTrainingMode() {
        try {
            console.log('Starting training mode...');
            this.mode = 'training';
            this.currentTrainingSession = {
                touchSignatures: [],  // Store all 5 touch signatures for consistency check
                currentPosition: 0,
                tokenName: null,
                referenceSignature: null  // First touch becomes the reference
            };
            this.updateModeDisplay();
            this.showTrainingStatus();

            // 🎯 NEW: Ensure touch pattern section is visible and ready
            const touchPatternSection = document.getElementById('touchPatternSection');
            if (touchPatternSection) {
                touchPatternSection.style.display = 'block';
            }

            // 🚨 DEBUG: Log training session creation
            console.log(`🎯 Training session created:`, {
                mode: this.mode,
                hasSession: !!this.currentTrainingSession,
                sessionData: this.currentTrainingSession
            });

            this.showToast('Training mode started. Use at least 2 fingers/touch points and repeat the SAME pattern 5 times for consistency.', 'info');
            console.log('Training mode started successfully');
        } catch (error) {
            console.error('Error starting training mode:', error);
            this.showToast('Error starting training mode: ' + error.message, 'error');
        }
    }

    startTestMode() {
        this.mode = 'test';
        this.updateModeDisplay();
        this.hideAllPanels();
        this.showTestResult();

        // 🎯 NEW: Ensure touch pattern section is visible and ready
        const touchPatternSection = document.getElementById('touchPatternSection');
        if (touchPatternSection) {
            touchPatternSection.style.display = 'block';
        }

        this.showToast('Test mode started. Touch to test token recognition.', 'info');
    }

    startIdentificationMode() {
        this.mode = 'identify';
        this.updateModeDisplay();
        this.hideAllPanels();
        this.showIdentificationResult();

        // 🎯 NEW: Ensure touch pattern section is visible and ready
        const touchPatternSection = document.getElementById('touchPatternSection');
        if (touchPatternSection) {
            touchPatternSection.style.display = 'block';
        }

        this.showToast('Identification mode started. Touch to identify tokens.', 'info');
    }

    returnToMainMenu() {
        // 🚨 SAFEGUARD: Don't reset training session if training is in progress
        if (this.mode === 'training' && this.currentTrainingSession && this.currentTrainingSession.currentPosition > 0) {
            console.log(`⚠️ WARNING: Attempting to return to main menu while training is in progress (${this.currentTrainingSession.currentPosition}/5)`);
            this.showToast('Training in progress! Complete training or cancel first.', 'warning');
            return;
        }

        this.mode = 'main';
        this.currentTrainingSession = null;
        this.updateModeDisplay();
        this.hideAllPanels();
        this.showSystemStatus();

        // 🎯 NEW: Ensure touch pattern section is visible and ready
        const touchPatternSection = document.getElementById('touchPatternSection');
        if (touchPatternSection) {
            touchPatternSection.style.display = 'block';
        }

        this.showToast('Returned to main menu', 'info');
    }

    updateModeDisplay() {
        const currentMode = document.getElementById('currentMode');
        if (currentMode) {
            currentMode.textContent = this.mode.toUpperCase();
        }
        
        const panelTitle = document.getElementById('panelTitle');
        if (panelTitle) {
            panelTitle.textContent = this.getPanelTitle();
        }

        // 🎯 NEW: Refresh touch pattern display when mode changes
        this.refreshTouchPatternDisplay();
    }

    // NEW: Refresh touch pattern display based on current state
    refreshTouchPatternDisplay() {
        // Get current active touches
        const currentTouches = Array.from(this.activeTouches.entries()).map(([id, data]) => ({
            identifier: id,
            clientX: data.x,
            clientY: data.y
        }));

        if (currentTouches.length > 0) {
            console.log(`🔄 Refreshing touch pattern display for ${currentTouches.length} touches in ${this.mode} mode`);
            this.updateTouchData(currentTouches);
        } else {
            console.log(`🔄 No active touches, clearing touch pattern display`);
            this.clearTouchDisplay();
        }
    }

    getPanelTitle() {
        switch (this.mode) {
            case 'training': return 'Training Mode';
            case 'test': return 'Test Mode';
            case 'identify': return 'Identification Mode';
            default: return 'System Status';
        }
    }

    showTrainingStatus() {
        this.hideAllPanels();
        const trainingStatus = document.getElementById('trainingStatus');
        if (trainingStatus) {
            trainingStatus.style.display = 'block';
        }

        // 🎯 NEW: Ensure touch pattern section is always visible
        const touchPatternSection = document.getElementById('touchPatternSection');
        if (touchPatternSection) {
            touchPatternSection.style.display = 'block';
        }
        
        const systemStatus = document.getElementById('systemStatus');
        if (systemStatus) {
            systemStatus.style.display = 'block';
        }

        this.updateTrainingProgress();
    }

    showSystemStatus() {
        this.hideAllPanels();
        const systemStatus = document.getElementById('systemStatus');
        if (systemStatus) {
            systemStatus.style.display = 'block';
        }
        
        const touchData = document.getElementById('touchData');
        if (touchData) {
            touchData.style.display = 'block';
        }

        // 🎯 NEW: Ensure touch pattern section is always visible
        const touchPatternSection = document.getElementById('touchPatternSection');
        if (touchPatternSection) {
            touchPatternSection.style.display = 'block';
        }
    }

    showTestResult() {
        this.hideAllPanels();
        document.getElementById('testResult').style.display = 'block';

        // 🎯 NEW: Ensure touch pattern section is always visible
        document.getElementById('touchPatternSection').style.display = 'block';
    }

    showIdentificationResult() {
        this.hideAllPanels();
        document.getElementById('identificationResult').style.display = 'block';

        // 🎯 NEW: Ensure touch pattern section is always visible
        document.getElementById('touchPatternSection').style.display = 'block';
    }

    hideAllPanels() {
        document.getElementById('trainingStatus').style.display = 'none';
        document.getElementById('testResult').style.display = 'none';
        document.getElementById('identificationResult').style.display = 'none';

        // 🎯 NEW: Keep touch pattern section visible but clear the data
        document.getElementById('touchPatternSection').style.display = 'block';
        this.clearTouchDisplay();
    }

    updateTrainingProgress() {
        const progress = (this.currentTrainingSession.currentPosition / 5) * 100;
        document.getElementById('progressFill').style.width = progress + '%';
        document.getElementById('trainingPosition').textContent = `Position: ${this.currentTrainingSession.currentPosition}/5`;

        // Update position indicators
        const indicators = document.querySelectorAll('.position-indicator');
        indicators.forEach((indicator, index) => {
            indicator.classList.remove('active', 'completed');
            if (index === this.currentTrainingSession.currentPosition) {
                indicator.classList.add('active');
            } else if (index < this.currentTrainingSession.currentPosition) {
                indicator.classList.add('completed');
            }
        });

        console.log(`🔄 Training Progress: ${this.currentTrainingSession.currentPosition}/5 (${progress}%)`);
    }

    handleTouchStart(e) {
        console.log('🖐️ Touch Start Event Detected!', e.touches.length, 'touches');
        console.log('Touch coordinates:', Array.from(e.touches).map(t => ({x: t.clientX, y: t.clientY})));
        e.preventDefault();
        
        // Visual feedback - draw a circle at touch point
        const rect = this.canvas.getBoundingClientRect();
        Array.from(e.touches).forEach(touch => {
            const x = touch.clientX - rect.left;
            const y = touch.clientY - rect.top;
            this.ctx.fillStyle = '#FF0000';
            this.ctx.beginPath();
            this.ctx.arc(x, y, 20, 0, Math.PI * 2);
            this.ctx.fill();
            console.log('Drew red circle at:', x, y);
        });
        
        this.processTouchEvent('start', e.touches);
    }

    handleTouchMove(e) {
        console.log('🖐️ Touch Move Event Detected!', e.touches.length, 'touches');
        // Only prevent default if we have active touches to avoid scrolling interference
        if (this.activeTouches.size > 0) {
            e.preventDefault();
            this.processTouchEvent('move', e.touches);
        }
    }

    handleTouchEnd(e) {
        console.log('🖐️ Touch End Event Detected!', e.changedTouches.length, 'touches');
        e.preventDefault();
        this.processTouchEvent('end', e.changedTouches);
    }

    handleMouseDown(e) {
        console.log('🖱️ Mouse Down Event Detected!', e.clientX, e.clientY);
        
        // Visual feedback - draw a blue circle at mouse point
        const rect = this.canvas.getBoundingClientRect();
        const x = e.clientX - rect.left;
        const y = e.clientY - rect.top;
        this.ctx.fillStyle = '#0000FF';
        this.ctx.beginPath();
        this.ctx.arc(x, y, 15, 0, Math.PI * 2);
        this.ctx.fill();
        console.log('Drew blue circle at:', x, y);
        
        this.processMouseEvent('start', e);
    }

    handleMouseMove(e) {
        this.processMouseEvent('move', e);
    }

    handleMouseUp(e) {
        this.processMouseEvent('end', e);
    }

    processTouchEvent(type, touches) {
        const touchArray = Array.from(touches);

        console.log(`🖐️ Touch Event [${type}]: ${touchArray.length} touches`);

        if (type === 'start') {
            // Store active touches for consistent tracking
            this.activeTouches.clear();
            touchArray.forEach(touch => {
                this.activeTouches.set(touch.identifier, {
                    x: touch.clientX,
                    y: touch.clientY,
                    startTime: Date.now()
                });
            });

            console.log(`📱 Touch START: ${touchArray.length} touches stored in activeTouches`);

            // 🎯 NEW: Always update touch pattern display immediately
            this.updateTouchData(touchArray);

            // Start touch averaging system
            this.startTouchAveraging(touchArray);

        } else if (type === 'move') {
            // Update active touches
            touchArray.forEach(touch => {
                if (this.activeTouches.has(touch.identifier)) {
                    this.activeTouches.set(touch.identifier, {
                        x: touch.clientX,
                        y: touch.clientY,
                        startTime: this.activeTouches.get(touch.identifier).startTime
                    });
                }
            });

            // Add to averaging samples
            this.addTouchSample(touchArray);

            // 🎯 NEW: Always update distance display in real-time during move events
            this.updateTouchData(touchArray);

        } else if (type === 'end') {
            console.log(`🔄 Touch END: Processing ${touchArray.length} touches`);

            // Store the current active touches BEFORE removing any
            const currentActiveTouches = Array.from(this.activeTouches.entries()).map(([id, data]) => ({
                identifier: id,
                clientX: data.x,
                clientY: data.y
            }));

            // Remove ended touches from active map
            touchArray.forEach(touch => {
                this.activeTouches.delete(touch.identifier);
            });

            console.log(`📊 Active touches before processing: ${currentActiveTouches.length}, After removal: ${this.activeTouches.size}`);

            // 🎯 IMPROVED: Handle partial finger lifts better
            if (touchArray.length === currentActiveTouches.length) {
                console.log(`✅ All touches ending: Waiting for stabilization to complete...`);
                // Let the timer finish naturally
            } else {
                console.log(`⚠️ Partial touch end: ${touchArray.length}/${currentActiveTouches.length} touches ending`);

                // For partial touches, check if we still have enough touches
                if (this.activeTouches.size >= 2) {
                    console.log(`✅ Still have ${this.activeTouches.size} touches, continuing...`);
                    // Continue with remaining touches
                } else {
                    console.log(`❌ Not enough touches (${this.activeTouches.size}), stopping averaging`);
                    this.stopTouchAveraging();
                }
            }

            // 🎯 NEW: Always update display based on remaining touches
            if (this.activeTouches.size > 0) {
                const remainingTouches = Array.from(this.activeTouches.entries()).map(([id, data]) => ({
                    identifier: id,
                    clientX: data.x,
                    clientY: data.y
                }));
                console.log(`📏 Touch END: Updating display with ${remainingTouches.length} remaining touches`);
                this.updateTouchData(remainingTouches);
            } else {
                // Clear display only when all touches are gone
                console.log(`📏 Touch END: All touches ended, clearing display`);
                this.clearTouchDisplay();
            }
        }
    }

    // Helper function to process stabilized touches (avoids code duplication)
    processStabilizedTouches(averagedTouches) {
        console.log(`🔄 Processing ${averagedTouches.length} stabilized touches in ${this.mode} mode`);

        // Use the same signature generation for all modes
        const signature = this.getTokenSignature(averagedTouches, true);
        if (signature) {
            console.log(`✅ Successfully generated signature for ${averagedTouches.length} touches`);

            // Process based on current mode
            if (this.mode === 'training') {
                console.log(`🏋️ Processing in TRAINING mode`);
                this.handleTrainingTouch(averagedTouches, true);
            } else if (this.mode === 'test') {
                console.log(`🧪 Processing in TEST mode`);
                this.handleTestTouch(averagedTouches, true);
            } else if (this.mode === 'identify') {
                console.log(`🔍 Processing in IDENTIFY mode`);
                this.handleIdentificationTouch(averagedTouches, true);
            } else {
                console.log(`📱 Processing in ${this.mode.toUpperCase()} mode`);
                // For main mode, just show the signature data
                this.showSignatureData(signature);
            }
        } else {
            console.log(`❌ Failed to generate signature for stabilized touches`);
        }
    }

    // Show signature data in main mode
    showSignatureData(signature) {
        console.log(`📊 Displaying signature data:`, signature);

        // Update the display to show the processed signature
        document.getElementById('touchArea').textContent = `✅ Processed! Touch Count: ${signature.touchCount}`;

        // Show toast notification
        this.showToast(`Successfully processed ${signature.touchCount} touches!`, 'success');
    }

    // New function to handle delayed processing with better error handling
    processStabilizedTouchesAfterDelay() {
        console.log(`⏰ DELAYED PROCESSING: Timer fired, checking system state...`);

        if (!this.touchAveraging.isActive) {
            console.log(`❌ Touch averaging stopped, cannot process`);
            return;
        }

        try {
            const averagedTouches = this.getAveragedTouches();
            console.log(`📊 Averaged touches from delay:`, averagedTouches);

            if (averagedTouches && averagedTouches.length >= 2) {
                console.log(`✅ Processing ${averagedTouches.length} touches after delay`);
                this.processStabilizedTouches(averagedTouches);
            } else {
                console.log(`❌ No valid touches after delay:`, averagedTouches);
                this.showToast('No valid touch data after stabilization', 'error');
            }
        } catch (error) {
            console.error(`💥 Error in delayed processing:`, error);
            this.showToast('Error processing touches: ' + error.message, 'error');
        } finally {
            // Always stop averaging and clear timers
            console.log(`🛑 Stopping touch averaging after delayed processing`);
            this.stopTouchAveraging();
        }
    }

    startTouchAveraging(initialTouches) {
        // 🚨 STRICT: Minimum 2 touches required
        if (initialTouches.length < 2) {
            console.log(`❌ REJECTED: Single touch detected (${initialTouches.length} touches). Minimum 2 touches required.`);
            this.showToast('Minimum 2 touches required for token recognition', 'error');
            return;
        }

        // Initialize averaging system
        this.touchAveraging.isActive = true;
        this.touchAveraging.touchSamples = [];
        this.touchAveraging.startTime = Date.now();

        console.log(`🚀 Touch averaging started at ${this.touchAveraging.startTime}`);
        console.log(`⏱️ Will complete at ${this.touchAveraging.startTime + this.touchAveraging.displayUpdateDelay} (${this.touchAveraging.displayUpdateDelay}ms from now)`);

        // Add initial sample
        this.addTouchSample(initialTouches);

        // Show initial "analyzing" state using updateTouchData
        this.updateTouchData(initialTouches);

        // Start sampling interval
        this.touchAveraging.updateInterval = setInterval(() => {
            if (this.activeTouches.size >= 2) { // Only sample if we have 2+ touches
                const currentTouches = Array.from(this.activeTouches.entries()).map(([id, data]) => ({
                    identifier: id,
                    clientX: data.x,
                    clientY: data.y
                }));
                this.addTouchSample(currentTouches);

                // Update countdown display (3-second countdown, but processing happens at 1s)
                const elapsed = Date.now() - this.touchAveraging.startTime;
                const displayDelay = 3000; // 3 seconds for display
                const processDelay = 1000;  // 1 second for actual processing

                const remainingDisplay = Math.max(0, displayDelay - elapsed);
                const secondsLeft = Math.ceil(remainingDisplay / 1000);

                if (secondsLeft > 0) {
                    document.getElementById('touchArea').textContent = `Stabilizing... ${secondsLeft}s remaining`;
                } else {
                    // Show completion message after 3 seconds
                    document.getElementById('touchArea').textContent = `✅ Stabilization complete!`;
                }

                // Also show processing status
                if (elapsed >= processDelay) {
                    console.log(`🎯 Processing should have completed at ${processDelay}ms, elapsed: ${elapsed}ms`);
                }
            }
        }, this.touchAveraging.sampleInterval);

        // Schedule stable display update after 2 seconds
        const startTime = Date.now();
        console.log(`⏰ Starting 2-second stabilization timer at ${startTime}`);

        // 🎯 SMART TIMING: Show 3s countdown but process at 1s
        const displayDelay = 3000; // Show 3 seconds on display
        const processDelay = 1000;  // Actually process at 1 second

        console.log(`⏰ Display countdown: ${displayDelay}ms, Actual processing: ${processDelay}ms`);

        // Main processing timer (1 second)
        const processTimer = setTimeout(() => {
            console.log(`⏰ PROCESS TIMER FIRED at ${Date.now()} (1 second)`);
            this.processStabilizedTouchesAfterDelay();
        }, processDelay);

        // Display completion timer (3 seconds) - just for visual feedback
        const displayTimer = setTimeout(() => {
            console.log(`⏰ DISPLAY TIMER FIRED at ${Date.now()} (3 seconds)`);
            // This timer is just for visual completion, no processing
        }, displayDelay);

        // Store timers for cleanup
        this.touchAveraging.activeTimers = [processTimer, displayTimer];
    }

    addTouchSample(touches) {
        if (!this.touchAveraging.isActive) return;

        // 🚨 STRICT: Only accept 2+ touches
        if (touches.length < 2) {
            console.log(`❌ REJECTED: addTouchSample called with single touch (${touches.length} touches). Minimum 2 required.`);
            return;
        }

        const sample = {
            timestamp: Date.now(),
            touches: touches.map(touch => ({
                identifier: touch.identifier,
                x: touch.clientX,
                y: touch.clientY
            }))
        };

        this.touchAveraging.touchSamples.push(sample);

        // Keep only recent samples (last 3 seconds)
        const cutoffTime = Date.now() - 3000;
        this.touchAveraging.touchSamples = this.touchAveraging.touchSamples.filter(
            sample => sample.timestamp > cutoffTime
        );
    }

    getAveragedTouches() {
        if (!this.touchAveraging.isActive || this.touchAveraging.touchSamples.length === 0) {
            return null;
        }

        const samples = this.touchAveraging.touchSamples;
        const touchMap = new Map();

        // Group samples by touch identifier
        samples.forEach(sample => {
            sample.touches.forEach(touch => {
                if (!touchMap.has(touch.identifier)) {
                    touchMap.set(touch.identifier, []);
                }
                touchMap.get(touch.identifier).push({ x: touch.x, y: touch.y });
            });
        });

        // Calculate average positions for each touch
        const averagedTouches = [];
        touchMap.forEach((positions, identifier) => {
            const avgX = positions.reduce((sum, pos) => sum + pos.x, 0) / positions.length;
            const avgY = positions.reduce((sum, pos) => sum + pos.y, 0) / positions.length;

            averagedTouches.push({
                identifier: identifier,
                clientX: avgX,
                clientY: avgY
            });
        });

        // 🚨 STRICT: Only return if we have 2+ touches
        if (averagedTouches.length < 2) {
            console.log(`❌ REJECTED: getAveragedTouches returned insufficient touches (${averagedTouches.length} touches). Minimum 2 required.`);
            return null;
        }

        return averagedTouches;
    }

    stopTouchAveraging() {
        console.log(`🛑 Stopping touch averaging system...`);
        this.touchAveraging.isActive = false;

        // Clear the sampling interval
        if (this.touchAveraging.updateInterval) {
            clearInterval(this.touchAveraging.updateInterval);
            this.touchAveraging.updateInterval = null;
        }

        // Clear all active timers
        if (this.touchAveraging.activeTimers) {
            this.touchAveraging.activeTimers.forEach(timerId => {
                console.log(`⏰ Clearing timer: ${timerId}`);
                clearTimeout(timerId);
            });
            this.touchAveraging.activeTimers = [];
        }

        console.log(`✅ Touch averaging stopped completely`);
    }

    // REMOVED: showAnalyzingState() - merged into updateTouchData()
    // All touch display updates now go through updateTouchData() for consistency

    clearTouchDisplay() {
        document.getElementById('touchCount').textContent = '0';
        document.getElementById('activeTouchCount').textContent = '0';
        document.getElementById('touchX').textContent = '-';
        document.getElementById('touchY').textContent = '-';
        document.getElementById('touchArea').textContent = '-';
    }

    processMouseEvent(type, event) {
        const rect = this.canvas.getBoundingClientRect();
        const x = event.clientX - rect.left;
        const y = event.clientY - rect.top;

        if (type === 'start') {
            this.startTouchDetection([{ clientX: x, clientY: y, identifier: 'mouse' }]);
        } else if (type === 'end') {
            this.endTouchDetection([{ clientX: x, clientY: y, identifier: 'mouse' }]);
        }
    }

    updateTouchData(touches) {
        if (touches.length > 0) {
            // Debug distance calculations
            this.debugDistanceCalculations(touches, 'updateTouchData');

            // Check if we're in analyzing state (touch averaging active)
            // Use 3-second display delay but 1-second processing delay
            const displayDelay = 3000; // 3 seconds for display
            const isAnalyzing = this.touchAveraging.isActive &&
                (Date.now() - this.touchAveraging.startTime) < displayDelay;

            // Debug timing information
            if (this.touchAveraging.isActive) {
                const elapsed = Date.now() - this.touchAveraging.startTime;
                const displayDelay = 3000; // 3 seconds for display
                const processDelay = 1000;  // 1 second for processing
                const remainingDisplay = displayDelay - elapsed;
                const remainingProcess = processDelay - elapsed;

                console.log(`⏱️ Touch Display: Elapsed: ${elapsed}ms, Display remaining: ${remainingDisplay}ms, Process remaining: ${remainingProcess}ms, IsAnalyzing: ${isAnalyzing}`);
            }

            if (isAnalyzing) {
                // Show analyzing state with real-time distance info
                const realTimeData = this.calculateRealTimeDistance(touches);

                document.getElementById('touchCount').textContent = realTimeData.touchCount;
                document.getElementById('activeTouchCount').textContent = realTimeData.touchCount;
                document.getElementById('touchX').textContent = 'Analyzing...';
                document.getElementById('touchY').textContent = `${realTimeData.touchCount} touch${realTimeData.touchCount > 1 ? 'es' : ''} detected`;

                // Show countdown status
                const elapsed = Date.now() - this.touchAveraging.startTime;
                const displayDelay = 3000;
                const processDelay = 1000;
                const remainingDisplay = Math.max(0, displayDelay - elapsed);
                const secondsLeft = Math.ceil(remainingDisplay / 1000);

                if (elapsed < processDelay) {
                    document.getElementById('touchArea').textContent = `Stabilizing... ${secondsLeft}s remaining`;
                } else if (elapsed < displayDelay) {
                    document.getElementById('touchArea').textContent = `✅ Processed at 1s! Display: ${secondsLeft}s remaining`;
                } else {
                    document.getElementById('touchArea').textContent = `✅ Complete!`;
                }
                return;
            }

            // 🎯 IMPROVED: Use real-time distance calculation for immediate updates
            const realTimeData = this.calculateRealTimeDistance(touches);

            // Update touch count
            document.getElementById('touchCount').textContent = realTimeData.touchCount;
            document.getElementById('activeTouchCount').textContent = realTimeData.touchCount;

            // Update distance information with real-time data
            document.getElementById('touchX').textContent = realTimeData.distanceInfo;
            document.getElementById('touchY').textContent = realTimeData.positions;
            document.getElementById('touchArea').textContent = realTimeData.details;

            // Also try to get signature data for additional properties (radius, etc.)
            const isStabilized = this.touchAveraging.isActive &&
                (Date.now() - this.touchAveraging.startTime) >= this.touchAveraging.displayUpdateDelay;
            const signature = this.getTokenSignature(touches, isStabilized);

            if (signature && signature.touchProperties && signature.touchProperties.hasRadius) {
                // Add radius information to the display if available
                const props = signature.touchProperties;
                const currentDetails = document.getElementById('touchArea').textContent;
                document.getElementById('touchArea').textContent = currentDetails + ` | Radius: ${props.avgRadius.toFixed(0)}px`;
            }

            // Log distance calculation for debugging
            console.log(`📏 Real-time Distance Update:`, {
                touchCount: realTimeData.touchCount,
                distanceInfo: realTimeData.distanceInfo,
                positions: realTimeData.positions,
                details: realTimeData.details
            });

        } else {
            // Clear display when no touches
            this.clearTouchDisplay();
        }
    }

    startTouchDetection(touches, isStabilized = false) {
        console.log(`🎯 startTouchDetection: Mode=${this.mode}, Touches=${touches.length}, Stabilized=${isStabilized}`);

        // 🚨 SAFEGUARD: Check if this is a partial touch pattern
        if (this.activeTouches.size > 0 && touches.length < this.activeTouches.size) {
            console.log(`⚠️ WARNING: Partial touch pattern detected! Expected ${this.activeTouches.size} touches, got ${touches.length}`);
            console.log(`🔄 Waiting for complete touch pattern...`);

            // Instead of returning, start the stabilization process for the current touches
            // This ensures we use the same stabilization system for all modes
            this.startTouchAveraging(touches);
            return;
        }

        if (this.mode === 'test') {
            console.log(`🧪 Test mode: Processing touch`);
            this.handleTestTouch(touches, isStabilized);
        } else if (this.mode === 'identify') {
            console.log(`🔍 Identify mode: Processing touch`);
            this.handleIdentificationTouch(touches, isStabilized);
        } else if (this.mode === 'training') {
            console.log(`🏋️ Training mode: Processing touch`);
            this.handleTrainingTouch(touches, isStabilized);
        } else {
            console.log(`❌ Unknown mode: ${this.mode}`);
        }
    }

    endTouchDetection(touches) {
        // Handle touch end logic
    }

    handleTrainingTouch(touches, isStabilized = false) {
        // 🚨 DEBUG: Log training session state
        console.log(`🔍 TRAINING DEBUG: Session state:`, {
            hasSession: !!this.currentTrainingSession,
            currentPosition: this.currentTrainingSession?.currentPosition || 'N/A',
            signaturesCount: this.currentTrainingSession?.touchSignatures?.length || 0,
            mode: this.mode
        });

        if (this.currentTrainingSession.currentPosition >= 5) {
            console.log(`✅ Training already complete (${this.currentTrainingSession.currentPosition}/5)`);
            return;
        }

        // 🚨 SAFEGUARD: Check for partial touch patterns (same as other modes)
        if (this.activeTouches.size > 0 && touches.length < this.activeTouches.size) {
            console.log(`⚠️ TRAINING: Partial touch pattern - waiting for complete pattern`);

            // Start stabilization process for current touches (same as other modes)
            this.startTouchAveraging(touches);
            return;
        }

        // 🚨 CRITICAL: Ensure minimum 2 touches for training (required by getTokenSignature)
        if (touches.length < 2) {
            console.log(`⚠️ TRAINING: Insufficient touches - need at least 2, got ${touches.length}`);
            this.showToast('Training requires at least 2 touch points. Use multiple fingers or touch points.', 'warning');
            return;
        }

        // 🚨 SAFEGUARD: Ensure training session exists and is valid
        if (!this.currentTrainingSession || !this.currentTrainingSession.touchSignatures) {
            console.log(`❌ TRAINING: Training session is invalid or missing!`);
            this.showToast('Training session error. Please restart training.', 'error');
            this.resetTraining();
            return;
        }

        // Additional validation: Check if this matches expected touch count
        const expectedTouchCount = this.activeTouches.size || touches.length;
        if (touches.length !== expectedTouchCount) {
            console.log(`⚠️ TRAINING: Touch count mismatch! Expected: ${expectedTouchCount}, Got: ${touches.length}`);

            // Start stabilization process for current touches (same as other modes)
            this.startTouchAveraging(touches);
            return;
        }

        // Use the single source of truth for signature data (with stabilization info)
        console.log(`🔍 TRAINING: Generating signature for ${touches.length} touches...`);
        const currentSignature = this.getTokenSignature(touches, isStabilized);
        if (!currentSignature) {
            console.log(`❌ TRAINING: Failed to generate signature for ${touches.length} touches`);
            this.showToast('Failed to capture touch data', 'error');
            return;
        }

        console.log(`✅ TRAINING: Signature generated successfully:`, {
            touchCount: currentSignature.touchCount,
            hasTokenHash: !!currentSignature.tokenHash,
            timestamp: currentSignature.timestamp
        });

        // Debug log the current touch
        console.log(`=== TRAINING Touch ${this.currentTrainingSession.currentPosition + 1} ===`);
        this.debugRawTouchData(touches, 'TRAINING');

        // First touch becomes the reference pattern
        if (this.currentTrainingSession.currentPosition === 0) {
            console.log(`🎯 FIRST TOUCH: Setting reference signature`);
            this.currentTrainingSession.referenceSignature = currentSignature;
            this.currentTrainingSession.touchSignatures.push(currentSignature);
            this.currentTrainingSession.currentPosition++;

            console.log(`📊 Training Session Updated:`, {
                currentPosition: this.currentTrainingSession.currentPosition,
                signaturesCount: this.currentTrainingSession.touchSignatures.length,
                hasReference: !!this.currentTrainingSession.referenceSignature
            });
            this.updateTrainingProgress();
            this.showToast(`Reference pattern recorded. Repeat the SAME pattern 4 more times.`, 'info');
            return;
        }

        // For subsequent touches, check consistency with reference
        console.log(`=== TRAINING COMPARISON ===`);
        console.log('Current Signature:', currentSignature);
        console.log('Reference Signature:', this.currentTrainingSession.referenceSignature);

        const consistencyScore = this.compareTokenSignatures(
            currentSignature,
            this.currentTrainingSession.referenceSignature
        );

        console.log(`Consistency Score: ${consistencyScore}% (Required: 80%+)`);

        // Require 80% similarity to reference pattern
        if (consistencyScore < 80) {
            this.showToast(
                `Inconsistent touch pattern! ${consistencyScore}% similarity (need 80%+). Try to match your first touch exactly.`,
                'error'
            );
            return; // Don't advance, let user try again
        }

        // Touch is consistent enough, store it
        console.log(`✅ CONSISTENT TOUCH: Adding to training data`);
        this.currentTrainingSession.touchSignatures.push(currentSignature);
        this.currentTrainingSession.currentPosition++;

        console.log(`📊 Training Session Updated:`, {
            currentPosition: this.currentTrainingSession.currentPosition,
            signaturesCount: this.currentTrainingSession.touchSignatures.length,
            totalSignatures: this.currentTrainingSession.touchSignatures.length
        });
        this.updateTrainingProgress();

        if (this.currentTrainingSession.currentPosition >= 5) {
            // All 5 touches completed successfully
            console.log(`🎉 TRAINING COMPLETE: All 5 touches recorded!`);
            this.validateTrainingConsistency();
        } else {
            const remaining = 5 - this.currentTrainingSession.currentPosition;
            this.showToast(
                `Good! Pattern matches (${consistencyScore}%). ${remaining} more touches needed.`,
                'success'
            );
        }
    }

    validateTrainingConsistency() {
        const signatures = this.currentTrainingSession.touchSignatures;
        const reference = signatures[0];

        let totalConsistency = 0;
        let minConsistency = 100;

        // Check consistency of all touches against the reference
        for (let i = 1; i < signatures.length; i++) {
            const consistency = this.compareTokenSignatures(signatures[i], reference);
            totalConsistency += consistency;
            minConsistency = Math.min(minConsistency, consistency);
        }

        const avgConsistency = totalConsistency / (signatures.length - 1);

        console.log(`=== TRAINING VALIDATION ===`);
        console.log(`Average Consistency: ${avgConsistency.toFixed(1)}%`);
        console.log(`Minimum Consistency: ${minConsistency}%`);
        console.log(`All touches above 80%: ${minConsistency >= 80}`);

        if (minConsistency >= 80 && avgConsistency >= 85) {
            // Training successful
            this.showTokenNameModal();
            this.showToast(
                `Training complete! Average consistency: ${avgConsistency.toFixed(1)}%`,
                'success'
            );
        } else {
            // Training failed consistency check
            this.showToast(
                `Training failed! Inconsistent touches detected. Average: ${avgConsistency.toFixed(1)}%, Min: ${minConsistency}%. Please restart training.`,
                'error'
            );
            this.resetTraining();
        }
    }

    resetTraining() {
        this.currentTrainingSession = {
            touchSignatures: [],
            currentPosition: 0,
            tokenName: null,
            referenceSignature: null
        };
        this.updateTrainingProgress();
        this.showToast('Training reset. Use at least 2 fingers/touch points and start over with your first touch pattern.', 'warning');
    }

    // REMOVED: captureDetailedTouchData - replaced by getTokenSignature()
    // All touch data capture now goes through getTokenSignature() for consistency

    calculateTouchArea(touches) {
        if (touches.length === 1) {
            return { width: 20, height: 20, area: 400 }; // Single touch approximation
        }

        // Calculate bounding box for multi-touch
        let minX = Infinity, maxX = -Infinity;
        let minY = Infinity, maxY = -Infinity;

        touches.forEach(touch => {
            minX = Math.min(minX, touch.clientX);
            maxX = Math.max(maxX, touch.clientX);
            minY = Math.min(minY, touch.clientY);
            maxY = Math.max(maxY, touch.clientY);
        });

        const width = maxX - minX;
        const height = maxY - minY;
        const area = width * height;

        return { width, height, area };
    }

    analyzeTouchPattern(touches) {
        console.log(`🔍 analyzeTouchPattern: Processing ${touches.length} touches`);

        if (touches.length === 1) {
            console.log(`👆 Single touch pattern detected`);
            return {
                type: 'single',
                complexity: 1,
                distances: [],
                distanceSignature: 'single'
            };
        }

        // Analyze multi-touch patterns with focus on distances
        const distances = [];
        const angles = [];
        const distancePairs = [];

        console.log(`📏 Calculating distances between ${touches.length} touches`);

        for (let i = 0; i < touches.length; i++) {
            for (let j = i + 1; j < touches.length; j++) {
                const touch1 = touches[i];
                const touch2 = touches[j];

                console.log(`📊 Touch ${i} (${touch1.clientX}, ${touch1.clientY}) to Touch ${j} (${touch2.clientX}, ${touch2.clientY})`);

                const dx = touch2.clientX - touch1.clientX;
                const dy = touch2.clientY - touch1.clientY;
                const distance = Math.sqrt(dx * dx + dy * dy);
                const angle = Math.atan2(dy, dx) * (180 / Math.PI);

                console.log(`📏 Distance ${i}-${j}: dx=${dx}, dy=${dy}, distance=${distance}, angle=${angle}`);

                distances.push(distance);
                angles.push(angle);
                distancePairs.push({
                    from: i,
                    to: j,
                    distance: distance,
                    angle: angle
                });
            }
        }

        console.log(`📊 All distances calculated:`, distances);
        console.log(`📊 All angles calculated:`, angles);

        // Create a unique distance signature
        const sortedDistances = distances.slice().sort((a, b) => a - b);
        const distanceSignature = this.createDistanceSignature(sortedDistances);

        const result = {
            type: 'multi',
            complexity: touches.length,
            distances: sortedDistances,
            distancePairs: distancePairs,
            avgDistance: distances.reduce((a, b) => a + b, 0) / distances.length,
            minDistance: Math.min(...distances),
            maxDistance: Math.max(...distances),
            distanceRange: Math.max(...distances) - Math.min(...distances),
            distanceVariance: this.calculateVariance(distances),
            distanceSignature: distanceSignature,
            angleSpread: this.calculateAngleSpread(angles),
            geometricCenter: this.calculateGeometricCenter(touches)
        };

        console.log(`✅ Touch pattern analysis complete:`, {
            type: result.type,
            complexity: result.complexity,
            distancesCount: result.distances.length,
            avgDistance: result.avgDistance,
            minDistance: result.minDistance,
            maxDistance: result.maxDistance,
            distanceSignature: result.distanceSignature
        });

        return result;
    }

    createDistanceSignature(sortedDistances) {
        // Create a unique signature based on distance ratios
        if (sortedDistances.length === 0) return 'single';
        if (sortedDistances.length === 1) return `d${Math.round(sortedDistances[0])}`;

        // Create ratios between distances for more robust comparison
        const ratios = [];
        for (let i = 1; i < sortedDistances.length; i++) {
            const ratio = sortedDistances[i] / sortedDistances[0]; // Ratio to smallest distance
            ratios.push(Math.round(ratio * 100) / 100); // Round to 2 decimal places
        }

        return `d${Math.round(sortedDistances[0])}_r${ratios.join('_')}`;
    }

    calculateRelativePosition(touch) {
        // Calculate position relative to canvas center
        const centerX = this.canvas.width / 2;
        const centerY = this.canvas.height / 2;

        return {
            relativeX: (touch.clientX - centerX) / centerX, // -1 to 1
            relativeY: (touch.clientY - centerY) / centerY, // -1 to 1
            distanceFromCenter: Math.sqrt(
                Math.pow(touch.clientX - centerX, 2) +
                Math.pow(touch.clientY - centerY, 2)
            )
        };
    }

    calculateMultiTouchGeometry(touches) {
        if (touches.length < 2) return null;

        // Calculate geometric properties of multi-touch
        const points = touches.map(t => ({ x: t.clientX, y: t.clientY }));
        const convexHull = this.calculateConvexHull(points);
        const perimeter = this.calculatePerimeter(convexHull);
        const area = this.calculatePolygonArea(convexHull);

        return {
            convexHull,
            perimeter,
            area,
            aspectRatio: this.calculateAspectRatio(convexHull),
            compactness: (4 * Math.PI * area) / (perimeter * perimeter) // Circularity measure
        };
    }

    calculateVariance(values) {
        const mean = values.reduce((a, b) => a + b, 0) / values.length;
        const squaredDiffs = values.map(v => Math.pow(v - mean, 2));
        return squaredDiffs.reduce((a, b) => a + b, 0) / values.length;
    }

    calculateAngleSpread(angles) {
        if (angles.length === 0) return 0;

        // Calculate the spread of angles (0-180 degrees)
        const sortedAngles = angles.sort((a, b) => a - b);
        let maxSpread = 0;

        for (let i = 0; i < sortedAngles.length; i++) {
            const next = (i + 1) % sortedAngles.length;
            let spread = Math.abs(sortedAngles[next] - sortedAngles[i]);
            if (spread > 180) spread = 360 - spread;
            maxSpread = Math.max(maxSpread, spread);
        }

        return maxSpread;
    }

    calculateGeometricCenter(touches) {
        const centerX = touches.reduce((sum, t) => sum + t.clientX, 0) / touches.length;
        const centerY = touches.reduce((sum, t) => sum + t.clientY, 0) / touches.length;
        return { x: centerX, y: centerY };
    }

    calculateConvexHull(points) {
        // Graham scan algorithm for convex hull
        if (points.length < 3) return points;

        // Find the lowest point (and leftmost if tied)
        let lowest = 0;
        for (let i = 1; i < points.length; i++) {
            if (points[i].y < points[lowest].y ||
                (points[i].y === points[lowest].y && points[i].x < points[lowest].x)) {
                lowest = i;
            }
        }

        // Sort points by polar angle with respect to lowest point
        const sorted = points.map((p, i) => ({ ...p, index: i }))
            .filter((_, i) => i !== lowest)
            .sort((a, b) => {
                const angleA = Math.atan2(a.y - points[lowest].y, a.x - points[lowest].x);
                const angleB = Math.atan2(b.y - points[lowest].y, b.x - points[lowest].x);
                return angleA - angleB;
            });

        // Build convex hull
        const hull = [points[lowest], sorted[0]];
        for (let i = 1; i < sorted.length; i++) {
            while (hull.length > 1 && this.crossProduct(hull[hull.length - 2], hull[hull.length - 1], sorted[i]) <= 0) {
                hull.pop();
            }
            hull.push(sorted[i]);
        }

        return hull;
    }

    crossProduct(o, a, b) {
        return (a.x - o.x) * (b.y - o.y) - (a.y - o.y) * (b.x - o.x);
    }

    calculatePerimeter(points) {
        let perimeter = 0;
        for (let i = 0; i < points.length; i++) {
            const next = (i + 1) % points.length;
            const dx = points[next].x - points[i].x;
            const dy = points[next].y - points[i].y;
            perimeter += Math.sqrt(dx * dx + dy * dy);
        }
        return perimeter;
    }

    calculatePolygonArea(points) {
        let area = 0;
        for (let i = 0; i < points.length; i++) {
            const next = (i + 1) % points.length;
            area += points[i].x * points[next].y - points[next].x * points[i].y;
        }
        return Math.abs(area) / 2;
    }

    calculateAspectRatio(points) {
        if (points.length < 2) return 1;

        let minX = Infinity, maxX = -Infinity;
        let minY = Infinity, maxY = -Infinity;

        points.forEach(p => {
            minX = Math.min(minX, p.x);
            maxX = Math.max(maxX, p.x);
            minY = Math.min(minY, p.y);
            maxY = Math.max(maxY, p.y);
        });

        const width = maxX - minX;
        const height = maxY - minY;

        return width > height ? width / height : height / width;
    }

    // REMOVED: generateTokenSignature - replaced by getTokenSignature()
    // All signature generation now goes through getTokenSignature() for consistency

    // REMOVED: extractGeometricFeatures, extractTemporalFeatures, extractSpatialFeatures
    // These were only used by the old generateTokenSignature function
    // All feature extraction is now handled within getTokenSignature()

    // REMOVED: analyzeRhythm, calculateSpatialDistribution, calculateUniquenessScore
    // These were part of the old signature system and are no longer needed

    // REMOVED: Old checkTokenUniqueness - this was using old signature format
    // Uniqueness checking is now done in checkSignatureUniqueness() below

    calculateTokenSimilarity(signature1, signature2) {
        if (!signature1 || !signature2) return 0;

        let similarity = 0;
        let totalWeight = 0;

        // Compare geometric features
        if (signature1.features.geometricFeatures && signature2.features.geometricFeatures) {
            const geo1 = signature1.features.geometricFeatures;
            const geo2 = signature2.features.geometricFeatures;

            similarity += this.compareFeature(geo1.avgTouchCount, geo2.avgTouchCount, 0.2);
            similarity += this.compareFeature(geo1.patternComplexity, geo2.patternComplexity, 0.3);
            similarity += this.compareFeature(geo1.geometricDiversity, geo2.geometricDiversity, 0.2);
            totalWeight += 0.7;
        }

        // Compare spatial features
        if (signature1.features.spatialFeatures && signature2.features.spatialFeatures) {
            const spa1 = signature1.features.spatialFeatures;
            const spa2 = signature2.features.spatialFeatures;

            similarity += this.compareFeature(spa1.avgDistance, spa2.avgDistance, 0.15);
            similarity += this.compareFeature(spa1.spatialDistribution, spa2.spatialDistribution, 0.15);
            totalWeight += 0.3;
        }

        return totalWeight > 0 ? similarity / totalWeight : 0;
    }

    compareFeature(value1, value2, weight) {
        if (value1 === 0 && value2 === 0) return weight;
        if (value1 === 0 || value2 === 0) return 0;

        const diff = Math.abs(value1 - value2);
        const max = Math.max(value1, value2);
        const similarity = Math.max(0, 1 - (diff / max));

        return similarity * weight;
    }

    handleTestTouch(touches, isStabilized = false) {
        // 🚨 SAFEGUARD: Check for partial touch patterns
        if (this.activeTouches.size > 0 && touches.length < this.activeTouches.size) {
            console.log(`⚠️ TEST: Partial touch pattern - waiting for complete pattern`);

            // Start stabilization process for current touches (same as other modes)
            this.startTouchAveraging(touches);
            return;
        }

        // Additional validation: Check if this matches expected touch count
        const expectedTouchCount = this.activeTouches.size || touches.length;
        if (touches.length !== expectedTouchCount) {
            console.log(`⚠️ TEST: Touch count mismatch! Expected: ${expectedTouchCount}, Got: ${touches.length}`);

            // Start stabilization process for current touches (same as other modes)
            this.startTouchAveraging(touches);
            return;
        }

        // Debug: Log raw touch data to console
        this.debugRawTouchData(touches, 'TEST');

        const result = this.identifyToken(touches);
        if (result) {
            document.getElementById('testTokenName').textContent = result.name;
            document.getElementById('testConfidence').textContent = result.confidence + '%';
            document.getElementById('testTouchCount').textContent = touches.length;
            document.getElementById('testArea').textContent = `${touches.length} touch${touches.length > 1 ? 'es' : ''}`;
            this.showTestResult();
            this.showToast(`Token identified: ${result.name} (${result.confidence}%)`, 'success');

            // 🎯 NEW: Show detailed token information for test results
            this.showTestTokenDetails(result.token, result.confidence);
        } else {
            this.showToast('No matching token found', 'warning');
        }
    }

    // NEW: Show detailed token information for test results
    showTestTokenDetails(token, confidence) {
        const modal = document.createElement('div');
        modal.className = 'modal';
        modal.style.display = 'block';
        modal.innerHTML = `
            <div class="modal-content" style="max-width: 600px;">
                <div class="modal-header">
                    <h2>🧪 Test Result: ${token.name}</h2>
                    <button class="close-btn" onclick="this.closest('.modal').remove()">&times;</button>
                </div>
                <div class="modal-body">
                    <div style="background: rgba(76, 175, 80, 0.2); border: 2px solid #4CAF50; border-radius: 8px; padding: 15px; margin-bottom: 20px; text-align: center;">
                        <h3 style="color: #4CAF50; margin-bottom: 10px;">✅ Token Identified!</h3>
                        <div style="font-size: 24px; font-weight: bold; color: #4CAF50;">${confidence}% Match</div>
                    </div>
                    
                    <div style="background: rgba(76, 175, 80, 0.1); border: 1px solid #4CAF50; border-radius: 8px; padding: 15px; margin-bottom: 20px;">
                        <h3 style="color: #4CAF50; margin-bottom: 10px;">🎯 Token Information</h3>
                        <div style="font-family: monospace; font-size: 14px;">
                            <div>📅 Created: ${new Date(token.createdAt).toLocaleString()}</div>
                            <div>🎯 Touch Count: ${token.signature.touchCount} fingers</div>
                            <div>📈 Training Consistency: ${token.trainingConsistency ? token.trainingConsistency.avg.toFixed(1) + '%' : 'N/A'}</div>
                        </div>
                    </div>
                    
                    <div style="background: rgba(255, 152, 0, 0.1); border: 1px solid #ff9800; border-radius: 8px; padding: 15px; margin-bottom: 20px;">
                        <h3 style="color: #ff9800; margin-bottom: 10px;">📏 Distance Information</h3>
                        <div style="font-family: monospace; font-size: 14px;">
                            ${this.generateDistanceInfoHTML(token.signature)}
                        </div>
                    </div>
                    
                    <div style="background: rgba(156, 39, 176, 0.1); border: 1px solid #9c27b0; border-radius: 8px; padding: 15px;">
                        <h3 style="color: #9c27b0; margin-bottom: 10px;">🔍 Pattern Analysis</h3>
                        <div style="font-family: monospace; font-size: 14px;">
                            ${this.generatePatternInfoHTML(token.signature)}
                        </div>
                    </div>
                </div>
                <div class="modal-footer">
                    <button class="btn" onclick="this.closest('.modal').remove()">Close</button>
                    <button class="btn info" onclick="tokenSystem.showTokenDetails(${token.id}); this.closest('.modal').remove();">View Full Details</button>
                </div>
            </div>
        `;

        document.body.appendChild(modal);

        // Auto-close after 15 seconds
        setTimeout(() => {
            if (modal.parentNode) {
                modal.remove();
            }
        }, 15000);
    }

    debugRawTouchData(touches, mode) {
        console.log(`=== ${mode} MODE - Raw Touch Data ===`);
        for (let i = 0; i < touches.length; i++) {
            const touch = touches[i];
            console.log(`Touch ${i}:`, {
                clientX: touch.clientX,
                clientY: touch.clientY,
                force: touch.force,
                radiusX: touch.radiusX,
                radiusY: touch.radiusY,
                rotationAngle: touch.rotationAngle,
                identifier: touch.identifier,
                target: touch.target ? touch.target.tagName : 'null'
            });
        }

        // Generate and log signature
        const signature = this.getTokenSignature(touches);
        console.log('Generated Signature:', {
            hash: signature.tokenHash,
            touchCount: signature.touchCount,
            touchProperties: signature.touchProperties,
            distanceSignature: signature.touchPattern?.distanceSignature
        });
        console.log('=== End Debug ===');
    }

    handleIdentificationTouch(touches, isStabilized = false) {
        // 🚨 SAFEGUARD: Check for partial touch patterns (same as test mode)
        if (this.activeTouches.size > 0 && touches.length < this.activeTouches.size) {
            console.log(`⚠️ IDENTIFY: Partial touch pattern - waiting for complete pattern`);

            // Start stabilization process for current touches (same as other modes)
            this.startTouchAveraging(touches);
            return;
        }

        // Additional validation: Check if this matches expected touch count
        const expectedTouchCount = this.activeTouches.size || touches.length;
        if (touches.length !== expectedTouchCount) {
            console.log(`⚠️ IDENTIFY: Touch count mismatch! Expected: ${expectedTouchCount}, Got: ${touches.length}`);

            // Start stabilization process for current touches (same as other modes)
            this.startTouchAveraging(touches);
            return;
        }

        const result = this.identifyToken(touches);
        if (result) {
            document.getElementById('identifiedTokenName').textContent = result.name;
            document.getElementById('identificationConfidence').textContent = result.confidence + '%';
            document.getElementById('lastDetectionTime').textContent = new Date().toLocaleTimeString();
            this.showIdentificationResult();
            this.showToast(`Token identified: ${result.name}`, 'success');

            // 🎯 NEW: Show detailed token information for identification results
            this.showIdentificationTokenDetails(result.token, result.confidence);
        }
    }

    // NEW: Show detailed token information for identification results
    showIdentificationTokenDetails(token, confidence) {
        const modal = document.createElement('div');
        modal.className = 'modal';
        modal.style.display = 'block';
        modal.innerHTML = `
            <div class="modal-content" style="max-width: 600px;">
                <div class="modal-header">
                    <h2>🔍 Identification: ${token.name}</h2>
                    <button class="close-btn" onclick="this.closest('.modal').remove()">&times;</button>
                </div>
                <div class="modal-body">
                    <div style="background: rgba(76, 175, 80, 0.2); border: 2px solid #4CAF50; border-radius: 8px; padding: 15px; margin-bottom: 20px; text-align: center;">
                        <h3 style="color: #4CAF50; margin-bottom: 10px;">✅ Token Identified!</h3>
                        <div style="font-size: 24px; font-weight: bold; color: #4CAF50;">${confidence}% Match</div>
                        <div style="font-size: 14px; color: #666;">Detected at ${new Date().toLocaleTimeString()}</div>
                    </div>
                    
                    <div style="background: rgba(76, 175, 80, 0.1); border: 1px solid #4CAF50; border-radius: 8px; padding: 15px; margin-bottom: 20px;">
                        <h3 style="color: #4CAF50; margin-bottom: 10px;">🎯 Token Information</h3>
                        <div style="font-family: monospace; font-size: 14px;">
                            <div>📅 Created: ${new Date(token.createdAt).toLocaleString()}</div>
                            <div>🎯 Touch Count: ${token.signature.touchCount} fingers</div>
                            <div>📈 Training Consistency: ${token.trainingConsistency ? token.trainingConsistency.avg.toFixed(1) + '%' : 'N/A'}</div>
                        </div>
                    </div>
                    
                    <div style="background: rgba(255, 152, 0, 0.1); border: 1px solid #ff9800; border-radius: 8px; padding: 15px; margin-bottom: 20px;">
                        <h3 style="color: #ff9800; margin-bottom: 10px;">📏 Distance Information</h3>
                        <div style="font-family: monospace; font-size: 14px;">
                            ${this.generateDistanceInfoHTML(token.signature)}
                        </div>
                    </div>
                    
                    <div style="background: rgba(156, 39, 176, 0.1); border: 1px solid #9c27b0; border-radius: 8px; padding: 15px;">
                        <h3 style="color: #9c27b0; margin-bottom: 10px;">🔍 Pattern Analysis</h3>
                        <div style="font-family: monospace; font-size: 14px;">
                            ${this.generatePatternInfoHTML(token.signature)}
                        </div>
                    </div>
                </div>
                <div class="modal-footer">
                    <button class="btn" onclick="this.closest('.modal').remove()">Close</button>
                    <button class="btn info" onclick="tokenSystem.showTokenDetails(${token.id}); this.closest('.modal').remove();">View Full Details</button>
                </div>
            </div>
        `;

        document.body.appendChild(modal);

        // Auto-close after 15 seconds
        setTimeout(() => {
            if (modal.parentNode) {
                modal.remove();
            }
        }, 15000);
    }

    // ==========================================
    // CORE TOKEN SIGNATURE FUNCTIONS
    // ==========================================

    /**
     * 1. GET TOKEN SIGNATURE - Single source of truth for token signatures
     * This is the ONLY function that should generate token signatures
     */
    getTokenSignature(touches, isStabilized = false) {
        if (!touches || touches.length === 0) return null;

        // 🚨 STRICT: Minimum 2 touches required for token signatures
        if (touches.length < 2) {
            console.log(`❌ REJECTED: getTokenSignature called with single touch (${touches.length} touches). Minimum 2 required.`);
            console.log(`💡 TIP: Use multiple fingers or touch points to create a valid token signature.`);
            return null;
        }

        // 🎯 DEBUG: Log touch data to identify the issue
        console.log(`🔍 getTokenSignature: Processing ${touches.length} touches`);
        console.log(`📊 Touch data sample:`, touches.slice(0, 2).map(t => ({
            hasClientX: 'clientX' in t,
            hasClientY: 'clientY' in t,
            clientX: t.clientX,
            clientY: t.clientY,
            identifier: t.identifier,
            type: typeof t.clientX
        })));

        // Validate touch data has required properties
        const validTouches = touches.filter(t =>
            typeof t.clientX === 'number' &&
            typeof t.clientY === 'number' &&
            !isNaN(t.clientX) &&
            !isNaN(t.clientY)
        );

        if (validTouches.length !== touches.length) {
            console.log(`⚠️ WARNING: ${touches.length - validTouches.length} touches have invalid coordinates`);
            console.log(`📊 Invalid touches:`, touches.filter(t =>
                typeof t.clientX !== 'number' ||
                typeof t.clientY !== 'number' ||
                isNaN(t.clientX) ||
                isNaN(t.clientY)
            ));
        }

        if (validTouches.length < 2) {
            console.log(`❌ REJECTED: Not enough valid touches (${validTouches.length} valid, ${touches.length} total)`);
            return null;
        }

        // Add stability indicator to signature
        const stabilityInfo = {
            isStabilized: isStabilized,
            generatedAt: Date.now(),
            sampleCount: isStabilized ? (this.touchAveraging.touchSamples?.length || 1) : 1
        };

        // Extract POSITION-INDEPENDENT touch properties only (NO PRESSURE)
        const touchProperties = [];
        for (let i = 0; i < validTouches.length; i++) {
            const touch = validTouches[i];
            touchProperties.push({
                // Touch properties (real browser API data) - NO POSITION, NO PRESSURE
                radiusX: touch.radiusX,               // Actual X radius or undefined  
                radiusY: touch.radiusY,               // Actual Y radius or undefined
                rotationAngle: touch.rotationAngle,   // Actual rotation or undefined

                // Touch identifier (for multi-touch tracking)
                identifier: touch.identifier
            });
        }

        // Calculate RELATIVE distances and patterns (position-independent)
        console.log(`📏 Analyzing touch pattern for ${validTouches.length} valid touches`);
        const touchPattern = this.analyzeTouchPattern(validTouches);
        console.log(`📊 Generated touch pattern:`, {
            type: touchPattern.type,
            complexity: touchPattern.complexity,
            distancesCount: touchPattern.distances?.length || 0,
            hasValidDistances: touchPattern.distances?.some(d => d !== null) || false,
            minDistance: touchPattern.minDistance,
            maxDistance: touchPattern.maxDistance
        });

        const touchGeometry = this.extractTouchGeometry(touchProperties);

        const signature = {
            // Core touch characteristics (position-independent)
            touchCount: validTouches.length,
            timestamp: Date.now(),

            // Stability information
            stability: stabilityInfo,

            // Touch properties from browser API (position-independent)
            touchProperties: touchGeometry,

            // Touch pattern analysis (distances, not positions)
            touchPattern: touchPattern,

            // Multi-touch geometry (relative distances only)
            multiTouchGeometry: validTouches.length > 1 ? this.calculateMultiTouchGeometry(validTouches) : null,

            // Token signature hash (based on pattern, NOT position)
            tokenHash: this.generateTokenHash(touchPattern, touchGeometry),

            // 🎯 NEW: Store original touches for debugging and regeneration
            originalTouches: validTouches.map(t => ({
                clientX: t.clientX,
                clientY: t.clientY,
                identifier: t.identifier
            }))
        };

        console.log(`✅ Generated signature with ${validTouches.length} touches and valid distance data`);
        return signature;
    }

    extractTouchGeometry(touchProperties) {
        const geometry = {
            hasRadius: false,
            hasRotation: false,
            radiusValues: [],
            rotationValues: [],
            avgRadius: 0,
            avgRotation: 0,
            radiusVariance: 0
        };

        // Extract actual browser-provided values (position-independent, NO PRESSURE)
        touchProperties.forEach(touch => {
            // Check for real radius data
            if ((touch.radiusX !== undefined && touch.radiusX !== null) ||
                (touch.radiusY !== undefined && touch.radiusY !== null)) {
                geometry.hasRadius = true;
                const radiusX = touch.radiusX || 0;
                const radiusY = touch.radiusY || 0;
                const avgRadius = (radiusX + radiusY) / 2;
                geometry.radiusValues.push(avgRadius);
            }

            // Check for real rotation data
            if (touch.rotationAngle !== undefined && touch.rotationAngle !== null) {
                geometry.hasRotation = true;
                geometry.rotationValues.push(touch.rotationAngle);
            }
        });

        // Calculate averages and variances for real data only (NO PRESSURE)
        if (geometry.radiusValues.length > 0) {
            geometry.avgRadius = geometry.radiusValues.reduce((a, b) => a + b, 0) / geometry.radiusValues.length;
            geometry.radiusVariance = this.calculateVariance(geometry.radiusValues);
        }

        if (geometry.rotationValues.length > 0) {
            geometry.avgRotation = geometry.rotationValues.reduce((a, b) => a + b, 0) / geometry.rotationValues.length;
        }

        return geometry;
    }

    generateTokenHash(touchPattern, touchGeometry) {
        // Create hash from position-independent data ONLY (NO PRESSURE)
        const tokenData = {
            touchCount: touchPattern.complexity || 1,
            distanceSignature: touchPattern.distanceSignature || 'single',
            avgDistance: Math.round(touchPattern.avgDistance || 0),
            minDistance: Math.round(touchPattern.minDistance || 0),
            maxDistance: Math.round(touchPattern.maxDistance || 0),
            hasRadius: touchGeometry.hasRadius,
            avgRadius: Math.round(touchGeometry.avgRadius || 0)
        };

        const dataString = JSON.stringify(tokenData);
        let hash = 0;
        for (let i = 0; i < dataString.length; i++) {
            const char = dataString.charCodeAt(i);
            hash = ((hash << 5) - hash) + char;
            hash = hash & hash;
        }
        return Math.abs(hash).toString(36);
    }

    // REMOVED: extractTouchProperties and generateRawDataHash
    // These were position-dependent and have been replaced with position-independent functions

    /**
     * 2. GET POSITION DATA - Single function for position data
     * This is the ONLY function that should extract position information
     */
    getPositionData(touches) {
        if (!touches || touches.length === 0) return null;

        return {
            x: touches[0].clientX,
            y: touches[0].clientY,
            centerX: touches.reduce((sum, t) => sum + t.clientX, 0) / touches.length,
            centerY: touches.reduce((sum, t) => sum + t.clientY, 0) / touches.length,
            boundingBox: this.calculateBoundingBox(touches),
            timestamp: Date.now()
        };
    }

    /**
     * 3. SAVE TOKEN SIGNATURE - Dedicated save function
     * This function should ONLY use getTokenSignature() for signature data
     */
    saveTokenSignature(name, trainingData) {
        if (!name || !trainingData || trainingData.length === 0) {
            return { success: false, error: 'Invalid input data' };
        }

        // Get signature using single source of truth
        const signature = this.getSignatureFromTrainingData(trainingData);
        if (!signature) {
            return { success: false, error: 'Failed to generate signature' };
        }

        // Check uniqueness
        const uniquenessCheck = this.checkSignatureUniqueness(signature);
        if (!uniquenessCheck.isUnique) {
            return { success: false, error: uniquenessCheck.reason };
        }

        // Create token
        const token = this.createTokenObject(name, signature);

        // Save to storage
        this.saveTokenToStorage(token);

        return { success: true, token: token };
    }

    // Single job: Extract signature from training data
    getSignatureFromTrainingData(trainingData) {
        console.log(`🔍 getSignatureFromTrainingData: Processing ${trainingData.length} training signatures`);

        if (!trainingData || trainingData.length === 0) {
            console.log(`❌ No training data provided`);
            return null;
        }

        // Training data contains signature objects directly, not wrapped in a signature property
        const firstSignature = trainingData[0];
        console.log(`📊 First signature structure:`, {
            hasSignature: !!firstSignature.signature,
            hasTouchPattern: !!firstSignature.touchPattern,
            hasTouchCount: !!firstSignature.touchCount,
            touchCount: firstSignature.touchCount,
            patternType: firstSignature.touchPattern?.type
        });

        // Check if the first item is already a complete signature object
        if (firstSignature && firstSignature.touchCount && firstSignature.touchPattern) {
            console.log(`✅ Using existing signature from training data`);

            // Validate that the signature has proper distance information
            if (firstSignature.touchPattern.distances && firstSignature.touchPattern.distances.some(d => d !== null)) {
                console.log(`✅ Signature has valid distance data`);
                return firstSignature;
            } else {
                console.log(`⚠️ Signature has null distance data, regenerating...`);
            }
        }

        // If we reach here, we need to generate a new signature from the training data
        console.log(`🔄 Generating new signature from training data`);

        // Use the first training signature as the base, but ensure it has proper distance data
        if (firstSignature && firstSignature.touchPattern) {
            // Try to regenerate the pattern with proper distance calculations
            const regeneratedPattern = this.regenerateTouchPattern(firstSignature);
            if (regeneratedPattern) {
                console.log(`✅ Successfully regenerated touch pattern with distance data`);
                return {
                    ...firstSignature,
                    touchPattern: regeneratedPattern
                };
            }
        }

        console.log(`❌ Failed to extract valid signature from training data`);
        return null;
    }

    // NEW: Regenerate touch pattern with proper distance calculations
    regenerateTouchPattern(signature) {
        try {
            // If we have the original touches, regenerate the pattern
            if (signature.originalTouches) {
                console.log(`🔄 Regenerating pattern from original touches`);
                return this.analyzeTouchPattern(signature.originalTouches);
            }

            // If we have distance pairs but they're null, try to reconstruct
            if (signature.touchPattern && signature.touchPattern.distancePairs) {
                console.log(`🔄 Attempting to reconstruct pattern from distance pairs`);

                // Check if we can extract valid distances from the pairs
                const validPairs = signature.touchPattern.distancePairs.filter(pair =>
                    pair.distance !== null && pair.distance !== undefined
                );

                if (validPairs.length > 0) {
                    console.log(`✅ Found ${validPairs.length} valid distance pairs`);

                    // Reconstruct the pattern
                    const distances = validPairs.map(pair => pair.distance);
                    const sortedDistances = distances.slice().sort((a, b) => a - b);

                    return {
                        ...signature.touchPattern,
                        distances: sortedDistances,
                        avgDistance: distances.reduce((a, b) => a + b, 0) / distances.length,
                        minDistance: Math.min(...distances),
                        maxDistance: Math.max(...distances),
                        distanceRange: Math.max(...distances) - Math.min(...distances),
                        distanceVariance: this.calculateVariance(distances),
                        distanceSignature: this.createDistanceSignature(sortedDistances)
                    };
                }
            }

            console.log(`❌ Cannot regenerate pattern - insufficient data`);
            return null;
        } catch (error) {
            console.error(`💥 Error regenerating touch pattern:`, error);
            return null;
        }
    }

    // Single job: Create token object
    createTokenObject(name, signature) {
        const trainingConsistency = this.calculateTrainingConsistency();

        return {
            id: Date.now(),
            name: name,
            signature: signature,
            trainingConsistency: trainingConsistency,
            createdAt: new Date().toISOString()
        };
    }

    // Single job: Save token to storage
    async saveTokenToStorage(token) {
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
    }

    /**
     * 4. COMPARE TOKEN SIGNATURES - Dedicated comparison function
     * This function should ONLY compare signature data
     */
    compareTokenSignatures(currentSignature, storedSignature) {
        if (!currentSignature || !storedSignature) return 0;

        // 1. Touch count comparison - must match exactly
        if (currentSignature.touchCount !== storedSignature.touchCount) {
            return 0; // Different finger count = different token
        }

        // 2. 🎯 PRIORITY: Distance-based comparison (95% threshold)
        const distanceScore = this.checkDistanceSimilarity(currentSignature, storedSignature);
        if (distanceScore >= 95) {
            return distanceScore; // Immediate return if distance matches
        }

        // 3. Fallback to detailed comparison if distance doesn't match
        return this.calculateDetailedSimilarity(currentSignature, storedSignature);
    }

    // Single job: Check distance similarity (95% threshold)
    checkDistanceSimilarity(currentSignature, storedSignature) {
        if (currentSignature.touchPattern && storedSignature.touchPattern) {
            const distanceSimilarity = this.calculateDistanceSimilarity(
                currentSignature.touchPattern,
                storedSignature.touchPattern
            );

            if (distanceSimilarity >= 95) {
                console.log(`🎯 DISTANCE MATCH: ${distanceSimilarity}% - Immediate identification!`);
                this.showToast(`🎯 Distance match: ${distanceSimilarity.toFixed(1)}% - Same token!`, 'success');
                return 95;
            }
        }
        return 0;
    }

    // Single job: Calculate detailed similarity score
    calculateDetailedSimilarity(currentSignature, storedSignature) {
        let totalScore = 0;
        let maxScore = 0;

        // Touch count (already verified above)
        totalScore += 25;
        maxScore += 25;

        // Token hash comparison (35% weight)
        if (currentSignature.tokenHash && storedSignature.tokenHash) {
            if (currentSignature.tokenHash === storedSignature.tokenHash) {
                totalScore += 35; // Exact pattern match
            }
        }
        maxScore += 35;

        // Touch pattern comparison (25% weight) - distance relationships
        if (currentSignature.touchPattern && storedSignature.touchPattern) {
            const patternSimilarity = this.calculatePatternSimilarity(
                currentSignature.touchPattern,
                storedSignature.touchPattern
            );
            totalScore += patternSimilarity * 25;
        }
        maxScore += 25;

        // Touch properties comparison (15% weight)
        if (currentSignature.touchProperties && storedSignature.touchProperties) {
            const propertySimilarity = this.compareTouchGeometry(
                currentSignature.touchProperties,
                storedSignature.touchProperties
            );
            totalScore += propertySimilarity * 15;
        }
        maxScore += 15;

        return maxScore > 0 ? Math.round((totalScore / maxScore) * 100) : 0;
    }

    compareTouchGeometry(geometry1, geometry2) {
        let similarity = 0;
        let totalWeight = 0;

        // Compare radius data (if available from browser) - NO PRESSURE
        if (geometry1.hasRadius && geometry2.hasRadius) {
            const radiusDiff = Math.abs(geometry1.avgRadius - geometry2.avgRadius);
            const radiusMax = Math.max(geometry1.avgRadius, geometry2.avgRadius, 1);
            const radiusSim = Math.max(0, 1 - (radiusDiff / radiusMax));
            similarity += radiusSim * 0.7; // Increased weight since no pressure
            totalWeight += 0.7;
        }

        // Compare rotation data (if available from browser)
        if (geometry1.hasRotation && geometry2.hasRotation) {
            const rotationDiff = Math.abs(geometry1.avgRotation - geometry2.avgRotation);
            const rotationSim = Math.max(0, 1 - (rotationDiff / 180)); // 180 degree max
            similarity += rotationSim * 0.3;
            totalWeight += 0.3;
        }

        // If no browser-specific data available, return neutral score
        return totalWeight > 0 ? similarity / totalWeight : 0.5;
    }

    /**
     * 5. POSITION IDENTIFICATION - Dedicated position identification function
     * This function identifies tokens and returns position + label data
     */
    identifyTokenAtPosition(touches) {
        if (this.learnedTokens.length === 0) return null;

        // Get current signature using single source of truth
        const currentSignature = this.getTokenSignature(touches);
        if (!currentSignature) return null;

        // Get position data using single source of truth
        const positionData = this.getPositionData(touches);
        if (!positionData) return null;

        let bestMatch = null;
        let bestConfidence = 0;

        // Compare with all stored tokens
        for (const token of this.learnedTokens) {
            if (!token.signature) continue; // Skip invalid tokens

            const confidence = this.compareTokenSignatures(currentSignature, token.signature);
            if (confidence > bestConfidence && confidence >= this.settings.minConfidence) {
                bestConfidence = confidence;
                bestMatch = {
                    name: token.name,
                    confidence: confidence,
                    position: positionData,
                    token: token
                };
            }
        }

        return bestMatch;
    }

    // ==========================================
    // HELPER FUNCTIONS FOR SIGNATURE COMPARISON
    // ==========================================

    calculateAreaSimilarity(area1, area2) {
        if (!area1 || !area2) return 0;
        const diff = Math.abs(area1.area - area2.area);
        const max = Math.max(area1.area, area2.area);
        return max > 0 ? Math.max(0, 1 - (diff / max)) : 0;
    }

    calculateDistanceSimilarity(pattern1, pattern2) {
        if (!pattern1 || !pattern2) return 0;
        if (pattern1.type !== pattern2.type) return 0;

        // For single touches, they're identical
        if (pattern1.type === 'single') return 100;

        console.log(`📏 Calculating distance similarity between patterns:`, {
            pattern1: { type: pattern1.type, complexity: pattern1.complexity, distances: pattern1.distances },
            pattern2: { type: pattern2.type, complexity: pattern2.complexity, distances: pattern2.distances }
        });

        // 🎯 ENHANCED: Better distance comparison for multi-touch patterns
        if (pattern1.distances && pattern2.distances &&
            pattern1.distances.length > 0 && pattern2.distances.length > 0) {

            // Sort distances for consistent comparison
            const sortedDistances1 = pattern1.distances.slice().sort((a, b) => a - b);
            const sortedDistances2 = pattern2.distances.slice().sort((a, b) => a - b);

            console.log(`📊 Sorted distances - Pattern 1:`, sortedDistances1);
            console.log(`📊 Sorted distances - Pattern 2:`, sortedDistances2);

            // Compare distances with adaptive tolerance based on pattern complexity
            let totalDistanceSimilarity = 0;
            let validComparisons = 0;

            // Adaptive tolerance: more complex patterns get slightly more tolerance
            const baseTolerance = 4; // Base 4px tolerance
            const complexityFactor = Math.min(pattern1.complexity, 5); // Cap at 5 fingers
            const adaptiveTolerance = baseTolerance + (complexityFactor - 2) * 0.5; // +0.5px per finger above 2

            console.log(`📏 Adaptive tolerance: ${adaptiveTolerance.toFixed(1)}px (base: ${baseTolerance}px, complexity: ${complexityFactor})`);

            // Compare each distance with adaptive tolerance
            for (let i = 0; i < Math.min(sortedDistances1.length, sortedDistances2.length); i++) {
                const dist1 = sortedDistances1[i];
                const dist2 = sortedDistances2[i];

                if (dist1 > 0 && dist2 > 0 && !isNaN(dist1) && !isNaN(dist2)) {
                    const pixelDifference = Math.abs(dist1 - dist2);

                    console.log(`📏 Distance ${i + 1}: ${dist1.toFixed(1)}px vs ${dist2.toFixed(1)}px = ${pixelDifference.toFixed(1)}px diff`);

                    // Calculate similarity based on pixel difference and tolerance
                    if (pixelDifference <= adaptiveTolerance) {
                        // Within tolerance - high similarity
                        const similarity = Math.max(0, 100 - (pixelDifference / adaptiveTolerance * 15));
                        totalDistanceSimilarity += similarity;
                        console.log(`  ✅ Within tolerance: ${similarity.toFixed(1)}% similar`);
                    } else {
                        // Outside tolerance - lower similarity but not zero
                        const similarity = Math.max(0, 100 - (pixelDifference * 1.5));
                        totalDistanceSimilarity += similarity;
                        console.log(`  ⚠️ Outside tolerance: ${similarity.toFixed(1)}% similar`);
                    }

                    validComparisons++;
                } else {
                    console.log(`  ❌ Invalid distance data: ${dist1} vs ${dist2}`);
                }
            }

            if (validComparisons > 0) {
                const avgDistanceSimilarity = totalDistanceSimilarity / validComparisons;
                console.log(`📏 Overall Distance Similarity: ${avgDistanceSimilarity.toFixed(1)}% (${validComparisons} valid comparisons)`);

                // Bonus for exact pattern match
                if (sortedDistances1.length === sortedDistances2.length) {
                    const exactMatchBonus = 5; // 5% bonus for same number of distances
                    const finalSimilarity = Math.min(100, avgDistanceSimilarity + exactMatchBonus);
                    console.log(`🎯 Pattern length bonus: +${exactMatchBonus}% = ${finalSimilarity.toFixed(1)}%`);
                    return finalSimilarity;
                }

                return avgDistanceSimilarity;
            }
        }

        console.log(`❌ No valid distance comparisons possible`);
        return 0;
    }

    calculatePatternSimilarity(pattern1, pattern2) {
        if (!pattern1 || !pattern2) return 0;
        if (pattern1.type !== pattern2.type) return 0;

        // For single touches, they're identical
        if (pattern1.type === 'single') return 1.0;

        let similarity = 0;
        let totalWeight = 0;

        // 1. Distance signature comparison (40% weight) - MOST IMPORTANT
        if (pattern1.distanceSignature && pattern2.distanceSignature) {
            if (pattern1.distanceSignature === pattern2.distanceSignature) {
                similarity += 0.4; // Exact match
            } else {
                // Compare distance ratios for partial similarity
                const distanceSim = this.compareDistancePatterns(pattern1, pattern2);
                similarity += distanceSim * 0.4;
            }
            totalWeight += 0.4;
        }

        // 2. Individual distance comparison (30% weight)
        if (pattern1.distances && pattern2.distances &&
            pattern1.distances.length === pattern2.distances.length) {
            const distanceMatchScore = this.compareDistanceArrays(pattern1.distances, pattern2.distances);
            similarity += distanceMatchScore * 0.3;
            totalWeight += 0.3;
        }

        // 3. Distance range comparison (20% weight)
        if (pattern1.distanceRange !== undefined && pattern2.distanceRange !== undefined) {
            const rangeDiff = Math.abs(pattern1.distanceRange - pattern2.distanceRange);
            const rangeMax = Math.max(pattern1.distanceRange, pattern2.distanceRange, 1);
            const rangeSimilarity = Math.max(0, 1 - (rangeDiff / rangeMax));
            similarity += rangeSimilarity * 0.2;
            totalWeight += 0.2;
        }

        // 4. Angle spread comparison (10% weight) - least important
        if (pattern1.angleSpread !== undefined && pattern2.angleSpread !== undefined) {
            const angleDiff = Math.abs(pattern1.angleSpread - pattern2.angleSpread);
            const angleSimilarity = Math.max(0, 1 - (angleDiff / 180)); // Max angle diff is 180
            similarity += angleSimilarity * 0.1;
            totalWeight += 0.1;
        }

        return totalWeight > 0 ? similarity / totalWeight : 0;
    }

    compareDistancePatterns(pattern1, pattern2) {
        if (!pattern1.distances || !pattern2.distances) return 0;
        if (pattern1.distances.length !== pattern2.distances.length) return 0;

        // Compare the smallest distances (most important for finger spacing)
        const minDist1 = pattern1.minDistance || pattern1.distances[0];
        const minDist2 = pattern2.minDistance || pattern2.distances[0];

        if (minDist1 === 0 || minDist2 === 0) return 0;

        // 🎯 STRICT: Use same 4px tolerance for pattern comparison
        const MAX_PIXEL_DIFFERENCE = 4;
        const pixelDifference = Math.abs(minDist1 - minDist2);

        if (pixelDifference <= MAX_PIXEL_DIFFERENCE) {
            // Within tolerance - high similarity
            const similarity = Math.max(0, 1 - (pixelDifference / MAX_PIXEL_DIFFERENCE * 0.2));
            console.log(`📏 Pattern Distance: ${minDist1.toFixed(1)}px vs ${minDist2.toFixed(1)}px = ${pixelDifference.toFixed(1)}px diff (${(similarity * 100).toFixed(1)}% match)`);
            return Math.min(1, similarity);
        } else {
            // Outside tolerance - very low similarity
            console.log(`📏 Pattern Distance: ${minDist1.toFixed(1)}px vs ${minDist2.toFixed(1)}px = ${pixelDifference.toFixed(1)}px diff (OUTSIDE ${MAX_PIXEL_DIFFERENCE}px tolerance)`);
            return 0;
        }
    }

    compareDistanceArrays(distances1, distances2) {
        if (distances1.length !== distances2.length) return 0;

        // 🎯 STRICT: Use same 4px tolerance for array comparison
        const MAX_PIXEL_DIFFERENCE = 4;
        let totalSimilarity = 0;
        let validComparisons = 0;

        for (let i = 0; i < distances1.length; i++) {
            const pixelDifference = Math.abs(distances1[i] - distances2[i]);

            if (pixelDifference <= MAX_PIXEL_DIFFERENCE) {
                // Within tolerance - high similarity
                const similarity = Math.max(0, 1 - (pixelDifference / MAX_PIXEL_DIFFERENCE * 0.2));
                totalSimilarity += similarity;
                console.log(`📏 Array Distance ${i}: ${distances1[i].toFixed(1)}px vs ${distances2[i].toFixed(1)}px = ${pixelDifference.toFixed(1)}px diff (${(similarity * 100).toFixed(1)}% match)`);
            } else {
                // Outside tolerance - very low similarity
                const similarity = Math.max(0, 1 - (pixelDifference * 0.1));
                totalSimilarity += similarity;
                console.log(`📏 Array Distance ${i}: ${distances1[i].toFixed(1)}px vs ${distances2[i].toFixed(1)}px = ${pixelDifference.toFixed(1)}px diff (OUTSIDE tolerance)`);
            }

            validComparisons++;
        }

        return validComparisons > 0 ? totalSimilarity / validComparisons : 0;
    }

    calculatePressureSimilarity(pressure1, pressure2) {
        const diff = Math.abs(pressure1 - pressure2);
        return Math.max(0, 1 - (diff / 1.0)); // Pressure range 0-1
    }

    calculateRadiusSimilarity(radius1, radius2) {
        const diff = Math.abs(radius1 - radius2);
        const max = Math.max(radius1, radius2, 1); // Avoid division by zero
        return Math.max(0, 1 - (diff / max));
    }

    // REMOVED: generateSignatureHash() - old position-dependent function
    // Replaced by getTokenSignature() for position-independent signatures

    calculateBoundingBox(touches) {
        if (touches.length === 0) return null;

        let minX = Infinity, maxX = -Infinity;
        let minY = Infinity, maxY = -Infinity;

        touches.forEach(touch => {
            minX = Math.min(minX, touch.clientX);
            maxX = Math.max(maxX, touch.clientX);
            minY = Math.min(minY, touch.clientY);
            maxY = Math.max(maxY, touch.clientY);
        });

        return {
            x: minX,
            y: minY,
            width: maxX - minX,
            height: maxY - minY,
            centerX: (minX + maxX) / 2,
            centerY: (minY + maxY) / 2
        };
    }

    checkSignatureUniqueness(signature) {
        if (!signature) {
            return { isUnique: false, reason: 'Invalid signature' };
        }

        // Check minimum complexity
        if (signature.touchCount < 1) {
            return { isUnique: false, reason: 'Token too simple - need at least 1 touch' };
        }

        // Check against existing tokens with more reasonable threshold
        for (const existingToken of this.learnedTokens) {
            if (!existingToken.signature) continue;

            const similarity = this.compareTokenSignatures(signature, existingToken.signature);
            if (similarity > 95) { // Increased threshold to 95% - only reject if nearly identical
                return {
                    isUnique: false,
                    reason: `Too similar to existing token "${existingToken.name}" (${similarity}% match). Try a different touch pattern.`
                };
            }
        }

        return { isUnique: true };
    }

    // ==========================================
    // UPDATED MAIN FUNCTIONS
    // ==========================================

    // REMOVED: identifyToken() - duplicate wrapper function
    // All calls now use identifyTokenAtPosition() directly

    showTokenNameModal() {
        document.getElementById('tokenNameModal').style.display = 'block';
        document.getElementById('tokenNameInput').focus();

        // Show training summary
        const summary = document.getElementById('trainingSummary');
        if (this.currentTrainingSession && this.currentTrainingSession.touchSignatures) {
            const signatures = this.currentTrainingSession.touchSignatures;
            summary.innerHTML = `
                <div>Touch signatures recorded: ${signatures.length}/5</div>
                <div>Touch patterns: ${signatures.map(sig => sig.touchCount).join(', ')}</div>
                <div>Training progress: ${signatures.length * 20}% complete</div>
            `;
        } else {
            summary.innerHTML = `
                <div style="color: #f44336;">⚠️ Training data not found</div>
                <div>Please complete the training process first</div>
            `;
        }
    }

    hideTokenNameModal() {
        document.getElementById('tokenNameModal').style.display = 'none';
        document.getElementById('tokenNameInput').value = '';
    }

    saveToken() {
        console.log(`💾 SAVE TOKEN: Starting save process...`);

        // 🚨 DEBUG: Check training session state before saving
        this.debugTrainingSession();

        // Validate input
        const name = this.validateTokenName();
        if (!name) return;

        // Validate training data
        if (!this.validateTrainingData()) return;

        // Save token using single source of truth
        const saveResult = this.saveTokenSignature(name, this.currentTrainingSession.touchSignatures);

        if (!saveResult.success) {
            console.log(`❌ SAVE FAILED: ${saveResult.error}`);
            this.showToast(saveResult.error, 'error');
            return;
        }

        console.log(`✅ Token saved successfully:`, saveResult.token);

        // Complete save process
        this.completeTokenSave(name);
    }

    // Single job: Validate token name
    validateTokenName() {
        const name = document.getElementById('tokenNameInput').value.trim();
        if (!name) {
            console.log(`❌ SAVE FAILED: No token name provided`);
            this.showToast('Please enter a token name', 'error');
            return null;
        }
        console.log(`📝 Token name: "${name}"`);
        return name;
    }

    // Single job: Validate training data
    validateTrainingData() {
        if (!this.currentTrainingSession) {
            console.log(`❌ SAVE FAILED: No training session found`);
            this.showToast('No training session found. Please start training first.', 'error');
            return false;
        }

        if (!this.currentTrainingSession.touchSignatures) {
            console.log(`❌ SAVE FAILED: No touch signatures in training session`);
            this.showToast('No touch signatures found. Please complete training first.', 'error');
            return false;
        }

        if (this.currentTrainingSession.touchSignatures.length !== 5) {
            console.log(`❌ SAVE FAILED: Incomplete training data - ${this.currentTrainingSession.touchSignatures.length}/5 touches`);
            this.showToast(`Incomplete training data: ${this.currentTrainingSession.touchSignatures.length}/5 touches required. Please complete training first.`, 'error');
            return false;
        }

        // Additional validation: Check if all signatures are valid
        const validSignatures = this.currentTrainingSession.touchSignatures.filter(sig =>
            sig && sig.touchCount && sig.touchCount >= 2
        );

        if (validSignatures.length !== 5) {
            console.log(`❌ SAVE FAILED: Invalid signatures found - ${validSignatures.length}/5 valid signatures`);
            this.showToast(`Invalid signatures detected. Please restart training with at least 2 touch points.`, 'error');
            return false;
        }

        console.log(`📊 Training data validated: ${this.currentTrainingSession.touchSignatures.length}/5 touches, all valid`);

        // Debug: Log signature details for troubleshooting
        this.currentTrainingSession.touchSignatures.forEach((sig, index) => {
            console.log(`📝 Signature ${index + 1}: ${sig.touchCount} touches, hash: ${sig.tokenHash?.substring(0, 8)}...`);
        });

        return true;
    }

    // 🚨 DEBUG: Check training session state
    debugTrainingSession() {
        console.log(`🔍 TRAINING SESSION DEBUG:`, {
            hasSession: !!this.currentTrainingSession,
            mode: this.mode,
            sessionData: this.currentTrainingSession ? {
                currentPosition: this.currentTrainingSession.currentPosition,
                signaturesCount: this.currentTrainingSession.touchSignatures?.length || 0,
                hasReference: !!this.currentTrainingSession.referenceSignature,
                touchSignatures: this.currentTrainingSession.touchSignatures?.map(sig => ({
                    touchCount: sig.touchCount,
                    timestamp: sig.timestamp,
                    hasHash: !!sig.tokenHash
                })) || []
            } : 'NO SESSION'
        });

        // 🚨 EXPOSE DEBUG FUNCTION TO CONSOLE
        if (typeof window !== 'undefined') {
            window.debugTraining = () => this.debugTrainingSession();
            console.log(`💡 DEBUG: Call 'debugTraining()' in console to check training state`);
        }
    }

    // Single job: Complete token save process
    completeTokenSave(name) {
        // Update UI and return to main menu
        this.updateTokenCount();
        this.hideTokenNameModal();
        this.returnToMainMenu();

        // Show success message with detailed token information
        const consistency = this.calculateTrainingConsistency();
        const savedToken = this.learnedTokens[this.learnedTokens.length - 1]; // Get the most recently saved token

        console.log(`🎉 TOKEN SAVED SUCCESSFULLY: "${name}" with ${consistency.avg.toFixed(1)}% consistency`);

        // 🗺️ NEW: Place token marker on map if map view is active
        if (this.mapViewActive && this.map) {
            const location = this.getCurrentMapLocation();
            if (location) {
                this.placeTokenMarkerOnMap(savedToken, location);
            }
        }

        // Display detailed token information
        this.displaySavedTokenInfo(savedToken, consistency);

        // Show toast notification
        this.showToast(`Token "${name}" saved! Training consistency: ${consistency.avg.toFixed(1)}%`, 'success');
    }

    // NEW: Display detailed saved token information
    displaySavedTokenInfo(token, consistency) {
        // Create a detailed information modal
        const modal = document.createElement('div');
        modal.className = 'modal';
        modal.style.display = 'block';
        modal.innerHTML = `
            <div class="modal-content" style="max-width: 600px;">
                <div class="modal-header">
                    <h2>✅ Token Saved Successfully!</h2>
                    <button class="close-btn" onclick="this.closest('.modal').remove()">&times;</button>
                </div>
                <div class="modal-body">
                    <div style="background: rgba(76, 175, 80, 0.1); border: 1px solid #4CAF50; border-radius: 8px; padding: 15px; margin-bottom: 20px;">
                        <h3 style="color: #4CAF50; margin-bottom: 10px;">🎯 Token: ${token.name}</h3>
                        <div style="font-family: monospace; font-size: 14px;">
                            <div>📅 Created: ${new Date(token.createdAt).toLocaleString()}</div>
                            <div>🆔 ID: ${token.id}</div>
                        </div>
                    </div>
                    
                    <div style="background: rgba(33, 150, 243, 0.1); border: 1px solid #2196F3; border-radius: 8px; padding: 15px; margin-bottom: 20px;">
                        <h3 style="color: #2196F3; margin-bottom: 10px;">📊 Training Statistics</h3>
                        <div style="font-family: monospace; font-size: 14px;">
                            <div>🎯 Touch Count: ${token.signature.touchCount} fingers</div>
                            <div>📈 Average Consistency: ${consistency.avg.toFixed(1)}%</div>
                            <div>📉 Minimum Consistency: ${consistency.min.toFixed(1)}%</div>
                            <div>📊 Maximum Consistency: ${consistency.max.toFixed(1)}%</div>
                            <div>🔄 Training Sessions: ${consistency.touchCount}/5 completed</div>
                        </div>
                    </div>
                    
                    <div style="background: rgba(255, 152, 0, 0.1); border: 1px solid #ff9800; border-radius: 8px; padding: 15px; margin-bottom: 20px;">
                        <h3 style="color: #ff9800; margin-bottom: 10px;">📏 Distance Information</h3>
                        <div style="font-family: monospace; font-size: 14px;">
                            ${this.generateDistanceInfoHTML(token.signature)}
                        </div>
                    </div>
                    
                    <div style="background: rgba(156, 39, 176, 0.1); border: 1px solid #9c27b0; border-radius: 8px; padding: 15px;">
                        <h3 style="color: #9c27b0; margin-bottom: 10px;">🔍 Pattern Analysis</h3>
                        <div style="font-family: monospace; font-size: 14px;">
                            ${this.generatePatternInfoHTML(token.signature)}
                        </div>
                    </div>
                </div>
                <div class="modal-footer">
                    <button class="btn" onclick="this.closest('.modal').remove()">Close</button>
                    <button class="btn info" onclick="tokenSystem.showTokenManagement(); this.closest('.modal').remove();">View All Tokens</button>
                </div>
            </div>
        `;

        document.body.appendChild(modal);

        // Auto-close after 10 seconds
        setTimeout(() => {
            if (modal.parentNode) {
                modal.remove();
            }
        }, 10000);
    }

    // Helper function to generate distance information HTML
    generateDistanceInfoHTML(signature) {
        if (!signature.touchPattern) {
            return '<div>❌ No distance pattern available</div>';
        }

        const pattern = signature.touchPattern;
        let html = '';

        if (pattern.type === 'single') {
            html = '<div>👆 Single touch pattern</div>';
        } else if (pattern.type === 'multi') {
            html = `
                <div>🤏 Multi-touch pattern (${pattern.complexity} fingers)</div>
                <div>📏 Minimum Distance: ${Math.round(pattern.minDistance)}px</div>
                <div>📏 Maximum Distance: ${Math.round(pattern.maxDistance)}px</div>
                <div>📏 Average Distance: ${Math.round(pattern.avgDistance)}px</div>
                <div>📏 Distance Range: ${Math.round(pattern.distanceRange)}px</div>
                <div>🔢 All Distances: ${pattern.distances.map(d => Math.round(d) + 'px').join(', ')}</div>
                <div>🏷️ Distance Signature: ${pattern.distanceSignature}</div>
            `;
        }

        return html;
    }

    // Helper function to generate pattern analysis HTML
    generatePatternInfoHTML(signature) {
        if (!signature.touchPattern) {
            return '<div>❌ No pattern analysis available</div>';
        }

        const pattern = signature.touchPattern;
        let html = '';

        if (pattern.type === 'single') {
            html = '<div>👆 Single touch - no complex pattern</div>';
        } else if (pattern.type === 'multi') {
            html = `
                <div>🤏 Multi-touch complexity: ${pattern.complexity}</div>
                <div>📐 Angle spread: ${Math.round(pattern.angleSpread)}°</div>
                <div>📊 Distance variance: ${pattern.distanceVariance ? Math.round(pattern.distanceVariance) : 'N/A'}</div>
            `;

            // Add geometric center if available
            if (pattern.geometricCenter) {
                html += `<div>🎯 Geometric center: (${Math.round(pattern.geometricCenter.x)}, ${Math.round(pattern.geometricCenter.y)})</div>`;
            }

            // Add distance pairs information
            if (pattern.distancePairs && pattern.distancePairs.length > 0) {
                html += '<div>🔗 Distance pairs:</div>';
                pattern.distancePairs.forEach((pair, index) => {
                    html += `<div style="margin-left: 20px;">${index + 1}. F${pair.from + 1}→F${pair.to + 1}: ${Math.round(pair.distance)}px @ ${Math.round(pair.angle)}°</div>`;
                });
            }
        }

        // Add touch properties if available
        if (signature.touchProperties) {
            const props = signature.touchProperties;
            html += '<div style="margin-top: 10px; border-top: 1px solid rgba(255,255,255,0.2); padding-top: 10px;">';
            html += '<div>👆 Touch Properties:</div>';
            html += `<div style="margin-left: 20px;">Radius: ${props.hasRadius ? 'Yes' : 'No'}</div>`;
            if (props.hasRadius) {
                html += `<div style="margin-left: 20px;">Average Radius: ${Math.round(props.avgRadius)}px</div>`;
            }
            html += `<div style="margin-left: 20px;">Rotation: ${props.hasRotation ? 'Yes' : 'No'}</div>`;
            if (props.hasRotation) {
                html += `<div style="margin-left: 20px;">Average Rotation: ${Math.round(props.avgRotation)}°</div>`;
            }
            html += '</div>';
        }

        return html;
    }

    calculateTrainingConsistency() {
        const signatures = this.currentTrainingSession.touchSignatures;
        const reference = signatures[0];

        let totalConsistency = 0;
        let minConsistency = 100;
        let maxConsistency = 0;

        for (let i = 1; i < signatures.length; i++) {
            const consistency = this.compareTokenSignatures(signatures[i], reference);
            totalConsistency += consistency;
            minConsistency = Math.min(minConsistency, consistency);
            maxConsistency = Math.max(maxConsistency, consistency);
        }

        return {
            avg: totalConsistency / (signatures.length - 1),
            min: minConsistency,
            max: maxConsistency,
            touchCount: signatures.length
        };
    }

    showTokenManagement() {
        document.getElementById('tokenManagementModal').style.display = 'block';
        this.updateManagementTokenList();
    }

    hideTokenManagement() {
        document.getElementById('tokenManagementModal').style.display = 'none';
    }

    updateManagementTokenList() {
        const container = document.getElementById('managementTokenList');
        const totalTokens = document.getElementById('totalTokens');

        totalTokens.textContent = `${this.learnedTokens.length} token${this.learnedTokens.length !== 1 ? 's' : ''} learned`;

        if (this.learnedTokens.length === 0) {
            container.innerHTML = '<div style="text-align: center; color: #E0E0E0; padding: 20px;">No tokens learned yet</div>';
            return;
        }

        container.innerHTML = this.learnedTokens.map(token => {
            const pattern = token.signature.touchPattern;
            const distanceInfo = pattern ? this.getDistanceSummary(pattern) : 'No pattern data';
            const consistency = token.trainingConsistency ? token.trainingConsistency.avg.toFixed(1) + '%' : 'N/A';

            return `
                <div class="token-item">
                    <div class="token-name">${token.name}</div>
                    <div class="token-details">
                        <div style="margin-bottom: 8px;">
                            <strong>📅 Created:</strong> ${new Date(token.createdAt).toLocaleDateString()}<br>
                            <strong>🎯 Touch Count:</strong> ${token.signature.touchCount} fingers<br>
                            <strong>📈 Training Consistency:</strong> ${consistency}
                        </div>
                        <div style="background: rgba(255,255,255,0.1); padding: 8px; border-radius: 4px; font-size: 12px;">
                            <strong>📏 Distance Info:</strong> ${distanceInfo}
                        </div>
                    </div>
                    <div style="margin-top: 10px; display: flex; gap: 5px;">
                        <button class="btn info" onclick="tokenSystem.showTokenDetails(${token.id})" style="padding: 5px 10px; font-size: 12px;">📋 Details</button>
                        <button class="btn danger" onclick="tokenSystem.deleteToken(${token.id})" style="padding: 5px 10px; font-size: 12px;">🗑️ Delete</button>
                    </div>
                </div>
            `;
        }).join('');
    }

    // NEW: Get distance summary for token management display
    getDistanceSummary(pattern) {
        if (!pattern || pattern.type === 'single') {
            return 'Single touch';
        }

        return `${pattern.complexity} fingers, ${Math.round(pattern.minDistance)}-${Math.round(pattern.maxDistance)}px range`;
    }

    // NEW: Show detailed token information
    showTokenDetails(tokenId) {
        const token = this.learnedTokens.find(t => t.id === tokenId);
        if (!token) return;

        const consistency = token.trainingConsistency;

        const modal = document.createElement('div');
        modal.className = 'modal';
        modal.style.display = 'block';
        modal.innerHTML = `
            <div class="modal-content" style="max-width: 700px;">
                <div class="modal-header">
                    <h2>📋 Token Details: ${token.name}</h2>
                    <button class="close-btn" onclick="this.closest('.modal').remove()">&times;</button>
                </div>
                <div class="modal-body">
                    <div style="background: rgba(76, 175, 80, 0.1); border: 1px solid #4CAF50; border-radius: 8px; padding: 15px; margin-bottom: 20px;">
                        <h3 style="color: #4CAF50; margin-bottom: 10px;">🎯 Basic Information</h3>
                        <div style="font-family: monospace; font-size: 14px;">
                            <div>📅 Created: ${new Date(token.createdAt).toLocaleString()}</div>
                            <div>🆔 ID: ${token.id}</div>
                            <div>🎯 Touch Count: ${token.signature.touchCount} fingers</div>
                        </div>
                    </div>
                    
                    <div style="background: rgba(33, 150, 243, 0.1); border: 1px solid #2196F3; border-radius: 8px; padding: 15px; margin-bottom: 20px;">
                        <h3 style="color: #2196F3; margin-bottom: 10px;">📊 Training Statistics</h3>
                        <div style="font-family: monospace; font-size: 14px;">
                            <div>📈 Average Consistency: ${consistency ? consistency.avg.toFixed(1) + '%' : 'N/A'}</div>
                            <div>📉 Minimum Consistency: ${consistency ? consistency.min.toFixed(1) + '%' : 'N/A'}</div>
                            <div>📊 Maximum Consistency: ${consistency ? consistency.max.toFixed(1) + '%' : 'N/A'}</div>
                            <div>🔄 Training Sessions: ${consistency ? consistency.touchCount + '/5' : 'N/A'} completed</div>
                        </div>
                    </div>
                    
                    <div style="background: rgba(255, 152, 0, 0.1); border: 1px solid #ff9800; border-radius: 8px; padding: 15px; margin-bottom: 20px;">
                        <h3 style="color: #ff9800; margin-bottom: 10px;">📏 Distance Information</h3>
                        <div style="font-family: monospace; font-size: 14px;">
                            ${this.generateDistanceInfoHTML(token.signature)}
                        </div>
                    </div>
                    
                    <div style="background: rgba(156, 39, 176, 0.1); border: 1px solid #9c27b0; border-radius: 8px; padding: 15px;">
                        <h3 style="color: #9c27b0; margin-bottom: 10px;">🔍 Pattern Analysis</h3>
                        <div style="font-family: monospace; font-size: 14px;">
                            ${this.generatePatternInfoHTML(token.signature)}
                        </div>
                    </div>
                </div>
                <div class="modal-footer">
                    <button class="btn" onclick="this.closest('.modal').remove()">Close</button>
                    <button class="btn danger" onclick="tokenSystem.deleteToken(${token.id}); this.closest('.modal').remove();">Delete Token</button>
                </div>
            </div>
        `;

        document.body.appendChild(modal);
    }

    deleteToken(tokenId) {
        const token = this.learnedTokens.find(t => t.id === tokenId);
        if (!token) return;

        document.getElementById('deleteConfirmMessage').textContent = `Are you sure you want to delete token "${token.name}"?`;
        document.getElementById('deleteTokenPreview').innerHTML = `
            <div class="token-item">
                <div class="token-name">${token.name}</div>
                <div class="token-details">Touch Count: ${token.signature.touchCount}</div>
            </div>
        `;
        document.getElementById('deleteConfirmModal').style.display = 'block';
        this.tokenToDelete = tokenId;
    }

    hideDeleteConfirmModal() {
        document.getElementById('deleteConfirmModal').style.display = 'none';
        this.tokenToDelete = null;
    }

    async confirmDeleteToken() {
        if (this.tokenToDelete) {
            const token = this.learnedTokens.find(t => t.id === this.tokenToDelete);
            if (!token) return;

            try {
                // Delete from database
                await window.apiService.deleteToken(this.tokenToDelete);
                
                // Delete associated map markers
                await window.apiService.deleteMapMarkersByToken(this.tokenToDelete);
                
                // Remove from local array
                this.learnedTokens = this.learnedTokens.filter(t => t.id !== this.tokenToDelete);
                
                // Remove from map if exists
                this.removeTokenMarker(this.tokenToDelete);
                
                console.log(`📡 Token ${this.tokenToDelete} deleted from database`);
                this.updateTokenCount();
                this.updateManagementTokenList();
                this.showToast(`Token "${token.name}" deleted successfully`, 'success');
            } catch (error) {
                console.error('Error deleting token from database:', error);
                this.showToast('Failed to delete token from database', 'error');
            }
        }
        this.hideDeleteConfirmModal();
    }

    async deleteAllTokens() {
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
                this.updateManagementTokenList();
                this.showToast('All tokens deleted', 'warning');
            } catch (error) {
                console.error('Error deleting all tokens from database:', error);
                this.showToast('Failed to delete all tokens from database', 'error');
            }
        }
    }

    exportTokens() {
        const dataStr = JSON.stringify(this.learnedTokens, null, 2);
        const dataBlob = new Blob([dataStr], { type: 'application/json' });
        const url = URL.createObjectURL(dataBlob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `tokens_${new Date().toISOString().split('T')[0]}.json`;
        link.click();
        URL.revokeObjectURL(url);
        this.showToast('Tokens exported successfully', 'success');
    }

    importTokens(event) {
        const file = event.target.files[0];
        if (!file) return;

        const reader = new FileReader();
        reader.onload = (e) => {
            try {
                const importedTokens = JSON.parse(e.target.result);
                this.learnedTokens = importedTokens;
                this.saveTokensToStorage();
                this.updateTokenCount();
                this.updateManagementTokenList();
                this.showToast(`Imported ${importedTokens.length} tokens successfully`, 'success');
            } catch (error) {
                this.showToast('Error importing tokens: Invalid file format', 'error');
            }
        };
        reader.readAsText(file);
        event.target.value = ''; // Reset file input
    }

    cancelTraining() {
        this.currentTrainingSession = null;
        this.returnToMainMenu();
        this.showToast('Training cancelled', 'warning');
    }

    clearAllPermanentLabels() {
        this.permanentLabels = [];
        this.showToast('Screen cleared', 'info');
    }

    updateTokenCount() {
        document.getElementById('tokenCount').textContent = this.learnedTokens.length;
    }

    // Initialize database operations
    async initializeDatabase() {
        try {
            console.log('📡 Initializing database operations...');
            
            // Check if API service is available
            if (!window.apiService) {
                console.error('API service not available');
                this.showToast('Database service not available', 'error');
                return;
            }
            
            // Try to migrate from localStorage if needed
            await this.migrateFromLocalStorage();
            
            // Load tokens from database
            await this.loadTokensFromStorage();
            
            console.log('📡 Database operations initialized successfully');
        } catch (error) {
            console.error('Error initializing database operations:', error);
            this.showToast('Failed to initialize database operations', 'error');
        }
    }

    // Migrate data from localStorage to database (one-time migration)
    async migrateFromLocalStorage() {
        try {
            console.log('🔄 Checking for localStorage migration...');
            
            // Check if migration is needed
            const hasLocalTokens = localStorage.getItem('learnedTokens');
            const hasLocalMarkers = localStorage.getItem('mapTokenMarkers');
            
            if (!hasLocalTokens && !hasLocalMarkers) {
                console.log('📡 No localStorage data to migrate');
                return;
            }
            
            console.log('🔄 Starting migration from localStorage to database...');
            
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
    }

    async loadTokensFromStorage() {
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
    }

    // 🗺️ NEW: Try to restore map markers when tokens are available
    tryRestoreMapMarkers() {
        if (this.pendingMapMarkers && this.map && this.learnedTokens.length > 0) {
            console.log('🗺️ Tokens loaded, attempting to restore map markers...');
            this.restorePendingMapMarkers();
        }
    }



    async saveTokensToStorage() {
        try {
            console.log('📡 Saving tokens to database...');
            
            // Convert JavaScript format to database format
            const dbTokens = this.learnedTokens.map(jsToken => 
                window.apiService.convertTokenToDbFormat(jsToken)
            );
            
            // Use bulk import to save all tokens
            const results = await window.apiService.bulkImportTokens(dbTokens);
            
            // Check for any failures
            const failures = results.filter(result => !result.Success);
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
    }

    // 🗺️ NEW: Save map markers to storage
    async saveMapMarkersToStorage() {
        try {
            console.log('📡 Saving map markers to database...');
            
            const markersData = Array.from(this.mapTokenMarkers.entries()).map(([id, data]) => ({
                id: id,
                tokenId: data.tokenId,
                location: data.location,
                createdAt: data.createdAt,
                tokenName: data.tokenName // Add token name for better debugging
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
    }

    // 🗺️ NEW: Load map markers from storage
    async loadMapMarkersFromStorage() {
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
    }

    // 🗺️ NEW: Restore pending map markers when map is ready
    restorePendingMapMarkers() {
        if (this.pendingMapMarkers && this.map && this.learnedTokens.length > 0) {
            console.log(`🗺️ Restoring ${this.pendingMapMarkers.length} pending map markers...`);

            let restoredCount = 0;
            this.pendingMapMarkers.forEach(markerData => {
                try {
                    // Find the corresponding token
                    const token = this.learnedTokens.find(t => t.id === markerData.tokenId);
                    if (token) {
                        console.log(`🗺️ Restoring marker for token: ${token.name} at ${markerData.location.lat}, ${markerData.location.lng}`);
                        this.placeTokenMarkerOnMap(token, markerData.location, true); // true = isRestore
                        restoredCount++;
                    } else {
                        console.warn(`🗺️ Token not found for marker: ${markerData.tokenId}`);
                    }
                } catch (error) {
                    console.error(`🗺️ Error restoring marker ${markerData.id}:`, error);
                }
            });

            this.pendingMapMarkers = null;
            console.log(`🗺️ Successfully restored ${restoredCount} map markers from storage`);

            // Show success message
            if (restoredCount > 0) {
                this.showToast(`🗺️ Restored ${restoredCount} token markers from previous session`, 'success');
            }
        } else {
            if (this.pendingMapMarkers) {
                console.log(`🗺️ Map markers restoration delayed - waiting for map (${!!this.map}) and tokens (${this.learnedTokens.length})`);
            }
        }
    }

    showToast(message, type = 'info') {
        const toast = document.createElement('div');
        toast.className = `toast ${type}`;
        toast.textContent = message;

        document.getElementById('toastContainer').appendChild(toast);

        setTimeout(() => toast.classList.add('show'), 100);
        setTimeout(() => {
            toast.classList.remove('show');
            setTimeout(() => toast.remove(), 300);
        }, 3000);
    }

    startAnimationLoop() {
        const animate = () => {
            this.draw();
            requestAnimationFrame(animate);
        };
        animate();
    }

    draw() {
        try {
            // Clear canvas
            this.ctx.clearRect(0, 0, this.canvas.width, this.canvas.height);

            // Draw grid if enabled
            if (this.settings.showGrid) {
                this.drawGrid();
            }

            // Draw active touches
            this.drawActiveTouches();

            // Draw permanent labels
            this.drawPermanentLabels();
        } catch (error) {
            console.error('Error in draw function:', error);
        }
    }

    drawGrid() {
        try {
            // Set grid style for better visibility on light background
            this.ctx.strokeStyle = 'rgba(34, 197, 94, 0.15)'; // Premium green grid lines - matches CSS variable --color-primary
            this.ctx.lineWidth = 1;
            this.ctx.setLineDash([]);

            const gridSize = 40; // Optimal grid size for visibility
            
            // Get the actual display dimensions from the canvas style
            const displayWidth = parseInt(this.canvas.style.width) || this.canvas.width;
            const displayHeight = parseInt(this.canvas.style.height) || this.canvas.height;
            
            // Calculate grid offset to center the grid properly
            const offsetX = (displayWidth % gridSize) / 2;
            const offsetY = (displayHeight % gridSize) / 2;
            
            // Draw vertical lines
            for (let x = offsetX; x <= displayWidth; x += gridSize) {
                this.ctx.beginPath();
                this.ctx.moveTo(x, 0);
                this.ctx.lineTo(x, displayHeight);
                this.ctx.stroke();
            }

            // Draw horizontal lines
            for (let y = offsetY; y <= displayHeight; y += gridSize) {
                this.ctx.beginPath();
                this.ctx.moveTo(0, y);
                this.ctx.lineTo(displayWidth, y);
                this.ctx.stroke();
            }
            
            // Draw center lines with premium accent color - matches CSS variable --secondary-500
            this.ctx.strokeStyle = 'rgba(16, 185, 129, 0.6)';
            this.ctx.lineWidth = 2;
            
            // Center vertical line
            this.ctx.beginPath();
            this.ctx.moveTo(displayWidth / 2, 0);
            this.ctx.lineTo(displayWidth / 2, displayHeight);
            this.ctx.stroke();
            
            // Center horizontal line
            this.ctx.beginPath();
            this.ctx.moveTo(0, displayHeight / 2);
            this.ctx.lineTo(displayWidth, displayHeight / 2);
            this.ctx.stroke();
            
        } catch (error) {
            console.error('Error drawing grid:', error);
        }
    }

    drawActiveTouches() {
        // Draw active touch points
        this.activeTouches.forEach((touch, id) => {
            this.ctx.fillStyle = '#4CAF50';
            this.ctx.beginPath();
            this.ctx.arc(touch.x, touch.y, 20, 0, Math.PI * 2);
            this.ctx.fill();

            this.ctx.fillStyle = 'white';
            this.ctx.font = '14px Arial';
            this.ctx.textAlign = 'center';
            this.ctx.fillText(id, touch.x, touch.y + 5);
        });
    }

    drawPermanentLabels() {
        // Draw permanent identification labels
        this.permanentLabels.forEach(label => {
            this.ctx.fillStyle = 'rgba(76, 175, 80, 0.8)';
            this.ctx.fillRect(label.x - 50, label.y - 30, 100, 60);

            this.ctx.fillStyle = 'white';
            this.ctx.font = '12px Arial';
            this.ctx.textAlign = 'center';
            this.ctx.fillText(label.name, label.x, label.y);
            this.ctx.fillText(`${label.confidence}%`, label.x, label.y + 15);
        });
    }

    // NEW: Real-time distance calculation function
    calculateRealTimeDistance(touches) {
        if (!touches || touches.length === 0) {
            return {
                touchCount: 0,
                distanceInfo: 'No touches',
                positions: 'No positions',
                details: 'No details'
            };
        }

        if (touches.length === 1) {
            // Single touch
            return {
                touchCount: 1,
                distanceInfo: 'Single Touch',
                positions: `(${Math.round(touches[0].clientX)}, ${Math.round(touches[0].clientY)})`,
                details: '1 touch'
            };
        }

        if (touches.length === 2) {
            // Two touches - calculate distance directly
            const touch1 = touches[0];
            const touch2 = touches[1];
            const distance = Math.sqrt(
                Math.pow(touch2.clientX - touch1.clientX, 2) +
                Math.pow(touch2.clientY - touch1.clientY, 2)
            );

            return {
                touchCount: 2,
                distanceInfo: `Distance: ${Math.round(distance)}px`,
                positions: `P1:(${Math.round(touch1.clientX)},${Math.round(touch1.clientY)}) P2:(${Math.round(touch2.clientX)},${Math.round(touch2.clientY)})`,
                details: `2 touches | Distance: ${Math.round(distance)}px`
            };
        }

        // Multi-touch (3+ touches) - calculate all distances
        const distances = [];
        for (let i = 0; i < touches.length; i++) {
            for (let j = i + 1; j < touches.length; j++) {
                const dx = touches[j].clientX - touches[i].clientX;
                const dy = touches[j].clientY - touches[i].clientY;
                const distance = Math.sqrt(dx * dx + dy * dy);
                distances.push(distance);
            }
        }

        const sortedDistances = distances.slice().sort((a, b) => a - b);
        const minDistance = Math.min(...distances);
        const maxDistance = Math.max(...distances);

        const positions = touches.map((touch, i) =>
            `P${i + 1}:(${Math.round(touch.clientX)},${Math.round(touch.clientY)})`
        ).join(' ');

        return {
            touchCount: touches.length,
            distanceInfo: `${touches.length} touches | Min:${Math.round(minDistance)}px Max:${Math.round(maxDistance)}px`,
            positions: positions,
            details: `Distances: ${sortedDistances.map(d => Math.round(d) + 'px').join(', ')}`
        };
    }

    // NEW: Debug function to monitor distance calculations
    debugDistanceCalculations(touches, context = 'Unknown') {
        console.log(`🔍 DEBUG DISTANCE CALCULATIONS [${context}]`);
        console.log(`📊 Input touches:`, touches.map((t, i) => ({
            index: i,
            x: Math.round(t.clientX),
            y: Math.round(t.clientY),
            identifier: t.identifier
        })));

        if (touches.length >= 2) {
            // Calculate distances manually for verification
            const manualDistances = [];
            for (let i = 0; i < touches.length; i++) {
                for (let j = i + 1; j < touches.length; j++) {
                    const dx = touches[j].clientX - touches[i].clientX;
                    const dy = touches[j].clientY - touches[i].clientY;
                    const distance = Math.sqrt(dx * dx + dy * dy);
                    manualDistances.push({
                        from: i,
                        to: j,
                        distance: Math.round(distance),
                        dx: Math.round(dx),
                        dy: Math.round(dy)
                    });
                }
            }

            console.log(`📏 Manual distance calculations:`, manualDistances);

            // Compare with module function
            const realTimeData = this.calculateRealTimeDistance(touches);
            console.log(`📏 Real-time distance data:`, realTimeData);

            // Also test the analyzeTouchPattern function
            try {
                const pattern = this.analyzeTouchPattern(touches);
                console.log(`📏 Module pattern analysis:`, {
                    type: pattern.type,
                    complexity: pattern.complexity,
                    distances: pattern.distances.map(d => Math.round(d)),
                    minDistance: Math.round(pattern.minDistance),
                    maxDistance: Math.round(pattern.maxDistance),
                    distanceSignature: pattern.distanceSignature
                });
            } catch (error) {
                console.error(`❌ Error in analyzeTouchPattern:`, error);
            }
        }

        console.log(`🔍 END DEBUG [${context}]`);
    }

    // NEW: Distance monitoring function
    monitorDistanceCalculations() {
        // Add a visual indicator to show when distance calculations are active
        const indicator = document.createElement('div');
        indicator.id = 'distanceIndicator';
        indicator.style.cssText = `
            position: fixed;
            top: 50px;
            right: 10px;
            width: 20px;
            height: 20px;
            background: #4CAF50;
            border-radius: 50%;
            z-index: 10000;
            opacity: 0;
            transition: opacity 0.3s ease;
        `;
        document.body.appendChild(indicator);

        // Monitor distance calculation frequency
        let calculationCount = 0;
        let lastCalculationTime = 0;

        const originalUpdateTouchData = this.updateTouchData.bind(this);
        this.updateTouchData = function (touches) {
            calculationCount++;
            lastCalculationTime = Date.now();

            // Show indicator
            indicator.style.opacity = '1';
            setTimeout(() => {
                indicator.style.opacity = '0';
            }, 200);

            // Log frequency
            if (calculationCount % 10 === 0) {
                console.log(`📊 Distance calculation frequency: ${calculationCount} calculations, last: ${new Date(lastCalculationTime).toLocaleTimeString()}`);
            }

            return originalUpdateTouchData(touches);
        };

        console.log('🔍 Distance monitoring activated');
    }

    // NEW: Test function to verify distance calculations
    testDistanceCalculations() {
        console.log(`🧪 TESTING DISTANCE CALCULATIONS`);

        // Create test touch data
        const testTouches = [
            { clientX: 100, clientY: 100, identifier: 1 },
            { clientX: 200, clientY: 100, identifier: 2 },
            { clientX: 150, clientY: 200, identifier: 3 }
        ];

        console.log(`📊 Test touches:`, testTouches);

        // Test the analyzeTouchPattern function
        const pattern = this.analyzeTouchPattern(testTouches);
        console.log(`📊 Test pattern result:`, pattern);

        // Test the getTokenSignature function
        const signature = this.getTokenSignature(testTouches, false);
        console.log(`📊 Test signature result:`, {
            touchCount: signature?.touchCount,
            hasTouchPattern: !!signature?.touchPattern,
            patternType: signature?.touchPattern?.type,
            distances: signature?.touchPattern?.distances,
            distanceSignature: signature?.touchPattern?.distanceSignature
        });

        // Verify the results
        if (signature && signature.touchPattern && signature.touchPattern.distances) {
            const hasValidDistances = signature.touchPattern.distances.every(d => d !== null && !isNaN(d));
            console.log(`✅ Distance calculation test: ${hasValidDistances ? 'PASSED' : 'FAILED'}`);

            if (hasValidDistances) {
                console.log(`📏 Valid distances:`, signature.touchPattern.distances);
                console.log(`📏 Distance signature:`, signature.touchPattern.distanceSignature);
            } else {
                console.log(`❌ Invalid distances found:`, signature.touchPattern.distances);
            }
        } else {
            console.log(`❌ Distance calculation test: FAILED - No valid signature generated`);
        }

        return signature;
    }





    // NEW: Main token identification function that compares distances between all touch points
    identifyToken(touches) {
        console.log(`🔍 IDENTIFY TOKEN: Processing ${touches.length} touches`);

        if (this.learnedTokens.length === 0) {
            console.log(`❌ No learned tokens available for identification`);
            return null;
        }

        // Get current signature using single source of truth
        const currentSignature = this.getTokenSignature(touches);
        if (!currentSignature) {
            console.log(`❌ Failed to generate signature for current touches`);
            return null;
        }

        console.log(`📊 Current signature:`, {
            touchCount: currentSignature.touchCount,
            hasTouchPattern: !!currentSignature.touchPattern,
            patternType: currentSignature.touchPattern?.type,
            distances: currentSignature.touchPattern?.distances
        });

        let bestMatch = null;
        let bestConfidence = 0;
        let matchDetails = [];

        // Compare with all stored tokens
        for (const token of this.learnedTokens) {
            if (!token.signature) {
                console.log(`⚠️ Skipping token "${token.name}" - invalid signature`);
                continue;
            }

            console.log(`🔍 Comparing with token: "${token.name}"`);

            // Check if touch counts match (must be exact)
            if (currentSignature.touchCount !== token.signature.touchCount) {
                console.log(`❌ Touch count mismatch: ${currentSignature.touchCount} vs ${token.signature.touchCount}`);
                continue;
            }

            // Compare token signatures
            const confidence = this.compareTokenSignatures(currentSignature, token.signature);
            console.log(`📊 Token "${token.name}" confidence: ${confidence}%`);

            // Store match details for debugging
            matchDetails.push({
                name: token.name,
                confidence: confidence,
                touchCount: token.signature.touchCount,
                distanceInfo: this.getDistanceComparisonInfo(currentSignature, token.signature)
            });

            if (confidence > bestConfidence && confidence >= this.settings.minConfidence) {
                bestConfidence = confidence;
                bestMatch = {
                    name: token.name,
                    confidence: confidence,
                    token: token,
                    matchDetails: matchDetails
                };

                console.log(`🎯 New best match: "${token.name}" with ${confidence}% confidence`);
            }
        }

        // Log all match details for debugging
        console.log(`📊 All token matches:`, matchDetails.sort((a, b) => b.confidence - a.confidence));

        if (bestMatch) {
            console.log(`✅ Best token identified: "${bestMatch.name}" with ${bestMatch.confidence}% confidence`);
            return bestMatch;
        } else {
            console.log(`❌ No token found above ${this.settings.minConfidence}% confidence threshold`);
            return null;
        }
    }

    // NEW: Get detailed distance comparison information
    getDistanceComparisonInfo(currentSignature, storedSignature) {
        if (!currentSignature.touchPattern || !storedSignature.touchPattern) {
            return 'No pattern data available';
        }

        const current = currentSignature.touchPattern;
        const stored = storedSignature.touchPattern;

        if (current.type !== stored.type) {
            return `Pattern type mismatch: ${current.type} vs ${stored.type}`;
        }

        if (current.type === 'single') {
            return 'Single touch - identical';
        }

        // Multi-touch comparison
        let info = `Multi-touch (${current.complexity} fingers)\n`;

        if (current.distances && stored.distances) {
            const minLength = Math.min(current.distances.length, stored.distances.length);
            info += `Comparing ${minLength} distances:\n`;

            for (let i = 0; i < minLength; i++) {
                const currentDist = current.distances[i];
                const storedDist = stored.distances[i];

                if (currentDist && storedDist) {
                    const diff = Math.abs(currentDist - storedDist);
                    const similarity = Math.max(0, 100 - (diff / Math.max(currentDist, storedDist) * 100));
                    info += `  Distance ${i + 1}: ${currentDist.toFixed(1)}px vs ${storedDist.toFixed(1)}px = ${diff.toFixed(1)}px diff (${similarity.toFixed(1)}% similar)\n`;
                } else {
                    info += `  Distance ${i + 1}: Invalid data\n`;
                }
            }
        }

        // Add overall distance similarity
        const distanceSimilarity = this.calculateDistanceSimilarity(current, stored);
        info += `Overall distance similarity: ${distanceSimilarity.toFixed(1)}%`;

        return info;
    }

    // NEW: Display real-time matching information
    showMatchingProcess(touches) {
        if (this.learnedTokens.length === 0) {
            this.showToast('No tokens available for matching', 'warning');
            return;
        }

        // Create a modal to show the matching process
        const modal = document.createElement('div');
        modal.className = 'modal';
        modal.style.display = 'block';
        modal.innerHTML = `
            <div class="modal-content" style="max-width: 800px;">
                <div class="modal-header">
                    <h2>🔍 Token Matching Process</h2>
                    <button class="close-btn" onclick="this.closest('.modal').remove()">&times;</button>
                </div>
                <div class="modal-body">
                    <div style="background: rgba(33, 150, 243, 0.1); border: 1px solid #2196F3; border-radius: 8px; padding: 15px; margin-bottom: 20px;">
                        <h3 style="color: #2196F3; margin-bottom: 10px;">📊 Current Touch Pattern</h3>
                        <div id="currentPatternInfo" style="font-family: monospace; font-size: 14px;">
                            Analyzing current touches...
                        </div>
                    </div>
                    
                    <div style="background: rgba(255, 152, 0, 0.1); border: 1px solid #ff9800; border-radius: 8px; padding: 15px;">
                        <h3 style="color: #ff9800; margin-bottom: 10px;">🔍 Matching Results</h3>
                        <div id="matchingResults" style="font-family: monospace; font-size: 14px;">
                            Starting token comparison...
                        </div>
                    </div>
                </div>
                <div class="modal-footer">
                    <button class="btn" onclick="this.closest('.modal').remove()">Close</button>
                </div>
            </div>
        `;

        document.body.appendChild(modal);

        // Start the matching process
        this.performMatchingAnalysis(touches, modal);
    }

    // NEW: Perform detailed matching analysis
    async performMatchingAnalysis(touches, modal) {
        try {
            // Get current signature
            const currentSignature = this.getTokenSignature(touches);
            if (!currentSignature) {
                document.getElementById('currentPatternInfo').innerHTML = '❌ Failed to generate signature';
                return;
            }

            // Display current pattern info
            const currentPatternInfo = this.generateCurrentPatternInfo(currentSignature);
            document.getElementById('currentPatternInfo').innerHTML = currentPatternInfo;

            // Perform matching analysis
            const matchingResults = await this.analyzeAllTokenMatches(currentSignature);
            document.getElementById('matchingResults').innerHTML = matchingResults;

        } catch (error) {
            console.error('Error in matching analysis:', error);
            document.getElementById('matchingResults').innerHTML = `❌ Error: ${error.message}`;
        }
    }

    // NEW: Generate current pattern information
    generateCurrentPatternInfo(signature) {
        if (!signature.touchPattern) {
            return '❌ No pattern data available';
        }

        const pattern = signature.touchPattern;
        let info = `🎯 Touch Count: ${signature.touchCount} fingers<br>`;
        info += `📏 Pattern Type: ${pattern.type}<br>`;

        if (pattern.type === 'multi' && pattern.distances) {
            info += `📊 Distances: ${pattern.distances.map(d => d ? `${d.toFixed(1)}px` : 'null').join(', ')}<br>`;
            info += `📏 Min Distance: ${pattern.minDistance ? pattern.minDistance.toFixed(1) + 'px' : 'N/A'}<br>`;
            info += `📏 Max Distance: ${pattern.maxDistance ? pattern.maxDistance.toFixed(1) + 'px' : 'N/A'}<br>`;
            info += `🏷️ Distance Signature: ${pattern.distanceSignature || 'N/A'}<br>`;
        }

        return info;
    }

    // NEW: Analyze all token matches with detailed information
    async analyzeAllTokenMatches(currentSignature) {
        const results = [];

        for (const token of this.learnedTokens) {
            if (!token.signature) continue;

            // Check touch count match
            if (currentSignature.touchCount !== token.signature.touchCount) {
                results.push({
                    name: token.name,
                    status: '❌ Touch count mismatch',
                    confidence: 0,
                    details: `Expected: ${token.signature.touchCount}, Got: ${currentSignature.touchCount}`
                });
                continue;
            }

            // Calculate confidence
            const confidence = this.compareTokenSignatures(currentSignature, token.signature);

            // Get detailed comparison info
            const comparisonInfo = this.getDistanceComparisonInfo(currentSignature, token.signature);

            results.push({
                name: token.name,
                status: confidence >= this.settings.minConfidence ? '✅ Match' : '⚠️ Below threshold',
                confidence: confidence,
                details: comparisonInfo
            });
        }

        // Sort by confidence
        results.sort((a, b) => b.confidence - a.confidence);

        // Generate HTML
        let html = '';
        results.forEach((result, index) => {
            const statusColor = result.status.includes('✅') ? '#4CAF50' :
                result.status.includes('❌') ? '#f44336' : '#ff9800';

            html += `
                <div style="border: 1px solid ${statusColor}; border-radius: 4px; padding: 10px; margin-bottom: 10px; background: rgba(255,255,255,0.05);">
                    <div style="font-weight: bold; color: ${statusColor}; margin-bottom: 5px;">
                        ${index + 1}. ${result.name} - ${result.status}
                    </div>
                    <div style="margin-bottom: 5px;">
                        <strong>Confidence:</strong> ${result.confidence.toFixed(1)}%
                    </div>
                    <div style="font-size: 12px; white-space: pre-line;">
                        ${result.details}
                    </div>
                </div>
            `;
        });

        return html || '<div style="color: #666;">No tokens available for comparison</div>';
    }

    // Map view functionality
    map = null;
    mapViewActive = false;
    mapTouchMarkers = [];

    // Initialize map view
    initMapView() {
        if (this.map) return; // Already initialized

        try {
            // Initialize Leaflet map with better default view
            this.map = L.map('map', {
                zoomControl: false, // We'll add custom zoom control
                attributionControl: true,
                maxZoom: 19,
                minZoom: 2,
                worldCopyJump: true,
                maxBounds: [[-90, -180], [90, 180]]
            }).setView([40.7128, -74.0060], 10); // Default to New York coordinates

            // 🗺️ ENHANCED: Add multiple tile layers for better appearance
            // Main OSM layer
            const osmLayer = L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
                attribution: '© OpenStreetMap contributors',
                maxZoom: 19,
                className: 'map-tile-layer'
            }).addTo(this.map);

            // Satellite layer option
            const satelliteLayer = L.tileLayer('https://server.arcgisonline.com/ArcGIS/rest/services/World_Imagery/MapServer/tile/{z}/{y}/{x}', {
                attribution: '© Esri',
                maxZoom: 19,
                className: 'map-tile-layer'
            });

            // Terrain layer option
            const terrainLayer = L.tileLayer('https://{s}.tile.opentopomap.org/{z}/{x}/{y}.png', {
                attribution: '© OpenTopoMap',
                maxZoom: 17,
                className: 'map-tile-layer'
            });

            // Store layers for switching
            this.mapLayers = {
                osm: osmLayer,
                satellite: satelliteLayer,
                terrain: terrainLayer
            };

            // 🗺️ NEW: Add custom zoom control
            this.addCustomMapControls();

            // 🗺️ NEW: Add elevation data layer
            this.addElevationLayer();

            // 🗺️ NEW: Add map style controls
            this.addMapStyleControls();

            // Add some default markers for demonstration
            this.addDefaultMapMarkers();

            // 🗺️ NEW: Restore pending map markers from storage
            this.restorePendingMapMarkers();

            // 🗺️ NEW: Also try to restore markers after a short delay to ensure everything is ready
            setTimeout(() => this.tryRestoreMapMarkers(), 500);

            console.log('🗺️ Enhanced map view initialized successfully');
        } catch (error) {
            console.error('❌ Error initializing map view:', error);
            this.showToast('Failed to initialize map view', 'error');
        }
    }

    // Add default markers to the map
    addDefaultMapMarkers() {
        if (!this.map) return;

        // No default markers - map starts clean
        console.log('🗺️ Map initialized with no default markers');
    }

    // 🗺️ NEW: Add custom map controls
    addCustomMapControls() {
        if (!this.map) return;

        // Custom zoom control
        const zoomControl = L.control.zoom({
            position: 'topright',
            zoomInTitle: 'Zoom In',
            zoomOutTitle: 'Zoom Out'
        }).addTo(this.map);

        // Fullscreen control (custom implementation)
        const fullscreenControl = L.control({
            position: 'topright'
        });

        fullscreenControl.onAdd = function () {
            const div = L.DomUtil.create('div', 'fullscreen-control');
            div.innerHTML = `
                <button class="fullscreen-toggle" title="Toggle Fullscreen">
                    <span>⛶</span>
                </button>
            `;

            div.querySelector('.fullscreen-toggle').addEventListener('click', () => {
                this.toggleMapFullscreen();
            });

            return div;
        }.bind(this);

        fullscreenControl.addTo(this.map);

        // Scale control
        const scaleControl = L.control.scale({
            position: 'bottomleft',
            maxWidth: 200,
            metric: true,
            imperial: true,
            updateWhenIdle: true
        }).addTo(this.map);

        // Coordinates display control
        const coordinatesControl = L.control({
            position: 'bottomright'
        });

        coordinatesControl.onAdd = function () {
            const div = L.DomUtil.create('div', 'coordinates-control');
            div.innerHTML = '<div class="coordinates-display">Click on map for coordinates</div>';
            return div;
        };

        coordinatesControl.addTo(this.map);

        // Update coordinates on map click
        this.map.on('click', (e) => {
            const coords = e.latlng;
            const coordsDiv = document.querySelector('.coordinates-display');
            if (coordsDiv) {
                coordsDiv.innerHTML = `
                    <strong>Coordinates:</strong><br>
                    Lat: ${coords.lat.toFixed(6)}<br>
                    Lng: ${coords.lng.toFixed(6)}<br>
                    Zoom: ${this.map.getZoom()}
                `;
            }
        });

        console.log('🗺️ Custom map controls added');
    }

    // 🗺️ NEW: Add elevation data layer for 3D-like effect
    addElevationLayer() {
        if (!this.map) return;

        try {
            // Create elevation overlay using OpenTopoMap data
            const elevationLayer = L.tileLayer('https://{s}.tile.opentopomap.org/{z}/{x}/{y}.png', {
                attribution: '© OpenTopoMap',
                maxZoom: 17,
                opacity: 0.3,
                className: 'elevation-layer'
            });

            // Store elevation layer reference
            this.elevationLayer = elevationLayer;

            // Add elevation toggle control
            const elevationControl = L.control({
                position: 'topright'
            });

            elevationControl.onAdd = function () {
                const div = L.DomUtil.create('div', 'elevation-control');
                div.innerHTML = `
                    <button class="elevation-toggle" title="Toggle Elevation View">
                        <span>🏔️</span> Elevation
                    </button>
                    <button class="elevation-3d" title="Create 3D Elevation Effect">
                        <span>🗻</span> 3D
                    </button>
                    <button class="elevation-profile" title="Create Elevation Profile">
                        <span>📊</span> Profile
                    </button>
                `;

                let elevationActive = false;
                div.querySelector('.elevation-toggle').addEventListener('click', () => {
                    if (elevationActive) {
                        elevationLayer.remove();
                        elevationActive = false;
                        div.querySelector('.elevation-toggle').innerHTML = '<span>🏔️</span> Elevation';
                    } else {
                        elevationLayer.addTo(this.map);
                        elevationActive = true;
                        div.querySelector('.elevation-toggle').innerHTML = '<span>🗺️</span> Hide Elevation';
                    }
                });

                div.querySelector('.elevation-3d').addEventListener('click', () => {
                    this.create3DElevationEffect();
                });

                div.querySelector('.elevation-profile').addEventListener('click', () => {
                    this.createRealisticElevationProfile(this.map.getCenter());
                });

                return div;
            }.bind(this);

            elevationControl.addTo(this.map);

            console.log('🗺️ Elevation layer added');
        } catch (error) {
            console.error('❌ Error adding elevation layer:', error);
        }
    }

    // 🗺️ NEW: Add map style controls
    addMapStyleControls() {
        if (!this.map) return;

        // Layer switcher control
        const layerControl = L.control.layers(
            {
                'OpenStreetMap': this.mapLayers.osm,
                'Satellite': this.mapLayers.satellite,
                'Terrain': this.mapLayers.terrain
            },
            {},
            {
                position: 'topright',
                collapsed: false
            }
        ).addTo(this.map);

        // Map style presets
        const styleControl = L.control({
            position: 'topright'
        });

        styleControl.onAdd = function () {
            const div = L.DomUtil.create('div', 'style-control');
            div.innerHTML = `
                <div class="style-presets">
                    <button class="style-btn active" data-style="default">Default</button>
                    <button class="style-btn" data-style="dark">Dark</button>
                    <button class="style-btn" data-style="vintage">Vintage</button>
                </div>
            `;

            // Add style switching functionality
            div.querySelectorAll('.style-btn').forEach(btn => {
                btn.addEventListener('click', (e) => {
                    // Remove active class from all buttons
                    div.querySelectorAll('.style-btn').forEach(b => b.classList.remove('active'));
                    // Add active class to clicked button
                    e.target.classList.add('active');

                    const style = e.target.dataset.style;
                    this.applyMapStyle(style);
                });
            });

            return div;
        }.bind(this);

        styleControl.addTo(this.map);

        // 🗺️ NEW: Add map positioning and resize controls
        this.addMapPositioningControls();

        // 🗺️ NEW: Add zoom level controls
        this.addZoomLevelControls();

        // 🗺️ NEW: Add zoom history
        this.addZoomHistory();

        console.log('🗺️ Map style controls added');
    }

    // 🗺️ NEW: Add map positioning and resize controls
    addMapPositioningControls() {
        if (!this.map) return;

        // Map positioning control
        const positionControl = L.control({
            position: 'topleft'
        });

        positionControl.onAdd = function () {
            const div = L.DomUtil.create('div', 'position-control');
            div.innerHTML = `
                <div class="position-buttons">
                    <button class="pos-btn" data-position="north" title="North View">⬆️</button>
                    <button class="pos-btn" data-position="south" title="South View">⬇️</button>
                    <button class="pos-btn" data-position="east" title="East View">➡️</button>
                    <button class="pos-btn" data-position="west" title="West View">⬅️</button>
                </div>
                <div class="position-buttons">
                    <button class="pos-btn" data-position="northeast" title="Northeast View">↗️</button>
                    <button class="pos-btn" data-position="northwest" title="Northwest View">↖️</button>
                    <button class="pos-btn" data-position="southeast" title="Southeast View">↘️</button>
                    <button class="pos-btn" data-position="southwest" title="Southwest View">↙️</button>
                </div>
                <div class="position-buttons">
                    <button class="pos-btn" data-position="center" title="Center View">🎯</button>
                    <button class="pos-btn" data-position="home" title="Home Position">🏠</button>
                </div>
            `;

            // Add positioning functionality
            div.querySelectorAll('.pos-btn').forEach(btn => {
                btn.addEventListener('click', (e) => {
                    const position = e.target.dataset.position;
                    this.moveMapToPosition(position);
                });
            });

            return div;
        }.bind(this);

        positionControl.addTo(this.map);

        // Map resize control
        const resizeControl = L.control({
            position: 'bottomright'
        });

        resizeControl.onAdd = function () {
            const div = L.DomUtil.create('div', 'resize-control');
            div.innerHTML = `
                <div class="resize-buttons">
                    <button class="resize-btn" data-size="small" title="Small View">📱</button>
                    <button class="resize-btn" data-size="medium" title="Medium View">💻</button>
                    <button class="resize-btn" data-size="large" title="Large View">🖥️</button>
                    <button class="resize-btn" data-size="fullscreen" title="Fullscreen">⛶</button>
                </div>
                <div class="resize-buttons">
                    <button class="resize-btn" data-size="custom" title="Custom Size">⚙️</button>
                    <button class="resize-btn" data-size="reset" title="Reset Size">🔄</button>
                </div>
            `;

            // Add resize functionality
            div.querySelectorAll('.resize-btn').forEach(btn => {
                btn.addEventListener('click', (e) => {
                    const size = e.target.dataset.size;
                    this.resizeMapView(size);
                });
            });

            return div;
        }.bind(this);

        resizeControl.addTo(this.map);

        console.log('🗺️ Map positioning and resize controls added');
    }

    // 🗺️ NEW: Add zoom level controls
    addZoomLevelControls() {
        if (!this.map) return;

        // Zoom level control
        const zoomControl = L.control({
            position: 'bottomleft'
        });

        zoomControl.onAdd = function () {
            const div = L.DomUtil.create('div', 'zoom-level-control');
            div.innerHTML = `
                <div class="zoom-header">
                    <span>🔍 Zoom Levels</span>
                </div>
                <div class="zoom-buttons">
                    <button class="zoom-btn" data-zoom="2" title="World View">🌍</button>
                    <button class="zoom-btn" data-zoom="5" title="Continent View">🗺️</button>
                    <button class="zoom-btn" data-zoom="8" title="Country View">🏛️</button>
                    <button class="zoom-btn" data-zoom="10" title="City View">🏙️</button>
                    <button class="zoom-btn" data-zoom="13" title="District View">🏘️</button>
                    <button class="zoom-btn" data-zoom="16" title="Street View">🛣️</button>
                    <button class="zoom-btn" data-zoom="19" title="Building View">🏢</button>
                </div>
                <div class="zoom-controls">
                    <button class="zoom-minus-btn" id="zoomMinusBtn" title="Zoom Out">➖</button>
                    <span class="zoom-display" id="currentZoomInfo">10</span>
                    <button class="zoom-plus-btn" id="zoomPlusBtn" title="Zoom In">➕</button>
                </div>
            `;

            // Add zoom functionality
            div.querySelectorAll('.zoom-btn').forEach(btn => {
                btn.addEventListener('click', (e) => {
                    const zoom = parseInt(e.target.dataset.zoom);
                    this.zoomToLevel(zoom);
                });
            });

            // Add plus/minus zoom functionality
            const minusBtn = div.querySelector('#zoomMinusBtn');
            const plusBtn = div.querySelector('#zoomPlusBtn');

            minusBtn.addEventListener('click', () => {
                const currentZoom = Math.round(this.map.getZoom());
                const newZoom = Math.max(1, currentZoom - 1);
                this.zoomToLevel(newZoom);
            });

            plusBtn.addEventListener('click', () => {
                const currentZoom = Math.round(this.map.getZoom());
                const newZoom = Math.min(19, currentZoom + 1);
                this.zoomToLevel(newZoom);
            });

            // Update zoom info on map zoom events
            this.map.on('zoomend', () => {
                const currentZoom = Math.round(this.map.getZoom());
                const zoomInfo = div.querySelector('#currentZoomInfo');
                if (zoomInfo) {
                    zoomInfo.textContent = currentZoom;
                }

                // Update active zoom button
                div.querySelectorAll('.zoom-btn').forEach(btn => {
                    btn.classList.remove('active');
                    if (parseInt(btn.dataset.zoom) === currentZoom) {
                        btn.classList.add('active');
                    }
                });
            });

            return div;
        }.bind(this);

        zoomControl.addTo(this.map);

        console.log('🗺️ Zoom level controls added');
    }

    // 🗺️ NEW: Zoom to specific level
    zoomToLevel(zoomLevel) {
        if (!this.map) return;

        // Ensure zoom level is an integer
        const roundedZoom = Math.round(zoomLevel);

        // Validate zoom level
        if (roundedZoom < 1 || roundedZoom > 19) {
            this.showToast('Invalid zoom level. Must be between 1-19', 'warning');
            return;
        }

        const currentZoom = Math.round(this.map.getZoom());
        const currentCenter = this.map.getCenter();

        // Smooth zoom animation to integer level
        this.map.flyTo(currentCenter, roundedZoom, {
            duration: 1.0,
            easeLinearity: 0.25
        });

        // Show zoom level description
        const zoomDescriptions = {
            1: 'World View',
            2: 'World View',
            3: 'World View',
            4: 'Continent View',
            5: 'Continent View',
            6: 'Country View',
            7: 'Country View',
            8: 'Country View',
            9: 'Region View',
            10: 'City View',
            11: 'City View',
            12: 'District View',
            13: 'District View',
            14: 'Neighborhood View',
            15: 'Street View',
            16: 'Street View',
            17: 'Building View',
            18: 'Building View',
            19: 'Building View'
        };

        const description = zoomDescriptions[zoomLevel] || 'Custom View';
        this.showToast(`Zoomed to ${description} (Level ${zoomLevel})`, 'info');

        console.log(`🗺️ Zoomed to level ${zoomLevel}: ${description}`);
    }

    // 🗺️ NEW: Add zoom history tracking
    addZoomHistory() {
        if (!this.map) return;

        // Initialize zoom history
        this.zoomHistory = [];
        this.maxZoomHistory = 5;

        // Track zoom changes
        this.map.on('zoomend', () => {
            const currentZoom = this.map.getZoom();
            const currentCenter = this.map.getCenter();

            // Add to history (avoid duplicates)
            const lastEntry = this.zoomHistory[this.zoomHistory.length - 1];
            if (!lastEntry || lastEntry.zoom !== currentZoom ||
                Math.abs(lastEntry.center.lat - currentCenter.lat) > 0.001 ||
                Math.abs(lastEntry.center.lng - currentCenter.lng) > 0.001) {

                this.zoomHistory.push({
                    zoom: currentZoom,
                    center: currentCenter,
                    timestamp: new Date()
                });

                // Keep only last N entries
                if (this.zoomHistory.length > this.maxZoomHistory) {
                    this.zoomHistory.shift();
                }
            }
        });

        // Add zoom history control
        const historyControl = L.control({
            position: 'bottomleft'
        });

        historyControl.onAdd = function () {
            const div = L.DomUtil.create('div', 'zoom-history-control');
            div.innerHTML = `
                <div class="history-header">
                    <span>⏱️ Recent Views</span>
                </div>
                <div class="history-list" id="zoomHistoryList">
                    <div class="history-placeholder">No recent views</div>
                </div>
            `;

            // Update history display
            this.updateZoomHistoryDisplay();

            return div;
        }.bind(this);

        historyControl.addTo(this.map);

        console.log('🗺️ Zoom history tracking added');
    }

    // 🗺️ NEW: Update zoom history display
    updateZoomHistoryDisplay() {
        const historyList = document.getElementById('zoomHistoryList');
        if (!historyList) return;

        if (this.zoomHistory.length === 0) {
            historyList.innerHTML = '<div class="history-placeholder">No recent views</div>';
            return;
        }

        const historyHTML = this.zoomHistory.slice().reverse().map((entry, index) => {
            const timeAgo = this.getTimeAgo(entry.timestamp);
            return `
                <button class="history-item" onclick="this.returnToZoomHistory(${index})" title="Return to this view">
                    <span class="history-zoom">🔍 ${entry.zoom}</span>
                    <span class="history-time">${timeAgo}</span>
                </button>
            `;
        }).join('');

        historyList.innerHTML = historyHTML;
    }

    // 🗺️ NEW: Return to zoom history entry
    returnToZoomHistory(index) {
        if (!this.map || !this.zoomHistory[index]) return;

        const entry = this.zoomHistory[index];
        this.map.flyTo(entry.center, entry.zoom, {
            duration: 1.5,
            easeLinearity: 0.25
        });

        this.showToast(`Returned to previous view (Zoom: ${entry.zoom})`, 'info');
        console.log(`🗺️ Returned to zoom history entry: ${entry.zoom}`);
    }

    // 🗺️ NEW: Get time ago string
    getTimeAgo(timestamp) {
        const now = new Date();
        const diff = now - timestamp;
        const minutes = Math.floor(diff / 60000);
        const seconds = Math.floor(diff / 1000);

        if (minutes > 0) {
            return `${minutes}m ago`;
        } else {
            return `${seconds}s ago`;
        }
    }

    // 🗺️ NEW: Move map to specific position
    moveMapToPosition(position) {
        if (!this.map) return;

        const currentCenter = this.map.getCenter();
        const currentZoom = this.map.getZoom();
        const offset = 0.01; // Small offset for directional movement

        let newLat, newLng, newZoom;

        switch (position) {
            case 'north':
                newLat = currentCenter.lat + offset;
                newLng = currentCenter.lng;
                newZoom = currentZoom;
                break;
            case 'south':
                newLat = currentCenter.lat - offset;
                newLng = currentCenter.lng;
                newZoom = currentZoom;
                break;
            case 'east':
                newLat = currentCenter.lat;
                newLng = currentCenter.lng + offset;
                newZoom = currentZoom;
                break;
            case 'west':
                newLat = currentCenter.lat;
                newLng = currentCenter.lng - offset;
                newZoom = currentZoom;
                break;
            case 'northeast':
                newLat = currentCenter.lat + offset;
                newLng = currentCenter.lng + offset;
                newZoom = currentZoom;
                break;
            case 'northwest':
                newLat = currentCenter.lat + offset;
                newLng = currentCenter.lng - offset;
                newZoom = currentZoom;
                break;
            case 'southeast':
                newLat = currentCenter.lat - offset;
                newLng = currentCenter.lng + offset;
                newZoom = currentZoom;
                break;
            case 'southwest':
                newLat = currentCenter.lat - offset;
                newLng = currentCenter.lng - offset;
                newZoom = currentZoom;
                break;
            case 'center':
                // Return to center of current view
                newLat = currentCenter.lat;
                newLng = currentCenter.lng;
                newZoom = currentZoom;
                break;
            case 'home':
                // Return to default home position (New York)
                newLat = 40.7128;
                newLng = -74.0060;
                newZoom = 10;
                break;
            default:
                return;
        }

        // Smoothly animate to new position
        this.map.flyTo([newLat, newLng], newZoom, {
            duration: 1.5,
            easeLinearity: 0.25
        });

        this.showToast(`Map moved to ${position} position`, 'info');
        console.log(`🗺️ Map moved to ${position} position: ${newLat}, ${newLng}`);
    }

    // 🗺️ NEW: Resize map view
    resizeMapView(size) {
        if (!this.map) return;

        const mapContainer = document.getElementById('mapContainer');
        const mapElement = document.getElementById('map');

        // Store original dimensions for reset
        if (!this.originalMapDimensions) {
            this.originalMapDimensions = {
                width: mapContainer.style.width,
                height: mapContainer.style.height
            };
        }

        let newWidth, newHeight;

        switch (size) {
            case 'small':
                newWidth = '60%';
                newHeight = '60%';
                break;
            case 'medium':
                newWidth = '80%';
                newHeight = '80%';
                break;
            case 'large':
                newWidth = '95%';
                newHeight = '95%';
                break;
            case 'fullscreen':
                this.toggleMapFullscreen();
                return;
            case 'custom':
                this.showCustomSizeDialog();
                return;
            case 'reset':
                newWidth = this.originalMapDimensions.width || '100%';
                newHeight = this.originalMapDimensions.height || '100%';
                break;
            default:
                return;
        }

        // Apply new dimensions
        mapContainer.style.width = newWidth;
        mapContainer.style.height = newHeight;

        // Trigger map resize event
        setTimeout(() => {
            this.map.invalidateSize();
        }, 100);

        this.showToast(`Map resized to ${size} view`, 'info');
        console.log(`🗺️ Map resized to ${size} view: ${newWidth} x ${newHeight}`);
    }

    // 🗺️ NEW: Show custom size dialog
    showCustomSizeDialog() {
        const modal = document.createElement('div');
        modal.className = 'modal';
        modal.style.display = 'block';
        modal.innerHTML = `
            <div class="modal-content" style="max-width: 400px;">
                <div class="modal-header">
                    <h2>🗺️ Custom Map Size</h2>
                    <button class="close-btn" onclick="this.closest('.modal').remove()">&times;</button>
                </div>
                <div class="modal-body">
                    <div class="form-group">
                        <label>Width:</label>
                        <input type="text" id="customWidth" placeholder="e.g., 800px or 80%" value="80%">
                    </div>
                    <div class="form-group">
                        <label>Height:</label>
                        <input type="text" id="customHeight" placeholder="e.g., 600px or 70%" value="70%">
                    </div>
                    <div class="form-group">
                        <label>Preset Sizes:</label>
                        <div class="preset-sizes">
                            <button class="preset-btn" data-width="50%" data-height="50%">Small</button>
                            <button class="preset-btn" data-width="75%" data-height="75%">Medium</button>
                            <button class="preset-btn" data-width="90%" data-height="90%">Large</button>
                        </div>
                    </div>
                </div>
                <div class="modal-footer">
                    <button class="btn" onclick="this.closest('.modal').remove()">Cancel</button>
                    <button class="btn success" onclick="applyCustomSize()">Apply</button>
                </div>
            </div>
        `;

        document.body.appendChild(modal);

        // Add preset button functionality
        modal.querySelectorAll('.preset-btn').forEach(btn => {
            btn.addEventListener('click', (e) => {
                const width = e.target.dataset.width;
                const height = e.target.dataset.height;
                document.getElementById('customWidth').value = width;
                document.getElementById('customHeight').value = height;
            });
        });

        // Add apply function
        window.applyCustomSize = () => {
            const width = document.getElementById('customWidth').value;
            const height = document.getElementById('customHeight').value;

            if (width && height) {
                this.applyCustomMapSize(width, height);
                modal.remove();
            } else {
                this.showToast('Please enter valid dimensions', 'warning');
            }
        };
    }

    // 🗺️ NEW: Apply custom map size
    applyCustomMapSize(width, height) {
        if (!this.map) return;

        const mapContainer = document.getElementById('mapContainer');

        // Apply custom dimensions
        mapContainer.style.width = width;
        mapContainer.style.height = height;

        // Trigger map resize
        setTimeout(() => {
            this.map.invalidateSize();
        }, 100);

        this.showToast(`Map resized to ${width} x ${height}`, 'success');
        console.log(`🗺️ Map resized to custom size: ${width} x ${height}`);
    }

    // 🗺️ NEW: Apply different map styles
    applyMapStyle(style) {
        if (!this.map) return;

        const mapContainer = this.map.getContainer();

        // Remove existing style classes
        mapContainer.classList.remove('map-style-default', 'map-style-dark', 'map-style-vintage');

        // Apply new style
        switch (style) {
            case 'dark':
                mapContainer.classList.add('map-style-dark');
                break;
            case 'vintage':
                mapContainer.classList.add('map-style-vintage');
                break;
            default:
                mapContainer.classList.add('map-style-default');
                break;
        }

        console.log(`🗺️ Applied map style: ${style}`);
    }

    // 🗺️ NEW: Show map controls information
    showMapControlsInfo() {
        const modal = document.createElement('div');
        modal.className = 'modal';
        modal.style.display = 'block';
        modal.innerHTML = `
            <div class="modal-content" style="max-width: 600px;">
                <div class="modal-header">
                    <h2>🗺️ Map Controls Guide</h2>
                    <button class="close-btn" onclick="this.closest('.modal').remove()">&times;</button>
                </div>
                <div class="modal-body">
                    <div class="controls-section">
                        <h3>🎯 Positioning Controls (Top-Left)</h3>
                        <div class="control-grid">
                            <div class="control-item">
                                <span class="control-icon">⬆️</span>
                                <span class="control-desc">North View</span>
                            </div>
                            <div class="control-item">
                                <span class="control-icon">⬇️</span>
                                <span class="control-desc">South View</span>
                            </div>
                            <div class="control-item">
                                <span class="control-icon">➡️</span>
                                <span class="control-desc">East View</span>
                            </div>
                            <div class="control-item">
                                <span class="control-icon">⬅️</span>
                                <span class="control-desc">West View</span>
                            </div>
                        </div>
                        <div class="control-grid">
                            <div class="control-item">
                                <span class="control-icon">↗️</span>
                                <span class="control-desc">Northeast</span>
                            </div>
                            <div class="control-item">
                                <span class="control-icon">↖️</span>
                                <span class="control-desc">Northwest</span>
                            </div>
                            <div class="control-item">
                                <span class="control-icon">↘️</span>
                                <span class="control-desc">Southeast</span>
                            </div>
                            <div class="control-item">
                                <span class="control-icon">↙️</span>
                                <span class="control-desc">Southwest</span>
                            </div>
                        </div>
                        <div class="control-grid">
                            <div class="control-item">
                                <span class="control-icon">🎯</span>
                                <span class="control-desc">Center View</span>
                            </div>
                            <div class="control-item">
                                <span class="control-icon">🏠</span>
                                <span class="control-desc">Home Position</span>
                            </div>
                        </div>
                    </div>
                    
                    <div class="controls-section">
                        <h3>📏 Resize Controls (Bottom-Right)</h3>
                        <div class="control-grid">
                            <div class="control-item">
                                <span class="control-icon">📱</span>
                                <span class="control-desc">Small View</span>
                            </div>
                            <div class="control-item">
                                <span class="control-icon">💻</span>
                                <span class="control-desc">Medium View</span>
                            </div>
                            <div class="control-item">
                                <span class="control-icon">🖥️</span>
                                <span class="control-desc">Large View</span>
                            </div>
                            <div class="control-item">
                                <span class="control-icon">⛶</span>
                                <span class="control-desc">Fullscreen</span>
                                <span class="control-desc">Elevation Profile</span>
                            </div>
                        </div>
                        <div class="control-grid">
                            <div class="control-item">
                                <span class="control-icon">⚙️</span>
                                <span class="control-desc">Custom Size</span>
                            </div>
                            <div class="control-item">
                                <span class="control-icon">🔄</span>
                                <span class="control-desc">Reset Size</span>
                            </div>
                        </div>
                    </div>
                    
                    <div class="controls-section">
                        <h3>🎨 Style Controls (Top-Right)</h3>
                        <div class="control-grid">
                            <div class="control-item">
                                <span class="control-icon">🏔️</span>
                                <span class="control-desc">Elevation Overlay</span>
                            </div>
                            <div class="control-item">
                                <span class="control-icon">🗻</span>
                                <span class="control-desc">3D Effect</span>
                            </div>
                            <div class="control-item">
                                <span class="control-icon">📊</span>
                                <span class="control-desc">Elevation Profile</span>
                            </div>
                        </div>
                    </div>
                </div>
                <div class="modal-footer">
                    <button class="btn" onclick="this.closest('.modal').remove()">Close</button>
                </div>
            </div>
        `;

        document.body.appendChild(modal);
    }

    // 🗺️ NEW: Create 3D-like elevation effect
    create3DElevationEffect() {
        if (!this.map) return;

        try {
            // Get current map bounds
            const bounds = this.map.getBounds();
            const center = this.map.getCenter();

            // Create elevation contour lines (simulated for demo)
            this.createElevationContours(bounds, center);

            // Add elevation data points
            this.addElevationDataPoints(center);

            // Add 3D perspective effect
            this.add3DPerspectiveEffect();

            console.log('🗺️ 3D elevation effect created');
        } catch (error) {
            console.error('❌ Error creating 3D elevation effect:', error);
        }
    }

    // 🗺️ NEW: Create elevation contour lines
    createElevationContours(bounds, center) {
        // Simulate elevation data with contour lines
        const contourData = [
            { lat: center.lat - 0.01, lng: center.lng - 0.01, elevation: 100 },
            { lat: center.lat + 0.01, lng: center.lng - 0.01, elevation: 150 },
            { lat: center.lat + 0.01, lng: center.lng + 0.01, elevation: 200 },
            { lat: center.lat - 0.01, lng: center.lng + 0.01, elevation: 120 }
        ];

        // Create contour lines
        contourData.forEach((point, index) => {
            const nextPoint = contourData[(index + 1) % contourData.length];

            const contourLine = L.polyline([
                [point.lat, point.lng],
                [nextPoint.lat, nextPoint.lng]
            ], {
                color: this.getElevationColor(point.elevation),
                weight: 2,
                opacity: 0.8,
                dashArray: '5, 5'
            }).addTo(this.map);

            // Add elevation label
            const elevationLabel = L.marker([point.lat, point.lng], {
                icon: L.divIcon({
                    className: 'elevation-label',
                    html: `<div class="elevation-value">${point.elevation}m</div>`,
                    iconSize: [40, 20],
                    iconAnchor: [20, 10]
                })
            }).addTo(this.map);
        });
    }

    // 🗺️ NEW: Get elevation color based on height
    getElevationColor(elevation) {
        if (elevation < 100) return '#8BC34A'; // Green for low elevation
        if (elevation < 200) return '#FFC107'; // Yellow for medium elevation
        if (elevation < 300) return '#FF9800'; // Orange for high elevation
        return '#F44336'; // Red for very high elevation
    }

    // 🗺️ NEW: Add elevation data points
    addElevationDataPoints(center) {
        // Create a grid of elevation points
        for (let i = -2; i <= 2; i++) {
            for (let j = -2; j <= 2; j++) {
                const lat = center.lat + (i * 0.005);
                const lng = center.lng + (j * 0.005);
                const elevation = Math.floor(Math.random() * 300) + 50; // Random elevation 50-350m

                const elevationPoint = L.circleMarker([lat, lng], {
                    radius: 6,
                    fillColor: this.getElevationColor(elevation),
                    color: 'white',
                    weight: 2,
                    opacity: 1,
                    fillOpacity: 0.8
                }).addTo(this.map);

                // Add popup with elevation info
                elevationPoint.bindPopup(`
                    <div class="elevation-popup">
                        <strong>Elevation Point</strong><br>
                        Height: ${elevation}m<br>
                        Coordinates: ${lat.toFixed(6)}, ${lng.toFixed(6)}
                    </div>
                `);
            }
        }
    }

    // 🗺️ NEW: Add 3D perspective effect
    add3DPerspectiveEffect() {
        const mapContainer = this.map.getContainer();

        // Add CSS transform for 3D effect
        mapContainer.style.transform = 'perspective(1000px) rotateX(5deg)';
        mapContainer.style.transformOrigin = 'center center';
        mapContainer.style.transition = 'transform 0.3s ease';

        // Add hover effect for more 3D feel
        mapContainer.addEventListener('mouseenter', () => {
            mapContainer.style.transform = 'perspective(1000px) rotateX(8deg) scale(1.02)';
        });

        mapContainer.addEventListener('mouseleave', () => {
            mapContainer.style.transform = 'perspective(1000px) rotateX(5deg)';
        });
    }

    // 🗺️ NEW: Get real elevation data from OpenTopoMap
    async getRealElevationData(lat, lng) {
        try {
            // Use OpenTopoMap elevation API (simulated for demo)
            // In a real implementation, you would use a proper elevation API
            const response = await fetch(`https://api.opentopodata.org/v1/aster30m?locations=${lat},${lng}`);
            const data = await response.json();

            if (data.results && data.results[0]) {
                return data.results[0].elevation;
            }
        } catch (error) {
            console.log('Using simulated elevation data');
        }

        // Fallback to simulated elevation based on coordinates
        return Math.floor(Math.abs(lat * 1000 + lng * 1000) % 300) + 50;
    }

    // 🗺️ NEW: Create realistic elevation profile
    async createRealisticElevationProfile(center) {
        const profileData = [];

        // Create a line of elevation points
        for (let i = -5; i <= 5; i++) {
            const lat = center.lat + (i * 0.002);
            const lng = center.lng;

            const elevation = await this.getRealElevationData(lat, lng);
            profileData.push({ lat, lng, elevation });
        }

        // Create elevation profile line
        const profileLine = L.polyline(
            profileData.map(p => [p.lat, p.lng]),
            {
                color: '#FF5722',
                weight: 3,
                opacity: 0.8,
                dashArray: '10, 5'
            }
        ).addTo(this.map);

        // Add elevation markers along the profile
        profileData.forEach(point => {
            const marker = L.circleMarker([point.lat, point.lng], {
                radius: 4,
                fillColor: this.getElevationColor(point.elevation),
                color: 'white',
                weight: 1
            }).addTo(this.map);

            marker.bindPopup(`Elevation: ${point.elevation}m`);
        });

        console.log('🗺️ Realistic elevation profile created');
    }

    // Toggle between canvas and map view
    toggleMapView() {
        const canvas = document.getElementById('touchCanvas');
        const mapContainer = document.getElementById('mapContainer');
        const toggleBtn = document.getElementById('toggleMapViewBtn');

        if (this.mapViewActive) {
            // Switch back to canvas view
            canvas.style.display = 'block';
            mapContainer.style.display = 'none';
            toggleBtn.textContent = '🗺️ Map View';
            toggleBtn.style.background = 'linear-gradient(135deg, #4CAF50, #45a049)';
            this.mapViewActive = false;

            // Clear map touch markers
            this.clearMapTouchMarkers();

            console.log('🔄 Switched to canvas view');
        } else {
            // Switch to map view
            canvas.style.display = 'none';
            mapContainer.style.display = 'block';
            toggleBtn.textContent = '🎨 Canvas View';
            toggleBtn.style.background = 'linear-gradient(135deg, #2196F3, #1976D2)';
            this.mapViewActive = true;

            // Initialize map if not already done
            if (!this.map) {
                this.initMapView();
            }

            console.log('🗺️ Switched to map view');
        }

        // Update touch event handling
        this.updateTouchEventHandling();
    }

    // Clear map touch markers
    clearMapTouchMarkers() {
        this.mapTouchMarkers.forEach(marker => {
            if (this.map && marker) {
                this.map.removeLayer(marker);
            }
        });
        this.mapTouchMarkers = [];
    }

    // Update touch event handling based on current view
    updateTouchEventHandling() {
        if (this.mapViewActive) {
            // Enable map touch events
            this.enableMapTouchEvents();
        } else {
            // Enable canvas touch events
            this.enableCanvasTouchEvents();
        }
    }

    // Enable map touch events
    enableMapTouchEvents() {
        if (!this.map) return;

        // Disable map's default touch behavior temporarily
        this.map.dragging.disable();
        this.map.touchZoom.disable();
        this.map.doubleClickZoom.disable();
        this.map.scrollWheelZoom.disable();

        // Add custom touch event listeners to map container
        const mapElement = document.getElementById('map');
        mapElement.addEventListener('touchstart', (e) => this.handleMapTouch(e), { passive: false });
        mapElement.addEventListener('touchmove', (e) => this.handleMapTouch(e), { passive: false });
        mapElement.addEventListener('touchend', (e) => this.handleMapTouch(e), { passive: false });

        console.log('👆 Map touch events enabled');
    }

    // Enable canvas touch events
    enableCanvasTouchEvents() {
        if (!this.map) return;

        // Re-enable map's default behavior
        this.map.dragging.enable();
        this.map.touchZoom.enable();
        this.map.doubleClickZoom.enable();
        this.map.scrollWheelZoom.enable();

        // Remove custom touch event listeners
        const mapElement = document.getElementById('map');
        mapElement.removeEventListener('touchstart', this.handleMapTouch);
        mapElement.removeEventListener('touchmove', this.handleMapTouch);
        mapElement.removeEventListener('touchend', this.handleMapTouch);

        console.log('🎨 Canvas touch events enabled');
    }

    // Handle touch events on the map
    handleMapTouch(e) {
        e.preventDefault();
        e.stopPropagation();

        const touches = Array.from(e.touches);
        const type = e.type;

        console.log(`🗺️ Map touch event: ${type} with ${touches.length} touches`);

        // Convert touch coordinates to map coordinates
        const mapTouches = touches.map(touch => {
            const rect = e.target.getBoundingClientRect();
            const x = touch.clientX - rect.left;
            const y = touch.clientY - rect.top;

            // Convert to map coordinates
            const latLng = this.map.containerPointToLatLng([x, y]);

            return {
                identifier: touch.identifier,
                clientX: x,
                clientY: y,
                mapLat: latLng.lat,
                mapLng: latLng.lng,
                force: touch.force,
                radiusX: touch.radiusX,
                radiusY: touch.radiusY,
                rotationAngle: touch.rotationAngle
            };
        });

        // Process the touch event
        this.processMapTouchEvent(type, mapTouches);
    }

    // Process map touch events
    processMapTouchEvent(type, touches) {
        console.log(`🗺️ Processing map touch: ${type}`, touches);

        switch (type) {
            case 'touchstart':
                this.handleMapTouchStart(touches);
                break;
            case 'touchmove':
                this.handleMapTouchMove(touches);
                break;
            case 'touchend':
                this.handleMapTouchEnd(touches);
                break;
        }

        // Update touch data display
        this.updateMapTouchDisplay(touches);
    }

    // Handle map touch start
    handleMapTouchStart(touches) {
        // Clear previous markers
        this.clearMapTouchMarkers();

        // 🎯 NEW: Store the initial touch data for later processing
        this.currentMapTouchData = touches.map(touch => ({
            identifier: touch.identifier,
            clientX: touch.clientX,
            clientY: touch.clientY,
            mapLat: touch.mapLat,
            mapLng: touch.mapLng,
            force: touch.force,
            radiusX: touch.radiusX,
            radiusY: touch.radiusY,
            rotationAngle: touch.rotationAngle,
            startTime: Date.now()
        }));

        console.log('🗺️ Stored initial map touch data:', this.currentMapTouchData);

        // Add markers for each touch point
        touches.forEach(touch => {
            const marker = L.marker([touch.mapLat, touch.mapLng], {
                icon: L.divIcon({
                    className: 'map-touch-marker',
                    html: `<div style="background: #ff5722; width: 20px; height: 20px; border-radius: 50%; border: 3px solid white; box-shadow: 0 2px 8px rgba(0,0,0,0.3);"></div>`,
                    iconSize: [20, 20],
                    iconAnchor: [10, 10]
                })
            }).addTo(this.map);

            this.mapTouchMarkers.push(marker);
        });

        // Start touch averaging for map touches
        this.startTouchAveraging(touches);
    }

    // 🎯 NEW: Clear stored map touch data
    clearMapTouchData() {
        this.currentMapTouchData = null;
        console.log('🗺️ Cleared stored map touch data');
    }

    // Handle map touch move
    handleMapTouchMove(touches) {
        // Update marker positions
        touches.forEach((touch, index) => {
            if (this.mapTouchMarkers[index]) {
                this.mapTouchMarkers[index].setLatLng([touch.mapLat, touch.mapLng]);
            }
        });

        // 🎯 NEW: Update stored touch data with current positions
        if (this.currentMapTouchData && touches.length === this.currentMapTouchData.length) {
            touches.forEach((touch, index) => {
                if (this.currentMapTouchData[index]) {
                    this.currentMapTouchData[index].mapLat = touch.mapLat;
                    this.currentMapTouchData[index].mapLng = touch.mapLng;
                    this.currentMapTouchData[index].clientX = touch.clientX;
                    this.currentMapTouchData[index].clientY = touch.clientY;
                }
            });
            console.log('🗺️ Updated stored map touch data with current positions');
        }

        // Update map touch display
        this.updateMapTouchDisplay(touches);
    }

    // Handle map touch end
    handleMapTouchEnd(touches) {
        console.log('🗺️ Map touch end called with touches:', touches);
        console.log('🗺️ Stored map touch data:', this.currentMapTouchData);

        // 🎯 FIXED: Use stored touch data instead of empty touches array
        if (this.currentMapTouchData && this.currentMapTouchData.length >= 2) {
            console.log('🗺️ Map touch pattern completed with stored data:', this.currentMapTouchData);

            // 🎯 NEW: Automatically identify token at touch location
            const avgLat = this.currentMapTouchData.reduce((sum, t) => sum + t.mapLat, 0) / this.currentMapTouchData.length;
            const avgLng = this.currentMapTouchData.reduce((sum, t) => sum + t.mapLng, 0) / this.currentMapTouchData.length;

            console.log('🗺️ Average location for token placement:', { lat: avgLat, lng: avgLng });

            // Convert stored map touches to canvas format for token identification
            const canvasTouches = this.currentMapTouchData.map(touch => ({
                clientX: touch.clientX,
                clientY: touch.clientY,
                identifier: touch.identifier,
                mapLat: touch.mapLat,
                mapLng: touch.mapLng
            }));

            console.log('🗺️ Converted touches for token identification:', canvasTouches);

            // 🎯 IMPROVED: Ensure touches have valid coordinates for token identification
            const validTouches = canvasTouches.filter(touch =>
                typeof touch.clientX === 'number' &&
                typeof touch.clientY === 'number' &&
                !isNaN(touch.clientX) &&
                !isNaN(touch.clientY) &&
                touch.clientX > 0 &&
                touch.clientY > 0
            );

            if (validTouches.length < 2) {
                console.log('🗺️ Insufficient valid touches for token identification:', validTouches.length);
                this.showToast('Need at least 2 valid touch points for token identification', 'warning');
                return;
            }

            console.log('🗺️ Valid touches for token identification:', validTouches);

            // First try to identify token using the touch pattern
            console.log('🗺️ Calling identifyToken with valid touches:', validTouches);
            const result = this.identifyToken(validTouches);
            console.log('🗺️ Token identification result:', result);

            // 🎯 DEBUG: Log the structure of the result
            if (result) {
                console.log('🗺️ Result structure:', {
                    name: result.name,
                    confidence: result.confidence,
                    hasToken: !!result.token,
                    tokenId: result.token?.id,
                    tokenName: result.token?.name
                });
            }

            if (result) {
                this.showToast(`🗺️ Token identified on map: ${result.name} (${result.confidence}%)`, 'success');

                // 🎯 FIXED: Use the same logic as the working "Place Marker" button
                // result.token contains the actual token object, result.id is undefined
                const token = result.token;
                const tokenId = token.id;

                // 🎯 NEW: Check if token already has a marker at this location
                const existingMarker = this.findExistingTokenMarker(tokenId, { lat: avgLat, lng: avgLng });
                if (existingMarker) {
                    // Update existing marker location if needed
                    this.updateTokenMarkerLocation(existingMarker, { lat: avgLat, lng: avgLng });
                    this.showToast(`🗺️ Updated existing marker for ${result.name}`, 'info');
                } else {
                    // Place new permanent token marker at the identified location
                    // Use the same function call as the working "Place Marker" button
                    const markerId = this.placeTokenMarkerOnMap(token, { lat: avgLat, lng: avgLng });

                    // 🎯 DEBUG: Verify marker was created and saved
                    if (markerId) {
                        console.log(`🗺️ Marker created successfully with ID: ${markerId}`);

                        // Verify marker is in the map
                        const markerEntry = this.mapTokenMarkers.get(markerId);
                        if (markerEntry) {
                            console.log(`🗺️ Marker verified in map:`, {
                                tokenId: markerEntry.tokenId,
                                location: markerEntry.location,
                                createdAt: markerEntry.createdAt
                            });

                            // Verify marker is saved to storage
                            const storedMarkers = localStorage.getItem('mapTokenMarkers');
                            if (storedMarkers) {
                                const markers = JSON.parse(storedMarkers);
                                const storedMarker = markers.find(m => m.id === markerId);
                                if (storedMarker) {
                                    console.log(`🗺️ Marker verified in storage:`, storedMarker);
                                } else {
                                    console.warn(`🗺️ Marker not found in storage!`);
                                }
                            }
                        } else {
                            console.error(`🗺️ Marker not found in map after creation!`);
                        }
                    } else {
                        console.error(`🗺️ Failed to create marker for token: ${token.name}`);
                    }
                }

                // Add temporary success indicator
                this.addMapSuccessMarker(avgLat, avgLng);
            } else {
                // If no token identified by pattern, check if touching near existing token
                const nearbyToken = this.findTokenByMapLocation({ lat: avgLat, lng: avgLng });
                if (nearbyToken) {
                    this.showToast(`🗺️ Touched near token: ${nearbyToken.name}`, 'info');

                    // Highlight the existing token marker
                    this.highlightExistingTokenMarker(nearbyToken.id);
                } else {
                    this.showToast('🗺️ No matching token found at this location', 'warning');
                }
            }

            // Process the touch pattern for any additional logic
            this.processMapTokenIdentification(canvasTouches);

            // Clear the stored touch data after processing
            this.currentMapTouchData = null;
        } else {
            console.log('🗺️ Insufficient touch data for token identification:', this.currentMapTouchData);
        }
    }

    // Process map token identification
    processMapTokenIdentification(touches) {
        // Get the current map center and zoom for context
        const mapCenter = this.map.getCenter();
        const mapZoom = this.map.getZoom();

        console.log(`🗺️ Map context: Center ${mapCenter.lat.toFixed(6)}, ${mapCenter.lng.toFixed(6)}, Zoom ${mapZoom}`);

        // 🎯 FIXED: Remove duplicate token identification and marker placement logic
        // This function was causing conflicts with the main marker placement in handleMapTouchEnd
        // Token identification and marker placement is now handled only in handleMapTouchEnd
        console.log('🗺️ ProcessMapTokenIdentification - Token processing completed in main handler');

        // No duplicate marker placement here - only the main logic in handleMapTouchEnd handles this
    }

    // Update map touch display
    updateMapTouchDisplay(touches) {
        // Update the touch pattern display even in map view
        if (touches.length >= 2) {
            this.updateTouchData(touches);
        }
    }

    // Go to specific map location
    goToMapLocation() {
        const latInput = document.getElementById('mapLatInput');
        const lngInput = document.getElementById('mapLngInput');

        if (!latInput || !lngInput || !this.map) return;

        const lat = parseFloat(latInput.value);
        const lng = parseFloat(lngInput.value);

        if (isNaN(lat) || isNaN(lng)) {
            this.showToast('Please enter valid coordinates', 'warning');
            return;
        }

        if (lat < -90 || lat > 90 || lng < -180 || lng > 180) {
            this.showToast('Coordinates out of range (Lat: -90 to 90, Lng: -180 to 180)', 'warning');
            return;
        }

        this.map.setView([lat, lng], 15);
        this.showToast(`Moved to ${lat.toFixed(6)}, ${lng.toFixed(6)}`, 'success');

        // Add a marker at the location
        L.marker([lat, lng], {
            icon: L.divIcon({
                className: 'map-location-marker',
                html: `<div style="background: #2196F3; width: 25px; height: 25px; border-radius: 50%; border: 3px solid white; box-shadow: 0 2px 8px rgba(0,0,0,0.3);"></div>`,
                iconSize: [25, 25],
                iconAnchor: [12.5, 12.5]
            })
        }).addTo(this.map).bindPopup(`Custom Location<br>${lat.toFixed(6)}, ${lng.toFixed(6)}`);
    }

    // Use current location
    useCurrentLocation() {
        if (!this.map) return;

        if (navigator.geolocation) {
            this.showToast('Getting your location...', 'info');

            navigator.geolocation.getCurrentPosition(
                (position) => {
                    const lat = position.coords.latitude;
                    const lng = position.coords.longitude;

                    // Update input fields
                    const latInput = document.getElementById('mapLatInput');
                    const lngInput = document.getElementById('mapLngInput');

                    if (latInput) latInput.value = lat.toFixed(6);
                    if (lngInput) lngInput.value = lng.toFixed(6);

                    // Move map to current location
                    this.map.setView([lat, lng], 15);

                    // Add current location marker
                    L.marker([lat, lng], {
                        icon: L.divIcon({
                            className: 'map-current-marker',
                            html: `<div style="background: #4CAF50; width: 25px; height: 25px; border-radius: 50%; border: 3px solid white; box-shadow: 0 2px 8px rgba(0,0,0,0.3); display: flex: center; justify-content: center; color: white; font-weight: bold;">📍</div>`,
                            iconSize: [25, 25],
                            iconAnchor: [12.5, 12.5]
                        })
                    }).addTo(this.map).bindPopup(`Your Location<br>${lat.toFixed(6)}, ${lng.toFixed(6)}`);

                    this.showToast(`Located at ${lat.toFixed(6)}, ${lng.toFixed(6)}`, 'success');
                },
                (error) => {
                    console.error('Geolocation error:', error);
                    this.showToast('Could not get your location', 'error');
                },
                {
                    enableHighAccuracy: true,
                    timeout: 10000,
                    maximumAge: 60000
                }
            );
        } else {
            this.showToast('Geolocation not supported by this browser', 'warning');
        }
    }

    // Demo map view functionality
    demoMapView() {
        console.log('🗺️ Starting map view demo...');

        // Switch to map view
        this.toggleMapView();

        // Wait for map to initialize, then show demo
        setTimeout(() => {
            if (this.map) {
                // Show some interesting locations
                const demoLocations = [
                    { lat: 40.7128, lng: -74.0060, name: 'New York City' },
                    { lat: 34.0522, lng: -118.2437, name: 'Los Angeles' },
                    { lat: 41.8781, lng: -87.6298, name: 'Chicago' }
                ];

                demoLocations.forEach((location, index) => {
                    setTimeout(() => {
                        this.map.setView([location.lat, location.lng], 12);

                        L.marker([location.lat, location.lng], {
                            icon: L.divIcon({
                                className: 'map-demo-marker',
                                html: `<div style="background: #9c27b0; width: 30px; height: 30px; border-radius: 50%; border: 3px solid white; box-shadow: 0 2px 8px rgba(0,0,0,0.3); display: flex: align-items: center; justify-content: center; color: white; font-weight: bold;">${index + 1}</div>`,
                                iconSize: [30, 30],
                                iconAnchor: [15, 15]
                            })
                        }).addTo(this.map).bindPopup(`<b>${location.name}</b><br>Demo Location ${index + 1}`);

                        this.showToast(`Demo: ${location.name}`, 'info');
                    }, index * 2000); // Show each location every 2 seconds
                });

                this.showToast('🗺️ Map demo started! Watch the locations change automatically.', 'success');
            }
        }, 1000);
    }

    // NEW: Place token marker on map when token is trained
    placeTokenMarkerOnMap(token, location, isRestore = false) {
        console.log('🗺️ placeTokenMarkerOnMap called with:', { token, location, isRestore });

        if (!this.map) {
            console.log('Map not initialized, cannot place marker');
            return;
        }

        try {
            const markerId = `token_${token.id || this.mapMarkerCounter++}`;

            // Check if marker already exists
            if (this.mapTokenMarkers.has(markerId)) {
                console.log(`🗺️ Marker already exists for token ${token.name}, updating location`);
                const existingMarker = this.mapTokenMarkers.get(markerId);
                existingMarker.marker.setLatLng([location.lat, location.lng]);
                existingMarker.location = location;
                this.saveMapMarkersToStorage();
                return markerId;
            }

            // Create custom icon for token marker
            const icon = L.divIcon({
                className: 'map-token-marker',
                html: `<div style="background: #4CAF50; width: 25px; height: 25px; border-radius: 50%; border: 3px solid white; box-shadow: 0 2px 8px rgba(0,0,0,0.3); display: flex; align-items: center; justify-content: center; color: white; font-weight: bold; font-size: 14px;">🎯</div>`,
                iconSize: [25, 25],
                iconAnchor: [12.5, 12.5]
            });

            // Create marker
            const marker = L.marker([location.lat, location.lng], { icon: icon });

            // Create detailed tooltip content (2-3 lines)
            const tooltipContent = this.createTokenTooltipContent(token);

            // Bind popup with token information
            marker.bindPopup(tooltipContent, {
                maxWidth: 300,
                className: 'token-marker-popup'
            });

            // Add marker to map
            marker.addTo(this.map);

            // Store marker reference
            this.mapTokenMarkers.set(markerId, {
                marker: marker,
                tokenId: token.id,
                location: location,
                createdAt: new Date(),
                tokenName: token.name // Add token name for better debugging
            });

            console.log(`🗺️ Marker stored in mapTokenMarkers:`, {
                markerId: markerId,
                tokenId: token.id,
                tokenName: token.name,
                location: location,
                totalMarkers: this.mapTokenMarkers.size
            });

            // 🗺️ NEW: Save markers to storage (only if not restoring)
            if (!isRestore) {
                this.saveMapMarkersToStorage();
            }

            if (isRestore) {
                console.log(`🗺️ Token marker restored at ${location.lat}, ${location.lng} for token: ${token.name}`);
            } else {
                console.log(`🗺️ Token marker placed at ${location.lat}, ${location.lng} for token: ${token.name}`);
                this.showToast(`Token marker placed on map: ${token.name}`, 'success');
            }

            return markerId;
        } catch (error) {
            console.error('Error placing token marker on map:', error);
            if (!isRestore) {
                this.showToast('Error placing token marker on map', 'error');
            }
        }
    }

    // NEW: Create tooltip content for token markers (2-3 lines)
    createTokenTooltipContent(token) {
        const lines = [];

        // Line 1: Token name and ID
        lines.push(`<strong>${token.name}</strong> (ID: ${token.id})`);

        // Line 2: Touch count and signature details
        if (token.signature && token.signature.touchCount) {
            const touchCount = token.signature.touchCount;
            const complexity = token.signature.touchPattern?.complexity || 'Unknown';
            lines.push(`Touch Pattern: ${touchCount} fingers (Complexity: ${complexity})`);
        }

        // Line 3: Training date and location info
        if (token.trainingDate) {
            lines.push(`Trained: ${new Date(token.trainingDate).toLocaleDateString()}`);
        }

        // Add detailed token information button
        lines.push(`<button onclick="window.currentTokenSystem.showDetailedTokenInfo('${token.id}')" style="background: #2196F3; color: white; border: none; padding: 8px 12px; border-radius: 4px; cursor: pointer; margin-top: 8px; margin-right: 5px;">📊 Details</button>`);

        // Add remove button
        lines.push(`<button onclick="window.currentTokenSystem.removeTokenMarker('${token.id}')" style="background: #f44336; color: white; border: none; padding: 5px 10px; border-radius: 4px; cursor: pointer; margin-top: 8px;">🗑️ Remove</button>`);

        return lines.join('<br>');
    }

    // NEW: Remove token marker from map
    async removeTokenMarker(tokenId) {
        const markerEntry = this.mapTokenMarkers.get(`token_${tokenId}`);
        if (markerEntry) {
            try {
                // Remove from map
                this.map.removeLayer(markerEntry.marker);
                this.mapTokenMarkers.delete(`token_${tokenId}`);

                // Remove from database
                await window.apiService.deleteMapMarker(`token_${tokenId}`);
                
                console.log(`📡 Token marker removed for token: ${tokenId}`);
                this.showToast('Token marker removed from map', 'info');
            } catch (error) {
                console.error('Error removing map marker from database:', error);
                this.showToast('Failed to remove map marker from database', 'error');
            }
        }
    }

    // NEW: Get current map location for token placement
    getCurrentMapLocation() {
        if (!this.map) {
            return null;
        }

        const center = this.map.getCenter();
        return {
            lat: center.lat,
            lng: center.lng,
            zoom: this.map.getZoom()
        };
    }

    // NEW: Place token at current map center
    placeTokenAtCurrentLocation(token) {
        const location = this.getCurrentMapLocation();
        if (location) {
            return this.placeTokenMarkerOnMap(token, location);
        }
        return null;
    }

    // NEW: Identify token at specific map location
    identifyTokenAtMapLocation(latlng) {
        if (!this.mapViewActive || !this.map) {
            return null;
        }

        // Check if there are any learned tokens
        if (this.learnedTokens.length === 0) {
            this.showToast('No tokens available for identification', 'info');
            return null;
        }

        // Create a temporary touch pattern at the map location
        // This simulates a touch pattern for token identification
        const simulatedTouch = {
            clientX: 0, // Not used for map identification
            clientY: 0, // Not used for map identification
            mapLat: latlng.lat,
            mapLng: latlng.lng,
            identifier: 'map-touch'
        };

        // Try to identify the token using the existing system
        // We'll use a simple proximity-based approach for map locations
        const identifiedToken = this.findTokenByMapLocation(latlng);

        if (identifiedToken) {
            this.showToast(`🗺️ Token found at location: ${identifiedToken.name}`, 'success');
            return identifiedToken;
        } else {
            this.showToast('🗺️ No token found at this location', 'info');
            return null;
        }
    }

    // NEW: Add success marker on map
    addMapSuccessMarker(lat, lng) {
        if (!this.map) return;

        const successMarker = L.marker([lat, lng], {
            icon: L.divIcon({
                className: 'map-success-marker',
                html: `<div style="background: #4CAF50; width: 30px; height: 30px; border-radius: 50%; border: 4px solid white; box-shadow: 0 4px 12px rgba(0,0,0,0.4); display: flex; align-items: center; justify-content: center; color: white; font-weight: bold;">✓</div>`,
                iconSize: [30, 30],
                iconAnchor: [15, 15]
            })
        }).addTo(this.map);

        // Auto-remove success marker after 3 seconds
        setTimeout(() => {
            if (this.map && successMarker) {
                this.map.removeLayer(successMarker);
            }
        }, 3000);

        return successMarker;
    }

    // NEW: Find token by map location (proximity-based)
    findTokenByMapLocation(latlng) {
        if (!this.mapTokenMarkers || this.mapTokenMarkers.size === 0) {
            return null;
        }

        // Check if there's a token marker near this location
        let closestToken = null;
        let closestDistance = Infinity;
        const proximityThreshold = 0.001; // About 100 meters in degrees

        for (const [markerId, markerData] of this.mapTokenMarkers) {
            const markerLat = markerData.location.lat;
            const markerLng = markerData.location.lng;

            // Calculate distance between touch location and marker
            const distance = Math.sqrt(
                Math.pow(latlng.lat - markerLat, 2) +
                Math.pow(latlng.lng - markerLng, 2)
            );

            if (distance < proximityThreshold && distance < closestDistance) {
                closestDistance = distance;
                closestToken = this.learnedTokens.find(t => t.id === markerData.tokenId);
            }
        }

        return closestToken;
    }

    // NEW: Highlight existing token marker
    highlightExistingTokenMarker(tokenId) {
        const markerEntry = this.mapTokenMarkers.get(`token_${tokenId}`);
        if (markerEntry && markerEntry.marker) {
            // Temporarily change the marker icon to highlight it
            const originalIcon = markerEntry.marker.getIcon();

            // Create a highlighted version of the icon
            const highlightedIcon = L.divIcon({
                className: 'map-token-marker-highlight',
                html: `<div style="background: #FF9800; width: 40px; height: 40px; border-radius: 50%; border: 4px solid white; box-shadow: 0 4px 12px rgba(255,152,0,0.6); display: flex; align-items: center; justify-content: center; color: white; font-weight: bold; font-size: 18px;">🎯</div>`,
                iconSize: [40, 40],
                iconAnchor: [20, 20]
            });

            markerEntry.marker.setIcon(highlightedIcon);

            // Restore original icon after 2 seconds
            setTimeout(() => {
                if (markerEntry.marker) {
                    markerEntry.marker.setIcon(originalIcon);
                }
            }, 2000);

            console.log(`🗺️ Highlighted token marker for: ${tokenId}`);
        }
    }

    // NEW: Add temporary success marker on map
    addMapSuccessMarker(lat, lng) {
        if (!this.map) return;

        const successMarker = L.marker([lat, lng], {
            icon: L.divIcon({
                className: 'map-success-marker',
                html: `<div style="background: #4CAF50; width: 30px; height: 30px; border-radius: 50%; border: 4px solid white; box-shadow: 0 4px 12px rgba(0,0,0,0.4); display: flex; align-items: center; justify-content: center; color: white; font-weight: bold;">✓</div>`,
                iconSize: [30, 30],
                iconAnchor: [15, 15]
            })
        }).addTo(this.map);

        // Auto-remove success marker after 3 seconds
        setTimeout(() => {
            if (this.map && successMarker) {
                this.map.removeLayer(successMarker);
            }
        }, 3000);
    }

    // NEW: Highlight existing token marker
    highlightExistingTokenMarker(tokenId) {
        const markerEntry = this.mapTokenMarkers.get(`token_${tokenId}`);
        if (markerEntry && markerEntry.marker) {
            // Create a temporary highlight effect
            const originalIcon = markerEntry.marker.getIcon();

            // Change to highlight icon
            const highlightIcon = L.divIcon({
                className: 'map-token-marker-highlight',
                html: `<div style="background: #FFD700; width: 35px; height: 35px; border-radius: 50%; border: 4px solid white; box-shadow: 0 0 20px rgba(255, 215, 0, 0.8); display: flex; align-items: center; justify-content: center; color: white; font-weight: bold; font-size: 14px;">🎯</div>`,
                iconSize: [35, 35],
                iconAnchor: [17.5, 17.5]
            });

            markerEntry.marker.setIcon(highlightIcon);

            // Restore original icon after 2 seconds
            setTimeout(() => {
                if (markerEntry.marker) {
                    markerEntry.marker.setIcon(originalIcon);
                }
            }, 2000);
        }
    }

    // NEW: Show dialog to place token marker at current map location
    showPlaceTokenMarkerDialog() {
        if (!this.mapViewActive || !this.map) {
            this.showToast('Please activate map view first', 'warning');
            return;
        }

        if (this.learnedTokens.length === 0) {
            this.showToast('No tokens available. Train some tokens first!', 'warning');
            return;
        }

        const location = this.getCurrentMapLocation();
        if (!location) {
            this.showToast('Could not get current map location', 'error');
            return;
        }

        // Create token selection modal
        const modal = document.createElement('div');
        modal.className = 'modal';
        modal.style.display = 'block';
        modal.innerHTML = `
            <div class="modal-content" style="max-width: 500px;">
                <div class="modal-header">
                    <h2>🗺️ Place Token Marker</h2>
                    <button class="close-btn" onclick="this.closest('.modal').remove()">&times;</button>
                </div>
                <div class="modal-body">
                    <div style="background: rgba(33, 150, 243, 0.1); border: 1px solid #2196F3; border-radius: 8px; padding: 15px; margin-bottom: 20px;">
                        <h3 style="color: #2196F3; margin-bottom: 10px;">📍 Map Location</h3>
                        <div style="font-family: monospace; font-size: 14px;">
                            <div>Latitude: ${location.lat.toFixed(6)}</div>
                            <div>Longitude: ${location.lng.toFixed(6)}</div>
                            <div>Zoom Level: ${location.zoom}</div>
                        </div>
                    </div>
                    
                    <div style="background: rgba(76, 175, 80, 0.1); border: 1px solid #4CAF50; border-radius: 8px; padding: 15px; margin-bottom: 20px;">
                        <h3 style="color: #4CAF50; margin-bottom: 10px;">🎯 Select Token</h3>
                        <div style="max-height: 200px; overflow-y: auto;">
                            ${this.learnedTokens.map((token, index) => `
                                <div style="padding: 10px; margin: 5px 0; background: rgba(255,255,255,0.1); border-radius: 6px; cursor: pointer;" 
                                     onclick="placeTokenFromDialog(${index}, ${location.lat}, ${location.lng})">
                                    <strong>${token.name}</strong> (${token.signature.touchCount} fingers)
                                    <div style="font-size: 12px; color: #ccc;">ID: ${token.id}</div>
                                </div>
                            `).join('')}
                        </div>
                    </div>
                </div>
                <div class="modal-footer">
                    <button class="btn" onclick="this.closest('.modal').remove()">Cancel</button>
                </div>
            </div>
        `;

        document.body.appendChild(modal);

        // Add the global function for placing tokens from dialog
        window.placeTokenFromDialog = (tokenIndex, lat, lng) => {
            try {
                console.log('🗺️ placeTokenFromDialog called with:', { tokenIndex, lat, lng });

                if (tokenIndex >= 0 && tokenIndex < this.learnedTokens.length) {
                    const token = this.learnedTokens[tokenIndex];
                    console.log('🗺️ Selected token:', token);

                    // Enter placement mode - close modal and wait for map click
                    const modal = document.querySelector('.modal');
                    if (modal) {
                        modal.remove();
                    }

                    // Show instruction message
                    this.showToast(`🎯 Click anywhere on the map to place "${token.name}"`, 'info');

                    // Store the token for placement mode
                    this.pendingTokenPlacement = token;

                    // Enable placement mode
                    this.enableTokenPlacementMode();

                } else {
                    console.error('🗺️ Invalid token index:', tokenIndex);
                    this.showToast('Invalid token selection', 'error');
                }
            } catch (error) {
                console.error('🗺️ Error in placeTokenFromDialog:', error);
                this.showToast('Error placing token marker: ' + error.message, 'error');
            }
        };
    }

    // NEW: Enable token placement mode
    enableTokenPlacementMode() {
        if (!this.map || !this.pendingTokenPlacement) {
            return;
        }

        console.log('🗺️ Token placement mode enabled for:', this.pendingTokenPlacement.name);

        // Add click event listener to map for token placement
        this.map.once('click', (e) => {
            this.placeTokenAtMapLocation(e.latlng);
        });

        // Change cursor to indicate placement mode
        this.map.getContainer().style.cursor = 'crosshair';

        // Add visual indicator that placement mode is active
        this.showPlacementModeIndicator();
    }

    // NEW: Place token at specific map location
    placeTokenAtMapLocation(latlng) {
        if (!this.pendingTokenPlacement || !this.map) {
            return;
        }

        try {
            console.log('🗺️ Placing token at map location:', latlng);

            // Place the token marker at the clicked location
            const markerId = this.placeTokenMarkerOnMap(this.pendingTokenPlacement, latlng);

            if (markerId) {
                console.log('🗺️ Token marker placed successfully at:', latlng);
                this.showToast(`✅ "${this.pendingTokenPlacement.name}" placed at selected location!`, 'success');

                // Clear placement mode
                this.clearTokenPlacementMode();
            } else {
                console.error('🗺️ Failed to place token marker');
                this.showToast('Failed to place token marker', 'error');
            }
        } catch (error) {
            console.error('🗺️ Error placing token at map location:', error);
            this.showToast('Error placing token marker: ' + error.message, 'error');
        }
    }

    // NEW: Clear token placement mode
    clearTokenPlacementMode() {
        if (this.map) {
            // Remove click event listener
            this.map.off('click');

            // Reset cursor
            this.map.getContainer().style.cursor = '';
        }

        // Clear pending token
        this.pendingTokenPlacement = null;

        // Remove placement mode indicator
        this.removePlacementModeIndicator();

        console.log('🗺️ Token placement mode cleared');
    }

    // NEW: Show placement mode indicator
    showPlacementModeIndicator() {
        // Create or update placement mode indicator
        let indicator = document.getElementById('placementModeIndicator');
        if (!indicator) {
            indicator = document.createElement('div');
            indicator.id = 'placementModeIndicator';
            indicator.style.cssText = `
                position: fixed;
                top: 20px;
                left: 50%;
                transform: translateX(-50%);
                background: linear-gradient(135deg, #4CAF50, #45a049);
                color: white;
                padding: 15px 25px;
                border-radius: 25px;
                box-shadow: 0 4px 15px rgba(0,0,0,0.3);
                z-index: 10000;
                font-weight: bold;
                font-size: 16px;
                border: 3px solid white;
            `;
            document.body.appendChild(indicator);
        }

        indicator.innerHTML = `🎯 Click on the map to place "${this.pendingTokenPlacement.name}"`;
        indicator.style.display = 'block';
    }

    // NEW: Remove placement mode indicator
    removePlacementModeIndicator() {
        const indicator = document.getElementById('placementModeIndicator');
        if (indicator) {
            indicator.remove();
        }
    }

    // NEW: Find existing token marker
    findExistingTokenMarker(tokenId, location) {
        const markerId = `token_${tokenId}`;
        const existingMarker = this.mapTokenMarkers.get(markerId);

        console.log(`🗺️ findExistingTokenMarker: tokenId=${tokenId}, markerId=${markerId}, found=${!!existingMarker}`);

        if (existingMarker) {
            console.log(`🗺️ Existing marker details:`, {
                tokenId: existingMarker.tokenId,
                location: existingMarker.location,
                createdAt: existingMarker.createdAt
            });
        }

        return existingMarker;
    }

    // NEW: Update token marker location
    updateTokenMarkerLocation(markerEntry, newLocation) {
        if (markerEntry && markerEntry.marker) {
            markerEntry.marker.setLatLng([newLocation.lat, newLocation.lng]);
            markerEntry.location = newLocation;
            this.saveMapMarkersToStorage();
            console.log('🗺️ Updated marker location for token:', markerEntry.tokenId);
        }
    }

    // NEW: Show detailed token information
    showDetailedTokenInfo(tokenId) {
        const token = this.learnedTokens.find(t => t.id == tokenId);
        if (!token) {
            this.showToast('Token not found', 'error');
            return;
        }

        // Find marker location
        const markerEntry = this.mapTokenMarkers.get(`token_${tokenId}`);
        const markerLocation = markerEntry ? markerEntry.location : null;

        // Create detailed information modal
        const modal = document.createElement('div');
        modal.className = 'modal';
        modal.style.display = 'block';
        modal.innerHTML = `
            <div class="modal-content" style="max-width: 700px; max-height: 80vh; overflow-y: auto;">
                <div class="modal-header">
                    <h2>📊 Token Details: ${token.name}</h2>
                    <button class="close-btn" onclick="this.closest('.modal').remove()">&times;</button>
                </div>
                <div class="modal-body">
                    <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 20px;">
                        <!-- Basic Information -->
                        <div style="background: rgba(76, 175, 80, 0.1); border: 1px solid #4CAF50; border-radius: 8px; padding: 15px;">
                            <h3 style="color: #4CAF50; margin-bottom: 15px;">🔍 Basic Information</h3>
                            <div style="font-family: monospace; font-size: 14px; line-height: 1.6;">
                                <div><strong>Name:</strong> ${token.name}</div>
                                <div><strong>ID:</strong> ${token.id}</div>
                                <div><strong>Training Date:</strong> ${token.trainingDate ? new Date(token.trainingDate).toLocaleString() : 'Unknown'}</div>
                                <div><strong>Touch Count:</strong> ${token.signature?.touchCount || 'Unknown'}</div>
                            </div>
                        </div>
                        
                        <!-- Map Location -->
                        <div style="background: rgba(33, 150, 243, 0.1); border: 1px solid #2196F3; border-radius: 8px; padding: 15px;">
                            <h3 style="color: #2196F3; margin-bottom: 15px;">🗺️ Map Location</h3>
                            <div style="font-family: monospace; font-size: 14px; line-height: 1.6;">
                                ${markerLocation ? `
                                    <div><strong>Latitude:</strong> ${markerLocation.lat.toFixed(6)}</div>
                                    <div><strong>Longitude:</strong> ${markerLocation.lng.toFixed(6)}</div>
                                    <div><strong>Placed:</strong> ${markerEntry.createdAt ? new Date(markerEntry.createdAt).toLocaleString() : 'Unknown'}</div>
                                ` : '<div style="color: #f44336;">No marker placed on map</div>'}
                            </div>
                        </div>
                    </div>
                    
                    <!-- Token Signature Details -->
                    <div style="background: rgba(255, 152, 0, 0.1); border: 1px solid #FF9800; border-radius: 8px; padding: 15px; margin-top: 20px;">
                        <h3 style="color: #FF9800; margin-bottom: 15px;">🎯 Token Signature</h3>
                        <div style="font-family: monospace; font-size: 13px; line-height: 1.5; max-height: 200px; overflow-y: auto;">
                            ${this.formatTokenSignature(token.signature)}
                        </div>
                    </div>
                    
                    <!-- Touch Pattern Analysis -->
                    <div style="background: rgba(156, 39, 176, 0.1); border: 1px solid #9C27B0; border-radius: 8px; padding: 15px; margin-top: 20px;">
                        <h3 style="color: #9C27B0; margin-bottom: 15px;">📏 Touch Pattern Analysis</h3>
                        <div style="font-family: monospace; font-size: 13px; line-height: 1.5;">
                            ${this.formatTouchPatternAnalysis(token.signature?.touchPattern)}
                        </div>
                    </div>
                </div>
                <div class="modal-footer">
                    <button class="btn" onclick="this.closest('.modal').remove()">Close</button>
                </div>
            </div>
        `;

        document.body.appendChild(modal);
    }

    // NEW: Format token signature for display
    formatTokenSignature(signature) {
        if (!signature) return 'No signature data available';

        let output = '';
        output += `<div><strong>Touch Count:</strong> ${signature.touchCount || 'Unknown'}</div>`;
        output += `<div><strong>Timestamp:</strong> ${signature.timestamp ? new Date(signature.timestamp).toLocaleString() : 'Unknown'}</div>`;

        if (signature.touchProperties) {
            output += `<div><strong>Touch Properties:</strong></div>`;
            output += `<div style="margin-left: 20px;">`;
            output += `<div>• Radius: ${signature.touchProperties.radius || 'Unknown'}</div>`;
            output += `<div>• Rotation: ${signature.touchProperties.rotation || 'Unknown'}</div>`;
            output += `</div>`;
        }

        if (signature.touchPattern) {
            output += `<div><strong>Pattern Type:</strong> ${signature.touchPattern.type || 'Unknown'}</div>`;
            output += `<div><strong>Complexity:</strong> ${signature.touchPattern.complexity || 'Unknown'}</div>`;
        }

        return output;
    }

    // NEW: Format touch pattern analysis for display
    formatTouchPatternAnalysis(touchPattern) {
        if (!touchPattern) return 'No touch pattern data available';

        let output = '';
        output += `<div><strong>Pattern Type:</strong> ${touchPattern.type || 'Unknown'}</div>`;
        output += `<div><strong>Complexity:</strong> ${touchPattern.complexity || 'Unknown'}</div>`;
        output += `<div><strong>Distances Count:</strong> ${touchPattern.distancesCount || 'Unknown'}</div>`;

        if (touchPattern.distances && touchPattern.distances.length > 0) {
            output += `<div><strong>Distances:</strong></div>`;
            output += `<div style="margin-left: 20px;">`;
            touchPattern.distances.forEach((dist, index) => {
                output += `<div>• Distance ${index + 1}: ${dist.toFixed(2)}px</div>`;
            });
            output += `</div>`;
        }

        if (touchPattern.avgDistance) {
            output += `<div><strong>Average Distance:</strong> ${touchPattern.avgDistance.toFixed(2)}px</div>`;
        }

        if (touchPattern.minDistance) {
            output += `<div><strong>Min Distance:</strong> ${touchPattern.minDistance.toFixed(2)}px</div>`;
        }

        if (touchPattern.maxDistance) {
            output += `<div><strong>Max Distance:</strong> ${touchPattern.maxDistance.toFixed(2)}px</div>`;
        }

        return output;
    }
}

// Initialize the system
let tokenSystem;
document.addEventListener('DOMContentLoaded', () => {
    try {
        tokenSystem = new TokenSystem();
        window.tokenSystem = tokenSystem; // Make it globally accessible
        window.currentTokenSystem = tokenSystem; // Keep backward compatibility
        console.log('TokenSystem initialized successfully');
    } catch (error) {
        console.error('Error initializing TokenSystem:', error);
    }
});

// Navigation functions are now methods of the TokenSystem class

