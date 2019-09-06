using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yuzu.Core.Events
{
    /// <summary>
    /// ハイスピードの変更を表します。
    /// </summary>
    [Newtonsoft.Json.JsonObject(Newtonsoft.Json.MemberSerialization.OptIn)]
    public class HighSpeedChangeEvent : EventBase
    {
        [Newtonsoft.Json.JsonProperty]
        private decimal speedRatio;

        /// <summary>
        /// 1を基準とする速度比を設定します。
        /// </summary>
        public decimal SpeedRatio
        {
            get => speedRatio;
            set => speedRatio = value;
        }
    }
}
