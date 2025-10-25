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
    
    // Show loader
    $("#simpleLoader").show();
    
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
        
        // Show the modal using Bootstrap's modal method
        const $modal = $('#attackOrderSelectionModal');
        
        setTimeout(() => {
            $modal.modal('show');
            console.log('✅ Attack Order Selection Modal shown (using Bootstrap)');
            console.log('✅ Buttons have onclick handlers attached via HTML');
            // Hide loader after modal is shown
            $("#simpleLoader").hide();
        }, 50);
        
    } catch (error) {
        console.error('❌ Error loading attack orders:', error);
        console.error('Error stack:', error.stack);
        $("#simpleLoader").hide();
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
    
    // Show loader
    $("#simpleLoader").show();
    
    try {
        // Close selection modal using Bootstrap
        $('#attackOrderSelectionModal').modal('hide');
        
        // Wait for modal to close
        await new Promise(resolve => setTimeout(resolve, 350));
        
        // Remove modal after it's hidden
        $('#attackOrderSelectionModal').remove();
        $('.modal-backdrop').remove();
        
        // Show loading notification
        if (typeof toastr !== 'undefined') {
            toastr.info(`Analyzing attack: ${attackerName} → ${targetName}...`, 'Combat Simulation');
        }
        
        // Load simulation results from server
        console.log('📡 Fetching simulation results...');
        const response = await fetch('/AttackPlanning/RunComprehensiveSimulation', {
            method: 'POST',
            headers: { 
                'Content-Type': 'application/x-www-form-urlencoded'
            },
            body: `attackOrderId=${orderId}`
        });
        
        console.log('📡 Response status:', response.status);
        const html = await response.text();
        console.log('📡 Response HTML length:', html.length);
        
        // Check if response contains an error (old style alert or new modal style)
        const hasAlertError = html.includes('alert-danger');
        const hasModalError = html.includes('Combat Simulation Error');
        const hasErrorModal = html.includes('combatSimulationModal');
        
        console.log('🔍 Error detection:', {
            hasAlertError,
            hasModalError,
            hasErrorModal,
            htmlPreview: html.substring(0, 500)
        });
        
        // Log the FULL HTML if error detected (for debugging)
        if (hasAlertError || hasModalError) {
            console.log('📄 FULL ERROR HTML:', html);
        }
        
        if (hasAlertError || hasModalError) {
            $("#simpleLoader").hide();
            console.error('❌ Simulation returned error, attempting to show error modal');
            
            // Clean up any existing modals first
            $('#combatSimulationModal').remove();
            $('.modal-backdrop').remove();
            
            // If it's the new error modal format, show it
            if (hasErrorModal) {
                console.log('✅ Error modal structure detected, appending to body');
                
                // Append error modal to body
                $('body').append(html);
                
                console.log('✅ Error modal appended, showing with Bootstrap');
                
                // Show error modal using Bootstrap
                setTimeout(() => {
                    $('#combatSimulationModal').modal('show');
                    console.log('✅ Error modal shown');
                }, 100);
            } else {
                // Old style alert-danger or exception message - show in toastr
                console.log('⚠️ Old style error format, showing in toastr');
                if (typeof toastr !== 'undefined') {
                    // Extract error message from alert-danger div
                    const tempDiv = document.createElement('div');
                    tempDiv.innerHTML = html;
                    const errorMsg = tempDiv.querySelector('.alert-danger')?.textContent?.trim() || html.trim();
                    toastr.error(errorMsg, 'Simulation Error', { timeOut: 8000 });
                }
            }
            return;
        }
        
        // Check if response is empty or invalid
        if (!html || html.length < 100) {
            $("#simpleLoader").hide();
            console.error('❌ Invalid or empty response from server');
            if (typeof toastr !== 'undefined') {
                toastr.error('No simulation results returned from server', 'Error');
            }
            return;
        }
        
        // Clean up any existing combat simulation modals
        $('#combatSimulationModal').remove();
        
        // Append modal to body
        $('body').append(html);
        console.log('✅ Modal HTML appended to body');
        
        // Check if modal element exists
        if ($('#combatSimulationModal').length === 0) {
            $("#simpleLoader").hide();
            console.error('❌ Modal element #combatSimulationModal not found in returned HTML');
            if (typeof toastr !== 'undefined') {
                toastr.error('Combat simulation modal not found', 'Error');
            }
            return;
        }
        
        console.log('✅ Modal element found, showing modal...');
        
        // Show the modal using Bootstrap's modal method
        setTimeout(() => {
            $('#combatSimulationModal').modal('show');
            console.log('✅ Modal shown');
            $("#simpleLoader").hide();
        }, 50);
        
        if (typeof toastr !== 'undefined') {
            toastr.success('Combat simulation completed successfully', 'Simulation Complete');
        }
        
    } catch (error) {
        console.error('❌ Error in combat simulation:', error);
        $("#simpleLoader").hide();
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

// Global close function for modals using Bootstrap
window.closeSimulationModal = function(modalId) {
    $(`#${modalId}`).modal('hide');
    // Remove modal after it's hidden
    setTimeout(() => {
        $(`#${modalId}`).remove();
        $('.modal-backdrop').remove();
        $('body').removeClass('modal-open');
    }, 300);
};

// Export runSimulationForOrder globally so onclick can access it
window.runSimulationForOrder = runSimulationForOrder;

console.log('✅ Combat Simulation module loaded (Dark Theme with Partial Views)');
