using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public static class FileManager
{
    public static bool WriteToFile(string fileName, string contents)
    {
        string fullPath = Path.Combine("c:/temp", fileName);
        bool returnValue = false;
        try
        {
            File.WriteAllText(fullPath, contents);
            Debug.Log(fullPath);
            returnValue = true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to write to {fullPath} with exception {e}");
        }
        return returnValue;
    }

    public static bool LoadFromFile(string fileName, out string result)
    {
        string fullPath = Path.Combine(Application.persistentDataPath, fileName);
        bool returnValue = false;
        try
        {
            result = File.ReadAllText(fullPath);
            returnValue = true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to read from {fullPath} with exception {e}");
            result = "";
        }
        return returnValue;
    }
}
