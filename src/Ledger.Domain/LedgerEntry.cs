namespace Ledger.Domain;

public sealed class LedgerEntry
{
    public Guid AccountId { get; }
    public Direction Direction { get; }
    public decimal Amount { get; }
    public string Currency { get; }

    public LedgerEntry(Guid accountId, Direction direction, decimal amount, string currency)
    {
        if (accountId == Guid.Empty)
        {
            throw new ArgumentException("Account ID cannot be empty.", nameof(accountId));
        }

        if (!Enum.IsDefined(direction))
        {
            throw new ArgumentOutOfRangeException(nameof(direction), direction, "Invalid ledger entry direction.");
        }

        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), amount, "Amount must be positive.");
        }

        AccountId = accountId;
        Direction = direction;
        Amount = amount;
        Currency = CurrencyCode.Normalize(currency);
    }
}
