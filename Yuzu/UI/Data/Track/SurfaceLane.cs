using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Yuzu.Collections;
using Yuzu.Core.Track;

namespace Yuzu.UI.Data.Track
{
    internal class SurfaceLane : IMovableLane, INoteContainer
    {
        public SurfaceLaneColor LaneColor { get; set; }
        public AVLTree<FieldPoint> Points { get; set; }
        public AVLTree<Note> Notes { get; set; }

        public SurfaceLane()
        {
            Points = new AVLTree<FieldPoint>(p => p.Tick);
            Notes = new AVLTree<Note>(p => p.TickRange.StartTick);
        }

        public SurfaceLane Convert(Core.Track.SurfaceLane lane)
        {
            LaneColor = lane.LaneColor;
            Points = new AVLTree<FieldPoint>(p => p.Tick, lane.Points);
            Notes = new AVLTree<Note>(p => p.TickRange.StartTick, lane.Notes);
            return this;
        }

        public Core.Track.SurfaceLane ConvertBack()
        {
            return new Core.Track.SurfaceLane()
            {
                LaneColor = LaneColor,
                Points = Points.ToList(),
                Notes = Notes.ToList()
            };
        }
    }
}
