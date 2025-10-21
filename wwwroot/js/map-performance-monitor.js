/**
 * Map Performance Monitor
 * Add this to console to monitor tile loading performance
 * 
 * Usage in Browser Console:
 * 1. Open DevTools (F12)
 * 2. Paste this entire file
 * 3. Call: startMonitoring()
 * 4. Load/switch map
 * 5. Call: getReport()
 */

class MapPerformanceMonitor {
    constructor() {
        this.tileStats = {
            total: 0,
            loaded: 0,
            failed: 0,
            cached: 0,
            times: [],
            startTime: null,
            firstTileTime: null,
            lastTileTime: null
        };
        
        this.isMonitoring = false;
        this.observer = null;
    }
    
    /**
     * Start monitoring map tile performance
     */
    start() {
        console.log('🔍 Starting Map Performance Monitor...');
        this.reset();
        this.isMonitoring = true;
        this.tileStats.startTime = performance.now();
        
        // Monitor tile requests via Performance API
        if (window.PerformanceObserver) {
            this.observer = new PerformanceObserver((list) => {
                for (const entry of list.getEntries()) {
                    if (entry.initiatorType === 'img' || entry.initiatorType === 'fetch') {
                        if (entry.name.includes('/tiles/') || entry.name.includes('.png') || entry.name.includes('.jpg')) {
                            this.recordTile(entry);
                        }
                    }
                }
            });
            
            try {
                this.observer.observe({ entryTypes: ['resource'] });
            } catch (e) {
                console.warn('Performance Observer not fully supported:', e);
            }
        }
        
        // Monitor Leaflet tile events if map exists
        if (window.gameMap) {
            this.attachLeafletMonitoring();
        }
        
        console.log('✅ Monitoring started. Switch/load a map to collect data.');
    }
    
    /**
     * Attach to Leaflet map events
     */
    attachLeafletMonitoring() {
        const map = window.gameMap;
        
        map.on('tileloadstart', (e) => {
            this.tileStats.total++;
        });
        
        map.on('tileload', (e) => {
            this.tileStats.loaded++;
            const now = performance.now();
            
            if (!this.tileStats.firstTileTime) {
                this.tileStats.firstTileTime = now;
                console.log(`⚡ First tile loaded: ${(now - this.tileStats.startTime).toFixed(0)}ms`);
            }
            
            this.tileStats.lastTileTime = now;
        });
        
        map.on('tileerror', (e) => {
            this.tileStats.failed++;
            console.warn('❌ Tile failed to load:', e.tile.src);
        });
    }
    
    /**
     * Record tile performance from Performance API
     */
    recordTile(entry) {
        const duration = entry.duration;
        this.tileStats.times.push(duration);
        
        // Check if from cache
        if (entry.transferSize === 0 && entry.decodedBodySize > 0) {
            this.tileStats.cached++;
        }
    }
    
    /**
     * Stop monitoring
     */
    stop() {
        console.log('🛑 Stopping Map Performance Monitor...');
        this.isMonitoring = false;
        
        if (this.observer) {
            this.observer.disconnect();
        }
        
        // Remove Leaflet listeners
        if (window.gameMap) {
            window.gameMap.off('tileloadstart');
            window.gameMap.off('tileload');
            window.gameMap.off('tileerror');
        }
        
        console.log('✅ Monitoring stopped.');
    }
    
    /**
     * Reset statistics
     */
    reset() {
        this.tileStats = {
            total: 0,
            loaded: 0,
            failed: 0,
            cached: 0,
            times: [],
            startTime: null,
            firstTileTime: null,
            lastTileTime: null
        };
    }
    
    /**
     * Get performance report
     */
    getReport() {
        const stats = this.tileStats;
        
        if (stats.times.length === 0) {
            console.log('⚠️ No tile data collected yet. Load or switch a map first.');
            return null;
        }
        
        const times = stats.times;
        const avgTime = times.reduce((a, b) => a + b, 0) / times.length;
        const minTime = Math.min(...times);
        const maxTime = Math.max(...times);
        const medianTime = this.getMedian(times);
        
        const totalTime = stats.lastTileTime - stats.startTime;
        const timeToFirstTile = stats.firstTileTime - stats.startTime;
        
        const report = {
            'Total Tiles Requested': stats.total,
            'Tiles Loaded': stats.loaded,
            'Tiles Failed': stats.failed,
            'Tiles from Cache': stats.cached,
            'Cache Hit Rate': `${((stats.cached / stats.loaded) * 100).toFixed(1)}%`,
            '---': '---',
            'Time to First Tile (TTFT)': `${timeToFirstTile.toFixed(0)}ms`,
            'Total Load Time': `${totalTime.toFixed(0)}ms`,
            'Average Tile Load': `${avgTime.toFixed(0)}ms`,
            'Median Tile Load': `${medianTime.toFixed(0)}ms`,
            'Fastest Tile': `${minTime.toFixed(0)}ms`,
            'Slowest Tile': `${maxTime.toFixed(0)}ms`,
            '----': '----',
            'Performance Rating': this.getRating(avgTime, timeToFirstTile)
        };
        
        console.log('📊 Map Performance Report:');
        console.table(report);
        
        return report;
    }
    
    /**
     * Calculate median
     */
    getMedian(arr) {
        const sorted = [...arr].sort((a, b) => a - b);
        const mid = Math.floor(sorted.length / 2);
        return sorted.length % 2 === 0 
            ? (sorted[mid - 1] + sorted[mid]) / 2 
            : sorted[mid];
    }
    
    /**
     * Get performance rating
     */
    getRating(avgTime, ttft) {
        if (avgTime < 50 && ttft < 200) return '🟢 EXCELLENT (Optimized)';
        if (avgTime < 100 && ttft < 500) return '🟡 GOOD (Acceptable)';
        if (avgTime < 200 && ttft < 1000) return '🟠 FAIR (Needs Improvement)';
        return '🔴 POOR (Needs Optimization)';
    }
    
    /**
     * Continuous monitoring mode
     */
    startContinuous(intervalMs = 5000) {
        this.start();
        
        const intervalId = setInterval(() => {
            if (!this.isMonitoring) {
                clearInterval(intervalId);
                return;
            }
            
            console.log(`\n⏱️ Performance Update (${new Date().toLocaleTimeString()}):`);
            this.getReport();
        }, intervalMs);
        
        console.log(`🔄 Continuous monitoring started (updates every ${intervalMs/1000}s)`);
        return intervalId;
    }
}

// Create global instance
window.mapPerformanceMonitor = new MapPerformanceMonitor();

// Convenience functions
window.startMonitoring = () => window.mapPerformanceMonitor.start();
window.stopMonitoring = () => window.mapPerformanceMonitor.stop();
window.getReport = () => window.mapPerformanceMonitor.getReport();
window.resetMonitoring = () => window.mapPerformanceMonitor.reset();
window.startContinuousMonitoring = (interval) => window.mapPerformanceMonitor.startContinuous(interval);

console.log(`
╔════════════════════════════════════════════════════════════╗
║         Map Performance Monitor Loaded                      ║
╠════════════════════════════════════════════════════════════╣
║  Commands:                                                  ║
║  • startMonitoring()        - Start monitoring              ║
║  • stopMonitoring()         - Stop monitoring               ║
║  • getReport()              - Get performance report        ║
║  • resetMonitoring()        - Reset statistics              ║
║  • startContinuousMonitoring(5000) - Auto-report every 5s   ║
╚════════════════════════════════════════════════════════════╝
`);

