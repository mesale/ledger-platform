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
}
