using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Yuzu.Core;
using Yuzu.Core.Events;
using Yuzu.Core.Track;

namespace Yuzu.Media
{
    public class SoundPreviewManager : IDisposable
    {
        public event EventHandler<TickUpdatedEventArgs> TickUpdated;
        public event EventHandler Finished;
        public event EventHandler ExceptionThrown;

        public int TicksPerBeat { get; set; } = 480;
        private int CurrentTick { get; set; }
        public SoundSource ClapSource { get; set; }
        private SoundManager SoundManager { get; } = new SoundManager();
        private LinkedListNode<int> TickElement;
        private LinkedListNode<BPMChangeEvent> BPMElement;
        private int LastSystemTick { get; set; }
        private int InitialTick { get; set; }
        private int StartTick { get; set; }
        private int EndTick { get; set; }
        private double ElapsedTick { get; set; }
        private Control SyncObject { get; }
        private Timer Timer { get; } = new Timer() { Interval = 4 };

        public bool Playing { get; private set; }
        public bool IsStopAtLastNote { get; set; }
        public bool IsSupported => SoundManager.IsSupported;

        public SoundPreviewManager(Control syncObj)
        {
            SyncObject = syncObj;
            Timer.Tick += Tick;
            SoundManager.ExceptionThrown += (s, e) =>
            {
                Stop();
                ExceptionThrown?.Invoke(this, EventArgs.Empty);
            };
        }

        public bool Start(SoundSource music, int startTick, HashSet<int> ticks, IEnumerable<BPMChangeEvent> bpms)
        {
            if (Playing) throw new InvalidOperationException();
            if (music == null) throw new ArgumentNullException("music");
            if (IsStopAtLastNote && ticks.Count == 0) return false;
            SoundManager.Register(ClapSource.FilePath);
            SoundManager.Register(music.FilePath);
            EndTick = IsStopAtLastNote ? ticks.Max() : GetTickFromTime(SoundManager.GetDuration(music.FilePath), bpms);
            if (EndTick < startTick) return false;

            TickElement = new LinkedList<int>(ticks.Where(p => p >= startTick).OrderBy(p => p)).First;
            BPMElement = new LinkedList<BPMChangeEvent>(bpms.OrderBy(p => p.Tick)).First;

            // スタート時まで進める
            while (TickElement != null && TickElement.Value < startTick) TickElement = TickElement.Next;
            while (BPMElement.Next != null && BPMElement.Next.Value.Tick <= startTick) BPMElement = BPMElement.Next;

            int clapLatencyTick = GetLatencyTick(ClapSource.Latency, (double)BPMElement.Value.BPM);
            InitialTick = startTick - clapLatencyTick;
            CurrentTick = InitialTick;
            StartTick = startTick;

            TimeSpan startTime = GetTimeFromTick(startTick, bpms);
            TimeSpan headGap = TimeSpan.FromSeconds(-music.Latency) - startTime;
            ElapsedTick = 0;
            Task.Run(() =>
            {
                LastSystemTick = Environment.TickCount;
                SyncObject.Invoke((MethodInvoker)(() => Timer.Start()));

                System.Threading.Thread.Sleep(TimeSpan.FromSeconds(Math.Max(ClapSource.Latency, 0)));
                if (headGap.TotalSeconds > 0)
                {
                    System.Threading.Thread.Sleep(headGap);
                }
                if (!Playing) return; // 音源再生前に停止されたら取り消す
                SoundManager.Play(music.FilePath, startTime + TimeSpan.FromSeconds(music.Latency));
            });

            Playing = true;
            return true;
        }

        public void Stop()
        {
            SyncObject.Invoke((MethodInvoker)(() => Timer.Stop()));
            SoundManager.StopAll();
            Playing = false;
            Finished?.Invoke(this, EventArgs.Empty);
        }

        private void Tick(object sender, EventArgs e)
        {
            int now = Environment.TickCount;
            int elapsed = now - LastSystemTick;
            LastSystemTick = now;

            ElapsedTick += TicksPerBeat * (double)BPMElement.Value.BPM * elapsed / 60 / 1000;
            CurrentTick = (int)(InitialTick + ElapsedTick);
            if (CurrentTick >= StartTick)
            {
                TickUpdated?.Invoke(this, new TickUpdatedEventArgs(Math.Max(CurrentTick, 0)));
            }

            while (BPMElement.Next != null && BPMElement.Next.Value.Tick <= CurrentTick)
            {
                BPMElement = BPMElement.Next;
            }

            if (CurrentTick >= EndTick + TicksPerBeat)
            {
                Stop();
            }

            int latencyTick = GetLatencyTick(ClapSource.Latency, (double)BPMElement.Value.BPM);
            if (TickElement == null || TickElement.Value - latencyTick > CurrentTick) return;
            while (TickElement != null && TickElement.Value - latencyTick <= CurrentTick)
            {
                TickElement = TickElement.Next;
            }

            SoundManager.Play(ClapSource.FilePath);
        }

        private int GetLatencyTick(double latency, double bpm)
        {
            return (int)(TicksPerBeat * latency * bpm / 60);
        }

        private TimeSpan GetLatencyTime(int tick, double bpm)
        {
            return TimeSpan.FromSeconds((double)tick * 60 / TicksPerBeat / bpm);
        }

        private TimeSpan GetTimeFromTick(int tick, IEnumerable<BPMChangeEvent> bpms)
        {
            var bpm = new LinkedList<BPMChangeEvent>(bpms.OrderBy(p => p.Tick)).First;
            if (bpm.Value.Tick != 0) throw new ArgumentException("Initial BPM change event not found");

            var time = new TimeSpan();
            while (bpm.Next != null)
            {
                if (tick < bpm.Next.Value.Tick) break; // 現在のBPMで到達
                time += GetLatencyTime(bpm.Next.Value.Tick - bpm.Value.Tick, (double)bpm.Value.BPM);
                bpm = bpm.Next;
            }
            return time + GetLatencyTime(tick - bpm.Value.Tick, (double)bpm.Value.BPM);
        }

        private int GetTickFromTime(TimeSpan time, IEnumerable<BPMChangeEvent> bpms)
        {
            var bpm = new LinkedList<BPMChangeEvent>(bpms.OrderBy(p => p.Tick)).First;
            if (bpm.Value.Tick != 0) throw new ArgumentException("Initial BPM change event not found");

            TimeSpan sum = new TimeSpan();
            while (bpm.Next != null)
            {
                TimeSpan section = GetLatencyTime(bpm.Next.Value.Tick - bpm.Value.Tick, (double)bpm.Value.BPM);
                if (time < sum + section) break;
                sum += section;
                bpm = bpm.Next;
            }
            return bpm.Value.Tick + GetLatencyTick((time - sum).TotalSeconds, (double)bpm.Value.BPM);
        }

        public void Dispose()
        {
            SoundManager.Dispose();
        }
    }

    public class TickUpdatedEventArgs : EventArgs
    {
        public int Tick { get; }

        public TickUpdatedEventArgs(int tick)
        {
            Tick = tick;
        }
    }

    internal static class Extensions
    {
        public static bool Start(this SoundPreviewManager manager, Score score, SoundSource source, int startTick, bool onlyNotes)
        {
            return manager.Start(source, startTick, score.GetGuideTicks(onlyNotes), score.Events.BPMChangeEvents);
        }

        public static HashSet<int> GetGuideTicks(this Score score, bool onlyNotes)
        {
            var set = new HashSet<int>();

            IEnumerable<int> ExtractNotes(IEnumerable<Note> notes) => notes.SelectMany(q => q.TickRange.Duration == 0 ? new[] { q.TickRange.StartTick } : new[] { q.TickRange.StartTick, q.TickRange.EndTick });
            IEnumerable<int> GetSideLaneTicks(FieldSide fs) => fs.SideLanes.SelectMany(p => ExtractNotes(p.Notes));
            foreach (int tick in GetSideLaneTicks(score.Field.Left)) set.Add(tick);
            foreach (int tick in GetSideLaneTicks(score.Field.Right)) set.Add(tick);

            foreach (int tick in score.SurfaceLanes.SelectMany(p => ExtractNotes(p.Notes))) set.Add(tick);

            foreach (var flick in score.Flicks) set.Add(flick.Position.Tick);

            if (onlyNotes) return set;

            foreach (var bell in score.Bells) set.Add(bell.Position.Tick);
            foreach (var bullet in score.Bullets) set.Add(bullet.Position.Tick);

            return set;
        }
    }
}
