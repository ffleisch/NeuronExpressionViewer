using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class DataController : MonoBehaviour
{
    private Button buttonTest;

    //public int step { get; set; } = 0;

    private SliderInt sliderStep;

    public VisualTreeAsset ui;

    public Toggle toggleRun;

    public DropdownField listViewDataSet;


    public int step
    {
        get
        {
            return sliderStep.value;
        }
        set
        {
            sliderStep.value= value;
        }
    }
    
    




    // Start is called before the first frame update
    void Start()
    {
        var uiDocument = GetComponent<UIDocument>() as UIDocument;

        var root = uiDocument.rootVisualElement;

        buttonTest = root.Q("ButtonTest") as Button;
        sliderStep = root.Q("SliderIntFrame") as SliderInt;
        toggleRun = root.Q("ToggleRun") as Toggle;
        listViewDataSet=root.Q("DropdownFieldDataset") as DropdownField;

        foreach (var c in uiDocument.rootVisualElement.Children())
        {
            Debug.Log(c);
        }
        buttonTest.RegisterCallback<ClickEvent>(buttonOnClick);
        sliderStep.RegisterCallback<ChangeEvent<int>>(testCallback);
        toggleRun.RegisterCallback<ChangeEvent<bool>>(toggleRunChanged);


        

    }

    void buttonOnClick(ClickEvent evt)
    {
        Debug.Log(evt);

    }
    void testCallback(ChangeEvent<int> evt)
    {
        Debug.Log(evt + " " + evt.newValue);
    }

    void toggleRunChanged(ChangeEvent<bool> evt)
    {
        Debug.Log(evt.newValue);

    }


    // Update is called once per frame
    void Update()
    {
    }

    private void FixedUpdate()
    {
        if (toggleRun.value) {
            step += 1;
        }
    }
}
