using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace LogitechLedTools
{

    class ScreenCheckerCondition
    {
        public string nameA { get; set; }
        public byte valueA { get; set; }
        public string nameB { get; set; }
        public byte valueB { get; set; }
        public string compareFunc { get; set; }

        public ScreenCheckerCondition(byte a, string compare, byte b)
        {
            nameA = null;
            valueA = a;
            nameB = null;
            valueB = b;
            compareFunc = compare;
        }

        public ScreenCheckerCondition(string a, string compare, byte b)
        {
            nameA = a;
            valueA = 0;
            nameB = null;
            valueB = b;
            compareFunc = compare;
        }

        public ScreenCheckerCondition(byte a, string compare, string b)
        {
            nameA = null;
            valueA = a;
            nameB = b;
            valueB = 0;
            compareFunc = compare;
        }

        public ScreenCheckerCondition(string a, string compare, string b)
        {
            nameA = a;
            valueA = 0;
            nameB = b;
            valueB = 0;
            compareFunc = compare;
        }

        public byte GetValue(string value, Color color)
        {
            value = value.ToLower();
            switch (value)
            {
                case "r":
                case "red":
                    return color.R;
                case "g":
                case "green":
                    return color.G;
                case "b":
                case "blue":
                    return color.B;
                default:
                    return 0;
            }
        }

        public bool GetResult(Color value)
        {
            byte a = (nameA == null ? valueA : GetValue(nameA, value));
            byte b = (nameB == null ? valueB : GetValue(nameB, value));
            switch (compareFunc)
            {
                default:
                case "=":
                case "==":
                    return (a == b);
                case "<":
                    return (a < b);
                case "<=":
                    return (a <= b);
                case ">":
                    return (a > b);
                case ">=":
                    return (a >= b);
            }
        }
    }

    class ScreenCheckerPoint
    {
        public int leftPixels { get; set; }
        public double leftRatio { get; set; }
        public int topPixels { get; set; }
        public double topRatio { get; set; }
        public IList<ScreenCheckerCondition> conditions { get; set; }
        public bool result { get; set; }

        public ScreenCheckerPoint(int xPixel, double xRatio, int yPixel, double yRatio)
        {
            leftPixels = xPixel;
            leftRatio = xRatio;
            topPixels = yPixel;
            topRatio = yRatio;
            conditions = new List<ScreenCheckerCondition>();
            result = false;
        }

        public ScreenCheckerPoint AddCondition(byte a, string compare, byte b)
        {
            conditions.Add(new ScreenCheckerCondition(a, compare, b));
            return this;
        }

        public ScreenCheckerPoint AddCondition(string a, string compare, byte b)
        {
            conditions.Add(new ScreenCheckerCondition(a, compare, b));
            return this;
        }

        public ScreenCheckerPoint AddCondition(byte a, string compare, string b)
        {
            conditions.Add(new ScreenCheckerCondition(a, compare, b));
            return this;
        }

        public ScreenCheckerPoint AddCondition(string a, string compare, string b)
        {
            conditions.Add(new ScreenCheckerCondition(a, compare, b));
            return this;
        }

        private bool CheckConditions(Color value)
        {
            foreach (ScreenCheckerCondition condition in conditions)
            {
                if (!condition.GetResult(value))
                {
                    return false;
                }
            }
            return true;
        }

        public void Execute(ref Bitmap bitmap, ref RECT screenArea, ref RECT windowSize)
        {
            int x = Convert.ToInt32(Math.Round(leftRatio * windowSize.Right)) + leftPixels - screenArea.Left;
            int y = Convert.ToInt32(Math.Round(topRatio * windowSize.Bottom)) + topPixels - screenArea.Top;
            Color value = bitmap.GetPixel(x, y);
            result = CheckConditions(value);
        }

        public bool GetResult()
        {
            return result;
        }

    }

    class ScreenCheckerBar
    {
        public int leftPixels { get; set; }
        public double leftRatio { get; set; }
        public int topPixels { get; set; }
        public double topRatio { get; set; }
        public int rightPixels { get; set; }
        public double rightRatio { get; set; }
        public int bottomPixels { get; set; }
        public double bottomRatio { get; set; }
        public bool backwardScan { get; set; }
        public IList<ScreenCheckerCondition> conditions { get; set; }
        public double result { get; set; }

        public ScreenCheckerBar(int leftPixels, double leftRatio, int topPixels, double topRatio, int rightPixels, double rightRatio, int bottomPixels, double bottomRatio)
        {
            this.leftPixels = leftPixels;
            this.leftRatio = leftRatio;
            this.topPixels = topPixels;
            this.topRatio = topRatio;
            this.rightPixels = rightPixels;
            this.rightRatio = rightRatio;
            this.bottomPixels = bottomPixels;
            this.bottomRatio = bottomRatio;
            this.backwardScan = true;
            conditions = new List<ScreenCheckerCondition>();
            result = 0;
        }

        public ScreenCheckerBar(int leftPixels, double leftRatio, int topPixels, double topRatio, int rightPixels, double rightRatio, int bottomPixels, double bottomRatio, bool backwardScan)
        {
            this.leftPixels = leftPixels;
            this.leftRatio = leftRatio;
            this.topPixels = topPixels;
            this.topRatio = topRatio;
            this.rightPixels = rightPixels;
            this.rightRatio = rightRatio;
            this.bottomPixels = bottomPixels;
            this.bottomRatio = bottomRatio;
            this.backwardScan = backwardScan;
            conditions = new List<ScreenCheckerCondition>();
            result = 0;
        }

        public ScreenCheckerBar AddCondition(byte a, string compare, byte b)
        {
            conditions.Add(new ScreenCheckerCondition(a, compare, b));
            return this;
        }

        public ScreenCheckerBar AddCondition(string a, string compare, byte b)
        {
            conditions.Add(new ScreenCheckerCondition(a, compare, b));
            return this;
        }

        public ScreenCheckerBar AddCondition(byte a, string compare, string b)
        {
            conditions.Add(new ScreenCheckerCondition(a, compare, b));
            return this;
        }

        public ScreenCheckerBar AddCondition(string a, string compare, string b)
        {
            conditions.Add(new ScreenCheckerCondition(a, compare, b));
            return this;
        }

        private bool CheckConditions(Color value)
        {
            foreach (ScreenCheckerCondition condition in conditions)
            {
                if (!condition.GetResult(value))
                {
                    return false;
                }
            }
            return true;
        }

        private bool CheckColumn(ref Bitmap bitmap, int x, int yMin, int yMax)
        {
            for (int y = yMin; y < yMax; y++)
            {
                var color = bitmap.GetPixel(x, y);
                if (CheckConditions(color))
                {
                    return true;
                }
            }
            return false;
        }

        public void Execute(ref Bitmap bitmap, ref RECT screenArea, ref RECT windowSize)
        {
            RECT barArea = new RECT(
                Convert.ToInt32(Math.Round(leftRatio * windowSize.Right)) + leftPixels - screenArea.Left,
                Convert.ToInt32(Math.Round(topRatio * windowSize.Bottom)) + topPixels - screenArea.Top,
                Convert.ToInt32(Math.Round(rightRatio * windowSize.Right)) + rightPixels - screenArea.Left,
                Convert.ToInt32(Math.Round(bottomRatio * windowSize.Bottom)) + bottomPixels - screenArea.Top
                );
            int x;
            if (backwardScan)
            {
                for (x = barArea.Right - 1; x >= barArea.Left; x--)
                {
                    if (CheckColumn(ref bitmap, x, barArea.Top, barArea.Bottom))
                    {
                        break;
                    }
                }
            } else
            {
                for (x = barArea.Left; x < barArea.Right; x++)
                {
                    if (!CheckColumn(ref bitmap, x, barArea.Top, barArea.Bottom))
                    {
                        x--;
                        break;
                    }
                }
            }
            result = Convert.ToDouble(x - barArea.Left) / Convert.ToDouble(barArea.Right - barArea.Left + 1);
        }

        public double GetResult()
        {
            return result;
        }

    }

    class ScreenChecker
    {
        public int hwnd { get; set; }
        public string scene { get; set; }
        public int countSuccessful { get; set; }
        public int countFail { get; set; }
        public int countDone { get; set; }
        public IDictionary<string, ScreenCheckerPoint> points { get; set; }
        public IDictionary<string, ScreenCheckerBar> bars { get; set; }

        public ScreenChecker(int hwndTarget, string sceneName)
        {
            hwnd = hwndTarget;
            scene = sceneName;
            countSuccessful = 0;
            countFail = 0;
            countDone = 0;
            points = new Dictionary<string, ScreenCheckerPoint>();
            bars = new Dictionary<string, ScreenCheckerBar>();
        }

        private void ExpandScreenArea(ref RECT screenArea, RECT windowSize, int xPixels, double xRatio, int yPixels, double yRatio)
        {
            int x = Convert.ToInt32(Math.Round(xRatio * windowSize.Right)) + xPixels;
            int y = Convert.ToInt32(Math.Round(yRatio * windowSize.Bottom)) + yPixels;
            if (x < screenArea.Left)
            {
                screenArea.Left = x;
            }
            if (x >= screenArea.Right)
            {
                screenArea.Right = x + 1;
            }
            if (y < screenArea.Top)
            {
                screenArea.Top = y;
            }
            if (y >= screenArea.Bottom)
            {
                screenArea.Bottom = y + 1;
            }
        }

        public ScreenCheckerPoint AddPoint(string name, int xPixel, double xRatio, int yPixel, double yRatio)
        {
            points.Add(name, new ScreenCheckerPoint(xPixel, xRatio, yPixel, yRatio));
            return points[name];
        }

        public ScreenCheckerBar AddBar(string name, int leftPixels, double leftRatio, int topPixels, double topRatio, int rightPixels, double rightRatio, int bottomPixels, double bottomRatio)
        {
            bars.Add(name, new ScreenCheckerBar(leftPixels, leftRatio, topPixels, topRatio, rightPixels, rightRatio, bottomPixels, bottomRatio));
            return bars[name];
        }

        public bool GetPointResult(string name)
        {
            if (points.ContainsKey(name))
            {
                return points[name].GetResult();
            }
            return false;
        }

        public double GetBarResult(string name)
        {
            if (bars.ContainsKey(name))
            {
                return bars[name].GetResult();
            }
            return 0;
        }

        public void Execute()
        {
            // Reset stats
            countSuccessful = 0;
            countFail = 0;
            countDone = 0;
            // Get window size
            RECT windowSize = WebinterfaceNative.GetWindowRect((int)hwnd);
            RECT screenArea = new RECT(windowSize.Right, windowSize.Bottom, windowSize.Left, windowSize.Top);
            // Calculate required screen area
            foreach (var pointEntry in points)
            {
                ScreenCheckerPoint point = pointEntry.Value;
                ExpandScreenArea(ref screenArea, windowSize, point.leftPixels, point.leftRatio, point.topPixels, point.topRatio);
            }
            foreach (var barEntry in bars)
            {
                ScreenCheckerBar bar = barEntry.Value;
                ExpandScreenArea(ref screenArea, windowSize, bar.leftPixels, bar.leftRatio, bar.topPixels, bar.topRatio);
                ExpandScreenArea(ref screenArea, windowSize, bar.rightPixels, bar.rightRatio, bar.bottomPixels, bar.bottomRatio);
            }
            // Get screen content
            Bitmap bitmap = WebinterfaceNative.CaptureScreen(hwnd, screenArea.Left, screenArea.Top, screenArea.Right, screenArea.Bottom);
            if (bitmap == null)
            {
                return;
            }
            // Check points
            foreach (var pointEntry in points)
            {
                ScreenCheckerPoint point = pointEntry.Value;
                point.Execute(ref bitmap, ref screenArea, ref windowSize);
                if (point.result)
                {
                    countSuccessful++;
                } else
                {
                    countFail++;
                }
                countDone++;
            }
            // Check bars
            foreach (var barEntry in bars)
            {
                ScreenCheckerBar bar = barEntry.Value;
                bar.Execute(ref bitmap, ref screenArea, ref windowSize);
            }
            // Dispose bitmap
            bitmap.Dispose();
        }

        internal void Debug(string name)
        {
            // Get window size
            RECT windowSize = WebinterfaceNative.GetWindowRect((int)hwnd);
            RECT screenArea = new RECT(windowSize.Right, windowSize.Bottom, windowSize.Left, windowSize.Top);
            // Calculate required screen area
            foreach (var pointEntry in points)
            {
                ScreenCheckerPoint point = pointEntry.Value;
                ExpandScreenArea(ref screenArea, windowSize, point.leftPixels, point.leftRatio, point.topPixels, point.topRatio);
            }
            foreach (var barEntry in bars)
            {
                ScreenCheckerBar bar = barEntry.Value;
                ExpandScreenArea(ref screenArea, windowSize, bar.leftPixels, bar.leftRatio, bar.topPixels, bar.topRatio);
                ExpandScreenArea(ref screenArea, windowSize, bar.rightPixels, bar.rightRatio, bar.bottomPixels, bar.bottomRatio);
            }
            // Get screen content
            Bitmap bitmap = WebinterfaceNative.CaptureScreen(hwnd, screenArea.Left, screenArea.Top, screenArea.Right, screenArea.Bottom);
            if (bitmap == null)
            {
                return;
            }
            // Save for debugging
            bitmap.Save("debug_" + name + ".png");
            // Dispose bitmap
            bitmap.Dispose();
        }
    }
}
