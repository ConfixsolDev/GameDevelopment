/**
 * Region Controls - Global functions for region management
 * Provides interface between UI controls and RegionManager
 */

// Global region control functions
window.startRegionDrawing = function() {
    console.log('startRegionDrawing called');
    if (window.gamePlayManager && window.gamePlayManager.regionManager) {
        window.gamePlayManager.regionManager.startDrawingRegion();
    } else {
        console.error('Region manager not available');
        alert('Region manager not available. Please wait for the system to initialize.');
    }
};

window.clearAllRegions = function() {
    console.log('clearAllRegions called');
    if (window.gamePlayManager && window.gamePlayManager.regionManager) {
        window.gamePlayManager.regionManager.clearAllRegions();
    } else {
        console.error('Region manager not available');
        alert('Region manager not available. Please wait for the system to initialize.');
    }
};

window.addMapLabel = function() {
    console.log('addMapLabel called');
    // TODO: Implement map label creation
    alert('Map label creation feature coming soon!');
};

window.clearAllLabels = function() {
    console.log('clearAllLabels called');
    // TODO: Implement label clearing
    alert('Label clearing feature coming soon!');
};

window.selectForce = function(force) {
    console.log('selectForce called:', force);
    if (window.gamePlayManager && window.gamePlayManager.regionManager) {
        window.gamePlayManager.regionManager.selectForce(force);
    } else {
        console.error('Region manager not available');
    }
};

// Initialize region controls when DOM is ready
$(document).ready(function() {
    console.log('Region controls initialized');
    
    // Add some debugging
    setTimeout(() => {
        console.log('GamePlayManager available:', !!window.gamePlayManager);
        if (window.gamePlayManager) {
            console.log('RegionManager available:', !!window.gamePlayManager.regionManager);
            console.log('LabelManager available:', !!window.gamePlayManager.labelManager);
        }
    }, 2000);
});

// Label management functions
window.addMapLabel = function() {
    console.log('addMapLabel called');
    if (window.gamePlayManager && window.gamePlayManager.labelManager && window.gameMap) {
        try {
            // Step 1: user clicks the button → enter placement mode
            window.gamePlayManager.labelManager.startPlacement();
        } catch (e) {
            console.error('Failed to open label modal', e);
            alert('Unable to open label creation modal');
        }
    } else {
        console.warn('Label manager not available, attempting lazy load...');
        try {
            // Try to load LabelManager dynamically
            if (typeof LabelManager === 'undefined') {
                const script = document.createElement('script');
                script.src = '/js/LabelManager.js';
                script.onload = () => {
                    try {
                        if (window.gamePlayManager && window.gameMap) {
                            window.gamePlayManager.labelManager = new LabelManager(window.gameMap);
                            window.gamePlayManager.labelManager.initialize().then(() => {
                                window.gamePlayManager.labelManager.startPlacement();
                            });
                        }
                    } catch (e) {
                        console.error('Label manager loaded but failed to open modal', e);
                        alert('Unable to open label creation modal');
                    }
                };
                script.onerror = () => {
                    console.error('Failed to load LabelManager script');
                    alert('Label manager not available. Please refresh the page.');
                };
                document.head.appendChild(script);
            } else {
                alert('Label manager not available. Please wait for the system to initialize.');
            }
        } catch (e) {
            console.error('Lazy load failed', e);
            alert('Label manager not available. Please wait for the system to initialize.');
        }
    }
};

window.clearAllLabels = function() {
    console.log('clearAllLabels called');
    if (window.gamePlayManager && window.gamePlayManager.labelManager) {
        try {
            const mgr = window.gamePlayManager.labelManager;
            mgr.layer.clearLayers();
            mgr.labelsById.clear();
            console.log('All labels cleared');
        } catch (e) {
            console.error('Failed to clear labels', e);
            alert('Unable to clear labels');
        }
    } else {
        console.error('Label manager not available');
        alert('Label manager not available. Please wait for the system to initialize.');
    }
};
