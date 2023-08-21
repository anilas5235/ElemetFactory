using System;

namespace Project.Scripts.ItemSystem
{
    [Serializable]
    public struct Item
    {
        public int[] ResourceIDs;

        public Item(int[] resourceIDs)
        {
            ResourceIDs = resourceIDs;
        }
    }
}
