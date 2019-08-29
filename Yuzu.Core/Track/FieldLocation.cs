using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yuzu.Core.Track
{
    public class FieldPoint
    {
        public int Tick { get; set; }
        public int LaneOffset { get; set; }
    }

    public class TickRange
    {
        public int StartTick { get; set; }
        public int Duration { get; set; }
        public int EndTick => StartTick + Duration;
    }
}
