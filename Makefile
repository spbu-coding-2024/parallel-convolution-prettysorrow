REPO_ROOT := $(dir $(abspath $(lastword $(MAKEFILE_LIST))))

export REPO_ROOT

clean:
	dotnet clean
	rm -rf $(REPO_ROOT)/Artifacts/Coverage
	rm -rf $(REPO_ROOT)/Artifacts/Benchmark

restore:
	dotnet restore
	dotnet tool restore

.PHONY: test
test:
	dotnet test $(REPO_ROOT)/Convolution.Tests/Convolution.Tests.csproj --filter "Suite=All"

coverage: restore
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

	mv $(REPO_ROOT)/Artifacts/Coverage/Summary.txt $(REPO_ROOT)/Artifacts/Coverage/coverage-summary.txt
	cat $(REPO_ROOT)/Artifacts/Coverage/coverage-summary.txt
