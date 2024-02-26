using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace JsonData
{
    [System.Serializable]
    public class BitgetTime
    {
        //"epoch":"1591099099.896",
        //"iso":"2020-06-02T11:58:19.896Z",
        //"timestamp":1591099099896
        public string epoch;
        public string iso;
        public long timestamp;
    }
    [System.Serializable]
    public class Market
    {
        //"symbol":"cmt_btcusdt",   //Contract name 
        //"best_ask":"8858.0",     //Best ask price
        //"best_bid":"7466.0",    //Best bid price   
        //"high_24h":"8858",     //Highest price in the past 24 hours
        //"last":"8858",        //Last traded price  
        //"low_24h":"8858",    //Lowest price in the past 24 hours
        //"timestamp":"1591252726275",  //System Timestamp
        //"volume_24h":"0",   //Trading volume of past 24 hours
        //"priceChangePercent": "-0.63"  //24-hour price change percentage
        public string symbol;
        public string best_ask;
        public string best_bid;
        public string high_24h;
        public string low_24h;
        public string last;
        public string volume_24h;
        public string priceChangePercent;
        public string timestamp;
    }
    [System.Serializable]
    public class MarketPrice
    {
        //"symbol":"cmt_btcusdt",       //Contract name
        //"mark_price":"8047.87",      //Specify the margin price
        //"timestamp":"1591264230941"   //System Timestamp
        public string symbol;
        public string mark_price;
        public string timestamp;
    }
    [System.Serializable]
    public class OrderDetail
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
        public string symbol;

        public string size;
        public string totalProfits;
        public float filled_qty;
        public string fee;
     
        public string price;
        public string price_avg;

        public string status;
        public string type;
        public string order_type;

        public string client_oid;
        public string order_id;
        public string createTime;
        public OrderDetail() { symbol = null; }
        public OrderDetail(Dictionary<string, string> set)
        {
            symbol = set["symbol"];
            size = set["size"];
            fee = set["fee"];
            price = set["price"];
            price_avg = set["price_avg"];
            type = set["type"];
            client_oid = set["client_oid"];
            order_id = set["order_id"];
            createTime = set["createTime"];
        }
        public string GetText()
        {
            string sn = "";
            sn += "CID:" + client_oid + "   ";
            sn += "\t" + symbol.ToUpper() + "   ";
            sn += string.Format("{0:0.####}   ", double.Parse(fee));
            sn += string.Format("{0:N1}   ", float.Parse(price_avg));
            if (type == "1") sn += "<color=#00AE61>매수오픈</color>   ";
            else if (type == "2") sn += "<color=#FF3946>매도오픈</color>   ";
            else if (type == "3") sn += "<color=#FF3946>매수종료</color>   ";
            else if (type == "4") sn += "<color=#00AE61>매도종료</color>   ";

            if (status == "-1") sn += "<color=#FF3946>취소됨</color>";
            else if(status == "0") sn += "게시됨";
            else if(status == "1") sn += "부분 완성";
            else if(status == "2") sn += "<color=#00AE61>완성됨</color>";

            double qp = double.Parse(totalProfits);
            if (qp > 0) sn += string.Format("   총 손익:<color=#00AE61>{0:0.####}</color>", qp);
            else if (qp < 0) sn += string.Format("   총 손익:<color=#FF3946>{0:0.####}</color>", qp);
            return sn;
        }
        public OrderDetail Instance(OrderDetail set)
        {
            symbol = set.symbol;

            status = set.status;
            type = set.type;
            order_type = set.order_type;

            size = set.size;
            fee = set.fee;
            filled_qty = set.filled_qty;
            totalProfits = set.totalProfits;

            price = set.price;
            price_avg = set.price_avg;

            order_id = set.order_id;
            client_oid = set.client_oid;
            createTime = set.createTime;
            return this;
        }
        public string GetInfo()
        {
            string result = "";
            result += string.Format("[설정가:{0:N0}]", float.Parse(price));
            result += string.Format(" [진입가:{0:N0}]", float.Parse(price_avg));
            result += string.Format(" [규모:{0}]", size);
            result += string.Format(" [수수료:{0:N3}]", double.Parse(fee));
            result += " [상태:" + status + "]";
            result += " [방향:" + type + "]";
            result += " [설정번호:" + client_oid + "]";
            result += " [주문번호:" + order_id + "]";
            DateTime curTime = SettingAPI.GET_DATE_TIMEST(createTime);
            result += string.Format(" [주문시각:{0}시{1}분{2}초]", curTime.Hour, curTime.Minute, curTime.Second); 

            return result;
        }
    }
    [System.Serializable]
    public class BatchOrder
    {
        public string symbol;
        public List<Order> orderDataList;

        public BatchOrder() { orderDataList = new List<Order>(); }
        public BatchOrder SET_Symbol(string _symbol)
        {
            symbol = _symbol;
            return this;
        }
        public BatchOrder SET_OrderList(List<Order> order)
        {
            orderDataList.Clear();
            orderDataList.AddRange(order);
            return this;
        }
        public BatchOrder SET_OrderList(List<float> price, int type)
        {
            orderDataList = new List<Order>();
            for (int i = 0; i < price.Count; i++)
            {
                int curPrice = Mathf.RoundToInt(price[i]);
                orderDataList.Add(new Order("cmt_btcusdt", SettingAPI.GET_CLIENT_OID(12), 1, type.ToString(), "0", "0").SET_PRICE(curPrice.ToString()));
                //orderDataList.Add(new BOrder(SettingAPI.GET_CLIENT_OID(12), 1, type.ToString(), "0", "0").SET_PRICE(price[i].ToString()));
            }
            return this;
        }
    }
    [System.Serializable]
    public class OrderId
    {
        //symbol String  Yes Contract name
        //orderId String Yes order id
        public string symbol;
        public string orderId;
        public OrderId(string _symbol, string _orderId)
        {
            symbol = _symbol; orderId = _orderId;
        }
        public string GetQuery()
        {
            string ret = "";
            ret += "symbol=" + symbol + "&";
            ret += "orderId=" + orderId;
            return ret;
        }
    }
    [System.Serializable]
    public class OrderIds
    {
        //symbol String  Yes Contract name
        //orderId String Yes order id
        public string symbol;
        public List<string> ids;
        public OrderIds() { ids = new List<string>(); }
    }
    [System.Serializable]
    public class OrderResultBatch
    {
        public bool result;
        public List<OrderResult> order_info;
    }
    [System.Serializable]
    public class OrderResult
    {
        //result Result of an Order
        //client_oid  Client ID
        //order_id Order ID
        public bool result;
        public string client_oid;
        public string order_id;
    }
    [System.Serializable]
    public class Order
    {
        public string symbol;
        public string size;
        public string type;
        public string match_price;
        public string order_type;
        public string client_oid;
        public string price;
        //public string presetTakeProfitPrice;
        //public string presetStopLossPrice;
        public Order() { }
        public Order(string symbol, string client_oid, int size, string type, string order_type, string match_price)
        {
            this.symbol = symbol;
            this.client_oid = client_oid;
            this.size = size.ToString();
            this.type = type;
            this.order_type = order_type;
            this.match_price = match_price;
        }
        public Order SET_PRICE(string price) { this.price = price;  return this; }
    }
    [System.Serializable]
    public class Contract
    {
        public int size;
        public float average_price;
        public float payoff;
        public void FilledOrder(Order order)
        {
            if (order.type == "1") {
                average_price = (average_price * size + float.Parse(order.price)) / (size + int.Parse(order.size));
                size += int.Parse(order.size);
            }
            else if(order.type == "3")
            {
                average_price = (average_price * size - float.Parse(order.price)) / (size - int.Parse(order.size));
                size -= int.Parse(order.size);

                if (size <= 0) average_price = 0;
            }
        }
        public void ADD_PAYOFF(float pay)
        {
            payoff += pay;
        }
        public float GET_UNREALIZE_PNL(float price)
        {
            float buy = average_price * (0.001f * size);
            float sell = price * (0.001f * size);

            float per = (sell - buy) / buy;
            float gap = per * buy;
            Debug.Log("price_avr:" + average_price + " sell:" + sell + " buy:" + buy + " gap:" + gap);
            //gap = (0.5f / 100) * buy;
            gap -= (0.02f / 100) * buy;
            gap -= (0.02f / 100) * sell;

            return gap;
        }
    }
    [System.Serializable]
    public class OrderContract
    {
        public Order start;
        public Order eixte;
        public float gap;
        public OrderContract SET_ORDER(Order start, Order exite)
        {
            this.start = start; this.eixte = exite;
            return this;
        }
        public float GET_GAP_PRICE()
        {
            float buy = float.Parse(start.price) * (0.001f * int.Parse(start.size));
            float sell = float.Parse(eixte.price) * (0.001f * int.Parse(eixte.size));

            float per = (sell - buy) / buy;
            float gap = per * buy;
            gap -= (0.02f / 100) * buy;
            gap -= (0.02f / 100) * sell;

            return gap;
        }
    }
    public class SecurityUtil
    {
        #region Base64
        public static string Base64Encode(string data)
        {
            try
            {
                byte[] encData_byte = new byte[data.Length];
                encData_byte = System.Text.Encoding.UTF8.GetBytes(data);
                string encodedData = Convert.ToBase64String(encData_byte);
                return encodedData;
            }
            catch (Exception e)
            {
                throw new Exception("Error in Base64Encode: " + e.Message);
            }
        }
        public static string Base64Decode(string data)
        {
            try
            {
                System.Text.UTF8Encoding encoder = new System.Text.UTF8Encoding();
                System.Text.Decoder utf8Decode = encoder.GetDecoder();

                byte[] todecode_byte = Convert.FromBase64String(data);
                int charCount = utf8Decode.GetCharCount(todecode_byte, 0, todecode_byte.Length);
                char[] decoded_char = new char[charCount];
                utf8Decode.GetChars(todecode_byte, 0, todecode_byte.Length, decoded_char, 0);
                string result = new String(decoded_char);
                return result;
            }
            catch (Exception e)
            {
                throw new Exception("Error in Base64Decode: " + e.Message);
            }
        }
        #endregion
    }
}
public class JsonClass
{
}
