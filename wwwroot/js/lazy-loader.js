/**
 * Lazy Loading Utility for GamePlay Partial Views
 * Handles lazy loading of HTML partials with caching
 */
class LazyLoader {
    constructor() {
        this.cache = new Map();
        this.loadingPromises = new Map();
    }

    /**
     * Load a partial view with lazy loading and caching
     * @param {string} partialName - Name of the partial view
     * @param {string} targetSelector - CSS selector for the target container
     * @param {Object} options - Loading options
     */
    async loadPartial(partialName, targetSelector, options = {}) {
        const {
            showLoading = true,
            cache = true,
            onLoaded = null,
            onError = null
        } = options;

        try {
            // Show loading indicator
            if (showLoading) {
                this.showLoadingIndicator(targetSelector);
            }

            // Check cache first
            if (cache && this.cache.has(partialName)) {
                console.log(`💾 Loading ${partialName} from cache`);
                const content = this.cache.get(partialName);
                this.insertContent(targetSelector, content);
                if (onLoaded) onLoaded();
                return;
            }

            console.log(`🔍 ${partialName} not in cache, will fetch from server`);

            // Check if already loading
            if (this.loadingPromises.has(partialName)) {
                await this.loadingPromises.get(partialName);
                const content = this.cache.get(partialName);
                this.insertContent(targetSelector, content);
                if (onLoaded) onLoaded();
                return;
            }

            // Start loading
            const loadingPromise = this.fetchPartial(partialName);
            this.loadingPromises.set(partialName, loadingPromise);

            const content = await loadingPromise;
            
            // Cache the content
            if (cache) {
                this.cache.set(partialName, content);
            }

            // Insert content
            this.insertContent(targetSelector, content);
            
            // Clean up loading promise
            this.loadingPromises.delete(partialName);

            if (onLoaded) onLoaded();

        } catch (error) {
            console.error(`Failed to load partial ${partialName}:`, error);
            this.showErrorMessage(targetSelector, `Failed to load ${partialName}`);
            
            // Clean up loading promise
            this.loadingPromises.delete(partialName);

            if (onError) onError(error);
        }
    }

    /**
     * Fetch partial content from server
     */
    async fetchPartial(partialName) {
        const url = `/GamePlay/LoadPartial?partialName=${encodeURIComponent(partialName)}`;
        console.log(`🌐 Making request to: ${url}`);
        
        const response = await fetch(url, {
            method: 'GET',
            headers: {
                'X-Requested-With': 'XMLHttpRequest'
            }
        });

        console.log(`📡 Response status: ${response.status} ${response.statusText}`);

        if (!response.ok) {
            throw new Error(`HTTP ${response.status}: ${response.statusText}`);
        }

        const content = await response.text();
        console.log(`📄 Received content length: ${content.length} characters`);
        return content;
    }

    /**
     * Show loading indicator
     */
    showLoadingIndicator(targetSelector) {
        const container = document.querySelector(targetSelector);
        if (container) {
            container.innerHTML = `
                <div class="lazy-loading-indicator">
                    <div class="loading-spinner">
                        <i class="fas fa-spinner fa-spin"></i>
                    </div>
                    <p>Loading...</p>
                </div>
            `;
        }
    }

    /**
     * Show error message
     */
    showErrorMessage(targetSelector, message) {
        const container = document.querySelector(targetSelector);
        if (container) {
            container.innerHTML = `
                <div class="lazy-loading-error">
                    <div class="error-icon">
                        <i class="fas fa-exclamation-triangle"></i>
                    </div>
                    <p>${message}</p>
                    <button class="btn btn-sm btn-primary" onclick="location.reload()">
                        <i class="fas fa-refresh"></i> Retry
                    </button>
                </div>
            `;
        }
    }

    /**
     * Insert content into target container
     */
    insertContent(targetSelector, content) {
        const container = document.querySelector(targetSelector);
        if (container) {
            // For modals, append instead of replace to preserve multiple modals
            if (targetSelector.includes('modalsContainer')) {
                container.insertAdjacentHTML('beforeend', content);
            } else {
                container.innerHTML = content;
            }
            
            // Trigger custom event for content loaded
            const event = new CustomEvent('partialLoaded', {
                detail: { selector: targetSelector, content: content }
            });
            container.dispatchEvent(event);
            
            console.log(`Content inserted into ${targetSelector}`);
        } else {
            console.warn(`Target container not found: ${targetSelector}`);
        }
    }

    /**
     * Load multiple partials in parallel
     */
    async loadPartials(partials) {
        const promises = partials.map(partial => 
            this.loadPartial(partial.name, partial.target, partial.options)
        );
        
        return Promise.allSettled(promises);
    }

    /**
     * Preload partials in the background
     */
    async preloadPartials(partialNames) {
        const promises = partialNames.map(async (partialName) => {
            if (!this.cache.has(partialName)) {
                try {
                    const content = await this.fetchPartial(partialName);
                    this.cache.set(partialName, content);
                } catch (error) {
                    console.warn(`Failed to preload partial ${partialName}:`, error);
                }
            }
        });

        return Promise.allSettled(promises);
    }

    /**
     * Clear cache
     */
    clearCache() {
        this.cache.clear();
    }

    /**
     * Get cache size
     */
    getCacheSize() {
        return this.cache.size;
    }
}

// Global instance
window.lazyLoader = new LazyLoader();

// CSS for loading indicators
const lazyLoaderCSS = `
    .lazy-loading-indicator,
    .lazy-loading-error {
        display: flex;
        flex-direction: column;
        align-items: center;
        justify-content: center;
        padding: 20px;
        text-align: center;
        min-height: 100px;
    }

    .loading-spinner {
        font-size: 24px;
        color: #007bff;
        margin-bottom: 10px;
    }

    .error-icon {
        font-size: 24px;
        color: #dc3545;
        margin-bottom: 10px;
    }

    .lazy-loading-indicator p,
    .lazy-loading-error p {
        margin: 5px 0;
        color: #6c757d;
    }
`;

// Inject CSS
const style = document.createElement('style');
style.textContent = lazyLoaderCSS;
document.head.appendChild(style);
