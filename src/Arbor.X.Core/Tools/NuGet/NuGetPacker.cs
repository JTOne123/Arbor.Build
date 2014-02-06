﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Arbor.Aesculus.Core;
using Arbor.X.Core.BuildVariables;
using Arbor.X.Core.Logging;
using Arbor.X.Core.ProcessUtils;

namespace Arbor.X.Core.Tools.NuGet
{
    [Priority(650)]
    public class NuGetPacker : ITool
    {
        public async Task<ExitCode> ExecuteAsync(ILogger logger, IReadOnlyCollection<IVariable> buildVariables)
        {
            var artifacts = buildVariables.Require(WellKnownVariables.Artifacts).ThrowIfEmptyValue();
            var version = buildVariables.Require(WellKnownVariables.Version).ThrowIfEmptyValue();
            var releaseBuild = buildVariables.Require(WellKnownVariables.ReleaseBuild).ThrowIfEmptyValue();
            var branchName = buildVariables.Require(WellKnownVariables.BranchName).ThrowIfEmptyValue();
            var configuration = buildVariables.Require(WellKnownVariables.Configuration).ThrowIfEmptyValue().Value;
            var tempDirectory = buildVariables.Require(WellKnownVariables.TempDirectory).ThrowIfEmptyValue();
            var nuGetExePath =
                buildVariables.Require(WellKnownVariables.ExternalTools_NuGet_ExePath).ThrowIfEmptyValue().Value;

            if (branchName.Value.Equals("master", StringComparison.InvariantCultureIgnoreCase))
            {
                logger.Write("NuGet package creation is not supported on 'master' branch");
                return ExitCode.Success;
            }

            var isReleaseBuild = IsReleaseBuild(releaseBuild);

            var packagesDirectory = Path.Combine(artifacts.Value, "packages");

            if (!Directory.Exists(packagesDirectory))
            {
                Directory.CreateDirectory(packagesDirectory);
            }

            if (!File.Exists(nuGetExePath))
            {
                logger.WriteError(string.Format(
                    "The NuGet.exe path {0} was not found or NuGet could not be downloaded", nuGetExePath));
                return ExitCode.Failure;
            }

            var vcsRootDir = VcsPathHelper.FindVcsRootPath();

            logger.Write(string.Format("Scanning directory '{0}' for .nuspec files", vcsRootDir));

            var packageDirectory = PackageDirectory();

            var packageSpecifications = GetPackageSpecifications(logger, vcsRootDir, packageDirectory);
            
            var result = await ProcessPackages(packageSpecifications, nuGetExePath,packagesDirectory,version,isReleaseBuild,configuration,branchName,logger,tempDirectory);

            return result;
        }

        static IEnumerable<string> GetPackageSpecifications(ILogger logger, string vcsRootDir, string packageDirectory)
        {
            var packageSpecifications = Directory.GetFiles(vcsRootDir, "*.nuspec", SearchOption.AllDirectories)
                                                 .Where(
                                                     file =>
                                                     file.IndexOf(packageDirectory, StringComparison.Ordinal) < 0)
                                                 .ToList();

            logger.Write(string.Format("Found nuspec files [{0}]: {1}{2}", packageSpecifications.Count,
                                       Environment.NewLine, string.Join(Environment.NewLine, packageSpecifications)));
            return packageSpecifications;
        }

        static string PackageDirectory()
        {
            var packageDirectory = Path.DirectorySeparatorChar + "packages" + Path.DirectorySeparatorChar;
            return packageDirectory;
        }

        static bool IsReleaseBuild(IVariable releaseBuild)
        {
            bool isReleaseBuild;

            if (!bool.TryParse(releaseBuild.Value, out isReleaseBuild))
            {
                throw new ArgumentException(string.Format("The build variable {0} is not a boolean", releaseBuild.Value));
            }
            return isReleaseBuild;
        }

        async Task<ExitCode> ProcessPackages(IEnumerable<string> packageSpecifications, string nuGetExePath, string packagesDirectory, IVariable version, bool isReleaseBuild, string configuration, IVariable branchName, ILogger logger, IVariable tempDirectory)
        {
            foreach (var packageSpecification in packageSpecifications)
            {
                var packageResult =
                    await
                    CreatePackageAsync(nuGetExePath, packageSpecification, packagesDirectory, version, isReleaseBuild,
                                       configuration, branchName, logger, tempDirectory);

                if (!packageResult.IsSuccess)
                {
                    logger.WriteError(string.Format("Could not create NuGet package from specification '{0}'",
                                                    packageSpecification));
                    return packageResult;
                }
            }

            return ExitCode.Success;
        }

        static async Task<ExitCode> CreatePackageAsync(string nuGetExePath, string nuspecFilePath, string packagesDirectory,
                                                  IVariable version, bool isReleaseBuild, string configuration,
                                                  IVariable branchName, ILogger logger, IVariable tempDirectory)
        {
            NuSpec nuSpec = NuSpec.Parse(nuspecFilePath);

            var properties = GetProperties(configuration);
            
            string packageId = NuGetPackageIdHelper.CreateNugetPackageId(nuSpec.PackageId, isReleaseBuild,
                                                                         branchName.Value);

            string nuGetPackageVersion = NuGetVersionHelper.GetVersion(version.Value, isReleaseBuild);
            
            var nuSpecInfo = new FileInfo(nuspecFilePath);
// ReSharper disable AssignNullToNotNullAttribute
            var nuSpecFileCopyPath = Path.Combine(nuSpecInfo.DirectoryName,
                                                  string.Format("{0}-{1}", Guid.NewGuid(), nuSpecInfo.Name));
// ReSharper restore AssignNullToNotNullAttribute

            var nuSpecCopy = new NuSpec(packageId, nuGetPackageVersion, nuSpecInfo.FullName);

            var nuSpecTempDirectory = Path.Combine(tempDirectory.Value, "nuget-specifications");

            if (!Directory.Exists(nuSpecTempDirectory))
            {
                Directory.CreateDirectory(nuSpecTempDirectory);
            }

            logger.Write(string.Format("Saving new nuspec {0}", nuSpecFileCopyPath));
            nuSpecCopy.Save(nuSpecFileCopyPath);

            logger.Write(string.Format("Created nuspec content: {0}{1}", Environment.NewLine, File.ReadAllText(nuSpecFileCopyPath)));

            var result = await ExecuteNuGetPackAsync(nuGetExePath, packagesDirectory, logger, nuSpecFileCopyPath, properties, nuSpecCopy);

            return result;
        }
        
        static async Task<ExitCode> ExecuteNuGetPackAsync(string nuGetExePath, string packagesDirectory, ILogger logger,
                                                string nuSpecFileCopyPath, string properties, NuSpec nuSpecCopy)
        {
            ExitCode result;
            try
            {
                var arguments = new List<string>
                                    {
                                        "pack",
                                        nuSpecFileCopyPath,
                                        "-Properties",
                                        properties,
                                        "-OutputDirectory",
                                        packagesDirectory,
                                        "-Version",
                                        nuSpecCopy.Version,
                                        "-Verbosity",
                                        "Detailed",
                                        "-Symbols"
                                    };

                var processResult =
                    await
                    ProcessRunner.ExecuteAsync(nuGetExePath, arguments: arguments, standardOutLog: logger.Write,
                                               standardErrorAction: logger.WriteError, toolAction: logger.Write);

                result = processResult;
            }
            finally
            {
                if (File.Exists(nuSpecFileCopyPath))
                {
                    File.Delete(nuSpecFileCopyPath);
                }
            }
            return result;
        }

        static string GetProperties(string configuration)
        {
            var propertyValues = new List<KeyValuePair<string, string>>
                                     {
                                         new KeyValuePair<string, string>(
                                             "configuration", configuration)
                                     };

            var formattedValues = propertyValues.Select(item => string.Format("{0}={1}", item.Key, item.Value));
            string properties = string.Join(";", formattedValues);
            return properties;
        }
    }
}