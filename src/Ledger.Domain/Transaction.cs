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

        if (id == Guid.Empty)
        {
            throw new ArgumentException("Transaction ID cannot be empty.", nameof(id));
        }

        if (idempotencyKey != null && string.IsNullOrWhiteSpace(idempotencyKey))
        {
            throw new ArgumentException("Idempotency key cannot be empty or whitespace.", nameof(idempotencyKey));
        }

        var entriesList = entries?.ToList() ?? throw new ArgumentNullException(nameof(entries));
        EnsureAtleastTwoEntries(entriesList);
        EnsureEntriesBelongToTransaction(id, entriesList);
        EnsureSingleCurrency(entriesList);
        EnsureBalanced(entriesList);

        Id = id;
        IdempotencyKey = idempotencyKey;
        CreatedAt = createdAt;
        Status = TransactionStatus.Pending;
        Entries = entries;
    }

    private static void EnsureAtleastTwoEntries(List<LedgerEntry> entries)
    {
        if (entries == null || entries.Count < 2)
        {
            throw new LedgerInvariantException("A transaction must have at least two ledger entries.");
        }
    }

    private static void EnsureEntriesBelongToTransaction(Guid transactionId, List<LedgerEntry> entries)
    {
        if (entries.Any(e => e.TransactionId != transactionId))
        {
            throw new LedgerInvariantException("All ledger entries must belong to the same transaction.");
        }
    }

    private static void EnsureSingleCurrency(List<LedgerEntry> entries)
    {
        if (entries.Select(e => e.Money.Currency).Distinct().Count() > 1)
        {
            throw new LedgerInvariantException("All ledger entries must have the same currency.");
        }
    }

    private static void EnsureBalanced(List<LedgerEntry> entries)
    {
        if (entries.Sum(e => e.Direction == Direction.Debit ? e.Money.Amount : -e.Money.Amount) != 0)
        {
            throw new LedgerInvariantException("The sum of debit and credit amounts must be equal.");
        }
    }

    public void Post()
    {
        if (Status != TransactionStatus.Pending)
        {
            throw new InvalidOperationException("Only pending transactions can be posted.");
        }

        Status = TransactionStatus.Posted;
    }

    public void Fail()
    {
        if (Status != TransactionStatus.Pending)
        {
            throw new InvalidOperationException("Only pending transactions can be failed.");
        }

        Status = TransactionStatus.Failed;
    }

    public void Reverse()
    {
        if (Status != TransactionStatus.Posted)
        {
            throw new InvalidOperationException("Only posted transactions can be reversed.");
        }

        Status = TransactionStatus.Reversed;
    }

}
