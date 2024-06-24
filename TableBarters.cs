using System.Threading.Tasks;
using System;
using System.Globalization;
using CsvHelper;

namespace TarkovDataOrganizer;
public partial class TarkovData
{
    public class BarterOffer
    {
        //ItemId Barter FOR, List of barter offers
        public static Dictionary<string,List<BarterOffer>> DataTable;
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
           
            DataTable = new Dictionary<string, List<BarterOffer>>();
            foreach (var barterData in results.Data.barters)
            {
                var barterOffer = new BarterOffer
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

                var rewardItemIdsToAdd = new List<string>();
                foreach (var rewardItem in barterData.rewardItems)
                {
                    string itemId = rewardItem.item.id.ToString();
                    barterOffer.RewardItemIds += $"{rewardItem.item.id}|";
                    barterOffer.RewardItemNames += $"{rewardItem.item.name}|";
                    barterOffer.RewardItemCount += $"{rewardItem.count}|";
                    barterOffer.RewardItemQuantities += $"{rewardItem.quantity}|";
                    if(!DataTable.ContainsKey(itemId))
                        DataTable.Add(itemId,new List<BarterOffer>());
                    rewardItemIdsToAdd.Add(itemId);

                }
                // Remove the last | character
                barterOffer.RewardItemIds = barterOffer.RewardItemIds.TrimEnd('|');
                barterOffer.RewardItemNames = barterOffer.RewardItemNames.TrimEnd('|');
                barterOffer.RewardItemCount = barterOffer.RewardItemCount.TrimEnd('|');
                barterOffer.RewardItemQuantities = barterOffer.RewardItemQuantities.TrimEnd('|');

                foreach (var id in rewardItemIdsToAdd)
                    DataTable[id].Add(barterOffer);
            }
            
            Console.WriteLine("Done!");
        }
        
        public static void WriteToCsv(string _filename = "tempBarters.csv")
        {
            var barterList = new List<BarterOffer>();
            foreach (var entry in DataTable)
                barterList.AddRange(entry.Value);
            
            barterList = barterList.GroupBy(_offer => _offer.RewardItemIds)
                .Select(_group => _group.First()).ToList();
            
            using var writer = new StreamWriter(_filename);
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
            csv.WriteRecords(barterList);

            Console.WriteLine("Successfully wrote Barter data to '" + _filename + "'");

        }
    }


    
}
