using Ledger.Domain;
using Xunit;

namespace Ledger.Domain.Tests;

public class AccountTests
{
    [Fact]
    public void Constructor_CreatesActiveAccountWithGivenValues()
    {
        var id = Guid.NewGuid();
        var createdAt = new DateTimeOffset(2026, 10, 29, 12, 0, 0, TimeSpan.Zero);

        var account = new Account(id, "Operating Account", "USD", AccountType.Asset, createdAt);

        Assert.Equal(id, account.Id);
        Assert.Equal("Operating Account", account.Name);
        Assert.Equal("USD", account.Currency);
        Assert.Equal(AccountType.Asset, account.Type);
        Assert.Equal(AccountStatus.Active, account.Status);
        Assert.Equal(createdAt, account.CreatedAt);
    }

    [Fact]
    public void Constructor_RejectsEmptyId()
    {
        Assert.Throws<ArgumentException>(() =>
            new Account(Guid.Empty, "Operating", "USD", AccountType.Asset, DateTimeOffset.UtcNow));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_RejectsBlankName(string? name)
    {
        Assert.Throws<ArgumentException>(() =>
            new Account(Guid.NewGuid(), name!, "USD", AccountType.Asset, DateTimeOffset.UtcNow));
    }

    [Theory]
    [InlineData("US")]
    [InlineData("U1D")]
    [InlineData("")]
    public void Constructor_RejectsInvalidCurrency(string currency)
    {
        Assert.Throws<ArgumentException>(() =>
            new Account(Guid.NewGuid(), "Operating", currency, AccountType.Asset, DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Constructor_NormalizesCurrencyAndTrimsName()
    {
        var account = new Account(Guid.NewGuid(), "  Operating  ", "usd", AccountType.Asset, DateTimeOffset.UtcNow);

        Assert.Equal("Operating", account.Name);
        Assert.Equal("USD", account.Currency);
    }

    [Fact]
    public void Constructor_RejectsUndefinedAccountType()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Account(Guid.NewGuid(), "Operating", "USD", (AccountType)99, DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Freez_FromActive_SetsFrozen()
    {
        var account = NewAccount();
        account.Freeze();
        Assert.Equal(AccountStatus.Frozen, account.Status);
    }

    [Fact]
    public void Unfreeze_FromFrozen_SetsActive()
    {
        var account = NewAccount();
        account.Freeze();
        account.Unfreeze();
        Assert.Equal(AccountStatus.Active, account.Status);
    }

    [Fact]
    public void Close_FromActive_SetsClosed()
    {
        var account = NewAccount();
        account.Close();
        Assert.Equal(AccountStatus.Closed, account.Status);
    }

    [Fact]
    public void Close_FromFrozen_SetsClosed()
    {
        var account = NewAccount();
        account.Freeze();
        account.Close();
        Assert.Equal(AccountStatus.Closed, account.Status);
    }

    [Fact]
    public void Freeze_WhenAlreadyFrozen_Throws()
    {
        var account = NewAccount();
        account.Freeze();
        Assert.Throws<InvalidOperationException>(() => account.Freeze());
    }

    [Fact]
    public void Unfreeze_FromActive_Throws()
    {
        var account = NewAccount();
        Assert.Throws<InvalidOperationException>(() => account.Unfreeze());
    }

    [Fact]
    public void Closed_IsTerminal()
    {
        var account = NewAccount();
        account.Close();
        Assert.Throws<InvalidOperationException>(() => account.Freeze());
        Assert.Throws<InvalidOperationException>(() => account.Unfreeze());
        Assert.Throws<InvalidOperationException>(() => account.Close());
    }

    private static Account NewAccount() =>
    new(Guid.NewGuid(), "Operating", "USD", AccountType.Asset, DateTimeOffset.UtcNow);
}
