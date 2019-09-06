using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing;
using System.Drawing.Drawing2D;

namespace Yuzu.UI
{
    internal static class PlaneExtensions
    {
        public static PointF TransformPoint(this Matrix matrix, PointF point)
        {
            var arr = new[] { point };
            matrix.TransformPoints(arr);
            return arr[0];
        }

        public static Matrix GetInvertedMatrix(this Matrix matrix)
        {
            var dest = matrix.Clone();
            dest.Invert();
            return dest;
        }
    }
}
