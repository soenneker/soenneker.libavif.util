using System.Collections.Generic;

namespace Soenneker.Libavif.Util.Commands.Abstract;

/// <summary>Describes an <c>avifenc</c> command using positional arguments, options, and flags.</summary>
public interface ILibavifCommand
{
    /// <summary>Gets the positional command arguments.</summary>
    IReadOnlyList<string> Arguments { get; }

    /// <summary>Gets option names paired with optional values. A <see langword="null"/> value represents a flag.</summary>
    IReadOnlyList<KeyValuePair<string, string?>> Options { get; }

    /// <summary>
    /// Adds a positional argument.
    /// </summary>
    /// <param name="value">Argument or option value to append to the command.</param>
    /// <returns>The same builder instance, so additional classes or variants can be chained.</returns>
    ILibavifCommand AddArgument(object value);

    /// <summary>
    /// Adds a named option and value.
    /// </summary>
    /// <param name="name">Name of the Libavif Command value to target.</param>
    /// <param name="value">Argument or option value to append to the command.</param>
    /// <returns>The same builder instance, so additional classes or variants can be chained.</returns>
    ILibavifCommand AddOption(string name, object value);

    /// <summary>
    /// Adds a named flag when <paramref name="enabled"/> is <see langword="true"/>.
    /// </summary>
    /// <param name="name">Name of the Libavif Command value to target.</param>
    /// <param name="enabled">Whether enabled.</param>
    /// <returns>The same builder instance, so additional classes or variants can be chained.</returns>
    ILibavifCommand AddFlag(string name, bool enabled = true);
}
