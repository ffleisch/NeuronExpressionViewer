using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class whereMousePoint : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetMouseButtonDown(1))
        {
            setConnectionsShown();
        }

    }



    public int index=0;

    private void setConnectionsShown()
    {

        if (Input.GetMouseButton(1))
        {



            RaycastHit hit;

            if (!Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
                return;

            MeshCollider meshCollider = hit.collider as MeshCollider;
            if (meshCollider == null || meshCollider.sharedMesh == null)
            {
                foreach (Transform t in transform)
                {
                    var renderer = t.gameObject.GetComponent<Renderer>();//the child should be the mesh for the coneectivity
                    if (renderer)
                    {
                        renderer.sharedMaterial.SetInteger("_n_index", -1);
                    }

                }
                return;
            }
            Mesh mesh = meshCollider.sharedMesh;
            Vector3[] vertices = mesh.vertices;
            int[] triangles = mesh.triangles;
            Vector2[] uvs = mesh.uv;
            var v1 = triangles[3 * hit.triangleIndex];
            var v2 = triangles[3 * hit.triangleIndex + 1];
            var v3 = triangles[3 * hit.triangleIndex + 2];
            var p1 = vertices[v1];
            var p2 = vertices[v2];
            var p3 = vertices[v3];
            var d1 = (transform.TransformPoint(p1) - hit.point).magnitude;
            var d2 = (transform.TransformPoint(p2) - hit.point).magnitude;
            var d3 = (transform.TransformPoint(p3) - hit.point).magnitude;
            int closest_index = -1;
            if (d1 <= d2 && d1 <= d3) { closest_index = v1; }
            if (d2 <= d3 && d2 <= d1) { closest_index = v2; }
            if (d3 <= d1 && d3 <= d2) { closest_index = v3; }
            var uv_tex = GetComponent<Renderer>().sharedMaterial.GetTexture("_neuronMap") as Texture2D;
            Vector2 uv_pos = uvs[closest_index] / uv_tex.texelSize + new Vector2(0.5f, 0.5f);
            Debug.Log(uv_pos);
            Debug.Log(uv_tex.isReadable);
            var col = uv_tex.GetPixel((int)uv_pos.x, 1 - (int)uv_pos.y);
            index = (int)(LoadTextures.ColorToFloat(col) + 0.5f) % 50000;//todo make number neurons dynamic
            Debug.Log(index);

        }

    }


    public void setShaderParams()
    {
        foreach (Transform t in transform)
        {
            var renderer = t.gameObject.GetComponent<Renderer>();//the child should be the mesh for the coneectivity
            if (renderer)
            {
                renderer.sharedMaterial.SetInteger("_n_index", index);
            }
        }
    }

}
