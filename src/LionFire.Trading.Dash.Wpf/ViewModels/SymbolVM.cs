﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Trading
{

    public class SymbolVM : INotifyPropertyChanged
    {
        public Symbol Symbol { get; private set; }
        //public string Code { get { return Symbol?.Code; } }
        public IAccount Account { get; private set; }

        //public TimedBar LastMinuteBar { get; set; }
        //public DateTime LastMinuteBarTime
        //{
        //    get { return LastMinuteBarTime; }
        //}

        public TimedBar LastHourBar { get; set; }

        public SymbolVM(Symbol symbol, IAccount account)
        {
            this.Symbol = symbol;
            this.Name = symbol.Code;
            this.Account = account;
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            UpdateBidAsk();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        public async Task UpdateBidAsk()
        {
            this.Ask = Symbol.Ask;
            this.Bid = Symbol.Bid;
            if (double.IsNaN(Bid))
            {
                if (
                     //Symbol.Code == "XAUUSD" 
                     //Symbol.Code == "EURUSD"
                    //|| Symbol.Code == "USDJPY"
                    Symbol.Code == "GBPUSD"
                    )
                {
                    var bar = await Symbol.GetLastBar(TimeFrame.h1);
                    if (bar != null)
                    {
                        this.Bid = bar.Close;
                    }
                }
            }
        }

        public string Name { get; set; }


        #region Subscribed

        public bool Subscribed
        {
            get { return subscribed; }
            set
            {
                subscribed = value;
                if (subscribed)
                {
                    Symbol.Tick += Symbol_Tick;
                }
                else
                {
                    Symbol.Tick -= Symbol_Tick;
                }
            }
        }
        private bool subscribed;

        private void Symbol_Tick(SymbolTick obj)
        {
            if (!double.IsNaN(obj.Ask)) { this.Ask = obj.Ask; }
            if (!double.IsNaN(obj.Bid)) { this.Bid = obj.Bid; }
        }

        #endregion

        #region Bid

        public double Bid
        {
            get { return bid; }
            set
            {
                if (bid == value) return;
                bid = value;
                OnPropertyChanged(nameof(Bid));
            }
        }
        private double bid;

        #endregion

        #region Ask

        public double Ask
        {
            get { return ask; }
            set
            {
                if (ask == value) return;
                ask = value;
                OnPropertyChanged(nameof(Ask));
            }
        }
        private double ask;

        #endregion

        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            var ev = PropertyChanged;
            if (ev != null) ev(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

    }

}
