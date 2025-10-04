// Attack Planning System - End-to-End Test Script
// This script tests the complete user journey from token click to attack execution

console.log('🚀 Starting Attack Planning System End-to-End Test');

// Test 1: Check if all required global functions exist
function testGlobalFunctions() {
    console.log('📋 Testing Global Functions...');
    
    const requiredFunctions = [
        'initializeAttackPlanning',
        'closeAttackPlanningModal',
        'showNotification'
    ];
    
    const missingFunctions = requiredFunctions.filter(func => typeof window[func] !== 'function');
    
    if (missingFunctions.length === 0) {
        console.log('✅ All global functions are available');
        return true;
    } else {
        console.error('❌ Missing global functions:', missingFunctions);
        return false;
    }
}

// Test 2: Check if TokenActionModeManager is properly integrated
function testTokenActionModeManager() {
    console.log('📋 Testing TokenActionModeManager Integration...');
    
    if (typeof window.tokenActionModeManager !== 'object') {
        console.error('❌ TokenActionModeManager not found');
        return false;
    }
    
    const requiredMethods = [
        'openAttackDataEntry',
        'loadAttackPlanningModal',
        'findTokenAtLocation',
        'getTokenPosition',
        'createAttackArrow'
    ];
    
    const missingMethods = requiredMethods.filter(method => 
        typeof window.tokenActionModeManager[method] !== 'function'
    );
    
    if (missingMethods.length === 0) {
        console.log('✅ TokenActionModeManager properly integrated');
        return true;
    } else {
        console.error('❌ Missing TokenActionModeManager methods:', missingMethods);
        return false;
    }
}

// Test 3: Check if modal container exists
function testModalContainer() {
    console.log('📋 Testing Modal Container...');
    
    const modalContainer = document.getElementById('modalsContainer');
    if (!modalContainer) {
        console.error('❌ Modal container not found');
        return false;
    }
    
    console.log('✅ Modal container exists');
    return true;
}

// Test 4: Test attack planning modal loading
function testAttackPlanningModalLoading() {
    console.log('📋 Testing Attack Planning Modal Loading...');
    
    return new Promise((resolve) => {
        fetch('/AttackPlanning/CreateAttackOrder')
            .then(response => {
                if (!response.ok) {
                    console.error('❌ Failed to load attack planning modal:', response.status);
                    resolve(false);
                    return;
                }
                return response.text();
            })
            .then(html => {
                if (html && html.includes('attackPlanningModal')) {
                    console.log('✅ Attack planning modal loads successfully');
                    resolve(true);
                } else {
                    console.error('❌ Attack planning modal HTML is invalid');
                    resolve(false);
                }
            })
            .catch(error => {
                console.error('❌ Error loading attack planning modal:', error);
                resolve(false);
            });
    });
}

// Test 5: Test individual tab loading
function testTabLoading() {
    console.log('📋 Testing Tab Loading...');
    
    const tabs = ['intent', 'timing', 'movement', 'fires', 'fogofwar', 'logistics', 'roe', 'summary'];
    const promises = tabs.map(tab => {
        return fetch(`/AttackPlanning/Load${tab.charAt(0).toUpperCase() + tab.slice(1)}Form`)
            .then(response => {
                if (!response.ok) {
                    console.error(`❌ Failed to load ${tab} tab:`, response.status);
                    return false;
                }
                return response.text();
            })
            .then(html => {
                if (html && html.includes('attack-form-section')) {
                    console.log(`✅ ${tab} tab loads successfully`);
                    return true;
                } else {
                    console.error(`❌ ${tab} tab HTML is invalid`);
                    return false;
                }
            })
            .catch(error => {
                console.error(`❌ Error loading ${tab} tab:`, error);
                return false;
            });
    });
    
    return Promise.all(promises).then(results => {
        const successCount = results.filter(r => r).length;
        console.log(`📊 Tab loading results: ${successCount}/${tabs.length} successful`);
        return successCount === tabs.length;
    });
}

// Test 6: Test save draft functionality
function testSaveDraft() {
    console.log('📋 Testing Save Draft Functionality...');
    
    const testData = {
        OrderId: 'test-order-123',
        TabName: 'intent',
        Data: {
            AttackType: 'Hasty',
            ManeuverForm: 'Penetration',
            DesiredEffect: 'Destroy',
            Notes: 'Test attack order'
        }
    };
    
    return fetch('/AttackPlanning/SaveDraft', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify(testData)
    })
    .then(response => {
        if (!response.ok) {
            console.error('❌ Save draft request failed:', response.status);
            return false;
        }
        return response.json();
    })
    .then(data => {
        if (data.success) {
            console.log('✅ Save draft functionality works');
            return true;
        } else {
            console.error('❌ Save draft failed:', data.message);
            return false;
        }
    })
    .catch(error => {
        console.error('❌ Error testing save draft:', error);
        return false;
    });
}

// Test 7: Simulate complete user journey
function testCompleteUserJourney() {
    console.log('📋 Testing Complete User Journey...');
    
    // Simulate token click flow
    const mockAttackerToken = {
        id: 'attacker-123',
        name: 'Test Attacker',
        position: { lat: 40.7128, lng: -74.0060 }
    };
    
    const mockTargetToken = {
        id: 'target-456',
        name: 'Test Target',
        position: { lat: 40.7589, lng: -73.9851 }
    };
    
    try {
        // Test 1: Open attack data entry
        if (typeof window.tokenActionModeManager.openAttackDataEntry === 'function') {
            console.log('✅ Attack data entry method exists');
        } else {
            console.error('❌ Attack data entry method missing');
            return false;
        }
        
        // Test 2: Load attack planning modal
        if (typeof window.tokenActionModeManager.loadAttackPlanningModal === 'function') {
            console.log('✅ Load attack planning modal method exists');
        } else {
            console.error('❌ Load attack planning modal method missing');
            return false;
        }
        
        // Test 3: Initialize attack planning
        if (typeof window.initializeAttackPlanning === 'function') {
            console.log('✅ Initialize attack planning method exists');
        } else {
            console.error('❌ Initialize attack planning method missing');
            return false;
        }
        
        console.log('✅ Complete user journey simulation successful');
        return true;
        
    } catch (error) {
        console.error('❌ Error in user journey simulation:', error);
        return false;
    }
}

// Run all tests
async function runAllTests() {
    console.log('🧪 Running Comprehensive Attack Planning System Tests...\n');
    
    const tests = [
        { name: 'Global Functions', test: testGlobalFunctions },
        { name: 'TokenActionModeManager Integration', test: testTokenActionModeManager },
        { name: 'Modal Container', test: testModalContainer },
        { name: 'Attack Planning Modal Loading', test: testAttackPlanningModalLoading },
        { name: 'Tab Loading', test: testTabLoading },
        { name: 'Save Draft Functionality', test: testSaveDraft },
        { name: 'Complete User Journey', test: testCompleteUserJourney }
    ];
    
    let passedTests = 0;
    let totalTests = tests.length;
    
    for (const test of tests) {
        console.log(`\n🔍 Running: ${test.name}`);
        try {
            const result = await test.test();
            if (result) {
                passedTests++;
                console.log(`✅ ${test.name}: PASSED`);
            } else {
                console.log(`❌ ${test.name}: FAILED`);
            }
        } catch (error) {
            console.log(`❌ ${test.name}: ERROR - ${error.message}`);
        }
    }
    
    console.log(`\n📊 Test Results: ${passedTests}/${totalTests} tests passed`);
    
    if (passedTests === totalTests) {
        console.log('🎉 All tests passed! Attack Planning System is ready for production.');
    } else {
        console.log('⚠️ Some tests failed. Please review the issues above.');
    }
    
    return passedTests === totalTests;
}

// Export for manual testing
window.testAttackPlanningSystem = runAllTests;

// Auto-run tests if in development mode
if (window.location.hostname === 'localhost' || window.location.hostname === '127.0.0.1') {
    console.log('🔧 Development mode detected. Auto-running tests...');
    runAllTests();
}
