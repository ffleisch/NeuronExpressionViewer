using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testInterpreter : MonoBehaviour
{

    Material mat;
    // Start is called before the first frame update

    public string expression = "";
    private string _expressionOld = "";
    void Start()
    {
        var renderer = GetComponent<Renderer>();
        mat = renderer.sharedMaterial;
        //mat.SetFloatArray("values",new float[] {1f,0.2f,0.4f,0.6f,1f });

        //var tokens = new float[] {-1,-2,3};
        //mat.SetFloatArray("tokens",tokens);
        //mat.SetInt("num_tokens", tokens.Length);
        setShaderExpression(expression);
        printCurrentValuesAndTokens();
    }

    void printCurrentValuesAndTokens()
    {
        var renderer = GetComponent<Renderer>();
        mat = renderer.sharedMaterial;
        var tokens = mat.GetFloatArray("tokens");
        var values = mat.GetFloatArray("values");

        if (values != null)
        {
            string values_string = "";
            foreach (float f in values)
            {
                values_string += " " + f.ToString();
            }
            Debug.Log("actual values: " + values_string);
        }

        if (tokens != null)
        {
            string tokens_string = "";
            foreach (float f in tokens)
            {
                tokens_string += " " + f.ToString();
            }
            Debug.Log("actual tokens: " + tokens_string);

        }

    }
    const int maxTokens = 256;
    const int maxValues = 128;
    void setShaderExpression(string s)
    {

        var renderer = GetComponent<Renderer>();
        mat = renderer.sharedMaterial;

        InfixParser parse = new();
        (var values, var tokens) = parse.parseToShaderArrays(s);


        if (tokens.Count > 0)
        {
            var tokenArray = new float[maxTokens];
            tokens.CopyTo(tokenArray, 0);
            mat.SetFloatArray("tokens", tokenArray);
        }

        mat.SetInt("num_tokens", tokens.Count);

        if (values.Count > 0)
        {
            var valueArray = new float[maxValues];
            values.CopyTo(valueArray, 0);
            mat.SetFloatArray("values", valueArray);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (_expressionOld != expression)
        {
            setShaderExpression(expression);
            printCurrentValuesAndTokens();
            _expressionOld = expression;
        }
    }

}
