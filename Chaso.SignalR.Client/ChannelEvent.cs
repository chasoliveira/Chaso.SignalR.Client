using System;

namespace Chaso.SignalR.Client
{
    public class ChannelEvent<TResult> where TResult: class
    {
        public ChannelEvent()
        {
            Timestamp = DateTimeOffset.Now;
        }
        public string Origin { get; set; }
        public string Name { get; set; }
        public string ConnectionId { get; set; }
        public string ChannelName { get; set; }

        public DateTimeOffset Timestamp { get; set; }

        public TResult Data { get; set; }
    }
}
