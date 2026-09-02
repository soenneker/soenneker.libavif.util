using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Soenneker.Extensions.ValueTask;
using Soenneker.Libavif.Util.Abstract;
using Soenneker.Libavif.Util.Commands;
using Soenneker.Libavif.Util.Commands.Abstract;
using Soenneker.Libavif.Util.Options;
using Soenneker.Utils.Directory.Abstract;
using Soenneker.Utils.File.Abstract;
using Soenneker.Utils.Path.Abstract;
using Soenneker.Utils.Process.Abstract;
using Soenneker.Utils.Paths.Resources.Abstract;
using Soenneker.Utils.Runtime;

namespace Soenneker.Libavif.Util;

public sealed class LibavifUtil : ILibavifUtil
{
    private static readonly HashSet<string> _supportedInputExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".y4m"
    };

    private readonly IDirectoryUtil _directoryUtil;
    private readonly IFileUtil _fileUtil;
    private readonly IPathUtil _pathUtil;
    private readonly IProcessUtil _processUtil;
    private readonly IResourcesPathUtil _resourcesPathUtil;
    private readonly string _encoderRelativePath;

    /// <summary>Creates a libavif utility using the registered process and filesystem services.</summary>
    public LibavifUtil(IProcessUtil processUtil, IDirectoryUtil directoryUtil, IFileUtil fileUtil)
        : this(processUtil, directoryUtil, fileUtil, new Soenneker.Utils.Path.PathUtil(),
            new Soenneker.Utils.Paths.Resources.ResourcesPathUtil(directoryUtil))
    {
    }

    /// <summary>Creates a libavif utility using the registered process, path, and filesystem services.</summary>
    public LibavifUtil(IProcessUtil processUtil, IDirectoryUtil directoryUtil, IFileUtil fileUtil, IPathUtil pathUtil)
        : this(processUtil, directoryUtil, fileUtil, pathUtil, new Soenneker.Utils.Paths.Resources.ResourcesPathUtil(directoryUtil))
    {
    }

    public LibavifUtil(IProcessUtil processUtil, IDirectoryUtil directoryUtil, IFileUtil fileUtil, IPathUtil pathUtil,
        IResourcesPathUtil resourcesPathUtil)
    {
        _processUtil = processUtil;
        _directoryUtil = directoryUtil;
        _fileUtil = fileUtil;
        _pathUtil = pathUtil;
        _resourcesPathUtil = resourcesPathUtil;

        EnsureSupportedPlatform();

        _encoderRelativePath = RuntimeUtil.IsWindows()
            ? Path.Join("win-x64", "libavif", "avifenc.exe")
            : Path.Join("linux-x64", "libavif", "avifenc");
    }

    public async ValueTask<List<string>> Run(string arguments, string? workingDirectory = null, bool log = true,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(arguments);

        string encoderPath = await _resourcesPathUtil.GetResourceFilePath(_encoderRelativePath, cancellationToken)
                                                     .NoSync();

        if (!await _fileUtil.Exists(encoderPath, cancellationToken).NoSync())
            throw new FileNotFoundException("The bundled avifenc executable was not found.", encoderPath);

        EnsureExecutable(encoderPath);
        return await _processUtil.Start(encoderPath, workingDirectory, arguments, log: log, cancellationToken: cancellationToken).NoSync();
    }

    public ValueTask<List<string>> Execute(ILibavifCommand command, string? workingDirectory = null, bool log = true,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        return Run(AvifCommand.Build(command), workingDirectory, log, cancellationToken);
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
        if (!await _fileUtil.Exists(fullInputPath, cancellationToken).NoSync())
            throw new FileNotFoundException("The AVIF input file was not found.", fullInputPath);

        if (!_supportedInputExtensions.Contains(Path.GetExtension(fullInputPath)))
            throw new ArgumentException("AVIF input must be JPEG, PNG, or Y4M.", nameof(inputPath));

        if (!Path.GetExtension(outputPath).Equals(".avif", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("AVIF output must use the .avif extension.", nameof(outputPath));

        options ??= new AvifEncodeOptions();
        options.Validate();

        string fullOutputPath = Path.GetFullPath(outputPath);
        string outputDirectory = Path.GetDirectoryName(fullOutputPath)!;
        await _directoryUtil.Create(outputDirectory, cancellationToken: cancellationToken).NoSync();
        string temporaryOutputPath = await _pathUtil.GetRandomUniqueFilePath(outputDirectory, ".avif", cancellationToken).NoSync();

        try
        {
            ILibavifCommand command = new AvifCommand()
                                           .AddFlag("no-overwrite")
                                           .AddOption("qcolor", options.Quality)
                                           .AddOption("qalpha", options.AlphaQuality ?? options.Quality)
                                           .AddOption("speed", options.Speed)
                                           .AddFlag("lossless", options.Lossless)
                                           .AddFlag("progressive", options.Progressive);

            if (options.StripMetadata)
            {
                command.AddFlag("ignore-exif")
                       .AddFlag("ignore-xmp")
                       .AddFlag("ignore-icc");
            }

            command.AddArgument(fullInputPath)
                   .AddArgument(temporaryOutputPath);

            await Execute(command, outputDirectory, log: false, cancellationToken).NoSync();
            cancellationToken.ThrowIfCancellationRequested();
            await _fileUtil.Move(temporaryOutputPath, fullOutputPath, log: false, cancellationToken).NoSync();
        }
        finally
        {
            await _fileUtil.TryDeleteIfExists(temporaryOutputPath, log: false, CancellationToken.None).NoSync();
        }
    }

    private static void EnsureExecutable(string encoderPath)
    {
        if (!OperatingSystem.IsLinux())
            return;

        File.SetUnixFileMode(encoderPath,
            UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute | UnixFileMode.GroupRead |
            UnixFileMode.GroupExecute | UnixFileMode.OtherRead | UnixFileMode.OtherExecute);
    }

    private static void EnsureSupportedPlatform()
    {
        if (RuntimeInformation.ProcessArchitecture != Architecture.X64 ||
            (!RuntimeUtil.IsLinux() && !RuntimeUtil.IsWindows()))
            throw new PlatformNotSupportedException("Soenneker.Libavif.Util currently supports Linux x64 and Windows x64.");
    }

}
