var datatable_page_length = 25;
var datatable_pageing_type = "simple_numbers";
var datatable_info = true;
var datatable_change_length = true;
var datatable_processing = true;
var datatable_length_menu = [[10, 25, 50], [10, 25, 50]];
//var datatable_positioning = "<'col-sm-12 col-md-12'f>" +
//	"<'col-12'tr>" +
//	"<'container'<'row'<'col-sm-12 col-md-3'l><'col-sm-12 col-md-3'i><'col-sm-12 col-md-6'p>>>";
var DataTable_SearchLabel = "_INPUT_";
var DataTable_SearchPlaceHolder = "Search...";
var datatable_processingHTML = "<i class='fa fa-spinner fa-spin fa-3x fa-fw'></i><span class='sr-only'>Loading...</span>";

$.extend($.fn.dataTable.defaults, {
	serverSide: true, // for process server side
	//filter: true, // this is for disable filter (search box)
	//orderMulti: false,
	//responsive: false,
	pageLength: datatable_page_length,
	//pagingType: datatable_pageing_type,
	//info: datatable_info,
	//lengthChange: datatable_change_length,
	//processing: datatable_processing,
	//lengthMenu: datatable_length_menu,
	//dom: datatable_positioning,
	order: [[1, "asc"]],
	//language: {
	//	search: DataTable_SearchLabel,
	//	searchPlaceholder: DataTable_SearchPlaceHolder,
	//	processing: datatable_processingHTML,
	//	info: "Showing _START_ - _END_ of _TOTAL_ total results",
	//},
	headerCallback: function (settings) {
		//$(".filters").insertBefore("div#dataTableGrid_filter");
		//$(".filters").show();
	},
	footerCallback: function (settings) {
		//$("#dataTableGrid_length").parent().removeClass().addClass('col-6 col-sm-6 col-md-6 col-lg-3');
		//$("#dataTableGrid_info").parent().removeClass().addClass('col-6 col-sm-6 col-md-6 col-lg-3');
		//$("#dataTableGrid_paginate").parent().removeClass().addClass('col-12 col-sm-12 col-md-12 col-lg-6');
	},
	drawCallback: function (settings) {
		if (Math.ceil((this.fnSettings().fnRecordsDisplay()) / this.fnSettings()._iDisplayLength) <= 1) {
			//$('.dataTables_paginate').parent().hide();
			//$('.dataTables_length').parent().hide();
			//$("#dataTableGrid_info").parent().removeClass().addClass('col-6 col-sm-6 col-md-6 col-lg-6');
		}
		else {
			//$('.dataTables_paginate').parent().show();
			//$('.dataTables_length').parent().show();
		}
		//$(".filters").insertBefore("div#dataTableGrid_filter");
		//$(".filters").show();
		////$('#dataTableGrid_wrapper div:first').addClass('d-table');
    }
});

//$.fn.dataTable.ext.errMode = 'none';

	//$('.dataTables_filter input').unbind();
	//$('.dataTables_filter input').bind('keyup', function (e) {
	//    if (e.keyCode == 13) {
	//        table.search($(this).val()).draw();
	//    }
	//});