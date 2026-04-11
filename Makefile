deps-brew:
	brew install dotnet-sdk
	brew install python

clean:
	dotnet clean
	rm -rf Artifacts

.PHONY: test
test:
	dotnet test

benchmark:
	dotnet run -c Release --project Convolution.Measurement

venv:
	python3 -m venv venv
	venv/bin/pip install matplotlib pandas seaborn

CSV_REPORT ?= Artifacts/results/Convolution.Measurement.RandomBenchmark-report.csv

$(CSV_REPORT):
	dotnet run -c Release --project Convolution.Measurement

plot: venv $(CSV_REPORT)
	CSV_REPORT=$(CSV_REPORT) venv/bin/python ./scripts/plot.py
