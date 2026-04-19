#pragma warning disable SA1200 // Using directives should be placed correctly

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using Convolution.Measurement;

var config = DefaultConfig.Instance
    .WithArtifactsPath("Artifacts"); // write results into "<repo root>/Artifacts"

BenchmarkRunner.Run<RandomBenchmark>(config);
