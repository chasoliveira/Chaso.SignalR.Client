using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

namespace Chaso.SignalR.Client.Test
{
    public class ReturnChannel
    {
        public string Code { get; set; }
    }
    [TestClass]
    public class SignalRClientTest
    {
        [TestMethod]
        public void MustListenSomeEventOnHub()
        {
            var signalRCliente = new SignalR.Client.SignalRClient<ReturnChannel>("http://localhost:4478/signalr", "EventHub", "OnEvent");
            signalRCliente.RegisterChannel("MessageIntegration");
            signalRCliente.OnStart += (s, e) => Debug.WriteLine(e);
            signalRCliente.OnStop += (s, e) => Debug.WriteLine(e);
            signalRCliente.OnEventReceived += (s, e) => Debug.WriteLine(e.Data.Code);

            Console.ReadKey();
            
        }
    }
}
