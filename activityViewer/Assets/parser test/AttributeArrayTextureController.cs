using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using System.Linq;
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
    private void Start()
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
        string path = "";

        //check if a single step is encoded in this texture
        public bool contains_step(int step)
        {
            return start <= step && step < end;
        }
        public TextureContainer(int start, int end, string path)
        {
            this.start = start;
            this.end = end;
            this.path = path;

            tex = LoadTextures.LoadPNG(path);

            this.finishedLoading = true;

        }


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
        Debug.Log(avavilabeFiles);
    }


    private bool texturesChanged = true;

    public void setAttributesToBeLoaded(List<attributesEnum> attributeList)
    {

        foreach (var attr in attributeList)
        {
            if (!loadedAttributes.ContainsKey(attr))
            {
                try
                {
                    var intervallsList = avavilabeFiles[dataSet][attr];
                    //find next smaller element (this is a naiv implementation)
                    foreach ((var s, var e, var path) in intervallsList)
                    {
                        if (s <= step)
                        {
                            Debug.Log("Loading " + path);
                            loadedAttributes[attr] = new(s, e, path);
                            break;
                        }
                    }
                }
                catch (KeyNotFoundException e)
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
            Debug.Log(t);
        }
        texturesChanged = true;

        Debug.Log("ayyyy");
    }
    public List<Texture2D> testList = new();

    void setStep(int step)
    {
        foreach (var pair in loadedAttributes)
        {
            if (!pair.Value.contains_step(step))
            {
                setAttributesToBeLoaded(new List<attributesEnum>(loadedAttributes.Keys));
                return;
            }
        }
    }
    void datasetChanged(dataSetsEnum newDataset)
    {
        setAttributesToBeLoaded(new List<attributesEnum>(loadedAttributes.Keys));
    }

    public Texture2DArray arrayTex;
    public List<int> attributesInArray=new();
    private int arrayTextureWidth = 0;
    private Vector2 arrayTextureTexelSize;
    void makeArrayTexture()
    {
        attributesInArray.Clear();
        
        if (loadedAttributes.Count == 0) { return; }
        var tex0 = loadedAttributes.Values.First().tex;
        arrayTextureWidth = tex0.width;
        arrayTextureTexelSize = tex0.texelSize;
        
        arrayTex = new(tex0.width, tex0.height, loadedAttributes.Count, TextureFormat.RGBA32, false);
        arrayTex.filterMode = FilterMode.Point;
        arrayTex.wrapMode = TextureWrapMode.Repeat;
        int i = 0;
        foreach (var pair in loadedAttributes)
        {
            attributesInArray.Add((int)pair.Key);
            arrayTex.SetPixels(pair.Value.tex.GetPixels(0), i, 0);
            i++;
        }
        arrayTex.Apply();
    }



    private void FixedUpdate()
    {
        if (texturesChanged) {
            makeArrayTexture();


            texturesChanged = false;
        
        }


    }
    void updateLoadedTextures()
    {

    }

}
