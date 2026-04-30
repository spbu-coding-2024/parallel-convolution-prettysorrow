#pragma warning disable SA1200 // Using directives should be placed correctly

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using Convolution.Measurement;

var config = DefaultConfig.Instance
                .WithArtifactsPath("Artifacts") // writes results to <repo root>/Artifacts/
                .WithOption(ConfigOptions.DisableLogFile, true); // no .log file

BenchmarkRunner.Run<RandomBenchmark>(config);

try
{
    Directory.Delete("Artifacts/RandomBenchmark", recursive: true);
}
catch (DirectoryNotFoundException)
{
}

try
{
    Directory.Move("Artifacts/results", "Artifacts/RandomBenchmark");
    File.Delete("Artifacts/RandomBenchmark/Convolution.Measurement.RandomBenchmark-report-default.md");
    File.Move("Artifacts/RandomBenchmark/Convolution.Measurement.RandomBenchmark-report-github.md", "Artifacts/RandomBenchmark/Convolution.Measurement.RandomBenchmark-report.md");
}
catch (Exception ex)
{
    Console.Error.WriteLine($"warning: benchmark artifacts postprocessing failed: {ex.Message}");
}
