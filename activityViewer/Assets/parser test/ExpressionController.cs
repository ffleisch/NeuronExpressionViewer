using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class ExpressionController : MonoBehaviour
{



    VisualElement root;

    private SliderInt sliderStep;
    public EnumField enumFieldDataset;
    public Toggle toggleRun;

    TextField textFieldExpression;
    Label labelStatus;
    //ExpressionTexturesControlller textureController;
    // Start is called before the first frame update
    AttributeArrayTextureController test;
    void Start()
    {
        var uiDocument = GetComponent<UIDocument>();
        //textureController = gameObject.AddComponent<ExpressionTexturesControlller>();

        root = uiDocument.rootVisualElement;
        textFieldExpression = root.Q("TextFieldExpression") as TextField;
        labelStatus = root.Q("LabelStatus") as Label;

        enumFieldDataset = root.Q("EnumFieldDataset") as EnumField;
        
        toggleRun = root.Q("ToggleRun") as Toggle;
        sliderStep = root.Q("SliderIntFrame") as SliderInt;
        
        textFieldExpression.RegisterValueChangedCallback(expressionChanged);
        enumFieldDataset.RegisterCallback<ChangeEvent<System.Enum>>(dataSetEnumFieldChanged);
        toggleRun.RegisterCallback<ChangeEvent<bool>>(toggleRunChanged);
        sliderStep.RegisterCallback<ChangeEvent<int>>(stepChanged);

        test = gameObject.AddComponent<AttributeArrayTextureController>();
        test.material = GetComponent<Renderer>().sharedMaterial;
    }

    const int maxTokens = 256;
    const int maxValues = 128;

    void expressionChanged(ChangeEvent<string> evt)
    {
        Debug.Log(evt.newValue);
        InfixParser parser = new();

        (var shaderValues, var shaderTokens, var neededAttributes) = parser.parseToShaderArrays(evt.newValue);
        //textureController.loadAttributes(neededAttributes);

        //textureController.bindAttributesToShader();

        var mat = GetComponent<Renderer>().sharedMaterial;
        if (shaderTokens.Count > 0)
        {
            var tokenArray = new float[maxTokens];
            shaderTokens.CopyTo(tokenArray, 0);
            mat.SetFloatArray("tokens", tokenArray);
        }

        mat.SetInt("num_tokens", shaderTokens.Count);

        if (shaderValues.Count > 0)
        {
            var valueArray = new float[maxValues];
            shaderValues.CopyTo(valueArray, 0);
            mat.SetFloatArray("values", valueArray);
        }
        test.setAttributesToBeLoaded(neededAttributes);
    }
    void stepChanged(ChangeEvent<int> evt)
    {
        test.step = evt.newValue;
        Debug.Log(evt + " " + evt.newValue);
    }
    void toggleRunChanged(ChangeEvent<bool> evt)
    {
        Debug.Log(evt.newValue);
    }
    void dataSetEnumFieldChanged(ChangeEvent<System.Enum> evt)
    {
        Debug.Log(evt.newValue);
        test.dataSet = (dataSetsEnum)evt.newValue;
    }
    private void FixedUpdate()
    {
        if (toggleRun.value)
        {
            if (!test.texturesChanged)
            {
                test.step += 1;

            }
        }
    }



    // Update is called once per frame
    void Update()
    {
    
    }
}
