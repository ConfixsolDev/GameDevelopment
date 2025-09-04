// Function to load SubClient/SubSupplier options based on selected Client/Supplier
function loadSubClientOptions(parentId, targetElement, selectedValue = null) {
    if (!parentId || parentId === '') {
        // If no parent is selected, clear the dropdown
        $(targetElement).empty();
        $(targetElement).append('<option value="">-- Select --</option>');
        return;
    }

    // Determine endpoint based on target element
    var url = '/SubClients/GetSubClientOptions';
    if (targetElement === '#SubSupplierId' || targetElement === '#SubSupplier') {
        url = '/SubClients/GetSubSupplierOptions';
    }

    console.log('Loading options from:', url, 'for parent:', parentId, 'target:', targetElement);

    // Make AJAX call to get options
    $.ajax({
        url: url,
        type: 'GET',
        data: { clientId: parentId, supplierId: parentId },
        success: function (data) {
            console.log('Received data:', data);
            // Clear existing options
            $(targetElement).empty();
            $(targetElement).append('<option value="">-- Select --</option>');
            
            // Add new options
            $.each(data, function (i, item) {
                var selected = (selectedValue && item.id === selectedValue) ? 'selected' : '';
                $(targetElement).append('<option value="' + item.id + '" ' + selected + '>' + item.name + '</option>');
            });
        },
        error: function (xhr, status, error) {
            console.error('Failed to load options from', url, 'Error:', error);
        }
    });
}