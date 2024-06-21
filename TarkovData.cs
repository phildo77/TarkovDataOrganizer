using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace TarkovDataOrganizer
{
    public static class Serialize
    {
        public static string ToJson(this TarkovData.TraderCashOffer self) => JsonConvert.SerializeObject(self, Converter.Settings);

    }
    
    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }


    public partial class TarkovData
    {
        public static Dictionary<string, TarkovItem> TarkovItems;

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

        public static bool HasValidValue(dynamic obj, string propertyName)
        {
            var jo = (JObject)obj;
            if (jo == null)
                return false;
            var token = jo[propertyName];
            if (token == null)
                return false;
            if (token.Type == JTokenType.Null)
                return false;
            return true;
        }

        public class TarkovItem
        {
            public string id;
            public string name;
            public string shortName;
            public string categoryId;
            public string categoryName;
            public string types; //Can contain multiple strings delim |


            public float weight;
            public float ergonomicsModifier;
            public float accuracyModifier;
            public float recoilModifier;
            public float loudness;
            public float velocity;

            public string conflictingSlotIds; //Can contain multiple strings delim |
            public string conflictingItemsIds; //Can contain multiple strings delim |
            public string conflictingItemsNames; //Can contain multiple strings delim |

            public string image8xLink;
            public string image512pxLink;
            public string gridImageLink;
            public string baseImageLink;
            public string wikiLink;

            //ItemPropWeapon
            public string caliber;
            public string ergonomics;
            public string fireRate;
            public string fireModes; //Can contain multiple strings delim |

            public float recoilVertical;
            public float recoilHorizontal;

            public string defaultPresetId = string.Empty;
            public string presetIds; //Multiple delim |


            //ItemPropWeaponMod
            //ItemPropBarrel
            //public int ergonomics;  //This is the same as ergonomicsModifier - not needed
            //public float recoilModifier; //This is the same as recoilModifier only / 100 - not needed
            public float centerOfImpact;
            public float deviationCurve;
            public float deviationMax;

            //All items w/ props

            public List<Slot> slots;


            public static async Task ScrapeAllItemData(bool force = false)
            {
                TarkovItems = new Dictionary<string, TarkovItem>();

                var graphData = await GraphQueries.QueryTarkovAPI(QueryAllItems);

                var items = new Dictionary<string, TarkovItem>();
                foreach (var graphItem in graphData.Data.items)
                {
                    var tItem = new TarkovItem();
                    tItem.id = graphItem.id;
                    tItem.name = graphItem.name;
                    tItem.shortName = graphItem.shortName;
                    tItem.categoryId = graphItem.category.id;
                    tItem.categoryName = graphItem.category.name;
                    tItem.types = string.Empty;
                    foreach (var giType in graphItem.types)
                        tItem.types += giType + "|";

                    tItem.weight = graphItem.weight;
                    tItem.ergonomicsModifier = graphItem.Value<float?>("ergonomicsModifier") ?? 0f;
                    tItem.accuracyModifier = graphItem.Value<float?>("accuracyModifier") ?? 0f;
                    tItem.recoilModifier = graphItem.Value<float?>("recoilModifier") ?? 0f;
                    tItem.loudness = graphItem.Value<float?>("loudness") ?? 0f;
                    tItem.velocity = graphItem.Value<float?>("velocity") ?? 0f;

                    tItem.conflictingSlotIds = string.Empty;
                    foreach (var conSlotId in graphItem.conflictingSlotIds)
                        tItem.conflictingSlotIds += conSlotId + "|";

                    tItem.conflictingItemsIds = string.Empty;
                    tItem.conflictingItemsNames = string.Empty;
                    foreach (var conItem in graphItem.conflictingItems)
                    {
                        tItem.conflictingItemsIds += conItem.id + "|";
                        tItem.conflictingItemsNames += conItem.name + "|";
                    }

                    tItem.image8xLink = graphItem.image8xLink;
                    tItem.image512pxLink = graphItem.image512pxLink;
                    tItem.gridImageLink = graphItem.gridImageLink;
                    tItem.baseImageLink = graphItem.baseImageLink;
                    tItem.wikiLink = graphItem.wikiLink;

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

                            if (HasValidValue(graphItem.defaultPreset, "id"))
                                tItem.defaultPresetId = graphItem.defaultPreset.id;
                            tItem.presetIds = string.Empty;

                            if (HasValidValue(graphItem.properties, "presets"))
                                foreach (var preset in graphItem.properties.presets)
                                    tItem.presetIds += preset.id + "|";

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

                    TarkovItems.Add(tItem.id, tItem);

                    Console.WriteLine("Successfully Scraped Item Data.");
                }
            }

            private static readonly string QueryAllItems =
                @"query {
            items
               {
                id
                name
                shortName
                category { id name }
                types
                wikiLink

                weight
                ergonomicsModifier
                accuracyModifier
                recoilModifier
                loudness
                velocity
                
                conflictingSlotIds
                conflictingItems
                {
                  id
                  name
                }
                
                image8xLink
                image512pxLink
                gridImageLink
                baseImageLink
                
                properties {
                  ... on ItemPropertiesWeapon
                  {        
                    caliber
                    
                    ergonomics
                    
                    fireRate
                    fireModes
                    
                    recoilVertical
                    recoilHorizontal
                    
                    defaultPreset {
                      id
                      name
                      baseImageLink
                      gridImageLink
                      image8xLink
                      image512pxLink
                    }
                    
                    presets {
                      id
                      name
                    }
                    
                    slots {
                      id
                      nameId
                      required
                      name
                      filters {
                        allowedItems { id name }
                        excludedItems { id name }
                        allowedCategories { id name }
                        excludedCategories { id name }
                      }
                    }
                  }
                  
                  ... on ItemPropertiesWeaponMod
                    {                        
                      slots {
                        id
                        nameId
                        required
                        name
                        filters {
                          allowedItems { id name }
                          excludedItems { id name }
                          allowedCategories { id name }
                          excludedCategories { id name }
                        }
                      }
                    }
                  ... on ItemPropertiesBarrel
                  {
                    ergonomics
                    recoilModifier
                    centerOfImpact
                    deviationCurve
                    deviationMax
                    
                    slots {
                        id
                        nameId
                        required
                        name
                        filters {
                          allowedItems { id name }
                          excludedItems { id name }
                          allowedCategories { id name }
                          excludedCategories { id name }
                        }
                      }
                  }
                }
              }
            }";
        }
    }
}