/**
 * Debug script to force refresh arrows with new curved design
 */

// Force clear all existing arrows and recreate them
window.forceRefreshArrows = function() {
    console.log('🔥 FORCE REFRESHING ARROWS WITH CURVED DESIGN');
    
    // Clear all existing attack lines
    if (window.attackVisualizationManager) {
        console.log('🧹 Clearing all existing attack lines...');
        window.attackVisualizationManager.clearAllAttackLines();
        
        // Wait a moment then reload
        setTimeout(() => {
            console.log('🔄 Reloading attack orders from database...');
            window.attackVisualizationManager.loadAttackOrdersFromDatabase();
        }, 500);
    }
};

// Test creating a single curved arrow
window.testCurvedArrow = function() {
    console.log('🧪 TESTING CURVED ARROW CREATION');
    
    if (!window.attackSymbolRenderer) {
        console.error('❌ AttackSymbolRenderer not found!');
        return;
    }
    
    // Create test points
    const testPath = [
        L.latLng(40.7128, -74.0060), // New York
        L.latLng(40.7589, -73.9851)  // Times Square
    ];
    
    console.log('🎯 Creating test curved arrow...');
    const testArrow = window.attackSymbolRenderer.createAttackLine(testPath, 'attack-main', {
        attackerName: 'Test Unit 01',
        attackerSymbol: '01'
    });
    
    if (testArrow && window.gameMap) {
        console.log('✅ Adding test arrow to map');
        testArrow.addTo(window.gameMap);
        console.log('🎯 Test arrow created and added to map');
    } else {
        console.error('❌ Failed to create test arrow');
    }
};

// Check if everything is loaded
window.checkArrowSystem = function() {
    console.log('🔍 CHECKING ARROW SYSTEM STATUS');
    console.log('AttackVisualizationManager:', !!window.attackVisualizationManager);
    console.log('AttackSymbolRenderer:', !!window.attackSymbolRenderer);
    console.log('Game Map:', !!window.gameMap);
    
    if (window.attackSymbolRenderer) {
        console.log('Available attack types:', window.attackSymbolRenderer.getAvailableAttackTypes());
    }
    
    if (window.attackVisualizationManager) {
        const allAttacks = window.attackVisualizationManager.getAllAttackLines();
        console.log('Current attack lines on map:', allAttacks.length);
    }
};

console.log('🎯 Arrow Debug Script Loaded!');
console.log('Available commands:');
console.log('  forceRefreshArrows() - Clear and reload all arrows');
console.log('  testCurvedArrow() - Create a test curved arrow');
console.log('  checkArrowSystem() - Check system status');
