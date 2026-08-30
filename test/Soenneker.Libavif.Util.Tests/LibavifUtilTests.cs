using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Soenneker.Libavif.Util.Abstract;
using Soenneker.Libavif.Util.Commands;
using Soenneker.Libavif.Util.Commands.Abstract;
using Soenneker.Libavif.Util.Options;
using Soenneker.Tests.HostedUnit;

namespace Soenneker.Libavif.Util.Tests;

[ClassDataSource<Host>(Shared = SharedType.PerTestSession)]
public sealed class LibavifUtilTests : HostedUnitTest
{
    private readonly ILibavifUtil _util;

    public LibavifUtilTests(Host host) : base(host)
    {
        _util = Resolve<ILibavifUtil>(true);
    }

    [Test]
    public async Task Resolves()
    {
        await Assert.That(_util).IsNotNull();
    }

    [Test]
    public async Task Rejects_invalid_speed()
    {
        var options = new AvifEncodeOptions {Speed = 11};
        await Assert.That(options.Validate).Throws<ArgumentOutOfRangeException>();
    }

    [Test]
    public async Task Builds_structured_command_with_quoted_paths()
    {
        var avifCommand = new AvifCommand();
        avifCommand.AddFlag("progressive")
                   .AddOption("speed", 6)
                   .AddArgument(@"C:\source images\input.png")
                   .AddArgument(@"C:\output images\result.avif");
        string command = avifCommand.ToString();

        await Assert.That(command)
                    .IsEqualTo("--progressive --speed 6 \"C:\\source images\\input.png\" \"C:\\output images\\result.avif\"");
    }

    [Test]
    public async Task Rejects_invalid_options_from_custom_commands()
    {
        Action execute = () => _ = _util.Execute(new InvalidCommand());
        await Assert.That(execute).Throws<InvalidOperationException>();
    }

    [Test]
    public async Task Encodes_progressive_avif()
    {
        string directory = Path.Combine(Path.GetTempPath(), $"soenneker-libavif-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        string output = Path.Combine(directory, "output.avif");

        try
        {
            await _util.Encode(Path.Combine(AppContext.BaseDirectory, "icon.png"), output,
                new AvifEncodeOptions {Quality = 70, Speed = 10, Progressive = true});
            await Assert.That(File.Exists(output)).IsTrue();
            await Assert.That(new FileInfo(output).Length).IsGreaterThan(0);
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    private sealed class InvalidCommand : ILibavifCommand
    {
        public IReadOnlyList<string> Arguments { get; } = [];
        public IReadOnlyList<KeyValuePair<string, string?>> Options { get; } = [new("speed --version", "6")];

        public ILibavifCommand AddArgument(object value) => this;
        public ILibavifCommand AddOption(string name, object value) => this;
        public ILibavifCommand AddFlag(string name, bool enabled = true) => this;
    }
}
