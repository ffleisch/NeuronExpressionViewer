import pandas
import scipy.spatial.distance
from scipy.cluster.hierarchy import dendrogram, linkage
import numpy as np
from matplotlib import pyplot as plt
import open3d

if __name__ == '__main__':
    path = "../raw_data/SciVisContest23/viz-calcium/positions/rank_0_positions.txt"

    df = pandas.read_csv(path, sep=" ", header=7)

    positions = df[["<pos_x>", "<pos_y>", "<pos_z>"]]

    print(positions)

    positions.to_csv("./pointclouds/neuron-positions.txt")

    # clustering of points to find centers of neuron groups
    # turns out they are in order in the file, in groups of 10
    # dists=scipy.spatial.distance.pdist(positions.to_numpy()[:1000,:],metric="euclid")
    # l=linkage(dists,method="single")
    # print("done")
    # dn=dendrogram(l)
    # plt.show()

    positions_reduced = positions.iloc[::10]
    dists = scipy.spatial.distance.pdist(positions_reduced.to_numpy(), metric="euclid")
    print(np.mean(dists), np.std(dists))

    positions_reduced.to_csv("./pointclouds/neuron-positions-reduced.txt")
    print(positions_reduced)

    pcl = open3d.geometry.PointCloud()


    #pcl.points = open3d.utility.Vector3dVector(positions.to_numpy())
    #open3d.io.write_point_cloud("./pointclouds/neuron-positions.pcd",pcl)

    pcl.points=open3d.utility.Vector3dVector(positions_reduced.to_numpy())
    pcl.estimate_normals(open3d.geometry.KDTreeSearchParamKNN(knn=30))

    pos = np.asarray(pcl.points)
    center = np.mean(pos)
    normals = np.asarray(pcl.normals)
    for i in range(normals.shape[0]):
        n=normals[i]
        p=pos[i]
        if np.dot(n, center - p) > 0:
            normals[i] *= -1
    pcl.normals=open3d.utility.Vector3dVector(normals)


    radii = open3d.utility.DoubleVector([0.5,1,2,3,4,5,6,7])#np.arange(0.05,6,0.5))#
    mesh = open3d.geometry.TriangleMesh.create_from_point_cloud_ball_pivoting(pcl, radii)


    print(np.asarray(mesh.triangles))

    points=np.asarray(mesh.vertices)
    dists=[]
    for t in np.asarray(mesh.triangles):
        dists.append(np.sum(np.linalg.norm(points[t]-np.roll(points[t],1,axis=0),axis=1)))
    print(dists)
    print(np.mean(dists))
    #mesh=open3d.t.geometry.TriangleMesh.from_legacy(mesh).fill_holes().to_legacy()
    #mesh.compute_vertex_normals()
    #mesh.fill_holes()

    open3d.io.write_triangle_mesh("./pointclouds/neuron_mesh_simplified.stl",mesh)
    open3d.visualization.draw_geometries([mesh], point_show_normal=False)
    print("eyy")
