using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yuzu.Core.Track
{
    /// <summary>
    /// フィールド上に存在するオブジェクトを表します。
    /// </summary>
    [Newtonsoft.Json.JsonObject(Newtonsoft.Json.MemberSerialization.OptIn)]
    public abstract class FieldObject
    {
        [Newtonsoft.Json.JsonProperty]
        private FieldPoint position = new FieldPoint();

        /// <summary>
        /// このオブジェクトの配置位置を設定します。
        /// </summary>
        public FieldPoint Position
        {
            get => position;
            set => position = value;
        }
    }

    public class Bell : FieldObject
    {
    }

    public class Bullet : FieldObject
    {
    }

    [Newtonsoft.Json.JsonObject(Newtonsoft.Json.MemberSerialization.OptIn)]
    public class Flick : FieldObject
    {
        [Newtonsoft.Json.JsonProperty]
        private HorizontalDirection direction;
        [Newtonsoft.Json.JsonProperty]
        private bool isCritical;

        public HorizontalDirection Direction
        {
            get => direction;
            set => direction = value;
        }
        public bool IsCritical
        {
            get => isCritical;
            set => isCritical = value;
        }
    }

    public enum HorizontalDirection
    {
        Left,
        Right
    }
}
