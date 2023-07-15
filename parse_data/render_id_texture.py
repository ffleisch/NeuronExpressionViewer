import os

import imageio as imageio
import numpy as np
import collada as cld
import scipy.spatial
from IPython.lib.deepreload import found_now
from PIL import Image
from matplotlib import pyplot as plt, patches
from matplotlib import tri
import aabbtree


# https://observablehq.com/@infowantstobeseen/barycentric-coordinates
def calc_barycentric(pt, vert_a, vert_b, vert_c):
    ab = vert_b - vert_a
    ac = vert_c - vert_a
    ap = pt - vert_a

    nac = [vert_a[1] - vert_c[1], vert_c[0] - vert_a[0]]
    nab = [vert_a[1] - vert_b[1], vert_b[0] - vert_a[0]]

    bary_beta = np.dot(ap, nac) / np.dot(ab, nac)
    bary_gamma = np.dot(ap, nab) / np.dot(ac, nab)
    bary_alpha = 1.000 - bary_beta - bary_gamma

    return np.array([bary_alpha, bary_beta, bary_gamma])


# mesh_path="./pointclouds/test dae/blender_uv_mapped_voronoi.dae"
mesh_path = "./pointclouds/neuron_voronoi.dae"
# mesh_path="./pointclouds/neuron_voronoi_mapped.dae"

base_name = os.path.splitext(os.path.basename(mesh_path))[0]
mesh = cld.Collada(mesh_path)

print(mesh.geometries)
geom = mesh.geometries[0]

ax = plt.figure().add_subplot()
uvx = geom.primitives[0].texcoordset[0][:, 0]
uvy = geom.primitives[0].texcoordset[0][:, 1]

neuron_indices = geom.primitives[0].texcoordset[1][:, 0]  # index and area are encoded in the second uvs set of the mesh
neuron_areas = geom.primitives[0].texcoordset[1][:, 1]
n_neurons = len(neuron_indices)

uv_triangle_array = geom.primitives[0].texcoord_indexset[0]
triangle_array = geom.primitives[0].vertex_index
vertices = geom.primitives[0].vertex

my_tri = tri.Triangulation(uvx, uvy, triangles=uv_triangle_array)
ax.triplot(my_tri, linewidth=0.5, alpha=1)
plt.show()
ax.triplot(my_tri, linewidth=0.5, alpha=1)

t_size = 2048
texture = np.zeros((t_size, t_size), dtype=int)

my_tri = tri.Triangulation(uvx, uvy)

bounding_boxes = np.vstack([np.min(uvx[uv_triangle_array], axis=1), np.min(uvy[uv_triangle_array], axis=1),
                            np.max(uvx[uv_triangle_array], axis=1), np.max(uvy[uv_triangle_array], axis=1)]).transpose()

for i in range(1000):
    box = bounding_boxes[i, :]
    patch = patches.Rectangle((box[0], box[1]), box[2] - box[0], box[3] - box[1], linewidth=1, facecolor="none",
                              edgecolor="r")
    plt.gca().add_patch(patch)
print(bounding_boxes)

print("building aabb tree")
tree = aabbtree.AABBTree()
for i, box in enumerate(bounding_boxes[0:1000]):
    tree.add(aabbtree.AABB([(box[0], box[2]), (box[1], box[3])]), i)
    if (i % 1000 == 0):
        print("%d percent" % (100 * i / bounding_boxes.shape[0]))
print("done building tree")

vertex_tree = scipy.spatial.KDTree(vertices)

plt.show()
# fig=plt.figure()
# ax=fig.add_subplot(111,projection="3d")
image = np.zeros((t_size, t_size), dtype=np.single)
print("creating image")
for i in range(t_size):
    for j in range(t_size):
        x=(i+.5)/t_size
        y=(j+.5)/t_size
        bb = aabbtree.AABB([(x, x), (y, y)])
        res = tree.overlap_values(bb)


        def try_triangle(ind):
            t = uv_triangle_array[ind]
            # print(i,j,x,y)
            # print(res)
            p1 = np.array((uvx[t[0]], uvy[t[0]]))
            p2 = np.array((uvx[t[1]], uvy[t[1]]))
            p3 = np.array((uvx[t[2]], uvy[t[2]]))
            bary = calc_barycentric(np.array((x, y)), p1, p2, p3)
            # print(bary)
            if (not np.any(bary < 0)):
                # plt.plot(x,y,"kx")
                tri_verts = vertices[triangle_array[ind]]  # original vertices
                interp_point = np.dot(bary, tri_verts)
                # print(interp_point)
                # ax.plot([interp_point[0]],[interp_point[1]],[interp_point[2]],"kx")
                index = vertex_tree.query(interp_point)[1]

                image[j, i] = neuron_indices[index] +neuron_areas[index]*n_neurons
                return True
            return False



        found_triangle = False
        for ind in res:
            found_triangle = found_triangle or try_triangle(ind)
        if not found_triangle:
            x+=1
            bb = aabbtree.AABB([(x, x), (y , y)])
            new_res = tree.overlap_values(bb)
            #plt.plot((x+1)*t_size,y*t_size,"bx")
            #plt.plot(x*t_size,y*t_size,"rx")

            for new_ind in new_res:
                try_triangle(new_ind)
    print("%d percent" % (100 * i / t_size))
print("done creating image")
# plt.show()
plt.imshow(image)
plt.show()

image_array = np.zeros(t_size * t_size * 4, dtype=np.uint8)

data = image.flatten()
# data = np.linspace(0,int(image_array.shape[0]/4)-1,int(image_array.shape[0]/4),dtype=np.single)

image_array[0:data.shape[0] * 4] = np.frombuffer(data.tobytes(), dtype=image_array.dtype)
# image_array[0:data.shape[0]*4]=np.frombuffer(data.tobytes(),dtype=image_array.dtype)

image_array = image_array.reshape((t_size, t_size, 4))
im = Image.fromarray(image_array)
print("saving image")
im.save("./pointclouds/uv_maps/%s_%d_neuron_assignment.png" % (base_name, t_size))
print("saved image")
print(geom)
