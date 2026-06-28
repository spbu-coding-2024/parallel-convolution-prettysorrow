namespace Convolution.Tests;

using Convolution.Core;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;

/// <summary>
/// Asserts that exceptions are thrown when they should be.
/// </summary>
public class EdgeCases
{
    [Fact]
    public void FilterConstructor_NonSquareKernel_ThrowsArgumentException()
    {
        var kernel3x4 = new float[3, 5];
        Assert.Throws<ArgumentException>(() => new Filter(kernel3x4));
    }

    [Fact]
    public void FilterConstructor_EvenSizedKernel_ThrowsArgumentException()
    {
        var kernel2x2 = new float[2, 2];
        Assert.Throws<ArgumentException>(() => new Filter(kernel2x2));
    }

    [Fact]
    public void FilterConstructor_SquareOddSizedKernel_DoesNotThrow()
    {
        var kernel1x1 = new float[1, 1];
        var kernel3x3 = new float[3, 3];
        var kernel5x5 = new float[5, 5];

        Assert.NotNull(new Filter(kernel1x1));
        Assert.NotNull(new Filter(kernel3x3));
        Assert.NotNull(new Filter(kernel5x5));
    }

    [Fact]
    public void Compose_DifferentEdgeModes_ThrowsArgumentException()
    {
        var kernel = new float[3, 3];

        var clampFilter = new Filter(kernel, edgeMode: EdgeMode.Clamp);
        var wrapFilter = new Filter(kernel, edgeMode: EdgeMode.Wrap);

        Assert.Throws<ArgumentException>(() => clampFilter.Compose(wrapFilter));
        Assert.Throws<ArgumentException>(() => wrapFilter.Compose(clampFilter));
    }

    [Fact]
    public void Compose_SameEdgeModes_DoesNotThrow()
    {
        var kernel3x3 = new float[3, 3];
        var kernel5x5 = new float[5, 5];
        var clampFilter3x3 = new Filter(kernel3x3, edgeMode: EdgeMode.Clamp);
        var clampFilter5x5 = new Filter(kernel5x5, edgeMode: EdgeMode.Clamp);
        var wrapFilter3x3 = new Filter(kernel3x3, edgeMode: EdgeMode.Wrap);
        var wrapFilter5x5 = new Filter(kernel5x5, edgeMode: EdgeMode.Wrap);

        Assert.NotNull(clampFilter3x3.Compose(clampFilter5x5));
        Assert.NotNull(wrapFilter5x5.Compose(wrapFilter3x3));
    }

    [Fact]
    public void Pad_NegativeOrZeroPadding_ThrowsArgumentOutOfRangeException()
    {
        var filter = Filters.Identity;
        Assert.Throws<ArgumentOutOfRangeException>(() => filter.Pad(padding: 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => filter.Pad(padding: -1));
    }

    [Fact]
    public void Pad_PozitivePadding_DoesNotThrows()
    {
        var filter = Filters.Identity;

        Assert.NotNull(filter.Pad(padding: 1));
        Assert.NotNull(filter.Pad(padding: 5));
        Assert.NotNull(filter.Pad(padding: 50));
    }

    [Fact]
    public void FilterGenerator_EvenKernelSize_ThrowsArgumentOutOfRangeException()
    {
        var generator = new FilterGenerator(null);

        Assert.Throws<ArgumentOutOfRangeException>(() => generator.Next(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => generator.Next(2));
        Assert.Throws<ArgumentOutOfRangeException>(() => generator.Next(16));
    }

    [Fact]
    public void FilterGenerator_NegativeKernelSize_ThrowsArgumentOutOfRangeException()
    {
        var generator = new FilterGenerator();

        Assert.Throws<ArgumentOutOfRangeException>(() => generator.Next(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => generator.Next(-2));
        Assert.Throws<ArgumentOutOfRangeException>(() => generator.Next(-15));
    }

    [Fact]
    public void FilterGenerator_PositiveOddKernelSize_DoesNotThrow()
    {
        var generator = new FilterGenerator();

        Assert.NotNull(() => generator.Next(1));
        Assert.NotNull(() => generator.Next(5));
        Assert.NotNull(() => generator.Next(111));
    }

    [Fact]
    public void ImageGenerator_NegativeOrZeroImageSize_ThrowsArgumentOutOfRangeException()
    {
        var generator = new ImageGenerator();

        Assert.Throws<ArgumentOutOfRangeException>(() => generator.Next(width: -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => generator.Next(width: 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => generator.Next(width: -1920));

        Assert.Throws<ArgumentOutOfRangeException>(() => generator.Next(height: -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => generator.Next(height: 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => generator.Next(height: -1080));
    }

    [Fact]
    public void ImageGenerator_PositiveImageSize_DoesNotThrow()
    {
        var generator = new ImageGenerator();

        Assert.NotNull(generator.Next());
        Assert.NotNull(generator.Next(width: 100));
        Assert.NotNull(generator.Next(height: 100));
        Assert.NotNull(generator.Next(width: 100, height: 100));
    }

    [Fact]
    public void ImageGenerator_NegativeShapeCount_ThrowsArgumentOutOfRangeException()
    {
        var generator = new ImageGenerator();

        Assert.Throws<ArgumentOutOfRangeException>(() => generator.Next(shapeCount: -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => generator.Next(shapeCount: -42));
    }

    [Fact]
    public void ImageGenerator_PositiveOrZeroShapeCount_DoesNotThrow()
    {
        var generator = new ImageGenerator();

        Assert.NotNull(generator.Next(shapeCount: 0));
        Assert.NotNull(generator.Next(shapeCount: 42));
    }

    [Fact]
    public void AsyncPipelineOptions_NegativeOrZeroCounters_ThrowsArgumentOutOfRangeException()
    {
        Func<Image<RgbaVector>, Image<RgbaVector>> stub = image => image;
        Assert.Throws<ArgumentOutOfRangeException>(() => new Convolution.Impl.AsyncPipelineOptions(stub, (0, 1, 1), (1, 1, 1)));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Convolution.Impl.AsyncPipelineOptions(stub, (-1, 1, 1), (1, 1, 1)));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Convolution.Impl.AsyncPipelineOptions(stub, (1, 0, 1), (1, 1, 1)));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Convolution.Impl.AsyncPipelineOptions(stub, (1, -1, 1), (1, 1, 1)));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Convolution.Impl.AsyncPipelineOptions(stub, (1, 1, 0), (1, 1, 1)));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Convolution.Impl.AsyncPipelineOptions(stub, (1, 1, -1), (1, 1, 1)));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Convolution.Impl.AsyncPipelineOptions(stub, (1, 1, 1), (0, 1, 1)));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Convolution.Impl.AsyncPipelineOptions(stub, (1, 1, 1), (-1, 1, 1)));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Convolution.Impl.AsyncPipelineOptions(stub, (1, 1, 1), (1, 0, 1)));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Convolution.Impl.AsyncPipelineOptions(stub, (1, 1, 1), (1, -1, 1)));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Convolution.Impl.AsyncPipelineOptions(stub, (1, 1, 1), (1, 1, 0)));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Convolution.Impl.AsyncPipelineOptions(stub, (1, 1, 1), (1, 1, -1)));
    }

    [Fact]
    public void AsyncPipelineOptions_PositiveCounters_DoesNotThrow()
    {
        Func<Image<RgbaVector>, Image<RgbaVector>> stub = image => image;
        Assert.NotNull(new Convolution.Impl.AsyncPipelineOptions(stub, (1, 20, 300), (4000, 50000, 600000)));
    }

    [Fact]
    public void Parallel_ApplyTiles_NegativeOrZeroTileSize_ThrowsArgumentOutOfRangeException()
    {
        var filter = FilterGenerator.Shared.Next();
        var image = ImageGenerator.Shared.Next();

        Assert.Throws<ArgumentOutOfRangeException>(() => Convolution.Impl.Parallel.ApplyTiles(filter, image, tileSize: -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => Convolution.Impl.Parallel.ApplyTiles(filter, image, tileSize: -5));
        Assert.Throws<ArgumentOutOfRangeException>(() => Convolution.Impl.Parallel.ApplyTiles(filter, image, tileSize: 0));
    }

    [Fact]
    public void Parallel_ApplyTiles_PositiveTileSize_DoesNotThrow()
    {
        var filter = FilterGenerator.Shared.Next();
        var image = ImageGenerator.Shared.Next();

        Assert.NotNull(Convolution.Impl.Parallel.ApplyTiles(filter, image, tileSize: 1));
        Assert.NotNull(Convolution.Impl.Parallel.ApplyTiles(filter, image, tileSize: 16));
        Assert.NotNull(Convolution.Impl.Parallel.ApplyTiles(filter, image, tileSize: 144));
    }
}
