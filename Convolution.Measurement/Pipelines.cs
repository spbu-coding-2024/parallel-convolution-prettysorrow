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

    [Params(3, 10)]
    public int ImageCount { get; set; }

    [Params(32, 128)]
    public int ImageSize { get; set; }

    [Params(5, 13)]
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
        => Impl.Pipeline.ProcessSequentialSync(this.inputPaths, MakeOutputPath, this.filter);

    [Benchmark]
    public async Task Async_Sequential()
        => await Impl.Pipeline.ProcessSequentialAsync(this.inputPaths, MakeOutputPath, this.filter);

    [Benchmark]
    public void Sync_Parallel()
        => Impl.Pipeline.ProcessParallelSync(this.inputPaths, MakeOutputPath, this.filter);

    [Benchmark]
    public async Task Async_Parallel()
        => await Impl.Pipeline.ProcessParallelAsync(this.inputPaths, MakeOutputPath, this.filter);

    [Benchmark]
    public async Task Sync_Unsafe()
        => Impl.Pipeline.ProccessUnsafeSync(this.inputPaths, MakeOutputPath, this.filter);

    [Benchmark]
    public async Task Async_Unsafe()
        => await Impl.Pipeline.ProccessUnsafeAsync(this.inputPaths, MakeOutputPath, this.filter);
}
