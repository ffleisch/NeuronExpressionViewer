import collada as cld
import os



def setup_basic_mesh():
    mesh=cld.Collada()

    effect = cld.material.Effect("effect0", [], "phong", diffuse=(1,0,0), specular=(0,1,0))
    mat = cld.material.Material("material0", "mymaterial", effect)
    mesh.effects.append(effect)
    mesh.materials.append(mat)
    return mesh

def save_basic_mesh(path,mesh,geom):
    mesh.geometries.append(geom)


    geomNode=cld.scene.GeometryNode(geom)
    node=cld.scene.Node("node0",children=[geomNode])

    myscene=cld.scene.Scene("myscene",[node])
    mesh.scenes.append(myscene)
    mesh.scene=myscene
    mesh.write(path)



