using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;

namespace TarkovDataOrganizer;

public partial class TarkovData
{
    public class TarkovItem
    {
        public static List<string> GetDistinctCategories()
        {
            return DataTable.Values
                .Where(item => item.types.Split('|').Contains("gun", StringComparer.OrdinalIgnoreCase))
                .Select(item => item.categoryName)
                .Distinct()
                .OrderBy(name => name)
                .ToList();
        }
        private static float CalculateRecoilHorizontal(TarkovItem baseWeapon, List<TarkovItem> attachments)
        {
            return baseWeapon.recoilHorizontal + attachments.Sum(a => a.recoilModifier); // Adjust the logic based on how horizontal recoil is calculated
        }

        public class WeaponCombination
        {
            public Dictionary<string, List<TarkovItem>> SlotGroups { get; set; }
            public float RecoilVertical { get; set; }
            public float RecoilHorizontal { get; set; } // New property
            public float Ergonomics { get; set; }
            public float Velocity { get; set; }
            public float AccuracyModifier { get; set; }
            public float Weight { get; set; }
            public int BasePrice { get; set; }
            // Add other stats as needed
        }
        public bool HasSubSlots()
        {
            // Return true if this item has sub-slots; otherwise, false.
            return DataTableSlots.TryGetValue(this.id, out var slots) && slots.Any();
        }

        public List<Slot> GetSubSlots()
        {
            if (DataTableSlots.TryGetValue(this.id, out var slots))
            {
                return slots.Values
                    .Where(slot => slot.required == false) // Example condition to filter subslots
                    .ToList();
            }

            return new List<Slot>(); // Return an empty list if no subslots are found
        }


        public static List<WeaponCombination> GenerateUniqueCombinations(TarkovItem selectedWeapon)
        {
            var uniqueCombinations = new Dictionary<(float RecoilVertical, float RecoilHorizontal, float Ergonomics), WeaponCombination>();

            // Make sure the weapon has slots
            if (DataTableSlots.TryGetValue(selectedWeapon.id, out var weaponSlots))
            {
                // Convert weaponSlots (Slot objects) to a dictionary of TarkovItems
                var convertedSlots = weaponSlots.ToDictionary(
                    slot => slot.Key, // Slot name
                    slot => slot.Value.allowedIDs
                        .Select(id => DataTable.GetValueOrDefault(id))
                        .Where(item => item != null)
                        .ToList()
                );

                // Recursive function to get all combinations for a slot and its subslots
                void GetCombinations(Dictionary<string, List<TarkovItem>> currentSlots, Dictionary<string, List<TarkovItem>> accumulatedCombination)
                {
                    if (currentSlots.Count == 0)
                    {
                        // Calculate weapon stats
                        var recoilVertical = CalculateRecoilVertical(selectedWeapon, accumulatedCombination.SelectMany(kv => kv.Value).ToList());
                        var recoilHorizontal = CalculateRecoilHorizontal(selectedWeapon, accumulatedCombination.SelectMany(kv => kv.Value).ToList()); // Added Horizontal Recoil Calculation
                        var ergonomics = CalculateErgonomics(selectedWeapon, accumulatedCombination.SelectMany(kv => kv.Value).ToList());

                        // Group by Recoil (Vertical, Horizontal) and Ergonomics
                        var key = (recoilVertical, recoilHorizontal, ergonomics);

                        if (!uniqueCombinations.TryGetValue(key, out var existingCombination))
                        {
                            uniqueCombinations[key] = new WeaponCombination
                            {
                                SlotGroups = new Dictionary<string, List<TarkovItem>>(accumulatedCombination),
                                RecoilVertical = recoilVertical,
                                RecoilHorizontal = recoilHorizontal, // Added Horizontal Recoil to WeaponCombination
                                Ergonomics = ergonomics,
                                Velocity = CalculateVelocity(selectedWeapon, accumulatedCombination.SelectMany(kv => kv.Value).ToList()),
                                AccuracyModifier = CalculateAccuracyModifier(selectedWeapon, accumulatedCombination.SelectMany(kv => kv.Value).ToList()),
                                Weight = CalculateWeight(selectedWeapon, accumulatedCombination.SelectMany(kv => kv.Value).ToList()),
                                BasePrice = CalculateBasePrice(selectedWeapon, accumulatedCombination.SelectMany(kv => kv.Value).ToList())
                            };
                        }
                        else
                        {
                            // Optionally: Aggregate other properties if needed
                            existingCombination.BasePrice += CalculateBasePrice(selectedWeapon, accumulatedCombination.SelectMany(kv => kv.Value).ToList());
                        }
                        return;
                    }

                    // Recursively process each slot and its allowed items
                    var currentSlot = currentSlots.First();
                    var remainingSlots = currentSlots.Skip(1).ToDictionary(kv => kv.Key, kv => kv.Value);

                    foreach (var item in currentSlot.Value)
                    {
                        // Check if the item has subslots and recurse
                        if (DataTableSlots.TryGetValue(item.id, out var subSlots) && subSlots.Any())
                        {
                            foreach (var subItem in subSlots)
                            {
                                var allowedSubItems = subItem.Value.allowedIDs.Select(id => DataTable.GetValueOrDefault(id)).Where(subItem => subItem != null).ToList();

                                var newCombination = new Dictionary<string, List<TarkovItem>>(accumulatedCombination)
                                {
                                    [currentSlot.Key] = new List<TarkovItem> { item }
                                };

                                GetCombinations(remainingSlots, newCombination); // Recurse into subslot
                            }
                        }
                        else
                        {
                            // If no subslots, simply add to the current combination and continue
                            var newCombination = new Dictionary<string, List<TarkovItem>>(accumulatedCombination)
                            {
                                [currentSlot.Key] = new List<TarkovItem> { item }
                            };

                            GetCombinations(remainingSlots, newCombination);
                        }
                    }
                }

                // Start the recursion with the converted slots (TarkovItems)
                GetCombinations(convertedSlots, new Dictionary<string, List<TarkovItem>>());
            }

            // Return only the unique combinations based on Recoil (Vertical, Horizontal) and Ergonomics
            return uniqueCombinations.Values.ToList();
        }

        private static float CalculateRecoilVertical(TarkovItem baseWeapon, List<TarkovItem> attachments)
        {
            return baseWeapon.recoilVertical + attachments.Sum(a => a.recoilModifier);
        }

        private static float CalculateErgonomics(TarkovItem baseWeapon, List<TarkovItem> attachments)
        {
            return baseWeapon.ergonomics + attachments.Sum(a => a.ergonomicsModifier);
        }

        private static float CalculateVelocity(TarkovItem baseWeapon, List<TarkovItem> attachments)
        {
            return baseWeapon.velocity + attachments.Sum(a => a.velocity);
        }

        private static float CalculateAccuracyModifier(TarkovItem baseWeapon, List<TarkovItem> attachments)
        {
            return baseWeapon.accuracyModifier + attachments.Sum(a => a.accuracyModifier);
        }

        private static float CalculateWeight(TarkovItem baseWeapon, List<TarkovItem> attachments)
        {
            return baseWeapon.weight + attachments.Sum(a => a.weight);
        }

        private static int CalculateBasePrice(TarkovItem baseWeapon, List<TarkovItem> attachments)
        {
            return baseWeapon.basePrice + attachments.Sum(a => a.basePrice);
        }

        private static List<List<T>> CartesianProduct<T>(List<List<T>> lists)
        {
            var result = new List<List<T>>();
            if (lists.Count == 0)
            {
                result.Add(new List<T>());
                return result;
            }

            var firstList = lists[0];
            var remainingLists = lists.Skip(1).ToList();

            foreach (var item in firstList)
            {
                foreach (var resultList in CartesianProduct(remainingLists))
                {
                    resultList.Insert(0, item);
                    result.Add(resultList);
                }
            }

            return result;
        }

        public static List<string> GetDistinctCalibers()
        {
            return DataTable.Values
                .Where(item => !string.IsNullOrEmpty(item.caliber))
                .Select(item => item.caliber)
                .Distinct()
                .OrderBy(caliber => caliber)
                .ToList();
        }

        public static List<float> GetDistinctRecoilVerticals()
        {
            return DataTable.Values
                .Where(item => item.recoilVertical > 0)
                .Select(item => item.recoilVertical)
                .Distinct()
                .OrderBy(recoil => recoil)
                .ToList();
        }

        public static Dictionary<string, TarkovItem> DataTable;
        public static Dictionary<string, Dictionary<string, Slot>> DataTableSlots;

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

        public static void LoadData(IEnumerable<TarkovData.TarkovItem> items)
        {
            // Ensure DataTable is initialized
            DataTable = items.ToDictionary(item => item.id, item => item);

            // Ensure DataTableSlots is initialized
            if (DataTableSlots == null)
            {
                DataTableSlots = new Dictionary<string, Dictionary<string, Slot>>();
            }

            // Initialize DataTableSlots based on items
            foreach (var item in items)
            {
                // Ensure the item has a valid id
                if (!string.IsNullOrEmpty(item.id))
                {
                    // Try to retrieve existing slots or create a new entry if not found
                    if (!DataTableSlots.TryGetValue(item.id, out var slots))
                    {
                        slots = new Dictionary<string, Slot>();
                        DataTableSlots[item.id] = slots;
                    }

                    // Additional processing if needed, for example, populating the slots dictionary
                    // You can add slots logic here if there is more data that needs to be loaded
                }
                else
                {
                    // Handle cases where the item's id is missing or invalid if necessary
                    Console.WriteLine($"Warning: Item with name '{item.name}' has an invalid or missing ID.");
                }
            }
        }


        public static async Task DownloadTable(bool force = false)
        {
            Console.WriteLine("Downloading Item Data... (this might take a few seconds)");
            DataTable = new Dictionary<string, TarkovItem>();
            DataTableSlots = new Dictionary<string, Dictionary<string, Slot>>();

            var graphData = await GraphQueries.QueryTarkovAPI(GraphQueries.QUERY_ITEMS_ALL_GENERIC_INFO);

            foreach (var graphItem in graphData.Data.items)
            {
                var tItem = new TarkovItem();
                tItem.id = graphItem.id;
                tItem.name = graphItem.name;
                tItem.shortName = graphItem.shortName;
                tItem.description = graphItem.description;
                if (tItem.description != null)
                    tItem.description = tItem.description
                        .Replace(",", "`").Replace(System.Environment.NewLine, " ")
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
                        if (tItem.categoryName.Contains("Barrel"))
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