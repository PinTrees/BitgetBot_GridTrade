using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;
public class FileState
{
    public string url;
    public string path;
    public FileState(string url, string path)
    {
        this.url = url; this.path = path;
    }
    public string GetPath() { return path.Split('?')[0]; }
    public string GetAllPath() { return path.Replace('?', '/'); }
    public string GetName() { return path.Split('?')[1]; }
}
public class Download : MonoBehaviour
{
    public static Download instance;
    void Awake()
    {
        instance = this;
    }
    private string pathForDocumentsFile(string filename)
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
    // ========================================================================================================
    public void FileCheck(List<FileState> data, Image bar=null, Text value=null)
    {
        List<int> removeIndex = new List<int>();
        for (int i = 0; i < data.Count; i++)
        {
            if (!Directory.Exists(pathForDocumentsFile(data[i].GetPath())))
                Directory.CreateDirectory(pathForDocumentsFile(data[i].GetPath()));

            FileInfo fileInfo = new FileInfo(pathForDocumentsFile(data[i].GetAllPath()));
            if (!fileInfo.Exists)
            {
                continue;
            }
            else removeIndex.Add(i);
        }
        for(int i = 0; i < removeIndex.Count; i++)
        {
            data.RemoveAt(removeIndex[i] - i);
        }
    }
    public IEnumerator DownLoad(FileState download, Image bar, Text value = null)
    {
        //Debug.Log(url);
        UnityWebRequest request = UnityWebRequest.Get(download.url);
        //WWW www = new WWW(url);
        bar.fillAmount = 0;
        if (value != null) value.text = "0%";
        request.SendWebRequest();
        while (!request.isDone)
        {
            if (value != null) value.text = string.Format("{0}{1}", Mathf.RoundToInt(request.downloadProgress * 100), "%");
            bar.fillAmount = request.downloadProgress / 1.0f;
            yield return new WaitForEndOfFrame();
        }
        bar.fillAmount = 1; if (value != null) value.text = "100%";

        if (request.isNetworkError)
            yield break;
        else if (request.isHttpError)
            yield break;

        if (!Directory.Exists(pathForDocumentsFile(download.GetPath())))
            Directory.CreateDirectory(pathForDocumentsFile(download.GetPath()));

        System.IO.File.WriteAllBytes(pathForDocumentsFile(download.GetAllPath()), request.downloadHandler.data);
        yield return null;
    }
    // ============================================================================================================================
    public IEnumerator getTextureFormWWW(string url, RawImage target, Image bar, Text value)
    {
        UnityWebRequest request = UnityWebRequest.Get(url);
        DownloadHandlerTexture texDl = new DownloadHandlerTexture(true);
        request.downloadHandler = texDl;

        bar.fillAmount = 0; value.text = "0%";
        request.SendWebRequest();
        while (!request.isDone)
        {
            value.text = string.Format("{0}{1}", Mathf.RoundToInt(request.downloadProgress * 100), "%");
            bar.fillAmount = request.downloadProgress / 1.0f;
            yield return new WaitForEndOfFrame();
        }
        bar.fillAmount = 1; value.text = "100%";

        if (!(request.isNetworkError || request.isHttpError))
        {
            Destroy(target.texture);
            target.texture = texDl.texture;
        }
    }
    public IEnumerator getAssetBundleFromGitHub(string url, string assetbundleName, Image bar, TextMeshProUGUI value)
    {
        UnityWebRequest request = UnityWebRequest.Get(url);
        DownloadHandlerTexture texDl = new DownloadHandlerTexture(true);
        request.downloadHandler = texDl;

        bar.fillAmount = 0; value.text = "0%";
        request.SendWebRequest();
        while (!request.isDone)
        {
            value.text = string.Format("{0}{1}", Mathf.RoundToInt(request.downloadProgress * 100), "%");
            bar.fillAmount = request.downloadProgress / 1.0f;
            yield return new WaitForEndOfFrame();
        }
        bar.fillAmount = 1; value.text = "100%";

        if (!(request.isNetworkError || request.isHttpError))
        {
            string folderpath = pathForDocumentsFile("Resource/Spine");
            if (!Directory.Exists(folderpath))
                Directory.CreateDirectory(folderpath);
            string filePath = folderpath + "/" + assetbundleName;
            System.IO.File.WriteAllBytes(filePath, request.downloadHandler.data);
        }
    }
}
