namespace Convolution.Tests;

using Convolution.Core;
using Convolution.Extentions;
using Xunit;

public class Compare
{
    private static readonly ImageGenerator imageGenerator = new();

    [Fact]
    public void Apply_IdentityFilter()
    {
        using var input = imageGenerator.Generate();

        using var parallelResult = Impl.Parallel.Apply(input, Filter.Identity);
        using var sequentialResult = Impl.Sequential.Apply(input, Filter.Identity);

        Assert.True(parallelResult.Equal(sequentialResult));
    }

    [Fact]
    public void Apply_Blur3()
    {
        using var input = imageGenerator.Generate();

        using var parallelResult = Impl.Parallel.Apply(input, Filter.Blur3);
        using var sequentialResult = Impl.Sequential.Apply(input, Filter.Blur3);

        Assert.True(parallelResult.Equal(sequentialResult));
    }

    [Fact]
    public void Apply_Sharpen()
    {
        using var input = imageGenerator.Generate();

        using var parallelResult = Impl.Parallel.Apply(input, Filter.Sharpen);
        using var sequentialResult = Impl.Sequential.Apply(input, Filter.Sharpen);

        Assert.True(parallelResult.Equal(sequentialResult));
    }
}