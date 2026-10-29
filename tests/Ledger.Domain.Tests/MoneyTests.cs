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

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("US")]
    [InlineData("USDD")]
    [InlineData("U1D")]
    [InlineData("$$$")]
    public void Constructor_RejectsInvalidCurrency(string? currency)
    {
        Assert.Throws<ArgumentException>(() => new Money(1m, currency!));

    }

    [Theory]
    [InlineData("usd")]
    [InlineData("Usd")]
    public void Constructor_NormalizesCurrencyToUpperCase(string currency)
    {
        var money = new Money(1m, currency);
        Assert.Equal(currency.ToUpper(), money.Currency);
    }

    [Fact]
    public void Add_SameCurrency_SumsAmount()
    {
        var result = new Money(10m, "USD") + new Money(5m, "USD");
        Assert.Equal(new Money(15m, "USD"), result);
    }

    [Fact]
    public void Add_UsesExact_DecimalArthmetic()
    {
        var result = new Money(0.1m, "USD") + new Money(0.2m, "USD");
        Assert.Equal(new Money(0.3m, "USD"), result);
    }

    [Fact]
    public void Add_DifferentCurrency_Throws()
    {
        var ex = Assert.Throws<CurrencyMismatchException>(() => new Money(10m, "USD") + new Money(5m, "EUR"));
        Assert.Equal("USD", ex.LeftCurrency);
        Assert.Equal("EUR", ex.RightCurrency);
    }

    [Fact]
    public void Subtract_SameCurrency_SubtractsAmounts()
    {
        var result = new Money(10m, "USD") - new Money(3.25m, "USD");

        Assert.Equal(new Money(6.75m, "USD"), result);
    }

    [Fact]
    public void Subtract_ToExactlyZero_IsAllowed()
    {
        var result = new Money(5m, "USD") - new Money(5m, "USD");

        Assert.Equal(0m, result.Amount);
    }

    [Fact]
    public void Subtract_ResultWouldBeNegative_Throws()
    {
        Assert.Throws<InvalidOperationException>(
            () => new Money(1m, "USD") - new Money(2m, "USD"));
    }

    [Fact]
    public void Subtract_DifferentCurrency_Throws()
    {
        Assert.Throws<CurrencyMismatchException>(
            () => new Money(5m, "USD") - new Money(1m, "EUR"));
    }

}
