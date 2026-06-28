namespace Convolution.Measurement;

public class SkipBenchmarkException : Exception
{
    public SkipBenchmarkException()
    : base()
    {
    }

    public SkipBenchmarkException(string message)
    : base(message)
    {
    }

    public SkipBenchmarkException(string message, Exception innerException)
    : base(message, innerException)
    {
    }
}
