using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yuzu.Core.Events
{
    /// <summary>
    /// BPMの変更を表します。
    /// </summary>
    [Newtonsoft.Json.JsonObject(Newtonsoft.Json.MemberSerialization.OptIn)]
    public class BPMChangeEvent : EventBase
    {
        [Newtonsoft.Json.JsonProperty]
        private decimal bpm;

        public decimal BPM
        {
            get => bpm;
            set => bpm = value;
        }
    }
}
