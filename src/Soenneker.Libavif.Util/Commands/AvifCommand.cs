using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using Soenneker.Libavif.Util.Commands.Abstract;
using Soenneker.Utils.PooledStringBuilders;

namespace Soenneker.Libavif.Util.Commands;

/// <inheritdoc cref="ILibavifCommand" />
public sealed class AvifCommand : ILibavifCommand
{
    private readonly List<string> _arguments = [];
    private readonly List<KeyValuePair<string, string?>> _options = [];
    private readonly ReadOnlyCollection<string> _readOnlyArguments;
    private readonly ReadOnlyCollection<KeyValuePair<string, string?>> _readOnlyOptions;

    public IReadOnlyList<string> Arguments => _readOnlyArguments;

    public IReadOnlyList<KeyValuePair<string, string?>> Options => _readOnlyOptions;

    /// <summary>Creates an empty <c>avifenc</c> command.</summary>
    public AvifCommand()
    {
        _readOnlyArguments = _arguments.AsReadOnly();
        _readOnlyOptions = _options.AsReadOnly();
    }

    public ILibavifCommand AddArgument(object value)
    {
        ArgumentNullException.ThrowIfNull(value);
        _arguments.Add(Format(value));
        return this;
    }

    public ILibavifCommand AddOption(string name, object value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(value);
        ValidateName(name);
        _options.Add(new KeyValuePair<string, string?>(name, Format(value)));
        return this;
    }

    public ILibavifCommand AddFlag(string name, bool enabled = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ValidateName(name);

        if (enabled)
            _options.Add(new KeyValuePair<string, string?>(name, null));

        return this;
    }

    internal static string Build(ILibavifCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        var builder = new PooledStringBuilder(CalculateCapacity(command));

        try
        {
            var hasValue = false;

            foreach (KeyValuePair<string, string?> option in command.Options)
            {
                if (!IsValidName(option.Key))
                    throw new InvalidOperationException($"'{option.Key}' is not a valid avifenc option name.");

                AppendSeparator(ref builder, ref hasValue);
                builder.Append("--");
                builder.Append(option.Key);

                if (option.Value is null)
                    continue;

                AppendSeparator(ref builder, ref hasValue);
                AppendQuoted(ref builder, option.Value);
            }

            foreach (string argument in command.Arguments)
            {
                AppendSeparator(ref builder, ref hasValue);
                AppendQuoted(ref builder, argument);
            }

            return builder.ToString();
        }
        finally
        {
            builder.Dispose();
        }
    }

    public override string ToString() => Build(this);

    private static int CalculateCapacity(ILibavifCommand command)
    {
        var capacity = command.Arguments.Count + command.Options.Count;

        foreach (string argument in command.Arguments)
            capacity += argument.Length + 2;

        foreach (KeyValuePair<string, string?> option in command.Options)
            capacity += option.Key.Length + 2 + (option.Value?.Length ?? 0) + (option.Value is null ? 0 : 3);

        return Math.Max(capacity, 32);
    }

    private static void AppendSeparator(ref PooledStringBuilder builder, ref bool hasValue)
    {
        if (hasValue)
            builder.Append(' ');
        else
            hasValue = true;
    }

    private static void AppendQuoted(ref PooledStringBuilder builder, string value)
    {
        bool requiresQuotes = value.Length == 0;

        for (var index = 0; index < value.Length && !requiresQuotes; index++)
            requiresQuotes = char.IsWhiteSpace(value[index]) || value[index] == '"';

        if (!requiresQuotes)
        {
            builder.Append(value);
            return;
        }

        builder.Append('"');
        var backslashCount = 0;

        foreach (char character in value)
        {
            if (character == '\\')
            {
                backslashCount++;
                continue;
            }

            if (character == '"')
            {
                builder.Append('\\', (backslashCount * 2) + 1);
                builder.Append('"');
                backslashCount = 0;
                continue;
            }

            builder.Append('\\', backslashCount);
            builder.Append(character);
            backslashCount = 0;
        }

        builder.Append('\\', backslashCount * 2);
        builder.Append('"');
    }

    private static string Format(object value) => value switch
    {
        bool boolean => boolean ? "true" : "false",
        IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
        _ => value.ToString() ?? string.Empty
    };

    private static void ValidateName(string value)
    {
        if (!char.IsAsciiLetter(value[0]))
            throw new ArgumentException("Option names must begin with an ASCII letter.", nameof(value));

        if (!IsValidName(value))
            throw new ArgumentException("Option names may contain only ASCII letters, digits, hyphens, and underscores.", nameof(value));
    }

    private static bool IsValidName(string? value)
    {
        if (string.IsNullOrEmpty(value) || !char.IsAsciiLetter(value[0]))
            return false;

        foreach (char character in value)
        {
            if (!char.IsAsciiLetterOrDigit(character) && character is not '-' and not '_')
                return false;
        }

        return true;
    }
}
