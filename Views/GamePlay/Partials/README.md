# GamePlay Arena - Lazy Loading Partial Views

This document explains the lazy loading system implemented for the GamePlay Arena to improve performance and maintainability.

## Overview

The GamePlay Arena has been refactored to use partial views with lazy loading to:
- Reduce initial page load time
- Improve maintainability by separating concerns
- Enable better caching of individual components
- Provide smoother user experience

## Structure

```
Views/GamePlay/
├── Index.cshtml (Original monolithic view)
├── IndexLazy.cshtml (New lazy-loading version)
└── Partials/
    ├── Controls/
    │   ├── _RegionPanel.cshtml
    │   └── _OverlayControls.cshtml
    ├── Modals/
    │   ├── _DataEntryModal.cshtml
    │   ├── _TokenManagementModal.cshtml
    │   ├── _TokenSelectionModal.cshtml
    │   ├── _SimulationPanel.cshtml
    │   ├── _UnitDeploymentModal.cshtml
    │   ├── _MovementPlanModal.cshtml
    │   ├── _BattleModal.cshtml
    │   ├── _ObjectiveModal.cshtml
    │   └── _SettingsModal.cshtml
    └── Scripts/
        ├── _CoreScripts.cshtml
        ├── _TokenScripts.cshtml
        └── _MapScripts.cshtml
```

## How It Works

### 1. Lazy Loader Service (`wwwroot/js/lazy-loader.js`)

The `LazyLoader` class provides:
- Asynchronous loading of partial views
- Caching to prevent duplicate requests
- Loading indicators and error handling
- Preloading capabilities for critical components

### 2. Controller Support (`Controllers/GamePlayController.cs`)

Added `LoadPartial` action method that:
- Validates partial names for security
- Maps friendly names to actual partial paths
- Returns partial views as HTML responses

### 3. Main View (`Views/GamePlay/IndexLazy.cshtml`)

The main view now:
- Loads core components immediately (map, essential controls)
- Lazy loads modals when needed
- Preloads critical modals in background
- Provides loading states and error handling

## Usage

### Loading Core Components

Core components are loaded immediately on page load:

```javascript
await gamePlayLoader.loadCoreComponents();
```

### Loading Modals On-Demand

Modals are loaded when the user requests them:

```javascript
// Load a modal
await gamePlayLoader.loadModal('data-entry-modal');

// Or use the data attribute
<button data-modal="token-selection-modal">Open Token Selection</button>
```

### Preloading Components

Critical components can be preloaded in the background:

```javascript
lazyLoader.preloadPartials(['data-entry-modal', 'token-selection-modal']);
```

## Available Partials

### Controls
- `region-panel` - Map region controls and settings
- `overlay-controls` - War game tools and controls

### Modals
- `data-entry-modal` - Military data management
- `token-management-modal` - Token management interface
- `token-selection-modal` - Token selection for placement
- `simulation-panel` - War game simulation controls
- `unit-deployment-modal` - Unit deployment interface
- `movement-plan-modal` - Movement planning
- `battle-modal` - Battle initiation
- `objective-modal` - Objective creation
- `settings-modal` - Game settings

### Scripts
- `scripts-core` - Core JavaScript functionality
- `scripts-token` - Token management scripts
- `scripts-map` - Map-related scripts

## Performance Benefits

1. **Reduced Initial Load**: Main page loads only essential components
2. **On-Demand Loading**: Modals load only when needed
3. **Caching**: Components are cached after first load
4. **Parallel Loading**: Multiple components can load simultaneously
5. **Background Preloading**: Critical components preload for instant access

## Migration from Original View

To switch from the original monolithic view to lazy loading:

1. Update the route or controller action to use `IndexLazy.cshtml`
2. Test all functionality to ensure proper loading
3. Monitor performance improvements
4. Gradually migrate any custom JavaScript to the new system

## Adding New Partials

1. Create the partial view in the appropriate subfolder
2. Add the mapping in `GamePlayController.LoadPartial` method
3. Update the lazy loading configuration in `IndexLazy.cshtml`
4. Test the new partial in isolation and integrated

## Error Handling

The system includes comprehensive error handling:
- Network failures show retry options
- Invalid partials return appropriate errors
- Loading states provide user feedback
- Fallback mechanisms for critical functionality

## Caching Strategy

- **Client-side caching**: Partials are cached in memory after first load
- **Browser caching**: Static resources use appropriate cache headers
- **Cache invalidation**: Cache can be cleared when needed

## Best Practices

1. **Keep partials focused**: Each partial should have a single responsibility
2. **Minimize dependencies**: Reduce coupling between partials
3. **Handle loading states**: Always provide user feedback during loading
4. **Error recovery**: Implement fallbacks for critical functionality
5. **Performance monitoring**: Track loading times and user experience

## Troubleshooting

### Common Issues

1. **Partial not loading**: Check the mapping in `LoadPartial` method
2. **JavaScript errors**: Ensure all dependencies are loaded
3. **Styling issues**: Check CSS dependencies and scope
4. **Caching problems**: Clear cache and test again

### Debug Mode

Enable debug logging:

```javascript
// Enable detailed logging
lazyLoader.debugMode = true;
```

This will provide detailed console output for troubleshooting.
