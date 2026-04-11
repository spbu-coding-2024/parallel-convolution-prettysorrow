namespace Convolution.Tests;

using Convolution.Core;
using Convolution.Extensions;
using Xunit;

public class Sequential
{
    private static readonly ImageGenerator imageGenerator = new();

    [Fact]
    public void Apply_IdentityFilter_ReturnsSameImage()
    {
        using var input = imageGenerator.Next();

        using var result = Impl.Sequential.Apply(input, Filter.Identity);

        Assert.Equal(input.Width, result.Width);
        Assert.Equal(input.Height, result.Height);
        Assert.True(input.Equal(result));
    }

    [Fact]
    public void Apply_BoxBlur_ProducesDifferentImage()
    {
        using var input = imageGenerator.Next();
        using var result = Impl.Sequential.Apply(input, Filter.BoxBlur(radius: 1));

        Assert.Equal(input.Width, result.Width);
        Assert.Equal(input.Height, result.Height);

        Assert.False(input.Equal(result));
    }
}