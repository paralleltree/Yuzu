using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Yuzu.Core.Events;

namespace Yuzu.Core
{
    [Newtonsoft.Json.JsonObject(Newtonsoft.Json.MemberSerialization.OptIn)]
    public class EventCollection
    {
        [Newtonsoft.Json.JsonProperty]
        private List<BPMChangeEvent> bpmChangeEvents = new List<BPMChangeEvent>();
        [Newtonsoft.Json.JsonProperty]
        private List<TimeSignatureChangeEvent> timeSignatureChangeEvents = new List<TimeSignatureChangeEvent>();
        [Newtonsoft.Json.JsonProperty]
        private List<HighSpeedChangeEvent> highSpeedChangeEvents = new List<HighSpeedChangeEvent>();

        public List<BPMChangeEvent> BPMChangeEvents
        {
            get => bpmChangeEvents;
            set => bpmChangeEvents = value;
        }

        public List<TimeSignatureChangeEvent> TimeSignatureChangeEvents
        {
            get => timeSignatureChangeEvents;
            set => timeSignatureChangeEvents = value;
        }

        public List<HighSpeedChangeEvent> HighSpeedChangeEvents
        {
            get => highSpeedChangeEvents;
            set => highSpeedChangeEvents = value;
        }
    }
}
