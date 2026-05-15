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
        this.SummaryStyle = SummaryStyle.Default
            .WithTimeUnit(TimeUnit.Millisecond)
            .WithSizeUnit(SizeUnit.B)
            .WithCultureInfo(CultureInfo.InvariantCulture);

        this.AddColumn(BenchmarkDotNet.Columns.BaselineColumn.Default); // is required for plots.
    }
}
