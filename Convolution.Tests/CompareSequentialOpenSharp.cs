namespace Convolution.Tests;

using Convolution.Core;
using Convolution.Extensions;
using SixLabors.ImageSharp.Processing;
using Xunit;

public class CompareSequentialImageSharp
{
    private static readonly ImageGenerator imageGenerator = new();
    [Fact]
    public void Apply_BoxBlur()
    {
        int radius = 1;
        byte tolerance = 2;

        using var input = imageGenerator.Generate();

        using var seqResult = Impl.Sequential.Apply(input, Filter.BoxBlur(radius));
        using var sharpResult = input.Clone(ctx => ctx.BoxBlur(radius));

        Assert.True(seqResult.Equal(sharpResult, tolerance));
    }

    [Fact]
    public void Apply_GaussianBlur()
    {
        double sigma = 1.5;
        byte tolerance = 2;

        using var input = imageGenerator.Generate();

        using var seqResult = Impl.Sequential.Apply(input, Filter.GaussianBlur(sigma));
        using var sharpResult = input.Clone(ctx => ctx.GaussianBlur((float)sigma));

        Assert.True(seqResult.Equal(sharpResult, tolerance));
    }
}