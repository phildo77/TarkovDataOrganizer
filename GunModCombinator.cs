using Newtonsoft.Json;

namespace TarkovDataOrganizer;

public partial class TarkovData
{
    public class GunModCombinator
    {
        //Gun Item Id, List of all possible gun configs
        public static Dictionary<string, List<GunConfig>> AllConfigs = new();

        public class GunConfig
        {
            public GunConfig(string _gunId)
            {
                GunId = _gunId;
                RootNode = new Node(_gunId, this);
            }

            public string GunId;
            public Node RootNode;

            public bool IsValid => IsValidConfig(RootNode);

            private static bool IsValidConfig(Node _node)
            {
                var itemSlots = TarkovItem.DataTableSlots[_node.ItemId];
                foreach (var slotChild in _node.SlotChildren)
                {
                    if (slotChild.Value == null)
                        return !itemSlots[slotChild.Key].required;
                    if (!IsValidConfig(slotChild.Value))
                        return false;
                }

                return true;
            }

            //TODO - is this efficient?  Does it work?
            public GunConfig DeepClone()
            {
                var json = JsonConvert.SerializeObject(this);
                return JsonConvert.DeserializeObject<GunConfig>(json);
            }



            public class Node
            {
                public Node(string _rootItemId, GunConfig _config)
                {
                    ItemId = _rootItemId;
                    ParentConfig = _config;
                    ParentId = string.Empty; //Empty if Root Gun
                    ParentSlotId = string.Empty; //Empty if Root Gun
                    SlotChildren = new Dictionary<string, Node?>();
                    foreach (var slot in TarkovItem.DataTableSlots[_rootItemId])
                        SlotChildren.Add(slot.Key, null);
                }

                public Node(string _itemId, Node _parentNode, string _slotIdTaken)
                {
                    ItemId = _itemId;
                    ParentConfig = _parentNode.ParentConfig;
                    ParentId = _parentNode.ParentId; //Empty if Root Gun
                    ParentSlotId = _parentNode.ParentSlotId; //Empty if Root Gun
                    SlotChildren = new Dictionary<string, Node?>();
                    foreach (var slot in TarkovItem.DataTableSlots[_itemId])
                        SlotChildren.Add(slot.Key, null);
                }

                public GunConfig ParentConfig;
                public string ItemId;
                public string ParentId;
                public string ParentSlotId;
                public Dictionary<string, Node?> SlotChildren; //(slotId, childNode ) child null if not slotted
            }

            public static List<GunConfig> Build(string _gunId)
            {
                Console.WriteLine("Finding and building all loadout combinations for: " +
                                  TarkovItem.DataTable[_gunId].name);
                
                var gun = TarkovItem.DataTable[_gunId];
                if (TarkovItem.DataTableSlots.ContainsKey(_gunId))
                {
                    var allConfigs = Build(new GunConfig(_gunId).RootNode);
                    
                    Console.WriteLine("Found " + allConfigs.Count + " unique and valid configs!");
                    
                }
                    
                Console.WriteLine("No slots for id: " + _gunId + " - " + gun.name);
                Console.WriteLine("Done!");
                return [];
            }
            private static List<GunConfig> Build(Node _currentNode)
            {
                Console.WriteLine(".");
                var configList = new List<GunConfig>();

                //Add config if valid (no unfilled required slots in the whole config)
                //TODO this is a lot of searching through the tree/config - could be more efficient?
                if (_currentNode.ParentConfig.IsValid)
                    configList.Add(_currentNode.ParentConfig.DeepClone());


                if (!TarkovItem.DataTableSlots.ContainsKey(_currentNode.ItemId) ||
                    TarkovItem.DataTableSlots[_currentNode.ItemId].Count == 0)
                {
                    Console.WriteLine("No slots for id: " + _currentNode.ItemId + " - " +
                                      TarkovItem.DataTable[_currentNode.ItemId].name);
                    return [];
                }

                foreach (var slot in TarkovItem.DataTableSlots[_currentNode.ItemId])
                {
                    if (slot.Value.name.Contains("Scope")) //TODO - make a switch for this?
                        continue;
                    foreach (var allowedItemId in slot.Value.allowedIDs)
                    {
                        var childNode = new Node(TarkovItem.DataTable[allowedItemId].id, _currentNode, slot.Key);

                        configList.AddRange(Build(childNode));
                    }
                }


                return configList;
            }
        }
    }
}