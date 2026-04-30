#!/usr/bin/env python3

import pandas
import matplotlib.pyplot as plt
import seaborn
import os

repo_root = os.environ["REPO_ROOT"]
input_path = os.environ["BENCHMARK_REPORT_CSV"]

os.makedirs(f"{repo_root}/Artifacts/Benchmark", exist_ok=True)
output_path = os.path.join(
    f"{repo_root}/Artifacts/Benchmark",
    os.path.basename(input_path).replace(".csv", ".png")
)

df = pandas.read_csv(input_path)
df = df[df["Method"].notna()]
df["Mean"] = df["Mean"].str.replace(" ms", "").str.replace(",", "").astype(float)

baseline = (
    df[df["Method"] == "Sequential"]
    .groupby(["ImageSize", "FilterSize"])["Mean"]
    .mean()
)

df["Speedup"] = df.apply(
    lambda r: baseline.loc[(r["ImageSize"], r["FilterSize"])] / r["Mean"],
    axis=1
)

seaborn.set(style="whitegrid")
plt.figure(figsize=(12, 6))

plt.subplot(1, 2, 1)
seaborn.barplot(data=df, x="ImageSize", y="Mean", hue="Method", palette="viridis")
plt.title("Mean time")
plt.ylabel("Mean time, ms")

plt.subplot(1, 2, 2)
seaborn.barplot(data=df, x="ImageSize", y="Speedup", hue="Method", palette="magma")
plt.axhline(1, color="gray", linestyle="--", linewidth=0.5)
plt.title("Speedup over Sequential")
plt.ylabel("Relative speed")

plt.tight_layout()
plt.savefig(output_path, dpi=300)
plt.close()