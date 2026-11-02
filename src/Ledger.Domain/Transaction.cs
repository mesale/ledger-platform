namespace Ledger.Domain;

public class Transaction
{
    public Guid Id { get; }
    public string? IdempotencyKey { get; }
    public Guid? ReversesTransactionId { get; }
    public TransactionStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; }
    public IReadOnlyList<LedgerEntry> Entries { get; }

    public Transaction(Guid id, string? idempotencyKey, DateTimeOffset createdAt, IReadOnlyList<LedgerEntry> entries)
        : this(id, idempotencyKey, createdAt, entries, null)
    {
    }

    private Transaction(
        Guid id,
        string? idempotencyKey,
        DateTimeOffset createdAt,
        IReadOnlyList<LedgerEntry> entries,
        Guid? reversesTransactionId)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Transaction ID cannot be empty.", nameof(id));
        }

        if (idempotencyKey != null && string.IsNullOrWhiteSpace(idempotencyKey))
        {
            throw new ArgumentException("Idempotency key cannot be empty or whitespace.", nameof(idempotencyKey));
        }

        if (reversesTransactionId == Guid.Empty)
        {
            throw new ArgumentException("Reversed transaction ID cannot be empty.", nameof(reversesTransactionId));
        }

        if (reversesTransactionId == id)
        {
            throw new ArgumentException("A transaction cannot reverse itself.", nameof(reversesTransactionId));
        }

        var entriesList = entries?.ToList() ?? throw new ArgumentNullException(nameof(entries));
        EnsureAtLeastTwoEntries(entriesList);
        EnsureEntriesBelongToTransaction(id, entriesList);
        EnsureSingleCurrency(entriesList);
        EnsureBalanced(entriesList);

        Id = id;
        IdempotencyKey = idempotencyKey;
        ReversesTransactionId = reversesTransactionId;
        CreatedAt = createdAt;
        Status = TransactionStatus.Pending;
        Entries = entriesList.AsReadOnly();
    }

    private static void EnsureAtLeastTwoEntries(List<LedgerEntry> entries)
    {
        if (entries.Count < 2)
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

    public Transaction Reverse(
        Guid reversalTransactionId,
        string? idempotencyKey,
        DateTimeOffset createdAt,
        IReadOnlyList<Guid> reversalEntryIds)
    {
        if (Status != TransactionStatus.Posted)
        {
            throw new InvalidOperationException("Only posted transactions can be reversed.");
        }

        if (reversalTransactionId == Id)
        {
            throw new ArgumentException("A reversal must have a distinct transaction ID.", nameof(reversalTransactionId));
        }

        var entryIds = reversalEntryIds?.ToList() ?? throw new ArgumentNullException(nameof(reversalEntryIds));

        if (entryIds.Count != Entries.Count)
        {
            throw new ArgumentException(
                "A reversal must provide one new entry ID for each original entry.",
                nameof(reversalEntryIds));
        }

        if (entryIds.Any(id => id == Guid.Empty) || entryIds.Distinct().Count() != entryIds.Count)
        {
            throw new ArgumentException("Reversal entry IDs must be non-empty and unique.", nameof(reversalEntryIds));
        }

        var originalEntryIds = Entries.Select(entry => entry.Id).ToHashSet();
        if (entryIds.Any(originalEntryIds.Contains))
        {
            throw new ArgumentException("Reversal entries must have new IDs.", nameof(reversalEntryIds));
        }

        var reversalEntries = Entries
            .Select((entry, index) => new LedgerEntry(
                entryIds[index],
                entry.AccountId,
                reversalTransactionId,
                Opposite(entry.Direction),
                entry.Money))
            .ToList();

        var reversal = new Transaction(
            reversalTransactionId,
            idempotencyKey,
            createdAt,
            reversalEntries,
            Id);

        reversal.Post();
        return reversal;
    }

    private static Direction Opposite(Direction direction) =>
        direction == Direction.Debit ? Direction.Credit : Direction.Debit;
}
