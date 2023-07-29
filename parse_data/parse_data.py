import numpy as np
import pandas as pd
import os

dataset = "viz-calcium"

n_neurons = 50000
n_steps = 10000
max_memory = 10e9

path = os.path.join("..", "raw_data", "SciVisContest23", dataset, "monitors")

#names of the columns of attributes in the csv files
attributes = ["step", "fired", "fired fraction", "activity", "dampening", "current calcium", "target calcium",
        "synaptic input", "background input", "grown axons", "connected axons", "grown dendrites",
        "connected dendrites"]
#datatypes for the attributes
col_dtypes = {n: t for n, t in zip(attributes,
                                   [np.int, np.bool, np.single, np.single, np.single, np.single, np.single, np.single,
                                    np.single, np.int, np.int, np.int, np.int])}
print(col_dtypes)

#estimate how many of the full numpy arrays (neurons x steps) fit in memory
max_arrays_at_once = int(max_memory / (n_neurons * n_steps * 4))
print(max_arrays_at_once)

#split the attributes into groups, such that their arrays are not larger than the memory limit
col_groups = []
group = []
for c in attributes:
    group.append(c)
    if len(group) == max_arrays_at_once:
        col_groups.append(group)
        group = []
col_groups.append(group)
print(col_groups)


#for each group of attributes
for i, g in enumerate(col_groups):
    #init empty arrays of size (number oif neurons, number of steps)
    arrays = {name: np.zeros((n_neurons, n_steps), dtype=col_dtypes[name]) for name in g}
    num = 0

    #iterate over the csv files for each neuron and update corresponding columns in the current arrays
    for f in os.listdir(path):
        base_name = os.path.splitext(f)[0]
        n = int(base_name.split("_")[1])
        df = pd.read_csv(os.path.join(path, f), header=None, sep=";", names=attributes)
        for name in g:
            arrays[name][n, :] = df[name]
        # l=np.genfromtxt(os.path.join(path,f),delimiter=";")
        # array[n,:]=l[:,i]

        num += 1
        if num % 100 == 0:
            print("%d/%d %.2f" % (i + 1, len(col_groups), 100 * float(num) / n_neurons))
    #for each array save the numpy arrayas an .npy
    for name in g:
        save_path = os.path.join("..", "parse_data", "parsed_data", dataset, "monitors", name + ".npy")
        os.makedirs(os.path.dirname(save_path), exist_ok=True)
        print("saving array ", name)
        # with open(save_path,"wb") as f:
        print(arrays[name])
        np.save(save_path, arrays[name])
