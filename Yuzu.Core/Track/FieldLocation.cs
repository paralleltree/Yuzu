using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yuzu.Core.Track
{
    /// <summary>
    /// 譜面における位置を表します。
    /// </summary>
    [Newtonsoft.Json.JsonObject(Newtonsoft.Json.MemberSerialization.OptIn)]
    public class FieldPoint
    {
        [Newtonsoft.Json.JsonProperty]
        private int tick;
        [Newtonsoft.Json.JsonProperty]
        private int laneOffset;

        public int Tick
        {
            get => tick;
            set => tick = value;
        }
        public int LaneOffset
        {
            get => laneOffset;
            set => laneOffset = value;
        }
    }

    /// <summary>
    /// 時間範囲を表します。
    /// </summary>
    [Newtonsoft.Json.JsonObject(Newtonsoft.Json.MemberSerialization.OptIn)]
    public class TickRange
    {
        [Newtonsoft.Json.JsonProperty]
        private int startTick;
        [Newtonsoft.Json.JsonProperty]
        private int duration;

        public int StartTick
        {
            get => startTick;
            set => startTick = value;
        }
        public int Duration
        {
            get => duration;
            set => duration = value;
        }
        public int EndTick => StartTick + Duration;
    }
}
