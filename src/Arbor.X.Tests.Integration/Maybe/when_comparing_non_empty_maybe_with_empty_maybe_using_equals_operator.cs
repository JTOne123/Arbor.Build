using Arbor.X.Core;
using Machine.Specifications;

namespace Arbor.X.Tests.Integration.Maybe
{
    public class when_comparing_non_empty_maybe_with_empty_maybe_using_equals_operator
    {
        Because of = () => equal = new Maybe<string>("a string")==default(Maybe<string>);

        It should_return_false = () => equal.ShouldBeFalse();

        static bool equal;
    }
}