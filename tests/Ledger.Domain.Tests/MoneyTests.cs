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

    [Theory]
    [InlineData(-0.01)]
    [InlineData(-1)]
    [InlineData(-1000.50)]
    public void Constractor_RejectsNegativeAmount(double raw)
    {
        var amount = (decimal)raw;

        Assert.Throws<ArgumentOutOfRangeException>(() => new Money(amount, "USD"));
    }

    [Fact]
    public void Constructor_AllowsZero()
    {
        var money = new Money(0m, "USD");
        Assert.Equal(0m, money.Amount);
    }

}
