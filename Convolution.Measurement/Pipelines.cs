namespace Convolution.Measurement;

using BenchmarkDotNet.Attributes;
using Convolution.Core;

[MarkdownExporter]
[CsvExporter]
[SimpleJob(warmupCount: 3, iterationCount: 5)]
[Config(typeof(BenchmarkConfig))]
public class Pipelines
{
    private static readonly Func<string, string> MakeOutputPath = path => Path.ChangeExtension(path, ".conv.png");

    [Params(3, 5)]
    public int ImageCount { get; set; }

    [Params(16, 32)]
    public int ImageSize { get; set; }

    [Params(3, 5)]
    public int FilterSize { get; set; }

    private readonly ImageGenerator imageGenerator = new(seed: 42);

    private readonly FilterGenerator filterGenerator = new(seed: 42);

    private string testDir = null!;

    private List<string> inputPaths = null!;

    private Filter filter = null!;

    [GlobalSetup]
    public void Setup()
    {
        this.testDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(this.testDir);
        this.inputPaths = this.imageGenerator.WriteRandomImages(this.testDir, count: this.ImageCount, width: this.ImageSize, height: this.ImageSize);
        this.filter = this.filterGenerator.Next(size: this.FilterSize);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        if (Directory.Exists(this.testDir))
        {
            Directory.Delete(this.testDir, recursive: true);
        }
    }

    [Benchmark(Baseline = true)]
    public void Sync_Sequential()
        => Convolution.Impl.Pipeline.ProcessSequentialSync(this.inputPaths, MakeOutputPath, this.filter);

    [Benchmark]
    public async Task Async_Sequential()
        => await Convolution.Impl.Pipeline.ProcessSequentialAsync(this.inputPaths, MakeOutputPath, this.filter);

    [Benchmark]
    public void Sync_Parallel()
        => Convolution.Impl.Pipeline.ProcessParallelSync(this.inputPaths, MakeOutputPath, this.filter);

    [Benchmark]
    public async Task Async_Parallel()
        => await Convolution.Impl.Pipeline.ProcessParallelAsync(this.inputPaths, MakeOutputPath, this.filter);

    [Benchmark]
    public async Task Sync_Unsafe()
        => Convolution.Impl.Pipeline.ProccessUnsafeSync(this.inputPaths, MakeOutputPath, this.filter);

    [Benchmark]
    public async Task Async_Unsafe()
        => await Convolution.Impl.Pipeline.ProcessUnsafeAsync(this.inputPaths, MakeOutputPath, this.filter);
}
