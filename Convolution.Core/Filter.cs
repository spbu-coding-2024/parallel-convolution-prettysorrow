namespace Convolution.Core;

/// <summary>
/// Specifies the edge handling mode for image convolution.
/// </summary>
public enum EdgeMode
{
    Clamp,
    Wrap,
}

/// <summary>
/// Represents a convolution filter kernel with associated processing parameters.
/// </summary>
public sealed class Filter
{
    public float[,] Kernel { get; }

    public int KernelSize => this.Kernel.GetLength(0);

    public float Factor { get; }

    public float Bias { get; }

    public EdgeMode EdgeMode { get; }

    public Filter(float[,] kernel, float factor = 1.0f, float bias = 0.0f, EdgeMode edgeMode = EdgeMode.Clamp)
    {
        if (kernel.GetLength(0) != kernel.GetLength(1))
        {
            throw new ArgumentException("Kernel must be square.", nameof(kernel));
        }

        if (kernel.GetLength(0) % 2 == 0)
        {
            throw new ArgumentException("Kernel size must be odd.", nameof(kernel));
        }

        this.Kernel = kernel;
        this.Factor = factor;
        this.Bias = bias;
        this.EdgeMode = edgeMode;
    }

    /// <summary>
    /// Composes two filters by convolving their kernels.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Thrown when filters edge handling modes differ.
    /// </exception>
    public static Filter Compose(Filter a, Filter b)
    {
        if (a.EdgeMode != b.EdgeMode)
        {
            throw new ArgumentException("Filters edge handling mode must be the same.");
        }

        int aSize = a.Kernel.GetLength(0);
        int bSize = b.Kernel.GetLength(0);
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
