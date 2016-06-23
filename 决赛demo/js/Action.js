function filterImgs(intent, text, entities) {
    switch (intent) {
        case "show_first":
            show_first();
            break;
        case "show_next":
            show_next();
            break;
        case "show_previous":
            show_previous();
            break;
        case "show_last":
            show_last();
            break;
        case "start_auto_show":
            start_auto_show();
            break;
        case "stop_auto_show":
            stop_auto_show();
            break;
        case "show_one_per_page":
            show_one_per_page();
            break;
        case "filter_by_trip":
            filter_by_trip();
            break;
        case "filter_by_date_location":
            filter_by_date_location(text, entities);
            break;
        case "rotate":
            rotate(text, entities);
            break;
        case "filter_by_tag":
            filter_by_tag(text, entities);
            break;
        case "remove_filter":
            remove_filter();
            break;
        case "add_tag":
            add_tag(text, entities);
            break;
        case "show_n_by_m_per_page":
            break;
        case "show_n_per_page":
            break;
        case "None":
            break;
        default:
            console.log("None");
            break;
    }
    isRunning = 0;
}

function isMatch(id, key, value) {
    console.log("value=" + value);
    for (var i = 0; i < maps.entities.length; i++) {
        if ($(maps.entities[i]).attr("id") == id) {
            switch (key) {
                case "tag":
                    for (var j = 0; j < $(maps.entities[i]).attr("tag").length; j++) {
                        if (value.indexOf($(maps.entities[i]).attr("tag")[j]) != -1) {
                            return true;
                        }
                    }
                    break;
                case "location":
                    for (var j = 0; j < $(maps.entities[i]).attr("location").length; j++) {
                        if (value.indexOf($(maps.entities[i]).attr("location")[j]) != -1) {
                            return true;
                        }
                    }
                    break;
                case "week":
                    var picTime = new Date($(maps.entities[i]).attr("location"));
                    var paraTime = new Date(value);
                    var days = Math.floor((picTime - paraTime) / (24 * 3600 * 1000));
                    if (days < 7)
                        return true;
                    break;
                case "day":
                    var picTime = new Date($(maps.entities[i]).attr("location"));
                    var paraTime = new Date(value);
                    var days = Math.floor((picTime - paraTime) / (24 * 3600 * 1000));
                    if (days == 0)
                        return true;
                    break;
                default:
                    break;
            }
        }
    }
    return false;
}

function show_first() {
    var $container = $('#fullscreenSlideshowContainer');
    if ($container.data("hiddenElements") == null) {
        var link = $("#images").children(":first").children(":first");
        $container.trigger("show", [link.attr("rel"), link.data("slide")]);
    }
    else {
        var slides = $container.data("slides");
        $container.trigger("showSlide", slides[0]);
    }
}

function show_next() {
    var $container = $('#fullscreenSlideshowContainer');
    if ($container.data("hiddenElements") == null) {
        var link = $("#images").children(":first").children(":first");
        $container.trigger("show", [link.attr("rel"), link.data("slide")]);
    }
    else {
        $container.trigger("nextSlide");
    }
}

function show_previous() {
    var $container = $('#fullscreenSlideshowContainer');
    if ($container.data("hiddenElements") == null) {
        var link = $("#images").children(":first").children(":first");
        $container.trigger("show", [link.attr("rel"), link.data("slide")]);
    }
    else {
        $container.trigger("prevSlide");
    }
}

function show_last() {
    var $container = $('#fullscreenSlideshowContainer');
    if ($container.data("hiddenElements") == null) {
        var link = $("#images").children(":last").children(":first");
        $container.trigger("show", [link.attr("rel"), link.data("slide")]);
    }
    else {
        var slides = $container.data("slides");
        $container.trigger("showSlide", slides[slides.length - 1]);
    }
}

var auto_show_id = 0;
function start_auto_show() {
    var $container = $('#fullscreenSlideshowContainer');
    if ($container.data("hiddenElements") == null) {
        var link = $("#images").children(":first").children(":first");
        $container.trigger("show", [link.attr("rel"), link.data("slide")]);
    }
    auto_show_id = setInterval(function () {
        $container.trigger("nextSlide");
    }, 3000);
}
function stop_auto_show() {
    if (auto_show_id > 0)
        clearInterval(auto_show_id);
    auto_show_id = 0;
}

function show_one_per_page() {
    var $container = $('#fullscreenSlideshowContainer');
    if ($container.data("hiddenElements") == null) {
        var link = $("#images").children(":first").children(":first");
        $container.trigger("show", [link.attr("rel"), link.data("slide")]);
    }
}

function filter_by_trip() {
    var $container = $('#fullscreenSlideshowContainer');
    if ($container.data("hiddenElements") == null) {
        $("#images").children().each(function () {
            var match = isMatch($(this).children(":first").attr("id"), "tag", "trip");
            //console.log(match);
            if (match == false) {
                $(this).attr("style", "display:none");
            }
        })
    }
}

function remove_filter() {
    var $container = $('#fullscreenSlideshowContainer');
    if ($container.data("hiddenElements") == null) {
    }
    else {
        $container.trigger("close");
    }
}

var angle = 0;
function rotate(text, entities) {
    var $container = $('#fullscreenSlideshowContainer');
    if ($container.data("hiddenElements") == null) {
    }
    else {
        for (var i = 0; i < entities.length; i++) {
            if (entities[i].type == 'direction') {
                var value = entities[i].entity;
                switch (value) {
                    case "逆时针":
                    case "反时针":
                    case '左':
                        angle -= 90;
                        $(".slide").rotate({ animateTo: angle });
                        break;
                    case "顺时针":
                    case '右':
                        angle += 90;
                        $(".slide").rotate({ animateTo: angle });
                        break;
                    default:
                        break;
                }
            }
        }
    }
}

function filter_by_tag(text, entities) {
    var $container = $('#fullscreenSlideshowContainer');
    if ($container.data("hiddenElements") == null) {
        for (var i = 0; i < entities.length; i++) {
            if (entities[i].type == 'tag') {
                var value = entities[i].entity;
                $("#images").children().each(function () {
                    var match = isMatch($(this).children(":first").attr("id"), "tag", value);
                    if (match == false) {
                        $(this).attr("style", "display:none");
                    }
                })
            }
        }
    }
}

function filter_by_date_location(text, entities) {
    var $container = $('#fullscreenSlideshowContainer');
    if ($container.data("hiddenElements") == null) {
        for (var i = 0; i < entities.length; i++) {
            if (entities[i].type == 'location') {
                var value = entities[i].entity;
                $("#images").children().each(function () {
                    var match = isMatch($(this).children(":first").attr("id"), "location", value);
                    if (match == false) {
                        $(this).attr("style", "display:none");
                    }
                })
            }
            else if (entities[i].type == 'builtin.datetime.date') {
                var re = /XXXX-W\d+/;
                if (re.test(entities[i].resolution.date)) {
                    $.ajax({
                        type: "POST",
                        dataType: "jsonp",
                        url: "./services.asmx/dateByWeek",
                        data: 'date=' + entities[i].resolution.date,
                        success: function (date) { //成功 
                            $("#images").children().each(function () {
                                var match = isMatch($(this).children(":first").attr("id"), "week", date);
                                console.log(date);
                                if (match == false) {
                                    $(this).attr("style", "display:none");
                                }
                            })
                        }
                    });
                }
            }
            else{
                $.ajax({
                    type: "POST",
                    dataType: "jsonp",
                    url: "./services.asmx/dateByDuration",
                    data: 'date=' + entities[i].resolution.date,
                    success: function (date) { //成功 
                        $("#images").children().each(function () {
                            var match = isMatch($(this).children(":first").attr("id"), "day", date);
                            console.log(date);
                            if (match == false) {
                                $(this).attr("style", "display:none");
                            }
                        })
                    }
                });
            }
        }
    }
}

function add_tag(text, entities) {
    var $container = $('#fullscreenSlideshowContainer');
    if ($container.data("hiddenElements") == null) {
    }
    else {
        var curSlide = $container.data("currentSlide");
        var idBegin = curSlide.image.lastIndexOf("/");
        var idEnd = curSlide.image.lastIndexOf(".");
        var id = curSlide.image.substring(idBegin + 1, idEnd);
        console.log("id=" + id);
        for (var i = 0; i < entities.length; i++) {
            if (entities[i].type == 'tag') {
                var value = entities[i].entity;
                for (var i = 0; i < maps.entities.length; i++) {
                    if ($(maps.entities[i]).attr("id") == id) {
                        console.log($(maps.entities[i]).attr("id"));
                        $(maps.entities[i]).attr("tag").push(value);
                        console.log($(maps.entities[i]).attr("tag"));
                    }
                }
            }
        }
        console.log(maps);
        var mapsStr = JSON.stringify(maps);
        $.ajax({
            type: "POST",
            dataType: "jsonp",
            url: "./services.asmx/store",
            data: 'json=' + mapsStr,
        });
    }
}