namespace Convolution.Tests;

using Convolution.Core;
using Convolution.Extensions;
using SixLabors.ImageSharp.Processing;
using Xunit;

public class CompareImageSharpSequential
{
    private static readonly ImageGenerator ImageGenerator = new();

    [Fact]
    public void Apply_BoxBlur()
    {
        int radius = 1;

        using var input = ImageGenerator.Next();

        using var seqResult = Impl.Sequential.Apply(input, Filters.BoxBlur(radius));
        using var sharpResult = input.Clone(ctx => ctx.BoxBlur(radius));

        Assert.True(seqResult.Equal(sharpResult));
    }

    [Fact]
    public void Apply_GaussianBlur()
    {
        double sigma = 1.5;

        using var input = ImageGenerator.Next();

        using var seqResult = Impl.Sequential.Apply(input, Filters.GaussianBlur(sigma));
        using var sharpResult = input.Clone(ctx => ctx.GaussianBlur((float)sigma));

        Assert.True(seqResult.Equal(sharpResult));
    }
}