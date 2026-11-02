using FsCheck.Xunit;

namespace Ledger.Domain.Tests;

public class TransactionPropertyTests
{
    private const int CasesPerProperty = 2_000;

    [Property(
        Arbitrary = new[] { typeof(TransactionArbitraries) },
        MaxTest = CasesPerProperty,
        EndSize = 100)]
    public void ValidBalancedTransactions_AreAccepted(
        TransactionArbitraries.ValidTransactionCase testCase)
    {
        var transactionSpec = testCase.Transaction;
        var debitTotal = transactionSpec.Entries
            .Where(entry => entry.Direction == Direction.Debit)
            .Sum(entry => entry.Amount);
        var creditTotal = transactionSpec.Entries
            .Where(entry => entry.Direction == Direction.Credit)
            .Sum(entry => entry.Amount);

        Assert.Equal(debitTotal, creditTotal);
        Assert.Single(transactionSpec.Entries.Select(entry => entry.Currency).Distinct());

        var transaction = transactionSpec.CreateTransaction();

        Assert.Equal(transactionSpec.Entries.Count, transaction.Entries.Count);
        Assert.Equal(TransactionStatus.Pending, transaction.Status);
    }

    [Property(
        Arbitrary = new[] { typeof(TransactionArbitraries) },
        MaxTest = CasesPerProperty,
        EndSize = 100)]
    public void SingleEntryTransactions_AreRejected(
        TransactionArbitraries.SingleEntryTransactionCase testCase)
    {
        Assert.Throws<LedgerInvariantException>(() => testCase.Transaction.CreateTransaction());
    }

    [Property(
        Arbitrary = new[] { typeof(TransactionArbitraries) },
        MaxTest = CasesPerProperty,
        EndSize = 100)]
    public void MixedCurrencyTransactions_AreRejected(
        TransactionArbitraries.MixedCurrencyTransactionCase testCase)
    {
        Assert.Throws<LedgerInvariantException>(() => testCase.Transaction.CreateTransaction());
    }

    [Property(
        Arbitrary = new[] { typeof(TransactionArbitraries) },
        MaxTest = CasesPerProperty,
        EndSize = 100)]
    public void FractionallyUnbalancedTransactions_AreRejected(
        TransactionArbitraries.FractionallyUnbalancedTransactionCase testCase)
    {
        Assert.Throws<LedgerInvariantException>(() => testCase.Transaction.CreateTransaction());
    }
}
