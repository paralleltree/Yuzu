﻿using System;
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

        public Field Field { get; set; }
        public List<SurfaceLane> SurfaceLanes { get; set; }

        public List<Core.Track.Flick> Flicks { get; set; }
        public List<Core.Track.Bell> Bells { get; set; }
        public List<Core.Track.Bullet> Bullets { get; set; }

        public Score()
        {
            Field = new Field();
            SurfaceLanes = new List<SurfaceLane>();
            Flicks = new List<Core.Track.Flick>();
            Bells = new List<Core.Track.Bell>();
            Bullets = new List<Core.Track.Bullet>();
        }
    }
}
