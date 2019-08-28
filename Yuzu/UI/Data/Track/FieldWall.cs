using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Yuzu.Collections;
using Yuzu.Core.Track;

namespace Yuzu.UI.Data.Track
{
    internal class FieldWall
    {
        public AVLTree<FieldPoint> Points { get; set; }
        public AVLTree<TickRange> GuardedSections { get; set; }

        public FieldWall()
        {
            Points = new AVLTree<FieldPoint>(p => p.Tick);
            GuardedSections = new AVLTree<TickRange>(p => p.StartTick);
        }
    }
}
