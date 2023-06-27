import numpy as np
import collada as cld
from matplotlib import pyplot as plt
from matplotlib import tri
import aabbtree


mesh=cld.Collada("./pointclouds/test dae/blender_uv_mapped_voronoi.dae")

print(mesh.geometries)
geom=mesh.geometries[0]

ax=plt.figure().add_subplot()
uvx=geom.primitives[0].texcoordset[0][:,0]
uvy=geom.primitives[0].texcoordset[0][:,1]
uv_triangle_array=geom.primitives[0].texcoord_indexset[0]
triangle_array=geom.primitives[0].vertex_index
my_tri=tri.Triangulation(uvx,uvy,triangles=uv_triangle_array)
ax.triplot(my_tri,linewidth=0.5,alpha=1)
plt.show()

t_size=64
texture=np.zeros((t_size,t_size),dtype=int)

my_tri=tri.Triangulation(uvx,uvy)





first_occ_dict={}
indices=[]

for t in uv_triangle_array.flatten():
    a=uvx[t]
    b=uvy[t]
    if not (a,b) in first_occ_dict:
        first_occ_dict[(a,b)]=t


request_triangles=np.array([first_occ_dict[(uvx[t],uvy[t])]for t in uv_triangle_array.flatten()]).reshape((-1,3))

print(first_occ_dict)
print(request_triangles,triangle_array)

request_triangulation=tri.Triangulation(uvx,uvy,triangles=request_triangles)
plt.triplot(request_triangulation,linewidth=0.5,alpha=1)
plt.show()

trifinder=request_triangulation.get_trifinder()

#for i,x in enumerate(np.linspace(0,1,t_size)):
#    for j,y in enumerate(np.linspace(0,1,t_size)):
#        print(i,j,x,y)
#        print(trifinder(x,y))


#print(geom)