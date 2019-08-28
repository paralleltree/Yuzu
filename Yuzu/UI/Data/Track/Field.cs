using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Yuzu.Collections;
using Yuzu.Core.Track;

namespace Yuzu.UI.Data.Track
{
    internal class Field : FieldSidePair<FieldSide>
    {
        public Field()
        {
            Left = new FieldSide();
            Right = new FieldSide();
        }
    }

    internal class FieldSide
    {
        public FieldWall FieldWall { get; set; }
        public AVLTree<SideLane> SideLanes { get; set; }

        public FieldSide()
        {
            FieldWall = new FieldWall();
            SideLanes = new AVLTree<SideLane>(p => p.ValidRange.StartTick);
        }
    }
}
