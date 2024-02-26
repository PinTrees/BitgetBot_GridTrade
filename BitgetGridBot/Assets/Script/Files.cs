using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Networking;
using System.Text.RegularExpressions;

public static class Files
{
    public static string DocumentsPath(string filename)
    {
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            string path = Application.dataPath.Substring(0, Application.dataPath.Length - 5);
            path = path.Substring(0, path.LastIndexOf('/'));
            return Path.Combine(Path.Combine(path, "Documents"), filename);
        }

        else if (Application.platform == RuntimePlatform.Android)
        {
            string path = Application.persistentDataPath;
            path = path.Substring(0, path.LastIndexOf('/'));
            return Path.Combine(path, filename);
        }

        else
        {
            string path = Application.dataPath;
            path = path.Substring(0, path.LastIndexOf('/'));
            return Path.Combine(path, filename);
        }
    }
    public static Texture2D GetTexture2D_PNG(string path)
    {
        if (!File.Exists(path))
            return null;

        Debug.Log(path);
        FileInfo fileInfo = new FileInfo(path);

        if (fileInfo.Exists)
        {
            byte[] byteTexture = System.IO.File.ReadAllBytes(path);
            Texture2D texture = new Texture2D(0, 0);
            texture.LoadImage(byteTexture);
            return texture;
        }
        return null;
    }
    public static void APPEND_TXT(string filePath, string message)
    {
        DirectoryInfo directoryInfo = new DirectoryInfo(Path.GetDirectoryName(filePath));

        if (!directoryInfo.Exists)
        {
            directoryInfo.Create();
        }

        FileStream fileStream = new FileStream(filePath, FileMode.Append, FileAccess.Write);

        StreamWriter writer = new StreamWriter(fileStream, System.Text.Encoding.Unicode);

        writer.WriteLine(message);
        writer.Close();
    }
    public static void WRITE_TXT(string filePath, string message)
    {
        DirectoryInfo directoryInfo = new DirectoryInfo(Path.GetDirectoryName(filePath));

        if (!directoryInfo.Exists)
        {
            directoryInfo.Create();
        }

        FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);

        StreamWriter writer = new StreamWriter(fileStream, System.Text.Encoding.Unicode);

        writer.Write(message);
        writer.Close();
    }
    public static void APPEND_TXT(string filePath, string message, int limit)
    {
        DirectoryInfo directoryInfo = new DirectoryInfo(Path.GetDirectoryName(filePath));

        if (!directoryInfo.Exists)
        {
            directoryInfo.Create();
        }

        string aa = READ_TXT(filePath);
        if (aa.Length > limit) WRITE_TXT(filePath, "");

        FileStream fileStream = new FileStream(filePath, FileMode.Append, FileAccess.Write);
        StreamWriter writer = new StreamWriter(fileStream, System.Text.Encoding.Unicode);

        writer.WriteLine(message);
        writer.Close();
    }
    public static string READ_TXT(string filePath)
    {
        FileInfo fileInfo = new FileInfo(filePath);
        string value = "";
        if (fileInfo.Exists)
        {
            StreamReader reader = new StreamReader(filePath);
            string line = reader.ReadLine();
            while (line != null)
            {
                value += line;
                line = reader.ReadLine();
            }
            reader.Close();
        }
        else
            value = "";
        return value;
    }
}
public static class CSVReader
{
    static string SPLIT_RE = @",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))";
    static string LINE_SPLIT_RE = @"\r\n|\n\r|\n|\r";
    static char[] TRIM_CHARS = { '\"' };
    public static List<Dictionary<string, string>> Read(string filePath)
    {
        var list = new List<Dictionary<string, string>>();
        string source;

        FileInfo fileInfo = new FileInfo(filePath);
        if (!fileInfo.Exists) return null;
      
        StreamReader sr = new StreamReader(filePath);
        source = sr.ReadToEnd();
        sr.Close();

        var lines = Regex.Split(source, LINE_SPLIT_RE);

        if (lines.Length <= 1) return list;

        var header = Regex.Split(lines[0], SPLIT_RE);
        for (var i = 1; i < lines.Length; i++)
        {

            var values = Regex.Split(lines[i], SPLIT_RE);
            if (values.Length == 0 || values[0] == "") continue;

            var entry = new Dictionary<string, string>();
            for (var j = 0; j < header.Length && j < values.Length; j++)
            {
                string value = values[j];
                value = value.TrimStart(TRIM_CHARS).TrimEnd(TRIM_CHARS).Replace("\\", "");
                string finalvalue = value;
                entry[header[j]] = finalvalue;
            }
            list.Add(entry);
        }
        return list;
    }
}