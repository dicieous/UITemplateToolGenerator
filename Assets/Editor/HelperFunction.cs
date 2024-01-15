using System;
using System.Collections.Generic;
using UnityEngine;

public static class HelperFunction 
{
    private const int MAX_HEIRARCHY_DEPTH = 50;
    
      public static GUIStyle ButtonsGUI()
    {
        GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
        guiStyle.fontSize = 16;
        guiStyle.fontStyle = FontStyle.Bold;

        return guiStyle;
    }

    public static void SwapElements(int indexA, int indexB, List<UIObjectTemplateElement> elements)
    {
        (elements[indexA], elements[indexB]) = (elements[indexB], elements[indexA]);
    }

    public static int ParentHierarchySpace(string parentName, int selectedIndex, List<UIObjectTemplateElement> elements, int currentDepth = 0 )
    {
        if (parentName is "None" or null || currentDepth >= MAX_HEIRARCHY_DEPTH)
        {
            return 0;
        }

        int parentIndex = GetParentIndex(parentName, elements);

        if (parentIndex < 0 || parentIndex == selectedIndex)
        {
            return 0;
        }

        try
        {
            return ParentHierarchySpace(GetParentName(parentName , elements), selectedIndex, elements,currentDepth + 1) + 1;
        }
        catch (Exception e)
        {
            Debug.LogError($"Exception in ParentHierarchySpace: {e.Message}");
            return 0;
        }
    }

    private static string GetParentName(string childName , List<UIObjectTemplateElement> elements)
    {
        for (int i = 0; i < elements.Count; i++)
        {
            if (elements[i].elementName == childName && GetParentIndex(elements[i].parentName, elements) != 0)
            {
                return elements[i].parentName; // returning the parent object
            }
        }

        return "None";
    }

    public static int GetParentIndex(string parentName, List<UIObjectTemplateElement> elements)
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

    public static string[] GetElementNames(string currentElementName, List<UIObjectTemplateElement> elements)
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
    
    
    public static string GetCurrentTemplateName(int selectedTemplateIndex, List<UIObjectTemplateData> templateList)
    {
        if (selectedTemplateIndex == 0)
        {
            return "None (Creating New Template)";
        }

        return templateList[selectedTemplateIndex - 1].templateName;
    }

    public static int GetTemplateIndex(string templateNAME, List<UIObjectTemplateData> templateList)
    {
        for (int i = 0; i < templateList.Count; i++)
        {
            if (templateList[i].templateName == templateNAME)
            {
                return i + 1; // Adding 1 because 0 is reserved for "Create New Template"
            }
        }

        return 0; // Template not found
    }

}
