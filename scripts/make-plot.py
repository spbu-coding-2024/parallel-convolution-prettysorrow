#!/usr/bin/env python3

import pandas
import matplotlib.pyplot as plt
import seaborn
import os
import numpy as np

repo_root = os.environ["REPO_ROOT"]
input_path = os.environ["REPORT_PATH_CSV"]
output_path = os.environ["REPORT_PATH_PNG"]

os.makedirs(f"{repo_root}/Artifacts/Benchmark", exist_ok=True)

df = pandas.read_csv(input_path)
df = df[df["Method"].notna()]
df["Mean"] = df["Mean"].str.replace(" ms", "").str.replace(",", "").astype(float)

baseline_rows = df[df["Baseline"].astype(str).str.strip() == "Yes"]
baseline = baseline_rows.groupby(["ImageCount", "ImageSize", "FilterSize"])["Mean"].mean()

df["Speedup"] = df.apply(
    lambda r: baseline.loc[(r["ImageCount"], r["ImageSize"], r["FilterSize"])] / r["Mean"],
    axis=1
)

image_counts = sorted(df["ImageCount"].unique())
image_sizes = sorted(df["ImageSize"].unique())
n_counts = len(image_counts)
n_sizes = len(image_sizes)

seaborn.set(style="whitegrid")

fig, axes = plt.subplots(n_counts, n_sizes, figsize=(12, 5 * n_counts))
axes = np.atleast_2d(axes)

for i, cnt in enumerate(image_counts):
    for j, sz in enumerate(image_sizes):
        subset = df[(df["ImageCount"] == cnt) & (df["ImageSize"] == sz)]

        seaborn.barplot(
            data=subset,
            x="FilterSize",
            y="Speedup",
            hue="Method",
            palette="magma",
            ax=axes[i, j]
        )
        axes[i, j].axhline(1, color="gray", linestyle="--", linewidth=0.5)
        axes[i, j].set_title(f"ImageCount = {cnt}, ImageSize = {sz}")
        axes[i, j].set_ylabel("Relative speed")

for ax in axes.flat:
    handles, labels = ax.get_legend_handles_labels()
    if handles:
        max_cols = 4
        estimated_cols = max(1, min(max_cols, len(labels)))
        
        legend = fig.legend(
            handles, labels,
            title="Method",
            loc="lower center",
            bbox_to_anchor=(0.5, -0.08),
            ncol=estimated_cols,
            frameon=True,
            handlelength=1.5,
            columnspacing=1.0,
            labelspacing=0.4,
            handletextpad=0.5
        )
        
        for text in legend.get_texts():
            text.set_wrap(True)
        
        for ax_inner in axes.flat:
            leg = ax_inner.get_legend()
            if leg:
                leg.remove()
        break

plt.tight_layout(rect=[0, 0.12, 1, 1])
plt.savefig(output_path, dpi=300, bbox_inches="tight")
plt.close()
