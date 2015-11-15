using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace BeyondVerbal.Scarlet.WebApi.Client
{
    public class Options : ConfigurationSection
    {
        [Option('a', "apiKey", Required = true, HelpText = "Api Key")]
        [ConfigurationProperty("apiKey", IsRequired = true)]
        public string ApiKey { get { return (string)this["apiKey"]; } set { this["apiKey"] = value; } }


        [Option('s', "server-url", Required = true, HelpText = "Server Url without path, e.g http://localhost:3388")]
        [ConfigurationProperty("serverUrl", IsRequired = true)]
        public string ServerUrl { get { return (string)this["serverUrl"]; } set { this["serverUrl"] = value; } }



        [Option('i', "input", Required = true, HelpText = "Path to local sound file, WAV or PCM")]
        [ConfigurationProperty("soundFile", IsRequired = true)]
        public string SoundFile { get { return (string)this["soundFile"]; } set { this["soundFile"] = value; } }



        [Option('p', "sampling-rate", DefaultValue = 8000, Required = false, HelpText = "Sampling rate, is mandatory if the input is PCM")]
        [ConfigurationProperty("samplingRate", IsRequired = false, DefaultValue = 8000)]
        public int SamplingRate { get { return (int)this["samplingRate"]; } set { this["samplingRate"] = value; } }




        [Option('b', "bits-per-sample", DefaultValue = 16, Required = false, HelpText = "Bits per sample, is mandatory if the input is PCM")]
        [ConfigurationProperty("bitsPerSample", IsRequired = false, DefaultValue = 16)]
        public int BitsPerSample { get { return (int)this["bitsPerSample"]; } set { this["bitsPerSample"] = value; } }




        [Option('d', "send-rate", DefaultValue = 16000, Required = false, HelpText = "Data upload rate, if omitted or 0, rate-control is disabled and file is sent with network rate")]
        [ConfigurationProperty("bytePerSecRate", IsRequired = false, DefaultValue = 16000)]
        public int BytePerSecRate { get { return (int)this["bytePerSecRate"]; } set { this["bytePerSecRate"] = value; } }





        [Option('o', "out-file", Required = false, HelpText = "Output file, Optional")]
        [ConfigurationProperty("outputFile")]
        public string OutputFile { get { return (string)this["outputFile"]; } set { this["outputFile"] = value; } }



        [Option('t', "out-file-is-template", Required = false, DefaultValue = false, HelpText = "Output file is template of C# string.Format method. e.g. dump{0}.log where {0} will be replaced with recordingId ")]
        [ConfigurationProperty("filenameIsTemplate", IsRequired = false, DefaultValue = false)]
        public bool FilenameIsTemplate { get { return (bool)this["filenameIsTemplate"]; } set { this["filenameIsTemplate"] = value; } }




        [Option('r', "result-polling-period", Required = false, DefaultValue = 1000L, HelpText = "Poling period for result request in milliseconds")]
        [ConfigurationProperty("resultPollingPeriodMsec", IsRequired = false, DefaultValue = 1000L)]
        public long ResultPollingPeriodMsec { get { return (long)this["resultPollingPeriodMsec"]; } set { this["resultPollingPeriodMsec"] = value; } }




        [Option('f', "result-first-delay", Required = false, DefaultValue = 3000L, HelpText = "Delay before sending first request for result in milliseconds")]
        [ConfigurationProperty("resultFirstDelayMsec", IsRequired = false, DefaultValue = 3000L)]
        public long ResultFirstDelayMsec { get { return (long)this["resultFirstDelayMsec"]; } set { this["resultFirstDelayMsec"] = value; } }

        [Option('l', "required-analysis-types", Required = false, HelpText = "Reqired analysys types, nothing means all")]
        [ConfigurationProperty("requiredAnalysisTypes", IsRequired = false, DefaultValue = null)]
        public string RequiredAnalysisTypes { get { return (string)this["requiredAnalysisTypes"]; } set { this["requiredAnalysisTypes"] = value; } }

        [Option('c', "console-off", Required = false, HelpText = "Switches off console output")]
        [ConfigurationProperty("consoleOff", IsRequired = false, DefaultValue = false)]
        public bool ConsoleOff { get { return (bool)this["consoleOff"]; } set { this["consoleOff"] = value; } }
    }
}
