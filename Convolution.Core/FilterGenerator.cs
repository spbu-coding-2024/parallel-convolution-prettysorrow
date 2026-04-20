namespace Convolution.Core;

/// <summary>
/// Generates random convolution filters.
/// </summary>
public class FilterGenerator(int? seed = null)
{
    private readonly Random random = seed.HasValue ? new Random(seed.Value) : new Random();

    /// <summary>
    /// Generates a random filter with the specified parameters.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Thrown when the size of <paramref name="kernel"/> is even (must be odd).
    /// </exception>
    public Filter Next(
        int size = 5,
        double factor = 1.0,
        double bias = 0.0,
        Filter.EdgeMode edgeMode = Filter.EdgeMode.Clamp)
    {
        if (size % 2 == 0)
        {
            throw new ArgumentException("Kernel size must be odd.", nameof(size));
        }

        var kernel = new double[size, size];
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                kernel[i, j] = (this.random.NextDouble() * 2) - 1; // (0, 1) ~> (-1, 1)
            }
        }

        return new Filter(kernel, factor, bias, edgeMode);
    }
}
