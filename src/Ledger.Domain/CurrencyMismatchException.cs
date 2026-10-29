
namespace Ledger.Domain;

public sealed class CurrencyMismatchException : InvalidOperationException
{
    public string LeftCurrency { get; }
    public string RightCurrency { get; }

    public CurrencyMismatchException(string leftCurrency, string rightCurrency)
        : base($"Currency mismatch: {leftCurrency} and {rightCurrency}.")
    {
        LeftCurrency = leftCurrency;
        RightCurrency = rightCurrency;
    }
}
