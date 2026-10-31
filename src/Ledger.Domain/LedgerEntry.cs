namespace Ledger.Domain;

public sealed class LedgerEntry
{
    public Guid Id { get; }
    public Guid TransactionId { get; }
    public Guid AccountId { get; }
    public Direction Direction { get; }
    public decimal Amount { get; }
    public string Currency { get; }

    public LedgerEntry(Guid accountId, Guid transactionId, Direction direction, decimal amount, string currency)
    {
        if (accountId == Guid.Empty)
        {
            throw new ArgumentException("Account ID cannot be empty.", nameof(accountId));
        }

        if (transactionId == Guid.Empty)
        {
            throw new ArgumentException("Transaction ID cannot be empty.", nameof(transactionId));
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
        TransactionId = transactionId;
        Currency = CurrencyCode.Normalize(currency);
    }
}
