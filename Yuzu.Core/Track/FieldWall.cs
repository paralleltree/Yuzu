using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yuzu.Core.Track
{
    /// <summary>
    /// フィールドの境界を表します。
    /// </summary>
    [Newtonsoft.Json.JsonObject(Newtonsoft.Json.MemberSerialization.OptIn)]
    public class FieldWall : IMovableLane
    {
        [Newtonsoft.Json.JsonProperty]
        private List<FieldPoint> points = new List<FieldPoint>();
        [Newtonsoft.Json.JsonProperty]
        private List<TickRange> guardedSections = new List<TickRange>();

        public List<FieldPoint> Points
        {
            get => points;
            set => points = value;
        }
        public List<TickRange> GuardedSections
        {
            get => guardedSections;
            set => guardedSections = value;
        }
    }
}
