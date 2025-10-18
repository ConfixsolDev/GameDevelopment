/**
 * Military Adjudication - Clean, simple implementation
 * Calls backend to analyze all token movements and displays comprehensive results
 */

/**
 * Run military adjudication for all tokens
 */
async function runMilitaryAdjudication() {
    console.log('🎯 Running military adjudication...');
    
    // Show loading notification
    if (typeof toastr !== 'undefined') {
        toastr.info('Analyzing all token movements...', 'Military Adjudication');
    }
    
    try {
        const response = await fetch('/GamePlay/AdjudicateAllMovements', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' }
        });
        
        const result = await response.json();
        
        if (result.success) {
            console.log('✅ Adjudication completed:', result);
            
            // Update route visualizations on map
            updateRouteVisualizations(result.results);
            
            // Show comprehensive results modal
            showAdjudicationResultsModal(result);
            
            if (typeof toastr !== 'undefined') {
                toastr.success(`Analyzed ${result.summary.totalTokens} token movements`, 'Adjudication Complete');
            }
        } else {
            console.error('❌ Adjudication failed:', result.message);
            if (typeof toastr !== 'undefined') {
                toastr.error(result.message, 'Adjudication Failed');
            } else {
                console.error('Adjudication failed: ' + result.message);
            }
        }
    } catch (error) {
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
 * Show comprehensive adjudication results modal
 */
function showAdjudicationResultsModal(data) {
    // Remove existing modal if any
    const existingModal = document.getElementById('militaryAdjudicationModal');
    if (existingModal) {
        existingModal.remove();
    }
    
    const modal = document.createElement('div');
    modal.id = 'militaryAdjudicationModal';
    modal.className = 'gameplay-modal';
    modal.style.display = 'flex';
    
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
                    ` : ''}
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
                            <div style="font-size: 14px; color: #ffaa00; font-weight: bold;">${result.terrainAnalysis.elevationGain}m</div>
                            <div style="font-size: 10px; color: #666;">Elevation Gain</div>
                        </div>
                        <div style="text-align: center;">
                            <div style="font-size: 14px; color: #ffaa00; font-weight: bold;">${result.terrainAnalysis.difficulty}</div>
                            <div style="font-size: 10px; color: #666;">Difficulty</div>
                        </div>
                        <div style="text-align: center;">
                            <div style="font-size: 14px; color: #ffaa00; font-weight: bold;">${result.terrainAnalysis.elevationPoints}</div>
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
                
                <!-- Recommendations -->
                ${result.recommendations.length > 0 ? `
                    <div>
                        <h5 style="color: #00ff00; font-size: 14px; margin-bottom: 10px;">Tactical Recommendations</h5>
                        <div class="recommendations-list">
                            ${result.recommendations.map(rec => {
                                const priorityColor = rec.priority === 'high' ? '#ff0000' : 
                                                     rec.priority === 'medium' ? '#ffaa00' : '#00ff00';
                                return `
                                    <div style="display: flex; gap: 10px; padding: 10px; background: #1a1a1a; margin: 5px 0; border-left: 3px solid ${priorityColor}; border-radius: 4px;">
                                        <div style="color: ${priorityColor}; font-weight: bold; min-width: 60px; font-size: 11px; text-transform: uppercase;">
                                            ${rec.priority}
                                        </div>
                                        <div style="color: #ccc; font-size: 12px; flex: 1;">
                                            ${rec.message}
                                        </div>
                                    </div>
                                `;
                            }).join('')}
                        </div>
                    </div>
                ` : ''}
            </div>
        `;
    });
    
    modal.innerHTML = `
        <div class="gameplay-modal-content" style="width: 1000px; max-width: 95vw; max-height: 90vh;">
            <div class="gameplay-modal-header">
                <h2 style="margin: 0; display: flex; align-items: center; gap: 10px;">
                    <i class="fas fa-chart-line"></i> 
                    Military Movement Adjudication
                </h2>
                <button class="gameplay-modal-close" onclick="this.closest('.gameplay-modal').remove()">&times;</button>
            </div>
            
            <div class="gameplay-modal-body" style="max-height: 75vh; overflow-y: auto; padding: 20px;">
                <!-- Executive Summary -->
                <div class="executive-summary" style="background: #1a1a1a; padding: 20px; border-radius: 8px; margin-bottom: 25px; border: 2px solid #00ff00;">
                    <h3 style="margin: 0 0 15px 0; color: #00ff00; font-size: 16px;">
                        <i class="fas fa-clipboard-check"></i> Executive Summary
                    </h3>
                    <div style="display: grid; grid-template-columns: repeat(5, 1fr); gap: 15px;">
                        <div style="text-align: center; padding: 15px; background: #2a2a2a; border-radius: 6px;">
                            <div style="font-size: 11px; color: #666; text-transform: uppercase; margin-bottom: 8px;">Total Units</div>
                            <div style="font-size: 28px; color: #00ff00; font-weight: bold;">${data.summary.totalTokens}</div>
                        </div>
                        <div style="text-align: center; padding: 15px; background: #2a2a2a; border-radius: 6px;">
                            <div style="font-size: 11px; color: #666; text-transform: uppercase; margin-bottom: 8px;">Feasible</div>
                            <div style="font-size: 28px; color: #00ff00; font-weight: bold;">${data.summary.feasibleCount}</div>
                        </div>
                        <div style="text-align: center; padding: 15px; background: #2a2a2a; border-radius: 6px;">
                            <div style="font-size: 11px; color: #666; text-transform: uppercase; margin-bottom: 8px;">Blocked</div>
                            <div style="font-size: 28px; color: #ff0000; font-weight: bold;">${data.summary.blockedCount}</div>
                        </div>
                        <div style="text-align: center; padding: 15px; background: #2a2a2a; border-radius: 6px;">
                            <div style="font-size: 11px; color: #666; text-transform: uppercase; margin-bottom: 8px;">Total Distance</div>
                            <div style="font-size: 28px; color: #00ff00; font-weight: bold;">${Math.round(data.summary.totalDistance)} km</div>
                        </div>
                        <div style="text-align: center; padding: 15px; background: #2a2a2a; border-radius: 6px;">
                            <div style="font-size: 11px; color: #666; text-transform: uppercase; margin-bottom: 8px;">Avg MP Usage</div>
                            <div style="font-size: 28px; color: ${data.summary.avgMPUtilization > 80 ? '#ffaa00' : '#00ff00'}; font-weight: bold;">${Math.round(data.summary.avgMPUtilization)}%</div>
                        </div>
                    </div>
                </div>
                
                <!-- Individual Token Results -->
                <div class="token-results">
                    <h3 style="margin: 0 0 15px 0; color: #00ff00; font-size: 16px;">
                        <i class="fas fa-list"></i> Detailed Analysis
                    </h3>
                    ${resultsHTML}
                </div>
            </div>
            
            <div class="gameplay-modal-footer" style="display: flex; justify-content: space-between; align-items: center; padding: 15px; background: #2a2a2a; border-top: 1px solid #444;">
                <div style="color: #666; font-size: 12px;">
                    Analysis completed at ${new Date(data.timestamp).toLocaleString()}
                </div>
                <button class="gameplay-btn" onclick="this.closest('.gameplay-modal').remove()">
                    <i class="fas fa-check"></i> Close
                </button>
            </div>
        </div>
    `;
    
    document.body.appendChild(modal);
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

