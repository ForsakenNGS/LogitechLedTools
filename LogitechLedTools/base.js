var configuration = JSON.parse(ConfigJsonRead());
var profileActive = null;
var profileInitialized = false;
var profileScreenshot = false;
var profiles = {};
var windowUpdateTime = 0;
var windowsByHwnd = {};
var windowsByName = {};
var status = 0;
var statusTime = 0;

LogitechKeyboard.SetKeyboardType(GetConfigValue("keyboardType", LogitechKeyboardType_PerKey));

function dictToObject(dict) {
    var result = {};
    var enumerator = dict.GetEnumerator();
    while (enumerator.MoveNext()) {
        result[enumerator.Current.Key] = enumerator.Current.Value;
    }
    return result;
}
function listToArray(list) {
    var result = [];
    var enumerator = list.GetEnumerator();
    while (enumerator.MoveNext()) {
        result.push(enumerator.Current);
    }
    return result;
}

function GetWindows() {
    var timeNow = GetEngineClock();
    if ((timeNow - windowUpdateTime) > 10000) {
        windowUpdateTime = timeNow;
        // Clear and update globals
        windowsByHwnd = {};
        windowsByName = {};
        // Get new result
        var windowList = GetOpenWindows();
        var enumerator = windowList.GetEnumerator();
        while (enumerator.MoveNext()) {
            var hwnd = enumerator.Current.Key;
            var name = enumerator.Current.Value;
            if (CheckProfile(name)) {
                windowsByHwnd[hwnd] = name;
                windowsByName[name] = hwnd;
            }
        }
    }
    var windowActive = GetForegroundWindow();
    var result = [];
    for (var hwnd in windowsByHwnd) {
        var name = windowsByHwnd[hwnd];
        result.push({
            hwnd: hwnd,
            name: name,
            active: ((profileActive == name) && (windowActive == hwnd)),
            profile: profiles[name].data,
            screenshot: GetWindowScreenshot(hwnd, name + ".jpg")
        });
    }
    return JSON.stringify(result);
}

function GetWindowScreenshot(hwnd, cacheFile) {
    var windowSize = GetWindowRect(hwnd);
    return GetWindowScreenshotRect(hwnd, windowSize.Left, windowSize.Top, windowSize.Right, windowSize.Bottom, cacheFile);
}

function GetWindowScreenshotRect(hwnd, left, top, right, bottom, cacheFile) {
    if (typeof cacheFile == "undefined") {
        // No caching
        cacheFile = "";
    }
    // Get cached screnshot if available
    var filename = "/cache/" + (cacheFile == "" ? "hwnd_" + hwnd + ".jpg" : cacheFile);
    var cacheFiletime = GetFileUpdateDate("webinterface" + filename);
    if (cacheFiletime == 0) {
        // Get live screenshot from application
        var bitmap = CaptureScreen(hwnd, left, top, right, bottom, cacheFile);
        bitmap.Save("webinterface" + filename, ImageFormat_Jpeg);
        // Dispose bitmap object
        bitmap.Dispose();
    }
    // Prevent browser caching
    filename += "?updated=" + GetFileUpdateDate("webinterface" + filename);
    // Return result
    return filename;
}

function CheckProfile(name) {
    if (typeof profiles[name] == "undefined") {
        profiles[name] = LoadProfileClass(name);
        if (profiles[name] != null) {
            // Ensure display method is available and load custom one if defined
            profiles[name].name = name;
            if (typeof profiles[name].display != "function") {
                // Dummy display method
                profiles[name].display = function (profileData) { };
            }
            profileDisplay = LoadProfileDisplay(name);
            if (typeof profileDisplay == "function") {
                // Apply custom display method
                profiles[name].display = profileDisplay;
            }
            LoadProfileConfigValues(name);
        }
    }
    return (profiles[name] !== null);
}

function LoadProfileConfigValues(name, profileConfig) {
    if (typeof profiles[name] != "undefined") {
        if (typeof profiles[name].config == "undefined") {
            profiles[name].config = {};
        }
        if (typeof profileConfig == "undefined") {
            profileConfig = JSON.parse(LoadProfileConfig(name));
        } else if (typeof profileConfig == "string") {
            profileConfig = JSON.parse(profileConfig);
        }
        for (var configName in profileConfig) {
            profiles[name].config[configName] = profileConfig[configName];
        }
    }
}

function LoadProfile(name) {
    if (CheckProfile(name)) {
        profileActive = name;
        return JSON.stringify(profiles[name].data);
    }
    return JSON.stringify(null);
}

function GetActiveProfile() {
    var windowActive = GetForegroundWindow();
    // Check if window for active profile was closed
    if ((profileActive != null) && (typeof windowsByName[profileActive] == "undefined")) {
        profileActive = null;
        profileInitialized = false;
    }
    // Check if the current window has a known profile attached
    if (profileActive == null) {
        if (typeof windowsByHwnd[windowActive] != "undefined") {
            LoadProfile(windowsByHwnd[windowActive]);
            profileInitialized = false;
        }
    }
    // Update active profile
    if (profileActive != null) {
        if (windowActive == windowsByName[profileActive]) {
            if (!profileInitialized) {
                Log.Log("Initializing profile: " + profileActive + "...", LogLevel_DEBUG);
                profileInitialized = true;
                RegisterProfile();
                Log.Log("Profile activated: " + profileActive + "!", LogLevel_DEBUG);
            }
            if (!profileScreenshot) {
                Log.Log("Updating profile screenshot: " + profileActive + "...", LogLevel_DEBUG);
                profileScreenshot = UpdateScreenshot(windowActive, profileActive + ".jpg");
            }
            profiles[profileActive].name = profileActive;
            profiles[profileActive].hwnd = windowActive;
            return profiles[profileActive];
        } else {
            // Application inactive, create new screenshot on focus!
            profileScreenshot = false;
        }
    }
    return null;
}

function GetActiveProfileData() {
    return GetProfileData(profileActive);
}

function GetProfileData(name) {
    var result = {};
    if (typeof profiles[name] != "undefined") {
        result = profiles[name];
        result.active = (profileActive == name);
        result.screenshot = "/cache/" + name + ".jpg?nocache=" + GetEngineClock();
    }
    return JSON.stringify(result);
}

function UpdateScreenshot(hwnd, cacheFile) {
    var windowSize = GetWindowRect(hwnd);
    var filename = "/cache/" + cacheFile;
    var bitmap = CaptureScreen(hwnd, windowSize.Left, windowSize.Top, windowSize.Right, windowSize.Bottom);
    if (bitmap != null) {
        bitmap.Save("webinterface" + filename, ImageFormat_Jpeg);
        bitmap.Dispose();
        return true;
    }
    return false;
}

function GetConfigAsJson() {
    return JSON.stringify(configuration);
}

function GetConfigValue(name, defaultValue) {
    if (typeof configuration[name] == "undefined") {
        configuration[name] = defaultValue;
        return defaultValue;
    } else {
        return configuration[name];
    }
}

function SetConfigValue(name, value) {
    configuration[name] = value;
    if (name == "keyboardType") {
        LogitechKeyboard.SetKeyboardType(value);
    }
}

function SaveConfig() {
    ConfigJsonWrite(JSON.stringify(configuration));
}