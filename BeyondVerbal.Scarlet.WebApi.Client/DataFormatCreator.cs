using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeyondVerbal.Scarlet.WebApi.Client.Models.BeyondVerbal.Scarlet.Api.Test.Models;

namespace BeyondVerbal.Scarlet.WebApi.Client
{
    static class DataFormatCreator
    {
        public static DataFormat Create( Options options)
        {
            if (options.SoundFile.EndsWith(".wav"))
                return new DataFormat()
                {
                    type = "WAV",
                    auto_detect = true,
                    channels = 1,

                };
            else
                return new DataFormat()
                {
                    type = "PCM",
                    bits_per_sample = options.BitsPerSample,
                    sample_rate = options.SamplingRate,
                    channels = 1,
                    auto_detect = false
                };
        }
    }
}
