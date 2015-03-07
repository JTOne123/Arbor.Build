using Arbor.X.Core;
using Machine.Specifications;

namespace Arbor.X.Tests.Integration.Maybe
{
    public class when_comparing_the_same_maybe_with_itself_with_not_equals_operator
    {
        Establish context = () => instance = new Core.Maybe<string>("a string");

        Because of = () => equal = instance != instance;

        It should_return_false = () => equal.ShouldBeFalse();

        static bool equal;
        static Maybe<string> instance;
    }
}