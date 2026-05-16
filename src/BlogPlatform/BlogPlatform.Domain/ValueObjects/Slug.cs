using System.Text;

namespace BlogPlatform.Domain.ValueObjects;

public sealed record Slug
{
    private Slug(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Slug Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Slug value cannot be empty.");
        }

        var normalized = value.Trim().ToLowerInvariant();
        var builder = new StringBuilder();
        var lastWasDash = false;

        foreach (var character in normalized)
        {
            if (char.IsLetterOrDigit(character))
            {
                builder.Append(character);
                lastWasDash = false;
                continue;
            }

            if (!lastWasDash)
            {
                builder.Append('-');
                lastWasDash = true;
            }
        }

        var slug = builder.ToString().Trim('-');

        if (string.IsNullOrWhiteSpace(slug))
        {
            throw new ArgumentException("Slug value cannot be empty.");
        }

        return new Slug(slug);
    }

    public bool EqualsValue(string? other)
    {
        return string.Equals(
            Value,
            other?.Trim(),
            StringComparison.OrdinalIgnoreCase);
    }

    public override string ToString()
    {
        return Value;
    }
}
