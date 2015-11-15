using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Handlers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BeyondVerbal.Scarlet.WebApi.Client.Models.BeyondVerbal.Scarlet.Api.Test.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BeyondVerbal.Scarlet.WebApi.Client
{
    public class SessionClient
    {
        private Options options;
        private string AnalysisUrl;
        private string UpstreamUrl;
        private string RecordingId;
        private Action<SessionEvent> eventCallback;
        public SessionClient(Options options, Action<SessionEvent> eventCallback)
        {
            this.options = options;
            this.eventCallback = eventCallback;
        }

        private Uri serviceUri
        {
            get
            {
                return new Uri(new Uri(options.ServerUrl), "v1/recording/");
                //return new UriBuilder(options.UseSsl ? "https" : "http", options.Host,options.UseSsl? 443:80, "v1/recording/").Uri;
            }
        }

        JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings() { Formatting = Formatting.Indented };

        public async Task<bool> Start()
        {
            using (var client = new HttpClient() { BaseAddress = serviceUri })
            {
                
                string url = string.Format("start?api_key={0}", options.ApiKey);

                var responce = await client.PostAsJsonAsync(url, new SessionParameters()
                {
                    data_format = DataFormatCreator.Create(options),//new DataFormat() { type="PCM", channels = 1, bits_per_sample = 16, sample_rate = 8000 },
                    recorder_info = new RecorderInfo()
                    {
                        //activity = "Sport",
                        device_id = "00000000000",
                        //device_info = "LGE Nexus 5",
                        email = "some@email.com",
                        coordinates = new Coordinates() { Long = 34.9002785, Lat = 32.131147 }
                    },
                    requiredAnalysisTypes = options.RequiredAnalysisTypes.Split(',')//new[] { "CompositMood", "ComposureMeter", "MoodGroup", "CooperationLevel", "MoodGroupSummary", "ServiceScore", "TemperMeter", "TemperValue" }
                });

                var content = await responce.Content.ReadAsStringAsync();
                if (responce.IsSuccessStatusCode)
                {
                    dynamic responceContent = JsonConvert.DeserializeObject(content, jsonSerializerSettings); 
                    dynamic followupActions = responceContent.followupActions;
                    AnalysisUrl = followupActions.analysis;
                    UpstreamUrl = followupActions.upStream;

                    RecordingId = new Uri(UpstreamUrl).Segments[3];//it is located at 3rd segment of url :-(
                    RaiseSessionEvent(SessionEventType.Started, new { });

                    return true;
                }
                else
                {
                    RaiseSessionEvent(SessionEventType.Error, responce.ToString() + "\r\n" + content);
                    throw new Exception(responce.ToString() + "\r\n" + content);
                }
            }
        }

        private void Always(Action call)
        {
            try { call(); }
            catch { }
        }

        public async Task Analyze(Action<long, long?, int> progressCallback)
        {
            //this cts signals that pooling need to be terminated
            CancellationTokenSource cts = new CancellationTokenSource();
            //results polling task
            var pollingTask = Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromMilliseconds(options.ResultFirstDelayMsec));
                while (!cts.IsCancellationRequested)
                {
                    try
                    {
                        await ReadAnalysisAsync(cts.Token);
                        await Task.Delay(TimeSpan.FromMilliseconds(options.ResultPollingPeriodMsec), cts.Token);
                    }
                    catch (OperationCanceledException ox)
                    {
                        Debug.WriteLine(ox.Message);
                    }
                }
            });

            //up-streaming block
            using (Stream stream = new ReadStreamWithRateControl(File.OpenRead(options.SoundFile), options.BytePerSecRate))
            {
                //progress callback
                var processMessageHander = new ProgressMessageHandler(new HttpClientHandler());
                processMessageHander.HttpSendProgress += (_, args) =>
                    Always(() => progressCallback(args.BytesTransferred, args.TotalBytes, args.ProgressPercentage));

                
                using (var client = new HttpClient(processMessageHander))
                {
                    client.Timeout = TimeSpan.FromMilliseconds(Timeout.Infinite);
                    client.DefaultRequestHeaders.TransferEncodingChunked = true;
                    try
                    {
                        var responce = await client.PostAsync(UpstreamUrl, new StreamContent(stream));//upstream post

                        var content = await responce.Content.ReadAsStringAsync();//wait for result
                        cts.CancelAfter(TimeSpan.FromSeconds(1));//signal cancel to polling task
                        if (responce.IsSuccessStatusCode)
                        {
                            dynamic responceContent = JsonConvert.DeserializeObject(content, jsonSerializerSettings);
                            dynamic result = responceContent.result;
                            RaiseSessionEvent(SessionEventType.Complete, result);
                        }
                        else
                        {
                            RaiseSessionEvent(SessionEventType.Error, responce.ToString() + "\r\n" + content);
                            throw new Exception(responce.ToString() + "\r\n" + content);
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message);
                        throw;
                    }
                }
            }


            await pollingTask;


        }

        private void RaiseSessionEvent(SessionEventType eventType, dynamic data)
        {
            Always(() => eventCallback(new SessionEvent()
            {
                EventType = eventType,
                RecordingId = RecordingId,
                TimeStamp = DateTimeOffset.Now,
                Data = data
            }));
        }

        private async Task ReadAnalysisAsync(CancellationToken cancelationToken)
        {
            using (var client = new HttpClient())
            {
                var responce = await client.GetAsync(AnalysisUrl, cancelationToken);
                var content = await responce.Content.ReadAsStringAsync();
                if (responce.IsSuccessStatusCode)
                {
                    dynamic responceContent = JsonConvert.DeserializeObject(content);
                    dynamic followupActions = responceContent.followupActions;
                    AnalysisUrl = followupActions.analysis;
                    dynamic result = responceContent.result;

                    RaiseSessionEvent(result.analysisSegments == null ? SessionEventType.Progress : SessionEventType.Analysis, result);
                }
                else
                {
                    RaiseSessionEvent(SessionEventType.Error, responce.ToString() + "\r\n" + content);
                    throw new Exception(responce.ToString() + "\r\n" + content);
                }

            }
        }


    }
}
