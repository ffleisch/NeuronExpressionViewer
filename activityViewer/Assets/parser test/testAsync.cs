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
        array = new(2048,2048,1,TextureFormat.BGRA32,false);
    }

    void test() {
        string file_path = Path.Combine("..", "parse_data", "parsed_data", AttributeUtils.dataSetNames[dataSet], "rendered_images", AttributeUtils.attributeNames[attributesEnum.activity], "04565_04648.png");
        Debug.Log(file_path);

        StartCoroutine("arrayTexCoroutine",file_path);
        

    }


    IEnumerator loadTex(string path) { 
        TextureImporter importer = new();
        yield return importer.ImportTexture(path,FREE_IMAGE_FORMAT.FIF_PNG,-1);

        
        tex = importer.texture;
        Debug.Log(tex);
        yield return null;
    
    }
    public Texture2DArray array;

    IEnumerator arrayTexCoroutine(string path) {
        TextureImporter importer = new();
        yield return importer.ImportOnlyData(path,FREE_IMAGE_FORMAT.FIF_PNG,-1);
        array.SetPixelData(importer.rawData.data,0,0);
        array.Apply(updateMipmaps:false);
        Debug.Log(array);
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
