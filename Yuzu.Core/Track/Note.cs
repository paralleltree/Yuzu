using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yuzu.Core.Track
{
    public class Note
    {
        /// <summary>
        /// 時間軸におけるノートの位置を設定します。
        /// </summary>
        /// <remarks>
        /// Durationが0の場合はTAPを表します。
        /// </remarks>
        public TickRange TickRange { get; set; }
        public bool IsCritical { get; set; }
    }
}
