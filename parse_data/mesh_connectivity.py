import networkx
import collada as cld
import numpy as np

from mesh_all_neurons import read_obj_inorder, write_obj

dataset="viz-no-network"

path="./parsed_data/%s/network/0150000_graph.edge-list"%(dataset)

graph=networkx.read_weighted_edgelist(path,create_using=networkx.DiGraph,nodetype=int)

vertices, normals, vertex_uvs, triangles, _, _ = read_obj_inorder("./pointclouds/neuron_voronoi.obj")



print([e for e in graph.edges])

mesh_edges=np.asarray([e for e in graph.edges],dtype=int)-1

mesh=cld.Collada()
vertex_source=cld.geometry.source.FloatSource("vertex-array",vertices.flatten(),("X","Y","Z"))
uv_source=cld.geometry.source.FloatSource("uvs-array",vertex_uvs.flatten(),("U","V"))

geom=cld.geometry.Geometry(mesh,"geometry0","edge_mesh_test",[vertex_source,uv_source])

input_list=cld.geometry.source.InputList()
input_list.addInput(0,"VERTEX","#vertex-array")
input_list.addInput(0,"TEXCOORD","#uvs-array")

line_set=geom.createLineSet(mesh_edges,input_list,"Default")
geom.primitives.append(line_set)

mesh.geometries.append(geom)


geomNode=cld.scene.GeometryNode(geom)
node=cld.scene.Node("node0",children=[geomNode])

myscene=cld.scene.Scene("myscene",[node])
mesh.scenes.append(myscene)
mesh.scene=myscene
mesh.write("./pointclouds/test.dae")
print(graph)
