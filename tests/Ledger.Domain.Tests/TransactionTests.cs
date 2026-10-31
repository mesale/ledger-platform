using Ledger.Domain;
using Xunit;

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
}