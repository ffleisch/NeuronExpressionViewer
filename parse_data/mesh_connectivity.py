import os

import networkx
import collada as cld
import numpy as np

import mesh_utils
from mesh_all_neurons import read_obj_inorder, write_obj

# vertices, normals, vertex_uvs, triangles, _, _ = read_obj_inorder("./pointclouds/neuron_voronoi.obj")

og_mesh = cld.Collada("./pointclouds/neuron_voronoi.dae")

vertex_source = og_mesh.geometries[0].sourceById["vertex"]
uv_source = og_mesh.geometries[0].sourceById["neuron-index"]
# vertex_source=cld.geometry.source.FloatSource("vertex-array",vertices.flatten(),("X","Y","Z"))
n_neurons = len(vertex_source)

index_map = np.zeros(n_neurons, dtype=int)
indices = uv_source.data[:, 0]

for i in range(n_neurons):
    index_map[int(indices[i])] = i
print(index_map)


# uv_source=cld.geometry.source.FloatSource("uvs-array",vertex_uvs.flatten(),("U","V"))

def create_neuron_mesh(path, path_last, dataset):
    print(path)
    file_name = os.path.splitext(os.path.basename(path))[0]

    graph = networkx.read_weighted_edgelist(path, create_using=networkx.DiGraph, nodetype=int)
    graph_last = networkx.read_weighted_edgelist(path_last, create_using=networkx.DiGraph, nodetype=int)

    # incredibly jank
    # unity reomves line edges on mesh import, so we need triangles
    # unity also seems to remove degenerate triangles of the form (a,b,b) from the mesh, so this adds the third vertex as the next in index

    def make_edge_triangles_from_graph(edges):
        return [(index_map[e[0] - 1], index_map[e[1] - 1], index_map[(e[1] + 1 - 1) % n_neurons]) for e in edges]

    all_edges_tris = []

    # (0,0) for persistent edges
    # (1,0) for new edges
    # (2,0) for removed edges
    all_edges_types = []

    #print(graph.nodes)
    #print(graph_last.nodes)

    if graph_last.nodes == graph.nodes:
        new_edges = networkx.difference(graph, graph_last)
        same_edges = networkx.intersection(graph, graph_last)
        removed_edges = networkx.difference(graph_last, graph)
        all_edges_tris.extend(make_edge_triangles_from_graph(same_edges.edges))
        all_edges_types.extend([(0, 0)] * len(same_edges.edges))
        all_edges_tris.extend(make_edge_triangles_from_graph(new_edges.edges))
        all_edges_types.extend([(1, 0)] * len(new_edges.edges))
        all_edges_tris.extend(make_edge_triangles_from_graph(removed_edges.edges))
        all_edges_types.extend([(2, 0)] * len(removed_edges.edges))

    else:
        all_edges_tris.extend(make_edge_triangles_from_graph(graph.edges))
        all_edges_types.extend([(1, 0)] * len(graph.edges))

    all_edges = np.asarray(all_edges_tris, dtype=int)

    edge_type = np.asarray(all_edges_types, dtype=np.single)
    edge_type_source = cld.geometry.source.FloatSource("edge-type", edge_type.flatten(), ("S", "T"))
    edge_indices = np.vstack([all_edges.flatten(), np.asarray(range(len(all_edges))).repeat(3)]).transpose().flatten()
    # print(edge_indices)

    # print(mesh_edges)
    mesh = mesh_utils.setup_basic_mesh()

    geom = cld.geometry.Geometry(mesh, "geometry0", "edge_mesh_test", [vertex_source, uv_source, edge_type_source])

    input_list = cld.geometry.source.InputList()
    input_list.addInput(0, "VERTEX", "#vertex")
    input_list.addInput(0, "TEXCOORD", "#neuron-index", set="0")

    input_list.addInput(1, "TEXCOORD", "#edge-type", set="1")

    tri_set = geom.createTriangleSet(edge_indices, input_list, "material0")
    geom.primitives.append(tri_set)

    save_path = os.path.join("..", "activityViewer", "Assets", "Resources", "connectivity", dataset, "network",
                             "%s.dae" % file_name)
    os.makedirs(os.path.dirname(save_path), exist_ok=True)

    mesh_utils.save_basic_mesh(save_path, mesh, geom)

    print(graph)


dataset = "viz-calcium"

dir_path = os.path.join(".", "parsed_data", dataset, "network")

print(dir_path)
sorted_files_paths = []
for f in os.listdir(dir_path):
    path = os.path.join(dir_path, f)
    if not os.path.isfile(path):
        continue
    parts = str.split(f, "_")
    n = int(parts[0])
    print(f, n)
    sorted_files_paths.append((n, path))
sorted_files_paths.sort(key=lambda x: x[0])
print(sorted_files_paths)

for i in range(0, len(sorted_files_paths)):
    j = max(0, i - 1)
    path_last = sorted_files_paths[j][1]
    path = sorted_files_paths[i][1]
    print(path, path_last)

    create_neuron_mesh(path, path_last, dataset)
