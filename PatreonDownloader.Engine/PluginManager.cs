using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using NLog;
using PatreonDownloader.Common.Interfaces.Plugins;
using PatreonDownloader.Interfaces.Models;

namespace PatreonDownloader.Engine
{
    internal sealed class PluginManager : IPluginManager
    {
        private static readonly string PluginsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins");
        private readonly IPlugin _defaultPlugin;
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private readonly List<IPlugin> _plugins;

        public PluginManager(IPlugin defaultPlugin)
        {
            _defaultPlugin = defaultPlugin;

            if (!Directory.Exists(PluginsDirectory))
            {
                Directory.CreateDirectory(PluginsDirectory);
            }

            _plugins = new List<IPlugin>();
            var files = Directory.EnumerateFiles(PluginsDirectory);
            foreach (var file in files)
            {
                try
                {
                    if (!file.EndsWith(".dll"))
                    {
                        continue;
                    }

                    var filename = Path.GetFileName(file);
                    var assembly = Assembly.LoadFrom(file);
                    var types = assembly.GetTypes();
                    var pluginType = types.SingleOrDefault(x => x.GetInterfaces().Contains(typeof(IPlugin)));
                    if (pluginType == null)
                    {
                        continue;
                    }

                    _logger.Debug($"New plugin found: {filename}");

                    var plugin = Activator.CreateInstance(pluginType) as IPlugin;
                    if (plugin == null)
                    {
                        _logger.Error($"Invalid plugin {filename}: IPlugin interface could not be created");
                        continue;
                    }

                    _plugins.Add(plugin);

                    _logger.Info(
                        $"Loaded plugin: {plugin.Name}"); // {assembly.GetName().Version} by {plugin.Author} ({plugin.ContactInformation})
                }
                catch (Exception ex)
                {
                    _logger.Error($"Unable to load plugin {file}: {ex}");
                }
            }
        }

        public async Task BeforeStart(PatreonDownloaderSettings settings)
        {
            foreach (var plugin in _plugins)
            {
                await plugin.BeforeStart(settings.OverwriteFiles);
            }

            await _defaultPlugin.BeforeStart(settings.OverwriteFiles);
        }

        public async Task DownloadCrawledUrl(CrawledUrl crawledUrl, string downloadDirectory)
        {
            if (crawledUrl == null)
            {
                throw new ArgumentNullException(nameof(crawledUrl));
            }

            if (downloadDirectory == null)
            {
                throw new ArgumentNullException(nameof(downloadDirectory));
            }

            var downloadPlugin = _defaultPlugin;

            if (_plugins != null && _plugins.Count > 0)
            {
                foreach (var plugin in _plugins)
                {
                    if (await plugin.IsSupportedUrl(crawledUrl.Url))
                    {
                        downloadPlugin = plugin;
                        break;
                    }
                }
            }

            // Sanitize dir.
            var title = Path.GetInvalidFileNameChars().Aggregate(crawledUrl.PostName, (current, c) => current.Replace(c, '_'));
            var dir = Path.Combine(downloadDirectory, crawledUrl.Date.ToString("yyyyMM"), $"[{crawledUrl.PostId}]{title}");

            Directory.CreateDirectory(dir);
            await downloadPlugin.Download(crawledUrl, dir);
        }

        public async Task<List<string>> ExtractSupportedUrls(string htmlContents)
        {
            var retHashSet = new HashSet<string>();
            if (_plugins != null && _plugins.Count > 0)
            {
                foreach (var plugin in _plugins)
                {
                    var pluginRetList = await plugin.ExtractSupportedUrls(htmlContents);
                    if (pluginRetList != null && pluginRetList.Count > 0)
                    {
                        foreach (var url in pluginRetList)
                        {
                            retHashSet.Add(url);
                        }
                    }
                }
            }

            var defaultPluginRetList = await _defaultPlugin.ExtractSupportedUrls(htmlContents);
            if (defaultPluginRetList != null && defaultPluginRetList.Count > 0)
            {
                foreach (var url in defaultPluginRetList.Where(url => !retHashSet.Contains(url)))
                {
                    retHashSet.Add(url);
                }
            }

            return retHashSet.ToList();
        }
    }
}
