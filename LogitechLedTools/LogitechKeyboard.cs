using LedCSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace LogitechLedTools
{
    public enum LogitechKeyboardType
    {
        MonochromeFull = 0, ColoredFull = 1, PerKey = 2
    };
    public enum KeyBar
    {
        F1_F12 = 0, F1_F8 = 1, F1_F4 = 2,
        F5_F12 = 3, F5_F8 = 4,
        F9_F12 = 5,
        NUMPAD_CIRCLE = 10,
        NUMPAD_BLOCK_A = 20, NUMPAD_BLOCK_B = 21,
    };

    public class LogitechKeyStatus
    {
        public bool effectActive { get; set; }
        public long lastEvent { get; set; }

        public LogitechKeyStatus()
        {
            effectActive = false;
            lastEvent = 0;
        }
    }

    public class LogitechKeyboard
    {
        private const long EventPadding = 60;

        private LogitechKeyboardType keyboardType;
        private LogitechKeyStatus statusGlobal;
        private IDictionary<keyboardNames, LogitechKeyStatus> statusKey;
        private ConcurrentQueue<LogitechLightningEvent> eventQueue;

        private Stopwatch timerEvents;
        private IDictionary<KeyBar, IList<int>> keyBarList;
        private keyboardNames[] keyGrid;

        public enum LightningType
        {
            NORMAL, FLASHING, PULSING
        }

        public static LogitechColor GetColor(int R, int G, int B)
        {
            return new LogitechColor(Color.FromArgb(R, G, B));
        }

        public LogitechKeyboard()
        {
            keyboardType = LogitechKeyboardType.PerKey;
            statusGlobal = new LogitechKeyStatus();
            statusKey = new Dictionary<keyboardNames, LogitechKeyStatus>();
            Array keys = Enum.GetValues(typeof(keyboardNames));
            foreach (keyboardNames key in keys)
            {
                statusKey[key] = new LogitechKeyStatus();
            }
            eventQueue = new ConcurrentQueue<LogitechLightningEvent>();
            timerEvents = Stopwatch.StartNew();
            keyBarList = new Dictionary<KeyBar, IList<int>>();
            InitKeyGrid();
        }

        private void AddEvent(LogitechLightningEvent element)
        {
            AddEvent(element, true);
        }

        private void AddEvent(LogitechLightningEvent element, bool queue)
        {
            if (queue)
            {
                element.time = GetNextQueueTime(element.key);
            }
            if ((element.key == 0) && (element.time > statusGlobal.lastEvent))
            {
                statusGlobal.lastEvent = element.time;
            }
            if ((element.key > 0) && (element.time > statusKey[element.key].lastEvent))
            {
                statusKey[element.key].lastEvent = element.time;
            }
            eventQueue.Enqueue(element);
        }

        private void AddEvents(ICollection<LogitechLightningEvent> elements)
        {
            long timeEnd = elements.Max(item => item.time);
            long timeQueue = GetNextQueueTime();
            long timeDelta = 0;
            if ((timeEnd > 0) && (timeEnd < timeQueue))
            {
                // Prevent event batches to overlap
                timeDelta = timeQueue - timeEnd + 200;
            }
            foreach (LogitechLightningEvent element in elements.OrderBy(item => item.time))
            {
                element.time += timeDelta;
                AddEvent(element, false);
            }
        }

        public long GetNextQueueTime()
        {
            if (statusGlobal.lastEvent > timerEvents.ElapsedMilliseconds)
            {
                return statusGlobal.lastEvent + EventPadding;
            } else
            {
                return timerEvents.ElapsedMilliseconds;
            }
        }

        public long GetNextQueueTime(keyboardNames key)
        {
            if ((key == 0) || (statusGlobal.lastEvent > (statusKey[key].lastEvent + EventPadding)))
            {
                return GetNextQueueTime();
            }
            else if (statusKey[key].lastEvent > timerEvents.ElapsedMilliseconds)
            {
                return statusKey[key].lastEvent + EventPadding;
            }
            else
            {
                return timerEvents.ElapsedMilliseconds;
            }
        }

        public void _Clear()
        {
            eventQueue = new ConcurrentQueue<LogitechLightningEvent>();
            timerEvents = Stopwatch.StartNew();
        }

        private void _ClearEffects()
        {
            if (statusGlobal.effectActive)
            {
                LogitechGSDK.LogiLedStopEffects();
                statusGlobal.effectActive = false;
            }
        }

        private void _ClearEffectsOnKey(keyboardNames key)
        {
            if (statusKey[key].effectActive)
            {
                LogitechGSDK.LogiLedStopEffectsOnKey(key);
                statusKey[key].effectActive = false;
            }
        }

        private void _SetLighting(int red, int green, int blue)
        {
            _ClearEffects();
            LogitechGSDK.LogiLedSetLighting(red, green, blue);
        }

        private void _SetLightingOnKey(keyboardNames key, int red, int green, int blue)
        {
            _ClearEffectsOnKey(key);
            LogitechGSDK.LogiLedSetLightingForKeyWithKeyName(key, red, green, blue);
        }

        private void _FlashLighting(int red, int green, int blue, int duration, int interval)
        {
            _ClearEffects();
            LogitechGSDK.LogiLedFlashLighting(red, green, blue, duration, interval);
            statusGlobal.effectActive = true;
        }

        private void _FlashLightingOnKey(keyboardNames key, int red, int green, int blue, int duration, int interval)
        {
            _ClearEffectsOnKey(key);
            LogitechGSDK.LogiLedFlashSingleKey(key, red, green, blue, duration, interval);
            statusKey[key].effectActive = true;
            statusGlobal.effectActive = true;
        }

        private void _PulseLighting(int red, int green, int blue, int duration, int interval)
        {
            _ClearEffects();
            LogitechGSDK.LogiLedPulseLighting(red, green, blue, duration, interval);
            statusGlobal.effectActive = true;
        }

        private void _PulseLightingOnKey(keyboardNames key, int redStart, int greenStart, int blueStart, int redEnd, int greenEnd, int blueEnd, int duration, bool isInfinite)
        {
            _ClearEffectsOnKey(key);
            LogitechGSDK.LogiLedPulseSingleKey(key, redStart, greenStart, blueStart, redEnd, greenEnd, blueEnd, duration, isInfinite);
            statusKey[key].effectActive = true;
            statusGlobal.effectActive = true;
        }

        private void _RestoreLighting()
        {
            _ClearEffects();
            LogitechGSDK.LogiLedRestoreLighting();
        }

        private void _RestoreLightingOnKey(keyboardNames key)
        {
            _ClearEffectsOnKey(key);
            LogitechGSDK.LogiLedRestoreLightingForKey(key);
        }

        private void _SaveCurrentLighting()
        {
            LogitechGSDK.LogiLedSaveCurrentLighting();
        }

        private void _SaveCurrentLightingOnKey(keyboardNames key)
        {
            LogitechGSDK.LogiLedSaveLightingForKey(key);
        }

        private void HandleEvents()
        {
            // Handle queued events
            long timeNow = timerEvents.ElapsedMilliseconds;
            LogitechLightningEvent nextEvent = new LogitechLightningEvent();
            while (eventQueue.TryPeek(out nextEvent)) {
                if ((nextEvent.time > timeNow) || !eventQueue.TryDequeue(out nextEvent))
                {
                    // Event is not due yet or dequeue failed! Stop execution.
                    return;
                }
                // Execute next event
                HandleEvent(nextEvent);
            }
        }

        private void HandleEvent(LogitechLightningEvent element)
        {
            switch (keyboardType)
            {
                case LogitechKeyboardType.MonochromeFull:
                case LogitechKeyboardType.ColoredFull:
                    switch (element.type)
                    {
                        case LogitechLightningEvent.Type.CLEAR_EFFECTS:
                            _ClearEffects();
                            break;
                        case LogitechLightningEvent.Type.CLEAR_EFFECTS_KEY:
                            _ClearEffectsOnKey(element.key);
                            break;
                        case LogitechLightningEvent.Type.SET_GLOBAL:
                            _SetLighting(element.colorStart.getRedPercentage(), element.colorStart.getGreenPercentage(), element.colorStart.getBluePercentage());
                            break;
                        case LogitechLightningEvent.Type.FLASH_GLOBAL:
                            _FlashLighting(
                                element.colorStart.getRedPercentage(), element.colorStart.getGreenPercentage(), element.colorStart.getBluePercentage(),
                                (element.duration < 0 ? LogitechGSDK.LOGI_LED_DURATION_INFINITE : element.duration), element.interval
                            );
                            break;
                        case LogitechLightningEvent.Type.PULSE_GLOBAL:
                            _SetLighting(element.colorStart.getRedPercentage(), element.colorStart.getGreenPercentage(), element.colorStart.getBluePercentage());
                            _PulseLighting(
                                element.colorEnd.getRedPercentage(), element.colorEnd.getGreenPercentage(), element.colorEnd.getBluePercentage(), 
                                (element.duration < 0 ? LogitechGSDK.LOGI_LED_DURATION_INFINITE : element.duration), element.interval / 10
                            );
                            break;
                        case LogitechLightningEvent.Type.RESTORE_GLOBAL:
                            _RestoreLighting();
                            break;
                        case LogitechLightningEvent.Type.SAVE_GLOBAL:
                            _SaveCurrentLighting();
                            break;
                    }
                    break;
                case LogitechKeyboardType.PerKey:
                    switch (element.type)
                    {
                        case LogitechLightningEvent.Type.CLEAR_EFFECTS:
                            _ClearEffects();
                            break;
                        case LogitechLightningEvent.Type.CLEAR_EFFECTS_KEY:
                            _ClearEffectsOnKey(element.key);
                            break;
                        case LogitechLightningEvent.Type.SET_KEY:
                            _SetLightingOnKey(element.key, element.colorStart.getRedPercentage(), element.colorStart.getGreenPercentage(), element.colorStart.getBluePercentage());
                            break;
                        case LogitechLightningEvent.Type.FLASH_KEY:
                            _FlashLightingOnKey(element.key, element.colorStart.getRedPercentage(), element.colorStart.getGreenPercentage(), element.colorStart.getBluePercentage(), element.duration, element.interval);
                            break;
                        case LogitechLightningEvent.Type.PULSE_KEY:
                            _PulseLightingOnKey(
                                element.key,
                                element.colorStart.getRedPercentage(), element.colorStart.getGreenPercentage(), element.colorStart.getBluePercentage(),
                                element.colorEnd.getRedPercentage(), element.colorEnd.getGreenPercentage(), element.colorEnd.getBluePercentage(), 
                                element.duration, (element.interval == -1)
                            );
                            break;
                        case LogitechLightningEvent.Type.RESTORE_KEY:
                            _RestoreLightingOnKey(element.key);
                            break;
                        case LogitechLightningEvent.Type.SAVE_KEY:
                            _SaveCurrentLightingOnKey(element.key);
                            break;
                        case LogitechLightningEvent.Type.SET_GLOBAL:
                            {
                                Array keys = Enum.GetValues(typeof(keyboardNames));
                                foreach (keyboardNames key in keys)
                                {
                                    _SetLightingOnKey(key, element.colorStart.getRedPercentage(), element.colorStart.getGreenPercentage(), element.colorStart.getBluePercentage());
                                }
                            }
                            break;
                        case LogitechLightningEvent.Type.FLASH_GLOBAL:
                            {
                                Array keys = Enum.GetValues(typeof(keyboardNames));
                                foreach (keyboardNames key in keys)
                                {
                                    _FlashLightingOnKey(
                                        key, element.colorStart.getRedPercentage(), element.colorStart.getGreenPercentage(), element.colorStart.getBluePercentage(),
                                        (element.duration < 0 ? LogitechGSDK.LOGI_LED_DURATION_INFINITE : element.duration), element.interval
                                    );
                                }
                            }
                            break;
                        case LogitechLightningEvent.Type.PULSE_GLOBAL:
                            {
                                Array keys = Enum.GetValues(typeof(keyboardNames));
                                foreach (keyboardNames key in keys)
                                {
                                    _SetLightingOnKey(key, element.colorStart.getRedPercentage(), element.colorStart.getGreenPercentage(), element.colorStart.getBluePercentage());
                                    _PulseLightingOnKey(
                                        key,
                                        element.colorStart.getRedPercentage(), element.colorStart.getGreenPercentage(), element.colorStart.getBluePercentage(),
                                        element.colorEnd.getRedPercentage(), element.colorEnd.getGreenPercentage(), element.colorEnd.getBluePercentage(),
                                        element.duration, (element.interval == -1)
                                    );
                                }
                            }
                            break;
                        case LogitechLightningEvent.Type.RESTORE_GLOBAL:
                            _RestoreLighting();
                            break;
                        case LogitechLightningEvent.Type.SAVE_GLOBAL:
                            _SaveCurrentLighting();
                            break;
                    }
                    break;
            }
        }

        public void EnforceKeyBounds(ref int left, ref int top, ref int right, ref int bottom)
        {
            // Enforce boundarys
            if (left < 0)
            {
                left = 0;
            }
            if (top < 0)
            {
                top = 0;
            }
            if (right > 23)
            {
                right = 23;
            }
            if (bottom > 5)
            {
                bottom = 5;
            }
        }

        public LogitechColor GetColorFade(double value, params LogitechColor[] colors)
        {
            int valueLower = Convert.ToInt32(Math.Floor(value));
            int valueUpper = Convert.ToInt32(Math.Ceiling(value));
            if (valueLower >= colors.Length)
            {
                valueLower = colors.Length - 2;
            }
            if (valueLower < 0)
            {
                valueLower = 0;
            }
            if (valueUpper <= valueLower)
            {
                valueUpper = valueLower + 1;
            }
            if (valueUpper >= colors.Length)
            {
                valueUpper = colors.Length - 1;
            }
            int valueFactor = Convert.ToInt32(value * 100) % 100;
            return LogitechColor.MixColors(colors[valueUpper], colors[valueLower], valueFactor, false);
        }

        public IList<int> GetKeyList(KeyBar layout)
        {
            if (keyBarList.ContainsKey(layout))
            {
                return keyBarList[layout];
            }
            IList<int> keyList = new List<int>();
            switch (layout)
            {
                case KeyBar.F1_F8:
                    keyList.Add((int)keyboardNames.F1);
                    keyList.Add((int)keyboardNames.F2);
                    keyList.Add((int)keyboardNames.F3);
                    keyList.Add((int)keyboardNames.F4);
                    keyList.Add((int)keyboardNames.F5);
                    keyList.Add((int)keyboardNames.F6);
                    keyList.Add((int)keyboardNames.F7);
                    keyList.Add((int)keyboardNames.F8);
                    break;
                case KeyBar.F5_F12:
                    keyList.Add((int)keyboardNames.F5);
                    keyList.Add((int)keyboardNames.F6);
                    keyList.Add((int)keyboardNames.F7);
                    keyList.Add((int)keyboardNames.F8);
                    keyList.Add((int)keyboardNames.F9);
                    keyList.Add((int)keyboardNames.F10);
                    keyList.Add((int)keyboardNames.F11);
                    keyList.Add((int)keyboardNames.F12);
                    break;
                case KeyBar.F1_F12:
                    keyList.Add((int)keyboardNames.F1);
                    keyList.Add((int)keyboardNames.F2);
                    keyList.Add((int)keyboardNames.F3);
                    keyList.Add((int)keyboardNames.F4);
                    keyList.Add((int)keyboardNames.F5);
                    keyList.Add((int)keyboardNames.F6);
                    keyList.Add((int)keyboardNames.F7);
                    keyList.Add((int)keyboardNames.F8);
                    keyList.Add((int)keyboardNames.F9);
                    keyList.Add((int)keyboardNames.F10);
                    keyList.Add((int)keyboardNames.F11);
                    keyList.Add((int)keyboardNames.F12);
                    break;
                case KeyBar.F1_F4:
                    keyList.Add((int)keyboardNames.F1);
                    keyList.Add((int)keyboardNames.F2);
                    keyList.Add((int)keyboardNames.F3);
                    keyList.Add((int)keyboardNames.F4);
                    break;
                case KeyBar.F5_F8:
                    keyList.Add((int)keyboardNames.F5);
                    keyList.Add((int)keyboardNames.F6);
                    keyList.Add((int)keyboardNames.F7);
                    keyList.Add((int)keyboardNames.F8);
                    break;
                case KeyBar.F9_F12:
                    keyList.Add((int)keyboardNames.F9);
                    keyList.Add((int)keyboardNames.F10);
                    keyList.Add((int)keyboardNames.F11);
                    keyList.Add((int)keyboardNames.F12);
                    break;
                case KeyBar.NUMPAD_CIRCLE:
                    keyList.Add((int)keyboardNames.NUM_FIVE);
                    keyList.Add((int)keyboardNames.NUM_EIGHT);
                    keyList.Add((int)keyboardNames.NUM_NINE);
                    keyList.Add((int)keyboardNames.NUM_SIX);
                    keyList.Add((int)keyboardNames.NUM_THREE);
                    keyList.Add((int)keyboardNames.NUM_TWO);
                    keyList.Add((int)keyboardNames.NUM_ONE);
                    keyList.Add((int)keyboardNames.NUM_FOUR);
                    keyList.Add((int)keyboardNames.NUM_SEVEN);
                    keyList.Add((int)keyboardNames.PAUSE_BREAK);
                    keyList.Add((int)keyboardNames.NUM_SLASH);
                    keyList.Add((int)keyboardNames.NUM_ASTERISK);
                    keyList.Add((int)keyboardNames.NUM_MINUS);
                    keyList.Add((int)keyboardNames.NUM_PLUS);
                    keyList.Add((int)keyboardNames.NUM_ENTER);
                    keyList.Add((int)keyboardNames.NUM_PERIOD);
                    keyList.Add((int)keyboardNames.NUM_ZERO);
                    break;
                case KeyBar.NUMPAD_BLOCK_A:
                    keyList.Add((int)keyboardNames.NUM_ZERO);
                    keyList.Add((int)keyboardNames.NUM_PERIOD);
                    keyList.Add((int)keyboardNames.NUM_ENTER);
                    keyList.Add((int)keyboardNames.NUM_THREE);
                    keyList.Add((int)keyboardNames.NUM_TWO);
                    keyList.Add((int)keyboardNames.NUM_ONE);
                    keyList.Add((int)keyboardNames.NUM_FOUR);
                    keyList.Add((int)keyboardNames.NUM_FIVE);
                    keyList.Add((int)keyboardNames.NUM_SIX);
                    keyList.Add((int)keyboardNames.NUM_PLUS);
                    keyList.Add((int)keyboardNames.NUM_NINE);
                    keyList.Add((int)keyboardNames.NUM_EIGHT);
                    keyList.Add((int)keyboardNames.NUM_SEVEN);
                    keyList.Add((int)keyboardNames.PAUSE_BREAK);
                    keyList.Add((int)keyboardNames.NUM_SLASH);
                    keyList.Add((int)keyboardNames.NUM_ASTERISK);
                    keyList.Add((int)keyboardNames.NUM_MINUS);
                    break;
                case KeyBar.NUMPAD_BLOCK_B:
                    keyList.Add((int)keyboardNames.NUM_ZERO);
                    keyList.Add((int)keyboardNames.NUM_PERIOD);
                    keyList.Add((int)keyboardNames.NUM_ONE);
                    keyList.Add((int)keyboardNames.NUM_TWO);
                    keyList.Add((int)keyboardNames.NUM_THREE);
                    keyList.Add((int)keyboardNames.NUM_ENTER);
                    keyList.Add((int)keyboardNames.NUM_FOUR);
                    keyList.Add((int)keyboardNames.NUM_FIVE);
                    keyList.Add((int)keyboardNames.NUM_SIX);
                    keyList.Add((int)keyboardNames.NUM_SEVEN);
                    keyList.Add((int)keyboardNames.NUM_EIGHT);
                    keyList.Add((int)keyboardNames.NUM_NINE);
                    keyList.Add((int)keyboardNames.NUM_PLUS);
                    keyList.Add((int)keyboardNames.PAUSE_BREAK);
                    keyList.Add((int)keyboardNames.NUM_SLASH);
                    keyList.Add((int)keyboardNames.NUM_ASTERISK);
                    keyList.Add((int)keyboardNames.NUM_MINUS);
                    break;
            }
            keyBarList[layout] = keyList;
            return keyList;
        }

        public keyboardNames GetKeyByPosition(int x, int y)
        {
            if (x > 24)
            {
                x = 24;
            }
            if (y > 5)
            {
                y = 5;
            }
            int index = (x * 6) + y;
            return keyGrid[index];
        }

        private void ClearEffects()
        {
            ClearEffects(GetNextQueueTime());
        }

        private void ClearEffects(long time)
        {
            AddEvent(new LogitechLightningEvent(LogitechLightningEvent.Type.CLEAR_EFFECTS, null, time));
        }

        public void SetLighting(LogitechColor color)
        {
            SetLighting(color, GetNextQueueTime());
        }

        public void SetLighting(LogitechColor color, long time)
        {
            AddEvent(new LogitechLightningEvent(LogitechLightningEvent.Type.CLEAR_EFFECTS, null, time));
            AddEvent(new LogitechLightningEvent(LogitechLightningEvent.Type.SET_GLOBAL, color, time + EventPadding));
        }

        public void FlashLighting(LogitechColor colorStart, LogitechColor colorEnd, int duration, int interval)
        {
            FlashLighting(colorStart, colorEnd, duration, interval, GetNextQueueTime());
        }

        public void FlashLighting(LogitechColor colorStart, LogitechColor colorEnd, int duration, int interval, long time)
        {
            AddEvent(new LogitechLightningEvent(LogitechLightningEvent.Type.CLEAR_EFFECTS, null, time));
            AddEvent(new LogitechLightningEvent(LogitechLightningEvent.Type.SET_GLOBAL, colorStart, time + EventPadding));
            AddEvent(new LogitechLightningEvent(LogitechLightningEvent.Type.FLASH_GLOBAL, colorEnd, null, 0, duration, interval, time + EventPadding));
        }

        public void PulseLighting(LogitechColor colorStart, LogitechColor colorEnd, int duration, int interval)
        {
            PulseLighting(colorStart, colorEnd, duration, interval, GetNextQueueTime());
        }

        public void PulseLighting(LogitechColor colorStart, LogitechColor colorEnd, int duration, int interval, long time)
        {
            AddEvent(new LogitechLightningEvent(LogitechLightningEvent.Type.CLEAR_EFFECTS, null, time));
            AddEvent(new LogitechLightningEvent(LogitechLightningEvent.Type.PULSE_GLOBAL, colorStart, colorEnd, 0, duration, interval, time + EventPadding));
        }

        private void RestoreLighting()
        {
            RestoreLighting(GetNextQueueTime());
        }

        private void RestoreLighting(long time)
        {
            AddEvent(new LogitechLightningEvent(LogitechLightningEvent.Type.RESTORE_GLOBAL, null, time), false);
        }

        private void SaveCurrentLighting()
        {
            SaveCurrentLighting(GetNextQueueTime());
        }

        private void SaveCurrentLighting(long time)
        {
            AddEvent(new LogitechLightningEvent(LogitechLightningEvent.Type.SAVE_GLOBAL, null, time), false);
        }

        private void ClearEffectsOnKey(keyboardNames key)
        {
            ClearEffectsOnKey(key, GetNextQueueTime(key));
        }

        private void ClearEffectsOnKey(keyboardNames key, long time)
        {
            AddEvent(new LogitechLightningEvent(LogitechLightningEvent.Type.CLEAR_EFFECTS_KEY, null, key, time), false);
        }

        public void SetLightingOnKey(keyboardNames key, LogitechColor color)
        {
            SetLightingOnKey(key, color, GetNextQueueTime(key));
        }

        public void SetLightingOnKey(keyboardNames key, LogitechColor color, long time)
        {
            AddEvent(new LogitechLightningEvent(LogitechLightningEvent.Type.CLEAR_EFFECTS_KEY, null, key, time), false);
            AddEvent(new LogitechLightningEvent(LogitechLightningEvent.Type.SET_KEY, color, key, time + EventPadding), false);
        }

        public void FlashLightingOnKey(keyboardNames key, LogitechColor colorStart, LogitechColor colorEnd, int duration, int interval)
        {
            FlashLightingOnKey(key, colorStart, colorEnd, duration, interval, GetNextQueueTime(key));
        }

        public void FlashLightingOnKey(keyboardNames key, LogitechColor colorStart, LogitechColor colorEnd, int duration, int interval, long time)
        {
            AddEvent(new LogitechLightningEvent(LogitechLightningEvent.Type.CLEAR_EFFECTS_KEY, null, key, time), false);
            AddEvent(new LogitechLightningEvent(LogitechLightningEvent.Type.SET_KEY, colorStart, key, time + EventPadding), false);
            AddEvent(new LogitechLightningEvent(LogitechLightningEvent.Type.FLASH_KEY, colorEnd, null, key, duration, interval, time + EventPadding), false);
        }

        public void PulseLightingOnKey(keyboardNames key, LogitechColor colorStart, LogitechColor colorEnd, int duration, int interval)
        {
            PulseLightingOnKey(key, colorStart, colorEnd, duration, interval, GetNextQueueTime(key));
        }

        public void PulseLightingOnKey(keyboardNames key, LogitechColor colorStart, LogitechColor colorEnd, int duration, int interval, long time)
        {
            AddEvent(new LogitechLightningEvent(LogitechLightningEvent.Type.CLEAR_EFFECTS_KEY, null, key, time), false);
            AddEvent(new LogitechLightningEvent(LogitechLightningEvent.Type.PULSE_KEY, colorEnd, colorStart, key, duration, interval, time + EventPadding), false);
        }

        private void RestoreLightingOnKey(keyboardNames key)
        {
            RestoreLightingOnKey(key, GetNextQueueTime(key));
        }

        private void RestoreLightingOnKey(keyboardNames key, long time)
        {
            AddEvent(new LogitechLightningEvent(LogitechLightningEvent.Type.RESTORE_KEY, null, key, time), false);
        }

        private void SaveCurrentLightingOnKey(keyboardNames key)
        {
            SaveCurrentLightingOnKey(key, GetNextQueueTime(key));
        }

        private void SaveCurrentLightingOnKey(keyboardNames key, long time)
        {
            AddEvent(new LogitechLightningEvent(LogitechLightningEvent.Type.SAVE_KEY, null, key, time), false);
        }

        public void SetKeyBar(KeyBar keyBar, LogitechColor colorFront, LogitechColor colorBack, int statusPercent)
        {
            SetKeyBar(keyBar, colorFront, colorBack, (float)statusPercent);
        }

        public void SetKeyBar(KeyBar keyBar, LogitechColor colorFront, LogitechColor colorBack, float statusPercent)
        {
            var keys = GetKeyList(keyBar);
            long timeStart = GetNextQueueTime();
            int countFilled = (int)(statusPercent * keys.Count / 100);
            for (int keyIndex = 0; keyIndex < keys.Count; keyIndex++)
            {
                if (keyIndex < countFilled)
                {
                    SetLightingOnKey((keyboardNames)keys.ElementAt(keyIndex), colorFront, timeStart);
                }
                else if ((keyIndex == countFilled) && (keys.Count < 100))
                {
                    int frontPercent = (countFilled * 100 / keys.Count);
                    float statusKeyPercent = (statusPercent - frontPercent) * 100 / (100 / keys.Count);
                    LogitechColor mixedColor = LogitechColor.MixColors(colorFront, colorBack, statusKeyPercent);
                    SetLightingOnKey((keyboardNames)keys.ElementAt(keyIndex), mixedColor, timeStart);
                }
                else
                {
                    SetLightingOnKey((keyboardNames)keys.ElementAt(keyIndex), colorBack, timeStart);
                }
            }
        }

        public void StartFlameAnimation(LogitechColor colorBottom, LogitechColor colorMiddle, LogitechColor colorTop, int speed)
        {
            StartFlameAnimation(colorBottom, colorMiddle, colorTop, speed, 0, 0, 23, 5);
        }

        public void StartFlameAnimation(LogitechColor colorBottom, LogitechColor colorMiddle, LogitechColor colorTop, int speed, int left, int top, int right, int bottom)
        {
            // Enforce boundarys
            EnforceKeyBounds(ref left, ref top, ref right, ref bottom);
            // Start animation
            long timeStart = GetNextQueueTime();
            if (keyboardType == LogitechKeyboardType.PerKey)
            {
                Random rng = new Random();
                IDictionary<keyboardNames, LogitechLightningEvent> animationEvents = new Dictionary<keyboardNames, LogitechLightningEvent>();
                keyboardNames key;
                LogitechColor colorStart, colorEnd;
                int x, y;
                double rowValue = 0;
                double colValue = rng.NextDouble() * 2.0;
                double colSpeed = rng.NextDouble() * 0.4 + 1.0;
                for (x = left; x <= right; x++)
                {
                    for (y = top; y <= bottom; y++)
                    {
                        key = GetKeyByPosition(x, y);
                        rowValue = Convert.ToDouble(bottom - y - top) / Convert.ToDouble(bottom - top + 1) * 2.0;
                        colorStart = GetColorFade(colValue + rowValue - 1.0, colorBottom, colorMiddle, colorTop);
                        colorEnd = GetColorFade(colValue + rowValue + 1.0, colorBottom, colorMiddle, colorTop);
                        animationEvents[key] = new LogitechLightningEvent(
                            LogitechLightningEvent.Type.PULSE_KEY, colorStart, colorEnd, key, Convert.ToInt32(speed * colSpeed), -1, timeStart + Convert.ToInt32(speed * colSpeed)
                        );
                    }
                    colValue += (rng.NextDouble() * 2.0 - colValue) / 2;
                    colSpeed = rng.NextDouble() * 0.4 + 1.0;
                }
                AddEvents(animationEvents.Values);
            }
            else
            {
                PulseLighting(colorBottom, colorMiddle, -1, speed, timeStart);
            }
        }

        public void StartWaveAnimation(LogitechColor colorStart, LogitechColor colorEnd, int speed)
        {
            StartWaveAnimation(colorStart, colorEnd, speed, 1);
        }

        public void StartWaveAnimation(LogitechColor colorStart, LogitechColor colorEnd, int speed, int waveCount)
        {
            StartWaveAnimation(colorStart, colorEnd, speed, waveCount, 0, 0, 23, 5);
        }

        public void StartWaveAnimation(LogitechColor colorStart, LogitechColor colorEnd, int speed, int waveCount, int left, int top, int right, int bottom)
        {
            // Enforce boundarys
            EnforceKeyBounds(ref left, ref top, ref right, ref bottom);
            // Start animation
            long timeStart = GetNextQueueTime();
            if (keyboardType == LogitechKeyboardType.PerKey)
            {
                IDictionary<keyboardNames, LogitechLightningEvent> animationEvents = new Dictionary<keyboardNames, LogitechLightningEvent>();
                keyboardNames key;
                int x, y;
                for (x = left; x <= right; x++)
                {
                    for (y = top; y <= bottom; y++)
                    {
                        key = GetKeyByPosition(x, y);
                        animationEvents[key] = new LogitechLightningEvent(
                            LogitechLightningEvent.Type.PULSE_KEY, colorStart, colorEnd, key, speed / 2, -1, timeStart + ((x * speed * waveCount / 24) % speed)
                        );
                    }
                }
                AddEvents(animationEvents.Values);
            } else
            {
                PulseLighting(colorStart, colorEnd, -1, speed, timeStart);
            }
        }

        public void SetKeyArea(LogitechColor color, int left, int top, int right, int bottom)
        {
            // Enforce boundarys
            EnforceKeyBounds(ref left, ref top, ref right, ref bottom);
            // Restore matching keys
            if (keyboardType == LogitechKeyboardType.PerKey)
            {
                long timeStart = GetNextQueueTime();
                IList<keyboardNames> keyList = new List<keyboardNames>();
                keyboardNames key;
                int x, y;
                for (x = left; x <= right; x++)
                {
                    for (y = top; y <= bottom; y++)
                    {
                        key = GetKeyByPosition(x, y);
                        if (!keyList.Contains(key))
                        {
                            keyList.Add(key);
                            SetLightingOnKey(key, color, timeStart);
                        }
                    }
                }
            }
        }

        public void StopEffectsOnKeyArea(int left, int top, int right, int bottom)
        {
            // Enforce boundarys
            EnforceKeyBounds(ref left, ref top, ref right, ref bottom);
            // Restore matching keys
            if (keyboardType == LogitechKeyboardType.PerKey)
            {
                long timeStart = GetNextQueueTime();
                IList<keyboardNames> keyList = new List<keyboardNames>();
                keyboardNames key;
                int x, y;
                for (x = left; x <= right; x++)
                {
                    for (y = top; y <= bottom; y++)
                    {
                        key = GetKeyByPosition(x, y);
                        if (!keyList.Contains(key))
                        {
                            keyList.Add(key);
                            ClearEffectsOnKey(key, timeStart);
                        }
                    }
                }
            }
        }

        public void SaveKeyArea(int left, int top, int right, int bottom)
        {
            // Enforce boundarys
            EnforceKeyBounds(ref left, ref top, ref right, ref bottom);
            // Restore matching keys
            if (keyboardType == LogitechKeyboardType.PerKey)
            {
                long timeStart = GetNextQueueTime();
                IList<keyboardNames> keyList = new List<keyboardNames>();
                keyboardNames key;
                int x, y;
                for (x = left; x <= right; x++)
                {
                    for (y = top; y <= bottom; y++)
                    {
                        key = GetKeyByPosition(x, y);
                        if (!keyList.Contains(key))
                        {
                            keyList.Add(key);
                            SaveCurrentLightingOnKey(key, timeStart);
                        }
                    }
                }
            }
            else
            {
                SaveCurrentLighting();
            }
        }

        public void RestoreKeyArea(int left, int top, int right, int bottom)
        {
            // Enforce boundarys
            EnforceKeyBounds(ref left, ref top, ref right, ref bottom);
            // Restore matching keys
            if (keyboardType == LogitechKeyboardType.PerKey)
            {
                long timeStart = GetNextQueueTime();
                IList<keyboardNames> keyList = new List<keyboardNames>();
                keyboardNames key;
                int x, y;
                for (x = left; x <= right; x++)
                {
                    for (y = top; y <= bottom; y++)
                    {
                        key = GetKeyByPosition(x, y);
                        if (!keyList.Contains(key))
                        {
                            keyList.Add(key);
                            RestoreLightingOnKey(key, timeStart);
                        }
                    }
                }
            }
            else
            {
                RestoreLighting();
            }
        }

        public void UpdateLightning()
        {
            HandleEvents();
        }

        public bool IsRgb()
        {
            return (keyboardType != LogitechKeyboardType.MonochromeFull);
        }

        public bool IsPerKey()
        {
            return (keyboardType == LogitechKeyboardType.PerKey);
        }

        public LogitechKeyboardType GetKeyboardType()
        {
            return keyboardType;
        }

        public void SetKeyboardType(LogitechKeyboardType type)
        {
            keyboardType = type;
        }

        internal void InitKeyGrid()
        {
            keyGrid = new keyboardNames[150];
            int colOffset = 0;
            // Col 1 (0-5)
            keyGrid[colOffset + 0] = keyboardNames.ESC;
            keyGrid[colOffset + 1] = keyboardNames.TILDE;
            keyGrid[colOffset + 2] = keyboardNames.TAB;
            keyGrid[colOffset + 3] = keyboardNames.CAPS_LOCK;
            keyGrid[colOffset + 4] = keyboardNames.LEFT_SHIFT;
            keyGrid[colOffset + 5] = keyboardNames.LEFT_CONTROL;
            colOffset += 6;
            // Col 2 (6-11)
            keyGrid[colOffset + 0] = keyboardNames.ESC;
            keyGrid[colOffset + 1] = keyboardNames.TILDE;
            keyGrid[colOffset + 2] = keyboardNames.TAB;
            keyGrid[colOffset + 3] = keyboardNames.CAPS_LOCK;
            keyGrid[colOffset + 4] = keyboardNames.PIPE;
            keyGrid[colOffset + 5] = keyboardNames.LEFT_WINDOWS;
            colOffset += 6;
            // Col 3 (12-17)
            keyGrid[colOffset + 0] = keyboardNames.ESC;
            keyGrid[colOffset + 1] = keyboardNames.ONE;
            keyGrid[colOffset + 2] = keyboardNames.Q;
            keyGrid[colOffset + 3] = keyboardNames.A;
            keyGrid[colOffset + 4] = keyboardNames.Z;
            keyGrid[colOffset + 5] = keyboardNames.LEFT_ALT;
            colOffset += 6;
            // Col 4 (18-23)
            keyGrid[colOffset + 0] = keyboardNames.F1;
            keyGrid[colOffset + 1] = keyboardNames.TWO;
            keyGrid[colOffset + 2] = keyboardNames.W;
            keyGrid[colOffset + 3] = keyboardNames.S;
            keyGrid[colOffset + 4] = keyboardNames.X;
            keyGrid[colOffset + 5] = keyboardNames.SPACE;
            colOffset += 6;
            // Col 5 (24-29)
            keyGrid[colOffset + 0] = keyboardNames.F2;
            keyGrid[colOffset + 1] = keyboardNames.THREE;
            keyGrid[colOffset + 2] = keyboardNames.E;
            keyGrid[colOffset + 3] = keyboardNames.D;
            keyGrid[colOffset + 4] = keyboardNames.C;
            keyGrid[colOffset + 5] = keyboardNames.SPACE;
            colOffset += 6;
            // Col 6 (30-35)
            keyGrid[colOffset + 0] = keyboardNames.F3;
            keyGrid[colOffset + 1] = keyboardNames.FOUR;
            keyGrid[colOffset + 2] = keyboardNames.R;
            keyGrid[colOffset + 3] = keyboardNames.F;
            keyGrid[colOffset + 4] = keyboardNames.V;
            keyGrid[colOffset + 5] = keyboardNames.SPACE;
            colOffset += 6;
            // Col 7 (36-41)
            keyGrid[colOffset + 0] = keyboardNames.F4;
            keyGrid[colOffset + 1] = keyboardNames.FIVE;
            keyGrid[colOffset + 2] = keyboardNames.T;
            keyGrid[colOffset + 3] = keyboardNames.G;
            keyGrid[colOffset + 4] = keyboardNames.B;
            keyGrid[colOffset + 5] = keyboardNames.SPACE;
            colOffset += 6;
            // Col 8 (42-47)
            keyGrid[colOffset + 0] = keyboardNames.F4;
            keyGrid[colOffset + 1] = keyboardNames.SIX;
            keyGrid[colOffset + 2] = keyboardNames.Y;
            keyGrid[colOffset + 3] = keyboardNames.H;
            keyGrid[colOffset + 4] = keyboardNames.N;
            keyGrid[colOffset + 5] = keyboardNames.SPACE;
            colOffset += 6;
            // Col 9 (48-53)
            keyGrid[colOffset + 0] = keyboardNames.F4;
            keyGrid[colOffset + 1] = keyboardNames.SEVEN;
            keyGrid[colOffset + 2] = keyboardNames.U;
            keyGrid[colOffset + 3] = keyboardNames.J;
            keyGrid[colOffset + 4] = keyboardNames.M;
            keyGrid[colOffset + 5] = keyboardNames.SPACE;
            colOffset += 6;
            // Col 10 (54-59)
            keyGrid[colOffset + 0] = keyboardNames.F5;
            keyGrid[colOffset + 1] = keyboardNames.SEVEN;
            keyGrid[colOffset + 2] = keyboardNames.U;
            keyGrid[colOffset + 3] = keyboardNames.J;
            keyGrid[colOffset + 4] = keyboardNames.M;
            keyGrid[colOffset + 5] = keyboardNames.SPACE;
            colOffset += 6;
            // Col 11 (60-65)
            keyGrid[colOffset + 0] = keyboardNames.F6;
            keyGrid[colOffset + 1] = keyboardNames.EIGHT;
            keyGrid[colOffset + 2] = keyboardNames.I;
            keyGrid[colOffset + 3] = keyboardNames.K;
            keyGrid[colOffset + 4] = keyboardNames.COMMA;
            keyGrid[colOffset + 5] = keyboardNames.RIGHT_ALT;
            colOffset += 6;
            // Col 12 (66-71)
            keyGrid[colOffset + 0] = keyboardNames.F7;
            keyGrid[colOffset + 1] = keyboardNames.NINE;
            keyGrid[colOffset + 2] = keyboardNames.O;
            keyGrid[colOffset + 3] = keyboardNames.L;
            keyGrid[colOffset + 4] = keyboardNames.PERIOD;
            keyGrid[colOffset + 5] = keyboardNames.RIGHT_WINDOWS;
            colOffset += 6;
            // Col 13 (72-77)
            keyGrid[colOffset + 0] = keyboardNames.F8;
            keyGrid[colOffset + 1] = keyboardNames.ZERO;
            keyGrid[colOffset + 2] = keyboardNames.P;
            keyGrid[colOffset + 3] = keyboardNames.SEMICOLON;
            keyGrid[colOffset + 4] = keyboardNames.FORWARD_SLASH;
            keyGrid[colOffset + 5] = keyboardNames.RIGHT_WINDOWS;
            colOffset += 6;
            // Col 14 (78-83)
            keyGrid[colOffset + 0] = keyboardNames.F9;
            keyGrid[colOffset + 1] = keyboardNames.MINUS;
            keyGrid[colOffset + 2] = keyboardNames.OPEN_BRACKET;
            keyGrid[colOffset + 3] = keyboardNames.APOSTROPHE;
            keyGrid[colOffset + 4] = keyboardNames.RIGHT_SHIFT;
            keyGrid[colOffset + 5] = keyboardNames.APPLICATION_SELECT;
            colOffset += 6;
            // Col 15 (84-89)
            keyGrid[colOffset + 0] = keyboardNames.F10;
            keyGrid[colOffset + 1] = keyboardNames.EQUALS;
            keyGrid[colOffset + 2] = keyboardNames.CLOSE_BRACKET;
            keyGrid[colOffset + 3] = keyboardNames.NUMBER_SIGN;
            keyGrid[colOffset + 4] = keyboardNames.RIGHT_SHIFT;
            keyGrid[colOffset + 5] = keyboardNames.RIGHT_CONTROL;
            colOffset += 6;
            // Col 16 (90-95)
            keyGrid[colOffset + 0] = keyboardNames.F11;
            keyGrid[colOffset + 1] = keyboardNames.BACKSPACE;
            keyGrid[colOffset + 2] = keyboardNames.ENTER;
            keyGrid[colOffset + 3] = keyboardNames.ENTER;
            keyGrid[colOffset + 4] = keyboardNames.RIGHT_SHIFT;
            keyGrid[colOffset + 5] = keyboardNames.RIGHT_CONTROL;
            colOffset += 6;
            // Col 17 (96-101)
            keyGrid[colOffset + 0] = keyboardNames.F12;
            keyGrid[colOffset + 1] = keyboardNames.BACKSPACE;
            keyGrid[colOffset + 2] = keyboardNames.ENTER;
            keyGrid[colOffset + 3] = keyboardNames.ENTER;
            keyGrid[colOffset + 4] = keyboardNames.RIGHT_SHIFT;
            keyGrid[colOffset + 5] = keyboardNames.RIGHT_CONTROL;
            colOffset += 6;
            // Col 18 (102-107)
            keyGrid[colOffset + 0] = keyboardNames.PRINT_SCREEN;
            keyGrid[colOffset + 1] = keyboardNames.INSERT;
            keyGrid[colOffset + 2] = keyboardNames.KEYBOARD_DELETE;
            keyGrid[colOffset + 3] = keyboardNames.KEYBOARD_DELETE;
            keyGrid[colOffset + 4] = keyboardNames.ARROW_LEFT;
            keyGrid[colOffset + 5] = keyboardNames.ARROW_LEFT;
            colOffset += 6;
            // Col 19 (108-113)
            keyGrid[colOffset + 0] = keyboardNames.SCROLL_LOCK;
            keyGrid[colOffset + 1] = keyboardNames.HOME;
            keyGrid[colOffset + 2] = keyboardNames.END;
            keyGrid[colOffset + 3] = keyboardNames.END;
            keyGrid[colOffset + 4] = keyboardNames.ARROW_UP;
            keyGrid[colOffset + 5] = keyboardNames.ARROW_DOWN;
            colOffset += 6;
            // Col 20 (114-119)
            keyGrid[colOffset + 0] = keyboardNames.PAUSE_BREAK;
            keyGrid[colOffset + 1] = keyboardNames.PAGE_UP;
            keyGrid[colOffset + 2] = keyboardNames.PAGE_DOWN;
            keyGrid[colOffset + 3] = keyboardNames.PAGE_DOWN;
            keyGrid[colOffset + 4] = keyboardNames.ARROW_RIGHT;
            keyGrid[colOffset + 5] = keyboardNames.ARROW_RIGHT;
            colOffset += 6;
            // Col 21 (120-125)
            keyGrid[colOffset + 0] = keyboardNames.NUM_LOCK;
            keyGrid[colOffset + 1] = keyboardNames.NUM_LOCK;
            keyGrid[colOffset + 2] = keyboardNames.NUM_SEVEN;
            keyGrid[colOffset + 3] = keyboardNames.NUM_FOUR;
            keyGrid[colOffset + 4] = keyboardNames.NUM_ONE;
            keyGrid[colOffset + 5] = keyboardNames.NUM_ZERO;
            colOffset += 6;
            // Col 22 (126-131)
            keyGrid[colOffset + 0] = keyboardNames.NUM_SLASH;
            keyGrid[colOffset + 1] = keyboardNames.NUM_SLASH;
            keyGrid[colOffset + 2] = keyboardNames.NUM_EIGHT;
            keyGrid[colOffset + 3] = keyboardNames.NUM_FIVE;
            keyGrid[colOffset + 4] = keyboardNames.NUM_TWO;
            keyGrid[colOffset + 5] = keyboardNames.NUM_ZERO;
            colOffset += 6;
            // Col 23 (132-137)
            keyGrid[colOffset + 0] = keyboardNames.NUM_ASTERISK;
            keyGrid[colOffset + 1] = keyboardNames.NUM_ASTERISK;
            keyGrid[colOffset + 2] = keyboardNames.NUM_NINE;
            keyGrid[colOffset + 3] = keyboardNames.NUM_SIX;
            keyGrid[colOffset + 4] = keyboardNames.NUM_THREE;
            keyGrid[colOffset + 5] = keyboardNames.NUM_PERIOD;
            colOffset += 6;
            // Col 24 (138-143)
            keyGrid[colOffset + 0] = keyboardNames.NUM_MINUS;
            keyGrid[colOffset + 1] = keyboardNames.NUM_MINUS;
            keyGrid[colOffset + 2] = keyboardNames.NUM_PLUS;
            keyGrid[colOffset + 3] = keyboardNames.NUM_PLUS;
            keyGrid[colOffset + 4] = keyboardNames.NUM_ENTER;
            keyGrid[colOffset + 5] = keyboardNames.NUM_ENTER;
        }
    }
}
