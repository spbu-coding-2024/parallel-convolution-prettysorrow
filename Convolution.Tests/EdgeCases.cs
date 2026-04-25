namespace Convolution.Tests;

using Convolution.Core;
using Xunit;

public class EdgeCases
{
    [Fact]
    public void FilterConstructor_NonSquareKernel_ThrowsArgumentException()
    {
        var kernel3x4 = new float[3, 4];
        Assert.Throws<ArgumentException>(() => new Filter(kernel3x4));
    }

    [Fact]
    public void FilterConstructor_EvenSizedKernel_ThrowsArgumentException()
    {
        var kernel2x2 = new float[2, 2];
        Assert.Throws<ArgumentException>(() => new Filter(kernel2x2));
    }

    [Fact]
    public void FilterConstructor_SquareOddSizedKernel_DoesNotThrow()
    {
        var valid1x1 = new float[1, 1];
        var valid3x3 = new float[3, 3];
        var valid5x5 = new float[5, 5];

        var filter1 = new Filter(valid1x1);
        var filter2 = new Filter(valid3x3);
        var filter3 = new Filter(valid5x5);

        Assert.NotNull(filter1);
        Assert.NotNull(filter2);
        Assert.NotNull(filter3);
    }

    [Fact]
    public void Compose_DifferentEdgeModes_ThrowsArgumentException()
    {
        var kernel = new float[3, 3];

        var clampFilter = new Filter(kernel, edgeMode: EdgeMode.Clamp);
        var wrapFilter = new Filter(kernel, edgeMode: EdgeMode.Wrap);

        Assert.Throws<ArgumentException>(() => Filter.Compose(clampFilter, wrapFilter));
        Assert.Throws<ArgumentException>(() => Filter.Compose(wrapFilter, clampFilter));
    }

    [Fact]
    public void Compose_SameEdgeModes_DoesNotThrow()
    {
        var kernel3x3 = new float[3, 3];
        var kernel5x5 = new float[5, 5];
        var clampFilter3x3 = new Filter(kernel3x3, edgeMode: EdgeMode.Clamp);
        var clampFilter5x5 = new Filter(kernel5x5, edgeMode: EdgeMode.Clamp);
        var wrapFilter3x3 = new Filter(kernel3x3, edgeMode: EdgeMode.Wrap);
        var wrapFilter5x5 = new Filter(kernel5x5, edgeMode: EdgeMode.Wrap);

        var composedClamp = Filter.Compose(clampFilter5x5, clampFilter3x3);
        var composedWrap = Filter.Compose(wrapFilter3x3, wrapFilter5x5);

        Assert.NotNull(composedClamp);
        Assert.NotNull(composedWrap);
    }
}
