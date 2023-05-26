import os

import numpy as np
import pandas as pd
from matplotlib import pyplot as plt

#dataset="viz-no-network"
dataset="viz-stimulus"

if __name__=="__main__":


    path = os.path.join("..", "raw_data", "SciVisContest23", dataset,"rank_0_neurons_overview.txt")

    df=pd.read_csv(path,header=1,sep="\s\s+",engine="python")
    print(df.columns.values)
    print(df)

    for name in df:
        print(name)
        print(df[name])
        plt.plot(df[name].to_numpy())
        plt.title(name)
        plt.show()
    pass