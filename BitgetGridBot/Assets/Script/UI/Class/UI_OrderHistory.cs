using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using JsonData;
using UI;
using TMPro;
using System;

public class UI_OrderHistory : UIView
{
    public List<OrderDetailUI> order_history_uis = new List<OrderDetailUI>();
    public Transform orders_tr;

    public TextMeshProUGUI orderDetail;
    void Start()
    {
        Init();
        Show();
    }
    public override void Init()
    {
        base.Init();
        for(int i = 0; i < orders_tr.childCount; i++)
        {
            order_history_uis.Add(new OrderDetailUI(orders_tr.GetChild(i)));
        }
    }
    public override void Show()
    {
        base.Show();
    }
    public void TR_Display()
    {
        Display(BitgetAPI.order_historys);
    }
    public void Display(List<OrderDetail> sn)
    {
        for(int i = 0; i < order_history_uis.Count; i++)
        {
            if (i >= sn.Count) continue;
            order_history_uis[i].Display(sn[i].GetText());
        }
        Show();
    }
    public void TR_Display_OrderHistory(Transform tr)
    {
        int index = tr.GetSiblingIndex();
        List<OrderDetail> orders = new List<OrderDetail>();
        orderDetail.text = "[선택된 주문 없음]";
        if (index == 0)
        {
            orderDetail.text = "[미체결 매수주문 목록]\n";
            orders = BitgetAPI.liveOrderDetails;
        }
        else if (index == 1)
        {
            orderDetail.text = "[미체결 매도주문 목록]\n";
            orders = BitgetAPI.liveOrders_CloseLong;
        }
        else if (index == 2)
        {
            orderDetail.text = "[체결된 매수주문 목록]\n";
            orders = BitgetAPI.filledOrderDetails_OpenLong;
        }
        else if (index == 3)
        {
            orderDetail.text = "[체결된 매도주문 목록]\n";

        }
        else if (index == 4)
        {
            orderDetail.text = "[체결된 페어주문 목록]\n";

            DateTime curDate = BitgetAPI.dateTime;
            string folder = string.Format("{0}_{1}_{2}", curDate.Year, curDate.Month, curDate.Day);
            string sn = Files.READ_TXT(Files.DocumentsPath(folder + "/CT.txt"));
            string[] orderlow = sn.Split(';');
            for(int i = 0; i < orderlow.Length; i++)
            {
                string[] value = orderlow[i].Split(',');
                if (value.Length < 13) continue;

                orderDetail.text += string.Format("[{0}매수가:{2}][{1}매도가:{3}]", value[1], value[2], value[3], value[4]);
                orderDetail.text += string.Format("[규모:{0}-{1}]", value[5], value[6]);
                orderDetail.text += string.Format("[수수료:{0:N4},{1:N4}]", double.Parse(value[7]), double.Parse(value[8]));
                orderDetail.text += string.Format("[주문번호:{0},{1}]", double.Parse(value[9]), double.Parse(value[10]));
                orderDetail.text += string.Format("[주문시각:{0},{1}]", double.Parse(value[11]), double.Parse(value[12]));
                orderDetail.text += "\n";
            }
            return;
        }
        for (int i = 0; i < orders.Count; i++)
            orderDetail.text += orders[i].GetInfo() + "\n";
    }
}
