using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using System.Linq;
using AsyncTextureImport;
using static AsyncTextureImport.TextureImporter;

public class AttributeArrayTextureController : MonoBehaviour
{


    /*public class LoadedAttribute{
        int intervalStart, intervalEnd;
        string path;
        public LoadedAttribute(LoadTextures.attributesEnum attribute,LoadTextures.dataSetsEnum dataSet,int step) {
            setPath(attribute,dataSet);
        }
        void checkStepValid
        void setPath(LoadTextures.attributesEnum attribute,LoadTextures.dataSetsEnum dataset) { 
            
            path= Path.Combine("..", "parse_data", "parsed_data", LoadTextures.dataSetNames[(int)dataset], "rendered_images", LoadTextures.attributeNames[attribute]);
        }
    }*/



    public Texture2DArray texArr;


    public dataSetsEnum dataSet
    {
        get { return _dataSet; }
        set
        {
            datasetChanged(value);
            _dataSet = value;
        }
    }
    //private field backing the dataset field, visible in editor
    [SerializeField]
    private dataSetsEnum _dataSet;




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



    Dictionary<attributesEnum, TextureContainer> loadedAttributes = new();
    public void init()
    {
        readAvailableFilenames();
    }


    class TextureContainer
    {
        public Texture2D tex;

        //range of steps represented in the texture
        public int start = -1;
        public int end = -1;
        public bool finishedLoading = false;
        public string path = "";
        AttributeArrayTextureController parent;
        //check if a single step is encoded in this texture
        public bool contains_step(int step)
        {
            return start <= step && step < end;
        }
        public TextureImporter importer;
        public TextureContainer(int start, int end, string path, AttributeArrayTextureController parent)
        {
            this.start = start;
            this.end = end;
            this.path = path;
            this.parent = parent;
            importer = new();

            //tex = LoadTextures.LoadPNG(path);
            //finishedLoading=true;

            //parent.startTextureContainerCoroutine(this);

            parent.startRawDataTextureContainerCoroutine(this);
        }
        public RawTextureData getRawData() {
            return importer.rawData;
        }


    }

    void startTextureContainerCoroutine(TextureContainer cont)
    {
        StartCoroutine(nameof(loadTexAsync), cont);
    }

    void startRawDataTextureContainerCoroutine(TextureContainer cont) {
        StartCoroutine(nameof(loadRawDataAsync), cont);
    }

    IEnumerator loadTexAsync(TextureContainer texContainer)
    {

        yield return texContainer.importer.ImportTexture(texContainer.path, FREE_IMAGE_FORMAT.FIF_PNG,1);


        texContainer.tex = texContainer.importer.texture;
        texContainer.finishedLoading = true;
        yield return null;

    }
    IEnumerator loadRawDataAsync(TextureContainer texContainer)
    {
        yield return texContainer.importer.ImportOnlyData(texContainer.path, FREE_IMAGE_FORMAT.FIF_PNG,1);
        texContainer.finishedLoading = true;
        yield return null;
    }



    Dictionary<dataSetsEnum, Dictionary<attributesEnum, List<(int, int, string)>>> avavilabeFiles = new();

    List<TextureContainer> loadedTextures;
    void readAvailableFilenames()
    {
        //List<(int, int, Dictionary<dataSetsEnum, Dictionary<attributesEnum, string>>)> stepsList = new();
        HashSet<int> seenStarts = new();
        var name_format_regex = new Regex("\\d+_\\d+.png");

        foreach (dataSetsEnum dataset in System.Enum.GetValues(typeof(dataSetsEnum)))
        {
            foreach (attributesEnum attribute in System.Enum.GetValues(typeof(attributesEnum)))
            {
                string file_path = Path.Combine("..", "parse_data", "parsed_data", AttributeUtils.dataSetNames[dataset], "rendered_images", AttributeUtils.attributeNames[attribute]);
                var info = new DirectoryInfo(file_path);
                if (!info.Exists)
                {
                    continue;
                }
                foreach (var f in info.GetFiles())
                {
                    if (!name_format_regex.IsMatch(f.Name))
                    {
                        continue;
                    }
                    var parts = Path.GetFileNameWithoutExtension(f.Name).Split("_");
                    int start = int.Parse(parts[0]);
                    int end = int.Parse(parts[1]);
                    if (!avavilabeFiles.ContainsKey(dataset))
                    {
                        avavilabeFiles[dataset] = new();
                    }
                    if (!avavilabeFiles[dataset].ContainsKey(attribute))
                    {
                        avavilabeFiles[dataset][attribute] = new();
                    }

                    avavilabeFiles[dataset][attribute].Add((start, end, f.FullName));

                }
            }
        }
        //Debug.Log(avavilabeFiles);
    }


    public bool texturesChanged = true;
    int currentIntervalstart;
    public void setAttributesToBeLoaded(List<attributesEnum> attributeList)
    {
        foreach (var attr in attributeList)
        {
            if (!loadedAttributes.ContainsKey(attr) || !loadedAttributes[attr].contains_step(step))
            {
                try
                {
                    var intervallsList = avavilabeFiles[dataSet][attr];
                    //find next smaller element (this is a naiv implementation)
                    foreach ((var s, var e, var path) in intervallsList)
                    {
                        //Debug.Log("step");
                        if (s <= step && step <= e)
                        {
                            currentIntervalstart = s;
                            Debug.Log("Loading " + path);
                            loadedAttributes[attr] = new(s, e, path, this);
                            break;
                        }
                    }
                }
                catch (KeyNotFoundException)
                {
                    throw new System.Exception("Could not find the image file");
                }
                //loadSingleAttribute(attr,);
            }
        }

        foreach (var attr in loadedAttributes.Keys)
        {
            if (!attributeList.Contains(attr))
            {//also not great performance, but we generally have very few attributes
                loadedAttributes.Remove(attr);
            }
        }

        testList.Clear();
        foreach (var t in loadedAttributes.Values)
        {
            testList.Add(t.tex);
            //Debug.Log(t);
        }
        texturesChanged = true;

        //Debug.Log("ayyyy");
    }
    public List<Texture2D> testList = new();

    void setStep(int step)
    {
        material.SetInteger("_step", step - currentIntervalstart);
        foreach (var pair in loadedAttributes)
        {
            if (!pair.Value.contains_step(step))
            {
                Debug.Log("Step Outside Range");
                setAttributesToBeLoaded(new List<attributesEnum>(loadedAttributes.Keys));
                return;
            }
        }
    }
    void datasetChanged(dataSetsEnum newDataset)
    {
        var toBeLoaded=new List<attributesEnum>(loadedAttributes.Keys);
        loadedAttributes.Clear();
        setAttributesToBeLoaded(toBeLoaded);
        Debug.Log("oops");
    }

    public Texture2DArray arrayTex;
    public List<float> attributesInArray = new();
    private int arrayTextureWidth = 0;
    private Vector2 arrayTextureTexelSize;
    void makeArrayTexture()
    {
        attributesInArray.Clear();

        if (loadedAttributes.Count == 0) { return; }

        //var tex0 = loadedAttributes.Values.First().tex;
        var rawData = loadedAttributes.Values.First().getRawData();
        
        arrayTextureWidth = rawData.width;
        arrayTextureTexelSize = Vector2.one/new Vector2(rawData.width,rawData.height);

        arrayTex = new(rawData.width, rawData.height, loadedAttributes.Count, TextureFormat.BGRA32, false);
        
        
        arrayTex.filterMode = FilterMode.Point;
        arrayTex.wrapMode = TextureWrapMode.Repeat;
        int i = 0;
        foreach (var pair in loadedAttributes)
        {
            //Debug.Log(pair.Value.path);
            attributesInArray.Add((int)pair.Key);
            //arrayTex.SetPixels(pair.Value.tex.GetPixels(0), i, 0);
            arrayTex.SetPixelData(pair.Value.getRawData().data,0,i);
            i++;
        }
        arrayTex.Apply();
    }

    void bindShaderProperties(Material mat)
    {
        if (arrayTex && arrayTex.depth > 0)
        {
            mat.SetTexture("_attributesArrayTexture", arrayTex);
            mat.SetInteger("_attributesArrayTextureWidth", arrayTextureWidth);
            mat.SetVector("_attributesArrayTextureTexelSize", arrayTextureTexelSize);
            float[] attributeIndices = new float[16];
            attributesInArray.CopyTo(attributeIndices, 0);
            mat.SetFloatArray("_attributeIndices", attributeIndices);
            mat.SetInteger("_step", step - currentIntervalstart);

        }
    }
    public Material material;
    private void FixedUpdate()
    {
        if (texturesChanged)
        {
            foreach (var x in loadedAttributes.Values)
            {
                if (!x.finishedLoading)
                {
                    Debug.Log("still loading textures");
                    return;
                }
            }

            makeArrayTexture();
            bindShaderProperties(material);
            texturesChanged = false;
        }


    }
    void updateLoadedTextures()
    {

    }

}
