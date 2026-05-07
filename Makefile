# env

REPO_ROOT := $(dir $(abspath $(lastword $(MAKEFILE_LIST))))

BENCHMARK_REPORT_CSV ?= $(REPO_ROOT)/Artifacts/Benchmark/Convolution.Measurement.Benchmark-report.csv
PIPELINES_BENCHMARK_REPORT_CSV ?= $(REPO_ROOT)/Artifacts/Benchmark/Convolution.Measurement.Pipelines-report.csv

BENCHMARK_REPORT_PNG ?= $(REPO_ROOT)/Artifacts/Benchmark/Convolution.Measurement.Benchmark-report.png
PIPELINES_BENCHMARK_REPORT_PNG ?= $(REPO_ROOT)/Artifacts/Benchmark/Convolution.Measurement.Pipelines-report.png

COVERAGE_SUMMARY_TXT ?= $(REPO_ROOT)/Artifacts/Coverage/coverage-summary.txt

export REPO_ROOT
export BENCHMARK_REPORT_CSV
export PIPELINES_BENCHMARK_REPORT_CSV
export BENCHMARK_REPORT_PNG
export PIPELINES_BENCHMARK_REPORT_PNG
export COVERAGE_SUMMARY_TXT

# deps

deps-brew:
	brew install dotnet-sdk
	brew install python
	dotnet restore
	dotnet tool restore

venv:
	python3 -m venv $(REPO_ROOT)/venv
	$(REPO_ROOT)/venv/bin/pip install matplotlib pandas seaborn

restore:
	dotnet restore
	dotnet tool restore

# benchmarks

$(BENCHMARK_REPORT_CSV) $(PIPELINES_BENCHMARK_REPORT_CSV) &:
	dotnet run -c Release --project $(REPO_ROOT)/Convolution.Measurement

.PHONY: benchmark
benchmark: $(BENCHMARK_REPORT_CSV) $(PIPELINES_BENCHMARK_REPORT_CSV)

# plots

$(BENCHMARK_REPORT_PNG): venv $(BENCHMARK_REPORT_CSV)
	$(REPO_ROOT)/venv/bin/python $(REPO_ROOT)/scripts/plot.py

$(PIPELINES_BENCHMARK_REPORT_PNG): venv $(PIPELINES_BENCHMARK_REPORT_CSV)
	$(REPO_ROOT)/venv/bin/python $(REPO_ROOT)/scripts/pipelines-plot.py

plots: $(BENCHMARK_REPORT_PNG) $(PIPELINES_BENCHMARK_REPORT_PNG)

# tests

.PHONY: test
test:
	dotnet test $(REPO_ROOT)/Convolution.Tests/Convolution.Tests.csproj --filter "Suite=All"

$(COVERAGE_SUMMARY_TXT): restore
	dotnet test $(REPO_ROOT)/Convolution.Tests/Convolution.Tests.csproj \
		--filter "Suite=Coverage" \
		/p:CollectCoverage=true \
		/p:Include="[$(REPO_ROOT)/Convolution.*]*" \
		/p:Exclude="[$(REPO_ROOT)/Convolution.Tests]*" \
		/p:CoverletOutput="$(REPO_ROOT)/Artifacts/Coverage/coverage-report.xml" \
		/p:CoverletOutputFormat="cobertura"

	dotnet tool run reportgenerator -- \
		-reports:"$(REPO_ROOT)/Artifacts/Coverage/coverage-report.xml" \
		-targetdir:"$(REPO_ROOT)/Artifacts/Coverage" \
		-reporttypes:"TextSummary"

	mv $(REPO_ROOT)/Artifacts/Coverage/Summary.txt $(COVERAGE_SUMMARY_TXT)
	cat $(COVERAGE_SUMMARY_TXT)

.PHONY: coverage
coverage: $(COVERAGE_SUMMARY_TXT)

# other

clean:
	dotnet clean
	rm -rf $(REPO_ROOT)/Artifacts/Coverage
	rm -rf $(REPO_ROOT)/Artifacts/Benchmark
