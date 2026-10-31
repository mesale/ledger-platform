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
        Currency = CurrencyCode.Normalize(currency);
    }

    public static Money operator +(Money left, Money right)
    {
        EnsureSameCurrency(left, right);

        return new Money(left.Amount + right.Amount, left.Currency);
    }

    public static Money operator -(Money left, Money right)
    {
        EnsureSameCurrency(left, right);

        if (left.Amount < right.Amount)
        {
            throw new InvalidOperationException("Cannot subtract a larger amount from a smaller amount.");
        }

        return new Money(left.Amount - right.Amount, left.Currency);
    }

    private static void EnsureSameCurrency(Money left, Money right)
    {
        if (left.Currency != right.Currency)
        {
            throw new CurrencyMismatchException(left.Currency, right.Currency);
        }
    }
}
