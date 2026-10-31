
namespace Ledger.Domain;

public sealed class LedgerEntry
{
    public Guid Id { get; }
    public Guid TransactionId { get; }
    public Guid AccountId { get; }
    public Direction Direction { get; }
    public Money Money { get; }
    public string Currency { get; }

    public LedgerEntry(Guid id, Guid accountId, Guid transactionId, Direction direction, Money money)
    {

        if (id == Guid.Empty)
        {
            throw new ArgumentException("Ledger entry ID cannot be empty.", nameof(id));
        }

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

        if (money.Amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(money.Amount), money.Amount, "Amount must be positive.");
        }
        Id = id;
        AccountId = accountId;
        Direction = direction;
        Money = money;
        TransactionId = transactionId;
        Currency = money.Currency;
    }
}
