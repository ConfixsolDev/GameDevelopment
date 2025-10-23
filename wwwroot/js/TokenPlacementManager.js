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
        
        // Attack mode properties
        this.attackMode = false;
        this.attackerToken = null;
        this.attackTargetHandler = null;
        this.currentAttackerToken = null;
        this.currentTargetToken = null;
        
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
        
        // Show instruction banner
        if (window.tokenActionModeManager && window.tokenActionModeManager.showDefenseInstructions) {
            window.tokenActionModeManager.showDefenseInstructions(`Click on the map to place "${token.name}"`, 'Placing Token');
        }
        
        this.notificationCallback(`Click on the map to place "${token.name}"`, 'info');
    }

    /**
     * Cancel placement mode
     */
    cancelPlacementMode() {
        this.isPlacementMode = false;
        this.selectedTokenForPlacement = null;
        this.map.getContainer().style.cursor = 'default';
        
        // Hide instruction banner
        if (window.tokenActionModeManager && window.tokenActionModeManager.hideAllInstructions) {
            window.tokenActionModeManager.hideAllInstructions();
        }
        
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

		// Add coverage area preview
		if (this.selectedTokenForPlacement.frontCoverageKm && this.selectedTokenForPlacement.rearCoverageKm) {
			// Oval coverage preview
			const sideKm = this.selectedTokenForPlacement.sideCoverageKm || 
				(this.selectedTokenForPlacement.frontCoverageKm + this.selectedTokenForPlacement.rearCoverageKm) / 2;
			
			const maxRadius = Math.max(
				this.selectedTokenForPlacement.frontCoverageKm, 
				this.selectedTokenForPlacement.rearCoverageKm, 
				sideKm
			);
			const radiusMeters = maxRadius * 1000;
			
			// Determine color based on force type
			let previewColor = '#3388ff'; // Default blue
			if (this.selectedTokenForPlacement.forceType) {
				const forceType = this.getStandardizedForceType(this.selectedTokenForPlacement.forceType);
				if (forceType === 'Fox Land') {
					previewColor = '#ff0000'; // Red for Fox Land
				} else if (forceType === 'Blue Land') {
					previewColor = '#0000ff'; // Blue for Blue Land
				} else if (forceType === 'UN') {
					previewColor = '#00ff00'; // Green for UN
				}
			}
			
			const plus = this.createPlusCoverage(latlng, radiusMeters, previewColor, 3, 0.5, '5, 5');
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

                // Create coverage areas with force type color using token attributes
                this.createCoverageAreas(null, latlng, this.selectedTokenForPlacement.forceType, this.selectedTokenForPlacement);

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
        
        // Show movement instructions in banner
        if (window.tokenActionModeManager && window.tokenActionModeManager.showMovementInstructions) {
            window.tokenActionModeManager.showMovementInstructions(`Click on the map to move "${tokenInfo.token.name}"`);
        }
        
        this.notificationCallback(`Click on the map to move "${tokenInfo.token.name}"`, 'info');
    }

    /**
     * Cancel move mode
     */
    cancelMoveMode() {
        this.isMovingToken = false;
        this.movingToken = null;
        this.map.getContainer().style.cursor = 'default';
        
        // Hide movement instructions when cancelling move mode
        if (window.tokenActionModeManager && window.tokenActionModeManager.hideAllInstructions) {
            window.tokenActionModeManager.hideAllInstructions();
        }
    }

    /**
     * Enable token dragging for all placed tokens
     */
    enableTokenDragging() {
        this.placedTokens.forEach((tokenInfo, tokenId) => {
            if (tokenInfo.marker) {
                tokenInfo.marker.draggable = true;
            }
        });
        console.log('✅ Token dragging enabled for all tokens');
    }

    /**
     * Disable token dragging for all placed tokens
     */
    disableTokenDragging() {
        this.placedTokens.forEach((tokenInfo, tokenId) => {
            if (tokenInfo.marker) {
                tokenInfo.marker.draggable = false;
            }
        });
        console.log('🚫 Token dragging disabled for all tokens');
    }

    /**
     * Enable token selection (click events) for all placed tokens
     */
    enableTokenSelection() {
        this.placedTokens.forEach((tokenInfo, tokenId) => {
            if (tokenInfo.marker) {
                // Ensure click events are enabled
                tokenInfo.marker.options.clickable = true;
            }
        });
        console.log('✅ Token selection enabled for all tokens');
    }

    /**
     * Find token at location
     */
    findTokenAtLocation(latlng) {
        console.log('🔍 TokenPlacementManager finding token at:', latlng);
        console.log('🔍 Placed tokens count:', this.placedTokens.size);
        
        // Debug: Log all placed tokens
        for (const [tokenId, tokenInfo] of this.placedTokens) {
            console.log('🔍 Placed token:', tokenId, tokenInfo.token?.name, tokenInfo.marker ? 'has marker' : 'no marker');
        }
        
        const tolerance = 0.01; // Increased tolerance from 0.001
        
        for (const [tokenId, tokenInfo] of this.placedTokens) {
            console.log('🔍 Checking token:', tokenId, tokenInfo.token?.name);
            if (tokenInfo.marker) {
                const markerLatLng = tokenInfo.marker.getLatLng();
                const distance = this.calculateDistance(
                    latlng.lat, latlng.lng,
                    markerLatLng.lat, markerLatLng.lng
                );
                
                console.log(`🔍 Token ${tokenInfo.token?.name} at ${markerLatLng.lat}, ${markerLatLng.lng}, distance: ${distance}`);
                
                if (distance < tolerance) {
                    console.log('✅ Token found:', tokenInfo.token);
                    return tokenInfo.token;
                }
            } else {
                console.log('❌ Token has no marker:', tokenId);
            }
        }
        
        console.log('❌ No token found in TokenPlacementManager');
        return null;
    }

    /**
     * Calculate distance between two points
     */
    calculateDistance(lat1, lng1, lat2, lng2) {
        const dx = lat2 - lat1;
        const dy = lng2 - lng1;
        return Math.sqrt(dx * dx + dy * dy);
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
        if (!tokenInfo) {
            console.warn(`⚠️ Token ${tokenId} not found in placedTokens`);
            return;
        }

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
                console.log(`✅ Server confirmed removal of token: ${tokenId}`);
                
                // Use comprehensive cleanup from TokenManager if available
                if (window.tokenManager && typeof window.tokenManager.removeAllMarkersForToken === 'function') {
                    console.log(`🧹 Using comprehensive cleanup from TokenManager`);
                    window.tokenManager.removeAllMarkersForToken(tokenId);
                } else {
                    // Fallback to basic cleanup
                    console.log(`⚠️ Using fallback cleanup`);
                    
                    // Remove marker from map
                    if (tokenInfo.marker && this.map) {
                        this.map.removeLayer(tokenInfo.marker);
                    }

                    // Remove coverage areas
                    this.removeCoverageAreas(tokenId);
                    
                    // Remove route lines
                    if (tokenInfo.routeLines) {
                        tokenInfo.routeLines.forEach(line => {
                            if (this.map && this.map.hasLayer(line)) {
                                this.map.removeLayer(line);
                            }
                        });
                    }
                    
                    // Remove waypoint markers
                    if (tokenInfo.waypointMarkers) {
                        tokenInfo.waypointMarkers.forEach(marker => {
                            if (this.map && this.map.hasLayer(marker)) {
                                this.map.removeLayer(marker);
                            }
                        });
                    }

                    // Remove from tracking
                    this.placedTokens.delete(tokenId);
                }

                this.notificationCallback(result.message, 'success');
            } else {
                this.notificationCallback(result.message || 'Failed to remove token', 'error');
            }
        } catch (error) {
            console.error('❌ Error removing token:', error);
            this.notificationCallback('Error removing token from map', 'error');
        }
    }

    /**
     * Create token marker
     */
	createTokenMarker(token, latlng) {
        const icon = this.createTokenIcon(token);
        
        // Make dragging mode-dependent
        const currentMode = window.tokenActionModeManager?.getCurrentMode();
        const isDraggable = currentMode === 'move' || currentMode === 'select' || !currentMode;
        
		const marker = L.marker(latlng, { 
            icon: icon, 
            draggable: isDraggable,  // Only draggable in specific modes
            autoPan: true 
        });

        // Add token ID as data attribute for easy access by other frontend tools
        marker.tokenData = token;
        marker.tokenId = token.id;
        
        // Also add to the marker element itself for DOM access
        marker.on('add', function() {
            if (this.getElement) {
                const element = this.getElement();
                if (element) {
                    element.setAttribute('data-id', token.id);
                    element.setAttribute('data-token-id', token.id);
                    element.setAttribute('data-token-name', token.name);
                    element.setAttribute('data-token-type', token.forceType || 'Unknown');
                    element.setAttribute('data-token-guid', token.id);
                    element.classList.add('token-marker');
                    
                    // Add title attribute to show token GUID on hover
                    element.setAttribute('title', `Token: ${token.name} (ID: ${token.id})`);
                    
                    console.log(`✅ Token marker DOM attributes set for ${token.name}: data-id="${token.id}"`);
                }
            }
        });

		// Add mode-dependent click event
		marker.on('click', () => {
			if (this.suppressNextClick) {
				this.suppressNextClick = false;
				return;
			}
			if (!this.isDraggingMarker) {
				this.handleTokenClick(token, marker);
			}
		});

		// Add context menu event (always enabled regardless of mode)
		marker.on('contextmenu', (e) => {
			e.originalEvent.preventDefault();
			e.originalEvent.stopPropagation();
			this.showTokenContextMenu(e, { token: token, marker: marker });
		});


		// Enable drag-to-move behavior with enhanced planning
		marker.on('dragstart', (e) => {
			console.log('🎯 Drag start event triggered for token:', token.name);
			
			// Check if move mode is active - if not, prevent drag
			const currentMode = window.tokenActionModeManager?.getCurrentMode();
			if (currentMode !== 'move') {
				console.log('🚫 Drag prevented - not in move mode. Current mode:', currentMode);
				e.preventDefault();
				e.stopPropagation();
				// Show instruction to select move mode
				if (window.tokenActionModeManager?.showMovementInstructions) {
					window.tokenActionModeManager.showMovementInstructions('Please select "Plan Move" mode to move tokens');
				}
				return false;
			}
			
			this.isDraggingMarker = true;
			this.originalPosition = e.target.getLatLng();
			console.log('🎯 Original position set to:', this.originalPosition);
			
			// Show movement instructions in banner
			if (window.tokenActionModeManager && window.tokenActionModeManager.showMovementInstructions) {
				window.tokenActionModeManager.showMovementInstructions(`Dragging "${token.name}" to new position`);
			}
			
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
				
				// Hide movement instructions immediately when drag ends
				if (window.tokenActionModeManager && window.tokenActionModeManager.hideAllInstructions) {
					window.tokenActionModeManager.hideAllInstructions();
				}
				
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
        
        // Determine route color based on force type
        let routeColor = '#ff6b6b'; // Default red
        if (token && token.forceType) {
            const forceType = this.getStandardizedForceType(token.forceType);
            if (forceType === 'Fox Land') {
                routeColor = '#ff0000'; // Red for Fox Land
            } else if (forceType === 'Blue Land') {
                routeColor = '#0000ff'; // Blue for Blue Land
            } else if (forceType === 'UN') {
                routeColor = '#00ff00'; // Green for UN
            }
        }
        
        // Create ghost route line
        this.dragPreview.routeLine = L.polyline([this.originalPosition], {
            color: routeColor,
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
     * End drag-to-move and save position directly (no popup)
     */
    async endDragToMove(token, marker, newPosition) {
        console.log('🎯 endDragToMove called for token:', token.name);
        console.log('🎯 Original position:', this.originalPosition);
        console.log('🎯 New position:', newPosition);
        
        // Remove route line if exists
        if (this.dragPreview?.routeLine) {
            this.map.removeLayer(this.dragPreview.routeLine);
        }
        
        // Remove tooltip
        this.hideDragTooltip();
        
        // Store preview marker reference for direct save
        this.dragPreview = {
            marker: marker,
            token: token
        };
        
        // Calculate final distance
        const distance = this.originalPosition ? this.originalPosition.distanceTo(newPosition) / 1000 : 0;
        console.log('🎯 Calculated distance:', distance, 'km');
        
        // Save directly without showing modal
        await this.saveTokenPositionDirectly(token.id, newPosition);
        
        // Hide movement instructions after successful move
        if (window.tokenActionModeManager && window.tokenActionModeManager.hideAllInstructions) {
            window.tokenActionModeManager.hideAllInstructions();
        }
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
        
        // Remove any existing backdrop
        const existingBackdrop = document.querySelector('.modal-backdrop');
        if (existingBackdrop) {
            existingBackdrop.remove();
        }
        
        const modalHtml = `
        <div class="modal fade" id="confirmMoveModal" tabindex="-1" role="dialog" aria-labelledby="confirmMoveModalLabel" aria-hidden="true">
            <div class="modal-dialog modal-xl" role="document">
                <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title" id="confirmMoveModalLabel">
                        <i class="fas fa-route"></i> Movement Planning - ${token.name}
                    </h5>
                    <button type="button" class="close" onclick="document.getElementById('confirmMoveModal').remove(); document.querySelector('.modal-backdrop')?.remove();" aria-label="Close">
                        <i class="fas fa-times"></i>
                    </button>
                </div>
                <div class="modal-body">
                    <div class="data-entry-container">
                        <div class="data-entry-form-section">
                            <!-- Movement Planning Form -->
                            <div class="brigade-data-form">
                                <!-- Movement Planning Section -->
                                <div class="data-tab-content active">
                                    
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
                                         <div class="col-md-6">
                                            <div class="form-group">
                                                <label for="plannedETA" style="color: #ccc; font-size: 12px; margin-bottom: 5px;">Planned ETA</label>
                                                <select class="form-control" id="plannedETA">
                                                    <option value="">Select ETA (Optional)</option>
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
                                                <small class="text-muted" style="font-size: 11px;">Estimated arrival time in military format</small>
                                            </div>
                                        </div>
                                    </div>
                                    
                                    <!-- Row 3: Engagement Rule & Shared with Allies -->
                                    <div class="row">
                                        <div class="col-md-6">
                                            <div class="form-group">
                                                <label for="engagementRule" style="color: #ccc; font-size: 12px; margin-bottom: 5px;">Engagement Rule</label>
                                                <select class="form-control" id="engagementRule">
                                                    <option value="">Select engagement rule (Optional)</option>
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
                            
                        </div>
                    </div>
                </div>
                <div class="modal-footer" style="display: flex; justify-content: flex-end; gap: 10px;">
                    <button type="button" class="btn btn-secondary" onclick="document.getElementById('confirmMoveModal').remove(); document.querySelector('.modal-backdrop')?.remove();">
                        <i class="fas fa-times"></i> Close
                    </button>
                    <button type="button" class="btn btn-primary" onclick="window.tokenPlacementManager.saveMoveOrder('${token.id}')">
                        <i class="fas fa-save"></i> Save Planning Details
                    </button>
                </div>
            </div>
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
        
        // Add modal to DOM
        document.body.insertAdjacentHTML('beforeend', modalHtml);
        console.log('🎯 Movement modal added to DOM');
        
        // Make sure the function is globally accessible
        window.tokenPlacementManager = this;
        
        // Show the modal using vanilla JavaScript
        const modal = document.getElementById('confirmMoveModal');
        if (modal) {
            modal.style.display = 'block';
            modal.style.opacity = '1';
            modal.style.visibility = 'visible';
            modal.classList.add('show');
            modal.classList.remove('fade');
            
            // Add backdrop
            const backdrop = document.createElement('div');
            backdrop.className = 'modal-backdrop fade show';
            backdrop.style.zIndex = '1040';
            document.body.appendChild(backdrop);
            
            console.log('✅ Movement modal displayed');
        } else {
            console.error('❌ Failed to find confirmMoveModal element');
        }
    }

    /**
     * Save token position directly without modal (for drag and drop)
     */
    async saveTokenPositionDirectly(tokenId, newPosition) {
        console.log('🎯 Saving token position directly:', tokenId);
        
        try {
            const response = await fetch('/GamePlay/MoveToken', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    TokenId: tokenId,
                    Latitude: newPosition.lat,
                    Longitude: newPosition.lng,
                    MovementMode: null,
                    StartTurn: null,
                    StartOffset: null,
                    PlannedETA: null,
                    MovementSpeed: null,
                    EngagementRule: null,
                    SharedOrder: false,
                    Notes: null
                })
            });
            
            const result = await response.json();
            console.log('MoveToken response:', result);
            
            if (result.success) {
                this.notificationCallback('Token position saved. Click on marker to add planning details.', 'success');
                
                // Update area coverage if any
                if (result.areaCoverages && result.areaCoverages.length > 0) {
                    this.updateAreaCoverages(tokenId, result.areaCoverages);
                }
                
                // Draw the complete movement history for this token
                await this.showCompleteTokenRoute(tokenId);
                
                // Clean up
                this.dragPreview = null;
                this.originalPosition = null;
            } else {
                this.notificationCallback('Failed to save position: ' + result.message, 'error');
                console.error('MoveToken failed:', result);
            }
        } catch (error) {
            console.error('Error saving token position:', error);
            this.notificationCallback('Error saving position: ' + error.message, 'error');
        }
    }

    /**
     * Save move order
     */
    async saveMoveOrder(tokenId) {
        console.log('saveMoveOrder called with tokenId:', tokenId);
        const tokenInfo = this.placedTokens.get(tokenId);
        if (!tokenInfo) {
            console.error('Token not found for ID:', tokenId);
            this.notificationCallback('Token not found', 'error');
            return;
        }
        
        // Get current token position
        let currentLat, currentLng;
        if (tokenInfo.marker && tokenInfo.marker.getLatLng) {
            const latLng = tokenInfo.marker.getLatLng();
            currentLat = latLng.lat;
            currentLng = latLng.lng;
        } else if (tokenInfo.token && tokenInfo.token.position) {
            currentLat = parseFloat(tokenInfo.token.position.lat);
            currentLng = parseFloat(tokenInfo.token.position.lng);
        } else {
            console.error('No position data available for token:', tokenId);
            this.notificationCallback('Token position not found', 'error');
            return;
        }
        
        // Get form values - only the fields that still exist
        const plannedETA = document.getElementById('plannedETA')?.value || null;
        const engagementRule = document.getElementById('engagementRule')?.value || null;
        
        console.log('Form values:', { plannedETA, engagementRule });
        
        // Get remaining form fields
        const movementMode = document.querySelector('input[name="movementMode"]:checked')?.value || null;
        const startOffset = document.getElementById('startOffset')?.value || null;
        const sharedOrder = document.getElementById('sharedOrder')?.checked || false;
        const notes = document.getElementById('moveNotes')?.value || null;
        
        console.log('All form values:', { 
            movementMode, startOffset, sharedOrder, notes,
            plannedETA, engagementRule 
        });
        
        try {
            console.log('Saving move order with data:', {
                tokenId: tokenId,
                latitude: currentLat,
                longitude: currentLng,
                movementMode: movementMode,
                startOffset: startOffset,
                plannedETA: plannedETA,
                engagementRule: engagementRule,
                sharedOrder: sharedOrder,
                notes: notes
            });

            const response = await fetch('/GamePlay/MoveToken', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    TokenId: tokenId,
                    Latitude: currentLat,
                    Longitude: currentLng,
                    MovementMode: movementMode || null,
                    StartTurn: null, // Removed field
                    StartOffset: startOffset ? parseFloat(startOffset) : null,
                    PlannedETA: plannedETA ? parseFloat(plannedETA) : null,
                    MovementSpeed: null, // Removed field
                    EngagementRule: engagementRule || null,
                    SharedOrder: sharedOrder || false,
                    Notes: notes || null
                })
            });
            
            const result = await response.json();
            console.log('MoveToken response:', result);
            
            if (result.success) {
                this.notificationCallback('Planning details saved successfully', 'success');
                
                // Add to scenario planning orders
                if (window.scenarioPlanning) {
                    const moveOrder = {
                        id: this.generateId(),
                        unitId: tokenId,
                        type: 'movement',
                        movementMode: movementMode,
                        startOffset: startOffset,
                        plannedETA: plannedETA,
                        engagementRule: engagementRule,
                        notes: notes,
                        route: [{ lat: currentLat, lng: currentLng }], // Current position only
                        timestamp: new Date()
                    };
                    
                    window.scenarioPlanning.orders.push(moveOrder);
                    window.scenarioPlanning.updateOrdersDisplay();
                }
                
                // Show success message
                this.notificationCallback('Movement planning saved successfully', 'success');
                
                // Close modal
                document.getElementById('confirmMoveModal').remove();
                const backdrop = document.querySelector('.modal-backdrop');
                if (backdrop) backdrop.remove();
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
     * Show complete token route by loading all movement history
     */
    async showCompleteTokenRoute(tokenId) {
        console.log('🎯 Loading complete route for token:', tokenId);
        
        try {
            // Clear existing routes first
            this.clearTokenRoutes(tokenId);
            
            // Fetch complete movement history from server
            const response = await fetch(`/GamePlay/GetTokenMovementHistory?tokenId=${tokenId}`);
            const result = await response.json();
            
            if (result.success && result.movementHistory && result.movementHistory.length > 1) {
                console.log(`📍 Found ${result.movementHistory.length} movement points for complete route`);
                
                // Create route lines for each movement segment
                const positions = result.movementHistory.map(m => [
                    typeof m.latitude === 'string' ? parseFloat(m.latitude) : m.latitude,
                    typeof m.longitude === 'string' ? parseFloat(m.longitude) : m.longitude
                ]);
                
                // Determine route color based on force type
                let routeColor = '#4299e1'; // Default blue
                if (tokenData && tokenData.token && tokenData.token.forceType) {
                    const forceTypeLower = tokenData.token.forceType.toLowerCase();
                    if (forceTypeLower.includes('fox') || forceTypeLower.includes('red')) {
                        routeColor = '#ff0000'; // Red for Fox Land
                    } else if (forceTypeLower.includes('blue') || forceTypeLower.includes('friendly')) {
                        routeColor = '#0000ff'; // Blue for Blue Land
                    }
                }
                
                // Create the complete route line
                const routeLine = L.polyline(positions, {
                    color: routeColor,
                    weight: 3,
                    opacity: 0.7,
                    dashArray: '10, 10',
                    smoothFactor: 1.0
                }).addTo(this.map);
                
                // Store route for cleanup
                const tokenData = this.placedTokens.get(tokenId);
                if (tokenData) {
                    if (!tokenData.routeLines) tokenData.routeLines = [];
                    tokenData.routeLines.push(routeLine);
                }
                
                console.log('🎯 Complete route line created with', positions.length, 'points');
            } else {
                console.log('No movement history found for complete route');
            }
        } catch (error) {
            console.error('Error loading complete token route:', error);
        }
    }

    /**
     * Show confirmed route on map
     */
    showConfirmedRoute(tokenId) {
        const tokenData = this.placedTokens.get(tokenId);
        if (!tokenData || !this.originalPosition) return;
        
        // Clean up any existing route lines for this token first
        if (tokenData.routeLines) {
            tokenData.routeLines.forEach(line => {
                if (this.map.hasLayer(line)) {
                    this.map.removeLayer(line);
                }
            });
            tokenData.routeLines = [];
        }
        
        // Determine route color based on force type
        let routeColor = '#4299e1'; // Default blue
        if (tokenData && tokenData.token && tokenData.token.forceType) {
            const forceTypeLower = tokenData.token.forceType.toLowerCase();
            if (forceTypeLower.includes('fox') || forceTypeLower.includes('red')) {
                routeColor = '#ff0000'; // Red for Fox Land
            } else if (forceTypeLower.includes('blue') || forceTypeLower.includes('friendly')) {
                routeColor = '#0000ff'; // Blue for Blue Land
            }
        }
        
        // Create a single continuous route line (matching refresh behavior)
        const routeLine = L.polyline([this.originalPosition, tokenData.marker.getLatLng()], {
            color: routeColor,
            weight: 3,
            opacity: 0.7,
            dashArray: '10, 10', // Match the refresh pattern
            smoothFactor: 1.0
        }).addTo(this.map);
        
        // Store route for cleanup (no waypoint markers for now to avoid interference)
        if (!tokenData.routeLines) tokenData.routeLines = [];
        tokenData.routeLines.push(routeLine);
        
        console.log('🎯 Route line created from', this.originalPosition, 'to', tokenData.marker.getLatLng());
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
        console.log(`🔍 Creating icon for token: ${token.name}`, {
            hasMilitaryRenderer: !!window.militarySymbolRenderer,
            organizationLevel: token.organizationLevel,
            unitType: token.unitType,
            unitDesignation: token.unitDesignation,
            forceType: token.forceType
        });

        // Use military symbol renderer if available and token has any military data
        if (window.militarySymbolRenderer) {
            console.log(`🎖️ Creating military symbol for ${token.name}`);
            return window.militarySymbolRenderer.createMilitaryIcon(token);
        }

        // Fallback to legacy image/icon based tokens
        console.log(`📷 Creating legacy icon for ${token.name} - military renderer not available`);
        
        // Determine border color based on force type
        let borderColor = '#00ff88'; // Default green
        if (token.forceType) {
            const forceTypeLower = token.forceType.toLowerCase();
            if (forceTypeLower.includes('fox') || forceTypeLower.includes('hostile') || forceTypeLower.includes('red')) {
                borderColor = '#ff0000'; // Red for hostile
            } else if (forceTypeLower.includes('blue') || forceTypeLower.includes('friendly')) {
                borderColor = '#0000ff'; // Blue for friendly
            } else if (forceTypeLower.includes('neutral') || forceTypeLower.includes('green')) {
                borderColor = '#00AA00'; // Green for neutral
            } else if (forceTypeLower.includes('unknown') || forceTypeLower.includes('yellow')) {
                borderColor = '#FFAA00'; // Orange/Yellow for unknown
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
     * Create coverage areas with force-type-based colors using token attributes
     */
    createCoverageAreas(areaCoverages, centerLatLng, forceType = null, token = null) {
        // Clear any existing coverage areas for this token first
        if (token && token.id) {
            this.removeCoverageAreas(token.id);
        }
        
        // If we have token attributes, create a single 4-sided polygon coverage that matches token definition
        if (token && token.frontCoverageKm && token.rearCoverageKm) {
            console.log(`🎯 Creating 4-sided coverage for token ${token.name}: Front=${token.frontCoverageKm}km, Rear=${token.rearCoverageKm}km, Side=${token.sideCoverageKm || 'auto'}km`);
            
            const sideKm = token.sideCoverageKm || (token.frontCoverageKm + token.rearCoverageKm) / 2;
            
            // Create single 4-sided polygon coverage area using token attributes
            const coverage = {
                id: 'token_coverage_' + token.id,
                name: 'Token Coverage',
                coverageType: 'Operational',
                frontRadiusKm: token.frontCoverageKm,
                rearRadiusKm: token.rearCoverageKm,
                sideRadiusKm: sideKm,
                rotationDegrees: 0
            };
            
            this.create4SidedCoverageFromToken(coverage, centerLatLng, forceType || token.forceType);
        } else if (areaCoverages && areaCoverages.length > 0) {
            // Process areaCoverages data
            areaCoverages.forEach(coverage => {
                // Check if geometry contains polygon data (regardless of shapeType)
                const hasPolygonGeometry = coverage.geometry && 
                    (coverage.geometry.includes('"type":"Polygon"') || 
                     (typeof coverage.geometry === 'object' && coverage.geometry.type === 'Polygon'));
                
                if (coverage.shapeType === 'Custom' || hasPolygonGeometry) {
                    // Handle custom polygon coverage areas
                    this.createCustomPolygonCoverage(coverage, centerLatLng, forceType || token?.forceType, token);
                } else {
                    // Handle standard coverage areas (oval, 4-sided, etc.)
                    this.createOvalCoverageFromToken(coverage, centerLatLng, forceType || token?.forceType);
                }
            });
        }
    }

    /**
     * Create custom polygon coverage area from saved geometry
     */
    createCustomPolygonCoverage(coverage, centerLatLng, forceType = null, token = null) {
        try {
            // Parse the geometry JSON
            let geometry;
            if (typeof coverage.geometry === 'string') {
                geometry = JSON.parse(coverage.geometry);
            } else {
                geometry = coverage.geometry;
            }
            
            if (!geometry || !geometry.coordinates || !geometry.coordinates[0]) {
                return;
            }
            
            // Determine color based on force type
            let fillColor, strokeColor, opacity;
            if (forceType) {
                const forceTypeLower = forceType.toLowerCase();
                if (forceTypeLower.includes('fox')) {
                    fillColor = '#ff0000'; // Red for Fox Land
                    strokeColor = '#cc0000';
                } else if (forceTypeLower.includes('blue')) {
                    fillColor = '#0000ff'; // Blue for Blue Land
                    strokeColor = '#0000cc';
                } else {
                    fillColor = '#3388ff'; // Default light blue
                    strokeColor = '#2266dd';
                }
            } else {
                fillColor = '#3388ff'; // Default light blue
                strokeColor = '#2266dd';
            }
            opacity = 0.2;
            
            // Convert coordinates to LatLng array
            // Check if coordinates are already in [lat, lng] format or need conversion from [lng, lat]
            const firstCoord = geometry.coordinates[0][0];
            const isAlreadyLatLng = firstCoord[0] > -90 && firstCoord[0] < 90; // Latitude range check
            
            let latLngs;
            if (isAlreadyLatLng) {
                // Coordinates are already in [lat, lng] format
                latLngs = geometry.coordinates[0].map(coord => [coord[0], coord[1]]);
            } else {
                // Coordinates are in [lng, lat] format, need conversion
                latLngs = geometry.coordinates[0].map(coord => [coord[1], coord[0]]);
            }
            
            // Validate coordinates before creating polygon
            if (latLngs.length < 3) {
                return;
            }
            
            // Check for duplicate or invalid coordinates
            const validLatLngs = [];
            const seenCoords = new Set();
            
            latLngs.forEach((coord, index) => {
                const isValid = coord[0] !== undefined && coord[1] !== undefined && 
                               !isNaN(coord[0]) && !isNaN(coord[1]);
                
                if (!isValid) {
                    return;
                }
                
                // Check for duplicates
                const coordKey = `${coord[0]},${coord[1]}`;
                if (seenCoords.has(coordKey)) {
                    return;
                }
                
                seenCoords.add(coordKey);
                validLatLngs.push(coord);
            });
            
            if (validLatLngs.length < 3) {
                return;
            }
            
            // Check if coordinates are within reasonable bounds
            const allCoordsValid = validLatLngs.every(coord => {
                const lat = coord[0];
                const lng = coord[1];
                return lat >= -90 && lat <= 90 && lng >= -180 && lng <= 180;
            });
            
            if (!allCoordsValid) {
                return;
            }
            
            // Create polygon with exact coordinates (no scaling or transformation)
            const polygon = L.polygon(validLatLngs, {
                color: strokeColor,
                weight: 2,
                opacity: 0.8,
                fillColor: fillColor,
                fillOpacity: opacity,
                className: 'coverage-area-polygon'
            }).addTo(this.map);
            
            
            // Add popup with coverage info
            const popupContent = `
                <div class="coverage-popup">
                    <h6><i class="fas fa-draw-polygon"></i> Custom Coverage Area</h6>
                    <p><strong>Type:</strong> ${coverage.coverageType || 'Operational'}</p>
                    <p><strong>Shape:</strong> Custom Polygon</p>
                    <p><strong>Points:</strong> ${latLngs.length}</p>
                </div>
            `;
            polygon.bindPopup(popupContent);
            
            // Store polygon reference with token association
            if (!this.coverageAreas) {
                this.coverageAreas = new Map();
            }
            
            // Store with both coverage ID and token association for easy removal
            const tokenId = coverage.tokenId || (token && token.id) || 'unknown';
            const coverageKey = `${coverage.id}_${tokenId}`;
            this.coverageAreas.set(coverageKey, polygon);
            this.coverageAreas.set(coverage.id, polygon); // Also store with just coverage ID for compatibility
            
        } catch (error) {
            // Silently handle errors
        }
    }

    /**
     * Create 4-sided polygon coverage area from token attributes
     */
    create4SidedCoverageFromToken(coverage, centerLatLng, forceType = null) {
        // Determine color based on force type
        let fillColor, strokeColor, opacity;
        
        // Use force type color for single coverage area
            if (forceType) {
                const forceTypeLower = forceType.toLowerCase();
                if (forceTypeLower.includes('fox')) {
                    fillColor = '#ff0000'; // Red for Fox Land
                strokeColor = '#cc0000';
                } else if (forceTypeLower.includes('blue')) {
                    fillColor = '#0000ff'; // Blue for Blue Land
                strokeColor = '#0000cc';
            } else {
                fillColor = '#3388ff'; // Default light blue
                strokeColor = '#2266dd';
            }
        } else {
            fillColor = '#3388ff'; // Default light blue
            strokeColor = '#2266dd';
        }
        
        opacity = 0.2;

        // Create 4-sided polygon using front/rear/side radius
        if (coverage.frontRadiusKm && coverage.rearRadiusKm) {
            let polygon;
            
            // For Blue Land tokens, create policy zone with C-shaped arc and parallel lines
            if (forceType && this.getStandardizedForceType(forceType) === 'Blue Land') {
                polygon = this.createPolicyZonePolygon(
                    centerLatLng, 
                    coverage.frontRadiusKm, 
                    coverage.rearRadiusKm, 
                    coverage.sideRadiusKm || (coverage.frontRadiusKm + coverage.rearRadiusKm) / 2,
                    coverage.rotationDegrees || 0,
                    fillColor, 
                    strokeColor,
                    opacity
                );
            } else {
                polygon = this.create4SidedPolygon(
                    centerLatLng, 
                    coverage.frontRadiusKm, 
                    coverage.rearRadiusKm, 
                    coverage.sideRadiusKm || (coverage.frontRadiusKm + coverage.rearRadiusKm) / 2,
                    coverage.rotationDegrees || 0,
                    fillColor, 
                    strokeColor,
                    opacity
                );
            }

            // Add popup with coverage info
            polygon.bindPopup(`
                <div class="coverage-popup">
                    <strong>${coverage.name || coverage.coverageType || 'Operational'} Range</strong><br/>
                    <small>Front: ${coverage.frontRadiusKm} km</small><br/>
                    <small>Rear: ${coverage.rearRadiusKm} km</small><br/>
                    ${coverage.sideRadiusKm ? `<small>Side: ${coverage.sideRadiusKm} km</small><br/>` : ''}
                    <small>Force: ${forceType || 'Unknown'}</small>
                    ${forceType && this.getStandardizedForceType(forceType) === 'Blue Land' ? '<br/><small>Policy Zone Active</small>' : ''}
                </div>
            `);

            // Store reference for later updates with token association
            if (!this.coverageAreas) this.coverageAreas = new Map();
            const coverageKey = `${coverage.id}_${coverage.tokenId || 'unknown'}`;
            this.coverageAreas.set(coverageKey, polygon);
            this.coverageAreas.set(coverage.id, polygon); // Also store with just coverage ID for compatibility
        }
    }

    /**
     * Create oval coverage area from token attributes or coverage data
     */
    createOvalCoverageFromToken(coverage, centerLatLng, forceType = null) {
        // Determine color based on force type
        let fillColor, strokeColor, opacity;
        
        // Use force type color for single coverage area
        console.log(`🎨 Coverage area - forceType: "${forceType}"`);
        if (forceType) {
            const forceTypeLower = forceType.toLowerCase();
            console.log(`🎨 Coverage area - forceTypeLower: "${forceTypeLower}"`);
            
            const standardizedForceType = this.getStandardizedForceType(forceType);
            if (standardizedForceType === 'Fox Land') {
                fillColor = '#ff0000'; // Red for Fox Land
                strokeColor = '#cc0000';
                console.log(`🔴 Setting RED coverage for Fox Land`);
            } else if (standardizedForceType === 'Blue Land') {
                fillColor = '#0000ff'; // Blue for Blue Land
                strokeColor = '#0000cc';
                console.log(`🔵 Setting BLUE coverage for Blue Land`);
            } else if (standardizedForceType === 'UN') {
                fillColor = '#00ff00'; // Green for UN
                strokeColor = '#00cc00';
                console.log(`🟢 Setting GREEN coverage for UN`);
                } else {
                    fillColor = '#3388ff'; // Default light blue
                    strokeColor = '#2266dd';
                    console.log(`⚠️ Using default blue - forceType not recognized`);
                }
            } else {
                console.log(`⚠️ No forceType provided for coverage area`);
                // Fallback to coverage type color
            switch (coverage.coverageType) {
                case 'Frontside':
                    fillColor = '#00ff00'; // Green for frontside
                    strokeColor = '#00cc00';
                    opacity = 0.2;
                    break;
                case 'Backside':
                    fillColor = '#ff6600'; // Orange for backside
                    strokeColor = '#cc5500';
                    opacity = 0.2;
                    break;
                case 'Fighting':
                    fillColor = '#ff0000'; // Red for fighting
                    strokeColor = '#cc0000';
                    opacity = 0.3;
                    break;
                default:
                    fillColor = '#3388ff'; // Default light blue
                    strokeColor = '#2266dd';
                    opacity = 0.2;
            }
        }
        
        // Set default opacity if not set
        if (opacity === undefined) {
            opacity = 0.2;
        }

        // Create oval coverage using front/rear/side radius
            if (coverage.frontRadiusKm && coverage.rearRadiusKm) {
            let polygon;
            
            // For Blue Land tokens, create policy zone with C-shaped arc and parallel lines
            const standardizedForceType = this.getStandardizedForceType(forceType);
            console.log(`🎯 Force type check: "${forceType}" -> "${standardizedForceType}"`);
            
            if (forceType && standardizedForceType === 'Blue Land') {
                console.log(`🔵 Creating policy zone for Blue Land token with C-shaped arc and || lines`);
                polygon = this.createPolicyZonePolygon(
                    centerLatLng, 
                    coverage.frontRadiusKm, 
                    coverage.rearRadiusKm, 
                    coverage.sideRadiusKm || (coverage.frontRadiusKm + coverage.rearRadiusKm) / 2,
                    coverage.rotationDegrees || 0,
                    fillColor, 
                    strokeColor,
                    opacity
                );
            } else {
                polygon = this.createOvalFromRadii(
                    centerLatLng, 
                    coverage.frontRadiusKm, 
                    coverage.rearRadiusKm, 
                    coverage.sideRadiusKm || (coverage.frontRadiusKm + coverage.rearRadiusKm) / 2,
                    coverage.rotationDegrees || 0,
                    fillColor, 
                    strokeColor,
                    opacity
                );
            }

            // Add popup with coverage info
            polygon.bindPopup(`
                <div class="coverage-popup">
                    <strong>${coverage.name || coverage.coverageType || 'Operational'} Range</strong><br/>
                    <small>Front: ${coverage.frontRadiusKm} km</small><br/>
                    <small>Rear: ${coverage.rearRadiusKm} km</small><br/>
                    ${coverage.sideRadiusKm ? `<small>Side: ${coverage.sideRadiusKm} km</small><br/>` : ''}
                    <small>Force: ${forceType || 'Unknown'}</small>
                    ${forceType && this.getStandardizedForceType(forceType) === 'Blue Land' ? '<br/><small>Policy Zone Active</small>' : ''}
                </div>
            `);

            // Store reference for later updates with token association
            if (!this.coverageAreas) this.coverageAreas = new Map();
            const coverageKey = `${coverage.id}_${coverage.tokenId || 'unknown'}`;
            this.coverageAreas.set(coverageKey, polygon);
            this.coverageAreas.set(coverage.id, polygon); // Also store with just coverage ID for compatibility
        }
    }

    /**
     * Create oval coverage area
     */
    createOvalCoverage(coverage, centerLatLng, fillColor, strokeColor, forceType, opacity = 0.2) {
        // Parse the GeoJSON geometry if available
        let polygon = null;
        if (coverage.geometry) {
            try {
                const geoJson = typeof coverage.geometry === 'string' 
                    ? JSON.parse(coverage.geometry) 
                    : coverage.geometry;
                
                if (geoJson.type === 'Polygon' && geoJson.coordinates && geoJson.coordinates[0]) {
                    // Convert GeoJSON coordinates to Leaflet format
                    const coordinates = geoJson.coordinates[0].map(coord => [coord[1], coord[0]]); // [lng, lat] -> [lat, lng]
                    polygon = L.polygon(coordinates, {
                        color: strokeColor,
                        fillColor: fillColor,
                        fillOpacity: opacity,
                        opacity: 0.6,
                        weight: 2
                    }).addTo(this.map);
                }
            } catch (error) {
                console.warn('Failed to parse coverage geometry:', error);
            }
        }

        // Fallback: create oval using front/rear/side radius
        if (!polygon) {
            polygon = this.createOvalFromRadii(
                centerLatLng, 
                coverage.frontRadiusKm, 
                coverage.rearRadiusKm, 
                coverage.sideRadiusKm || (coverage.frontRadiusKm + coverage.rearRadiusKm) / 2,
                coverage.rotationDegrees || 0,
                fillColor, 
                strokeColor,
                opacity
            );
        }

        // Add popup with coverage info
        polygon.bindPopup(`
            <div class="coverage-popup">
                <strong>${coverage.name || coverage.coverageType || 'Operational'} Range</strong><br/>
                <small>Front: ${coverage.frontRadiusKm} km</small><br/>
                <small>Rear: ${coverage.rearRadiusKm} km</small><br/>
                ${coverage.sideRadiusKm ? `<small>Side: ${coverage.sideRadiusKm} km</small><br/>` : ''}
                <small>Force: ${forceType || 'Unknown'}</small>
            </div>
        `);

        // Store reference for later updates with token association
        if (!this.coverageAreas) this.coverageAreas = new Map();
        const coverageKey = `${coverage.id}_${coverage.tokenId || 'unknown'}`;
        this.coverageAreas.set(coverageKey, polygon);
        this.coverageAreas.set(coverage.id, polygon); // Also store with just coverage ID for compatibility
    }

    /**
     * Create 4-sided polygon from front/rear/side radius values
     * Now creates an oval/elliptical shape with more points for smoother appearance
     */
    create4SidedPolygon(centerLatLng, frontKm, rearKm, sideKm, rotationDegrees, fillColor, strokeColor, opacity = 0.2) {
        // Convert km to degrees (approximate)
        const frontDegrees = frontKm / 111.32;
        const rearDegrees = rearKm / 111.32;
        const sideDegrees = sideKm / 111.32;
        
        // Calculate rotation in radians
        const rotationRad = (rotationDegrees * Math.PI) / 180;
        
        // For Blue Land tokens, use policy zone instead of regular oval
        if (fillColor === '#0000ff' || strokeColor === '#0000cc') {
            return this.createPolicyZonePolygon(
                centerLatLng, 
                frontKm, 
                rearKm, 
                sideKm, 
                rotationDegrees, 
                fillColor, 
                strokeColor, 
                opacity
            );
        }
        
        // Create oval/elliptical shape with multiple points for smoother curves
        const numPoints = 32; // More points = smoother oval
        const points = [];
        
        for (let i = 0; i < numPoints; i++) {
            const angle = (i / numPoints) * 2 * Math.PI;
            
            // Use different radii for front/rear to create elongated oval
            let radiusLat, radiusLng;
            
            // Front half (0 to PI)
            if (angle <= Math.PI) {
                radiusLat = frontDegrees * Math.sin(angle);
                radiusLng = sideDegrees * Math.cos(angle);
            } else {
                // Rear half (PI to 2*PI)
                radiusLat = rearDegrees * Math.sin(angle);
                radiusLng = sideDegrees * Math.cos(angle);
            }
            
            let lat = centerLatLng.lat + radiusLat;
            let lng = centerLatLng.lng + radiusLng;
            
            // Apply rotation if needed
            if (rotationRad !== 0) {
                const cos = Math.cos(rotationRad);
                const sin = Math.sin(rotationRad);
                const x = lng - centerLatLng.lng;
                const y = lat - centerLatLng.lat;
                
                const rotatedX = x * cos - y * sin;
                const rotatedY = x * sin + y * cos;
                
                lat = centerLatLng.lat + rotatedY;
                lng = centerLatLng.lng + rotatedX;
            }
            
            points.push([lat, lng]);
        }
        
        // Create the main polygon
        const polygon = L.polygon(points, {
            color: strokeColor,
            fillColor: fillColor,
            fillOpacity: opacity,
            opacity: 0.6,
            weight: 2
        }).addTo(this.map);
        
        return polygon;
    }

    /**
     * Create policy zone polygon for Blue Land tokens with C-shaped arc on left and parallel lines on right
     */
    createPolicyZonePolygon(centerLatLng, frontKm, rearKm, sideKm, rotationDegrees, fillColor, strokeColor, opacity = 0.2) {
        console.log(`🔵 Creating policy zone polygon: Front=${frontKm}km, Rear=${rearKm}km, Side=${sideKm}km`);
        
        // Use much smaller, more reasonable scaling for policy zone
        const scaleFactor = 0.01; // Make it 100x smaller - very compact around token
        const frontDegrees = (frontKm * scaleFactor) / 111.32;
        const rearDegrees = (rearKm * scaleFactor) / 111.32;
        const sideDegrees = (sideKm * scaleFactor) / 111.32;
        
        console.log(`🔵 Policy zone degrees (scaled): Front=${frontDegrees}, Rear=${rearDegrees}, Side=${sideDegrees}`);
        
        // Calculate rotation in radians
        const rotationRad = (rotationDegrees * Math.PI) / 180;
        
        // Create policy zone points
        const points = [];
        
        // Create C-shaped arc on the left side
        const cArcPoints = this.createCShapedArc(centerLatLng, sideDegrees, frontDegrees, rearDegrees, rotationRad);
        points.push(...cArcPoints);
        console.log(`🔵 C-shaped arc points: ${cArcPoints.length}`);
        
        // Create parallel lines (||) on the right side
        const parallelLinePoints = this.createParallelLines(centerLatLng, sideDegrees, frontDegrees, rearDegrees, rotationRad);
        points.push(...parallelLinePoints);
        console.log(`🔵 Parallel lines points: ${parallelLinePoints.length}`);
        
        // Close the polygon by connecting back to the start
        if (points.length > 0) {
            points.push(points[0]);
        }
        
        console.log(`🔵 Total policy zone points: ${points.length}`);
        
        // Create the main polygon
        const polygon = L.polygon(points, {
            color: strokeColor,
            fillColor: fillColor,
            fillOpacity: opacity,
            opacity: 0.6,
            weight: 2,
            className: 'policy-zone-polygon'
        }).addTo(this.map);
        
        console.log('✅ Created policy zone polygon for Blue Land token');
        return polygon;
    }

    /**
     * Create C-shaped arc points for the left side of the policy zone
     */
    createCShapedArc(centerLatLng, sideDegrees, frontDegrees, rearDegrees, rotationRad) {
        const points = [];
        const numPoints = 16; // Number of points for the C-shape
        
        // C-shape parameters - use very small, compact scaling
        const cRadius = sideDegrees * 0.5; // Small radius relative to token
        const cGap = sideDegrees * 0.3; // Small gap
        
        console.log(`🔵 C-shaped arc: radius=${cRadius}, gap=${cGap}, points=${numPoints}`);
        
        // Start from top-left, go around the left side
        for (let i = 0; i < numPoints; i++) {
            const angle = (i / numPoints) * (Math.PI * 2 - cGap) + cGap/2; // Create gap in the C
            
            let lat = centerLatLng.lat + cRadius * Math.sin(angle);
            let lng = centerLatLng.lng - sideDegrees * 0.1 + cRadius * Math.cos(angle); // Very close to center
            
            // Apply rotation if needed
            if (rotationRad !== 0) {
                const cos = Math.cos(rotationRad);
                const sin = Math.sin(rotationRad);
                const x = lng - centerLatLng.lng;
                const y = lat - centerLatLng.lat;
                
                const rotatedX = x * cos - y * sin;
                const rotatedY = x * sin + y * cos;
                
                lat = centerLatLng.lat + rotatedY;
                lng = centerLatLng.lng + rotatedX;
            }
            
            points.push([lat, lng]);
        }
        
        console.log(`🔵 C-shaped arc created with ${points.length} points`);
        return points;
    }

    /**
     * Create parallel lines (||) points for the right side of the policy zone
     */
    createParallelLines(centerLatLng, sideDegrees, frontDegrees, rearDegrees, rotationRad) {
        const points = [];
        const lineSpacing = sideDegrees * 0.1; // Small spacing
        const lineLength = sideDegrees * 0.2; // Small length
        const lineWidth = sideDegrees * 0.05; // Small width
        
        console.log(`🔵 Parallel lines: spacing=${lineSpacing}, length=${lineLength}, width=${lineWidth}`);
        
        // Position on the right side - very close to center
        const rightPosition = centerLatLng.lng + sideDegrees * 0.1;
        const topPosition = centerLatLng.lat + sideDegrees * 0.05;
        const bottomPosition = centerLatLng.lat - sideDegrees * 0.05;
        
        console.log(`🔵 Parallel lines position: right=${rightPosition}, top=${topPosition}, bottom=${bottomPosition}`);
        
        // Create two parallel vertical lines
        const lines = [
            // First line (outer)
            [
                [topPosition, rightPosition],
                [bottomPosition, rightPosition],
                [bottomPosition, rightPosition + lineWidth],
                [topPosition, rightPosition + lineWidth]
            ],
            // Second line (inner)
            [
                [topPosition, rightPosition + lineSpacing],
                [bottomPosition, rightPosition + lineSpacing],
                [bottomPosition, rightPosition + lineSpacing + lineWidth],
                [topPosition, rightPosition + lineSpacing + lineWidth]
            ]
        ];
        
        // Apply rotation and add to points
        lines.forEach((line, lineIndex) => {
            line.forEach(point => {
                let lat = point[0];
                let lng = point[1];
                
                // Apply rotation if needed
                if (rotationRad !== 0) {
                    const cos = Math.cos(rotationRad);
                    const sin = Math.sin(rotationRad);
                    const x = lng - centerLatLng.lng;
                    const y = lat - centerLatLng.lat;
                    
                    const rotatedX = x * cos - y * sin;
                    const rotatedY = x * sin + y * cos;
                    
                    lat = centerLatLng.lat + rotatedY;
                    lng = centerLatLng.lng + rotatedX;
                }
                
                points.push([lat, lng]);
            });
        });
        
        console.log(`🔵 Parallel lines created with ${points.length} points`);
        return points;
    }
    
    /**
     * Get standardized force type from various input formats
     */
    getStandardizedForceType(input) {
        if (!input) return 'Blue Land'; // Default
        
        const lower = input.toLowerCase().trim();
        
        // Blue Land variations
        if (lower.includes('blue') || lower.includes('friendly') || lower.includes('blueland')) {
            return 'Blue Land';
        }
        
        // Fox Land variations  
        if (lower.includes('fox') || lower.includes('hostile') || lower.includes('red') || lower.includes('foxland')) {
            return 'Fox Land';
        }
        
        // UN variations
        if (lower.includes('un') || lower.includes('united nations') || lower.includes('neutral') || lower.includes('control')) {
            return 'UN';
        }
        
        // Default to Blue Land if not recognized
        return 'Blue Land';
    }

    /**
     * Add parallel lines (||) decoration on the front side of the oval for Blue Land tokens
     * Returns array of polyline layers for cleanup
     */
    addParallelLinesToOval(centerLatLng, sideDegrees, frontDegrees, rearDegrees, rotationRad, strokeColor) {
        const lineSpacing = frontDegrees * 0.1; // Space between the two parallel lines
        const lineWidth = sideDegrees * 0.6; // Width of the parallel lines
        
        // Position at the front (north) of the oval
        const frontPosition = centerLatLng.lat + frontDegrees * 0.75; // 75% towards front edge
        
        // Create two horizontal parallel lines at the front
        const lines = [
            // First line (outer/front)
            [[frontPosition, centerLatLng.lng - lineWidth/2], [frontPosition, centerLatLng.lng + lineWidth/2]],
            // Second line (inner, slightly behind the first)
            [[frontPosition - lineSpacing, centerLatLng.lng - lineWidth/2], [frontPosition - lineSpacing, centerLatLng.lng + lineWidth/2]]
        ];
        
        // Apply rotation if needed
        if (rotationRad !== 0) {
            const cos = Math.cos(rotationRad);
            const sin = Math.sin(rotationRad);
            
            lines.forEach(line => {
                line.forEach(point => {
                    const x = point[1] - centerLatLng.lng;
                    const y = point[0] - centerLatLng.lat;
                    
                    const rotatedX = x * cos - y * sin;
                    const rotatedY = x * sin + y * cos;
                    
                    point[0] = centerLatLng.lat + rotatedY;
                    point[1] = centerLatLng.lng + rotatedX;
                });
            });
        }
        
        // Draw the parallel lines and store references
        const polylines = [];
        lines.forEach(lineCoords => {
            const polyline = L.polyline(lineCoords, {
                color: strokeColor,
                weight: 2,
                opacity: 0.8
            }).addTo(this.map);
            polylines.push(polyline);
        });
        
        console.log('✅ Added parallel lines (||) decoration to front of Blue Land coverage area');
        return polylines;
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
     * Remove coverage areas for a specific token
     */
    removeCoverageAreas(tokenId) {
        if (!this.coverageAreas) {
            console.log(`⚠️ No coverage areas map found for token ${tokenId}`);
            return;
        }

        console.log(`🧹 Removing coverage areas for token: ${tokenId}`);
        console.log(`🧹 Current coverage areas:`, Array.from(this.coverageAreas.keys()));

        // Find and remove coverage areas for this specific token
        const coverageIdsToRemove = [];
        for (const [coverageId, polygon] of this.coverageAreas) {
            // Check if this coverage area belongs to the token
            // Coverage IDs can be in format: "token_coverage_{tokenId}" or just the coverage ID
            if (coverageId.includes(tokenId) || coverageId === `token_coverage_${tokenId}`) {
                console.log(`🧹 Removing coverage area: ${coverageId}`);
                if (this.map && this.map.hasLayer(polygon)) {
                    this.map.removeLayer(polygon);
                }
                coverageIdsToRemove.push(coverageId);
            }
        }

        // Remove from the coverage areas map
        coverageIdsToRemove.forEach(id => {
            this.coverageAreas.delete(id);
        });

        console.log(`✅ Removed ${coverageIdsToRemove.length} coverage areas for token ${tokenId}`);
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
     * Show token context menu on right-click
     */
    showTokenContextMenu(e, tokenInfo) {
        e.originalEvent.preventDefault();
        e.originalEvent.stopPropagation();
        
        // Extract token data from tokenInfo
        const token = tokenInfo.token || tokenInfo;
        const marker = tokenInfo.marker;
        
        console.log('🎯 Showing context menu for token:', token);
        
        // Remove any existing context menu
        const existingMenu = document.querySelector('.token-context-menu');
        if (existingMenu) {
            existingMenu.remove();
        }
        
        // Create context menu
        const menu = document.createElement('div');
        menu.className = 'token-context-menu';
        menu.style.cssText = `
            position: fixed;
            background: #2a2a2a;
            border: 1px solid #444;
            border-radius: 8px;
            box-shadow: 0 4px 12px rgba(0,0,0,0.5);
            z-index: 10000;
            min-width: 200px;
            padding: 8px 0;
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
            color: #fff;
        `;
        
        // Menu items
        const menuItems = [
            {
                icon: 'fas fa-route',
                text: 'Movement Planning',
                action: () => {
                    console.log('🎯 Context menu calling showConfirmMoveModal for token:', token.name);
                    console.log('🎯 TokenPlacementManager instance:', this);
                    console.log('🎯 showConfirmMoveModal method exists:', typeof this.showConfirmMoveModal);
                    this.showConfirmMoveModal(token);
                }
            },
            {
                icon: 'fas fa-info-circle',
                text: 'Token Summary',
                action: () => this.showTokenSummary(token)
            },
            {
                icon: 'fas fa-draw-polygon',
                text: 'Coverage Areas',
                action: () => this.showCoverageAreaManagement(token)
            },
            {
                icon: 'fas fa-map-marker-alt',
                text: 'Organizational Signs',
                action: () => this.showOrganizationalSignManagement(token)
            },
            {
                icon: 'fas fa-edit',
                text: 'Edit Token',
                action: () => this.editToken(token)
            },
            {
                icon: 'fas fa-trash',
                text: 'Remove from Map',
                action: () => this.removeTokenFromMap(token.id),
                className: 'text-danger'
            }
        ];
        
        menuItems.forEach(item => {
            const menuItem = document.createElement('div');
            menuItem.className = `context-menu-item ${item.className || ''}`;
            menuItem.style.cssText = `
                padding: 8px 16px;
                cursor: pointer;
                display: flex;
                align-items: center;
                gap: 8px;
                transition: background-color 0.2s;
            `;
            
            menuItem.innerHTML = `
                <i class="${item.icon}"></i>
                <span>${item.text}</span>
            `;
            
            menuItem.addEventListener('mouseenter', () => {
                menuItem.style.backgroundColor = '#3a3a3a';
            });
            
            menuItem.addEventListener('mouseleave', () => {
                menuItem.style.backgroundColor = 'transparent';
            });
            
            menuItem.addEventListener('click', () => {
                item.action();
                menu.remove();
            });
            
            menu.appendChild(menuItem);
        });
        
        // Position menu at cursor
        menu.style.left = `${e.originalEvent.clientX}px`;
        menu.style.top = `${e.originalEvent.clientY}px`;
        
        // Add to document
        document.body.appendChild(menu);
        
        // Remove menu when clicking elsewhere
        const removeMenu = (e) => {
            if (!menu.contains(e.target)) {
                menu.remove();
                document.removeEventListener('click', removeMenu);
            }
        };
        
        setTimeout(() => {
            document.addEventListener('click', removeMenu);
        }, 100);
    }



    /**
     * Get force type color for badges
     */
    getForceTypeColor(forceType) {
        const colors = {
            'Friendly': 'primary',
            'Hostile': 'danger', 
            'Neutral': 'success',
            'Unknown': 'warning'
        };
        return colors[forceType] || 'secondary';
    }

    /**
     * Get organization level name
     */
    getOrgLevelName(orgLevel) {
        const levels = {
            1: 'Squad (8-13 personnel)',
            2: 'Platoon (26-64 personnel)',
            3: 'Company (80-250 personnel)',
            4: 'Battalion (300-1,000 personnel)',
            5: 'Regiment (1,000-3,000 personnel)',
            6: 'Brigade (3,000-5,000 personnel)',
            7: 'Division (10,000-25,000 personnel)',
            8: 'Corps (20,000-45,000 personnel)',
            9: 'Army (50,000+ personnel)'
        };
        return levels[orgLevel] || 'Unknown Level';
    }

    /**
     * Get unit type name
     */
    getUnitTypeName(unitType) {
        const types = {
            0: 'Infantry',
            1: 'Armoured',
            2: 'Mechanized',
            3: 'Artillery',
            4: 'Aviation',
            5: 'Air Defense',
            6: 'Engineers',
            7: 'Signals',
            8: 'Logistics',
            9: 'Medical',
            10: 'Reconnaissance',
            11: 'Special Forces',
            12: 'Airborne/Paratroop',
            13: 'Marines',
            14: 'Cavalry',
            15: 'Headquarters/Command',
            16: 'Intelligence',
            17: 'Military Police',
            18: 'CBRN',
            19: 'Maintenance',
            20: 'Cyber'
        };
        return types[unitType] || 'Unknown Type';
    }

    /**
     * Update area coverages for a token
     */
    updateAreaCoverages(tokenId, areaCoverages) {
        console.log('🎯 Updating area coverages for token:', tokenId, areaCoverages);
        
        const tokenInfo = this.placedTokens.get(tokenId);
        if (tokenInfo && tokenInfo.marker) {
            // Store the area coverages
            tokenInfo.coverageAreas = areaCoverages;
            
            // Recreate coverage areas on the map
            const markerLatLng = tokenInfo.marker.getLatLng();
            this.createCoverageAreas(null, markerLatLng, tokenInfo.token.forceType, tokenInfo.token, areaCoverages);
        }
    }

    /**
     * Save movement plan
     */
    saveMovementPlan(tokenId) {
        const movementType = document.getElementById('movementType').value;
        const movementSpeed = document.getElementById('movementSpeed').value;
        const routeDescription = document.getElementById('routeDescription').value;
        const estimatedDuration = document.getElementById('estimatedDuration').value;
        const specialInstructions = document.getElementById('specialInstructions').value;
        const priorityLevel = document.getElementById('priorityLevel').value;
        const weatherConditions = document.getElementById('weatherConditions').value;
        const visibility = document.getElementById('visibility').value;
        const equipmentResources = document.getElementById('equipmentResources').value;
        
        const movementPlan = {
            tokenId: tokenId,
            movementType: movementType,
            movementSpeed: movementSpeed,
            routeDescription: routeDescription,
            estimatedDuration: estimatedDuration,
            specialInstructions: specialInstructions,
            priorityLevel: priorityLevel,
            weatherConditions: weatherConditions,
            visibility: visibility,
            equipmentResources: equipmentResources,
            timestamp: new Date().toISOString()
        };
        
        console.log('💾 Saving movement plan:', movementPlan);
        
        // Here you would typically save to backend
        // For now, just show success message
        if (this.notificationCallback) {
            this.notificationCallback(`Movement plan saved for token ${tokenId}`, 'success');
        }
        
        // Close modal
        document.querySelector('.modal').remove();
    }

    /**
     * Show existing token details using TokenManager
     */
    showExistingTokenDetails(token) {
        console.log('📋 Showing existing token details for:', token);
        try {
            if (typeof tokenManager !== 'undefined' && tokenManager && typeof tokenManager.showTokenDetails === 'function') {
                console.log('✅ Using tokenManager.showTokenDetails');
                tokenManager.showTokenDetails(token);
                return;
            }
            console.warn('tokenManager.showTokenDetails not available, trying direct method');
            
            // Fallback: Try direct AJAX call
            if (token && token.id) {
                console.log('🔄 Fallback: Loading token summary directly');
                    $("#simpleLoader").show();
                
                $.ajax({
                    url: '/DataManagement/GetTokenSummary',
                    type: 'GET',
                    data: { tokenId: token.id },
                    success: function(modalHtml) {
                        console.log('✅ Token summary loaded via fallback');
                        $('#tokenSummaryModal').remove();
                        $('body').append(modalHtml);
                        
                        const modal = document.getElementById('tokenSummaryModal');
                        if (modal) {
                            modal.style.display = 'flex';
                            window.currentTokenId = token.id;
                        }
                    },
                    error: function(xhr, status, error) {
                        console.error('❌ Fallback token summary failed:', error);
                        alert('Failed to load token summary: ' + error);
                    },
                    complete: function() {
                        $("#simpleLoader").hide();
                    }
                });
            } else {
                console.error('❌ Invalid token data');
                alert('Invalid token data');
            }
        } catch (err) {
            console.error('Error showing token details:', err);
            alert('Error showing token details: ' + err.message);
        }
    }

    /**
     * Format token position for display
     */
    formatTokenPosition(position) {
        if (!position) return 'Not placed';
        
        const lat = typeof position.lat === 'string' ? parseFloat(position.lat) : position.lat;
        const lng = typeof position.lng === 'string' ? parseFloat(position.lng) : position.lng;
        
        if (isNaN(lat) || isNaN(lng)) return 'Invalid position';
        
        return `${lat.toFixed(6)}, ${lng.toFixed(6)}`;
    }

    /**
     * Show token summary with coverage areas and organizational signs
     */
    showTokenSummary(token) {
        console.log('🎯 Showing token summary for:', token.name);
        
        // Use the existing tokenManager.showTokenDetails method
        if (window.tokenManager && window.tokenManager.showTokenDetails) {
            window.tokenManager.showTokenDetails(token);
        } else {
            console.error('TokenManager not available for showing token summary');
            if (this.notificationCallback) {
                this.notificationCallback('Token summary not available', 'error');
            }
        }
    }

    /**
     * Show coverage area management
     */
    showCoverageAreaManagement(token) {
        console.log('🗺️ Showing coverage area management for:', token.name);
        
        // Store token data for modal context
        this.currentModalToken = token;
        
        // Start coverage area mapping immediately
        console.log('🗺️ Starting coverage area mapping immediately for token:', token.name);
        this.startCoverageMapping(token, 'Operational');
    }

    /**
     * Show organizational sign management
     */
    showOrganizationalSignManagement(token) {
        console.log('🏷️ Showing organizational sign management for:', token.name);
        
        // Store token data for modal context
        this.currentModalToken = token;
        
        const modal = document.createElement('div');
        modal.className = 'gameplay-modal';
        modal.id = 'organizationalSignManagementModal';
        modal.innerHTML = `
            <div class="gameplay-modal-content">
                <div class="gameplay-modal-header">
                    <h3><i class="fas fa-map-marker-alt"></i> Organizational Sign Management - ${token.name}</h3>
                    <button class="gameplay-modal-close" onclick="this.closest('.gameplay-modal').remove()">&times;</button>
                </div>
                <div class="gameplay-modal-body">
                    <div class="organizational-sign-management">
                        <div class="organizational-sign-list">
                            <h6>Existing Organizational Signs</h6>
                            <div id="organizational-sign-list">
                                ${this.renderOrganizationalSignList(token)}
                            </div>
                        </div>
                        
                        <div class="organizational-sign-actions">
                            <h6>Add New Organizational Sign</h6>
                            <p>Click on the map to place an organizational sign for this token.</p>
                            <button class="btn btn-success" onclick="window.tokenPlacementManager.startOrganizationalSignMappingFromModal()">
                                <i class="fas fa-map-marker-alt"></i> Place Sign
                            </button>
                        </div>
                    </div>
                </div>
                <div class="gameplay-modal-footer">
                    <button class="btn btn-secondary" onclick="this.closest('.gameplay-modal').remove()">Close</button>
                </div>
            </div>
        `;
        
        document.body.appendChild(modal);
    }

    /**
     * Start coverage area mapping from modal
     */
    startCoverageMappingFromModal() {
        console.log('🗺️ Starting coverage mapping from modal...');
        console.log('🗺️ Current modal token:', this.currentModalToken);
        
        // Get the coverage type from the select
        const coverageTypeSelect = document.getElementById('coverageType');
        const coverageType = coverageTypeSelect ? coverageTypeSelect.value : 'Operational';
        console.log('🗺️ Selected coverage type:', coverageType);
        
        // Get token data from the modal context
        const tokenData = this.currentModalToken;
        if (!tokenData) {
            console.error('❌ Token data not available in modal context');
            if (window.showNotification) {
                window.showNotification('Token data not available', 'error');
            }
            return;
        }
        
        console.log('🗺️ Starting coverage mapping for token:', tokenData.name, 'ID:', tokenData.id);
        
        // Start the mapping with token ID
        this.startCoverageMapping(tokenData, coverageType);
    }

    /**
     * Start coverage area mapping
     */
    startCoverageMapping(token, coverageType) {
        console.log(`🗺️ Starting coverage mapping for ${token.name}, type: ${coverageType}`);
        debugger; // DEBUG: Check if method is called with correct parameters
        
        // Close modal
        const modal = document.getElementById('coverageAreaManagementModal');
        console.log('🗺️ Modal to close found:', !!modal);
        debugger; // DEBUG: Check modal removal
        
        if (modal) {
            modal.remove();
            console.log('🗺️ Modal removed');
        }
        
        // Initialize coverage area manager if not exists
        console.log('🗺️ CoverageAreaManager exists:', !!window.coverageAreaManager);
        debugger; // DEBUG: Check CoverageAreaManager existence
        
        if (!window.coverageAreaManager) {
            console.log('🗺️ Initializing CoverageAreaManager...');
            try {
                console.log('🗺️ Map available:', !!this.map);
                console.log('🗺️ TokenManager available:', !!window.tokenManager);
                debugger; // DEBUG: Check dependencies
                
                window.coverageAreaManager = new CoverageAreaManager(this.map, window.tokenManager);
                console.log('✅ CoverageAreaManager initialized successfully');
                debugger; // DEBUG: Check initialization success
            } catch (error) {
                console.error('❌ Error initializing CoverageAreaManager:', error);
                debugger; // DEBUG: Check initialization error
                if (window.showNotification) {
                    window.showNotification('Error initializing coverage area manager', 'error');
                }
                return;
            }
        }
        
        // Start mapping
        try {
            console.log('🗺️ Calling startCoverageMapping on CoverageAreaManager...');
            console.log('🗺️ CoverageAreaManager object:', window.coverageAreaManager);
            console.log('🗺️ startCoverageMapping method exists:', typeof window.coverageAreaManager.startCoverageMapping);
            debugger; // DEBUG: Check CoverageAreaManager method
            
            window.coverageAreaManager.startCoverageMapping(token, coverageType);
            console.log('✅ Coverage mapping started successfully');
            debugger; // DEBUG: Check if mapping started
        } catch (error) {
            console.error('❌ Error starting coverage mapping:', error);
            debugger; // DEBUG: Check mapping error
            if (window.showNotification) {
                window.showNotification('Error starting coverage mapping', 'error');
            }
        }
    }

    /**
     * Start organizational sign mapping from modal
     */
    startOrganizationalSignMappingFromModal() {
        console.log('🏷️ Starting organizational sign mapping from modal...');
        
        // Get token data from the modal context
        const tokenData = this.currentModalToken;
        if (!tokenData) {
            console.error('❌ Token data not available in modal context');
            if (window.showNotification) {
                window.showNotification('Token data not available', 'error');
            }
            return;
        }
        
        console.log('🏷️ Using token data:', tokenData);
        
        // Start the mapping
        this.startOrganizationalSignMapping(tokenData);
    }

    /**
     * Start organizational sign mapping
     */
    startOrganizationalSignMapping(token) {
        console.log(`🏷️ Starting organizational sign mapping for ${token.name}`);
        
        // Close modal
        const modal = document.getElementById('organizationalSignManagementModal');
        if (modal) {
            modal.remove();
        }
        
        // Initialize coverage area manager if not exists
        if (!window.coverageAreaManager) {
            console.log('🗺️ Initializing CoverageAreaManager...');
            window.coverageAreaManager = new CoverageAreaManager(this.map, window.tokenManager);
        }
        
        // Start mapping
        window.coverageAreaManager.startOrganizationalSignMapping(token);
    }

    /**
     * Render coverage area list
     */
    renderCoverageAreaList(token) {
        const coverageAreas = token.areaCoverages || [];
        
        if (coverageAreas.length === 0) {
            return '<p class="text-muted">No coverage areas defined</p>';
        }
        
        return coverageAreas.map(area => `
            <div class="coverage-area-item">
                <div class="coverage-area-info">
                    <div class="coverage-area-type">${area.coverageType || 'Operational'}</div>
                    <div class="coverage-area-details">${area.shapeType || 'Custom'} - ${area.geometry?.coordinates?.[0]?.length || 0} points</div>
                </div>
                <div class="coverage-area-actions">
                    <button class="btn btn-sm btn-outline-primary" onclick="window.tokenPlacementManager.editCoverageArea('${area.id}')">
                        <i class="fas fa-edit"></i>
                    </button>
                    <button class="btn btn-sm btn-outline-danger" onclick="window.tokenPlacementManager.deleteCoverageArea('${area.id}')">
                        <i class="fas fa-trash"></i>
                    </button>
                </div>
            </div>
        `).join('');
    }

    /**
     * Render organizational sign list
     */
    renderOrganizationalSignList(token) {
        const organizationalSigns = token.organizationalSigns || [];
        
        if (organizationalSigns.length === 0) {
            return '<p class="text-muted">No organizational signs placed</p>';
        }
        
        return organizationalSigns.map(sign => `
            <div class="organizational-sign-item">
                <div class="organizational-sign-info">
                    <div class="organizational-sign-level">${token.organizationLevel || 'ORG'}</div>
                    <div class="organizational-sign-position">${sign.position?.lat?.toFixed(6)}, ${sign.position?.lng?.toFixed(6)}</div>
                </div>
                <div class="organizational-sign-actions">
                    <button class="btn btn-sm btn-outline-primary" onclick="window.tokenPlacementManager.editOrganizationalSign('${sign.id}')">
                        <i class="fas fa-edit"></i>
                    </button>
                    <button class="btn btn-sm btn-outline-danger" onclick="window.tokenPlacementManager.deleteOrganizationalSign('${sign.id}')">
                        <i class="fas fa-trash"></i>
                    </button>
                </div>
            </div>
        `).join('');
    }

    /**
     * Edit coverage area
     */
    editCoverageArea(areaId) {
        console.log('✏️ Editing coverage area:', areaId);
        // TODO: Implement coverage area editing
        if (window.showNotification) {
            window.showNotification('Coverage area editing not yet implemented', 'info');
        }
    }

    /**
     * Delete coverage area
     */
    deleteCoverageArea(areaId) {
        console.log('🗑️ Deleting coverage area:', areaId);
        // TODO: Implement coverage area deletion
        if (window.showNotification) {
            window.showNotification('Coverage area deletion not yet implemented', 'info');
        }
    }

    /**
     * Edit organizational sign
     */
    editOrganizationalSign(signId) {
        console.log('✏️ Editing organizational sign:', signId);
        // TODO: Implement organizational sign editing
        if (window.showNotification) {
            window.showNotification('Organizational sign editing not yet implemented', 'info');
        }
    }

    /**
     * Delete organizational sign
     */
    deleteOrganizationalSign(signId) {
        console.log('🗑️ Deleting organizational sign:', signId);
        // TODO: Implement organizational sign deletion
        if (window.showNotification) {
            window.showNotification('Organizational sign deletion not yet implemented', 'info');
        }
    }

    /**
     * Edit token
     */
    editToken(token) {
        console.log('✏️ Editing token:', token.name);
        // Open edit page in new tab
        window.open(`/AdminToken/Edit/${token.id}`, '_blank');
    }

    /**
     * Handle token click based on current mode
     */
    handleTokenClick(token, marker) {
        const currentMode = window.tokenActionModeManager?.getCurrentMode();
        
        switch (currentMode) {
            case null: // No mode selected - show token details
            case 'select':
                // Show token details modal instead of movement planning
                this.showExistingTokenDetails(token);
                break;
                
            case 'attack':
                // Start attack mode with this token as attacker
                if (window.tokenActionModeManager) {
                    window.tokenActionModeManager.handleTokenAttack(marker.getLatLng());
                }
                break;
                
            case 'pan-attack':
                // Start pan attack mode with this token
                if (window.tokenActionModeManager) {
                    window.tokenActionModeManager.handlePanAttack(marker.getLatLng());
                }
                break;
                
            case 'move':
                // Show movement planning modal
                this.dragPreview = {
                    marker: marker,
                    token: token
                };
                this.originalPosition = marker.getLatLng();
                this.showConfirmMoveModal(token);
                break;
                
            case 'place':
                // In placement mode, clicking tokens shouldn't do anything
                console.log('Token click ignored in placement mode');
                break;
                
            default:
                // Fallback to showing token details
                this.showExistingTokenDetails(token);
                break;
        }
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

    /**
     * Start attack mode for token-to-token attacks
     */
    startAttackMode(attackerTokenId) {
        try {
            // Get attacker token info
            const attackerToken = this.placedTokens.get(attackerTokenId);
            if (!attackerToken) {
                console.error('Attacker token not found:', attackerTokenId);
                return;
            }

            // Store attacker token for attack planning
            this.attackerToken = attackerToken;
            this.attackMode = true;

            // Show notification
            if (this.notificationCallback) {
                this.notificationCallback('Attack mode activated. Click on an enemy token to target.', 'info');
            }

            // Change cursor to crosshair
            this.map.getContainer().style.cursor = 'crosshair';

            // Add click handler for target selection
            this.map.off('click', this.attackTargetHandler);
            this.attackTargetHandler = (e) => {
                this.selectAttackTarget(e.latlng);
            };
            this.map.on('click', this.attackTargetHandler);

            console.log('Attack mode started for token:', attackerTokenId);
        } catch (err) {
            console.error('Error starting attack mode:', err);
        }
    }

    /**
     * Select attack target
     */
    selectAttackTarget(latlng) {
        try {
            if (!this.attackMode || !this.attackerToken) {
                return;
            }

            // Find token at clicked location
            const targetToken = this.findTokenAtLocation(latlng);
            if (!targetToken) {
                if (this.notificationCallback) {
                    this.notificationCallback('No token found at selected location.', 'warning');
                }
                return;
            }

            // Check if target is different from attacker
            if (targetToken.id === this.attackerToken.id) {
                if (this.notificationCallback) {
                    this.notificationCallback('Cannot attack own token.', 'warning');
                }
                return;
            }

            // Open attack panel
            this.openAttackPanel(this.attackerToken, targetToken);

            // Exit attack mode
            this.exitAttackMode();
        } catch (err) {
            console.error('Error selecting attack target:', err);
        }
    }

    /**
     * Find token at specific location
     */
    findTokenAtLocation(latlng) {
        for (const [tokenId, tokenInfo] of this.placedTokens) {
            const markerLatLng = tokenInfo.marker.getLatLng();
            const distance = this.map.distance(latlng, markerLatLng);
            
            // If within 50 meters, consider it a hit
            if (distance < 50) {
                return tokenInfo.token;
            }
        }
        return null;
    }

    /**
     * Open attack panel
     */
    openAttackPanel(attackerToken, targetToken) {
        console.log('🎯 openAttackPanel called with:', attackerToken?.name, '->', targetToken?.name);
        try {
            // Remove any existing attack panel modal first
            $('#attackPanelModal').remove();
            $('.modal-backdrop').remove();
            
            // Create attack panel modal - Bootstrap Modal
            const attackPanelHtml = `
                <div class="modal fade" id="attackPanelModal" tabindex="-1" role="dialog" aria-labelledby="attackPanelModalLabel" aria-hidden="true">
                    <div class="modal-dialog modal-lg" role="document">
                        <div class="modal-content">
                            <div class="modal-header">
                                <h5 class="modal-title" id="attackPanelModalLabel">
                                    <i class="fas fa-crosshairs"></i> Plan Attack
                                </h5>
                                <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                                    <i class="fas fa-times"></i>
                                </button>
                            </div>
                            <div class="modal-body">
                            <div class="attack-planning-form">
                                <div class="alert alert-info">
                                    <i class="fas fa-info-circle"></i>
                                    <span>Planning attack from <strong>${attackerToken.name}</strong> to <strong>${targetToken.name}</strong></span>
                                </div>

                                <div class="form-group">
                                    <label for="attackAxisId">Axis ID (Optional)</label>
                                    <input type="text" id="attackAxisId" class="form-control" placeholder="e.g., Alpha, Bravo, Charlie" />
                                </div>

                                <div class="form-group">
                                    <label for="attackPosture">Attack Posture</label>
                                    <select id="attackPosture" class="form-control">
                                        <option value="Advance">Advance</option>
                                        <option value="Fix">Fix</option>
                                        <option value="Feint">Feint</option>
                                    </select>
                                </div>

                                <div class="form-group">
                                    <label for="attackMpReserve">Movement Points Reserve (%)</label>
                                    <input type="range" id="attackMpReserve" class="form-range" min="0" max="50" value="10" oninput="document.getElementById('mpReserveValue').textContent = this.value + '%'" />
                                    <div class="d-flex justify-content-between">
                                        <span>0%</span>
                                        <span id="mpReserveValue">10%</span>
                                        <span>50%</span>
                                    </div>
                                </div>

                                <div class="form-group">
                                    <label for="attackStartTurn">Expected Start Turn</label>
                                    <input type="number" id="attackStartTurn" class="form-control" min="1" value="1" />
                                </div>

                                <div class="form-group">
                                    <label for="attackDuration">Duration (Turns)</label>
                                    <input type="number" id="attackDuration" class="form-control" min="1" max="10" value="1" />
                                </div>

                                <div class="form-group">
                                    <label for="attackExecutionMode">Execution Mode</label>
                                    <select id="attackExecutionMode" class="form-control">
                                        <option value="Plan">Plan for Later</option>
                                        <option value="ExecuteNow">Execute Now</option>
                                    </select>
                                </div>

                                <div class="form-actions">
                                    <button type="button" class="btn btn-info" onclick="tokenPlacementManager.previewAttack()">
                                        <i class="fas fa-eye"></i> Preview
                                    </button>
                                    <button type="button" class="btn btn-primary" onclick="tokenPlacementManager.executeAttack()">
                                        <i class="fas fa-play"></i> Execute Attack
                                    </button>
                                    <button type="button" class="btn btn-secondary" data-dismiss="modal">
                                        <i class="fas fa-times"></i> Cancel
                                    </button>
                                </div>
                            </div>
                        </div>
                    </div>
                    </div>
                </div>
            `;

            // Add to page
            $('body').append(attackPanelHtml);

            // Store token references
            this.currentAttackerToken = attackerToken;
            this.currentTargetToken = targetToken;

            // Force show Bootstrap modal with setTimeout to ensure DOM is ready
            setTimeout(function() {
                const modalElement = document.getElementById('attackPanelModal');
                console.log('🎯 Attack panel in DOM:', modalElement ? 'YES' : 'NO');
                console.log('🎯 Modal HTML length:', attackPanelHtml.length);
                
                if (modalElement) {
                    console.log('🎯 Modal element found, applying styles...');
                    $('#attackPanelModal').css({
                        'display': 'block',
                        'opacity': '1',
                        'visibility': 'visible',
                        'z-index': '9999'
                    }).addClass('show').removeClass('fade');
                    
                    console.log('🎯 Initializing Bootstrap modal...');
                    $('#attackPanelModal').modal({
                        backdrop: false,
                        keyboard: true,
                        show: true
                    });
                    
                    console.log('✅ Attack panel displayed successfully');
                    
                    // Additional check to ensure modal is visible
                    setTimeout(function() {
                        const modalVisible = $('#attackPanelModal').is(':visible');
                        console.log('🎯 Modal visibility check:', modalVisible);
                        if (!modalVisible) {
                            console.error('❌ Modal is not visible after initialization');
                            // Force show again
                            $('#attackPanelModal').show().addClass('show');
                        }
                    }, 200);
                } else {
                    console.error('❌ Attack panel not found in DOM after append');
                    console.log('🎯 Body HTML length:', $('body').html().length);
                }
            }, 100);
        } catch (err) {
            console.error('Error opening attack panel:', err);
        }
    }

    /**
     * Close attack panel
     */
    closeAttackPanel() {
        try {
            $('#attackPanelModal').hide().removeClass('show');
            $('.modal-backdrop').remove();
            $('body').removeClass('modal-open');
            $('#attackPanelModal').remove();
            this.currentAttackerToken = null;
            this.currentTargetToken = null;
        } catch (err) {
            console.error('Error closing attack panel:', err);
        }
    }

    /**
     * Preview attack
     */
    async previewAttack() {
        try {
            if (!this.currentAttackerToken || !this.currentTargetToken) {
                return;
            }

            const startTurn = parseInt(document.getElementById('attackStartTurn').value) || 1;
            
            // Call preview API
            const response = await fetch(`/api/orders/preview-attack-token?attackerId=${this.currentAttackerToken.id}&targetId=${this.currentTargetToken.id}&startTurn=${startTurn}`);
            const result = await response.json();

            if (result.success) {
                // Show preview results
                this.showAttackPreview(result.preview);
            } else {
                if (this.notificationCallback) {
                    this.notificationCallback('Failed to preview attack: ' + result.message, 'error');
                }
            }
        } catch (err) {
            console.error('Error previewing attack:', err);
            if (this.notificationCallback) {
                this.notificationCallback('Error previewing attack', 'error');
            }
        }
    }

    /**
     * Show attack preview results
     */
    showAttackPreview(preview) {
        const previewHtml = `
            <div class="attack-preview-results">
                <h5>Attack Preview</h5>
                <div class="row">
                    <div class="col-md-6">
                        <p><strong>Detection Confidence:</strong> ${(preview.detectionConfidence * 100).toFixed(1)}%</p>
                        <p><strong>Attacker Combat Power:</strong> ${preview.attackerEffectiveCombatPower.toFixed(1)}</p>
                        <p><strong>Defender Combat Power:</strong> ${preview.defenderEffectiveCombatPowerEstimated.toFixed(1)}</p>
                    </div>
                    <div class="col-md-6">
                        <p><strong>Attacker Casualties:</strong> ${preview.attackerExpectedCasualtyPercent.toFixed(1)}%</p>
                        <p><strong>Defender Casualties:</strong> ${preview.defenderExpectedCasualtyPercent.toFixed(1)}%</p>
                        <p><strong>Success Probability:</strong> ${(preview.probabilityOfSuccess * 100).toFixed(1)}%</p>
                    </div>
                </div>
                ${preview.uncertaintyNotes ? `<div class="alert alert-warning">${preview.uncertaintyNotes}</div>` : ''}
                ${preview.supplyWarnings.length > 0 ? `<div class="alert alert-danger">${preview.supplyWarnings.join('<br>')}</div>` : ''}
            </div>
        `;

        // Insert preview into modal
        const modalBody = document.querySelector('#attackPanelModal .gameplay-modal-body');
        const existingPreview = modalBody.querySelector('.attack-preview-results');
        if (existingPreview) {
            existingPreview.remove();
        }
        modalBody.insertAdjacentHTML('beforeend', previewHtml);
    }

    /**
     * Execute attack
     */
    async executeAttack() {
        try {
            if (!this.currentAttackerToken || !this.currentTargetToken) {
                return;
            }

            // Gather form data including NATO attack intent
            const attackData = {
                attackerId: this.currentAttackerToken.id,
                targetId: this.currentTargetToken.id,
                axisId: document.getElementById('attackAxisId').value || null,
                artilleryAttached: [], // TODO: Add artillery selection
                mpReservePercent: parseInt(document.getElementById('attackMpReserve').value) / 100,
                posture: document.getElementById('attackPosture').value,
                expectedStartTurn: parseInt(document.getElementById('attackStartTurn').value),
                durationTurns: parseInt(document.getElementById('attackDuration').value),
                executionMode: document.getElementById('attackExecutionMode').value,
                notes: `Attack planned from ${this.currentAttackerToken.name} to ${this.currentTargetToken.name}`,
                // NATO Attack Intent Data
                intent: {
                    attackPreparation: document.getElementById('AttackPreparation')?.value || 'Deliberate',
                    natoAttackType: document.getElementById('NatoAttackType')?.value || 'frontal',
                    attackIntensity: document.getElementById('AttackIntensity')?.value || 'standard',
                    coordinationType: document.getElementById('CoordinationType')?.value || 'independent',
                    desiredEffect: document.getElementById('DesiredEffect')?.value || 'Destroy',
                    notes: document.getElementById('Notes')?.value || ''
                }
            };

            // Call plan attack API
            const response = await fetch('/api/orders/plan-attack-token', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(attackData)
            });

            const result = await response.json();

            if (result.success) {
                if (this.notificationCallback) {
                    this.notificationCallback(`Attack ${result.status === 'Executed' ? 'executed' : 'planned'} successfully!`, 'success');
                }
                this.closeAttackPanel();
            } else {
                if (this.notificationCallback) {
                    this.notificationCallback('Failed to execute attack: ' + result.message, 'error');
                }
            }
        } catch (err) {
            console.error('Error executing attack:', err);
            if (this.notificationCallback) {
                this.notificationCallback('Error executing attack', 'error');
            }
        }
    }

    /**
     * Exit attack mode
     */
    exitAttackMode() {
        this.attackMode = false;
        this.attackerToken = null;
        this.map.getContainer().style.cursor = '';
        this.map.off('click', this.attackTargetHandler);
    }
}

// Export for use in other modules
if (typeof module !== 'undefined' && module.exports) {
    module.exports = TokenPlacementManager;
}
