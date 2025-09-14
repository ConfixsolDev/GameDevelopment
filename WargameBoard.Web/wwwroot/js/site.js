// WargameBoard JavaScript Helpers

function selectHex(el){
  document.querySelectorAll('.hex').forEach(x=>x.classList.remove('selected'));
  el.classList.add('selected');
  const hexId = el.dataset.hexid;
    load(`/Map/Map/EditHex/${hexId}`, '#hexEditHost');
    load(`/Map/Map/ListHexFeatures?hexId=${hexId}`, '#hexFeaturesHost');
}

async function load(url, hostSel){ 
  const r = await fetch(url); 
  document.querySelector(hostSel).innerHTML = await r.text(); 
}

async function postForm(form, hostSel){
  const r = await fetch(form.action, { method:'POST', body:new FormData(form) });
  const txt = await r.text();
  document.querySelector(hostSel).innerHTML = txt;
}

// Real-time form submission with AJAX
async function submitFormRealTime(form, successCallback, errorCallback) {
    try {
        const formData = new FormData(form);
        const response = await fetch(form.action, {
            method: 'POST',
            body: formData
        });
        
        if (response.ok) {
            const result = await response.text();
            if (successCallback) successCallback(result);
        } else {
            if (errorCallback) errorCallback('Form submission failed');
        }
    } catch (error) {
        console.error('Form submission error:', error);
        if (errorCallback) errorCallback('Network error occurred');
    }
}

// Enhanced AJAX form handling
function setupRealTimeForms() {
    document.querySelectorAll('form[data-realtime="true"]').forEach(form => {
        form.addEventListener('submit', function(e) {
            e.preventDefault();
            
            const submitBtn = form.querySelector('button[type="submit"]');
            const originalText = submitBtn.textContent;
            
            // Show loading state
            submitBtn.disabled = true;
            submitBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Processing...';
            
            submitFormRealTime(form, 
                function(result) {
                    // Success
                    submitBtn.disabled = false;
                    submitBtn.textContent = originalText;
                    showNotification('Operation completed successfully', 'success');
                    
                    // Update UI if needed
                    if (form.dataset.updateTarget) {
                        document.querySelector(form.dataset.updateTarget).innerHTML = result;
                    }
                },
                function(error) {
                    // Error
                    submitBtn.disabled = false;
                    submitBtn.textContent = originalText;
                    showNotification(error, 'error');
                }
            );
        });
    });
}

// Real-time notification system
function showNotification(message, type = 'info') {
    const alertClass = type === 'error' ? 'alert-danger' : 
                      type === 'warning' ? 'alert-warning' : 
                      type === 'success' ? 'alert-success' : 'alert-info';
    
    const notification = document.createElement('div');
    notification.className = `alert ${alertClass} alert-dismissible fade show position-fixed`;
    notification.style.cssText = 'top: 20px; right: 20px; z-index: 9999; min-width: 300px;';
    notification.innerHTML = `
        ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    `;
    
    document.body.appendChild(notification);
    
    // Auto-remove after 3 seconds
    setTimeout(() => {
        if (notification.parentNode) {
            notification.remove();
        }
    }, 3000);
}

// Real-time data refresh
function refreshData(url, targetSelector) {
    fetch(url)
        .then(response => response.text())
        .then(html => {
            document.querySelector(targetSelector).innerHTML = html;
        })
        .catch(error => {
            console.error('Error refreshing data:', error);
            showNotification('Failed to refresh data', 'error');
        });
}

// Auto-refresh functionality
function setupAutoRefresh(interval = 30000) {
    setInterval(() => {
        // Refresh any elements with data-auto-refresh attribute
        document.querySelectorAll('[data-auto-refresh]').forEach(element => {
            const url = element.dataset.autoRefresh;
            const target = element.dataset.target || element.id;
            if (url && target) {
                refreshData(url, `#${target}`);
            }
        });
    }, interval);
}

// Initialize tooltips on page load
document.addEventListener('DOMContentLoaded', function() {
    // Initialize Bootstrap tooltips
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });
    
    // Initialize Bootstrap popovers
    var popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'));
    var popoverList = popoverTriggerList.map(function (popoverTriggerEl) {
        return new bootstrap.Popover(popoverTriggerEl);
    });
    
    // Setup real-time forms
    setupRealTimeForms();
    
    // Setup auto-refresh
    setupAutoRefresh();
    
    // Initialize any real-time components
    if (typeof initializeRealTimeComponents === 'function') {
        initializeRealTimeComponents();
    }
});
