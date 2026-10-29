namespace Ledger.Domain;

public sealed record class Money
{
    public decimal Amount {get;}
    public string Currency {get;}

    public Money(decimal amount, string currency)
    {
        if (amount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount cannot be negative.");
        }
        Amount = amount;
        Currency = currency;
    }
}
