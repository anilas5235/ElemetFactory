using System.Collections.Generic;
using System.Linq;

namespace Project.Scripts.ItemSystem
{
    public static class ItemMemory
    {
        private static Dictionary<uint, Item> ItemDataBank;

        public static void SetUpItemDataBank(Item[] previous = null)
        {
            ItemDataBank = new Dictionary<uint, Item>();

            if (previous == null) return;
            for (uint i = 0; i < previous.Length; i++)
            {
                ItemDataBank.Add(i,previous[i]);
            }
        }

        public static uint GetItemID(Item item)
        {
            if (ItemDataBank.ContainsValue(item)) return ItemDataBank.FirstOrDefault(x => x.Value.Equals(item)).Key;
            uint key = (uint)ItemDataBank.Count;
            ItemDataBank.Add(key, item);
            return key;
        }

        public static bool GetItem(uint ID, out Item item)
        {
            item = default;
            return (ItemDataBank.TryGetValue(ID, out item));
        }

        public static Item[] GetCurrentData()
        {
            return ItemDataBank.Values.ToArray();
        }
    }
}
