namespace Project.Scripts.ItemSystem
{
    public static class ItemUtility 
    {
        public static ItemContainer GetItemContainerWith(Item item)
        {
            ItemContainer container = ItemPool.Instance.GetObjectFromPool().GetComponent<ItemContainer>();
            container.SetItem(item);
            return container;
        }
        public static ItemContainer GetItemContainerWith(int[] resourceIDs)
        {
            ItemContainer container = ItemPool.Instance.GetObjectFromPool().GetComponent<ItemContainer>();
            container.SetItem(new Item(resourceIDs));
            return container;
        }
    }
}
