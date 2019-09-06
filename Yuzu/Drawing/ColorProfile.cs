using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing;

namespace Yuzu.Drawing
{
    public class ColorProfile
    {
        public FieldObjectColor BackgroundColors { get; set; }
        public Color CriticalColor { get; set; }
        public LaneColor Red { get; set; }
        public LaneColor Green { get; set; }
        public LaneColor Blue { get; set; }
        public LaneColor LeftSide { get; set; }
        public LaneColor RightSide { get; set; }
        public FieldObjectColor Flick { get; set; }
        public FieldObjectColor Bell { get; set; }
        public FieldObjectColor Bullet { get; set; }
    }

    public class LaneColor
    {
        public ButtonColor ButtonColor { get; set; }
        public HoldColor HoldColor { get; set; }
        public Color GuideColor { get; set; }
    }

    public class ButtonColor
    {
        public Color BlackColor { get; set; }
        public Color DarkColor { get; set; }
        public Color LightColor { get; set; }
    }

    public class HoldColor
    {
        public Color BaseColor { get; set; }
        public Color MiddleColor { get; set; }
        public Color LineColor { get; set; }
    }

    public class FieldObjectColor
    {
        public Color LightColor { get; set; }
        public Color DarkColor { get; set; }
    }
}
