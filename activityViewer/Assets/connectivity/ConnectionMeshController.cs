using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

public class ConnectionMeshController : MonoBehaviour
{
    // Start is called before the first frame update
    public int step
    {
        get { return _step; }
        set
        {
            _step = value;
            setStep(value);
        }
    }
    [SerializeField]
    private int _step;

     public dataSetsEnum dataSet
    {
        get { return _dataSet; }
        set
        {
            _dataSet = value;
            datasetChanged(value);
            setStep(step);
        }
    }
    //private field backing the dataset field, visible in editor
    [SerializeField]
    private dataSetsEnum _dataSet;


    



    void Start()
    {
        loadMeshesForDatasetAttribute(dataSetsEnum.viz_no_network);
    }

    // Update is called once per frame
    void Update()
    {

    }

    Dictionary<dataSetsEnum, Dictionary<attributesEnum, List<(int, int, string)>>> availabeMeshes = new();



    List<(int, GameObject)> meshes = new();


    void loadMeshesForDatasetAttribute(dataSetsEnum dataSet)
    {
        

        var name_format_regex = new Regex("\\.dae$");

        string file_path = Path.Combine(".", "Assets", "Resources", "connectivity", AttributeUtils.dataSetNames[dataSet], "network");
        //Debug.Log(file_path);
        var info = new DirectoryInfo(file_path);
        //Debug.Log(info);
        List<(string, int)> namesAndStep = new();
        if (!info.Exists)
        {
            //return;
        }
        foreach (var f in info.GetFiles())
        {
            if (!name_format_regex.IsMatch(f.Name))
            {
                continue;
            }
            var parts = Path.GetFileNameWithoutExtension(f.Name).Split("_");
            int num = int.Parse(parts[0]);
            string asset_path = "connectivity/" + AttributeUtils.dataSetNames[dataSet] + "/network/" + f.Name.Split(".")[0];
            //string asset_path = f.Name.Split(".")[0];

            namesAndStep.Add((asset_path, num));
        }
        namesAndStep.Sort((a, b) => a.Item2.CompareTo(b.Item2));
        foreach ((var path, var step) in namesAndStep)
        {
            var m = Resources.Load<GameObject>(path);
            Debug.Log(path);
            Debug.Log(m); ;
            if (m != null)
            {
                meshes.Add((step / 100, m));
            }
            else
            {
                meshes.Add((step / 100, null));

            }
        }



    }

    void datasetChanged(dataSetsEnum newDataset) {
        loadMeshesForDatasetAttribute(newDataset);    
    }
    void setStep(int step)
    {
        int last_i = 0;
        foreach ((var i, var go) in meshes)
        {
            if (last_i <= step && step < i)
            {
                //transform.DetachChildren();
                var mf_o = go.GetComponent<MeshFilter>();
                foreach (Transform t in transform)
                {
                    var mf = t.gameObject.GetComponent<MeshFilter>(); //SetActive(false);
                    if (mf_o)
                    {
                        if (mf)
                            mf.sharedMesh = mf_o.sharedMesh;
                    }
                    else {
                        if (mf)
                            mf.sharedMesh = null;
                    }
                }

                //newMesh.transform.parent = transform;

                var mp = GetComponent<whereMousePoint>();
                mp.setShaderParams();
            }
            last_i = i;
        }

    }
}
