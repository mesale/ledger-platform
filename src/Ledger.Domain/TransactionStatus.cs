namespace Ledger.Domain;

public enum TransactionStatus
{
    Pending = 1,
    Posted = 2,
    Failed = 3,
    Reversed = 4
}
