using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Soenneker.Extensions.ValueTask;
using Soenneker.Libavif.Util.Abstract;
using Soenneker.Libavif.Util.Options;
using Soenneker.Utils.Process.Abstract;

namespace Soenneker.Libavif.Util;

/// <inheritdoc cref="ILibavifUtil"/>
public sealed class LibavifUtil : ILibavifUtil
{
    private static readonly HashSet<string> _supportedInputExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".y4m"
    };

    private readonly IProcessUtil _processUtil;
    private readonly string _encoderPath;

    public LibavifUtil(IProcessUtil processUtil)
    {
        _processUtil = processUtil ?? throw new ArgumentNullException(nameof(processUtil));

        if (RuntimeInformation.ProcessArchitecture != Architecture.X64 || (!OperatingSystem.IsWindows() && !OperatingSystem.IsLinux()))
            throw new PlatformNotSupportedException("Soenneker.Libavif.Util supports Windows x64 and Linux x64.");

        _encoderPath = OperatingSystem.IsWindows()
            ? Path.Join(AppContext.BaseDirectory, "Resources", "win-x64", "libavif", "avifenc.exe")
            : Path.Join(AppContext.BaseDirectory, "Resources", "linux-x64", "libavif", "avifenc");
    }

    public ValueTask<List<string>> Run(string arguments, string? workingDirectory = null, bool log = true,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(arguments);
        EnsureEncoderExists();
        EnsureExecutable();
        return _processUtil.Start(_encoderPath, workingDirectory, arguments, log: log, cancellationToken: cancellationToken);
    }

    public async ValueTask<string> GetVersion(CancellationToken cancellationToken = default)
    {
        List<string> output = await Run("--version", log: false, cancellationToken: cancellationToken).NoSync();
        return output.Count == 0 ? string.Empty : output[0];
    }

    public async ValueTask Encode(string inputPath, string outputPath, AvifEncodeOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(inputPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);

        string fullInputPath = Path.GetFullPath(inputPath);
        if (!File.Exists(fullInputPath))
            throw new FileNotFoundException("The AVIF input file was not found.", fullInputPath);

        if (!_supportedInputExtensions.Contains(Path.GetExtension(fullInputPath)))
            throw new ArgumentException("AVIF input must be JPEG, PNG, or Y4M.", nameof(inputPath));

        if (!Path.GetExtension(outputPath).Equals(".avif", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("AVIF output must use the .avif extension.", nameof(outputPath));

        options ??= new AvifEncodeOptions();
        options.Validate();

        string fullOutputPath = Path.GetFullPath(outputPath);
        string outputDirectory = Path.GetDirectoryName(fullOutputPath)!;
        Directory.CreateDirectory(outputDirectory);
        string temporaryOutputPath = Path.Combine(outputDirectory, $".{Path.GetFileNameWithoutExtension(fullOutputPath)}.{Guid.NewGuid():N}.avif");

        try
        {
            var arguments = new List<string>
            {
                "--no-overwrite",
                "--qcolor", options.Quality.ToString(CultureInfo.InvariantCulture),
                "--qalpha", (options.AlphaQuality ?? options.Quality).ToString(CultureInfo.InvariantCulture),
                "--speed", options.Speed.ToString(CultureInfo.InvariantCulture)
            };

            if (options.Lossless)
                arguments.Add("--lossless");
            if (options.Progressive)
                arguments.Add("--progressive");
            if (options.StripMetadata)
            {
                arguments.Add("--ignore-exif");
                arguments.Add("--ignore-xmp");
                arguments.Add("--ignore-icc");
            }

            arguments.Add(fullInputPath);
            arguments.Add(temporaryOutputPath);

            await Run(BuildArgumentString(arguments), outputDirectory, log: false, cancellationToken).NoSync();
            cancellationToken.ThrowIfCancellationRequested();
            File.Move(temporaryOutputPath, fullOutputPath, true);
        }
        finally
        {
            File.Delete(temporaryOutputPath);
        }
    }

    private void EnsureEncoderExists()
    {
        if (!File.Exists(_encoderPath))
            throw new FileNotFoundException("The bundled avifenc executable was not found.", _encoderPath);
    }

    private void EnsureExecutable()
    {
        if (!OperatingSystem.IsLinux())
            return;

        File.SetUnixFileMode(_encoderPath,
            UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute | UnixFileMode.GroupRead |
            UnixFileMode.GroupExecute | UnixFileMode.OtherRead | UnixFileMode.OtherExecute);
    }

    private static string BuildArgumentString(IReadOnlyList<string> arguments)
    {
        var builder = new StringBuilder();

        for (var index = 0; index < arguments.Count; index++)
        {
            if (index > 0)
                builder.Append(' ');

            builder.Append(Quote(arguments[index]));
        }

        return builder.ToString();
    }

    private static string Quote(string value)
    {
        bool requiresQuotes = value.Length == 0;
        for (var index = 0; index < value.Length && !requiresQuotes; index++)
            requiresQuotes = char.IsWhiteSpace(value[index]) || value[index] == '"';

        if (!requiresQuotes)
            return value;

        var builder = new StringBuilder(value.Length + 2).Append('"');
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
                builder.Append('\\', (backslashCount * 2) + 1).Append('"');
                backslashCount = 0;
                continue;
            }

            builder.Append('\\', backslashCount).Append(character);
            backslashCount = 0;
        }

        builder.Append('\\', backslashCount * 2).Append('"');
        return builder.ToString();
    }
}
