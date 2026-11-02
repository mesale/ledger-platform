using FsCheck;
using FsCheck.Fluent;

namespace Ledger.Domain.Tests;

public static class TransactionArbitraries
{
    private static readonly string[] _currencies = ["USD", "EUR", "ETB", "GBP", "JPY"];
    private static readonly decimal[] _scaleFactors = [1m, 10m, 100m, 1_000m, 10_000m, 100_000m, 1_000_000m];
    private static readonly decimal[] _imbalanceDeltas = [0.1m, 0.01m, 0.001m, 0.0001m, 0.00001m, 0.000001m, 0.0000001m, 0.00000001m];

    public sealed record EntrySpec(Direction Direction, decimal Amount, string Currency);

    public sealed record TransactionSpec(Guid TransactionId, IReadOnlyList<EntrySpec> Entries)
    {
        public Transaction CreateTransaction()
        {
            var ledgerEntries = Entries
                .Select((entry, index) => new LedgerEntry(
                    CreateGuid(index + 1),
                    CreateGuid(index + 101),
                    TransactionId,
                    entry.Direction,
                    new Money(entry.Amount, entry.Currency)))
                .ToArray();

            return new Transaction(TransactionId, null, DateTimeOffset.UnixEpoch, ledgerEntries);
        }
    }

    public sealed record ValidTransactionCase(TransactionSpec Transaction);

    public sealed record SingleEntryTransactionCase(TransactionSpec Transaction);

    public sealed record MixedCurrencyTransactionCase(TransactionSpec Transaction);

    public sealed record FractionallyUnbalancedTransactionCase(TransactionSpec Transaction);

    public static Arbitrary<ValidTransactionCase> ValidTransactions() =>
        ValidTransactionSpecs()
            .Select(transaction => new ValidTransactionCase(transaction))
            .ToArbitrary();

    public static Arbitrary<SingleEntryTransactionCase> SingleEntryTransactions() =>
        (from transaction in ValidTransactionSpecs()
         from entryIndex in Gen.Choose(0, transaction.Entries.Count - 1)
         select new SingleEntryTransactionCase(
             transaction with { Entries = [transaction.Entries[entryIndex]] }))
        .ToArbitrary();

    public static Arbitrary<MixedCurrencyTransactionCase> MixedCurrencyTransactions() =>
        (from transaction in ValidTransactionSpecs()
         from entryIndex in Gen.Choose(0, transaction.Entries.Count - 1)
         let originalEntry = transaction.Entries[entryIndex]
         let tamperedEntry = originalEntry with { Currency = NextCurrency(originalEntry.Currency) }
         select new MixedCurrencyTransactionCase(ReplaceEntry(transaction, entryIndex, tamperedEntry)))
        .ToArbitrary();

    public static Arbitrary<FractionallyUnbalancedTransactionCase> FractionallyUnbalancedTransactions() =>
        (from transaction in ValidTransactionSpecs()
         from entryIndex in Gen.Choose(0, transaction.Entries.Count - 1)
         from delta in Gen.Elements(_imbalanceDeltas)
         let originalEntry = transaction.Entries[entryIndex]
         let tamperedEntry = originalEntry with { Amount = originalEntry.Amount + delta }
         select new FractionallyUnbalancedTransactionCase(ReplaceEntry(transaction, entryIndex, tamperedEntry)))
        .ToArbitrary();

    private static Gen<TransactionSpec> ValidTransactionSpecs() =>
        from transactionNumber in Gen.Choose(1, 1_000_000_000)
        let transactionId = CreateGuid(transactionNumber)
        from currency in Gen.Elements(_currencies)
        from scaleIndex in Gen.Choose(0, _scaleFactors.Length - 1)
        from debitCount in Gen.Choose(1, 5)
        from creditCount in Gen.Choose(1, 5)
        let minimumTotalUnits = Math.Max(debitCount, creditCount)
        from totalUnits in Gen.Choose(minimumTotalUnits, 1_000_000)
        from debitWeights in Gen.ArrayOf(Gen.Choose(1, 1_000), debitCount)
        from creditWeights in Gen.ArrayOf(Gen.Choose(1, 1_000), creditCount)
        from entries in Gen.Shuffle(CreateBalancedEntries(
            PartitionUnits(totalUnits, debitWeights),
            PartitionUnits(totalUnits, creditWeights),
            _scaleFactors[scaleIndex],
            currency))
        select new TransactionSpec(transactionId, entries);

    private static EntrySpec[] CreateBalancedEntries(
        int[] debitPartitions,
        int[] creditPartitions,
        decimal scaleFactor,
        string currency)
    {
        var debits = debitPartitions.Select(units =>
            new EntrySpec(Direction.Debit, (units + 1) / scaleFactor, currency));
        var credits = creditPartitions.Select(units =>
            new EntrySpec(Direction.Credit, (units + 1) / scaleFactor, currency));

        return debits.Concat(credits).ToArray();
    }

    private static int[] PartitionUnits(int totalUnits, int[] weights)
    {
        var partitions = new int[weights.Length];
        var remainingExtraUnits = totalUnits - weights.Length;
        var remainingWeight = weights.Sum();

        for (var index = 0; index < weights.Length; index++)
        {
            var extraUnits = index == weights.Length - 1
                ? remainingExtraUnits
                : (int)((long)remainingExtraUnits * weights[index] / remainingWeight);

            partitions[index] = extraUnits;
            remainingExtraUnits -= extraUnits;
            remainingWeight -= weights[index];
        }

        return partitions;
    }

    private static Guid CreateGuid(int value) => new(value, 0, 0, new byte[8]);

    private static TransactionSpec ReplaceEntry(
        TransactionSpec transaction,
        int entryIndex,
        EntrySpec replacement)
    {
        var entries = transaction.Entries.ToArray();
        entries[entryIndex] = replacement;
        return transaction with { Entries = entries };
    }

    private static string NextCurrency(string currency)
    {
        var currencyIndex = Array.IndexOf(_currencies, currency);
        return _currencies[(currencyIndex + 1) % _currencies.Length];
    }
}
