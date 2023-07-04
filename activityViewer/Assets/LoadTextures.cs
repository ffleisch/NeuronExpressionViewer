using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

public class LoadTextures : MonoBehaviour
{

    public Texture2D tex = null;

    TextureWithInterval tex_current = new(null, -1, -1, "");


    //class for keeping track of what texture is loaded, its path and the steps encoded in that texture
    private class TextureWithInterval
    {
        //the texture gameobject
        public Texture2D tex;

        //range of steps represented in the texture
        public int start = -1;
        public int end = -1;
        string path = "";

        //check if a single step is encoded in this texture
        public bool contains_step(int step)
        {
            return start <= step && step < end;
        }
        public TextureWithInterval(Texture2D tex, int start, int end, string path)
        {
            this.tex = tex;
            this.start = start;
            this.end = end;
            this.path = path;
        }
    }


    //possible datasets
   
    //c sharp property for accessing the dataset curently selected 
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


    //same setup for currently selected attribute
    public attributesEnum attribute
    {
        get { return _attribute; }
        set
        {
            attributeChanged(value);
            _attribute = value;
        }
    }

    [SerializeField]
    private attributesEnum _attribute;



   

    //how many neruons in the simulation
    const int num_neurons = 50000;
    //how many frames are captured in the simulation (does not equal timesteps simulated by about x100)
    const int num_steps = 10000;
    [Range(0, num_steps - 1)]
    public int step;//kind of a misnomer TODO rename step everywhere

    public bool do_run;


    //all intervalls available in the current directory
    List<(int, int, string)> intervals_list;
    // Start is called before the first frame update
    void Start()
    {


        //var mf = GetComponent<MeshFilter>();
        //var test_uvs = mf.mesh.uv;
        initAttribute(dataSet, attribute);
    }

    //find all the files in the directory for the selected attribute of dataset
    //make the list of intervals for requesting the right filename for a tep
    void initAttribute(dataSetsEnum dataSet, attributesEnum attribute)
    {
        //make the file path from the selected attribute and dataset
        string file_path = Path.Combine("..", "parse_data", "parsed_data", AttributeUtils.dataSetNames[dataSet], "rendered_images", AttributeUtils.attributeNames[attribute]);
        Debug.Log(file_path);


        //load a test png, not sure if still necessary/usfull
        tex = LoadPNG(Path.Combine(file_path, "test.png"));

        var info = new DirectoryInfo(file_path);
        intervals_list = new List<(int, int, string)>();

        var name_format_regex = new Regex("\\d+_\\d+.png");


        foreach (var f in info.GetFiles())
        {
            //Debug.Log(f);
            if (!name_format_regex.IsMatch(f.Name))
            {
                continue;
            }
            var parts = Path.GetFileNameWithoutExtension(f.Name).Split("_");
            int start = int.Parse(parts[0]);
            int end = int.Parse(parts[1]);
            intervals_list.Add((start, end, f.FullName));
        }
        intervals_list.Sort((a, b) => a.Item1.CompareTo(b.Item1));
        tex_current = LoadTextureForStep(step);
    }


    // Update is called once per frame
    void Update()
    {
        //(int x, int y) = CoordsFromIndex(0, Time.frameCount-1);
        //Color c = tex.GetPixel(x, y);
        //Debug.Log(ColorToFloat(c));


        //check if the current texture encodes the current step 
        if (!tex_current.contains_step(step))
        {
            //if not load the appropraite texture
            tex_current = LoadTextureForStep(step);
            //Debug.Log("Loaded_Texture");   
        }


        //set the parameters and texture for tzhe shacder to render the attribute

        Renderer renderer = GetComponent<Renderer>();
        if(renderer)
        {
            var mat = renderer.sharedMaterial;
            mat.SetTexture("_MainTex", tex);
            mat.SetInteger("_n_neurons", num_neurons);
            mat.SetInteger("_step", step - tex_current.start);
        }
    }

    //callback for when the selected dataset is changed
    //just init the attribute again
    void datasetChanged(dataSetsEnum newSet)
    {
        Debug.Log(newSet);
        initAttribute(newSet, attribute);
    }


    //callback for when the selected attribute is changed
    //just init the attribute again
    void attributeChanged(attributesEnum newAttribute)
    {
        Debug.Log(newAttribute);
        initAttribute(dataSet, newAttribute);
    }


    //advance one frame in the simulation, if the toggle is enabled
    private void FixedUpdate()
    {
        if (do_run)
        {
            step += 1;
        }
    }


    //test function for extracting the encoded float from the color from the image
    private static unsafe float ColorToFloat(Color c)
    {
        byte[] bytes = { (byte)(255 * c[0]), (byte)(255 * c[1]), (byte)(255 * c[2]), (byte)(255 * c[3]) };
        return BitConverter.ToSingle(bytes);
    }

    //load the approriate texture for a given step by iterating over the intervall list and returning the associated path
    private TextureWithInterval LoadTextureForStep(int step)
    {
        foreach ((var s, var e, var path) in intervals_list)
        {
            if (s <= step && step < e)
            {//TODO check if indices align at borders
                tex = LoadPNG(path);
                return new(tex, s, e, path);
            }
        }
        return null;

    }


    //same thing but only gives the path
    private string file_name_from_step(int step)
    {
        foreach ((var s, var e, var name) in intervals_list)
        {
            if (s <= step && step < e)
            {//TODO check if indices align at borders
                return name;
            }
        }
        return "not found";
    }

    //testfuncton for getting the value of a single neuron in a timestep
    private float FloatFromIndex(int step, int neuron, int n_neurons = 50000, int width = 2048)
    {
        (int x, int y) = CoordsFromIndex(step, neuron, n_neurons, width);
        Color c = tex.GetPixel(x, y);
        return ColorToFloat(c);
    }
    private static (int, int) CoordsFromIndex(int step, int neuron, int n_neurons = 50000, int width = 2048)
    {
        int index = n_neurons * step + neuron;
        int x = index % width;
        int y = index / width;
        return (x, width - y - 1);
    }

    //load a png at runtime into a Texture2D gameobject
    //taken from https://gist.github.com/openroomxyz/bb22a79fcae656e257d6153b867ad437
    public static Texture2D LoadPNG(string filePath)
    {

        Texture2D tex = null;
        byte[] fileData;

        if (System.IO.File.Exists(filePath))
        {
            fileData = System.IO.File.ReadAllBytes(filePath);
            tex = new Texture2D(2, 2);
            tex.requestedMipmapLevel = 0;//dont make a mip map, we want to sample this texture for its exact values
            tex.anisoLevel = 0;
            tex.filterMode = FilterMode.Point;//nearest neighbor for interpolation, there should be no interpolation of uncorrelated (but neighboring in the image) neuron values

            tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.
        }

        return tex;
    }

    //set windowing paramters in shader
    public void setWindowingFunctionLimits(float minV, float maxV)
    {

        Material mat = GetComponent<Renderer>().sharedMaterial;
        mat.SetFloat("_windowMin", minV);
        mat.SetFloat("_windowMax", maxV);
    }
}
