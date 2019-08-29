using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Yuzu.Collections;
using Yuzu.Core.Track;
using Yuzu.Drawing;

namespace Yuzu.UI
{
    internal partial class NoteView : Control
    {
        private int ticksPerBeat = 480;
        private float heightPerBeat = 120;
        private int horizontalResolution = 20;
        private float halfLaneWidth = 120;

        private int headTick;
        private ColorProfile colorProfile;


        protected Data.Score Score { get; set; }
        protected int TicksPerBeat
        {
            get { return ticksPerBeat; }
            set
            {
                ticksPerBeat = value;
                Invalidate();
            }
        }
        public float HeightPerBeat
        {
            get { return heightPerBeat; }
            set
            {
                heightPerBeat = value;
                Invalidate();
            }
        }
        protected int HorizontalResolution
        {
            get { return horizontalResolution; }
            set
            {
                horizontalResolution = value;
                Invalidate();
            }
        }
        public float HalfLaneWidth
        {
            get { return halfLaneWidth; }
            set
            {
                halfLaneWidth = value;
                Invalidate();
            }
        }
        public float LaneWidth => HalfLaneWidth * 2;

        public int HeadTick
        {
            get { return headTick; }
            set
            {
                headTick = value;
                Invalidate();
            }
        }
        public int TailTick => HeadTick + (int)(ClientSize.Height * TicksPerBeat / HeightPerBeat);
        public ColorProfile ColorProfile
        {
            get { return colorProfile; }
            set
            {
                colorProfile = value;
                Invalidate();
            }
        }
        public SizeF SurfaceTapSize { get; set; } = new Size(20, 8);
        public SizeF SideTapSize { get; set; } = new Size(14, 26);
        public SizeF FlickSize { get; set; } = new Size(56, 8);
        public float CircularObjectSize { get; set; } = 11;

        public NoteView()
        {
            InitializeComponent();
            BackColor = Color.Black;
            DoubleBuffered = true;
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.Opaque, true);

            ColorProfile = new ColorProfile()
            {
                BackgroundColors = new FieldObjectColor() { LightColor = Color.FromArgb(128, 128, 128), DarkColor = Color.FromArgb(64, 64, 64) },
                CriticalColor = Color.FromArgb(255, 222, 60),
                Red = new LaneColor()
                {
                    ButtonColor = new ButtonColor() { DarkColor = Color.FromArgb(224, 62, 62), LightColor = Color.FromArgb(249, 159, 159), BlackColor = Color.FromArgb(152, 42, 42) },
                    HoldColor = new HoldColor() { BaseColor = Color.FromArgb(188, 50, 18), MiddleColor = Color.FromArgb(226, 80, 80), LineColor = Color.FromArgb(240, 112, 112) },
                    GuideColor = Color.FromArgb(208, 64, 64)
                },
                Green = new LaneColor()
                {
                    ButtonColor = new ButtonColor() { DarkColor = Color.FromArgb(0, 155, 33), LightColor = Color.FromArgb(153, 232, 159), BlackColor = Color.FromArgb(0, 100, 20) },
                    HoldColor = new HoldColor() { BaseColor = Color.FromArgb(16, 160, 32), MiddleColor = Color.FromArgb(92, 216, 32), LineColor = Color.FromArgb(150, 240, 112) },
                    GuideColor = Color.FromArgb(60, 160, 60)
                },
                Blue = new LaneColor()
                {
                    ButtonColor = new ButtonColor() { DarkColor = Color.FromArgb(38, 70, 214), LightColor = Color.FromArgb(153, 166, 232), BlackColor = Color.FromArgb(0, 50, 120) },
                    HoldColor = new HoldColor() { BaseColor = Color.FromArgb(16, 92, 180), MiddleColor = Color.FromArgb(32, 114, 214), LineColor = Color.FromArgb(112, 160, 240) },
                    GuideColor = Color.FromArgb(32, 114, 214)
                },
                LeftSide = new LaneColor
                {
                    ButtonColor = new ButtonColor() { DarkColor = Color.FromArgb(66, 0, 204), LightColor = Color.FromArgb(172, 160, 204), BlackColor = Color.FromArgb(38, 0, 122) },
                    HoldColor = new HoldColor() { BaseColor = Color.FromArgb(60, 0, 180), MiddleColor = Color.FromArgb(150, 32, 214), LineColor = Color.FromArgb(190, 112, 240) },
                    GuideColor = Color.FromArgb(86, 32, 216)
                },
                RightSide = new LaneColor()
                {
                    ButtonColor = new ButtonColor() { DarkColor = Color.FromArgb(146, 0, 132), LightColor = Color.FromArgb(198, 160, 204), BlackColor = Color.FromArgb(120, 0, 108) },
                    HoldColor = new HoldColor() { BaseColor = Color.FromArgb(130, 0, 120), MiddleColor = Color.FromArgb(214, 32, 150), LineColor = Color.FromArgb(240, 100, 240) },
                    GuideColor = Color.FromArgb(214, 32, 150)
                },
                Flick = new FieldObjectColor() { DarkColor = Color.FromArgb(204, 116, 16), LightColor = Color.FromArgb(244, 196, 116) },
                Bell = new FieldObjectColor() { DarkColor = Color.FromArgb(240, 240, 32), LightColor = Color.FromArgb(240, 240, 108) },
                Bullet = new FieldObjectColor() { DarkColor = Color.FromArgb(240, 64, 150), LightColor = Color.FromArgb(222, 90, 230) }
            };
        }

        protected float GetYPositionFromTick(int tick)
        {
            return HeightPerBeat * tick / TicksPerBeat;
        }

        protected float GetXPositionFromOffset(int laneOffset)
        {
            return HalfLaneWidth * laneOffset / HorizontalResolution;
        }

        protected PointF GetPositionFromFieldPoint(FieldPoint point)
        {
            return new PointF(GetXPositionFromOffset(point.LaneOffset), GetYPositionFromTick(point.Tick));
        }

        protected float GetInterpolated(float first, float second, float rate)
        {
            return first + (second - first) * rate;
        }

        protected void ExtendFieldToTail(IList<FieldPoint> points, int tailTick)
        {
            var last = points[points.Count - 1];
            if (last.Tick < tailTick)
            {
                points.Add(new FieldPoint() { Tick = tailTick, LaneOffset = last.LaneOffset });
            }
        }
        protected PointF[] GetInterpolatedLines(AVLTree<FieldPoint> line, int start, int end)
        {
            // フィールド境界を出してから実際の始点と終点に対して座標を補正する
            var pos = line.EnumerateFrom(start).TakeUntil(p => p.Tick <= end).ToList();
            if ((pos[pos.Count - 1].Tick < end)) ExtendFieldToTail(pos, end);
            var points = pos.Select(p => GetPositionFromFieldPoint(p)).ToArray();
            if (points.Length == 1) return points; // lineの最終定義以降の直線の場合
            var head = new PointF(GetInterpolated(points[0].X, points[1].X, (float)(start - pos[0].Tick) / (pos[1].Tick - pos[0].Tick)), GetYPositionFromTick(start));
            int last = pos.Count - 1;
            var tail = new PointF(GetInterpolated(points[last - 1].X, points[last].X, (float)(end - pos[last - 1].Tick) / (pos[last].Tick - pos[last - 1].Tick)), GetYPositionFromTick(end));
            points[0] = head;
            points[last] = tail;
            return points;
        }


        protected Matrix GetDrawingMatrix(Matrix baseMatrix, bool flipY)
        {
            var matrix = baseMatrix.Clone();
            if (flipY)
            {
                // Y軸増加方向を時間軸にとる
                matrix.Scale(1, -1);
            }
            // コントロール高さ分ずらす
            matrix.Translate(0, ClientSize.Height - 1, MatrixOrder.Append);
            // 下端をHeadTickと合わせる
            matrix.Translate(0, HeadTick * HeightPerBeat / TicksPerBeat, MatrixOrder.Append);
            // 水平方向に対して中央寄せ
            matrix.Translate(ClientSize.Width / 2, 0);

            return matrix;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var prevMatrix = e.Graphics.Transform;
            e.Graphics.Transform = GetDrawingMatrix(prevMatrix, true);
            var dc = new DrawingContext(e.Graphics, ColorProfile);

            int tailTick = HeadTick + (int)(ClientSize.Height * TicksPerBeat / HeightPerBeat);

            // フィールド端描画
            DrawSideLane(Score.Field.Left, -HalfLaneWidth);
            DrawSideLane(Score.Field.Right, HalfLaneWidth);

            void DrawSideLane(Data.Track.FieldSide fs, float xlimit)
            {
                var borderPositions = fs.FieldWall.Points.EnumerateFrom(HeadTick).TakeUntil(p => p.Tick <= tailTick).ToList();
                // 最終フィールド位置定義から後ろを表示する場合
                // 最終フィールド位置から延長した仮想位置定義を追加
                ExtendFieldToTail(borderPositions, tailTick);
                var borderPoints = borderPositions.Select(p => GetPositionFromFieldPoint(p)).ToArray();
                var guardedSections = fs.FieldWall.GuardedSections.EnumerateFrom(HeadTick).TakeUntil(p => p.StartTick <= tailTick).ToList();
                var guardedPoints = guardedSections.Select(p => GetInterpolatedLines(fs.FieldWall.Points, Math.Max(HeadTick, p.StartTick), Math.Min(tailTick, p.EndTick)));

                using (var fieldRegion = new Region(GetFieldPath(borderPoints, xlimit)))
                {
                    foreach (var points in guardedPoints)
                        using (var guardedPath = GetGuardedPath(points, xlimit / 4f))
                            fieldRegion.Exclude(guardedPath);

                    dc.FillField(fieldRegion);
                }

                dc.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                dc.DrawFieldBorder(borderPoints);
                dc.Graphics.SmoothingMode = SmoothingMode.Default;
            }

            GraphicsPath GetFieldPath(IList<PointF> points, float xlimit)
            {
                var path = new GraphicsPath();
                for (int i = 1; i < points.Count; i++)
                {
                    var poly = new PointF[]
                    {
                        points[i - 1],
                        points[i],
                        new PointF(xlimit, points[i].Y),
                        new PointF(xlimit, points[i - 1].Y)
                    };
                    path.AddPolygon(poly);
                }
                return path;
            }
            GraphicsPath GetGuardedPath(IList<PointF> points, float width)
            {
                // 左フィールド境界は負のwidthで指定
                var path = new GraphicsPath();
                for (int i = 1; i < points.Count; i++)
                {
                    var poly = new PointF[]
                    {
                        points[i - 1],
                        points[i],
                        new PointF(points[i].X + width, points[i].Y),
                        new PointF(points[i - 1].X + width, points[i - 1].Y)
                    };
                    path.AddPolygon(poly);
                }
                return path;
            }


            // ガイド線描画
            // サイド
            DrawSideGuideLine(Score.Field.Left, dc.ColorProfile.LeftSide.GuideColor);
            DrawSideGuideLine(Score.Field.Right, dc.ColorProfile.RightSide.GuideColor);

            void DrawSideGuideLine(Data.Track.FieldSide fs, Color lineColor)
            {
                var guideSections = fs.SideLanes.EnumerateFrom(HeadTick).TakeUntil(p => p.ValidRange.StartTick <= tailTick).ToList();
                var guidePoints = guideSections.Select(p => GetInterpolatedLines(fs.FieldWall.Points, Math.Max(HeadTick, p.ValidRange.StartTick), Math.Min(tailTick, p.ValidRange.EndTick)));

                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var pen = new Pen(lineColor, 2))
                {
                    foreach (var points in guidePoints)
                        dc.Graphics.DrawLines(pen, points);
                }
                e.Graphics.SmoothingMode = SmoothingMode.Default;
            }

            // RGB
            var surfaceLanes = Score.SurfaceLanes.Where(p => p.Points.GetFirst().Tick <= tailTick || p.Points.GetLast().Tick >= HeadTick).ToList();
            foreach (var lane in surfaceLanes)
            {
                DrawSurfaceGuideLine(lane);
            }

            LaneColor GetSurfaceLaneColor(SurfaceLaneColor color)
            {
                return color == SurfaceLaneColor.Red ? dc.ColorProfile.Red :
                    color == SurfaceLaneColor.Green ? dc.ColorProfile.Green :
                    dc.ColorProfile.Blue;
            }

            void DrawSurfaceGuideLine(Data.Track.SurfaceLane lane)
            {
                if (lane.Points.GetFirst().Tick >= tailTick || lane.Points.GetLast().Tick <= HeadTick) return;
                var lanePoints = GetInterpolatedLines(lane.Points, Math.Max(lane.Points.GetFirst().Tick, HeadTick), Math.Min(lane.Points.GetLast().Tick, tailTick));

                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var pen = new Pen(GetSurfaceLaneColor(lane.LaneColor).GuideColor, 2))
                    dc.Graphics.DrawLines(pen, lanePoints);
                e.Graphics.SmoothingMode = SmoothingMode.Default;
            }

            // ロング背景描画
            // サイド
            DrawSideHoldBackground(Score.Field.Left, dc.ColorProfile.LeftSide.HoldColor);
            DrawSideHoldBackground(Score.Field.Right, dc.ColorProfile.RightSide.HoldColor);

            void DrawSideHoldBackground(Data.Track.FieldSide fs, HoldColor holdColor)
            {
                dc.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                foreach (var lane in fs.SideLanes.EnumerateFrom(HeadTick).TakeUntil(p => p.ValidRange.StartTick <= tailTick))
                {
                    foreach (var hold in lane.Notes.EnumerateFrom(HeadTick).TakeUntil(p => p.TickRange.StartTick <= tailTick).Where(p => p.TickRange.Duration > 0))
                    {
                        var points = GetInterpolatedLines(fs.FieldWall.Points, Math.Max(hold.TickRange.StartTick, HeadTick), Math.Min(tailTick, hold.TickRange.EndTick));
                        dc.FillHoldBackground(holdColor, points, SurfaceTapSize.Width * 0.85f);
                    }
                }
                dc.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            }

            // RGB
            foreach (var lane in surfaceLanes)
            {
                DrawSurfaceHoldBackground(lane);
            }

            void DrawSurfaceHoldBackground(Data.Track.SurfaceLane lane)
            {
                dc.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                foreach (var hold in lane.Notes.Where(p => p.TickRange.Duration > 0))
                {
                    var points = GetInterpolatedLines(lane.Points, hold.TickRange.StartTick, hold.TickRange.EndTick);
                    dc.FillHoldBackground(GetSurfaceLaneColor(lane.LaneColor).HoldColor, points, SurfaceTapSize.Width * 0.85f);
                }
                dc.Graphics.SmoothingMode = SmoothingMode.Default;
            }

            // ショートノーツ描画
            // サイド
            DrawSideNotes(Score.Field.Left, dc.ColorProfile.LeftSide, HorizontalDirection.Left);
            DrawSideNotes(Score.Field.Right, dc.ColorProfile.RightSide, HorizontalDirection.Right);

            void DrawSideNotes(Data.Track.FieldSide fs, LaneColor laneColor, HorizontalDirection direction)
            {
                foreach (var lane in fs.SideLanes.EnumerateFrom(HeadTick).TakeUntil(p => p.ValidRange.StartTick <= tailTick))
                {
                    foreach (var note in lane.Notes.EnumerateFrom(HeadTick).TakeUntil(p => p.TickRange.StartTick <= tailTick))
                    {
                        var points = GetInterpolatedLines(fs.FieldWall.Points, note.TickRange.StartTick, note.TickRange.EndTick);
                        var startRect = points[0].GetCenteredRect(SideTapSize);
                        if (note.TickRange.Duration == 0) dc.DrawSideTap(laneColor.ButtonColor, startRect, direction);
                        else
                        {
                            var endRect = points[points.Length - 1].GetCenteredRect(SurfaceTapSize);
                            dc.DrawSideHoldBegin(laneColor.ButtonColor, startRect, direction);
                            dc.DrawHoldEnd(laneColor.ButtonColor, endRect);
                        }
                    }
                }
            }

            // RGB
            foreach (var lane in surfaceLanes)
            {
                DrawSurfaceNotes(lane);
            }

            void DrawSurfaceNotes(Data.Track.SurfaceLane lane)
            {
                foreach (var note in lane.Notes)
                {
                    var points = GetInterpolatedLines(lane.Points, note.TickRange.StartTick, note.TickRange.EndTick);
                    var startRect = points[0].GetCenteredRect(SurfaceTapSize);
                    var buttonColor = GetSurfaceLaneColor(lane.LaneColor).ButtonColor;
                    dc.DrawSurfaceTap(buttonColor, startRect);
                    if (note.TickRange.Duration == 0) continue;
                    var endRect = points[points.Length - 1].GetCenteredRect(SurfaceTapSize);
                    dc.DrawHoldEnd(buttonColor, endRect);
                }
            }

            // オブジェクト系
            {
                foreach (var flick in Score.Flicks.Where(p => p.Position.Tick >= HeadTick && p.Position.Tick <= tailTick))
                {
                    dc.DrawFlick(GetPositionFromFieldPoint(flick.Position).GetCenteredRect(FlickSize), flick.Direction);
                }

                dc.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                foreach (var bell in Score.Bells.Where(p => p.Position.Tick >= HeadTick && p.Position.Tick <= tailTick))
                {
                    dc.DrawBell(GetPositionFromFieldPoint(bell.Position), CircularObjectSize);
                }

                foreach (var bullet in Score.Bullets.Where(p => p.Position.Tick >= HeadTick && p.Position.Tick <= tailTick))
                {
                    dc.DrawBullet(GetPositionFromFieldPoint(bullet.Position), CircularObjectSize);
                }
                dc.Graphics.SmoothingMode = SmoothingMode.Default;
            }

            // テキスト描画

            e.Graphics.Transform = prevMatrix;
        }
    }
}
