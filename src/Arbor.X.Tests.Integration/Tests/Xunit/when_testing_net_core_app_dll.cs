using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Arbor.Build.Core.Tools.Testing;
using Arbor.Build.Tests.Integration.Tests.MSpec;
using Machine.Specifications;
using Mono.Cecil;
using Serilog;
using Serilog.Core;
using Xunit;

namespace Arbor.Build.Tests.Integration.Tests.Xunit
{
    [Ignore("local")]
    [Subject(typeof(UnitTestFinder))]
    public class when_testing_net_core_app_dll
    {
        static UnitTestFinder finder;
        static bool isTestType;

        Establish context = () =>
        {
            ILogger logger = Logger.None;
            finder = new UnitTestFinder(new List<Type>
                {
                    typeof(FactAttribute)
                },
                logger: logger);
        };

        Because of =
            () =>
            {
                AssemblyDefinition assemblyDefinition = AssemblyDefinition.ReadAssembly(
                    Path.Combine(VcsTestPathHelper.FindVcsRootPath(),
                        "src",
                        "Arbor.X.Tests.NetCoreAppSamle",
                        "Arbor.X.Tests.NetCoreAppSamle.dll"));

                TypeDefinition typeDefinition =
                    assemblyDefinition.MainModule.Types.Single(t =>
                        t.FullName.StartsWith("Arbor", StringComparison.Ordinal));

                isTestType = finder.TryIsTypeTestFixture(typeDefinition);
            };

        It should_Behaviour = () => isTestType.ShouldBeTrue();
    }
}