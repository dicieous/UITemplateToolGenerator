using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class UIObjectTemplateGenerator : EditorWindow
{
    public List<UIObjectTemplateData> templateList = new List<UIObjectTemplateData>();
    public int selectedTemplateIndex;

    private Rect headerSection;
    private Texture2D headerSectionTexture;
    private Color headerSectionColor;

    private string templateName;
    public List<UIObjectTemplateElement> elements = new List<UIObjectTemplateElement>();
    private Vector2 scrollPosition;

    // Default folder path

    [MenuItem("Tools/UI Object Template Generator")]
    public static void ShowWindow()
    {
        GetWindow<UIObjectTemplateGenerator>("UI Template Generator");
    }

    private void OnEnable()
    {
        SaveSystem.Init();
        SaveSystem.LoadTemplates(templateList);
        InitTexture();
    }
    
    private void InitTexture()
    {
        headerSectionColor = new Color(39f / 255f, 53f / 255f, 60f / 255f, 1f);
        headerSectionTexture = new Texture2D(1, 1);
        headerSectionTexture.SetPixel(0, 0, headerSectionColor);
        headerSectionTexture.Apply();
    }

    private void OnGUI()
    {
        DrawHeader();
        DrawInstantiateElementTab();
    }

    private void DrawHeader()
    {
        headerSection.x = 0;
        headerSection.y = 0;
        headerSection.width = Screen.width;
        headerSection.height = 179;

        GUI.DrawTexture(headerSection, headerSectionTexture);
    }

    private void DrawInstantiateElementTab()
    {
        GUIStyle guiStyle = new GUIStyle(GUI.skin.label);
        guiStyle.fontSize = 14;
        guiStyle.alignment = TextAnchor.MiddleLeft;
        guiStyle.fontStyle = FontStyle.Bold;

        //Title
        GUILayout.Label("UI OBJECT TEMPLATE GENERATOR", guiStyle);

        GUILayout.Space(10);

        // Template Selection Dropdown
        var label = "Select Template";
        TemplateDropDownSelector(label);

        GUILayout.Space(10);

        GUILayout.Label("Selected Template: " + HelperFunction.GetCurrentTemplateName(selectedTemplateIndex, templateList), EditorStyles.boldLabel);

        GUILayout.Space(10);

        GUILayout.Label("Generated JSON File Path:");
        EditorGUILayout.LabelField(SaveSystem.GetCurrentJsonFilePath(templateList, selectedTemplateIndex));

        GUILayout.Space(10);


        GUILayout.Label("Template Name:");
        templateName = EditorGUILayout.TextField(templateName);

        GUILayout.Space(10);

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("UI ELEMENTS:", EditorStyles.boldLabel, GUILayout.Height(20), GUILayout.Width(90));

        // Add Element button
        GUIStyle addButtonGui = new GUIStyle(GUI.skin.button);
        guiStyle.fontSize = 10;
        guiStyle.fontStyle = FontStyle.Bold;
        
        if (GUILayout.Button("ADD ELEMENT", addButtonGui, GUILayout.Height(20), GUILayout.Width(100)))
        {
            elements.Insert(0, new UIObjectTemplateElement());
        }

        EditorGUILayout.EndHorizontal();
        GUILayout.Space(10);

        if (selectedTemplateIndex > 0)
        {
            // Load and display UI elements from the selected template
            UIObjectTemplateData selectedTemplate = templateList[selectedTemplateIndex - 1];
            elements = new List<UIObjectTemplateElement>(selectedTemplate.elements);
        }

        // Display UI elements list
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        DisplayUIElements();

        EditorGUILayout.EndScrollView();

        EditorGUILayout.BeginHorizontal();


        GUILayout.Space(5);

        // Generate Template button
        if (GUILayout.Button("GENERATE TEMPLATE", HelperFunction.ButtonsGUI(), GUILayout.Height(30)))
        {
            GenerateTemplateFromInput();
        }

        GUILayout.Space(5);

        // Instantiate UI button
        if (GUILayout.Button("INSTANTIATE UI", HelperFunction.ButtonsGUI(), GUILayout.Height(30)))
        {
            InstantiateUITemplate();
        }

        EditorGUILayout.EndHorizontal();
    }

    // Display UI elements list
    private void DisplayUIElements()
    {
        for (int i = elements.Count - 1; i >= 0; i--)
        {
            EditorGUILayout.BeginHorizontal();

            GUILayout.Space(HelperFunction.ParentHierarchySpace(elements[i].parentName, i, elements) * 50);
            EditorGUILayout.BeginVertical();
            //Set Space for Children Objects

            // Element Type (Button, Text, Image)
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField(">>", EditorStyles.boldLabel, GUILayout.Width(20));
            EditorGUILayout.LabelField("Element Type", GUILayout.Width(80));
            elements[i].elementType =
                (UIElementType)EditorGUILayout.EnumPopup(elements[i].elementType, GUILayout.Width(80));

            EditorGUILayout.EndHorizontal();
            GUILayout.Space(20);

            // Element Name
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(25);


            EditorGUILayout.LabelField("Element Name", GUILayout.Width(90));
            elements[i].elementName = EditorGUILayout.TextField(elements[i].elementName, GUILayout.Width(80));

            EditorGUILayout.EndVertical();
            GUILayout.Space(20);

            // Parent Dropdown
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(25);


            EditorGUILayout.LabelField("Parent", GUILayout.Width(50));
            string[] elementNames = HelperFunction.GetElementNames(elements[i].elementName,elements);
            int selectedParentIndex =
                EditorGUILayout.Popup(HelperFunction.GetParentIndex(elements[i].parentName,elements), elementNames, GUILayout.Width(100));
            elements[i].parentName = (selectedParentIndex >= 0) ? elementNames[selectedParentIndex] : "";

            EditorGUILayout.EndHorizontal();
            GUILayout.Space(20);
            EditorGUILayout.EndVertical();

            // Additional properties based on element type
            switch (elements[i].elementType)
            {
                case UIElementType.Button:

                    ElementTypeButtonProperties(i);

                    break;
                case UIElementType.Text:

                    ElementTypeTextProperties(i);

                    break;
                case UIElementType.Image:

                    ElementTypeImageProperties(i);

                    break;
                case UIElementType.Template:

                    ElementTypeTemplateProperties(i);

                    break;
            }

            GUILayout.Space(20);
            EditorGUILayout.LabelField("|", EditorStyles.boldLabel, GUILayout.Width(10));
            EditorGUILayout.BeginVertical();


            EditorGUILayout.LabelField("RECT TRANSFORM VALUES", GUILayout.Width(170));
            GUILayout.Space(4);

            // Position
            elements[i].position = EditorGUILayout.Vector2Field("Position", elements[i].position);
            EditorGUILayout.Space(5);

            //Height and width
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Height", GUILayout.Width(50));
            elements[i].height = EditorGUILayout.FloatField(elements[i].height);

            EditorGUILayout.LabelField("Width", GUILayout.Width(50));
            elements[i].width = EditorGUILayout.FloatField(elements[i].width);
            EditorGUILayout.EndHorizontal();

            // Rotation
            elements[i].rotation = EditorGUILayout.Vector3Field("Rotation", elements[i].rotation);

            // Scale
            elements[i].scale = EditorGUILayout.Vector3Field("Scale", elements[i].scale);

            EditorGUILayout.EndVertical();
            EditorGUILayout.LabelField("|", EditorStyles.boldLabel, GUILayout.Width(10));


            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            // Move Down button
            if (GUILayout.Button("Up", GUILayout.Width(45)))
            {
                if (i < elements.Count - 1)
                {
                    HelperFunction.SwapElements(i, i + 1,elements);

                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndHorizontal();

                    break; // break after swapping to avoid conflicts
                }
            }

            // Move Up button
            if (GUILayout.Button("Down", GUILayout.Width(45)))
            {
                if (i > 0)
                {
                    HelperFunction.SwapElements(i, i - 1, elements);

                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndHorizontal();

                    break; // break after swapping to avoid conflicts
                }
            }

            EditorGUILayout.EndHorizontal();
            
            // Remove button for each element
            if (GUILayout.Button("Remove", GUILayout.Width(92)))
            {
                elements.RemoveAt(i);
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
            GUILayout.Space(50);
        }
    }
    
    //To select Template from the DropDown
    private void TemplateDropDownSelector(string label)
    {
        string[] templateNames = GetTemplateNames();
        selectedTemplateIndex = EditorGUILayout.Popup(label, selectedTemplateIndex, templateNames);
    }
    
    private string[] GetTemplateNames()
    {
        string[] templateNames = new string[templateList.Count + 1];
        templateNames[0] = "Create New Template";

        for (int i = 0; i < templateList.Count; i++)
        {
            templateNames[i + 1] = templateList[i].templateName;
        }

        return templateNames;
    }

    
    private void ElementTypeButtonProperties(int i)
    {
        EditorGUILayout.LabelField("|", EditorStyles.boldLabel, GUILayout.Width(10));
        EditorGUILayout.LabelField("Name Of Button", GUILayout.Width(100));
        elements[i].buttonText = EditorGUILayout.TextField(elements[i].buttonText, GUILayout.Width(80));
        GUILayout.Space(20);

        // Button to open the file panel and select an image
        EditorGUILayout.LabelField("|", EditorStyles.boldLabel, GUILayout.Width(10));
        EditorGUILayout.LabelField("Image on Button", GUILayout.Width(100));
        if (GUILayout.Button("Select", GUILayout.Width(50)))
        {
            string imagePath = EditorUtility.OpenFilePanel("Select Image", "Assets/", "png,jpg,jpeg");
            if (!string.IsNullOrEmpty(imagePath))
            {
                // Convert the absolute path to a relative path
                elements[i].buttonSpritePath = "Assets" + imagePath.Replace(Application.dataPath, "");
            }
        }

        EditorGUILayout.TextField(elements[i].buttonSpritePath, GUILayout.Width(80));
        GUILayout.Space(20);
    }

    private void ElementTypeImageProperties(int i)
    {
        EditorGUILayout.LabelField("|", EditorStyles.boldLabel, GUILayout.Width(10));
        EditorGUILayout.LabelField("Select Color", GUILayout.Width(80));

        Color color = EditorGUILayout.ColorField(new Color(elements[i].colorR,elements[i].colorG,elements[i].colorB,elements[i].colorA), GUILayout.Width(80));
        elements[i].colorR = color.r;
        elements[i].colorG = color.g;
        elements[i].colorB = color.b;
        elements[i].colorA = color.a;

        EditorGUILayout.LabelField("|", EditorStyles.boldLabel, GUILayout.Width(10));
        EditorGUILayout.LabelField("Select Image", GUILayout.Width(80));

        // Button to open the file panel and select an image
        if (GUILayout.Button("Select", GUILayout.Width(50)))
        {
            string imagePath = EditorUtility.OpenFilePanel("Select Image", "Assets/", "png,jpg,jpeg");
            if (!string.IsNullOrEmpty(imagePath))
            {
                // Convert the absolute path to a relative path
                elements[i].spritePath = "Assets" + imagePath.Replace(Application.dataPath, "");
            }
        }


        EditorGUILayout.TextField(elements[i].spritePath, GUILayout.Width(80));
        GUILayout.Space(20);
    }

    private void ElementTypeTextProperties(int i)
    {
        EditorGUILayout.LabelField("|", EditorStyles.boldLabel, GUILayout.Width(10));
        EditorGUILayout.LabelField("Text Value", GUILayout.Width(80));
        elements[i].textValue = EditorGUILayout.TextField(elements[i].textValue);

        EditorGUILayout.LabelField("|", EditorStyles.boldLabel, GUILayout.Width(10));
        EditorGUILayout.LabelField("Font Size", GUILayout.Width(60));
        elements[i].fontSize = EditorGUILayout.IntField(elements[i].fontSize);
        GUILayout.Space(20);
    }

    private void ElementTypeTemplateProperties(int i)
    {
        EditorGUILayout.LabelField("|", EditorStyles.boldLabel, GUILayout.Width(10));
        EditorGUILayout.LabelField("Template Reference", GUILayout.Width(120));

        string[] templateNames = GetTemplateNames();
        elements[i].templateReferenceIndex = EditorGUILayout.Popup(elements[i].templateReferenceIndex,
            templateNames, GUILayout.Width(150));
        elements[i].templateReference = elements[i].templateReferenceIndex > 0
            ? templateNames[elements[i].templateReferenceIndex]
            : "";
        GUILayout.Space(20);
    }
    
    

    // USED TO GENERATE AND SAVE JASON FILE
    private void GenerateTemplateFromInput()
    {
        if (string.IsNullOrEmpty(templateName))
        {
            Debug.LogError("Template Name is empty!");
            return;
        }

        UIObjectTemplateData templateData = new UIObjectTemplateData
        {
            templateName = templateName,
            elements = elements.ToArray()
        };

        // Save template data if it's a regular element
        if (selectedTemplateIndex == 0)
        {
            try
            {
                SaveSystem.SaveTemplate(templateData);
                SaveSystem.LoadTemplates(templateList);
            }
            catch (Exception e)
            {
                Debug.LogError("Error writing JSON file: " + e.Message);
            }
        }
    }

    
    //USED TO INSTANTIATE UI TEMPLATES IN HIERARCHY
    private void InstantiateUITemplate()
    {
        if (selectedTemplateIndex == 0)
        {
            Debug.LogError("Cannot instantiate UI without selecting a template.");
            return;
        }

        try
        {
            UIObjectTemplateData selectedTemplate = templateList[selectedTemplateIndex - 1];
            CreateUIObjects(selectedTemplate);

            Debug.Log("UI instantiated from template successfully!");
        }
        catch (Exception e)
        {
            Debug.LogError("Error instantiating UI from template: " + e.Message);
        }
    }

    
    //USED TO CREATE THE TEMPLATE BEFORE INSTANTIATING IT IN HIERARCHY
    private void CreateUIObjects(UIObjectTemplateData templateData)
    {
        // Instantiate a Canvas as the root
        GameObject canvasGameObject = new GameObject(templateData.templateName + "_Canvas");
        Canvas canvas = canvasGameObject.AddComponent<Canvas>();
        CanvasScaler canvasScaler = canvasGameObject.AddComponent<CanvasScaler>();
        GraphicRaycaster graphicRaycaster = canvasGameObject.AddComponent<GraphicRaycaster>();

        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        canvasScaler.referencePixelsPerUnit = 100;

        // Dictionary to store created GameObjects by element name
        Dictionary<string, GameObject> createdElements = new Dictionary<string, GameObject>();

        // First pass: Create all GameObjects without setting parents
        for (var i = templateData.elements.Length - 1; i >= 0; i--)
        {
            var elementData = templateData.elements[i];
            GameObject uiElementGameObject = new GameObject(elementData.elementName);
            RectTransform rectTransform = uiElementGameObject.AddComponent<RectTransform>();
            rectTransform.SetParent(canvas.transform);

            createdElements[elementData.elementName] = uiElementGameObject;

            switch (elementData.elementType)
            {
                case UIElementType.Button:
                    //Set Button and ButtonImage
                    Button button = uiElementGameObject.AddComponent<Button>();
                    Image buttonImage = uiElementGameObject.AddComponent<Image>();

                    Sprite buttonSprite = AssetDatabase.LoadAssetAtPath<Sprite>(elementData.buttonSpritePath);
                    if (buttonSprite != null)
                    {
                        buttonImage.sprite = buttonSprite;
                    }
                    button.targetGraphic = buttonImage;

                    break;
                case UIElementType.Text:
                    //Set Text
                    var text = uiElementGameObject.AddComponent<TextMeshProUGUI>();
                    text.text = elementData.textValue;
                    text.fontSize = elementData.fontSize;

                    break;
                case UIElementType.Image:

                    Image image = uiElementGameObject.AddComponent<Image>();
                    // Load and set the sprite from the specified path
                    Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(elementData.spritePath);
                    if (sprite != null)
                    {
                        image.sprite = sprite;
                    }
                    image.color = new Color(elementData.colorR, elementData.colorG, elementData.colorB,
                        elementData.colorA);

                    break;

                case UIElementType.Template:
                    //Set Template
                    if (!string.IsNullOrEmpty(elementData.templateReference))
                    {
                        int templateIndex =
                            HelperFunction.GetTemplateIndex(elementData.templateReference, templateList);
                        if (templateIndex > 0)
                        {
                            UIObjectTemplateData referencedTemplate = templateList[templateIndex - 1];

                            // Instantiate template values
                            RectTransform templateRectTransform = uiElementGameObject.GetComponent<RectTransform>();

                            CreateUIObjects(referencedTemplate, templateRectTransform);
                        }
                        else
                        {
                            Debug.LogError(
                                $"Template '{elementData.templateReference}' not found for '{elementData.elementName}'");
                        }
                    }

                    break;
            }

            // Set position, rotation, and scale based on the template data
            rectTransform.anchoredPosition = new Vector2(elementData.position.x, elementData.position.y);
            rectTransform.sizeDelta = new Vector2(elementData.width, elementData.height);
            rectTransform.eulerAngles = elementData.rotation;
            rectTransform.localScale = elementData.scale;
        }

        foreach (var elementData in templateData.elements)
        {
            // Set the parent based on the template data
            if (!string.IsNullOrEmpty(elementData.parentName) )
            {
                if (createdElements.ContainsKey(elementData.parentName) &&
                    createdElements.ContainsKey(elementData.elementName))
                {
                    createdElements[elementData.elementName].transform
                        .SetParent(createdElements[elementData.parentName].transform, false);
                }
                else
                {
                    if(elementData.parentName != "None") Debug.LogError($"Parent '{elementData.parentName}' not found for '{elementData.elementName}'");
                }
            }
        }
    }

    
    //OVERLOADED METHOD FOR NESTING
    private void CreateUIObjects(UIObjectTemplateData templateData, RectTransform parentTransform)
    {
        // Dictionary to store created GameObjects by element name
        Dictionary<string, GameObject> createdObjects = new Dictionary<string, GameObject>();

        // First pass: Create all GameObjects without setting parents
        for (var i = templateData.elements.Length - 1; i >= 0; i--)
        {
            var elementData = templateData.elements[i];
            GameObject uiElementGameObject = new GameObject(elementData.elementName);
            RectTransform rectTransform = uiElementGameObject.AddComponent<RectTransform>();
            rectTransform.SetParent(parentTransform);

            createdObjects[elementData.elementName] = uiElementGameObject;

            switch (elementData.elementType)
            {
                case UIElementType.Button:

                    Button button = uiElementGameObject.AddComponent<Button>();
                    Image buttonImage = uiElementGameObject.AddComponent<Image>();

                    Sprite buttonSprite = AssetDatabase.LoadAssetAtPath<Sprite>(elementData.buttonSpritePath);
                    if (buttonSprite != null)
                    {
                        buttonImage.sprite = buttonSprite;
                    }
                    button.targetGraphic = buttonImage;

                    break;
                case UIElementType.Text:

                    var text = uiElementGameObject.AddComponent<TextMeshProUGUI>();
                    text.text = elementData.textValue;
                    text.fontSize = elementData.fontSize;

                    break;
                case UIElementType.Image:

                    Image image = uiElementGameObject.AddComponent<Image>();
                    // Load and set the sprite from the specified path
                    Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(elementData.spritePath);
                    if (sprite != null)
                    {
                        image.sprite = sprite;
                    }

                    image.color = new Color(elementData.colorR, elementData.colorG, elementData.colorB,
                        elementData.colorA);

                    break;
            }


            // Set position, rotation, and scale based on the template data
            rectTransform.anchoredPosition = new Vector2(elementData.position.x, elementData.position.y);
            rectTransform.eulerAngles = elementData.rotation;
            rectTransform.localScale = elementData.scale;
        }

        // Second pass: Set parents based on parent-child relationships
        foreach (var elementData in templateData.elements)
        {
            // Set the parent based on the template data
            if (!string.IsNullOrEmpty(elementData.parentName))
            {
                if (createdObjects.ContainsKey(elementData.parentName) &&
                    createdObjects.ContainsKey(elementData.elementName))
                {
                    createdObjects[elementData.elementName].transform
                        .SetParent(createdObjects[elementData.parentName].transform, false);
                }
                else
                {
                    if(elementData.parentName != "None") Debug.LogError($"Parent '{elementData.parentName}' not found for '{elementData.elementName}'");
                }
            }
        }
    }
    
    
}

public enum UIElementType
{
    Button,
    Text,
    Image,
    Template
}

[System.Serializable]
public class UIObjectTemplateData
{
    public string templateName;
    public UIObjectTemplateElement[] elements;
}

[System.Serializable]
public class UIObjectTemplateElement
{
    public UIElementType elementType;

    public string elementName;

    public string buttonText; // Additional properties for Button
    public string buttonSpritePath; // Additional properties for Button

    public string textValue; // Additional properties for Text
    public int fontSize; // Additional properties for Text

    public string spritePath; // Additional properties for Image
    public float colorR; //colorData
    public float colorG;
    public float colorB;
    public float colorA;

    public Vector2 position; // Position for UI elements
    public Vector3 rotation; // Rotation for UI elements
    public Vector3 scale; // Scale for UI elements
    public float height; //Height for UI elements
    public float width; //width for UI elements

    public string parentName; // Parent's name for UI elements
    public string templateReference; // Template reference for UI elements
    public int templateReferenceIndex; // Index of the selected template
    
}