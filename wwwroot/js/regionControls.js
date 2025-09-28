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
        }
    }, 2000);
});
