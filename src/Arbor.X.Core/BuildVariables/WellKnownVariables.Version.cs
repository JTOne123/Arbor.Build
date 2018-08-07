﻿namespace Arbor.X.Core.BuildVariables
{
    public static partial class WellKnownVariables
    {
        [VariableDescription("Major version")]
        public const string VersionMajor = "Version.Major";

        [VariableDescription("Minor version")]
        public const string VersionMinor = "Version.Minor";

        [VariableDescription("Patch version")]
        public const string VersionPatch = "Version.Patch";

        [VariableDescription("Build version")]
        public const string VersionBuild = "Version.Build";
    }
}
