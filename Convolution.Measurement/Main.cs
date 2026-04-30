#pragma warning disable SA1200 // Using directives should be placed correctly

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using Convolution.Measurement;

var config = DefaultConfig.Instance
                .WithArtifactsPath("Artifacts") // writes results to <repo root>/Artifacts/
                .WithOption(ConfigOptions.DisableLogFile, true); // no .log file

BenchmarkRunner.Run<Benchmark>(config);

try
{
    Directory.Delete("Artifacts/Benchmark", recursive: true);
}
catch (DirectoryNotFoundException)
{
}

try
{
    Directory.Move("Artifacts/results", "Artifacts/Benchmark");
    File.Delete("Artifacts/Benchmark/Convolution.Measurement.Benchmark-report-default.md");
    File.Move("Artifacts/Benchmark/Convolution.Measurement.Benchmark-report-github.md", "Artifacts/Benchmark/Convolution.Measurement.Benchmark-report.md");
}
catch (Exception ex)
{
    Console.Error.WriteLine($"warning: benchmark artifacts postprocessing failed: {ex.Message}");
}
