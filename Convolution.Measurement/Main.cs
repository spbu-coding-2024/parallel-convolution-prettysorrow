using Convolution.Measurement;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

var config = DefaultConfig.Instance
    .WithArtifactsPath("Artifacts"); // write results into "<repo root>/Artifacts"

BenchmarkRunner.Run<RandomBenchmark>(config);
