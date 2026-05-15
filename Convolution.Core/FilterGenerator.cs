namespace Convolution.Core;

/// <summary>
/// Generates random convolution filters.
/// </summary>
public class FilterGenerator(int? seed = null)
{
    private readonly Random random = seed.HasValue ? new Random(seed.Value) : new Random();

    public static readonly FilterGenerator Shared = new(seed: 42);

    public Filter Next(
        int size = 5,
        float factor = 1.0f,
        float bias = 0.0f,
        EdgeMode edgeMode = EdgeMode.Clamp)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(size, nameof(size));

        if (size % 2 == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(size), "Kernel size must be odd.");
        }

        var kernel = new float[size, size];
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                kernel[i, j] = (float)((this.random.NextDouble() * 2) - 1); // (0, 1) ~> (-1, 1)
            }
        }

        return new Filter(kernel, factor, bias, edgeMode);
    }
}
