using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace LogitechLedTools
{
    public class LogitechColor
    {
        public int Red { get; set; }
        public int Green { get; set; }
        public int Blue { get; set; }

        public LogitechColor()
        {
            Red = 0;
            Green = 0;
            Blue = 0;
        }

        public LogitechColor(Color color)
        {
            Red = color.R;
            Green = color.G;
            Blue = color.B;
        }

        public static LogitechColor MixColors(LogitechColor colorA, LogitechColor colorB, float percentageA)
        {
            return MixColors(colorA, colorB, (int)percentageA);
        }

        public static LogitechColor MixColors(LogitechColor colorA, LogitechColor colorB, int percentageA)
        {
            return MixColors(colorA, colorB, percentageA, true);
        }

        public static LogitechColor MixColors(LogitechColor colorA, LogitechColor colorB, float percentageA, bool brighten)
        {
            return MixColors(colorA, colorB, (int)percentageA, brighten);
        }

        public static LogitechColor MixColors(LogitechColor colorA, LogitechColor colorB, int percentageA, bool brighten)
        {
            if (percentageA > 100)
            {
                percentageA = 100;
            } else if (percentageA < 0)
            {
                percentageA = 0;
            }
            int percentageB = (100 - percentageA);
            int maxValue = 0;
            LogitechColor mixedColor = new LogitechColor();
            mixedColor.Red = maxValue = colorA.Red * percentageA / 100 + colorB.Red * percentageB / 100;
            mixedColor.Green = colorA.Green * percentageA / 100 + colorB.Green * percentageB / 100;
            maxValue = (colorA.Green > maxValue ? colorA.Green : maxValue);
            mixedColor.Blue = colorA.Blue * percentageA / 100 + colorB.Blue * percentageB / 100;
            maxValue = (colorA.Blue > maxValue ? colorA.Blue : maxValue);
            if (brighten)
            {
                // Scale color up to full brightness
                mixedColor.Red = (maxValue > 0 ? mixedColor.Red * 255 / maxValue : 0);
                mixedColor.Green = (maxValue > 0 ? mixedColor.Green * 255 / maxValue : 0);
                mixedColor.Blue = (maxValue > 0 ? mixedColor.Blue * 255 / maxValue : 0);
            }
            return mixedColor;
        }

        public bool Equals(LogitechColor otherColor)
        {
            return ((Red == otherColor.Red) && (Green == otherColor.Green) && (Blue == otherColor.Blue));
        }

        public int getDifference(LogitechColor otherColor)
        {
            return Math.Abs((int)otherColor.Red - Red) + Math.Abs((int)otherColor.Green - Green) + Math.Abs((int)otherColor.Blue - Blue);
        }

        public int getDifference(Color otherColor)
        {
            return Math.Abs((int)otherColor.R - Red) + Math.Abs((int)otherColor.G - Green) + Math.Abs((int)otherColor.B - Blue);
        }

        public int getRedPercentage()
        {
            return Red * 100 / 255;
        }

        public int getGreenPercentage()
        {
            return Green * 100 / 255;
        }

        public int getBluePercentage()
        {
            return Blue * 100 / 255;
        }

        public Color getColor()
        {
            return Color.FromArgb(Red, Green, Blue);
        }

        internal SolidBrush getBrush()
        {
            return new SolidBrush(getColor());
        }

        public void setColor(Color color)
        {
            Red = color.R;
            Green = color.G;
            Blue = color.B;
        }

    }
}
