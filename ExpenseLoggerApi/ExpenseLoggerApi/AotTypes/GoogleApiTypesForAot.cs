using System.Diagnostics.CodeAnalysis;
using Google.Apis.Sheets.v4.Data;

namespace ExpenseLoggerApi.AotTypes;

// dummy class with attributes to preserve the necessary types:
internal static class GoogleApiTypesForAot
{
    // Force preservation of ThemeColorPair and its constructors/properties for reflection
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(ThemeColorPair))]
    // Force preservation of the concrete List<T> and its constructor when used with ThemeColorPair
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(List<ThemeColorPair>))]
    // Might need the containing types too, if the issue is deeper
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(SpreadsheetTheme))]
    // Add others here if errors shift to different types during deserialization
    static GoogleApiTypesForAot() { } // Dummy static constructor
}