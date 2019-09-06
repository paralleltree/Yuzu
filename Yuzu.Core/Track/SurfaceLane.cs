using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yuzu.Core.Track
{
    [Newtonsoft.Json.JsonObject(Newtonsoft.Json.MemberSerialization.OptIn)]
    public class SurfaceLane : IMovableLane, INoteContainer
    {
        [Newtonsoft.Json.JsonProperty]
        private SurfaceLaneColor laneColor;
        [Newtonsoft.Json.JsonProperty]
        private List<FieldPoint> points = new List<FieldPoint>();
        [Newtonsoft.Json.JsonProperty]
        private List<Note> notes = new List<Note>();

        public SurfaceLaneColor LaneColor
        {
            get => laneColor;
            set => laneColor = value;
        }
        public List<FieldPoint> Points
        {
            get => points;
            set => points = value;
        }
        public List<Note> Notes
        {
            get => notes;
            set => notes = value;
        }
    }

    public enum SurfaceLaneColor
    {
        Red,
        Green,
        Blue
    }
}
