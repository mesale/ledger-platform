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
        Assert.Equal(TransactionStatus.Pending, transaction.Status);
        Assert.Equal(createdAt, transaction.CreatedAt);
        Assert.Equal(2, transaction.Entries.Count);
    }

    private static Transaction Create(Guid id, params LedgerEntry[] entries) =>
        new(id, null, DateTimeOffset.UtcNow, entries);

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

}
