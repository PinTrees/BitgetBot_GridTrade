using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UI;

public class UI_PriceChart : UIView
{
    public TextMeshProUGUI symbol;
    public TextMeshProUGUI highPrice_24h;
    public TextMeshProUGUI lowPrice_24h;
    public TextMeshProUGUI price;

    public TextMeshProUGUI depth_low;
    public TextMeshProUGUI depth_lowPer;

    public Transform candleTr;
    public Transform priceTr;

    RectTransform candlePanel;
    List<CandleUI> candles;
    float defaultHeight = 0;

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

        candles = new List<CandleUI>();
        candlePanel = candleTr.GetComponent<RectTransform>();
        for (int i = 0; i < candleTr.childCount; i++)
            candles.Add(new CandleUI(candleTr.GetChild(i)));

        defaultHeight = 1000 / 100000;
    }
    public override void Show()
    {
        base.Show();
        if (fram != null) StopCoroutine(fram);
        fram = StartCoroutine(Update_Refresh());
        StartCoroutine(Update_Candle());
    }
    IEnumerator Update_Refresh()
    {
        while (true)
        {
            highPrice_24h.text = BitgetAPI.GET_MARKET_BTCUSDT(1) + " usdt";
            lowPrice_24h.text = BitgetAPI.GET_MARKET_BTCUSDT(2) + " usdt";
            price.text = BitgetAPI.GET_MARKET_BTCUSDT() + " usdt";
            yield return new WaitForSeconds(0.1f);
        }
        yield return null;
    }
    IEnumerator Update_Candle()
    {
        //yield return StartCoroutine(Market.Get_MARKET_CANDLE_DAY_BN());
        while (true)
        {
            //yield return StartCoroutine(Market.Get_MARKET_CANDLE_BN());

            float scale = 500 / (float)Market.Get_PriceHeight();
            candlePanel.localScale = new Vector2(1, 1);

            for (int i = 0; i < candles.Count; i++)
            {
                if (Market.candles.Count <= i)
                {
                    candles[i].gobject.gameObject.SetActive(false);
                    continue;
                }
                candles[i].Refresh(Market.candles[i], scale, Market.price_low);
            }
            yield return new WaitForSeconds(10f);
        }
    }
}
