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

        public Field Convert(Core.Track.Field field)
        {
            Left = new FieldSide().Convert(field.Left);
            Right = new FieldSide().Convert(field.Right);
            return this;
        }

        public Core.Track.Field ConvertBack()
        {
            return new Core.Track.Field()
            {
                Left = Left.ConvertBack(),
                Right = Right.ConvertBack()
            };
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

        public FieldSide Convert(Core.Track.FieldSide fs)
        {
            FieldWall = new FieldWall();
            FieldWall.Convert(fs.FieldWall);
            SideLanes = new AVLTree<SideLane>(p => p.ValidRange.StartTick, fs.SideLanes.Select(p => new SideLane().Convert(p)));
            return this;
        }

        public Core.Track.FieldSide ConvertBack()
        {
            return new Core.Track.FieldSide()
            {
                FieldWall = FieldWall.ConvertBack(),
                SideLanes = SideLanes.Select(p => p.ConvertBack()).ToList()
            };
        }
    }
}
