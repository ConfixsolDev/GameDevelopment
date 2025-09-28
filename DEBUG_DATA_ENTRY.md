# Debug Data Entry Function

## Quick Tests to Run in Browser Console

After the page loads, open browser console (F12) and run these commands:

### Test 1: Check if function exists
```javascript
console.log('openDataEntry exists:', typeof window.openDataEntry);
console.log('Function:', window.openDataEntry);
```

### Test 2: Try to call the function manually
```javascript
if (typeof window.openDataEntry === 'function') {
    console.log('Calling openDataEntry manually...');
    window.openDataEntry();
} else {
    console.error('openDataEntry not available');
}
```

### Test 3: Check all global functions
```javascript
// List all functions on window that contain 'data' or 'entry'
Object.keys(window).filter(key => 
    key.toLowerCase().includes('data') || 
    key.toLowerCase().includes('entry')
).forEach(key => {
    console.log(key + ':', typeof window[key]);
});
```

### Test 4: Manual function registration (if needed)
```javascript
// If the function isn't available, you can manually register it
window.openDataEntry = function() {
    console.log('Manual openDataEntry called');
    alert('Data Entry function manually triggered - this means the script loaded but function registration failed');
};
```

## Expected Console Output

When working correctly, you should see:
- ✅ `🎯 openDataEntry function registered globally`
- ✅ `🔍 Testing function availability: function`
- ✅ `🔒 Function locked into window object`
- ✅ `✅ openDataEntry function is properly registered and callable`

## If Function Still Not Available

If the function still isn't available after running the tests, it means there's a script loading order issue. The temporary fix is:

1. **Open browser console**
2. **Run this command**:
```javascript
window.openDataEntry = function() {
    console.log('Temporary openDataEntry function');
    // Load the data entry modal manually
    if (typeof openModal === 'function') {
        // Try to find and click a token or show data entry modal
        alert('Data Entry functionality temporarily enabled. The master script will be fixed.');
    }
};
```

This will at least make the button work while we fix the script loading issue.
