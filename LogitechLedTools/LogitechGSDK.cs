using System;
using System.Collections;
using System.Runtime.InteropServices;

namespace LedCSharp
{

    public enum keyboardNames
    {
        ESC = 0x01,
        F1 = 0x3b,
        F2 = 0x3c,
        F3 = 0x3d,
        F4 = 0x3e,
        F5 = 0x3f,
        F6 = 0x40,
        F7 = 0x41,
        F8 = 0x42,
        F9 = 0x43,
        F10 = 0x44,
        F11 = 0x57,
        F12 = 0x58,
        PRINT_SCREEN = 0x137,
        SCROLL_LOCK = 0x46,
        PAUSE_BREAK = 0x45,
        TILDE = 0x29,
        ONE = 0x02,
        TWO = 0x03,
        THREE = 0x04,
        FOUR = 0x05,
        FIVE = 0x06,
        SIX = 0x07,
        SEVEN = 0x08,
        EIGHT = 0x09,
        NINE = 0x0A,
        ZERO = 0x0B,
        MINUS = 0x0C,
        EQUALS = 0x0D,
        BACKSPACE = 0x0E,
        INSERT = 0x152,
        HOME = 0x147,
        PAGE_UP = 0x149,
        NUM_LOCK = 0x145,
        NUM_SLASH = 0x135,
        NUM_ASTERISK = 0x37,
        NUM_MINUS = 0x4A,
        TAB = 0x0F,
        Q = 0x10,
        W = 0x11,
        E = 0x12,
        R = 0x13,
        T = 0x14,
        Y = 0x15,
        U = 0x16,
        I = 0x17,
        O = 0x18,
        P = 0x19,
        OPEN_BRACKET = 0x1A,
        CLOSE_BRACKET = 0x1B,
        NUMBER_SIGN = 0x5D,
        BACKSLASH = 0x2B,
        KEYBOARD_DELETE = 0x153,
        END = 0x14F,
        PAGE_DOWN = 0x151,
        NUM_SEVEN = 0x47,
        NUM_EIGHT = 0x48,
        NUM_NINE = 0x49,
        NUM_PLUS = 0x4E,
        CAPS_LOCK = 0x3A,
        A = 0x1E,
        S = 0x1F,
        D = 0x20,
        F = 0x21,
        G = 0x22,
        H = 0x23,
        J = 0x24,
        K = 0x25,
        L = 0x26,
        SEMICOLON = 0x27,
        APOSTROPHE = 0x28,
        ENTER = 0x1C,
        NUM_FOUR = 0x4B,
        NUM_FIVE = 0x4C,
        NUM_SIX = 0x4D,
        LEFT_SHIFT = 0x2A,
        PIPE = 0x56,
        Z = 0x2C,
        X = 0x2D,
        C = 0x2E,
        V = 0x2F,
        B = 0x30,
        N = 0x31,
        M = 0x32,
        COMMA = 0x33,
        PERIOD = 0x34,
        FORWARD_SLASH = 0x35,
        RIGHT_SHIFT = 0x36,
        ARROW_UP = 0x148,
        NUM_ONE = 0x4F,
        NUM_TWO = 0x50,
        NUM_THREE = 0x51,
        NUM_ENTER = 0x11C,
        LEFT_CONTROL = 0x1D,
        LEFT_WINDOWS = 0x15B,
        LEFT_ALT = 0x38,
        SPACE = 0x39,
        RIGHT_ALT = 0x138,
        RIGHT_WINDOWS = 0x15C,
        APPLICATION_SELECT = 0x15D,
        RIGHT_CONTROL = 0x11D,
        ARROW_LEFT = 0x14B,
        ARROW_DOWN = 0x150,
        ARROW_RIGHT = 0x14D,
        NUM_ZERO = 0x52,
        NUM_PERIOD = 0x53
    };

    public class LogitechGSDK
    {
        //LED SDK
        private const int LOGI_DEVICETYPE_MONOCHROME_ORD = 0;
        private const int LOGI_DEVICETYPE_RGB_ORD = 1;
        private const int LOGI_DEVICETYPE_PERKEY_RGB_ORD = 2;

        public const int LOGI_DEVICETYPE_MONOCHROME = (1 << LOGI_DEVICETYPE_MONOCHROME_ORD);
        public const int LOGI_DEVICETYPE_RGB = (1 << LOGI_DEVICETYPE_RGB_ORD);
        public const int LOGI_DEVICETYPE_PERKEY_RGB = (1 << LOGI_DEVICETYPE_PERKEY_RGB_ORD);
        public const int LOGI_LED_BITMAP_WIDTH = 21;
        public const int LOGI_LED_BITMAP_HEIGHT = 6;
        public const int LOGI_LED_BITMAP_BYTES_PER_KEY = 4;

        public const int LOGI_LED_BITMAP_SIZE = LOGI_LED_BITMAP_WIDTH * LOGI_LED_BITMAP_HEIGHT * LOGI_LED_BITMAP_BYTES_PER_KEY;
        public const int LOGI_LED_DURATION_INFINITE = 0;

        [DllImport("LogitechLedEnginesWrapper ", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LogiLedInit();

        [DllImport("LogitechLedEnginesWrapper ", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LogiLedSetTargetDevice(int targetDevice);

        [DllImport("LogitechLedEnginesWrapper ", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LogiLedGetSdkVersion(ref int majorNum, ref int minorNum, ref int buildNum);

        [DllImport("LogitechLedEnginesWrapper ", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LogiLedSaveCurrentLighting();

        [DllImport("LogitechLedEnginesWrapper ", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LogiLedSetLighting(int redPercentage, int greenPercentage, int bluePercentage);

        [DllImport("LogitechLedEnginesWrapper ", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LogiLedRestoreLighting();

        [DllImport("LogitechLedEnginesWrapper ", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LogiLedFlashLighting(int redPercentage, int greenPercentage, int bluePercentage, int milliSecondsDuration, int milliSecondsInterval);

        [DllImport("LogitechLedEnginesWrapper ", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LogiLedPulseLighting(int redPercentage, int greenPercentage, int bluePercentage, int milliSecondsDuration, int milliSecondsInterval);

        [DllImport("LogitechLedEnginesWrapper ", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LogiLedStopEffects();

        [DllImport("LogitechLedEnginesWrapper ", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LogiLedSetLightingFromBitmap(byte[] bitmap);

        [DllImport("LogitechLedEnginesWrapper ", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LogiLedSetLightingForKeyWithScanCode(int keyCode, int redPercentage, int greenPercentage, int bluePercentage);

        [DllImport("LogitechLedEnginesWrapper ", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LogiLedSetLightingForKeyWithHidCode(int keyCode, int redPercentage, int greenPercentage, int bluePercentage);

        [DllImport("LogitechLedEnginesWrapper ", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LogiLedSetLightingForKeyWithQuartzCode(int keyCode, int redPercentage, int greenPercentage, int bluePercentage);

        [DllImport("LogitechLedEnginesWrapper ", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LogiLedSetLightingForKeyWithKeyName(keyboardNames keyCode, int redPercentage, int greenPercentage, int bluePercentage);

        [DllImport("LogitechLedEnginesWrapper ", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LogiLedSaveLightingForKey(keyboardNames keyName);

        [DllImport("LogitechLedEnginesWrapper ", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LogiLedRestoreLightingForKey(keyboardNames keyName);

        [DllImport("LogitechLedEnginesWrapper ", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LogiLedFlashSingleKey(keyboardNames keyName, int redPercentage, int greenPercentage, int bluePercentage, int msDuration, int msInterval);

        [DllImport("LogitechLedEnginesWrapper ", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LogiLedPulseSingleKey(keyboardNames keyName, int startRedPercentage, int startGreenPercentage, int startBluePercentage, int finishRedPercentage, int finishGreenPercentage, int finishBluePercentage, int msDuration, bool isInfinite);

        [DllImport("LogitechLedEnginesWrapper ", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LogiLedStopEffectsOnKey(keyboardNames keyName);

        [DllImport("LogitechLedEnginesWrapper ", CallingConvention = CallingConvention.Cdecl)]
        public static extern void LogiLedShutdown();

        public static bool LogiLedSetLightingForKeyWithKeyName(keyboardNames key, LogitechLedTools.LogitechColor color)
        {
            return LogiLedSetLightingForKeyWithKeyName(key, color.getRedPercentage(), color.getGreenPercentage(), color.getBluePercentage());
        }

        public static bool LogiLedSetLightingForKeyWithScanCode(int key, LogitechLedTools.LogitechColor color)
        {
            return LogiLedSetLightingForKeyWithScanCode(key, color.getRedPercentage(), color.getGreenPercentage(), color.getBluePercentage());
        }

        public static bool LogiLedSetLightingForKeyWithQuartzCode(int key, LogitechLedTools.LogitechColor color)
        {
            return LogiLedSetLightingForKeyWithQuartzCode(key, color.getRedPercentage(), color.getGreenPercentage(), color.getBluePercentage());
        }

        public static string GetKeyName(int value)
        {
            return GetKeyName((keyboardNames)value);
        }

        public static string GetKeyName(keyboardNames value)
        {
            return GetKeyName(value, "unknown");
        }

        public static string GetKeyName(keyboardNames value, string fallback)
        {
            switch (value)
            {
                case keyboardNames.A: return "A";
                case keyboardNames.B: return "B";
                case keyboardNames.C: return "C";
                case keyboardNames.D: return "D";
                case keyboardNames.E: return "E";
                case keyboardNames.F: return "F";
                case keyboardNames.G: return "G";
                case keyboardNames.H: return "H";
                case keyboardNames.I: return "I";
                case keyboardNames.J: return "J";
                case keyboardNames.K: return "K";
                case keyboardNames.L: return "L";
                case keyboardNames.M: return "M";
                case keyboardNames.N: return "N";
                case keyboardNames.O: return "O";
                case keyboardNames.P: return "P";
                case keyboardNames.Q: return "Q";
                case keyboardNames.R: return "R";
                case keyboardNames.S: return "S";
                case keyboardNames.T: return "T";
                case keyboardNames.U: return "U";
                case keyboardNames.V: return "V";
                case keyboardNames.W: return "W";
                case keyboardNames.X: return "X";
                case keyboardNames.Y: return "Z";
                case keyboardNames.Z: return "Y";
                case keyboardNames.ZERO: return "0";
                case keyboardNames.ONE: return "1";
                case keyboardNames.TWO: return "2";
                case keyboardNames.THREE: return "3";
                case keyboardNames.FOUR: return "4";
                case keyboardNames.FIVE: return "5";
                case keyboardNames.SIX: return "6";
                case keyboardNames.SEVEN: return "7";
                case keyboardNames.EIGHT: return "8";
                case keyboardNames.NINE: return "9";
                case keyboardNames.SPACE: return " ";
            }
            return fallback;
        }

        internal static keyboardNames GetKeyCode(char keyChar)
        {
            switch (keyChar)
            {
                case 'a': return keyboardNames.A;
                case 'b': return keyboardNames.B;
                case 'c': return keyboardNames.C;
                case 'd': return keyboardNames.D;
                case 'e': return keyboardNames.E;
                case 'f': return keyboardNames.F;
                case 'g': return keyboardNames.G;
                case 'h': return keyboardNames.H;
                case 'i': return keyboardNames.I;
                case 'j': return keyboardNames.J;
                case 'k': return keyboardNames.K;
                case 'l': return keyboardNames.L;
                case 'm': return keyboardNames.M;
                case 'n': return keyboardNames.N;
                case 'o': return keyboardNames.O;
                case 'p': return keyboardNames.P;
                case 'q': return keyboardNames.Q;
                case 'r': return keyboardNames.R;
                case 's': return keyboardNames.S;
                case 't': return keyboardNames.T;
                case 'u': return keyboardNames.U;
                case 'v': return keyboardNames.V;
                case 'w': return keyboardNames.W;
                case 'x': return keyboardNames.X;
                case 'y': return keyboardNames.Z;
                case 'z': return keyboardNames.Y;
                case '0': return keyboardNames.ZERO;
                case '1': return keyboardNames.ONE;
                case '2': return keyboardNames.TWO;
                case '3': return keyboardNames.THREE;
                case '4': return keyboardNames.FOUR;
                case '5': return keyboardNames.FIVE;
                case '6': return keyboardNames.SIX;
                case '7': return keyboardNames.SEVEN;
                case '8': return keyboardNames.EIGHT;
                case '9': return keyboardNames.NINE;
                case ' ': default: return keyboardNames.SPACE;
            }
        }
    }

}
