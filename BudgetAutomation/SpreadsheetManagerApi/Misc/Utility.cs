namespace SpreadsheetManagerApi.Misc;

public static class Utility
{

    public static int LetterToColumnIndex(this string columnLetter)
    {
        if (string.IsNullOrEmpty(columnLetter))
            throw new ArgumentNullException(nameof(columnLetter));

        int sum = 0;
        foreach (char c in columnLetter.ToUpper())
        {
            sum = sum * 26 + (c - 'A' + 1);
        }
        return sum - 1; // Convert to 0-indexed
    }
}