using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeyondVerbal.Scarlet.WebApi.Client.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    namespace BeyondVerbal.Scarlet.Api.Test.Models
    {
        public class DataFormat
        {
            public string type { get; set; }
            public int channels { get; set; }
            public int sample_rate { get; set; }
            public int bits_per_sample { get; set; }
            public bool auto_detect { get; set; }
        }

        public class Coordinates
        {
            public double Long { get; set; }
            public double Lat { get; set; }
        }

        public class RecorderInfo
        {
            public string IP { get; set; }
            public string activity { get; set; }
            public Coordinates coordinates { get; set; }
            public string device_id { get; set; }
            public string device_info { get; set; }
            public string email { get; set; }
            public string gender { get; set; }
            public string phone { get; set; }
            public string facebook_id { get; set; }
            public string twitter_id { get; set; }
        }

        public class SessionParameters
        {
            public DataFormat data_format { get; set; }
            public RecorderInfo recorder_info { get; set; }
            [Obsolete]
            public IEnumerable<string> requiredAnalysisTypes { get; set; }
        }
    }

}
