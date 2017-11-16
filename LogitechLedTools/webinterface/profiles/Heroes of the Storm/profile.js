(function () {
    var hotsProfile = {
        intervals: {
            updateScene: 2000,
            updateGame: 500
        },
        data: {
            scene: "Unknown",
            detail: "Unknown",
            status: "",
            debug: "",
            dirty: false,
            game: {
                health: 0,
                mana: 0,
                energy: 0,
                talentAvailable: true,
                animating: {
                    lowHp: false,
                    talent: false
                }
            },
            lastUpdate: {
                scene: 0,
                game: 0,
            }
        }
    };

    // Register all checks for this profile
    hotsProfile.register = function (updater) {
        // HOTS logo
        var checkLogo = updater.CreateCheck("base_logo");
        checkLogo.AddPoint("P1", 0, 44 / 1920, 0, 22 / 1080)
            .AddCondition("B", ">", "R").AddCondition("R", ">", "G").AddCondition("R", "<", 160).AddCondition("G", "<", 50).AddCondition("B", ">", 40);
        checkLogo.AddPoint("P2", 0, 52 / 1920, 0, 44 / 1080)
            .AddCondition("B", ">", "R").AddCondition("R", ">", "G").AddCondition("R", "<", 160).AddCondition("G", "<", 50).AddCondition("B", ">", 40);
        checkLogo.AddPoint("P3", 0, 30 / 1920, 0, 38 / 1080)
            .AddCondition("B", ">", "R").AddCondition("R", ">", "G").AddCondition("R", "<", 160).AddCondition("G", "<", 50).AddCondition("B", ">", 40);
        checkLogo.AddPoint("P4", 0, 42 / 1920, 0, 30 / 1080)
            .AddCondition("B", ">", "R").AddCondition("R", ">", "G").AddCondition("R", ">", 150).AddCondition("G", ">", 150).AddCondition("B", ">", 210);
        // Match search box
        var checkMatchSearch = updater.CreateCheck("menu_match_search", "Menu");
        checkMatchSearch.AddPoint("P1", 0, 856 / 1920, 0, 1030 / 1080)
            .AddCondition("B", ">", "R").AddCondition("R", ">", "G").AddCondition("R", ">", 100).AddCondition("G", ">", 100).AddCondition("B", ">", 150);
        checkMatchSearch.AddPoint("P2", 0, 1063 / 1920, 0, 1030 / 1080)
            .AddCondition("B", ">", "R").AddCondition("R", ">", "G").AddCondition("R", ">", 100).AddCondition("G", ">", 100).AddCondition("B", ">", 150);
        checkMatchSearch.AddPoint("P3", 0, 960 / 1920, 0, 1020 / 1080)
            .AddCondition("B", ">", "R").AddCondition("R", ">", "G").AddCondition("R", ">", 100).AddCondition("G", ">", 100).AddCondition("B", ">", 150);
        checkMatchSearch.AddPoint("P4", 0, 960 / 1920, 0, 1048 / 1080)
            .AddCondition("B", ">", "R").AddCondition("R", ">", "G").AddCondition("R", "<", 70).AddCondition("G", "<", 40).AddCondition("B", ">", 80);
        // HP and MP bar
        var checkBars = updater.CreateCheck("game_bars", "Game");
        checkBars.AddBar("health", 0, 208 / 1920, 0, 992 / 1080, 0, 414 / 1920, 0, 1010 / 1080)
            .AddCondition("G", ">", "R").AddCondition("G", ">", "B").AddCondition("R", "<", 160).AddCondition("G", ">", 180);
        checkBars.AddBar("mana", 0, 198 / 1920, 0, 1010 / 1080, 0, 404 / 1920, 0, 1026 / 1080)
            .AddCondition("G", ">", "R").AddCondition("B", ">", "R").AddCondition("R", "<", 140).AddCondition("G", ">", 120).AddCondition("B", ">", 180);
        checkBars.AddBar("energy", 0, 198 / 1920, 0, 1010 / 1080, 0, 404 / 1920, 0, 1026 / 1080)
            .AddCondition("R", ">", "B").AddCondition("G", ">", "B").AddCondition("R", ">", 140).AddCondition("G", ">", 140);
        // Talent notification
        var checkTalent = updater.CreateCheck("game_talent", "Game");
        checkTalent.AddPoint("P01Blue", 0, 60 / 1920, 0, 1023 / 1080).AddCondition("B", ">", "R").AddCondition("B", ">", "G").AddCondition("B", ">", 140);
        checkTalent.AddPoint("P02Blue", 0, 70 / 1920, 0, 1023 / 1080).AddCondition("B", ">", "R").AddCondition("B", ">", "G").AddCondition("B", ">", 140);
        checkTalent.AddPoint("P03Blue", 0, 80 / 1920, 0, 1023 / 1080).AddCondition("B", ">", "R").AddCondition("B", ">", "G").AddCondition("B", ">", 140);
        checkTalent.AddPoint("P05Blue", 0, 90 / 1920, 0, 1023 / 1080).AddCondition("B", ">", "R").AddCondition("B", ">", "G").AddCondition("B", ">", 140);
        checkTalent.AddPoint("P06Blue", 0, 100 / 1920, 0, 1023 / 1080).AddCondition("B", ">", "R").AddCondition("B", ">", "G").AddCondition("B", ">", 140);
        checkTalent.AddPoint("P07Blue", 0, 110 / 1920, 0, 1023 / 1080).AddCondition("B", ">", "R").AddCondition("B", ">", "G").AddCondition("B", ">", 140);
        checkTalent.AddPoint("P08Blue", 0, 120 / 1920, 0, 1023 / 1080).AddCondition("B", ">", "R").AddCondition("B", ">", "G").AddCondition("B", ">", 140);
        checkTalent.AddPoint("P09Blue", 0, 130 / 1920, 0, 1023 / 1080).AddCondition("B", ">", "R").AddCondition("B", ">", "G").AddCondition("B", ">", 140);
        checkTalent.AddPoint("P10Blue", 0, 140 / 1920, 0, 1023 / 1080).AddCondition("B", ">", "R").AddCondition("B", ">", "G").AddCondition("B", ">", 140);
        checkTalent.AddPoint("P11Blue", 0, 150 / 1920, 0, 1023 / 1080).AddCondition("B", ">", "R").AddCondition("B", ">", "G").AddCondition("B", ">", 140);
        checkTalent.AddPoint("P12Blue", 0, 170 / 1920, 0, 1023 / 1080).AddCondition("B", ">", "R").AddCondition("B", ">", "G").AddCondition("B", ">", 140);
        checkTalent.AddPoint("P13Blue", 0, 170 / 1920, 0, 1023 / 1080).AddCondition("B", ">", "R").AddCondition("B", ">", "G").AddCondition("B", ">", 140);
        checkTalent.AddPoint("P01White", 0, 60 / 1920, 0, 1023 / 1080).AddCondition("R", ">", 180).AddCondition("G", ">", 180).AddCondition("B", ">", 180);
        checkTalent.AddPoint("P02White", 0, 70 / 1920, 0, 1023 / 1080).AddCondition("R", ">", 180).AddCondition("G", ">", 180).AddCondition("B", ">", 180);
        checkTalent.AddPoint("P03White", 0, 80 / 1920, 0, 1023 / 1080).AddCondition("R", ">", 180).AddCondition("G", ">", 180).AddCondition("B", ">", 180);
        checkTalent.AddPoint("P05White", 0, 90 / 1920, 0, 1023 / 1080).AddCondition("R", ">", 180).AddCondition("G", ">", 180).AddCondition("B", ">", 180);
        checkTalent.AddPoint("P06White", 0, 100 / 1920, 0, 1023 / 1080).AddCondition("R", ">", 180).AddCondition("G", ">", 180).AddCondition("B", ">", 180);
        checkTalent.AddPoint("P07White", 0, 110 / 1920, 0, 1023 / 1080).AddCondition("R", ">", 180).AddCondition("G", ">", 180).AddCondition("B", ">", 180);
        checkTalent.AddPoint("P08White", 0, 120 / 1920, 0, 1023 / 1080).AddCondition("R", ">", 180).AddCondition("G", ">", 180).AddCondition("B", ">", 180);
        checkTalent.AddPoint("P09White", 0, 130 / 1920, 0, 1023 / 1080).AddCondition("R", ">", 180).AddCondition("G", ">", 180).AddCondition("B", ">", 180);
        checkTalent.AddPoint("P10White", 0, 140 / 1920, 0, 1023 / 1080).AddCondition("R", ">", 180).AddCondition("G", ">", 180).AddCondition("B", ">", 180);
        checkTalent.AddPoint("P11White", 0, 150 / 1920, 0, 1023 / 1080).AddCondition("R", ">", 180).AddCondition("G", ">", 180).AddCondition("B", ">", 180);
        checkTalent.AddPoint("P12White", 0, 170 / 1920, 0, 1023 / 1080).AddCondition("R", ">", 180).AddCondition("G", ">", 180).AddCondition("B", ">", 180);
        checkTalent.AddPoint("P13White", 0, 170 / 1920, 0, 1023 / 1080).AddCondition("R", ">", 180).AddCondition("G", ">", 180).AddCondition("B", ">", 180);

    };

    // Update active scene (detect if in menu / game / ...)
    hotsProfile.updateScene = function (updater) {
        if (updater.GetPointPercentage("base_logo") == 1) {
            // Logo found!
            hotsProfile.updateSceneBase("Menu");
            return "Menu";
        } else {
            // In game!
            hotsProfile.updateSceneBase("Game");
            return "Game";
        }
    }

    // Set the base screen
    hotsProfile.updateSceneBase = function (scene) {
        if (hotsProfile.data.scene != scene) {
            hotsProfile.data.dirty = true;
            hotsProfile.data.scene = scene;
        }
    }

    // Update the scene-dependend checks
    hotsProfile.updateResult = function (updater, scene) {
        if (scene == "Menu") {
            if (updater.GetPointPercentage("menu_match_search") == 1) {
                hotsProfile.updateSceneDetail("Searching");
            } else {
                hotsProfile.updateSceneDetail("Default");
            }
        }
        if (scene == "Game") {
            hotsProfile.updateSceneDetail("Default");
            hotsProfile.updateGameData(
                updater.GetBarResult("game_bars", "health"), updater.GetBarResult("game_bars", "mana"), updater.GetBarResult("game_bars", "energy"),
                (updater.GetPointPercentage("game_talent") > 0.3)
            );
            //Console_WriteLine("Debug talent: " + updater.GetPointPercentage("game_talent"));
            //updater.DebugCheck("game_talent");
        }
    }

    // Set the scene detail name
    hotsProfile.updateSceneDetail = function (detail) {
        if (hotsProfile.data.detail != detail) {
            hotsProfile.data.dirty = true;
            hotsProfile.data.detail = detail;
        }
    }

    // Set the game data
    hotsProfile.updateGameData = function (health, mana, energy, talentAvailable) {
        if ((hotsProfile.data.game.health != health) || (hotsProfile.data.game.mana != mana) || (hotsProfile.data.game.energy != energy)
            || (hotsProfile.data.game.talentAvailable != talentAvailable)) {
            hotsProfile.data.dirty = true;
            hotsProfile.data.game.health = health;
            hotsProfile.data.game.mana = mana;
            hotsProfile.data.game.energy = energy;
            hotsProfile.data.game.talentAvailable = talentAvailable;
        }
    };

    // Update led display
    hotsProfile.display = function (profileData) {
        //LogitechKeyboard.Clear();
        if (profileData.dirty) {
            var statusHtml = "";
            if (profileData.scene == "Menu") {
                if (profileData.detail == "Searching") {
                    LogitechKeyboard.StartWaveAnimation(LogitechKeyboard.GetColor(255, 255, 0), LogitechKeyboard.GetColor(0, 0, 0), 2000, 2);
                } else {
                    LogitechKeyboard.SetGlobalColor(LogitechKeyboard.GetColor(255, 255, 0));
                }
            } else if (profileData.scene == "Game") {
                // Generate status html for webinterface
                if (profileData.game.health > 0) {
                    statusHtml += "<strong>Health: " + Math.round(profileData.game.health * 100) + "%</strong><br />";
                }
                if (profileData.game.mana > 0) {
                    statusHtml += "<strong>Mana: " + Math.round(profileData.game.mana * 100) + "%</strong><br />";
                }
                // Update keyboard lightning
                if (LogitechKeyboard.IsPerKey()) {
                    // Per key lightning
                    // - Health
                    if ((profileData.game.health > 0.01) && (profileData.game.health < 0.3)) {
                        if (!profileData.game.animating.lowHp) {
                            profileData.game.lowHp = true;
                            LogitechKeyboard.SetKeyArea(LogitechKeyboard.GetColor(0, 60, 255), 0, 1, 19, 5);
                            LogitechKeyboard.StartWaveAnimation(LogitechKeyboard.GetColor(255, 0, 0), LogitechKeyboard.GetColor(0, 0, 0), 1000, 2, 0, 1, 19, 5);
                        }
                    } else {
                        if (profileData.game.animating.lowHp) {
                            profileData.game.animating.lowHp = false;
                            LogitechKeyboard.SetKeyArea(LogitechKeyboard.GetColor(0, 60, 255), 0, 1, 19, 5);
                        }
                        if (profileData.game.animating.talent) {
                            if (!profileData.game.talentAvailable) {
                                profileData.game.animating.talent = false;
                                LogitechKeyboard.SetKeyArea(LogitechKeyboard.GetColor(0, 60, 255), 0, 1, 19, 5);
                            }
                        } else if (profileData.game.talentAvailable) {
                            profileData.game.animating.talent = true;
                            LogitechKeyboard.SetKeyArea(LogitechKeyboard.GetColor(0, 60, 255), 0, 1, 19, 5);
                            LogitechKeyboard.StartWaveAnimation(LogitechKeyboard.GetColor(255, 255, 255), LogitechKeyboard.GetColor(0, 0, 0), 1000, 4, 0, 1, 19, 5);
                        }
                    }
                    LogitechKeyboard.SetKeyBar(KeyBar_F1_F12, LogitechKeyboard.GetColor(0, 255, 0), LogitechKeyboard.GetColor(255, 0, 0), profileData.game.health * 100);
                    // - Mana / Energy
                    if (profileData.game.mana > 0.01) {
                        LogitechKeyboard.SetKeyBar(KeyBar_NUMPAD_BLOCK_A, LogitechKeyboard.GetColor(0, 0, 255), LogitechKeyboard.GetColor(0, 0, 0), profileData.game.mana * 100);
                    } else if (profileData.game.energy > 0.01) {
                        LogitechKeyboard.SetKeyBar(KeyBar_NUMPAD_BLOCK_A, LogitechKeyboard.GetColor(255, 255, 0), LogitechKeyboard.GetColor(0, 0, 0), profileData.game.energy * 100);
                    }
                } else {
                    // Global lightning
                    if ((profileData.game.health > 0.01) && (profileData.game.health < 0.3)) {
                        if (!profileData.game.lowHp) {
                            profileData.game.lowHp = true;
                            LogitechKeyboard.StartWaveAnimation(LogitechKeyboard.GetColor(0, 0, 0), LogitechKeyboard.GetColor(255, 0, 0), 300);
                        }
                    } else {
                        profileData.game.lowHp = false;
                        LogitechKeyboard.SetGlobalColor(LogitechKeyboard.GetColorFade(
                            profileData.game.health * 2,
                            LogitechKeyboard.GetColor(255, 0, 0), LogitechKeyboard.GetColor(255, 255, 0), LogitechKeyboard.GetColor(0, 255, 0)
                        ))
                    }
                }
            } else {
                LogitechKeyboard.SetGlobalColor(LogitechKeyboard.GetColor(255, 0, 0));
            }
            profileData.dirty = false;
            profileData.status = statusHtml;
        }
    };
    
    return hotsProfile;
})();