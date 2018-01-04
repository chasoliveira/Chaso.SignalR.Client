using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chaso.SignalR.Client;

namespace Chaso.SignalR.Client.Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            var url = "http://gppdev:4478/signalr";
            var hubName = "EventHub";
            var eventName = "OnEvent";
            var signalRClient = new SignalRClient<object>(url, hubName, eventName);
            signalRClient.RegisterChannel("SourceIntegration");
            signalRClient.RegisterChannel("MessageIntegration");
            signalRClient.RegisterChannel("Movimento");
            signalRClient.RegisterChannel("Notificacao");
            signalRClient.OnEventReceived += SignalRClient_OnEventReceived;
            signalRClient.OnConnectionSlow += SignalRClient_OnConnectionSlow;
            signalRClient.OnStart += SignalRClient_OnStart;
            signalRClient.OnStop += SignalRClient_OnStop;
            signalRClient.OnError += SignalRClient_OnError;

            signalRClient.Start();

            Console.ReadKey();
        }

        private static void SignalRClient_OnError(object sender, string e)
        {
            Console.WriteLine($"{e}");
        }

        private static void SignalRClient_OnStop(object sender, string e)
        {
            Console.WriteLine($"{e}");
        }

        private static void SignalRClient_OnStart(object sender, string e)
        {
            Console.WriteLine($"{e}");
        }

        private static void SignalRClient_OnConnectionSlow(object sender, string e)
        {
            Console.WriteLine($"{e}");
        }

        private static void SignalRClient_OnEventReceived(object sender, ChannelEvent<object> e)
        {
            Console.WriteLine($"Channel: {e.ChannelName}, Data: {Newtonsoft.Json.JsonConvert.SerializeObject(e.Data)}");
        }
    }
}
