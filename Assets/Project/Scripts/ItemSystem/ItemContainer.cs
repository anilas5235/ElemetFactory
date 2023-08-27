using System;
using Project.Scripts.Buildings;
using Project.Scripts.Grid;
using Project.Scripts.SlotSystem;
using Project.Scripts.Visualisation;
using UnityEngine;

namespace Project.Scripts.ItemSystem
{
    public class ItemContainer : MonoBehaviour
    {
        [SerializeField] private Item item;
        public Item Item => item;

        [SerializeField]private Slot mySlot;
        public bool InView = true;

        [SerializeField] private SpriteRenderer[] itemContentRenders;

        [SerializeField] private ItemForm myItemForm;

        [SerializeField] private GameObject visualParentGas, visualParentFluid, visualParentSolid;

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
            container.transform.localScale = new Vector3(GridBuildingSystem.CellSize, GridBuildingSystem.CellSize);
            return SetContainer(container, content, slot);
        }
        
        public static ItemContainer CreateNewContainer()
        {
            ItemContainer container = Instantiate(VisualResources.ItemContainer).GetComponent<ItemContainer>();
            container.transform.localScale = new Vector3(GridBuildingSystem.CellSize, GridBuildingSystem.CellSize);
            return container;
        }

        public static ItemContainer SetContainer(ItemContainer container, Item content, Slot slot)
        {
            container.SetItem(content);
            container.SetSlot(slot);
            container.transform.position = slot.transform.position;
            return container;
        }

        public void Destroy()
        {
            mySlot = null;
            ItemPool.Instance.AddObjectToPool(gameObject);
        }

        public void SetColor(Color color)
        {
            foreach (SpriteRenderer itemContentRender in itemContentRenders)
            {
                itemContentRender.color = color;
            }
        }

        public void SetItemForm(ItemForm itemForm)
        {
            if(myItemForm == itemForm)return;

            switch (myItemForm)
            {
                case ItemForm.Gas: visualParentGas.SetActive(false);
                    break;
                case ItemForm.Fluid: visualParentFluid.SetActive(false);
                    break;
                case ItemForm.Solid: visualParentSolid.SetActive(false);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            myItemForm = itemForm;

            switch (myItemForm)
            {
                case ItemForm.Gas: visualParentGas.SetActive(true);
                    break;
                case ItemForm.Fluid: visualParentFluid.SetActive(true);
                    break;
                case ItemForm.Solid: visualParentSolid.SetActive(true);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
