namespace Ledger.Domain.Tests;

public class TransactionTests
{
    private static LedgerEntry Entry(Guid transactionId, Direction direction, decimal amount, string currency = "USD") =>
        new(Guid.NewGuid(), Guid.NewGuid(), transactionId, direction, new Money(amount, currency));

    [Fact]
    public void Constructor_CreatesPendingTransactionWithEntries()
    {
        var id = Guid.NewGuid();
        var createdAt = new DateTimeOffset(2026, 10, 31, 9, 0, 0, TimeSpan.Zero);
        var entries = new[]
        {
            Entry(id, Direction.Debit, 100m),
            Entry(id, Direction.Credit, 100m)
        };

        var transaction = new Transaction(id, "key-1", createdAt, entries);

        Assert.Equal(id, transaction.Id);
        Assert.Equal("key-1", transaction.IdempotencyKey);
        Assert.Null(transaction.ReversesTransactionId);
        Assert.Equal(TransactionStatus.Pending, transaction.Status);
        Assert.Equal(createdAt, transaction.CreatedAt);
        Assert.Equal(2, transaction.Entries.Count);
    }

    private static Transaction Create(Guid id, params LedgerEntry[] entries) =>
        new(id, null, DateTimeOffset.UtcNow, entries);

    private static Transaction NewPendingTransaction()
    {
        var id = Guid.NewGuid();
        return new Transaction(id, null, DateTimeOffset.UtcNow, new[]
        {
            Entry(id, Direction.Debit, 10m),
            Entry(id, Direction.Credit, 10m)
        });
    }

    [Fact]
    public void Constructor_RejectsEmptyId()
    {
        var empty = Guid.Empty;

        Assert.Throws<ArgumentException>(() =>
            Create(empty, Entry(empty, Direction.Debit, 10m), Entry(empty, Direction.Credit, 10m)));
    }

    [Fact]
    public void Constructor_RejectsFewerThanTwoEntries()
    {
        var id = Guid.NewGuid();

        Assert.Throws<LedgerInvariantException>(() => Create(id, Entry(id, Direction.Debit, 10m)));
    }

    [Fact]
    public void Constructor_RejectsMixedCurrencies()
    {
        var id = Guid.NewGuid();

        Assert.Throws<LedgerInvariantException>(() =>
            Create(id, Entry(id, Direction.Debit, 10m, "USD"), Entry(id, Direction.Credit, 10m, "EUR")));
    }

    [Fact]
    public void Constructor_RejectsUnbalancedEntries()
    {
        var id = Guid.NewGuid();

        Assert.Throws<LedgerInvariantException>(() =>
            Create(id, Entry(id, Direction.Debit, 10m), Entry(id, Direction.Credit, 9.99m)));
    }

    [Fact]
    public void Constructor_RejectsEntryFromAnotherTransaction()
    {
        var id = Guid.NewGuid();

        Assert.Throws<LedgerInvariantException>(() =>
            Create(id, Entry(id, Direction.Debit, 10m), Entry(Guid.NewGuid(), Direction.Credit, 10m)));
    }

    [Fact]
    public void Constructor_AcceptsMultipleDebitsAndCreditsThatBalance()
    {
        var id = Guid.NewGuid();

        var transaction = Create(id,
            Entry(id, Direction.Debit, 60m),
            Entry(id, Direction.Debit, 40m),
            Entry(id, Direction.Credit, 100m));

        Assert.Equal(3, transaction.Entries.Count);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_RejectsBlankIdempotencyKey(string? key)
    {
        var id = Guid.NewGuid();
        var entries = new[] { Entry(id, Direction.Debit, 10m), Entry(id, Direction.Credit, 10m) };

        if (key is null)
        {
            var tx = new Transaction(id, key, DateTimeOffset.UtcNow, entries);
            Assert.Null(tx.IdempotencyKey);
            return;
        }

        Assert.Throws<ArgumentException>(() => new Transaction(id, key, DateTimeOffset.UtcNow, entries));
    }

    [Fact]
    public void Post_FromPending_SetsPosted()
    {
        var transaction = NewPendingTransaction();

        transaction.Post();

        Assert.Equal(TransactionStatus.Posted, transaction.Status);
    }

    [Fact]
    public void Fail_FromPending_SetsFailed()
    {
        var transaction = NewPendingTransaction();

        transaction.Fail();

        Assert.Equal(TransactionStatus.Failed, transaction.Status);
    }

    [Fact]
    public void Reverse_FromPosted_CreatesDistinctPostedTransactionWithOppositeEntries()
    {
        var originalId = Guid.NewGuid();
        var debitAccountId = Guid.NewGuid();
        var creditAccountId = Guid.NewGuid();
        var originalEntries = new[]
        {
            new LedgerEntry(Guid.NewGuid(), debitAccountId, originalId, Direction.Debit, new Money(10m, "USD")),
            new LedgerEntry(Guid.NewGuid(), creditAccountId, originalId, Direction.Credit, new Money(10m, "USD"))
        };
        var original = new Transaction(originalId, "original-key", DateTimeOffset.UtcNow, originalEntries);
        original.Post();
        var reversalId = Guid.NewGuid();
        var reversalEntryIds = new[] { Guid.NewGuid(), Guid.NewGuid() };
        var reversedAt = new DateTimeOffset(2026, 11, 2, 10, 0, 0, TimeSpan.Zero);

        var reversal = original.Reverse(reversalId, "reversal-key", reversedAt, reversalEntryIds);

        Assert.NotSame(original, reversal);
        Assert.Equal(reversalId, reversal.Id);
        Assert.Equal("reversal-key", reversal.IdempotencyKey);
        Assert.Equal(original.Id, reversal.ReversesTransactionId);
        Assert.Equal(TransactionStatus.Posted, reversal.Status);
        Assert.Equal(reversedAt, reversal.CreatedAt);
        Assert.Collection(
            reversal.Entries,
            entry =>
            {
                Assert.Equal(reversalEntryIds[0], entry.Id);
                Assert.Equal(debitAccountId, entry.AccountId);
                Assert.Equal(reversalId, entry.TransactionId);
                Assert.Equal(Direction.Credit, entry.Direction);
                Assert.Equal(new Money(10m, "USD"), entry.Money);
            },
            entry =>
            {
                Assert.Equal(reversalEntryIds[1], entry.Id);
                Assert.Equal(creditAccountId, entry.AccountId);
                Assert.Equal(reversalId, entry.TransactionId);
                Assert.Equal(Direction.Debit, entry.Direction);
                Assert.Equal(new Money(10m, "USD"), entry.Money);
            });
    }

    [Fact]
    public void Reverse_LeavesOriginalTransactionAndEntriesUnchanged()
    {
        var original = NewPendingTransaction();
        original.Post();
        var originalEntries = original.Entries.ToArray();

        _ = original.Reverse(
            Guid.NewGuid(),
            "reversal-key",
            DateTimeOffset.UtcNow,
            original.Entries.Select(_ => Guid.NewGuid()).ToArray());

        Assert.Equal(TransactionStatus.Posted, original.Status);
        Assert.Null(original.ReversesTransactionId);
        Assert.Equal(originalEntries, original.Entries);
    }

    [Fact]
    public void Reverse_IsBalancedAndNetsOriginalAccountMovementsToZero()
    {
        var transactionId = Guid.NewGuid();
        var firstAccountId = Guid.NewGuid();
        var secondAccountId = Guid.NewGuid();
        var thirdAccountId = Guid.NewGuid();
        var original = Create(
            transactionId,
            new LedgerEntry(Guid.NewGuid(), firstAccountId, transactionId, Direction.Debit, new Money(60m, "USD")),
            new LedgerEntry(Guid.NewGuid(), secondAccountId, transactionId, Direction.Debit, new Money(40m, "USD")),
            new LedgerEntry(Guid.NewGuid(), thirdAccountId, transactionId, Direction.Credit, new Money(100m, "USD")));
        original.Post();

        var reversal = original.Reverse(
            Guid.NewGuid(),
            "reversal-key",
            DateTimeOffset.UtcNow,
            original.Entries.Select(_ => Guid.NewGuid()).ToArray());

        Assert.Equal(
            reversal.Entries.Where(entry => entry.Direction == Direction.Debit).Sum(entry => entry.Money.Amount),
            reversal.Entries.Where(entry => entry.Direction == Direction.Credit).Sum(entry => entry.Money.Amount));

        var netMovements = original.Entries
            .Concat(reversal.Entries)
            .GroupBy(entry => entry.AccountId)
            .Select(group => group.Sum(entry =>
                entry.Direction == Direction.Debit ? entry.Money.Amount : -entry.Money.Amount));

        Assert.All(netMovements, amount => Assert.Equal(0m, amount));
    }

    [Fact]
    public void Reverse_FromPending_Throws()
    {
        var transaction = NewPendingTransaction();

        Assert.Throws<InvalidOperationException>(() => transaction.Reverse(
            Guid.NewGuid(),
            "reversal-key",
            DateTimeOffset.UtcNow,
            transaction.Entries.Select(_ => Guid.NewGuid()).ToArray()));
    }

    [Fact]
    public void Reverse_FromFailed_Throws()
    {
        var transaction = NewPendingTransaction();
        transaction.Fail();

        Assert.Throws<InvalidOperationException>(() => transaction.Reverse(
            Guid.NewGuid(),
            "reversal-key",
            DateTimeOffset.UtcNow,
            transaction.Entries.Select(_ => Guid.NewGuid()).ToArray()));
    }

    [Fact]
    public void Reverse_RejectsOriginalEntryIds()
    {
        var transaction = NewPendingTransaction();
        transaction.Post();

        Assert.Throws<ArgumentException>(() => transaction.Reverse(
            Guid.NewGuid(),
            "reversal-key",
            DateTimeOffset.UtcNow,
            transaction.Entries.Select(entry => entry.Id).ToArray()));
    }

    [Fact]
    public void Post_WhenAlreadyPosted_Throws()
    {
        var transaction = NewPendingTransaction();
        transaction.Post();

        Assert.Throws<InvalidOperationException>(() => transaction.Post());
    }

    [Fact]
    public void Fail_WhenPosted_Throws()
    {
        var transaction = NewPendingTransaction();
        transaction.Post();

        Assert.Throws<InvalidOperationException>(() => transaction.Fail());
    }

    [Fact]
    public void Failed_IsTerminal()
    {
        var transaction = NewPendingTransaction();
        transaction.Fail();

        Assert.Throws<InvalidOperationException>(() => transaction.Post());
        Assert.Throws<InvalidOperationException>(() => transaction.Fail());
        Assert.Throws<InvalidOperationException>(() => transaction.Reverse(
            Guid.NewGuid(),
            "reversal-key",
            DateTimeOffset.UtcNow,
            transaction.Entries.Select(_ => Guid.NewGuid()).ToArray()));
    }

    [Fact]
    public void Constructor_CopiesEntryCollection()
    {
        var id = Guid.NewGuid();
        var entries = new List<LedgerEntry>
        {
            Entry(id, Direction.Debit, 10m),
            Entry(id, Direction.Credit, 10m)
        };
        var transaction = new Transaction(id, null, DateTimeOffset.UtcNow, entries);

        entries.Clear();

        Assert.Equal(2, transaction.Entries.Count);
    }

}
