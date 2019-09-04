using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Yuzu.Core.Track;

namespace Yuzu.Core
{
    /// <summary>
    /// 譜面データを表します。
    /// </summary>
    [Newtonsoft.Json.JsonObject(Newtonsoft.Json.MemberSerialization.OptIn)]
    public class Score
    {
        [Newtonsoft.Json.JsonProperty]
        private int ticksPerBeat = 480;
        [Newtonsoft.Json.JsonProperty]
        private int halfHorizontalResolution = 20;
        [Newtonsoft.Json.JsonProperty]
        private EventCollection events = new EventCollection();
        [Newtonsoft.Json.JsonProperty]
        private Field field = new Field();
        [Newtonsoft.Json.JsonProperty]
        private List<SurfaceLane> surfaceLanes = new List<SurfaceLane>();
        [Newtonsoft.Json.JsonProperty]
        private List<Flick> flicks = new List<Flick>();
        [Newtonsoft.Json.JsonProperty]
        private List<Bell> bells = new List<Bell>();
        [Newtonsoft.Json.JsonProperty]
        private List<Bullet> bullets = new List<Bullet>();

        /// <summary>
        /// 1拍あたりの分解能を設定します。
        /// </summary>
        public int TicksPerBeat
        {
            get => ticksPerBeat;
            set => ticksPerBeat = value;
        }

        /// <summary>
        /// レーンの半分における横方向の分解能を設定します。
        /// </summary>
        public int HalfHorizontalResolution
        {
            get => halfHorizontalResolution;
            set => halfHorizontalResolution = value;
        }

        public EventCollection Events
        {
            get => events;
            set => events = value;
        }

        public Field Field
        {
            get => field;
            set => field = value;
        }
        public List<SurfaceLane> SurfaceLanes
        {
            get => surfaceLanes;
            set => surfaceLanes = value;
        }

        public List<Flick> Flicks
        {
            get => flicks;
            set => flicks = value;
        }
        public List<Bell> Bells
        {
            get => bells;
            set => bells = value;
        }
        public List<Bullet> Bullets
        {
            get => bullets;
            set => bullets = value;
        }


        public int GetLastTick()
        {
            return new int[]
            {
                Field.Left.FieldWall.Points.Max(p => p.Tick),
                Field.Right.FieldWall.Points.Max(p => p.Tick),
                SurfaceLanes.Count > 0 ? SurfaceLanes.Max(p => p.Points.Max(q=>q.Tick)) : 0,
                Flicks.Count > 0 ? Flicks.Max(p => p.Position.Tick) : 0,
                Bells.Count > 0 ? Bells.Max(p => p.Position.Tick) : 0 ,
                Bullets.Count > 0 ? Bullets.Max(p => p.Position.Tick) : 0
            }.Max();
        }

        public Score Clone()
        {
            var serialized = Newtonsoft.Json.JsonConvert.SerializeObject(this, ScoreBook.SerializerSettings);
            var score = Newtonsoft.Json.JsonConvert.DeserializeObject<Score>(serialized);
            return score;
        }
    }
}
