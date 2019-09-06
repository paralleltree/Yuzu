using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing;
using System.Drawing.Drawing2D;

namespace Yuzu.Drawing
{
    public static class GraphicsExtensions
    {
        public static RectangleF GetCenteredRect(this PointF point, float size)
        {
            return point.GetCenteredRect(size, size);
        }

        public static RectangleF GetCenteredRect(this PointF point, SizeF size)
        {
            return point.GetCenteredRect(size.Width, size.Height);
        }

        public static RectangleF GetCenteredRect(this PointF point, float width, float height)
        {
            return new RectangleF(point.X - width / 2f, point.Y - height / 2f, width, height);
        }

        public static GraphicsPath ExpandLinesWidth(this PointF[] points, float width)
        {
            var path = new GraphicsPath();
            for (int i = 1; i < points.Length; i++)
            {
                var poly = new PointF[]
                {
                        new PointF(points[i - 1].X - width / 2f, points[i - 1].Y),
                        new PointF(points[i].X - width / 2f, points[i].Y),
                        new PointF(points[i].X + width / 2f, points[i].Y),
                        new PointF(points[i - 1].X + width /2f, points[i - 1].Y)
                };
                path.AddPolygon(poly);
            }
            return path;
        }

        public static void DrawRectangle(this Graphics g, Pen pen, RectangleF rect)
        {
            g.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);
        }
    }
}
