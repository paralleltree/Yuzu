using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Un4seen.Bass;

namespace Yuzu.Media
{
    public class SoundManager : IDisposable
    {
        private readonly HashSet<int> playing = new HashSet<int>();
        private readonly HashSet<SYNCPROC> syncProcs = new HashSet<SYNCPROC>();
        private readonly Dictionary<string, Queue<int>> handles = new Dictionary<string, Queue<int>>();
        private readonly Dictionary<string, TimeSpan> durations = new Dictionary<string, TimeSpan>();

        public bool IsSupported { get; } = true;

        public event EventHandler ExceptionThrown;

        public void Dispose()
        {
            if (!IsSupported) return;
            Bass.BASS_Stop();
            Bass.BASS_PluginFree(0);
            Bass.BASS_Free();
        }

        public SoundManager()
        {
            if (!Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero))
            {
                IsSupported = false;
            }
        }

        public void Register(string path)
        {
            CheckSupported();
            lock (handles)
            {
                if (handles.ContainsKey(path)) return;
                int handle = GetHandle(path);
                long len = Bass.BASS_ChannelGetLength(handle);
                handles.Add(path, new Queue<int>());
                lock (durations) durations.Add(path, TimeSpan.FromSeconds(Bass.BASS_ChannelBytes2Seconds(handle, len)));
            }
        }

        protected int GetHandle(string filepath)
        {
            int handle = Bass.BASS_StreamCreateFile(filepath, 0, 0, BASSFlag.BASS_DEFAULT);
            if (handle == 0) throw new ArgumentException("cannot create a stream.");
            return handle;
        }

        public void Play(string path)
        {
            Play(path, TimeSpan.Zero);
        }

        public void Play(string path, TimeSpan offset)
        {
            CheckSupported();
            Task.Run(() => PlayInternal(path, offset))
                .ContinueWith(p =>
                {
                    if (p.Exception != null)
                    {
                        Program.DumpExceptionTo(p.Exception, "sound_exception.json");
                        ExceptionThrown?.Invoke(this, EventArgs.Empty);
                    }
                });
        }

        private void PlayInternal(string path, TimeSpan offset)
        {
            Queue<int> freelist;
            lock (handles)
            {
                if (!handles.ContainsKey(path)) throw new InvalidOperationException("sound source was not registered.");
                freelist = handles[path];
            }

            int handle;
            lock (freelist)
            {
                if (freelist.Count > 0) handle = freelist.Dequeue();
                else
                {
                    handle = GetHandle(path);

                    var proc = new SYNCPROC((h, channel, data, user) =>
                    {
                        lock (freelist) freelist.Enqueue(handle);
                    });

                    int syncHandle = Bass.BASS_ChannelSetSync(handle, BASSSync.BASS_SYNC_END, 0, proc, IntPtr.Zero);
                    if (syncHandle == 0) throw new InvalidOperationException("cannot set sync");
                    lock (syncProcs) syncProcs.Add(proc); // avoid GC
                }
            }

            lock (playing) playing.Add(handle);
            Bass.BASS_ChannelSetPosition(handle, offset.TotalSeconds);
            Bass.BASS_ChannelPlay(handle, false);
        }

        public void StopAll()
        {
            CheckSupported();
            lock (playing)
            {
                foreach (int handle in playing)
                {
                    Bass.BASS_ChannelStop(handle);
                }
                playing.Clear();
            }
        }

        public TimeSpan GetDuration(string path)
        {
            Register(path);
            lock (durations) return durations[path];
        }

        protected void CheckSupported()
        {
            if (IsSupported) return;
            throw new NotSupportedException("The sound engine is not supported.");
        }
    }
}
