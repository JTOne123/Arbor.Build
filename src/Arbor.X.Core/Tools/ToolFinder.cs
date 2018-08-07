﻿using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Arbor.Defensive.Collections;
using Autofac;
using Serilog;

namespace Arbor.X.Core.Tools
{
    public static class ToolFinder
    {
        public static IReadOnlyCollection<ToolWithPriority> GetTools(ILifetimeScope lifetimeScope, ILogger logger)
        {
            var tools = lifetimeScope.Resolve<IReadOnlyCollection<ITool>>();

            List<ToolWithPriority> prioritizedTools = tools
                .Select(tool =>
                {
                    PriorityAttribute priorityAttribute =
                        tool.GetType()
                            .GetCustomAttributes()
                            .OfType<PriorityAttribute>()
                            .SingleOrDefault();

                    int priority = priorityAttribute?.Priority ?? int.MaxValue;

                    bool runAlways = priorityAttribute != null && priorityAttribute.RunAlways;

                    return new ToolWithPriority(tool, priority, runAlways);
                })
                .OrderBy(item => item.Priority)
                .ToList();

            logger.Verbose("Found {Count} prioritized tools", prioritizedTools.Count);

            return prioritizedTools;
        }
    }
}
