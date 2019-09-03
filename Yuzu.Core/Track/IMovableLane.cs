using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yuzu.Core.Track
{
    public interface IMovableLane
    {
        List<FieldPoint> Points { get; set; }
    }
}
