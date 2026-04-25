namespace Convolution.Tests;

using Convolution.Core;
using Convolution.Extensions;
using Xunit;

[Trait("Suite", "Abstract")]
public abstract class InvariantTests
{
    private static readonly ImageGenerator ImageGenerator = new();
    private static readonly FilterGenerator FilterGenerator = new();

    [Theory]
    [MemberData("ImageSizes")]
    public void Apply_Zeros_OnDifferentImages_ReturnsSameImage(int width, int height)
    {
        using var image1 = ImageGenerator.Next(width, height);
        using var image2 = ImageGenerator.Next(width, height);

        using var seqResultImage1 = Impl.Sequential.Apply(image1, Filters.Zeros);
        using var seqResultImage2 = Impl.Sequential.Apply(image2, Filters.Zeros);
        using var parResultImage1 = Impl.Parallel.Apply(image1, Filters.Zeros);
        using var parResultImage2 = Impl.Parallel.Apply(image2, Filters.Zeros);

        Assert.True(seqResultImage1.IsEqualTo(seqResultImage2));
        Assert.True(parResultImage1.IsEqualTo(parResultImage2));
        Assert.True(seqResultImage1.IsEqualTo(parResultImage1));
    }

    [Theory]
    [MemberData("ImageSizes")]
    public void Apply_IdentityFilter_ReturnsSameImage(int width, int height)
    {
        using var image = ImageGenerator.Next(width, height);

        using var seqResult = Impl.Sequential.Apply(image, Filters.Identity);
        using var parResult = Impl.Parallel.Apply(image, Filters.Identity);

        Assert.True(image.IsEqualTo(seqResult));
        Assert.True(image.IsEqualTo(parResult));
    }

    [Theory]
    [MemberData("ImageSizes")]
    public void Apply_RandomFilter_ReturnsDifferentImage(int width, int height)
    {
        using var image = ImageGenerator.Next(width, height);
        var filter = FilterGenerator.Next();

        using var seqResult = Impl.Sequential.Apply(image, filter);
        using var parResult = Impl.Parallel.Apply(image, filter);

        Assert.Equal(image.Width, seqResult.Width);
        Assert.Equal(image.Height, seqResult.Height);
        Assert.False(image.IsEqualTo(seqResult));

        Assert.Equal(image.Width, parResult.Width);
        Assert.Equal(image.Height, parResult.Height);
        Assert.False(image.IsEqualTo(parResult));

        Assert.True(seqResult.IsEqualTo(parResult));
    }

    [Theory]
    [MemberData("ImageSizes")]
    public void Pad_DoesNotChangeResult(int width, int height)
    {
        using var image = ImageGenerator.Next(width, height);
        var defaultFilter = FilterGenerator.Next();
        var paddedFilter1 = defaultFilter.Pad(1);
        var paddedFilter5 = defaultFilter.Pad(5);

        using var seqResultDefault = Impl.Sequential.Apply(image, defaultFilter);
        using var seqResultPad1 = Impl.Sequential.Apply(image, paddedFilter1);
        using var seqResultPad5 = Impl.Sequential.Apply(image, paddedFilter5);

        using var parResultDefault = Impl.Parallel.Apply(image, defaultFilter);
        using var parResultPad1 = Impl.Parallel.Apply(image, paddedFilter1);
        using var parResultPad5 = Impl.Parallel.Apply(image, paddedFilter5);

        Assert.True(seqResultDefault.IsEqualTo(seqResultPad1));
        Assert.True(seqResultPad1.IsEqualTo(seqResultPad5));

        Assert.True(parResultDefault.IsEqualTo(parResultPad1));
        Assert.True(parResultPad1.IsEqualTo(parResultPad5));

        Assert.True(seqResultDefault.IsEqualTo(parResultDefault));
    }
}

[Trait("Suite", "Coverage")]
public class InvariantTests_Coverage : InvariantTests
{
    public static TheoryData<int, int> ImageSizes => new() { { 50, 50 } };
}

[Trait("Suite", "All")]
public class InvariantTests_All : InvariantTests
{
    public static TheoryData<int, int> ImageSizes => new()
    {
        { 1, 1 },
        { 32, 32 },
        { 256, 144 },
        { 640, 360 },
    };
}
