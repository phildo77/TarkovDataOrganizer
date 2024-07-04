using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;

namespace TarkovDataOrganizer;

public partial class TarkovData
{
    public class TarkovItem
    {
        public const string CATEGORY_NAME_BARREL = "Barrel";
        
        public static Dictionary<string,TarkovItem> DataTable;
        public static Dictionary<string, Dictionary<string,Slot>> DataTableSlots;

        public string id { get; set; }
        public string name { get; set; }
        public string shortName { get; set; }
        public string description { get; set; }
        public string categoryId { get; set; }
        public string categoryName { get; set; }
        public string types { get; set; } //Can contain multiple strings delim |
        public string updated { get; set; }  //Last time this data was updated or changed?

        public float weight { get; set; }
        public float ergonomicsModifier { get; set; }
        public float accuracyModifier { get; set; }
        public float recoilModifier { get; set; }
        public float loudness { get; set; }
        public float velocity { get; set; }

        public bool blocksHeadphones { get; set; }
        public string conflictingSlotIds { get; set; } //Can contain multiple strings delim |
        public string conflictingItemsIds { get; set; } //Can contain multiple strings delim |
        public string conflictingItemsNames { get; set; } //Can contain multiple strings delim |

        public bool hasGrid { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public string backgroundColor { get; set; }
        public string gridImageLink { get; set; }

        public string link { get; set; }
        public string iconLink { get; set; }
        public string inspectImageLink { get; set; }
        public string image8xLink { get; set; }
        public string image512pxLink { get; set; }
        public string baseImageLink { get; set; }
        public string wikiLink { get; set; }

        public int basePrice { get; set; }
        public int avg24hPrice { get; set; }
        public int low24hPrice { get; set; }
        public int lastLowPrice { get; set; }
        public int high24hPrice { get; set; }
        public int changeLast48h { get; set; }
        public float changeLast48hPercent { get; set; }
        public int lastOfferCount { get; set; }

        //ItemPropWeapon
        public string caliber { get; set; }
        public float ergonomics { get; set; }
        public float fireRate { get; set; }
        public string fireModes { get; set; } //Can contain multiple strings delim |

        public float recoilVertical { get; set; }
        public float recoilHorizontal { get; set; }

        public string defaultPresetId { get; set; }
        public string defaultPresetName { get; set; }
        public string defaultPresetGridImageLink { get; set; }
        public string defaultPresetLink { get; set; }
        public string defaultPresetIconLink { get; set; }
        public string defaultPresetInspectImageLink { get; set; }
        public string defaultPresetImage8xLink { get; set; }
        public string defaultPresetImage512pxLink { get; set; }
        public string defaultPresetBaseImageLink { get; set; }
        public string defaultPresetWikiLink { get; set; }

        public string presetIds { get; set; } //Multiple delim |

        //ItemPropWeaponMod
        //ItemPropBarrel
        //public int ergonomics;  //This is the same as ergonomicsModifier - not needed
        //public float recoilModifier; //This is the same as recoilModifier only / 100 - not needed
        public float centerOfImpact { get; set; }
        public float deviationCurve { get; set; }
        public float deviationMax { get; set; }

        //All items w/ props

        //public List<Slot> slots;  Making this a separate table - see DataTableSlots

        public class Slot
        {
            public string itemId { get; set; }  // Item having the associated slot info
            public string itemName { get; set; } //Redundant - for debug / checking / testing
            public string slotId { get; set; } //Used in DataTableSlots as secondary key
            public string nameId { get; set; } //Normalized slot type
            public bool required { get; set; } //Is the gun valid / fireable without this slot filled?
            public string name { get; set; } //Slot type (ie Pistol Grip, Scope, Barrel, etc.)
            public string allowedIDsStr { get; set; } // mutliple delim | ... For use in exporting to CSV
            public List<string> allowedIDs = new List<string>();  // For easier use getting all combos
            public string allowedCategories { get; set; } // mutliple delim | - Currently no data here?  FUture?
            public string excludedIDs { get; set; } // mutliple delim | - Currently no data here?  FUture?
            public string excludedCategories { get; set; } // mutliple delim | - Currently no data here?  FUture?
        }
        
        public static async Task DownloadTable(bool force = false)
        {
            Console.WriteLine("Downloading Item Data... (this might take a few seconds)");
            DataTable = new Dictionary<string,TarkovItem>();
            DataTableSlots = new Dictionary<string, Dictionary<string, Slot>>(); 

            var graphData = await GraphQueries.QueryTarkovAPI(GraphQueries.QUERY_ITEMS_ALL_GENERIC_INFO);

            foreach (var graphItem in graphData.Data.items)
            {
                var tItem = new TarkovItem();
                tItem.id = graphItem.id;
                tItem.name = graphItem.name;
                tItem.shortName = graphItem.shortName;
                tItem.description = graphItem.description;
                if(tItem.description != null)
                    tItem.description = tItem.description
                        .Replace(",", "`").Replace(System.Environment.NewLine," ")
                        .Replace("\n", " ").Replace("\r", " ")
                        .Replace("\r\n", " ").Replace("\n\r", " "); // TODO more elegant solution to commas in all strings
                tItem.categoryId = graphItem.category.id;
                tItem.categoryName = graphItem.category.name;
                tItem.types = string.Empty;
                foreach (var giType in graphItem.types)
                    tItem.types += giType + "|";
                tItem.types.TrimEnd('|');
                tItem.updated = graphItem.updated;

                tItem.weight = graphItem.weight;
                tItem.ergonomicsModifier = graphItem.Value<float?>("ergonomicsModifier") ?? 0f;
                tItem.accuracyModifier = graphItem.Value<float?>("accuracyModifier") ?? 0f;
                tItem.recoilModifier = graphItem.Value<float?>("recoilModifier") ?? 0f;
                tItem.loudness = graphItem.Value<float?>("loudness") ?? 0f;
                tItem.velocity = graphItem.Value<float?>("velocity") ?? 0f;

                tItem.blocksHeadphones = graphItem.blocksHeadphones == "true";
                tItem.conflictingSlotIds = string.Empty;
                foreach (var conSlotId in graphItem.conflictingSlotIds)
                    tItem.conflictingSlotIds += conSlotId + "|";
                tItem.conflictingSlotIds.TrimEnd('|');
                
                tItem.conflictingItemsIds = string.Empty;
                tItem.conflictingItemsNames = string.Empty;
                foreach (var conItem in graphItem.conflictingItems)
                {
                    tItem.conflictingItemsIds += conItem.id + "|";
                    tItem.conflictingItemsNames += conItem.name + "|";
                }
                tItem.conflictingItemsIds.TrimEnd('|');
                tItem.conflictingItemsNames.TrimEnd('|');

                tItem.hasGrid = graphItem.hasGrid == "true";
                tItem.width = graphItem.width;
                tItem.height = graphItem.height;
                tItem.backgroundColor = graphItem.backgroundColor;
                tItem.gridImageLink = graphItem.gridImageLink;

                tItem.link = graphItem.link;
                tItem.iconLink = graphItem.iconLink;
                tItem.inspectImageLink = graphItem.inspectImageLink;
                tItem.image8xLink = graphItem.image8xLink;
                tItem.image512pxLink = graphItem.image512pxLink;
                tItem.baseImageLink = graphItem.baseImageLink;
                tItem.wikiLink = graphItem.wikiLink;

                tItem.basePrice = graphItem.Value<int?>("basePrice") ?? 0;
                tItem.avg24hPrice = graphItem.Value<int?>("avg24hPrice") ?? 0;
                tItem.low24hPrice = graphItem.Value<int?>("low24hPrice") ?? 0;
                tItem.lastLowPrice = graphItem.Value<int?>("lastLowPrice") ?? 0;
                tItem.high24hPrice = graphItem.Value<int?>("high24hPrice") ?? 0;
                tItem.changeLast48h = graphItem.Value<int?>("changeLast48h") ?? 0;
                tItem.changeLast48hPercent = graphItem.Value<float?>("changeLast48hPercent") ?? 0f;
                tItem.lastOfferCount = graphItem.Value<int?>("lastOfferCount") ?? 0;
                
                
                
                if (HasValidValue(graphItem, "properties"))
                {
                    //Item Properties Weapon
                    if (tItem.types.Contains("gun")) //TODO const 
                    {
                        tItem.caliber = graphItem.properties.caliber;
                        tItem.ergonomics = graphItem.properties.ergonomics;
                        tItem.fireRate = graphItem.properties.fireRate;
                        tItem.fireModes = string.Empty;
                        foreach (var fireMode in graphItem.properties.fireModes)
                            tItem.fireModes += fireMode + "|";
                        tItem.recoilVertical = graphItem.properties.Value<float?>("recoilVertical") ?? 0f;
                        tItem.recoilHorizontal = graphItem.properties.Value<float?>("recoilHorizontal") ?? 0f;

                        if (HasValidValue(graphItem.properties, "defaultPreset"))
                        {
                            tItem.defaultPresetId = graphItem.properties.defaultPreset.id;
                            tItem.defaultPresetName = graphItem.properties.defaultPreset.name;
                            tItem.defaultPresetGridImageLink = graphItem.properties.defaultPreset.gridImageLink;
                            tItem.defaultPresetLink = graphItem.properties.defaultPreset.link;
                            tItem.defaultPresetIconLink = graphItem.properties.defaultPreset.iconLink;
                            tItem.defaultPresetInspectImageLink = graphItem.properties.defaultPreset.inspectImageLink;
                            tItem.defaultPresetImage8xLink = graphItem.properties.defaultPreset.image8xLink;
                            tItem.defaultPresetImage512pxLink = graphItem.properties.defaultPreset.image512pxLink;
                            tItem.defaultPresetBaseImageLink = graphItem.properties.defaultPreset.baseImageLink;
                            tItem.defaultPresetWikiLink = graphItem.properties.defaultPreset.wikiLink;
                        }
                            
                        
                        tItem.presetIds = string.Empty;
                        if (HasValidValue(graphItem.properties, "presets"))
                            foreach (var preset in graphItem.properties.presets)
                                tItem.presetIds += preset.id + "|";
                        tItem.presetIds.TrimEnd('|');

                        DataTableSlots.Add(tItem.id, new Dictionary<string, Slot>());
                        foreach (var slot in graphItem.properties.slots)
                        {
                            var newSlot = new Slot();
                            newSlot.itemId = tItem.id;
                            newSlot.itemName = tItem.name;
                            newSlot.slotId = slot.id;
                            newSlot.nameId = slot.nameId;
                            newSlot.required = slot.required == "true";
                            newSlot.name = slot.name;
                            foreach (var allowedItem in slot.filters.allowedItems)
                            {
                                newSlot.allowedIDsStr += allowedItem.id + "|";
                                newSlot.allowedIDs.Add(allowedItem.id.ToString());
                            }
                                

                            foreach (var excludedItem in slot.filters.excludedItems)
                                newSlot.excludedIDs += excludedItem.id + "|";

                            foreach (var allowedCategory in slot.filters.allowedCategories)
                                newSlot.allowedCategories += allowedCategory.id + "|";

                            foreach (var excludedCategory in slot.filters.excludedCategories)
                                newSlot.excludedCategories += excludedCategory.id + "|";
                            DataTableSlots[tItem.id].Add(newSlot.slotId, newSlot);
                        }
                    }
                    //Item properties Weapon Mod - TODO double slots code
                    else if (tItem.types.Contains("mods")) // TODO const - missing anything by doing this?
                    {
                        if (tItem.categoryName.Contains(CATEGORY_NAME_BARREL))
                        {
                            tItem.centerOfImpact = graphItem.properties.centerOfImpact;
                            tItem.deviationCurve = graphItem.properties.deviationCurve;
                            tItem.deviationMax = graphItem.properties.deviationMax;
                        }
                        
                        DataTableSlots.Add(tItem.id, new Dictionary<string, Slot>());
                        if (HasValidValue(graphItem.properties, "slots"))
                        {
                            foreach (var slot in graphItem.properties.slots)
                            {
                                var newSlot = new Slot();
                                newSlot.itemId = tItem.id;
                                newSlot.itemName = tItem.name;
                                newSlot.slotId = slot.id;
                                newSlot.nameId = slot.nameId;
                                newSlot.required = slot.required == "true";
                                newSlot.name = slot.name;
                                foreach (var allowedItem in slot.filters.allowedItems)
                                {
                                    newSlot.allowedIDsStr += allowedItem.id + "|";
                                    newSlot.allowedIDs.Add(allowedItem.id.ToString());
                                }

                                foreach (var excludedItem in slot.filters.excludedItems)
                                    newSlot.excludedIDs += excludedItem.id + "|";

                                foreach (var allowedCategory in slot.filters.allowedCategories)
                                    newSlot.allowedCategories += allowedCategory.id + "|";

                                foreach (var excludedCategory in slot.filters.excludedCategories)
                                    newSlot.excludedCategories += excludedCategory.id + "|";
                                DataTableSlots[tItem.id].Add(newSlot.slotId, newSlot);
                            }
                        }
                    }
                }

                DataTable.Add(tItem.id, tItem);

            }
            
            Console.WriteLine("Done!");
        }
        
        public static void WriteToCsv(string _itemsFilename = "tempItemData.csv", string _slotsFilename = "tempItemSlotData.csv")
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = "~" };
            
            using var writerItems = new StreamWriter(_itemsFilename);
            using var csvItems = new CsvWriter(writerItems, config);
            
            csvItems.WriteRecords(DataTable.Values.ToList());

            using var writerSlots = new StreamWriter(_slotsFilename);
            using var csvSlots = new CsvWriter(writerSlots, config);
            var slotsList = new List<SlotsCSVHelper>();
            foreach (var itemId in DataTableSlots.Keys)
                foreach (var slotEntry in DataTableSlots[itemId])
                    slotsList.Add(new SlotsCSVHelper()
                        { ItemId = itemId, SlotId = slotEntry.Key, Desc = slotEntry.Value.name });
            csvSlots.WriteRecords(slotsList);
            
            Console.WriteLine("Successfully wrote Item Data to '" + _itemsFilename + "' and slot data to '" + _slotsFilename + "'");
        }

        
        private class SlotsCSVHelper
        {
            public string ItemId { get; set; }
            public string SlotId { get; set; }
            public string Desc { get; set; }
        }

    }
}
