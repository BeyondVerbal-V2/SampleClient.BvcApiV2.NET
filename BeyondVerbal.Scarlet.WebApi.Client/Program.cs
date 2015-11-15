using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using Newtonsoft.Json;

namespace BeyondVerbal.Scarlet.WebApi.Client
{
    public class Program
    {
        static int Main(string[] args)
        {
            //var options = new Options()
            //{
            //    ApiKey = "a867cc12d5c24657ac4b7f2263c42f9c",
            //    ServerUrl = "http://localhost:3388",
            //    SoundFile = @"C:\Users\pavel.smirnov\Documents\Beyond\beyondverbal\ScarletTesterByHike\Debug\Sample.wav",
            //    BytePerSecRate = 16000
            //};

            var result = Parser.Default.ParseArguments<Options>(args);
            var options = result.Value;
            if (result.Errors.Count() > 0)
            {
                options = (Options)ConfigurationManager.GetSection("clientOptions");
                if (options == null)
                {
                    Console.WriteLine("Nor arguments neither config section (clientOptions) were not found");
                    return 1;
                }
            }


            Test(options).Wait();

            return 0;
        }

        static Action<SessionEvent> noop = (_) => { };



        static Action<SessionEvent> ConsoleDump(Options options)
        {
            Func<Action<SessionEvent>> objectFunc = () =>
            {
                SessionEventType? recentEvtType = null;
                return (evt) =>
                {
                    //nice print
                    if (evt.EventType != SessionEventType.Progress && recentEvtType == SessionEventType.Progress)
                        Console.WriteLine();
                    recentEvtType = evt.EventType;


                    switch (evt.EventType)
                    {
                        case SessionEventType.Analysis:
                            Console.WriteLine(evt.Data);
                            break;
                        case SessionEventType.Progress:
                            Console.Write(".");
                            break;
                        case SessionEventType.Started:
                            Console.WriteLine("Started");
                            break;

                        case SessionEventType.Complete:
                            Console.WriteLine(evt.Data);
                            break;
                        case SessionEventType.Error:
                            Console.WriteLine(evt.Data);
                            break;
                    }
                };
            };
            return objectFunc();
        }

        public static async Task<IEnumerable<SessionEvent>> Test(Options options)
        {
            ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
            ServicePointManager.DefaultConnectionLimit = 10000;
            List<SessionEvent> collector = new List<SessionEvent>();

            Action<SessionEvent> consoleDump = ConsoleDump(options);



            try
            {
                var client = new SessionClient(options, (evt) =>
                {
                    if (options.ConsoleOff == false)
                        consoleDump(evt);
                    //file dump
                    collector.Add(evt);

                });

                var ok = await client.Start();
                await client.Analyze(
                    progressCallback: (transferred, total, progress) =>
                        {
                            //Console.WriteLine("Sent: " + transferred + " bytes");
                        });

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                if (options.OutputFile != null)
                {
                    try
                    {
                        DumpResultToJsonFile(options, collector);
                    }
                    catch { }
                }
            }
            return collector;
        }


        private static void DumpResultToJsonFile(Options options, IList<SessionEvent> events)
        {
            string fileName = options.FilenameIsTemplate ? string.Format(options.OutputFile, events.First().RecordingId) : options.OutputFile;

            using (FileStream fs = File.Open(fileName, FileMode.Create))
            using (StreamWriter sw = new StreamWriter(fs))
            using (JsonWriter jw = new JsonTextWriter(sw))
            {
                jw.Formatting = Formatting.Indented;
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(jw, events);
            }
        }
    }
}
