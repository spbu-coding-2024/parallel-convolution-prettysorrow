namespace Convolution.Tests;

using Convolution.Core;
using Convolution.Extensions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;

public class CompareSequentialParallel
{
    private static readonly ImageGenerator imageGenerator = new();
    private static readonly FilterGenerator filterGenerator = new();

    private static void RunSingle(Image<Rgb24> image, Filter filter)
    {
        using var parallelResult = Impl.Parallel.Apply(image, filter);
        using var sequentialResult = Impl.Sequential.Apply(image, filter);
        Assert.True(parallelResult.Equal(sequentialResult, tolerance: 2));
    }

    [Fact]
    public void Apply_IdentityFilter()
    {
        using var input = imageGenerator.Next();
        var filter = Filter.Identity;
        RunSingle(input, filter);
    }

    [Fact]
    public void Apply_BoxBlur()
    {
        using var input = imageGenerator.Next();
        var filter = Filter.BoxBlur(radius: 1);
        RunSingle(input, filter);
    }


    [Fact]
    public void Apply_GaussianBlur()
    {
        using var input = imageGenerator.Next();
        var filter = Filter.GaussianBlur(size: 3, sigma: 0.5);
        RunSingle(input, filter);
    }

    [Fact]
    public void Apply_SobelX()
    {
        using var input = imageGenerator.Next();
        var filter = Filter.SobelX;
        RunSingle(input, filter);
    }

    [Fact]
    public void Apply_SobelY()
    {
        using var input = imageGenerator.Next();
        var filter = Filter.SobelY;
        RunSingle(input, filter);
    }

    [Fact]
    public void Apply_PrewittX()
    {
        using var input = imageGenerator.Next();
        var filter = Filter.PrewittX;
        RunSingle(input, filter);
    }

    [Fact]
    public void Apply_PrewittY()
    {
        using var input = imageGenerator.Next();
        var filter = Filter.PrewittY;
        RunSingle(input, filter);
    }

    [Fact]
    public void Apply_RandomFilter5()
    {
        using var input = imageGenerator.Next();
        var filter = filterGenerator.Next(size: 5);
        RunSingle(input, filter);
    }
}