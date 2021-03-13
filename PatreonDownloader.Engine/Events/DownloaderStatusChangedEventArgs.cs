using System;
using PatreonDownloader.Engine.Enums;

namespace PatreonDownloader.Engine.Events
{
    public sealed class DownloaderStatusChangedEventArgs : EventArgs
    {
        public DownloaderStatusChangedEventArgs(DownloaderStatus status)
        {
            Status = status;
        }

        public DownloaderStatus Status { get; }
    }
}
