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
    }
}
