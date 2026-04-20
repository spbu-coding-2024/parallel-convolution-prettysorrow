#pragma warning disable SA1025 // Code should not contain multiple whitespace in a row
#pragma warning disable SA1600 // Elements should be documented

namespace Convolution.Core;

/// <summary>
/// Collection of predefined convolution filters.
/// </summary>
public static class Filters
{
    public static readonly Filter Identity = new(kernel: new float[,]
    {
        { 0, 0, 0 },
        { 0, 1, 0 },
        { 0, 0, 0 },
    }, factor: 1, bias: 0);

    public static readonly Filter Edges = new(kernel: new float[,]
    {
        { 0, 0, -1f, 0, 0 },
        { 0, 0, -1f, 0, 0 },
        { 0, 0,  2f, 0, 0 },
        { 0, 0,   0, 0, 0 },
        { 0, 0,   0, 0, 0 },
    }, factor: 1, bias: 0);

    public static readonly Filter Laplacian = new(kernel: new float[,]
    {
        {  0, -1,  0 },
        { -1,  4, -1 },
        {  0, -1,  0 },
    }, factor: 1, bias: 0);

    public static readonly Filter SobelX = new(kernel: new float[,]
    {
        { -1,  0,  1 },
        { -2,  0,  2 },
        { -1,  0,  1 },
    }, factor: 1, bias: 128);

    public static readonly Filter SobelY = new(kernel: new float[,]
    {
        { -1, -2, -1 },
        {  0,  0,  0 },
        {  1,  2,  1 },
    }, factor: 1, bias: 128);

    public static readonly Filter PrewittX = new(kernel: new float[,]
    {
        { -1,  0,  1 },
        { -1,  0,  1 },
        { -1,  0,  1 },
    }, factor: 1, bias: 128);

    public static readonly Filter PrewittY = new(kernel: new float[,]
    {
        { -1, -1, -1 },
        {  0,  0,  0 },
        {  1,  1,  1 },
    }, factor: 1, bias: 128);

    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the <paramref name="distance"/> is less than 0.
    /// </exception>
    public static Filter ShiftRight(int distance = 1)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(distance, 0, nameof(distance));

        int size = (2 * distance) + 1;
        var kernel = new float[size, size];
        kernel[distance, size - 1] = 1.0f;
        return new Filter(kernel, factor: 1, bias: 0, EdgeMode.Wrap);
    }

    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the <paramref name="distance"/> is less than 0.
    /// </exception>
    public static Filter ShiftLeft(int distance = 1)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(distance, 0, nameof(distance));

        int size = (2 * distance) + 1;
        var kernel = new float[size, size];
        kernel[distance, 0] = 1;
        return new Filter(kernel, factor: 1, bias: 0, EdgeMode.Wrap);
    }

    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the <paramref name="distance"/> is less than 0.
    /// </exception>
    public static Filter ShiftTop(int distance = 1)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(distance, 0, nameof(distance));

        int size = (2 * distance) + 1;
        var kernel = new float[size, size];
        kernel[0, distance] = 1;
        return new Filter(kernel, factor: 1, bias: 0, EdgeMode.Wrap);
    }

    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the <paramref name="distance"/> is less than 0.
    /// </exception>
    public static Filter ShiftBottom(int distance = 1)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(distance, 0, nameof(distance));

        int size = (2 * distance) + 1;
        var kernel = new float[size, size];
        kernel[size - 1, distance] = 1;
        return new Filter(kernel, factor: 1, bias: 0, EdgeMode.Wrap);
    }

    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the <paramref name="radius"/> is less than 1.
    /// </exception>
    public static Filter BoxBlur(int radius)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(radius, 1, nameof(radius));

        int size = (2 * radius) + 1;
        float weight = 1.0f / (size * size);
        var kernel = new float[size, size];
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                kernel[i, j] = weight;
            }
        }

        return new Filter(kernel, factor: 1.0, bias: 0.0);
    }

    public static Filter GaussianBlur(float sigma)
    {
        if (sigma <= 0)
        {
            throw new ArgumentException("Sigma must be positive", nameof(sigma));
        }

        int radius = (int)Math.Ceiling(3 * sigma);
        int size = (2 * radius) + 1;
        var kernel = new float[size, size];
        int offset = size / 2;
        float sum = 0;

        for (int y = -offset; y <= offset; y++)
        {
            for (int x = -offset; x <= offset; x++)
            {
                float value = MathF.Exp(-((x * x) + (y * y)) / (2 * sigma * sigma));
                kernel[y + offset, x + offset] = value;
                sum += value;
            }
        }

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                kernel[i, j] /= sum;
            }
        }

        return new Filter(kernel, factor: 1, bias: 0);
    }
}
