using FullSerializer;
using Markets.Binance.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Market
{
    public static float price_high = 0;
    public static float price_low = 0;
    public static Candle currentDayCandle = new Candle();
    public static Candle currentHourCandle = new Candle();
    public static Candle currentCandle = new Candle();
    public static List<Candle> candles = new List<Candle>();
    public static fsSerializer serializer = new fsSerializer();
    public static IEnumerator Get_MARKET_CANDLE_BN()
    {
        string binase = "https://api.binance.com";
        UnityWebRequest www = UnityWebRequest.Get(string.Format("{0}/api/v3/klines?symbol=BTCUSDT&interval=1m&limit=120", binase));
        //www.method = "PATCH";
        www.method = "GET";

        yield return www.SendWebRequest();

        //Error error = SET_ERROR(www);
        //if (_error != null) _error = _error.Instance(error);
        //if (error.error) yield break;
        if (www.isNetworkError && www.isHttpError)
        {
        }
        else
        {
            var data = fsJsonParser.Parse(www.downloadHandler.text);
            object deserialized = null;
           
            serializer.TryDeserialize(data, typeof(List<string[]>), ref deserialized);
            var tmp = deserialized as List<string[]>;
            if (tmp != null)
            {
                price_high = 0;
                price_low = 0;
                candles.Clear();
                currentCandle = new Candle().Initialize(tmp[0]);
                for (int i = 0; i < tmp.Count; i++)
                {
                    Candle n = new Candle().Initialize(tmp[i]);
                    if (n.highPrice > price_high || price_high == 0) price_high = n.highPrice;
                    if (n.lowPrice < price_low || price_low == 0) price_low = n.lowPrice;
                    candles.Add(n);
                }
            }
        }
        yield return null;
    }
    public static IEnumerator Get_MARKET_CANDLE_DAY_BN()
    {
        string binase = "https://api.binance.com";
        UnityWebRequest www = UnityWebRequest.Get(string.Format("{0}/api/v3/klines?symbol=BTCUSDT&interval=1h&limit=1", binase));
        //www.method = "PATCH";
        www.method = "GET";

        yield return www.SendWebRequest();

        //Error error = SET_ERROR(www);
        //if (_error != null) _error = _error.Instance(error);
        //if (error.error) yield break;
        if (www.isNetworkError && www.isHttpError)
        {
        }
        else
        {
            var data = fsJsonParser.Parse(www.downloadHandler.text);
            object deserialized = null;

            serializer.TryDeserialize(data, typeof(List<string[]>), ref deserialized);
            var tmp = deserialized as List<string[]>;
            if (tmp != null)
            {
                currentHourCandle = new Candle().Initialize(tmp[0]);
            }
        }
        yield return null;
    }
    public static IEnumerator Get_BACKTEST_CANDLE_BITMEX_MINIT(DateTime start, DateTime exite, List<Candle> targetCandle)
    {
        List<Dictionary<string, string>> BtcUsdt = new List<Dictionary<string, string>>();

        var current = start;
        var counter = 0;
        Debug.Log(current.ToString());
        while (current < exite)
        {
            List<Dictionary<string, string>> sn = CSVReader.Read(Files.DocumentsPath(string.Format("BacktestData/XBT_USD/{0}{1:D2}{2:D2}.csv", current.Year, current.Month, current.Day)));
            if (sn != null)
            {
                BtcUsdt.AddRange(sn);
                //timebar_1m,symbol,open_price,high_price,low_price,close_price,volume\
                for(int i = 0; i < sn.Count; i++)
                {
                    var curCandle = new Candle();
                    curCandle.openPrice = (int)float.Parse(sn[i]["open_price"]);
                    curCandle.closePrice = (int)float.Parse(sn[i]["close_price"]);
                    curCandle.lowPrice = (int)float.Parse(sn[i]["low_price"]);
                    curCandle.highPrice = (int)float.Parse(sn[i]["high_price"]);
                    curCandle.volume = (int)float.Parse(sn[i]["volume"]);
                    candles.Add(curCandle.Initialize());
                    targetCandle.Add(curCandle.Initialize());
                    //Debug.Log(sn[i]["timebar_1m"]);
                    yield return new WaitForEndOfFrame();
                }
            }
            current = current.AddDays(1);
            counter++;

            if(counter > 24)
            {
                yield return new WaitForEndOfFrame();
                counter = 0;
            }
        }
    }
    public static float Get_PriceHeight(List<Candle> candle)
    {
        var price_high = candle[0].highPrice;
        var price_low = candle[0].lowPrice;
        for(int i = 0; i < candle.Count; i++)
        {
            if (price_high < candle[i].highPrice) price_high = candle[i].highPrice;
            if(price_low > candle[i].lowPrice) price_low = candle[i].lowPrice;
        }
        return price_high - price_low;
    }
    public static float Get_PriceLow(List<Candle> candle)
    {
        var price_low = candle[0].lowPrice;
        for (int i = 0; i < candle.Count; i++)
        {
            if (price_low > candle[i].lowPrice) price_low = candle[i].lowPrice;
        }
        return price_low;
    }
    public static float Get_PriceHeight()
    {
        return price_high - price_low;
    }
}
