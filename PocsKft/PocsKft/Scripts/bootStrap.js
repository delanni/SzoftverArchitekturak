$(function () {
    window.resize = function () {
        var topheight = $(".topNavBar").height();
        var bottomheight = $(".bottomNavBar").height();

        var minimumHeight = $(".content").css("height", "auto").height();
        var contentHeight = Math.max(Math.max($(window).height() - topheight - bottomheight, 0), minimumHeight);
        $(".content").css("height", contentHeight);
    };
    $(window).resize(window.resize);
    window.resize();
    setInterval(window.resize, 300);
});

