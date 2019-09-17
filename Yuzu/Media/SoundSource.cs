using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yuzu.Media
{
    /// <summary>
    /// 音源を表すクラスです。
    /// </summary>
    [Serializable]
    public class SoundSource
    {
        /// <summary>
        /// この音源における遅延時間を取得します。
        /// この値はタイミングよく音声が出力されるまでの秒数です。
        /// </summary>
        public double Latency { get; }

        public string FilePath { get; }

        public SoundSource(string path, double latency)
        {
            FilePath = path;
            Latency = latency;
        }
    }
}
