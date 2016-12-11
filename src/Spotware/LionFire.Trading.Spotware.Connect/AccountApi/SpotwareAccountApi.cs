﻿using LionFire.ExtensionMethods;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;


namespace LionFire.Trading.Spotware.Connect.AccountApi
{
    

    public static class SpotwareAccountApi
    {
        public static string SandboxUriRoot { get; } = "https://sandbox-api.spotware.com/";
        public static string UriRoot { get; } = "https://api.spotware.com/";

        public static string GetRoot(bool isSandbox) { return isSandbox ? SandboxUriRoot : UriRoot; }

        // requestTimeFrame: h1 or m1
        public const string TrendBarsUri = @"/connect/tradingaccounts/{id}/symbols/{symbolName}/trendbars/{requestTimeFrame}?access_token={oauth_token}&from={from}&to={to}";
        public const string PositionsUri = @"/connect/tradingaccounts/{id}/positions?access_token={access_token}&limit={limit}";
        public const string AllPositionsUri = @"/connect/tradingaccounts/{id}/deals?access_token={access_token}&limit={limit}";

        public static string ToSpotwareUriParameter(this DateTime time)
        {
            return time.ToString("yyyyMMddHHmmss");
        }

        public static DateTime ToDateTime(this long timestamp)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc) + TimeSpan.FromMilliseconds(timestamp);
        }

        public static HttpClient NewHttpClient(bool isSandbox = false)
        {
            var client = new HttpClient();
            AddHeaders(client);
            client.BaseAddress = new Uri(SpotwareAccountApi.GetRoot(isSandbox));
            return client;
        }


        public static void AddHeaders(HttpClient client)
        {
            // REVIEW unnecessary ones

            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/54.0.2840.71 Safari/537.36");
            client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
            client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.8,en-CA;q=0.6");
            client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, sdch, br");
            client.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");
            client.DefaultRequestHeaders.Add("Connection", "keep-alive");
            client.DefaultRequestHeaders.Add("Host", "api.spotware.com");
            client.DefaultRequestHeaders.Add("Cookie", "_ga=GA1.2.1217132727.1477434575");
        }

        #region Positions

        public async static Task<List<Position>> GetPositions(long accountId, string accessToken, IAccount market)
        {
            List<Position> Result = new List<Position>();

            var apiInfo = Defaults.Get<ISpotwareConnectAppInfo>();
            var client = NewHttpClient();

            var uri = SpotwareAccountApi.PositionsUri;
            uri = uri
                .Replace("{id}", accountId.ToString())
                .Replace("{access_token}", accessToken)
                .Replace("{limit}", "10000")  // HARDCODE REVIEW
                ;
            //UpdateProgress(0.11, "Sending request");
            var response = await client.GetAsyncWithRetries(uri);

            //UpdateProgress(0.12, "Receiving response");

            var receiveStream = await response.Content.ReadAsStreamAsync();
            System.IO.StreamReader readStream = new System.IO.StreamReader(receiveStream, System.Text.Encoding.UTF8);
            var json = readStream.ReadToEnd();

            //UpdateProgress(0.95, "Deserializing");
            //var error = Newtonsoft.Json.JsonConvert.DeserializeObject<SpotwareErrorContainer>(json);
            //if (error?.error != null)
            //{
            //    throw new Exception($"API returned error: {error.error.errorCode} - '{error.error.description}'");
            //}

            var data = Newtonsoft.Json.JsonConvert.DeserializeObject<SpotwarePositionsResult>(json);

            //UpdateProgress(0.98, "Processing data");

            if (data.data != null)
            {
                foreach (var pos in data.data)
                {
                    var position = new Position()
                    {
                        Id = pos.positionId,
                        EntryPrice = pos.entryPrice,
                        EntryTime = pos.entryTimestamp.ToDateTime(),
                        GrossProfit = pos.profit,
                        Label = pos.label,
                        Pips = pos.profitInPips,
                        StopLoss = pos.stopLoss,
                        Swap = pos.swap,
                        TakeProfit = pos.takeProfit,
                        TradeType = pos.tradeSide.ToTradeType(),
                        Volume = pos.volume,
                        SymbolCode = pos.symbolName,
                        Symbol = market.GetSymbol(pos.symbolName),
                        Commissions = pos.commission,
                        Comment = pos.comment,
                    };
                    
                    Result.Add(position);
                }
            }

            //UpdateProgress(1, "Done");
            return Result;
        }

        public static TradeType ToTradeType(this string tradeKind)
        {
            tradeKind = tradeKind.Trim('"');

            TradeType tradeType;
            if (tradeKind == "SELL")
            {
                tradeType = TradeType.Sell;
            }
            else if (tradeKind == "BUY")
            {
                tradeType = TradeType.Buy;
            }
            else
            {
                throw new Exception("Invalid tradeSide from server: " + tradeKind);
            }
            return tradeType;
        }

        public async static Task<List<Position>> GetHistoricalPositions(long accountId, string accessToken, IAccount market)
        {
            List<Position> Result = new List<Position>();

            var apiInfo = Defaults.Get<ISpotwareConnectAppInfo>();
            var client = NewHttpClient();

            var uri = SpotwareAccountApi.PositionsUri;
            uri = uri
                .Replace("{id}", accountId.ToString())
                .Replace("{access_token}", accessToken)
                .Replace("{limit}", "10000")  // HARDCODE REVIEW
                ;
            //UpdateProgress(0.11, "Sending request");
            var response = await client.GetAsync(uri);

            //UpdateProgress(0.12, "Receiving response");

            var receiveStream = await response.Content.ReadAsStreamAsync();
            System.IO.StreamReader readStream = new System.IO.StreamReader(receiveStream, System.Text.Encoding.UTF8);
            var json = readStream.ReadToEnd();

            //UpdateProgress(0.95, "Deserializing");
            //var error = Newtonsoft.Json.JsonConvert.DeserializeObject<SpotwareErrorContainer>(json);
            //if (error?.error != null)
            //{
            //    throw new Exception($"API returned error: {error.error.errorCode} - '{error.error.description}'");
            //}

            var data = Newtonsoft.Json.JsonConvert.DeserializeObject<SpotwareDealsResult>(json);

            //UpdateProgress(0.98, "Processing data");


            if (data.data != null)
            {
                foreach (var pos in data.data)
                {
                    TradeType tradeType;
                    var tradeSide = pos.tradeSide.Trim('"');
                    if (tradeSide == "SELL")
                    {
                        tradeType = TradeType.Sell;
                    }
                    else if (tradeSide == "BUY")
                    {
                        tradeType = TradeType.Buy;
                    }
                    else
                    {
                        throw new Exception("Invalid tradeSide from server: " + pos.tradeSide);
                    }
                    
                    var position = new Position()
                    {
                        Id = pos.positionId,
                        OrderId = pos.orderId,
                        DealId = pos.dealId,
                        TradeType = tradeType,
                        Volume = pos.volume,
                        FilledVolume = pos.filledVolume,
                        SymbolCode = pos.symbolName,
                        Symbol = market.GetSymbol(pos.symbolName),
                        Commissions = pos.commission,
                        EntryPrice = pos.executionPrice,
                        EntryTime = pos.executionTimestamp.ToDateTime(),
                        CreateTime = pos.createTimestamp.ToDateTime(),
                        Comment = pos.comment,
                    };

                    if (pos.positionCloseDetails != null)
                    {
                        position.CloseDetails = new PositionCloseDetails()
                        {
                            Balance = pos.positionCloseDetails.balance,
                            ClosedVolume = pos.positionCloseDetails.closedVolume,
                            Comment = pos.positionCloseDetails.comment,
                            Commission = pos.positionCloseDetails.commission,
                            EntryPrice = pos.positionCloseDetails.entryPrice,
                            Equity = pos.positionCloseDetails.equity,
                            EquityBasedRoi = pos.positionCloseDetails.equityBasedRoi,
                            Profit = pos.positionCloseDetails.profit,
                            ProfitInPips = pos.positionCloseDetails.profitInPips,
                            QuoteToDepositConversionRate = pos.positionCloseDetails.quoteToDepositConversionRate,
                            Roi = pos.positionCloseDetails.roi,
                            StopLossPrice = pos.positionCloseDetails.stopLossPrice,
                            Swap = pos.positionCloseDetails.swap,
                            TakeProfitPrice = pos.positionCloseDetails.takeProfitPrice,
                        };
                    }

                    Result.Add(position);
                }
            }

            //UpdateProgress(1, "Done");
            return Result;
        }

        private class SpotwarePositionsResult
        {
            public SpotwarePosition[] data { get; set; }
        }


        private class SpotwarePosition
        {
            public int positionId { get; set; }

            public long entryTimestamp { get; set; }
            public long utcLastUpdateTimestamp { get; set; }

            public string symbolName { get; set; }

            /// <summary>
            /// SELL or BUY
            /// </summary>
            public string tradeSide { get; set; }
            public TradeType TradeType
            {
                get
                {
                    if (tradeSide == "SELL")
                    {
                        return TradeType.Sell;
                    }
                    else if (tradeSide == "BUY")
                    {
                        return TradeType.Buy;
                    }
                    else
                    {
                        throw new Exception("Invalid tradeSide from server: " + tradeSide);
                    }
                }
            }

            public double entryPrice { get; set; }

            public int volume { get; set; }

            public double? stopLoss { get; set; }

            public double? takeProfit { get; set; }
            public double profit { get; set; }
            public double profitInPips { get; set; }

            public double commission { get; set; }
            public double marginRate { get; set; }
            public double swap { get; set; }
            public double currentPrice { get; set; }
            public string comment { get; set; }
            public string channel { get; set; }
            public string label { get; set; }
            
        }

        private class SpotwareDealsResult
        {
            public SpotwareDeal[] data { get; set; }
        }
        private class SpotwareDeal
        {
            public int dealId { get; set; }
            public int positionId { get; set; }
            public int orderId { get; set; }

            /// <summary>
            /// SELL or BUY
            /// </summary>
            public string tradeSide { get; set; }

            public int volume { get; set; }
            public int filledVolume { get; set; }

            public string symbolName { get; set;  }
            public double commission { get; set; }
            public double executionPrice { get; set; }
            public double baseToUsdConversionRate { get; set; }
            public double marginRate { get; set; }
            public string channel { get; set; }
            public string label { get; set; }
            public string comment { get; set; }

            public long createTimestamp { get; set; }
            public long executionTimestamp { get; set; }
            public SpotwarePositionCloseDetails positionCloseDetails { get; set; }

        }
        private class SpotwarePositionCloseDetails
        {
            public double entryPrice { get; set; }
            public double profit { get; set; }
            public double swap { get; set; }
            public double commission { get; set; }
            public long balance { get; set; }
            public int balanceVersion { get; set; }
            public string comment { get; set; }
            public double stopLossPrice { get; set; }
            public double takeProfitPrice { get; set; }

            public double quoteToDepositConversionRate { get; set; }
            public double closedVolume { get; set; }
            public double profitInPips { get; set; }
            public double roi { get; set; }
            public double equityBasedRoi { get; set; }
            public double equity { get; set; }
        }

        #endregion


    }

}
