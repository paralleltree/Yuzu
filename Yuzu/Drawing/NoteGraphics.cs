using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing;
using System.Drawing.Drawing2D;
using Yuzu.Core.Track;

namespace Yuzu.Drawing
{
    public static class NoteGraphics
    {
        public static void DrawFieldBorder(this DrawingContext dc, PointF[] points)
        {
            using (var pen = new Pen(dc.ColorProfile.BackgroundColors.LightColor))
                dc.Graphics.DrawLines(pen, points);
        }

        public static void FillField(this DrawingContext dc, Region region)
        {
            using (var brush = new HatchBrush(HatchStyle.WideUpwardDiagonal, dc.ColorProfile.BackgroundColors.LightColor, dc.ColorProfile.BackgroundColors.DarkColor))
                dc.Graphics.FillRegion(brush, region);
        }

        public static void FillHoldBackground(this DrawingContext dc, HoldColor holdColor, PointF[] points, float width)
        {
            var pairs = new[]
            {
                Tuple.Create(holdColor.BaseColor, width),
                Tuple.Create(holdColor.MiddleColor, width / 5 * 3),
                Tuple.Create(holdColor.LineColor, width / 5)
            };
            foreach (var item in pairs)
            {
                using (var brush = new SolidBrush(item.Item1))
                using (var path = points.ExpandLinesWidth(item.Item2))
                {
                    dc.Graphics.FillPath(brush, path);
                }
            }
        }

        public static void DrawSurfaceTap(this DrawingContext dc, ButtonColor buttonColor, RectangleF rect)
        {
            using (var brush = new SolidBrush(buttonColor.DarkColor))
            using (var pen = new Pen(buttonColor.LightColor))
            {
                float lineRate = 0.8f;
                float centerY = rect.Top + rect.Height / 2;
                dc.Graphics.FillRectangle(brush, rect);
                dc.Graphics.DrawRectangle(pen, rect);
                dc.Graphics.DrawLine(pen, rect.Left + rect.Width * lineRate, centerY, rect.Right - rect.Width * lineRate, centerY);
            }
        }

        public static void DrawSideTap(this DrawingContext dc, ButtonColor buttonColor, RectangleF rect, HorizontalDirection direction)
        {
            dc.DrawSideTap(buttonColor, rect, direction, false);
        }

        public static void DrawSideTap(this DrawingContext dc, ButtonColor buttonColor, RectangleF rect, HorizontalDirection direction, bool isHold)
        {
            // ノートを内包するRect(ノートの中心が原点)
            var box = new RectangleF(-rect.Width / 2f, -rect.Height / 2f, rect.Width, rect.Height);
            // 右壁ドンの構成点
            var points = new PointF[]
            {
                new PointF(box.Left, 0),
                new PointF(0, box.Top),
                new PointF(box.Right, box.Top),
                new PointF(box.Right, box.Bottom),
                new PointF(0, box.Bottom)
            };

            var prevMatrix = dc.Graphics.Transform;
            var matrix = prevMatrix.Clone();
            if (direction == HorizontalDirection.Left) matrix.Scale(-1, 1);
            matrix.Translate((direction == HorizontalDirection.Left ? -1 : 1) * (rect.Left + rect.Width / 2f), rect.Top + rect.Height / 2f);
            dc.Graphics.Transform = matrix;

            if (isHold)
            {
                // 壁ドンのロング始点ベース
                var basePoints = new PointF[]
                {
                new PointF(box.Left, 0),
                new PointF(0, box.Top),
                new PointF(box.Right, box.Top),
                new PointF(box.Right * 2, 0),
                new PointF(box.Right, box.Bottom),
                new PointF(0, box.Bottom)
                };
                using (var path = new GraphicsPath())
                {
                    path.AddPolygon(basePoints);
                    using (var brush = new SolidBrush(buttonColor.LightColor))
                    {
                        dc.Graphics.FillPath(brush, path);
                    }
                }
            }

            using (var path = new GraphicsPath())
            {
                path.AddPolygon(points);
                using (var brush = new SolidBrush(buttonColor.DarkColor))
                using (var pen = new Pen(buttonColor.LightColor))
                {
                    dc.Graphics.FillPath(brush, path);
                    dc.Graphics.DrawPath(pen, path);
                    dc.Graphics.DrawLine(pen, 0, 0, box.Left, 0);
                }

            }
            dc.Graphics.Transform = prevMatrix;
        }

        public static void DrawSideHoldBegin(this DrawingContext dc, ButtonColor buttonColor, RectangleF rect, HorizontalDirection direction)
        {
            dc.DrawSideTap(buttonColor, rect, direction, true);
        }

        public static void DrawHoldEnd(this DrawingContext dc, ButtonColor buttonColor, RectangleF rect)
        {
            using (var brush = new SolidBrush(buttonColor.BlackColor))
            using (var pen = new Pen(buttonColor.DarkColor))
            {
                dc.Graphics.FillRectangle(brush, rect);
                dc.Graphics.DrawRectangle(pen, rect);
            }
        }

        public static void DrawFlick(this DrawingContext dc, RectangleF rect, HorizontalDirection direction)
        {
            // 下の長方形
            using (var brush = new SolidBrush(dc.ColorProfile.Flick.DarkColor))
            using (var pen = new Pen(dc.ColorProfile.Flick.LightColor))
            {
                float lineRate = 0.9f;
                float centerY = rect.Top + rect.Height / 2f;
                dc.Graphics.FillRectangle(brush, rect);
                dc.Graphics.DrawRectangle(pen, rect);
                dc.Graphics.DrawLine(pen, new PointF(rect.Left + rect.Width * lineRate, centerY), new PointF(rect.Right - rect.Width * lineRate, centerY));
            }

            // 矢印
            // 一つの矢印を内包する原点中心のbox
            var box = new RectangleF(-rect.Height * 1.2f, -rect.Height * 1.2f, rect.Height * 2.4f, rect.Height * 2.4f);
            var points = new PointF[]
            {
                new PointF(box.Left, 0),
                new PointF(0, box.Top),
                new PointF(box.Right, box.Top),
                new PointF(0, 0),
                new PointF(box.Right, box.Bottom),
                new PointF(0, box.Bottom)
            };

            using (var path = new GraphicsPath())
            {
                path.AddPolygon(points);
                var prevMatrix = dc.Graphics.Transform;
                var matrix = prevMatrix.Clone();

                matrix.Translate(rect.Left + rect.Width / 2f, rect.Top + rect.Height / 2f + rect.Height * 2.1f);

                using (var brush = new SolidBrush(dc.ColorProfile.Flick.DarkColor))
                using (var pen = new Pen(dc.ColorProfile.Flick.LightColor))
                {
                    for (int i = 0; i < 3; i++)
                    {
                        var arrowMatrix = matrix.Clone();
                        arrowMatrix.Translate((i - 1) * rect.Height * 2f, 0);
                        dc.Graphics.Transform = arrowMatrix;
                        dc.Graphics.FillPath(brush, path);
                        dc.Graphics.DrawPath(pen, path);
                    }
                }

                dc.Graphics.Transform = prevMatrix;
            }
        }

        public static void DrawCircularObject(this DrawingContext dc, PointF point, float size, FieldObjectColor color)
        {
            var prevMatrix = dc.Graphics.Transform;
            var matrix = prevMatrix.Clone();
            matrix.Translate(point.X, point.Y);
            dc.Graphics.Transform = matrix;
            dc.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            using (var outer = new GraphicsPath())
            using (var inner = new GraphicsPath())
            {
                float innerSize = size * 0.5f;
                var outerRect = new RectangleF(-size / 2, -size / 2, size, size);
                outer.AddEllipse(outerRect);
                inner.AddEllipse(-innerSize / 2, -innerSize / 2, innerSize, innerSize);
                using (var region = new Region(outer))
                using (var brush = new LinearGradientBrush(outerRect, color.LightColor, color.DarkColor, LinearGradientMode.Vertical))
                {
                    region.Exclude(inner);
                    dc.Graphics.FillRegion(brush, region);
                }
            }

            dc.Graphics.Transform = prevMatrix;
            dc.Graphics.SmoothingMode = SmoothingMode.Default;
        }

        public static void DrawBell(this DrawingContext dc, PointF point, float size)
        {
            dc.DrawCircularObject(point, size, dc.ColorProfile.Bell);
        }

        public static void DrawBullet(this DrawingContext dc, PointF point, float size)
        {
            dc.DrawCircularObject(point, size, dc.ColorProfile.Bullet);
        }
    }
}
