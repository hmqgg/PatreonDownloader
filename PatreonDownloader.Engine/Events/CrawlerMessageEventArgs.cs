using System;

namespace PatreonDownloader.Engine.Events
{
    public enum CrawlerMessageType
    {
        Info,
        Warning,
        Error
    }

    public sealed class CrawlerMessageEventArgs : EventArgs
    {
        public CrawlerMessageEventArgs(CrawlerMessageType messageType, string message, long postId = -1)
        {
            MessageType = messageType;
            Message = message ?? throw new ArgumentNullException(nameof(message));
            PostId = postId;
        }

        public CrawlerMessageType MessageType { get; }

        public string Message { get; }

        public long PostId { get; }
    }
}
