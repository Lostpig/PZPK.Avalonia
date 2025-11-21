namespace PZPK.Core.Packing;

public record PackingOptions
{
    public string Password { get; init; }
    public string Name { get; init; }
    public string Description { get; init; }
    public string[] Tags { get; init; }
    public int BlockSize { get; init; }
    public PZType Type { get; init; }

    public PackingOptions(string password, int blockSize, PZType type, string name, string? description, string[]? tags)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be null or empty.", nameof(password));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be null or empty.", nameof(name));
        if (blockSize % (64 * 1024) != 0)
            throw new ArgumentException("Block size must be a multiple of 64KB.", nameof(blockSize));

        Password = password;
        BlockSize = blockSize;
        Type = type;
        Name = name;
        Description = description ?? "";
        Tags = tags ?? [];
    }
}
