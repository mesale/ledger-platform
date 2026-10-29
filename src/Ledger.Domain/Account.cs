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
        Id = id;
        Name = name;
        Currency = currency;
        Type = type;
        Status = AccountStatus.Active; // Default status is Active
        CreatedAt = createdAt;
    }
}
