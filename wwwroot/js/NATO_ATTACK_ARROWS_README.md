# NATO Attack Arrows - Double-Lined Arrowheads with Symbol Numbers

## Overview
This implementation provides NATO-standard attack arrows with:
- **Double parallel lines** for attack paths (NATO standard)
- **Arrowheads showing attacker symbol numbers** inside them
- **Proper NATO-style arrowhead shapes** with SVG rendering
- **Curved attack paths** for better visibility
- **Dynamic updates** when tokens move

## Features

### 1. Double-Lined Attack Arrows
Attack arrows are now rendered with two parallel lines, following NATO military symbology standards:
- Main attacks: Solid double lines
- Supporting attacks: Dashed double lines
- Feint attacks: Light dashed double lines
- Different line weights for different attack types

### 2. NATO Arrowhead with Symbol Number
Arrowheads now display:
- **Symbol number** extracted from attacker token name (e.g., "01" from "Token 01")
- **Proper arrowhead shape** using SVG
- **Color coding** based on attack type
- **Drop shadows** for better visibility
- **Rotation** to point at target

### 3. Attack Types Supported
- **Frontal Attack**: Direct assault with solid lines
- **Flanking Attack**: Side approach
- **Envelopment**: Surrounding maneuver
- **Penetration**: Break-through attack
- **Raid**: Quick strike operation
- **Ambush**: Concealed attack
- **Supporting Attack**: Secondary effort
- **Feint**: Diversionary attack

## Usage

### Creating Attack Arrows

```javascript
// Create attack line between two tokens
const attackId = window.attackVisualizationManager.addAttackLine(
    attackerToken,  // Token object with name, id, etc.
    targetToken,    // Target token object
    attackOrder     // Optional: attack order data
);
```

### Customizing Arrowhead

The arrowhead automatically extracts the symbol number from the attacker token name:
- "Token 01" → "01"
- "Unit 12" → "12"
- "Alpha 3" → "3"
- "Brigade" → "BRI" (if no number found)

### Symbol Number Extraction

The system uses smart pattern matching to extract numbers:
1. Numbers at the end: "Token 01" → "01"
2. Numbers in the middle: "Unit 12 Alpha" → "12"
3. Letter followed by number: "A 123" → "123"
4. Fallback: First 2-3 characters if no number found

## Visual Characteristics

### Arrowhead Design
- **Width**: 60px
- **Height**: 50px
- **Shape**: NATO standard arrow with rectangular tail
- **Color**: #ff4444 (red) for main attacks, customizable
- **Symbol text**: White, bold, 14px
- **Border**: 2px black stroke
- **Shadow**: Drop shadow for depth

### Double Lines
- **Spacing**: 0.00008 degrees between parallel lines
- **Weight**: 3px per line (adjustable by attack type)
- **Color**: Matches attack type (red for main attacks)
- **Opacity**: 0.9 (increases to 1.0 on hover)

## Examples

### Simple Attack Line
```javascript
// Get tokens
const attacker = { id: '123', name: 'Token 01', /* ... */ };
const target = { id: '456', name: 'Token 02', /* ... */ };

// Create attack
const attackId = window.attackVisualizationManager.addAttackLine(attacker, target);
```

### Attack with Custom Type
```javascript
// Create flanking attack
const attackOrder = {
    intent: {
        natoAttackType: 'flanking'
    }
};

const attackId = window.attackVisualizationManager.addAttackLine(
    attackerToken,
    targetToken,
    attackOrder
);
```

## CSS Classes

### Main Classes
- `.nato-attack-arrowhead-marker` - Arrowhead container
- `.nato-attack-arrowhead` - Arrowhead SVG wrapper
- `.nato-attack-line` - Attack line base class
- `.attack-arrow-fallback` - Fallback style if renderer unavailable

### Attack Type Classes
- `.nato-attack-main` - Main attack
- `.nato-attack-frontal` - Frontal attack
- `.nato-attack-flanking` - Flanking attack
- `.nato-attack-envelopment` - Envelopment
- `.nato-attack-penetration` - Penetration
- `.nato-attack-raid` - Raid
- `.nato-attack-ambush` - Ambush
- `.nato-attack-supporting` - Supporting attack
- `.nato-attack-feint` - Feint

## Interactions

### Hover Effects
- Lines become thicker (5px)
- Opacity increases to 1.0
- Arrowhead scales up slightly (1.1x)
- Drop shadow becomes more pronounced

### Click Events
- Opens attack summary popup
- Shows attacker and target info
- Displays attack order details
- Provides edit and delete options

### Token Movement
- Attack arrows update automatically when tokens are dragged
- Both lines and arrowhead reposition
- Arrowhead rotates to maintain correct direction
- Curved path recalculates smoothly

## Performance

### Optimization Features
- Uses Leaflet LayerGroups for efficient rendering
- SVG for crisp arrowheads at any zoom level
- Minimal DOM elements (2 polylines + 1 marker per attack)
- Smart event handling on layer groups
- Efficient path offset calculations

## Browser Compatibility

### Fully Supported
- Chrome 90+
- Firefox 88+
- Safari 14+
- Edge 90+

### Required Features
- SVG support
- CSS transforms
- Drop shadow filters
- Leaflet.js 1.7+

## Accessibility

### Features
- High contrast mode support
- Reduced motion support (disables animations)
- Semantic HTML structure
- Proper ARIA labels (where applicable)
- Keyboard navigation support

## Troubleshooting

### Arrowheads Not Showing
1. Verify `AttackSymbolRenderer.js` is loaded
2. Check that `window.attackSymbolRenderer` exists
3. Ensure attacker token has a valid name
4. Check browser console for errors

### Double Lines Not Parallel
1. Verify path has at least 2 points
2. Check that spacing value is appropriate for zoom level
3. Ensure `offsetPath` method is working correctly

### Symbol Numbers Not Extracting
1. Check token name format
2. Verify regex patterns in `extractSymbolNumber`
3. Test with different token naming conventions
4. Check browser console logs

### Lines Not Updating on Token Move
1. Verify token markers have drag enabled
2. Check that `setupTokenMovementTracking` is called
3. Ensure markers have `tokenId` property
4. Verify LayerGroup event handlers are set up

## Future Enhancements

### Planned Features
- Multiple arrowhead styles (NATO variants)
- Attack strength indicators (line thickness)
- Animated attack progression
- 3D arrow effects for certain attack types
- Custom symbol text (not just numbers)
- Attack path waypoints
- Time-based attack animations

### Configuration Options
- Adjustable line spacing
- Custom colors per attack
- Variable arrowhead sizes
- Configurable symbol fonts
- Attack line dash patterns

## Code Structure

### Files
- `AttackSymbolRenderer.js` - Arrow and symbol rendering
- `AttackVisualizationManager.js` - Attack line management
- `attackSymbols.css` - Styling for arrows and symbols

### Key Methods

#### AttackSymbolRenderer
- `createAttackArrow()` - Creates arrowhead with symbol
- `createAttackLine()` - Creates double parallel lines
- `offsetPath()` - Calculates parallel line offsets
- `extractSymbolNumber()` - Extracts number from token name

#### AttackVisualizationManager
- `addAttackLine()` - Adds new attack arrow
- `drawAttackLine()` - Renders attack on map
- `createAttackArrow()` - Creates arrowhead marker
- `setupTokenMovementTracking()` - Enables dynamic updates
- `removeAttackLine()` - Removes attack arrow

## Credits

Created for KSAGAME - Military Wargaming System
Implements NATO APP-6 military symbology standards
Built with Leaflet.js mapping library

## License

Copyright © 2025 KSAGAME
All rights reserved.

