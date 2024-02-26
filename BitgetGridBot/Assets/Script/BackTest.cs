using JsonData;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackTest
{
    public static float order_gap = 0f;
    public static int index = 0;

    public static float past_btc_usdt = 0;

    public static float btc_ustd = 0;
    public static float btc_ustd_low = 0;
    public static float btc_usdt_high = 0;
    public static string time = "";
    public static string curdate = "";

    public static float small_gap = 0;
    public static float high_gap = 0;
    public static float low_price_close = 0;
    public static bool pivo_236 = false;
    public static bool pivo_382 = false;

    public static List<OrderContract> orderContract = new List<OrderContract>();
    public static List<Order> orderHistory = new List<Order>();
    public static List<Order> liveOrder = new List<Order>();
    public static List<Order> filledOrder = new List<Order>();
    public static Contract cmtBTCUSDT;

    public static List<Dictionary<string, string>> BtcUsdt = new List<Dictionary<string, string>>();

    static DateTime[] date;
    public static void Initialize(DateTime start, DateTime exite)
    {
        date = new DateTime[2];
        date[0] = start; 
        date[1] = exite;
    }
    public static List<Dictionary<string, string>> GET_PRICE_HISTORY()
    {
        BtcUsdt.Clear();
        for (int m = date[0].Month; m <= date[1].Month; m++)
        {
            int dayCount = 31; int dayStart = 1;
            if (m == date[1].Month) dayCount = date[1].Day;
            if(m == date[0].Month) dayStart = date[0].Day;

            for (int d = dayStart; d <= dayCount; d++)
            {
                List<Dictionary<string, string>> sn = CSVReader.Read(Files.DocumentsPath(string.Format("BTCUSD/2018{0:D2}{1:D2}.csv", m, d)));
                if (sn == null) break;
                else BtcUsdt.AddRange(sn);
            }
        }
        return BtcUsdt;
    }
    public static void ADD_OREDER(Order order)
    {
        orderHistory.Add(order);
        liveOrder.Add(order);
    }
    public static bool CHECK_ORDER(Order order)
    {
        List<Order> orders = new List<Order>();
        orders.AddRange(liveOrder);
        orders.AddRange(filledOrder);

        bool flag = false;
        for(int i = 0; i < orders.Count; i++)
        {
            float targetPrice = float.Parse(orders[i].price);
            float orderPrice = float.Parse(order.price);
            if (targetPrice <= orderPrice)
            {
                float gap = SettingAPI.openOrderPer / 100 * orderPrice;
                if (targetPrice + gap * 0.74f < orderPrice) continue;
                else
                {
                    flag = true;
                    break;
                }
            }
        }
        return flag;
    }
    public static float GET_BTCUSDT_PRICE()
    {
        past_btc_usdt = btc_ustd;

        if (index >= BtcUsdt.Count) return -1f;
        btc_ustd = float.Parse(BtcUsdt[index]["close_price"]);
        btc_ustd_low = float.Parse(BtcUsdt[index]["low_price"]);
        btc_usdt_high = float.Parse(BtcUsdt[index]["high_price"]);
        curdate = BtcUsdt[index]["timebar_1m"].Split('T')[0];
        time = BtcUsdt[index++]["timebar_1m"].Split('T')[1];
        //timebar_1m,symbol,open_price,high_price,low_price,close_price,volume
        return btc_ustd;
    }
    public static float SET_ORDER_GAP(float price = 0)
    {
        float btc = btc_ustd;
        if (price > 0) btc = price;

        float order_gap = SettingAPI.closeOrderPer / 100 * btc;
        return order_gap;
    }
    public static void SET_ACCOUNT()
    {
        cmtBTCUSDT = new Contract();
    }
    public static void UPDATE_BACKTEST_FRAME()
    {
        if (past_btc_usdt > btc_ustd)
        {
            for (int i = 0; i < filledOrder.Count; i++)
            {
                if (i >= filledOrder.Count) break;
                float curPrice = float.Parse(filledOrder[i].price);
                if (curPrice + SET_ORDER_GAP(curPrice) < btc_usdt_high)
                {
                    int size = int.Parse(filledOrder[i].size);

                    Order order = new Order("", "", size, "3", "0", "").SET_PRICE((curPrice + SET_ORDER_GAP(curPrice)).ToString());
                    OrderContract orderC = new OrderContract().SET_ORDER(filledOrder[i], order);
                    cmtBTCUSDT.FilledOrder(order);
                    cmtBTCUSDT.ADD_PAYOFF(orderC.GET_GAP_PRICE());
                    orderContract.Add(new OrderContract().SET_ORDER(filledOrder[i], order));
                    filledOrder.RemoveAt(i);
                    i--;
                }
            }
            for (int i = 0; i < liveOrder.Count; i++)
            {
                if (i >= liveOrder.Count) break;
                float curPrice = float.Parse(liveOrder[i].price);
                if (curPrice > btc_ustd_low)
                {
                    cmtBTCUSDT.FilledOrder(liveOrder[i]);
                    filledOrder.Add(liveOrder[i]);
                    liveOrder.RemoveAt(i);
                    i--;
                }
            }
        }
        else
        {
            for (int i = 0; i < liveOrder.Count; i++)
            {
                if (i >= liveOrder.Count) break;
                float curPrice = float.Parse(liveOrder[i].price);
                if (curPrice > btc_ustd_low)
                {
                    cmtBTCUSDT.FilledOrder(liveOrder[i]);
                    filledOrder.Add(liveOrder[i]);
                    liveOrder.RemoveAt(i);
                    i--;
                }
            }
            for (int i = 0; i < filledOrder.Count; i++)
            {
                if (i >= filledOrder.Count) break;
                float curPrice = float.Parse(filledOrder[i].price);
                if (curPrice + SET_ORDER_GAP(curPrice) < btc_ustd)
                {
                    int size = int.Parse(filledOrder[i].size);

                    Order order = new Order("", "", size, "3", "0", "").SET_PRICE((curPrice + SET_ORDER_GAP(curPrice)).ToString());
                    OrderContract orderC = new OrderContract().SET_ORDER(filledOrder[i], order);
                    cmtBTCUSDT.FilledOrder(order);
                    cmtBTCUSDT.ADD_PAYOFF(orderC.GET_GAP_PRICE());
                    orderContract.Add(orderC);
                    filledOrder.RemoveAt(i);
                    i--;
                }
            }
        }
    }
    public static int stoploss_orderCount = 0;
    public static void UPDATE_CLOSING_POSITION_DEPTH()
    {
        float unreal_pnl_small = 0;
        float price = btc_usdt_high - SettingAPI.GET_PER_GAP(btc_usdt_high, 0.074f);
        for (int i = 0; i < filledOrder.Count; i++)
        {
            int size = int.Parse(filledOrder[i].size);
            Order order = new Order("", "", size, "3", "0", "").SET_PRICE((price).ToString());
            OrderContract orderC = new OrderContract().SET_ORDER(filledOrder[i], order);
            unreal_pnl_small += orderC.GET_GAP_PRICE();
        }
        float unreal_pnl_high = 0;
        price = btc_ustd_low;
        for (int i = 0; i < filledOrder.Count; i++)
        {
            int size = int.Parse(filledOrder[i].size);
            Order order = new Order("", "", size, "3", "0", "").SET_PRICE((price).ToString());
            OrderContract orderC = new OrderContract().SET_ORDER(filledOrder[i], order);
            unreal_pnl_high += orderC.GET_GAP_PRICE();
        }

        float real_pnl = cmtBTCUSDT.payoff + unreal_pnl_small;
        if (small_gap == 0) small_gap = real_pnl;
        else if (small_gap < real_pnl) small_gap = real_pnl;

        float real_pnl_high = cmtBTCUSDT.payoff + unreal_pnl_high;
        if (high_gap == 0) high_gap = real_pnl_high;
        else if (high_gap > real_pnl_high) high_gap = real_pnl_high;

        if (low_price_close == 0) low_price_close = btc_ustd_low;
        else if (low_price_close > btc_ustd_low)
        {
            low_price_close = btc_ustd_low;
            pivo_236 = false;
        }
        float order_high = 0;
        for (int i = 0; i < filledOrder.Count; i++)
        {
            float currentPrice = float.Parse(filledOrder[i].price);
            if (currentPrice > order_high) order_high = currentPrice;
        }
        // 다량의 주문 대기가 생길경우 리스크 관리 - 펀딩비 유지
        if (order_high > 0)
        {
            float low_per = (low_price_close / order_high * 100) - 100;
            if (low_per < -23.6)
            {
                float gap = order_high - low_price_close;
                float rebound_per = ((gap + btc_usdt_high - low_price_close) / gap * 100) - 100;
                if (rebound_per > 38.2)
                {
                    // 오픈 매수 주문 가격 오름차순 정렬
                    filledOrder.Sort(delegate (Order a, Order b)
                    {
                        if (float.Parse(a.price) > float.Parse(b.price)) { return -1; }
                        else if (float.Parse(a.price) < float.Parse(b.price)) { return 1; }
                        else return 0;
                    });


                    int count = filledOrder.Count / 2; // 리스크 분할 청산
                    Debug.Log("low:" + low_price_close + " current:" + btc_usdt_high + "low_per:" + low_per+ " re_per:" + rebound_per + " count:" + count);

                    for (int i = 0; i < count; i++)
                    {
                        int index = 0;
                        float close_price = btc_usdt_high - SettingAPI.GET_PER_GAP(btc_usdt_high, 0.074f); // 슬리피지 (발동확률 100%, 지연량 0.074%);
                        Order close_order = filledOrder[index];
                        int size = int.Parse(close_order.size);

                        Order order = new Order("", "", size, "3", "0", "").SET_PRICE((close_price).ToString());
                        OrderContract orderC = new OrderContract().SET_ORDER(close_order, order);
                        cmtBTCUSDT.FilledOrder(order);
                        cmtBTCUSDT.ADD_PAYOFF(orderC.GET_GAP_PRICE());
                        orderContract.Add(orderC);

                        filledOrder.RemoveAt(index);
                        stoploss_orderCount++;
                    }
                    low_price_close = 0;
                }
            }
        }
        if (filledOrder.Count == 0)
        {
            low_price_close = 0;
            pivo_236 = false;
        }
        if (time == "23:59:00Z")
        {
            Debug.Log("time:" + curdate + " high_pnl:" + high_gap + " low_pnl:" + small_gap + " live order:" + filledOrder.Count + " position size:" + cmtBTCUSDT.size + ", avg_price:" + cmtBTCUSDT.average_price + ", contract:" + orderContract.Count + ", payoff:" + cmtBTCUSDT.payoff + ", unreal_pnl:" + unreal_pnl_small +
                " stoploss count:" + stoploss_orderCount);
            small_gap = 0;
            stoploss_orderCount = 0;
        }
    }
    public static void UPDATE_CLOSING_POSITION()
    {
        if(time == "23:59:00Z")
        {
            int count = (int)((float)filledOrder.Count * 0.74f);
            Debug.Log("time:" + curdate + " live order count:" + filledOrder.Count + " close count:" + count);
          
            for (int i = 0; i < count; i++)
            {
                Order curOrder = filledOrder[0];
                float curPrice = btc_ustd;
                Order order = new Order("", "", 1, "3", "0", "").SET_PRICE((curPrice).ToString());
                OrderContract orderC = new OrderContract().SET_ORDER(curOrder, order);
                cmtBTCUSDT.FilledOrder(order);
                cmtBTCUSDT.ADD_PAYOFF(orderC.GET_GAP_PRICE());
                orderContract.Add(orderC);
                filledOrder.RemoveAt(0);
            }

            Debug.Log("position size:" + cmtBTCUSDT.size + ", avg_price:" + cmtBTCUSDT.average_price + ", contract:" + orderContract.Count + ", payoff:" + cmtBTCUSDT.payoff + ", unreal_pnl:" + cmtBTCUSDT.GET_UNREALIZE_PNL(btc_ustd));
        }
    }
    public static void EXITE_FILLED_ORDER_CURPRICE()
    {
        Debug.Log("live order count:" + filledOrder.Count);
        for(int i = 0; i < filledOrder.Count; i++)
        {
            float curPrice = btc_ustd;
            int size = int.Parse(filledOrder[i].size);
            Order order = new Order("", "", size, "3", "0", "").SET_PRICE((curPrice).ToString());
            OrderContract orderC = new OrderContract().SET_ORDER(filledOrder[i], order);
            cmtBTCUSDT.FilledOrder(order);
            cmtBTCUSDT.ADD_PAYOFF(orderC.GET_GAP_PRICE());
            orderContract.Add(orderC);
        }
    }
}
public class BackgorundOrder
{
    public List<Order> orders;
    public BackgorundOrder()
    {
        orders = new List<Order>();
    }
    public BackgorundOrder SET_BackgroundOrder(float max, float low)
    {
        List<Order> sn_orders = new List<Order>();
        float btc_usdt = max;
        while (btc_usdt > low)
        {
            sn_orders.Add(new Order("", "", 1, "", "", "0").SET_PRICE(btc_usdt.ToString()));
            btc_usdt = GET_NEXTORDER_PRICE(btc_usdt);
        }
        orders.Clear();
        orders.AddRange(sn_orders);
        return this;
    }
    public float GET_ORDER_PRICE(float targetPrice, float gapPer)
    {
        float ret = 0;
        float gap = (gapPer / 100) * targetPrice;
        for(int i = 0; i < orders.Count; i++)
        {
            float curPrice = float.Parse(orders[i].price);
            if (curPrice + gap < targetPrice) 
                if(curPrice > ret) ret = curPrice;
        }
        return ret;
    }
    public static float GET_NEXTORDER_PRICE(float price)
    {
        float btc = price;
        return btc - SettingAPI.openOrderPer / 100 * price;
    }
    public List<float> GET_ORDER_PRICE_DEPTH(float targetPrice, float gapPer, int depth)
    {
        List<float> ret = new List<float>();
        for (int i = 0; i < depth; i++)
        {
            ret.Add(GET_ORDER_PRICE(targetPrice, gapPer));
            targetPrice = BitgetAPI.GET_NEXTORDER_PRICE(targetPrice);
        }
        return ret;
    }
}
