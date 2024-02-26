using JsonData;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackTextSystem : MonoBehaviour
{
    public BackgorundOrder back_orders = new BackgorundOrder();
    bool used = false;
    public static ClearEdite clearEdite;
    DateTime start;
    DateTime exite;
    public void SET_StartTime(DateTime s, DateTime e)
    {
        start = s; exite = e;
        BackTest.Initialize(start, exite);
    }
    public void StartBackTest()
    {
        Application.runInBackground = true;

        if (used) return;
        BackTest.GET_PRICE_HISTORY();
        back_orders.orders.Clear();
        back_orders.SET_BackgroundOrder(100000, 1000);

        StartCoroutine(Initialize());
    }
    IEnumerator Initialize()
    {
        used = true;

        BackTest.SET_ACCOUNT();

        float curPrice = BackTest.GET_BTCUSDT_PRICE();
        int delay_count = 0;
        while (curPrice >= 0)
        {
            List<float> orderPrice = back_orders.GET_ORDER_PRICE_DEPTH(curPrice, 0.0174f, 20);

            for (int i = 0; i < orderPrice.Count; i++)
            {
                int size = SettingAPI.GET_ORDER_SIZE(BackTest.filledOrder.Count);

                Order order = new Order("", "", size, "1", "", "").SET_PRICE(orderPrice[i].ToString());
                bool check = BackTest.CHECK_ORDER(order);
                if (!check) BackTest.ADD_OREDER(order);
            }
            BackTest.UPDATE_BACKTEST_FRAME();
            //BackTest.UPDATE_CLOSING_POSITION();
            curPrice = BackTest.GET_BTCUSDT_PRICE();

            BackTest.UPDATE_CLOSING_POSITION_DEPTH();
          
            delay_count++;
            if (delay_count > 10)
            {
                delay_count = 0;
                Debug.Log("-----------------");
                yield return new WaitForEndOfFrame();
            }
        }
        BackTest.EXITE_FILLED_ORDER_CURPRICE();

        for (int i = 0; i < BackTest.orderContract.Count; i++) {
            float start = float.Parse(BackTest.orderContract[i].start.price);
            float exite = float.Parse(BackTest.orderContract[i].eixte.price);
            float gap = BackTest.orderContract[i].GET_GAP_PRICE();
            //Debug.Log("start price:" + start + ", exite price:" + exite + ", gap:" + (exite - start) + ", price:" + gap);
        }
        Debug.Log("position size:" + BackTest.cmtBTCUSDT.size + ", avg_price:" + BackTest.cmtBTCUSDT.average_price + ", contract:" + BackTest.orderContract.Count + ", payoff:" + BackTest.cmtBTCUSDT.payoff + ", unreal_pnl:" + BackTest.cmtBTCUSDT.GET_UNREALIZE_PNL(BackTest.btc_ustd));
      
        used = false;
    }
}
