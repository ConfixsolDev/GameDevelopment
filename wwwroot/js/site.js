// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.
$(document).ajaxStart(function () {
    $("#loading").show();
});

$(document).ajaxStop(function () {
    $("#loading").hide();
});

function ShowMessage(msg) {
    toastr.success(msg);
}

function ShowMessageError(msg) {
    toastr.error(msg);
}
function ShowMessageInfo(msg) {
    toastr.info(msg);
}
function ShowMessageWarning(msg) {
    toastr.warning(msg);
}

var toasterOptions = {
    closeButton: true,
    progressBar: true,
    showDuration: 600,
    preventDuplicates: true,
    preventOpenDuplicates: true
};

var PopupTypes = {
    Error: "Error",
    Success: "Success",
    Information: "Information"
};

$(document).keypress(function (event) {
    var keycode = (event.keyCode ? event.keyCode : event.which);
    if (keycode == '13') {
        $('.click_on_enterkey').click();
        $('.click_on_enterkey').prop('disabled', true);
        setTimeout(function () {
            $('.click_on_enterkey').prop('disabled', false);
        }, 2000);
    }
});
function displayAlert(msg, type, title = "") {

    if (typeof (title) == undefined || $.trim(title) == "")
        title = type;

    if (msg != null && msg != "")
        if (!hasSameErrorToastr(msg)) {
            if (type == PopupTypes.Error)
                toastr.error(msg, title, toasterOptions);
            else if (type == PopupTypes.Information)
                toastr.info(msg, title, toasterOptions);
            else if (type == PopupTypes.Success)
                toastr.success(msg, title, toasterOptions);
        }
}

function hasSameErrorToastr(message) {

    var hasSameErrorToastr = false;
    console.log("msg: " + message);
    var $toastContainer = $('#toast-container');
    if ($toastContainer.length > 0) {
        var $errorToastr = $toastContainer.find('.toast-error');
        if ($errorToastr.length > 0) {
            var currentText = $errorToastr.find('.toast-message:contains("' + message + '")').text();
            var areEqual = message.toUpperCase() === currentText.toUpperCase();
            if (areEqual) {
                hasSameErrorToastr = true;
            }
        }
    }
    return hasSameErrorToastr;
}

function populateDropdownOnChange(sourceSelector, targetSelector, url) {
    $(sourceSelector).change(function () {
        var selectedID = $(this).val();
        var apiUrl = url + '?id=' + selectedID;
        if (selectedID != 0) {
            $.getJSON(apiUrl, function (data) {
                var items = "";
                $(targetSelector).empty();
                $.each(data, function (i, row) {
                    items += "<option value='" + row.value + "'>" + row.text + "</option>";
                });
                $(targetSelector).html(items);
            });
        }
    });
}

$(document).on("submit", ".frmSubmit", function (e) {
   
    e.preventDefault();
    var url = $(this).attr("action");
    var data = $(this).serialize();
    var beforeSubmit = $(this).attr("data-beforeSubmit");
    if ($.trim(beforeSubmit) != "")
        eval(beforeSubmit); //

    var strSuccess = $(this).attr("data-success"); //
    var strError = $(this).attr("data-error");

    $.ajax({
        url: url,
        type: 'POST',
        data: data,
        cache: false,
        success: function (response) {
          
            if (response.message === "Already Scanned.") {
                toastr.error(response.message);
            }
           // displayAlert(response, PopupTypes.Success);
            if ($.trim(strSuccess) != "") {
                eval(strSuccess);
            }
            $("#ModelForAll").modal("hide");
            $("#ModelForAllLg").modal("hide");

        },
        error: function (response) {
            handleError(response);
            if ($.trim(strError) != "") {
                eval(strError);
            }
        }
    });
});
function initializeDateRangePicker(start, end, elementId) {
    function cb(start, end) {
        $('#' + elementId + ' span').html(start.format('DD/MM/YYYY') + ' - ' + end.format('DD/MM/YYYY'));
        window.startDate = start.format('YYYY-MM-DD'); // ISO format for API
        window.endDate = end.format('YYYY-MM-DD'); // ISO format for API
    }

    let $inputField = $('#' + elementId + ' input');

    $('#' + elementId).daterangepicker({
        startDate: start,
        endDate: end,
        autoUpdateInput: false, 
        ranges: {
            'Today': [moment(), moment()],
            'Yesterday': [moment().subtract(1, 'days'), moment().subtract(1, 'days')],
            'Last 7 Days': [moment().subtract(6, 'days'), moment()],
            'Last 30 Days': [moment().subtract(29, 'days'), moment()],
            'Last 60 Days': [moment().subtract(59, 'days'), moment()],
            'This Month': [moment().startOf('month'), moment().endOf('month')],
            'Last Month': [moment().subtract(1, 'month').startOf('month'), moment().subtract(1, 'month').endOf('month')],
            'Last 3 Months': [moment().subtract(3, 'months').startOf('month'), moment().subtract(1, 'month').endOf('month')],
            'Last 6 Months': [moment().subtract(6, 'months').startOf('month'), moment().subtract(1, 'month').endOf('month')],
            'Last One Year': [moment().subtract(365, 'days'), moment()],
        }
    }, cb);

    cb(start, end);

    $inputField.on('keyup change', function () {
        let inputValue = $(this).val().trim();
        let dates = inputValue.split('-');

        if (dates.length === 2) {
            let startDate = moment(dates[0].trim(), 'DD/MM/YYYY');
            let endDate = moment(dates[1].trim(), 'DD/MM/YYYY');

            if (startDate.isValid() && endDate.isValid()) {
                $('#' + elementId).data('daterangepicker').setStartDate(startDate);
                $('#' + elementId).data('daterangepicker').setEndDate(endDate);
            }
        }
    });
}


$(function () {
    if ($('#reportrange1').length) {
        var start = moment().subtract(6, 'days');
        var end = moment();
        initializeDateRangePicker(start, end, 'reportrange1');
    } else if ($('#reportrange').length) {
        var start = moment();
        var end = moment();
        initializeDateRangePicker(start, end, 'reportrange');
    } else if ($('#reportrangeMonth').length) {
        var start = moment().startOf('month');
        var end = moment().endOf('month');
        initializeDateRangePicker(start, end, 'reportrangeMonth');
    }
});


function resetToToday(elementId) {

    if (elementId === 'reportrange1') {
        var start = moment().subtract(6, 'days');
        var end = moment();
        initializeDateRangePicker(start, end, 'reportrange1');
    } 
    else if (elementId === 'reportrange') {
        var start = moment();
        var end = moment();
        initializeDateRangePicker(start, end, 'reportrange');
    }
    else if (elementId === 'reportrangeMonth') {
        var start = moment().startOf('month');
        var end = moment().endOf('month');
        initializeDateRangePicker(start, end, 'reportrangeMonth');
    }
}


$(document).on("submit", ".LovfrmSubmit", function (e) {
    e.preventDefault();
    var $form = $(this);
    var url = $form.attr("action");
    var data = $form.serialize();
    var beforeSubmit = $form.attr("data-beforeSubmit");
    var dataTableId = $form.attr("data-table");  

    if ($.trim(beforeSubmit) != "") {
        eval(beforeSubmit);
    }

    var strSuccess = $form.attr("data-success");
    var strError = $form.attr("data-error");

    $.ajax({
        url: url,
        type: 'POST',
        data: data,
        cache: false,
        success: function (response) {
            displayAlert(response, PopupTypes.Success);
            $("#ModelForAll").modal("hide");
            if (dataTableId && typeof dataTableId === "string" && $('#' + dataTableId).length) {
                $('#' + dataTableId).DataTable().ajax.reload();
            } else {
                setTimeout(function () {
                    window.location.reload();
                }, 300);
            }
        },
        error: function (response) {
            handleError(response);
            if ($.trim(strError) != "") {
                eval(strError);
            }
        }
    });

});

$(document).on("submit", ".frmSubmitupload", function (e) {

    // Prevent the default form submission
    event.preventDefault();

    // Create FormData object
    var formData = new FormData(this);

    $.ajax({
        url: $(this).attr('action'),
        type: 'POST',
        data: formData,
        processData: false,
        contentType: false,
        success: function (response) {
            displayAlert(response, PopupTypes.Success);
            $("#ModelForAll").modal("hide");
        },
        error: function (response) {
            handleError(response);
            if ($.trim(strError) != "") {
                eval(strError);
            }
        }
    });
});
$(document).on("submit", ".fileFrmSubmitupload", function (e) {
    e.preventDefault();
    var formData = new FormData(this);
    var $form = $(this);
    var strSuccess = $form.attr("data-success");
    var dataTableName = $(this).data('datatable');
    $.ajax({
        xhr: function () {
            var xhr = new window.XMLHttpRequest();
            xhr.upload.addEventListener("progress", function (evt) {
                if (evt.lengthComputable) {
                    var percentComplete = evt.loaded / evt.total * 100;
                    $(".progress-bar").width(percentComplete + '%').attr('aria-valuenow', percentComplete).text(percentComplete.toFixed(2) + '%');
                }
            }, false);
            return xhr;
        },
        url: $(this).attr('action'),
        type: 'POST',
        data: formData,
        processData: false,
        contentType: false,
        beforeSend: function () {
            $(".progress").show();
            $(".progress-bar").width('0%').attr('aria-valuenow', 0).text('0%');
        },
        success: function (response) {
            displayAlert(response, PopupTypes.Success);
            $("#ModelForAll").modal("hide");
            $(".progress").hide();
            eval(strSuccess);

            if (window[dataTableName]) {
                $(window[dataTableName]).DataTable().ajax.reload();
            } else {
                setTimeout(function () {
                    window.location.reload();
                }, 300);
            }
        },
        error: function (xhr, status, error) {
            var response = xhr.responseJSON || {};
            var errorMessage = response.message || "An error occurred while uploading the file.";
            handleError(response);
            displayAlert(errorMessage, PopupTypes.Error);
            $(".progress").hide();
        }
    });
});
function getControllerName() {
    var urlParts = window.location.pathname.split('/');
    return urlParts[1]; // Assuming controller name is the second part of the URL
}
$(document).on("submit", ".formSubmit", function (e) {
    
    e.preventDefault(); // Prevent default form submission
    
    // Get the controller name
    var controllerName = getControllerName();
    // Construct the base URL using the controller name
    var baseUrl = '/' + controllerName;
    var indexUrl = baseUrl + '/Index';


    // AJAX request
    $.ajax({
        url: $(this).attr('action'), // Form action URL
        type: $(this).attr('method'), // Form method (POST)
        data: $(this).serialize(), // Form data
        success: function (response) {
            // Check if response contains success message
            if (response.success) {
                // Show success Toastr message
                toastr.success('Success.');
                // Redirect to desired page
                window.location.href = indexUrl;
            }
        },
        error: function (xhr) {
            // Show error Toastr message
            toastr.error(xhr.responseJSON.error);
        }
    });
});
$(document).on("submit", ".frmSubmitFormData", function (e) {
    e.preventDefault();
    var url = $(this).attr("action");
    var data = new FormData($(this)[0]);

    var beforeSubmit = $(this).attr("data-beforeSubmit");
    if ($.trim(beforeSubmit) != "")
        eval(beforeSubmit); //

    var strSuccess = $(this).attr("data-success"); //
    var strError = $(this).attr("data-error");

    $.ajax({
        url: url,
        type: 'POST',
        data: data,
        cache: false,
        contentType: false,
        processData: false,
        success: function (response) {
            // displayAlert(response, PopupTypes.Success);
            if ($.trim(strSuccess) != "") {
                eval(strSuccess);
            }
        },
        error: function (response) {
            handleError(response);
            if ($.trim(strError) != "") {
                eval(strError);
            }
        }
    });
});

$(document).on("click", ".delete_link", function (e) {
    e.preventDefault();
    var name = $(this).attr("data-name");
    var c = confirm('Are you sure to delete ' + name);

    if (!c)
        return;

    var url = $(this).attr("href");
    var strSuccess = $(this).attr("data-success");
    var strError = $(this).attr("data-error");
    var dataTableId = $(this).attr("data-table");

    $.ajax({
        url: url,
        type: 'GET',
        cache: false,
        success: function (response) {
            displayAlert(response, PopupTypes.Success);
            if ($.trim(strSuccess) != "") {
                eval(strSuccess);
            }

            if (dataTableId && typeof dataTableId === "string" && $('#' + dataTableId).length) {
                $('#' + dataTableId).DataTable().ajax.reload();
            } else {
                setTimeout(function () {
                    window.location.reload();
                }, 500);
            }
        },
        error: function (response) {
            toastr.error("Error: " + response.responseText); // Using Toastr for error notification
            if ($.trim(strError) != "") {
                eval(strError);
            }
        }
    });
});

//$(document).on("click", ".delete_link", function (e) {

//    e.preventDefault();

//    var url = $(this).attr("href");
//    var strSuccess = $(this).attr("data-success");
//    var strError = $(this).attr("data-error");
//    var tableId = $(this).closest('table').attr('id');

//    console.log("Parent table ID:", tableId);
//    swal({
//        title: "Are you sure want to Delete?",
//        text: "You will not be able to restore the data!",
//        type: "warning",
//        showCancelButton: true,
//        confirmButtonColor: "#dd4b39",
//        confirmButtonText: "Yes, delete it!",
//        closeOnConfirm: true,
//        customClass: {
//            popup: 'small-swal-popup'
//        }
//    }, function () {
//        $.ajax({
//            url: url,
//            type: 'GET',
//            cache: false,
//            data: { TableId: tableId },
//            success: function (response) {
//                if (tableId != undefined) {
//                    $('#' + tableId).DataTable().ajax.reload();
//                }
//                if ($.trim(strSuccess) != "") {
//                    eval(strSuccess); 
//                }
//            },
//            error: function (response) {
//                handleError(response);
//                if ($.trim(strError) != "") {
//                    eval(strError);
//                }
//            }
//        });
//    });
//});

$(document).on("click", ".GetAjaxRender", function (e) {
    e.preventDefault();
    var url = $(this).attr("href");
    var message = $(this).attr("data-confirm");
    if ($.trim(message) != "") {
        var c = confirm(message);
        if (!c) {
            return;
        }
    }

    var strSuccess = $(this).attr("data-success");
    var strError = $(this).attr("data-error");

    $.ajax({
        url: url,
        type: 'GET',
        cache: false,
        success: function (response) {
            displayAlert(response, PopupTypes.Success);
            if ($.trim(strSuccess) != "") {
                eval(strSuccess);
            }
            RefreshDataTable();
        },
        error: function (response) {
            handleError(response);
            if ($.trim(strError) != "") {
                eval(strError);
            }
        }
    });
});

//$(document).on("click", ".action_link", function (e) {
//    e.preventDefault();
//    var url = $(this).attr("href");
//    $.get(url, "", function (response) {
//        $("#ModelContentDiv").html(response);
//        $("#ModelForAll").modal("show");

//        $.validator.unobtrusive.parse("#ModelContentDiv form");

//        $('.select2').select2({
//            dropdownParent: $('#ModelForAll'),
//            search: true,
//            width: '100%',
//        });
//    });
//});
$(document).on("click", ".action_link", function (e) {
    e.preventDefault();

    var $modal = $("#ModelForAll");
    var $modelContentDiv = $("#ModelContentDiv");

    var url = $(this).attr("href");

    // Show loading indicator
    $modelContentDiv.html('<div class="loader">Loading...</div>');

    $.get(url, "", function (response) {
        $modelContentDiv.html(response);

        // Safely parse unobtrusive validation
        try {
            $.validator.unobtrusive.parse("#ModelForAll form");
        } catch (e) {
            console.error("Unobtrusive validation failed:", e);
        }

        $modal.modal("show");

        // Initialize Select2 safely
        $modal.find('.select2').each(function () {
            if (!$(this).hasClass("select2-hidden-accessible")) {
                $(this).select2({
                    dropdownParent: $('#ModelForAll'),
                    allowClear: true,
                    width: '100%'
                });
            }
        });

        // Prevent modal from closing on Select2 click
        $modal.on('mousedown', '.select2-container', function (e) {
            e.stopPropagation();
        });

        // Handle Select All functionality
        $('#selectedEmployees').on('change', function () {
            var $selectAllOption = $(this).find('option[value="select-all"]');
            var $otherOptions = $(this).find('option:not([value="select-all"])');
            var selectedValues = $(this).val();

            if (selectedValues.includes('select-all')) {
                $otherOptions.each(function () {
                    $(this).prop('selected', true);
                });
                $selectAllOption.prop('selected', false);
                $(this).trigger('change.select2');
            } else {
                $selectAllOption.prop('selected', false);
            }
        });

    }).fail(function (jqXHR, textStatus, errorThrown) {
        ShowMessageError(jqXHR.responseText);
        console.log(`Error loading content: ${textStatus}, ${errorThrown}`);
        $modelContentDiv.html('<div>Error loading the information. Please try again later.</div>');
    });
});

$(document).on("click", ".action_link_lg", function (e) {
    e.preventDefault();
    var url = $(this).attr("href");
    $.get(url, "", function (response) {

        $("#ModelContentDivlg").html(response);
        $("#ModelForAllLg").modal("show");

        $.validator.unobtrusive.parse("#ModelContentDivlg form");

        $('.select2').select2({
            dropdownParent: $('#ModelContentDivlg'),
            search: true,
            width: '100%',
        });
    });
});


$('#ModelForAll').on('shown.bs.modal', function () {
    $('.datepicker').datepicker({
        format: 'dd/mm/yyyy',
        autoclose: true
    });
});

$('#ModelForAllLg').on('shown.bs.modal', function () {
    $('.datepicker').datepicker({
        format: 'dd/mm/yyyy',
        autoclose: true
    });
});

$(document).on("focus", ".datetimepicker", function () {
    if (!$(this).hasClass("hasDatepicker")) { 
        $(this).datetimepicker({
            dateFormat: "dd/mm/yy",
            timeFormat: "HH:mm",
            showSecond: false,   // Hide seconds
            controlType: 'select', // Dropdown for time
            oneLine: true // Compact view
        });
    }
});



$(document).on("click", ".Helpinfo", function (e) {
    $("#ModelContentDiv").html($('.page-info').html());
    $("#ModelForAll").modal("show");
})

$(document).on("click", ".Helpinfolg", function (e) {
    $("#ModelContentDivlg").html($('.page-info-lg').html());
    $("#ModelForAllLg").modal("show");
})

$(document).on("click", ".Auditinfolg", function (e) {
    $("#AuditModelContentDivlg").html($('.audit-info-lg').html());
    $("#AuditModelForAllLg").modal("show");
})

$(document).on("click", ".UploadDoc", function (e) {
    e.preventDefault();

    var entityId = $(this).data("entityid");
    var path = $(this).data("path");
    var url = `/FileUploads/Index?EntityId=${entityId}&Path=${path}`;
    $.get(url, "", function (response) {
        $("#ModelContentDiv").html(response);
        $.validator.unobtrusive.parse("#ModelForAll form");
        $("#ModelForAll").modal("show");
    });
    $("#ModelForAll").modal("show");
})

$(document).on("click", ".action_mail", function (e) {
    e.preventDefault();
    var url = "/Mail/Mails/Create";
    $.get(url, "", function (response) {

        $("#ModelContentDivlg").html(response);
        $("#ModelForAllLg").modal("show");

        $.validator.unobtrusive.parse("#ModelContentDivlg form");

        $('.select2').select2({
            dropdownParent: $('#ModelForAllLg'),
            search: true,
            width: '100%',
        });
    });
});

$(document).ready(function () {
    // Ensure global modal close buttons work
    $(document).on('click', '[data-dismiss="modal"]', function(e) {
        var $modal = $(this).closest('.modal');
        if ($modal.length) {
            $modal.modal('hide');
        }
    });
    
    // ESC key handler for modals
    $(document).on('keydown', function(e) {
        if (e.key === 'Escape' || e.keyCode === 27) {
            $('.modal.show').modal('hide');
        }
    });
    
    $('select.select2').each(function () {
        var $select = $(this);
        var placeholderText = $select.find('option:first').text(); 

        $select.select2({
            placeholder: placeholderText,  
            allowClear: true,              
            width: '100%'                  
        });
    });
});

function handleError(response) {
    if (response.status == 400) {
        displayAlert(response.responseText, PopupTypes.Error);
    }
    else if (response.status == 500) {
        displayAlert(response.responseJSON.detail, PopupTypes.Error);
    }
    else {
        displayAlert(response.responseText, PopupTypes.Error);
    }
}

function displayLoader(bShow, message = "") {
    if (bShow) {
        $("#divLoader").find("#lblMessage").html(message);
        $("#divLoader").show();
    }
    else {
        $("#divLoader").hide();
    }
}

function displayModalLoader(bShow, message = "") {
    if (bShow) {
        $(".modal-dialog").find(".modal_loader").show();
        $(".modal-dialog").find("input,button").attr("disabled", "disabled");
        if (message != "")
            $(".modal-dialog").find(".btn-primary").find("span").html(message);
    }
    else {
        $(".modal-dialog").find(".modal_loader").hide();
        $(".modal-dialog").find("input,button").removeAttr("disabled");
        if (message != "")
            $(".modal-dialog").find(".btn-primary").find("span").html(message);
    }
}

function onSuccessMsg(response) {
    displayAlert("Settings successfully imported.", PopupTypes.Success);
    $("#ModelForAll").modal("hide");
}

function formatDate(dateString) {
    if (!dateString) return ''; // Return empty string if dateString is empty or null

    // Create a new Date object from the provided dateString
    var date = new Date(dateString);

    // Check if the date is valid
    if (isNaN(date.getTime())) {
        // Return original string if the date is invalid
        return dateString;
    } else {
        // Define format options
        var day = ('0' + date.getDate()).slice(-2);
        var month = ('0' + (date.getMonth() + 1)).slice(-2);
        var year = date.getFullYear();

        return `${day}/${month}/${year}`;
    }
}