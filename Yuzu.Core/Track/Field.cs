using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yuzu.Core.Track
{
    [Newtonsoft.Json.JsonObject(Newtonsoft.Json.MemberSerialization.OptIn)]
    public abstract class FieldSidePair<T>
    {
        [Newtonsoft.Json.JsonProperty]
        private T left;
        [Newtonsoft.Json.JsonProperty]
        private T right;

        public T Left
        {
            get => left;
            set => left = value;
        }
        public T Right
        {
            get => right;
            set => right = value;
        }
    }

    public class Field : FieldSidePair<FieldSide>
    {
        public Field()
        {
            Left = new FieldSide();
            Right = new FieldSide();
        }
    }

    /// <summary>
    /// フィールドの一端を表します。
    /// </summary>
    [Newtonsoft.Json.JsonObject(Newtonsoft.Json.MemberSerialization.OptIn)]
    public class FieldSide
    {
        [Newtonsoft.Json.JsonProperty]
        private FieldWall fieldWall = new FieldWall();
        [Newtonsoft.Json.JsonProperty]
        private List<SideLane> sideLanes = new List<SideLane>();

        public FieldWall FieldWall
        {
            get => fieldWall;
            set => fieldWall = value;
        }
        public List<SideLane> SideLanes
        {
            get => sideLanes;
            set => sideLanes = value;
        }
    }
}
