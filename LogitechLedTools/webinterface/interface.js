var updateTimer = null;

function UtilFormToObject(formData) {
    // Convert form array into object
    var result = {};
    for (var i = 0; i < formData.length; i++) {
        result[formData[i]['name']] = formData[i]['value'];
    }
    return result;
}

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

function ShowProfileDetails(name) {
    jQuery.get("/profileDetails.json", function (profileData) {
        if (typeof profileData.name != "undefined") {
            jQuery.get("/profiles/" + encodeURIComponent(profileData.name) + "/details.html", function (template) {
                Mustache.parse(template);
                jQuery("#profileDetail").html(
                    Mustache.render(template, profileData)
                );
                jQuery("[data-content=profile-config]").submit(function (event) {
                    event.preventDefault();
                    var formElement = jQuery(this);
                    var formData = UtilFormToObject(formElement.serializeArray());
                    jQuery.post("/profileConfig.json", {
                        name: name,
                        config: JSON.stringify(formData)
                    }, function (result) {
                        if (result.success) {
                            formElement.find("[data-alert=success]").show();
                        } else {
                            formElement.find("[data-alert=fail]").show();
                        }
                    });
                }).each(function () {
                    var formElement = jQuery(this);
                    for (var configName in profileData.config) {
                        var configValue = profileData.config[configName];
                        formElement.find("[name=" + configName + "]").val(configValue);
                    }
                    var formElement = jQuery(this);
                    formElement.find("[data-submit=auto]").change(function (event) {
                        formElement.submit();
                    });
                });
                PageSelect("#profileDetail");
            });
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
    // Disable previous nav
    jQuery("#menuMain .nav-item.active").removeClass("active");
    // Enable new nav + content
    var navTarget = jQuery(menuItem).attr("href");
    jQuery(menuItem).parents(".nav-item").addClass("active");
    PageSelect(navTarget);
}

function PageSelect(pageIdent) {
    // Disable previous nav + content
    jQuery(".content-container").hide();
    // Enable new nav + content
    var navTargetContent = jQuery(pageIdent);
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
    PageSelect("#profileCards");
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