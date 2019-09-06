using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing;

namespace Yuzu.Drawing
{
    public class DrawingContext
    {
        public Graphics Graphics { get; }
        public ColorProfile ColorProfile { get; }

        public DrawingContext(Graphics g, ColorProfile colorProfile)
        {
            Graphics = g;
            ColorProfile = colorProfile;
        }
    }
}
