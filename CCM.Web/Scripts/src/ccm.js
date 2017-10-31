$('.dropdown-menu').on('click', function (e) {
    if ($(this).hasClass('dropdown-menu-checkboxes')) {
        e.stopPropagation();
    }
});

$(document).ready(function () {
    $(".body-content").show();
});