import copy

from mesh_all_neurons import read_obj_inorder,write_obj
import open3d as o3d
import numpy as np
import scipy.spatial
verts, normals, uvs, tris, tri_normals, tri_uvs = read_obj_inorder("./pointclouds/neuron_voronoi.obj")

print(verts, normals, uvs, tris, tri_normals, tri_uvs)




#bubble= o3d.geometry.TriangleMesh.create_sphere(radius=0.5,resolution=2)
#bubble.compute_vertex_normals()

bubble=o3d.io.read_triangle_mesh("pointclouds/particle_meshes/chip.obj")

bubble.compute_vertex_normals()

o3d.visualization.draw_geometries([bubble])

new_mesh=o3d.geometry.TriangleMesh()
#all_bubbles=[]
new_verts=[]
new_normals=[]
new_triangles=[]
new_uvs=[]

for v,n,uv in zip(verts,normals,uvs):
    newbubble=copy.deepcopy(bubble)
    newbubble.translate(v)


    cross_direction=np.cross((1,0,0),n)
    cross_direction=cross_direction/np.linalg.norm(cross_direction)

    angle=np.arccos(n[0]/np.linalg.norm(n))
    cross_direction*=angle

    rotation_matrix=scipy.spatial.transform.Rotation.from_rotvec(cross_direction).as_matrix()
    newbubble.rotate(rotation_matrix)
    #new_verts.extend(newbubble.vertices)

    new_mesh+=newbubble

    #all_bubbles.append(newbubble)
    #o3d.visualization.draw_geometries(all_bubbles)

normals_pcd=o3d.geometry.PointCloud()
normals_pcd.points=o3d.utility.Vector3dVector(verts)
normals_pcd.normals=o3d.utility.Vector3dVector(normals)
o3d.visualization.draw_geometries([new_mesh,normals_pcd],point_show_normal=True)


new_verts=np.asarray(new_mesh.vertices)
new_normals=np.asarray(new_mesh.vertex_normals)
new_triangles=np.asarray(new_mesh.triangles)
new_uvs=uvs #np.repeat(uvs,len(bubble.vertices),axis=0)

bubble_triangles=np.asarray(bubble.triangles)
print(bubble_triangles)
new_triangle_uvs=np.repeat(range(len(uvs)),bubble_triangles.flatten().shape,axis=0).reshape((-1,3))
print(new_triangle_uvs)
write_obj("./pointclouds/test_bubble_mesh.obj",new_verts,new_triangles,new_normals,new_uvs,triangle_uvs=new_triangle_uvs)

