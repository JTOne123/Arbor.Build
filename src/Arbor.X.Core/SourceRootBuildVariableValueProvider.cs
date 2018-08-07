using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Arbor.X.Core.BuildVariables;
using Arbor.X.Core.IO;
using JetBrains.Annotations;
using Serilog;

namespace Arbor.X.Core
{
    [UsedImplicitly]
    public class SourceRootBuildVariableValueProvider : IVariableProvider
    {
        private readonly string _sourceDirectory;

        public SourceRootBuildVariableValueProvider(SourceRootValue sourceDirectory = null)
        {
            _sourceDirectory = sourceDirectory?.SourceRoot;
        }

        public int Order => int.MinValue;

        public Task<IEnumerable<IVariable>> GetBuildVariablesAsync(
            ILogger logger,
            IReadOnlyCollection<IVariable> buildVariables,
            CancellationToken cancellationToken)
        {
            var variables = new List<IVariable>();

            if (!string.IsNullOrWhiteSpace(_sourceDirectory))
            {
                variables.Add(new BuildVariable(WellKnownVariables.SourceRoot, _sourceDirectory));
                variables.Add(new BuildVariable(
                    WellKnownVariables.ExternalTools,
                    new DirectoryInfo(Path.Combine(_sourceDirectory, "tools", "external")).EnsureExists()
                        .FullName));
            }

            return Task.FromResult<IEnumerable<IVariable>>(variables);
        }
    }
}
