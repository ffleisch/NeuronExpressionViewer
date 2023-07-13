import open3d as o3d
import numpy as np
import open3d.utility
import pandas as pd
from scipy import spatial
from scipy.special.cython_special import poch

import mesh_all_neurons
import mesh_utils
from mesh_all_neurons import mesh_points_sphere, encode_uvs

import collada as cld


from matplotlib import pyplot as plt, patches
from matplotlib import tri

orienting_mesh = o3d.t.io.read_triangle_mesh("./pointclouds/neuron_mesh_very_smooth.obj", enable_post_processing=False)
draw_mesh = orienting_mesh.to_legacy()
# vertices=np.asarray(orienting_mesh.vertices)
# normals=np.asarray(orienting_mesh.vertex_normals)


# pcl=o3d.io.read_point_cloud("./pointclouds/neuron-positions.pcd")
# positions=np.reshape(np.asarray(pcl.points,dtype=np.single),(50000,3))

# spread_mesh = o3d.io.read_triangle_mesh("./pointclouds/neuron_mesh_full_flattened_and_spread.obj",
#                                        enable_post_processing=False)
# positions = np.asarray(spread_mesh.vertices, dtype=np.single)

positions = np.load("./pointclouds/neuron_positions_flattened_and_spread.npy")
n_neurons=len(positions)
print(n_neurons)

def project_vertices(points):
    closest_res = scene.compute_closest_points(o3d.core.Tensor(points, o3d.core.float32))

    return closest_res["points"].numpy(), closest_res["primitive_normals"].numpy()


pcl = o3d.geometry.PointCloud()
pcl.points = o3d.utility.Vector3dVector(positions)

#o3d.visualization.draw_geometries([draw_mesh, pcl])

scene = o3d.t.geometry.RaycastingScene()
print(type(orienting_mesh))
scene.add_triangles(orienting_mesh)

c_points, c_normals = project_vertices(positions)
c_start_flattened = np.array(c_points)

pcl_closest = o3d.geometry.PointCloud()
pcl_closest.points = o3d.utility.Vector3dVector(c_points)
#o3d.visualization.draw_geometries([draw_mesh, pcl_closest])

line_indices = [(x, x + len(positions)) for x in range(len(positions))]

pcl_ref = pcl
iters = 1
max_len = 0.5
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

    movement_vector = centroids - c_points
    lengths = np.linalg.norm(movement_vector, axis=1)
    movement_vector[lengths > max_len, :] *= (max_len / lengths[lengths > max_len, None])

    dots = np.einsum("ij,ij->i", movement_vector, c_normals)
    movement_vector = movement_vector - c_normals * dots[:, None]

    # movement_vector/=np.reshape(np.repeat(lengths,3),movement_vector.shape)
    # lengths=np.reshape(np.repeat(lengths,3),movement_vector.shape)
    # too_long=lengths>max_len
    # movement_vector[too_long,0]/=(lengths[too_long]/max_len)#(movement_vector/lengths)*max_len #jank

    movement_vector = movement_vector  # movement_vector/np.linalg.norm(movement_vector)

    c_points = c_points + movement_vector
    c_points, c_normals = project_vertices(c_points)

    lines = o3d.t.geometry.LineSet()
    lines.point.positions = o3d.core.Tensor(np.vstack((c_start_flattened, c_points)))
    lines.line.indices = o3d.core.Tensor(line_indices)

    print("iteration:", i)
    if (i == iters - 1):
        pcl_current = o3d.geometry.PointCloud()
        pcl_current.points = o3d.utility.Vector3dVector(c_points)
        pcl_current.normals = o3d.utility.Vector3dVector(c_normals)
        #o3d.visualization.draw_geometries([pcl_current, lines.to_legacy()], point_show_normal=False)

pcl_out = o3d.geometry.PointCloud()
pcl_out.points = o3d.utility.Vector3dVector(c_points)
pcl_out.normals = o3d.utility.Vector3dVector(c_normals)
o3d.io.write_point_cloud("pointclouds/neuron_positions_voronoi_spread.pcd", pcl_out)

mesh = mesh_points_sphere(c_points, c_normals)

#o3d.visualization.draw_geometries([mesh])

df = pd.read_csv(mesh_all_neurons.path, sep=" ", header=7)
areas = pd.to_numeric(df["<area>"].str.split("_", expand=True)[1]).to_numpy()
uvs = encode_uvs(c_points, areas)

# o3d.io.write_triangle_mesh("pointclouds/test_write.obj",mesh)




center=np.mean(c_points,axis=0)
print(center)
tris=np.asarray(mesh.triangles)
uv_triangle_map=np.zeros_like(tris)

uvx=[]
uvy=[]
signs=[]
for t in tris:
    points=c_points[t]-center
    a=np.arccos(points[:,0]/np.linalg.norm(points,axis=1))/np.pi#np.arctan2(points[:,0],points[:,1])/(np.pi*2)+0.5
    b=np.arctan2(points[:,1],points[:,2])/(np.pi*2)+0.5
    #print(["(%f,%f)"%(x,y)for x,y in zip(a,b)])
    #sign_pre=np.dot(np.cross(points[2]-points[0],points[1]-points[0]),points[0])
    sign=np.cross((a[2]-a[0],b[2]-b[0],0),(a[1]-a[0],b[1]-b[0],0))
    sign=sign[2]<0
    if sign:
        b[b<0.5]+=1
    signs.append(sign)
    #if(sign):
    uvx.extend(a)
    uvy.extend(b)
uv_map=np.hstack([uvx,uvy])

#signs=np.asarray(signs).repeat(3).reshape((-1,3))
#signs=["r" if x else "k" for x in signs]
#print(len(signs))

uv_triangle_map=np.asarray(range(len(uvx))).reshape((-1,3))

my_tri=tri.Triangulation(uvx,uvy,triangles=uv_triangle_map)

print(len(my_tri.triangles))
#plt.gca().tripcolor(my_tri,signs,alpha=0.2,mask=signs)
plt.gca().triplot(my_tri,linewidth=0.5,alpha=1)
plt.show()

new_mesh = mesh_utils.setup_basic_mesh()

#uvs=np.hstack([uvs/n_neurons,np.zeros_like(uvs)])
print(uvs)

vertex_source = cld.geometry.source.FloatSource("vertex", c_points.flatten(), ("X", "Y", "Z"))
uv_source = cld.geometry.source.FloatSource("uv", uv_map.flatten(), ("U","V"))
#normal_source = cld.geometry.source.FloatSource("normal", np.asarray(mesh.vertex_normals).flatten(), ("X", "Y", "Z"))

geom = cld.geometry.Geometry(new_mesh, "geometry0", "voronoi_spread_neurons", [vertex_source, uv_source])
input_list = cld.geometry.source.InputList()
input_list.addInput(0, "VERTEX", "#vertex")
#input_list.addInput(0, "NORMAL", "#normal")
input_list.addInput(1, "TEXCOORD","#uv",set="0")
# holds coded information for neuron and area information
#input_list.addInput(0, "TEXCOORD0",
#                    "#neuron-index-array")  # Pycollada currently only supports one texcoord input, so im nack to using the color in its place

combined_indices=np.vstack([np.asarray(mesh.triangles).flatten(),uv_triangle_map.flatten()]).transpose().flatten()
print(combined_indices)
tri_set = geom.createTriangleSet(combined_indices, input_list, "material0")
geom.primitives.append(tri_set)
save_path = "pointclouds/neuron_voronoi.dae"
mesh_utils.save_basic_mesh(save_path, new_mesh, geom)
# mesh_all_neurons.write_obj("pointclouds/neuron_voronoi.obj", c_points, np.asarray(mesh.triangles), np.asarray(mesh.vertex_normals), uvs)

print("done")
