using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine.UI;

public class UIObjectTemplateGenerator : EditorWindow
{
    private List<UIObjectTemplateData> templateList = new List<UIObjectTemplateData>();
    private int selectedTemplateIndex = 0;

    private string jsonFolderPath;
    private string templateName;
    private List<UIObjectTemplateElement> elements = new List<UIObjectTemplateElement>();
    private Vector2 scrollPosition;

    private const string defaultSaveFolder = "Assets/UIObjectTemplates"; // Default folder path

    [MenuItem("Tools/UI Object Template Generator")]
    public static void ShowWindow()
    {
        GetWindow<UIObjectTemplateGenerator>("UI Template Generator");
    }

    private void OnEnable()
    {
        jsonFolderPath = defaultSaveFolder;
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

        GUILayout.Space(10);

        GUILayout.Label("Template Name:");
        templateName = EditorGUILayout.TextField(templateName);

        GUILayout.Space(10);

        GUILayout.Label("UI Elements:");

        // Display UI elements list
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        for (int i = 0; i < elements.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            
            //Set Space for Children Objects
            GUILayout.Space(ParentHierarchySpace(elements[i].parentName) * 50 );
            
            // Element Type (Button, Text, Image)
            elements[i].elementType = (UIElementType)EditorGUILayout.EnumPopup("Element Type", elements[i].elementType);

            GUILayout.Space( 50 );
            // Element Name
            elements[i].elementName = EditorGUILayout.TextField("Element Name", elements[i].elementName);

            // Parent Dropdown
            GUILayout.Space( 50 );
            string[] elementNames = GetElementNames();
            int selectedParentIndex = EditorGUILayout.Popup("Parent", GetParentIndex(elements[i].parentName), elementNames);
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
                break;
            }
            
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(20);
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
        }

        GUILayout.Space(10);

        // Instantiate UI button
        if (GUILayout.Button("Instantiate UI"))
        {
        }
    }

    private int ParentHierarchySpace(string parentName)
    {
        if (parentName is "None" or null)
        {
            return 0;
        }

        return ParentHierarchySpace (GetParentName(parentName)) + 1;
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

    private string[] GetElementNames()
    {
        string[] elementNames = new string[elements.Count + 1];
        elementNames[0] = "None";

        for (int i = 0; i < elements.Count; i++)
        {
            elementNames[i + 1] = elements[i].elementName;
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
}
