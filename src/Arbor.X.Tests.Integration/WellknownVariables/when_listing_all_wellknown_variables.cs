﻿using System;
using System.Collections.Generic;
using Arbor.X.Core.BuildVariables;
using Machine.Specifications;

namespace Arbor.X.Tests.Integration.WellknownVariables
{
    [Tags("Arbor_X_Recursive")]
    public class when_listing_all_wellknown_variables
    {
        static IReadOnlyCollection<VariableDescription> readOnlyCollection;

        Because of = () => { readOnlyCollection = WellKnownVariables.AllVariables; };

        It should_print_all_variables = () =>
        {
            foreach (VariableDescription variableDescription in readOnlyCollection)
            {
                Console.WriteLine(variableDescription.ToString());
            }
        };
    }
}