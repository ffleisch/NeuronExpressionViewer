import open3d as o3d
import numpy as np
import open3d.utility
import pandas as pd
from scipy import spatial
from scipy.special.cython_special import poch

import mesh_all_neurons
from mesh_all_neurons import mesh_points_sphere,  encode_uvs

orienting_mesh = o3d.t.io.read_triangle_mesh("./pointclouds/neuron_mesh_very_smooth.obj", enable_post_processing=False)
draw_mesh = orienting_mesh.to_legacy()
# vertices=np.asarray(orienting_mesh.vertices)
# normals=np.asarray(orienting_mesh.vertex_normals)


# pcl=o3d.io.read_point_cloud("./pointclouds/neuron-positions.pcd")
# positions=np.reshape(np.asarray(pcl.points,dtype=np.single),(50000,3))

#spread_mesh = o3d.io.read_triangle_mesh("./pointclouds/neuron_mesh_full_flattened_and_spread.obj",
#                                        enable_post_processing=False)
#positions = np.asarray(spread_mesh.vertices, dtype=np.single)

positions=np.load("./pointclouds/neuron_positions_flattened_and_spread.npy")

def project_vertices(points):
    closest_res = scene.compute_closest_points(o3d.core.Tensor(points, o3d.core.float32))

    return closest_res["points"].numpy(),closest_res["primitive_normals"].numpy()



pcl = o3d.geometry.PointCloud()
pcl.points = o3d.utility.Vector3dVector(positions)

o3d.visualization.draw_geometries([draw_mesh, pcl])





scene = o3d.t.geometry.RaycastingScene()
print(type(orienting_mesh))
scene.add_triangles(orienting_mesh)

c_points,c_normals=project_vertices(positions)
c_start_flattened=np.array(c_points)

pcl_closest = o3d.geometry.PointCloud()
pcl_closest.points = o3d.utility.Vector3dVector(c_points)
o3d.visualization.draw_geometries([draw_mesh, pcl_closest])



line_indices=[(x,x+len(positions)) for x in range(len(positions))]


pcl_ref=pcl
iters=5000
max_len=0.5
for i in range(iters):

    vor = spatial.Voronoi(c_points)

    centroids = np.zeros_like(c_points)

    for j in range(len(c_points)):
        region = vor.regions[vor.point_region[j]]
        region = [x for x in region if x >= 0]

        # print(region)
        # print(np.mean(vor.vertices[region],axis=0))
        centroids[j] = np.mean(vor.vertices[region], axis=0)

        pass

    movement_vector=centroids-c_points
    lengths=np.linalg.norm(movement_vector,axis=1)
    movement_vector[lengths>max_len,:]*=(max_len/lengths[lengths>max_len,None])


    dots = np.einsum("ij,ij->i", movement_vector, c_normals)
    movement_vector=movement_vector-c_normals*dots[:,None]

    #movement_vector/=np.reshape(np.repeat(lengths,3),movement_vector.shape)
    #lengths=np.reshape(np.repeat(lengths,3),movement_vector.shape)
    #too_long=lengths>max_len
    #movement_vector[too_long,0]/=(lengths[too_long]/max_len)#(movement_vector/lengths)*max_len #jank


    movement_vector=movement_vector#movement_vector/np.linalg.norm(movement_vector)

    c_points=c_points+ movement_vector
    c_points,c_normals=project_vertices(c_points)


    lines=o3d.t.geometry.LineSet()
    lines.point.positions=o3d.core.Tensor(np.vstack((c_start_flattened,c_points)))
    lines.line.indices=o3d.core.Tensor(line_indices)

    print("iteration:", i)
    if(i==iters-1):
        pcl_current = o3d.geometry.PointCloud()
        pcl_current.points = o3d.utility.Vector3dVector(c_points)
        pcl_current.normals=o3d.utility.Vector3dVector(c_normals)
        o3d.visualization.draw_geometries([pcl_current, lines.to_legacy()],point_show_normal=False)

pcl_out = o3d.geometry.PointCloud()
pcl_out.points = o3d.utility.Vector3dVector(c_points)
pcl_out.normals=o3d.utility.Vector3dVector(c_normals)
o3d.io.write_point_cloud("pointclouds/neuron_positions_voronoi_spread.pcd",pcl_out)

mesh=mesh_points_sphere(c_points, c_normals)

o3d.visualization.draw_geometries([mesh])

df = pd.read_csv(mesh_all_neurons.path, sep=" ", header=7)
areas=pd.to_numeric(df["<area>"].str.split("_",expand=True)[1]).to_numpy()
uvs=encode_uvs(c_points,areas)

#o3d.io.write_triangle_mesh("pointclouds/test_write.obj",mesh)

mesh_all_neurons.write_obj("pointclouds/neuron_voronoi.obj", c_points, np.asarray(mesh.triangles), np.asarray(mesh.vertex_normals), uvs)

print("done")