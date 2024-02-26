using Bitget.Order;
using BitgetClassJson;
using JsonData;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoTrade : MonoBehaviour
{
    public List<OrderDetail> order_history;
    public BackgorundOrder backOrders = new BackgorundOrder();
    public List<OrderContract_BT> orderContracts = new List<OrderContract_BT>();

    public bool closeOrder = false;
    bool orderPosting = false;

    UI_Debug debug;
    UI_PriceChart price_chart;

    bool closePosition = true;
    DateTime lastUpdate;
    public void Start()
    {
        Application.runInBackground = true;

        debug = (UI_Debug)UINavigation.instance.GetUIView("debug_canvas");
        price_chart = (UI_PriceChart)UINavigation.instance.GetUIView("price_chart");
    }
    public void TR_START()
    {
        StartCoroutine(StartAuto());
    }
    IEnumerator StartAuto()
    {
        yield return StartCoroutine(Initialize());

        StartCoroutine(Update_Price());
        StartCoroutine(Update_Status());
        StartCoroutine(Update_AutoTrade());
    }
    IEnumerator Initialize()
    {
        SettingAPI.pos_low_price = 0;
        backOrders.SET_BackgroundOrder(100000, 5000);
        yield return StartCoroutine(BitgetAPI.Get_SERVER_TIME());
        yield return StartCoroutine(BitgetAPI.GET_ACCOUNT_DETAIL("cmt_btcusdt", null, BitgetAPI.startAccount));
        yield return StartCoroutine(BitgetAPI.GET_ACCOUNT_DETAIL("cmt_btcusdt"));

        yield return StartCoroutine(BitgetAPI.Get_MARKET_PRICE("cmt_btcusdt"));
        yield return StartCoroutine(BitgetAPI.GET_POSITION_DETAIL("cmt_btcusdt", null));

        yield return StartCoroutine(BitgetAPI.GET_ORDERS_HISTORY("cmt_btcusdt"));

        yield return null;
    }
    IEnumerator Get_SaveData_OrderDetail(string path, List<OrderDetail> od)
    {
        string filled_long = Files.READ_TXT(path);
        if (filled_long == null) yield break;
        if (filled_long.Length < 5) yield break;

        List<OrderDetail> order_list = new List<OrderDetail>();
        string[] orders = filled_long.Split(';');
        for (int i = 0; i < orders.Length; i++)
        {
            if (orders[i].Length < 5) continue;

            Dictionary<string, string> var = new Dictionary<string, string>();
            string[] val = orders[i].Split(',');
            for (int j = 0; j < val.Length; j++)
                if (val[j].Length > 2)
                    var.Add(val[j].Split(':')[0], val[j].Split(':')[1]);

            OrderDetail order_new = new OrderDetail(var);
            order_list.Add(order_new);
        }

        if (od == null) od = new List<OrderDetail>();
        od.Clear();
        od.AddRange(order_list);
        yield return null;
    }

    public IEnumerator Closing_LiveOrders()
    {
        closeOrder = true;
        while (true)
        {
            yield return new WaitForSeconds(0.05f);
            if (orderPosting) continue;

            break;
        }
        while (true)
        {
            OrderIds orderIds = new OrderIds();
            orderIds.symbol = "cmt_btcusdt";

            for (int i = 0; i < BitgetAPI.liveOrderDetails.Count; i++)
                if (orderIds.ids.Count < 20)
                    orderIds.ids.Add(BitgetAPI.liveOrderDetails[i].order_id);
            for (int i = 0; i < BitgetAPI.liveOrders_CloseLong.Count; i++)
                if (orderIds.ids.Count < 20)
                    orderIds.ids.Add(BitgetAPI.liveOrders_CloseLong[i].order_id);

            if (orderIds.ids.Count <= 0) yield break;
            yield return StartCoroutine(BitgetAPI.POST_CLOSE_ORDERS(orderIds));

            yield return StartCoroutine(Rf_Update_OpenLong());    // ��ü�� ���¸ż� �ֹ� ������Ʈ
            yield return StartCoroutine(Rf_Update_CloseLong());       // ��ü��� �ŵ��ֹ� ������Ʈ

            if (BitgetAPI.liveOrderDetails.Count <= 0 && BitgetAPI.liveOrders_CloseLong.Count <= 0) break;
            yield return new WaitForSeconds(0.1f);
        }
    }
    IEnumerator Update_System()
    {
      
        while (true)
        {
            yield return new WaitForSeconds(10f);

            Error[] error = new Error[3] { new Error(), new Error(), new Error() };
            yield return StartCoroutine(BitgetAPI.Get_SERVER_TIME(error[0]));
            yield return StartCoroutine(BitgetAPI.Get_MARKET_PRICE("cmt_btcusdt", error[1]));
            yield return StartCoroutine(BitgetAPI.Get_MARKET_LIMIT("cmt_btcusdt", error[2]));

            bool error_flag = false;
            for(int i = 0; i < error.Length; i++)
            {
                if (error[i].code == 403)
                    debug.system.text = "update:" + DateTime.Now.Second.ToString("N2") + " system error: ������ �Ǵ� ���� ����";

                if (error_flag) continue;
                else error_flag = error[i].error;
            }

            if(!error_flag) yield break;
        }
    }
    IEnumerator Update_Price()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.1f);

            Error[] error = new Error[3] { new Error(), new Error(), new Error() };
            yield return StartCoroutine(BitgetAPI.Get_SERVER_TIME(error[0]));
            yield return StartCoroutine(BitgetAPI.Get_MARKET_PRICE("cmt_btcusdt", error[1]));
            yield return StartCoroutine(BitgetAPI.Get_MARKET_LIMIT("cmt_btcusdt", error[2]));

            for (int i = 0; i < error.Length; i++)
            {
                if (error[i].code == 403)
                {
                    yield return StartCoroutine(Update_System());
                    break;
                }
            }

            if (SettingAPI.pos_low_price == 0) SettingAPI.pos_low_price = (int)BitgetAPI.curPrice_BTCUSDT;
            else if (SettingAPI.pos_low_price > BitgetAPI.curPrice_BTCUSDT) SettingAPI.pos_low_price = (int)BitgetAPI.curPrice_BTCUSDT;

            DateTime result = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            BitgetAPI.dateTime = result = result.AddMilliseconds(BitgetAPI.timestamp).ToLocalTime();

            DateTime closeTime = result.AddDays(1);
            //closeTime = closeTime.AddMinutes(0);
            TimeSpan dist = result - closeTime;
            debug.time.text = string.Format("[server time:{0:D2}H{1:D2}M{2:D2}S] [day sec:{3:N0}S]", result.Hour, result.Minute, result.Second, dist.TotalSeconds);

            IO_SAVE_Price(SettingAPI.pos_low_price);
            ID_SAVE_TEXT("last:" + BitgetAPI.timestamp, "setting", "update_time.txt");
        }
    }
    IEnumerator Update_Status()
    {
        Error[] error = new Error[2] { new Error(), new Error() };

        while (true)
        {
            yield return new WaitForSeconds(0.3f);

            yield return StartCoroutine(BitgetAPI.GET_POSITION_DETAIL("cmt_btcusdt", error[0]));
            yield return StartCoroutine(BitgetAPI.GET_ACCOUNT_DETAIL("cmt_btcusdt", error[1]));
        }
    }
    IEnumerator Update_AutoTrade()
    {
        yield return StartCoroutine(Get_SaveData_OrderDetail(Files.DocumentsPath("orders/update_long_filled.txt"), BitgetAPI.filledOrderDetails_OpenLong));
        yield return StartCoroutine(Get_SaveData_OrderDetail(Files.DocumentsPath("orders/update_short_live.txt"), BitgetAPI.liveOrders_CloseLong));
        yield return StartCoroutine(Get_SaveData_OrderDetail(Files.DocumentsPath("orders/update_long_live.txt"), BitgetAPI.liveOrderDetails));
   
        yield return StartCoroutine(Rf_Update_OpenLong());    // ��ü�� ���¸ż� �ֹ� ������Ʈ
        yield return StartCoroutine(Rf_Update_CloseLong());   // ��ü��� �ŵ��ֹ� ������Ʈ + ü��� �ŵ��ֹ� Ȯ��
        yield return StartCoroutine(Rf_Range_CloseLong()); // ���� �̰ݵ��� ���� ��ü�� �ŵ� �ֹ� ��� + �ű� �ŵ��ֹ� ����

        while (true)
        {
            yield return new WaitForSeconds(0.3f);

            lastUpdate = BitgetAPI.dateTime;
            debug.updateTime.text = string.Format("[last_update-{0:D2}:{1:D2}:{2:D2}.{3:D2}]", lastUpdate.Hour, lastUpdate.Minute, lastUpdate.Second, lastUpdate.Millisecond);

            if (closeOrder) continue;
            orderPosting = true;
            yield return StartCoroutine(Rf_Start_OpenLong());   // �ű� �ż��ֹ� ������Ʈ

            yield return StartCoroutine(Rf_Range_OpenLong());  // �ֹ��� ���� ���� �̰��� ���� ��� �ֹ����
            yield return StartCoroutine(Rf_Update_OpenLong());    // ��ü�� ���¸ż� �ֹ� ������Ʈ

            yield return StartCoroutine(Rf_Update_CloseLong());   // ��ü��� �ŵ��ֹ� ������Ʈ + ü��� �ŵ��ֹ� Ȯ��
            yield return StartCoroutine(Rf_Range_CloseLong()); // ���� �̰ݵ��� ���� ��ü�� �ŵ� �ֹ� ��� + �ű� �ŵ��ֹ� ����

            if (closePosition)
                yield return StartCoroutine(Rf_ClosePosition());    // ü��� �ż��ֹ� �Ը� ����

            yield return StartCoroutine(Rf_Live_Position());

            IO_Save_OrderDetail(BitgetAPI.filledOrderDetails_OpenLong, "orders", "update_long_filled.txt");
            IO_Save_OrderDetail(BitgetAPI.liveOrders_CloseLong, "orders", "update_short_live.txt");
            IO_Save_OrderDetail(BitgetAPI.liveOrderDetails, "orders", "update_long_live.txt");

            orderPosting = false;
        }
    }
    IEnumerator Rf_Start_OpenLong()
    {
        debug.oderStartDebug.text = "[������Ʈ:" + BitgetAPI.dateTime.Second + "]";

        List<float> orderPrice = backOrders.GET_ORDER_PRICE_DEPTH(BitgetAPI.curPrice_BTCUSDT, 0.074f, SettingAPI.orderDepth); // ���� 2 - �� 1.74%
        for (int i = 0; i < orderPrice.Count; i++)
            if (!BitgetAPI.GET_OPENLONG_OVERAP(orderPrice[i])) { orderPrice.RemoveAt(i); i--; }   // ���� �ֹ��� ���� ������ Ȯ��

        if (orderPrice.Count <= 0) { debug.oderStartDebug.text += "[�ű��ֹ� ����]"; yield break; }

        debug.oderStartDebug.text += "[�ű��ֹ� " + orderPrice.Count + "�� ó����]";
        for (int i = 0; i < orderPrice.Count; i++)
        {
            int orderCount = BitgetAPI.filledOrderDetails_OpenLong.Count;

            int price = (int)orderPrice[i];
            int size = SettingAPI.GET_ORDER_SIZE(orderCount, SettingAPI.leverage);
            Order order = new Order("cmt_btcusdt", SettingAPI.GET_CLIENT_OID(12), size, "1", "0", "0");
            order.SET_PRICE(price.ToString());

            OrderDetail orderInfo = new OrderDetail();
            yield return StartCoroutine(POST_ORDER(order, orderInfo));

            if (orderInfo.symbol != null)
                BitgetAPI.liveOrderDetails.Add(orderInfo);
            yield return new WaitForSeconds(0.1f);
        }
        debug.oderStartDebug.text += "[�ű��ֹ� " + orderPrice.Count + "�� �ֹ��Ϸ�]";
    }

    IEnumerator Rf_Update_OpenLong()
    {
        List<OrderDetail> orders = BitgetAPI.liveOrderDetails;

        if(orders.Count <= 0)
        debug.liveOL_txt.text = string.Format("[������Ʈ:{0}][��ü�� �ż��ֹ� ����:{1}]", BitgetAPI.dateTime.Second, orders.Count);

        for (int i = 0; i < orders.Count; i++)
        {
            debug.liveOL_txt.text = string.Format("[������Ʈ:{0}][������:{2}][��ü�� �ż��ֹ� ����:{1}]", BitgetAPI.dateTime.Second, orders.Count, i);
            yield return StartCoroutine(BitgetAPI.GET_ORDER_DETAIL(new OrderId("cmt_btcusdt", orders[i].order_id), orders[i]));
            yield return new WaitForSeconds(0.1f);

            if (orders[i].status == "-1") { orders.RemoveAt(i); i--; continue; }
            else if (orders[i].status == "2") // filled
            {
                Refresh_Save_Filled_OL(orders[i]); // ü��� �ż� �ֹ� ���
                BitgetAPI.filledOrderDetails_OpenLong.Add(orders[i]); // �ֹ� ü������� ����
                orders.RemoveAt(i--); // ��ü�� �ֹ� ��Ͽ��� ����
            }
        }
    }
    IEnumerator Rf_Range_OpenLong()
    {
        List<OrderDetail> orders = BitgetAPI.liveOrderDetails;

        OrderIds orderIds = new OrderIds();
        orderIds.symbol = "cmt_btcusdt";

        int price = (int)BitgetAPI.curPrice_BTCUSDT;
        for (int i = 0; i < orders.Count; i++)
        {
            if (orders[i].status != "0") continue;

            float curPrice = float.Parse(orders[i].price);
            if (price > (int)curPrice) price = (int)curPrice;

            float gap = SettingAPI.GET_PER_GAP(curPrice, SettingAPI.canclePer);
            float limitPrice = curPrice + gap;

            if (limitPrice < BitgetAPI.curPrice_BTCUSDT)
                if (orderIds.ids.Count < 20)
                    orderIds.ids.Add(orders[i].order_id);
        }

        float percent = (BitgetAPI.curPrice_BTCUSDT - price) / BitgetAPI.curPrice_BTCUSDT * 100;

        price_chart.depth_low.text = price.ToString();
        price_chart.depth_lowPer.text = percent.ToString();

        if (orderIds.ids.Count <= 0) yield break;
        yield return StartCoroutine(BitgetAPI.POST_CLOSE_ORDERS(orderIds));
    }
    IEnumerator Rf_Range_CloseLong()
    {
        List<OrderDetail> filled_orders = BitgetAPI.filledOrderDetails_OpenLong;
        List<OrderDetail> live_CLorders = BitgetAPI.liveOrders_CloseLong;
        string debugs = "y";

        OrderIds orderIds = new OrderIds();
        orderIds.symbol = "cmt_btcusdt";

        debug.newCL_txt.text = string.Format("[������Ʈ:{0}][�ű� �ŵ��ֹ� Ȯ����]", BitgetAPI.dateTime.Second);

        for (int i = 0; i < filled_orders.Count; i++)
        {
            debug.newCL_txt.text = string.Format("[������Ʈ:{0}][IDX:{1}][�ű� �ŵ��ֹ� Ȯ����]", BitgetAPI.dateTime.Second, i);

            int price = (int)float.Parse(filled_orders[i].price);
            float percent = (BitgetAPI.curPrice_BTCUSDT / price * 100) - 100;
            debugs += " [id:" + i + ",per:" + percent.ToString("N2")+ "]";
            yield return null;
            // �ż� �ֹ� �ǽð� ü�� ���� ������ ���ԵǴ��� Ȯ��
            if (percent > SettingAPI.canclePer_CL)
            {
                debug.newCL_txt.text = string.Format("[������Ʈ:{0}][IDX:{1}][�ű� �ŵ��ֹ� ������]", BitgetAPI.dateTime.Second, i);
                // �ż��ֹ��� �����Ǵ� ��ü�� �ŵ��ֹ� ��û
                OrderDetail close_order = BitgetAPI.GET_CLOSELONG_ORDER(filled_orders[i].client_oid);
                if (close_order != null) continue;

                int size = int.Parse(filled_orders[i].size);
                float limit = float.Parse(BitgetAPI.marketLimit.lowest); limit += SettingAPI.GET_PER_GAP(limit, SettingAPI.transactionPer);
                int closePrice = (int)(price + SettingAPI.GET_PER_GAP(price, SettingAPI.closeOrderPer));

                Order order;
                if (closePrice <= (int)limit) // ���� �ǸŰ��� ���� �ǸŰ����� ���ٸ� - �޵� - ���尡�� ����
                {
                    order = new Order("cmt_btcusdt", filled_orders[i].client_oid + "#" + SettingAPI.GET_CLIENT_OID(12), size, "3", "2", "1");
                    order.SET_PRICE(((int)BitgetAPI.curPrice_BTCUSDT).ToString());
                }
                else
                {
                    order = new Order("cmt_btcusdt", filled_orders[i].client_oid + "#" + SettingAPI.GET_CLIENT_OID(12), size, "3", "0", "0");
                    order.SET_PRICE(closePrice.ToString());
                }

                OrderDetail order_info = new OrderDetail();
                yield return StartCoroutine(POST_ORDER(order, order_info));
                if (order_info.symbol != null)
                {
                    // Fill or Kill �ֹ� �� Ȯ�� - �ֹ� ó�� �ð��� �ʿ��� ��� ���߿� ������
                    if (order_info.status == "-1") continue;
                    BitgetAPI.liveOrders_CloseLong.Add(order_info);
                }
                yield return new WaitForSeconds(0.01f);
            }
            else
            {
                OrderDetail close_order = BitgetAPI.GET_CLOSELONG_ORDER(filled_orders[i].client_oid);
                if (close_order != null)
                {
                    int status = -1;
                    int.TryParse(close_order.status, out status);
                    if (status == -1) continue;
                    else if (status != 0) continue;
                    if (orderIds.ids.Count >= 10) continue;
                    orderIds.ids.Add(close_order.order_id);
                }
            }
        }

        debug.newCL_txt.text = string.Format("[������Ʈ:{0}][�ű� �ŵ��ֹ� ����]", BitgetAPI.dateTime.Second);
        Files.WRITE_TXT(Files.DocumentsPath("debug/orders_closelong.txt"), debugs + ";");
        yield return new WaitForSeconds(0.1f);
        if (orderIds.ids.Count > 0)
            debug.newCL_txt.text = string.Format("[������Ʈ:{0}][�ŵ��ֹ� ��� ����{1}]", BitgetAPI.dateTime.Second, orderIds.ids.Count);
        else yield break;

        yield return StartCoroutine(BitgetAPI.POST_CLOSE_ORDERS(orderIds));
    }

    IEnumerator POST_ORDER(Order order, OrderDetail orderResult)
    {
        Error error = new Error();
        OrderId_Post resurlt = new OrderId_Post();
        yield return StartCoroutine(BitgetAPI.POST_ORDER(order, resurlt, error));
        if (error.error) yield break;
        if (error.code != 200) yield break;

        yield return new WaitForSeconds(0.1f);

        OrderDetail result = new OrderDetail();
        OrderId post = new OrderId("cmt_btcusdt", resurlt.order_id);
        yield return StartCoroutine(BitgetAPI.GET_ORDER_DETAIL(post, result));
        orderResult = orderResult.Instance(result);
    }

    IEnumerator Rf_Update_CloseLong()
    {
        List<OrderDetail> orders = BitgetAPI.liveOrders_CloseLong;

        BitgetAPI.liveOrders_CloseLong.Sort(delegate (OrderDetail a, OrderDetail b)
        {
            if (float.Parse(a.price) > float.Parse(b.price)) { return -1; }
            else if (float.Parse(a.price) < float.Parse(b.price)) { return 1; }
            else return 0;
        });

        debug.liveCL_txt.text = "[������Ʈ:" + BitgetAPI.dateTime.Second + "][��ü�� �ŵ��ֹ� ����:" + orders.Count + "]";
        for (int i = 0; i < orders.Count; i++)
        {
            float curPrice = float.Parse(orders[i].price);
            float gap = SettingAPI.GET_PER_GAP(curPrice, SettingAPI.canclePer);
            float limitPrice = curPrice - gap;

            if (i != 0)
                if (limitPrice > BitgetAPI.curPrice_BTCUSDT) continue;

            yield return StartCoroutine(BitgetAPI.GET_ORDER_DETAIL(new OrderId("cmt_btcusdt", orders[i].order_id), orders[i]));
            debug.liveCL_txt.text = string.Format("[Ȯ����][������Ʈ:{0}][�����ֹ�����:{1}][��ü�� �ŵ��ֹ� ����:{2}]", BitgetAPI.dateTime.Second, orders[i].status, orders.Count);

            if (orders[i].status == "-1") { orders.RemoveAt(i); i--; continue; }
            if (orders[i].status == "2") // �ֹ� ä����
            {
                string clienId = orders[i].client_oid.Split('#')[0]; // ��� �ŷ� ������ Ŭ���̾�Ʈ ���̵�
                OrderDetail start = BitgetAPI.GET_OPENLONG_ORDER(clienId); // ��� �ŷ� ������
                orderContracts.Add(new OrderContract_BT().SET_START_ORDER(start).SET_EXITE_ORDER(orders[i])); // ��� �ŷ� ���

                BitgetAPI.filledOrderDetails_OpenLong.Remove(start); // ���� ��� �ż� �ֹ� ����
                Refresh_Save_Filled_CL(orders[i]); // ü��� �ŵ� �ֹ� ���
                orders.RemoveAt(i); i--;
            }
            yield return new WaitForSeconds(0.1f);
        }
        Refresh_Save_Contract();
    }

    IEnumerator Rf_ClosePosition()  // ������ �Ը� ������ ��� �Ϻ� ������ ���� û��
    {
        if (SettingAPI.pos_low_price <= 0) yield break;
        if (BitgetAPI.filledOrderDetails_OpenLong.Count <= 0) yield break;

        BitgetAPI.filledOrderDetails_OpenLong.Sort(delegate (OrderDetail a, OrderDetail b)
        {
            if (float.Parse(a.price) > float.Parse(b.price)) { return -1; }
            else if (float.Parse(a.price) < float.Parse(b.price)) { return 1; }
            else return 0;
        });
        List<OrderDetail> orders = BitgetAPI.filledOrderDetails_OpenLong;

        float order_high = float.Parse(orders[0].price);    // �ְ� �ż� ����
        float low_per = (SettingAPI.pos_low_price / order_high * 100) - 100;    // �ְ� �ż������� ���� �϶� ��

        float gap = order_high - SettingAPI.pos_low_price;      // �ְ� ���� ������ �Ǻ���ġ ����
        float rebound_per = ((gap + BitgetAPI.curPrice_BTCUSDT - SettingAPI.pos_low_price) / gap * 100) - 100;  // �Ǻ���ġ ������ ���� �ݵ� �Ը�

        string debugTxt = string.Format("[�ְ�ż�����:{0}] [��������:{1}] [�̰ݵ�:{2:N2}%] [�Ǻ���ġ�ݵ�:{3:D2}%]", (int)float.Parse(orders[0].price), (int)SettingAPI.pos_low_price, low_per, (int)rebound_per);
        debug.rebound.text = debugTxt;

        if (low_per < -23.6)    // ���� �϶� Ȯ��
        {
            if (rebound_per > 38.2) // �Ǻ���ġ �ݵ� Ȯ��
            {
                int count = orders.Count / 2;   // ������ �Ը��� ��ܺ� û��

                OrderIds orderIds = new OrderIds(); // �Ϻ� ��ü�� �ŵ��ֹ� ���
                orderIds.symbol = "cmt_btcusdt";

                for (int i = 0; i < count; i++)    
                {
                    if (orderIds.ids.Count >= 20)   // ��ü�� �ŵ��ֹ� ó�� �Ѱ�
                    {
                        yield return StartCoroutine(BitgetAPI.POST_CLOSE_ORDERS(orderIds));
                        yield return new WaitForSeconds(1f);
                        orderIds.ids.Clear();   // �ʱ�ȭ
                    }
                    // ü��� �ż� �ֹ��� ��� �ŵ� �ֹ� �˻�
                    OrderDetail cancle_order = BitgetAPI.GET_CLOSELONG_ORDER(orders[i].client_oid);
                    if (cancle_order != null)  
                        orderIds.ids.Add(cancle_order.order_id);
                }

                if (orderIds.ids.Count > 0)
                    yield return StartCoroutine(BitgetAPI.POST_CLOSE_ORDERS(orderIds));
                yield return new WaitForSeconds(1f);
                
                // ������ ����û�꿡 ����� ü��ż��ֹ��� ���� �ŵ��ֹ� ��û
                for (int i = 0; i < count; i++)
                {
                    // ü��� �ż��ֹ��� ��� �ű� �ŵ��ֹ� ���� - ���尡 ����
                    int size = int.Parse(orders[i].size);
                    Order order = new Order("cmt_btcusdt", orders[i].client_oid + "#" + SettingAPI.GET_CLIENT_OID(12), size, "3", "2", "1");
                    order.SET_PRICE(((int)BitgetAPI.curPrice_BTCUSDT).ToString());

                    OrderDetail orderResult = new OrderDetail();
                    yield return StartCoroutine(POST_ORDER(order, orderResult));
                    if (orderResult.symbol != null)
                    {
                        // Fill or Kill �ֹ� �� Ȯ�� - �ֹ� ó�� �ð��� �ʿ��� ��� ���߿� ������
                        if (orderResult.status == "-1") continue; 
                        // ��ü�� �ŵ��ֹ��� �߰� - ���� ������ Ȯ�ο� ü����� Ȯ�� �ű� ��Ʈ��Ʈ�� �и���
                        BitgetAPI.liveOrders_CloseLong.Add(orderResult);
                    }
                    yield return new WaitForSeconds(0.1f);
                }
                SettingAPI.pos_low_price = 0;   // �ʱ�ȭ
            }
        }
    }
    IEnumerator Rf_Live_Position()
    {
        List<OrderDetail> orders = BitgetAPI.filledOrderDetails_OpenLong;
        int size = 0, parse;
        for (int i = 0; i < orders.Count; i++)
            if (int.TryParse(orders[i].size, out parse))
                size += parse;

        if (orders.Count <= 0) SettingAPI.pos_low_price = 0;

        int realsize;
        int.TryParse(BitgetAPI.holdPosition.holding[0].position, out realsize);

        debug.filledOL_txt.text = string.Format("[update:{0}] [ü��ż��ֹ�:{1}+{5}] [ü�������ǱԸ�:{2}] [��ü��Ը�:{3}] [�̰�:{4}]", BitgetAPI.dateTime.Second,
           orders.Count, size, realsize, realsize - size, SettingAPI.fixedOrderCount);

        if (size != realsize)
        {

        }
        yield return null;
    } // �ֹ� �Ը� Ȯ��

    void Refresh_Save_Contract()
    {
        for (int i = 0; i < orderContracts.Count; i++)
        {
            OrderContract_BT curContract = orderContracts[0];

            string save = "";
            if (curContract.start == null || curContract.eixte == null) save = "error";
            else
            {
                save += curContract.start.symbol;
                save += "," + curContract.start.type;
                save += "," + curContract.eixte.type;

                save += "," + curContract.start.price_avg;
                save += "," + curContract.eixte.price_avg;

                save += "," + curContract.start.size;
                save += "," + curContract.eixte.size;

                save += "," + curContract.start.fee;
                save += "," + curContract.eixte.fee;

                save += "," + curContract.start.order_id;
                save += "," + curContract.eixte.order_id;

                save += "," + curContract.start.createTime;
                save += "," + curContract.eixte.createTime;
            }
            save += ";";

            DateTime curDate = BitgetAPI.dateTime;
            string folder = string.Format("{0}_{1}_{2}", curDate.Year, curDate.Month, curDate.Day);
            Files.APPEND_TXT(Files.DocumentsPath(folder + "/CT.txt"), save);
            orderContracts.RemoveAt(i);
            i--;
        }
    }
    void Refresh_Save_Filled_OL(OrderDetail curOrder)
    {
        string save = "";
        if (curOrder == null) save = "error";
        else
        {
            save = curOrder.symbol;
            save += "," + curOrder.type;
            save += "," + curOrder.size;
            save += "," + curOrder.price;
            save += "," + curOrder.price_avg;
            save += "," + curOrder.fee;
            save += "," + curOrder.order_id;
            save += "," + curOrder.client_oid;
            save += "," + curOrder.createTime;
        }
        save += ";";

        DateTime curDate = BitgetAPI.dateTime;
        string folder = string.Format("{0}_{1}_{2}", curDate.Year, curDate.Month, curDate.Day);
        Files.APPEND_TXT(Files.DocumentsPath(folder + "/OpenLong.txt"), save);
    }
    void Refresh_Save_Filled_CL(OrderDetail curOrder)
    {
        string save = curOrder.symbol;
        save += "," + curOrder.type;
        save += "," + curOrder.size;
        save += "," + curOrder.price;
        save += "," + curOrder.price_avg;
        save += "," + curOrder.fee;
        save += "," + curOrder.order_id;
        save += "," + curOrder.client_oid;
        save += "," + curOrder.createTime;
        save += ";";

        DateTime curDate = BitgetAPI.dateTime;
        string folder = string.Format("{0}_{1}_{2}", curDate.Year, curDate.Month, curDate.Day);
        Files.APPEND_TXT(Files.DocumentsPath(folder + "/CloseLong.txt"), save);
    }
    void IO_Save_OrderDetail(List<OrderDetail> orders, string path, string file)
    {
        string save = "";
        for (int i = 0; i < orders.Count; i++)
        {
            OrderDetail curOrder = orders[i];

            save += "symbol:" + curOrder.symbol;
            save += ",type:" + curOrder.type;
            save += ",size:" + curOrder.size;
            save += ",price:" + curOrder.price;
            save += ",price_avg:" + curOrder.price_avg;
            save += ",fee:" + curOrder.fee;
            save += ",order_id:" + curOrder.order_id;
            save += ",client_oid:" + curOrder.client_oid;
            save += ",createTime:" + curOrder.createTime;
            save += ";\n";
        }
        Debug.Log(save);
        Files.WRITE_TXT(Files.DocumentsPath(path + "/" + file), save);
    }
    void ID_SAVE_TEXT(string save, string path, string file)
    {
        Files.WRITE_TXT(Files.DocumentsPath(path + "/" + file), save);
    }
    void IO_SAVE_Price(float low)
    {
        string save = "";

        save += "price_low:" + low;

        string folder = string.Format("setting");
        Files.WRITE_TXT(Files.DocumentsPath(folder + "/price_update.txt"), save);
    }
    void SAVE_TradingSetting()
    {
        Files.WRITE_TXT(Files.DocumentsPath("setting_trading.txt"), "count:1;\nlastOrder:00150032305");
        string data = Files.READ_TXT(Files.DocumentsPath("setting_trading.txt"));
        Debug.Log(data);
    }
}
