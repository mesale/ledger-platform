namespace Ledger.Domain;

internal static class CurrencyCode
{
    public static string Normalize(string? currency)
    {
        if (string.IsNullOrWhiteSpace(currency))
        {
            throw new ArgumentException("Currency cannot be null or whitespace.", nameof(currency));
        }

        var code = currency.Trim().ToUpperInvariant();

        if (code.Length != 3 || !code.All(char.IsLetter))
        {
            throw new ArgumentException("Currency must be a 3-letter code.", nameof(currency));
        }

        return code;
    }
}
