# KSA Game - Reusable UI Component Library

## Overview

This document provides comprehensive documentation for the KSA Game reusable UI component library, including the dark theme system, modal components, token cards, and all other reusable UI elements.

## Table of Contents

1. [Dark Theme System](#dark-theme-system)
2. [Modal System](#modal-system)
3. [Token Cards](#token-cards)
4. [Component Library](#component-library)
5. [Usage Examples](#usage-examples)
6. [Integration Guide](#integration-guide)
7. [Customization](#customization)

---

## Dark Theme System

### Features
- **Complete CSS Variable System**: Comprehensive color palette with light/dark variants
- **Automatic Theme Detection**: Detects system preference and saves user choice
- **Smooth Transitions**: All theme changes include smooth transitions
- **Enhanced Animations**: Professional hover effects with shimmer, glow, and pulse animations
- **Mobile Support**: Updates mobile browser theme colors
- **Persistent Storage**: Remembers user's theme preference
- **Advanced Micro-interactions**: Creative hover states and loading animations

### CSS Variables

The theme system uses CSS custom properties for all colors, spacing, and other design tokens:

```css
:root {
    /* Primary Colors */
    --primary-500: #E1261C;
    --primary-600: #dc2626;
    
    /* Background Colors */
    --bg-primary: #ffffff;
    --bg-secondary: #f8fafc;
    
    /* Text Colors */
    --text-primary: #0f172a;
    --text-secondary: #475569;
    
    /* Spacing Scale */
    --space-1: 0.25rem;
    --space-2: 0.5rem;
    
    /* And many more... */
}
```

### JavaScript API

```javascript
// Initialize theme system
window.themeSystem = new ThemeSystem();

// Toggle theme
themeSystem.toggleTheme();

// Set specific theme
themeSystem.setTheme('dark');

// Get current theme
const currentTheme = themeSystem.getCurrentTheme();

// Check if dark mode
const isDark = themeSystem.isDark();
```

---

## Modal System

### Features
- **Standardized Design**: Consistent modal appearance across the application
- **Multiple Sizes**: sm, md, lg, xl, full
- **Keyboard Support**: ESC key to close, focus management
- **Backdrop Control**: Optional backdrop with blur effect
- **Stack Support**: Multiple modals can be stacked
- **Animation**: Smooth show/hide animations

### Usage

#### HTML Structure
```html
<div id="myModal" class="modal modal-md">
    <div class="modal-overlay"></div>
    <div class="modal-container">
        <div class="modal">
            <div class="modal-header">
                <h3 class="modal-title">Modal Title</h3>
                <button class="modal-close" onclick="modalSystem.hide('myModal')">
                    <i class="fas fa-times"></i>
                </button>
            </div>
            <div class="modal-body">
                Modal content here
            </div>
            <div class="modal-footer">
                <button class="btn btn-secondary" onclick="modalSystem.hide('myModal')">Cancel</button>
                <button class="btn btn-primary">Save</button>
            </div>
        </div>
    </div>
</div>
```

#### JavaScript API
```javascript
// Show modal
modalSystem.show('myModal', {
    size: 'lg',
    closable: true,
    backdrop: true,
    keyboard: true
});

// Hide modal
modalSystem.hide('myModal');

// Close all modals
modalSystem.closeAll();
```

#### Razor Component Usage
```csharp
@await Component.InvokeAsync("ReusableUI", new { 
    component = "Modal", 
    data = new { 
        id = "myModal",
        title = "Modal Title",
        content = "Modal content here",
        size = "lg"
    } 
})
```

---

## Enhanced Animations & Effects

### Animation Classes
The component library includes a comprehensive set of animation classes for enhanced user experience:

#### Basic Animations
- `animate-fade-in` - Smooth fade in effect
- `animate-slide-up` - Slide up from bottom
- `animate-slide-down` - Slide down from top
- `animate-slide-left` - Slide in from left
- `animate-slide-right` - Slide in from right

#### Advanced Effects
- `animate-pulse` - Pulsing scale effect
- `animate-glow` - Glowing shadow effect
- `animate-shimmer` - Shimmer sweep effect
- `animate-float` - Gentle floating motion
- `animate-bounce` - Bouncing animation

### Hover Effects
All components include sophisticated hover effects:
- **Shimmer Animation**: Light sweep effect on hover
- **Scale Transform**: Subtle scale increase (1.02x-1.05x)
- **Enhanced Shadows**: Multi-layered shadow effects
- **Color Transitions**: Smooth color transitions
- **Glow Effects**: Colored glow around interactive elements

### Usage Examples
```css
/* Apply animations to elements */
.token-card {
    animation: slideInUp 0.6s ease-out;
}

.btn:hover {
    animation: pulse 0.6s ease-in-out;
}

.status-badge.active {
    animation: glow 2s infinite;
}
```

---

## Token Cards

### Features
- **Consistent Design**: Standardized token card appearance with enhanced styling
- **Enhanced Animations**: Professional hover effects with shimmer, scale, and glow animations
- **Status Indicators**: Visual status badges with animated glow effects
- **Image Support**: Token images with enhanced hover effects and fallback placeholders
- **Metadata Display**: Configurable metadata fields with animated hover states
- **Action Buttons**: Customizable action buttons with enhanced animations
- **Selection Support**: Optional selection functionality with visual feedback
- **Professional Hover States**: Creative hover effects that enhance user interaction

### Usage

#### JavaScript Creation
```javascript
const tokenCard = componentLibrary.create('token-card', {
    id: 'token-123',
    name: 'Infantry Unit',
    description: 'Standard infantry unit',
    image: '/images/tokens/infantry.jpg',
    status: 'active',
    meta: {
        type: 'Infantry',
        strength: '100',
        range: '500m'
    },
    actions: [
        {
            text: 'Edit',
            variant: 'primary',
            onclick: 'editToken("token-123")'
        },
        {
            text: 'Delete',
            variant: 'error',
            onclick: 'deleteToken("token-123")'
        }
    ]
}, container);
```

#### Razor Component Usage
```csharp
@await Component.InvokeAsync("ReusableUI", new { 
    component = "TokenCard", 
    data = new { 
        id = token.Id,
        name = token.Name,
        description = token.Description,
        image = token.AssetImagePath,
        status = token.Status,
        meta = new Dictionary<string, object> {
            {"type", token.Type},
            {"strength", token.Strength}
        },
        actions = new List<dynamic> {
            new { text = "Edit", variant = "primary", onclick = $"editToken('{token.Id}')" }
        }
    } 
})
```

---

## Component Library

### Available Components

#### 1. Alert Component
```csharp
@await Component.InvokeAsync("ReusableUI", new { 
    component = "Alert", 
    data = new { 
        type = "success",
        title = "Success!",
        message = "Operation completed successfully",
        closable = true,
        autoClose = 5000
    } 
})
```

#### 2. Button Component
```csharp
@await Component.InvokeAsync("ReusableUI", new { 
    component = "Button", 
    data = new { 
        text = "Click Me",
        variant = "primary",
        size = "md",
        icon = "fas fa-save",
        onclick = "saveData()"
    } 
})
```

#### 3. Card Component
```csharp
@await Component.InvokeAsync("ReusableUI", new { 
    component = "Card", 
    data = new { 
        title = "Card Title",
        subtitle = "Card subtitle",
        content = "Card content here",
        hoverable = true
    } 
})
```

#### 4. Badge Component
```csharp
@await Component.InvokeAsync("ReusableUI", new { 
    component = "Badge", 
    data = new { 
        text = "Badge Text",
        variant = "success",
        icon = "fas fa-check",
        closable = true
    } 
})
```

---

## Usage Examples

### Complete Modal with Token Selection

```csharp
<!-- Modal -->
@await Component.InvokeAsync("ReusableUI", new { 
    component = "Modal", 
    data = new { 
        id = "tokenSelectionModal",
        title = "Select Token",
        size = "lg"
    } 
})

<!-- Token Grid -->
<div class="modal-body">
    <div class="row">
        @foreach (var token in Model.Tokens)
        {
            <div class="col-md-4 col-lg-3 mb-3">
                @await Component.InvokeAsync("ReusableUI", new { 
                    component = "TokenCard", 
                    data = new { 
                        id = token.Id,
                        name = token.Name,
                        description = token.Description,
                        image = token.AssetImagePath,
                        status = token.IsActive ? "active" : "inactive",
                        selectable = true,
                        meta = new Dictionary<string, object> {
                            {"type", token.Type},
                            {"team", token.TeamName}
                        }
                    } 
                })
            </div>
        }
    </div>
</div>
```

### Form with Alerts

```csharp
<!-- Alert -->
@if (TempData["SuccessMessage"] != null)
{
    @await Component.InvokeAsync("ReusableUI", new { 
        component = "Alert", 
        data = new { 
            type = "success",
            title = "Success!",
            message = TempData["SuccessMessage"].ToString(),
            closable = true
        } 
    })
}

<!-- Form -->
<form method="post">
    <div class="row">
        <div class="col-md-6">
            <div class="form-group">
                <label class="form-label">Token Name</label>
                <input type="text" class="form-control" name="Name" required>
            </div>
        </div>
        <div class="col-md-6">
            <div class="form-group">
                <label class="form-label">Token Type</label>
                <select class="form-control" name="Type">
                    <option value="Infantry">Infantry</option>
                    <option value="Armored">Armored</option>
                </select>
            </div>
        </div>
    </div>
    
    <div class="d-flex gap-3">
        @await Component.InvokeAsync("ReusableUI", new { 
            component = "Button", 
            data = new { 
                text = "Save Token",
                variant = "primary",
                icon = "fas fa-save",
                type = "submit"
            } 
        })
        
        @await Component.InvokeAsync("ReusableUI", new { 
            component = "Button", 
            data = new { 
                text = "Cancel",
                variant = "secondary",
                type = "button",
                onclick = "history.back()"
            } 
        })
    </div>
</form>
```

---

## Integration Guide

### 1. Add to Existing Views

Replace existing layouts with the new theme layout:

```csharp
@{
    Layout = "_ThemeLayout";
}
```

### 2. Update Existing Modals

Convert Bootstrap modals to the new system:

```html
<!-- Old Bootstrap Modal -->
<div class="modal fade" id="oldModal">
    <!-- content -->
</div>

<!-- New Theme Modal -->
@await Component.InvokeAsync("ReusableUI", new { 
    component = "Modal", 
    data = new { 
        id = "newModal",
        title = "Modal Title",
        content = "Modal content"
    } 
})
```

### 3. Replace Custom Cards

Convert existing token cards to the new system:

```html
<!-- Old Custom Card -->
<div class="custom-token-card">
    <!-- content -->
</div>

<!-- New Token Card -->
@await Component.InvokeAsync("ReusableUI", new { 
    component = "TokenCard", 
    data = tokenData 
})
```

### 4. Update JavaScript

Use the new JavaScript APIs:

```javascript
// Old modal show
$('#modal').modal('show');

// New modal show
modalSystem.show('modal');

// Old theme toggle
document.body.classList.toggle('dark-theme');

// New theme toggle
themeSystem.toggleTheme();
```

---

## Customization

### 1. Custom Colors

Add custom colors to the CSS variables:

```css
:root {
    --custom-accent: #your-color;
    --custom-accent-dark: #your-dark-color;
}

[data-theme="dark"] {
    --custom-accent: #your-dark-color;
}
```

### 2. Custom Components

Create new components by adding them to the component library:

```csharp
// In _YourComponent.cshtml
@model dynamic
<div class="your-component">
    <!-- Your component content -->
</div>

// Register in Default.cshtml
case "yourcomponent":
    @await Html.PartialAsync("_YourComponent", data)
    break;
```

### 3. Custom Themes

Create additional themes by adding new data-theme attributes:

```css
[data-theme="custom"] {
    --primary-500: #your-primary;
    --bg-primary: #your-background;
    /* ... other variables ... */
}
```

---

## Best Practices

### 1. Consistent Usage
- Always use the component library for new UI elements
- Follow the established naming conventions
- Use the provided CSS variables for colors and spacing

### 2. Performance
- Load the theme system early in the page lifecycle
- Use the lazy loading features for modals
- Minimize custom CSS overrides

### 3. Accessibility
- Always provide proper ARIA labels
- Ensure keyboard navigation works
- Test with screen readers

### 4. Responsive Design
- Use the provided responsive utilities
- Test on multiple device sizes
- Follow mobile-first principles

---

## Troubleshooting

### Common Issues

1. **Theme not applying**: Ensure the theme system JavaScript is loaded
2. **Modals not showing**: Check that modal IDs are unique and JavaScript is initialized
3. **Components not rendering**: Verify the component name matches the registered components
4. **Styles not updating**: Clear browser cache and check CSS file loading order

### Debug Mode

Enable debug mode to see console logs:

```javascript
// Add to console for debugging
window.themeSystem.debug = true;
window.modalSystem.debug = true;
```

---

## Future Enhancements

### Planned Features
- Additional component types (tables, forms, navigation)
- Theme customization interface
- Component playground/demo page
- Advanced animation options
- Component testing framework

### Contributing
- Follow the established patterns
- Add comprehensive documentation
- Include usage examples
- Test across different browsers and devices

---

This component library provides a solid foundation for building consistent, accessible, and maintainable user interfaces across the KSA Game application. The dark theme system ensures a modern user experience while the reusable components reduce development time and ensure design consistency.
