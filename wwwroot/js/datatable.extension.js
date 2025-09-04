function ApplySearch(searchForm, datatable) {
    try {
        
        for (var index = 0; index < datatable.columns()[0].length;index++)
        {
            var columnName = datatable.column(index).dataSrc();

            if (columnName != null) {
                columnName = (new String(columnName)).replace('.', '\\.');
                
                if ($(searchForm).find(`[data-column='${columnName}']`).length == 1) {
                    datatable.column(index).search($(searchForm).find(`[data-column='${columnName}']`).val(),
                        ($(searchForm).find(`[data-column='${columnName}']`).data('filter') != null || $(searchForm).find(`[data-column='${columnName}']`).data('filter') == 'like'));
                }
            }
        };
    }
    catch (e) {
        alert(e);
    }
    return datatable;

}