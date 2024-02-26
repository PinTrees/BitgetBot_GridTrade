using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UI;
using System;

public class UI_CandleChart : UIView
{
    public Transform candleObject;
    public Transform candleTr;

    RectTransform candlePanel;
    List<CandleUI> candleUIs;

    Coroutine fram;
    void Start()
    {
        Init();
        Show();
    }
    public override void Init()
    {
        base.Init();
        if (fram != null) StopCoroutine(fram);
        fram = null;

        candleUIs = new List<CandleUI>();
        candlePanel = candleTr.GetComponent<RectTransform>();
    }
    public override void Show()
    {
        base.Show();
        if (fram != null) StopCoroutine(fram);
        StartCoroutine(Update_Candle());
    }
    IEnumerator Update_Candle()
    {
        var start = new DateTime(2018, 01, 01, 0, 0, 0);
        var exite = new DateTime(2018, 01, 02, 0, 0, 0);
        var candle = new List<Candle>();
        yield return StartCoroutine(Market.Get_BACKTEST_CANDLE_BITMEX_MINIT(start, exite, candle));
        Debug.Log("ddddddddddddddddddddddddddddddddddddddd");
        while (true)
        {
            float scale = 880 / (float)Market.Get_PriceHeight(candle);
            var lowPrice = Market.Get_PriceLow(candle);

            candlePanel.localScale = new Vector2(1, 1);

            for(int i = 0; i < candle.Count; i++)
            {
                var cd = GameObject.Instantiate(candleObject);
                cd.SetParent(candleTr);
                cd.localScale = Vector2.one;
                var curCandle = new CandleUI(cd);
                candleUIs.Add(curCandle);
                curCandle.Refresh(candle[i], scale, lowPrice);
                yield return new WaitForEndOfFrame();
            }
            yield break;
        }
    }
}
