import open3d as o3d
import numpy as np
import pandas as pd
path = "../raw_data/SciVisContest23/viz-calcium/positions/rank_0_positions.txt"

df = pd.read_csv(path, sep=" ", header=7)

# dataframe ith only the positions
positions_df = df[["<pos_x>", "<pos_y>", "<pos_z>"]]

positions = positions_df.to_numpy()

# every 10 lines are neurons closely clustered around a single point
clusters = np.reshape(positions, (5000, 10, 3))
print(clusters)

# find the centers of these clusters
# find the absolute center
centers = np.mean(clusters, axis=1)
center = np.mean(centers, axis=0)

print(centers)
print(center)

# create a pointcloud of the centers
pcd = o3d.geometry.PointCloud()
pcd.points = o3d.utility.Vector3dVector(centers)
# estimate the normals fo this pointcloud
pcd.estimate_normals(o3d.geometry.KDTreeSearchParamKNN(knn=30))
# o3d.visualization.draw_geometries([pcd], point_show_normal=True)

normals = np.asarray(pcd.normals)

# the normals are not always flipped in the right direction, i suspect because parts of the model are concave
# flip every normal that points towards the center of the model outwards
normals[np.einsum("ij,ij->i", centers - center, normals) < 0] *= -1
pcd.normals = o3d.utility.Vector3dVector(normals)
# o3d.visualization.draw_geometries([pcd], point_show_normal=True)

# project all point of the pointcloud onto the unitsphere surrounding the center
# this makes the mehsing way easier and avoids holes
# this fine to do because there are no overlapping surfaces from the center out
centered_points = centers - center
pcd.points = o3d.utility.Vector3dVector(centered_points / np.linalg.norm(centered_points, axis=1)[:, None])

# mesh the resulting sphere with the ball pivot algorithm
radii = o3d.utility.DoubleVector([0.01, 0.1, 1])
mesh = o3d.geometry.TriangleMesh.create_from_point_cloud_ball_pivoting(pcd, radii)

# set the vertices of theww mehs back to the original position from their tepmporary sphere positions
mesh.vertices = o3d.utility.Vector3dVector(centers)
mesh.compute_vertex_normals()
# mesh.normals=pcd.normals
o3d.visualization.draw_geometries([mesh, pcd], point_show_normal=True)
o3d.io.write_triangle_mesh("./pointclouds/neuron_mesh_simplified_alt.stl", mesh)

# now we do the whoe process again but for tzhe whole pointcloud and not only the centers of the clusters

# the normals are taken from the sestimated normals of th ecenters pointcloud
ex_normals = np.repeat(normals, 10, axis=0)
ex_centers = np.repeat(centers, 10, axis=0)

# vectors pointing to every point from their respective cluster center
offsets = positions - ex_centers

# the dotproduct means how far any point is out of the plane projected by the normal and clucter center
offset_dots = np.einsum("ij,ij->i", offsets, ex_normals)

# move them into the plane projected by the normal and the cluster center
flattened_offsets = offsets - ex_normals * offset_dots[:, None]

# stretch the clusters out, to make the meshing easier
flattened_positions = ex_centers + 12 * flattened_offsets

# small_pcd= o3d.geometry.PointCloud()

# small_pcd.points = o3d.utility.Vector3dVector(flattened_positions[0:100,:])

# o3d.visualization.draw_geometries([small_pcd], point_show_normal=True)


# use the same sphere trick to mehs this pointcloud
pcd_large = o3d.geometry.PointCloud()

centered_points = flattened_positions - center
positions_sphere = centered_points / np.linalg.norm(centered_points, axis=1)[:, None]

pcd_large.points = o3d.utility.Vector3dVector(positions_sphere)

radii = o3d.utility.DoubleVector([0.01, 0.1, 1])
pcd_large.normals = o3d.utility.Vector3dVector(ex_normals)
o3d.visualization.draw_geometries([pcd_large], point_show_normal=True)
mesh_large = o3d.geometry.TriangleMesh.create_from_point_cloud_ball_pivoting(pcd_large, radii)

mesh_large.vertices = o3d.utility.Vector3dVector(flattened_positions)
# mesh_large=mesh_large.filter_smooth_taubin(10)
mesh_large.compute_vertex_normals()

#set the uvs of the mesh
#they encode the original neurons index
uvs=np.zeros((len(positions), 2))
uvs[:,0]=range(len(positions))
print(uvs)
mesh_large.triangle_uvs=o3d.utility.Vector2dVector(uvs)

# save the mesh
o3d.visualization.draw_geometries([mesh_large], point_show_normal=True)
#o3d.io.write_triangle_mesh("./pointclouds/test.obj", mesh_large,write_triangle_uvs=True)

#sadly o3d doesent seem to be able to write out the uvs to the .obj
#so here we go

with open("./pointclouds/neuron_mesh_full_flattened_and_spread.obj","w") as f:
    #write vertices
    for v in mesh_large.vertices:
        f.write("v %f %f %f\n"%tuple(v))
    for n in mesh_large.vertex_normals:
        f.write("vn %f %f %f\n"%tuple(n))
    for uv in mesh_large.triangle_uvs:
        f.write("vt %d %d\n"%tuple(uv))
    for t in mesh_large.triangles:
        f.write("f %d/%d/%d %d/%d/%d %d/%d/%d}\n"%tuple(np.repeat(t+1,3)))
    pass



