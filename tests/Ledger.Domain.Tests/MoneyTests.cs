using Ledger.Domain;
using Xunit;

namespace Ledger.Domain.Tests;

public class MoneyTests
{

    [Fact]
    public void Constructor_StoresAmountAndCurrency()
    {
        var money = new Money(12.34m, "USD");
        Assert.Equal(12.34m, money.Amount);
        Assert.Equal("USD", money.Currency);
    }

}
