using UnityEngine;
using System.Collections.Generic;
using System.IO;

public static class SaveSystem 
{
    
    public const string DEFAULT_SAVE_FOLDER = "Assets/UIObjectTemplates";

    public static void Init()
    {
        if (!Directory.Exists(DEFAULT_SAVE_FOLDER))
        {
            Directory.CreateDirectory(DEFAULT_SAVE_FOLDER);
        }
    }
    
    public static void SaveTemplate(UIObjectTemplateData templateData)
    {
        string jsonData = SerializeTemplateData(templateData);

        // Use the result of SetJsonFilePath() to set the correct JSON file path
        string jsonFilePath = SetJsonFilePath(templateData.templateName);

        File.WriteAllText(jsonFilePath, jsonData);

        Debug.Log("UI Template generated and saved successfully!");
    }
    
    
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
    
    public static string SetJsonFilePath( string templateName)
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

    
    public static string SerializeTemplateData(UIObjectTemplateData templateData)
    {
        return JsonUtility.ToJson(templateData, true);
    }

    public static UIObjectTemplateData DeserializeTemplateData(string jsonContent)
    {
        return JsonUtility.FromJson<UIObjectTemplateData>(jsonContent);
    }
}
