using LedCSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogitechLedTools
{
    class LogitechLightningEvent
    {
        public enum Type
        {
            CLEAR = 0, CLEAR_EFFECTS = 1, CLEAR_EFFECTS_KEY = 2,
            SET_KEY = 10, FLASH_KEY = 11, PULSE_KEY = 12, RESTORE_KEY = 13, SAVE_KEY = 14,
            SET_GLOBAL = 21, FLASH_GLOBAL = 22, PULSE_GLOBAL = 23, RESTORE_GLOBAL = 24, SAVE_GLOBAL = 25
        };

        public Type type { get; set; }
        public LogitechColor colorStart { get; set; }
        public LogitechColor colorEnd { get; set; }
        public keyboardNames key { get; set; }
        public int duration { get; set; }
        public int interval { get; set; }
        public long time { get; set; }

        public LogitechLightningEvent(Type type, LogitechColor colorStart, LogitechColor colorEnd, keyboardNames key, int duration, int interval, long time)
        {
            this.type = type;
            this.colorStart = colorStart;
            this.colorEnd = colorEnd;
            this.key = key;
            this.duration = duration;
            this.interval = interval;
            this.time = time;
        }

        public LogitechLightningEvent(Type type, LogitechColor colorStart, keyboardNames key)
        {
            this.type = type;
            this.colorStart = colorStart;
            this.colorEnd = colorStart;
            this.key = key;
            this.duration = 0;
            this.interval = 0;
            this.time = 0;
        }

        public LogitechLightningEvent(Type type, LogitechColor colorStart)
        {
            this.type = type;
            this.colorStart = colorStart;
            this.colorEnd = colorStart;
            this.key = 0;
            this.duration = 0;
            this.interval = 0;
            this.time = 0;
        }

        public LogitechLightningEvent()
        {
            this.type = Type.CLEAR;
            this.colorStart = null;
            this.colorEnd = null;
            this.key = 0;
            this.duration = 0;
            this.interval = 0;
            this.time = 0;
        }
    }
}
