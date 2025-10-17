class LabelManager {
    constructor(map) {
        this.map = map;
        this.layer = window.labelGroup || new L.FeatureGroup().addTo(this.map);
        this.labelsById = new Map();
        this.iconChoices = [
            { id: 'none', html: '' },
            { id: 'mountain', html: '⛰️' },
            { id: 'peak', html: '🗻' },
            { id: 'camp', html: '🏕️' },
            { id: 'view', html: '📍' },
            { id: 'flag', html: '🚩' },
            { id: 'custom', html: '🔖' }
        ];
        this.defaultStyle = {
            color: '#111',
            bg: 'rgba(255,255,255,0.85)',
            border: '1px solid rgba(0,0,0,0.25)',
            fontSize: 13,
            fontWeight: '600'
        };
        this._placementActive = false;
        this._placementHandler = null;
    }

    async initialize() {
        await this.loadLabels();
        // Quick-add with Shift+L: adds label at map center
        window.addEventListener('keydown', (e) => {
            if ((e.key === 'l' || e.key === 'L') && e.shiftKey) {
                const center = this.map.getCenter();
                this.openAddLabelModal(center);
            }
        });
        // Right-click add label disabled (labels are added via button flow only)
        // Expose helpers
        try { window.labelManager = this; } catch (_) {}
    }

    // Enter placement mode: user clicks map to pick location, then modal opens
    startPlacement() {
        if (this._placementActive) {
            this.stopPlacement();
        }
        this._placementActive = true;
        this.map.getContainer().style.cursor = 'crosshair';
        const once = (e) => {
            if (!this._placementActive) return;
            this.stopPlacement();
            this.openAddLabelModal(e.latlng);
        };
        this._placementHandler = once;
        this.map.once('click', this._placementHandler);
    }

    stopPlacement() {
        this._placementActive = false;
        this.map.getContainer().style.cursor = '';
        if (this._placementHandler) {
            // 'once' listener auto-removed on fire; just clear reference
            this._placementHandler = null;
        }
    }

    async loadLabels() {
        try {
            const res = await fetch('/Map/GetLabels');
            const json = await res.json();
            if (!json.success) return;
            for (const l of json.data) {
                this.renderLabel(l);
            }
        } catch (e) {
            console.warn('Failed to load labels', e);
        }
    }

    renderLabel(l) {
        const iconHtml = this.getIconHtml(l.icon) + this.getTextHtml(l.text, l);
        const div = L.divIcon({
            className: 'custom-map-label',
            html: `<div class="label-pill" style="${this.inlineStyle(l)}">${iconHtml}</div>`,
            iconSize: null
        });
        const marker = L.marker([parseFloat(l.latitude), parseFloat(l.longitude)], { icon: div, zIndexOffset: 1000 });
        marker.addTo(this.layer);
        marker.bindTooltip(l.description || l.text || '', { direction: 'top', opacity: 0.9 });
        marker.on('dblclick', () => this.confirmDelete(l.id, marker));
        this.labelsById.set(l.id, { data: l, marker });
    }

    inlineStyle(l) {
        // Transparent container; text carries the color and halo for readability
        const fontSize = (l.fontSize || this.defaultStyle.fontSize) + 'px';
        const fontWeight = l.fontWeight || this.defaultStyle.fontWeight;
        return `display:inline-flex;align-items:center;gap:6px;padding:2px 4px;background:transparent;border:none;border-radius:4px;font-size:${fontSize};font-weight:${fontWeight};`;
    }

    getIconHtml(iconId) {
        if (!iconId || iconId === 'none') return '';
        const found = this.iconChoices.find(i => i.id === iconId);
        const html = (found ? found.html : '');
        if (!html) return '';
        return `<span class="label-icon" style="font-size:16px;line-height:1;">${html}</span>`;
    }

    getTextHtml(text, l) {
        const color = l.color || this.defaultStyle.color;
        const size = (l.fontSize || this.defaultStyle.fontSize) + 'px';
        const weight = l.fontWeight || this.defaultStyle.fontWeight;
        // Halo/shadow to stand out over imagery (no background)
        const shadow = '0 0 3px rgba(0,0,0,0.9), 0 0 6px rgba(0,0,0,0.7), 0 1px 2px rgba(0,0,0,0.9)';
        return `<span class="label-text" style="color:${color} !important; font-size:${size}; font-weight:${weight}; text-shadow:${shadow};">${this.escape(text || '')}</span>`;
    }

    openAddLabelModal(latlng) {
        const modalId = 'labelAddModal';
        let modal = document.getElementById(modalId);
        if (!modal) {
            modal = document.createElement('div');
            modal.id = modalId;
            modal.className = 'gameplay-modal';
            modal.innerHTML = this.buildModalHtml();
            document.body.appendChild(modal);
            this.attachModalHandlers();
        }
        modal.style.display = 'flex';
        modal.querySelector('#labelLat').value = latlng.lat.toFixed(6);
        modal.querySelector('#labelLng').value = latlng.lng.toFixed(6);
    }

    buildModalHtml() {
        const icons = this.iconChoices.map(i => `<option value="${i.id}">${i.id}</option>`).join('');
        return `
<div class="gameplay-modal-content" style="max-width:420px;">
  <div class="gameplay-modal-header">
    <h3><i class="fas fa-tag"></i> Add Map Label</h3>
    <button class="gameplay-modal-close" id="labelCloseBtn">&times;</button>
  </div>
  <div class="gameplay-modal-body">
    <div class="form-group"><label>Title</label><input id="labelText" class="form-control" placeholder="e.g., Snæfellsjökull"/></div>
    <div class="form-group"><label>Icon (optional)</label><select id="labelIcon" class="form-control">${icons}</select></div>
    <div class="form-group"><label>Color</label>
      <div style="display:flex;gap:8px;align-items:center;">
        <input id="labelColor" type="color" value="#ff4444" style="width:44px;height:34px;padding:0;border:none;background:transparent;">
        <input id="labelColorHex" class="form-control" style="flex:1;" value="#ff4444" placeholder="#RRGGBB" maxlength="9" />
      </div>
    </div>
    <div class="form-row" style="display:flex; gap:8px;">
      <div class="form-group" style="flex:1;"><label>Lat</label><input id="labelLat" class="form-control"/></div>
      <div class="form-group" style="flex:1;"><label>Lng</label><input id="labelLng" class="form-control"/></div>
    </div>
    <div class="form-group"><label>Description</label><textarea id="labelDesc" class="form-control" rows="2" placeholder="Optional details"></textarea></div>
    <div class="text-muted" style="font-size:12px;">Tip: Shift+L to add at map center. Double‑click a label to delete.</div>
  </div>
  <div class="gameplay-modal-footer">
    <button class="gameplay-btn" id="labelCancelBtn">Cancel</button>
    <button class="gameplay-btn gameplay-btn-primary" id="labelSaveBtn">Save</button>
  </div>
</div>`;
    }

    attachModalHandlers() {
        const close = () => { const m = document.getElementById('labelAddModal'); if (m) m.style.display = 'none'; };
        document.getElementById('labelCloseBtn').onclick = close;
        document.getElementById('labelCancelBtn').onclick = close;
        // Sync color picker and hex input both ways
        const colorInput = document.getElementById('labelColor');
        const colorHex = document.getElementById('labelColorHex');
        if (colorInput && colorHex) {
            colorInput.addEventListener('input', () => { colorHex.value = colorInput.value; });
            colorHex.addEventListener('input', () => {
                let v = colorHex.value.trim();
                if (!v.startsWith('#')) v = '#' + v;
                if (/^#([0-9A-Fa-f]{3}|[0-9A-Fa-f]{6})$/.test(v)) { colorInput.value = v; }
            });
        }

        document.getElementById('labelSaveBtn').onclick = async () => {
            const text = document.getElementById('labelText').value.trim();
            const icon = document.getElementById('labelIcon').value;
            const color = this.getSelectedColor();
            const lat = parseFloat(document.getElementById('labelLat').value);
            const lng = parseFloat(document.getElementById('labelLng').value);
            const desc = document.getElementById('labelDesc').value.trim();
            if (!text || !Number.isFinite(lat) || !Number.isFinite(lng)) { alert('Please provide title and valid coordinates.'); return; }
            await this.saveLabel({ text, icon, color, lat, lng, desc });
            close();
        };
    }

    getSelectedColor() {
        const colorInput = document.getElementById('labelColor');
        const colorHex = document.getElementById('labelColorHex');
        let v = (colorHex && colorHex.value) ? colorHex.value.trim() : (colorInput ? colorInput.value : '#ff4444');
        if (!v) v = '#ff4444';
        if (!v.startsWith('#')) v = '#' + v;
        return v;
    }

    async saveLabel({ text, icon, color, lat, lng, desc }) {
        try {
            const payload = {
                Text: text,
                Latitude: lat,
                Longitude: lng,
                LabelType: 'point',
                Color: color,
                Icon: icon,
                FontSize: 13,
                FontWeight: '600',
                Properties: null,
                Description: desc
            };
            const res = await fetch('/Map/SaveLabel', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload)
            });
            const json = await res.json();
            if (json && json.success) {
                // render new label
                this.renderLabel({
                    id: (json.data && json.data.id) || crypto.randomUUID(),
                    text, latitude: lat, longitude: lng, labelType: 'point', color, icon, fontSize: 13, fontWeight: '600', description: desc
                });
            } else {
                alert('Failed to save label');
            }
        } catch (e) {
            console.error('Save label failed', e);
            alert('Error saving label');
        }
    }

    async confirmDelete(id, marker) {
        if (!confirm('Delete this label?')) return;
        try {
            const res = await fetch(`/Map/DeleteLabel/${id}`, { method: 'DELETE' });
            const json = await res.json();
            if (json && json.success) {
                this.layer.removeLayer(marker);
                this.labelsById.delete(id);
            } else {
                alert('Failed to delete label');
            }
        } catch (e) {
            console.error('Delete failed', e);
            alert('Error deleting label');
        }
    }

    escape(s) {
        return (s || '').replace(/[&<>"]/g, c => ({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;'}[c]));
    }
}

/**
 * LegacyLabelManager - legacy localStorage-based implementation (kept for reference)
 * Not used by the current UI; renamed to avoid class name conflicts.
 */
class LegacyLabelManager {
    constructor(map, notificationCallback) {
        this.map = map;
        this.notificationCallback = notificationCallback || this.defaultNotification;
        this.labels = new Map(); // labelId -> { marker, data }
        this.isLabelMode = false;
        this.labelLayer = null;
        
        this.setupLabelLayer();
        this.setupEventListeners();
    }

    /**
     * Setup label layer for all labels
     */
    setupLabelLayer() {
        this.labelLayer = L.layerGroup().addTo(this.map);
    }

    /**
     * Setup event listeners
     */
    setupEventListeners() {
        // Map click events for label placement
        this.map.on('click', (e) => {
            if (this.isLabelMode) {
                this.placeLabelAt(e.latlng);
            }
        });
    }

    /**
     * Start label placement mode
     */
    startLabelMode() {
        if (this.isLabelMode) {
            this.cancelLabelMode();
        }

        this.isLabelMode = true;
        
        // Change cursor
        this.map.getContainer().style.cursor = 'crosshair';
        
        this.notificationCallback('Click on the map to place a label', 'info');
    }

    /**
     * Place label at specified location
     */
    placeLabelAt(latlng) {
        // Show label creation modal
        this.showLabelCreationModal(latlng);
    }

    /**
     * Show label creation modal
     */
    showLabelCreationModal(latlng) {
        const existingModal = document.getElementById('labelCreationModal');
        if (existingModal) {
            existingModal.remove();
        }

        const modal = document.createElement('div');
        modal.className = 'gameplay-modal';
        modal.id = 'labelCreationModal';
        modal.innerHTML = `
            <div class="gameplay-modal-content">
                <div class="gameplay-modal-header">
                    <h3><i class="fas fa-tag"></i> Create Map Label</h3>
                    <button class="gameplay-modal-close" onclick="this.closest('.gameplay-modal').remove()">&times;</button>
                </div>
                <div class="gameplay-modal-body">
                    <div class="data-entry-container">
                        <div class="data-entry-form-section">
                            <!-- Label Header -->
                            <div class="brigade-header-section">
                                <div class="brigade-name-display">
                                    <h5><i class="fas fa-tag"></i> Label Information</h5>
                                </div>
                            </div>
                            
                            <!-- Label Form -->
                            <div class="brigade-data-form">
                                <div class="data-tab-content active">
                                    <div class="tab-content-header">
                                        <h6><i class="fas fa-info-circle"></i> Label Details</h6>
                                        <p class="text-muted">Define the label text and properties</p>
                                    </div>
                                    
                                    <!-- Label Text -->
                                    <div class="form-group">
                                        <label for="labelText" style="color: #ccc; font-size: 12px; margin-bottom: 5px;">Label Text *</label>
                                        <input type="text" class="form-control" id="labelText" placeholder="Enter label text" required>
                                    </div>
                                    
                                    <!-- Label Type -->
                                    <div class="form-group">
                                        <label for="labelType" style="color: #ccc; font-size: 12px; margin-bottom: 5px;">Label Type</label>
                                        <select class="form-control" id="labelType">
                                            <option value="location">Location</option>
                                            <option value="landmark">Landmark</option>
                                            <option value="objective">Objective</option>
                                            <option value="waypoint">Waypoint</option>
                                            <option value="note">Note</option>
                                        </select>
                                    </div>
                                    
                                    <!-- Font Size -->
                                    <div class="form-group">
                                        <label for="labelFontSize" style="color: #ccc; font-size: 12px; margin-bottom: 5px;">Font Size</label>
                                        <select class="form-control" id="labelFontSize">
                                            <option value="12">Small (12px)</option>
                                            <option value="14" selected>Medium (14px)</option>
                                            <option value="16">Large (16px)</option>
                                            <option value="18">Extra Large (18px)</option>
                                        </select>
                                    </div>
                                    
                                    <!-- Font Weight -->
                                    <div class="form-group">
                                        <label for="labelFontWeight" style="color: #ccc; font-size: 12px; margin-bottom: 5px;">Font Weight</label>
                                        <select class="form-control" id="labelFontWeight">
                                            <option value="normal">Normal</option>
                                            <option value="bold" selected>Bold</option>
                                        </select>
                                    </div>
                                    
                                    <!-- Label Color -->
                                    <div class="form-group">
                                        <label for="labelColor" style="color: #ccc; font-size: 12px; margin-bottom: 5px;">Label Color</label>
                                        <select class="form-control" id="labelColor">
                                            <option value="#ffffff">White</option>
                                            <option value="#00d4ff" selected>Blue</option>
                                            <option value="#ff6b6b">Red</option>
                                            <option value="#51cf66">Green</option>
                                            <option value="#ffd43b">Yellow</option>
                                            <option value="#000000">Black</option>
                                        </select>
                                    </div>
                                    
                                    <!-- Description -->
                                    <div class="form-group">
                                        <label for="labelDescription" style="color: #ccc; font-size: 12px; margin-bottom: 5px;">Description</label>
                                        <textarea class="form-control" id="labelDescription" rows="2" placeholder="Label description..."></textarea>
                                    </div>
                                </div>
                            </div>
                            
                            <!-- Action Buttons -->
                            <div class="add-unit-section">
                                <button type="button" class="btn btn-outline-secondary" onclick="this.closest('.gameplay-modal').remove()" style="margin-right: 10px;">
                                    <i class="fas fa-times"></i> Cancel
                                </button>
                                <button type="button" class="btn btn-primary" onclick="window.labelManager.saveLabel()">
                                    <i class="fas fa-save"></i> Create Label
                                </button>
                            </div>
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
                max-width: 500px;
                width: 90%;
            }

            /* Dark Theme Data Entry Form */
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

            .brigade-name-display h5 {
                color: #00d4ff;
                margin: 0;
                font-weight: 600;
            }

            .data-tab-content {
                display: block;
                padding: 20px;
                border: 1px solid #333;
                border-radius: 8px;
                background: #2a2a2a;
                margin-top: 10px;
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
                color: #ccc;
                margin-bottom: 5px;
                font-size: 12px;
            }
            </style>
        `;
        
        document.body.appendChild(modal);
        window.labelManager = this;
        window.labelManager.currentLatLng = latlng;
    }

    /**
     * Save the label to localStorage
     */
    saveLabel() {
        const text = document.getElementById('labelText').value.trim();
        const type = document.getElementById('labelType').value;
        const fontSize = parseInt(document.getElementById('labelFontSize').value);
        const fontWeight = document.getElementById('labelFontWeight').value;
        const color = document.getElementById('labelColor').value;
        const description = document.getElementById('labelDescription').value.trim();

        if (!text) {
            this.notificationCallback('Please enter label text', 'error');
            return;
        }

        try {
            // Generate unique ID
            const labelId = 'label_' + Date.now() + '_' + Math.random().toString(36).substr(2, 9);

            // Create label data
            const labelData = {
                id: labelId,
                text: text,
                latitude: this.currentLatLng.lat,
                longitude: this.currentLatLng.lng,
                labelType: type,
                color: color,
                fontSize: fontSize,
                fontWeight: fontWeight,
                description: description,
                createdDate: new Date().toISOString()
            };

            // Save to localStorage
            this.saveLabelToLocalStorage(labelData);

            // Create label marker on map
            const marker = L.marker(this.currentLatLng, {
                icon: L.divIcon({
                    className: 'map-label',
                    html: `<div style="
                        background: rgba(0,0,0,0.8);
                        color: ${color};
                        padding: 4px 8px;
                        border-radius: 4px;
                        font-size: ${fontSize}px;
                        font-weight: ${fontWeight};
                        text-align: center;
                        border: 1px solid ${color};
                        text-shadow: 1px 1px 2px rgba(0,0,0,0.8);
                        white-space: nowrap;
                        max-width: 200px;
                        overflow: hidden;
                        text-overflow: ellipsis;
                    ">${text}</div>`,
                    iconSize: [text.length * (fontSize * 0.6), fontSize + 8],
                    iconAnchor: [text.length * (fontSize * 0.3), (fontSize + 8) / 2]
                })
            }).addTo(this.labelLayer);

            // Store label data
            this.labels.set(labelId, {
                marker: marker,
                data: labelData
            });

            // Clean up and reset for next label
            document.getElementById('labelCreationModal').remove();
            
            this.notificationCallback(`Label "${text}" created successfully. You can now create another label.`, 'success');
            
            // Reset label mode to allow creating another label
            this.isLabelMode = false;
            this.map.getContainer().style.cursor = '';
            
            // Update label count in UI
            this.updateLabelCount();
            
        } catch (error) {
            console.error('Error saving label:', error);
            this.notificationCallback('Error creating label', 'error');
        }
    }

    /**
     * Cancel label mode
     */
    cancelLabelMode() {
        this.isLabelMode = false;
        this.map.getContainer().style.cursor = '';
        this.notificationCallback('Label placement cancelled', 'info');
    }

    /**
     * Save label to localStorage
     */
    saveLabelToLocalStorage(labelData) {
        try {
            // Get existing labels from localStorage
            const existingLabels = this.loadLabelsFromLocalStorage();
            
            // Add new label
            existingLabels.push(labelData);
            
            // Save back to localStorage
            localStorage.setItem('gamePlay_labels', JSON.stringify(existingLabels));
            console.log(`💾 Saved label "${labelData.text}" to localStorage`);
        } catch (error) {
            console.error('Error saving label to localStorage:', error);
        }
    }

    /**
     * Load existing labels from localStorage
     */
    loadLabelsFromLocalStorage() {
        try {
            const stored = localStorage.getItem('gamePlay_labels');
            if (stored) {
                return JSON.parse(stored);
            }
            return [];
        } catch (error) {
            console.error('Error loading labels from localStorage:', error);
            return [];
        }
    }

    /**
     * Load existing labels from localStorage
     */
    loadLabels() {
        try {
            const labels = this.loadLabelsFromLocalStorage();
            
            labels.forEach(labelData => {
                this.createLabelFromData(labelData);
            });
            
            console.log(`📂 Loaded ${labels.length} labels from localStorage`);
        } catch (error) {
            console.error('Error loading labels:', error);
        }
    }

    /**
     * Create label from stored data
     */
    createLabelFromData(labelData) {
        try {
            const latlng = L.latLng(labelData.latitude, labelData.longitude);
            
            // Create label marker
            const marker = L.marker(latlng, {
                icon: L.divIcon({
                    className: 'map-label',
                    html: `<div style="
                        background: rgba(0,0,0,0.8);
                        color: ${labelData.color};
                        padding: 4px 8px;
                        border-radius: 4px;
                        font-size: ${labelData.fontSize}px;
                        font-weight: ${labelData.fontWeight};
                        text-align: center;
                        border: 1px solid ${labelData.color};
                        text-shadow: 1px 1px 2px rgba(0,0,0,0.8);
                        white-space: nowrap;
                        max-width: 200px;
                        overflow: hidden;
                        text-overflow: ellipsis;
                    ">${labelData.text}</div>`,
                    iconSize: [labelData.text.length * (labelData.fontSize * 0.6), labelData.fontSize + 8],
                    iconAnchor: [labelData.text.length * (labelData.fontSize * 0.3), (labelData.fontSize + 8) / 2]
                })
            }).addTo(this.labelLayer);

            // Store label data
            this.labels.set(labelData.id, {
                marker: marker,
                data: labelData
            });
        } catch (error) {
            console.error('Error creating label from data:', error);
        }
    }

    /**
     * Delete a label from localStorage
     */
    deleteLabel(labelId) {
        try {
            // Remove from map
            const label = this.labels.get(labelId);
            if (label) {
                this.labelLayer.removeLayer(label.marker);
                this.labels.delete(labelId);
            }
            
            // Remove from localStorage
            const existingLabels = this.loadLabelsFromLocalStorage();
            const updatedLabels = existingLabels.filter(l => l.id !== labelId);
            localStorage.setItem('gamePlay_labels', JSON.stringify(updatedLabels));
            
            this.notificationCallback('Label deleted successfully', 'success');
        } catch (error) {
            console.error('Error deleting label:', error);
            this.notificationCallback('Error deleting label', 'error');
        }
    }

    /**
     * Clear all labels from localStorage
     */
    clearAllLabels() {
        if (this.labels.size === 0) {
            this.notificationCallback('No labels to clear', 'info');
            return;
        }

        if (confirm(`Are you sure you want to delete all ${this.labels.size} labels?`)) {
            try {
                // Remove all labels from map
                for (const [labelId, label] of this.labels) {
                    this.labelLayer.removeLayer(label.marker);
                }
                
                // Clear labels map
                this.labels.clear();
                
                // Clear localStorage
                localStorage.removeItem('gamePlay_labels');
                
                this.notificationCallback('All labels cleared successfully', 'success');
            } catch (error) {
                console.error('Error clearing labels:', error);
                this.notificationCallback('Error clearing labels', 'error');
            }
        }
    }

    /**
     * Toggle labels layer visibility
     */
    toggleVisibility() {
        this.isVisible = !this.isVisible;
        
        if (this.isVisible) {
            this.map.addLayer(this.labelLayer);
        } else {
            this.map.removeLayer(this.labelLayer);
        }
        
        const checkbox = document.getElementById('chkShowLabels');
        if (checkbox) {
            checkbox.checked = this.isVisible;
        }
        
        console.log(`Labels layer ${this.isVisible ? 'shown' : 'hidden'}`);
    }

    /**
     * Update label count in UI
     */
    updateLabelCount() {
        // Update any label count display if needed
        console.log(`Total labels: ${this.labels.size}`);
    }

    /**
     * Default notification callback
     */
    defaultNotification(message, type) {
        console.log(`[${type.toUpperCase()}] ${message}`);
    }

    /**
     * Clean up resources
     */
    destroy() {
        this.cancelLabelMode();
        
        // Remove all labels
        for (const [labelId, label] of this.labels) {
            this.labelLayer.removeLayer(label.marker);
        }
        
        this.labels.clear();
        
        if (this.labelLayer) {
            this.map.removeLayer(this.labelLayer);
        }
    }
}

// Export for use in other modules
if (typeof module !== 'undefined' && module.exports) {
    module.exports = LabelManager;
}
