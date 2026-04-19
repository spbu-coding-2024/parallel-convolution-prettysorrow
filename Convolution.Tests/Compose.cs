namespace Convolution.Tests;

using Convolution.Core;
using Convolution.Extensions;
using Xunit;

public class Compose
{
    private static readonly ImageGenerator ImageGenerator = new();
    private static readonly FilterGenerator FilterGenerator = new();

    [Fact]
    public void Compose_Identity()
    {
        using var image = ImageGenerator.Next();

        var composition = Filter.Compose(Filters.Identity, Filters.Identity);
        using var output = Impl.Sequential.Apply(image, composition);

        Assert.True(output.Equal(image));
    }

    [Fact]
    public void Compose_Commutativity_Wrap()
    {
        using var image = ImageGenerator.Next();
        var filter1 = FilterGenerator.Next(size: 3, edgeMode: Filter.EdgeMode.Wrap);
        var filter2 = FilterGenerator.Next(size: 3, edgeMode: Filter.EdgeMode.Wrap);

        var composition1 = Filter.Compose(filter1, filter2);
        var composition2 = Filter.Compose(filter2, filter1);

        using var output1 = Impl.Sequential.Apply(image, composition1);
        using var output2 = Impl.Sequential.Apply(image, composition2);

        Assert.True(output1.Equal(output2));
    }

    [Fact]
    public void Compose_Commutativity_Clamp()
    {
        using var image = ImageGenerator.Next();
        var filter1 = FilterGenerator.Next(size: 3, edgeMode: Filter.EdgeMode.Clamp);
        var filter2 = FilterGenerator.Next(size: 3, edgeMode: Filter.EdgeMode.Clamp);

        var composition1 = Filter.Compose(filter1, filter2);
        var composition2 = Filter.Compose(filter2, filter1);

        using var output1 = Impl.Sequential.Apply(image, composition1);
        using var output2 = Impl.Sequential.Apply(image, composition2);

        Assert.True(output1.Equal(output2));
    }

    [Fact]
    public void Compose_Compositionality_Wrap()
    {
        using var image = ImageGenerator.Next();
        var filter1 = FilterGenerator.Next(size: 3, edgeMode: Filter.EdgeMode.Wrap);
        var filter2 = FilterGenerator.Next(size: 3, edgeMode: Filter.EdgeMode.Wrap);

        var composition = Filter.Compose(filter1, filter2);

        using var output1 = Impl.Sequential.Apply(Impl.Sequential.Apply(image, filter1), filter2);
        using var output2 = Impl.Sequential.Apply(image, composition);

        Assert.True(output1.Equal(output2));
    }

    [Fact]
    public void Compose_Shift_LeftRight()
    {
        using var image = ImageGenerator.Next();

        using var leftRight = Impl.Sequential.Apply(image, Filter.Compose(Filters.ShiftLeft, Filters.ShiftRight));
        using var rightLeft = Impl.Sequential.Apply(image, Filter.Compose(Filters.ShiftRight, Filters.ShiftLeft));

        Assert.True(leftRight.Equal(image));
        Assert.True(rightLeft.Equal(image));
    }

    [Fact]
    public void Compose_Shift_TopBottop()
    {
        using var image = ImageGenerator.Next();

        using var topBottom = Impl.Sequential.Apply(image, Filter.Compose(Filters.ShiftTop, Filters.ShiftBottom));
        using var bottomTop = Impl.Sequential.Apply(image, Filter.Compose(Filters.ShiftBottom, Filters.ShiftTop));

        Assert.True(topBottom.Equal(image));
        Assert.True(bottomTop.Equal(image));
    }

    [Fact]
    public void Compose_Shift_LeftLeftRight()
    {
        using var image = ImageGenerator.Next();

        using var llr = Impl.Sequential.Apply(image, Filter.Compose(Filter.Compose(Filters.ShiftLeft, Filters.ShiftLeft), Filters.ShiftRight));
        using var lrl = Impl.Sequential.Apply(image, Filter.Compose(Filter.Compose(Filters.ShiftLeft, Filters.ShiftRight), Filters.ShiftLeft));
        using var rll = Impl.Sequential.Apply(image, Filter.Compose(Filter.Compose(Filters.ShiftRight, Filters.ShiftLeft), Filters.ShiftLeft));

        Assert.True(llr.Equal(lrl));
        Assert.True(lrl.Equal(rll));
    }
}
