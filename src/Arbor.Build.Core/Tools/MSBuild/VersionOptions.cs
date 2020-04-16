﻿using Arbor.Build.Core.Tools.NuGet;
using Serilog;

namespace Arbor.Build.Core.Tools.MSBuild
{
    public class VersionOptions
    {
        public VersionOptions(string version)
        {
            Version = version;
        }

        public GitModel? GitModel { get; set; }

        public string Version { get; }

        public bool IsReleaseBuild { get; set; }

        public string? BuildSuffix { get; set; }

        public bool BuildNumberEnabled { get; set; } = true;

        public string? Metadata { get; set; }

        public ILogger? Logger { get; set; }

        public NuGetVersioningSettings NuGetVersioningSettings { get; set; } = NuGetVersioningSettings.Default;
    }
}