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
            var SignalRClienteCategory = "SignalRCliente";
            var signalRCliente = new SignalR.Client.SignalRClient<object>("http://gppdev:4478/signalr", "EventHub", "OnEvent");
            signalRCliente.RegisterChannel("MessageIntegration");
            signalRCliente.RegisterChannel("SourceIntegration");
            signalRCliente.OnStart += (s, e) => Debug.WriteLine(e, SignalRClienteCategory);
            signalRCliente.OnStop += (s, e) => Debug.WriteLine(e, SignalRClienteCategory);
            signalRCliente.OnError += (s, e) => Debug.WriteLine(e, SignalRClienteCategory);
            signalRCliente.OnConnectionSlow += (s, e) => Debug.WriteLine(e, SignalRClienteCategory);
            signalRCliente.OnEventReceived += (s, e) => Debug.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(e.Data), SignalRClienteCategory);

            signalRCliente.Start();
            System.Threading.Thread.Sleep(100000);
            signalRCliente.Stop();
        }
    }
}
