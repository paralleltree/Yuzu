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

using Yuzu.Configuration;
using Yuzu.Core;
using Yuzu.Core.Events;
using Yuzu.Core.Track;
using Yuzu.Plugins;
using Yuzu.Properties;
using Yuzu.UI.Operations;
using Yuzu.UI.Forms;
using Yuzu.Media;

namespace Yuzu.UI
{
    internal partial class MainForm : Form
    {
        private readonly string FileExtension = ".ysf";
        private string FileTypeFilter => "Yuzu専用形式" + string.Format("({0})|{1}", "*" + FileExtension, "*" + FileExtension);

        private Exporter lastExportCache;
        private bool isPreviewMode;

        private ScoreBook ScoreBook { get; set; }

        private OperationManager OperationManager { get; }
        private NoteView NoteView { get; }
        private ScrollBar NoteViewScrollBar { get; }

        private SoundSource CurrentSoundSource;
        private PluginManager PluginManager { get; } = PluginManager.GetInstance();
        private Exporter LastExportCache
        {
            get { return lastExportCache; }
            set
            {
                lastExportCache = value;
                LastExportCacheChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private bool IsPreviewMode
        {
            get { return isPreviewMode; }
            set
            {
                isPreviewMode = value;
                NoteView.Editable = !value;
                NoteView.HeightPerBeat = value ? 48 : 120;
                NoteView.HalfLaneWidth = value ? 32 : 120;
                NoteView.SurfaceTapSize = value ? new SizeF(10, 4) : new SizeF(20, 8);
                NoteView.SideTapSize = value ? new SizeF(7, 13) : new SizeF(14, 26);
                NoteView.FlickSize = value ? new SizeF(28 - 1, 5 - 1) : new SizeF(56, 8);
                NoteView.CircularObjectSize = value ? 7 : 11;
            }
        }

        private event EventHandler LastExportCacheChanged;

        public MainForm()
        {
            InitializeComponent();
            Size = new Size(420, 700);
            Icon = Resources.MainIcon;
            AllowDrop = true;
            ToolStripManager.RenderMode = ToolStripManagerRenderMode.System;

            OperationManager = new OperationManager();
            OperationManager.ChangesCommitted += (s, e) => SetText(ScoreBook.Path);
            OperationManager.OperationHistoryChanged += (s, e) =>
            {
                SetText(ScoreBook.Path);
                NoteView.Invalidate();
            };

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

            Menu = CreateMainMenu();
            Controls.Add(NoteView);
            Controls.Add(NoteViewScrollBar);
            Controls.Add(CreateEditTargetToolStrip(NoteView));
            Controls.Add(CreateMainToolStrip(NoteView));

            NoteView.EditMode = EditMode.Edit;
            NoteView.EditTarget = EditTarget.Field;

            LoadEmptyBook();

            if (PluginManager.FailedFiles.Count > 0)
            {
                string message = "以下のプラグインの読み込みに失敗しました。DLLファイルがブロックされているか無効なファイルである可能性があります。";
                MessageBox.Show(this, string.Join("\n", new[] { message }.Concat(PluginManager.FailedFiles)), Program.ApplicationName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        public MainForm(string path) : this()
        {
            LoadFile(path);
        }

        protected override void OnDragEnter(DragEventArgs e)
        {
            base.OnDragEnter(e);
            e.Effect = DragDropEffects.None;
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
            var items = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (items.Length == 1 && Path.GetExtension(items[0]) == FileExtension)
                e.Effect = DragDropEffects.Copy;
        }

        protected override void OnDragDrop(DragEventArgs e)
        {
            base.OnDragDrop(e);
            string path = ((string[])e.Data.GetData(DataFormats.FileDrop))[0];
            if (OperationManager.IsChanged && !ConfirmDiscardChanges()) return;
            LoadFile(path);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            if (OperationManager.IsChanged && !ConfirmDiscardChanges())
            {
                e.Cancel = true;
            }
        }

        protected void Clear()
        {
            if (!OperationManager.IsChanged || ConfirmDiscardChanges())
            {
                LoadEmptyBook();
            }
        }

        protected void OpenFile()
        {
            if (OperationManager.IsChanged && !ConfirmDiscardChanges()) return;

            var dialog = new OpenFileDialog()
            {
                Filter = FileTypeFilter
            };
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                LoadFile(dialog.FileName);
            }
        }

        protected void LoadFile(string path)
        {
            try
            {
                if (!ScoreBook.IsCompatible(path))
                {
                    MessageBox.Show(this, "現在のバージョンでは開けないファイルです。", Program.ApplicationName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (ScoreBook.IsUpgradeNeeded(path))
                {
                    if (MessageBox.Show(this, "古いバージョンで作成されたファイルです。バージョンアップしてよろしいですか？\n(以前のバージョンでは開けなくなります。)", Program.ApplicationName, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
                    {
                        return;
                    }
                }
                LoadBook(ScoreBook.LoadFile(path));
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "ファイルを読み込むことができませんでした。", Program.ApplicationName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                Program.DumpExceptionTo(ex, "file_exception.json");
                LoadEmptyBook();
            }
        }

        protected void LoadBook(ScoreBook book)
        {
            LastExportCache = null;
            ScoreBook = book;
            OperationManager.Clear();
            NoteView.Load(book.Score);
            InitializeScrollBar(book.Score.GetLastTick());
            UpdateThumbHeight();
            SetText(book.Path);

            if (!string.IsNullOrEmpty(book.Path))
            {
                SoundSettings.Default.ScoreSounds.TryGetValue(book.Path, out CurrentSoundSource);
            }
            else
            {
                CurrentSoundSource = null;
            }
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

        protected void SaveAs()
        {
            var dialog = new SaveFileDialog()
            {
                Filter = FileTypeFilter
            };
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                ScoreBook.Path = dialog.FileName;
                SaveFile();
                SetText(ScoreBook.Path);
            }
        }

        protected void SaveFile()
        {
            if (string.IsNullOrEmpty(ScoreBook.Path))
            {
                SaveAs();
                return;
            }
            CommitChanges();
            ScoreBook.Save();
            OperationManager.CommitChanges();
            if (CurrentSoundSource != null)
            {
                SoundSettings.Default.ScoreSounds[ScoreBook.Path] = CurrentSoundSource;
                SoundSettings.Default.Save();
            }
        }

        protected bool ExportFile(Exporter exporter)
        {
            CommitChanges();
            try
            {
                exporter.Run();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "エクスポート中にエラーが発生しました。", Program.ApplicationName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                Program.DumpExceptionTo(ex, "export_exception.json");
                return false;
            }
            LastExportCache = exporter;
            return true;
        }

        protected void CommitChanges()
        {
            ScoreBook.Score = NoteView.Restore();
        }

        private void SetText(string path)
        {
            Text = Program.ApplicationName + (string.IsNullOrEmpty(path) ? "" : " - " + Path.GetFileName(path)) + (OperationManager.IsChanged ? " *" : "");
        }

        private bool ConfirmDiscardChanges()
        {
            return MessageBox.Show(this, "変更は破棄されますがよろしいですか？", Program.ApplicationName, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.Yes;
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

        private MainMenu CreateMainMenu()
        {
            var bookPropertiesItem = new MenuItem("譜面プロパティ(&P)", (s, e) =>
            {
                var form = new BookPropertiesForm(ScoreBook, CurrentSoundSource);
                if (form.ShowDialog(this) != DialogResult.OK) return;

                ScoreBook.Title = form.Title;
                ScoreBook.ArtistName = form.ArtistName;
                ScoreBook.NotesDesignerName = form.NotesDesignerName;
                CurrentSoundSource = form.SoundSource;
            });

            var exportPluginItems = PluginManager.ScoreBookExportPlugins.Select(p => new MenuItem(p.DisplayName, (s, e) =>
            {
                var dialog = new SaveFileDialog() { Filter = p.Filter, Title = "エクスポート先選択" };
                if (dialog.ShowDialog(this) != DialogResult.OK) return;
                var args = new ScoreBookExportPluginArgs(() => ScoreBook.Clone()) { OutputPath = dialog.FileName };
                ExportFile(new Exporter(p, args));
            }));

            var fileMenuItems = new MenuItem[]
            {
                new MenuItem("新規作成(&N)", (s, e) => Clear()) { Shortcut = Shortcut.CtrlN },
                new MenuItem("開く(&O)", (s, e) => OpenFile()) { Shortcut = Shortcut.CtrlO },
                new MenuItem("保存(&S)", (s, e) => SaveFile()) { Shortcut = Shortcut.CtrlS },
                new MenuItem("名前をつけて保存(&A)", (s, e) => SaveAs()) { Shortcut = Shortcut.CtrlShiftS },
                new MenuItem("-"),
                new MenuItem("エクスポート(&E)", exportPluginItems.ToArray()),
                new MenuItem("-"),
                bookPropertiesItem,
                new MenuItem("-"),
                new MenuItem("終了(&X)", (s, e) => Close())
            };

            var undoItem = new MenuItem("元に戻す", (s, e) => OperationManager.Undo())
            {
                Shortcut = Shortcut.CtrlZ,
                Enabled = false
            };
            var redoItem = new MenuItem("やり直す", (s, e) => OperationManager.Redo())
            {
                Shortcut = Shortcut.CtrlY,
                Enabled = false
            };

            var removeEventsItem = new MenuItem("選択範囲内イベントを削除", (s, e) =>
            {
                int head = NoteView.SelectedRange.StartTick + (NoteView.SelectedRange.Duration < 0 ? NoteView.SelectedRange.Duration : 0);
                int tail = NoteView.SelectedRange.StartTick + (NoteView.SelectedRange.Duration < 0 ? 0 : NoteView.SelectedRange.Duration);
                IEnumerable<IOperation> RemoveFilter<T>(List<T> list) where T : EventBase
                {
                    return list.Where(p => p.Tick != 0 && p.Tick >= head && p.Tick <= tail).Select(p => new RemoveEventOperation<T>(p, list));
                }

                var events = NoteView.Score.Events;
                var ops = new[]
                {
                    RemoveFilter(events.BPMChangeEvents),
                    RemoveFilter(events.TimeSignatureChangeEvents),
                    RemoveFilter(events.HighSpeedChangeEvents)
                }.SelectMany(p => p);

                OperationManager.ExecuteAndPush(new CompositeOperation("イベント削除", ops.ToArray()));
            });

            var editMenuItems = new MenuItem[]
            {
                undoItem, redoItem,
                new MenuItem("-"),
                removeEventsItem
            };

            var previewItem = new MenuItem("譜面プレビュー", (s, e) =>
            {
                IsPreviewMode = !IsPreviewMode;
                ((MenuItem)s).Checked = IsPreviewMode;
            }, Shortcut.CtrlP);

            var viewMenuItems = new MenuItem[]
            {
                previewItem
            };

            var addBpmItem = new MenuItem("BPM", (s, e) =>
            {
                var form = new BPMSelectionForm()
                {
                    BPM = NoteView.Score.Events.BPMChangeEvents.OrderBy(p => p.Tick).LastOrDefault(p => p.Tick <= NoteView.CurrentTick)?.BPM ?? 120m
                };
                if (form.ShowDialog(this) != DialogResult.OK) return;

                var item = new BPMChangeEvent()
                {
                    Tick = NoteView.CurrentTick,
                    BPM = form.BPM
                };

                IOperation op = ProcessAddEvent(item, NoteView.Score.Events.BPMChangeEvents);
                OperationManager.ExecuteAndPush(op);
                NoteView.Invalidate();
            });

            var addTimeSignatureItem = new MenuItem("拍子", (s, e) =>
            {
                var form = new TimesignatureSelectionForm();
                if (form.ShowDialog(this) != DialogResult.OK) return;

                var item = new TimeSignatureChangeEvent()
                {
                    Tick = NoteView.CurrentTick,
                    Numerator = form.Numerator,
                    DenominatorExponent = form.DenominatorExponent
                };

                IOperation op = ProcessAddEvent(item, NoteView.Score.Events.TimeSignatureChangeEvents);
                OperationManager.ExecuteAndPush(op);
                NoteView.Invalidate();
            });

            var addHighSpeedItem = new MenuItem("ハイスピード", (s, e) =>
            {
                var form = new HighSpeedSelectionForm();
                if (form.ShowDialog(this) != DialogResult.OK) return;

                var item = new HighSpeedChangeEvent()
                {
                    Tick = NoteView.CurrentTick,
                    SpeedRatio = form.SpeedRatio
                };

                IOperation op = ProcessAddEvent(item, NoteView.Score.Events.HighSpeedChangeEvents);
                OperationManager.ExecuteAndPush(op);
                NoteView.Invalidate();
            });

            IOperation ProcessAddEvent<T>(T item, List<T> list) where T : EventBase
            {
                var prev = list.SingleOrDefault(p => p.Tick == item.Tick);
                var addOp = new AddEventOperation<T>(item, list);
                if (prev == null) return addOp;
                var removeOp = new RemoveEventOperation<T>(prev, list);
                return new CompositeOperation(addOp.Description, new IOperation[] { removeOp, addOp });
            }

            var insertMenuItems = new MenuItem[]
            {
                addBpmItem, addTimeSignatureItem, addHighSpeedItem
            };

            var helpMenuItems = new MenuItem[]
            {
                new MenuItem("Wikiを開く", (s, e) => System.Diagnostics.Process.Start("https://github.com/paralleltree/Yuzu/wiki"), Shortcut.F1),
                new MenuItem("バージョン情報", (s, e) => new VersionInfoForm().ShowDialog(this))
            };

            OperationManager.OperationHistoryChanged += (s, e) =>
            {
                undoItem.Enabled = OperationManager.CanUndo;
                redoItem.Enabled = OperationManager.CanRedo;
            };

            return new MainMenu(new MenuItem[]
            {
                new MenuItem("ファイル(&F)", fileMenuItems),
                new MenuItem("編集(&E)", editMenuItems),
                new MenuItem("表示(&V)", viewMenuItems),
                new MenuItem("挿入(&I)", insertMenuItems),
                new MenuItem("ヘルプ(&H)", helpMenuItems)
            });
        }

        private ToolStrip CreateMainToolStrip(NoteView noteView)
        {
            var newFileButton = new ToolStripButton("新規作成", Resources.NewIcon, (s, e) => Clear())
            {
                DisplayStyle = ToolStripItemDisplayStyle.Image
            };
            var openFileButton = new ToolStripButton("開く", Resources.OpenIcon, (s, e) => OpenFile())
            {
                DisplayStyle = ToolStripItemDisplayStyle.Image
            };
            var saveFileButton = new ToolStripButton("上書き保存", Resources.SaveIcon, (s, e) => SaveFile())
            {
                DisplayStyle = ToolStripItemDisplayStyle.Image
            };
            var exportButton = new ToolStripButton("エクスポート", Resources.ExportIcon, (s, e) =>
            {
                if (LastExportCache == null) return;
                if (!ExportFile(LastExportCache)) return;
                MessageBox.Show(this, "再エクスポートが完了しました。", Program.ApplicationName, MessageBoxButtons.OK, MessageBoxIcon.Information);
            })
            {
                DisplayStyle = ToolStripItemDisplayStyle.Image,
                Enabled = false
            };

            var undoButton = new ToolStripButton("元に戻す", Resources.UndoIcon, (s, e) => OperationManager.Undo())
            {
                DisplayStyle = ToolStripItemDisplayStyle.Image,
                Enabled = false
            };
            var redoButton = new ToolStripButton("やり直す", Resources.RedoIcon, (s, e) => OperationManager.Redo())
            {
                DisplayStyle = ToolStripItemDisplayStyle.Image,
                Enabled = false
            };

            var penButton = new ToolStripButton("ペン", Resources.EditIcon, (s, e) => noteView.EditMode = EditMode.Edit)
            {
                DisplayStyle = ToolStripItemDisplayStyle.Image
            };
            var selectionButton = new ToolStripButton("選択", Resources.SelectionIcon, (s, e) => noteView.EditMode = EditMode.Select)
            {
                DisplayStyle = ToolStripItemDisplayStyle.Image
            };
            var eraserButton = new ToolStripButton("消しゴム", Resources.EraserIcon, (s, e) => noteView.EditMode = EditMode.Erase)
            {
                DisplayStyle = ToolStripItemDisplayStyle.Image
            };

            LastExportCacheChanged += (s, e) => exportButton.Enabled = LastExportCache != null;

            OperationManager.OperationHistoryChanged += (s, e) =>
            {
                undoButton.Enabled = OperationManager.CanUndo;
                redoButton.Enabled = OperationManager.CanRedo;
            };

            noteView.EditModeChanged += (s, e) =>
            {
                penButton.Checked = noteView.EditMode == EditMode.Edit;
                selectionButton.Checked = noteView.EditMode == EditMode.Select;
                eraserButton.Checked = noteView.EditMode == EditMode.Erase;
            };

            return new ToolStrip(new ToolStripItem[]
            {
                newFileButton, openFileButton, saveFileButton, exportButton, new ToolStripSeparator(),
                undoButton, redoButton, new ToolStripSeparator(),
                penButton, selectionButton, eraserButton
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
            quantizeComboBox.Items.Add("カスタム");
            quantizeComboBox.SelectedIndexChanged += (s, e) =>
            {
                if (quantizeComboBox.SelectedIndex == quantizeComboBox.Items.Count - 1)
                {
                    var form = new CustomQuantizeSelectionForm(noteView.Score.TicksPerBeat * 4);
                    if (form.ShowDialog(this) == DialogResult.OK)
                    {
                        NoteView.QuantizeTick = form.QuantizeTick;
                    }
                }
                else
                {
                    noteView.QuantizeTick = noteView.TicksPerBeat * 4 / quantizeTicks[quantizeComboBox.SelectedIndex];
                }
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

    internal class Exporter
    {
        public IScoreBookExportPlugin LastUsedPlugin { get; }
        public ScoreBookExportPluginArgs LastUsedArgs { get; }

        public Exporter(IScoreBookExportPlugin plugin, ScoreBookExportPluginArgs args)
        {
            LastUsedPlugin = plugin;
            LastUsedArgs = args;
        }

        public void Run()
        {
            LastUsedPlugin.Export(LastUsedArgs);
        }
    }
}
