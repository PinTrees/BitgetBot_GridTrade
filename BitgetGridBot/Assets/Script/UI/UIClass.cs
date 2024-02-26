using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UI
{
    public class OrderDetailUI
    {
        //"symbol":"cmt_btcusdt",    //Contract name
        //"size":"12",              //The amount in this order
        //"client_oid":"cmdtde",           //Client ID
        //"createTime":"1698475585258",   //Creation time
        //"filled_qty":"0",              //The amount which has been filled
        //"fee":"0",                    //Transaction fee
        //"order_id":"513468410013679613",    //Order ID
        //"price":"12",                      //The limit price of limit order 
        //"price_avg":"0",                  //Average filled price
        //"status":"-1",                   //Order status
        //"type":"1",                     //Commission type
        //"order_type":"0",              //Order type
        //"totalProfits":"253"          //Total profit and loss   

        public TextMeshProUGUI text;
        public Transform ob;
        public OrderDetailUI(Transform tr)
        {
            ob = tr;
            text = tr.GetChild(0).GetComponent<TextMeshProUGUI>();
            ob.gameObject.SetActive(false);
        }
        public void Display(string set)
        {
            text.text = set;
            ob.gameObject.SetActive(true);
        }
    }
    public class CandleUI
    {
        public Transform gobject;
        public RectTransform bodyTr;
        public RectTransform stickTr;
        public RawImage body;
        public RawImage stick;
        public CandleUI(Transform target)
        {
            gobject = target;
            bodyTr = target.GetChild(0).GetComponent<RectTransform>();
            body = target.GetChild(0).GetComponent<RawImage>();
            stickTr = target.GetChild(0).GetChild(0).GetComponent<RectTransform>();
            stick = target.GetChild(0).GetChild(0).GetComponent<RawImage>();
        }
        public void Refresh(Candle candle, float scale, float lowprice)
        {
            float stickHigh = candle.GetPriceHeight() * scale;
            if (stickHigh < 1) stickHigh = 1;
            float bodyHigh = candle.GetHigh() * scale;
            if (bodyHigh < 1) bodyHigh = 1;
            float body_pos = 0;

            if (candle.State() == 1)
            {
                stick.color = body.color = Color.red;

                float start_by = (candle.openPrice - lowprice);
                float exite_by = (candle.closePrice - lowprice);
                body_pos = (start_by + exite_by) * 0.5f * scale;
            }
            else if (candle.State() == 2)
            {
                stick.color = body.color = Color.blue;

                float start_by = (candle.closePrice - lowprice);
                float exite_by = (candle.openPrice - lowprice);
                body_pos = (start_by + exite_by) * 0.5f * scale;
            }

            float start_st = (candle.lowPrice - lowprice);
            float start_et = (candle.highPrice - lowprice);
            float candlePos = (start_st + start_et) * 0.5f * scale;
           
            float stick_pos = candlePos - body_pos;

            stickTr.sizeDelta = new Vector2(stickTr.rect.width, stickHigh);
            stickTr.anchoredPosition = new Vector2(0, stick_pos);
            bodyTr.sizeDelta = new Vector2(bodyTr.rect.width, bodyHigh);
            bodyTr.anchoredPosition = new Vector2(0, body_pos);

            //Debug.Log(stick_pos + " + " + body_pos + " + " + stickHigh);
            if (!gobject.gameObject.activeSelf) gobject.gameObject.SetActive(true);
        }
    }
}

public class Candle
{
    public int openPrice;
    public int closePrice;

    public float middlePrice;
    public float middlePrice_st;
    public int highPrice;
    public int lowPrice;

    public int volume;

    public Candle() { }
    public Candle(int price)
    {
        openPrice = price;
        closePrice = price;
        highPrice = price;
        lowPrice = price;
        volume = 0;
    }
    public Candle Initialize(string[] set)
    {
        openPrice = (int)float.Parse(set[0]);
        highPrice = (int)float.Parse(set[1]);
        lowPrice = (int)float.Parse(set[2]);
        closePrice = (int)float.Parse(set[3]);
        volume = (int)float.Parse(set[4]);

        middlePrice = (openPrice + closePrice) * 0.5f;
        middlePrice_st = (highPrice + lowPrice) * 0.5f;

        return this;
    }
    public Candle Initialize()
    {
        middlePrice = (openPrice + closePrice) * 0.5f;
        middlePrice_st = (highPrice + lowPrice) * 0.5f;

        return this;
    }
    public float GetPoss(int price, int unit)
    {
        float pos = middlePrice - price;
        if (pos == 0) return 0;
        else return (pos) / unit;
    }
    public float GetStickPos(int unit)
    {
        float pos = middlePrice_st - middlePrice;
        if (pos == 0) return 0;
        else return (pos) / unit;
    }
    public int GetHigh()
    {
        return Mathf.Abs(openPrice - closePrice);
    }
    public int GetPriceHeight()
    {
        return Mathf.Abs(highPrice - lowPrice);
    }
    public float GetWnidow(int price, int unit)
    {
        float pos = price - middlePrice_st;
        if (pos == 0) return 0;
        else return (pos) / unit;
    }
    public int State()
    {
        if (openPrice < closePrice) return 1;
        else if (openPrice > closePrice) return 2;
        else return 0;
    }
}

public class UIClass : MonoBehaviour
{
 
}
