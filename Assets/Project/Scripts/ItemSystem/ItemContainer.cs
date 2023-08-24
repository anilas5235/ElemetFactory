using Project.Scripts.Buildings;
using Project.Scripts.Grid;
using Project.Scripts.SlotSystem;
using Project.Scripts.Visualisation;
using UnityEngine;

namespace Project.Scripts.ItemSystem
{
    public class ItemContainer : MonoBehaviour
    {
        private Item item;
        public Item Item => item;

        [SerializeField]private Slot mySlot;
        public bool InView = true;

        public void SetItem(Item newItem)
        {
            item = newItem;
        }

        public void SetSlot(Slot slot)
        {
            mySlot = slot;
        }

        private void FixedUpdate()
        {
            if(!mySlot) return;
            if (Vector3.Distance(transform.position, mySlot.transform.position) > .1f)
                if (InView)
                {
                    transform.position += (mySlot.transform.position - transform.position).normalized *
                                          (Time.fixedDeltaTime * ConveyorBelt.ItemsPerSecond *
                                           GridBuildingSystem.CellSize);
                }
                else transform.position = mySlot.transform.position;

            else transform.position = mySlot.transform.position;
        }

        public static ItemContainer CreateNewContainer(Item content, Slot slot)
        {
            ItemContainer container = Instantiate(VisualResources.ItemContainer).GetComponent<ItemContainer>();
            return SetContainer(container, content, slot);
        }

        public static ItemContainer SetContainer(ItemContainer container, Item content, Slot slot)
        {
            container.SetItem(content);
            container.SetSlot(slot);
            container.transform.position = slot.transform.position;
            container.transform.localScale = new Vector3(GridBuildingSystem.CellSize, GridBuildingSystem.CellSize);
            return container;
        }

        public void Destroy()
        {
            mySlot = null;
            ItemPool.Instance.AddObjectToPool(gameObject);
        }
    }
}
