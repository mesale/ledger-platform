namespace Ledger.Domain;

public class Transaction
{
    public Guid Id { get; }
    public string? IdempotencyKey { get; }
    public TransactionStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; }
    public IReadOnlyList<LedgerEntry> Entries { get; }

    public Transaction(Guid id, string? idempotencyKey, DateTimeOffset createdAt, IReadOnlyList<LedgerEntry> entries)
    {
        Id = id;
        IdempotencyKey = idempotencyKey;
        CreatedAt = createdAt;
        Status = TransactionStatus.Pending;
        Entries = entries;
    }

}