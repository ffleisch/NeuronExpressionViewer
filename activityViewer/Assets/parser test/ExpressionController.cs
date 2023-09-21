using System.Collections;
using System.Collections.Generic;
using System.Linq;
//using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class ExpressionController : MonoBehaviour
{

    //main script for reading user input and controlling other components



    //field to be bound to the ui
    VisualElement root;

    private SliderInt sliderStep;
    private Slider sliderLineWidth;

    public DropdownField enumFieldDataset;
    public DropdownField gradientDropdownField;

    public Toggle toggleRun;

    public Toggle toggleShowNew;
    public Toggle toggleShowSame;
    public Toggle toggleShowRemoved;

    public Label labelSelectedNeuron;
    public Label labelSelectedArea;
    public Label labelCurrentFrame;


    public RadioButtonGroup radioButtonGroupSelection;

    TextField textFieldExpression;
    Label labelStatus;
    //ExpressionTexturesControlller textureController;
    // Start is called before the first frame update
    BufferedAttributeArrayTextureController arrayTextureController;
    ConnectionMeshController meshController;
    public Dictionary<string, Texture2D> gradientDict = new();
    public Dictionary<string, int> gradientIndexDict = new();
    public Dictionary<string, dataSetsEnum> datasetDropdownDict = new();

    WhereMousePoint mousePointer;
    void Start()
    {
        var uiDocument = GetComponent<UIDocument>();
        mousePointer = GetComponent<WhereMousePoint>();
        meshController = GetComponent<ConnectionMeshController>();
        
        //textureController = gameObject.AddComponent<ExpressionTexturesControlller>();

        root = uiDocument.rootVisualElement;
        textFieldExpression = root.Q("TextFieldExpression") as TextField;
        labelStatus = root.Q("LabelStatus") as Label;

        enumFieldDataset = root.Q("EnumFieldDataset") as DropdownField;
        enumFieldDataset.RegisterCallback<ChangeEvent<string>>(dataSetEnumFieldChanged);

        enumFieldDataset.choices.Clear();
        foreach (dataSetsEnum d in System.Enum.GetValues(typeof(dataSetsEnum))) {
            string name =d.ToString();
            datasetDropdownDict[name] = d;
            enumFieldDataset.choices.Add(name);
        }


        labelCurrentFrame = root.Q("LabelCurrentFrame") as Label;
        labelSelectedNeuron= root.Q("LabelSelectedNeuron") as Label;
        labelSelectedArea= root.Q("LabelSelectedArea") as Label;



        toggleRun = root.Q("ToggleRun") as Toggle;
        sliderStep = root.Q("SliderIntFrame") as SliderInt;

        sliderLineWidth = root.Q("SliderLineWidth") as Slider;
        sliderLineWidth.RegisterCallback<ChangeEvent<float>>(lineWidthChanged);
        lineWidthChanged(new ChangeEvent<float>());


        toggleShowNew= root.Q("ToggleNew") as Toggle;
        toggleShowSame = root.Q("ToggleSame") as Toggle;
        toggleShowRemoved = root.Q("ToggleRemoved") as Toggle;
        toggleShowNew.RegisterCallback<ChangeEvent<bool>>(connectionShowChanged);
        toggleShowSame.RegisterCallback<ChangeEvent<bool>>(connectionShowChanged);
        toggleShowRemoved.RegisterCallback<ChangeEvent<bool>>(connectionShowChanged);

        radioButtonGroupSelection = root.Q("RadioButtonGroupSelection") as RadioButtonGroup;
        radioButtonGroupSelection.RegisterCallback<ChangeEvent<int>>(connectionSelectionChanged);
        radioButtonGroupSelection.value = 0;

        textFieldExpression.RegisterValueChangedCallback(expressionChanged);
        toggleRun.RegisterCallback<ChangeEvent<bool>>(toggleRunChanged);
        sliderStep.RegisterCallback<ChangeEvent<int>>(stepChanged);

        arrayTextureController = gameObject.AddComponent<BufferedAttributeArrayTextureController>();
        arrayTextureController.init();
        arrayTextureController.material = GetComponent<Renderer>().sharedMaterial;
        
        setExpression(textFieldExpression.value);



        MeshFilter mf = GetComponent<MeshFilter>();
        center = mf.mesh.vertices.Aggregate(new Vector3(0, 0, 0), (s, v) => s + transform.TransformPoint(v)) / mf.mesh.vertices.Length;
        center = center - transform.position;
        center.z *=-1;//no idea why this is necessary, but i figured it out using gizmos
        
        connectionShowChanged(new());
        VisualTreeAsset gradientDropdownPrefab = Resources.Load<VisualTreeAsset>("gradientDropdown");
        Debug.Log(gradientDropdownPrefab);
        gradientDropdownField = root.Q("DropdownFieldGradient") as DropdownField;
        gradientDropdownField.choices = new();
        List<string> choiceList=new();
        int num = 0;
        foreach (Texture2D gradient in Resources.LoadAll("gradients")) {

            gradientDict[gradient.name]=gradient;
            gradientIndexDict[gradient.name] =num;
            var root = gradientDropdownPrefab.CloneTree();
            //gradientDropdownField.Add(new Label(gradient.name));

            gradientDropdownField.choices.Add(gradient.name);

            choiceList.Add(gradient.name);
            
            Label labelName=root.Q("LabelName")as Label;
            //labelName.text =gradient.name;
            num++;
        };
        gradientDropdownField.RegisterCallback<ChangeEvent<string>>(selectedGradientChanged);
        gradientDropdownField.index=gradientIndexDict["Magma"];
        //var newGradientDropdown = new DropdownField("Gradient",choiceList,0);
        //var groupBoxGradientContainer = root.Q("GroupBoxGradientContainer");
        //groupBoxGradientContainer.Clear();
        
        enumFieldDataset.value = dataSetsEnum.viz_no_network.ToString();
    }

    const int maxTokens = 256;
    const int maxValues = 128;
    Vector3 center;
    void expressionChanged(ChangeEvent<string> evt)
    {
        setExpression(evt.newValue);
    }

    void connectionShowChanged(ChangeEvent<bool> evt) {
        meshController.setConnectionsVisibility(toggleShowNew.value,toggleShowSame.value,toggleShowRemoved.value);
    }

    void connectionSelectionChanged(ChangeEvent<int> evt) {
        meshController.setConnectionsSelection(radioButtonGroupSelection.value);
    }

    void selectedGradientChanged(ChangeEvent<string> evt) {
        Debug.Log(gradientDict[evt.newValue]);
        var mat = GetComponent<Renderer>().sharedMaterial;
        mat.SetTexture("_gradientTex",gradientDict[evt.newValue]); 
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
        labelCurrentFrame.text =evt.newValue.ToString();
    }



    void lineWidthChanged(ChangeEvent<float> evt) {
        mousePointer.lineWidth = sliderLineWidth.value;
        mousePointer.setShaderParams();
    }

    void toggleRunChanged(ChangeEvent<bool> evt)
    {
        Debug.Log(evt.newValue);
    }
    void dataSetEnumFieldChanged(ChangeEvent<string> evt)
    {
        dataSetsEnum newValue =datasetDropdownDict[evt.newValue];
        Debug.Log(evt.newValue);
        meshController.dataSet = newValue;
        arrayTextureController.dataSet = newValue;
    }
    private void FixedUpdate()
    {
        if (toggleRun.value)
        {

                if (Time.frameCount % 1 == 0) { 
                    arrayTextureController.attemptIncrementStep(1);
                    sliderStep.value = arrayTextureController.step;
                    labelCurrentFrame.text =arrayTextureController.step.ToString();
                }
        }
        labelSelectedArea.text =mousePointer.area_index.ToString() ;
        labelSelectedNeuron.text =mousePointer.index.ToString() ;

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
