var updateTimer = null;
var profileDetailName = null;
var profileDetailRetrys = 0;

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
        result += "{\n";
        for (var dataName in data) {
            result += indent + "  " + JSON.stringify(dataName) + ": " + UtilObjectToString(data[dataName], indent + "  ")+"\n";
        }
        result += indent+"}";
    } else {
        result += JSON.stringify(data);
    }
    return result;
}

function GetWindows(callbackSuccess, callbackFail) {
    jQuery.ajax({
        url: "/windows.json",
        success: function (result) {
            if (typeof callbackSuccess == "function") {
                callbackSuccess(result);
            }
        },
        error: function (jqXHR, textStatus, error) {
            if (typeof callbackFail == "function") {
                callbackFail(jqXHR, textStatus, error);
            }
        }
    });
}

function GetWindowScreenshot(hwnd, callbackSuccess, callbackFail) {
    jQuery.ajax({
        url: "/screenshot.json?hwnd=" + encodeURIComponent(hwnd),
        success: function (result) {
            if (typeof callbackSuccess == "function") {
                callbackSuccess(result);
            }
        },
        error: function (jqXHR, textStatus, error) {
            if (typeof callbackFail == "function") {
                callbackFail(jqXHR, textStatus, error);
            }
        }
    });
}

function GetProfileDetails(name, callbackSuccess, callbackFail) {
    jQuery.ajax({
        url: "/profileDetails.json?name=" + encodeURIComponent(name),
        success: function (profileData) {
            if (typeof profileData.name != "undefined") {
                if (typeof callbackSuccess == "function") {
                    callbackSuccess(profileData);
                }
            } else {
                if (typeof callbackFail == "function") {
                    callbackFail(null, null, "Profile not available!");
                }
            }
        },
        error: function (jqXHR, textStatus, error) {
            if (typeof callbackFail == "function") {
                callbackFail(jqXHR, textStatus, error);
            }
        }
    });
}

function LoadProfile(name, callbackSuccess, callbackFail) {
    jQuery.ajax({
        url: "/profile.json?name=" + encodeURIComponent(name),
        success: function (result) {
            if (typeof callbackSuccess == "function") {
                callbackSuccess(result);
            }
        },
        error: function (jqXHR, textStatus, error) {
            if (typeof callbackFail == "function") {
                callbackFail(jqXHR, textStatus, error);
            }
        }
    });
}

function ShowProfileDetails(name) {
    GetProfileDetails(name, function (profileData) {
        // Success
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
            profileDetailRetrys = 0;
        });
    }, function (jqXHR, textStatus, error) {
        // Error!
        if (profileDetailRetrys < 3) {
            profileDetailRetrys++;
            window.setTimeout(function () {
                ShowProfileDetails(name);
            }, 5000);
        } else {
            profileDetailRetrys = 0;
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
    var pageActive = location.hash.match(/^(#[^\-]+)-?(.*)$/);
    if (pageActive !== null) {
        switch (pageActive[1]) {
            default:
                PageSelect(pageActive[1]);
                break;
            case "#profileDetail":
                ShowProfileDetails(pageActive[2]);
                break;
        }
    } else {
        PageSelect("#profileCards");
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