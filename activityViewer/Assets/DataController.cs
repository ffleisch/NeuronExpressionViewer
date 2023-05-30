using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
public class DataController : MonoBehaviour
{
    private Button buttonResetSliders;

    //public int step { get; set; } = 0;

    private SliderInt sliderStep;
    private Slider sliderMax;
    private Slider sliderMin;

    public VisualTreeAsset ui;

    public Toggle toggleRun;

    public EnumField enumFieldDataset;
    public EnumField enumFieldAttribute;

    LoadTextures textureLoader;


    public int step
    {
        get
        {
            return sliderStep.value;
        }
        set
        {
            sliderStep.value = value;
        }
    }






    // Start is called before the first frame update
    void Start()
    {
        textureLoader = GetComponent<LoadTextures>();
        if (!textureLoader)
        {
            textureLoader = GetComponentInChildren<LoadTextures>();
        }

        var uiDocument = GetComponent<UIDocument>() as UIDocument;
        var root = uiDocument.rootVisualElement;

        buttonResetSliders = root.Q("ButtonResetSliders") as Button;
        sliderStep = root.Q("SliderIntFrame") as SliderInt;

        sliderMax = root.Q("SliderWindowMax") as Slider;
        sliderMin = root.Q("SliderWindowMin") as Slider;

        toggleRun = root.Q("ToggleRun") as Toggle;
        enumFieldDataset = root.Q("EnumFieldDataset") as EnumField;
        enumFieldAttribute = root.Q("EnumFieldAttribute") as EnumField;
        var test = root.Q("DropdownFieldDataset");

        buttonResetSliders.RegisterCallback<ClickEvent>(resetSlidersClicked);
        sliderStep.RegisterCallback<ChangeEvent<int>>(stepChanged);

        sliderMax.RegisterCallback<ChangeEvent<float>>(maxValueChanged);
        sliderMin.RegisterCallback<ChangeEvent<float>>(minValueChanged);

        toggleRun.RegisterCallback<ChangeEvent<bool>>(toggleRunChanged);


        enumFieldDataset.value = textureLoader.dataSet;
        enumFieldDataset.RegisterCallback<ChangeEvent<System.Enum>>(dataSetEnumFieldChanged);
        enumFieldAttribute.value = textureLoader.attribute;
        enumFieldAttribute.RegisterCallback<ChangeEvent<System.Enum>>(attributeEnumFieldChanged);
        sliderStep.value = textureLoader.step;
    }

    void resetSlidersClicked(ClickEvent evt)
    {
        sliderMin.value = 0;
        sliderMax.value = 1;
    }
    void stepChanged(ChangeEvent<int> evt)
    {
        textureLoader.step = step;
        Debug.Log(evt + " " + evt.newValue);
    }

    void toggleRunChanged(ChangeEvent<bool> evt)
    {
        Debug.Log(evt.newValue);

    }

    void dataSetEnumFieldChanged(ChangeEvent<System.Enum> evt)
    {
        Debug.Log(evt.newValue);
        textureLoader.dataSet = (LoadTextures.dataSetsEnum)evt.newValue;
    }

    void attributeEnumFieldChanged(ChangeEvent<System.Enum> evt)
    {
        Debug.Log(evt.newValue);
        textureLoader.attribute = (LoadTextures.attributesEnum)evt.newValue;
    }

    void maxValueChanged(ChangeEvent<float> evt)
    {
        textureLoader.setWindowingFunctionLimits(sliderMin.value, sliderMax.value);
    }

    void minValueChanged(ChangeEvent<float> evt)
    {
        textureLoader.setWindowingFunctionLimits(sliderMin.value, sliderMax.value);
    }
    // Update is called once per frame
    void Update()
    {
    }

    private void FixedUpdate()
    {
        if (toggleRun.value)
        {
            step += 1;
        }
    }
}
