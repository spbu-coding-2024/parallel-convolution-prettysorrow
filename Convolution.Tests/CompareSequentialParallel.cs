namespace Convolution.Tests;

using Convolution.Core;
using Convolution.Extensions;
using Xunit;

public class CompareSequentialParallel
{
    private static readonly ImageGenerator imageGenerator = new();

    [Fact]
    public void Apply_IdentityFilter()
    {
        using var input = imageGenerator.Generate();

        using var parallelResult = Impl.Parallel.Apply(input, Filter.Identity);
        using var sequentialResult = Impl.Sequential.Apply(input, Filter.Identity);

        Assert.True(parallelResult.Equal(sequentialResult, tolerance: 2));
    }

    [Fact]
    public void Apply_Sharpen()
    {
        using var input = imageGenerator.Generate();

        using var parallelResult = Impl.Parallel.Apply(input, Filter.Sharpen);
        using var sequentialResult = Impl.Sequential.Apply(input, Filter.Sharpen);

        Assert.True(parallelResult.Equal(sequentialResult, tolerance: 2));
    }

    [Fact]
    public void Apply_BoxBlur()
    {
        using var input = imageGenerator.Generate();

        using var parallelResult = Impl.Parallel.Apply(input, Filter.BoxBlur(radius: 1));
        using var sequentialResult = Impl.Sequential.Apply(input, Filter.BoxBlur(radius: 1));

        Assert.True(parallelResult.Equal(sequentialResult, tolerance: 2));
    }


    [Fact]
    public void Apply_GaussianBlur()
    {
        using var input = imageGenerator.Generate();

        using var parallelResult = Impl.Parallel.Apply(input, Filter.GaussianBlur(size: 3, sigma: 0.5));
        using var sequentialResult = Impl.Sequential.Apply(input, Filter.GaussianBlur(size: 3, sigma: 0.5));

        Assert.True(parallelResult.Equal(sequentialResult, tolerance: 2));
    }
}