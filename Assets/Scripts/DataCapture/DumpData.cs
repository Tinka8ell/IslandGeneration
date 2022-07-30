using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DumpData
{
    public Vector2 coord;
    public int width;
    public Vector2 sampleCentre;
    public bool useFalloff;
    public string noiseValues;
    public string falloffMap;
    public string values;

    public void CaptureNoise(float[,] noise)
    {
        noiseValues = FloatsToJson(noise);
    }

    public void CaptureFalloffMap(float[,] falloffMap)
    {
        this.falloffMap = FloatsToJson(falloffMap);
    }

    public void CaptureValues(float[,] values)
    {
        this.values = FloatsToJson(values);
    }

    public string ToJson()
    {
        return JsonUtility.ToJson(this, true);
    }

    public void LoadFromJson(string json)
    {
        JsonUtility.FromJsonOverwrite(json, this);
    }

    private string FloatsToJson(float[,] values)
    {
        int rowLength = values.GetLength(0);
        string[] row = new string[rowLength];
        int rows = values.GetLength(1);
        string[] allRows = new string[rows];
        for (int j = 0; j < rows; j++) 
        {
            Copy(values, j, row, row.Length);
            allRows[j] = Join(row);
        }
        return Join(allRows);
    }

    private void Copy(float[,] values, int j, string[] row, int length)
    {
        for (int i = 0; i < length; i++)
        {
            row[i] = values[j, i].ToString();
        }
    }

    private string Join(string[] row)
    {
        string result = "";
        string prefix = "[";
        for (int i = 0; i < row.Length; i++)
        {
            result += prefix + row[i];
            prefix = ", ";
        }
        return result + "]";
    }

    public void ToFile()
    {
        string fileName = $"DumpData={coord.x}x{coord.y}.json";
        FileManager.WriteToFile(fileName, ToJson());
    }
}
