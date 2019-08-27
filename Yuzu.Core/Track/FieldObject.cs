using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yuzu.Core.Track
{
    public abstract class FieldObject
    {
        public FieldPoint Position { get; set; }
    }

    public class Bell : FieldObject
    {
    }

    public class Bullet : FieldObject
    {
    }

    public class Flick : FieldObject
    {
        public HorizontalDirection Direction { get; set; }
        public bool IsCritical { get; set; }
    }

    public enum HorizontalDirection
    {
        Left,
        Right
    }
}
