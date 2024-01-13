using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.IO;
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
        GUILayout.Label("UI Object Template Generator", EditorStyles.boldLabel);

        GUILayout.Space(10);

        // Template Selection Dropdown
        string[] templateNames = GetTemplateNames();
        selectedTemplateIndex = EditorGUILayout.Popup("Select Template", selectedTemplateIndex, templateNames);

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
        for (int i = elements.Count - 1; i >= 0; i--)
        {
            EditorGUILayout.BeginHorizontal();

            //Set Space for Children Objects
            GUILayout.Space(ParentHierarchySpace(elements[i].parentName, i) * 50);

            // Element Type (Button, Text, Image)
            elements[i].elementType = (UIElementType)EditorGUILayout.EnumPopup("Element Type", elements[i].elementType);

            GUILayout.Space(50);
            // Element Name
            elements[i].elementName = EditorGUILayout.TextField("Element Name", elements[i].elementName);

            // Parent Dropdown
            GUILayout.Space(50);
            string[] elementNames = GetElementNames(elements[i].elementName);
            int selectedParentIndex =
                EditorGUILayout.Popup("Parent", GetParentIndex(elements[i].parentName), elementNames);
            elements[i].parentName = (selectedParentIndex >= 0) ? elementNames[selectedParentIndex] : "";

            // Additional properties based on element type
            switch (elements[i].elementType)
            {
                case UIElementType.Button:
                    elements[i].buttonText = EditorGUILayout.TextField("Button Text", elements[i].buttonText);
                    break;
                case UIElementType.Text:
                    elements[i].textValue = EditorGUILayout.TextField("Text Value", elements[i].textValue);
                    elements[i].fontSize = EditorGUILayout.IntField("Font Size", elements[i].fontSize);
                    break;
                case UIElementType.Image:
                    elements[i].spritePath = EditorGUILayout.TextField("Sprite Path", elements[i].spritePath);
                    break;
            }

            EditorGUILayout.BeginVertical();
            // Position
            elements[i].position = EditorGUILayout.Vector2Field("Position", elements[i].position);

            // Rotation
            elements[i].rotation = EditorGUILayout.Vector3Field("Rotation", elements[i].rotation);

            // Scale
            elements[i].scale = EditorGUILayout.Vector3Field("Scale", elements[i].scale);

            EditorGUILayout.EndVertical();

            // Remove button for each element
            if (GUILayout.Button("Remove", GUILayout.Width(80)))
            {
                elements.RemoveAt(i);
            }

            // Move Up button
            if (GUILayout.Button("Down", GUILayout.Width(45)))
            {
                if (i > 0)
                {
                    SwapElements(i, i - 1);
                    EditorGUILayout.EndHorizontal();
                    break; // break after swapping to avoid conflicts
                }
            }

            // Move Down button
            if (GUILayout.Button("Up", GUILayout.Width(45)))
            {
                if (i < elements.Count - 1)
                {
                    SwapElements(i, i + 1);
                    EditorGUILayout.EndHorizontal();
                    break; // break after swapping to avoid conflicts
                }
            }



            GUILayout.Space(20);
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();
        // Add Element button
        if (GUILayout.Button("Add Element"))
        {
            elements.Add(new UIObjectTemplateElement());
        }

        GUILayout.Space(10);

        // Generate Template button
        if (GUILayout.Button("Generate Template"))
        {
            GenerateTemplateFromInput();
        }

        GUILayout.Space(10);

        // Instantiate UI button
        if (GUILayout.Button("Instantiate UI"))
        {
            InstantiateUITemplate();
        }
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


    private string GetParentName(string parentName)
    {
        for (int i = 0; i < elements.Count; i++)
        {
            if (elements[i].elementName == parentName && GetParentIndex(elements[i].parentName) != 0)
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

        try
        {
           SaveSystem.SaveTemplate(templateData); // Save Template Data
           SaveSystem.LoadTemplates(templateList); // Refresh template list
        }
        catch (Exception e)
        {
            Debug.LogError("Error writing JSON file: " + e.Message);
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
        GameObject canvasGO = new GameObject(templateData.templateName + "_Canvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        // Create UI elements based on the template data
        foreach (var elementData in templateData.elements)
        {
            GameObject uiElementGO = new GameObject(elementData.elementName);
            RectTransform rectTransform = uiElementGO.AddComponent<RectTransform>();
            rectTransform.SetParent(canvas.transform);

            switch (elementData.elementType)
            {
                case UIElementType.Button:
                    Button button = uiElementGO.AddComponent<Button>();
                    Text buttonText = uiElementGO.AddComponent<Text>();
                    buttonText.text = elementData.buttonText;
                    break;
                case UIElementType.Text:
                    Text text = uiElementGO.AddComponent<Text>();
                    text.text = elementData.textValue;
                    text.fontSize = elementData.fontSize;
                    break;
                case UIElementType.Image:
                    Image image = uiElementGO.AddComponent<Image>();
                    // Load and set the sprite from the specified path
                    Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(elementData.spritePath);
                    if (sprite != null)
                        image.sprite = sprite;
                    break;
            }

            // Set position, rotation, and scale based on the template data
            rectTransform.anchoredPosition = new Vector2(elementData.position.x, elementData.position.y);
            rectTransform.eulerAngles = elementData.rotation;
            rectTransform.localScale = elementData.scale;

            // Set the parent based on the template data
            if (!string.IsNullOrEmpty(elementData.parentName))
            {
                GameObject parent = GameObject.Find(elementData.parentName);
                if (parent != null)
                {
                    rectTransform.SetParent(parent.transform);
                }
            }
        }
    }
}

public enum UIElementType
{
    Button,
    Text,
    Image
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
    public string textValue; // Additional properties for Text
    public int fontSize; // Additional properties for Text
    public string spritePath; // Additional properties for Image
    public Vector2 position; // Position for UI elements
    public Vector3 rotation; // Rotation for UI elements
    public Vector3 scale; // Scale for UI elements
    public string parentName; // Parent's name for UI elements
    [NonSerialized] public List<UIObjectTemplateElement> children = new List<UIObjectTemplateElement>();
}