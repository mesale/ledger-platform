using Ledger.Domain;

namespace Ledger.Domain.Tests;

public class LedgerEntryTests
{
    [Theory]
    [InlineData(Direction.Debit)]
    [InlineData(Direction.Credit)]
    public void Constructor_CreatesEntryWithGivenValues(Direction direction)
    {
        var id = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        var transactionId = Guid.NewGuid();

        var entry = new LedgerEntry(id, accountId, transactionId, direction, new Money(12.34m, "USD"));

        Assert.Equal(id, entry.Id);
        Assert.Equal(accountId, entry.AccountId);
        Assert.Equal(transactionId, entry.TransactionId);
        Assert.Equal(direction, entry.Direction);
        Assert.Equal(12.34m, entry.Money.Amount);
        Assert.Equal("USD", entry.Money.Currency);
    }

    [Fact]
    public void Constructor_RejectsEmptyId()
    {
        Assert.Throws<ArgumentException>(() =>
            new LedgerEntry(Guid.Empty, Guid.NewGuid(), Guid.NewGuid(), Direction.Debit, new Money(1m, "USD")));
    }

    [Fact]
    public void Constructor_RejectsEmptyAccountId()
    {
        Assert.Throws<ArgumentException>(() =>
            new LedgerEntry(Guid.NewGuid(), Guid.Empty, Guid.NewGuid(), Direction.Debit, new Money(1m, "USD")));
    }

    [Fact]
    public void Constructor_RejectsEmptyTransactionId()
    {
        Assert.Throws<ArgumentException>(() =>
            new LedgerEntry(Guid.NewGuid(), Guid.NewGuid(), Guid.Empty, Direction.Debit, new Money(1m, "USD")));
    }

    [Fact]
    public void Constructor_RejectsUndefinedDirection()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new LedgerEntry(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), (Direction)99, new Money(1m, "USD")));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-0.01)]
    [InlineData(-1000.50)]
    public void Constructor_RejectsNonPositiveAmount(double rawAmount)
    {
        var amount = (decimal)rawAmount;

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new LedgerEntry(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Direction.Debit, new Money(amount, "USD")));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("US")]
    [InlineData("USDD")]
    [InlineData("U1D")]
    public void Constructor_RejectsInvalidCurrency(string? currency)
    {
        Assert.Throws<ArgumentException>(() =>
            new LedgerEntry(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Direction.Debit, new Money(1m, currency!)));
    }

    [Fact]
    public void Constructor_NormalizesCurrency()
    {
        var entry = new LedgerEntry(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Direction.Credit, new Money(1m, " usd "));

        Assert.Equal("USD", entry.Money.Currency);
    }

    [Fact]
    public void Properties_HaveNoPublicSetters()
    {
        var publicProperties = typeof(LedgerEntry).GetProperties();

        Assert.All(publicProperties, property => Assert.False(property.SetMethod?.IsPublic ?? false));
    }
}
