namespace Ledger.Domain;

public sealed record class Money
{
    public decimal Amount {get;}
    public string Currency {get;}

    public Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }
}
