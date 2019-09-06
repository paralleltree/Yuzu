using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yuzu.Core.Events
{
    [Newtonsoft.Json.JsonObject(Newtonsoft.Json.MemberSerialization.OptIn)]
    public class EventBase
    {
        [Newtonsoft.Json.JsonProperty]
        private int tick;

        public int Tick
        {
            get => tick;
            set => tick = value;
        }
    }
}
