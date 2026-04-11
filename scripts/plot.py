#!/usr/bin/env python3

import pandas
import matplotlib.pyplot as plt
import seaborn
import os

input_path = os.environ["CSV_REPORT"]

os.makedirs("Artifacts/results", exist_ok=True)
output_path = os.path.join("Artifacts/results", os.path.basename(input_path).replace(".csv", ".png"))

df = pandas.read_csv(input_path)

df["Mean"] = df["Mean"].str.replace(" ms", "").str.replace(",", "").astype(float)
df["Allocated"] = df["Allocated"].str.replace(" B", "").astype(float) / 1024

seaborn.set(style="whitegrid")
plt.figure(figsize=(18, 6))

# mean time
plt.subplot(1, 3, 1)
seaborn.barplot(data=df, x="ImageSize", y="Mean", hue="Method", palette="viridis")
plt.title("Mean time")
plt.ylabel("Mean time, ms")

# speedup
plt.subplot(1, 3, 2)
baseline = df[df["Method"] == "Sequential"].set_index(["ImageSize", "FilterSize"])["Mean"]
df["Speedup"] = df.apply(lambda r: baseline.get((r["ImageSize"], r["FilterSize"]), r["Mean"]) / r["Mean"], axis=1)
seaborn.barplot(data=df, x="ImageSize", y="Speedup", hue="Method", palette="magma")
plt.axhline(1, color="gray", linestyle="--", linewidth=0.5)
plt.title("Speedup over Sequential")
plt.ylabel("Relative speed")

# allocated memory
plt.subplot(1, 3, 3)
seaborn.barplot(data=df, x="ImageSize", y="Allocated", hue="Method", palette="crest")
plt.title("Allocated memory")
plt.ylabel("Allocated memory, KB")

plt.tight_layout()
plt.savefig(output_path, dpi=300)
plt.close()
