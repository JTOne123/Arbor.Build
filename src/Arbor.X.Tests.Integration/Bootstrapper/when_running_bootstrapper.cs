﻿using System;
using System.IO;
using Arbor.Processing.Core;
using Arbor.X.Core.Bootstrapper;
using Arbor.X.Core.IO;
using Machine.Specifications;
using Serilog.Core;

namespace Arbor.X.Tests.Integration.Bootstrapper
{
    [Ignore("Not complete")]
    [Subject(typeof(Core.Bootstrapper.Bootstrapper))]
    public class when_running_bootstrapper
    {
        static Core.Bootstrapper.Bootstrapper bootstrapper;

        static BootstrapStartOptions startOptions;
        static ExitCode exitCode;
        static DirectoryInfo baseDirectory;

        Cleanup after = () =>
        {
            try
            {
                baseDirectory.DeleteIfExists(true);
            }
            catch (IOException ex)
            {
                Console.Error.WriteLine(ex);
            }
        };

        Establish context = () =>
        {
            string tempDirectoryPath = Path.Combine(Path.GetTempPath(),
                $"{DefaultPaths.TempPathPrefix}_Bootstrapper_Test_{Guid.NewGuid()}");

            baseDirectory = new DirectoryInfo(tempDirectoryPath).EnsureExists();
            Console.WriteLine("Temp directory is {0}", baseDirectory.FullName);

            startOptions = new BootstrapStartOptions(baseDirectory.FullName,
                true,
                "develop");
            bootstrapper = new Core.Bootstrapper.Bootstrapper(Logger.None);
        };

        Because of = () => { exitCode = bootstrapper.StartAsync(startOptions).Result; };

        It should_return_success_exit_code = () => exitCode.IsSuccess.ShouldBeTrue();
    }
}
