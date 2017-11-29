(function () {
    var overwatchProfile = {
        intervals: {
            updateScene: 2000,
            updateGame: 500
        },
        config: {
            mode: "bars"
        },
        data: {
            scene: "Unknown",
            detail: "Unknown",
            status: "",
            debug: "",
            dirty: false,
            game: {
                ultimateReady: false
            },
            lastUpdate: {
                scene: 0,
                game: 0,
            }
        }
    };

    // Register all checks for this profile
    overwatchProfile.register = function (updater) {
        // Profile box
        var condProfileBoxBlue = updater.CreateConditionList()
            .AddCondition("B", ">", "R").AddCondition("B", ">", "G").AddCondition("B", ">", 140);
        var condProfileBoxGreen = updater.CreateConditionList()
            .AddCondition("G", ">", "R").AddCondition("G", ">", "B").AddCondition("R", "<", 140).AddCondition("R", ">", 100).AddCondition("G", ">", 245).AddCondition("B", "<", 20);
        var checkProfileBox = updater.CreateCheck("base_profile_box");
        checkProfileBox.AddPoint("P1", 0, 1582 / 1920, 0, 34 / 1080, condProfileBox);
        checkProfileBox.AddPoint("P2", 0, 1582 / 1920, 0, 84 / 1080, condProfileBox);
        checkProfileBox.AddPoint("P3", 0, 1864 / 1920, 0, 34 / 1080, condProfileBox);
        checkProfileBox.AddPoint("P4", 0, 1864 / 1920, 0, 84 / 1080, condProfileBox);
        checkProfileBox.AddPoint("P5", 0, 1516 / 1920, 0, 60 / 1080, condProfileBoxGreen);
        // Overwatch logo
        var condLogoYellow = updater.CreateConditionList()
            .AddCondition("R", ">", "B").AddCondition("G", ">", "B").AddCondition("R", ">", 245).AddCondition("G", "<", 245).AddCondition("B", "<", 200);
        var condLogoBlue = updater.CreateConditionList()
            .AddCondition("R", "<", 210).AddCondition("G", "<", 210).AddCondition("B", ">", 210);
        var checkLogo = updater.CreateCheck("menu_logo", "Menu");
        checkLogo.AddPoint("P1", 0, 124 / 1920, 0, 54 / 1080, condLogoYellow);
        checkLogo.AddPoint("P2", 0, 328 / 1920, 0, 100 / 1080, condLogoYellow);
        checkLogo.AddPoint("P3", 0, 328 / 1920, 0, 68 / 1080, condLogoBlue);
        // Match search box
        var condMatchSearchBlue = updater.CreateConditionList()
            .AddCondition("B", ">", "R").AddCondition("B", ">", "G").AddCondition("R", "<", 90).AddCondition("G", "<", 90).AddCondition("B", "<", 110);
        var checkMatchSearch = updater.CreateCheck("menu_match_search", "Menu");
        checkMatchSearch.AddPoint("P1", 0, 812 / 1920, 0, 34 / 1080, condMatchSearchBlue);
        checkMatchSearch.AddPoint("P2", 0, 812 / 1920, 0, 102 / 1080, condMatchSearchBlue);
        checkMatchSearch.AddPoint("P3", 0, 1106 / 1920, 0, 34 / 1080, condMatchSearchBlue);
        checkMatchSearch.AddPoint("P4", 0, 1106 / 1920, 0, 102 / 1080, condMatchSearchBlue);
        // Ultimate check
        var condUltimateOuter = updater.CreateConditionList()
            .AddCondition("R", ">", 225).AddCondition("G", ">", 225).AddCondition("B", ">", 245);
        var checkUltimateOuter = updater.CreateCheck("game_ultimate_outer", "Game");
        checkUltimateOuter.AddPoint("P1", 0, 912 / 1920, 0, 912 / 1080, condUltimateOuter);
        checkUltimateOuter.AddPoint("P2", 0, 916 / 1920, 0, 912 / 1080, condUltimateOuter);
        checkUltimateOuter.AddPoint("P3", 0, 1004 / 1920, 0, 912 / 1080, condUltimateOuter);
        checkUltimateOuter.AddPoint("P4", 0, 1008 / 1920, 0, 912 / 1080, condUltimateOuter);
        var condUltimateInner = updater.CreateConditionList()
            .AddCondition("R", "<", 225).AddCondition("G", "<", 225).AddCondition("B", "<", 225);
        var checkUltimateInner = updater.CreateCheck("game_ultimate_inner", "Game");
        checkUltimateInner.AddPoint("P1", 0, 925 / 1920, 0, 910 / 1080, condUltimateInner);
        checkUltimateInner.AddPoint("P2", 0, 992 / 1920, 0, 910 / 1080, condUltimateInner);
    };

    // Update active scene (detect if in menu / game / ...)
    overwatchProfile.updateScene = function (updater) {
        if (updater.GetPointPercentage("base_profile_box") == 1) {
            // Profile box found!
            overwatchProfile.updateSceneBase("Menu");
            return "Menu";
        } else {
            // In game!
            overwatchProfile.updateSceneBase("Game");
            return "Game";
        }
    }

    // Set the base screen
    overwatchProfile.updateSceneBase = function (scene) {
        if (overwatchProfile.data.scene != scene) {
            overwatchProfile.data.dirty = true;
            overwatchProfile.data.scene = scene;
        }
    }

    // Update the scene-dependend checks
    overwatchProfile.updateResult = function (updater, scene) {
        if (scene == "Menu") {
            if (updater.GetPointPercentage("menu_match_search") == 1) {
                overwatchProfile.updateSceneDetail("Searching");
            } else {
                overwatchProfile.updateSceneDetail("Default");
            }
        }
        if (scene == "Game") {
            overwatchProfile.updateSceneDetail("Default");
            if ((updater.GetPointPercentage("game_ultimate_outer") >= 0.5) && (updater.GetPointPercentage("game_ultimate_inner") == 1)) {
                overwatchProfile.updateGameData(true);
            } else {
                overwatchProfile.updateGameData(false);
            }
        }
    }

    // Set the scene detail name
    overwatchProfile.updateSceneDetail = function (detail) {
        if (overwatchProfile.data.detail != detail) {
            overwatchProfile.data.dirty = true;
            overwatchProfile.data.detail = detail;
        }
    }

    // Set the game data
    overwatchProfile.updateGameData = function (ultimateReady) {
        if (overwatchProfile.data.game.ultimateReady != ultimateReady) {
            overwatchProfile.data.dirty = true;
            overwatchProfile.data.game.ultimateReady = ultimateReady;
        }
    };

    // Update led display
    overwatchProfile.display = function (profileData) {
        //LogitechKeyboard.Clear();
        if (profileData.dirty) {
            var statusHtml = "";
            if (profileData.scene == "Menu") {
                if (profileData.detail == "Searching") {
                    LogitechKeyboard.StartWaveAnimation(LogitechKeyboard.GetColor(255, 255, 0), LogitechKeyboard.GetColor(0, 0, 0), 2000, 2);
                } else {
                    LogitechKeyboard.SetLighting(LogitechKeyboard.GetColor(255, 255, 0));
                }
            } else if (profileData.scene == "Game") {
                // Generate status html for webinterface
                if (profileData.game.ultimateReady) {
                    statusHtml += "<strong>Ultimate ready!</strong><br />";
                }
                // Update keyboard lightning
                if (profileData.game.ultimateReady) {
                    LogitechKeyboard.StartWaveAnimation(LogitechKeyboard.GetColor(0, 0, 255), LogitechKeyboard.GetColor(255, 255, 0), 1000, 1);
                } else {
                    LogitechKeyboard.SetLighting(LogitechKeyboard.GetColor(255, 255, 0));
                }
            } else {
                LogitechKeyboard.SetLighting(LogitechKeyboard.GetColor(255, 0, 0));
            }
            profileData.dirty = false;
            profileData.status = statusHtml;
        }
    };

    return overwatchProfile;
})();