#pragma warning disable SA1200 // Using directives should be placed correctly

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

var config = DefaultConfig.Instance
                .WithArtifactsPath("Artifacts") // writes results to <repo root>/Artifacts/
                .WithOption(ConfigOptions.DisableLogFile, true); // no .log file

BenchmarkRunner.Run<Convolution.Measurement.Benchmark>(config);
BenchmarkRunner.Run<Convolution.Measurement.Pipelines>(config);
BenchmarkRunner.Run<Convolution.Measurement.Unsafe>(config);

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
    File.Delete("Artifacts/Benchmark/Convolution.Measurement.Pipelines-report-default.md");
    File.Delete("Artifacts/Benchmark/Convolution.Measurement.Unsafe-report-default.md");
    File.Move("Artifacts/Benchmark/Convolution.Measurement.Benchmark-report-github.md", "Artifacts/Benchmark/Convolution.Measurement.Benchmark-report.md");
    File.Move("Artifacts/Benchmark/Convolution.Measurement.Pipelines-report-github.md", "Artifacts/Benchmark/Convolution.Measurement.Pipelines-report.md");
    File.Move("Artifacts/Benchmark/Convolution.Measurement.Unsafe-report-github.md", "Artifacts/Benchmark/Convolution.Measurement.Unsafe-report.md");
}
catch (Exception ex)
{
    Console.Error.WriteLine($"warning: benchmark artifacts postprocessing failed: {ex.Message}");
}
