namespace Convolution.Core;

public class Filter
{
    public double[,] Kernel { get; }
    public double Factor { get; }
    public double Bias { get; }
    public EdgeHandlingMode EdgeHandling { get; }

    public enum EdgeHandlingMode
    {
        Clamp,
        Wrap,
    }

    public Filter(double[,] kernel, double factor = 1.0, double bias = 0.0, EdgeHandlingMode edgeHandling = EdgeHandlingMode.Clamp)
    {
        Kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));
        if (kernel.GetLength(0) != kernel.GetLength(1))
            throw new ArgumentException("Kernel must be square.", nameof(kernel));
        if (kernel.GetLength(0) % 2 == 0)
            throw new ArgumentException("Kernel size must be odd.", nameof(kernel));

        Factor = factor;
        Bias = bias;
        EdgeHandling = edgeHandling;
    }


    public static readonly Filter Identity = new(kernel: new double[,]
    {
        { 0, 0, 0 },
        { 0, 1, 0 },
        { 0, 0, 0 }
    }, factor: 1.0, bias: 0.0);

    public static readonly Filter Edges = new(kernel: new double[,]
   {
        { 0.0, 0.0, -1f, 0.0, 0.0 },
        { 0.0, 0.0, -1f, 0.0, 0.0 },
        { 0.0, 0.0, 2f, 0.0, 0.0 },
        { 0.0, 0.0, 0.0, 0.0, 0.0 },
        { 0.0, 0.0, 0.0, 0.0, 0.0 },
   }, factor: 1.0, bias: 0.0);

    public static Filter BoxBlur(int radius)
    {
        if (radius < 1) throw new ArgumentException("Radius must be at least 1", nameof(radius));
        int size = 2 * radius + 1;
        double weight = 1.0 / (size * size);
        var kernel = new double[size, size];
        for (int i = 0; i < size; i++)
            for (int j = 0; j < size; j++)
                kernel[i, j] = weight;
        return new Filter(kernel, factor: 1.0, bias: 0.0);
    }

    public static Filter GaussianBlur(int size, double sigma)
    {
        if (size % 2 == 0) throw new ArgumentException("Kernel size must be odd", nameof(size));
        if (sigma <= 0) throw new ArgumentException("Sigma must be positive", nameof(sigma));

        var kernel = new double[size, size];
        int offset = size / 2;
        double sum = 0.0;

        for (int y = -offset; y <= offset; y++)
        {
            for (int x = -offset; x <= offset; x++)
            {
                double value = Math.Exp(-(x * x + y * y) / (2 * sigma * sigma));
                kernel[y + offset, x + offset] = value;
                sum += value;
            }
        }

        for (int i = 0; i < size; i++)
            for (int j = 0; j < size; j++)
                kernel[i, j] /= sum;

        return new Filter(kernel, factor: 1.0, bias: 0.0);
    }

    public static Filter GaussianBlur(double sigma)
    {
        if (sigma <= 0) throw new ArgumentException("Sigma must be positive", nameof(sigma));
        int radius = (int)Math.Ceiling(3 * sigma);
        int size = 2 * radius + 1;
        return GaussianBlur(size, sigma);
    }

    public static readonly Filter Laplacian = new(kernel: new double[,]
    {
        {  0, -1,  0 },
        { -1,  4, -1 },
        {  0, -1,  0 }
    }, factor: 1.0, bias: 0.0);

    public static readonly Filter SobelX = new(kernel: new double[,]
    {
        { -1,  0,  1 },
        { -2,  0,  2 },
        { -1,  0,  1 }
    }, factor: 1.0, bias: 128.0);

    public static readonly Filter SobelY = new(kernel: new double[,]
    {
        { -1, -2, -1 },
        {  0,  0,  0 },
        {  1,  2,  1 }
    }, factor: 1.0, bias: 128.0);

    public static readonly Filter PrewittX = new(kernel: new double[,]
    {
        { -1,  0,  1 },
        { -1,  0,  1 },
        { -1,  0,  1 }
    }, factor: 1.0, bias: 128.0);

    public static readonly Filter PrewittY = new(kernel: new double[,]
    {
        { -1, -1, -1 },
        {  0,  0,  0 },
        {  1,  1,  1 }
    }, factor: 1.0, bias: 128.0);

    public static Filter Compose(Filter f1, Filter f2)
    {
        int aSize = f1.Kernel.GetLength(0);
        int bSize = f2.Kernel.GetLength(0);
        int cSize = aSize + bSize - 1;
        int aOff = aSize / 2;
        int bOff = bSize / 2;
        int cOff = cSize / 2;

        var kernel = new double[cSize, cSize];
        double sumK2 = 0;

        for (int by = 0; by < bSize; by++)
            for (int bx = 0; bx < bSize; bx++)
                sumK2 += f2.Kernel[by, bx];

        for (int ay = 0; ay < aSize; ay++)
            for (int ax = 0; ax < aSize; ax++)
                for (int by = 0; by < bSize; by++)
                    for (int bx = 0; bx < bSize; bx++)
                    {
                        int ky = (ay - aOff) + (by - bOff) + cOff;
                        int kx = (ax - aOff) + (bx - bOff) + cOff;
                        kernel[ky, kx] += f1.Kernel[ay, ax] * f2.Kernel[by, bx];
                    }

        double factor = f1.Factor * f2.Factor;
        double bias = f1.Bias * sumK2 * f2.Factor + f2.Bias;

        return new Filter(kernel, factor, bias, f1.EdgeHandling);
    }
}
