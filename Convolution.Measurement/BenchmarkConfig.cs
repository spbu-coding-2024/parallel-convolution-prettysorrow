namespace Convolution.Measurement;

using System.Globalization;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Reports;
using Perfolizer.Horology;
using Perfolizer.Metrology;

public class BenchmarkConfig : ManualConfig
{
    public BenchmarkConfig()
    {
        this.Options = ConfigOptions.StopOnFirstError;
        this.SummaryStyle = SummaryStyle.Default
            .WithTimeUnit(TimeUnit.Millisecond)
            .WithSizeUnit(SizeUnit.B)
            .WithCultureInfo(CultureInfo.InvariantCulture);
    }
}
