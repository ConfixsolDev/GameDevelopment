// Token Summary Test - Debug and Fix Token Summary Issues
console.log('🔧 Token Summary Test Script Loaded');

// Test function to manually trigger token summary
function testTokenSummary(tokenId) {
    console.log('🧪 Testing token summary for token ID:', tokenId);
    
    // Check if tokenManager is available
    if (typeof tokenManager === 'undefined') {
        console.error('❌ tokenManager is not defined');
        return false;
    }
    
    if (!tokenManager.showTokenDetails) {
        console.error('❌ tokenManager.showTokenDetails method not available');
        return false;
    }
    
    // Create a test token object
    const testToken = {
        id: tokenId,
        name: 'Test Token',
        forceType: 'Blue'
    };
    
    console.log('✅ Calling tokenManager.showTokenDetails with:', testToken);
    tokenManager.showTokenDetails(testToken);
    return true;
}

// Alternative method to test token summary directly
function testTokenSummaryDirect(tokenId) {
    console.log('🧪 Testing token summary directly for token ID:', tokenId);
    
    $("#loading").show();
    
    // Load the token summary directly from GetTokenSummary
    $.ajax({
        url: '/DataManagement/GetTokenSummary',
        type: 'GET',
        data: { tokenId: tokenId },
        success: function(modalHtml) {
            console.log('✅ Token summary loaded successfully');
            console.log('Modal HTML length:', modalHtml.length);
            
            // Remove any existing modal and add the new one
            $('#tokenSummaryModal').remove();
            $('body').append(modalHtml);
            
            // Show the modal
            const modal = document.getElementById('tokenSummaryModal');
            if (modal) {
                modal.style.display = 'flex';
                window.currentTokenId = tokenId;
                console.log('✅ Token summary modal displayed');
            } else {
                console.error('❌ Token summary modal element not found');
            }
        },
        error: function(xhr, status, error) {
            console.error('❌ Error loading token summary:', error);
            console.error('Status:', status);
            console.error('Response:', xhr.responseText);
        },
        complete: function() {
            $("#loading").hide();
        }
    });
}

// Check token click handling
function debugTokenClick() {
    console.log('🔍 Debugging token click handling...');
    
    // Check if tokenPlacementManager is available
    if (typeof window.tokenPlacementManager === 'undefined') {
        console.error('❌ tokenPlacementManager is not defined');
        return;
    }
    
    // Check if tokenActionModeManager is available
    if (typeof window.tokenActionModeManager === 'undefined') {
        console.error('❌ tokenActionModeManager is not defined');
        return;
    }
    
    console.log('✅ tokenPlacementManager available');
    console.log('✅ tokenActionModeManager available');
    
    // Check current mode
    const currentMode = window.tokenActionModeManager.getCurrentMode();
    console.log('Current mode:', currentMode);
    
    // Check if tokenManager is available
    if (typeof tokenManager === 'undefined') {
        console.error('❌ tokenManager is not defined');
    } else {
        console.log('✅ tokenManager is available');
        console.log('showTokenDetails method available:', typeof tokenManager.showTokenDetails === 'function');
    }
}

// Add to window for easy testing
window.testTokenSummary = testTokenSummary;
window.testTokenSummaryDirect = testTokenSummaryDirect;
window.debugTokenClick = debugTokenClick;

console.log('🔧 Token Summary Test functions available:');
console.log('- testTokenSummary(tokenId)');
console.log('- testTokenSummaryDirect(tokenId)');
console.log('- debugTokenClick()');
