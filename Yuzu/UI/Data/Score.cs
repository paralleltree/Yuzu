using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Yuzu.UI.Data.Track;

namespace Yuzu.UI.Data
{
    internal class Score
    {
        public int TicksPerBeat { get; set; } = 480;

        public int HorizontalResolution { get; set; } = 20;

        public Core.EventCollection Events { get; set; }

        public Field Field { get; set; }
        public List<SurfaceLane> SurfaceLanes { get; set; }

        public List<Core.Track.Flick> Flicks { get; set; }
        public List<Core.Track.Bell> Bells { get; set; }
        public List<Core.Track.Bullet> Bullets { get; set; }

        public Score()
        {
            Events = new Core.EventCollection();
            Field = new Field();
            SurfaceLanes = new List<SurfaceLane>();
            Flicks = new List<Core.Track.Flick>();
            Bells = new List<Core.Track.Bell>();
            Bullets = new List<Core.Track.Bullet>();
        }

        public Score Convert(Core.Score score)
        {
            TicksPerBeat = score.TicksPerBeat;
            HorizontalResolution = score.HalfHorizontalResolution;
            Events = score.Events;
            Field = Field.Convert(score.Field);
            SurfaceLanes = score.SurfaceLanes.Select(p => new SurfaceLane().Convert(p)).ToList();
            Flicks = score.Flicks;
            Bells = score.Bells;
            Bullets = score.Bullets;
            return this;
        }

        public Core.Score ConvertBack()
        {
            return new Core.Score()
            {
                TicksPerBeat = TicksPerBeat,
                HalfHorizontalResolution = HorizontalResolution,
                Events = Events,
                Field = Field.ConvertBack(),
                SurfaceLanes = SurfaceLanes.Select(p => p.ConvertBack()).ToList(),
                Flicks = Flicks,
                Bells = Bells,
                Bullets = Bullets
            };
        }
    }
}
