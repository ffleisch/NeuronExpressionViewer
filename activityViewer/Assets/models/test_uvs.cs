using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test_uvs : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        Mesh m = mf.mesh;
        var uvs = m.uv;
        var tris = m.triangles;
        Debug.Log(m);
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
