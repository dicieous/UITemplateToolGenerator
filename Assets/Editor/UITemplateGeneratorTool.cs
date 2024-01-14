using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class UIObjectTemplateGenerator : EditorWindow
{
    private const int MAX_HEIRARCHY_DEPTH = 50;

    private List<UIObjectTemplateData> templateList = new List<UIObjectTemplateData>();
    private int selectedTemplateIndex = 0;

    private string templateName;
    private List<UIObjectTemplateElement> elements = new List<UIObjectTemplateElement>();
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
    }

    private void OnGUI()
    {
        DrawInstantiateElementTab();
    }


    private void DisplayUIElements(List<UIObjectTemplateElement> elementList)
    {
        for (int i = elements.Count - 1; i >= 0; i--)
        {
            EditorGUILayout.BeginHorizontal();

            GUILayout.Space(ParentHierarchySpace(elements[i].parentName, i) * 50);
            EditorGUILayout.BeginVertical();
            //Set Space for Children Objects

            // Element Type (Button, Text, Image)
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField(">>", EditorStyles.boldLabel, GUILayout.Width(20));
            EditorGUILayout.LabelField("Element Type", GUILayout.Width(80));
            elements[i].elementType =
                (UIElementType)EditorGUILayout.EnumPopup(elements[i].elementType, GUILayout.Width(70));

            EditorGUILayout.EndHorizontal();
            GUILayout.Space(20);

            // Element Name
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(20);

            EditorGUILayout.LabelField("|", EditorStyles.boldLabel, GUILayout.Width(10));
            EditorGUILayout.LabelField("Element Name", GUILayout.Width(90));
            elements[i].elementName = EditorGUILayout.TextField(elements[i].elementName, GUILayout.Width(80));

            EditorGUILayout.EndVertical();
            GUILayout.Space(20);

            // Parent Dropdown
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(20);

            EditorGUILayout.LabelField("|", EditorStyles.boldLabel, GUILayout.Width(10));
            EditorGUILayout.LabelField("Parent", GUILayout.Width(50));
            string[] elementNames = GetElementNames(elements[i].elementName);
            int selectedParentIndex =
                EditorGUILayout.Popup(GetParentIndex(elements[i].parentName), elementNames, GUILayout.Width(70));
            elements[i].parentName = (selectedParentIndex >= 0) ? elementNames[selectedParentIndex] : "";

            EditorGUILayout.EndHorizontal();

            GUILayout.Space(20);
            EditorGUILayout.EndVertical();

            // Additional properties based on element type
            switch (elements[i].elementType)
            {
                case UIElementType.Button:

                    EditorGUILayout.LabelField("|", EditorStyles.boldLabel, GUILayout.Width(10));
                    EditorGUILayout.LabelField("Name Of Button", GUILayout.Width(100));
                    elements[i].buttonText = EditorGUILayout.TextField(elements[i].buttonText, GUILayout.Width(80));
                    GUILayout.Space(20);

                    EditorGUILayout.LabelField("|", EditorStyles.boldLabel, GUILayout.Width(10));
                    EditorGUILayout.LabelField("Image on Button", GUILayout.Width(100));
                    // Button to open the file panel and select an image
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

                    break;
                case UIElementType.Text:

                    EditorGUILayout.LabelField("|", EditorStyles.boldLabel, GUILayout.Width(10));
                    EditorGUILayout.LabelField("Text Value", GUILayout.Width(80));
                    elements[i].textValue = EditorGUILayout.TextField(elements[i].textValue);

                    EditorGUILayout.LabelField("|", EditorStyles.boldLabel, GUILayout.Width(10));
                    EditorGUILayout.LabelField("Font Size", GUILayout.Width(60));
                    elements[i].fontSize = EditorGUILayout.IntField(elements[i].fontSize);
                    GUILayout.Space(20);

                    break;
                case UIElementType.Image:

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

                    break;
                case UIElementType.Template:

                    EditorGUILayout.LabelField("|", EditorStyles.boldLabel, GUILayout.Width(10));
                    EditorGUILayout.LabelField("Template Reference", GUILayout.Width(120));

                    string[] templateNames = GetTemplateNames();
                    elements[i].templateReferenceIndex = EditorGUILayout.Popup(elements[i].templateReferenceIndex,
                        templateNames, GUILayout.Width(150));
                    elements[i].templateReference = elements[i].templateReferenceIndex > 0
                        ? templateNames[elements[i].templateReferenceIndex]
                        : "";
                    GUILayout.Space(20);

                    break;
            }

            GUILayout.Space(20);
            EditorGUILayout.LabelField("|", EditorStyles.boldLabel, GUILayout.Width(10));
            EditorGUILayout.BeginVertical();
            // Position
            //EditorGUILayout.RectField("Rect Values ", new Rect());
            elements[i].position = EditorGUILayout.Vector2Field("Position", elements[i].position);

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
                    SwapElements(i, i + 1);

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
                    SwapElements(i, i - 1);

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


    private void DrawInstantiateElementTab()
    {
        GUIStyle guiStyle = new GUIStyle(GUI.skin.label);
        guiStyle.fontSize = 14;
        guiStyle.alignment = TextAnchor.MiddleLeft;
        guiStyle.fontStyle = FontStyle.Bold;

        GUILayout.Label("UI OBJECT TEMPLATE GENERATOR", guiStyle);

        GUILayout.Space(10);

        // Template Selection Dropdown
        var label = "Select Template";
        TemplateDropDownSelector(label);

        GUILayout.Space(10);

        GUILayout.Label("Selected Template: " + GetCurrentTemplateName(), EditorStyles.boldLabel);

        GUILayout.Space(10);

        GUILayout.Label("Generated JSON File Path:");
        EditorGUILayout.LabelField(SaveSystem.GetCurrentJsonFilePath(templateList, selectedTemplateIndex));

        GUILayout.Space(10);


        GUILayout.Label("Template Name:");
        templateName = EditorGUILayout.TextField(templateName);

        GUILayout.Space(10);

        GUILayout.Label("UI Elements:");

        if (selectedTemplateIndex > 0)
        {
            // Load and display UI elements from the selected template
            UIObjectTemplateData selectedTemplate = templateList[selectedTemplateIndex - 1];
            elements = new List<UIObjectTemplateElement>(selectedTemplate.elements);
        }

        // Display UI elements list
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        DisplayUIElements(elements);

        EditorGUILayout.EndScrollView();

        EditorGUILayout.BeginHorizontal();


        // Add Element button
        if (GUILayout.Button("ADD ELEMENT", ButtonsGUI(), GUILayout.Height(30)))
        {
            elements.Add(new UIObjectTemplateElement());
            SwapElements(elements.Count - 1, elements.Count - 2);
        }

        GUILayout.Space(5);

        // Generate Template button
        if (GUILayout.Button("GENERATE TEMPLATE", ButtonsGUI(), GUILayout.Height(30)))
        {
            GenerateTemplateFromInput();
        }

        GUILayout.Space(5);

        // Instantiate UI button
        if (GUILayout.Button("INSTANTIATE UI", ButtonsGUI(), GUILayout.Height(30)))
        {
            InstantiateUITemplate();
        }

        EditorGUILayout.EndHorizontal();
    }


    private GUIStyle ButtonsGUI()
    {
        GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
        guiStyle.fontSize = 16;
        guiStyle.fontStyle = FontStyle.Bold;

        return guiStyle;
    }


    private void SwapElements(int indexA, int indexB)
    {
        (elements[indexA], elements[indexB]) = (elements[indexB], elements[indexA]);
    }

    private int ParentHierarchySpace(string parentName, int selectedIndex, int currentDepth = 0)
    {
        if (parentName is "None" or null || currentDepth >= MAX_HEIRARCHY_DEPTH)
        {
            return 0;
        }

        int parentIndex = GetParentIndex(parentName);

        if (parentIndex < 0 || parentIndex == selectedIndex)
        {
            return 0;
        }

        try
        {
            return ParentHierarchySpace(GetParentName(parentName), selectedIndex, currentDepth + 1) + 1;
        }
        catch (Exception e)
        {
            Debug.LogError($"Exception in ParentHierarchySpace: {e.Message}");
            return 0;
        }
    }


    private string GetParentName(string childName)
    {
        for (int i = 0; i < elements.Count; i++)
        {
            if (elements[i].elementName == childName && GetParentIndex(elements[i].parentName) != 0)
            {
                return elements[i].parentName; // returning the parent object
            }
        }

        return "None";
    }

    private int GetParentIndex(string parentName)
    {
        for (int i = 0; i < elements.Count; i++)
        {
            if (elements[i].elementName == parentName)
            {
                return i + 1; // Adding 1 because 0 is reserved for "None"
            }
        }

        return 0; // "None"
    }

    private string[] GetElementNames(string currentElementName)
    {
        string[] elementNames = new string[elements.Count + 1];
        elementNames[0] = "None";

        for (int i = 0; i < elements.Count; i++)
        {
            if (elements[i].elementName != currentElementName)
            {
                elementNames[i + 1] = elements[i].elementName;
            }
        }

        return elementNames;
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

    private string GetCurrentTemplateName()
    {
        if (selectedTemplateIndex == 0)
        {
            return "None (Creating New Template)";
        }

        return templateList[selectedTemplateIndex - 1].templateName;
    }


    private void TemplateDropDownSelector(string label)
    {
        string[] templateNames = GetTemplateNames();
        selectedTemplateIndex = EditorGUILayout.Popup(label, selectedTemplateIndex, templateNames);
    }

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
        else
        {
            // Save template element if it's a template
            UIObjectTemplateElement templateElement = new UIObjectTemplateElement
            {
                elementType = UIElementType.Template,
                elementName = templateName,
                templateData = templateData
            };

            elements.Add(templateElement);
        }
    }

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
        foreach (var elementData in templateData.elements)
        {
            GameObject uiElementGameObject = new GameObject(elementData.elementName);
            RectTransform rectTransform = uiElementGameObject.AddComponent<RectTransform>();
            rectTransform.SetParent(canvas.transform);

            createdElements[elementData.elementName] = uiElementGameObject;

            switch (elementData.elementType)
            {
                case UIElementType.Button:

                    Button button = uiElementGameObject.AddComponent<Button>();
                    Image buttonImage = uiElementGameObject.AddComponent<Image>();

                    Sprite buttonSprite = AssetDatabase.LoadAssetAtPath<Sprite>(elementData.buttonSpritePath);
                    if (buttonSprite != null)
                    {
                        buttonImage.sprite = buttonSprite;
                        button.targetGraphic = buttonImage;
                    }

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
                    if (sprite != null) image.sprite = sprite;

                    break;

                case UIElementType.Template:

                    if (!string.IsNullOrEmpty(elementData.templateReference))
                    {
                        int templateIndex = GetTemplateIndex(elementData.templateReference);
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
            rectTransform.eulerAngles = elementData.rotation;
            rectTransform.localScale = elementData.scale;
        }

        foreach (var elementData in templateData.elements)
        {
            // Set the parent based on the template data
            if (!string.IsNullOrEmpty(elementData.parentName) /*|| elementData.parentName != "None"*/)
            {
                if (createdElements.ContainsKey(elementData.parentName) &&
                    createdElements.ContainsKey(elementData.elementName))
                {
                    createdElements[elementData.elementName].transform
                        .SetParent(createdElements[elementData.parentName].transform, false);
                }
                else
                {
                    Debug.LogError($"Parent '{elementData.parentName}' not found for '{elementData.elementName}'");
                }
            }
        }
    }

    private void CreateUIObjects(UIObjectTemplateData templateData, RectTransform parentTransform)
    {
        // Dictionary to store created GameObjects by element name
        Dictionary<string, GameObject> createdObjects = new Dictionary<string, GameObject>();

        // First pass: Create all GameObjects without setting parents
        foreach (var elementData in templateData.elements)
        {
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
                        button.targetGraphic = buttonImage;
                    }

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
                    if (sprite != null) image.sprite = sprite;

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
                    Debug.LogError($"Parent '{elementData.parentName}' not found for '{elementData.elementName}'");
                }
            }
        }
    }

    private int GetTemplateIndex(string templateName)
    {
        for (int i = 0; i < templateList.Count; i++)
        {
            if (templateList[i].templateName == templateName)
            {
                return i + 1; // Adding 1 because 0 is reserved for "Create New Template"
            }
        }

        return 0; // Template not found
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
    public Vector2 position; // Position for UI elements
    public Vector3 rotation; // Rotation for UI elements
    public Vector3 scale; // Scale for UI elements
    public string parentName; // Parent's name for UI elements
    public string templateReference; // Template reference for UI elements
    public int templateReferenceIndex; // Index of the selected template

    //colorData
    public float r;
    public float g;
    public float b;
    public float a;

    [NonSerialized] public UIObjectTemplateData templateData; // Template data for template elements
}