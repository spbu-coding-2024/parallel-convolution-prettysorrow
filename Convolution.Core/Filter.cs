namespace Convolution.Core;

/// <summary>
/// Specifies the edge handling mode for image convolution.
/// </summary>
public enum EdgeMode
{
    /// <summary>
    /// Pixels outside the image boundaries are clamped to the nearest edge pixel.
    /// </summary>
    Clamp,

    /// <summary>
    /// Pixels outside the image boundaries are wrapped around to the opposite edge.
    /// </summary>
    Wrap,
}

/// <summary>
/// Represents a convolution filter kernel with associated processing parameters.
/// </summary>
public sealed class Filter
{
    /// <summary>
    /// Gets the filter kernel matrix.
    /// </summary>
    public float[,] Kernel { get; }

    /// <summary>
    /// Gets the factor multiplier applied to the convolution result.
    /// </summary>
    public float Factor { get; }

    /// <summary>
    /// Gets the bias value added to the convolution result.
    /// </summary>
    public float Bias { get; }

    /// <summary>
    /// Gets the edge handling mode for pixels outside image boundaries.
    /// </summary>
    public EdgeMode EdgeMode { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Filter"/> class.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="kernel"/> is not square (first and second dimensions differ).
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the size of <paramref name="kernel"/> is even (must be odd).
    /// </exception>
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

        int aSize = f1.Kernel.GetLength(0);
        int bSize = f2.Kernel.GetLength(0);
        int cSize = aSize + bSize - 1;
        int aOff = aSize / 2;
        int bOff = bSize / 2;
        int cOff = cSize / 2;

        var kernel = new double[cSize, cSize];
        double sumK2 = 0;

        for (int by = 0; by < bSize; by++)
        {
            for (int bx = 0; bx < bSize; bx++)
            {
                sumK2 += f2.Kernel[by, bx];
            }
        }

        for (int ay = 0; ay < aSize; ay++)
        {
            for (int ax = 0; ax < aSize; ax++)
            {
                for (int by = 0; by < bSize; by++)
                {
                    for (int bx = 0; bx < bSize; bx++)
                    {
                        int ky = (ay - aOff) + (by - bOff) + cOff;
                        int kx = (ax - aOff) + (bx - bOff) + cOff;
                        kernel[ky, kx] += f1.Kernel[ay, ax] * f2.Kernel[by, bx];
                    }
                }
            }
        }

        double factor = f1.Factor * f2.Factor;
        double bias = (f1.Bias * sumK2 * f2.Factor) + f2.Bias;
        EdgeMode edgeMode = f1.edgeMode;

        return new Filter(kernel, factor, bias, edgeMode);
    }
}
