using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

public class load_tetxures : MonoBehaviour
{

    public Texture2D tex = null;

    TextureWithInterval tex_current=new(null,-1,-1,"");

    private class TextureWithInterval {
        public Texture2D tex;
        public int start=-1;
        public int end=-1;
        string path="";
        public bool contains_step(int step) {
            return start <= step && step<end;
        }
        public TextureWithInterval(Texture2D tex,int start,int end, string path) {
            this.tex = tex;
            this.start = start;
            this.end = end;
            this.path = path;
        }
    }
    
    public enum dataSetsEnum
    { 
    viz_no_network,viz_calcium,viz_disable,viz_stimulus
    }
    static string[] dataSetNames ={    "viz-no-network","viz-calcium","viz-disable","viz-stimulus"};
    public dataSetsEnum dataSet;
    public enum columsEnum { 
    step, fired, fired_fraction, activity, dampening,current_calcium, target_calcium,
        synaptic_input, background_input,grown_axons, connected_axons, grown_dendrites,
        connected_dendrites
    }
    static string[] columnNames = {"step", "fired", "fired fraction", "activity", "dampening", "current calcium", "target calcium",
        "synaptic input", "background input", "grown axons", "connected axons", "grown dendrites",
        "connected dendrites" };
    public columsEnum column;
    const int num_neurons = 50000;
    const int num_steps = 10000;
    [Range(0,num_steps-1)]
    public int step;

    public bool do_run;

    List<(int, int, string)> intervals_list;
    // Start is called before the first frame update
    void Start()
    {
        string file_path = Path.Combine("..", "parse_data", "parsed_data", dataSetNames[(int)dataSet], "rendered_images", columnNames[(int)column]);
        Debug.Log(file_path);

        tex = LoadPNG(Path.Combine(file_path, "test.png"));

        var info = new DirectoryInfo(file_path);
        intervals_list = new List<(int, int, string)>();

        var name_format_regex = new Regex("\\d{5}_\\d{5}.png");


        foreach (var f in info.GetFiles()) {
            //Debug.Log(f);
            if (!name_format_regex.IsMatch(f.Name)) {
                continue;
            }
            var parts = Path.GetFileNameWithoutExtension(f.Name).Split("_");
            int start = int.Parse(parts[0]);
            int end = int.Parse(parts[1]);
            intervals_list.Add((start, end, f.FullName));
        }
        intervals_list.Sort((a, b )=>  a.Item1.CompareTo(b.Item1));

    }

    // Update is called once per frame
    void Update()
    {
        //(int x, int y) = CoordsFromIndex(0, Time.frameCount-1);
        //Color c = tex.GetPixel(x, y);
        //Debug.Log(ColorToFloat(c));
        
        if (!tex_current.contains_step(step)) {
            tex_current=LoadTextureForStep(step);
            //Debug.Log("Loaded_Texture");   
        }
        Material mat = GetComponent<Renderer>().sharedMaterial;
        mat.SetTexture("_MainTex", tex);
        mat.SetInteger("_n_neurons", num_neurons);
        mat.SetInteger("_step", step - tex_current.start);
    }
    private void FixedUpdate()
    {
        if (do_run) {
            step += 1;
        }
    }
    private static unsafe float ColorToFloat(Color c)
    {
        byte[] bytes = { (byte)(255 * c[0]), (byte)(255 * c[1]), (byte)(255 * c[2]), (byte)(255 * c[3]) };
        return BitConverter.ToSingle(bytes);
    }


    private TextureWithInterval LoadTextureForStep(int step) {
        foreach ((var s, var e, var path) in intervals_list) {
            if (s <= step && step < e) {//TODO check if indices align at borders
                tex=LoadPNG(path);
                return new (tex,s,e,path);
            }
        }
            return null;

    }
    
    private string file_name_from_step(int step) {
        foreach ((var s, var e, var name) in intervals_list) {
            if (s <= step && step < e) {//TODO check if indices align at borders
                return name;
            }
        }
        return "not found";    
    }

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

    private static Texture2D LoadPNG(string filePath)
    {

        Texture2D tex = null;
        byte[] fileData;

        if (System.IO.File.Exists(filePath))
        {
            fileData = System.IO.File.ReadAllBytes(filePath);
            tex = new Texture2D(2, 2);
            tex.requestedMipmapLevel = 0;
            tex.anisoLevel = 0;
            tex.filterMode = FilterMode.Point;

            tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.
        }
        
        return tex;
    }


}
