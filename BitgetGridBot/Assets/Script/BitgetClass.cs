using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JsonData;

namespace BitgetClassJson
{
    [System.Serializable]
    public class CloseOrderResult
    {
        //     "symbol":"cmt_btcusdt",      //Contract name
        //"result":true,              //processing result
        //"order_ids":[
        //       "258414711",   //Successful id
        //       "478585558"
        // ],
        //"fail_infos":[
        //       {
        //     "order_id":"258414711",   //Failed id
        //     "err_code":"401",        //Error code
        //     "err_msg":""            //Error message
        public string symbol;
        public bool result;
        public string[] order_ids;
        public List<ErrorInfo> fail_infos;

     }
    [System.Serializable]
    public class ErrorInfo
    {
        //"order_id":"258414711",   //Failed id
        //"err_code":"401",        //Error code
        //"err_msg":""            //Error message
        public string order_id;
        public string err_code;
        public string err_msg;
    }
    [System.Serializable]
    public class OrderContract_BT
    {
        public OrderDetail start;
        public OrderDetail eixte;
        public float gap;
        public OrderContract_BT SET_START_ORDER(OrderDetail start)
        {
            this.start = start; 
            return this;
        }
        public OrderContract_BT SET_EXITE_ORDER(OrderDetail eixte)
        {
            this.eixte = eixte;
            return this;
        }
        public float GET_GAP_PRICE()
        {
            if (start == null || eixte == null) return 999f;

            float buy = float.Parse(start.price) * (0.001f * 1);
            float sell = float.Parse(eixte.price) * (0.001f * 1);

            float per = (sell - buy) / buy;
            float gap = per * buy;
            gap -= (0.02f / 100) * buy;
            gap -= (0.02f / 100) * sell;

            return gap;
        }
    }
    [System.Serializable]
    public class MarketPosition
    {
        public string margin_mode;
        public List<Position> holding;
        public MarketPosition() { holding = new List<Position>(); }
        public MarketPosition Instance(MarketPosition set)
        {
            margin_mode = set.margin_mode;
            holding.Clear();
            holding.AddRange(set.holding);

            return this;
        }
        public string GetInfoText()
        {
            if (holding.Count <= 0) return "[포지션 미 보유] [마지막업데이트:" + BitgetAPI.dateTime.Second + "]";

            string resurlt = "";
            for (int i = 0; i < holding.Count; i++)
            {
                resurlt += holding[i].GetInfo() + "\n";
            }
            return resurlt;
        }
    }
    [System.Serializable]
    public class Position
    {
        //"symbol":"cmt_btcusdt",       //Contract name
        //"liquidation_price":"0.00",  //Estimated liquidation price
        //"position":"0",             //Position Margi(the margin for holding current positions)
        //"avail_position":"0",      //Available position
        //"avg_cost":"0.00",        //Transaction average price
        //"leverage":"2",          //Leverage
        //"realized_pnl":"0.00000000",    //Realized Profit and loss
        //"keepMarginRate":"0.005",      //Maintenance margin rate
        //"side":"1",                    //Position Direction Long or short     Mark obsolete
        //"holdSide":"1",                    //Position Direction Long or short
        //"timestamp":"1557571623963",  // System timestamp
        //"margin":"0.0000000000000000"   //Used margin
        //"unrealized_pnl":"0.00000000" //Unrealized profit and loss
        public string symbol;
        public string liquidation_price;
        public string position;
        public string avail_position;
        public string avg_cost;
        public string leverage;
        public string keepMarginRate;
        public string side;
        public string holdSide;
        public string timestamp;
        public string margin;
        public string realized_pnl;
        public string unrealized_pnl;

        public string GetInfo()
        {
            string result = "";
            result += "[레버리지:" + leverage + "]";
            result += " [청산가격:" + liquidation_price + "]";
            result += string.Format(" [거래평균:{0:N1}]", float.Parse(avg_cost));
            result += string.Format(" [마진:{0:N2}]", double.Parse(margin));
            result += string.Format(" [미실현손익:{0:N4}]", double.Parse(unrealized_pnl));
            result += string.Format(" [실현된손익:{0:N4}]", double.Parse(realized_pnl));
            result += " [증거금:" + position + "]";

            result += " [방향:" + side + "]";
            result += " [마지막업데이트:" + BitgetAPI.dateTime.Second + "]";

            return result;
        }
    }
    [System.Serializable]
    public class Account
    {
        //"symbol":"cmt_btcusdt",      //Contract name
        //"equity":"0.00000000",      //Equity of the account
        //"fixed_balance":"0.00000000",      //Obsolete field
        //"total_avail_balance":"0.00000000",   //Available Balance
        //"margin":"0",                        //Used margin
        //"realized_pnl":"0",                 //Realized profits and losses
        //"unrealized_pnl":"0",              //Unrealized profits and losses
        //"longMarginRatio":"0",            //Margin rate for multiple positions
        //"shortMarginRatio": "0",         //Margin rate for short positions
        //"marginRatio": "0",             //Whole position margin rate
        //"margin_frozen":"0",              //Freeze margin for opening positions
        //"timestamp":"1658098718494",     //Creation time
        //"margin_mode":"fixed",          //Margin Mode: crossed / fixed
        //"forwardContractFlag":true     //Is it a forward contract
        public string symbol;
        public string equity; //계좌의 자본
        public string fixed_balance; // 고정 잔액
        public string total_avail_balance; // 사용가능 잔액
        public string margin;
        public string realized_pnl;
        public string unrealized_pnl;
        public string longMarginRatio;
        public string shortMarginRatio;
        public string marginRatio;
        public string margin_frozen;
        public string timestamp;
        public string margin_mode;
        public string forwardContractFlag;
        public Account() {  }
        public Account Instance(Account set)
        {
            symbol = set.symbol;
            equity = set.equity;
            fixed_balance = set.fixed_balance;
            total_avail_balance = set.total_avail_balance;
            timestamp = set.timestamp;
            return this;
        }
    }
    [System.Serializable]
    public class Error
    {
        public bool error;
        public long code;
        public string messege;
        public Error Instance(Error set)
        {
            error = set.error;
            code = set.code;
            messege = set.messege;
            return this;
        }
        //public Error Append(Error set)
        //{
        //    if(!error) error = set.error;
        //    if(code == 200) code = set.code;
        //    messege = set.messege;
        //    return this;
        //}
        public Error SET_ERROR(bool set) { error = set; return this; }
    }
  
}
namespace Bitget
{
    namespace Price
    {
        [System.Serializable]
        public class PriceLimit
        {
            //"symbol":"cmt_btcusdt",    //Contract name
            //"forwardContractFlag":true,  //Is it a forward contract
            //"highest":"14474.5",        //Ceiling of buying price
            //"lowest":"4824.5",         //Floor of selling price
            //"timestamp":"1591257126461"   //System Timestamp
            public string symbol;
            public string forwardContractFlag;
            public string highest;
            public string lowest;
            public string timestamp;
            public PriceLimit Instance(PriceLimit set)
            {
                symbol = set.symbol;
                forwardContractFlag = set.forwardContractFlag;
                highest = set.highest;
                lowest = set.lowest;
                timestamp = set.timestamp;
                return this;
            }
        }
    }
    namespace Order
    {
        [System.Serializable]
        public class OrderId_Post
        {
            //"client_oid":"bitget#123456",      //Client  ID 
            //"order_id":"513466539039522813"   //Order ID 

            public string client_oid;
            public string order_id;
            public OrderId_Post Instace(OrderId_Post set)
            {
                client_oid = set.client_oid;
                order_id = set.order_id;
                return this;
            }
        }
    }
}
public class BitgetClass
{
 
}
