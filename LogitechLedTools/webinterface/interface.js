var updateTimer = null;
var profileDetailName = null;

function UtilFormToObject(formData) {
    // Convert form array into object
    var result = {};
    for (var i = 0; i < formData.length; i++) {
        result[formData[i]['name']] = formData[i]['value'];
    }
    return result;
}

function UtilObjectToString(data, indent) {
    var result = "";
    if (typeof indent == "undefined") {
        indent = "";
    }
    if (typeof data == "object") {
        result += indent+"{\n";
        for (var dataName in data) {
            result += indent + "  " + JSON.stringify(dataName) + ": " + UtilObjectToString(data[dataName], indent + "  ")+"\n";
        }
        result += indent+"}";
    } else {
        result += JSON.stringify(data);
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

function GetProfileDetails(name, callback) {
    jQuery.get("/profileDetails.json?name="+encodeURIComponent(name), function (profileData) {
        if (typeof profileData.name != "undefined") {
            if (typeof callback == "function") {
                callback(profileData);
            }
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
    GetProfileDetails(name, function (profileData) {
        jQuery.get("/profiles/" + encodeURIComponent(profileData.name) + "/details.html", function (content) {
            jQuery("#profileDetail").html(content);
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
            profileDetailName = name;
            PageSelect("#profileDetail");
            UpdateProfileDetails();
        });
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

function UpdateProfileDetails() {
    var profileDataElements = jQuery("#profileDetail [data-content=profile-data]");
    if (profileDataElements.length > 0) {
        GetProfileDetails(profileDetailName, function (profileData) {
            profileData.json = UtilObjectToString(profileData);
            profileDataElements.each(function () {
                var templateElement = jQuery(this).attr("data-template");
                var template = jQuery(templateElement).html();
                Mustache.parse(template);
                jQuery(this).html(Mustache.render(template, profileData));
            });
        });
    }
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
    if (jQuery("#profileCards").is(":visible")) {
        UpdateProfiles();
    }
    if (jQuery("#profileDetail").is(":visible")) {
        UpdateProfileDetails();
    }
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