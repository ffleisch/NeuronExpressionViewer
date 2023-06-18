using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ExpressionController : MonoBehaviour
{



    VisualElement root;

    TextField textFieldExpression;
    Label labelStatus;
    ExpressionTexturesControlller textureController;
    // Start is called before the first frame update
    void Start()
    {
        var uiDocument = GetComponent<UIDocument>();
        textureController = gameObject.AddComponent<ExpressionTexturesControlller>();

        root = uiDocument.rootVisualElement;
        textFieldExpression = root.Q("TextFieldExpression") as TextField;
        labelStatus = root.Q("LabelStatus") as Label;

        textFieldExpression.RegisterValueChangedCallback(expressionChanged);


    }

    const int maxTokens = 256;
    const int maxValues = 128;

    void expressionChanged(ChangeEvent<string> evt)
    {
        Debug.Log(evt.newValue);
        InfixParser parser = new();

        (var shaderValues, var shaderTokens, var neededAttributes) = parser.parseToShaderArrays(evt.newValue);
        textureController.loadAttributes(neededAttributes);

        textureController.bindAttributesToShader();

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

    }


    // Update is called once per frame
    void Update()
    {

    }
}
