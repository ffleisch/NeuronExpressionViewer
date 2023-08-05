using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    BufferedAttributeArrayTextureController arrayTextureController;
    ConnectionMeshController meshController;
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

        arrayTextureController = gameObject.AddComponent<BufferedAttributeArrayTextureController>();
        arrayTextureController.init();
        arrayTextureController.material = GetComponent<Renderer>().sharedMaterial;
        
        setExpression(textFieldExpression.value);

        meshController = GetComponent<ConnectionMeshController>();


        MeshFilter mf = GetComponent<MeshFilter>();
        center = mf.mesh.vertices.Aggregate(new Vector3(0, 0, 0), (s, v) => s + transform.TransformPoint(v)) / mf.mesh.vertices.Length;
        center = center - transform.position;
        center.z *=-1;//no idea why this is necessary, but i figured it out using gizmos

    }

    const int maxTokens = 256;
    const int maxValues = 128;
    Vector3 center;
    void expressionChanged(ChangeEvent<string> evt)
    {
        setExpression(evt.newValue);
    }


    void setExpression(string expression)
    {
        //Debug.Log(expression);
        InfixParser parser = new();

        (var shaderValues, var shaderTokens, var neededAttributes) = parser.parseToShaderArrays(expression);
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
        arrayTextureController.setAttributesToBeLoaded(neededAttributes);
    }


    Vector2 mouseStartDrag;
    Quaternion startRotation;
    Vector3 startPosition;

    bool succesfullClick = false;

    bool checkMouseOnModel()
    {
        RaycastHit hit;
        if (!Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
            return false;
        return hit.collider.gameObject == this.gameObject;
    }

   
    private void OnMouseUp()
    {
    }

    private void OnMouseDrag()
    {
    }



    void stepChanged(ChangeEvent<int> evt)
    {
        arrayTextureController.step = evt.newValue;

        if (meshController) {
            meshController.step =evt.newValue;
        }
        Debug.Log(evt + " " + evt.newValue);
    }
    void toggleRunChanged(ChangeEvent<bool> evt)
    {
        Debug.Log(evt.newValue);
    }
    void dataSetEnumFieldChanged(ChangeEvent<System.Enum> evt)
    {
        Debug.Log(evt.newValue);
        meshController.dataSet = (dataSetsEnum)evt.newValue;
        arrayTextureController.dataSet = (dataSetsEnum)evt.newValue;
    }
    private void FixedUpdate()
    {
        if (toggleRun.value)
        {

                if (Time.frameCount % 1 == 0) { 
                    arrayTextureController.attemptIncrementStep(1);
                    sliderStep.value = arrayTextureController.step;
                }
        }


    }



    // Update is called once per frame
    Vector3 dragPositionStart;
    Vector2 dragMousePositionStart;
    bool succesfullMiddleDrag = false;
    void Update()
    {
        Camera cam = Camera.main;
        float scroll_delta = Input.mouseScrollDelta.y;
        cam.fieldOfView += scroll_delta * 3;
        if (checkMouseOnModel())
        {
            if (Input.GetMouseButtonDown(2))
            {
                succesfullMiddleDrag = true;
                dragPositionStart = transform.position;
                dragMousePositionStart = Input.mousePosition;
            }
            if (Input.GetMouseButtonDown(0))
            {
                succesfullClick = true;
                startRotation = transform.rotation;
                startPosition = transform.position;
                mouseStartDrag = Input.mousePosition;
            }

        };

        if (Input.GetMouseButton(2))
        {
            if (succesfullMiddleDrag)
            {
                Vector2 delta = (Vector2)Input.mousePosition - dragMousePositionStart;
                Vector3 up = cam.transform.TransformVector(Vector3.up);
                Vector3 right = cam.transform.TransformVector(Vector3.right);
                float speed =0.5f;
                transform.position = dragPositionStart + up * delta.y*speed + right * delta.x*speed;
            }

        }
        if (Input.GetMouseButton(0))
        {
            if (succesfullClick)
            {
                Vector2 delta = mouseStartDrag - (Vector2)Input.mousePosition;

                transform.rotation = startRotation;// * Quaternion.EulerAngles(dPos.x/100f,dPos.y/100f,0);
                transform.position = startPosition;
                Vector3 worldCenter = transform.TransformPoint(center);
                //Debug.Log(worldCenter);
                //Debug.DrawLine(new Vector3(0, 0, 0), worldCenter);
                Vector3 up = cam.transform.TransformVector(Vector3.up);
                Vector3 left = cam.transform.TransformVector(Vector3.forward);

                up = transform.InverseTransformVector(up);
                left = transform.InverseTransformVector(left);
                
                transform.RotateAround(worldCenter, Vector3.up, 100 * delta.x / Screen.width);
                transform.RotateAround(worldCenter, Vector3.left, 100 * delta.y / Screen.width);
            }
        }




        if (Input.GetMouseButtonUp(2))
        {
            succesfullMiddleDrag= false;
        }
        if (Input.GetMouseButtonUp(0)) { 
            succesfullClick=false;
        }

    }


   /* private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawCube(transform.TransformPoint(center),Vector3.one*10);
        Gizmos.color = Color.green;
        Gizmos.DrawCube(Vector3.zero, Vector3.one * 2);

    }*/


}
