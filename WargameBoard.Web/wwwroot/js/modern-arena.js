// Modern Game Arena JavaScript
let arenaState = {
    isFullscreen: false,
    currentRole: 'tokens',
    selectedTeam: 'alpha',
    selectedToken: null,
    mapMode: 'battle',
    terrainMode: false,
    isInitialized: false,
    currentSession: null,
    gameStarted: false
};

// Initialize the modern arena
document.addEventListener('DOMContentLoaded', function() {
    initializeArena();
    setupEventListeners();
    loadArenaAssets();
});

// Initialize the arena with immersive setup
function initializeArena() {
    console.log('Initializing Modern Game Arena...');
    
    // Show role selection first
    showRoleSelectionModal();
}

function hideSidebarsInArena() {
    // Hide sidebars when entering arena mode
    const leftSidebar = document.querySelector('.arena-sidebar-left');
    const rightSidebar = document.querySelector('.arena-sidebar-right');
    
    if (leftSidebar) {
        leftSidebar.style.display = 'none';
    }
    if (rightSidebar) {
        rightSidebar.style.display = 'none';
    }
    
    // Expand map to full width
    const mapContainer = document.querySelector('.real-map-container');
    if (mapContainer) {
        mapContainer.style.width = '100vw';
        mapContainer.style.left = '0';
    }
    
    console.log('Sidebars hidden for arena mode');
}

function showSidebarsInNormal() {
    // Show sidebars when exiting arena mode
    const leftSidebar = document.querySelector('.arena-sidebar-left');
    const rightSidebar = document.querySelector('.arena-sidebar-right');
    
    if (leftSidebar) {
        leftSidebar.style.display = 'block';
    }
    if (rightSidebar) {
        rightSidebar.style.display = 'block';
    }
    
    // Restore map width
    const mapContainer = document.querySelector('.real-map-container');
    if (mapContainer) {
        mapContainer.style.width = 'calc(100% - 480px)';
        mapContainer.style.left = '240px';
    }
    
    console.log('Sidebars restored for normal mode');
}

// Clean up modal backdrop
function cleanupModalBackdrop() {
    // Remove any existing backdrop elements
    const backdrops = document.querySelectorAll('.modal-backdrop');
    backdrops.forEach(backdrop => backdrop.remove());
    
    // Remove modal-open class from body
    document.body.classList.remove('modal-open');
    
    // Reset body padding if it was modified
    document.body.style.paddingRight = '';
}

// Show role selection modal
function showRoleSelectionModal() {
    // Clean up any existing modals first
    cleanupModalBackdrop();
    // Create role selection modal
    const modalHtml = `
        <div class="modal fade" id="roleSelectionModal" tabindex="-1" data-bs-backdrop="static" data-bs-keyboard="false">
            <div class="modal-dialog modal-lg">
                <div class="modal-content arena-modal">
                    <div class="modal-header">
                        <h4 class="modal-title">🎮 SELECT YOUR ROLE</h4>
                    </div>
                    <div class="modal-body">
                        <div class="role-selection-grid">
                            <div class="role-card" data-role="spectator" onclick="selectRole('spectator')">
                                <div class="role-icon">👁️</div>
                                <h5>SPECTATOR</h5>
                                <p>Master Observer - Full access to all game data and controls</p>
                                <div class="role-badge master">MASTER</div>
                            </div>
                            <div class="role-card" data-role="fox" onclick="selectRole('fox')">
                                <div class="role-icon">🦊</div>
                                <h5>FOX LAND</h5>
                                <p>Command Fox forces and manage territory</p>
                            </div>
                            <div class="role-card" data-role="blue" onclick="selectRole('blue')">
                                <div class="role-icon">🔵</div>
                                <h5>BLUE LAND</h5>
                                <p>Command Blue forces and manage territory</p>
                            </div>
                            <div class="role-card" data-role="tokens" onclick="selectRole('tokens')">
                                <div class="role-icon">🎯</div>
                                <h5>TOKENS</h5>
                                <p>Manage token placement and movement</p>
                            </div>
                            <div class="role-card" data-role="data" onclick="selectRole('data')">
                                <div class="role-icon">📊</div>
                                <h5>DATA ENTRY</h5>
                                <p>Configure maps, settings, and game parameters</p>
                            </div>
                            <div class="role-card" data-role="settings" onclick="selectRole('settings')">
                                <div class="role-icon">⚙️</div>
                                <h5>SETTINGS</h5>
                                <p>Manage game settings and preferences</p>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    `;
    
    // Add modal to body
    document.body.insertAdjacentHTML('beforeend', modalHtml);
    
    // Show modal
    const modal = new bootstrap.Modal(document.getElementById('roleSelectionModal'));
    modal.show();
    
    // Clean up backdrop when modal is hidden
    document.getElementById('roleSelectionModal').addEventListener('hidden.bs.modal', function() {
        cleanupModalBackdrop();
        this.remove();
    });
}

// Select role and initialize arena
function selectRole(role) {
    console.log(`Selected role: ${role}`);
    
    // Update arena state
    arenaState.currentRole = role;
    
    // Close modal
    const modal = bootstrap.Modal.getInstance(document.getElementById('roleSelectionModal'));
    if (modal) modal.hide();
    
    // Show loading screen
    showArenaLoadingScreen();
    
    // Initialize all systems based on role
    setTimeout(() => {
        // Initialize 3D-style effects
        initializeVisualEffects();
        
        // Setup role management
        initializeRoleManagement();
        
        // Initialize map tools
        initializeMapTools();
        
        // Setup team management
        initializeTeamManagement();
        
        // Initialize token system
        initializeTokenSystem();
        
        // Setup fullscreen functionality
        initializeFullscreenMode();
        
        // Load arena assets
        loadArenaAssets();
        
        // Setup event listeners
        setupEventListeners();
        
        // Add game play interface
        addGamePlayInterface();
        
        // Initialize map editor
        if (window.initializeMapEditor) {
            window.initializeMapEditor();
        }
        
        // Initialize group management system
        initializeGroupManagement();
        
        // Initialize arena with realistic effects
        setTimeout(() => {
            hideArenaLoadingScreen();
            arenaState.isInitialized = true;
            
            showNotification(`Welcome! You are now ${role.toUpperCase()}`, 'success');
            
            // Show role-specific guidance
            showRoleGuidance(role);
        }, 2000);
    }, 1000);
}

// Show role-specific guidance
function showRoleGuidance(role) {
    const guidance = {
        spectator: {
            title: "👁️ SPECTATOR MASTER MODE",
            message: "You have full access to all game controls. Use the panels on the left to manage the game, place tokens, and configure settings.",
            actions: ["Use Map Tools to create/edit maps", "Place tokens using the token panel", "Configure game settings in Data Entry"]
        },
        fox: {
            title: "🦊 FOX LAND COMMANDER",
            message: "You are commanding the Fox forces. Use the terrain editor to modify the battlefield and place your units.",
            actions: ["Use terrain editor to modify hexes", "Place Fox tokens on the map", "Manage your territory"]
        },
        blue: {
            title: "🔵 BLUE LAND COMMANDER", 
            message: "You are commanding the Blue forces. Use the terrain editor to modify the battlefield and place your units.",
            actions: ["Use terrain editor to modify hexes", "Place Blue tokens on the map", "Manage your territory"]
        },
        tokens: {
            title: "🎯 TOKEN MANAGER",
            message: "You manage all token placement and movement. Select tokens and click on the map to place them.",
            actions: ["Select tokens from the token panel", "Click on map hexes to place tokens", "Click on existing tokens to move them"]
        },
        data: {
            title: "📊 DATA ENTRY SPECIALIST",
            message: "You configure all game parameters. Use the Data Entry panel to set up maps, rules, and settings.",
            actions: ["Configure map settings in Data Entry", "Set up game rules and parameters", "Save and load map configurations"]
        },
        settings: {
            title: "⚙️ SETTINGS MANAGER",
            message: "You manage game settings and preferences. Configure the arena and game parameters.",
            actions: ["Adjust arena settings", "Configure visual preferences", "Manage game parameters"]
        }
    };
    
    const roleInfo = guidance[role];
    if (roleInfo) {
        showNotification(roleInfo.title, 'info');
        
        // Show detailed guidance modal
        setTimeout(() => {
            showGuidanceModal(roleInfo);
        }, 1000);
    }
}

// Show guidance modal
function showGuidanceModal(roleInfo) {
    // Clean up any existing modals first
    cleanupModalBackdrop();
    
    const modalHtml = `
        <div class="modal fade" id="guidanceModal" tabindex="-1">
            <div class="modal-dialog modal-lg">
                <div class="modal-content arena-modal">
                    <div class="modal-header">
                        <h4 class="modal-title">${roleInfo.title}</h4>
                        <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                    </div>
                    <div class="modal-body">
                        <p class="guidance-message">${roleInfo.message}</p>
                        <div class="guidance-actions">
                            <h6>What you can do:</h6>
                            <ul>
                                ${roleInfo.actions.map(action => `<li>${action}</li>`).join('')}
                            </ul>
                        </div>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-primary" data-bs-dismiss="modal">Got it!</button>
                    </div>
                </div>
            </div>
        </div>
    `;
    
    document.body.insertAdjacentHTML('beforeend', modalHtml);
    const modal = new bootstrap.Modal(document.getElementById('guidanceModal'));
    modal.show();
    
    // Remove modal after closing and clean up backdrop
    document.getElementById('guidanceModal').addEventListener('hidden.bs.modal', function() {
        cleanupModalBackdrop();
        this.remove();
    });
}

// Show immersive loading screen
function showArenaLoadingScreen() {
    const loadingScreen = document.createElement('div');
    loadingScreen.id = 'arenaLoadingScreen';
    loadingScreen.innerHTML = `
        <div class="loading-container">
            <div class="loading-logo">
                <i class="bi bi-shield-fill"></i>
            </div>
            <div class="loading-text">
                <h2>Initializing Battle Arena</h2>
                <p>Loading tactical systems...</p>
            </div>
            <div class="loading-progress">
                <div class="progress-bar"></div>
            </div>
            </div>
        `;

    loadingScreen.style.cssText = `
        position: fixed;
        top: 0;
        left: 0;
        width: 100%;
        height: 100%;
        background: linear-gradient(135deg, #1a1a1a, #2c3e50);
        z-index: 10000;
        display: flex;
        align-items: center;
        justify-content: center;
        color: white;
    `;
    
    document.body.appendChild(loadingScreen);
}

// Hide loading screen with transition
function hideArenaLoadingScreen() {
    const loadingScreen = document.getElementById('arenaLoadingScreen');
    if (loadingScreen) {
        loadingScreen.style.transition = 'opacity 1s ease-out';
        loadingScreen.style.opacity = '0';
        setTimeout(() => {
            loadingScreen.remove();
        }, 1000);
    }
}

// Initialize visual effects for immersive experience
function initializeVisualEffects() {
    // Add particle effects to the arena
    createParticleSystem();
    
    // Add dynamic lighting effects
    addDynamicLighting();
    
    // Add realistic terrain textures
    enhanceTerrainTextures();
    
    // Add weather effects
    addWeatherEffects();
}

// Create particle system for immersive effects
function createParticleSystem() {
    const particleContainer = document.createElement('div');
    particleContainer.id = 'particleContainer';
    particleContainer.style.cssText = `
        position: fixed;
        top: 0;
        left: 0;
        width: 100%;
        height: 100%;
        pointer-events: none;
        z-index: 1;
    `;
    
    document.querySelector('.arena-map-container').appendChild(particleContainer);
    
    // Create floating particles
    for (let i = 0; i < 50; i++) {
        createFloatingParticle();
    }
}

// Create individual floating particles
function createFloatingParticle() {
    const particle = document.createElement('div');
    particle.className = 'floating-particle';
    particle.style.cssText = `
        position: absolute;
        width: 2px;
        height: 2px;
        background: rgba(52, 152, 219, 0.6);
        border-radius: 50%;
        left: ${Math.random() * 100}%;
        top: ${Math.random() * 100}%;
        animation: float ${5 + Math.random() * 10}s infinite linear;
    `;
    
    document.getElementById('particleContainer').appendChild(particle);
}

// Add dynamic lighting effects
function addDynamicLighting() {
    const lightingOverlay = document.createElement('div');
    lightingOverlay.id = 'lightingOverlay';
    lightingOverlay.style.cssText = `
        position: absolute;
        top: 0;
        left: 0;
        width: 100%;
        height: 100%;
        background: radial-gradient(circle at 30% 20%, rgba(52, 152, 219, 0.1) 0%, transparent 50%),
                    radial-gradient(circle at 70% 80%, rgba(231, 76, 60, 0.1) 0%, transparent 50%);
        pointer-events: none;
        z-index: 2;
    `;
    
    document.querySelector('.arena-map-container').appendChild(lightingOverlay);
}

// Enhance terrain textures for realistic look
function enhanceTerrainTextures() {
    const hexes = document.querySelectorAll('.hex-modern');
    hexes.forEach(hex => {
        // Add texture overlays based on terrain type
        const terrainClass = Array.from(hex.classList).find(cls => cls.startsWith('terrain-'));
        if (terrainClass) {
            addTerrainTexture(hex, terrainClass);
        }
    });
}

// Add specific terrain textures
function addTerrainTexture(hexElement, terrainClass) {
    const textureOverlay = document.createElement('div');
    textureOverlay.className = 'terrain-texture';
    
    const textureStyles = {
        'terrain-forest': `
            background: 
                radial-gradient(circle at 20% 20%, rgba(39, 174, 96, 0.3) 0%, transparent 50%),
                radial-gradient(circle at 80% 80%, rgba(46, 204, 113, 0.2) 0%, transparent 50%),
                linear-gradient(45deg, rgba(39, 174, 96, 0.1) 25%, transparent 25%),
                linear-gradient(-45deg, rgba(39, 174, 96, 0.1) 25%, transparent 25%);
        `,
        'terrain-mountain': `
            background: 
                linear-gradient(135deg, rgba(149, 165, 166, 0.4) 0%, rgba(127, 140, 141, 0.2) 100%),
                radial-gradient(circle at 30% 30%, rgba(189, 195, 199, 0.3) 0%, transparent 70%);
        `,
        'terrain-water': `
            background: 
                linear-gradient(45deg, rgba(52, 152, 219, 0.3) 25%, transparent 25%),
                linear-gradient(-45deg, rgba(52, 152, 219, 0.3) 25%, transparent 25%),
                radial-gradient(circle at 50% 50%, rgba(174, 214, 241, 0.2) 0%, transparent 70%);
        `,
        'terrain-desert': `
            background: 
                radial-gradient(circle at 25% 25%, rgba(243, 156, 18, 0.3) 0%, transparent 50%),
                linear-gradient(90deg, rgba(230, 126, 34, 0.1) 0%, transparent 50%);
        `,
        'terrain-urban': `
            background: 
                linear-gradient(0deg, rgba(231, 76, 60, 0.2) 0%, transparent 50%),
                linear-gradient(90deg, rgba(192, 57, 43, 0.1) 0%, transparent 50%);
        `,
        'terrain-swamp': `
            background: 
                radial-gradient(circle at 50% 50%, rgba(155, 89, 182, 0.3) 0%, transparent 70%),
                linear-gradient(45deg, rgba(142, 68, 173, 0.1) 25%, transparent 25%);
        `
    };
    
    textureOverlay.style.cssText = `
        position: absolute;
        top: 0;
        left: 0;
        width: 100%;
        height: 100%;
        ${textureStyles[terrainClass] || ''}
        pointer-events: none;
        z-index: 1;
    `;
    
    hexElement.appendChild(textureOverlay);
}

// Add weather effects
function addWeatherEffects() {
    const weatherContainer = document.createElement('div');
    weatherContainer.id = 'weatherContainer';
    weatherContainer.style.cssText = `
        position: absolute;
        top: 0;
        left: 0;
        width: 100%;
        height: 100%;
        pointer-events: none;
        z-index: 3;
    `;
    
    document.querySelector('.arena-map-container').appendChild(weatherContainer);
    
    // Add subtle rain effect
    createRainEffect();
}

// Create rain effect
function createRainEffect() {
    for (let i = 0; i < 100; i++) {
        const rainDrop = document.createElement('div');
        rainDrop.className = 'rain-drop';
        rainDrop.style.cssText = `
            position: absolute;
            width: 1px;
            height: 20px;
            background: linear-gradient(to bottom, rgba(52, 152, 219, 0.6), transparent);
            left: ${Math.random() * 100}%;
            top: -20px;
            animation: rain ${2 + Math.random() * 3}s infinite linear;
        `;
        
        document.getElementById('weatherContainer').appendChild(rainDrop);
    }
}

// Initialize role management system
function initializeRoleManagement() {
    const roleButtons = document.querySelectorAll('.role-btn');
    roleButtons.forEach(btn => {
        btn.addEventListener('click', function() {
            const role = this.dataset.role;
            switchRole(role);
        });
    });
}

// Switch between different roles
function switchRole(role) {
    // Remove active class from all role buttons
    document.querySelectorAll('.role-btn').forEach(btn => {
        btn.classList.remove('active');
    });
    
    // Add active class to selected role
    document.querySelector(`[data-role="${role}"]`).classList.add('active');
    
    arenaState.currentRole = role;
    
    // Update UI based on role
    updateUIForRole(role);
    
    showNotification(`Switched to ${role.toUpperCase()} mode`, 'info');
}

// Update UI based on selected role
function updateUIForRole(role) {
    console.log(`Updating UI for role: ${role} - GRANTING FULL ACCESS TO ALL SYSTEMS`);
    
    const tokenPanel = document.getElementById('tokenPanel');
    const terrainPanel = document.getElementById('terrainPanel');
    const dataEntryPanel = document.getElementById('dataEntryPanel');
    
    // ALL ROLES GET FULL ACCESS - NO RESTRICTIONS FOR MILITARY SIMULATION
    if (tokenPanel) {
        tokenPanel.style.display = 'block';
        console.log('✓ Token panel enabled for role:', role);
    }
    if (terrainPanel) {
        terrainPanel.style.display = 'block';
        console.log('✓ Terrain panel enabled for role:', role);
    }
    if (dataEntryPanel) {
        dataEntryPanel.classList.add('active');
        console.log('✓ Data entry panel enabled for role:', role);
    }
    
    // Log comprehensive access granted
    console.log(`🎖️ MILITARY SIMULATION: Role ${role.toUpperCase()} granted FULL ACCESS to all systems`);
    console.log('✓ Token Management: ENABLED');
    console.log('✓ Terrain Control: ENABLED');
    console.log('✓ Data Entry: ENABLED');
    console.log('✓ Map Tools: ENABLED');
    console.log('✓ Feature Assignment: ENABLED');
}

// Initialize map tools
function initializeMapTools() {
    // Map control buttons
    const layersBtn = document.querySelector('[title="Layers"]');
    const fullscreenBtn = document.querySelector('[title="Fullscreen"]');
    const resetBtn = document.querySelector('[title="Reset"]');
    const coordsBtn = document.querySelector('[title="Coordinates"]');
    
    if (layersBtn) layersBtn.addEventListener('click', toggleMapLayers);
    if (fullscreenBtn) fullscreenBtn.addEventListener('click', toggleFullscreen);
    if (resetBtn) resetBtn.addEventListener('click', resetMap);
    if (coordsBtn) coordsBtn.addEventListener('click', toggleCoordinates);
    
    // Game tool buttons
    const placeTokenBtn = document.getElementById('placeTokenBtn');
    const selectBtn = document.getElementById('selectBtn');
    const measureBtn = document.querySelector('[title="Measure"]');
    const drawBtn = document.querySelector('[title="Draw Area"]');
    
    if (placeTokenBtn) placeTokenBtn.addEventListener('click', () => setToolMode('place'));
    if (selectBtn) selectBtn.addEventListener('click', () => setToolMode('select'));
    if (measureBtn) measureBtn.addEventListener('click', () => setToolMode('measure'));
    if (drawBtn) drawBtn.addEventListener('click', () => setToolMode('draw'));
    
    // Map tools buttons
    const newMapBtn = document.getElementById('newMapBtn');
    const loadMapBtn = document.getElementById('loadMapBtn');
    const saveMapBtn = document.getElementById('saveMapBtn');
    const terrainEditorBtn = document.getElementById('terrainEditorBtn');
    
    if (newMapBtn) newMapBtn.addEventListener('click', showNewMapModal);
    if (loadMapBtn) loadMapBtn.addEventListener('click', showLoadMapModal);
    if (saveMapBtn) saveMapBtn.addEventListener('click', showSaveMapModal);
    if (terrainEditorBtn) terrainEditorBtn.addEventListener('click', toggleTerrainEditor);
    
    // Map tools panel toggle
    const layersPanelBtn = document.getElementById('layersBtn');
    if (layersPanelBtn) {
        layersPanelBtn.addEventListener('click', function() {
            toggleMapToolsPanel('layers');
        });
    }
    
    // Terrain editor functionality
    initializeTerrainEditor();
    
    // Map layers functionality
    initializeMapLayers();
}

// Toggle fullscreen mode
function toggleFullscreen() {
    if (!arenaState.isFullscreen) {
        enterFullscreenMode();
    } else {
        exitFullscreenMode();
    }
}

// Enhanced fullscreen functionality
function enterFullscreenMode() {
    const element = document.documentElement;
    
    console.log('Entering fullscreen mode for entire window');
    
    if (element.requestFullscreen) {
        element.requestFullscreen().then(() => {
            arenaState.isFullscreen = true;
            updateFullscreenUI(true);
            hideSidebarsInFullscreen();
            showNotification('Entered fullscreen mode - Sidebars hidden', 'success');
            console.log('Fullscreen mode activated');
        }).catch(err => {
            console.error('Error entering fullscreen:', err);
            showNotification('Failed to enter fullscreen mode: ' + err.message, 'error');
        });
    } else if (element.webkitRequestFullscreen) {
        element.webkitRequestFullscreen();
        arenaState.isFullscreen = true;
        updateFullscreenUI(true);
        hideSidebarsInFullscreen();
        showNotification('Entered fullscreen mode - Sidebars hidden', 'success');
    } else if (element.msRequestFullscreen) {
        element.msRequestFullscreen();
        arenaState.isFullscreen = true;
        updateFullscreenUI(true);
        hideSidebarsInFullscreen();
        showNotification('Entered fullscreen mode - Sidebars hidden', 'success');
    } else {
        console.error('Fullscreen API not supported');
        showNotification('Fullscreen not supported in this browser', 'error');
    }
}

function exitFullscreenMode() {
    if (document.exitFullscreen) {
        document.exitFullscreen().then(() => {
            arenaState.isFullscreen = false;
            updateFullscreenUI(false);
            showSidebarsInWindowed();
            showNotification('Exited fullscreen mode - Sidebars restored', 'info');
        }).catch(err => {
            console.error('Error exiting fullscreen:', err);
        });
    } else if (document.webkitExitFullscreen) {
        document.webkitExitFullscreen();
        arenaState.isFullscreen = false;
        updateFullscreenUI(false);
        showSidebarsInWindowed();
    } else if (document.msExitFullscreen) {
        document.msExitFullscreen();
        arenaState.isFullscreen = false;
        updateFullscreenUI(false);
        showSidebarsInWindowed();
    }
}

function hideSidebarsInFullscreen() {
    // Keep left sidebar open in fullscreen, only hide right sidebar
    const leftSidebar = document.querySelector('.arena-sidebar-left');
    const rightSidebar = document.querySelector('.arena-sidebar-right');
    
    // Keep left sidebar visible for easy navigation
    if (leftSidebar) {
        leftSidebar.style.display = 'block';
        leftSidebar.style.width = '300px'; // Slightly wider for better navigation
    }
    
    // Hide right sidebar to give more space to map
    if (rightSidebar) {
        rightSidebar.style.display = 'none';
    }
    
    // Adjust map to use remaining space
    const mapContainer = document.querySelector('.real-map-container');
    if (mapContainer) {
        mapContainer.style.width = 'calc(100vw - 300px)';
        mapContainer.style.left = '300px';
    }
}

function showSidebarsInWindowed() {
    // Show sidebars when exiting fullscreen
    const leftSidebar = document.querySelector('.arena-sidebar-left');
    const rightSidebar = document.querySelector('.arena-sidebar-right');
    
    if (leftSidebar) {
        leftSidebar.style.display = 'block';
    }
    if (rightSidebar) {
        rightSidebar.style.display = 'block';
    }
    
    // Restore map width
    const mapContainer = document.querySelector('.real-map-container');
    if (mapContainer) {
        mapContainer.style.width = 'calc(100% - 480px)';
        mapContainer.style.left = '240px';
    }
}

function updateFullscreenUI(isFullscreen) {
    const fullscreenBtn = document.getElementById('fullscreenBtn');
    if (fullscreenBtn) {
        if (isFullscreen) {
            fullscreenBtn.classList.add('active');
            fullscreenBtn.innerHTML = '<i class="bi bi-fullscreen-exit"></i>';
        } else {
            fullscreenBtn.classList.remove('active');
            fullscreenBtn.innerHTML = '<i class="bi bi-fullscreen"></i>';
        }
    }
    
    // Hide/show sidebars based on fullscreen state
    const leftSidebar = document.querySelector('.arena-sidebar-left');
    const rightSidebar = document.querySelector('.arena-sidebar-right');
    
    if (isFullscreen) {
        if (leftSidebar) leftSidebar.style.display = 'none';
        if (rightSidebar) rightSidebar.style.display = 'none';
    } else {
        if (leftSidebar) leftSidebar.style.display = 'block';
        if (rightSidebar) rightSidebar.style.display = 'block';
    }
}

// Add game play interface
function addGamePlayInterface() {
    const gamePlayHtml = `
        <div class="game-play-interface" id="gamePlayInterface">
            <div class="play-controls">
                <span class="game-status">Ready to Play</span>
                <button class="play-btn" id="startGameBtn">Start Game</button>
                <button class="play-btn" id="pauseGameBtn" disabled>Pause</button>
                <button class="play-btn" id="endGameBtn" disabled>End Game</button>
                <button class="play-btn" id="debugBtn" onclick="testArenaSystems()" style="background: #f39c12;">Debug</button>
            </div>
        </div>
    `;
    
    document.body.insertAdjacentHTML('beforeend', gamePlayHtml);
    
    // Add event listeners
    document.getElementById('startGameBtn').addEventListener('click', startGame);
    document.getElementById('pauseGameBtn').addEventListener('click', pauseGame);
    document.getElementById('endGameBtn').addEventListener('click', endGame);
}

// Game control functions
function startGame() {
    showNotification('Game started!', 'success');
    document.getElementById('startGameBtn').disabled = true;
    document.getElementById('pauseGameBtn').disabled = false;
    document.getElementById('endGameBtn').disabled = false;
    
    // Update status
    const status = document.querySelector('.game-status');
    if (status) status.textContent = 'Game in Progress';
}

function pauseGame() {
    showNotification('Game paused', 'info');
    document.getElementById('pauseGameBtn').disabled = true;
    document.getElementById('startGameBtn').disabled = false;
    
    // Update status
    const status = document.querySelector('.game-status');
    if (status) status.textContent = 'Game Paused';
}

function endGame() {
    showNotification('Game ended', 'info');
    document.getElementById('startGameBtn').disabled = false;
    document.getElementById('pauseGameBtn').disabled = true;
    document.getElementById('endGameBtn').disabled = true;
    
    // Update status
    const status = document.querySelector('.game-status');
    if (status) status.textContent = 'Game Ended';
}

// Reset map
function resetMap() {
    if (confirm('Are you sure you want to reset the map? This will clear all tokens and features.')) {
        // Clear all tokens and features
        document.querySelectorAll('.token-modern, .feature-modern').forEach(element => {
            element.remove();
        });
        
        // Reset terrain to clear
        document.querySelectorAll('.hex-modern').forEach(hex => {
            hex.className = 'hex-modern terrain-clear';
        });
        
        showNotification('Map reset successfully', 'success');
    }
}

// Toggle coordinate display
function toggleCoordinates() {
    const hexLabels = document.querySelectorAll('.hex-label');
    const isVisible = hexLabels[0] && hexLabels[0].style.display !== 'none';
    
    hexLabels.forEach(label => {
        label.style.display = isVisible ? 'none' : 'flex';
    });
    
    showNotification(`Coordinates ${isVisible ? 'hidden' : 'shown'}`, 'info');
}

// Set tool mode
function setToolMode(mode) {
    // Remove active class from all tool buttons
    document.querySelectorAll('.control-btn').forEach(btn => {
        btn.classList.remove('active');
    });
    
    // Add active class to selected tool
    const toolButtons = {
        'place': 'placeTokenBtn',
        'select': 'selectBtn',
        'measure': '[title="Measure"]',
        'draw': '[title="Draw Area"]'
    };
    
    const selectedButton = document.querySelector(toolButtons[mode]);
    if (selectedButton) {
        selectedButton.classList.add('active');
    }
    
    showNotification(`Switched to ${mode} mode`, 'info');
}

// Toggle map layers
function toggleMapLayers() {
    const layersPanel = document.getElementById('mapLayersPanel');
    const toolsPanel = document.getElementById('mapToolsPanel');
    const terrainPanel = document.getElementById('terrainEditorPanel');
    
    // Hide other panels
    if (toolsPanel) toolsPanel.classList.remove('active');
    if (terrainPanel) terrainPanel.classList.remove('active');
    
    // Toggle layers panel
    if (layersPanel) {
        layersPanel.classList.toggle('active');
        const isActive = layersPanel.classList.contains('active');
        showNotification(`Map layers ${isActive ? 'opened' : 'closed'}`, 'info');
    }
}

// Initialize team management
function initializeTeamManagement() {
    // Team selection functionality
    const teamSelectors = document.querySelectorAll('.team-info');
    teamSelectors.forEach(selector => {
        selector.addEventListener('click', function() {
            showTeamSelectionModal();
        });
    });
}

// Show team selection modal
function showTeamSelectionModal() {
    // Clean up any existing modals first
    cleanupModalBackdrop();
    
    const modal = document.createElement('div');
    modal.className = 'modal fade show';
    modal.style.display = 'block';
    modal.innerHTML = `
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title">Select Team</h5>
                    <button type="button" class="btn-close" onclick="this.closest('.modal').remove(); cleanupModalBackdrop();"></button>
                </div>
                <div class="modal-body">
                    <div class="team-selection-grid">
                        <div class="team-option" data-team="alpha">
                            <div class="team-icon-large" style="background: linear-gradient(135deg, #27ae60, #2ecc71);">
                                <i class="bi bi-shield-fill"></i>
                            </div>
                            <h6>Alpha Team</h6>
                            <p>Primary assault force</p>
                        </div>
                        <div class="team-option" data-team="beta">
                            <div class="team-icon-large" style="background: linear-gradient(135deg, #3498db, #5dade2);">
                                <i class="bi bi-shield-fill"></i>
                            </div>
                            <h6>Beta Team</h6>
                            <p>Support and defense</p>
                        </div>
                        <div class="team-option" data-team="gamma">
                            <div class="team-icon-large" style="background: linear-gradient(135deg, #e74c3c, #ec7063);">
                                <i class="bi bi-shield-fill"></i>
                            </div>
                            <h6>Gamma Team</h6>
                            <p>Special operations</p>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    `;
    
    document.body.appendChild(modal);
    
    // Add click handlers for team selection
    modal.querySelectorAll('.team-option').forEach(option => {
        option.addEventListener('click', function() {
            const team = this.dataset.team;
            selectTeam(team);
            modal.remove();
            cleanupModalBackdrop();
        });
    });
}

// Select team
function selectTeam(team) {
    arenaState.selectedTeam = team;
    
    // Update team display
    const teamName = document.querySelector('.team-name');
    const teamIcon = document.querySelector('.logo-icon');
    
    if (teamName) teamName.textContent = `${team.toUpperCase()} Team`;
    
    // Update team colors
    const teamColors = {
        'alpha': 'linear-gradient(135deg, #27ae60, #2ecc71)',
        'beta': 'linear-gradient(135deg, #3498db, #5dade2)',
        'gamma': 'linear-gradient(135deg, #e74c3c, #ec7063)'
    };
    
    if (teamIcon) {
        teamIcon.style.background = teamColors[team];
    }
    
    showNotification(`Joined ${team.toUpperCase()} Team`, 'success');
}

// Initialize token system
function initializeTokenSystem() {
    const tokenSelect = document.getElementById('tokenSelect');
    if (tokenSelect) {
        tokenSelect.addEventListener('change', function() {
            const tokenId = this.value;
            selectToken(tokenId);
        });
    }
}

// Select token for placement
function selectToken(tokenId) {
    arenaState.selectedToken = tokenId;
    
    console.log('Token selected:', tokenId);
    
    const selectedDisplay = document.getElementById('selectedTokenDisplay');
    if (selectedDisplay) {
        if (tokenId) {
            const selectedOption = document.querySelector(`#tokenSelect option[value="${tokenId}"]`);
            const serial = selectedOption ? selectedOption.dataset.serial : 'Unknown';
            
            selectedDisplay.innerHTML = `
                <span class="badge bg-primary">
                    <i class="bi bi-shapes"></i> ${serial}
                </span>
            `;
            
            showNotification(`Token selected! Click anywhere on the map to place it.`, 'success');
        } else {
            selectedDisplay.innerHTML = '<span class="text-muted">No token selected</span>';
            showNotification('Token selection cleared', 'info');
        }
    }
}

// Initialize fullscreen mode
function initializeFullscreenMode() {
    // Listen for fullscreen change events
    document.addEventListener('fullscreenchange', handleFullscreenChange);
    document.addEventListener('webkitfullscreenchange', handleFullscreenChange);
    document.addEventListener('mozfullscreenchange', handleFullscreenChange);
    document.addEventListener('MSFullscreenChange', handleFullscreenChange);
}

// Handle fullscreen change
function handleFullscreenChange() {
    const isFullscreen = !!(document.fullscreenElement || document.webkitFullscreenElement || 
                           document.mozFullScreenElement || document.msFullscreenElement);
    
    arenaState.isFullscreen = isFullscreen;
    
    const arenaContainer = document.querySelector('.game-arena-container');
    if (isFullscreen) {
        arenaContainer.classList.add('fullscreen-mode');
        const leftSidebar = document.querySelector('.arena-sidebar-left');
        const rightSidebar = document.querySelector('.arena-sidebar-right');
        const footer = document.querySelector('.arena-footer');
        
        if (leftSidebar) leftSidebar.style.display = 'none';
        if (rightSidebar) rightSidebar.style.display = 'none';
        if (footer) footer.style.display = 'none';
    } else {
        arenaContainer.classList.remove('fullscreen-mode');
        const leftSidebar = document.querySelector('.arena-sidebar-left');
        const rightSidebar = document.querySelector('.arena-sidebar-right');
        const footer = document.querySelector('.arena-footer');
        
        if (leftSidebar) leftSidebar.style.display = 'block';
        if (rightSidebar) rightSidebar.style.display = 'block';
        if (footer) footer.style.display = 'flex';
    }
}

// Load arena assets
function loadArenaAssets() {
    // Load additional CSS for animations
    const style = document.createElement('style');
    style.textContent = `
        @keyframes float {
            0% { transform: translateY(100vh) rotate(0deg); opacity: 0; }
            10% { opacity: 1; }
            90% { opacity: 1; }
            100% { transform: translateY(-100vh) rotate(360deg); opacity: 0; }
        }
        
        @keyframes rain {
            0% { transform: translateY(-20px); }
            100% { transform: translateY(100vh); }
        }
        
        .fullscreen-mode {
            height: 100vh !important;
        }
        
        .fullscreen-mode .arena-main {
            height: 100vh !important;
        }
        
        .team-selection-grid {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
            gap: 20px;
            margin: 20px 0;
        }
        
        .team-option {
            text-align: center;
            padding: 20px;
            border: 2px solid transparent;
            border-radius: 12px;
            cursor: pointer;
            transition: all 0.3s ease;
            background: rgba(255, 255, 255, 0.05);
        }
        
        .team-option:hover {
            border-color: #3498db;
            background: rgba(52, 152, 219, 0.1);
            transform: translateY(-5px);
        }
        
        .team-icon-large {
            width: 80px;
            height: 80px;
            border-radius: 50%;
            display: flex;
            align-items: center;
            justify-content: center;
            font-size: 40px;
            color: white;
            margin: 0 auto 15px;
        }
        
        .loading-container {
            text-align: center;
        }
        
        .loading-logo {
            font-size: 80px;
            color: #3498db;
            margin-bottom: 30px;
            animation: pulse 2s infinite;
        }
        
        .loading-text h2 {
            margin-bottom: 15px;
            color: #ecf0f1;
        }
        
        .loading-text p {
            color: #bdc3c7;
            margin-bottom: 30px;
        }
        
        .loading-progress {
            width: 300px;
            height: 4px;
            background: rgba(255, 255, 255, 0.1);
            border-radius: 2px;
            overflow: hidden;
            margin: 0 auto;
        }
        
        .progress-bar {
            height: 100%;
            background: linear-gradient(90deg, #3498db, #2ecc71);
            border-radius: 2px;
            animation: loading 2s ease-in-out infinite;
        }
        
        @keyframes loading {
            0% { width: 0%; }
            50% { width: 70%; }
            100% { width: 100%; }
        }
        
        @keyframes pulse {
            0% { transform: scale(1); }
            50% { transform: scale(1.1); }
            100% { transform: scale(1); }
        }
    `;
    
    document.head.appendChild(style);
}

// Setup event listeners
function setupEventListeners() {
    // Hex click handlers
    document.addEventListener('click', function(e) {
        if (e.target.classList.contains('hex-modern') || e.target.closest('.hex-modern')) {
            const hexElement = e.target.classList.contains('hex-modern') ? e.target : e.target.closest('.hex-modern');
            handleHexClick(hexElement);
        }
    });
    
    // Token interaction handlers
    document.addEventListener('click', function(e) {
        if (e.target.classList.contains('token-modern')) {
            e.stopPropagation();
            handleTokenClick(e.target);
        }
    });
    
    // Feature interaction handlers
    document.addEventListener('click', function(e) {
        if (e.target.classList.contains('feature-modern')) {
            e.stopPropagation();
            handleFeatureClick(e.target);
        }
    });
}

// Handle hex click
function handleHexClick(hexElement) {
    if (arenaState.currentRole === 'spectator') {
        showNotification('Spectator mode - no interactions allowed', 'warning');
        return;
    }
    
    if (arenaState.selectedToken && arenaState.currentRole === 'tokens') {
        placeTokenOnHex(hexElement);
    } else {
        selectHex(hexElement);
    }
}

// Place token on hex
function placeTokenOnHex(hexElement) {
    const hexId = hexElement.dataset.hexid;
    const q = parseInt(hexElement.dataset.q);
    const r = parseInt(hexElement.dataset.r);
    const tokenId = arenaState.selectedToken;
    
    if (tokenId && hexId && !isNaN(q) && !isNaN(r)) {
        const tokenOption = document.querySelector(`#tokenSelect option[value="${tokenId}"]`);
        const tokenIcon = tokenOption ? tokenOption.dataset.icon : 'fa-solid fa-circle';
        const tokenColor = tokenOption ? tokenOption.dataset.color : '#e74c3c';
        const tokenSerial = tokenOption ? tokenOption.dataset.serial : 'TKN';
        const placementId = Date.now().toString();
        
        // Check if real map system is ready
        if (window.realMapArena && window.realMapArena.isInitialized && window.getHexCenterLatLng && window.placeTokenOnMap) {
            console.log('Using real map system for token placement');
            const latlng = window.getHexCenterLatLng(q, r);
            if (latlng) {
                // Place token directly on the real map for immediate local feedback
                window.placeTokenOnMap(latlng.lat, latlng.lng, hexElement, tokenId, placementId, tokenIcon, tokenColor, tokenSerial);
                console.log('Token placement called successfully');
            } else {
                console.error('Could not get LatLng for hex:', q, r);
                showNotification('Failed to place token: Invalid map coordinates.', 'error');
                return;
            }
        } else {
            console.log('Real map system not ready, using fallback');
            console.log('realMapArena:', window.realMapArena);
            console.log('isInitialized:', window.realMapArena ? window.realMapArena.isInitialized : 'undefined');
            console.log('getHexCenterLatLng:', typeof window.getHexCenterLatLng);
            console.log('placeTokenOnMap:', typeof window.placeTokenOnMap);
            
            // Fallback for hidden hex grid if real map system is not fully initialized
            const tokenElement = document.createElement('div');
            tokenElement.className = 'token-visual';
            tokenElement.setAttribute('data-token-id', tokenId);
            tokenElement.setAttribute('data-placement-id', placementId);
            tokenElement.innerHTML = `<i class="${tokenIcon}" style="color: ${tokenColor};"></i>`;
            tokenElement.title = `Token: ${tokenSerial}`;
            
            const overlay = hexElement.querySelector('.hex-overlay');
            if (overlay) {
                overlay.appendChild(tokenElement);
            }
            showNotification('Token placed on hidden hex grid (real map not ready).', 'warning');
        }
        
        // Clear token selection
        arenaState.selectedToken = null;
        const tokenSelect = document.getElementById('tokenSelect');
        if (tokenSelect) tokenSelect.value = '';
        
        // Trigger real-time update for other clients
        if (window.placeTokenRealTime) {
            window.placeTokenRealTime(hexElement, placementId, tokenId, tokenIcon, tokenColor, tokenSerial);
        }
    } else {
        showNotification('Please select a token and a hex to place it.', 'warning');
    }
}

// Get token team
function getTokenTeam(tokenId) {
    // This would typically come from your token data
    // For now, return based on selected team or token ID
    if (arenaState.selectedTeam) {
        return arenaState.selectedTeam;
    }
    return 'neutral';
}

// Select hex
function selectHex(hexElement) {
    // Remove previous selection
    document.querySelectorAll('.hex-modern').forEach(h => h.classList.remove('selected'));
    
    // Add selection to current hex
    hexElement.classList.add('selected');
    
    const q = hexElement.dataset.q;
    const r = hexElement.dataset.r;
    showNotification(`Selected hex (${q}, ${r})`, 'info');
}

// Handle token click
function handleTokenClick(tokenElement) {
    const placementId = tokenElement.dataset.placementId;
    const tokenId = tokenElement.dataset.tokenId;
    
    // Show token options
    showTokenOptions(placementId, tokenElement);
}

// Show token options
function showTokenOptions(placementId, tokenElement) {
    const options = confirm('Remove this token?');
    if (options) {
        tokenElement.remove();
        showNotification('Token removed', 'success');
    }
}

// Handle feature click
function handleFeatureClick(featureElement) {
    const featureId = featureElement.dataset.featureId;
    
    // Show feature options
    const options = confirm('Remove this feature?');
    if (options) {
        featureElement.remove();
        showNotification('Feature removed', 'success');
    }
}

// Enhanced notification system
function showNotification(message, type = 'info') {
    // Remove existing notifications
    const existingNotifications = document.querySelectorAll('.notification-toast');
    existingNotifications.forEach(n => n.remove());
    
    // Create notification element
    const notification = document.createElement('div');
    notification.className = `notification-toast alert alert-${type === 'error' ? 'danger' : type} alert-dismissible fade show`;
    notification.style.cssText = `
        position: fixed;
        top: 20px;
        right: 20px;
        z-index: 9999;
        min-width: 300px;
        box-shadow: 0 4px 6px rgba(0,0,0,0.1);
        border: none;
        border-radius: 8px;
    `;
    
    const iconMap = {
        'success': 'bi-check-circle-fill',
        'error': 'bi-exclamation-triangle-fill',
        'warning': 'bi-exclamation-circle-fill',
        'info': 'bi-info-circle-fill'
    };
    
    notification.innerHTML = `
        <div class="d-flex align-items-center">
            <i class="bi ${iconMap[type]} me-2"></i>
            <span>${message}</span>
            <button type="button" class="btn-close ms-auto" data-bs-dismiss="alert"></button>
        </div>
    `;
    
    // Add to page
    document.body.appendChild(notification);
    
    // Auto-remove after 5 seconds
    setTimeout(() => {
        if (notification.parentNode) {
            notification.remove();
        }
    }, 5000);
}

// Map Tools Functions
function toggleMapToolsPanel(panel) {
    const toolsPanel = document.getElementById('mapToolsPanel');
    const layersPanel = document.getElementById('mapLayersPanel');
    const terrainPanel = document.getElementById('terrainEditorPanel');
    
    // Hide all panels first
    if (toolsPanel) toolsPanel.classList.remove('active');
    if (layersPanel) layersPanel.classList.remove('active');
    if (terrainPanel) terrainPanel.classList.remove('active');
    
    // Show selected panel
    switch(panel) {
        case 'tools':
            if (toolsPanel) toolsPanel.classList.add('active');
            break;
        case 'layers':
            if (layersPanel) layersPanel.classList.add('active');
            break;
        case 'terrain':
            if (terrainPanel) terrainPanel.classList.add('active');
            break;
    }
}

function showNewMapModal() {
    const modal = new bootstrap.Modal(document.getElementById('newMapModal'));
    modal.show();
    
    // Setup form submission
    document.getElementById('createMapBtn').addEventListener('click', function() {
        createNewMap();
    });
}

function showLoadMapModal() {
    const modal = new bootstrap.Modal(document.getElementById('loadMapModal'));
    modal.show();
    
    // Setup map loading
    document.querySelectorAll('.map-item').forEach(item => {
        item.addEventListener('click', function() {
            const mapId = this.dataset.mapId;
            loadMap(mapId);
            modal.hide();
        });
    });
}

function showSaveMapModal() {
    const modal = new bootstrap.Modal(document.getElementById('saveMapModal'));
    modal.show();
    
    // Setup form submission
    document.getElementById('saveMapBtn').addEventListener('click', function() {
        saveCurrentMap();
    });
}

function toggleTerrainEditor() {
    const terrainPanel = document.getElementById('terrainEditorPanel');
    const toolsPanel = document.getElementById('mapToolsPanel');
    const layersPanel = document.getElementById('mapLayersPanel');
    
    // Hide other panels
    if (toolsPanel) toolsPanel.classList.remove('active');
    if (layersPanel) layersPanel.classList.remove('active');
    
    // Toggle terrain panel
    if (terrainPanel) {
        terrainPanel.classList.toggle('active');
        const isActive = terrainPanel.classList.contains('active');
        showNotification(`Terrain editor ${isActive ? 'opened' : 'closed'}`, 'info');
    }
}

function initializeTerrainEditor() {
    const terrainOptions = document.querySelectorAll('.terrain-option');
    terrainOptions.forEach(option => {
        option.addEventListener('click', function() {
            // Remove selection from all options
            terrainOptions.forEach(opt => opt.classList.remove('selected'));
            
            // Add selection to clicked option
            this.classList.add('selected');
            
            // Set current terrain type
            const terrainType = this.dataset.terrain;
            arenaState.selectedTerrain = terrainType;
            
            showNotification(`Selected terrain: ${terrainType}`, 'info');
        });
    });
}

function initializeMapLayers() {
    // Layer visibility toggles
    document.querySelectorAll('.layer-visibility').forEach(toggle => {
        toggle.addEventListener('click', function() {
            this.classList.toggle('visible');
            this.classList.toggle('hidden');
            
            const layer = this.dataset.layer;
            const isVisible = this.classList.contains('visible');
            
            // Toggle layer visibility
            toggleLayerVisibility(layer, isVisible);
            
            showNotification(`${layer} layer ${isVisible ? 'shown' : 'hidden'}`, 'info');
        });
    });
    
    // Layer opacity sliders
    document.querySelectorAll('.layer-opacity').forEach(slider => {
        slider.addEventListener('click', function(e) {
            const rect = this.getBoundingClientRect();
            const x = e.clientX - rect.left;
            const percentage = (x / rect.width) * 100;
            
            const opacitySlider = this.querySelector('.layer-opacity-slider');
            opacitySlider.style.width = percentage + '%';
            
            const layer = this.dataset.layer;
            setLayerOpacity(layer, percentage / 100);
            
            showNotification(`${layer} layer opacity: ${Math.round(percentage)}%`, 'info');
        });
    });
}

function createNewMap() {
    const mapName = document.getElementById('mapName').value;
    const mapSize = document.getElementById('mapSize').value;
    const mapTheme = document.getElementById('mapTheme').value;
    const mapDescription = document.getElementById('mapDescription').value;
    
    if (!mapName) {
        showNotification('Please enter a map name', 'warning');
        return;
    }
    
    // Create new map based on parameters
    const size = mapSize.split('x').map(Number);
    const width = size[0];
    const height = size[1];
    
    // Generate terrain based on theme
    generateMapTerrain(width, height, mapTheme);
    
    showNotification(`Created new map: ${mapName}`, 'success');
    
    // Close modal
    const modal = bootstrap.Modal.getInstance(document.getElementById('newMapModal'));
    modal.hide();
}

function loadMap(mapId) {
    // Simulate loading a map
    showNotification(`Loading map ${mapId}...`, 'info');
    
    // In a real implementation, this would load the map data from the server
    setTimeout(() => {
        showNotification('Map loaded successfully', 'success');
    }, 1000);
}

function saveCurrentMap() {
    const mapName = document.getElementById('saveMapName').value;
    const mapDescription = document.getElementById('saveMapDescription').value;
    const includeTokens = document.getElementById('includeTokens').checked;
    const includeFeatures = document.getElementById('includeFeatures').checked;
    
    if (!mapName) {
        showNotification('Please enter a map name', 'warning');
        return;
    }
    
    // Collect current map state
    const mapData = {
        name: mapName,
        description: mapDescription,
        includeTokens: includeTokens,
        includeFeatures: includeFeatures,
        hexes: Array.from(document.querySelectorAll('.hex-modern')).map(hex => ({
            id: hex.dataset.hexid,
            q: hex.dataset.q,
            r: hex.dataset.r,
            terrain: Array.from(hex.classList).find(cls => cls.startsWith('terrain-')) || 'terrain-clear',
            tokens: Array.from(hex.querySelectorAll('.token-modern')).map(token => ({
                id: token.dataset.placementId,
                tokenId: token.dataset.tokenId,
                serial: token.textContent
            })),
            features: Array.from(hex.querySelectorAll('.feature-modern')).map(feature => ({
                id: feature.dataset.featureId,
                text: feature.textContent
            }))
        }))
    };
    
    // In a real implementation, this would save to the server
    console.log('Saving map data:', mapData);
    
    showNotification(`Map "${mapName}" saved successfully`, 'success');
    
    // Close modal
    const modal = bootstrap.Modal.getInstance(document.getElementById('saveMapModal'));
    modal.hide();
}

function generateMapTerrain(width, height, theme) {
    const hexGrid = document.getElementById('hexGrid');
    if (!hexGrid) return;
    
    // Clear existing hexes
    hexGrid.innerHTML = '';
    
    // Generate new hexes
    for (let q = 0; q < width; q++) {
        for (let r = 0; r < height; r++) {
            const hex = document.createElement('div');
            hex.className = `hex-modern ${getTerrainForTheme(theme)}`;
            hex.setAttribute('data-hexid', `${q}-${r}`);
            hex.setAttribute('data-q', q);
            hex.setAttribute('data-r', r);
            hex.setAttribute('onclick', 'onHexClick(this)');
            hex.setAttribute('title', `Hex (${q},${r})`);
            
            const label = document.createElement('div');
            label.className = 'hex-label';
            label.textContent = `${q},${r}`;
            hex.appendChild(label);
            
            const overlay = document.createElement('div');
            overlay.className = 'hex-overlay';
            overlay.id = `overlay-${q}-${r}`;
            hex.appendChild(overlay);
            
            hexGrid.appendChild(hex);
        }
    }
    
    // Update grid template
    hexGrid.style.gridTemplateColumns = `repeat(${width}, 60px)`;
    
    showNotification(`Generated ${width}x${height} map with ${theme} theme`, 'success');
}

function getTerrainForTheme(theme) {
    const terrainMap = {
        'forest': 'terrain-forest',
        'desert': 'terrain-desert',
        'urban': 'terrain-urban',
        'mountain': 'terrain-mountain',
        'mixed': () => {
            const terrains = ['terrain-clear', 'terrain-forest', 'terrain-mountain', 'terrain-water', 'terrain-desert', 'terrain-urban', 'terrain-swamp'];
            return terrains[Math.floor(Math.random() * terrains.length)];
        }
    };
    
    const terrain = terrainMap[theme];
    return typeof terrain === 'function' ? terrain() : terrain;
}

function toggleLayerVisibility(layer, isVisible) {
    const elements = document.querySelectorAll(`[data-layer="${layer}"]`);
    elements.forEach(element => {
        element.style.display = isVisible ? 'block' : 'none';
    });
}

function setLayerOpacity(layer, opacity) {
    const elements = document.querySelectorAll(`[data-layer="${layer}"]`);
    elements.forEach(element => {
        element.style.opacity = opacity;
    });
}

// ========================================
// GROUP MANAGEMENT SYSTEM
// ========================================

// Initialize comprehensive group management system
function initializeGroupManagement() {
    console.log('🎖️ Initializing Military Group Management System...');
    
    // Add group management to data entry panel
    const dataEntryPanel = document.getElementById('dataEntryPanel');
    if (dataEntryPanel) {
        addGroupManagementToPanel(dataEntryPanel);
    }
    
    // Load existing groups
    loadUserGroups();
    loadTokenGroups();
    
    console.log('✓ Group Management System initialized');
}

// SIMPLE MILITARY INTERFACE
function addGroupManagementToPanel(panel) {
    const groupManagementHtml = `
        <div class="military-section">
            <h5>🎖️ UNITS</h5>
            <div class="simple-list">
                <div class="unit-item" onclick="selectUnit('infantry')">
                    <span>🪖 Infantry</span>
                </div>
                <div class="unit-item" onclick="selectUnit('armor')">
                    <span>🚗 Armor</span>
                </div>
                <div class="unit-item" onclick="selectUnit('support')">
                    <span>🚁 Support</span>
                </div>
            </div>
        </div>
        
        <div class="military-section">
            <h5>🏗️ FEATURES</h5>
            <div class="simple-list">
                <div class="feature-item" onclick="selectFeature('fortification')">
                    <span>🛡️ Fortification</span>
                </div>
                <div class="feature-item" onclick="selectFeature('obstacle')">
                    <span>🚧 Obstacle</span>
                </div>
                <div class="feature-item" onclick="selectFeature('objective')">
                    <span>🎯 Objective</span>
                </div>
                <div class="feature-item remove-item" onclick="setRemoveMode('feature')">
                    <span>🗑️ Remove Features</span>
                </div>
            </div>
        </div>
        
        <div class="military-section">
            <h5>🌍 TERRAIN</h5>
            <div class="simple-list">
                <div class="terrain-item" onclick="selectTerrain('clear')">
                    <span>🌱 Clear</span>
                </div>
                <div class="terrain-item" onclick="selectTerrain('forest')">
                    <span>🌲 Forest</span>
                </div>
                <div class="terrain-item" onclick="selectTerrain('mountain')">
                    <span>⛰️ Mountain</span>
                </div>
                <div class="terrain-item" onclick="selectTerrain('water')">
                    <span>💧 Water</span>
                </div>
                <div class="terrain-item remove-item" onclick="setRemoveMode('terrain')">
                    <span>🗑️ Remove Terrain</span>
                </div>
            </div>
        </div>
        
        <div class="military-section">
            <h5>🛠️ GAME TOOLS</h5>
            <div class="simple-list">
                <div class="tool-item tool-btn" data-tool="draw" onclick="setGameTool('draw')">
                    <span>✏️ Draw Area</span>
                </div>
                <div class="tool-item tool-btn" data-tool="measure" onclick="setGameTool('measure')">
                    <span>📏 Measure</span>
                </div>
                <div class="tool-item tool-btn" data-tool="select" onclick="setGameTool('select')">
                    <span>🎯 Select</span>
                </div>
                <div class="tool-item tool-btn" data-tool="area" onclick="setGameTool('area')">
                    <span>📐 Fixed Map Area</span>
                </div>
                <div class="tool-item tool-btn" data-tool="maparea" onclick="setGameTool('maparea')">
                    <span>🗺️ Select Map Area</span>
                </div>
            </div>
        </div>
        
        <div class="military-section">
            <h5>🔍 VIEW MODES</h5>
            <div class="simple-list">
                <div class="mode-item" onclick="toggleHexMode()">
                    <span>⬡ Hex Mode</span>
                </div>
                <div class="mode-item" onclick="showAllFeatures()">
                    <span>👁️ Show All Features</span>
                </div>
                <div class="mode-item" onclick="clearMap()">
                    <span>🗑️ Clear Map</span>
                </div>
            </div>
        </div>
        
        <div class="military-section">
            <h5>🌍 LOCATION SEARCH</h5>
            <div class="location-search">
                <input type="text" id="locationSearch" placeholder="Search location (e.g., Dubai, New York)" 
                       class="form-control" style="margin-bottom: 10px;">
                <div class="quick-locations">
                    <div class="location-btn" onclick="goToLocation('Dubai, UAE')">
                        <span>🏙️ Dubai</span>
                    </div>
                    <div class="location-btn" onclick="goToLocation('New York, USA')">
                        <span>🗽 New York</span>
                    </div>
                    <div class="location-btn" onclick="goToLocation('London, UK')">
                        <span>🇬🇧 London</span>
                    </div>
                    <div class="location-btn" onclick="goToLocation('Tokyo, Japan')">
                        <span>🇯🇵 Tokyo</span>
                    </div>
                </div>
            </div>
        </div>
        
        <div class="military-section">
            <h5>👥 TEAMS & PLAYERS</h5>
            <div class="simple-list">
                <div class="team-item">
                    <span>🦊 Fox Land: 2 Players</span>
                </div>
                <div class="team-item">
                    <span>🔵 Blue Land: 1 Player</span>
                </div>
                <div class="team-item">
                    <span>👁️ Spectators: 1 Player</span>
                </div>
                <div class="team-item" onclick="showPlayerDetails()">
                    <span>📋 Player Details</span>
                </div>
            </div>
        </div>
        
        <div class="military-section">
            <h5>📱 SIDEBAR CONTROL</h5>
            <div class="simple-list">
                <div class="control-item" onclick="toggleLeftSidebar()">
                    <span>👈 Toggle Left Sidebar</span>
                </div>
                <div class="control-item" onclick="toggleRightSidebar()">
                    <span>👉 Toggle Right Sidebar</span>
                </div>
                <div class="control-item" onclick="toggleAllSidebars()">
                    <span>🔄 Toggle All Sidebars</span>
                </div>
            </div>
        </div>
    `;
    
    panel.insertAdjacentHTML('beforeend', groupManagementHtml);
}

// Load user groups from server
async function loadUserGroups() {
    try {
        console.log('Loading user groups...');
        // This would typically make an API call to load groups
        // For now, we'll use the static data in the HTML
        console.log('✓ User groups loaded');
    } catch (error) {
        console.error('Error loading user groups:', error);
        showNotification('Failed to load user groups', 'error');
    }
}

// Load token groups from server
async function loadTokenGroups() {
    try {
        console.log('Loading token groups...');
        // This would typically make an API call to load groups
        // For now, we'll use the static data in the HTML
        console.log('✓ Token groups loaded');
    } catch (error) {
        console.error('Error loading token groups:', error);
        showNotification('Failed to load token groups', 'error');
    }
}

// Create new user group
function createUserGroup() {
    showNotification('User Group creation - Feature ready for implementation', 'info');
    console.log('Creating new user group...');
}

// Edit user group
function editUserGroup(groupId) {
    showNotification(`Editing User Group ${groupId} - Feature ready for implementation`, 'info');
    console.log('Editing user group:', groupId);
}

// Create new token group
function createTokenGroup() {
    showNotification('Token Group creation - Feature ready for implementation', 'info');
    console.log('Creating new token group...');
}

// Edit token group
function editTokenGroup(groupId) {
    showNotification(`Editing Token Group ${groupId} - Feature ready for implementation`, 'info');
    console.log('Editing token group:', groupId);
}

// SIMPLE MILITARY FUNCTIONS
function selectUnit(unitType) {
    console.log('Selected unit:', unitType);
    showNotification(`Selected ${unitType} unit`, 'info');
    // Store selection for placement
    window.selectedUnitType = unitType;
}

function selectFeature(featureType) {
    console.log('Selected feature:', featureType);
    showNotification(`Selected ${featureType} - click on map to place`, 'info');
    // Store selection for placement
    window.selectedFeatureType = featureType;
}

function selectTerrain(terrainType) {
    console.log('Selected terrain:', terrainType);
    showNotification(`Selected ${terrainType} - click on hex to change`, 'info');
    // Store selection for placement
    window.selectedTerrainType = terrainType;
}

// SIMPLE SESSION JOINING - MILITARY STYLE
function joinSessionSimple() {
    console.log('Joining session...');
    
    // Create a simple session ID
    const sessionId = 'MILITARY_SESSION_' + Date.now();
    
    // Simulate successful join
    showNotification('Joined Military Session', 'success');
    console.log('Session joined:', sessionId);
    
    // Update UI
    const joinBtn = document.getElementById('joinSessionBtn');
    if (joinBtn) {
        joinBtn.textContent = 'Leave Session';
        joinBtn.onclick = leaveSessionSimple;
    }
    
    // Initialize arena
    initializeArena();
}

function leaveSessionSimple() {
    console.log('Leaving session...');
    
    showNotification('Left Military Session', 'info');
    
    // Update UI
    const joinBtn = document.getElementById('joinSessionBtn');
    if (joinBtn) {
        joinBtn.textContent = 'Join Session';
        joinBtn.onclick = joinSessionSimple;
    }
}

// SIDEBAR CONTROL FUNCTIONS - USER CHOICE
let leftSidebarVisible = true;
let rightSidebarVisible = true;

function toggleLeftSidebar() {
    const leftSidebar = document.querySelector('.arena-sidebar-left');
    if (leftSidebar) {
        leftSidebarVisible = !leftSidebarVisible;
        leftSidebar.style.display = leftSidebarVisible ? 'block' : 'none';
        
        // Adjust map width
        const mapContainer = document.querySelector('.real-map-container');
        if (mapContainer) {
            if (leftSidebarVisible && rightSidebarVisible) {
                mapContainer.style.width = 'calc(100% - 480px)';
                mapContainer.style.left = '240px';
            } else if (leftSidebarVisible || rightSidebarVisible) {
                mapContainer.style.width = 'calc(100% - 240px)';
                mapContainer.style.left = leftSidebarVisible ? '240px' : '0';
            } else {
                mapContainer.style.width = '100vw';
                mapContainer.style.left = '0';
            }
        }
        
        showNotification(`Left Sidebar ${leftSidebarVisible ? 'Shown' : 'Hidden'}`, 'info');
    }
}

function toggleRightSidebar() {
    const rightSidebar = document.querySelector('.arena-sidebar-right');
    if (rightSidebar) {
        rightSidebarVisible = !rightSidebarVisible;
        rightSidebar.style.display = rightSidebarVisible ? 'block' : 'none';
        
        // Adjust map width
        const mapContainer = document.querySelector('.real-map-container');
        if (mapContainer) {
            if (leftSidebarVisible && rightSidebarVisible) {
                mapContainer.style.width = 'calc(100% - 480px)';
                mapContainer.style.left = '240px';
            } else if (leftSidebarVisible || rightSidebarVisible) {
                mapContainer.style.width = 'calc(100% - 240px)';
                mapContainer.style.left = leftSidebarVisible ? '240px' : '0';
            } else {
                mapContainer.style.width = '100vw';
                mapContainer.style.left = '0';
            }
        }
        
        showNotification(`Right Sidebar ${rightSidebarVisible ? 'Shown' : 'Hidden'}`, 'info');
    }
}

function toggleAllSidebars() {
    const leftSidebar = document.querySelector('.arena-sidebar-left');
    const rightSidebar = document.querySelector('.arena-sidebar-right');
    const mapContainer = document.querySelector('.real-map-container');
    
    if (leftSidebar && rightSidebar && mapContainer) {
        const bothVisible = leftSidebarVisible && rightSidebarVisible;
        
        leftSidebarVisible = !bothVisible;
        rightSidebarVisible = !bothVisible;
        
        leftSidebar.style.display = leftSidebarVisible ? 'block' : 'none';
        rightSidebar.style.display = rightSidebarVisible ? 'block' : 'none';
        
        if (bothVisible) {
            // Hide both
            mapContainer.style.width = '100vw';
            mapContainer.style.left = '0';
            showNotification('All Sidebars Hidden - Full Map View', 'info');
        } else {
            // Show both
            mapContainer.style.width = 'calc(100% - 480px)';
            mapContainer.style.left = '240px';
            showNotification('All Sidebars Shown - Normal View', 'info');
        }
    }
}

// PLAYER DETAILS FUNCTION
function showPlayerDetails() {
    // Clean up any existing modals first
    cleanupModalBackdrop();
    
    const playerDetails = `
        <div class="player-details-modal">
            <h4>👥 MILITARY PERSONNEL</h4>
            <div class="player-list">
                <div class="player-item fox-team">
                    <span class="player-name">🦊 Commander Fox</span>
                    <span class="player-role">Fox Land - Commander</span>
                    <span class="player-status online">Online</span>
                </div>
                <div class="player-item fox-team">
                    <span class="player-name">🦊 Lieutenant Alpha</span>
                    <span class="player-role">Fox Land - Tactical Officer</span>
                    <span class="player-status online">Online</span>
                </div>
                <div class="player-item blue-team">
                    <span class="player-name">🔵 Captain Blue</span>
                    <span class="player-role">Blue Land - Field Commander</span>
                    <span class="player-status online">Online</span>
                </div>
                <div class="player-item spectator">
                    <span class="player-name">👁️ Observer One</span>
                    <span class="player-role">Spectator - Intelligence</span>
                    <span class="player-status online">Online</span>
                </div>
            </div>
            <div class="team-summary">
                <h5>📊 TEAM SUMMARY</h5>
                <div class="summary-item">
                    <span>🦊 Fox Land: 2 Active Players</span>
                </div>
                <div class="summary-item">
                    <span>🔵 Blue Land: 1 Active Player</span>
                </div>
                <div class="summary-item">
                    <span>👁️ Spectators: 1 Active Player</span>
                </div>
                <div class="summary-item">
                    <span>📈 Total: 4 Players Online</span>
                </div>
            </div>
        </div>
    `;
    
    // Create modal
    const modal = document.createElement('div');
    modal.className = 'modal fade show';
    modal.style.display = 'block';
    modal.innerHTML = `
        <div class="modal-dialog modal-lg">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title">Military Personnel Status</h5>
                    <button type="button" class="btn-close" onclick="this.closest('.modal').remove(); cleanupModalBackdrop();"></button>
                </div>
                <div class="modal-body">
                    ${playerDetails}
                </div>
            </div>
        </div>
    `;
    
    document.body.appendChild(modal);
    showNotification('Player details displayed', 'info');
}

// Export functions for global access
window.arenaState = arenaState;
window.toggleFullscreen = toggleFullscreen;
window.switchRole = switchRole;
window.cleanupModalBackdrop = cleanupModalBackdrop;
window.selectTeam = selectTeam;
window.toggleMapToolsPanel = toggleMapToolsPanel;
window.selectUnit = selectUnit;
window.selectFeature = selectFeature;
window.selectTerrain = selectTerrain;
window.setRemoveMode = setRemoveMode;
window.setGameTool = setGameTool;
window.toggleHexMode = toggleHexMode;
window.showAllFeatures = showAllFeatures;
window.clearMap = clearMap;
window.goToLocation = goToLocation;
window.showPlayerDetails = showPlayerDetails;
window.toggleLeftSidebar = toggleLeftSidebar;
window.toggleRightSidebar = toggleRightSidebar;
window.toggleAllSidebars = toggleAllSidebars;
window.initializeGroupManagement = initializeGroupManagement;