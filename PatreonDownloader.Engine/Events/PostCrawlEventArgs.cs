using System;

namespace PatreonDownloader.Engine.Events
{
    public sealed class PostCrawlEventArgs : EventArgs
    {
        public PostCrawlEventArgs(long postId, bool success, string errorMessage = null)
        {
            PostId = postId > 0 ? postId : throw new ArgumentOutOfRangeException(nameof(postId), "Value cannot be lower than 1");
            Success = success;
            if (!success)
            {
                ErrorMessage = errorMessage ?? throw new ArgumentNullException(nameof(errorMessage), "Value could not be null if success is false");
            }
        }

        public long PostId { get; }

        public bool Success { get; }

        public string ErrorMessage { get; }
    }
}
