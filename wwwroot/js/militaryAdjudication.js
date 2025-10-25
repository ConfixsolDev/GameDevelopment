/**
 * Military Adjudication - Clean, simple implementation
 * Calls backend to analyze all token movements and displays comprehensive results
 */

/**
 * Run military adjudication for all tokens
 */
async function runMilitaryAdjudication() {
    console.log('🎯 Running military adjudication...');
    
    // Check if terrain database is available
    console.log('🔍 Current terrain database:', window.currentTerrainDb);
    
    // If no terrain database is loaded, try to get current map path and load it
    if (!window.currentTerrainDb) {
        const currentMapPath = document.getElementById('currentMapPath')?.value;
        if (currentMapPath) {
            console.log('🔍 No terrain database loaded, but found current map path:', currentMapPath);
            console.log('🔍 Attempting to load terrain database for current map...');
            
            // Try to load terrain database for current map
            if (window.gamePlayManager && typeof window.gamePlayManager.loadTerrainDatabase === 'function') {
                try {
                    await window.gamePlayManager.loadTerrainDatabase(currentMapPath);
                    console.log('🔍 Terrain database loading attempted for:', currentMapPath);
                } catch (error) {
                    console.warn('⚠️ Failed to load terrain database for current map:', error);
                }
            }
        }
    }
    
    if (!window.currentTerrainDb) {
        console.warn('⚠️ No terrain database available - analysis will use basic calculations');
        if (typeof toastr !== 'undefined') {
            toastr.warning('No terrain data available. Analysis will use basic calculations.', 'Limited Data', { timeOut: 3000 });
        }
    } else {
        console.log('✅ Terrain database available:', window.currentTerrainDb);
    }
    
    // Show loading notification
    if (typeof toastr !== 'undefined') {
        toastr.info('Analyzing all token movements...', 'Military Adjudication');
    }
    
    // Show loader
    showLoader();
    
    try {
        // Get current map path from multiple sources
        const currentMapPath = document.getElementById('currentMapPath')?.value || '';
        
        // Try multiple fallback sources
        let finalMapPath = currentMapPath;
        
        // Fallback 1: GamePlayManager
        if (!finalMapPath && window.gamePlayManager && window.gamePlayManager.currentMapPath) {
            finalMapPath = window.gamePlayManager.currentMapPath;
        }
        
        // Fallback 2: Check URL parameters
        if (!finalMapPath) {
            const urlParams = new URLSearchParams(window.location.search);
            const fileParam = urlParams.get('file');
            if (fileParam) {
                finalMapPath = fileParam;
            }
        }
        
        // Update the hidden field if we found a map path
        if (finalMapPath && !currentMapPath) {
            const hiddenField = document.getElementById('currentMapPath');
            if (hiddenField) {
                hiddenField.value = finalMapPath;
            }
        }
        
        const response = await fetch('/GamePlay/AdjudicateAllMovements', {
            method: 'POST',
            headers: { 
                'Content-Type': 'application/json',
                'X-Terrain-Database': window.currentTerrainDb || '',
                'X-Current-Map-Path': finalMapPath
            }
        });
        
        const result = await response.json();
        
        if (result.success) {
            console.log('✅ Adjudication completed:', result);
            
            // Collect all recommendations from individual results
            const allRecommendations = [];
            if (result.results && Array.isArray(result.results)) {
                result.results.forEach(tokenResult => {
                    if (tokenResult.recommendations && Array.isArray(tokenResult.recommendations)) {
                        allRecommendations.push(...tokenResult.recommendations);
                    }
                });
            }
            
            // Add recommendations to summary
            if (!result.summary) {
                result.summary = {};
            }
            result.summary.recommendations = allRecommendations;
            
            // Update route visualizations on map
            updateRouteVisualizations(result.results);
            
            // Show comprehensive results modal
            showAdjudicationResultsModal(result);
            
            // Hide loader
            hideLoader();
            
            if (typeof toastr !== 'undefined') {
                toastr.success(`Analyzed ${result.summary.totalTokens} token movements`, 'Adjudication Complete');
            }
        } else {
            // Hide loader
            hideLoader();
            
            console.error('❌ Adjudication failed:', result.message);
            if (typeof toastr !== 'undefined') {
                toastr.error(result.message, 'Adjudication Failed');
            } else {
                console.error('Adjudication failed: ' + result.message);
            }
        }
    } catch (error) {
        // Hide loader
        hideLoader();
        
        console.error('❌ Error in military adjudication:', error);
        if (typeof toastr !== 'undefined') {
            toastr.error('Error running adjudication: ' + error.message, 'Error');
        } else {
            console.error('Error: ' + error.message);
        }
    }
}

/**
 * Update route visualizations based on adjudication results
 */
function updateRouteVisualizations(results) {
    if (!window.gameMap) return;
    
    results.forEach(result => {
        // Determine route color based on feasibility
        let routeColor = '#4299e1'; // Default blue
        
        switch (result.feasibility.status) {
            case 'feasible':
                routeColor = '#00ff00'; // Green
                break;
            case 'moderate':
                routeColor = '#ffff00'; // Yellow
                break;
            case 'high_consumption':
                routeColor = '#ffaa00'; // Orange
                break;
            case 'insufficient_mp':
                routeColor = '#ff0000'; // Red
                break;
            case 'terrain_blocked':
                routeColor = '#ff0000'; // Red - Impassable
                break;
        }
        
        // Draw route with color coding
        if (result.waypoints && result.waypoints.length > 1) {
            const positions = result.waypoints.map(wp => [wp.lat, wp.lng]);
            
            const routeLine = L.polyline(positions, {
                color: routeColor,
                weight: 4,
                opacity: 0.8,
                dashArray: '10, 10',
                smoothFactor: 1.0
            }).addTo(window.gameMap);
            
            // Add tooltip showing status
            routeLine.bindTooltip(`${result.tokenName}: ${result.feasibility.status.toUpperCase()}`, {
                permanent: false,
                direction: 'center',
                className: 'route-tooltip'
            });
            
            console.log(`🎯 Route drawn for ${result.tokenName}: ${routeColor}`);
        }
    });
}

/**
 * Show comprehensive adjudication results modal - Bootstrap Version
 */
function showAdjudicationResultsModal(data) {
    // Remove existing modal if any
    const existingModal = document.getElementById('militaryAdjudicationModal');
    if (existingModal) {
        existingModal.remove();
    }
    
    const modal = document.createElement('div');
    modal.id = 'militaryAdjudicationModal';
    modal.className = 'modal fade';
    modal.setAttribute('tabindex', '-1');
    modal.setAttribute('role', 'dialog');
    modal.setAttribute('aria-labelledby', 'militaryAdjudicationModalLabel');
    modal.setAttribute('aria-hidden', 'true');
    
    // Build results HTML
    let resultsHTML = '';
    
    data.results.forEach((result, index) => {
        const feasibilityColor = result.feasibility.isFeasible ? '#00ff00' : '#ff0000';
        const feasibilityIcon = result.feasibility.isFeasible ? '✓' : '✗';
        const statusColor = getStatusColor(result.feasibility.status);
        
        resultsHTML += `
            <div class="adjudication-card" style="margin-bottom: 20px; padding: 20px; background: #2a2a2a; border-left: 5px solid ${feasibilityColor}; border-radius: 6px;">
                <!-- Token Header -->
                <div class="token-header" style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 15px;">
                    <div>
                        <h3 style="margin: 0; color: #00ff00; font-size: 18px;">${result.tokenName}</h3>
                        <div style="margin-top: 5px; display: flex; gap: 8px; font-size: 11px;">
                            <span class="badge" style="padding: 3px 8px; background: #1a1a1a; border: 1px solid #00ff00; border-radius: 3px; color: #00ff00;">
                                ${result.unitType}
                            </span>
                            <span class="badge" style="padding: 3px 8px; background: #1a1a1a; border: 1px solid #00ff00; border-radius: 3px; color: #00ff00;">
                                ${result.mobilityType.toUpperCase()}
                            </span>
                            <span class="badge" style="padding: 3px 8px; background: #1a1a1a; border: 1px solid ${result.strength >= 75 ? '#00ff00' : '#ffaa00'}; border-radius: 3px; color: ${result.strength >= 75 ? '#00ff00' : '#ffaa00'};">
                                Strength: ${result.strength}%
                            </span>
                            <span class="badge" style="padding: 3px 8px; background: #1a1a1a; border: 1px solid #00ff00; border-radius: 3px; color: #00ff00;">
                                Supply: ${result.supplyState}
                            </span>
                        </div>
                    </div>
                    <div style="font-size: 36px; color: ${feasibilityColor};">
                        ${feasibilityIcon}
                    </div>
                </div>
                
                <!-- Unit Composition -->
                <div style="background: #1a1a1a; padding: 15px; border-radius: 6px; margin-bottom: 15px; border-left: 3px solid #00ff00;">
                    <h5 style="color: #00ff00; font-size: 14px; margin-bottom: 10px;">
                        <i class="fas fa-users"></i> Unit Composition
                    </h5>
                    <div style="display: grid; grid-template-columns: repeat(3, 1fr); gap: 10px; margin-bottom: 10px;">
                        <div style="text-align: center;">
                            <div style="font-size: 18px; color: #00ff00; font-weight: bold;">${result.unitComposition.personnel}</div>
                            <div style="font-size: 10px; color: #666;">Personnel</div>
                        </div>
                        <div style="text-align: center;">
                            <div style="font-size: 18px; color: #00ff00; font-weight: bold;">${result.unitComposition.vehicles}</div>
                            <div style="font-size: 10px; color: #666;">Vehicles</div>
                        </div>
                        <div style="text-align: center;">
                            <div style="font-size: 18px; color: #00ff00; font-weight: bold;">${result.unitComposition.weapons}</div>
                            <div style="font-size: 10px; color: #666;">Weapons</div>
                        </div>
                    </div>
                    <div style="font-size: 12px; color: #ccc; font-style: italic;">
                        ${result.unitComposition.details}
                    </div>
                </div>

                <!-- Movement Analysis Grid -->
                <div style="display: grid; grid-template-columns: repeat(4, 1fr); gap: 12px; margin-bottom: 15px;">
                    <div style="background: #1a1a1a; padding: 12px; border-radius: 4px; text-align: center;">
                        <div style="font-size: 10px; color: #666; text-transform: uppercase; margin-bottom: 5px;">Distance</div>
                        <div style="font-size: 20px; color: #00ff00; font-weight: bold;">${result.movement.totalDistance} km</div>
                    </div>
                    <div style="background: #1a1a1a; padding: 12px; border-radius: 4px; text-align: center;">
                        <div style="font-size: 10px; color: #666; text-transform: uppercase; margin-bottom: 5px;">MP Required</div>
                        <div style="font-size: 20px; color: ${result.mpAnalysis.mpUtilization > 80 ? '#ffaa00' : '#00ff00'}; font-weight: bold;">${result.mpAnalysis.totalMPRequired}</div>
                    </div>
                    <div style="background: #1a1a1a; padding: 12px; border-radius: 4px; text-align: center;">
                        <div style="font-size: 10px; color: #666; text-transform: uppercase; margin-bottom: 5px;">MP Usage</div>
                        <div style="font-size: 20px; color: ${result.mpAnalysis.mpUtilization > 80 ? '#ffaa00' : '#00ff00'}; font-weight: bold;">${result.mpAnalysis.mpUtilization}%</div>
                    </div>
                    <div style="background: #1a1a1a; padding: 12px; border-radius: 4px; text-align: center;">
                        <div style="font-size: 10px; color: #666; text-transform: uppercase; margin-bottom: 5px;">Est. Time</div>
                        <div style="font-size: 20px; color: #00ff00; font-weight: bold;">${result.timeEstimate.hours}h ${result.timeEstimate.minutes}m</div>
                    </div>
                </div>

                <!-- Terrain Analysis -->
                <div style="background: #1a1a1a; padding: 15px; border-radius: 6px; margin-bottom: 15px; border-left: 3px solid ${result.terrainAnalysis.isRouteBlocked ? '#ff0000' : '#ffaa00'};">
                    <h5 style="color: ${result.terrainAnalysis.isRouteBlocked ? '#ff0000' : '#ffaa00'}; font-size: 14px; margin-bottom: 10px;">
                        <i class="fas fa-mountain"></i> Terrain Analysis
                        <span style="font-size: 10px; color: ${result.terrainAnalysis.dataSource === 'Real elevation data' ? '#00ff00' : '#ffaa00'}; margin-left: 10px;">
                            (${result.terrainAnalysis.dataSource})
                        </span>
                    </h5>
                    ${result.terrainAnalysis.isRouteBlocked ? `
                        <div style="padding: 10px; background: #ff0000; border-radius: 4px; margin-bottom: 12px; border: 2px solid #fff;">
                            <div style="font-size: 14px; color: #fff; font-weight: bold; text-align: center;">
                                ⛔ ROUTE BLOCKED - MOVEMENT IMPOSSIBLE
                            </div>
                            <div style="font-size: 12px; color: #fff; margin-top: 5px; text-align: center;">
                                ${result.terrainAnalysis.blockageReason}
                            </div>
                        </div>
                    ` : `
                        <div style="padding: 10px; background: #00ff00; border-radius: 4px; margin-bottom: 12px; border: 2px solid #fff;">
                            <div style="font-size: 14px; color: #000; font-weight: bold; text-align: center;">
                                ✅ ROUTE CLEAR - MOVEMENT POSSIBLE
                            </div>
                        </div>
                    `}
                    <div style="display: grid; grid-template-columns: repeat(4, 1fr); gap: 10px; margin-bottom: 10px;">
                        <div style="text-align: center;">
                            <div style="font-size: 16px; color: #ffaa00; font-weight: bold;">${result.terrainAnalysis.startElevation}m</div>
                            <div style="font-size: 10px; color: #666;">Start Elevation</div>
                        </div>
                        <div style="text-align: center;">
                            <div style="font-size: 16px; color: #ffaa00; font-weight: bold;">${result.terrainAnalysis.endElevation}m</div>
                            <div style="font-size: 10px; color: #666;">End Elevation</div>
                        </div>
                        <div style="text-align: center;">
                            <div style="font-size: 16px; color: #ffaa00; font-weight: bold;">${result.terrainAnalysis.maxSlope}°</div>
                            <div style="font-size: 10px; color: #666;">Max Slope</div>
                        </div>
                        <div style="text-align: center;">
                            <div style="font-size: 16px; color: #ffaa00; font-weight: bold;">${result.terrainAnalysis.terrainType}</div>
                            <div style="font-size: 10px; color: #666;">Terrain Type</div>
                        </div>
                    </div>
                    <div style="display: grid; grid-template-columns: repeat(3, 1fr); gap: 10px; margin-bottom: 10px;">
                        <div style="text-align: center;">
                            <div style="font-size: 16px; color: ${result.terrainAnalysis.elevationGain >= 0 ? '#00ff00' : '#ff6600'}; font-weight: bold;">${result.terrainAnalysis.elevationGain >= 0 ? '+' : ''}${result.terrainAnalysis.elevationGain}m</div>
                            <div style="font-size: 10px; color: #666;">Elevation Gain</div>
                        </div>
                        <div style="text-align: center;">
                            <div style="font-size: 14px; color: ${result.terrainAnalysis.maxSlope > 20 ? '#ff0000' : result.terrainAnalysis.maxSlope > 10 ? '#ffaa00' : '#00ff00'}; font-weight: bold;">${result.terrainAnalysis.maxSlope}°</div>
                            <div style="font-size: 10px; color: #666;">Max Slope</div>
                        </div>
                        <div style="text-align: center;">
                            <div style="font-size: 14px; color: ${result.terrainAnalysis.difficulty === 'High' ? '#ff0000' : result.terrainAnalysis.difficulty === 'Moderate' ? '#ffaa00' : '#00ff00'}; font-weight: bold;">${result.terrainAnalysis.difficulty}</div>
                            <div style="font-size: 10px; color: #666;">Difficulty</div>
                        </div>
                        <div style="text-align: center;">
                            <div style="font-size: 14px; color: ${result.terrainAnalysis.elevationPoints > 50 ? '#00ff00' : result.terrainAnalysis.elevationPoints > 10 ? '#ffaa00' : '#ff0000'}; font-weight: bold;">${result.terrainAnalysis.elevationPoints}</div>
                            <div style="font-size: 10px; color: #666;">Data Points</div>
                        </div>
                    </div>
                    <div style="margin-top: 10px; font-size: 11px; color: #666;">
                        <strong>Vehicle Weight:</strong> ${result.terrainAnalysis.vehicleWeight}T
                    </div>
                    ${result.terrainAnalysis.obstacles && result.terrainAnalysis.obstacles.length > 0 ? `
                        <div style="margin-top: 10px;">
                            <div style="font-size: 12px; color: #ff0000; font-weight: bold; margin-bottom: 5px;">Obstacles Detected:</div>
                            ${result.terrainAnalysis.obstacles.map(obs => {
                                const isImpassable = obs.isImpassable === true;
                                const color = isImpassable ? '#ff0000' : '#ffaa00';
                                const icon = isImpassable ? '⛔' : '⚠️';
                                return `<div style="font-size: 11px; color: ${color}; margin: 2px 0; font-weight: ${isImpassable ? 'bold' : 'normal'};">${icon} ${obs.description}</div>`;
                            }).join('')}
                        </div>
                    ` : ''}
                    ${result.terrainAnalysis.isRouteBlocked ? `
                        <div style="margin-top: 10px; padding: 8px; background: #ff0000; border-radius: 4px;">
                            <div style="font-size: 12px; color: #fff; font-weight: bold;">🚫 ROUTE BLOCKED</div>
                            <div style="font-size: 11px; color: #fff;">${result.terrainAnalysis.blockageReason}</div>
                        </div>
                    ` : ''}
                    ${result.terrainAnalysis.obstacleCategories ? `
                        <div style="margin-top: 10px;">
                            <div style="font-size: 11px; color: #666; margin-bottom: 5px;">Obstacle Summary:</div>
                            <div style="display: grid; grid-template-columns: repeat(4, 1fr); gap: 5px; font-size: 10px;">
                                ${Object.entries(result.terrainAnalysis.obstacleCategories).map(([type, count]) => 
                                    count > 0 ? `<div style="color: #ffaa00;">${type}: ${count}</div>` : ''
                                ).join('')}
                            </div>
                        </div>
                    ` : ''}
                    ${result.terrainAnalysis.error ? `
                        <div style="margin-top: 10px; padding: 8px; background: #ff0000; border-radius: 4px;">
                            <div style="font-size: 11px; color: #fff;">Error: ${result.terrainAnalysis.error}</div>
                        </div>
                    ` : ''}
                </div>

                <!-- Enhanced Time Estimates -->
                <div style="background: #1a1a1a; padding: 15px; border-radius: 6px; margin-bottom: 15px; border-left: 3px solid #4299e1;">
                    <h5 style="color: #4299e1; font-size: 14px; margin-bottom: 10px;">
                        <i class="fas fa-clock"></i> Time Analysis
                    </h5>
                    <div style="display: grid; grid-template-columns: repeat(3, 1fr); gap: 10px;">
                        <div style="text-align: center;">
                            <div style="font-size: 16px; color: #4299e1; font-weight: bold;">${result.timeEstimate.hours}h ${result.timeEstimate.minutes}m</div>
                            <div style="font-size: 10px; color: #666;">Basic Estimate</div>
                        </div>
                        <div style="text-align: center;">
                            <div style="font-size: 16px; color: #4299e1; font-weight: bold;">${result.timeEstimate.realisticHours}h ${result.timeEstimate.realisticMinutes}m</div>
                            <div style="font-size: 10px; color: #666;">Realistic Estimate</div>
                        </div>
                        <div style="text-align: center;">
                            <div style="font-size: 16px; color: #4299e1; font-weight: bold;">${result.timeEstimate.effectiveSpeed || 'N/A'} km/h</div>
                            <div style="font-size: 10px; color: #666;">Effective Speed</div>
                        </div>
                    </div>
                </div>
                
                <!-- Feasibility Status -->
                <div style="padding: 15px; background: #1a1a1a; border: 2px solid ${statusColor}; border-radius: 6px; margin-bottom: 15px;">
                    <div style="font-size: 14px; font-weight: bold; color: ${statusColor}; text-transform: uppercase; margin-bottom: 5px;">
                        ${result.feasibility.status}
                    </div>
                    <div style="font-size: 13px; color: #ccc;">
                        ${result.feasibility.reason}
                    </div>
                </div>
                
                <!-- Route Segments -->
                <div style="margin-bottom: 15px;">
                    <h5 style="color: #00ff00; font-size: 14px; margin-bottom: 10px;">Route Breakdown (${result.movement.segments.length} segments)</h5>
                    <div class="segments-list" style="max-height: 150px; overflow-y: auto;">
                        ${result.movement.segments.map(seg => `
                            <div style="display: flex; justify-content: space-between; padding: 8px 10px; background: #1a1a1a; margin: 4px 0; border-radius: 4px; font-size: 12px;">
                                <span style="color: #ccc;">Seg ${seg.segmentNumber}: ${seg.distance} km • ${seg.terrainType}</span>
                                <span style="color: ${seg.mpConsumption > 15 ? '#ffaa00' : '#00ff00'}; font-weight: bold;">${seg.mpConsumption} MP</span>
                            </div>
                        `).join('')}
                    </div>
                </div>
                
            </div>
        `;
    });
    
    modal.innerHTML = `
        <div class="modal-dialog modal-xl" role="document" style="max-width: 1200px;">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title" id="militaryAdjudicationModalLabel" style="display: flex; align-items: center; gap: 10px;">
                        <i class="fas fa-chart-line"></i> 
                        Military Movement Adjudication
                    </h5>
                    <button type="button" class="close" data-dismiss="modal" aria-label="Close" style="color: #ffb000; font-size: 24px; font-weight: bold;" onclick="window.closeMilitaryAdjudicationModal()">
                        <span aria-hidden="true">&times;</span>
                    </button>
                </div>
                
                <div class="modal-body" style="max-height: 70vh; overflow-y: auto;">
                <!-- Tactical Recommendations by Force -->
                <div class="tactical-recommendations" style="background: #1a1a1a; padding: 20px; border-radius: 8px; margin-bottom: 25px; border: 2px solid #ffaa00;">
                    <h3 style="margin: 0 0 15px 0; color: #ffaa00; font-size: 16px;">
                        <i class="fas fa-lightbulb"></i> Tactical Recommendations by Force
                    </h3>
                    ${generateForceSpecificRecommendations(data)}
                </div>
                
                <!-- Executive Summary by Force Type -->
                <div class="executive-summary" style="background: #1a1a1a; padding: 20px; border-radius: 8px; margin-bottom: 25px; border: 2px solid #00ff00;">
                    <h3 style="margin: 0 0 15px 0; color: #00ff00; font-size: 16px;">
                        <i class="fas fa-clipboard-check"></i> Executive Summary by Force
                    </h3>
                    ${generateForceSpecificSummary(data)}
                </div>
                
                <!-- Individual Token Results -->
                <div class="token-results">
                    <h3 style="margin: 0 0 15px 0; color: #00ff00; font-size: 16px;">
                        <i class="fas fa-list"></i> Detailed Analysis
                    </h3>
                    ${resultsHTML}
                </div>
                </div>
                
                <div class="modal-footer" style="display: flex; justify-content: space-between; align-items: center; background: #1a1a1a; border-top: 1px solid #333;">
                    <div style="color: #ccc; font-size: 12px;">
                        Analysis completed at ${new Date(data.timestamp).toLocaleString()}
                    </div>
                    <button type="button" class="btn btn-primary" data-dismiss="modal" style="background: #ffb000; border-color: #ffb000; color: #000; font-weight: bold;" onclick="window.closeMilitaryAdjudicationModal()">
                        <i class="fas fa-check"></i> Close
                    </button>
                </div>
            </div>
        </div>
    `;
    
    document.body.appendChild(modal);
    
    // Show modal using Bootstrap
    $('#militaryAdjudicationModal').modal({
        backdrop: true,
        keyboard: true,
        show: true
    });
    
    // Use proper Bootstrap modal close method
    window.closeMilitaryAdjudicationModal = function() {
        // Close using Bootstrap's modal method
        $('#militaryAdjudicationModal').modal('hide');
        
        // Remove modal after Bootstrap hide animation completes (300ms)
        setTimeout(() => {
            $('#militaryAdjudicationModal').remove();
            $('.modal-backdrop').remove();
            $('body').removeClass('modal-open');
            console.log('✅ Military adjudication modal closed');
        }, 300);
    };
    
    // Ensure close buttons work with proper event delegation
    $(document).off('click.militaryModal').on('click.militaryModal', '#militaryAdjudicationModal .close, #militaryAdjudicationModal [data-dismiss="modal"]', function(e) {
        e.preventDefault();
        window.closeMilitaryAdjudicationModal();
    });
    
    // Handle escape key
    $(document).off('keydown.militaryModal').on('keydown.militaryModal', function(e) {
        if (e.key === 'Escape' && $('#militaryAdjudicationModal').length) {
            window.closeMilitaryAdjudicationModal();
        }
    });
    
    // Force display for large screens
    $('#militaryAdjudicationModal').css({
        'display': 'block',
        'opacity': '1',
        'visibility': 'visible'
    }).addClass('show');
}

/**
 * Generate force-specific recommendations grouped by Blue Land and Fox Land
 */
function generateForceSpecificRecommendations(data) {
    // Group results by force type
    const blueResults = data.results.filter(result => {
        const forceType = result.forceType || '';
        return forceType.toLowerCase().includes('blue') || forceType.toLowerCase().includes('friendly');
    });
    
    const foxResults = data.results.filter(result => {
        const forceType = result.forceType || '';
        return forceType.toLowerCase().includes('fox') || forceType.toLowerCase().includes('red') || forceType.toLowerCase().includes('hostile');
    });
    
    // Generate recommendations for each force
    const blueRecommendations = generateRecommendationsForForce(blueResults, 'Blue Land');
    const foxRecommendations = generateRecommendationsForForce(foxResults, 'Fox Land');
    
    return `
        <div style="display: flex; flex-direction: column; gap: 20px;">
            <!-- Blue Land Recommendations -->
            <div>
                <h4 style="margin: 0 0 15px 0; color: #00ff00; font-size: 16px; text-align: center;">
                    Blue Land Recommendations
                </h4>
                ${blueRecommendations}
            </div>
            
            <!-- Fox Land Recommendations -->
            <div>
                <h4 style="margin: 0 0 15px 0; color: #00ff00; font-size: 16px; text-align: center;">
                    Fox Land Recommendations
                </h4>
                ${foxRecommendations}
            </div>
        </div>
    `;
}

/**
 * Generate recommendations for a specific force
 */
function generateRecommendationsForForce(forceResults, forceName) {
    if (forceResults.length === 0) {
        return `
            <div style="color: #666; font-size: 12px; text-align: center; padding: 20px;">
                No units for ${forceName}
            </div>
        `;
    }
    
    let recommendationsHTML = '';
    
    forceResults.forEach(result => {
        const unitNumber = result.tokenName || 'Unknown';
        const feasibility = result.feasibility;
        const priority = feasibility.isFeasible ? 'LOW' : 'HIGH';
        const priorityColor = feasibility.isFeasible ? '#00ff00' : '#ff0000';
        const statusIcon = feasibility.isFeasible ? '✓' : '✗';
        
        let recommendationMessage = '';
        if (feasibility.isFeasible) {
            recommendationMessage = 'Movement plan appears sound - proceed as planned';
        } else {
            if (feasibility.status === 'terrain_blocked') {
                recommendationMessage = 'ROUTE IMPASSABLE - Terrain obstacles block movement. Select an alternate route avoiding water, cliffs, or steep terrain.';
            } else if (feasibility.status === 'insufficient_mp') {
                recommendationMessage = 'Consider engineering support (bridge-laying, road construction) or air/water transport alternatives';
            } else {
                recommendationMessage = 'Consider engineering support (bridge-laying, road construction) or air/water transport alternatives';
            }
        }
        
        recommendationsHTML += `
            <div style="display: flex; align-items: center; gap: 8px; padding: 8px; margin: 5px 0; border-left: 3px solid ${priorityColor};">
                <div style="color: ${priorityColor}; font-weight: bold; min-width: 50px; font-size: 10px; text-transform: uppercase;">
                    ${priority}
                </div>
                <div style="color: #fff; font-weight: bold; min-width: 40px; font-size: 12px;">
                    Unit ${unitNumber}
                </div>
                <div style="color: ${priorityColor}; font-size: 14px; margin-right: 5px;">
                    ${statusIcon}
                </div>
                <div style="color: #ccc; font-size: 11px; flex: 1; line-height: 1.3;">
                    ${recommendationMessage}
                </div>
            </div>
        `;
    });
    
    return recommendationsHTML;
}

/**
 * Generate force-specific summary with Blue Land and Fox Land breakdown
 */
function generateForceSpecificSummary(data) {
    // Group results by force type
    const blueResults = data.results.filter(result => {
        const forceType = result.forceType || '';
        return forceType.toLowerCase().includes('blue') || forceType.toLowerCase().includes('friendly');
    });
    
    const foxResults = data.results.filter(result => {
        const forceType = result.forceType || '';
        return forceType.toLowerCase().includes('fox') || forceType.toLowerCase().includes('red') || forceType.toLowerCase().includes('hostile');
    });
    
    // Calculate statistics for each force
    const blueStats = {
        total: blueResults.length,
        feasible: blueResults.filter(r => r.feasibility.isFeasible).length,
        blocked: blueResults.filter(r => !r.feasibility.isFeasible).length,
        totalDistance: blueResults.reduce((sum, r) => sum + r.movement.totalDistance, 0)
    };
    
    const foxStats = {
        total: foxResults.length,
        feasible: foxResults.filter(r => r.feasibility.isFeasible).length,
        blocked: foxResults.filter(r => !r.feasibility.isFeasible).length,
        totalDistance: foxResults.reduce((sum, r) => sum + r.movement.totalDistance, 0)
    };
    
                return `
                    <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 20px;">
                        <!-- Blue Land Summary -->
                        <div style="background: #2a2a2a; padding: 20px; border-radius: 8px; border: 2px solid #ffb000;">
                            <h4 style="margin: 0 0 15px 0; color: #ffb000; font-size: 18px; text-align: center;">
                                <i class="fas fa-flag"></i> Blue Land
                            </h4>
                            <div style="margin-bottom: 15px;">
                                <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 8px;">
                                    <span style="color: #fff; font-size: 14px;">Total Units:</span>
                                    <span style="color: #ffb000; font-size: 18px; font-weight: bold;">${blueStats.total}</span>
                                </div>
                                <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 8px;">
                                    <span style="color: #fff; font-size: 14px;">Feasible:</span>
                                    <span style="color: #00ff00; font-size: 18px; font-weight: bold;">${blueStats.feasible}</span>
                                </div>
                                <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 8px;">
                                    <span style="color: #fff; font-size: 14px;">Blocked:</span>
                                    <span style="color: #ff0000; font-size: 18px; font-weight: bold;">${blueStats.blocked}</span>
                                </div>
                                <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 8px;">
                                    <span style="color: #fff; font-size: 14px;">Total Distance:</span>
                                    <span style="color: #ffb000; font-size: 18px; font-weight: bold;">${Math.round(blueStats.totalDistance)} km</span>
                                </div>
                            </div>
                            ${blueStats.feasible > 0 ? `
                                <div style="background: #00ff00; color: #000; padding: 10px; border-radius: 4px; font-size: 12px; text-align: center; font-weight: bold;">
                                    ✅ ${blueStats.feasible} unit(s) can execute movement orders successfully
                                </div>
                            ` : blueStats.blocked > 0 ? `
                                <div style="background: #ff0000; color: #fff; padding: 10px; border-radius: 4px; font-size: 12px; text-align: center; font-weight: bold;">
                                    ❌ All ${blueStats.blocked} unit(s) blocked by terrain or insufficient MP
                                </div>
                            ` : `
                                <div style="background: #666; color: #fff; padding: 10px; border-radius: 4px; font-size: 12px; text-align: center;">
                                    No movement orders for Blue Land units
                                </div>
                            `}
                        </div>
                        
                        <!-- Fox Land Summary -->
                        <div style="background: #2a2a2a; padding: 20px; border-radius: 8px; border: 2px solid #ffb000;">
                            <h4 style="margin: 0 0 15px 0; color: #ffb000; font-size: 18px; text-align: center;">
                                <i class="fas fa-flag"></i> Fox Land
                            </h4>
                            <div style="margin-bottom: 15px;">
                                <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 8px;">
                                    <span style="color: #fff; font-size: 14px;">Total Units:</span>
                                    <span style="color: #ffb000; font-size: 18px; font-weight: bold;">${foxStats.total}</span>
                                </div>
                                <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 8px;">
                                    <span style="color: #fff; font-size: 14px;">Feasible:</span>
                                    <span style="color: #00ff00; font-size: 18px; font-weight: bold;">${foxStats.feasible}</span>
                                </div>
                                <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 8px;">
                                    <span style="color: #fff; font-size: 14px;">Blocked:</span>
                                    <span style="color: #ff0000; font-size: 18px; font-weight: bold;">${foxStats.blocked}</span>
                                </div>
                                <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 8px;">
                                    <span style="color: #fff; font-size: 14px;">Total Distance:</span>
                                    <span style="color: #ffb000; font-size: 18px; font-weight: bold;">${Math.round(foxStats.totalDistance)} km</span>
                                </div>
                            </div>
                            ${foxStats.feasible > 0 ? `
                                <div style="background: #00ff00; color: #000; padding: 10px; border-radius: 4px; font-size: 12px; text-align: center; font-weight: bold;">
                                    ✅ ${foxStats.feasible} unit(s) can execute movement orders successfully
                                </div>
                            ` : foxStats.blocked > 0 ? `
                                <div style="background: #ff0000; color: #fff; padding: 10px; border-radius: 4px; font-size: 12px; text-align: center; font-weight: bold;">
                                    ❌ All ${foxStats.blocked} unit(s) blocked by terrain or insufficient MP
                                </div>
                            ` : `
                                <div style="background: #666; color: #fff; padding: 10px; border-radius: 4px; font-size: 12px; text-align: center;">
                                    No movement orders for Fox Land units
                                </div>
                            `}
                        </div>
                    </div>
                `;
}

/**
 * Get status color for feasibility
 */
function getStatusColor(status) {
    switch (status) {
        case 'feasible':
            return '#00ff00'; // Green
        case 'moderate':
            return '#ffff00'; // Yellow
        case 'high_consumption':
            return '#ffaa00'; // Orange
        case 'insufficient_mp':
            return '#ff0000'; // Red
        case 'terrain_blocked':
            return '#ff0000'; // Red - Impassable
        default:
            return '#4299e1'; // Blue
    }
}

// Make function globally available
window.runMilitaryAdjudication = runMilitaryAdjudication;

