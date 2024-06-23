namespace TarkovDataOrganizer;

public partial class TarkovData
{
    public class GunModCombinator
    {
        //Gun Item Id, List of all possible gun configs
        public static Dictionary<string, List<GunConfig>> AllConfigs = new();

        public class GunConfig
        {
            public GunConfig(Node _root)
            {
                RootNode = _root;
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
            public GunConfig Clone()
            {
                return new GunConfig(Node.CloneRoot(RootNode));
            }


            public class Node
            {
                private Node(string _rootItemId)
                {
                    ItemId = _rootItemId;
                    Parent = null;
                    SlotChildren = new Dictionary<string, Node?>();
                    foreach (var slot in TarkovItem.DataTableSlots[_rootItemId])
                        SlotChildren.Add(slot.Key, null);
                }

                private Node(string _itemId, Node _parentNode)
                {
                    ItemId = _itemId;
                    Parent = _parentNode;
                    SlotChildren = new Dictionary<string, Node?>();
                    foreach (var slot in TarkovItem.DataTableSlots[_itemId])
                        SlotChildren.Add(slot.Key, null);
                }

                public string ItemId;
                public Node? Parent;
                public Dictionary<string, Node?> SlotChildren; //(slotId, childNode ) child null if not slotted

                public static Node CloneRoot(Node _rootToClone)
                {
                    var newNode = new Node(_rootToClone.ItemId);
                    foreach (var childEntry in _rootToClone.SlotChildren)
                        if (childEntry.Value != null)
                            newNode.SlotChildren[childEntry.Key] = Clone(childEntry.Value, newNode);
                    return newNode;
                }

                private static Node Clone(Node _nodeToClone, Node _parentNode)
                {
                    var newNode = new Node(_nodeToClone.ItemId);
                    newNode.Parent = _parentNode;
                    foreach (var childEntry in _nodeToClone.SlotChildren)
                        if (childEntry.Value != null)
                            newNode.SlotChildren[childEntry.Key] = Clone(childEntry.Value, newNode);
                    return newNode;
                }

                public static Node CreateRoot(string _gunId)
                {
                    return new Node(_gunId);
                }

                public Node GetRoot
                {
                    get
                    {
                        var checkParent = Parent;
                        if (checkParent == null) return this;
                        while (checkParent != null)
                            checkParent = checkParent.Parent;
                        return Parent;
                    }
                }

                public Node AttachItemToSlot(string _itemId, string _slotIdTaken)
                {
                    var newNode = new Node(_itemId, this);

                    SlotChildren[_slotIdTaken] = newNode;
                    return newNode;
                }

                public string ParentSlotId
                {
                    get
                    {
                        if (Parent == null)
                            return string.Empty;
                        foreach (var entry in Parent.SlotChildren)
                            if (entry.Value != null && entry.Value.Equals(this))
                                return entry.Key;
                        return string.Empty;
                    }
                }
            }

            public static List<GunConfig> Build(string _gunId)
            {
                Console.WriteLine("Finding and building all loadout combinations for: " +
                                  TarkovItem.DataTable[_gunId].name);

                var configs = new List<GunConfig>();
                if (TarkovItem.DataTableSlots.ContainsKey(_gunId))
                {
                    configs = Build(Node.CreateRoot(_gunId));

                    Console.WriteLine("Found " + configs.Count + " unique and valid configs!");
                }
                Console.WriteLine("Done!");
                return configs;
            }

            private static List<GunConfig> Build(Node _currentNode)
            {
                Console.Write(".");
                var configList = new List<GunConfig>();

                var config = new GunConfig(_currentNode.GetRoot);

                //Add config if valid (no unfilled required slots in the whole config)
                //TODO this is a lot of searching through the tree/config - could be more efficient?
                if (config.IsValid)
                    configList.Add(config.Clone());


                if (!TarkovItem.DataTableSlots.ContainsKey(_currentNode.ItemId) ||
                    TarkovItem.DataTableSlots[_currentNode.ItemId].Count == 0)
                    //Console.WriteLine("No slots for id: " + _currentNode.ItemId + " - " + TarkovItem.DataTable[_currentNode.ItemId].name);
                    return [];

                foreach (var slot in TarkovItem.DataTableSlots[_currentNode.ItemId])
                {
                    if (slot.Value.name.Contains("Scope")) //TODO - make a switch for this?
                        continue;
                    foreach (var allowedItemId in slot.Value.allowedIDs)
                    {
                        var childNode = _currentNode.AttachItemToSlot(allowedItemId, slot.Key);
                        configList.AddRange(Build(childNode));
                    }
                }


                return configList;
            }
        }
    }
}