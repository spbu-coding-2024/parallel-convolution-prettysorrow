deps-brew:
	brew install dotnet-sdk
	brew install python
	dotnet restore
	dotnet tool restore

clean:
	dotnet clean
	rm -rf Artifacts/*

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

restore:
	dotnet restore
	dotnet tool restore

coverage: restore
	dotnet test ./Convolution.Tests/Convolution.Tests.csproj \
		/p:CollectCoverage=true \
		/p:Include="[Convolution.*]*" \
		/p:Exclude="[Convolution.Tests]*" \
		/p:CoverletOutput="../Artifacts/coverage.xml" \
		/p:CoverletOutputFormat="cobertura"

	dotnet tool run reportgenerator -- \
		-reports:"./Artifacts/coverage.xml" \
		-targetdir:"./Artifacts" \
		-reporttypes:"TextSummary"

	mv ./Artifacts/Summary.txt ./Artifacts/coverage-summary.txt
	cat ./Artifacts/coverage-summary.txt
