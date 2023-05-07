import numpy as np
import pandas as pd
import os

dataset = "viz-no-network"

n_neurons = 50000
n_steps = 10000
max_memory = 8e9

path = os.path.join("..", "raw_data", "SciVisContest23", dataset, "monitors")

cols = ["step", "fired", "fired fraction", "activity", "dampening", "current calcium", "target calcium",
        "synaptic input", "background input", "grown axons", "connected axons", "grown dendrites",
        "connected dendrites"]

col_dtypes = {n: t for n, t in zip(cols,
                                   [np.int, np.bool, np.single, np.single, np.single, np.single, np.single, np.single,
                                    np.single, np.int, np.int, np.int, np.int])}
print(col_dtypes)

max_arrays_at_once = int(max_memory / (n_neurons * n_steps * 4))
print(max_arrays_at_once)

col_groups = []
group = []
for c in cols:
    group.append(c)
    if len(group) == max_arrays_at_once:
        col_groups.append(group)
        group = []
col_groups.append(group)
print(col_groups)

for i, g in enumerate(col_groups):
    arrays = {name: np.zeros((n_neurons, n_steps), dtype=col_dtypes[name]) for name in g}
    num = 0
    for f in os.listdir(path):
        base_name = os.path.splitext(f)[0]
        n = int(base_name.split("_")[1])
        df = pd.read_csv(os.path.join(path, f), header=None, sep=";", names=cols)
        for name in g:
            arrays[name][n, :] = df[name]
        # l=np.genfromtxt(os.path.join(path,f),delimiter=";")
        # array[n,:]=l[:,i]

        num += 1
        if num % 100 == 0:
            print("%d/%d %.2f" % (i + 1, len(col_groups), 100 * float(num) / n_neurons))
    for name in g:
        save_path = os.path.join("..", "parse_data", "parsed_data", dataset, "monitors", name + ".npy")
        os.makedirs(os.path.dirname(save_path), exist_ok=True)
        print("saving array ", name)
        # with open(save_path,"wb") as f:
        print(arrays[name])
        np.save(save_path, arrays[name])
