#!/usr/bin/env python3

import pandas
import matplotlib.pyplot as plt
import seaborn
import os

repo_root = os.environ["REPO_ROOT"]
input_path = os.environ["UNSAFE_BENCHMARK_REPORT_CSV"]
output_path = os.environ["UNSAFE_BENCHMARK_REPORT_PNG"]

os.makedirs(f"{repo_root}/Artifacts/Benchmark", exist_ok=True)

df = pandas.read_csv(input_path)
df = df[df["Method"].notna()]
df["Mean"] = df["Mean"].str.replace(" ms", "").str.replace(",", "").astype(float)

baseline = (
    df[df["Method"] == "Unsafe_MinEverything"]
    .groupby(["ImageCount", "ImageSize", "FilterSize"])["Mean"]
    .mean()
)

df["Speedup"] = df.apply(
    lambda r: baseline.loc[(r["ImageCount"], r["ImageSize"], r["FilterSize"])] / r["Mean"],
    axis=1
)

image_counts = sorted(df["ImageCount"].unique())
n_counts = len(image_counts)

seaborn.set(style="whitegrid")

fig, axes = plt.subplots(n_counts, 1, figsize=(12, 5 * n_counts))

if n_counts == 1:
    axes = [axes]

for i, cnt in enumerate(image_counts):
    subset = df[df["ImageCount"] == cnt]

    seaborn.barplot(
        data=subset,
        x="ImageSize",
        y="Speedup",
        hue="Method",
        palette="magma",
        ax=axes[i]
    )
    axes[i].axhline(1, color="gray", linestyle="--", linewidth=0.5)
    axes[i].set_title(f"ImageCount = {cnt}")
    axes[i].set_ylabel("Relative speed")

plt.tight_layout()
plt.savefig(output_path, dpi=300)
plt.close()
