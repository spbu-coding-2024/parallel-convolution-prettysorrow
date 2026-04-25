namespace Convolution.Extensions;

using Convolution.Core;

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

        int oldSize = filter.Kernel.GetLength(0);
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
}
