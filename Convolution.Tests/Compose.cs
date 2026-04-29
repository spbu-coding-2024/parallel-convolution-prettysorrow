namespace Convolution.Tests;

using Convolution.Core;
using static Convolution.Impl.Parallel;
using Xunit;

/// <summary>
/// Tests Convolution.Core.Filter.Compose using parallel implementation.
/// </summary>
[Trait("Suite", "Abstract")]
public abstract class ComposeTests
{
    private static readonly ImageGenerator ImageGenerator = new();
    private static readonly FilterGenerator FilterGenerator = new();

    [Theory]
    [MemberData("ImageSizes")]
    public void Compose_Identity(int width, int height)
    {
        using var image = ImageGenerator.Next(width, height);
        var filter = Filters.Identity.Compose(Filters.Identity);
        using var output = filter.Apply(image);
        Assert.True(output.IsEqualTo(image));
    }

    [Theory]
    [MemberData("ImageSizes")]
    public void Compose_Commutativity_Wrap(int width, int height)
    {
        using var image = ImageGenerator.Next(width, height);
        var filter1 = FilterGenerator.Next(size: 3, edgeMode: EdgeMode.Wrap);
        var filter2 = FilterGenerator.Next(size: 3, edgeMode: EdgeMode.Wrap);

        var composition1 = filter1.Compose(filter2);
        var composition2 = filter2.Compose(filter1);

        using var output1 = composition1.Apply(image);
        using var output2 = composition2.Apply(image);

        Assert.True(output1.IsEqualTo(output2));
    }

    [Theory]
    [MemberData("ImageSizes")]
    public void Compose_Commutativity_Clamp(int width, int height)
    {
        using var image = ImageGenerator.Next(width, height);
        var filter1 = FilterGenerator.Next(size: 3, edgeMode: EdgeMode.Clamp);
        var filter2 = FilterGenerator.Next(size: 3, edgeMode: EdgeMode.Clamp);

        var composition1 = filter1.Compose(filter2);
        var composition2 = filter2.Compose(filter1);

        using var output1 = composition1.Apply(image);
        using var output2 = composition2.Apply(image);

        Assert.True(output1.IsEqualTo(output2));
    }

    [Theory]
    [MemberData("ImageSizes")]
    public void Compose_Compositionality_Wrap(int width, int height)
    {
        using var image = ImageGenerator.Next(width, height);
        var filter1 = FilterGenerator.Next(size: 3, edgeMode: EdgeMode.Wrap);
        var filter2 = FilterGenerator.Next(size: 3, edgeMode: EdgeMode.Wrap);

        var composition = filter1.Compose(filter2);
        using var output1 = filter2.Apply(filter1.Apply(image));
        using var output2 = composition.Apply(image);

        Assert.True(output1.IsEqualTo(output2));
    }

    [Theory]
    [MemberData("ImageSizes")]
    public void Compose_Shift_LeftRight(int width, int height)
    {
        using var image = ImageGenerator.Next(width, height);

        using var leftRightResult = image.Filter(Filters.ShiftLeft(5).Compose(Filters.ShiftRight(5)));
        using var rightLeftResult = image.Filter(Filters.ShiftRight(5).Compose(Filters.ShiftLeft(5)));

        Assert.True(leftRightResult.IsEqualTo(image));
        Assert.True(rightLeftResult.IsEqualTo(image));
    }

    [Theory]
    [MemberData("ImageSizes")]
    public void Compose_Shift_TopBottom(int width, int height)
    {
        using var image = ImageGenerator.Next(width, height);
        using var topBottom = image.Filter(Filters.ShiftTop(5).Compose(Filters.ShiftBottom(5)));
        using var bottomTop = image.Filter(Filters.ShiftBottom(5).Compose(Filters.ShiftTop(5)));
        Assert.True(topBottom.IsEqualTo(image));
        Assert.True(bottomTop.IsEqualTo(image));
    }

    [Theory]
    [MemberData("ImageSizes")]
    public void Compose_Shift_LeftLeftRight(int width, int height)
    {
        using var image = ImageGenerator.Next(width, height);
        var llrFilter = Filters.ShiftLeft(5).Compose(Filters.ShiftLeft(5)).Compose(Filters.ShiftRight(5));
        var lrlFilter = Filters.ShiftLeft(5).Compose(Filters.ShiftRight(5)).Compose(Filters.ShiftLeft(5));
        var rllFilter = Filters.ShiftRight(5).Compose(Filters.ShiftLeft(5)).Compose(Filters.ShiftLeft(5));

        using var llrResult = llrFilter.Apply(image);
        using var lrlResult = lrlFilter.Apply(image);
        using var rllResult = rllFilter.Apply(image);

        Assert.True(llrResult.IsEqualTo(lrlResult));
        Assert.True(lrlResult.IsEqualTo(rllResult));
    }

    [Theory]
    [MemberData("ImageSizes")]
    public void Compose_TopRight(int width, int height)
    {
        using var image = ImageGenerator.Next(width, height);

        using var applyComposeTopRight = image.Filter(Filters.ShiftTop(10).Compose(Filters.ShiftRight(10)));
        using var applyComposeRightTop = image.Filter(Filters.ShiftRight(10).Compose(Filters.ShiftTop(10)));

        using var applyApplyTopRight = image.Filter(Filters.ShiftTop(10)).Filter(Filters.ShiftRight(10));
        using var applyApplyRightTop = image.Filter(Filters.ShiftRight(10)).Filter(Filters.ShiftTop(10));

        Assert.True(applyComposeTopRight.IsEqualTo(applyComposeRightTop));
        Assert.True(applyComposeRightTop.IsEqualTo(applyApplyTopRight));
        Assert.True(applyApplyTopRight.IsEqualTo(applyApplyRightTop));
    }
}

[Trait("Suite", "Coverage")]
public class ComposeTests_Coverage : ComposeTests
{
    public static TheoryData<int, int> ImageSizes => new() { { 50, 50 } };
}

[Trait("Suite", "All")]
public class ComposeTests_All : ComposeTests
{
    public static TheoryData<int, int> ImageSizes => new()
    {
        { 1, 1 },
        { 32, 32 },
        { 256, 144 },
        { 640, 360 },
    };
}