/**
 * Dark Theme System - JavaScript Implementation
 * Complete theme management with localStorage persistence
 */

class ThemeSystem {
    constructor() {
        this.currentTheme = 'light';
        this.themes = {
            light: 'light',
            dark: 'dark'
        };
        this.storageKey = 'ksagame-theme';
        this.mediaQuery = window.matchMedia('(prefers-color-scheme: dark)');
        
        this.init();
    }

    /**
     * Initialize the theme system
     */
    init() {
        this.loadTheme();
        this.createThemeSwitcher();
        this.bindEvents();
        this.applyTheme();
        console.log('🎨 Theme System initialized');
    }

    /**
     * Load theme from localStorage or system preference
     */
    loadTheme() {
        const stored = localStorage.getItem(this.storageKey);
        
        if (stored && this.themes[stored]) {
            this.currentTheme = stored;
        } else {
            // Use system preference as default
            this.currentTheme = this.mediaQuery.matches ? 'dark' : 'light';
        }
    }

    /**
     * Save theme to localStorage
     */
    saveTheme(theme) {
        localStorage.setItem(this.storageKey, theme);
    }

    /**
     * Apply theme to document
     */
    applyTheme() {
        const root = document.documentElement;
        
        // Remove existing theme classes
        Object.values(this.themes).forEach(theme => {
            root.classList.remove(`theme-${theme}`);
        });
        
        // Add current theme class
        root.classList.add(`theme-${this.currentTheme}`);
        
        // Set data attribute for CSS targeting
        root.setAttribute('data-theme', this.currentTheme);
        
        // Update meta theme-color for mobile browsers
        this.updateMetaThemeColor();
        
        console.log(`🎨 Applied theme: ${this.currentTheme}`);
    }

    /**
     * Update meta theme-color for mobile browsers
     */
    updateMetaThemeColor() {
        let metaThemeColor = document.querySelector('meta[name="theme-color"]');
        
        if (!metaThemeColor) {
            metaThemeColor = document.createElement('meta');
            metaThemeColor.name = 'theme-color';
            document.head.appendChild(metaThemeColor);
        }
        
        const colors = {
            light: '#ffffff',
            dark: '#0f172a'
        };
        
        metaThemeColor.content = colors[this.currentTheme] || colors.light;
    }

    /**
     * Toggle between light and dark themes
     */
    toggleTheme() {
        const newTheme = this.currentTheme === 'light' ? 'dark' : 'light';
        this.setTheme(newTheme);
    }

    /**
     * Set specific theme
     */
    setTheme(theme) {
        if (!this.themes[theme]) {
            console.warn(`Invalid theme: ${theme}`);
            return;
        }
        
        this.currentTheme = theme;
        this.saveTheme(theme);
        this.applyTheme();
        this.updateThemeSwitcher();
        
        // Dispatch custom event
        window.dispatchEvent(new CustomEvent('themeChanged', {
            detail: { theme: this.currentTheme }
        }));
    }

    /**
     * Create theme switcher button
     */
    createThemeSwitcher() {
        // Check if switcher already exists
        if (document.querySelector('.theme-switcher')) {
            return;
        }
        
        const switcher = document.createElement('div');
        switcher.className = 'theme-switcher';
        
        const button = document.createElement('button');
        button.className = 'theme-toggle';
        button.setAttribute('aria-label', 'Toggle theme');
        button.setAttribute('title', 'Toggle theme');
        
        // Add click event
        button.addEventListener('click', () => {
            this.toggleTheme();
        });
        
        switcher.appendChild(button);
        document.body.appendChild(switcher);
        
        this.updateThemeSwitcher();
    }

    /**
     * Update theme switcher icon and tooltip
     */
    updateThemeSwitcher() {
        const button = document.querySelector('.theme-toggle');
        if (!button) return;
        
        const icons = {
            light: '🌙', // Moon for switching to dark
            dark: '☀️'   // Sun for switching to light
        };
        
        const tooltips = {
            light: 'Switch to dark theme',
            dark: 'Switch to light theme'
        };
        
        button.textContent = icons[this.currentTheme];
        button.setAttribute('title', tooltips[this.currentTheme]);
        button.setAttribute('aria-label', tooltips[this.currentTheme]);
    }

    /**
     * Bind event listeners
     */
    bindEvents() {
        // Listen for system theme changes
        this.mediaQuery.addEventListener('change', (e) => {
            if (!localStorage.getItem(this.storageKey)) {
                // Only auto-switch if no manual preference is set
                this.setTheme(e.matches ? 'dark' : 'light');
            }
        });
        
        // Listen for custom theme change events
        window.addEventListener('themeChanged', (e) => {
            console.log(`🎨 Theme changed to: ${e.detail.theme}`);
        });
    }

    /**
     * Get current theme
     */
    getCurrentTheme() {
        return this.currentTheme;
    }

    /**
     * Check if current theme is dark
     */
    isDark() {
        return this.currentTheme === 'dark';
    }

    /**
     * Check if current theme is light
     */
    isLight() {
        return this.currentTheme === 'light';
    }
}

/**
 * Modal System - Enhanced with theme support
 */
class ModalSystem {
    constructor() {
        this.activeModals = new Set();
        this.modalStack = [];
        this.init();
    }

    /**
     * Initialize modal system
     */
    init() {
        // Bind escape key to close modals
        document.addEventListener('keydown', (e) => {
            if (e.key === 'Escape') {
                this.closeTopModal();
            }
        });
        
        // Bind click outside to close modals
        document.addEventListener('click', (e) => {
            if (e.target.classList.contains('modal-overlay')) {
                this.closeTopModal();
            }
        });
        
        console.log('🎭 Modal System initialized');
    }

    /**
     * Show modal
     */
    show(modalId, options = {}) {
        const modal = document.getElementById(modalId);
        if (!modal) {
            console.error(`Modal not found: ${modalId}`);
            return false;
        }

        const {
            size = 'md',
            closable = true,
            backdrop = true,
            keyboard = true,
            focus = true,
            animation = true
        } = options;

        // Set modal size
        modal.className = modal.className.replace(/modal-\w+/g, '');
        modal.classList.add(`modal-${size}`);

        // Create overlay if it doesn't exist
        let overlay = document.querySelector('.modal-overlay');
        if (!overlay) {
            overlay = this.createOverlay();
        }

        // Show modal
        this.showModal(modal, overlay, { closable, backdrop, animation });

        // Add to stack
        this.modalStack.push(modalId);
        this.activeModals.add(modalId);

        // Focus management
        if (focus) {
            this.focusModal(modal);
        }

        // Dispatch event
        window.dispatchEvent(new CustomEvent('modalShown', {
            detail: { modalId, modal }
        }));

        return true;
    }

    /**
     * Hide modal
     */
    hide(modalId, animation = true) {
        const modal = document.getElementById(modalId);
        if (!modal) {
            console.error(`Modal not found: ${modalId}`);
            return false;
        }

        // Remove from stack
        this.modalStack = this.modalStack.filter(id => id !== modalId);
        this.activeModals.delete(modalId);

        // Hide modal
        this.hideModal(modal, animation);

        // Dispatch event
        window.dispatchEvent(new CustomEvent('modalHidden', {
            detail: { modalId, modal }
        }));

        return true;
    }

    /**
     * Close top modal in stack
     */
    closeTopModal() {
        if (this.modalStack.length > 0) {
            const topModal = this.modalStack[this.modalStack.length - 1];
            this.hide(topModal);
        }
    }

    /**
     * Close all modals
     */
    closeAll() {
        const modals = [...this.activeModals];
        modals.forEach(modalId => this.hide(modalId, false));
    }

    /**
     * Create modal overlay
     */
    createOverlay() {
        const overlay = document.createElement('div');
        overlay.className = 'modal-overlay';
        document.body.appendChild(overlay);
        return overlay;
    }

    /**
     * Show modal with animation
     */
    showModal(modal, overlay, options) {
        // Create container if it doesn't exist
        let container = document.querySelector('.modal-container');
        if (!container) {
            container = document.createElement('div');
            container.className = 'modal-container';
            document.body.appendChild(container);
        }

        // Move modal to container
        container.appendChild(modal);

        // Show overlay
        if (options.backdrop) {
            overlay.classList.add('show');
        }

        // Show container and modal
        container.classList.add('show');
        modal.classList.add('show');

        // Prevent body scroll
        document.body.style.overflow = 'hidden';
    }

    /**
     * Hide modal with animation
     */
    hideModal(modal, animation) {
        const container = document.querySelector('.modal-container');
        const overlay = document.querySelector('.modal-overlay');

        // Hide modal
        modal.classList.remove('show');

        if (animation) {
            // Wait for animation to complete
            setTimeout(() => {
                this.cleanupModal(modal, container, overlay);
            }, 300);
        } else {
            this.cleanupModal(modal, container, overlay);
        }
    }

    /**
     * Cleanup modal elements
     */
    cleanupModal(modal, container, overlay) {
        // Move modal back to original position
        const originalParent = modal.getAttribute('data-original-parent');
        if (originalParent) {
            document.getElementById(originalParent).appendChild(modal);
        }

        // Hide container if no more modals
        if (this.activeModals.size === 0) {
            container.classList.remove('show');
            if (overlay) {
                overlay.classList.remove('show');
            }
            document.body.style.overflow = '';
        }
    }

    /**
     * Focus modal for accessibility
     */
    focusModal(modal) {
        const focusableElements = modal.querySelectorAll(
            'button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])'
        );
        
        if (focusableElements.length > 0) {
            focusableElements[0].focus();
        }
    }

    /**
     * Get active modals
     */
    getActiveModals() {
        return Array.from(this.activeModals);
    }
}

/**
 * Component Library - Reusable UI Components
 */
class ComponentLibrary {
    constructor() {
        this.components = new Map();
        this.init();
    }

    /**
     * Initialize component library
     */
    init() {
        this.registerDefaultComponents();
        console.log('🧩 Component Library initialized');
    }

    /**
     * Register default components
     */
    registerDefaultComponents() {
        // Token Card Component
        this.register('token-card', {
            template: (data) => this.createTokenCard(data),
            styles: this.getTokenCardStyles()
        });

        // Alert Component
        this.register('alert', {
            template: (data) => this.createAlert(data),
            styles: this.getAlertStyles()
        });

        // Button Component
        this.register('button', {
            template: (data) => this.createButton(data),
            styles: this.getButtonStyles()
        });
    }

    /**
     * Register a component
     */
    register(name, component) {
        this.components.set(name, component);
    }

    /**
     * Create component instance
     */
    create(name, data, container) {
        const component = this.components.get(name);
        if (!component) {
            console.error(`Component not found: ${name}`);
            return null;
        }

        const element = document.createElement('div');
        element.innerHTML = component.template(data);
        element.className = `component-${name}`;

        if (container) {
            container.appendChild(element);
        }

        return element.firstElementChild;
    }

    /**
     * Create token card
     */
    createTokenCard(data) {
        const {
            id,
            name,
            description,
            image,
            status = 'active',
            meta = {},
            actions = []
        } = data;

        return `
            <div class="token-card" data-token-id="${id}">
                <div class="token-card-header">
                    <h3 class="token-card-title">${name}</h3>
                    <span class="token-card-status ${status}">${status}</span>
                </div>
                
                ${image ? `
                    <img src="${image}" alt="${name}" class="token-card-image">
                ` : `
                    <div class="token-card-placeholder">
                        <i class="fas fa-crosshairs"></i>
                    </div>
                `}
                
                <div class="token-card-content">
                    <p class="token-card-description">${description || ''}</p>
                    
                    <div class="token-card-meta">
                        ${Object.entries(meta).map(([key, value]) => `
                            <div class="token-card-meta-item">
                                <i class="fas fa-${this.getMetaIcon(key)}"></i>
                                <span>${value}</span>
                            </div>
                        `).join('')}
                    </div>
                </div>
                
                ${actions.length > 0 ? `
                    <div class="token-card-actions">
                        ${actions.map(action => `
                            <button class="btn ${action.variant || 'primary'}" 
                                    onclick="${action.onclick || ''}">
                                ${action.icon ? `<i class="${action.icon}"></i>` : ''}
                                ${action.text}
                            </button>
                        `).join('')}
                    </div>
                ` : ''}
            </div>
        `;
    }

    /**
     * Create alert
     */
    createAlert(data) {
        const {
            type = 'info',
            title,
            message,
            closable = true,
            icon = true
        } = data;

        const icons = {
            success: 'fas fa-check-circle',
            warning: 'fas fa-exclamation-triangle',
            error: 'fas fa-times-circle',
            info: 'fas fa-info-circle'
        };

        return `
            <div class="alert alert-${type}" role="alert">
                ${icon ? `<i class="${icons[type]}"></i>` : ''}
                <div class="alert-content">
                    ${title ? `<strong>${title}</strong>` : ''}
                    ${message ? `<span>${message}</span>` : ''}
                </div>
                ${closable ? `
                    <button type="button" class="alert-close" onclick="this.parentElement.remove()">
                        <i class="fas fa-times"></i>
                    </button>
                ` : ''}
            </div>
        `;
    }

    /**
     * Create button
     */
    createButton(data) {
        const {
            text,
            variant = 'primary',
            size = 'md',
            icon,
            onclick,
            disabled = false,
            loading = false
        } = data;

        return `
            <button class="btn btn-${variant} btn-${size}" 
                    ${disabled ? 'disabled' : ''}
                    ${onclick ? `onclick="${onclick}"` : ''}>
                ${loading ? '<i class="fas fa-spinner fa-spin"></i>' : ''}
                ${icon && !loading ? `<i class="${icon}"></i>` : ''}
                ${text}
            </button>
        `;
    }

    /**
     * Get meta icon for token card
     */
    getMetaIcon(key) {
        const iconMap = {
            'type': 'tag',
            'team': 'users',
            'status': 'circle',
            'location': 'map-marker-alt',
            'strength': 'shield-alt',
            'range': 'crosshairs'
        };
        return iconMap[key] || 'info-circle';
    }

    /**
     * Get component styles (placeholder)
     */
    getTokenCardStyles() {
        return '';
    }

    getAlertStyles() {
        return '';
    }

    getButtonStyles() {
        return '';
    }
}

/**
 * Utility Functions
 */
const ThemeUtils = {
    /**
     * Get CSS custom property value
     */
    getCSSVar(property) {
        return getComputedStyle(document.documentElement)
            .getPropertyValue(property)
            .trim();
    },

    /**
     * Set CSS custom property value
     */
    setCSSVar(property, value) {
        document.documentElement.style.setProperty(property, value);
    },

    /**
     * Create toast notification
     */
    toast(message, type = 'info', duration = 3000) {
        const toast = document.createElement('div');
        toast.className = `toast toast-${type}`;
        toast.textContent = message;
        
        // Style the toast
        Object.assign(toast.style, {
            position: 'fixed',
            top: '20px',
            right: '20px',
            padding: '12px 20px',
            borderRadius: '8px',
            color: 'white',
            fontWeight: '500',
            zIndex: '9999',
            transform: 'translateX(100%)',
            transition: 'transform 0.3s ease'
        });

        // Set background color based on type
        const colors = {
            success: '#22c55e',
            error: '#ef4444',
            warning: '#f59e0b',
            info: '#3b82f6'
        };
        toast.style.backgroundColor = colors[type] || colors.info;

        document.body.appendChild(toast);

        // Animate in
        setTimeout(() => {
            toast.style.transform = 'translateX(0)';
        }, 100);

        // Auto remove
        setTimeout(() => {
            toast.style.transform = 'translateX(100%)';
            setTimeout(() => {
                if (toast.parentNode) {
                    toast.parentNode.removeChild(toast);
                }
            }, 300);
        }, duration);
    }
};

// Initialize systems when DOM is ready
document.addEventListener('DOMContentLoaded', () => {
    // Initialize theme system
    window.themeSystem = new ThemeSystem();
    
    // Initialize modal system
    window.modalSystem = new ModalSystem();
    
    // Initialize component library
    window.componentLibrary = new ComponentLibrary();
    
    // Expose utilities globally
    window.ThemeUtils = ThemeUtils;
    
    console.log('🎨 All theme systems initialized successfully');
});

// Export for module systems
if (typeof module !== 'undefined' && module.exports) {
    module.exports = {
        ThemeSystem,
        ModalSystem,
        ComponentLibrary,
        ThemeUtils
    };
}
