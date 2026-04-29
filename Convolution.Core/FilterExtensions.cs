namespace Convolution.Core;

/// <summary>
/// Provides extension methods for Convolution.Core.Filter.
/// </summary>
public static class FilterExtensions
{
    /// <summary>
    /// Returns a new filter with the kernel padded with zeros around the edges.
    /// The kernel size increases by <paramref name="padding"/> * 2 in each dimension.
    /// </summary>
    public static Filter Pad(this Filter filter, int padding)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(padding, nameof(padding));

        int oldSize = filter.KernelSize;
        int newSize = oldSize + (2 * padding);
        var newKernel = new float[newSize, newSize];

        for (int y = 0; y < oldSize; y++)
        {
            for (int x = 0; x < oldSize; x++)
            {
                newKernel[y + padding, x + padding] = filter.Kernel[y, x];
            }
        }

        return new Filter(newKernel, filter.Factor, filter.Bias, filter.EdgeMode);
    }

    public static Filter Compose(this Filter a, Filter b)
    {
        if (a.EdgeMode != b.EdgeMode)
        {
            throw new ArgumentException("Filters edge handling mode must be the same.");
        }

        int aSize = a.KernelSize;
        int bSize = b.KernelSize;
        int cSize = aSize + bSize - 1;

        var kernel = new float[cSize, cSize];

        for (int ay = 0; ay < aSize; ay++)
        {
            for (int ax = 0; ax < aSize; ax++)
            {
                for (int by = 0; by < bSize; by++)
                {
                    for (int bx = 0; bx < bSize; bx++)
                    {
                        int cy = ay + by;
                        int cx = ax + bx;
                        kernel[cy, cx] += a.Kernel[ay, ax] * b.Kernel[by, bx];
                    }
                }
            }
        }

        float bSum = 0;

        for (int by = 0; by < bSize; by++)
        {
            for (int bx = 0; bx < bSize; bx++)
            {
                bSum += b.Kernel[by, bx];
            }
        }

        float bias = (a.Bias * bSum * b.Factor) + b.Bias;
        float factor = a.Factor * b.Factor;
        EdgeMode edgeMode = a.EdgeMode;

        return new Filter(kernel, factor, bias, edgeMode);
    }
}
