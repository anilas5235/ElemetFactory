using Project.Scripts.SlotSystem;
using Project.Scripts.Visualisation;
using UnityEngine;

namespace Project.Scripts.ItemSystem
{
    public static class ItemUtility 
    {
        public static ItemContainer GetItemContainerWith(Item item, Slot slot)
        {
            ItemContainer container = ItemContainer.SetContainer(ItemPool.Instance.GetObjectFromPool().GetComponent<ItemContainer>(),item,slot);
            Color contentColor = new Color(0, 0, 0);
            float fromID = 0;
            foreach (int resourceID in item.ResourceIDs)
            {
                contentColor += VisualResources.GetResourceColor(resourceID);
                fromID +=(int) VisualResources.GetResourceForm(resourceID);
            }

            contentColor *= 1f/ item.ResourceIDs.Length;
            fromID *= 1f/ item.ResourceIDs.Length;
            fromID = Mathf.RoundToInt(fromID);
            container.SetColor(contentColor);
            container.SetItemForm((ItemForm)fromID);
            return container;
        }
        public static ItemContainer GetItemContainerWith(int[] resourceIDs, Slot slot)
        {
            return GetItemContainerWith(new Item(resourceIDs),slot);
        }
    }
}
