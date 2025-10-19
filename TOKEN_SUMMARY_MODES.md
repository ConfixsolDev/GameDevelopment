# Token Summary Display - Mode Reference

## Overview
The token summary modal shows detailed ORBAT (Order of Battle) information when you click on a token. However, **token clicks behave differently based on the current action mode**.

## Modes Where Token Summary WORKS ✅

### 1. **No Mode / Default** (`null`)
- **When**: No action mode button is selected
- **Behavior**: Clicking on any token shows the token summary modal
- **Use Case**: General inspection of token details

### 2. **Select Mode** (`select`)
- **When**: Select button is active
- **Behavior**: Clicking on any token shows the token summary modal
- **Use Case**: Selecting and viewing token information

### 3. **Default/Fallback**
- **Behavior**: If an unknown mode is detected, it falls back to showing token details

## Modes Where Token Summary DOES NOT WORK ❌

### 1. **Place Mode** (`place`)
- **When**: Place Token button is active (currently active in your log)
- **Behavior**: **Token clicks are completely ignored**
- **Message**: "Token click ignored in placement mode"
- **Reason**: This mode is for placing NEW tokens, not interacting with existing ones

### 2. **Move Mode** (`move`)
- **When**: Move button is active
- **Behavior**: Shows **movement planning modal** instead of token summary
- **Use Case**: Planning token movements, not viewing details

### 3. **Attack Mode** (`attack`)
- **When**: Attack button is active
- **Behavior**: Clicking token **starts attack mode** with that token as attacker
- **Use Case**: Planning attacks, not viewing details

### 4. **Pan-Attack Mode** (`pan-attack`)
- **When**: Pan-Attack button is active
- **Behavior**: Clicking token **starts pan-attack mode** with that token
- **Use Case**: Planning pan attacks, not viewing details

---

## Current Issue in Your Session

Based on your logs:
```javascript
TokenActionModeManager.js:919 🔄 Mode restored from storage: place
```

**You are currently in PLACE mode**, which is why clicking on tokens does nothing. The code explicitly blocks token clicks in this mode:

```javascript
case 'place':
    // In placement mode, clicking tokens shouldn't do anything
    console.log('Token click ignored in placement mode');
    break;
```

---

## Solution: How to View Token Summary

### Option 1: Clear the Mode (Recommended)
1. **Click on an empty area of the map** (not on a token)
2. OR **click the same mode button again** to deselect it
3. This will set mode to `null`, allowing token summary to work

### Option 2: Switch to Select Mode
1. **Add/click a "Select" mode button** if available
2. This explicitly enables token inspection

### Option 3: Test Token Summary Directly (Bypass Mode)
Use the debug function in the console:
```javascript
// Test with a specific token ID
testTokenSummary('7df4a88c-c11a-4fbd-b422-2eef438c49ce');

// Or use this to view token 98
testTokenSummaryDirect('7df4a88c-c11a-4fbd-b422-2eef438c49ce');
```

---

## Code Location Reference

### Token Click Handler
**File**: `wwwroot/js/TokenPlacementManager.js`  
**Method**: `handleTokenClick(token, marker)` (Line 2238)

```javascript
handleTokenClick(token, marker) {
    const currentMode = window.tokenActionModeManager?.getCurrentMode();
    
    switch (currentMode) {
        case null: // ✅ Shows token details
        case 'select': // ✅ Shows token details
            this.showExistingTokenDetails(token);
            break;
            
        case 'place': // ❌ Ignores clicks
            console.log('Token click ignored in placement mode');
            break;
            
        case 'move': // ❌ Shows movement modal
            this.showConfirmMoveModal(token);
            break;
            
        case 'attack': // ❌ Starts attack
            window.tokenActionModeManager.handleTokenAttack(marker.getLatLng());
            break;
            
        // ... other modes
    }
}
```

### Token Summary Method
**File**: `wwwroot/js/TokenManager.js`  
**Method**: `showTokenDetails(token)` (Line 307)

This method loads the token summary from `/DataManagement/GetTokenSummary` and displays it in a Bootstrap modal.

---

## Performance Notes

Your optimizations are working well:
- ⚡ Phase 1 (Map): 754ms
- ⚡ Phase 2 (Core Managers): **4ms** (95% improvement!)
- ⚡ Phase 3 (UI): 759ms
- ⚡ Background loading: 2680ms (now in optimized order - tokens load BEFORE attack visualization)

### Fixed Issues:
1. ✅ Token placement error fixed (was calling `startPlacementMode()` without token parameter)
2. ✅ Invalid LatLng error fixed (added validation and fallback in `getLatLngFromEvent`)
3. ✅ Attack lines now load correctly (tokens load BEFORE attack visualization in background)

---

## Recommendation

**Add a "Cancel" or "Clear Mode" button** to allow users to easily exit placement mode and return to normal token inspection mode. This would improve UX significantly.

Alternatively, **add a tooltip** to the action buttons explaining:
- "While in Place mode, you cannot view existing tokens. Click here again to exit and view token details."

