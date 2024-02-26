//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.UI;
//using System.Linq;
//using System.IO;
//using UnityEngine.Networking;
//using FullSerializer;
//using Proyecto26;
//public class UserCache
//{
//    string uid;
//    string pin;
//    string key;
//    public string status;
//    public UserCache() { }
//    public UserCache(string _uid, string _pin, string _key, string _status)
//    {
//        uid = _uid;
//        pin = _pin;
//        key = _key;
//        status = _status;
//    }
//    public void Set(string _uid, string _pin, string _key, string _status)
//    {
//        uid = _uid;
//        pin = _pin;
//        key = _key;
//        status = _status;
//    }
//    public string GetUID() { return uid; }
//    public string GetPin() { return pin; }
//    public string GetKey() { return key; }
//}
//public class FirebaseDataBase : MonoBehaviour
//{
//    public static FirebaseDataBase instance;

//    public string operatorName;

//    string idToken;
//    string localId;
//    string error;

//    UserCache curUserCache;
//    fsSerializer serializer = new fsSerializer();

//    string DataBaseURL = "https://arknights-db.firebaseio.com/users";

//    string cacheXml = string.Empty;
//    private void Awake()
//    {
//        instance = this;

//        localId = string.Empty;
//        error = string.Empty;

//        curUserCache = new UserCache();

//        DontDestroyOnLoad(this.gameObject);
//        string key = string.Empty;
//    }
//    IEnumerator SignInUser(string _uid, string _pin)
//    {
//        bool result = false; localId = string.Empty;
//        error = string.Empty;

//        if(cacheXml.Length < 256)
//        {
//            UnityWebRequest cacheRequest = UnityWebRequest.Get("https://raw.githubusercontent.com/PinTrees/Arknight_DB/master/cache/cache.xml");
//            yield return cacheRequest.SendWebRequest();
//            cacheXml = cacheRequest.downloadHandler.text;
//        }

//        string userData = "{\"email\":\"" + _uid + "@xxx.xxx" + "\",\"password\":\"" + _pin + "\",\"returnSecureToken\":true}";
//        RestClient.Post<SignResponse>("https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key=" +
//            Program.Decrypt(XML.This.GetCache_A("a", cacheXml), Program.Decrypt(XML.This.GetCache_A("l", cacheXml), XML.This.GetCache_A("k", cacheXml))), userData).Then(response =>
//        {
//            idToken = response.idToken;
//            localId = response.localId;

//            Debug.Log("[auth] email login success");
//            result = true;
//        })
//        .Catch(error =>
//        {
//            Debug.Log(error);
//            result = true;
//        });
//        yield return new WaitUntil(() => result);
//        result = false;

//        currUser = null;
//        if (localId.Equals(string.Empty))
//            yield break;

//        RestClient.Get<User>(DataBaseURL + "/" + localId + ".json").Then(response =>
//        {
//            currUser = response;
//            result = true;

//            if (response == null)
//                Debug.Log("none user data");
//        });
//        yield return new WaitUntil(() => result);
//        yield return null;
//    }
//    IEnumerator getIdToken(string _uid, string _pin)
//    {
//        bool result = false; localId = string.Empty;
//        error = string.Empty;

//        if (cacheXml.Length < 256)
//        {
//            UnityWebRequest cacheRequest = UnityWebRequest.Get("https://raw.githubusercontent.com/PinTrees/Arknight_DB/master/cache/cache.xml");
//            yield return cacheRequest.SendWebRequest();
//            cacheXml = cacheRequest.downloadHandler.text;
//        }

//        string userData = "{\"email\":\"" + _uid + "@xxx.xxx" + "\",\"password\":\"" + _pin + "\",\"returnSecureToken\":true}";
//        RestClient.Post<SignResponse>("https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key=" +
//            Program.Decrypt(XML.This.GetCache_A("a", cacheXml), Program.Decrypt(XML.This.GetCache_A("l", cacheXml), XML.This.GetCache_A("k", cacheXml))), userData).Then(response =>
//        {
//            idToken = response.idToken;
//            localId = response.localId;
//            result = true;
//        })
//        .Catch(error =>
//        {
//            Debug.Log(error);
//            result = true;
//        });

//        yield return new WaitUntil(() => result);
//        result = false;
//    }
//    // SinIn Start ============================================================
//    public IEnumerator SignInWithInput(string _uid, string _pin)
//    {
//        yield return SignInUser(_uid, _pin);
//        yield return null;
//    }
//    public IEnumerator SignInWithSaveFile()
//    {
//        string key = string.Empty;
//        error = string.Empty;

//        if (cacheXml.Length < 256)
//        {
//            UnityWebRequest cacheRequest = UnityWebRequest.Get("https://raw.githubusercontent.com/PinTrees/Arknight_DB/master/cache/cache.xml");
//            yield return cacheRequest.SendWebRequest();
//            cacheXml = cacheRequest.downloadHandler.text;
//        }

//        key = XML.This.GetCache_B("j", cacheXml);

//        yield return StartCoroutine(SignInUser(Program.Decrypt(curUserCache.GetUID(), Program.Decrypt(curUserCache.GetKey(), key)),
//            Program.Decrypt(curUserCache.GetPin(), Program.Decrypt(curUserCache.GetKey(), key))));
//    }
//    // Push ======================================================================
//    // StageDropData ======================================================================
//    public IEnumerator PushDropData(StageDropData _set)
//    {
//        bool result = false;

//        _set.date = System.DateTime.Now.ToString("yyyyMMddhhmmss");

//        string json = JsonUtility.ToJson(_set);

//        yield return StartCoroutine(SignInWithSaveFile());
//        yield return RestClient.Post("https://arknights-db.firebaseio.com/stage-drop/public/" + localId + ".json?auth=" + idToken, json).Then(response =>
//        {
//            result = true;
//        });
//        yield return new WaitUntil(() => result);
//        yield return null;
//    }
//    public IEnumerator GetUserDropData(List<StageDropData> dropData)
//    {
//        if (localId == string.Empty)
//            yield return StartCoroutine(SignInWithSaveFile());

//        UnityWebRequest www = UnityWebRequest.Get(string.Format("https://arknights-db.firebaseio.com/stage-drop/public/{0}.json?orderBy=\"date\"&limitToLast=10&print=pretty", localId));
//        yield return www.SendWebRequest();
//        if (www.isNetworkError && www.isHttpError)
//        {
//        }
//        else
//        {
//            var data = fsJsonParser.Parse(www.downloadHandler.text);
//            object deserialized = null;
//            serializer.TryDeserialize(data, typeof(Dictionary<string, StageDropData>), ref deserialized);
//            var tmp = deserialized as Dictionary<string, StageDropData>;
//            if (tmp != null)
//            {
//                var res = tmp.Values.ToList();
//                res.Reverse();
//                for (int i = 0; i < res.Count; i++)
//                    dropData.Add(res[i]);
//            }
//            else
//                dropData.Clear();
//        }
//    }
//    public IEnumerator getStagePublicDropDataOnlyKey(PublicDropData value)
//    {
//        UnityWebRequest www = UnityWebRequest.Get(string.Format("https://arknights-db.firebaseio.com/stage-drop/public.json"));
//        yield return www.SendWebRequest();
//        if (www.isNetworkError && www.isHttpError)
//        {
//        }
//        else
//        {
//            var data = fsJsonParser.Parse(www.downloadHandler.text);
//            object deserialized = null;
//            serializer.TryDeserialize(data, typeof(Dictionary<string, Dictionary<string, StageDropData>>), ref deserialized);
//            Dictionary<string, Dictionary<string, StageDropData>> valuePairs = deserialized as Dictionary<string, Dictionary<string, StageDropData>>;
//            Debug.Log(data);

//            value.dropData = new List<StageDropData[]>();
//            value.pushId = new List<string[]>();

//            value.key = valuePairs.Keys.ToList();

//            for (int i = 0; i < value.key.Count; i++)
//            {
//                string[] pushId = valuePairs[value.key[i]].Keys.ToArray();
//                StageDropData[] stageDrops = valuePairs[value.key[i]].Values.ToArray();
//                value.dropData.Add(stageDrops);
//                value.pushId.Add(pushId);
//            }
//        }
//    }
//    public IEnumerator getStagePublicDropDataOnlyPoint(PointS set, string _localId)
//    {
//        UnityWebRequest www = UnityWebRequest.Get(string.Format("https://arknights-db.firebaseio.com/stage-drop/point/{0}.json", _localId));
//        yield return www.SendWebRequest();
//        if (www.isNetworkError && www.isHttpError) {       }
//        else
//        {
//            var data = fsJsonParser.Parse(www.downloadHandler.text);
//            object deserialized = null;
//            serializer.TryDeserialize(data, typeof(PointS), ref deserialized);
//            var curData = deserialized as PointS;
//            if (curData != null)
//                set.point = curData.point;
//            else
//                set.point = 0;
//        }
//    }

//    public IEnumerator setStagePublicDropDataByDelete(string localId, string pushId)
//    {
//        UnityWebRequest www = UnityWebRequest.Delete(string.Format("https://arknights-db.firebaseio.com/stage-drop/public/{0}/{1}.json?auth={2}", localId, pushId, idToken));
//        yield return www.SendWebRequest();
//        if (www.isNetworkError && www.isHttpError)
//        {
//        }
//        else
//        {
//            Debug.Log("delete:" + localId + " key:" + pushId);
//        }
//    }
//    public IEnumerator setStagePublicDropDataOnlyPoint(string localId, int addPoint)
//    {
//        PointS point = new PointS();
//        yield return getStagePublicDropDataOnlyPoint(point, localId);

//        Debug.Log(point.point);
//        point.point += addPoint;
//        string json = JsonUtility.ToJson(point);

//        UnityWebRequest www = UnityWebRequest.Put(string.Format("https://arknights-db.firebaseio.com/stage-drop/point/{0}.json?auth={1}", localId, idToken), json);
//        www.method = "PATCH";
//        yield return www.SendWebRequest();

//        if (www.isNetworkError && www.isHttpError)
//        {
//        }
//        else
//        {
//            Debug.Log("write database:" + localId + " value:" + point.point);
//        }
//    }

//    public IEnumerator setStageDropData_History_Day(string filename, string curday)
//    {
//        List<string> dropstage = XML.This.getStageDropDataFromFile_OnlyInData(filename);

//        int trypoint = 0;
//        for (int i = 0; i < dropstage.Count; i++)
//        {
//            List<string[]> dropdata = XML.This.getStageDropDataFromFile(dropstage[i], filename);

//            for (int j = 0; j < dropdata.Count; j++)
//            {
//                if (j.Equals(0))
//                {
//                    trypoint += int.Parse(dropdata[j][2]);
//                    yield return StartCoroutine(setStagePublicDropDataTry(dropstage[i], int.Parse(dropdata[j][2]), curday));
//                    Debug.Log("dropdata-historyDay-update trycount");
//                }

//                if (dropdata[j][1].Equals("0"))
//                    continue;

//                yield return StartCoroutine(setStagePublicDropData(dropstage[i], dropdata[j][0], int.Parse(dropdata[j][1]), curday));
//                Debug.Log("dropdata-historyDay-update");
//            }
//        }

//        yield return StartCoroutine(FirebaseDataBase.instance.set_droptry_all_count(trypoint, curday));
//    }
//    public IEnumerator setStagePublicDropDataTry(string stageCode, int _try, string curday)
//    {
//        DropStatePoint pushData = new DropStatePoint();
//        pushData.tryp = _try;
//        string json = JsonUtility.ToJson(pushData);

//        UnityWebRequest www = UnityWebRequest.Put(string.Format("https://arknights-db-drop.firebaseio.com/history-day/{0}/stage/{1}/state.json?auth={2}", curday, stageCode, idToken), json);
//        yield return www.SendWebRequest();
//        if (www.isNetworkError && www.isHttpError) {       }
//        else
//        {
//        }
//    }
//    public IEnumerator set_droptry_all_count(int _try, string curday)
//    {
//        DropStatePoint pushData = new DropStatePoint();
//        pushData.tryp = _try;
//        string json = JsonUtility.ToJson(pushData);

//        UnityWebRequest www = UnityWebRequest.Put(string.Format("https://arknights-db-drop.firebaseio.com/history-day/{0}/try.json?auth={1}", curday, idToken), json);
//        yield return www.SendWebRequest();
//        if (www.isNetworkError && www.isHttpError) { }
//        else
//        {
//            Debug.Log("dropdata-historyDay-update trycount-all");
//        }
//    }
//    public IEnumerator setStagePublicDropData(string stageCode, string material, int count, string curday)
//    {
//        DropMaterial_historyD pushData = new DropMaterial_historyD();
//        pushData.tryp = 0;
//        pushData.getp = count;
//        string json = JsonUtility.ToJson(pushData);

//        UnityWebRequest www = UnityWebRequest.Put(string.Format("https://arknights-db-drop.firebaseio.com/history-day/{0}/stage/{1}/drop/{2}.json?auth={3}", curday, stageCode, material, idToken), json);
//        yield return www.SendWebRequest();
//        if (www.isNetworkError && www.isHttpError)   {       }
//        else
//        {
//        }
//    }


//    // Push ======================================================================
//    public UserCache GetUserCache()
//    {
//        return curUserCache;
//    }
//    public User GetCurrentUser()
//    {
//        return currUser;
//    }
//    public string GetUID()
//    {
//        return localId;
//    }
//    public string GetError()
//    {
//        return error;
//    }
//    public void TR_PutPassword(InputField set)
//    {
//        Debug.Log(set.text);
//    }
//}
 