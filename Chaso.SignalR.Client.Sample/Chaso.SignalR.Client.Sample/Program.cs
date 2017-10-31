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
            var url = "http://localhost:4478/signalr";
            var hubName = "MyHub";
            var eventName = "OnEvent";
            SignalRClient<MyModel> signalRClient = new SignalRClient<MyModel>(url, hubName, eventName);
            signalRClient.RegisterChannel("MyChannel");
            signalRClient.OnEventReceived += SignalRClient_OnEventReceived;
        }

        private static void SignalRClient_OnEventReceived(object sender, ChannelEvent<MyModel> e)
        {
            //Do something...
            Console.Write("Channel: {0}, Data Key: {1}, Data Value: {2}",e.ChannelName, e.Data.Key, e.Data.Value);
        }
    }

    public class MyModel {
        public string Key { get; set; }
        public string Value { get; set; }
    }
}
