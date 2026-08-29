using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Soenneker.Libavif.Util.Commands.Abstract;
using Soenneker.Libavif.Util.Options;

namespace Soenneker.Libavif.Util.Abstract;

/// <summary>A structured, cross-platform API for the bundled libavif command-line tools.</summary>
public interface ILibavifUtil
{
    /// <summary>
    /// Runs <c>avifenc</c> with raw command-line arguments.
    /// </summary>
    /// <param name="arguments">Arguments for the run operation.</param>
    /// <param name="workingDirectory">Working Directory for the run operation.</param>
    /// <param name="log">Whether log.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A task whose result is the collection returned by run.</returns>
    ValueTask<List<string>> Run(string arguments, string? workingDirectory = null, bool log = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a structured <c>avifenc</c> command.
    /// </summary>
    /// <param name="command">Command for the execute operation.</param>
    /// <param name="workingDirectory">Working Directory for the execute operation.</param>
    /// <param name="log">Whether log.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A task whose result is the collection returned by execute.</returns>
    ValueTask<List<string>> Execute(ILibavifCommand command, string? workingDirectory = null, bool log = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the bundled <c>avifenc</c> version.
    /// </summary>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A task whose result is the text returned by get Version.</returns>
    ValueTask<string> GetVersion(CancellationToken cancellationToken = default);

    /// <summary>
    /// Encodes a JPEG, PNG, or Y4M input as AVIF.
    /// </summary>
    /// <param name="inputPath">Path of the input to use.</param>
    /// <param name="outputPath">Path of the output to use.</param>
    /// <param name="options">Options to configure for the libavif.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A task that completes when the encode operation is complete.</returns>
    ValueTask Encode(string inputPath, string outputPath, AvifEncodeOptions? options = null,
        CancellationToken cancellationToken = default);
}
