﻿using NLog;
using NLog.Config;
using NLog.Targets;

namespace PatreonDownloader.App
{
    internal static class NLogManager
    {
        public static void ReconfigureNLog(bool debug = false)
        {
            var configuration = new LoggingConfiguration();
            var consoleTarget = new ColoredConsoleTarget("console")
            {
                Layout = "${longdate} ${uppercase:${level}} ${message}"
            };
            configuration.AddTarget(consoleTarget);

            var fileTarget = new FileTarget("file")
            {
                FileName = "${basedir}/logs/${shortdate}.log",
                Layout = "${longdate} ${uppercase:${level}} [${logger}] ${message}"
            };
            configuration.AddTarget(fileTarget);

            configuration.AddRule(debug ? LogLevel.Debug : LogLevel.Info, LogLevel.Fatal, consoleTarget, debug ? "*" : "PatreonDownloader.App.*");
            //configuration.AddRule(debug ? LogLevel.Debug : LogLevel.Info, LogLevel.Fatal, consoleTarget, debug ? "*" : "PatreonDownloader.PuppeteerEngine.*");
            configuration.AddRuleForAllLevels(fileTarget);

            LogManager.Configuration = configuration;
        }
    }
}
