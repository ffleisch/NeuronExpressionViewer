import os

import numpy as np
import pandas as pd
import networkx as nx
dataset = "viz-stimulus"
n_neurons = 50000
n_steps = 10000


path = os.path.join("..", "raw_data", "SciVisContest23", dataset, "network")
out_path=os.path.join(".","parsed_data",dataset,"network")
columns=["target_rank","target_id","source_rank","source_id","weight"]

steps_dict={}

#list the files in the network directory and match them up based on thier step count
#for each there is an in and an out file
#i am not sure if there is a difference in the networks
for f in os.listdir(path):
    print(f)
    base_name=os.path.splitext(f)[0]
    parts=base_name.split("_")
    print(parts)
    step,direction=int(parts[3]),parts[4]
    if not step in steps_dict:
        steps_dict[step]=[]
    steps_dict[step].append(f)
print(steps_dict)

for step,file_list in steps_dict.items():

    graph=nx.DiGraph()
    arrays=[]
    #for f in file_list:
    f=file_list[0]
    df=pd.read_csv(os.path.join(path,f),names=columns,header=4,sep=" |\t",engine="python")
    print(f,len(df.index))
    print(df)
    array=df.to_numpy()

    #the arrays for in and out depict the same graph
    #this can be seen using this lexicographic sort
    #array=array[np.lexsort((array[:,3],array[:,1]))]
    #arrays.append(array)

    for _,e,_,s,w in array:
        graph.add_edge(s,e,weight=w)


    save_path=os.path.join(out_path,"%07d_graph.edge-list"%step)
    os.makedirs(os.path.dirname(save_path), exist_ok=True)
    nx.write_weighted_edgelist(graph,save_path)

