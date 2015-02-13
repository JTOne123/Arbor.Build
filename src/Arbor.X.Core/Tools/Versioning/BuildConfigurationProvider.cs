﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Arbor.X.Core.BuildVariables;
using Arbor.X.Core.Logging;

namespace Arbor.X.Core.Tools.Versioning
{
    public class BuildConfigurationProvider : IVariableProvider
    {
        public Task<IEnumerable<IVariable>> GetEnvironmentVariablesAsync(ILogger logger,
            IReadOnlyCollection<IVariable> buildVariables,
            CancellationToken cancellationToken)
        {
            var variables = new List<IVariable>();

            if (buildVariables.GetVariableValueOrDefault(WellKnownVariables.NetAssemblyConfiguration, null) == null)
            {
                variables.Add(new DynamicVariable(WellKnownVariables.NetAssemblyConfiguration, () =>
                {
                    string currentBuildConfiguration =
                        Environment.GetEnvironmentVariable(WellKnownVariables.CurrentBuildConfiguration);

                    return currentBuildConfiguration;
                }));
            }

            return Task.FromResult<IEnumerable<IVariable>>(variables);
        }
    }
}