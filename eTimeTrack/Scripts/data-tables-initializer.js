function setDisabledBoxes() {
    $('.assignCheckbox').each(function () {
        if ($(this).prop('checked') === false) {
            $('.approvedCheckbox[data-taskid="' + $(this).data('taskid') + '"]').prop('disabled', true);
            $('.closedCheckbox[data-taskid="' + $(this).data('taskid') + '"]').prop('disabled', true);
        }
    });
}

$(document).ready(function () {

    $(".data-table").each(function () {
        $(this).DataTable({
            pageLength: 50,
            lengthChange: false,
            scrollCollapse: true,
            responsive: true,
            "autoWidth": false,
            "aaSorting": [],
            "fnDrawCallback": function (oSettings) {
                if (oSettings._iDisplayLength > oSettings.fnRecordsDisplay()) {
                    $(oSettings.nTableWrapper).find(".dataTables_paginate").hide();
                }
            }
        });
    });

    // Page Load
    setDisabledBoxes();

    $(document).on('click', '.paginate_button', function () {
        setDisabledBoxes();
    });

    $(document).on('keyup', '.dataTables_filter input', function () {
        setDisabledBoxes();
    });


});