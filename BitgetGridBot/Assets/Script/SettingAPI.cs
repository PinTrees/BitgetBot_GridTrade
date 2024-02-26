using JsonData;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SettingAPI
{
    public static float stoploss_Fibonacci = 33.3f;
    public static int leverage = 1;
    public static float closeOrderPer = 0.34f;
    public static float openOrderPer = 0.34f;
    public static float transactionPer = 0.01f;

    public static float canclePer = 1.74f;
    public static float canclePer_CL = -10f;

    public static float overlapPer = 0.074f;
    public static int orderDepth = 2;
    public static int pos_low_price;

    public static int fixedOrderCount = 0;

    private static System.Random random = new System.Random((int)DateTime.Now.Ticks & 0x0000FFFF); //랜덤 시드값
    public static DateTime GET_DATE_TIMEST(string id)
    {
        long timestamp = long.Parse(id);
        DateTime result = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        result = result.AddMilliseconds(timestamp).ToLocalTime();

        return result;
    }
    public static float GET_PER_GAP(float price, float per)
    {
        return (per / 100) * price;
    }
    public static int GET_ORDER_SIZE(int orderCount, int leverage = 1, int default_size = 1)
    {
        orderCount += fixedOrderCount;
        int size = 1;
        if (orderCount < 50) size = default_size * 1;
        else if (orderCount < 100) size = default_size * 2;
        else if (orderCount < 150) size = default_size * 4;
        else if (orderCount < 200) size = default_size * 7;
        else size = 11;

        size = size * leverage;
        return size;
    }
    public static string GET_CLIENT_OID(int lenth)
    {
        return RandomString(lenth).ToLower();
    }
    private static string RandomString(int _nLength = 12)
    {
        const string strPool = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789"; //문자 생성 풀
        char[] chRandom = new char[_nLength]; for (int i = 0; i < _nLength; i++)
        {
            chRandom[i] = strPool[random.Next(strPool.Length)];
        }
        string strRet = new String(chRandom); // char to string
        return strRet; 
    }
    public static List<Order> GET_OrderList(List<float> price, int type)
    {
        List<Order> orders = new List<Order>();
        for (int i = 0; i < price.Count; i++)
        {
            int curPrice = Mathf.RoundToInt(price[i]);
            orders.Add(new Order("cmt_btcusdt", SettingAPI.GET_CLIENT_OID(12), 1, type.ToString(), "0", "0").SET_PRICE(curPrice.ToString())); //price[i].ToString()
        }
        return orders;
    }
}

public enum ClearEdite
{
    FixedTime,
    Depth
}
