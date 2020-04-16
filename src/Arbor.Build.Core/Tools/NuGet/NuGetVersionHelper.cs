﻿using System;
using System.Globalization;
using Arbor.Build.Core.GenericExtensions;
using Arbor.Build.Core.Tools.MSBuild;
using NuGet.Versioning;
using Serilog;

namespace Arbor.Build.Core.Tools.NuGet
{
    public static class NuGetVersionHelper
    {
        public static string GetVersion(
            string version,
            bool isReleaseBuild,
            string? suffix,
            bool enableBuildNumber,
            string? packageBuildMetadata,
            ILogger? logger,
            NuGetVersioningSettings? nugetVersioningSettings = null,
            GitModel? gitModel = null)
        {
            if (!Version.TryParse(version, out Version? parsedVersion))
            {
                throw new ArgumentException($"The version '{version} is not a valid version format");
            }

            if (isReleaseBuild && GitModel.GitFlowBuildOnMaster != gitModel)
            {
                string parsed = parsedVersion.ToString(3);

                logger?.Information("Build is release build, using major.minor.patch as the version, {Parsed}", parsed);

                return parsed;
            }

            string buildVersion;

            var settings = nugetVersioningSettings ?? NuGetVersioningSettings.Default;

            int usePadding =
                settings.SemVerVersion == 1 && settings.MaxZeroPaddingLength > 0
                    ? settings.MaxZeroPaddingLength
                    : 0;

            string semVer2PreReleaseSeparator = settings.SemVerVersion >= 2
                ? "."
                : string.Empty;

            if (GitModel.GitFlowBuildOnMaster == gitModel && isReleaseBuild)
            {
                suffix ??= "rc";
            }
            else
            {
                suffix ??= "build";
            }

            if (suffix.Length > 0)
            {
                if (enableBuildNumber)
                {
                    buildVersion =
                        $"{parsedVersion.Major}.{parsedVersion.Minor}.{parsedVersion.Build}-{suffix}{semVer2PreReleaseSeparator}{parsedVersion.Revision.ToString(CultureInfo.InvariantCulture).LeftPad(usePadding, '0')}";

                    logger?.Information(
                        "Package suffix is {Suffix}, using major.minor.patch-{UsedSuffix}build as the version, {BuildVersion}",
                        suffix,
                        suffix,
                        buildVersion);
                }
                else
                {
                    buildVersion = $"{parsedVersion.Major}.{parsedVersion.Minor}.{parsedVersion.Build}-{suffix}";

                    logger?.Information(
                        "Package suffix is {Suffix}, using major.minor.patch-{UsedSuffix} as the version, {BuildVersion}",
                        suffix,
                        suffix,
                        buildVersion);
                }
            }
            else
            {
                if (enableBuildNumber)
                {
                    buildVersion =
                        $"{parsedVersion.Major}.{parsedVersion.Minor}.{parsedVersion.Build}-{parsedVersion.Revision.ToString().LeftPad(usePadding, '0')}";

                    logger?.Information("Using major.minor.patch-build as the version, {BuildVersion}", buildVersion);
                }
                else
                {
                    buildVersion = $"{parsedVersion.Major}.{parsedVersion.Minor}.{parsedVersion.Build}";
                    logger?.Information("Using major.minor.patch as the version, {BuildVersion}", buildVersion);
                }
            }

            string final = !string.IsNullOrWhiteSpace(packageBuildMetadata)
                ? $"{buildVersion}+{packageBuildMetadata.TrimStart('+')}"
                : buildVersion;

            if (!SemanticVersion.TryParse(final, out SemanticVersion _))
            {
                throw new InvalidOperationException($"The NuGet version '{final}' is not a valid Semver 2.0 version");
            }

            return final;
        }

        public static string GetPackageVersion(VersionOptions versionOptions)
        {
            string version = GetVersion(
                versionOptions.Version,
                versionOptions.IsReleaseBuild,
                versionOptions.BuildSuffix,
                versionOptions.BuildNumberEnabled,
                versionOptions.Metadata,
                versionOptions.Logger,
                versionOptions.NuGetVersioningSettings,
                versionOptions.GitModel);

            string packageVersion = SemanticVersion.Parse(
                version).ToNormalizedString();

            return packageVersion;
        }
    }
}
