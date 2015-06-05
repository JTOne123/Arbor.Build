using Arbor.X.Core;
using Machine.Specifications;

namespace Arbor.X.Tests.Integration.Maybe
{
    public class when_comparing_non_empty_maybe_with_null
    {
        Because of = () => equal = Equals(new Maybe<string>("a string"), null);

        It should_return_false = () => equal.ShouldBeFalse();

        static bool equal;
    }
}