﻿using System;
using JetBrains.Annotations;

namespace Arbor.Build.Core.Tools.MSBuild
{
    public static class Platforms
    {
        public static string Normalize([NotNull] string platform)
        {
            if (string.IsNullOrWhiteSpace(platform))
            {
                throw new ArgumentNullException(nameof(platform));
            }

            return platform.Replace(" ", string.Empty, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
