import open3d as o3d
import numpy as np


orienting_mesh=o3d.io.read_triangle_mesh("./pointclouds/neuron_mesh_simplified_hand_cleaned.obj",enable_post_processing=False)

pcl=o3d.io.read_point_cloud("./pointclouds/neuron-positions.pcd")
vertices=np.asarray(orienting_mesh.vertices)
normals=np.asarray(orienting_mesh.vertex_normals)


positions=np.reshape(np.asarray(pcl.points),(5000,10,3))


o3d.visualization.draw_geometries([orienting_mesh,pcl])







