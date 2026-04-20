REPO_ROOT := $(dir $(abspath $(lastword $(MAKEFILE_LIST))))
BENCHMARK_REPORT_CSV ?= $(REPO_ROOT)/Artifacts/RandomBenchmark/Convolution.Measurement.RandomBenchmark-report.csv

export REPO_ROOT
export BENCHMARK_REPORT_CSV

deps-brew:
	brew install dotnet-sdk
	brew install python
	dotnet restore
	dotnet tool restore

clean:
	dotnet clean
	rm -rf $(REPO_ROOT)/Artifacts/*

.PHONY: test
test:
	dotnet test

benchmark:
	dotnet run -c Release --project $(REPO_ROOT)/Convolution.Measurement

venv:
	python3 -m venv $(REPO_ROOT)/venv
	$(REPO_ROOT)/venv/bin/pip install matplotlib pandas seaborn

$(BENCHMARK_REPORT_CSV):
	dotnet run -c Release --project $(REPO_ROOT)/Convolution.Measurement

plot: venv $(BENCHMARK_REPORT_CSV)
	$(REPO_ROOT)/venv/bin/python $(REPO_ROOT)/scripts/plot.py

restore:
	dotnet restore
	dotnet tool restore

coverage: restore
	dotnet test $(REPO_ROOT)/Convolution.Tests/Convolution.Tests.csproj \
		/p:CollectCoverage=true \
		/p:Include="[$(REPO_ROOT)/Convolution.*]*" \
		/p:Exclude="[$(REPO_ROOT)/Convolution.Tests]*" \
		/p:CoverletOutput="$(REPO_ROOT)/Artifacts/Coverage/coverage-report.xml" \
		/p:CoverletOutputFormat="cobertura"

	dotnet tool run reportgenerator -- \
		-reports:"$(REPO_ROOT)/Artifacts/Coverage/coverage-report.xml" \
		-targetdir:"$(REPO_ROOT)/Artifacts/Coverage" \
		-reporttypes:"TextSummary"

	mv $(REPO_ROOT)/Artifacts/Coverage/Summary.txt $(REPO_ROOT)/Artifacts/Coverage/coverage-summary.txt
