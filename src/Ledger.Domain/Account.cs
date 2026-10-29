namespace Ledger.Domain;

public sealed class Account
{
    public Guid Id { get; }
    public string Name { get; }
    public string Currency { get; }
    public AccountType Type { get; }
    public AccountStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; }

    public Account(Guid id, string name, string currency, AccountType type, DateTimeOffset createdAt)
    {

        if (id == Guid.Empty)
        {
            throw new ArgumentException("Account ID cannot be empty.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Account name cannot be null or whitespace.", nameof(name));
        }

        if (!Enum.IsDefined(type))
        {
            throw new ArgumentOutOfRangeException(nameof(type), type, "Invalid account type.");
        }

        Id = id;
        Name = name.Trim();
        Currency = CurrencyCode.Normalize(currency);
        Type = type;
        Status = AccountStatus.Active; // Default status is Active
        CreatedAt = createdAt;
    }
}
