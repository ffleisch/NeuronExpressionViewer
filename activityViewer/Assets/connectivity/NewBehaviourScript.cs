using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var mf = GetComponent<MeshFilter>();


        //var test= Resources.Load<GameObject>("connectivity/viz-no-network/network/0060000_graph");
        //mf.sharedMesh = test.GetComponent<MeshFilter>().mesh;
        
        Debug.Log(mf);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
