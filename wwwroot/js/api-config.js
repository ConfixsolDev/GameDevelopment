/**
 * Centralized API Configuration
 * All API endpoints are defined here to avoid hardcoded URLs in views
 */
window.API_CONFIG = {
    // Admin Token Management APIs
    AdminToken: {
        baseUrl: '/AdminToken',
        endpoints: {
            groups: '/groups',
            createGroup: '/create-group',
            updateGroup: '/update-group',
            deleteGroup: '/delete-group',
            create: '/create',
            teams: '/teams',
            assignGroupToTeam: '/assign-group-to-team',
            groupAssignments: '/group-assignments',
            removeAssignment: '/remove-assignment',
            teamAssignments: '/team-assignments'
        }
    },
    
    // Team Management APIs
    TeamManagement: {
        baseUrl: '/TeamManagement',
        endpoints: {
            teams: '/teams',
            createTeam: '/create-team',
            updateTeam: '/update-team',
            deleteTeam: '/delete-team',
            assignUserToTeam: '/assign-user-to-team',
            removeUserFromTeam: '/remove-user-from-team',
            teamMembers: '/team-members',
            users: '/users',
            updateUserRole: '/update-user-role'
        }
    },
    
    // Game Management APIs
    GameManagement: {
        baseUrl: '/GameManagement',
        endpoints: {
            startSession: '/start-session',
            endSession: '/end-session',
            activeSessions: '/active-sessions',
            freeTokens: '/free-tokens',
            bindToken: '/bind-token',
            unbindToken: '/unbind-token',
            searchFreeTokens: '/search-free-tokens',
            tokenDetails: '/token-details',
            tokenSignature: '/token-signature',
            assignToken: '/assign-token',
            deleteFreeToken: '/delete-free-token',
            sessionDetails: '/session-details',
            sessionBindings: '/session-bindings',
            bindTokens: '/bind-tokens',
            removeBinding: '/remove-binding'
        }
    },
    
    // Admin Game Management APIs
    AdminGameManagement: {
        baseUrl: '/api/admin/GameManagement',
        endpoints: {
            activeSessions: '/active-sessions',
            session: '/session',
            tokenBindings: '/token-bindings',
            bindToken: '/bind-token',
            unbindToken: '/unbind-token'
        }
    },
    
    // Token System APIs
    TokenSystem: {
        baseUrl: '/api/token',
        endpoints: {
            train: '/train',
            identify: '/identify',
            validate: '/validate'
        }
    },
    
    // Unified Token APIs
    UnifiedToken: {
        baseUrl: '/api/UnifiedToken',
        endpoints: {
            identify: '/identify',
            save: '/save',
            teamTokens: '/team-tokens'
        }
    },
    
    // Map Marker APIs
    MapMarker: {
        baseUrl: '/api/MapMarker',
        endpoints: {
            byToken: '/by-token',
            get: '/',
            update: '/',
            delete: '/',
            deleteByToken: '/by-token',
            bulk: '/bulk'
        }
    }
};

/**
 * Helper function to build full API URLs
 * @param {string} controller - Controller name (e.g., 'AdminToken')
 * @param {string} endpoint - Endpoint name (e.g., 'groups')
 * @returns {string} Full API URL
 */
window.getApiUrl = function(controller, endpoint) {
    const config = window.API_CONFIG[controller];
    if (!config) {
        console.error(`Controller '${controller}' not found in API_CONFIG`);
        return '';
    }
    
    const endpointPath = config.endpoints[endpoint];
    if (!endpointPath) {
        console.error(`Endpoint '${endpoint}' not found for controller '${controller}'`);
        return '';
    }
    
    return config.baseUrl + endpointPath;
};

/**
 * Helper function to build API URLs with parameters
 * @param {string} controller - Controller name
 * @param {string} endpoint - Endpoint name
 * @param {Object} params - Parameters to replace in the URL
 * @returns {string} Full API URL with parameters
 */
window.getApiUrlWithParams = function(controller, endpoint, params = {}) {
    let url = window.getApiUrl(controller, endpoint);
    
    // Replace parameters in the URL (e.g., {id} with actual value)
    Object.keys(params).forEach(key => {
        url = url.replace(`{${key}}`, params[key]);
    });
    
    return url;
};

/**
 * Centralized Alert System
 * Prevents duplicate alerts and provides consistent error handling
 */
window.AlertSystem = {
    // Track active alerts to prevent duplicates
    activeAlerts: new Set(),
    
    /**
     * Show an alert message
     * @param {string} message - Alert message
     * @param {string} type - Alert type (success, danger, warning, info)
     * @param {number} duration - Auto-dismiss duration in milliseconds (default: 5000)
     */
    show: function(message, type = 'info', duration = 5000) {
        // Create unique key for this alert
        const alertKey = `${type}-${message}`;
        
        // Prevent duplicate alerts
        if (this.activeAlerts.has(alertKey)) {
            console.log('Duplicate alert prevented:', alertKey);
            return;
        }
        
        // Add to active alerts
        this.activeAlerts.add(alertKey);
        console.log('New alert added:', alertKey);
        
        // Create alert HTML
        const alertDiv = `
            <div class="alert alert-${type} alert-dismissible fade show" role="alert" data-alert-key="${alertKey}">
                ${message}
                <button type="button" class="btn-close" data-bs-dismiss="alert" onclick="AlertSystem.remove('${alertKey}')"></button>
            </div>
        `;
        
        // Prepend to container
        $('.container-fluid').prepend(alertDiv);
        
        // Auto-remove after duration
        if (duration > 0) {
            setTimeout(() => {
                this.remove(alertKey);
            }, duration);
        }
    },
    
    /**
     * Remove an alert
     * @param {string} alertKey - Alert key to remove
     */
    remove: function(alertKey) {
        this.activeAlerts.delete(alertKey);
        $(`[data-alert-key="${alertKey}"]`).fadeOut(300, function() {
            $(this).remove();
        });
    },
    
    /**
     * Clear all alerts
     */
    clear: function() {
        this.activeAlerts.clear();
        $('.alert[data-alert-key]').fadeOut(300, function() {
            $(this).remove();
        });
    },
    
    // Convenience methods
    success: function(message, duration) { this.show(message, 'success', duration); },
    error: function(message, duration) { this.show(message, 'danger', duration); },
    warning: function(message, duration) { this.show(message, 'warning', duration); },
    info: function(message, duration) { this.show(message, 'info', duration); }
};

// Global shortcut for backward compatibility
window.showAlert = function(message, type, duration) {
    window.AlertSystem.show(message, type, duration);
};

// Helper function for JSON POST requests
window.postJson = function(url, data) {
    return $.ajax({
        url: url,
        type: 'POST',
        data: JSON.stringify(data),
        contentType: 'application/json',
        dataType: 'json'
    });
};

// Debug function to test API URL generation
window.debugApiUrl = function(controller, endpoint) {
    const url = getApiUrl(controller, endpoint);
    console.log(`API URL for ${controller}.${endpoint}:`, url);
    return url;
};

// Session Management - Clear session when browser closes
window.addEventListener('beforeunload', function(event) {
    // Clear session data when browser is closing
    if (window.API_CONFIG && window.API_CONFIG.baseUrl) {
        // Make a synchronous request to clear session
        try {
            const xhr = new XMLHttpRequest();
            xhr.open('POST', '/Account/ClearSession', false); // Synchronous request
            xhr.send();
        } catch (e) {
            // Ignore errors during browser close
        }
    }
});

// Also clear session on page unload (for cases where beforeunload doesn't fire)
window.addEventListener('unload', function(event) {
    try {
        const xhr = new XMLHttpRequest();
        xhr.open('POST', '/Account/ClearSession', false);
        xhr.send();
    } catch (e) {
        // Ignore errors
    }
});

// Session timeout handler - redirect to login after 30 minutes of inactivity
let sessionTimeout;
const SESSION_TIMEOUT_MINUTES = 30;

function resetSessionTimeout() {
    clearTimeout(sessionTimeout);
    sessionTimeout = setTimeout(function() {
        // Session expired, redirect to login
        window.location.href = '/Account/Login';
    }, SESSION_TIMEOUT_MINUTES * 60 * 1000);
}

// Reset timeout on user activity
['mousedown', 'mousemove', 'keypress', 'scroll', 'touchstart'].forEach(function(event) {
    document.addEventListener(event, resetSessionTimeout, true);
});

// Initialize session timeout
resetSessionTimeout();
