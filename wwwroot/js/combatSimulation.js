/**
 * Combat Simulation - Using Server-side Partial Views with Dark Theme
 */

/**
 * Show combat simulation - loads attack order selection modal
 */
async function runCombatSimulation() {
    console.log('⚔️ Combat Simulation button clicked');
    console.log('jQuery available:', typeof $ !== 'undefined');
    console.log('fetch available:', typeof fetch !== 'undefined');
    
    try {
        // Clean up existing modals
        console.log('Cleaning up existing modals...');
        $('.gameplay-modal').hide().remove();
        $('.modal-backdrop').remove();
        $('body').removeClass('modal-open');
        
        // Load attack order selection modal from server
        console.log('Fetching attack orders from server...');
        const response = await fetch('/AttackPlanning/GetAttackOrderSelectionModal');
        console.log('Response status:', response.status);
        
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        
        const html = await response.text();
        console.log('HTML length received:', html.length);
        
        if (html.includes('Login') || html.includes('sign in')) {
            throw new Error('You may need to login again');
        }
        
        // Append modal to body
        $('body').append(html);
        console.log('Modal appended to body');
        
        // Show the modal with fade-in effect
        setTimeout(() => {
            $('#attackOrderSelectionModal').css('display', 'flex').hide().fadeIn(300);
            console.log('Modal shown');
        }, 50);
        
        // Attach event listeners to simulate buttons (using military-simulate-btn class)
        $('.military-simulate-btn').on('click', function() {
            const orderId = $(this).data('order-id');
            const attackerName = $(this).data('attacker-name');
            const targetName = $(this).data('target-name');
            console.log(`Simulate button clicked for order ${orderId}`);
            runSimulationForOrder(orderId, attackerName, targetName);
        });
        
        console.log('Event listeners attached');
        
    } catch (error) {
        console.error('❌ Error loading attack orders:', error);
        console.error('Error stack:', error.stack);
        if (typeof toastr !== 'undefined') {
            toastr.error('Error loading attack orders: ' + error.message, 'Error');
        } else {
            console.error('Error loading attack orders: ' + error.message);
        }
    }
}

// Make function globally available
window.runCombatSimulation = runCombatSimulation;

/**
 * Run simulation for a specific attack order
 */
async function runSimulationForOrder(orderId, attackerName, targetName) {
    console.log(`⚔️ Running simulation for order ${orderId}: ${attackerName} → ${targetName}`);
    
    try {
        // Close selection modal with fade out
        $('#attackOrderSelectionModal').fadeOut(300, function() {
            $(this).remove();
        });
        
        // Wait for modal to close
        await new Promise(resolve => setTimeout(resolve, 350));
        
        // Show loading notification
        if (typeof toastr !== 'undefined') {
            toastr.info(`Analyzing attack: ${attackerName} → ${targetName}...`, 'Combat Simulation');
        }
        
        // Load simulation results from server
        const response = await fetch('/AttackPlanning/RunComprehensiveSimulation', {
            method: 'POST',
            headers: { 
                'Content-Type': 'application/x-www-form-urlencoded'
            },
            body: `attackOrderId=${orderId}`
        });
        
        const html = await response.text();
        
        // Check if it's an error message
        if (html.includes('alert-danger')) {
            if (typeof toastr !== 'undefined') {
                toastr.error('Simulation failed. Please check the logs.', 'Error');
            }
            return;
        }
        
        // Append modal to body
        $('body').append(html);
        
        // Show the modal with fade-in effect
        setTimeout(() => {
            $('#combatSimulationModal').css('display', 'flex').hide().fadeIn(300);
        }, 50);
        
        if (typeof toastr !== 'undefined') {
            toastr.success('Combat simulation completed successfully', 'Simulation Complete');
        }
        
    } catch (error) {
        console.error('❌ Error in combat simulation:', error);
        if (typeof toastr !== 'undefined') {
            toastr.error('Error running simulation: ' + error.message, 'Error');
        } else {
            console.error('Error: ' + error.message);
        }
    }
}

/**
 * Run simulation for all attack orders
 */
function simulateAllOrders() {
    if (typeof toastr !== 'undefined') {
        toastr.info('Simulating all attack orders. This feature is coming soon!', 'Multiple Simulations');
    } else {
        console.log('Simulating all attack orders. This feature is coming soon!');
    }
}

/**
 * Export simulation results
 */
function exportSimulationResults() {
    // Get all the data from the modal
    const attackerName = $('#combatSimulationModal .attacker-card strong').text().trim();
    const defenderName = $('#combatSimulationModal .defender-card strong').text().trim();
    
    // Extract attack summary
    const attackSummaryLines = [];
    $('#attack-content .summary-line').each(function() {
        attackSummaryLines.push($(this).text().trim());
    });
    
    // Extract defense summary
    const defenseSummaryLines = [];
    $('#defense-content .summary-line').each(function() {
        defenseSummaryLines.push($(this).text().trim());
    });
    
    const data = {
        attacker: attackerName,
        defender: defenderName,
        timestamp: new Date().toISOString(),
        attackSummary: attackSummaryLines.join('\n'),
        defenseSummary: defenseSummaryLines.join('\n'),
        fullDetails: {
            attackPhases: $('#attack-content').text().trim(),
            defensePhases: $('#defense-content').text().trim(),
            summary: $('#summary-content').text().trim()
        }
    };
    
    const blob = new Blob([JSON.stringify(data, null, 2)], { type: 'application/json' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `combat_simulation_${Date.now()}.json`;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
    
    if (typeof toastr !== 'undefined') {
        toastr.success('Simulation results exported', 'Export Complete');
    }
}

// Global close function for modals
window.closeSimulationModal = function(modalId) {
    $(`#${modalId}`).fadeOut(300, function() {
        $(this).remove();
    });
};

console.log('✅ Combat Simulation module loaded (Dark Theme with Partial Views)');
