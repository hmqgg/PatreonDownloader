using PatreonDownloader.Engine.Helpers;

namespace PatreonDownloader.Engine
{
    public sealed class PatreonDownloaderSettings
    {
        private bool _attachmentOnly;
        private string _downloadDirectory;
        private string _filenameExclude;
        private string _filenameInclude;
        private bool _noExternal;
        private bool _overwriteFiles;
        private string[] _postIds;
        private bool _saveAvatarAndCover;
        private bool _saveDescriptions;
        private bool _saveEmbeds;
        private bool _saveJson;
        private string _titleExclude;
        private string _titleInclude;
        private string _upgradeId;

        public PatreonDownloaderSettings()
        {
            _saveDescriptions = true;
            _saveEmbeds = true;
            _saveJson = true;
            _saveAvatarAndCover = true;
            _downloadDirectory = null;
            _overwriteFiles = false;
            _titleInclude = null;
            _titleExclude = null;
            _postIds = null;
            _attachmentOnly = false;
            _noExternal = false;
            _upgradeId = null;
        }

        /// <summary>
        ///     Any attempt to set properties will result in exception if this set to true
        /// </summary>
        internal bool Consumed { get; set; }

        public bool SaveDescriptions
        {
            get => _saveDescriptions;
            set => ConsumableSetter.Set(Consumed, ref _saveDescriptions, value);
        }

        public bool SaveEmbeds
        {
            get => _saveEmbeds;
            set => ConsumableSetter.Set(Consumed, ref _saveEmbeds, value);
        }

        public bool SaveJson
        {
            get => _saveJson;
            set => ConsumableSetter.Set(Consumed, ref _saveJson, value);
        }

        public bool SaveAvatarAndCover
        {
            get => _saveAvatarAndCover;
            set => ConsumableSetter.Set(Consumed, ref _saveAvatarAndCover, value);
        }

        /// <summary>
        ///     Target directory for downloaded files. If set to null files will be downloaded into
        ///     #AppDirectory#/downloads/#CreatorName#.
        /// </summary>
        public string DownloadDirectory
        {
            get => _downloadDirectory;
            set => ConsumableSetter.Set(Consumed, ref _downloadDirectory, value);
        }

        /// <summary>
        ///     Overwrite already existing files
        /// </summary>
        public bool OverwriteFiles
        {
            get => _overwriteFiles;
            set => ConsumableSetter.Set(Consumed, ref _overwriteFiles, value);
        }

        public string TitleInclude
        {
            get => _titleInclude;
            set => ConsumableSetter.Set(Consumed, ref _titleInclude, value);
        }

        public string TitleExclude
        {
            get => _titleExclude;
            set => ConsumableSetter.Set(Consumed, ref _titleExclude, value);
        }

        public string FilenameInclude
        {
            get => _filenameInclude;
            set => ConsumableSetter.Set(Consumed, ref _filenameInclude, value);
        }

        public string FilenameExclude
        {
            get => _filenameExclude;
            set => ConsumableSetter.Set(Consumed, ref _filenameExclude, value);
        }

        public string[] PostIds
        {
            get => _postIds;
            set => ConsumableSetter.Set(Consumed, ref _postIds, value);
        }

        public bool AttachmentOnly
        {
            get => _attachmentOnly;
            set => ConsumableSetter.Set(Consumed, ref _attachmentOnly, value);
        }

        public bool NoExternal
        {
            get => _noExternal;
            set => ConsumableSetter.Set(Consumed, ref _noExternal, value);
        }

        public string UpgradeId
        {
            get => _upgradeId;
            set => ConsumableSetter.Set(Consumed, ref _upgradeId, value);
        }

        public override string ToString()
        {
            return
                $"SaveDescriptions={_saveDescriptions},SaveEmbeds={_saveEmbeds},SaveJson={_saveJson},SaveAvatarAndCover={_saveAvatarAndCover},DownloadDirectory={_downloadDirectory},OverwriteFiles={_overwriteFiles}";
        }
    }
}
