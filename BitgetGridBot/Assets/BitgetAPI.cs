using BitgetClassJson;
using FullSerializer;
using JsonData;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

using Bitget.Price;
using Bitget.Order;

public class BitgetAPI
{
    public static string api_scr_password = ""; 
    public static string api_scr_acceskey = ""; 
    public static string api_scr_key = "";     

    public static long timestamp = 0;
    public static DateTime dateTime = DateTime.Now;
    public static float curPrice_BTCUSDT = 0f;
    public static string bitget = "https://capi.bitget.com";

    // -------------------------
    public static MarketPosition holdPosition = new MarketPosition();
    public static Account startAccount = new Account();
    public static Account currentAccount = new Account();
    public static PriceLimit marketLimit = new PriceLimit();
    public static List<OrderDetail> order_historys = new List<OrderDetail>();
    public static List<OrderDetail> liveOrderDetails = new List<OrderDetail>(); // 미체결 주문
    public static List<OrderDetail> filledOrderDetails_OpenLong = new List<OrderDetail>(); // 체결된 시작 주문    - same count
    public static List<OrderDetail> liveOrders_CloseLong = new List<OrderDetail>(); // 미체결된 종료 주문   -
    public static List<OrderDetail> exiteOrders_CloseLong = new List<OrderDetail>(); // 청산 주문

    public static List<string> addLiveOrderIds = new List<string>();
    public static List<string> addCloseOrderIds = new List<string>();

    public static fsSerializer serializer = new fsSerializer();
    public static IEnumerator Get_SERVER_TIME(Error _error = null)
    {
        UnityWebRequest www = UnityWebRequest.Get(string.Format("https://capi.bitget.com/api/swap/v3/market/time"));
        //www.method = "PATCH";
        www.method = "GET";
    
        yield return www.SendWebRequest();
     
        Error error = SET_ERROR(www);
        if (_error != null) _error = _error.Instance(error);
        if (error.error) yield break;

        if (www.isNetworkError && www.isHttpError)
        {
        }
        else
        {
            var data = fsJsonParser.Parse(www.downloadHandler.text);
            object deserialized = null;
            serializer.TryDeserialize(data, typeof(BitgetTime), ref deserialized);
            var tmp = deserialized as BitgetTime;
            if (tmp != null)
            {
                timestamp = tmp.timestamp;
            }
        }
        yield return null;
    }
    public static IEnumerator GET_ACCOUNT_DETAIL(string index, Error _error = null, Account set = null)
    {
        UnityWebRequest www = UnityWebRequest.Get(string.Format("{0}/api/swap/v3/account/account?symbol={1}", bitget, index));
        string signature = string.Format("{0}GET/api/swap/v3/account/account?symbol={1}", timestamp.ToString(), index);

        SET_WEB_HEADER(www, signature);
        www.method = "GET";

        yield return www.SendWebRequest();

        Error error = SET_ERROR(www);
        if (_error != null) _error = _error.Instance(error);
        if (error.code != 200) yield break;

        if (www.isNetworkError && www.isHttpError) { }
        else
        {
            var data = fsJsonParser.Parse(www.downloadHandler.text);
            object deserialized = null;
            serializer.TryDeserialize(data, typeof(Account), ref deserialized);
            var tmp = deserialized as Account;
            if (tmp != null)
            {
                if (set != null) set = set.Instance(tmp);
                else currentAccount = currentAccount.Instance(tmp);
            }
        }
    }
    public static IEnumerator GET_POSITION_DETAIL(string index, Error _error)
    {
        UnityWebRequest www = UnityWebRequest.Get(string.Format("{0}/api/swap/v3/position/singlePosition?symbol={1}", bitget, index));
        string signature = string.Format("{0}GET/api/swap/v3/position/singlePosition?symbol={1}", timestamp.ToString(), index);

        SET_WEB_HEADER(www, signature);
        www.method = "GET";
        yield return www.SendWebRequest();

        Error error = SET_ERROR(www);
        if (_error != null) _error = _error.Instance(error);
        if (error.code != 200) yield break;

        if (www.isNetworkError && www.isHttpError)
        {
        }
        else
        {
            var data = fsJsonParser.Parse(www.downloadHandler.text);
            object deserialized = null;
            serializer.TryDeserialize(data, typeof(MarketPosition), ref deserialized);
            var tmp = deserialized as MarketPosition;
            if (tmp != null)
            {
                holdPosition = holdPosition.Instance(tmp);
            }
        }
    }
    public static IEnumerator GET_ORDER_DETAIL(OrderId orderid, OrderDetail intsance)
    {
        UnityWebRequest www = UnityWebRequest.Get(string.Format("{0}/api/swap/v3/order/detail?{1}", bitget, orderid.GetQuery()));
        string signature = string.Format("{0}GET/api/swap/v3/order/detail?{1}", timestamp.ToString(), orderid.GetQuery());

        SET_WEB_HEADER(www, signature);
        www.method = "GET";
        yield return www.SendWebRequest();

        if (www.responseCode != 200) { yield break; }
        if (www.isNetworkError && www.isHttpError) { yield break; }
        else
        {
            var data = fsJsonParser.Parse(www.downloadHandler.text);
            object deserialized = null;

            serializer.TryDeserialize(data, typeof(OrderDetail), ref deserialized);
            var tmp = deserialized as OrderDetail;
            if (tmp != null)
            {
                intsance = intsance.Instance(tmp);
            }
        }
    }
    public static bool GET_OPENLONG_OVERAP(float price)
    {
        bool result = true;
        for (int i = 0; i < liveOrderDetails.Count; i++)
        {
            int gap = (int)Mathf.Abs(float.Parse(liveOrderDetails[i].price) - price);
            if (gap < SettingAPI.GET_PER_GAP(price, SettingAPI.overlapPer))
            {
                result = false;
                break;
            }
        }
        for (int i = 0; i < filledOrderDetails_OpenLong.Count; i++)
        {
            int gap = (int)Mathf.Abs(float.Parse(filledOrderDetails_OpenLong[i].price) - price);
            if (gap < SettingAPI.GET_PER_GAP(price, SettingAPI.overlapPer))
            {
                result = false;
                break;
            }
        }
        return result;
    }
    public static bool GET_CLOSELONG_OVERAP(float price)
    {
        bool result = true;
        for (int i = 0; i < liveOrders_CloseLong.Count; i++)
        {
            int gap = (int)Mathf.Abs(float.Parse(liveOrders_CloseLong[i].price) - price);
            if (gap < SettingAPI.GET_PER_GAP(price, SettingAPI.overlapPer))
            {
                result = false;
                break;
            }
        }
        return result;
    }

    public static IEnumerator Get_MARKET_PRICE(string index, Error _error=null)
    {
        UnityWebRequest www = UnityWebRequest.Get(string.Format("{0}/api/swap/v3/market/mark_price?symbol={1}", bitget, index));
        www.method = "GET";

        yield return www.SendWebRequest();

        Error error = SET_ERROR(www);
        if (_error != null) _error = _error.Instance(error);
        if (error.code != 200) yield break;

        if (www.isNetworkError && www.isHttpError) { }
        else
        {
            var data = fsJsonParser.Parse(www.downloadHandler.text);

            object deserialized = null;
            serializer.TryDeserialize(data, typeof(MarketPrice), ref deserialized);
            var tmp = deserialized as MarketPrice;
            if (tmp != null)
            {
                curPrice_BTCUSDT = float.Parse(tmp.mark_price);
            }
        }
    }
    public static IEnumerator Get_MARKET_LIMIT(string index, Error _error= null)
    {
        UnityWebRequest www = UnityWebRequest.Get(string.Format("{0}/api/swap/v3/market/price_limit?symbol={1}", bitget, index));
        www.method = "GET";

        yield return www.SendWebRequest();

        Error error = SET_ERROR(www);
        if (_error != null) _error = _error.Instance(error);
        if (error.code != 200) yield break;

        if (www.isNetworkError && www.isHttpError) { }
        else
        {
            var data = fsJsonParser.Parse(www.downloadHandler.text);
            object deserialized = null;
            serializer.TryDeserialize(data, typeof(PriceLimit), ref deserialized);
            var tmp = deserialized as PriceLimit;

            if (tmp != null)
                marketLimit = marketLimit.Instance(tmp);
        }
    }

    public static IEnumerator GetCurrentTrade(string index)
    {
        //UnityWebRequest www = UnityWebRequest.Get(string.Format("{0}/api/swap/v3/order/fills?symbol={1}&orderId={2}", bitget, index, ""));
        //string signature = string.Format("{0}GET/api/swap/v3/order/fills?symbol={1}&orderId={2}", timestamp, index, "");

        UnityWebRequest www = UnityWebRequest.Get(string.Format("{0}/api/swap/v3/trace/currentTrack?symbol={1}&pageSize=1", bitget, index));
        string signature = string.Format("{0}GET/api/swap/v3/trace/currentTrack?symbol={1}&pageSize=1", timestamp, index);
        Debug.Log(signature);

        string sssss = SecurityUtil.Base64Encode(api_scr_key);
        Debug.Log(SecurityUtil.Base64Decode(sssss));
        string aaaa = SecurityUtil.Base64Encode(signature);
        Debug.Log(SecurityUtil.Base64Decode(aaaa));

        HMACSHA256 aa = new HMACSHA256(Convert.FromBase64String(sssss));
        Debug.Log(Convert.ToBase64String(aa.Key));
        byte[] signatureBT = aa.ComputeHash(Convert.FromBase64String(aaaa));
        Debug.Log(Convert.ToBase64String(aa.Hash));
        signature = Convert.ToBase64String(signatureBT);
        Debug.Log(signature);
        //signature = SecurityUtil.Base64Encode(signature);
        Debug.Log(signature);

        www.SetRequestHeader("ACCESS-KEY", api_scr_acceskey);
        www.SetRequestHeader("ACCESS-SIGN", signature);
        www.SetRequestHeader("ACCESS-TIMESTAMP", timestamp.ToString());
        www.SetRequestHeader("ACCESS-PASSPHRASE", api_scr_password);
        www.method = "GET";
        yield return www.SendWebRequest();
        if (www.isNetworkError && www.isHttpError)
        {
        }
        else
        {
            var data = fsJsonParser.Parse(www.downloadHandler.text);
            Debug.Log(data);
            object deserialized = null;
            serializer.TryDeserialize(data, typeof(MarketPrice), ref deserialized);
            var tmp = deserialized as MarketPrice;
            if (tmp != null)
            {
                //Debug.Log(data);
                //Debug.Log(tmp.mark_price);
            }
        }
    }
    public static IEnumerator GET_ORDERS_HISTORY(string index)
    {
        UnityWebRequest www = UnityWebRequest.Get(string.Format("{0}/api/swap/v3/order/history?symbol={1}&pageIndex=1&pageSize=5&createDate=90", bitget, index));
        string signature = string.Format("{0}GET/api/swap/v3/order/history?symbol={1}&pageIndex=1&pageSize=5&createDate=90", timestamp, index);
        Debug.Log(signature);

        SET_WEB_HEADER(www, signature);
        www.method = "GET";

        yield return www.SendWebRequest();

        if (www.responseCode != 200) { yield break; }
        if (www.isNetworkError && www.isHttpError) { yield break; }
        else
        {
            var data = fsJsonParser.Parse(www.downloadHandler.text);
            object deserialized = null;
            serializer.TryDeserialize(data, typeof(List<OrderDetail>), ref deserialized);
            var tmp = deserialized as List<OrderDetail>;
            if (tmp != null)
            {
                order_historys.Clear();
                for (int i = 0; i < tmp.Count; i++)
                    order_historys.Add(tmp[i]);
            }
        }
    }
    public static IEnumerator POST_ORDER(Order order, OrderId_Post resurlt, Error _error=null)
    {
        string json = JsonUtility.ToJson(order);
        var www = new UnityWebRequest("https://capi.bitget.com/api/swap/v3/order/placeOrder", "POST");
        string signature = string.Format("{0}POST/api/swap/v3/order/placeOrder{1}", timestamp.ToString(), json);

        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);
        www.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
        www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();

        SET_WEB_HEADER(www, signature);
        www.method = UnityWebRequest.kHttpVerbPOST;
        www.chunkedTransfer = false;

        yield return www.SendWebRequest();

        string file = string.Format("{0}_{1}_{2}.txt", dateTime.Year, dateTime.Month, dateTime.Day);
        Files.APPEND_TXT(Files.DocumentsPath("debug/orders.post/" + file), www.downloadHandler.text, 50000);

        Error error = SET_ERROR(www);
        if (_error != null) _error = _error.Instance(error);
        if (error.code != 200) yield break;

        if (www.isNetworkError && www.isHttpError) { yield break; }
        else
        {
            var data = fsJsonParser.Parse(www.downloadHandler.text);
            object deserialized = null;

            serializer.TryDeserialize(data, typeof(OrderId_Post), ref deserialized);
            var tmp = deserialized as OrderId_Post;
            if (tmp != null)
            {
                if (resurlt == null) yield break;
                resurlt = resurlt.Instace(tmp);
            }
        }
    }
    public static IEnumerator POST_BATCH_ORDERS(BatchOrder order, int type=0)
    {
        string json = JsonUtility.ToJson(order);
        //string.Format("{0}\"symbol\":\"cmt_btcusdt\",\"size\":\"1\",\"type\":\"2\",\"match_price\":\"0\",\"price\":\"{1}\",\"order_type\":\"0\",\"client_oid\":\"56840602\"{2}", "{", 100000 /*GET_TRANSACTION_PRICE(1)*/, "}");
        //Debug.Log(json);

        var www = new UnityWebRequest("https://capi.bitget.com/api/swap/v3/order/batchOrders", "POST");

        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);
        www.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
        www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();

        string signature = string.Format("{0}POST/api/swap/v3/order/batchOrders{1}", timestamp.ToString(), json);

        SET_WEB_HEADER(www, signature);
        www.method = UnityWebRequest.kHttpVerbPOST;
        www.chunkedTransfer = false;
        yield return www.SendWebRequest();
        if (www.isNetworkError && www.isHttpError)
        {
        }
        else
        {
            var sn_data = fsJsonParser.Parse(www.downloadHandler.text);
            object deserialized = null;
            serializer.TryDeserialize(sn_data, typeof(OrderResultBatch), ref deserialized);
            var tmp = deserialized as OrderResultBatch;
            if (tmp != null)
            {
                Debug.Log(sn_data);
                if (type == 3)
                {
                    for (int i = 0; i < tmp.order_info.Count; i++)
                        addCloseOrderIds.Add(tmp.order_info[i].order_id);
                }
                else
                {
                    for (int i = 0; i < tmp.order_info.Count; i++)
                        addLiveOrderIds.Add(tmp.order_info[i].order_id);
                }
            }
        }
        yield return null;
    }
    public static IEnumerator POST_CLOSE_ORDERS(OrderIds orderIds)
    {
        string json = JsonUtility.ToJson(orderIds);
        Debug.Log(json);

        var www = new UnityWebRequest("https://capi.bitget.com/api/swap/v3/order/cancel_batch_orders", "POST");

        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);
        www.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
        www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();

        string signature = string.Format("{0}POST/api/swap/v3/order/cancel_batch_orders{1}", timestamp.ToString(), json);

        SET_WEB_HEADER(www, signature);
        www.method = UnityWebRequest.kHttpVerbPOST;
        //www.chunkedTransfer = false;
        yield return www.SendWebRequest();
        if (www.isNetworkError && www.isHttpError)
        {
        }
        else
        {
            var sn_data = fsJsonParser.Parse(www.downloadHandler.text);
            object deserialized = null;
            serializer.TryDeserialize(sn_data, typeof(CloseOrderResult), ref deserialized);
            var tmp = deserialized as CloseOrderResult;
            if (tmp != null)
            {
                Debug.Log(tmp.order_ids.Length);
            }
        }
    }
    public static string GET_MARKET_BTCUSDT(int index = 0)
    {
        if (index == 0) return curPrice_BTCUSDT.ToString();

        if (marketLimit == null) return "";
        else if (index == 1) return marketLimit.highest;
        else if (index == 2) return marketLimit.lowest;
        return "";
        //if (market == null) return "";
        //return market.last;
    }
    public static float SET_ORDER_GAP(float price=0)
    {
        float btc = curPrice_BTCUSDT;
        if (price > 0) btc = price;

        float sper = SettingAPI.closeOrderPer / (float)100;
        float order_gap = btc * sper;
        return order_gap;
    }
    public static float GET_NEXTORDER_PRICE(float price=0)
    {
        float btc = curPrice_BTCUSDT;
        if (price > 0) btc = price;
        return btc - SET_ORDER_GAP(btc);
    }
    public static OrderDetail GET_OPENLONG_ORDER(string clientId)
    {
        OrderDetail ret = filledOrderDetails_OpenLong.Find(delegate (OrderDetail a)
        {
            return a.client_oid == clientId;
        });
        return ret;
    }
    public static OrderDetail GET_CLOSELONG_ORDER(string clientId)
    {
        OrderDetail ret = liveOrders_CloseLong.Find(delegate (OrderDetail a)
        {
            return a.client_oid.Split('#')[0] == clientId;
        });
        return ret;
    }
    public static Error SET_ERROR(UnityWebRequest www)
    {
        Error error = new Error();
        error.code = www.responseCode;
        error.messege = www.downloadHandler.text;

        if (www.responseCode != 200) { error = error.SET_ERROR(true); }
        return error;
    }
    public static UnityWebRequest SET_WEB_HEADER(UnityWebRequest www, string signature)
    {
        string base64_api_key = SecurityUtil.Base64Encode(api_scr_key);
        string base64_signature = SecurityUtil.Base64Encode(signature);

        HMACSHA256 sha256 = new HMACSHA256(Convert.FromBase64String(base64_api_key));
        byte[] signature_byte = sha256.ComputeHash(Convert.FromBase64String(base64_signature));
        base64_signature = Convert.ToBase64String(signature_byte);

        www.SetRequestHeader("Content-Type", "application/json; charset=UTF-8");
        www.SetRequestHeader("ACCESS-KEY", api_scr_acceskey);
        www.SetRequestHeader("ACCESS-SIGN", base64_signature);
        www.SetRequestHeader("ACCESS-TIMESTAMP", timestamp.ToString());
        www.SetRequestHeader("ACCESS-PASSPHRASE", api_scr_password);
        www.SetRequestHeader("locale", "en-US");
        return www;
    }
}
