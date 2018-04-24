using System;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;

namespace FF12TZAPCPatcher
{
    internal static class Updater
    {
        private const string GithubLink =
            "https://raw.githubusercontent.com/mztikk/FF12TZAPCPatcher/master/FF12TZAPCPatcher/Properties/AssemblyInfo.cs";

        private static readonly Regex VersionMatch = new Regex(@"n\(""\d.\d.\d.\d", RegexOptions.Compiled);

        internal static string GetAssemblyVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        internal static string GetGithubVersion(string githubInfo)
        {
            var wc = new WebClient();
            var webData = wc.DownloadString(githubInfo);
            var reg = VersionMatch.Match(webData);
            var version = reg.Value.Remove(0, 3);
            return version;
        }

        internal static string GetGithubVersion()
        {
            return GetGithubVersion(GithubLink);
        }

        internal static bool IsOnlineDiff(string githubInfo)
        {
            var currentVersion = GetAssemblyVersion();
            var githubVersion = GetGithubVersion(githubInfo);
            return !string.Equals(currentVersion, githubVersion, StringComparison.Ordinal);
        }

        internal static bool IsOnlineDiff()
        {
            return IsOnlineDiff(GithubLink);
        }
    }
}