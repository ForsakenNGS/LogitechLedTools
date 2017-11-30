(function () {
    var hotsProfile = {
        intervals: {
            updateScene: 2000,
            updateGame: 500
        },
        config: {
            mode: "full"
        },
        data: {
            scene: "Unknown",
            detail: "Unknown",
            status: "",
            debug: "",
            dirty: false,
            game: {
                health: 0,
                healthRound: 0,
                mana: 0,
                manaRound: 0,
                energy: 0,
                energyRound: 0,
                tooltipLastShown: 0,
                talentAvailable: true,
                animating: {
                    lowHp: false,
                    talent: false,
                    talentPaused: false
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
        // HOTS Menu buttons
        var condMenuButtonsBorder = updater.CreateConditionList()
            .AddCondition("B", ">", "R").AddCondition("B", ">", "G").AddCondition("R", "<", 128).AddCondition("G", "<", 128).AddCondition("B", ">", 70).AddCondition("B", "<", 200);
        var condMenuButtonsSymbol = updater.CreateConditionList()
            .AddCondition("B", ">", "R").AddCondition("B", ">", "G").AddCondition("R", "<", 128).AddCondition("G", "<", 180).AddCondition("B", ">", 180).AddCondition("B", "<", 220);
        var checkMenuButtons = updater.CreateCheck("base_menu_buttons");
        checkMenuButtons.AddPoint("P1", 0, 1650 / 1920, 0, 1069 / 1080, condMenuButtonsBorder);
        checkMenuButtons.AddPoint("P2", 0, 1700 / 1920, 0, 1069 / 1080, condMenuButtonsBorder);
        checkMenuButtons.AddPoint("P3", 0, 1750 / 1920, 0, 1069 / 1080, condMenuButtonsBorder);
        checkMenuButtons.AddPoint("P4", 0, 1800 / 1920, 0, 1069 / 1080, condMenuButtonsBorder);
        checkMenuButtons.AddPoint("P5", 0, 1745 / 1920, 0, 1055 / 1080, condMenuButtonsSymbol);
        // Ready check
        var condMatchSearchIcon = updater.CreateConditionList()
            .AddCondition("G", ">", "R").AddCondition("G", ">", "B").AddCondition("R", "<", 50).AddCondition("G", ">", 200).AddCondition("B", "<", 220);
        var checkMatchSearch = updater.CreateCheck("menu_ready_check", "Menu");
        checkMatchSearch.AddPoint("P1", 0, 1865 / 1920, 0, 42 / 1080, condMatchSearchIcon);
        checkMatchSearch.AddPoint("P2", 0, 1865 / 1920, 0, 60 / 1080, condMatchSearchIcon);
        checkMatchSearch.AddPoint("P3", 0, 1865 / 1920, 0, 72 / 1080, condMatchSearchIcon);
        checkMatchSearch.AddPoint("P4", 0, 1880 / 1920, 0, 46 / 1080, condMatchSearchIcon);
        // HP and MP bar
        var condBarsHealth = updater.CreateConditionList()
            .AddCondition("G", ">", "R").AddCondition("G", ">", "B").AddCondition("R", "<", 160).AddCondition("G", ">", 180);
        var condBarsMana = updater.CreateConditionList()
            .AddCondition("G", ">", "R").AddCondition("B", ">", "R").AddCondition("R", "<", 140).AddCondition("G", ">", 120).AddCondition("B", ">", 180);
        var condBarsEnergy = updater.CreateConditionList()
            .AddCondition("R", ">", "B").AddCondition("G", ">", "B").AddCondition("R", ">", 140).AddCondition("G", ">", 140);
        var checkBars = updater.CreateCheck("game_bars", "Game");
        checkBars.AddBar("health", 0, 208 / 1920, 0, 992 / 1080, 0, 414 / 1920, 0, 1010 / 1080, condBarsHealth);
        checkBars.AddBar("mana", 0, 198 / 1920, 0, 1010 / 1080, 0, 402 / 1920, 0, 1026 / 1080, condBarsMana);
        checkBars.AddBar("energy", 0, 198 / 1920, 0, 1010 / 1080, 0, 402 / 1920, 0, 1026 / 1080, condBarsEnergy);
        // Buff / Healing fountain tooltip
        var condTooltipBack = updater.CreateConditionList()
            .AddCondition("B", ">", "R").AddCondition("B", ">", "G").AddCondition("R", "<", 30).AddCondition("G", "<", 30).AddCondition("B", ">", 40).AddCondition("B", "<", 70);
        var checkTooltip = updater.CreateCheck("game_tooltip", "Game");
        checkTooltip.AddPoint("P01", 0, 370 / 1920, 0, 1025 / 1080, condTooltipBack);
        checkTooltip.AddPoint("P02", 0, 400 / 1920, 0, 1025 / 1080, condTooltipBack);
        // Talent notification
        var condTalentBlue = updater.CreateConditionList()
            .AddCondition("B", ">", "R").AddCondition("B", ">", "G").AddCondition("B", ">", 140);
        var condTalentWhite = updater.CreateConditionList()
            .AddCondition("R", ">", 170).AddCondition("G", ">", 170).AddCondition("B", ">", 170);
        var checkTalent = updater.CreateCheck("game_talent", "Game");
        var checkTalentBlueMin = 140;
        var checkTalentWhiteMin = 170;
        checkTalent.AddPoint("P01Blue", 0, 60 / 1920, 0, 1023 / 1080, condTalentBlue);
        checkTalent.AddPoint("P02Blue", 0, 70 / 1920, 0, 1023 / 1080, condTalentBlue);
        checkTalent.AddPoint("P03Blue", 0, 80 / 1920, 0, 1023 / 1080, condTalentBlue);
        checkTalent.AddPoint("P05Blue", 0, 90 / 1920, 0, 1023 / 1080, condTalentBlue);
        checkTalent.AddPoint("P06Blue", 0, 100 / 1920, 0, 1023 / 1080, condTalentBlue);
        checkTalent.AddPoint("P07Blue", 0, 110 / 1920, 0, 1023 / 1080, condTalentBlue);
        checkTalent.AddPoint("P08Blue", 0, 120 / 1920, 0, 1023 / 1080, condTalentBlue);
        checkTalent.AddPoint("P09Blue", 0, 130 / 1920, 0, 1023 / 1080, condTalentBlue);
        checkTalent.AddPoint("P10Blue", 0, 140 / 1920, 0, 1023 / 1080, condTalentBlue);
        checkTalent.AddPoint("P11Blue", 0, 150 / 1920, 0, 1023 / 1080, condTalentBlue);
        checkTalent.AddPoint("P12Blue", 0, 170 / 1920, 0, 1023 / 1080, condTalentBlue);
        checkTalent.AddPoint("P13Blue", 0, 170 / 1920, 0, 1023 / 1080, condTalentBlue);
        checkTalent.AddPoint("P01White", 0, 60 / 1920, 0, 1023 / 1080, condTalentWhite);
        checkTalent.AddPoint("P02White", 0, 70 / 1920, 0, 1023 / 1080, condTalentWhite);
        checkTalent.AddPoint("P03White", 0, 80 / 1920, 0, 1023 / 1080, condTalentWhite);
        checkTalent.AddPoint("P05White", 0, 90 / 1920, 0, 1023 / 1080, condTalentWhite);
        checkTalent.AddPoint("P06White", 0, 100 / 1920, 0, 1023 / 1080, condTalentWhite);
        checkTalent.AddPoint("P07White", 0, 110 / 1920, 0, 1023 / 1080, condTalentWhite);
        checkTalent.AddPoint("P08White", 0, 120 / 1920, 0, 1023 / 1080, condTalentWhite);
        checkTalent.AddPoint("P09White", 0, 130 / 1920, 0, 1023 / 1080, condTalentWhite);
        checkTalent.AddPoint("P10White", 0, 140 / 1920, 0, 1023 / 1080, condTalentWhite);
        checkTalent.AddPoint("P11White", 0, 150 / 1920, 0, 1023 / 1080, condTalentWhite);
        checkTalent.AddPoint("P12White", 0, 170 / 1920, 0, 1023 / 1080, condTalentWhite);
        checkTalent.AddPoint("P13White", 0, 170 / 1920, 0, 1023 / 1080, condTalentWhite);
    };

    // Update active scene (detect if in menu / game / ...)
    hotsProfile.updateScene = function (updater) {
        if (updater.GetPointPercentage("base_menu_buttons") > 0.75) {
            // Menu buttons found!
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
        var timeNow = GetEngineClock();
        if (scene == "Menu") {
            if (updater.GetPointPercentage("menu_ready_check") == 1) {
                hotsProfile.updateSceneDetail("Searching");
            } else {
                hotsProfile.updateSceneDetail("Default");
            }
        }
        if (scene == "Game") {
            var health = hotsProfile.data.game.health;
            var mana = hotsProfile.data.game.mana;
            var energy = hotsProfile.data.game.energy;
            if (updater.GetPointPercentage("game_tooltip") > 0) {
                hotsProfile.data.game.tooltipLastShown = timeNow;
            }
            if ((timeNow - hotsProfile.data.game.tooltipLastShown) > 1500) {
                health = updater.GetBarResult("game_bars", "health") * 100;
                mana = updater.GetBarResult("game_bars", "mana") * 100;
                energy = updater.GetBarResult("game_bars", "energy") * 100;
            }
            hotsProfile.updateSceneDetail("Default");
            hotsProfile.updateGameData(
                health, mana, energy, (updater.GetPointPercentage("game_talent") > 0.3)
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
            hotsProfile.data.game.healthRound = Math.round(health * 10) / 10;
            hotsProfile.data.game.mana = mana;
            hotsProfile.data.game.manaRound = Math.round(mana * 10) / 10;
            hotsProfile.data.game.energy = energy;
            hotsProfile.data.game.energyRound = Math.round(energy * 10) / 10;
            hotsProfile.data.game.talentAvailable = talentAvailable;
        }
    };

    // Update led display
    hotsProfile.display = function (profileData, profileConfig) {
        //LogitechKeyboard.Clear();
        if (profileData.dirty) {
            var timeNow = GetEngineClock();
            var statusHtml = "";
            if (profileData.scene == "Menu") {
                if (profileData.detail == "Searching") {
                    LogitechKeyboard.StartWaveAnimation(LogitechKeyboard.GetColor(0, 0, 0), LogitechKeyboard.GetColor(255, 255, 0), 1000, 2);
                } else {
                    LogitechKeyboard.SetLighting(LogitechKeyboard.GetColor(255, 255, 0));
                }
            } else if (profileData.scene == "Game") {
                // Generate status html for webinterface
                if (profileData.game.health > 0) {
                    statusHtml += "<strong>Health: " + Math.round(profileData.game.health) + "%</strong><br />";
                }
                if (profileData.game.mana > 0) {
                    statusHtml += "<strong>Mana: " + Math.round(profileData.game.mana) + "%</strong><br />";
                }
                // Update keyboard lightning
                if (LogitechKeyboard.IsPerKey() && (profileConfig.mode == "bars")) {
                    // Per key lightning
                    // - Health
                    if ((profileData.game.health > 0.1) && (profileData.game.health < 25)) {
                        if (!profileData.game.animating.lowHp) {
                            profileData.game.animating.lowHp = true;
                            LogitechKeyboard.SetKeyArea(LogitechKeyboard.GetColor(0, 60, 255), 0, 1, 19, 5);
                            LogitechKeyboard.StartWaveAnimation(LogitechKeyboard.GetColor(0, 0, 0), LogitechKeyboard.GetColor(255, 0, 0), 1000, 2, 0, 1, 19, 5);
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
                    LogitechKeyboard.SetKeyBar(KeyBar_F1_F12, LogitechKeyboard.GetColor(0, 255, 0), LogitechKeyboard.GetColor(255, 0, 0), profileData.game.health);
                    // - Mana / Energy
                    if (profileData.game.mana > 0.1) {
                        LogitechKeyboard.SetKeyBar(KeyBar_NUMPAD_BLOCK_A, LogitechKeyboard.GetColor(0, 0, 255), LogitechKeyboard.GetColor(0, 0, 0), profileData.game.mana);
                    } else if (profileData.game.energy > 0.1) {
                        LogitechKeyboard.SetKeyBar(KeyBar_NUMPAD_BLOCK_A, LogitechKeyboard.GetColor(255, 255, 0), LogitechKeyboard.GetColor(0, 0, 0), profileData.game.energy);
                    }
                } else {
                    // Global lightning
                    if ((profileData.game.health > 0.1) && (profileData.game.health < 25)) {
                        if (!profileData.game.animating.lowHp) {
                            profileData.game.animating.lowHp = true;
                            LogitechKeyboard.StartWaveAnimation(LogitechKeyboard.GetColor(0, 0, 0), LogitechKeyboard.GetColor(255, 0, 0), 1000, 3);
                        }
                    } else {
                        profileData.game.animating.lowHp = false;
                        if (profileData.game.animating.talentPaused < timeNow) {
                            profileData.game.animating.talentPaused = false;
                        }
                        if (!profileData.game.animating.talent) {
                            if (profileData.game.talentAvailable && !profileData.game.animating.talentPaused) {
                                profileData.game.animating.talent = timeNow + 10000;
                                profileData.game.animating.talentPaused = timeNow + 20000;
                                LogitechKeyboard.StartWaveAnimation(LogitechKeyboard.GetColor(0, 0, 0), LogitechKeyboard.GetColor(80, 80, 255), 1000, 3);
                            }
                        } else if (!profileData.game.talentAvailable) {
                            profileData.game.animating.talent = false;
                            profileData.game.animating.talentPaused = false;
                        }
                        if (!profileData.game.animating.talent || (profileData.game.animating.talent < timeNow)) {
                            LogitechKeyboard.SetLighting(LogitechKeyboard.GetColorFade(
                                profileData.game.health / 50,
                                LogitechKeyboard.GetColor(255, 0, 0), LogitechKeyboard.GetColor(255, 255, 0), LogitechKeyboard.GetColor(0, 255, 0)
                            ));
                            profileData.game.animating.talent = false;
                        }
                    }
                }
            } else {
                LogitechKeyboard.SetLighting(LogitechKeyboard.GetColor(255, 0, 0));
            }
            profileData.dirty = false;
            profileData.status = statusHtml;
        }
    };
    
    return hotsProfile;
})();