using System.Collections;
using System.Data;
using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;

namespace TarkovDataOrganizer;

public partial class TarkovData
{
    
    
    public class CombinationExplorer
    {

        public static List<List<string>> GetAllCombinationsForItemId(string _itemId)
        {
            
            var rootNode = Node.Build(_itemId);
            var allCombos = new List<List<string>>();

            Console.WriteLine("Finding All combinations for " + rootNode.ItemName);
            
            do
                allCombos.Add(rootNode.GetConfig());
            //while (rootNode.MoveNext());
            while (rootNode.MoveNext() && allCombos.Count <= 1000);
            
            Console.WriteLine("Found " + allCombos.Count + " Combinations!");
            
            //Test uniqueness
            for (int idx = 0; idx < allCombos.Count(); ++idx)
            {
                var checkSet = new HashSet<string>(allCombos[idx]);
                for (int toIdx = idx + 1; toIdx < allCombos.Count(); ++toIdx)
                {
                    var toSet = new HashSet<string>(allCombos[toIdx]);
                    if (checkSet.SetEquals(toSet))
                    {
                        if(allCombos[idx].Count == allCombos[toIdx].Count)
                            Console.WriteLine("Found Identical combos at idx " + idx + " and " + toIdx);
                    }
                }
            }
            return allCombos;
        }
        

        public class Node : IEnumerator<string>
        {
            public const int INDEX_EMPTY = -1;
            public const string CURRENT_EMPTY = "Empty";
            public static readonly List<string> SLOTS_TO_IGNORE = ["Scope", "Magazine", "Tactical"];

            private Node() { }
            public Node(Node _parent, string _slotId)
            {
                Parent = _parent;
                SlotId = _slotId;
                if (EmptyAllowed)
                    CurrentIndex = INDEX_EMPTY;
                else
                    CurrentIndex = 0;
            }

            public readonly Node Parent;
            public readonly string SlotId;
            public List<Node> Children = [];
            
            public int CurrentIndex;
            public int CurrentChildIndex = 0;
            
            public string ItemName => IsEmpty ? CURRENT_EMPTY : TarkovItem.DataTable[Current].name;
            public virtual string SlotType => TarkovItem.DataTableSlots[Parent.Current][SlotId].name;
            public virtual List<string> AllowedIds
            {
                get
                {
                    return IsEmpty ? [] : TarkovItem.DataTableSlots[Parent.Current][SlotId].allowedIDs;
                    /*
                    if (IsEmpty)
                        return [];
                    var slot = TarkovItem.DataTableSlots[Parent.Current][SlotId];
                    if (SLOTS_TO_IGNORE.Any(_cat => slot.name.Trim().ToLower().Contains(_cat.Trim().ToLower())))
                        return [];
                    if(slot.name.Equals("Tactical"))
                        Console.WriteLine("I FOUND YOU");
                    return
                        TarkovItem.DataTableSlots[Parent.Current][SlotId].allowedIDs;
                        */
                }
            }

            public virtual bool EmptyAllowed => !TarkovItem.DataTableSlots[Parent.Current][SlotId].required;
            public bool IsEmpty => CurrentIndex == INDEX_EMPTY;

            public List<string> GetConfig()
            {
                if (IsEmpty)
                    return new List<string>();
                
                var config = new List<string>() { AllowedIds[CurrentIndex] };
                foreach (var child in Children)
                    config.AddRange(child.GetConfig());
                return config;
            }

            private void ResetPreviousChildren()
            {
                for (int childIdx = 0; childIdx < CurrentChildIndex - 1; ++childIdx)
                    Children[childIdx].Reset();
                CurrentChildIndex = 0;

            }

            public bool MoveNext()
            {
                if (IsEmpty)
                {
                    CurrentIndex = 0;
                    BuilderRecurseChildren(this);
                    //return AllowedIds.Count != 0;
                    return true; //Assuming that every slot has at least one possible mod.
                }

                while (CurrentChildIndex < Children.Count)
                {
                    if (Children[CurrentChildIndex].MoveNext())
                    {
                        ResetPreviousChildren();
                        return true;
                    }
                    CurrentChildIndex++;
                }
                
                if (++CurrentIndex >= AllowedIds.Count)
                {
                    CurrentIndex--;
                    return false;
                }

                BuilderRecurseChildren(this);
                return true;
            }

            public void Reset()
            {
                CurrentIndex = EmptyAllowed ? INDEX_EMPTY : 0;
                BuilderRecurseChildren(this);
                    
            }

            public string Current =>  
                IsEmpty ? CURRENT_EMPTY : AllowedIds[CurrentIndex];

            object IEnumerator.Current => Current;

            public void Dispose() { }
            
            private class Root(string _itemId) : Node
            {
                public override List<string> AllowedIds => [_itemId];
                public override bool EmptyAllowed => false;
                public override string SlotType => "Gun";
            }

            public static Node Build(string _itemId)
            {
                var root = new Root(_itemId);
                BuilderRecurseChildren(root);
                return root;
            }

            private static void BuilderRecurseChildren(Node _node)
            {
                _node.Children = new List<Node>();
                _node.CurrentChildIndex = 0;
                if (_node.IsEmpty) return;
                foreach (var childSlot in TarkovItem.DataTableSlots[_node.Current])
                {
                    //Ignore Scopes, Magazines and Tacticals?
                    /*
                    if (childSlot.Value.name.Contains("Scope") ||
                        childSlot.Value.name.Contains("Magazine") ||
                        childSlot.Value.name.Equals("Comb. tact. device"))
                        continue;*/
                    if (SLOTS_TO_IGNORE.Any(_cat => childSlot.Value.name.ToLower().Contains(_cat.ToLower())))
                        continue;
                    var childNode = new Node(_node, childSlot.Key);
                    _node.Children.Add(childNode);
                    BuilderRecurseChildren(childNode);
                        
                }

                
            }
        }


        public static class Report
        {
            public static string Verbose(List<string> _comboIds)
            {
                var dataList = new List<ReportData>();
                foreach (var id in _comboIds)
                {
                    dataList.Add(new ReportData(id));
                }

                return TarkovHelper.WriteCsvToString(dataList);
            }

            public static string MultiVerbose(List<List<string>> _multipleCombos)
            {
                Console.WriteLine("Generating CSV string for report...");
                var sb = new StringBuilder();

                for (var index = 0; index < _multipleCombos.Count; index++)
                {
                    sb.AppendLine(index.ToString());
                    var combo = _multipleCombos[index];
                    sb.AppendLine(Verbose(combo));
                    sb.AppendLine();
                    sb.AppendLine();

                    Console.Write(".");
                }

                Console.WriteLine("Done!");
                return sb.ToString();
            }

            
        }
        private class ReportData
        {
            public ReportData(string _itemId)
            {
                var item = TarkovItem.DataTable[_itemId];
                //TODO ParentName, ParentSlotType 
                ItemId = item.id;
                ItemName = item.name;
                CategoryName = item.categoryName;
                Weight = item.weight;
                ErgonomicsModifier = item.ergonomicsModifier;
                AccuracyModifier = item.accuracyModifier;
                RecoilModifier = item.recoilModifier;
                Loudness = item.loudness;
                Velocity = item.velocity;

                if (item.types.Contains("gun"))
                {
                    Ergonomics = item.ergonomics;
                    FireRate = item.fireRate;
                    RecoilHorizontal = item.recoilHorizontal;
                    RecoilVertical = item.recoilVertical;
                }
                else if(item.types.Contains("barrel"))
                {
                    CenterOfImpact = item.centerOfImpact;
                    DeviationCurve = item.deviationCurve;
                    DeviationMax = item.deviationMax;
                }

                Avg24hPrice = item.avg24hPrice;
                Low24hPrice = item.low24hPrice;
                LastLowPrice = item.lastLowPrice;

                //TODO add switches/filters for minimum Trader level and tasks completed
                if (CashOffer.DataTable.ContainsKey(item.id))
                {
                    var cashOffer = CashOffer.DataTable[item.id];
                    LowestTraderPriceRUB = int.MaxValue;
                    foreach (var offer in cashOffer)
                    {
                        if (offer.PriceRUB < LowestTraderPriceRUB)
                        {
                            LowestTraderPriceRUB = offer.PriceRUB;
                            LowestTraderPriceName = offer.TraderName;
                            LowestTraderPriceMinLevel = offer.MinTraderLevel;
                            LowestTraderPriceTaskUnlockName = offer.TaskUnlockName;
                        }
                    }
                }
                

                //TODO barters - need a better class structure (not just delimted strings)
                //TODO crafts
            }
            
            public string ParentName { get; set; }
            public string ParentSlotName { get; set; }
            public string ItemId { get; set; }
            public string ItemName { get; set; }
            public string CategoryName { get; set; }

            public float Weight { get; set; }
            public float ErgonomicsModifier { get; set; }
            public float AccuracyModifier { get; set; }
            public float RecoilModifier { get; set; }
            public float Loudness  { get; set; }
            public float Velocity  { get; set; }
            
            //Weapon Prop
            public float Ergonomics { get; set; }
            public float FireRate { get; set; }
            
            public float RecoilVertical { get; set; }
            public float RecoilHorizontal { get; set; }
            
            //Barrel Prop
            public float CenterOfImpact { get; set; }
            public float DeviationCurve { get; set; }
            public float DeviationMax { get; set; }
            
            // PRICING ------------------
            public int Avg24hPrice  { get; set; }
            public int Low24hPrice { get; set; }
            public int LastLowPrice { get; set; }
            
            //From CashOffers
            public int LowestTraderPriceRUB { get; set; }
            public string LowestTraderPriceName { get; set; }
            public int LowestTraderPriceMinLevel { get; set; }
            public string LowestTraderPriceTaskUnlockName { get; set; }
            
            //From Barters
            public int LowestBarterPriceRUB { get; set; }
            public int LowestBarterPriceName { get; set; }
            public int LowestBarterPriceTradeDescription { get; set; }
            
            
            
        }
    }
    
}