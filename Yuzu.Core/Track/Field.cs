using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yuzu.Core.Track
{
    public abstract class FieldSidePair<T>
    {
        public T Left { get; set; }
        public T Right { get; set; }
    }
}
