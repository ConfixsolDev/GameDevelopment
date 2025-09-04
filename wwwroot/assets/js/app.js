!function (t) { "use strict"; function e() { for (var e = document.getElementById("topnav-menu-content").getElementsByTagName("a"), t = 0, a = e.length; t < a; t++)"nav-item dropdown active" === e[t].parentElement.getAttribute("class") && (e[t].parentElement.classList.remove("active"), e[t].nextElementSibling.classList.remove("show")) } if (t("#side-menu").metisMenu(), t("#vertical-menu-btn").on("click", function (e) { e.preventDefault(), t("body").toggleClass("sidebar-enable"), 992 <= t(window).width() ? t("body").toggleClass("vertical-collpsed") : t("body").removeClass("vertical-collpsed") }), t("#sidebar-menu a").each(function () { var e = window.location.href.split(/[?#]/)[0]; this.href == e && (t(this).addClass("active"), t(this).parent().addClass("mm-active"), t(this).parent().parent().addClass("mm-show"), t(this).parent().parent().prev().addClass("mm-active"), t(this).parent().parent().parent().addClass("mm-active"), t(this).parent().parent().parent().parent().addClass("mm-show"), t(this).parent().parent().parent().parent().parent().addClass("mm-active")) }), t(".navbar-nav a").each(function () { var e = window.location.href.split(/[?#]/)[0]; this.href == e && (t(this).addClass("active"), t(this).parent().addClass("active"), t(this).parent().parent().addClass("active"), t(this).parent().parent().parent().addClass("active"), t(this).parent().parent().parent().parent().addClass("active"), t(this).parent().parent().parent().parent().parent().addClass("active")) }), t(document).ready(function () { var e; 0 < t("#sidebar-menu").length && 0 < t("#sidebar-menu .mm-active .active").length && 300 < (e = t("#sidebar-menu .mm-active .active").offset().top) && (e -= 300, t(".vertical-menu .simplebar-content-wrapper").animate({ scrollTop: e }, "slow")) }), document.getElementById("topnav-menu-content")) { for (var a = document.getElementById("topnav-menu-content").getElementsByTagName("a"), n = 0, s = a.length; n < s; n++)a[n].onclick = function (e) { "#" === e.target.getAttribute("href") && (e.target.parentElement.classList.toggle("active"), e.target.nextElementSibling.classList.toggle("show")) }; window.addEventListener("resize", e) } t(function () { t('[data-bs-toggle="tooltip"]').tooltip() }), t(function () { t('[data-bs-toggle="popover"]').popover() }), t(window).on("load", function () { t("#status").fadeOut(), t("#preloader").delay(350).fadeOut("slow") }), t("body").on("click", "table .dd-wrap .dd-label, table .dd-wrap .dd-action", function () { t("table .dd-wrap").each(function () { t(this).hasClass("dd-open") && t(this).removeClass("dd-open") }); var e = t(this).closest(".dd-wrap"); t(e).hasClass("dd-open") ? t(e).removeClass("dd-open") : t(e).addClass("dd-open") }), t("body").on("mouseleave", ".dd-wrap .dd-list", function () { var e = t(this).parent(); t(e).hasClass("dd-open") && t(e).removeClass("dd-open") }) }(jQuery);
$('#zoomBtn').click(function () {
    $('.zoom-btn-sm').toggleClass('scale-out');
    if (!$('.zoom-card').hasClass('scale-out')) {
        $('.zoom-card').toggleClass('scale-out');
    }
});

$('.zoom-btn-sm').click(function () {
    var btn = $(this);
    var card = $('.zoom-card');

    if ($('.zoom-card').hasClass('scale-out')) {
        $('.zoom-card').toggleClass('scale-out');
    }
    if (btn.hasClass('zoom-btn-person')) {
        card.css('background-color', '#d32f2f');
    } else if (btn.hasClass('zoom-btn-doc')) {
        card.css('background-color', '#fbc02d');
    } else if (btn.hasClass('zoom-btn-tangram')) {
        card.css('background-color', '#388e3c');
    } else if (btn.hasClass('zoom-btn-report')) {
        card.css('background-color', '#1976d2');
    } else {
        card.css('background-color', '#7b1fa2');
    }
});
