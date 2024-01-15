using UnityEngine;
using System.Collections.Generic;
using System.IO;

public static class SaveSystem 
{
    
    private const string DEFAULT_SAVE_FOLDER = "Assets/UIObjectTemplates";

    public static void Init()
    {
        if (!Directory.Exists(DEFAULT_SAVE_FOLDER))
        {
            Directory.CreateDirectory(DEFAULT_SAVE_FOLDER);
        }
    }
    
    
    //To Save the template
    public static void SaveTemplate(UIObjectTemplateData templateData)
    {
        string jsonData = SerializeTemplateData(templateData);
        
        string jsonFilePath = SetJsonFilePath(templateData.templateName);

        File.WriteAllText(jsonFilePath, jsonData);

        Debug.Log("UI Template generated and saved successfully!");
    }
    
    //To Load the template
    public static void LoadTemplates(List<UIObjectTemplateData> templateList)
    {
        templateList.Clear();

        string[] templateFiles = Directory.GetFiles(DEFAULT_SAVE_FOLDER, "*.json");
        foreach (string filePath in templateFiles)
        {
            string jsonContent = File.ReadAllText(filePath);
            UIObjectTemplateData templateData = DeserializeTemplateData(jsonContent);

            if (templateData != null)
            {
                templateList.Add(templateData);
            }
        }
    }
    
    private static string SetJsonFilePath( string templateName)
    {
        return Path.Combine(DEFAULT_SAVE_FOLDER, templateName + ".json");
    }

    public static string GetCurrentJsonFilePath(List<UIObjectTemplateData> templateList, int selectedTemplateIndex)
    {
        if (selectedTemplateIndex == 0)
        {
            return "None (Creating New Template)";
        }

        return Path.Combine(DEFAULT_SAVE_FOLDER, templateList[selectedTemplateIndex - 1].templateName + ".json");
    }

    
    private static string SerializeTemplateData(UIObjectTemplateData templateData)
    {
        return JsonUtility.ToJson(templateData, true);
    }

   private static UIObjectTemplateData DeserializeTemplateData(string jsonContent)
    {
        return JsonUtility.FromJson<UIObjectTemplateData>(jsonContent);
    }
}
