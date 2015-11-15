using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BeyondVerbal.Scarlet.WebApi.Client
{

    public enum SessionEventType { Started,Progress,Analysis,Complete, Error}
    public class SessionEvent
    {
        public string RecordingId { get; set; }
        public DateTimeOffset TimeStamp { get; set; }
        public dynamic Data { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public SessionEventType EventType { get; set; }

        
    }



}
