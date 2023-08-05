using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using System.Linq;
using AsyncTextureImport;
using static AsyncTextureImport.TextureImporter;
using System;

public class BufferedAttributeArrayTextureController : MonoBehaviour
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
            stepChanged(value);
        }
    }
    [SerializeField]
    private int _step;



    public void init()
    {
        readAvailableFilenames();
    }

    internal class ImporterContainer
    {
        public TextureImporter importer;
        public string path;
        public int attributeIndex;
        public bool done = false;
    }

    class ArrayTextureContainer
    {
        //range of steps represented in the texture
        public int start = -1;
        public int end = -1;

        public bool arrayTexComplete = false;
        BufferedAttributeArrayTextureController parent;
        //check if a single step is encoded in this texture
        public bool contains_step(int step)
        {
            return start <= step && step < end;
        }


        public List<ImporterContainer> importers;
        public ArrayTextureContainer(int start, int end, List<(attributesEnum, string)> attributePaths, BufferedAttributeArrayTextureController parent)
        {
            this.start = start;
            this.end = end;
            this.parent = parent;
            importers = new();

            //tex = LoadTextures.LoadPNG(path);
            //finishedLoading=true;

            //parent.startTextureContainerCoroutine(this);
            foreach ((var attr, var s) in attributePaths)
            {
                var importer = new ImporterContainer();
                importer.path = s;
                importer.importer = new();
                importer.attributeIndex = (int)attr;
                importers.Add(importer);
                parent.startRawDataTextureContainerCoroutine(importer);
            }
            arrayTex = null;
            attributesInArray.Clear();
            parent.startMakeArrayTextureCoroutine(this);
        }


        public bool finishedLoading()
        {
            return !importers.Any(x => !x.done);
        }


        public Texture2DArray arrayTex;
        public List<float> attributesInArray = new();
    }

    void startRawDataTextureContainerCoroutine(ImporterContainer cont)
    {
        StartCoroutine(nameof(loadRawDataAsync), cont);
    }
    void startMakeArrayTextureCoroutine(ArrayTextureContainer cont)
    {
        StartCoroutine(nameof(makeArrayTextureCoroutine), cont);
    }

    IEnumerator loadRawDataAsync(ImporterContainer impoCont)
    {
        yield return impoCont.importer.ImportOnlyData(impoCont.path, FREE_IMAGE_FORMAT.FIF_PNG, 1);
        impoCont.done = true;
        yield return null;
    }

    IEnumerator makeArrayTextureCoroutine(ArrayTextureContainer cont)
    {
        while (!cont.finishedLoading())
        {
            yield return null;
        }


        if (cont.importers.Count == 0) { yield break; }

        //var tex0 = loadedAttributes.Values.First().tex;
        var rawData = cont.importers.First().importer.rawData;

        var arrayTextureWidth = rawData.width;
        var arrayTextureTexelSize = Vector2.one / new Vector2(rawData.width, rawData.height);

        cont.arrayTex = new(rawData.width, rawData.height, cont.importers.Count, TextureFormat.BGRA32, false);


        cont.arrayTex.filterMode = FilterMode.Point;
        cont.arrayTex.wrapMode = TextureWrapMode.Repeat;
        int i = 0;
        foreach (var imp in cont.importers)
        {
            cont.attributesInArray.Add(imp.attributeIndex);
            cont.arrayTex.SetPixelData(imp.importer.rawData.data, 0, i);
            i++;
        }
        cont.arrayTex.Apply();
        cont.arrayTexComplete = true;
        yield return null;
    }


    Dictionary<dataSetsEnum, Dictionary<attributesEnum, List<(int, int, string)>>> avavilabeFiles = new();

    List<ArrayTextureContainer> loadedTextures;
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


    private void cacheLoader(int start, int end, List<(attributesEnum, string)> attibutPaths)
    {
        var matches = cachedContainers.Where(a => a.start == start && a.end == end);
        if (matches.Count() > 0)
        {
            return;
        }
        var cont = new ArrayTextureContainer(start, end, attibutPaths, this);

        cachedContainers.Add(cont);
        if (cachedContainers.Count() > maxCachedContainers)
        {
            cachedContainers.RemoveAt(0);
        }
    }

    int currentIntervallEnd = 0;
    private void cacheStepRange(int step)
    {


        var intervallsList = avavilabeFiles[dataSet][attributesEnum.fired];

        int ind = 0;
        foreach ((var s, var e, var _) in intervallsList)
        {
            //Debug.Log("step");
            if (s <= step && step <= e)
            {
                currentIntervallStart = s;
                currentIntervallEnd = e;
                break;
            }
            ind += 1;
        }

        for (int i = -cacheInterval; i <= cacheInterval; i++)
        {
            try
            {
                var paths = new List<(attributesEnum, string)>();


                foreach (var attr in attributeList)
                {
                    paths.Add((attr, avavilabeFiles[dataSet][attr][ind + i].Item3));
                }
                var s_current = avavilabeFiles[dataSet][attributesEnum.fired][ind + i].Item1;
                var e_current = avavilabeFiles[dataSet][attributesEnum.fired][ind + i].Item2;

                cacheLoader(s_current, e_current, paths);
            }
            catch (ArgumentOutOfRangeException)
            {

            }
        }
    }



    List<ArrayTextureContainer> cachedContainers = new();

    int maxCachedContainers = 5;//how many array textures to cache at most at once
    int cacheInterval = 2;//how far ahead and behind to cache
    public bool texturesChanged = true;
    int currentIntervallStart;

    List<attributesEnum> attributeList;
    public void setAttributesToBeLoaded(List<attributesEnum> attributeList)
    {
        this.attributeList = attributeList;
        cachedContainers.Clear();//maybe introduce a cvheck for the lists so not all cached array textures have to be thrown away
        currentContainer = null;
        cacheStepRange(step);

        //Debug.Log("ayyyy");
    }

    public List<Texture2D> testList = new();

    void stepChanged(int step)
    {
        cacheStepRange(step);
        material.SetInteger("_step", step - currentIntervallStart);
    }


    void datasetChanged(dataSetsEnum newDataset)
    {
        cachedContainers.Clear();//maybe introduce a cvheck for the lists so not all cached array textures have to be thrown away
        currentContainer = null;
        cacheStepRange(step);
    }
    public int attemptIncrementStep(int stepSize)
    {
        if (currentContainer == null || !currentContainer.contains_step(step + stepSize))
        {
            foreach (var c in cachedContainers)
            {
                if (c.contains_step(step + stepSize))
                {
                    currentContainer = c;
                    step += stepSize;
                    return step;
                }
            }
            cacheStepRange(step+stepSize);
            return step;

        }
        else
        {
            step += stepSize;
            return step;
        }
    }

    public List<float> attributesInArray = new();

    ArrayTextureContainer currentContainer;

    public Material material;
    public Texture2DArray testArray;
    private void FixedUpdate()
    {
        if (currentContainer == null || !currentContainer.contains_step(step))
        {
            foreach (var c in cachedContainers)
            {
                if (c.contains_step(step))
                {
                    currentContainer = c;
                }
            }
            cacheStepRange(step);
        }

        if (currentContainer != null && currentContainer.arrayTexComplete && currentContainer.arrayTex.depth > 0)
        {
            testArray = currentContainer.arrayTex;
            var arrayTextureWidth = currentContainer.arrayTex.width;
            var arrayTextureTexelSize = currentContainer.arrayTex.texelSize;
            material.SetTexture("_attributesArrayTexture", currentContainer.arrayTex);
            material.SetInteger("_attributesArrayTextureWidth", arrayTextureWidth);
            material.SetVector("_attributesArrayTextureTexelSize", arrayTextureTexelSize);
            float[] attributeIndices = new float[16];
            currentContainer.attributesInArray.CopyTo(attributeIndices, 0);
            material.SetFloatArray("_attributeIndices", attributeIndices);
            material.SetInteger("_step", step - currentIntervallStart);
        }


    }

}
