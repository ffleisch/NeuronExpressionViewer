using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

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
            updateLoadedTextures();
        }
    }
    [SerializeField]
    private int _step;



    Dictionary<attributesEnum,int> loadedAttributes;

    void setAttributesToBeLoaded(List<attributesEnum> attributeList)
    {

        foreach (var attr in attributeList) {
            if (!loadedAttributes.ContainsKey(attr)) {
                //loadSingleAttribute(attr,);
            }
        }
    }

    void loadSingleAttribute(attributesEnum attribute, int targetLayer)
    {

    }

    void setStep(int step)
    {

    }
    void datasetChanged(dataSetsEnum newDataset)
    {

    }

    void updateLoadedTextures()
    {

    }

}
