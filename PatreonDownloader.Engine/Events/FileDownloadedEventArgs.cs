using System;

namespace PatreonDownloader.Engine.Events
{
    public class FileDownloadedEventArgs : EventArgs
    {
        public FileDownloadedEventArgs(string url, int totalFiles, bool success = true, string errorMessage = null)
        {
            Success = success;
            Url = url ?? throw new ArgumentNullException(nameof(url), "Value could not be null");
            TotalFiles = totalFiles > 0
                ? totalFiles
                : throw new ArgumentOutOfRangeException(nameof(totalFiles), "Value cannot be lower than 1");

            if (!success)
            {
                ErrorMessage = errorMessage ?? throw new ArgumentNullException(nameof(errorMessage), "Value could not be null if success is false");
            }
        }

        public string Url { get; }

        public int TotalFiles { get; }

        public bool Success { get; }

        public string ErrorMessage { get; }
    }
}
