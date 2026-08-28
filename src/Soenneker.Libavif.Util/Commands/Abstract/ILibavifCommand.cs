using System.Collections.Generic;

namespace Soenneker.Libavif.Util.Commands.Abstract;

/// <summary>Describes an <c>avifenc</c> command using positional arguments, options, and flags.</summary>
public interface ILibavifCommand
{
    /// <summary>Gets the positional command arguments.</summary>
    IReadOnlyList<string> Arguments { get; }

    /// <summary>Gets option names paired with optional values. A <see langword="null"/> value represents a flag.</summary>
    IReadOnlyList<KeyValuePair<string, string?>> Options { get; }

    /// <summary>Adds a positional argument.</summary>
    ILibavifCommand AddArgument(object value);

    /// <summary>Adds a named option and value.</summary>
    ILibavifCommand AddOption(string name, object value);

    /// <summary>Adds a named flag when <paramref name="enabled"/> is <see langword="true"/>.</summary>
    ILibavifCommand AddFlag(string name, bool enabled = true);
}
