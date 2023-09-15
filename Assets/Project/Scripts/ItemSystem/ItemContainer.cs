using System;
using Project.Scripts.Buildings;
using Project.Scripts.Grid;
using Project.Scripts.SlotSystem;
using Project.Scripts.UI;
using Project.Scripts.Utilities;
using TMPro;
using UI.Windows;
using UnityEngine;
using UnityEngine.Serialization;

namespace Project.Scripts.ItemSystem
{
    public class ItemContainer : MonoBehaviour
    {
        [SerializeField] private Item item;
        public Item Item => item;

        [SerializeField]private Slot mySlot;
        
        [SerializeField] private bool inView = true;

        [SerializeField] private SpriteRenderer itemContentRender;

        [SerializeField] private ItemForm myItemForm;
        public ItemForm MyItemForm=> myItemForm;
        public Color MyColor { get; protected set; }

        private Vector3 previousPos;
        private bool arrived = true;
        private float progress = 0;

        public void SetItem(Item newItem)
        {
            item = newItem;
        }

        public void SetSlot(Slot slot)
        {
            if(slot == mySlot)return;
            if (mySlot)
            {
               previousPos = mySlot.transform.position;
               arrived = false;
               progress = 0;
            }
            mySlot = slot;
        }

        private void Update()
        {
            if (!mySlot || arrived) return;
            if (Vector3.Distance(transform.position, mySlot.transform.position) > .01f && inView)
            {
                progress += Time.deltaTime* ConveyorBelt.ItemsPerSecond;
                transform.position = Vector3.Lerp(previousPos, mySlot.transform.position, progress);
                return;
            }

            transform.position = mySlot.transform.position;
            arrived = true;
        }

        private void FixedUpdate()
        {
            VisibilityCheck();
        }

        private void VisibilityCheck()
        {
            if(!mySlot) return;
            if (mySlot.MyBuilding.MyChunk.Loaded == inView) return;
            inView = !inView;
            itemContentRender.enabled = inView;
        }

        public static ItemContainer CreateNewContainer(Item content, Slot slot)
        {
            return SetContainer(CreateNewContainer(), content, slot);
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
            container.VisibilityCheck();
            return container;
        }

        public void Destroy()
        {
            mySlot = null;
            ItemPool.Instance.AddObjectToPool(gameObject);
        }

        public void SetColor(Color color)
        {
            MyColor = color;
            Material[] materials = itemContentRender.materials;
            materials[0] = VisualResources.GetItemMaterial(MyColor, myItemForm);
            itemContentRender.materials = materials;
        }

        public void SetItemForm(ItemForm itemForm)
        {
            myItemForm = itemForm;
            Material[] materials = itemContentRender.materials;
            materials[0] = VisualResources.GetItemMaterial(MyColor, myItemForm);
            itemContentRender.materials = materials;
        }

        private void OnMouseDown()
        {
            Debug.Log(ToString());
            ItemInspector.Instance.InspectItem(this);
        }

        public override string ToString()
        {
            return item.ToString();
        }
    }
}
