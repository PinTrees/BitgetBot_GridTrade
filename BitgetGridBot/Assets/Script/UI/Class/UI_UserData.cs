using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using JsonData;
using System;

public class UI_UserData : UIView
{
    public TextMeshProUGUI holdPositionInfo;
    public TextMeshProUGUI currentStatue;
    public TextMeshProUGUI account;
    private void Start()
    {
        Init();
        Show();
    }
    public override void Init()
    {
        base.Init();
    }
    public override void Show()
    {
        base.Show();
        StartCoroutine(Update_UserPosition());
        StartCoroutine(Update_Statue());
    }
    public override void Close()
    {
        base.Close();
    }
    IEnumerator Update_UserPosition()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.5f);
            holdPositionInfo.text = BitgetAPI.holdPosition.GetInfoText();
        }
    }
    IEnumerator Update_Statue()
    {
        while (true)
        {
            yield return new WaitForSeconds(3f);
 
            currentStatue.text = "";

            string folder = string.Format("{0}_{1}_{2}", BitgetAPI.dateTime.Year, BitgetAPI.dateTime.Month, BitgetAPI.dateTime.Day);
            string sn_contract = Files.READ_TXT(Files.DocumentsPath(folder + "/CT.txt"));
            string[] orderlow = sn_contract.Split(';');

            float contractCount = 0;
            float real = 0;
            for (int i = 0; i < orderlow.Length; i++)
            {
                string[] value = orderlow[i].Split(',');
                if (value.Length < 13) continue;

                contractCount++;
                OrderContract contract = new OrderContract();
                contract.start = new Order("", "", 1, "", "", "").SET_PRICE(value[3]);
                contract.eixte = new Order("", "", 1, "", "", "").SET_PRICE(value[4]);
                real += contract.GET_GAP_PRICE();
            }

            float unreal = 0;
            for (int i = 0; i < BitgetAPI.filledOrderDetails_OpenLong.Count; i++)
            {
                OrderDetail curOrder = BitgetAPI.filledOrderDetails_OpenLong[i];
                OrderContract contract = new OrderContract();
                contract.start = new Order("", "", 1, "", "", "").SET_PRICE(curOrder.price_avg);
                contract.eixte = new Order("", "", 1, "", "", "").SET_PRICE(BitgetAPI.curPrice_BTCUSDT.ToString());
                unreal += contract.GET_GAP_PRICE();
            }
            
            if (BitgetAPI.currentAccount.equity == null || BitgetAPI.startAccount.equity == null) continue;

            float accountGap = float.Parse(BitgetAPI.currentAccount.equity) - float.Parse(BitgetAPI.startAccount.equity);
            float gap = accountGap - (real + unreal);
            currentStatue.text += string.Format("[페어거래횟수:{0}] [실현손익{1:N2}] [미실현손익{2:N2}] [예상순이익:{3:N2}] [오차:{4:N2}]", contractCount, real, unreal, real + unreal, gap);

            DateTime curdate = SettingAPI.GET_DATE_TIMEST(BitgetAPI.startAccount.timestamp);
            account.text = string.Format("[{0}월{1}일{2}시{3}분{4}초] [시작잔고: {5:N5}]\n[업데이트:{6}] [{7}월{8}일{9}시{10}분{11}초] [현재잔고: {12:N5}]\n[차익 :{13:N5}]", curdate.Month, curdate.Day, curdate.Hour, curdate.Minute, curdate.Second,
                float.Parse(BitgetAPI.startAccount.equity), BitgetAPI.dateTime.Second, BitgetAPI.dateTime.Month, BitgetAPI.dateTime.Day, BitgetAPI.dateTime.Hour, BitgetAPI.dateTime.Minute, BitgetAPI.dateTime.Second,
                   float.Parse(BitgetAPI.currentAccount.equity), float.Parse(BitgetAPI.currentAccount.equity) - float.Parse(BitgetAPI.startAccount.equity));
        }

    }
}
