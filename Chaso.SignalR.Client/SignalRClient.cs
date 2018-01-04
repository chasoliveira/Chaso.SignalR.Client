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
        public event EventHandler<string> OnReconnecting;
        public event EventHandler<int> OnTryReconnectMax;
        public event EventHandler<ChannelEvent<TResult>> OnEventReceived;

        HubConnection hubConnection;
        IHubProxy eventHubProxy;

        private string hubName;
        private string eventName;
        private string hubUrl;
        private bool tryToReconnect = false;
        private int maxTryReconnect = 10;

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
                hubConnection.Reconnecting += HubConnection_Reconnecting;
                hubConnection.Closed += HubConnection_Closed;
                hubConnection.Reconnected += HubConnection_Reconnected;
                hubConnection.ConnectionSlow += HubConnection_ConnectionSlow;
                hubConnection.Error += HubConnection_Error;

                hubConnection.Start().Wait();
            }
            catch (Exception ex)
            {
                this.OnError?.Invoke(this, $"Erro on Initialize SignalRClient. {ex.ToStringAllMessage()}");
            }
        }

        public void SetMaxTryReconnect(int max) => this.maxTryReconnect = max;
        private void TryToReconnect(string origin)
        {
            int count = 0;
            while (tryToReconnect && hubConnection.State != ConnectionState.Connected)
            {
                System.Threading.Thread.Sleep(5000);
                hubConnection.Start().Wait();
                this.OnStart?.Invoke(this, $"SignalRPush - The hub {hubName} was reconnected by brute force from {origin} try count: {count}, on url {this.hubUrl}.");
                count++;
                if (count >= maxTryReconnect)
                    this.OnTryReconnectMax?.Invoke(this, count);
            }
        }

        private void HubConnection_Reconnecting()
        {
            this.OnReconnecting?.Invoke(this, $"Reconnecting (State: {this.hubConnection.State}), Last Error is: {this.hubConnection.LastError?.ToStringAllMessage()}");
            this.tryToReconnect = true;
        }
        private void HubConnection_Reconnected()
        {
            this.OnStart?.Invoke(this, $"SignalRPush - The hub {hubName} was reconnected on url {this.hubUrl}.");
            this.tryToReconnect = false;
        }

        private void HubConnection_Closed()
        {
            this.OnStop?.Invoke(this, $"SignalRPush - The hub { hubName} was closed from url {this.hubUrl}");
            TryToReconnect("OnClosed");
        }

        private void HubConnection_ConnectionSlow()
        {
            this.OnConnectionSlow?.Invoke(this, $"Connection Slow {this.hubConnection.ConnectionId}, Last Error is: {this.hubConnection.LastError?.ToStringAllMessage()}");
        }

        private void HubConnection_Error(Exception ex)
        {
            this.OnError?.Invoke(this, $"An Erro occurres on SignalRPush. { ex.ToStringAllMessage()}");
            TryToReconnect("OnError");
        }

        private void OnEvent(string channel, ChannelEvent<TResult> ev)
        {
            if (!ev.Name.Contains(".subscribed"))
                this.OnEventReceived?.Invoke(this, ev);
        }

        public async void Start()
        {
            await hubConnection.Start();
            await RegisterChannels();
            this.OnStart?.Invoke(this, $"SignalRPush - The hub {hubName} was called to connect on url {this.hubUrl}.");
        }
        public async void Stop()
        {
            this.tryToReconnect = false;
            await UnregisterChannels();
            hubConnection.Stop();
            this.OnStop?.Invoke(this, $"SignalRPush - The hub { hubName} was called to Stop for url {this.hubUrl}");
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
    }

    public interface ISignalRClient<T> where T : class
    {
        event EventHandler<string> OnStart;
        event EventHandler<string> OnStop;
        event EventHandler<string> OnError;
        event EventHandler<string> OnConnectionSlow;
        event EventHandler<string> OnReconnecting;
        event EventHandler<ChannelEvent<T>> OnEventReceived;
        void RegisterChannel(string channel);
        void UnregisterChannel(string channel);
        void ClearChannels();
        void Start();
        void Stop();
    }
}
