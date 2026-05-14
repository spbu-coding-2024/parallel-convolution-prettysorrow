REPO_ROOT := $(dir $(abspath $(lastword $(MAKEFILE_LIST))))
export REPO_ROOT

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

BENCHMARK_REPORT_CSV ?= $(REPO_ROOT)/Artifacts/Benchmark/Convolution.Measurement.Benchmark-report.csv
PIPELINES_BENCHMARK_REPORT_CSV ?= $(REPO_ROOT)/Artifacts/Benchmark/Convolution.Measurement.Pipelines-report.csv
UNSAFE_BENCHMARK_REPORT_CSV ?= $(REPO_ROOT)/Artifacts/Benchmark/Convolution.Measurement.Unsafe-report.csv

$(BENCHMARK_REPORT_CSV) $(PIPELINES_BENCHMARK_REPORT_CSV) $(UNSAFE_BENCHMARK_REPORT_CSV) &:
	dotnet run -c Release --project $(REPO_ROOT)/Convolution.Measurement

.PHONY: benchmark
benchmark:
	dotnet run -c Release --project $(REPO_ROOT)/Convolution.Measurement

# plots

BENCHMARK_REPORT_PNG ?= $(REPO_ROOT)/Artifacts/Benchmark/Convolution.Measurement.Benchmark-report.png
PIPELINES_BENCHMARK_REPORT_PNG ?= $(REPO_ROOT)/Artifacts/Benchmark/Convolution.Measurement.Pipelines-report.png
UNSAFE_BENCHMARK_REPORT_PNG ?= $(REPO_ROOT)/Artifacts/Benchmark/Convolution.Measurement.Unsafe-report.png

$(BENCHMARK_REPORT_PNG): venv $(BENCHMARK_REPORT_CSV)
	REPORT_PATH_CSV=$(BENCHMARK_REPORT_CSV) \
	REPORT_PATH_PNG=$(BENCHMARK_REPORT_PNG) \
	$(REPO_ROOT)/venv/bin/python $(REPO_ROOT)/scripts/make-plot.py

$(PIPELINES_BENCHMARK_REPORT_PNG): venv $(PIPELINES_BENCHMARK_REPORT_CSV)
	REPORT_PATH_CSV=$(PIPELINES_BENCHMARK_REPORT_CSV) \
	REPORT_PATH_PNG=$(PIPELINES_BENCHMARK_REPORT_PNG) \
	$(REPO_ROOT)/venv/bin/python $(REPO_ROOT)/scripts/make-plot.py

$(UNSAFE_BENCHMARK_REPORT_PNG): venv $(UNSAFE_BENCHMARK_REPORT_CSV)
	REPORT_PATH_CSV=$(UNSAFE_BENCHMARK_REPORT_CSV) \
	REPORT_PATH_PNG=$(UNSAFE_BENCHMARK_REPORT_PNG) \
	$(REPO_ROOT)/venv/bin/python $(REPO_ROOT)/scripts/make-plot.py

plots: $(BENCHMARK_REPORT_PNG) $(PIPELINES_BENCHMARK_REPORT_PNG) $(UNSAFE_BENCHMARK_REPORT_PNG)

# tests

.PHONY: test
test:
	dotnet test $(REPO_ROOT)/Convolution.Tests/Convolution.Tests.csproj --filter "Suite=All"

COVERAGE_SUMMARY_TXT ?= $(REPO_ROOT)/Artifacts/Coverage/coverage-summary.txt

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

LOCALHOST_PORT ?= 8080

preview:
	lsof -ti:$(LOCALHOST_PORT) | xargs kill -9 2>/dev/null || true
	cd $(REPO_ROOT)/Artifacts && $(REPO_ROOT)/venv/bin/python -m http.server $(LOCALHOST_PORT) & sleep 1
	open "http://localhost:$(LOCALHOST_PORT)/index.html"

kill-preview:
	lsof -ti:$(LOCALHOST_PORT) | xargs kill -9 2>/dev/null || true
