using System;
using Project.Scripts.Grid;

namespace Project.Scripts.Buildings
{
    [Serializable]
    public class Item
    {
        public int[] ResourceIDs;

        public Item(int[] resourceIDs)
        {
            ResourceIDs = resourceIDs;
        }
    }
}
