using Arbor.X.Core;
using Machine.Specifications;

namespace Arbor.X.Tests.Integration.Maybe
{
    public class when_comparing_empty_maybe_with_null_using_overrided_equals
    {
        Because of = () => equal = new Maybe<string>().Equals(null);

        It should_return_false = () => equal.ShouldBeFalse();

        static bool equal;
    }
}