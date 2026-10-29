namespace Ledger.Domain;

public sealed record class Money
{
    public decimal Amount { get; }
    public string Currency { get; }

    public Money(decimal amount, string currency)
    {
        if (amount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount cannot be negative.");
        }
        Amount = amount;
        Currency = NormalizeCurrency(currency);
    }

    private static string NormalizeCurrency(string currency)
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
