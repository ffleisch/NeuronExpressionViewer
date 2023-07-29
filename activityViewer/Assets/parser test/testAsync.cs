using AsyncTextureImport;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class testAsync : MonoBehaviour
{
    // Start is called before the first frame update

    public dataSetsEnum dataSet;
    dataSetsEnum oldDataSet;
    public Texture2D tex;
    void Start()
    {
        
    }

    void test() {
        string file_path = Path.Combine("..", "parse_data", "parsed_data", AttributeUtils.dataSetNames[dataSet], "rendered_images", AttributeUtils.attributeNames[attributesEnum.activity], "04565_04648.png");
        Debug.Log(file_path);

        StartCoroutine("loadTex",file_path);
        

    }


    IEnumerator loadTex(string path) { 
        TextureImporter importer = new();
        yield return importer.ImportTexture(path,FREE_IMAGE_FORMAT.FIF_PNG);

        
        tex = importer.texture;
        Debug.Log(tex);
        yield return null;
    
    }


    // Update is called once per frame
    void Update()
    {
        if (oldDataSet != dataSet) {
            test();
            oldDataSet = dataSet;
        }
    }
}
