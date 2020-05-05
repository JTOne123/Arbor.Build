using Arbor.Build.Core;
using Arbor.Build.Core.Tools.Git;
using Arbor.Build.Tests.Integration.Bootstrapper;
using Machine.Specifications;

namespace Arbor.Build.Tests.Integration.GitBranches
{
    [Subject(typeof(BranchHelper))]
    public class when_parsing_version_from_branch_name_with_slash
    {
        static string branchName;
        static string version;
        Establish context = () => branchName = "refs/heads/release/1.2.3";

        Because of = () => version = BranchHelper.BranchSemVerMajorMinorPatch(branchName, EnvironmentVariables.Empty).ToString();

        It should_extract_the_version = () => version.ShouldEqual("1.2.3");
    }
}
