using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Yuzu.Collections;
using Yuzu.Core.Track;

namespace Yuzu.UI.Data.Track
{
    public class SurfaceLane : IMovableLane, INoteContainer
    {
        public SurfaceLaneColor LaneColor { get; set; }
        public AVLTree<FieldPoint> Points { get; set; }
        public AVLTree<Note> Notes { get; set; }

        public SurfaceLane()
        {
            Points = new AVLTree<FieldPoint>(p => p.Tick);
            Notes = new AVLTree<Note>(p => p.TickRange.StartTick);
        }
    }
}
