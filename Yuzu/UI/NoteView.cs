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

using System.Reactive.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using Yuzu.Collections;
using Yuzu.Core.Track;
using Yuzu.Drawing;
using Yuzu.UI.Operations;

namespace Yuzu.UI
{
    internal partial class NoteView : Control
    {
        private int ticksPerBeat = 480;
        private float heightPerBeat = 120;
        private int horizontalResolution = 20;
        private float halfLaneWidth = 120;

        private int headTick;
        private bool editable = true;
        private EditMode editMode;
        private EditTarget editTarget;
        private NewNoteType newNoteType;
        private SurfaceLaneColor newSurfaceLaneColor;
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
        public int PaddingHeadTick => TicksPerBeat / 8;
        public int QuantizeTick { get; set; } = 480;
        public bool Editable
        {
            get { return editable; }
            set
            {
                editable = value;
                Cursor = value ? Cursors.Default : Cursors.No;
            }
        }
        public EditMode EditMode
        {
            get { return editMode; }
            set
            {
                editMode = value;
            }
        }
        public EditTarget EditTarget
        {
            get { return editTarget; }
            set
            {
                editTarget = value;
            }
        }
        public NewNoteType NewNoteType
        {
            get { return newNoteType; }
            set
            {
                newNoteType = value;
            }
        }
        public SurfaceLaneColor NewSurfaceLaneColor
        {
            get { return newSurfaceLaneColor; }
            set
            {
                newSurfaceLaneColor = value;
            }
        }
        public HorizontalDirection FlickDirection { get; set; }

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
        public Color StepColor { get; set; } = Color.FromArgb(224, 224, 224);
        public float StepRadius { get; set; } = 5;
        public Color HighlightColor { get; set; } = Color.FromArgb(250, 250, 90);

        protected bool IsDrawObjects => !Editable || (editTarget & (EditTarget.Field | EditTarget.Lane | EditTarget.Note)) == 0;
        protected bool IsDrawNotes => !Editable || (EditTarget & (EditTarget.Field | EditTarget.Lane)) == 0;
        protected bool IsDrawSteps => Editable && (EditTarget & (EditTarget.Field | EditTarget.Lane)) != 0;

        protected OperationManager OperationManager { get; }
        protected CompositeDisposable CompositeDisposable { get; } = new CompositeDisposable();

        protected event PaintEventHandler PaintFinished;

        public NoteView(OperationManager operationManager)
        {
            InitializeComponent();
            BackColor = Color.Black;
            DoubleBuffered = true;
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.Opaque, true);

            OperationManager = operationManager;
            InitializeMouseHandler();
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

        protected int GetTickFromYPosition(float y)
        {
            return (int)(y * TicksPerBeat / HeightPerBeat);
        }

        protected int GetOffsetFromXPosition(float x)
        {
            return (int)Math.Round(x * HorizontalResolution / HalfLaneWidth);
        }

        protected int GetQuantizedTick(int tick)
        {
            return (int)(Math.Round((double)tick / QuantizeTick) * QuantizeTick);
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

                if (IsDrawSteps && EditTarget == EditTarget.Field)
                {
                    var borderSteps = fs.FieldWall.Points.EnumerateFrom(HeadTick).TakeUntil(p => p.Tick <= tailTick).Select(p => GetPositionFromFieldPoint(p));
                    dc.DrawSteps(StepColor, borderSteps, StepRadius);
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

                if (IsDrawSteps && EditTarget == EditTarget.Lane)
                {
                    foreach (var lane in guideSections)
                    {
                        var points = GetInterpolatedLines(fs.FieldWall.Points, lane.ValidRange.StartTick, lane.ValidRange.EndTick);
                        dc.DrawSteps(StepColor, new[] { points[0], points[points.Length - 1] }, StepRadius);
                    }
                }
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

                if (IsDrawSteps)
                {
                    var steps = lane.Points.EnumerateFrom(HeadTick).TakeUntil(p => p.Tick <= tailTick).Select(p => GetPositionFromFieldPoint(p));
                    dc.DrawSteps(StepColor, steps, StepRadius);
                }
            }

            // ロング背景描画
            // サイド
            if (IsDrawNotes)
            {
                DrawSideHoldBackground(Score.Field.Left, dc.ColorProfile.LeftSide.HoldColor);
                DrawSideHoldBackground(Score.Field.Right, dc.ColorProfile.RightSide.HoldColor);
            }

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
            if (IsDrawNotes)
            {
                foreach (var lane in surfaceLanes)
                {
                    DrawSurfaceHoldBackground(lane);
                }
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
            if (IsDrawNotes)
            {
                DrawSideNotes(Score.Field.Left, dc.ColorProfile.LeftSide, HorizontalDirection.Left);
                DrawSideNotes(Score.Field.Right, dc.ColorProfile.RightSide, HorizontalDirection.Right);
            }

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
            if (IsDrawNotes)
            {
                foreach (var lane in surfaceLanes)
                {
                    DrawSurfaceNotes(lane);
                }
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
            if (IsDrawObjects)
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
            PaintFinished?.Invoke(this, e);
        }

        protected void InitializeMouseHandler()
        {
            var mouseDown = this.MouseDownAsObservable();
            var mouseMove = this.MouseMoveAsObservable();
            var mouseUp = this.MouseUpAsObservable();

            var editSubscription = mouseDown
                .Where(p => Editable && p.Button == MouseButtons.Left && EditMode == EditMode.Edit)
                .SelectMany(p =>
                {
                    int tailTick = TailTick;
                    Matrix matrix = GetDrawingMatrix(new Matrix(), true);
                    matrix.Invert();
                    PointF pos = matrix.TransformPoint(p.Location);
                    float halfEditWidth = HalfLaneWidth + StepRadius;
                    if (pos.X < -halfLaneWidth || pos.X > halfLaneWidth) return Observable.Empty<MouseEventArgs>();

                    // 既存オブジェクト操作
                    // レーン系Step移動の際は2分木の構造を崩さない範囲のTick移動しか認めない
                    switch (EditTarget)
                    {
                        case EditTarget.Field:
                        case EditTarget.Lane:
                            foreach (var lane in Score.SurfaceLanes.AsEnumerable().Reverse())
                            {
                                var subscription = MoveSurfaceLaneStep(lane, pos, tailTick);
                                if (subscription != null) return subscription;
                            }
                            if (EditTarget == EditTarget.Field)
                            {
                                var subscription = MoveSideLaneStep(Score.Field.Left.FieldWall.Points, pos, tailTick);
                                if (subscription != null) return subscription;
                                subscription = MoveSideLaneStep(Score.Field.Right.FieldWall.Points, pos, tailTick);
                                if (subscription != null) return subscription;
                            }
                            else
                            {
                                var subscription = MoveSideLaneGuide(Score.Field.Left, pos, tailTick);
                                if (subscription != null) return subscription;
                                subscription = MoveSideLaneGuide(Score.Field.Right, pos, tailTick);
                                if (subscription != null) return subscription;
                            }
                            break;

                        case EditTarget.Note:
                            foreach (var lane in Score.SurfaceLanes.AsEnumerable().Reverse())
                            {
                                var subscription = MoveSurfaceNote(lane, pos, tailTick);
                                if (subscription != null) return subscription;
                            }
                            {
                                var subscription = MoveSideNote(Score.Field.Left, pos, tailTick);
                                if (subscription != null) return subscription;
                                subscription = MoveSideNote(Score.Field.Right, pos, tailTick);
                                if (subscription != null) return subscription;
                            }
                            break;

                        case EditTarget.Flick:
                        case EditTarget.Bell:
                        case EditTarget.Bullet:
                            foreach (var bullet in Score.Bullets)
                            {
                                var subscription = MoveCircularObject(bullet, pos);
                                if (subscription != null) return subscription;
                            }
                            foreach (var bell in Score.Bells)
                            {
                                var subscription = MoveCircularObject(bell, pos);
                                if (subscription != null) return subscription;
                            }
                            foreach (var flick in Score.Flicks)
                            {
                                var subscription = MoveFlick(flick, pos);
                                if (subscription != null) return subscription;
                            }
                            break;
                    }

                    // 新規追加
                    switch (EditTarget)
                    {
                        case EditTarget.Field:
                        case EditTarget.Lane:
                            foreach (var lane in Score.SurfaceLanes.AsEnumerable().Reverse())
                            {
                                var subscription = InsertLaneStep(lane.Points, pos, tailTick, true);
                                if (subscription != null) return subscription;
                            }
                            if (EditTarget == EditTarget.Field)
                            {
                                var subscription = InsertLaneStep(Score.Field.Left.FieldWall.Points, pos, tailTick, false);
                                if (subscription != null) return subscription;
                                subscription = InsertLaneStep(Score.Field.Right.FieldWall.Points, pos, tailTick, false);
                                if (subscription != null) return subscription;
                            }
                            else
                            {
                                var subscription = InsertSideLane(Score.Field.Left, pos, tailTick);
                                if (subscription != null) return subscription;
                                subscription = InsertSideLane(Score.Field.Right, pos, tailTick);
                                if (subscription != null) return subscription;
                            }
                            {
                                // RGBレーンの追加
                                var lane = new Data.Track.SurfaceLane() { LaneColor = NewSurfaceLaneColor };
                                var start = new FieldPoint()
                                {
                                    Tick = GetQuantizedTick(GetTickFromYPosition(pos.Y)),
                                    LaneOffset = GetOffsetFromXPosition(pos.X)
                                };
                                var end = new FieldPoint()
                                {
                                    Tick = start.Tick + QuantizeTick,
                                    LaneOffset = start.LaneOffset
                                };
                                lane.Points.Add(start);
                                lane.Points.Add(end);
                                Score.SurfaceLanes.Add(lane);
                                Invalidate();
                                return HandleFieldPoint(end, start.Tick + 1, -1, false)
                                    .Finally(() => OperationManager.Push(new AddSurfaceLaneOperation(lane, Score.SurfaceLanes)));
                            }

                        case EditTarget.Note:
                            foreach (var lane in Score.SurfaceLanes.AsEnumerable().Reverse())
                            {
                                var subscription = InsertSurfaceNote(lane, pos, tailTick);
                                if (subscription != null) return subscription;
                            }
                            {
                                var subscription = InsertSideNote(Score.Field.Left, pos, tailTick);
                                if (subscription != null) return subscription;
                                subscription = InsertSideNote(Score.Field.Right, pos, tailTick);
                                if (subscription != null) return subscription;
                            }
                            break;

                        default:
                            FieldObject obj = null;
                            IOperation op = null;
                            switch (EditTarget)
                            {
                                case EditTarget.Flick:
                                    var flick = new Flick() { Direction = FlickDirection };
                                    op = new AddFieldObjectOperation<Flick>(flick, Score.Flicks);
                                    obj = flick;
                                    break;

                                case EditTarget.Bell:
                                    var bell = new Bell();
                                    op = new AddFieldObjectOperation<Bell>(bell, Score.Bells);
                                    obj = bell;
                                    break;

                                case EditTarget.Bullet:
                                    var bullet = new Bullet();
                                    op = new AddFieldObjectOperation<Bullet>(bullet, Score.Bullets);
                                    obj = bullet;
                                    break;
                            }
                            obj.Position = new FieldPoint()
                            {
                                Tick = GetQuantizedTick(GetTickFromYPosition(pos.Y)),
                                LaneOffset = GetOffsetFromXPosition(pos.X)
                            };
                            op.Redo();
                            Invalidate();
                            return HandleFieldObject(obj).Finally(() => OperationManager.Push(op));
                    }

                    return Observable.Empty<MouseEventArgs>();
                }).Subscribe(p => Invalidate());

            IObservable<MouseEventArgs> MoveSideLaneStep(AVLTree<FieldPoint> points, PointF clicked, int tailTick)
            {
                var steps = points.EnumerateFrom(HeadTick).TakeUntil(p => p.Tick <= tailTick).ToList();
                foreach (var step in steps)
                {
                    var stepRect = GetPositionFromFieldPoint(step).GetCenteredRect(StepRadius * 2);
                    if (!stepRect.Contains(clicked)) continue;
                    (int minTick, int maxTick) = GetAroundPointTick(points, step, true);
                    int beforeTick = step.Tick;
                    int beforeOffset = step.LaneOffset;
                    return HandleFieldPoint(step, minTick + 1, maxTick == -1 ? -1 : maxTick - 1, true)
                        .Finally(() =>
                        {
                            var op = new MoveLaneStepOperation(step, beforeTick, beforeOffset, step.Tick, step.LaneOffset);
                            OperationManager.Push(op);
                        });
                }
                return null;
            }
            IObservable<MouseEventArgs> MoveSurfaceLaneStep(Data.Track.SurfaceLane lane, PointF clicked, int tailTick)
            {
                var steps = lane.Points.EnumerateFrom(HeadTick).TakeUntil(p => p.Tick <= tailTick).ToList();
                foreach (var step in steps)
                {
                    var stepRect = GetPositionFromFieldPoint(step).GetCenteredRect(StepRadius * 2);
                    if (!stepRect.Contains(clicked)) continue;

                    // レーン外に存在するNoteが出ないようにハンドル
                    (int minTick, int maxTick) = GetAroundPointTick(lane.Points, step, true);
                    if (minTick != -1) minTick += 1;
                    if (maxTick != -1) maxTick -= 1;
                    if (step == lane.Points.GetFirst() && lane.Notes.Count > 0)
                    {
                        maxTick = Math.Min(lane.Notes.GetFirst().TickRange.StartTick, maxTick);
                    }
                    if (step == lane.Points.GetLast() && lane.Notes.Count > 0)
                    {
                        minTick = Math.Max(lane.Notes.GetLast().TickRange.EndTick, minTick);
                    }

                    int beforeTick = step.Tick;
                    int beforeOffset = step.LaneOffset;
                    return HandleFieldPoint(step, minTick, maxTick, false)
                        .Finally(() =>
                        {
                            var op = new MoveLaneStepOperation(step, beforeTick, beforeOffset, step.Tick, step.LaneOffset);
                            OperationManager.Push(op);
                        });
                }
                return null;
            }
            IObservable<MouseEventArgs> InsertLaneStep(AVLTree<FieldPoint> points, PointF clicked, int tailTick, bool limitEdge)
            {
                var lines = GetInterpolatedLines(points, Math.Max(HeadTick, points.GetFirst().Tick), limitEdge ? Math.Min(tailTick, points.GetLast().Tick) : tailTick);
                var path = lines.ExpandLinesWidth(StepRadius * 2);
                if (!path.IsVisible(clicked)) return null;
                var point = new FieldPoint()
                {
                    Tick = GetQuantizedTick(GetTickFromYPosition(clicked.Y)),
                    LaneOffset = GetOffsetFromXPosition(clicked.X)
                };
                (int minTick, int maxTick) = GetAroundPointTick(points, point, false);
                if (maxTick == -1 && limitEdge) maxTick = points.GetLast().Tick;
                if (point.Tick <= minTick || (maxTick >= 0 && point.Tick >= maxTick)) return null;
                points.Add(point);
                Invalidate();
                return HandleFieldPoint(point, minTick + 1, maxTick == -1 ? -1 : maxTick - 1, false)
                    .Finally(() => OperationManager.Push(new InsertLaneStepOperation(point, points)));
            }
            IObservable<MouseEventArgs> HandleFieldPoint(FieldPoint fp, int minTick, int maxTick, bool holdZero)
            {
                return mouseMove.TakeUntil(mouseUp).Do(p =>
                {
                    var matrix = GetDrawingMatrix(new Matrix(), true).GetInvertedMatrix();
                    var pos = matrix.TransformPoint(p.Location);
                    int xoffset = GetOffsetFromXPosition(pos.X);
                    int tick = GetQuantizedTick(GetTickFromYPosition(pos.Y));
                    fp.LaneOffset = Math.Min(Math.Max(xoffset, -HorizontalResolution), HorizontalResolution);
                    // 制限Tick外であれば反映しない
                    if ((minTick >= 0 && tick < minTick) || (maxTick >= 0 && tick > maxTick)) return;
                    // holdZero == trueの場合Tick == 0のフィールド定義は移動させない
                    fp.Tick = holdZero && fp.Tick == 0 ? 0 : tick;
                });
            }
            (int, int) GetAroundPointTick(AVLTree<FieldPoint> points, FieldPoint fp, bool contained)
            {
                // あるノードの前後のノードにおけるTickを取得する
                // 該当するノードが存在しないならば-1を返す。
                // contained: fpがpointsに含まれるならばTrue
                int min = -1;
                int max = -1;
                int count = -1;
                foreach (var p in points.EnumerateFrom(contained ? fp.Tick - 1 : fp.Tick))
                {
                    count++;
                    if (contained && p.Tick == fp.Tick) continue;
                    if (count == 0)
                    {
                        min = p.Tick;
                        continue;
                    }
                    else
                    {
                        max = p.Tick;
                        break;
                    }
                }
                return (min, max);
            }

            IObservable<MouseEventArgs> MoveSideLaneGuide(Data.Track.FieldSide fs, PointF clicked, int tailTick)
            {
                var lanes = fs.SideLanes.EnumerateFrom(HeadTick).TakeUntil(p => p.ValidRange.StartTick <= tailTick).ToList();
                foreach (var lane in lanes)
                {
                    var guidePoints = GetInterpolatedLines(fs.FieldWall.Points, lane.ValidRange.StartTick, lane.ValidRange.EndTick);
                    int minTick = 0;
                    int maxTick = -1;
                    var prev = fs.SideLanes.EnumerateFrom(lane.ValidRange.StartTick - 1).FirstOrDefault();
                    if (prev != null && prev != lane) minTick = prev.ValidRange.EndTick + 1;
                    var next = fs.SideLanes.EnumerateFrom(lane.ValidRange.StartTick).Skip(1).FirstOrDefault();
                    if (next != null && next != lane) maxTick = next.ValidRange.StartTick - 1;

                    int beforeStart = lane.ValidRange.StartTick;
                    int beforeDuration = lane.ValidRange.Duration;
                    if (guidePoints[0].GetCenteredRect(StepRadius * 2).Contains(clicked))
                    {
                        if (lane.Notes.Count > 0) maxTick = lane.Notes.GetFirst().TickRange.StartTick;
                        return HandleSideLaneGuideRange(lane.ValidRange, minTick, maxTick, true)
                            .Finally(() =>
                            {
                                var op = new ChangeSideLaneGuideRangeOperation(lane, beforeStart, beforeDuration, lane.ValidRange.StartTick, lane.ValidRange.Duration);
                                OperationManager.Push(op);
                            });
                    }
                    if (guidePoints[guidePoints.Length - 1].GetCenteredRect(StepRadius * 2).Contains(clicked))
                    {
                        if (lane.Notes.Count > 0) minTick = lane.Notes.GetLast().TickRange.EndTick;
                        return HandleSideLaneGuideRange(lane.ValidRange, minTick, maxTick, false)
                            .Finally(() =>
                            {
                                var op = new ChangeSideLaneGuideRangeOperation(lane, beforeStart, beforeDuration, lane.ValidRange.StartTick, lane.ValidRange.Duration);
                                OperationManager.Push(op);
                            });
                    }
                }
                return null;
            }
            IObservable<MouseEventArgs> InsertSideLane(Data.Track.FieldSide fs, PointF clicked, int tailTick)
            {
                var lines = GetInterpolatedLines(fs.FieldWall.Points, HeadTick, tailTick);
                var path = lines.ExpandLinesWidth(StepRadius * 2);
                if (!path.IsVisible(clicked)) return null;
                var range = new TickRange()
                {
                    StartTick = GetQuantizedTick(GetTickFromYPosition(clicked.Y)),
                    Duration = QuantizeTick
                };
                int minTick = fs.SideLanes.EnumerateFrom(range.StartTick).FirstOrDefault()?.ValidRange.EndTick + 1 ?? -1;
                int maxTick = fs.SideLanes.EnumerateFrom(range.StartTick).FirstOrDefault(p => p.ValidRange.StartTick >= range.StartTick)?.ValidRange.StartTick - 1 ?? -1;
                if ((minTick != -1 && range.StartTick < minTick) || (maxTick != -1 && range.EndTick > maxTick)) return null;
                var lane = new Data.Track.SideLane() { ValidRange = range };
                fs.SideLanes.Add(lane);
                Invalidate();
                return HandleSideLaneGuideRange(range, minTick, maxTick, false)
                    .Finally(() => OperationManager.Push(new InsertSideLaneOperation(lane, fs.SideLanes)));
            }
            IObservable<MouseEventArgs> HandleSideLaneGuideRange(TickRange range, int minTick, int maxTick, bool isHead)
            {
                int startTick = range.StartTick;
                int endTick = range.EndTick;
                return mouseMove.TakeUntil(mouseUp).Do(p =>
                {
                    var matrix = GetDrawingMatrix(new Matrix(), true).GetInvertedMatrix();
                    var pos = matrix.TransformPoint(p.Location);
                    int tick = GetQuantizedTick(GetTickFromYPosition(pos.Y));
                    if ((minTick >= 0 && tick < minTick) || (maxTick >= 0 && tick > maxTick)) return;
                    if (isHead)
                    {
                        if (tick >= endTick) return;
                    }
                    else
                    {
                        if (tick <= startTick) return;
                    }
                    if (isHead) range.StartTick = tick;
                    range.Duration = isHead ? endTick - tick : tick - startTick;
                });
            }

            IObservable<MouseEventArgs> MoveSideNote(Data.Track.FieldSide fs, PointF clicked, int tailTick)
            {
                var lanes = fs.SideLanes.EnumerateFrom(HeadTick).TakeUntil(p => p.ValidRange.StartTick <= tailTick).ToList();
                foreach (var lane in lanes)
                {
                    foreach (var note in lane.Notes.EnumerateFrom(HeadTick).TakeUntil(p => p.TickRange.StartTick <= tailTick))
                    {
                        var startRect = GetNotePosition(fs.FieldWall.Points, note.TickRange.StartTick).GetCenteredRect(SideTapSize);
                        if (startRect.Contains(clicked))
                        {
                            int minTick = lane.ValidRange.StartTick;
                            int maxTick = lane.ValidRange.EndTick;
                            var prev = lane.Notes.EnumerateFrom(note.TickRange.StartTick - 1).FirstOrDefault();
                            if (prev != null && prev != note) minTick = Math.Max(prev.TickRange.EndTick + 1, minTick);
                            var next = lane.Notes.EnumerateFrom(note.TickRange.StartTick).Skip(1).FirstOrDefault();
                            if (next != null && next != note) maxTick = Math.Min(next.TickRange.StartTick - 1, maxTick);
                            return HandleNote(note, minTick, maxTick);
                        }

                        var endRect = GetNotePosition(fs.FieldWall.Points, note.TickRange.EndTick).GetCenteredRect(SurfaceTapSize);
                        if (endRect.Contains(clicked))
                        {
                            int maxDuration = lane.ValidRange.EndTick - note.TickRange.StartTick;
                            var next = lane.Notes.EnumerateFrom(note.TickRange.StartTick).Skip(1).FirstOrDefault();
                            if (next != null && next != note) maxDuration = Math.Min(next.TickRange.StartTick - note.TickRange.StartTick - 1, maxDuration);
                            return HandleHoldEnd(note, maxDuration);
                        }
                    }
                }
                return null;
            }
            IObservable<MouseEventArgs> InsertSideNote(Data.Track.FieldSide fs, PointF clicked, int tailTick)
            {
                foreach (var lane in fs.SideLanes)
                {
                    var lines = GetInterpolatedLines(fs.FieldWall.Points, Math.Max(HeadTick, lane.ValidRange.StartTick), Math.Min(tailTick, lane.ValidRange.EndTick));
                    var path = lines.ExpandLinesWidth(StepRadius * 2);
                    if (!path.IsVisible(clicked)) return null;
                    var range = new TickRange()
                    {
                        StartTick = GetQuantizedTick(GetTickFromYPosition(clicked.Y)),
                        Duration = NewNoteType == NewNoteType.Tap ? 0 : QuantizeTick
                    };
                    (int minTick, int maxTick) = GetAroundNoteTick(lane.Notes, range.StartTick);
                    minTick = minTick == -1 ? lane.ValidRange.StartTick : minTick + 1;
                    maxTick = maxTick == -1 ? lane.ValidRange.EndTick : maxTick - 1;
                    if (range.StartTick < minTick || range.EndTick > maxTick) return null;
                    var note = new Note() { TickRange = range };
                    lane.Notes.Add(note);
                    Invalidate();
                    var handler = NewNoteType == NewNoteType.Tap ? HandleNote(note, minTick, maxTick) : HandleHoldEnd(note, maxTick - range.StartTick);
                    return handler.Finally(() => OperationManager.Push(new InsertNoteOperation(note, lane.Notes)));
                }
                return null;
            }
            IObservable<MouseEventArgs> MoveSurfaceNote(Data.Track.SurfaceLane lane, PointF clicked, int tailTick)
            {
                var notes = lane.Notes.EnumerateFrom(HeadTick).TakeUntil(p => p.TickRange.EndTick <= tailTick).ToList();
                foreach (var note in notes)
                {
                    int startTick = note.TickRange.StartTick;
                    int duration = note.TickRange.Duration;
                    var startRect = GetNotePosition(lane.Points, note.TickRange.StartTick).GetCenteredRect(SurfaceTapSize);
                    if (startRect.Contains(clicked))
                    {
                        int minTick = lane.Points.GetFirst().Tick;
                        int maxTick = lane.Points.GetLast().Tick;
                        var prev = lane.Notes.EnumerateFrom(note.TickRange.StartTick - 1).FirstOrDefault();
                        if (prev != null && prev != note) minTick = Math.Max(prev.TickRange.EndTick + 1, minTick);
                        var next = lane.Notes.EnumerateFrom(note.TickRange.StartTick).Skip(1).FirstOrDefault();
                        if (next != null && next != note) maxTick = Math.Min(next.TickRange.StartTick - 1, maxTick);
                        return HandleNote(note, minTick, maxTick)
                            .Finally(() =>
                            {
                                var op = new MoveNoteOperation(note, startTick, duration, note.TickRange.StartTick, note.TickRange.Duration);
                                OperationManager.Push(op);
                            });
                    }

                    var endRect = GetNotePosition(lane.Points, note.TickRange.EndTick).GetCenteredRect(SurfaceTapSize);
                    if (endRect.Contains(clicked))
                    {
                        int maxDuration = lane.Points.GetLast().Tick - note.TickRange.StartTick;
                        var next = lane.Notes.EnumerateFrom(note.TickRange.StartTick).Skip(1).FirstOrDefault();
                        if (next != null && next != note) maxDuration = Math.Min(next.TickRange.StartTick - note.TickRange.StartTick - 1, maxDuration);
                        return HandleHoldEnd(note, maxDuration)
                            .Finally(() =>
                            {
                                var op = new MoveNoteOperation(note, startTick, duration, note.TickRange.StartTick, note.TickRange.Duration);
                                OperationManager.Push(op);
                            });
                    }
                }
                return null;
            }
            IObservable<MouseEventArgs> InsertSurfaceNote(Data.Track.SurfaceLane lane, PointF clicked, int tailTick)
            {
                var lines = GetInterpolatedLines(lane.Points, Math.Max(HeadTick, lane.Points.GetFirst().Tick), Math.Min(tailTick, lane.Points.GetLast().Tick));
                var path = lines.ExpandLinesWidth(StepRadius * 2);
                if (!path.IsVisible(clicked)) return null;
                var range = new TickRange()
                {
                    StartTick = GetQuantizedTick(GetTickFromYPosition(clicked.Y)),
                    Duration = NewNoteType == NewNoteType.Tap ? 0 : QuantizeTick
                };
                (int minTick, int maxTick) = GetAroundNoteTick(lane.Notes, range.StartTick);
                minTick = minTick == -1 ? lane.Points.GetFirst().Tick : minTick + 1;
                maxTick = maxTick == -1 ? lane.Points.GetLast().Tick : maxTick - 1;
                if (range.StartTick < minTick || range.EndTick > maxTick) return null;
                var note = new Note() { TickRange = range };
                lane.Notes.Add(note);
                Invalidate();
                var handler = newNoteType == NewNoteType.Tap ? HandleNote(note, minTick, maxTick) : HandleHoldEnd(note, maxTick - range.StartTick);
                return handler.Finally(() => OperationManager.Push(new InsertNoteOperation(note, lane.Notes)));
            }
            IObservable<MouseEventArgs> HandleNote(Note note, int minTick, int maxTick)
            {
                return mouseMove.TakeUntil(mouseUp).Do(p =>
                {
                    var matrix = GetDrawingMatrix(new Matrix(), true).GetInvertedMatrix();
                    var pos = matrix.TransformPoint(p.Location);
                    int startTick = GetQuantizedTick(GetTickFromYPosition(pos.Y));
                    if (startTick < minTick || startTick + note.TickRange.Duration > maxTick) return;
                    note.TickRange.StartTick = startTick;
                });
            }
            IObservable<MouseEventArgs> HandleHoldEnd(Note note, int maxDuration)
            {
                return mouseMove.TakeUntil(mouseUp).Do(p =>
                {
                    var matrix = GetDrawingMatrix(new Matrix(), true).GetInvertedMatrix();
                    var pos = matrix.TransformPoint(p.Location);
                    int tick = GetQuantizedTick(GetTickFromYPosition(pos.Y));
                    int duration = tick - note.TickRange.StartTick;
                    if (duration <= 0 || duration > maxDuration) return;
                    note.TickRange.Duration = duration;
                });
            }
            (int, int) GetAroundNoteTick(AVLTree<Note> notes, int tick)
            {
                int minTick = -1;
                int maxTick = -1;
                var prev = notes.EnumerateFrom(tick - 1).FirstOrDefault(p => p.TickRange.EndTick <= tick);
                if (prev != null) minTick = prev.TickRange.EndTick;
                var next = notes.EnumerateFrom(tick).FirstOrDefault(p => p.TickRange.StartTick >= tick);
                if (next != null) maxTick = next.TickRange.StartTick;
                return (minTick, maxTick);
            }
            PointF GetNotePosition(AVLTree<FieldPoint> points, int tick)
            {
                var steps = points.EnumerateFrom(tick).Take(2).ToList();
                float x = steps.Count == 1 ? GetXPositionFromOffset(steps[0].LaneOffset) :
                    GetInterpolated(GetXPositionFromOffset(steps[0].LaneOffset), GetXPositionFromOffset(steps[1].LaneOffset), (float)(tick - steps[0].Tick) / (steps[1].Tick - steps[0].Tick));
                return new PointF(x, GetYPositionFromTick(tick));
            }

            IObservable<MouseEventArgs> MoveFlick(Flick flick, PointF clicked)
            {
                var rect = GetPositionFromFieldPoint(flick.Position).GetCenteredRect(FlickSize);
                if (!rect.Contains(clicked)) return null;
                int tick = flick.Position.Tick;
                int offset = flick.Position.LaneOffset;
                return HandleFieldObject(flick)
                    .Finally(() =>
                    {
                        var op = new MoveFieldObjectOperation(flick, tick, offset, flick.Position.Tick, flick.Position.LaneOffset);
                        OperationManager.Push(op);
                    });
            }
            IObservable<MouseEventArgs> MoveCircularObject(FieldObject obj, PointF clicked)
            {
                var rect = GetPositionFromFieldPoint(obj.Position).GetCenteredRect(CircularObjectSize);
                if (!rect.Contains(clicked)) return null;
                int tick = obj.Position.Tick;
                int offset = obj.Position.LaneOffset;
                return HandleFieldObject(obj)
                    .Finally(() =>
                    {
                        var op = new MoveFieldObjectOperation(obj, tick, offset, obj.Position.Tick, obj.Position.LaneOffset);
                        OperationManager.Push(op);
                    });
            }
            IObservable<MouseEventArgs> HandleFieldObject(FieldObject obj)
            {
                return mouseMove.TakeUntil(mouseUp).Do(p =>
                {
                    var matrix = GetDrawingMatrix(new Matrix(), true).GetInvertedMatrix();
                    var pos = matrix.TransformPoint(p.Location);
                    int tick = GetQuantizedTick(GetTickFromYPosition(pos.Y));
                    int offset = GetOffsetFromXPosition(pos.X);
                    obj.Position.Tick = tick < 0 ? 0 : tick;
                    obj.Position.LaneOffset = Math.Max(Math.Min(offset, HorizontalResolution), -HorizontalResolution);
                });
            }

            var eraseSubscription = mouseDown
                .Where(p => Editable && p.Button == MouseButtons.Left && EditMode == EditMode.Erase)
                .SelectMany(p =>
                {
                    var mouseDownAt = p.Location;
                    return mouseMove.TakeUntil(mouseUp).Count().Zip(mouseUp, (q, r) => new { MoveCount = q, MouseUpEventArgs = r });
                })
                .Do(p =>
                {
                    if (p.MoveCount > 0) return; // ドラッグなしクリックのみ処理
                    var matrix = GetDrawingMatrix(new Matrix(), true).GetInvertedMatrix();
                    var pos = matrix.TransformPoint(p.MouseUpEventArgs.Location);
                    var op = ProcessRemove(pos, TailTick, ModifierKeys == Keys.Shift);
                    if (op != null) OperationManager.Push(op);
                }).Subscribe(p => Invalidate());

            IOperation ProcessRemove(PointF clicked, int tailTick, bool shiftPressed)
            {
                float halfEditWidth = HalfLaneWidth + StepRadius;
                if (clicked.X < -halfLaneWidth || clicked.X > halfLaneWidth) return null;

                switch (EditTarget)
                {
                    case EditTarget.Field:
                    case EditTarget.Lane:
                        // Stepの削除
                        if (EditTarget == EditTarget.Field)
                        {
                            var op = RemoveSideLaneStep(Score.Field.Left, clicked, tailTick) ?? RemoveSideLaneStep(Score.Field.Right, clicked, tailTick);
                            if (op != null) return op;
                        }
                        else
                        {
                            var op = RemoveSideLane(Score.Field.Left, clicked, tailTick) ?? RemoveSideLane(Score.Field.Right, clicked, tailTick);
                            if (op != null) return op;
                        }
                        if (shiftPressed)
                        {
                            var op = RemoveSurfaceLane(Score.SurfaceLanes, clicked, tailTick);
                            if (op != null) return op;
                        }
                        else
                        {
                            foreach (var lane in Score.SurfaceLanes)
                            {
                                var op = RemoveSurfaceLaneStep(lane, clicked, tailTick);
                                if (op != null) return op;
                            }
                        }
                        break;

                    case EditTarget.Note:
                        foreach (var lane in Score.SurfaceLanes)
                        {
                            var op = RemoveSurfaceNote(lane, clicked, tailTick);
                            if (op != null) return op;
                        }
                        {
                            var op = RemoveSideNote(Score.Field.Left, clicked, tailTick) ?? RemoveSideNote(Score.Field.Right, clicked, tailTick);
                            if (op != null) return op;
                        }
                        break;

                    case EditTarget.Flick:
                    case EditTarget.Bell:
                    case EditTarget.Bullet:
                        {
                            var op = RemoveFieldObject(Score.Bullets, clicked, tailTick, new SizeF(CircularObjectSize, CircularObjectSize)) ??
                                RemoveFieldObject(Score.Bells, clicked, tailTick, new SizeF(CircularObjectSize, CircularObjectSize)) ??
                                RemoveFieldObject(Score.Flicks, clicked, tailTick, FlickSize);
                            return op;
                        }
                }
                return null;
            }

            IOperation RemoveSideLaneStep(Data.Track.FieldSide fs, PointF clicked, int tailTick)
            {
                var steps = fs.FieldWall.Points.EnumerateFrom(HeadTick).TakeUntil(p => p.Tick <= tailTick).ToList();
                foreach (var step in steps)
                {
                    var rect = GetPositionFromFieldPoint(step).GetCenteredRect(StepRadius * 2);
                    if (!rect.Contains(clicked)) continue;
                    var op = new RemoveLaneStepOperation(step, fs.FieldWall.Points);
                    op.Redo();
                    return op;
                }
                return null;
            }
            IOperation RemoveSurfaceLaneStep(Data.Track.SurfaceLane lane, PointF clicked, int tailTick)
            {
                var steps = lane.Points.EnumerateFrom(HeadTick).TakeUntil(p => p.Tick <= tailTick).ToList();
                foreach (var step in steps)
                {
                    var rect = GetPositionFromFieldPoint(step).GetCenteredRect(StepRadius * 2);
                    if (!rect.Contains(clicked)) continue;
                    if (lane.Points.Count == 2)
                    {
                        var laneOp = new RemoveSurfaceLaneOperation(lane, Score.SurfaceLanes);
                        laneOp.Redo();
                        return laneOp;
                    }
                    if (step.Tick == lane.Points.GetFirst().Tick)
                    {
                        int nextTick = lane.Points.Skip(1).First().Tick;
                        if (lane.Notes.Count > 0 && lane.Notes.GetFirst().TickRange.StartTick < nextTick) continue;
                    }
                    if (step.Tick == lane.Points.GetLast().Tick)
                    {
                        int prevTick = lane.Points.EnumerateFrom(lane.Points.GetLast().Tick - 1).First().Tick;
                        if (lane.Notes.Count > 0 && lane.Notes.GetLast().TickRange.EndTick > prevTick) continue;
                    }
                    var op = new RemoveLaneStepOperation(step, lane.Points);
                    op.Redo();
                    return op;
                }
                return null;
            }
            IOperation RemoveSideLane(Data.Track.FieldSide fs, PointF clicked, int tailTick)
            {
                foreach (var lane in fs.SideLanes.EnumerateFrom(HeadTick).TakeUntil(p => p.ValidRange.StartTick <= tailTick).ToList())
                {
                    var lines = GetInterpolatedLines(fs.FieldWall.Points, Math.Max(HeadTick, lane.ValidRange.StartTick), Math.Min(tailTick, lane.ValidRange.EndTick));
                    var path = lines.ExpandLinesWidth(StepRadius * 2);
                    if (!path.IsVisible(clicked)) continue;
                    var op = new RemoveSideLaneOperation(lane, fs.SideLanes);
                    op.Redo();
                    Invalidate();
                    return op;
                }
                return null;
            }
            IOperation RemoveSurfaceLane(List<Data.Track.SurfaceLane> lanes, PointF clicked, int tailTick)
            {
                foreach (var lane in lanes)
                {
                    var lines = GetInterpolatedLines(lane.Points, Math.Max(HeadTick, lane.Points.GetFirst().Tick), Math.Min(tailTick, lane.Points.GetLast().Tick));
                    var path = lines.ExpandLinesWidth(StepRadius * 2);
                    if (!path.IsVisible(clicked)) continue;
                    var op = new RemoveSurfaceLaneOperation(lane, Score.SurfaceLanes);
                    op.Redo();
                    return op;
                }
                return null;
            }
            IOperation RemoveSideNote(Data.Track.FieldSide fs, PointF clicked, int tailTick)
            {
                foreach (var lane in fs.SideLanes.EnumerateFrom(HeadTick).TakeUntil(p => p.ValidRange.StartTick <= tailTick))
                {
                    var notes = lane.Notes.EnumerateFrom(HeadTick).TakeUntil(p => p.TickRange.StartTick <= tailTick).ToList();
                    foreach (var note in notes)
                    {
                        var startRect = GetNotePosition(fs.FieldWall.Points, note.TickRange.StartTick).GetCenteredRect(SideTapSize);
                        var endRect = GetNotePosition(fs.FieldWall.Points, note.TickRange.EndTick).GetCenteredRect(SurfaceTapSize);
                        var path = GetInterpolatedLines(fs.FieldWall.Points, Math.Max(HeadTick, note.TickRange.StartTick), Math.Min(tailTick, note.TickRange.EndTick)).ExpandLinesWidth(SurfaceTapSize.Width);
                        if (!startRect.Contains(clicked))
                        {
                            if (note.TickRange.Duration == 0) continue;
                            if (!endRect.Contains(clicked) && !path.IsVisible(clicked)) continue;
                        }

                        var op = new RemoveNoteOperation(note, lane.Notes);
                        op.Redo();
                        Invalidate();
                        return op;
                    }
                }
                return null;
            }
            IOperation RemoveSurfaceNote(Data.Track.SurfaceLane lane, PointF clicked, int tailTick)
            {
                var notes = lane.Notes.EnumerateFrom(HeadTick).TakeUntil(p => p.TickRange.StartTick <= tailTick).ToList();
                foreach (var note in notes)
                {
                    var startRect = GetNotePosition(lane.Points, note.TickRange.StartTick).GetCenteredRect(SurfaceTapSize);
                    var endRect = GetNotePosition(lane.Points, note.TickRange.EndTick).GetCenteredRect(SurfaceTapSize);
                    var path = GetInterpolatedLines(lane.Points, Math.Max(HeadTick, note.TickRange.StartTick), Math.Min(tailTick, note.TickRange.EndTick)).ExpandLinesWidth(SurfaceTapSize.Width);
                    if (!startRect.Contains(clicked))
                    {
                        if (note.TickRange.Duration == 0) continue;
                        if (!endRect.Contains(clicked) && !path.IsVisible(clicked)) continue;
                    }

                    var op = new RemoveNoteOperation(note, lane.Notes);
                    op.Redo();
                    Invalidate();
                    return op;
                }
                return null;
            }
            IOperation RemoveFieldObject<T>(List<T> objects, PointF clicked, int tailTick, SizeF size) where T : FieldObject
            {
                foreach (var obj in objects)
                {
                    var rect = GetPositionFromFieldPoint(obj.Position).GetCenteredRect(size);
                    if (!rect.Contains(clicked)) continue;
                    var op = new RemoveFieldObjectOperation<T>(obj, objects);
                    op.Redo();
                    Invalidate();
                    return op;
                }
                return null;
            }

            var eraseDragSubscription = mouseDown
                .Where(p => Editable && p.Button == MouseButtons.Left && EditMode == EditMode.Erase)
                .SelectMany(p =>
                {
                    var matrix = GetDrawingMatrix(new Matrix(), true).GetInvertedMatrix();
                    var pos = matrix.TransformPoint(p.Location);
                    switch (EditTarget)
                    {
                        case EditTarget.Field:
                        case EditTarget.Lane:
                            // RGB範囲消し
                            {
                                var subscription = RemoveSurfaceLaneRange(Score.SurfaceLanes, pos, TailTick);
                                if (subscription != null) return subscription;
                            }
                            // サイド範囲消し
                            {
                                var subscription = RemoveSideLaneRange(Score.Field.Left, pos, TailTick) ?? RemoveSideLaneRange(Score.Field.Right, pos, TailTick);
                                if (subscription != null) return subscription;
                            }
                            break;
                    }
                    return Observable.Empty<MouseEventArgs>();
                }).Subscribe();

            IObservable<MouseEventArgs> RemoveSideLaneRange(Data.Track.FieldSide fs, PointF clicked, int tailTick)
            {
                var lanes = fs.SideLanes.EnumerateFrom(HeadTick).TakeUntil(p => p.ValidRange.StartTick <= TailTick).ToList();
                foreach (var lane in lanes)
                {
                    var lines = GetInterpolatedLines(fs.FieldWall.Points, Math.Max(HeadTick, lane.ValidRange.StartTick), Math.Min(TailTick, lane.ValidRange.EndTick));
                    var path = lines.ExpandLinesWidth(StepRadius * 2);
                    var beginPoint = GetNotePosition(fs.FieldWall.Points, lane.ValidRange.StartTick).GetCenteredRect(StepRadius * 2);
                    var endPoint = GetNotePosition(fs.FieldWall.Points, lane.ValidRange.EndTick).GetCenteredRect(StepRadius * 2);
                    if (!path.IsVisible(clicked) && !beginPoint.Contains(clicked) && !endPoint.Contains(clicked)) continue;

                    int firstTick = GetQuantizedTick(GetTickFromYPosition(clicked.Y));
                    if (beginPoint.Contains(clicked)) firstTick = lane.ValidRange.StartTick;
                    if (endPoint.Contains(clicked)) firstTick = lane.ValidRange.EndTick;
                    (int minTick, int maxTick) = GetAroundNoteTick(lane.Notes, firstTick);
                    if (minTick == -1) minTick = lane.ValidRange.StartTick;
                    if (maxTick == -1) maxTick = lane.ValidRange.EndTick;
                    if (firstTick < minTick || firstTick > maxTick) continue;
                    int secondTick = firstTick;
                    return PaintFinishedAsObservable().TakeUntil(mouseUp)
                        .WithLatestFrom(mouseMove.TakeUntil(mouseUp).Do(p => Invalidate()), (p, q) => new { PaintEventArgs = p, MouseEventArgs = q })
                        .Do(p =>
                        {
                            // 選択範囲描画
                            var prevMatrix = p.PaintEventArgs.Graphics.Transform;
                            var matrix = GetDrawingMatrix(prevMatrix, true);
                            p.PaintEventArgs.Graphics.Transform = matrix;
                            var inverted = matrix.Clone().GetInvertedMatrix();
                            var pos = inverted.TransformPoint(p.MouseEventArgs.Location);
                            int tick = GetQuantizedTick(GetTickFromYPosition(pos.Y));
                            secondTick = Math.Max(Math.Min(tick, maxTick), minTick);
                            using (var pen = new Pen(HighlightColor, 2))
                            {
                                var currentLines = GetInterpolatedLines(fs.FieldWall.Points, Math.Min(firstTick, secondTick), Math.Max(firstTick, secondTick));
                                if (currentLines.Length == 1) return;
                                p.PaintEventArgs.Graphics.DrawLines(pen, currentLines);
                                p.PaintEventArgs.Graphics.DrawEllipse(pen, currentLines[0].GetCenteredRect(StepRadius * 2));
                                p.PaintEventArgs.Graphics.DrawEllipse(pen, currentLines[currentLines.Length - 1].GetCenteredRect(StepRadius * 2));
                            }
                            p.PaintEventArgs.Graphics.Transform = prevMatrix;
                        })
                        .Count().Where(p => p > 0) // 実際にドラッグされたもののみ処理
                        .Zip(mouseUp, (p, q) => q)
                        .Do(p =>
                        {
                            if (firstTick == secondTick) return;
                            int begin = Math.Min(firstTick, secondTick);
                            int end = Math.Max(firstTick, secondTick);
                            IOperation op = null;
                            if (begin == lane.ValidRange.StartTick && end == lane.ValidRange.EndTick)
                            {
                                op = new RemoveSideLaneOperation(lane, fs.SideLanes);
                            }
                            else if (begin == lane.ValidRange.StartTick)
                            {
                                int afterStart = end;
                                int afterDuration = lane.ValidRange.EndTick - afterStart;
                                op = new ChangeSideLaneGuideRangeOperation(lane, lane.ValidRange.StartTick, lane.ValidRange.Duration, afterStart, afterDuration);
                            }
                            else if (end == lane.ValidRange.EndTick)
                            {
                                int afterDuration = begin - lane.ValidRange.StartTick;
                                op = new ChangeSideLaneGuideRangeOperation(lane, lane.ValidRange.StartTick, lane.ValidRange.Duration, lane.ValidRange.StartTick, afterDuration);
                            }
                            else
                            {
                                // レーンの内部で分割
                                int newDuration = begin - lane.ValidRange.StartTick;
                                var movedNotes = lane.Notes.EnumerateFrom(end).SkipWhile(q => q.TickRange.EndTick < end).ToList();
                                var postLane = new Data.Track.SideLane()
                                {
                                    ValidRange = new TickRange() { StartTick = end, Duration = lane.ValidRange.EndTick - end }
                                };
                                foreach (var note in movedNotes) postLane.Notes.Add(note);
                                var ops = movedNotes.Select(q => new RemoveNoteOperation(q, lane.Notes)).Cast<IOperation>().Concat(new IOperation[]
                                {
                                    new ChangeSideLaneGuideRangeOperation(lane, lane.ValidRange.StartTick, lane.ValidRange.Duration, lane.ValidRange.StartTick, newDuration),
                                    new InsertSideLaneOperation(postLane, fs.SideLanes)
                                }).ToArray();
                                op = new CompositeOperation("サイドレーンの削除", ops);
                            }
                            op.Redo();
                            OperationManager.Push(op);
                            Invalidate();
                        });
                }
                return null;
            }
            IObservable<MouseEventArgs> RemoveSurfaceLaneRange(List<Data.Track.SurfaceLane> lanes, PointF clicked, int tailTick)
            {
                foreach (var lane in lanes.AsEnumerable().Reverse())
                {
                    var points = lane.Points.EnumerateFrom(HeadTick).TakeUntil(p => p.Tick <= tailTick).ToList();
                    foreach (var point in points)
                    {
                        var rect = GetPositionFromFieldPoint(point).GetCenteredRect(StepRadius * 2);
                        if (!rect.Contains(clicked)) continue;
                        (int minTick, int maxTick) = GetAroundNoteTick(lane.Notes, point.Tick);
                        if (minTick == -1) minTick = lane.Points.GetFirst().Tick;
                        if (maxTick == -1) maxTick = lane.Points.GetLast().Tick;
                        if (point.Tick < minTick || point.Tick > maxTick) continue;
                        int firstTick = point.Tick;
                        int secondTick = firstTick;
                        return PaintFinishedAsObservable().TakeUntil(mouseUp)
                            .WithLatestFrom(mouseMove.TakeUntil(mouseUp).Do(p => Invalidate()), (p, q) => new { PaintEventArgs = p, MouseEventArgs = q })
                            .Do(p =>
                            {
                                var prevMatrix = p.PaintEventArgs.Graphics.Transform;
                                var matrix = GetDrawingMatrix(prevMatrix, true);
                                p.PaintEventArgs.Graphics.Transform = matrix;
                                var inverted = matrix.Clone().GetInvertedMatrix();
                                var pos = inverted.TransformPoint(p.MouseEventArgs.Location);
                                int tick = GetTickFromYPosition(pos.Y);
                                var steps = lane.Points.EnumerateFrom(tick).Take(2).ToList();
                                if (steps.Count == 1) return;
                                tick = Math.Round((float)(tick - steps[0].Tick) / (steps[1].Tick - steps[0].Tick)) == 0 ? steps[0].Tick : steps[1].Tick;
                                if (!(tick < minTick || tick > maxTick)) secondTick = tick;
                                using (var pen = new Pen(HighlightColor, 2))
                                {
                                    var lines = GetInterpolatedLines(lane.Points, Math.Min(firstTick, secondTick), Math.Max(firstTick, secondTick));
                                    if (lines.Length == 1) return;
                                    p.PaintEventArgs.Graphics.DrawLines(pen, lines);
                                    p.PaintEventArgs.Graphics.DrawEllipse(pen, lines[0].GetCenteredRect(StepRadius * 2));
                                    p.PaintEventArgs.Graphics.DrawEllipse(pen, lines[lines.Length - 1].GetCenteredRect(StepRadius * 2));
                                }
                                p.PaintEventArgs.Graphics.Transform = prevMatrix;
                            })
                            .Count().Where(p => p > 0)
                            .Zip(mouseUp, (p, q) => q)
                            .Do(p =>
                            {
                                if (firstTick == secondTick) return;
                                int begin = Math.Min(firstTick, secondTick);
                                int end = Math.Max(firstTick, secondTick);
                                IOperation op = null;
                                if (begin == lane.Points.GetFirst().Tick && end == lane.Points.GetLast().Tick)
                                {
                                    op = new RemoveSurfaceLaneOperation(lane, lanes);
                                }
                                else if (begin == lane.Points.GetFirst().Tick)
                                {
                                    var ops = lane.Points.TakeWhile(q => q.Tick < end).Select(q => new RemoveLaneStepOperation(q, lane.Points)).ToArray();
                                    op = new CompositeOperation("レーンの削除", ops);
                                }
                                else if (end == lane.Points.GetLast().Tick)
                                {
                                    var ops = lane.Points.EnumerateFrom(begin).Skip(1).Select(q => new RemoveLaneStepOperation(q, lane.Points)).ToArray();
                                    op = new CompositeOperation("レーンの削除", ops);
                                }
                                else
                                {
                                    var movedNotes = lane.Notes.EnumerateFrom(end).SkipWhile(q => q.TickRange.EndTick < end).ToList();
                                    var movedPoints = lane.Points.EnumerateFrom(end).SkipWhile(q => q.Tick < end).ToList();
                                    var postLane = new Data.Track.SurfaceLane()
                                    {
                                        LaneColor = lane.LaneColor
                                    };
                                    foreach (var postPoint in movedPoints)
                                        postLane.Points.Add(postPoint);
                                    foreach (var postNote in movedNotes)
                                        postLane.Notes.Add(postNote);
                                    var ops = movedNotes.Select(q => new RemoveNoteOperation(q, lane.Notes)).Cast<IOperation>()
                                        .Concat(movedPoints.Select(q => new RemoveLaneStepOperation(q, lane.Points)))
                                        .Concat(new IOperation[]
                                        {
                                            new AddSurfaceLaneOperation(postLane, lanes)
                                        }).ToArray();
                                    op = new CompositeOperation("レーンの削除", ops);
                                }
                                op.Redo();
                                OperationManager.Push(op);
                                Invalidate();
                            });
                    }
                }
                return null;
            }
            IObservable<PaintEventArgs> PaintFinishedAsObservable()
            {
                return Observable.FromEvent<PaintEventHandler, PaintEventArgs>(h => (o, e) => h(e), h => this.PaintFinished += h, h => this.PaintFinished -= h);
            }

            CompositeDisposable.Add(editSubscription);
            CompositeDisposable.Add(eraseSubscription);
            CompositeDisposable.Add(eraseDragSubscription);
        }
    }

    internal enum EditMode
    {
        Edit,
        Select,
        Erase
    }

    [Flags]
    internal enum EditTarget
    {
        Field = 1,
        Lane = 1 << 1,
        Note = 1 << 2,
        Flick = 1 << 3,
        Bell = 1 << 4,
        Bullet = 1 << 5
    }

    internal enum NewNoteType
    {
        Tap,
        Hold
    }

    internal static class DrawingExtensions
    {
        public static void DrawSteps(this DrawingContext dc, Color color, IEnumerable<PointF> points, float radius)
        {
            using (var pen = new Pen(color))
            {
                foreach (var point in points)
                    dc.Graphics.DrawEllipse(pen, point.GetCenteredRect(radius * 2));
            }
        }
    }
}
