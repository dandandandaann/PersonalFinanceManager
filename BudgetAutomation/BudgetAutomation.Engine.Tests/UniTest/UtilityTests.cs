using System.Text.RegularExpressions;
using BudgetAutomation.Engine.Misc;
using Shouldly;
using Xunit;

namespace BudgetAutomation.Engine.Tests.UniTest;

public partial class UtilityTests
{

#pragma warning disable CS8974 // Converting method group to non-delegate type
    public static IEnumerable<object[]> TryExtractCommandArgumentsData =>
        new List<object[]>
        {
            new object[] { "/signup", "signup", null! },
            new object[] { "/signup Arguments123 ", "signup", new Func<Regex> (() => new Regex("^[a-zA-Z0-9]+$")) },
            new object[] { "/signup email@email.com", "signup", EmailRegex },
            new object[] { "/command QWE123qwe", "command", LettersAndNumbersRegex },
        };
#pragma warning restore CS8974 // Converting method group to non-delegate type

    [GeneratedRegex("^[a-zA-Z0-9]+$")] private static partial Regex LettersAndNumbersRegex();
    [GeneratedRegex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$")] private static partial Regex EmailRegex();

    [Theory]
    [MemberData(nameof(TryExtractCommandArgumentsData))]
    public void TryExtractCommandArguments_ValidInput_ShouldReturnTrue(string text, string commandName, Func<Regex>? regex = null)
    {
        // Arrange

        // Act
        var result = Utility.TryExtractCommandArguments(text, commandName, regex, out _);

        // Assert
        result.ShouldBeTrue();
    }
}