# Convolution

Image convolution implementations

## Usage

```sh
make test        # run tests
make coverage    # run tests with coverage report
make benchmark   # run benchmarks on random images
make plot        # visualize benchmark results
```

All output is written to `Artifacts/`.

## Benchmarks

`make benchmark` measures several convolution implementations across different image sizes and filter sizes:

- `Sequential` — single-threaded baseline
- `Parallel_Rows`  — parallelism over rows
- `Parallel_Columns` — parallelism over columns
- `Parallel_Tiles` — tile-based parallelism

`make plot` generates a PNG chart from the last benchmark run

![Benchmark results](Artifacts/Benchmark/Convolution.Measurement.Benchmark-report.png)
