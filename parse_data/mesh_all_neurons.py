import numpy
import open3d as o3d
import numpy as np
import pandas as pd
path = "../raw_data/SciVisContest23/viz-calcium/positions/rank_0_positions.txt"




def mesh_points_sphere(points, normals,uvs=None):
    # project all point of the pointcloud onto the unitsphere surrounding the center
    # this makes the mehsing way easier and avoids holes
    # this fine to do because there are no overlapping surfaces from the center out
    pcd_sphere = o3d.geometry.PointCloud()

    center=np.mean(points,axis=0)
    centered_points = points - center
    positions_sphere = centered_points / np.linalg.norm(centered_points, axis=1)[:, None]

    pcd_sphere.points = o3d.utility.Vector3dVector(positions_sphere)

    radii = o3d.utility.DoubleVector([0.001,0.01,0.05, 0.1, 1])

    pcd_sphere.normals = o3d.utility.Vector3dVector(normals)
    mesh_out= o3d.geometry.TriangleMesh.create_from_point_cloud_ball_pivoting(pcd_sphere, radii)
    #mesh_out= o3d.geometry.TriangleMesh.create_from_point_cloud_alpha_shape(pcd_sphere,1)

    mesh_out.compute_vertex_normals()
    o3d.visualization.draw_geometries([mesh_out], point_show_normal=False)
    mesh_out.vertices = o3d.utility.Vector3dVector(points)
    # mesh_large=mesh_large.filter_smooth_taubin(10)
    mesh_out.compute_vertex_normals()
    return mesh_out

def write_obj(path,vertices,triangles,normals=None,uvs=None):
    with open(path,"w+") as f:
        #write vertices
        for v in vertices:
            f.write("v %f %f %f\n"%tuple(v))
        if normals is not None:
            for n in normals:
                f.write("vn %f %f %f\n"%tuple(n))
        if uvs is not None:
            for uv in uvs:
                f.write("vt %d %d\n"%tuple(uv))
        for t in triangles:
            f.write("f %d/%d/%d %d/%d/%d %d/%d/%d\n"%tuple(np.repeat(t+1,3)))
        pass
    print("saved model")

#this is intented to preserve the vertex order and the per vertex uv coordinates
def read_obj_inorder(path):
    with open(path,"r") as f:

        vertices=[]
        normals=[]
        uvs=[]
        triangle_vertices=[]
        triangle_normals=[]
        triangle_uvs=[]
        for l in f.readlines():
            parts=l.split(" ")
            if parts[0]=="v":
                vertices.append((float(parts[1]),float(parts[2]),float(parts[3])))

            if parts[0]=="vn":
                normals.append((float(parts[1]),float(parts[2]),float(parts[3])))
            if parts[0]=="vt":
                uvs.append((float(parts[1]),float(parts[2])))

            if parts[0]=="f":
                triparts=[]
                for p in parts[1:]:
                    triparts.append(p.split("/"))

                triangle_vertices.append((int(triparts[0][0]),int(triparts[1][0]),int(triparts[2][0])))
                triangle_normals.append((int(triparts[0][1]),int(triparts[1][1]),int(triparts[2][1])))
                triangle_uvs.append((int(triparts[0][2]),int(triparts[1][2]),int(triparts[2][2])))
        return np.asarray(vertices),np.asarray(normals),np.asarray(uvs),np.asarray(triangle_vertices),np.asarray(triangle_normals),np.asarray(triangle_uvs)
    return
def encode_uvs(points,areas):
    uvs=np.zeros((len(points), 2))
    uvs[:,0]=range(len(points))
    uvs[:,1]=areas
    return uvs

if __name__=="__main__":

    df = pd.read_csv(path, sep=" ", header=7)

    # dataframe ith only the positions
    positions_df = df[["<pos_x>", "<pos_y>", "<pos_z>"]]
    areas=pd.to_numeric(df["<area>"].str.split("_",expand=True)[1]).to_numpy()

    positions = positions_df.to_numpy()

    # every 10 lines are neurons closely clustered around a single point
    clusters = np.reshape(positions, (5000, 10, 3))
    print(clusters)

    # find the centers of these clusters
    # find the absolute center
    centers = np.mean(clusters, axis=1)

    # create a pointcloud of the centers
    pcd = o3d.geometry.PointCloud()
    pcd.points = o3d.utility.Vector3dVector(centers)
    # estimate the normals fo this pointcloud
    pcd.estimate_normals(o3d.geometry.KDTreeSearchParamKNN(knn=30))
    # o3d.visualization.draw_geometries([pcd], point_show_normal=True)

    normals = np.asarray(pcd.normals)

    # the normals are not always flipped in the right direction, i suspect because parts of the model are concave
    # flip every normal that points towards the center of the model outwards
    center = np.mean(centers, axis=0)
    normals[np.einsum("ij,ij->i", centers - center, normals) < 0] *= -1
    pcd.normals = o3d.utility.Vector3dVector(normals)
    # o3d.visualization.draw_geometries([pcd], point_show_normal=True)

    mesh=mesh_points_sphere(centers, normals)
    '''centered_points = centers - center
    pcd.points = o3d.utility.Vector3dVector(centered_points / np.linalg.norm(centered_points, axis=1)[:, None])

    # mesh the resulting sphere with the ball pivot algorithm
    radii = o3d.utility.DoubleVector([0.01, 0.1, 1])
    mesh = o3d.geometry.TriangleMesh.create_from_point_cloud_ball_pivoting(pcd, radii)

    # set the vertices of theww mehs back to the original position from their tepmporary sphere positions
    mesh.vertices = o3d.utility.Vector3dVector(centers)'''
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

    mesh_large=mesh_points_sphere(flattened_positions, ex_normals)

    #set the uvs of the mesh
    #x coorinate encodes the original neurons index
    #y coordinate encodes which area the neuron belongs to
    uvs=encode_uvs(positions,areas)
    print(uvs)
    mesh_large.triangle_uvs=o3d.utility.Vector2dVector(uvs)

    # save the mesh
    o3d.visualization.draw_geometries([mesh_large], point_show_normal=True)
    #o3d.io.write_triangle_mesh("./pointclouds/test.obj", mesh_large,write_triangle_uvs=True)

    #sadly o3d doesent seem to be able to write out the uvs to the .obj
    #so here we go
    write_obj("./pointclouds/neuron_mesh_full_flattened_and_spread.obj",mesh_large.vertices,mesh_large.triangles,
              mesh_large.vertex_normals,uvs)

    numpy.save("./pointclouds/neuron_positions_flattened_and_spread.npy",flattened_positions)