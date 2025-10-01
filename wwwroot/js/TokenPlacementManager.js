/**
 * TokenPlacementManager - Handles token placement, movement, and deletion on the map
 */
class TokenPlacementManager {
    constructor(map, notificationCallback) {
        this.map = map;
        this.notificationCallback = notificationCallback || this.defaultNotification;
        this.placedTokens = new Map(); // tokenId -> { marker, coverageAreas, token }
        this.isPlacementMode = false;
        this.selectedTokenForPlacement = null;
        this.isMovingToken = false;
        this.movingToken = null;
        this.tempMarker = null;
		this.isDraggingMarker = false;
		this.suppressNextClick = false;
        
        this.setupEventListeners();
    }

    /**
     * Setup event listeners for map interactions
     */
    setupEventListeners() {
        // Map click events
        this.map.on('click', (e) => {
            if (this.isPlacementMode && this.selectedTokenForPlacement) {
                this.placeTokenAtLocation(e.latlng);
            } else if (this.isMovingToken && this.movingToken) {
                this.moveTokenToLocation(e.latlng);
            }
        });

        // Map mouse move events for preview
        this.map.on('mousemove', (e) => {
            if (this.isPlacementMode && this.selectedTokenForPlacement) {
                this.updatePlacementPreview(e.latlng);
            }
        });

        // Map mouse leave events
        this.map.on('mouseout', () => {
            if (this.tempMarker) {
                this.map.removeLayer(this.tempMarker);
                this.tempMarker = null;
            }
        });
    }

    /**
     * Start token placement mode
     */
    startPlacementMode(token) {
        this.isPlacementMode = true;
        this.selectedTokenForPlacement = token;
        this.map.getContainer().style.cursor = 'crosshair';
        this.notificationCallback(`Click on the map to place "${token.name}"`, 'info');
    }

    /**
     * Cancel placement mode
     */
    cancelPlacementMode() {
        this.isPlacementMode = false;
        this.selectedTokenForPlacement = null;
        this.map.getContainer().style.cursor = 'default';
        
        if (this.tempMarker) {
            this.map.removeLayer(this.tempMarker);
            this.tempMarker = null;
        }
    }

    /**
     * Update placement preview marker
     */
    updatePlacementPreview(latlng) {
        if (!this.selectedTokenForPlacement) return;

        // Remove existing temp marker
		if (this.tempMarker) {
			// Also remove any previous coverage preview linked to the temp marker
			if (this.tempMarker.coveragePreview) {
				this.map.removeLayer(this.tempMarker.coveragePreview);
			}
			this.map.removeLayer(this.tempMarker);
		}

        // Create preview marker
        const icon = this.createTokenIcon(this.selectedTokenForPlacement);
        this.tempMarker = L.marker(latlng, { icon: icon })
            .addTo(this.map);

		// Add coverage area preview as a plus shape if token has radius
		if (this.selectedTokenForPlacement.coverageRadius && this.selectedTokenForPlacement.coverageRadius > 0) {
			const radiusMeters = this.selectedTokenForPlacement.coverageRadius * 1000;
			const plus = this.createPlusCoverage(latlng, radiusMeters, '#3388ff', 3, 0.5, '5, 5');
			this.tempMarker.coveragePreview = plus.addTo(this.map);
		}
    }

    /**
     * Place token at specified location
     */
    async placeTokenAtLocation(latlng) {
        if (!this.selectedTokenForPlacement) return;

        try {
            const response = await fetch('/GamePlay/PlaceTokenOnMap', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    tokenId: this.selectedTokenForPlacement.id,
                    latitude: latlng.lat,
                    longitude: latlng.lng
                })
            });

            const result = await response.json();

            if (result.success) {
                // Create permanent marker
                const marker = this.createTokenMarker(this.selectedTokenForPlacement, latlng);
                
                // Add to token layer for visibility control
                if (window.tokenLayer) {
                    window.tokenLayer.addLayer(marker);
                } else {
                    this.map.addLayer(marker);
                }

                // Create coverage areas with force type color
                if (result.areaCoverages && result.areaCoverages.length > 0) {
                    this.createCoverageAreas(result.areaCoverages, latlng, this.selectedTokenForPlacement.forceType);
                }

                // Store token info
                this.placedTokens.set(this.selectedTokenForPlacement.id, {
                    marker: marker,
                    token: this.selectedTokenForPlacement,
                    coverageAreas: result.areaCoverages || []
                });

                // no client-side cache

                // Clean up
                this.cancelPlacementMode();
                this.notificationCallback(result.message, 'success');
            } else {
                this.notificationCallback(result.message, 'error');
            }
        } catch (error) {
            console.error('Error placing token:', error);
            this.notificationCallback('Error placing token', 'error');
        }
    }

    /**
     * Start token movement mode
     */
    startMoveMode(tokenId) {
        const tokenInfo = this.placedTokens.get(tokenId);
        if (!tokenInfo) return;

        this.isMovingToken = true;
        this.movingToken = tokenInfo;
        this.map.getContainer().style.cursor = 'move';
        this.notificationCallback(`Click on the map to move "${tokenInfo.token.name}"`, 'info');
    }

    /**
     * Cancel move mode
     */
    cancelMoveMode() {
        this.isMovingToken = false;
        this.movingToken = null;
        this.map.getContainer().style.cursor = 'default';
    }

    /**
     * Move token to new location
     */
    async moveTokenToLocation(latlng) {
        if (!this.movingToken) return;

        try {
            const response = await fetch('/GamePlay/MoveToken', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    tokenId: this.movingToken.token.id,
                    latitude: latlng.lat,
                    longitude: latlng.lng
                })
            });

            const result = await response.json();

            if (result.success) {
                // Update marker position
                this.movingToken.marker.setLatLng(latlng);

                // Update coverage areas
                if (result.areaCoverages && result.areaCoverages.length > 0) {
                    this.updateCoverageAreas(this.movingToken.token.id, result.areaCoverages, latlng);
                }

                // Clear existing movement history visuals
                this.clearMovementHistory(this.movingToken.token.id);

                // Refresh movement history to show the new path
                await this.refreshMovementHistory(this.movingToken.token.id);

                this.notificationCallback(result.message, 'success');
            } else {
                this.notificationCallback(result.message, 'error');
            }
        } catch (error) {
            console.error('Error moving token:', error);
            this.notificationCallback('Error moving token', 'error');
        } finally {
            this.cancelMoveMode();
        }
    }

    /**
     * Remove token from map permanently
     */
    async removeTokenFromMap(tokenId) {
        const tokenInfo = this.placedTokens.get(tokenId);
        if (!tokenInfo) return;

        if (!confirm(`Are you sure you want to remove "${tokenInfo.token.name}" from the map?`)) {
            return;
        }

        try {
            const response = await fetch('/GamePlay/RemoveTokenFromMap', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    tokenId: tokenId
                })
            });

            const result = await response.json();

            if (result.success) {
                // Remove marker from map
                this.map.removeLayer(tokenInfo.marker);

                // Remove coverage areas
                this.removeCoverageAreas(tokenId);

                // Remove from tracking
                this.placedTokens.delete(tokenId);

                this.notificationCallback(result.message, 'success');
            } else {
                this.notificationCallback(result.message, 'error');
            }
        } catch (error) {
            console.error('Error removing token:', error);
            this.notificationCallback('Error removing token', 'error');
        }
    }

    /**
     * Create token marker
     */
	createTokenMarker(token, latlng) {
        const icon = this.createTokenIcon(token);
		const marker = L.marker(latlng, { icon: icon, draggable: true, autoPan: true });

        // Add click event for token details
		marker.on('click', () => {
			if (this.suppressNextClick) {
				this.suppressNextClick = false;
				return;
			}
			if (!this.isDraggingMarker) {
				this.showTokenDetails(token);
			}
		});

        // Add context menu for token actions
        marker.on('contextmenu', (e) => {
            this.showTokenContextMenu(e, token);
        });

		// Enable drag-to-move behavior with enhanced planning
		marker.on('dragstart', (e) => {
			console.log('🎯 Drag start event triggered for token:', token.name);
			this.isDraggingMarker = true;
			this.originalPosition = e.target.getLatLng();
			console.log('🎯 Original position set to:', this.originalPosition);
			this.startDragToMove(token, e.target);
		});

		marker.on('drag', (e) => {
			if (this.isDraggingMarker) {
				this.updateDragPreview(token, e.target.getLatLng());
			}
		});

		marker.on('dragend', async (e) => {
			try {
				console.log('🎯 Drag end event triggered for token:', token.name);
				const newLatLng = e.target.getLatLng();
				console.log('🎯 New position:', newLatLng);
				this.endDragToMove(token, e.target, newLatLng);
			} catch (err) {
				console.error('Error in drag end:', err);
				this.notificationCallback('Error processing move', 'error');
			} finally {
				this.suppressNextClick = true;
				this.isDraggingMarker = false;
			}
		});

        return marker;
    }

    /**
     * Create token icon
     */
    /**
     * Start drag-to-move planning
     */
    startDragToMove(token, marker) {
        this.dragPreview = {
            token: token,
            marker: marker,
            routeLine: null,
            previewCard: null,
            originalPosition: this.originalPosition
        };
        
        // Create ghost route line
        this.dragPreview.routeLine = L.polyline([this.originalPosition], {
            color: '#ff6b6b',
            weight: 2,
            opacity: 0.6,
            dashArray: '5, 5'
        }).addTo(this.map);
        
        // Show live info tooltip
        this.showDragTooltip(token, this.originalPosition);
    }

    /**
     * Update drag preview during drag
     */
    updateDragPreview(token, currentPosition) {
        if (!this.dragPreview) return;
        
        // Update route line
        this.dragPreview.routeLine.setLatLngs([this.originalPosition, currentPosition]);
        
        // Calculate distance and ETA
        const distance = this.originalPosition.distanceTo(currentPosition) / 1000; // km
        const etaHours = distance / 20; // Assume 20 km/h base speed
        
        // Update tooltip
        this.updateDragTooltip(distance, etaHours);
    }

    /**
     * End drag-to-move and open planning form directly
     */
    endDragToMove(token, marker, newPosition) {
        console.log('🎯 endDragToMove called for token:', token.name);
        console.log('🎯 Original position:', this.originalPosition);
        console.log('🎯 New position:', newPosition);
        
        if (!this.dragPreview) {
            console.log('🎯 No drag preview found, creating basic movement modal');
            // Calculate distance even without drag preview
            const distance = this.originalPosition ? this.originalPosition.distanceTo(newPosition) / 1000 : 0;
            this.showConfirmMoveModal(token, distance);
            return;
        }
        
        // Remove route line
        if (this.dragPreview.routeLine) {
            this.map.removeLayer(this.dragPreview.routeLine);
        }
        
        // Remove tooltip
        this.hideDragTooltip();
        
        // Calculate final distance (only distance is calculated automatically)
        const distance = this.originalPosition.distanceTo(newPosition) / 1000;
        console.log('🎯 Calculated distance:', distance, 'km');
        
        // Open planning form directly (no preview card)
        this.showConfirmMoveModal(token, distance);
    }

    /**
     * Show drag tooltip
     */
    showDragTooltip(token, position) {
        this.dragTooltip = L.tooltip({
            permanent: true,
            direction: 'top',
            offset: [0, -10]
        }).setContent(`
            <div class="drag-tooltip">
                <div class="unit-name">${token.name}</div>
                <div class="distance">Distance: <span id="dragDistance">0</span> km</div>
                <div class="eta">ETA: <span id="dragETA">0000</span> hours</div>
            </div>
        `).setLatLng(position).addTo(this.map);
    }

    /**
     * Update drag tooltip
     */
    updateDragTooltip(distance, etaHours) {
        if (this.dragTooltip) {
            const distanceSpan = document.getElementById('dragDistance');
            const etaSpan = document.getElementById('dragETA');
            
            if (distanceSpan) distanceSpan.textContent = distance.toFixed(1);
            if (etaSpan) {
                // Convert hours to military time format (e.g., 2.5 hours = 0230 hours)
                const totalMinutes = Math.round(etaHours * 60);
                const hours = Math.floor(totalMinutes / 60);
                const minutes = totalMinutes % 60;
                const militaryTime = String(hours).padStart(2, '0') + String(minutes).padStart(2, '0');
                etaSpan.textContent = militaryTime;
            }
        }
    }

    /**
     * Hide drag tooltip
     */
    hideDragTooltip() {
        if (this.dragTooltip) {
            this.map.removeLayer(this.dragTooltip);
            this.dragTooltip = null;
        }
    }

    /**
     * Show preview card after drag
     */
    showPreviewCard(token, marker, position, distance, etaHours) {
        const previewCard = document.createElement('div');
        previewCard.className = 'preview-card';
        previewCard.innerHTML = `
            <div class="preview-header">
                <h4>${token.name}</h4>
                <button class="close-preview" onclick="tokenPlacementManager.closePreviewCard()">&times;</button>
            </div>
            <div class="preview-content">
                <div class="preview-info">
                    <div class="info-item">
                        <span class="label">Distance:</span>
                        <span class="value">${distance.toFixed(1)} km</span>
                    </div>
                    <div class="info-item">
                        <span class="label">Estimated ETA:</span>
                        <span class="value">${Math.floor(etaHours).toString().padStart(2, '0')}${Math.round((etaHours % 1) * 60).toString().padStart(2, '0')} hours</span>
                    </div>
                </div>
                <div class="preview-actions">
                    <button class="btn-confirm" onclick="tokenPlacementManager.confirmMove('${token.id}')">
                        Plan Move
                    </button>
                    <button class="btn-edit" onclick="tokenPlacementManager.editRoute('${token.id}')">
                        Edit Route
                    </button>
                    <button class="btn-cancel" onclick="tokenPlacementManager.cancelMove('${token.id}')">
                        Cancel
                    </button>
                </div>
            </div>
        `;
        
        // Position the card above the marker
        const markerPoint = this.map.latLngToContainerPoint(position);
        previewCard.style.position = 'absolute';
        previewCard.style.left = (markerPoint.x - 100) + 'px';
        previewCard.style.top = (markerPoint.y - 120) + 'px';
        previewCard.style.zIndex = '1000';
        
        document.body.appendChild(previewCard);
        
        // Store reference for cleanup
        this.dragPreview.previewCard = previewCard;
    }

    /**
     * Close preview card
     */
    closePreviewCard() {
        if (this.dragPreview?.previewCard) {
            this.dragPreview.previewCard.remove();
            this.dragPreview.previewCard = null;
        }
    }

    /**
     * Confirm move and open detailed modal
     */
    async confirmMove(tokenId) {
        const token = this.placedTokens.get(tokenId)?.token;
        if (!token) return;
        
        // Remove preview card
        this.closePreviewCard();
        
        // Open confirm move modal
        this.showConfirmMoveModal(token);
    }

    /**
     * Edit route
     */
    editRoute(tokenId) {
        const token = this.placedTokens.get(tokenId)?.token;
        if (!token) return;
        
        // Remove preview card
        this.closePreviewCard();
        
        // Open route editor
        if (window.scenarioPlanning) {
            window.scenarioPlanning.startRouteDrawing();
        }
    }

    /**
     * Cancel move and revert position
     */
    cancelMove(tokenId) {
        const tokenData = this.placedTokens.get(tokenId);
        if (!tokenData) return;
        
        // Revert marker to original position
        tokenData.marker.setLatLng(this.originalPosition);
        
        // Remove preview card
        this.closePreviewCard();
        
        // Clean up
        this.dragPreview = null;
        
        this.notificationCallback('Move cancelled', 'info');
    }

    /**
     * Show confirm move modal
     */
    showConfirmMoveModal(token, calculatedDistance = null) {
        console.log('🎯 showConfirmMoveModal called for token:', token.name, 'distance:', calculatedDistance);
        
        // Remove any existing modal first
        const existingModal = document.getElementById('confirmMoveModal');
        if (existingModal) {
            existingModal.remove();
        }
        
        const modal = document.createElement('div');
        modal.className = 'gameplay-modal';
        modal.id = 'confirmMoveModal';
        modal.innerHTML = `
            <div class="gameplay-modal-content">
                <div class="gameplay-modal-header">
                    <h3><i class="fas fa-route"></i> Movement Planning - ${token.name}</h3>
                    <button class="gameplay-modal-close" onclick="this.closest('.gameplay-modal').remove()">&times;</button>
                </div>
                <div class="gameplay-modal-body">
                    <div class="data-entry-container">
                        <div class="data-entry-form-section">
                            <!-- Movement Planning Header -->
                            <div class="brigade-header-section">
                                <div class="brigade-name-display">
                                    <h5><i class="fas fa-route"></i> Movement Planning — ${token.name}</h5>
                                </div>
                            </div>
                            
                            <!-- Movement Planning Form -->
                            <div class="brigade-data-form">
                                <!-- Movement Planning Section -->
                                <div class="data-tab-content active">
                                    <div class="tab-content-header">
                                        <h6><i class="fas fa-route"></i> Movement Planning</h6>
                                        <p class="text-muted">Configure movement parameters and timing</p>
                                    </div>
                                    
                                    <!-- Movement Mode - Full Width -->
                                    <div class="form-group">
                                        <label>Movement Mode</label>
                                        <div class="form-check-group" style="display: flex; gap: 15px; margin-top: 8px;">
                                            <div class="form-check">
                                                <input class="form-check-input" type="radio" name="movementMode" id="marchMode" value="march" checked style="margin-right: 5px;">
                                                <label class="form-check-label" for="marchMode" style="color: #ccc; font-size: 12px;">Road March</label>
                                            </div>
                                            <div class="form-check">
                                                <input class="form-check-input" type="radio" name="movementMode" id="normalMode" value="normal" style="margin-right: 5px;">
                                                <label class="form-check-label" for="normalMode" style="color: #ccc; font-size: 12px;">Normal</label>
                                            </div>
                                            <div class="form-check">
                                                <input class="form-check-input" type="radio" name="movementMode" id="stealthMode" value="stealth" style="margin-right: 5px;">
                                                <label class="form-check-label" for="stealthMode" style="color: #ccc; font-size: 12px;">Stealth</label>
                                            </div>
                                        </div>
                                    </div>
                                    
                                    <!-- Row 1: Proceed & Start Offset -->
                                    <div class="row">
                                        <div class="col-md-6">
                                            <div class="form-group">
                                                <label for="startTurn" style="color: #ccc; font-size: 12px; margin-bottom: 5px;">Proceed</label>
                                                <select class="form-control" id="startTurn">
                                                    <option value="1">PR</option>
                                                    <option value="2">PR</option>
                                                    <option value="3">PR</option>
                                                    <option value="4">PR</option>
                                                    <option value="5">PR</option>
                                                </select>
                                            </div>
                                        </div>
                                        <div class="col-md-6">
                                            <div class="form-group">
                                                <label for="startOffset" style="color: #ccc; font-size: 12px; margin-bottom: 5px;">Start Offset (hours)</label>
                                                <select class="form-control" id="startOffset">
                                                    <option value="0">0 hours</option>
                                                    <option value="1">1 hour</option>
                                                    <option value="2">2 hours</option>
                                                    <option value="3">3 hours</option>
                                                    <option value="4">4 hours</option>
                                                    <option value="5">5 hours</option>
                                                    <option value="6">6 hours</option>
                                                    <option value="7">7 hours</option>
                                                    <option value="8">8 hours</option>
                                                    <option value="9">9 hours</option>
                                                    <option value="10">10 hours</option>
                                                    <option value="11">11 hours</option>
                                                    <option value="12">12 hours</option>
                                                    <option value="13">13 hours</option>
                                                    <option value="14">14 hours</option>
                                                    <option value="15">15 hours</option>
                                                </select>
                                            </div>
                                        </div>
                                    </div>
                                    
                                    <!-- Row 2: Planned ETA & Movement Speed -->
                                    <div class="row">
                                        <div class="col-md-6">
                                            <div class="form-group">
                                                <label for="plannedETA" style="color: #ccc; font-size: 12px; margin-bottom: 5px;">Planned ETA *</label>
                                                <select class="form-control" id="plannedETA" required>
                                                    <option value="">Select ETA</option>
                                                    <option value="1">0600 hours</option>
                                                    <option value="2">0800 hours</option>
                                                    <option value="3">1000 hours</option>
                                                    <option value="4">1200 hours</option>
                                                    <option value="5">1400 hours</option>
                                                    <option value="6">1600 hours</option>
                                                    <option value="7">1700 hours</option>
                                                    <option value="8">1800 hours</option>
                                                    <option value="9">1900 hours</option>
                                                    <option value="10">2000 hours</option>
                                                    <option value="11">2100 hours</option>
                                                    <option value="12">2200 hours</option>
                                                    <option value="13">2300 hours</option>
                                                    <option value="14">2400 hours</option>
                                                    <option value="15">0100 hours</option>
                                                </select>
                                                <small class="text-muted" style="font-size: 11px;">Estimated arrival time in military format (required)</small>
                                            </div>
                                        </div>
                                        <div class="col-md-6">
                                            <div class="form-group">
                                                <label for="movementSpeed" style="color: #ccc; font-size: 12px; margin-bottom: 5px;">Movement Speed (km/h) *</label>
                                                <select class="form-control" id="movementSpeed" required>
                                                    <option value="">Select speed</option>
                                                    <option value="1">1 km/h</option>
                                                    <option value="2">2 km/h</option>
                                                    <option value="3">3 km/h</option>
                                                    <option value="4">4 km/h</option>
                                                    <option value="5">5 km/h</option>
                                                    <option value="8">8 km/h</option>
                                                    <option value="10">10 km/h</option>
                                                    <option value="12">12 km/h</option>
                                                    <option value="14">14 km/h</option>
                                                    <option value="18">18 km/h</option>
                                                    <option value="30">30 km/h</option>
                                                    <option value="40">40 km/h</option>
                                                </select>
                                                <small class="text-muted" style="font-size: 11px;">Unit's movement speed (required)</small>
                                            </div>
                                        </div>
                                    </div>
                                    
                                    <!-- Row 3: Engagement Rule & Shared with Allies -->
                                    <div class="row">
                                        <div class="col-md-6">
                                            <div class="form-group">
                                                <label for="engagementRule" style="color: #ccc; font-size: 12px; margin-bottom: 5px;">Engagement Rule *</label>
                                                <select class="form-control" id="engagementRule" required>
                                                    <option value="">Select engagement rule</option>
                                                    <option value="avoid">Avoid Strongpoints</option>
                                                    <option value="engage">Engage If Encountered</option>
                                                    <option value="hold">Hold If Supply < 50%</option>
                                                </select>
                                            </div>
                                        </div>
                                        <div class="col-md-6">
                                            <div class="form-group">
                                                <label style="color: #ccc; font-size: 12px; margin-bottom: 5px;">Options</label>
                                                <div class="form-check" style="margin-top: 8px;">
                                                    <input class="form-check-input" type="checkbox" id="sharedOrder" style="margin-right: 8px;">
                                                    <label class="form-check-label" for="sharedOrder" style="color: #ccc; font-size: 12px;">Shared with Allies</label>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                    
                                    <!-- Notes - Full Width -->
                                    <div class="form-group">
                                        <label for="moveNotes" style="color: #ccc; font-size: 12px; margin-bottom: 5px;">Notes</label>
                                        <textarea class="form-control" id="moveNotes" rows="3" placeholder="Planning notes..." style="resize: vertical;"></textarea>
                                    </div>
                                </div>
                            </div>
                            
                            <!-- Action Buttons -->
                            <div class="add-unit-section">
                                <button type="button" class="btn btn-outline-secondary" onclick="this.closest('.gameplay-modal').remove()" style="margin-right: 10px;">
                                    <i class="fas fa-times"></i> Cancel
                                </button>
                                <button type="button" class="btn btn-primary" onclick="window.tokenPlacementManager.saveMoveOrder('${token.id}')">
                                    <i class="fas fa-check"></i> Confirm Move
                                </button>
                            </div>
                        </div>
                    </div>
                </div>
                <div class="gameplay-modal-footer">
                    <button class="gameplay-btn" onclick="this.closest('.gameplay-modal').remove()">Close</button>
                </div>
            </div>
            
            <style>
            /* Modal Centering */
            .gameplay-modal {
                display: flex !important;
                align-items: center;
                justify-content: center;
            }

            .gameplay-modal-content {
                margin: 0;
                max-width: 800px;
                width: 90%;
            }

            /* Dark Theme Data Entry Form - Exact Copy */
            .data-entry-container {
                background: #1a1a1a;
                border-radius: 8px;
                padding: 20px;
            }

            .brigade-header-section {
                margin-bottom: 20px;
                padding: 15px;
                background: #2a2a2a;
                border-radius: 8px;
                border-left: 4px solid #00d4ff;
            }

            .brigade-name-display {
                display: flex;
                justify-content: space-between;
                align-items: center;
            }

            .brigade-name-display h5 {
                color: #00d4ff;
                margin: 0;
                font-weight: 600;
            }

            .data-tab-content {
                display: none;
                padding: 20px;
                border: 1px solid #333;
                border-radius: 8px;
                background: #2a2a2a;
                margin-top: 10px;
            }

            .data-tab-content.active {
                display: block;
            }

            .tab-content-header {
                margin-bottom: 15px;
                padding-bottom: 10px;
                border-bottom: 1px solid #444;
            }

            .tab-content-header h6 {
                color: #00d4ff;
                margin: 0 0 5px 0;
                font-weight: 600;
            }

            .tab-content-header .text-muted {
                color: #888 !important;
                font-size: 13px;
            }

            .btn {
                padding: 10px 20px;
                border-radius: 6px;
                font-weight: 600;
                transition: all 0.3s ease;
            }

            .btn-primary {
                background: #00d4ff;
                border-color: #00d4ff;
                color: #000;
            }

            .btn-primary:hover {
                background: #00b8e6;
                border-color: #00b8e6;
                color: #000;
            }

            .btn-outline-secondary {
                background: transparent;
                border-color: #666;
                color: #ccc;
            }

            .btn-outline-secondary:hover {
                background: #666;
                border-color: #666;
                color: #fff;
            }

            .form-control {
                background: #1a1a1a;
                border: 1px solid #444;
                color: #fff;
                border-radius: 6px;
                padding: 10px 12px;
            }

            .form-control:focus {
                background: #2a2a2a;
                border-color: #00d4ff;
                color: #fff;
                box-shadow: 0 0 0 0.2rem rgba(0, 212, 255, 0.25);
            }

            .text-muted {
                color: #888 !important;
            }

            .unit-item {
                background: #2a2a2a;
                border: 1px solid #444;
                border-radius: 6px;
                padding: 12px;
                margin-bottom: 8px;
                display: flex;
                justify-content: space-between;
                align-items: center;
                transition: all 0.3s ease;
            }

            .unit-item:hover {
                background: #3a3a3a;
                border-color: #00d4ff;
            }

            .unit-info {
                flex: 1;
                margin-right: 12px;
                overflow: hidden;
            }

            .unit-name1 {
                color: #fff;
                font-weight: 600;
                font-size: 12px;
                margin-bottom: 4px;
                white-space: nowrap;
                overflow: hidden;
                text-overflow: ellipsis;
            }

            .unit-details {
                color: #888;
                font-size: 12px;
                white-space: nowrap;
                overflow: hidden;
                text-overflow: ellipsis;
            }

            .add-unit-section {
                text-align: center;
                padding: 15px;
                border-top: 1px solid #444;
                margin-top: 10px;
            }

            .form-group {
                margin-bottom: 15px;
            }

            .form-group label {
                font-weight: 600;
                color: #495057;
                margin-bottom: 5px;
                font-size: 14px;
            }
            </style>
        `;
        
        document.body.appendChild(modal);
        console.log('🎯 Movement modal added to DOM');
        
        // Make sure the function is globally accessible
        window.tokenPlacementManager = this;
        
        // Show the modal
        modal.style.display = 'block';
        console.log('🎯 Movement modal should now be visible');
    }

    /**
     * Save move order
     */
    async saveMoveOrder(tokenId) {
        console.log('saveMoveOrder called with tokenId:', tokenId);
        const token = this.placedTokens.get(tokenId)?.token;
        if (!token) {
            console.error('Token not found for ID:', tokenId);
            this.notificationCallback('Token not found', 'error');
            return;
        }
        
        // Validate required fields
        const plannedETA = document.getElementById('plannedETA').value;
        const movementSpeed = document.getElementById('movementSpeed').value;
        const engagementRule = document.getElementById('engagementRule').value;
        
        console.log('Form values:', { plannedETA, movementSpeed, engagementRule });
        
        if (!plannedETA || !movementSpeed || !engagementRule) {
            this.notificationCallback('Please select all required fields (marked with *)', 'error');
            return;
        }
        
        const movementMode = document.querySelector('input[name="movementMode"]:checked').value;
        const startTurn = document.getElementById('startTurn').value;
        const startOffset = document.getElementById('startOffset').value;
        const sharedOrder = document.getElementById('sharedOrder').checked;
        const notes = document.getElementById('moveNotes').value;
        
        console.log('All form values:', { 
            movementMode, startTurn, startOffset, sharedOrder, notes,
            plannedETA, movementSpeed, engagementRule 
        });
        
        try {
            console.log('Saving move order with data:', {
                tokenId: tokenId,
                latitude: this.dragPreview.marker.getLatLng().lat,
                longitude: this.dragPreview.marker.getLatLng().lng,
                movementMode: movementMode,
                startTurn: startTurn,
                startOffset: startOffset,
                plannedETA: plannedETA,
                movementSpeed: movementSpeed,
                engagementRule: engagementRule,
                sharedOrder: sharedOrder,
                notes: notes
            });

            const response = await fetch('/GamePlay/MoveToken', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    TokenId: tokenId,
                    Latitude: this.dragPreview.marker.getLatLng().lat,
                    Longitude: this.dragPreview.marker.getLatLng().lng,
                    MovementMode: movementMode,
                    StartTurn: parseInt(startTurn),
                    StartOffset: parseFloat(startOffset),
                    PlannedETA: parseFloat(plannedETA),
                    MovementSpeed: parseFloat(movementSpeed),
                    EngagementRule: engagementRule,
                    SharedOrder: sharedOrder,
                    Notes: notes
                })
            });
            
            const result = await response.json();
            console.log('MoveToken response:', result);
            
            if (result.success) {
                this.notificationCallback('Move order confirmed and saved successfully', 'success');
                
                // Add to scenario planning orders
                if (window.scenarioPlanning) {
                    const moveOrder = {
                        id: this.generateId(),
                        unitId: tokenId,
                        type: 'movement',
                        movementMode: movementMode,
                        startTurn: startTurn,
                        startOffset: startOffset,
                        plannedETA: plannedETA,
                        movementSpeed: movementSpeed,
                        engagementRule: engagementRule,
                        notes: notes,
                        route: [this.originalPosition, this.dragPreview.marker.getLatLng()],
                        timestamp: new Date()
                    };
                    
                    window.scenarioPlanning.orders.push(moveOrder);
                    window.scenarioPlanning.updateOrdersDisplay();
                }
                
                // Show route on map
                this.showConfirmedRoute(tokenId);
                
                // Close modal
                document.getElementById('confirmMoveModal').remove();
                this.dragPreview = null;
            } else {
                this.notificationCallback('Failed to confirm move: ' + result.message, 'error');
                console.error('MoveToken failed:', result);
            }
        } catch (error) {
            console.error('Error saving move order:', error);
            this.notificationCallback('Error saving move order: ' + error.message, 'error');
        }
    }

    /**
     * Show confirmed route on map
     */
    showConfirmedRoute(tokenId) {
        const tokenData = this.placedTokens.get(tokenId);
        if (!tokenData || !this.originalPosition) return;
        
        const routeLine = L.polyline([this.originalPosition, tokenData.marker.getLatLng()], {
            color: '#4299e1',
            weight: 3,
            opacity: 0.8,
            dashArray: '10, 5'
        }).addTo(this.map);
        
        // Add enhanced waypoint marker at endpoint
        const waypointMarker = this.createWaypointMarker(
            { 
                latitude: tokenData.marker.getLatLng().lat, 
                longitude: tokenData.marker.getLatLng().lng,
                createdDate: new Date()
            }, 
            1, 
            []
        );
        waypointMarker.addTo(this.map);
        
        // Store route for cleanup
        if (!tokenData.routeLines) tokenData.routeLines = [];
        if (!tokenData.waypointMarkers) tokenData.waypointMarkers = [];
        tokenData.routeLines.push(routeLine);
        tokenData.waypointMarkers.push(waypointMarker);
    }

    /**
     * Calculate distance between original and current position
     */
    calculateDistance() {
        if (!this.originalPosition || !this.dragPreview?.marker) return 0;
        return this.originalPosition.distanceTo(this.dragPreview.marker.getLatLng()) / 1000;
    }

    /**
     * Calculate ETA based on distance
     */
    calculateETA() {
        const distance = this.calculateDistance();
        return distance / 20; // Assume 20 km/h base speed
    }

    /**
     * Generate unique ID
     */
    generateId() {
        return 'move_' + Date.now() + '_' + Math.random().toString(36).substr(2, 9);
    }

	createTokenIcon(token) {
        // Determine border color based on force type
        let borderColor = '#00ff88'; // Default green
        if (token.forceType) {
            const forceTypeLower = token.forceType.toLowerCase();
            if (forceTypeLower.includes('fox')) {
                borderColor = '#ff0000'; // Red for Fox Land
            } else if (forceTypeLower.includes('blue')) {
                borderColor = '#0000ff'; // Blue for Blue Land
            }
        }

        // Create square token with colored border
        const iconHtml = token.assetImagePath ? 
            `<div class="token-square" style="border-color: ${borderColor};">
                <img src="${token.assetImagePath}" class="token-image" onerror="this.style.display='none'; this.nextElementSibling.style.display='flex';">
                <div class="token-fallback-icon" style="display:none;">
                    <i class="fas fa-crosshairs"></i>
                </div>
             </div>` :
            `<div class="token-square" style="border-color: ${borderColor};">
                <div class="token-fallback-icon">
                    <i class="fas fa-crosshairs"></i>
                </div>
             </div>`;

        return L.divIcon({
            className: 'token-marker-container',
            html: iconHtml,
			iconSize: [50, 50],
			iconAnchor: [25, 25],
            popupAnchor: [0, -25]
        });
    }

    /**
     * Create coverage areas with force-type-based colors
     */
    createCoverageAreas(areaCoverages, centerLatLng, forceType = null) {
        areaCoverages.forEach(coverage => {
			if (coverage.radiusKm) {
				// Determine color based on force type (Fox Land / Blue Land)
				let fillColor, strokeColor;
				if (forceType) {
					const forceTypeLower = forceType.toLowerCase();
					if (forceTypeLower.includes('fox')) {
						fillColor = '#ff0000'; // Red for Fox Land
						strokeColor = '#cc0000'; // Darker red for border
					} else if (forceTypeLower.includes('blue')) {
						fillColor = '#0000ff'; // Blue for Blue Land
						strokeColor = '#0000cc'; // Darker blue for border
					} else {
						fillColor = '#3388ff'; // Default light blue
						strokeColor = '#2266dd';
					}
				} else {
					// Fallback to coverage type color
					fillColor = this.getCoverageColor(coverage.coverageType);
					strokeColor = fillColor;
				}

				// Create a filled circle with semi-transparent fill
				const circle = L.circle(centerLatLng, {
					radius: coverage.radiusKm * 1000, // Convert km to meters
					color: strokeColor,
					fillColor: fillColor,
					fillOpacity: 0.2, // Semi-transparent
					opacity: 0.6,
					weight: 2
				}).addTo(this.map);

				// Add popup with coverage info
				circle.bindPopup(`
					<div class="coverage-popup">
						<strong>${coverage.coverageType || 'Operational'} Range</strong><br/>
						<small>Radius: ${coverage.radiusKm} km</small><br/>
						<small>Force: ${forceType || 'Unknown'}</small>
					</div>
				`);

				// Store reference for later updates
				if (!this.coverageAreas) this.coverageAreas = new Map();
				this.coverageAreas.set(coverage.id, circle);
			}
        });
    }

	/**
	 * Create a plus-shaped coverage overlay centered at a location
	 * Returns an L.layerGroup with two perpendicular polylines
	 */
	createPlusCoverage(centerLatLng, radiusMeters, color = '#3388ff', weight = 3, opacity = 0.5, dashArray) {
		const lat = centerLatLng.lat;
		const lng = centerLatLng.lng;
		const metersPerDegLat = 111320;
		const metersPerDegLng = 111320 * Math.cos(lat * Math.PI / 180);
		const dLat = radiusMeters / metersPerDegLat;
		const dLng = radiusMeters / metersPerDegLng;

		const north = [lat + dLat, lng];
		const south = [lat - dLat, lng];
		const east = [lat, lng + dLng];
		const west = [lat, lng - dLng];

		const style = { color, weight, opacity };
		if (dashArray) style.dashArray = dashArray;

		const vertical = L.polyline([north, south], style);
		const horizontal = L.polyline([east, west], style);
		return L.layerGroup([vertical, horizontal]);
	}

    /**
     * Update coverage areas
     */
    updateCoverageAreas(tokenId, areaCoverages, newCenterLatLng) {
        // Remove existing coverage areas for this token
        this.removeCoverageAreas(tokenId);

        // Get force type from token
        const tokenInfo = this.placedTokens.get(tokenId);
        const forceType = tokenInfo?.token?.forceType;

        // Create new coverage areas with force type color
        this.createCoverageAreas(areaCoverages, newCenterLatLng, forceType);
    }

    /**
     * Remove coverage areas
     */
    removeCoverageAreas(tokenId) {
        if (!this.coverageAreas) return;

        // Find and remove coverage areas for this token
        for (const [coverageId, circle] of this.coverageAreas) {
            this.map.removeLayer(circle);
            this.coverageAreas.delete(coverageId);
        }
    }

    /**
     * Get coverage color based on type
     */
    getCoverageColor(coverageType) {
        const colors = {
            'Operational': '#3388ff',
            'Surveillance': '#ff6b35',
            'Defensive': '#ff4757',
            'Support': '#2ed573',
            'Patrol': '#ffa502',
            'Combat': '#ff3838',
            'Reconnaissance': '#9c88ff'
        };
        return colors[coverageType] || '#3388ff';
    }

    /**
     * Show token details
     */
    showTokenDetails(token) {
		// Delegate to existing TokenManager details UI to avoid duplication
		try {
			if (typeof tokenManager !== 'undefined' && tokenManager && typeof tokenManager.showTokenDetails === 'function') {
				tokenManager.showTokenDetails(token);
				return;
			}
			console.warn('tokenManager.showTokenDetails not available');
		} catch (err) {
			console.error('Error showing token details:', err);
		}
    }

    /**
     * Show token context menu
     */
    showTokenContextMenu(e, token) {
        // Create context menu
        const contextMenu = L.popup()
            .setLatLng(e.latlng)
            .setContent(`
                <div class="token-context-menu">
                    <h6>${token.name}</h6>
                    <button class="btn btn-sm btn-primary" onclick="tokenPlacementManager.startMoveMode('${token.id}')">
                        <i class="fas fa-arrows-alt"></i> Move
                    </button>
                    <button class="btn btn-sm btn-info" onclick="tokenPlacementManager.showTokenDetails(${JSON.stringify(token).replace(/"/g, '&quot;')})">
                        <i class="fas fa-info-circle"></i> Details
                    </button>
                    <button class="btn btn-sm btn-success" onclick="tokenPlacementManager.showTokenMovementHistory('${token.id}')">
                        <i class="fas fa-route"></i> Show History
                    </button>
                    <button class="btn btn-sm btn-warning" onclick="tokenPlacementManager.clearTokenRoutes('${token.id}')">
                        <i class="fas fa-eye-slash"></i> Hide Routes
                    </button>
                    <button class="btn btn-sm btn-danger" onclick="tokenPlacementManager.removeTokenFromMap('${token.id}')">
                        <i class="fas fa-trash"></i> Remove
                    </button>
                </div>
            `)
            .openOn(this.map);
    }

    /**
     * Load existing placed tokens
     */
    async loadPlacedTokens() {
        try {
            // This would typically fetch from an endpoint that returns all placed tokens
            // For now, we'll implement a basic version
            console.log('Loading placed tokens...');
        } catch (error) {
            console.error('Error loading placed tokens:', error);
        }
    }

    /**
     * Get all placed tokens
     */
    getPlacedTokens() {
        return Array.from(this.placedTokens.values()).map(info => info.token);
    }

    /**
     * Clear all route lines and ETA labels for a token
     */
    clearTokenRoutes(tokenId) {
        const tokenData = this.placedTokens.get(tokenId);
        if (tokenData) {
            // Remove route lines
            if (tokenData.routeLines) {
                tokenData.routeLines.forEach(line => {
                    if (this.map.hasLayer(line)) {
                        this.map.removeLayer(line);
                    }
                });
                tokenData.routeLines = [];
            }
            
            // Remove waypoint markers
            if (tokenData.waypointMarkers) {
                tokenData.waypointMarkers.forEach(marker => {
                    if (this.map.hasLayer(marker)) {
                        this.map.removeLayer(marker);
                    }
                });
                tokenData.waypointMarkers = [];
            }
        }
    }

    /**
     * Show movement history for a token
     */
    async showTokenMovementHistory(tokenId) {
        try {
            const response = await fetch(`/GamePlay/GetTokenMovementHistory?tokenId=${tokenId}`);
            const result = await response.json();
            
            if (result.success && result.movementHistory && result.movementHistory.length > 1) {
                // Clear existing routes
                this.clearTokenRoutes(tokenId);
                
                // Create route lines for each movement
                const positions = result.movementHistory.map(m => [parseFloat(m.latitude), parseFloat(m.longitude)]);
                
                // Create the main route line
                const routeLine = L.polyline(positions, {
                    color: '#4299e1',
                    weight: 3,
                    opacity: 0.8,
                    dashArray: '10, 5'
                }).addTo(this.map);
                
                // Add enhanced waypoint markers for each movement point
                result.movementHistory.forEach((movement, index) => {
                    if (index > 0) { // Skip first position (starting point)
                        const waypointMarker = this.createWaypointMarker(movement, index, result.movementOrders);
                        waypointMarker.addTo(this.map);
                        
                        // Store reference for cleanup
                        const tokenData = this.placedTokens.get(tokenId);
                        if (tokenData) {
                            if (!tokenData.routeLines) tokenData.routeLines = [];
                            if (!tokenData.waypointMarkers) tokenData.waypointMarkers = [];
                            tokenData.routeLines.push(routeLine);
                            tokenData.waypointMarkers.push(waypointMarker);
                        }
                    }
                });
                
                this.notificationCallback(`Movement history displayed for ${result.token.name}`, 'success');
            } else {
                this.notificationCallback('No movement history found for this token', 'info');
            }
        } catch (error) {
            console.error('Error showing movement history:', error);
            this.notificationCallback('Error loading movement history', 'error');
        }
    }

    /**
     * Create enhanced waypoint marker with tactical information
     */
    createWaypointMarker(movement, index, movementOrders = []) {
        const position = [parseFloat(movement.latitude), parseFloat(movement.longitude)];
        const eta = index;
        const riskLevel = this.calculateRiskLevel(movement, movementOrders);
        const terrain = this.getTerrainType(position);
        
        // Create minimal on-map tag
        const waypointIcon = L.divIcon({
            className: 'waypoint-marker',
            html: `
                <div class="waypoint-tag">
                    <div class="waypoint-eta">T+${eta}</div>
                    <div class="waypoint-risk risk-${riskLevel.level}"></div>
                    <div class="waypoint-terrain">${terrain.glyph}</div>
                </div>
            `,
            iconSize: [60, 30],
            iconAnchor: [30, 15]
        });
        
        const marker = L.marker(position, { icon: waypointIcon });
        
        // Create detailed tooltip
        const tooltipContent = this.createWaypointTooltip(movement, index, movementOrders, terrain, riskLevel);
        
        marker.bindTooltip(tooltipContent, {
            permanent: false,
            direction: 'top',
            offset: [0, -10],
            className: 'waypoint-tooltip'
        });
        
        return marker;
    }

    /**
     * Create detailed waypoint tooltip
     */
    createWaypointTooltip(movement, index, movementOrders, terrain, riskLevel) {
        const order = movementOrders.find(o => o.createdDate <= movement.createdDate) || {};
        const distance = this.calculateSegmentDistance(movement, index);
        const time = this.calculateSegmentTime(distance, order.movementSpeed || 20);
        const supplyCost = this.calculateSupplyCost(distance, order.movementMode || 'normal');
        const supplyAfter = Math.max(0, (order.supplyAfter || 100) - supplyCost);
        
        return `
            <div class="waypoint-tooltip-content">
                <div class="tooltip-header">
                    <span class="waypoint-id">WP-${String(index).padStart(2, '0')}</span>
                    <span class="waypoint-eta">T+${index}</span>
                    <span class="risk-indicator risk-${riskLevel.level}">${riskLevel.level} risk (${riskLevel.probability}%)</span>
                </div>
                
                <div class="tooltip-line">
                    <span class="label">Dist:</span> ${distance.toFixed(1)} km
                    <span class="separator">|</span>
                    <span class="label">Time:</span> ${time.toFixed(0)}h${((time % 1) * 60).toFixed(0).padStart(2, '0')}m
                    <span class="separator">|</span>
                    <span class="label">Cum ETA:</span> T+${index}
                </div>
                
                <div class="tooltip-line">
                    <span class="label">Terrain:</span> ${terrain.name} (slope ${terrain.slope}%)
                    <span class="separator">|</span>
                    <span class="label">Road:</span> ${terrain.roadType}
                </div>
                
                <div class="tooltip-line">
                    <span class="label">Supply:</span> -${supplyCost.toFixed(1)}
                    <span class="separator">|</span>
                    <span class="label">After:</span> ${supplyAfter.toFixed(1)}%
                </div>
                
                <div class="tooltip-line">
                    <span class="label">Enemy:</span> ${this.getEnemyIntel(movement)}
                </div>
                
                <div class="tooltip-line">
                    <span class="label">Action:</span> ${order.movementType || 'Transit'}
                    <span class="separator">|</span>
                    <span class="label">Support:</span> ${this.getSupportInfo(order)}
                </div>
                
                <div class="tooltip-footer">
                    <div class="warnings">${this.getWarnings(movement, riskLevel, supplyAfter)}</div>
                    <button class="btn-details" onclick="tokenPlacementManager.showWaypointDetails('${movement.id}')">
                        Open details
                    </button>
                </div>
            </div>
        `;
    }

    /**
     * Calculate risk level for waypoint
     */
    calculateRiskLevel(movement, movementOrders) {
        // Simulate risk calculation based on movement data
        const baseRisk = Math.random() * 0.4 + 0.1; // 10-50% base risk
        const timeFactor = movement.createdDate ? (Date.now() - new Date(movement.createdDate).getTime()) / (1000 * 60 * 60 * 24) : 0;
        const risk = Math.min(0.9, baseRisk + (timeFactor * 0.1));
        
        if (risk < 0.3) return { level: 'low', probability: Math.round(risk * 100) };
        if (risk < 0.6) return { level: 'medium', probability: Math.round(risk * 100) };
        return { level: 'high', probability: Math.round(risk * 100) };
    }

    /**
     * Get terrain type for position
     */
    getTerrainType(position) {
        // Simulate terrain detection based on position
        const terrains = [
            { name: 'Offroad', glyph: '🌲', slope: 8, roadType: 'track' },
            { name: 'Road', glyph: '🛣️', slope: 2, roadType: 'paved' },
            { name: 'Forest', glyph: '🌳', slope: 12, roadType: 'trail' },
            { name: 'Urban', glyph: '🏢', slope: 1, roadType: 'street' },
            { name: 'Desert', glyph: '🏜️', slope: 5, roadType: 'sand' }
        ];
        
        return terrains[Math.floor(Math.random() * terrains.length)];
    }

    /**
     * Calculate segment distance
     */
    calculateSegmentDistance(movement, index) {
        // Simulate distance calculation
        return Math.random() * 25 + 5; // 5-30 km
    }

    /**
     * Calculate segment time
     */
    calculateSegmentTime(distance, speed) {
        return distance / speed;
    }

    /**
     * Calculate supply cost
     */
    calculateSupplyCost(distance, movementMode) {
        const baseCost = distance * 0.5;
        const modeMultiplier = {
            'march': 0.8,
            'normal': 1.0,
            'stealth': 1.3
        };
        return baseCost * (modeMultiplier[movementMode] || 1.0);
    }

    /**
     * Get enemy intelligence
     */
    getEnemyIntel(movement) {
        const intelOptions = [
            'lastT11 • light_infantry (±1500m)',
            'no contact',
            'patrol • mechanized (±800m)',
            'outpost • heavy_armor (±2000m)'
        ];
        return intelOptions[Math.floor(Math.random() * intelOptions.length)];
    }

    /**
     * Get support information
     */
    getSupportInfo(order) {
        if (order.supportType) {
            return `${order.supportType} @ T+${order.supportTime || '1.8'}`;
        }
        return 'none';
    }

    /**
     * Get warnings for waypoint
     */
    getWarnings(movement, riskLevel, supplyAfter) {
        const warnings = [];
        
        if (riskLevel.level === 'high') {
            warnings.push('high threat area');
        }
        
        if (supplyAfter < 20) {
            warnings.push('low supply');
        }
        
        if (Math.random() > 0.7) {
            warnings.push('crosses mine risk');
        }
        
        return warnings.length > 0 ? warnings.join(' • ') : 'clear';
    }

    /**
     * Show waypoint details in side panel
     */
    showWaypointDetails(waypointId) {
        // Implementation for detailed waypoint inspection
        console.log('Showing waypoint details for:', waypointId);
        this.notificationCallback('Waypoint details panel would open here', 'info');
    }

    /**
     * Check if token is placed
     */
    isTokenPlaced(tokenId) {
        return this.placedTokens.has(tokenId);
    }

    /**
     * Default notification callback
     */
    defaultNotification(message, type) {
        console.log(`[${type.toUpperCase()}] ${message}`);
    }

    /**
     * Clear movement history visuals (route lines and waypoint markers) for a token
     */
    clearMovementHistory(tokenId) {
        const tokenInfo = this.placedTokens.get(tokenId);
        if (!tokenInfo) return;

        // Remove route lines
        if (tokenInfo.routeLines) {
            tokenInfo.routeLines.forEach(line => {
                this.map.removeLayer(line);
            });
            tokenInfo.routeLines = [];
        }

        // Remove waypoint markers
        if (tokenInfo.waypointMarkers) {
            tokenInfo.waypointMarkers.forEach(marker => {
                this.map.removeLayer(marker);
            });
            tokenInfo.waypointMarkers = [];
        }
    }

    /**
     * Refresh movement history for a token after it has been moved
     * This fetches the latest token data (including updated movement history) in one API call
     */
    async refreshMovementHistory(tokenId) {
        try {
            console.log(`🔄 Refreshing movement history for token ${tokenId}`);
            
            // Get updated token data with movement history from single API call
            const response = await fetch('/GamePlay/GetPlacedTokens');
            const result = await response.json();

            if (result.success && result.tokens) {
                const tokenData = result.tokens.find(t => t.id === tokenId);
                if (tokenData) {
                    // Update stored token data with latest info
                    const tokenInfo = this.placedTokens.get(tokenId);
                    if (tokenInfo) {
                        tokenInfo.token = tokenData;
                    }
                    
                    // Draw movement history if it exists
                    if (tokenData.movementHistory && tokenData.movementHistory.length > 1) {
                        // Use the gamePlayManager's method to draw movement history
                        if (window.gamePlayManager && typeof window.gamePlayManager.drawTokenMovementHistory === 'function') {
                            await window.gamePlayManager.drawTokenMovementHistory(tokenData);
                        } else {
                            // Fallback: Draw movement history inline
                            await this.drawMovementHistoryInline(tokenData);
                        }
                    }
                    
                    console.log(`✅ Movement history refreshed for token ${tokenId}`);
                }
            }
        } catch (error) {
            console.error(`❌ Error refreshing movement history for token ${tokenId}:`, error);
        }
    }

    /**
     * Draw movement history inline (fallback method)
     */
    async drawMovementHistoryInline(tokenData) {
        try {
            const tokenId = tokenData.id;
            const movementHistory = tokenData.movementHistory;
            const forceType = tokenData.forceType;

            console.log(`🔄 Drawing movement history inline for token: ${tokenData.name}`);

            if (!movementHistory || movementHistory.length < 2) {
                return;
            }

            // Determine color based on force type (Fox Land / Blue Land)
            let lineColor = '#4299e1';
            if (forceType) {
                const forceTypeLower = forceType.toLowerCase();
                if (forceTypeLower.includes('fox')) {
                    lineColor = '#ff0000'; // Red for Fox Land
                } else if (forceTypeLower.includes('blue')) {
                    lineColor = '#0000ff'; // Blue for Blue Land
                }
            }

            // Create positions array
            const positions = movementHistory.map(m => [
                typeof m.latitude === 'string' ? parseFloat(m.latitude) : m.latitude,
                typeof m.longitude === 'string' ? parseFloat(m.longitude) : m.longitude
            ]);

            // Create dotted route line
            const routeLine = L.polyline(positions, {
                color: lineColor,
                weight: 3,
                opacity: 0.7,
                dashArray: '10, 10'
            }).addTo(this.map);

            // Add waypoint markers
            const waypointMarkers = [];
            movementHistory.forEach((movement, index) => {
                if (index > 0 && index < movementHistory.length - 1) {
                    const lat = typeof movement.latitude === 'string' ? parseFloat(movement.latitude) : movement.latitude;
                    const lng = typeof movement.longitude === 'string' ? parseFloat(movement.longitude) : movement.longitude;

                    const waypointMarker = L.circleMarker([lat, lng], {
                        radius: 4,
                        color: lineColor,
                        fillColor: lineColor,
                        fillOpacity: 0.8,
                        weight: 2
                    }).addTo(this.map);

                    waypointMarker.bindPopup(`
                        <div class="waypoint-popup">
                            <strong>${tokenData.name}</strong><br/>
                            <small>Waypoint ${index}</small><br/>
                            <small>${movement.createdDate ? new Date(movement.createdDate).toLocaleString() : 'Unknown time'}</small>
                        </div>
                    `);

                    waypointMarkers.push(waypointMarker);
                }
            });

            // Store references
            const tokenInfo = this.placedTokens.get(tokenId);
            if (tokenInfo) {
                if (!tokenInfo.routeLines) tokenInfo.routeLines = [];
                if (!tokenInfo.waypointMarkers) tokenInfo.waypointMarkers = [];
                tokenInfo.routeLines.push(routeLine);
                tokenInfo.waypointMarkers.push(...waypointMarkers);
            }

            console.log(`✅ Movement history drawn inline with ${lineColor} color`);
        } catch (error) {
            console.error('Error drawing movement history inline:', error);
        }
    }

    /**
     * Clean up resources
     */
    destroy() {
        this.cancelPlacementMode();
        this.cancelMoveMode();
        
        if (this.tempMarker) {
            this.map.removeLayer(this.tempMarker);
        }

        // Clean up all movement history visuals
        this.placedTokens.forEach((tokenInfo, tokenId) => {
            this.clearMovementHistory(tokenId);
        });

        // Remove all placed tokens
        for (const [tokenId, tokenInfo] of this.placedTokens) {
            this.map.removeLayer(tokenInfo.marker);
        }
        
        this.placedTokens.clear();
        
        if (this.coverageAreas) {
            for (const [coverageId, circle] of this.coverageAreas) {
                this.map.removeLayer(circle);
            }
            this.coverageAreas.clear();
        }
    }
}

// Export for use in other modules
if (typeof module !== 'undefined' && module.exports) {
    module.exports = TokenPlacementManager;
}
