using JsonData;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class UI_TradeEditor : UIView
{
    public AutoTrade autoTrade;
    public TMP_InputField checkPer;
    public TMP_InputField depth;
    public TMP_InputField lowPrice;
    public TMP_InputField fixedOrderCount;
    public TextMeshProUGUI fixedOrderCountText;

    public override void Init()
    {
        base.Init();
    }
    public override void Show()
    {
        base.Show();
    }
    public override void Close()
    {
        base.Close();
    }
    public void SET_LOWPRICE()
    {
        SettingAPI.pos_low_price = (int)float.Parse(lowPrice.text);
    }
    public void TR_CloseLiveOrders()
    {
        SettingAPI.orderDepth = 0;
        StartCoroutine(autoTrade.Closing_LiveOrders());
    }
    public void TR_OrderStart()
    {
        autoTrade.closeOrder = false;
        SettingAPI.orderDepth = 1;
    }
    public void TR_SettingChange()
    {
        if (depth.text != string.Empty)
            SettingAPI.orderDepth = int.Parse(depth.text);

        if (checkPer.text != string.Empty)
            SettingAPI.canclePer = float.Parse(checkPer.text);
    }
    public void TR_FixedOrderCountChange()
    {
        if (fixedOrderCount.text != string.Empty)
            SettingAPI.fixedOrderCount = int.Parse(fixedOrderCount.text);

        fixedOrderCountText.text = SettingAPI.fixedOrderCount.ToString();
    }

    public void TR_AddFieldOrder()
    {

    }
}
