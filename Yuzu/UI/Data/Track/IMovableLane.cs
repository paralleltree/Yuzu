using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Yuzu.Collections;
using Yuzu.Core.Track;

namespace Yuzu.UI.Data.Track
{
    internal interface IMovableLane
    {
        AVLTree<FieldPoint> Points { get; set; }
    }
}
