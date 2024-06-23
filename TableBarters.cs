using System.Threading.Tasks;
using System;
using System.Globalization;
using CsvHelper;

namespace TarkovDataOrganizer;
public partial class TarkovData
{
    public class TraderBarterOffer
    {
        public static List<TraderBarterOffer> DataTable;
        public string Id { get; set; }
        public string Level { get; set; }

        public string TraderId { get; set; }
        public string TraderName { get; set; }
        public int BuyLimit { get; set; }

        // The required and reward items as string where each item and its properties are separated by |
        public string RequiredItemIds { get; set; }
        public string RequiredItemNames { get; set; }
        public string RequiredItemCount { get; set; }
        public string RequiredItemQuantities { get; set; }

        public string RewardItemIds { get; set; }
        public string RewardItemNames { get; set; }
        public string RewardItemCount { get; set; }
        public string RewardItemQuantities { get; set; }
        public DateTime TimestampLastDownload { get; set; }
        public DateTime TimestampLastChanged { get; set; }
        
        public static async Task DownloadTable()
        {
            Console.WriteLine("Downloading Barter Data...");
            var results = await GraphQueries.QueryTarkovAPI(GraphQueries.QUERY_BARTERS);
           
            DataTable = new List<TraderBarterOffer>();
            foreach (var barterData in results.Data.barters)
            {
                var barterOffer = new TraderBarterOffer
                {
                    Id = barterData.id,
                    Level = barterData.level,
                    TraderId = barterData.trader.id,
                    TraderName = barterData.trader.name,
                    BuyLimit = barterData.buyLimit,
                    TimestampLastDownload = DateTime.Now,
                    TimestampLastChanged = DateTime.Now
                };

                foreach (var requiredItem in barterData.requiredItems)
                {
                    barterOffer.RequiredItemIds += $"{requiredItem.item.id}|";
                    barterOffer.RequiredItemNames += $"{requiredItem.item.name}|";
                    barterOffer.RequiredItemCount += $"{requiredItem.count}|";
                    barterOffer.RequiredItemQuantities += $"{requiredItem.quantity}|";
                }
                // Remove the last | character
                barterOffer.RequiredItemIds = barterOffer.RequiredItemIds.TrimEnd('|');
                barterOffer.RequiredItemNames = barterOffer.RequiredItemNames.TrimEnd('|');
                barterOffer.RequiredItemCount = barterOffer.RequiredItemCount.TrimEnd('|');
                barterOffer.RequiredItemQuantities = barterOffer.RequiredItemQuantities.TrimEnd('|');

                foreach (var rewardItem in barterData.rewardItems)
                {
                    barterOffer.RewardItemIds += $"{rewardItem.item.id}|";
                    barterOffer.RewardItemNames += $"{rewardItem.item.name}|";
                    barterOffer.RewardItemCount += $"{rewardItem.count}|";
                    barterOffer.RewardItemQuantities += $"{rewardItem.quantity}|";
                }
                // Remove the last | character
                barterOffer.RewardItemIds = barterOffer.RewardItemIds.TrimEnd('|');
                barterOffer.RewardItemNames = barterOffer.RewardItemNames.TrimEnd('|');
                barterOffer.RewardItemCount = barterOffer.RewardItemCount.TrimEnd('|');
                barterOffer.RewardItemQuantities = barterOffer.RewardItemQuantities.TrimEnd('|');

                DataTable.Add(barterOffer);
            }
            
            Console.WriteLine("Done!");
        }
        
        public static void WriteToCsv(string _filename = "tempBarters.csv")
        {
            using var writer = new StreamWriter(_filename);
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
            csv.WriteRecords(DataTable);

            Console.WriteLine("Successfully wrote Barter data to '" + _filename + "'");

        }
    }


    
}
