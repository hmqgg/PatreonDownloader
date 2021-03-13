using CommandLine;

namespace PatreonDownloader.App.Models
{
    public class CommandLineOptions
    {
        [Option("url", Required = true, HelpText = "Url of the creator's page")]
        public string Url { get; set; }

        [Option("descriptions", Required = false, HelpText = "Save post descriptions", Default = false)]
        public bool SaveDescriptions { get; set; }

        [Option("embeds", Required = false, HelpText = "Save embedded content metadata", Default = false)]
        public bool SaveEmbeds { get; set; }

        [Option("json", Required = false, HelpText = "Save json data", Default = false)]
        public bool SaveJson { get; set; }

        [Option("upgrade-id", Required = false, HelpText = "Download posts that upgraded at specified Tier ID only")]
        public string UpgradeId { get; set; }

        [Option("title-include", Required = false, HelpText = "Download posts only of which title includes the value")]
        public string TitleInclude { get; set; }

        [Option("title-exclude", Required = false, HelpText = "Download posts only of which title doesn't include the value")]
        public string TitleExclude { get; set; }

        [Option("filename-include", Required = false, HelpText = "Download files only of which title includes the value")]
        public string FilenameInclude { get; set; }

        [Option("filename-exclude", Required = false, HelpText = "Download files only of which title doesn't include the value")]
        public string FilenameExclude { get; set; }

        [Option("post-ids", Required = false, HelpText = "Download posts only with specified ids, split with ';'")]
        public string PostIds { get; set; }

        [Option("attachment-only", Required = false, HelpText = "Download attachments only", Default = false)]
        public bool AttachmentOnly { get; set; }

        [Option("no-external", Required = false, HelpText = "Do not download external URLs", Default = false)]
        public bool NoExternal { get; set; }

        [Option("campaign-images", Required = false, HelpText = "Download campaign's avatar and cover images", Default = false)]
        public bool SaveAvatarAndCover { get; set; }

        [Option("download-directory", Required = false,
            HelpText = "Directory to save all downloaded files in, default: #AppDirectory#/downloads/#CreatorName#.")]
        public string DownloadDirectory { get; set; }

        [Option("verbose", Required = false, HelpText = "Enable verbose (debug) logging", Default = false)]
        public bool Verbose { get; set; }

        [Option("no-headless", Required = false, HelpText = "Show internal browser window (disable headless mode)", Default = false)]
        public bool NoHeadless { get; set; }

        [Option("overwrite-files", Required = false,
            HelpText =
                "Overwrite already existing files (recommended if creator might have files multiple files with the same filename or makes changes to already existing posts)",
            Default = false)]
        public bool OverwriteFiles { get; set; }

        [Option("remote-browser-address", Required = false,
            HelpText = "Advanced users only. Address of the browser with remote debugging enabled. Refer to documentation for more details.")]
        public string RemoteBrowserAddress { get; set; }
    }
}
