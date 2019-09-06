using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yuzu.Core.Track
{
    /// <summary>
    /// フィールド境界に沿ったレーンを表します。
    /// </summary>
    [Newtonsoft.Json.JsonObject(Newtonsoft.Json.MemberSerialization.OptIn)]
    public class SideLane : INoteContainer
    {
        [Newtonsoft.Json.JsonProperty]
        private TickRange validRange = new TickRange();
        [Newtonsoft.Json.JsonProperty]
        private List<Note> notes = new List<Note>();

        /// <summary>
        /// ガイドラインの範囲を設定します。
        /// </summary>
        public TickRange ValidRange
        {
            get => validRange;
            set => validRange = value;
        }
        public List<Note> Notes
        {
            get => notes;
            set => notes = value;
        }
    }
}
