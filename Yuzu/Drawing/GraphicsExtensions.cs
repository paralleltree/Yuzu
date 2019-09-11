using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

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

        // ref: https://www.codeproject.com/Articles/4958/Combining-GDI-and-GDI-to-Draw-Rubber-Band-Rectangl
        public static void DrawXorRectangle(this Graphics g, PenStyles style, int x1, int y1, int x2, int y2)
        {
            IntPtr hdc = g.GetHdc();
            IntPtr pen = NativeMethods.CreatePen(style, 1, NativeMethods.BLACK_PEN);

            NativeMethods.SetROP2(hdc, NativeMethods.R2_XORPEN);

            IntPtr oldPen = NativeMethods.SelectObject(hdc, pen);
            IntPtr oldBrush = NativeMethods.SelectObject(hdc, NativeMethods.GetStockObject(NativeMethods.NULL_BRUSH));

            NativeMethods.Rectangle(hdc, x1, y1, x2, y2);

            NativeMethods.SelectObject(hdc, oldBrush);
            NativeMethods.SelectObject(hdc, oldPen);
            NativeMethods.DeleteObject(pen);

            g.ReleaseHdc(hdc);
        }

        public static void DrawXorRectangle(this Graphics g, PenStyles style, Point start, Point end)
        {
            g.DrawXorRectangle(style, start.X, start.Y, end.X, end.Y);
        }

        internal class NativeMethods
        {
            [DllImport("gdi32.dll")]
            public static extern int SetROP2(IntPtr hdc, int enDrawMode);

            [DllImport("gdi32.dll")]
            public static extern IntPtr CreatePen(PenStyles enPenStyle, int nWidth, int crColor);

            [DllImport("gdi32.dll")]
            public static extern bool DeleteObject(IntPtr hObject);

            [DllImport("gdi32.dll")]
            public static extern IntPtr SelectObject(IntPtr hdc, IntPtr hObject);

            [DllImport("gdi32.dll")]
            public static extern bool Rectangle(IntPtr hdc, int x1, int y1, int x2, int y2);

            [DllImport("gdi32.dll")]
            public static extern IntPtr GetStockObject(int brStyle);

            public static readonly int BLACK_PEN = 0;
            public static readonly int R2_XORPEN = 7;
            public static readonly int NULL_BRUSH = 5;
        }
    }

    public enum PenStyles
    {
        Solid = 0,
        Dash = 1,
        Dot = 2,
        DashDot = 3,
        DashDotDot = 4
    }
}
