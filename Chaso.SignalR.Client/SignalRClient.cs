using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Chaso.SignalR.Client
{
    public class SignalRClient<TResult> : ISignalRClient<TResult> where TResult : class
    {
        const string invokeSubscribeMethod = "Subscribe";
        const string invokeUnsubscribeMethod = "Unsubscribe";

        public event EventHandler<string> OnStart;
        public event EventHandler<string> OnStop;
        public event EventHandler<string> OnError;
        public event EventHandler<string> OnConnectionSlow;

        public event EventHandler<ChannelEvent<TResult>> OnEventReceived;

        HubConnection hubConnection;
        IHubProxy eventHubProxy;

        private string hubName;
        private string eventName;
        private string hubUrl;

        private IList<string> RegisteredChannels;

        public SignalRClient(string url, string hubName, string eventName)
        {
            this.hubName = hubName;
            this.eventName = eventName;
            this.hubUrl = url;
            RegisteredChannels = new List<string>();
            Initialize();
        }

        private void Initialize()
        {
            try
            {
                hubConnection = new HubConnection(this.hubUrl);
                eventHubProxy = hubConnection.CreateHubProxy(this.hubName);
                eventHubProxy.On<string, ChannelEvent<TResult>>(this.eventName, OnEvent);
                hubConnection.EnsureReconnecting();
                hubConnection.ConnectionSlow += HubConnection_ConnectionSlow;
                hubConnection.Error += HubConnection_Error;
                hubConnection.Start().Wait();
            }
            catch (Exception ex)
            {
                this.HubConnection_Error(ex);
            }
        }

        private void HubConnection_ConnectionSlow()
        {
            this.OnError?.Invoke(this, $"The Connection {this.hubConnection.ConnectionId}, Last Error is: {this.hubConnection.LastError?.ToStringAllMessage()}");
        }

        private void HubConnection_Error(Exception ex)
        {
            this.OnError?.Invoke(this, ex.ToStringAllMessage());
        }

        private void OnEvent(string channel, ChannelEvent<TResult> ev)
        {
            if (!ev.Name.Contains(".subscribed"))
                this.OnEventReceived?.Invoke(this, ev);
        }

        public async void RegisterChannel(string hubInvokeChannel)
        {
            if (this.RegisteredChannels.Contains(hubInvokeChannel))
                return;

            RegisteredChannels.Add(hubInvokeChannel);
            if (RegisteredChannels.Any())
                await eventHubProxy.Invoke(invokeSubscribeMethod, hubInvokeChannel);
        }
        public async void UnregisterChannel(string channel)
        {
            if (this.RegisteredChannels.Contains(channel))
                RegisteredChannels.Remove(channel);
            await eventHubProxy.Invoke(invokeSubscribeMethod, channel);
        }
        public async void ClearChannels()
        {
            await UnregisterChannels();
            RegisteredChannels.Clear();
        }
        private async System.Threading.Tasks.Task RegisterChannels()
        {
            foreach (var channel in RegisteredChannels)
                await eventHubProxy.Invoke(invokeSubscribeMethod, channel);
        }

        private async System.Threading.Tasks.Task UnregisterChannels()
        {
            foreach (var channel in RegisteredChannels)
                await eventHubProxy.Invoke(invokeUnsubscribeMethod, channel);
        }

        public async void Start()
        {
            await hubConnection.Start();
            await RegisterChannels();
            this.OnStart?.Invoke(this, $"SignalRPush - The hub {hubName} was connected.");
        }
        public async void Stop()
        {
            await UnregisterChannels();
            hubConnection.Stop();
            this.OnStop?.Invoke(this, $"SignalRPush - The hub { hubName} was disconnected");
        }
    }

    public interface ISignalRClient<T> where T : class
    {
        event EventHandler<string> OnStart;
        event EventHandler<string> OnStop;
        event EventHandler<string> OnError;
        event EventHandler<string> OnConnectionSlow;
        event EventHandler<ChannelEvent<T>> OnEventReceived;
        void RegisterChannel(string channel);
        void UnregisterChannel(string channel);
        void ClearChannels();
        void Start();
        void Stop();
    }
}
