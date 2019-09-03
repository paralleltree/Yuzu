using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Yuzu.Collections;
using Yuzu.Core.Track;

namespace Yuzu.UI.Data.Track
{
    internal class SideLane : INoteContainer
    {
        public TickRange ValidRange { get; set; }
        public AVLTree<Note> Notes { get; set; }

        public SideLane()
        {
            Notes = new AVLTree<Note>(p => p.TickRange.StartTick);
        }

        public SideLane Convert(Core.Track.SideLane lane)
        {
            ValidRange = lane.ValidRange;
            Notes = new AVLTree<Note>(p => p.TickRange.StartTick, lane.Notes);
            return this;
        }

        public Core.Track.SideLane ConvertBack()
        {
            return new Core.Track.SideLane()
            {
                ValidRange = ValidRange,
                Notes = Notes.ToList()
            };
        }
    }
}
