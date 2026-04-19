namespace Convolution.Core;

public class Filter
{
    public double[,] Kernel { get; }

    public double Factor { get; }

    public double Bias { get; }

    public enum EdgeMode
    {
        Clamp,
        Wrap
    }

    public EdgeMode edgeMode { get; }

    public Filter(double[,] kernel, double factor = 1.0, double bias = 0.0, EdgeMode edgeMode = EdgeMode.Clamp)
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
        this.edgeMode = edgeMode;
    }

    public static Filter Compose(Filter f1, Filter f2)
    {
        if (f1.edgeMode != f2.edgeMode)
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
