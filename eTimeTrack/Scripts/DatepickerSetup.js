Date.prototype.addDays = function (days) {
    var dat = new Date(this.valueOf());
    dat.setDate(dat.getDate() + days);
    return dat;
}

$(document).ready(function () {
    $(".datepicker").datepicker({
        dateFormat: "dd M yy"
    });

    $("#startDate").change(function () {
        var startDate = $("#startDate").datepicker("getDate");
        $("#endDateOneWeek").val(moment(startDate.addDays(7)).format("DD MMM YYYY"));
    });
});