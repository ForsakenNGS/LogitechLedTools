var updateTimer = null;

function GetWindows(callback) {
    jQuery.get("/windows.json", function (result) {
        if (typeof callback == "function") {
            callback(result);
        }
    });
}

function GetWindowScreenshot(hwnd, callback) {
    jQuery.get("/screenshot.json?hwnd=" + encodeURIComponent(hwnd), function (result) {
        if (typeof callback == "function") {
            callback(result);
        }
    });
}

function LoadProfile(name, callback) {
    jQuery.get("/profile.json?name=" + encodeURIComponent(name), function (result) {
        if (typeof callback == "function") {
            callback(result);
        }
    });
}

function UpdateProfiles() {
    GetWindows(function (windowList) {
        var template = jQuery("#tplProfileCards").html();
        Mustache.parse(template);
        jQuery("#profileCards").html(Mustache.render(template, {
            profiles: windowList
        }));
    });
}

function MenuSelect(menuItem) {
    // Disable previous nav + content
    jQuery("#menuMain .nav-item.active").removeClass("active");
    jQuery(".content-container").hide();
    // Enable new nav + content
    var navTarget = jQuery(menuItem).attr("href");
    var navTargetContent = jQuery(navTarget);
    jQuery(menuItem).parents(".nav-item").addClass("active");
    navTargetContent.show();
}

function UpdateUi() {
    UpdateProfiles();
}

function UpdateUiStart() {
    ReadSettings();
    window.setTimeout(function () { UpdateUi() }, 0);
    if (updateTimer == null) {
        updateTimer = window.setInterval(function () { UpdateUi() }, 5000);
    }
}

function ReadSettings() {
    jQuery.get("/setting.json", function (configSettings) {
        for (var name in configSettings) {
            jQuery("#settingsForm input[name=" + name + "],#settingsForm select[name=" + name + "]").val(configSettings[name]);
        }
    });
}

function UpdateSettings() {
    jQuery.post("/setting.json", jQuery("#settingsForm").serialize(), function (result) {
        if (!result.success) {
            alert("Error saving configuration!");
        }
    });
}

UpdateUiStart();