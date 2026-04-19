namespace Convolution.Tests;

using Convolution.Core;
using Convolution.Extensions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;

public class CompareSequentialParallel
{
    private static readonly ImageGenerator ImageGenerator = new();
    private static readonly FilterGenerator FilterGenerator = new();

    private static void RunSingle(Image<RgbaVector> image, Filter filter)
    {
        using var parallelResult = Impl.Parallel.Apply(image, filter);
        using var sequentialResult = Impl.Sequential.Apply(image, filter);
        Assert.True(parallelResult.Equal(sequentialResult));
    }

    [Fact]
    public void Apply_IdentityFilter()
    {
        using var input = ImageGenerator.Next();
        var filter = Filters.Identity;
        RunSingle(input, filter);
    }

    [Fact]
    public void Apply_BoxBlur()
    {
        using var input = ImageGenerator.Next();
        var filter = Filters.BoxBlur(radius: 1);
        RunSingle(input, filter);
    }

    [Fact]
    public void Apply_GaussianBlur()
    {
        using var input = ImageGenerator.Next();
        var filter = Filters.GaussianBlur(sigma: 0.5);
        RunSingle(input, filter);
    }

    [Fact]
    public void Apply_SobelX()
    {
        using var input = ImageGenerator.Next();
        var filter = Filters.SobelX;
        RunSingle(input, filter);
    }

    [Fact]
    public void Apply_SobelY()
    {
        using var input = ImageGenerator.Next();
        var filter = Filters.SobelY;
        RunSingle(input, filter);
    }

    [Fact]
    public void Apply_PrewittX()
    {
        using var input = ImageGenerator.Next();
        var filter = Filters.PrewittX;
        RunSingle(input, filter);
    }

    [Fact]
    public void Apply_PrewittY()
    {
        using var input = ImageGenerator.Next();
        var filter = Filters.PrewittY;
        RunSingle(input, filter);
    }

    [Fact]
    public void Apply_RandomFilter5()
    {
        using var input = ImageGenerator.Next();
        var filter = FilterGenerator.Next(size: 5);
        RunSingle(input, filter);
    }
}
