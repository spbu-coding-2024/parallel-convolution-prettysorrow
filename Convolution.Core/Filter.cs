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
}
