using Machine.Specifications;

namespace Arbor.Build.Tests.Integration.Maybe
{
#pragma warning disable 1718

    [Subject(typeof(Defensive.Maybe<string>))]
    public class when_comparing_the_same_maybe_with_itself_with_not_equals_operator
    {
        static bool equal;
        static Defensive.Maybe<string> instance;
        Establish context = () => instance = new Defensive.Maybe<string>("a string");

        // ReSharper disable once EqualExpressionComparison
        Because of = () => equal = instance != instance;

        It should_return_false = () => equal.ShouldBeFalse();
    }

#pragma warning restore 1718
}
