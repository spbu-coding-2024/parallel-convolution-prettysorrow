namespace Convolution.Measurement;

using BenchmarkDotNet.Attributes;
using Convolution.Core;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

[MarkdownExporter]
[CsvExporter]
[SimpleJob(warmupCount: 3, iterationCount: 5)]
[Config(typeof(BenchmarkConfig))]
public class Unsafe
{
    private static readonly Func<string, string> MakeOutputPath = path => Path.ChangeExtension(path, ".conv.png");

    [Params(3, 15)]
    public int ImageCount { get; set; }

    [Params(32, 512)]
    public int ImageSize { get; set; }

    [Params(3, 13)]
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

    public async Task Run_Single(int imageLevelWorkers, (int read, int convolve, int write) workerCount, (int paths, int raw, int convolved) channelCapacity)
    {
        var convolutionOptions = new ParallelOptions { MaxDegreeOfParallelism = imageLevelWorkers };
        Func<Image<RgbaVector>, Image<RgbaVector>> convolve = image => Convolution.Impl.Unsafe.Apply(this.filter, image, convolutionOptions);
        var pipelineOptions = new Convolution.Impl.AsyncPipelineOptions(convolve, workerCount, channelCapacity);
        await Convolution.Impl.Pipeline.ProcessAsync(this.inputPaths, MakeOutputPath, pipelineOptions);
    }

    [Benchmark(Baseline = true)]
    public async Task Unsafe_MinEverything()
    {
        (int read, int convolve, int write) workerCount = (1, 1, 1);
        (int paths, int raw, int convolved) channelCapacity = (1, 1, 1);
        var imageLevelWorkers = 1;
        await this.Run_Single(imageLevelWorkers, workerCount, channelCapacity);
    }

    [Benchmark]
    public async Task Unsafe_MaxEverything()
    {
        int pc = Environment.ProcessorCount;
        (int read, int convolve, int write) workerCount = (pc, pc, pc);
        (int paths, int raw, int convolved) channelCapacity = (this.ImageCount, this.ImageCount, this.ImageCount);
        var imageLevelWorkers = pc;
        await this.Run_Single(imageLevelWorkers, workerCount, channelCapacity);
    }

    private static (int read, int convolve, int write) MakeWorkerCount(int helpers, int imageLevelWorkers)
    {
        if ((helpers < 2) || (helpers % 2 != 0) || (imageLevelWorkers < 1))
        {
            throw new SkipBenchmarkException("invalid params");
        }

        int pc = Environment.ProcessorCount;

        if (pc < helpers + 1)
        {
            throw new SkipBenchmarkException("not enough processors for this config");
        }

        var readers = helpers / 2;
        var convolvers = (int)Math.Ceiling((pc - helpers) / ((float)imageLevelWorkers));
        var writers = helpers / 2;

        if ((convolvers < 1) || (readers + (imageLevelWorkers * convolvers) + writers < pc))
        {
            throw new SkipBenchmarkException("assertion failed");
        }

        return (readers, convolvers, writers);
    }

    [Benchmark]
    public async Task Unsafe_2Helpers_1ForImage()
    {
        var helpers = 2;
        var imageLevelWorkers = 1;
        var workerCount = MakeWorkerCount(helpers, imageLevelWorkers);
        (int paths, int raw, int convolved) channelCapacity = (this.ImageCount, this.ImageCount, this.ImageCount);
        await this.Run_Single(imageLevelWorkers, workerCount, channelCapacity);
    }

    [Benchmark]
    public async Task Unsafe_2Helpers_4ForImage()
    {
        var helpers = 2;
        var imageLevelWorkers = 4;
        var workerCount = MakeWorkerCount(helpers, imageLevelWorkers);
        (int paths, int raw, int convolved) channelCapacity = (this.ImageCount, this.ImageCount, this.ImageCount);
        await this.Run_Single(imageLevelWorkers, workerCount, channelCapacity);
    }

    [Benchmark]
    public async Task Unsafe_2Helpers_8ForImage()
    {
        var helpers = 2;
        var imageLevelWorkers = 8;
        var workerCount = MakeWorkerCount(helpers, imageLevelWorkers);
        (int paths, int raw, int convolved) channelCapacity = (this.ImageCount, this.ImageCount, this.ImageCount);
        await this.Run_Single(imageLevelWorkers, workerCount, channelCapacity);
    }

    [Benchmark]
    public async Task Unsafe_4Helpers_1ForImage()
    {
        var helpers = 4;
        var imageLevelWorkers = 1;
        var workerCount = MakeWorkerCount(helpers, imageLevelWorkers);
        (int paths, int raw, int convolved) channelCapacity = (this.ImageCount, this.ImageCount, this.ImageCount);
        await this.Run_Single(imageLevelWorkers, workerCount, channelCapacity);
    }

    [Benchmark]
    public async Task Unsafe_4Helpers_4ForImage()
    {
        var helpers = 4;
        var imageLevelWorkers = 4;
        var workerCount = MakeWorkerCount(helpers, imageLevelWorkers);
        (int paths, int raw, int convolved) channelCapacity = (this.ImageCount, this.ImageCount, this.ImageCount);
        await this.Run_Single(imageLevelWorkers, workerCount, channelCapacity);
    }

    [Benchmark]
    public async Task Unsafe_4Helpers_8ForImage()
    {
        var helpers = 4;
        var imageLevelWorkers = 8;
        var workerCount = MakeWorkerCount(helpers, imageLevelWorkers);
        (int paths, int raw, int convolved) channelCapacity = (this.ImageCount, this.ImageCount, this.ImageCount);
        await this.Run_Single(imageLevelWorkers, workerCount, channelCapacity);
    }
}
