using System.Globalization;
using CsvHelper;

namespace TarkovDataOrganizer;

public partial class TarkovData
{
    public class TarkovItem
    {
        
        
        public static List<TarkovItem> DataTable;

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
        public string ergonomics { get; set; }
        public string fireRate { get; set; }
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

        public List<Slot> slots = new List<Slot>();

        public class Slot
        {
            public string id;
            public string nameId;
            public bool required;
            public string name;
            public List<string> allowedIDs;
            public List<string> allowedCategories;
            public List<string> excludedIDs;
            public List<string> exlcludedCategories;
        }
        
        public static async Task DownloadTable(bool force = false)
        {
            DataTable = new List<TarkovItem>();

            var graphData = await GraphQueries.QueryTarkovAPI(GraphQueries.QUERY_ITEMS_ALL_GENERIC_INFO);

            foreach (var graphItem in graphData.Data.items)
            {
                var tItem = new TarkovItem();
                tItem.id = graphItem.id;
                tItem.name = graphItem.name;
                tItem.shortName = graphItem.shortName;
                tItem.description = graphItem.description;
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

                        if (HasValidValue(graphItem.properties.defaultPreset, "id"))
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

                        tItem.slots = new List<Slot>();
                        foreach (var slot in graphItem.properties.slots)
                        {
                            var newSlot = new Slot();
                            newSlot.id = slot.id;
                            newSlot.nameId = slot.nameId;
                            newSlot.required = slot.required == "true";
                            newSlot.name = slot.name;
                            newSlot.allowedIDs = new List<string>();
                            foreach (var allowedItem in slot.filters.allowedItems)
                                newSlot.allowedIDs.Add((string)allowedItem.id);
                            foreach (var excludedItem in slot.filters.excludedItems)
                                newSlot.excludedIDs.Add((string)excludedItem.id);
                            foreach (var allowedCategory in slot.filters.allowedCategories)
                                newSlot.allowedCategories.Add((string)allowedCategory.id);
                            foreach (var excludedCategory in slot.filters.excludedCategories)
                                newSlot.allowedCategories.Add((string)excludedCategory.id);
                            tItem.slots.Add(newSlot);
                        }
                    }
                    //Item properties Weapon Mod
                    else if (tItem.types.Contains("mods")) // TODO const - missing anything by doing this?
                    {
                        if (tItem.categoryName.Contains("Barrel"))
                        {
                            tItem.centerOfImpact = graphItem.properties.centerOfImpact;
                            tItem.deviationCurve = graphItem.properties.deviationCurve;
                            tItem.deviationMax = graphItem.properties.deviationMax;
                        }

                        tItem.slots = new List<Slot>();
                        if (HasValidValue(graphItem.properties, "slots"))
                            foreach (var slot in graphItem.properties.slots)
                            {
                                var newSlot = new Slot();
                                newSlot.id = slot.id;
                                newSlot.nameId = slot.nameId;
                                newSlot.required = slot.required == "true";
                                newSlot.name = slot.name;
                                newSlot.allowedIDs = new List<string>();
                                foreach (var allowedItem in slot.filters.allowedItems)
                                    newSlot.allowedIDs.Add((string)allowedItem.id);
                                foreach (var excludedItem in slot.filters.excludedItems)
                                    newSlot.excludedIDs.Add((string)excludedItem.id);
                                foreach (var allowedCategory in slot.filters.allowedCategories)
                                    newSlot.allowedCategories.Add((string)allowedCategory.id);
                                foreach (var excludedCategory in slot.filters.excludedCategories)
                                    newSlot.allowedCategories.Add((string)excludedCategory.id);
                                tItem.slots.Add(newSlot);
                            }
                    }
                }

                DataTable.Add(tItem);

            }
            
            Console.WriteLine("Successfully Downloaded Item Data.");
        }
        
        public static void WriteToCsv(string _filename = "tempItemData.csv")
        {
            using var writer = new StreamWriter(_filename);
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
            csv.WriteRecords(DataTable);
        }

    }
}
