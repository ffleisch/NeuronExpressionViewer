import os

import networkx
import collada as cld
import numpy as np

import mesh_utils
from mesh_all_neurons import read_obj_inorder, write_obj


def create_neuron_mesh(path,dataset):
    print(path)
    file_name=os.path.splitext(os.path.basename(path))[0]

    graph=networkx.read_weighted_edgelist(path,create_using=networkx.DiGraph,nodetype=int)

    vertices, normals, vertex_uvs, triangles, _, _ = read_obj_inorder("./pointclouds/neuron_voronoi.obj")




    #incredibly jank
    #unity reomves line edges on mesh import, so we need triangles
    #unity also seems to remove degenerate triangles of the form (a,b,b) from the mesh, so this adds the third vertex as the next in index
    mesh_edges=np.asarray([(e[0],e[1],1+(e[1])%len(vertices)) for e in graph.edges],dtype=int)-1


    print(mesh_edges)
    mesh=mesh_utils.setup_basic_mesh()

    vertex_source=cld.geometry.source.FloatSource("vertex-array",vertices.flatten(),("X","Y","Z"))
    uv_source=cld.geometry.source.FloatSource("uvs-array",vertex_uvs.flatten(),("U","V"))

    geom=cld.geometry.Geometry(mesh,"geometry0","edge_mesh_test",[vertex_source,uv_source])

    input_list=cld.geometry.source.InputList()
    input_list.addInput(0,"VERTEX","#vertex-array")
    input_list.addInput(0,"TEXCOORD","#uvs-array")




    tri_set=geom.createTriangleSet(mesh_edges,input_list,"material0")
    geom.primitives.append(tri_set)


    save_path=os.path.join("..","activityViewer","Assets","Rescources","connectivity",dataset,"network","%s.dae"%file_name)
    os.makedirs(os.path.dirname(save_path), exist_ok=True)

    mesh_utils.save_basic_mesh(save_path,mesh,geom)

    print(graph)


dataset="viz-no-network"

dir_path=os.path.join(".","parsed_data",dataset,"network")


print(dir_path)
for f in os.listdir(dir_path):
    print(f)
    path=os.path.join(dir_path,f)
    if not os.path.isfile(path):
        continue
    create_neuron_mesh(path,dataset)