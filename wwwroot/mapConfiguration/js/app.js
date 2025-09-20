// ===== Shared local storage helpers =====
const STORE_KEY = 'leaflet_saved_regions_v1';
function ensureStore() { if (!localStorage.getItem(STORE_KEY)) { localStorage.setItem(STORE_KEY, JSON.stringify({ type: 'FeatureCollection', features: [] })); } }
function readStore() { ensureStore(); try { return JSON.parse(localStorage.getItem(STORE_KEY)); } catch { return { type: 'FeatureCollection', features: [] }; } }
function writeStore(fc) { localStorage.setItem(STORE_KEY, JSON.stringify(fc)); renderSavedList(); syncSavedToMap(); syncSavedToMapTiler(); }

// ===== View toggle =====
const btn2d = document.getElementById('btn2d');
const btn3d = document.getElementById('btn3d');
const btn4d = document.getElementById('btn4d'); // ✅ add this
const stage2d = document.getElementById('stage2d');
const stage3d = document.getElementById('stage3d');
const stage4d = document.getElementById('stage4d'); // ✅ separate var

function setMode(mode) {
    if (mode === '2d') {
        stage2d.classList.remove('hide');
        stage3d.classList.add('hide');
        stage4d.classList.add('hide');
        btn2d.classList.add('active');
        btn3d.classList.remove('active');
        btn4d.classList.remove('active');
        setTimeout(() => map.invalidateSize(), 50);
    }
    else if (mode === '3d') {
        stage3d.classList.remove('hide');
        stage2d.classList.add('hide');
        stage4d.classList.add('hide');
        btn3d.classList.add('active');
        btn2d.classList.remove('active');
        btn4d.classList.remove('active');
        setTimeout(() => {
            if (!mtMap) initMapTiler(true);
            else mtMap.resize && mtMap.resize();
        }, 0);
    }
    else if (mode === '4d') {
        stage4d.classList.remove('hide');
        stage2d.classList.add('hide');
        stage3d.classList.add('hide');
        btn4d.classList.add('active');
        btn2d.classList.remove('active');
        btn3d.classList.remove('active');
        // 👉 Add your 4D initialization logic here
        initCesium

        setTimeout(() => {
            if (!mtMap) initCesium(true);
            else mtMap.resize && mtMap.resize();
        }, 0);
        console.log("4D mode activated!");
    }
}

btn2d.onclick = () => setMode('2d');
btn3d.onclick = () => setMode('3d');
btn4d.onclick = () => setMode('4d');

// ===== 2D (Leaflet) setup =====
const map = L.map('map').setView([25.2854, 51.5310], 8);
const osm = L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', { attribution: '&copy; OSM' }).addTo(map);
const topo = L.tileLayer('https://{s}.tile.opentopomap.org/{z}/{x}/{y}.png', { attribution: '&copy; OpenTopoMap' });
document.getElementById('chkTopo').addEventListener('change', e => { if (e.target.checked) topo.addTo(map); else map.removeLayer(topo); });

let owmTileLayer = null; const weatherSelect = document.getElementById('weatherLayer'); const weatherOpacity = document.getElementById('weatherOpacity'); const owmKeyInput = document.getElementById('owmKey');
const DEFAULT_OWM_KEY = 'bd5e378503939ddaee76f12ad7a97608';
const storedKey = localStorage.getItem('owm_api_key');
owmKeyInput.value = storedKey || DEFAULT_OWM_KEY;
owmKeyInput.addEventListener('change', () => localStorage.setItem('owm_api_key', owmKeyInput.value.trim()));
function refreshWeatherLayer() {
    const layer = weatherSelect.value; const key = owmKeyInput.value.trim();
    if (owmTileLayer) { map.removeLayer(owmTileLayer); owmTileLayer = null; }
    if (layer && key) { owmTileLayer = L.tileLayer(`https://tile.openweathermap.org/map/${layer}/{z}/{x}/{y}.png?appid=${key}`, { opacity: parseFloat(weatherOpacity.value) }); owmTileLayer.addTo(map); }
    refreshMapTilerWeather();
}
weatherSelect.addEventListener('change', refreshWeatherLayer);
weatherOpacity.addEventListener('input', () => { if (owmTileLayer) owmTileLayer.setOpacity(parseFloat(weatherOpacity.value)); if (mtMap && mtMap.getLayer('owm-layer')) mtMap.setPaintProperty('owm-layer', 'raster-opacity', parseFloat(weatherOpacity.value)); });

const drawnItems = new L.FeatureGroup(); map.addLayer(drawnItems);
const obstaclesLayer = new L.FeatureGroup().addTo(map); const safeLayer = new L.FeatureGroup().addTo(map);
const drawControl = new L.Control.Draw({ draw: { polygon: true, rectangle: true, polyline: false, circle: false, marker: false, circlemarker: false }, edit: false });
map.addControl(drawControl);

let currentFeature = null;
function setEditableOnly(layer) { drawnItems.eachLayer(l => { if (l.editing && l.editing.enabled()) l.editing.disable(); }); if (layer.editing && !layer.editing.enabled()) layer.editing.enable(); }
function setCurrentFeature(layer) { currentFeature = layer; drawnItems.eachLayer(l => l.setStyle && l.setStyle({ color: '#16a34a', weight: 2 })); layer.setStyle && layer.setStyle({ color: '#dc2626', weight: 3 }); setEditableOnly(layer); if (layer.bringToFront) layer.bringToFront(); }

const chkObstacleMode = document.getElementById('chkObstacleMode');
map.on(L.Draw.Event.CREATED, e => { const layer = e.layer; if (chkObstacleMode.checked) { obstaclesLayer.addLayer(layer); layer.setStyle && layer.setStyle({ color: '#7f1d1d', weight: 2 }); if (layer.editing && layer.editing.enabled()) layer.editing.disable(); attachObstaclePopup(layer); syncObstaclesToMapTiler(); } else { drawnItems.addLayer(layer); setCurrentFeature(layer); } });
drawnItems.on('click', e => setCurrentFeature(e.layer));

// Saved features (blue) + click = make editable copy (red)
const savedLayer = L.geoJSON({ type: 'FeatureCollection', features: [] }, {
    style: { color: '#2563eb', weight: 2 }, onEachFeature: (feature, layer) => {
        layer.on('click', () => { highlightSaved(feature.properties.id); selectSavedFeature(feature.properties.id); flyMapTilerToFeature(feature); });
        if (feature && feature.properties && feature.properties.name) layer.bindTooltip(feature.properties.name);
    }
}).addTo(map);

function syncSavedToMap() { const fc = readStore(); savedLayer.clearLayers(); savedLayer.addData(fc); document.getElementById('savedCount').textContent = fc.features.length; }

function highlightSaved(id) { savedLayer.eachLayer(layer => { const isMatch = layer.feature && layer.feature.properties && layer.feature.properties.id === id; layer.setStyle && layer.setStyle({ color: isMatch ? '#f59e0b' : '#2563eb', weight: isMatch ? 4 : 2 }); }); }

// Create an editable copy of a saved feature into drawnItems and select it
function selectSavedFeature(id) {
    drawnItems.clearLayers();
    const fc = readStore();
    const f = (fc.features || []).find(x => x.properties && x.properties.id === id);
    if (!f) return;
    let editableLayer = null;
    L.geoJSON({ type: 'Feature', geometry: f.geometry, properties: f.properties || {} }, { style: { color: '#dc2626', weight: 3 } }).eachLayer(l => { editableLayer = l; drawnItems.addLayer(l); });
    if (editableLayer) { setCurrentFeature(editableLayer); map.fitBounds(editableLayer.getBounds(), { maxZoom: 14, padding: [20, 20] }); }
}

document.getElementById('btnSaveCurrent').addEventListener('click', () => {
    if (!currentFeature) { alert('Draw or select a region first.'); return; }
    const gj = currentFeature.toGeoJSON();
    const feature = turf.feature(gj.geometry);
    const fc = readStore();
    const idx = fc.features.length + 1;
    const id = `region_${Date.now()}`;
    const enriched = { type: 'Feature', geometry: gj.geometry, properties: { id, name: `Region ${idx}`, area_m2: turf.area(feature), centroid: turf.centroid(feature).geometry.coordinates, bbox: turf.bbox(feature) } };
    fc.features.push(enriched);
    writeStore(fc);
    highlightSaved(id);
    selectSavedFeature(id);
});
document.getElementById('btnDownloadAll').addEventListener('click', () => { const fc = readStore(); const blob = new Blob([JSON.stringify(fc, null, 2)], { type: 'application/json' }); const url = URL.createObjectURL(blob); const a = document.createElement('a'); a.href = url; a.download = 'saved-regions.json'; a.click(); URL.revokeObjectURL(url); });

document.getElementById('fileInputMaps').addEventListener('change', async e => { const file = e.target.files[0]; if (!file) return; try { const text = await file.text(); const fc = JSON.parse(text); fc.features = (fc.features || []).map((f, i) => ({ type: 'Feature', geometry: f.geometry, properties: { id: f.properties && f.properties.id ? f.properties.id : `region_${Date.now()}_${i}`, name: f.properties && f.properties.name ? f.properties.name : `Imported ${i + 1}`, area_m2: f.properties && f.properties.area_m2 != null ? f.properties.area_m2 : turf.area(f), centroid: f.properties && f.properties.centroid ? f.properties.centroid : turf.centroid(f).geometry.coordinates, bbox: f.properties && f.properties.bbox ? f.properties.bbox : turf.bbox(f) } })); writeStore(fc); if ((fc.features || []).length) { selectSavedFeature(fc.features[0].properties.id); } } catch { alert('Invalid JSON for maps.'); } finally { e.target.value = ''; } });
document.getElementById('fileInputObstacles').addEventListener('change', async e => { const file = e.target.files[0]; if (!file) return; try { const text = await file.text(); const fc = JSON.parse(text); obstaclesLayer.clearLayers(); L.geoJSON(fc, { style: { color: '#7f1d1d', weight: 2 }, onEachFeature: (_, ly) => attachObstaclePopup(ly) }).eachLayer(l => obstaclesLayer.addLayer(l)); syncObstaclesToMapTiler(); } catch { alert('Invalid JSON for obstacles.'); } finally { e.target.value = ''; } });

const savedList = document.getElementById('savedList'); const searchInput = document.getElementById('searchInput'); searchInput.addEventListener('input', renderSavedList);
function renderSavedList() { const q = (searchInput.value || '').toLowerCase(); const fc = readStore(); savedList.innerHTML = ''; fc.features.filter(f => !q || (f.properties.name || '').toLowerCase().includes(q)).forEach(f => { const li = document.createElement('li'); li.dataset.id = f.properties.id; li.innerHTML = `<span>${f.properties.name} <span class="tag">${(f.properties.area_m2 / 1e6).toFixed(3)} km²</span></span><span><button class="btn" data-action="focus">Focus</button><button class="btn" data-action="delete">Delete</button></span>`; li.addEventListener('click', ev => { const action = ev.target && ev.target.getAttribute('data-action'); if (action === 'focus') { highlightSaved(f.properties.id); selectSavedFeature(f.properties.id); flyMapTilerToFeature(f); } else if (action === 'delete') { const updated = readStore(); updated.features = updated.features.filter(x => x.properties.id !== f.properties.id); writeStore(updated); } }); savedList.appendChild(li); }); }

document.getElementById('btnComputeSafe').addEventListener('click', () => {
    if (!currentFeature) { alert('Select a region first.'); return; }
    const meters = parseFloat(document.getElementById('bufferMeters').value) || 0;
    let unionBuffer = null;
    obstaclesLayer.eachLayer(l => { const gj = l.toGeoJSON(); const buf = turf.buffer(gj, meters / 1000, { units: 'kilometers' }); unionBuffer = unionBuffer ? turf.union(unionBuffer, buf) : buf; });
    safeLayer.clearLayers();
    if (!unionBuffer) { safeLayer.addLayer(L.geoJSON(currentFeature.toGeoJSON(), { style: { color: '#16a34a', weight: 3, dashArray: '4 2' } })); syncSafeToMapTiler(); alert('No obstacles found. Safe area equals the selected region.'); return; }
    const region = currentFeature.toGeoJSON();
    try { const diff = turf.difference(region, unionBuffer); if (!diff) { alert('Buffered obstacles fully cover the region. No safe area.'); syncSafeToMapTiler(); return; } L.geoJSON(diff, { style: { color: '#16a34a', weight: 3, dashArray: '4 2' } }).addTo(safeLayer); let bounds = null; safeLayer.eachLayer(l => bounds = bounds ? bounds.extend(l.getBounds()) : l.getBounds()); if (bounds) map.fitBounds(bounds, { maxZoom: 14, padding: [20, 20] }); syncSafeToMapTiler(); } catch (err) { alert('Unable to compute difference.'); }
});
document.getElementById('btnClearObstacles').addEventListener('click', () => { obstaclesLayer.clearLayers(); safeLayer.clearLayers(); syncObstaclesToMapTiler(); syncSafeToMapTiler(); });

// ===== Helpers =====
function attachObstaclePopup(layer) { try { const gj = layer.toGeoJSON(); const p = (gj.properties || {}); const t = (p.tags || p); const kind = t.man_made || t.barrier || t.natural || t.amenity || 'obstacle'; const name = t.name ? ` • ${t.name}` : ''; const h = t.height || t['height:meters'] || (t['building:levels'] ? `${t['building:levels']} levels` : ''); const hTxt = h ? ` • h=${h}` : ''; const html = `<b>${kind}</b>${name}${hTxt}`; if (layer.bindPopup) layer.bindPopup(html); if (layer.bindTooltip) layer.bindTooltip(kind); } catch (_) { } }

// ===== 3D via MapTiler SDK =====
const maptilerKeyInput = document.getElementById('maptilerKey');
const styleIdInput = document.getElementById('styleId');
const PROVIDED_MAPTILER_KEY = 'Qtj3cVK3NsJ8PZv6ENko';
maptilerKeyInput.value = PROVIDED_MAPTILER_KEY;
localStorage.setItem('maptiler_key', PROVIDED_MAPTILER_KEY);

let mtMap = null;  // MapTiler map instance
let MsMap = null;  // MapTiler map instance

function resolveStyle(val) { const v = (val || '').trim().toUpperCase(); const S = maptilersdk.MapStyle; if (v === 'STREETS') return S.STREETS; if (v === 'SATELLITE') return S.SATELLITE; if (v === 'OUTDOOR') return S.OUTDOOR; if (v === 'DATAVIZ') return S.DATAVIZ; if ((val || '').startsWith('http')) return val; return S.SATELLITE; }

function initMapTiler(force) {
    const key = (maptilerKeyInput.value || '').trim(); if (!key) { alert('Enter your MapTiler key first.'); return; }
    maptilersdk.config.apiKey = key;
    const style = resolveStyle(styleIdInput.value);
    if (mtMap && !force) return; if (mtMap && force) { try { mtMap.remove(); } catch (_) { } mtMap = null; }

    mtMap = new maptilersdk.Map({ container: 'mtContainer', style, center: [51.5310, 25.2854], zoom: 8, pitch: 60, bearing: 0, antialias: true });
    mtMap.addControl(new maptilersdk.NavigationControl({ visualizePitch: true }));
    mtMap.addControl(new maptilersdk.ScaleControl());

    mtMap.on('load', () => {
        try { mtMap.addSource('terrain-dem', { type: 'raster-dem', url: `https://api.maptiler.com/tiles/terrain-rgb-v2/tiles.json?key=${key}`, tileSize: 256 }); mtMap.setTerrain({ source: 'terrain-dem', exaggeration: 1.4 }); } catch (e) { console.warn('Terrain DEM add failed:', e); }
        try { const layers = mtMap.getStyle().layers || []; const buildingLayer = layers.find(l => (l.id || '').includes('building')); const labelLayerId = (layers.find(l => l.type === 'symbol' && l.layout && l.layout['text-field']) || {}).id; if (buildingLayer) { mtMap.addLayer({ id: '3d-buildings', type: 'fill-extrusion', source: buildingLayer.source, 'source-layer': buildingLayer['source-layer'] || 'building', filter: ['==', ['geometry-type'], 'Polygon'], paint: { 'fill-extrusion-color': '#d1d5db', 'fill-extrusion-height': ['coalesce', ['get', 'render_height'], ['get', 'height'], 10], 'fill-extrusion-base': ['coalesce', ['get', 'render_min_height'], ['get', 'min_height'], 0], 'fill-extrusion-opacity': 0.6 } }, labelLayerId); } } catch (e) { console.warn('3D buildings add failed:', e); }
        syncSavedToMapTiler(); syncObstaclesToMapTiler(); syncSafeToMapTiler(); refreshMapTilerWeather();
    });

    window.addEventListener('resize', () => mtMap && mtMap.resize && mtMap.resize());
}

MsMap  || null;
function initCesium(force) {
    debugger
    const key = (maptilerKeyInput.value || '').trim(); if (!key) { alert('Enter your MapTiler key first.'); return; }
    maptilersdk.config.apiKey = key;
    const style = resolveStyle(styleIdInput.value);
    if (mtMap && !force) return; if (mtMap && force) { try { mtMap.remove(); } catch (_) { } mtMap = null; }
    MsMap = new Cesium.Viewer('cesiumContainer', {
        animation: false,
        baseLayerPicker: false,
        navigationHelpButton: false,
        sceneModePicker: false,
        homeButton: false,
        geocoder: false,
        fullscreenButton: false,
        timeline: false,
        baseLayer: new Cesium.ImageryLayer(new Cesium.UrlTemplateImageryProvider({
            url: `https://api.maptiler.com/tiles/satellite-v2/{z}/{x}/{y}.jpg?key=${key}`,
            minimumLevel: 0,
            maximumLevel: 20,
            tileWidth: 512,
            tileHeight: 512,
            credit: new Cesium.Credit("<a href='https://www.maptiler.com/copyright/' target='_blank'>&copy; MapTiler</a> <a href='https://www.openstreetmap.org/copyright' target='_blank'>&copy; OpenStreetMap contributors</a>", true)
        })),
        terrain: new Cesium.Terrain(Cesium.CesiumTerrainProvider.fromUrl(
            `https://api.maptiler.com/tiles/terrain-quantized-mesh-v2/?key=${key}`, {
            credit: new Cesium.Credit("<a href='https://www.maptiler.com/copyright/' target='_blank'>&copy; MapTiler</a> <a href='https://www.openstreetmap.org/copyright' target='_blank'>&copy; OpenStreetMap contributors</a>", true),
            requestVertexNormals: true
        }))
    });
    try { if (typeof syncSavedToCesium === 'function') syncSavedToCesium(MsMap); } catch (e) { console.warn('syncSavedToCesium failed:', e); }
    try { if (typeof syncObstaclesToCesium === 'function') syncObstaclesToCesium(MsMap); } catch (e) { console.warn('syncObstaclesToCesium failed:', e); }
    try { if (typeof syncSafeToCesium === 'function') syncSafeToCesium(MsMap); } catch (e) { console.warn('syncSafeToCesium failed:', e); }
    try { if (typeof refreshCesiumWeather === 'function') refreshCesiumWeather(MsMap); } catch (e) { console.warn('refreshCesiumWeather failed:', e); }
    if (navigator.geolocation) {
        navigator.geolocation.getCurrentPosition((position) => {
            const longitude = position.coords.longitude, latitude = position.coords.latitude;
            MsMap.camera.flyTo({ destination: Cesium.Cartesian3.fromDegrees(longitude, latitude, 5000), orientation: { pitch: Cesium.Math.toRadians(-30) } });
            const userPinId = 'you-are-here-pin', existing = MsMap.entities.getById(userPinId);
            if (existing) MsMap.entities.remove(existing);
            MsMap.entities.add({ id: userPinId, position: Cesium.Cartesian3.fromDegrees(longitude, latitude), point: { pixelSize: 12, color: Cesium.Color.RED }, label: { text: "You are here", verticalOrigin: Cesium.VerticalOrigin.TOP } });
        }, (error) => { console.error("Geolocation error:", error); });
    } else { console.error("Geolocation not supported by this browser."); }
    window.addEventListener('resize', () => {
        try { if (MsMap && MsMap.resize) MsMap.resize(); else if (MsMap && MsMap.scene) MsMap.scene.requestRender(); } catch (_) { }
    });
}


function styleReady() { return mtMap && mtMap.getStyle && mtMap.getStyle(); }

function syncSavedToMapTiler() { if (!styleReady()) return; const fc = readStore(); try { if (mtMap.getSource('saved')) mtMap.removeLayer('saved-fill'), mtMap.removeLayer('saved-line'), mtMap.removeSource('saved'); } catch (_) { } mtMap.addSource('saved', { type: 'geojson', data: fc }); mtMap.addLayer({ id: 'saved-fill', type: 'fill', source: 'saved', paint: { 'fill-color': '#2563eb', 'fill-opacity': 0.25 } }); mtMap.addLayer({ id: 'saved-line', type: 'line', source: 'saved', paint: { 'line-color': '#2563eb', 'line-width': 2 } }); }

function fcFromGroup(group) { try { return group.toGeoJSON(); } catch { return { type: 'FeatureCollection', features: [] }; } }
function fcFromGroupOrEmpty(group) { try { return group.toGeoJSON(); } catch { return { type: 'FeatureCollection', features: [] }; } }
function buildSavePayload() {
    const name = (document.getElementById('mapName')?.value || 'Default Map').trim();
    const regions = readStore();
    const obstacles = fcFromGroupOrEmpty(obstaclesLayer);
    const safe = fcFromGroupOrEmpty(safeLayer);
    return { name, regions, obstacles, safe };
}

function upsertGeoJSON(id, data, layers) { if (!styleReady()) return; if (mtMap.getSource(id)) { mtMap.getSource(id).setData(data); return; } mtMap.addSource(id, { type: 'geojson', data }); (layers || []).forEach(def => { try { const before = (mtMap.getStyle().layers.find(l => l.type === 'symbol') || {}).id; mtMap.addLayer(def, before); } catch (e) { try { mtMap.addLayer(def); } catch (_) { } } }); }

function syncObstaclesToMapTiler() { if (!styleReady()) return; const fc = fcFromGroup(obstaclesLayer); const pointLayer = { id: 'obstacles-pt', type: 'circle', source: 'obstacles', filter: ['==', ['geometry-type'], 'Point'], paint: { 'circle-radius': 5, 'circle-color': '#7f1d1d', 'circle-stroke-color': '#7f1d1d', 'circle-stroke-width': 1, 'circle-opacity': 0.9 } }; const lineLayer = { id: 'obstacles-line', type: 'line', source: 'obstacles', filter: ['in', ['geometry-type'], 'LineString', 'MultiLineString', 'Polygon', 'MultiPolygon'], paint: { 'line-color': '#7f1d1d', 'line-width': 2, 'line-opacity': 0.9 } }; const fillLayer = { id: 'obstacles-fill', type: 'fill', source: 'obstacles', filter: ['in', ['geometry-type'], 'Polygon', 'MultiPolygon'], paint: { 'fill-color': '#7f1d1d', 'fill-opacity': 0.15 } };['obstacles-pt', 'obstacles-line', 'obstacles-fill'].forEach(id => { if (mtMap.getLayer(id)) try { mtMap.removeLayer(id); } catch (_) { } }); if (mtMap.getSource('obstacles')) try { mtMap.removeSource('obstacles'); } catch (_) { } upsertGeoJSON('obstacles', fc, [fillLayer, lineLayer, pointLayer]); }

function syncSafeToMapTiler() { if (!styleReady()) return; const fc = fcFromGroup(safeLayer); const fillLayer = { id: 'safe-fill', type: 'fill', source: 'safe', paint: { 'fill-color': '#16a34a', 'fill-opacity': 0.25 } }; const lineLayer = { id: 'safe-line', type: 'line', source: 'safe', paint: { 'line-color': '#16a34a', 'line-width': 3, 'line-dasharray': [2, 2] } };['safe-line', 'safe-fill'].forEach(id => { if (mtMap.getLayer(id)) try { mtMap.removeLayer(id); } catch (_) { } }); if (mtMap.getSource('safe')) try { mtMap.removeSource('safe'); } catch (_) { } upsertGeoJSON('safe', fc, [fillLayer, lineLayer]); }

function flyMapTilerToSaved() { if (!mtMap) return; const fc = readStore(); if (!fc.features.length) return; const bbox = turf.bbox(fc); mtMap.fitBounds([[bbox[0], bbox[1]], [bbox[2], bbox[3]]], { padding: 40, pitch: 60, duration: 1200 }); }
function flyMapTilerToFeature(f) { if (!mtMap || !f) return; const c = f.properties && f.properties.centroid; if (c) mtMap.flyTo({ center: c, zoom: 12, pitch: 60, duration: 1200 }); }

// Weather overlay in 3D (OWM via raster tiles)
function refreshMapTilerWeather() { if (!styleReady()) return; const layer = weatherSelect.value; const key = owmKeyInput.value.trim(); try { if (mtMap.getLayer('owm-layer')) mtMap.removeLayer('owm-layer'); if (mtMap.getSource('owm')) mtMap.removeSource('owm'); } catch (_) { } if (!layer || !key) return; mtMap.addSource('owm', { type: 'raster', tiles: [`https://tile.openweathermap.org/map/${layer}/{z}/{x}/{y}.png?appid=${key}`], tileSize: 256, attribution: 'OpenWeatherMap' }); mtMap.addLayer({ id: 'owm-layer', type: 'raster', source: 'owm', paint: { 'raster-opacity': parseFloat(weatherOpacity.value) } }); }

// Reinitialize map when style dropdown changes
styleIdInput.addEventListener('change', () => initMapTiler(true));
document.getElementById('btnInitMapTiler').addEventListener('click', () => initMapTiler(true));
document.getElementById('btnFlyToSaved').addEventListener('click', () => flyMapTilerToSaved());
document.getElementById('btnSaveToDb').addEventListener('click', () => saveAllToDb());


// ===== Backend API (save/load) =====
function getApiBase() {
    return 'https://localhost:44313';
}
async function saveAllToDb() {
    try {
        const payload = buildSavePayload();
        const res = await fetch(`${getApiBase()}/api/map/saveAll`, {
            method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(payload)
        });
        if (!res.ok) { throw new Error(`HTTP ${res.status}`); }
        const data = await res.json();
        alert(`Saved! id=${data.id}`);
    } catch (err) { console.error(err); alert('Save failed. Check API base and CORS.'); }
}
async function loadLatestFromDb() {
    try {
        const res = await fetch(`${getApiBase()}/api/map/loadLatest`);
        if (!res.ok) { throw new Error(`HTTP ${res.status}`); }
        const data = await res.json();
        writeStore(data.regions || { type: 'FeatureCollection', features: [] });
        obstaclesLayer.clearLayers();
        if (data.obstacles && data.obstacles.features) {
            L.geoJSON(data.obstacles, { style: { color: '#7f1d1d', weight: 2 }, onEachFeature: (_, ly) => attachObstaclePopup(ly) }).eachLayer(l => obstaclesLayer.addLayer(l));
        }
        syncObstaclesToMapTiler();
        safeLayer.clearLayers();
        if (data.safe && data.safe.features) {
            L.geoJSON(data.safe, { style: { color: '#16a34a', weight: 3, dashArray: '4 2' } }).eachLayer(l => safeLayer.addLayer(l));
        }
        syncSafeToMapTiler();
        const fc = readStore();
        if ((fc.features || []).length) { selectSavedFeature(fc.features[0].properties.id); }
        alert(`Loaded: ${data.name || 'map'} (id=${data.id})`);
    } catch (err) { console.error(err); alert('Load failed. Check API base and CORS.'); }
}
document.get
