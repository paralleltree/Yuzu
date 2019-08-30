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
        public float StepRadius { get; set; } = 4;

        protected bool IsDrawObjects => !Editable || (editTarget & (EditTarget.Field | EditTarget.Lane | EditTarget.Note)) == 0;
        protected bool IsDrawNotes => !Editable || (EditTarget & (EditTarget.Field | EditTarget.Lane)) == 0;
        protected bool IsDrawSteps => Editable && (EditTarget & (EditTarget.Field | EditTarget.Lane)) != 0;

        protected OperationManager OperationManager { get; }
        protected CompositeDisposable CompositeDisposable { get; } = new CompositeDisposable();

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
                    dc.DrawSteps(StepColor, lanePoints, StepRadius);
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

                    // 既存オブジェクト操作
                    // レーン系Step移動の際は2分木の構造を崩さない範囲のTick移動しか認めない
                    switch (EditTarget)
                    {
                        case EditTarget.Field:
                        case EditTarget.Lane:
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
                            foreach (var lane in Score.SurfaceLanes)
                            {
                                var subscription = MoveSurfaceLaneStep(lane, pos, tailTick);
                                if (subscription != null) return subscription;
                            }
                            break;

                        case EditTarget.Note:
                            {
                                var subscription = MoveSideNote(Score.Field.Left, pos, tailTick);
                                if (subscription != null) return subscription;
                                subscription = MoveSideNote(Score.Field.Right, pos, tailTick);
                                if (subscription != null) return subscription;
                            }
                            foreach (var lane in Score.SurfaceLanes)
                            {
                                var subscription = MoveSurfaceNote(lane, pos, tailTick);
                                if (subscription != null) return subscription;
                            }
                            break;

                        case EditTarget.Flick:
                        case EditTarget.Bell:
                        case EditTarget.Bullet:
                            foreach (var flick in Score.Flicks)
                            {
                                var subscription = MoveFlick(flick, pos);
                                if (subscription != null) return subscription;
                            }
                            foreach (var bell in Score.Bells)
                            {
                                var subscription = MoveCircularObject(bell, pos);
                                if (subscription != null) return subscription;
                            }
                            foreach (var bullet in Score.Bullets)
                            {
                                var subscription = MoveCircularObject(bullet, pos);
                                if (subscription != null) return subscription;
                            }
                            break;
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
                    (int minTick, int maxTick) = GetAroundPointTick(points, step);
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
                    (int minTick, int maxTick) = GetAroundPointTick(lane.Points, step);
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
            (int, int) GetAroundPointTick(AVLTree<FieldPoint> points, FieldPoint fp)
            {
                // あるノードの前後のノードにおけるTickを取得する
                // 該当するノードが存在しないならば-1を返す。
                int min = -1;
                int max = -1;
                int count = -1;
                foreach (var p in points.EnumerateFrom(fp.Tick - 1))
                {
                    count++;
                    if (p.Tick == fp.Tick) continue;
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
                        if (tick > endTick) return;
                    }
                    else
                    {
                        if (tick < startTick) return;
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
                        var startPoints = fs.FieldWall.Points.EnumerateFrom(note.TickRange.StartTick).Take(2).ToList();
                        float startx = startPoints.Count == 1 ? GetXPositionFromOffset(startPoints[0].LaneOffset) :
                            GetInterpolated(GetXPositionFromOffset(startPoints[0].LaneOffset), GetXPositionFromOffset(startPoints[1].LaneOffset), (float)(note.TickRange.StartTick - startPoints[0].Tick) / (startPoints[1].Tick - startPoints[0].Tick));
                        var startRect = new PointF(startx, GetYPositionFromTick(note.TickRange.StartTick)).GetCenteredRect(SideTapSize);
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

                        var endPoints = fs.FieldWall.Points.EnumerateFrom(note.TickRange.EndTick).Take(2).ToList();
                        float endx = endPoints.Count == 1 ? GetXPositionFromOffset(endPoints[0].LaneOffset) :
                            GetInterpolated(GetXPositionFromOffset(endPoints[0].LaneOffset), GetXPositionFromOffset(endPoints[1].LaneOffset), (float)(note.TickRange.EndTick - endPoints[0].Tick) / (endPoints[1].Tick - endPoints[0].Tick));
                        var endRect = new PointF(endx, GetYPositionFromTick(note.TickRange.EndTick)).GetCenteredRect(SurfaceTapSize);
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
            IObservable<MouseEventArgs> MoveSurfaceNote(Data.Track.SurfaceLane lane, PointF clicked, int tailTick)
            {
                var notes = lane.Notes.EnumerateFrom(HeadTick).TakeUntil(p => p.TickRange.EndTick <= tailTick).ToList();
                foreach (var note in notes)
                {
                    var startPoints = lane.Points.EnumerateFrom(note.TickRange.StartTick).Take(2).ToList();
                    float startx = startPoints.Count == 1 ? GetXPositionFromOffset(startPoints[0].LaneOffset) :
                        GetInterpolated(GetXPositionFromOffset(startPoints[0].LaneOffset), GetXPositionFromOffset(startPoints[1].LaneOffset), (float)(note.TickRange.StartTick - startPoints[0].Tick) / (startPoints[1].Tick - startPoints[0].Tick));
                    int startTick = note.TickRange.StartTick;
                    int duration = note.TickRange.Duration;
                    var startRect = new PointF(startx, GetYPositionFromTick(note.TickRange.StartTick)).GetCenteredRect(SurfaceTapSize);
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

                    var endPoints = lane.Points.EnumerateFrom(note.TickRange.EndTick).Take(2).ToList();
                    float endx = endPoints.Count == 1 ? GetXPositionFromOffset(endPoints[0].LaneOffset) :
                        GetInterpolated(GetXPositionFromOffset(endPoints[0].LaneOffset), GetXPositionFromOffset(endPoints[1].LaneOffset), (float)(note.TickRange.EndTick - endPoints[0].Tick) / (endPoints[1].Tick - endPoints[0].Tick));
                    var endRect = new PointF(endx, GetYPositionFromTick(note.TickRange.EndTick)).GetCenteredRect(SurfaceTapSize);
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

            CompositeDisposable.Add(editSubscription);
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
