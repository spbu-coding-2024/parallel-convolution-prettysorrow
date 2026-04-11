namespace Convolution.Core;

public class FilterGenerator(int? seed = null)
{
    private readonly Random _random = seed.HasValue ? new Random(seed.Value) : new Random();

    public Filter Next(
        int size,
        double factor = 1.0,
        double bias = 0.0,
        Filter.EdgeHandlingMode edgeHandling = Filter.EdgeHandlingMode.Clamp)
    {
        if (size % 2 == 0) throw new ArgumentException("Kernel size must be odd.", nameof(size));

        var kernel = new double[size, size];
        for (int i = 0; i < size; i++)
            for (int j = 0; j < size; j++)
                kernel[i, j] = _random.NextDouble() * 2 - 1;

        return new Filter(kernel, factor, bias, edgeHandling);
    }
}
