namespace Convolution.Measurement;

using BenchmarkDotNet.Attributes;
using Convolution.Core;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

/// <summary>
/// Benchmark which runs on random generated images and compares different convolution implementations.
/// </summary>
[MarkdownExporter]
[CsvExporter]
[SimpleJob(warmupCount: 3, iterationCount: 10)]
[Config(typeof(BenchmarkConfig))]
public class RandomBenchmark
{
    [Params(128, 256, 512)]
    public int ImageSize { get; set; }

    [Params(5, 13, 32)]
    public int FilterSize { get; set; }

    private readonly ImageGenerator imageGenerator = new(seed: 42);
    private readonly FilterGenerator filterGenerator = new(seed: 42);
    private Image<RgbaVector> sourceImage = null!;
    private Filter filter = null!;

    [GlobalSetup]
    public void Setup()
    {
        this.sourceImage = this.imageGenerator.Next(width: this.ImageSize, height: this.ImageSize);
        this.filter = this.filterGenerator.Next(size: this.FilterSize);
    }

    [GlobalCleanup]
    public void Cleanup()
        => this.sourceImage.Dispose();

    [Benchmark(Baseline = true)]
    public Image<RgbaVector> Sequential()
        => Impl.Sequential.Apply(this.filter, this.sourceImage);

    [Benchmark]
    public Image<RgbaVector> Parallel_Rows()
        => Impl.Parallel.ApplyRows(this.filter, this.sourceImage);

    [Benchmark]
    public Image<RgbaVector> Parallel_Columns()
        => Impl.Parallel.ApplyColumns(this.filter, this.sourceImage);

    [Benchmark]
    public Image<RgbaVector> Parallel_Tiles_Size8()
        => Impl.Parallel.ApplyTiles(this.filter, this.sourceImage, tileSize: 8);

    [Benchmark]
    public Image<RgbaVector> Parallel_Tiles_Size32()
   => Impl.Parallel.ApplyTiles(this.filter, this.sourceImage, tileSize: 32);

    [Benchmark]
    public Image<RgbaVector> Parallel_Tiles_Size128()
   => Impl.Parallel.ApplyTiles(this.filter, this.sourceImage, tileSize: 128);
}
