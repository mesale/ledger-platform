namespace Ledger.Domain;

public sealed class LedgerInvariantException : Exception
{
    public LedgerInvariantException(string message) : base(message) { }
}
