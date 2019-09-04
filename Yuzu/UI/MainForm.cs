using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Yuzu.Core;
using Yuzu.Core.Events;
using Yuzu.Core.Track;
using Yuzu.Properties;
using Yuzu.UI.Operations;
using Yuzu.UI.Forms;

namespace Yuzu.UI
{
    internal partial class MainForm : Form
    {
        private ScoreBook ScoreBook { get; set; }

        private OperationManager OperationManager { get; }
        private NoteView NoteView { get; }
        private ScrollBar NoteViewScrollBar { get; }

        public MainForm()
        {
            InitializeComponent();
            Size = new Size(420, 700);
            ToolStripManager.RenderMode = ToolStripManagerRenderMode.System;

            OperationManager = new OperationManager();

            NoteView = new NoteView(OperationManager)
            {
                Dock = DockStyle.Fill
            };

            NoteViewScrollBar = new VScrollBar()
            {
                Dock = DockStyle.Right
            };
            NoteViewScrollBar.ValueChanged += (s, e) =>
            {
                NoteView.HeadTick = -NoteViewScrollBar.Value / 60 * 60; // 60の倍数できれいに表示されるので…
                NoteView.Invalidate();
            };
            NoteViewScrollBar.Scroll += (s, e) =>
            {
                if (e.Type == ScrollEventType.EndScroll)
                {
                    ExtendScrollBarRange(NoteViewScrollBar);
                }
            };

            NoteView.Resize += (s, e) => UpdateThumbHeight();
            NoteView.MouseWheel += (s, e) =>
            {
                int value = NoteViewScrollBar.Value - e.Delta / 120 * NoteViewScrollBar.SmallChange;
                NoteViewScrollBar.Value = Math.Min(Math.Max(value, NoteViewScrollBar.Minimum), NoteViewScrollBar.GetMaximumValue());
                ExtendScrollBarRange(NoteViewScrollBar);
            };
            NoteView.DragScroll += (s, e) =>
            {
                NoteViewScrollBar.Value = Math.Max(-NoteView.HeadTick, NoteViewScrollBar.Minimum);
                ExtendScrollBarRange(NoteViewScrollBar);
            };

            void ExtendScrollBarRange(ScrollBar bar)
            {
                if (bar.Value < bar.Minimum * 0.9f)
                {
                    bar.Minimum = (int)(NoteViewScrollBar.Minimum * 1.2);
                }
            }

            Controls.Add(NoteView);
            Controls.Add(NoteViewScrollBar);
            Controls.Add(CreateEditTargetToolStrip(NoteView));
            Controls.Add(CreateMainToolStrip(NoteView));

            NoteView.EditMode = EditMode.Edit;
            NoteView.EditTarget = EditTarget.Field;

            LoadEmptyBook();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            if (OperationManager.IsChanged && !ConfirmDiscardChanges())
            {
                e.Cancel = true;
            }
        }

        protected void LoadBook(ScoreBook book)
        {
            ScoreBook = book;
            OperationManager.Clear();
            NoteView.Load(book.Score);
            InitializeScrollBar(book.Score.GetLastTick());
            UpdateThumbHeight();
            SetText(book.Path);
        }

        protected void LoadEmptyBook()
        {
            var book = new ScoreBook();
            var score = book.Score;
            score.Events.TimeSignatureChangeEvents.Add(new TimeSignatureChangeEvent() { Tick = 0, Numerator = 4, DenominatorExponent = 2 });
            score.Events.BPMChangeEvents.Add(new BPMChangeEvent() { Tick = 0, BPM = 120 });
            score.Field.Left.FieldWall.Points.Add(new FieldPoint() { Tick = 0, LaneOffset = -score.HalfHorizontalResolution });
            score.Field.Right.FieldWall.Points.Add(new FieldPoint() { Tick = 0, LaneOffset = score.HalfHorizontalResolution });
            LoadBook(book);
        }

        private void SetText(string path)
        {
            Text = Program.ApplicationName + (string.IsNullOrEmpty(path) ? "" : " - " + Path.GetFileName(path)) + (OperationManager.IsChanged ? " *" : "");
        }

        private bool ConfirmDiscardChanges()
        {
            return MessageBox.Show(this, "変更を保存せず終了しますか？", Program.ApplicationName, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.Yes;
        }

        private void InitializeScrollBar(int maxTick)
        {
            NoteViewScrollBar.Value = NoteViewScrollBar.GetMaximumValue();
            NoteViewScrollBar.Minimum = -Math.Max(maxTick, NoteView.TicksPerBeat * 4 * 20);
            NoteViewScrollBar.SmallChange = NoteView.TicksPerBeat;
        }

        private void UpdateThumbHeight()
        {
            NoteViewScrollBar.LargeChange = NoteView.TailTick - NoteView.HeadTick;
            NoteViewScrollBar.Maximum = NoteViewScrollBar.LargeChange + NoteView.PaddingHeadTick;
        }

        private ToolStrip CreateMainToolStrip(NoteView noteView)
        {
            var penButton = new ToolStripButton("ペン", Resources.EditIcon, (s, e) => noteView.EditMode = EditMode.Edit)
            {
                DisplayStyle = ToolStripItemDisplayStyle.Image
            };
            var eraserButton = new ToolStripButton("消しゴム", Resources.EraserIcon, (s, e) => noteView.EditMode = EditMode.Erase)
            {
                DisplayStyle = ToolStripItemDisplayStyle.Image
            };

            noteView.EditModeChanged += (s, e) =>
            {
                penButton.Checked = noteView.EditMode == EditMode.Edit;
                eraserButton.Checked = noteView.EditMode == EditMode.Erase;
            };

            return new ToolStrip(new ToolStripItem[]
            {
                penButton, eraserButton
            });
        }

        private ToolStrip CreateEditTargetToolStrip(NoteView noteView)
        {
            var fieldButton = new ToolStripButton("フィールド", Resources.FieldIcon, (s, e) => noteView.EditTarget = EditTarget.Field)
            {
                DisplayStyle = ToolStripItemDisplayStyle.Image
            };

            var laneButton = new CheckableToolStripSplitButton()
            {
                DisplayStyle = ToolStripItemDisplayStyle.Image,
                Text = "レーン"
            };
            laneButton.Click += (s, e) => noteView.EditTarget = EditTarget.Lane;
            laneButton.DropDown.Items.AddRange(new[]
            {
                new ToolStripMenuItem("赤レーン", Resources.RedLaneIcon, (s, e) => noteView.NewSurfaceLaneColor = Core.Track.SurfaceLaneColor.Red),
                new ToolStripMenuItem("緑レーン", Resources.GreenLaneIcon, (s, e) => noteView.NewSurfaceLaneColor = Core.Track.SurfaceLaneColor.Green),
                new ToolStripMenuItem("青レーン", Resources.BlueLaneIcon, (s,e) => noteView.NewSurfaceLaneColor = Core.Track.SurfaceLaneColor.Blue)
            });
            laneButton.Image = Resources.RedLaneIcon;

            var noteButton = new CheckableToolStripSplitButton()
            {
                DisplayStyle = ToolStripItemDisplayStyle.Image,
                Text = "ノート"
            };
            noteButton.Click += (s, e) => noteView.EditTarget = EditTarget.Note;
            noteButton.DropDown.Items.AddRange(new[]
            {
                new ToolStripMenuItem("TAP", Resources.TapIcon, (s, e) => noteView.NewNoteType = NewNoteType.Tap),
                new ToolStripMenuItem("HOLD", Resources.HoldIcon, (s, e) => noteView.NewNoteType = NewNoteType.Hold)
            });
            noteButton.Image = Resources.TapIcon;

            var flickRightIcon = new Bitmap(Resources.FlickLeftIcon.Size.Width, Resources.FlickLeftIcon.Size.Height);
            using (var g = Graphics.FromImage(flickRightIcon))
            {
                g.TranslateTransform(flickRightIcon.Width, 0);
                g.ScaleTransform(-1, 1);
                g.DrawImage(Resources.FlickLeftIcon, 0, 0);
            }
            var flickButton = new CheckableToolStripSplitButton()
            {
                DisplayStyle = ToolStripItemDisplayStyle.Image,
                Text = "フリック"
            };
            flickButton.Click += (s, e) => noteView.EditTarget = EditTarget.Flick;
            flickButton.DropDown.Items.AddRange(new[]
            {
                new ToolStripMenuItem("左フリック", Resources.FlickLeftIcon, (s, e) => noteView.FlickDirection = HorizontalDirection.Left),
                new ToolStripMenuItem("右フリック", flickRightIcon, (s, e) => noteView.FlickDirection = HorizontalDirection.Right)
            });
            flickButton.Image = Resources.FlickLeftIcon;

            var bellButton = new ToolStripButton("ベル", Resources.BellIcon, (s, e) => noteView.EditTarget = EditTarget.Bell)
            {
                DisplayStyle = ToolStripItemDisplayStyle.Image
            };

            var bulletButton = new ToolStripButton("敵弾", Resources.BulletIcon, (s, e) => noteView.EditTarget = EditTarget.Bullet)
            {
                DisplayStyle = ToolStripItemDisplayStyle.Image
            };

            var quantizeTicks = new int[] { 4, 8, 12, 16, 24, 32, 48, 64, 96, 128, 192 };
            var quantizeComboBox = new ToolStripComboBox("クォンタイズ")
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                AutoSize = false,
                Width = 80
            };
            quantizeComboBox.Items.AddRange(quantizeTicks.Select(p => p + "分").ToArray());
            quantizeComboBox.SelectedIndexChanged += (s, e) =>
            {
                noteView.QuantizeTick = noteView.TicksPerBeat * 4 / quantizeTicks[quantizeComboBox.SelectedIndex];
                noteView.Focus();
            };
            quantizeComboBox.SelectedIndex = 1;

            noteView.EditTargetChanged += (s, e) =>
            {
                fieldButton.Checked = noteView.EditTarget == EditTarget.Field;
                laneButton.Checked = noteView.EditTarget == EditTarget.Lane;
                noteButton.Checked = noteView.EditTarget == EditTarget.Note;
                flickButton.Checked = noteView.EditTarget == EditTarget.Flick;
                bellButton.Checked = noteView.EditTarget == EditTarget.Bell;
                bulletButton.Checked = noteView.EditTarget == EditTarget.Bullet;
            };

            noteView.NewSurfaceLaneColorChanged += (s, e) =>
            {
                switch (noteView.NewSurfaceLaneColor)
                {
                    case SurfaceLaneColor.Red:
                        laneButton.Image = Resources.RedLaneIcon;
                        break;

                    case SurfaceLaneColor.Green:
                        laneButton.Image = Resources.GreenLaneIcon;
                        break;

                    case SurfaceLaneColor.Blue:
                        laneButton.Image = Resources.BlueLaneIcon;
                        break;
                }
            };

            noteView.NewNoteTypeChanged += (s, e) =>
            {
                noteButton.Image = noteView.NewNoteType == NewNoteType.Tap ? Resources.TapIcon : Resources.HoldIcon;
            };

            noteView.FlickDirectionChanged += (s, e) =>
            {
                flickButton.Image = noteView.FlickDirection == HorizontalDirection.Left ? Resources.FlickLeftIcon : flickRightIcon;
            };

            return new ToolStrip(new ToolStripItem[]
            {
                fieldButton, laneButton, noteButton, flickButton, bellButton, bulletButton,
                quantizeComboBox
            });
        }
    }
}
