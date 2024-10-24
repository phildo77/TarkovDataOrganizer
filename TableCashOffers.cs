﻿
using System.Globalization;
using System.Reflection;
using CsvHelper;

namespace TarkovDataOrganizer;



public partial class TarkovData
{
    public class CashOffer
    {
        
        public static Dictionary<string, List<CashOffer>> DataTable;
        
        public string TraderName { get; set; }
        
        public string ItemId { get; set; }
        public string ItemName { get; set; }
        public int MinTraderLevel { get; set; }
        public int Price { get; set; }
        public string Currency { get; set; }
        public int PriceRUB { get; set; }
        public int BuyLimitCount { get; set; }
        public string TaskUnlockName { get; set; }
        public int TaskUnlockPlayerLevel { get; set; }
        public string TaskUnlockTraderId { get; set; }
        public string TaskUnlockId { get; set; }
        
        public DateTime TimestampLastDownload { get; set; }
        public DateTime TimestampLastChanged { get; set; }  
        
        public static async Task DownloadTable()
        {
            Console.WriteLine("Downloading CashOffer Data...");


            var graphData = await GraphQueries.QueryTarkovAPI(GraphQueries.QUERY_TRADER_CASH_OFFERS);

            DataTable = new Dictionary<string, List<CashOffer>>();

            foreach (var traderData in graphData.Data.traders)
            {
                var traderName = traderData.name;
                foreach (var cashOfferData in traderData.cashOffers)
                {
                    var cashOffer = new CashOffer();
                    cashOffer.TraderName = traderName;
                
                    cashOffer.ItemId = cashOfferData.item.id;
                    cashOffer.ItemName = cashOfferData.item.name;
                    cashOffer.MinTraderLevel = cashOfferData.minTraderLevel;
                    cashOffer.Price = cashOfferData.price;
                    cashOffer.PriceRUB = cashOfferData.priceRUB;
                    cashOffer.Currency = cashOfferData.currency;
                    if (cashOfferData.taskUnlock != null)
                    {
                        cashOffer.TaskUnlockId = cashOfferData.taskUnlock.id;
                        cashOffer.TaskUnlockName = cashOfferData.taskUnlock.name;
                        cashOffer.TaskUnlockPlayerLevel = cashOfferData.taskUnlock.minPlayerLevel;
                        cashOffer.TaskUnlockTraderId = cashOfferData.taskUnlock.trader.id;
                    }
                
                    cashOffer.BuyLimitCount = cashOfferData.buyLimit;
                
                    cashOffer.TimestampLastDownload = DateTime.Now;
                    cashOffer.TimestampLastChanged = DateTime.Now;
                    if(!DataTable.ContainsKey(cashOffer.ItemId))
                        DataTable.Add(cashOffer.ItemId, new List<CashOffer>());
                    DataTable[cashOffer.ItemId].Add(cashOffer);
                
                }
            }

            Console.WriteLine("Done!");
        }

        public static void WriteToCsv(string _filename = "tempCashOffers.csv")
        {
            var cashOfferList = new List<CashOffer>();
            foreach (var cashOfferEntry in DataTable)
                cashOfferList.AddRange(cashOfferEntry.Value);
            
            using var writer = new StreamWriter(_filename);
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
            csv.WriteRecords(cashOfferList);
            
            
            Console.WriteLine("Successfully wrote CashOffer data to '" + _filename + "'");
        }
        
        
    }
    

}




