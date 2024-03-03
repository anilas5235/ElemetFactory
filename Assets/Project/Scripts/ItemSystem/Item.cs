namespace Project.Scripts.ItemSystem
{
    public readonly struct Item
    {
        public static readonly Item EmptyItem = new();
        
        public int ItemID { get; }
       
        public Item(int itemID = -1)
        {
            ItemID = itemID;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType()) {return false;}
            var target = (Item) obj;
            return target.ItemID == ItemID;
        }
        public override int GetHashCode()
        {
            return ItemID;
        }
    }
}
