using Project.Scripts.Buildings.BuildingFoundation;
using Project.Scripts.Buildings.Parts;
using Project.Scripts.ItemSystem;
using Project.Scripts.SlotSystem;
using UnityEngine;

namespace Project.Scripts.Buildings
{
    public class Separator : PlacedBuilding, IHaveOutput,IHaveInput
    {
        private static float SepartionPerSecond = 0.25f;

        protected override void StartWorking()
        {
            
        }

        protected override void SetUpSlots(FacingDirection facingDirection)
        {
            throw new System.NotImplementedException();
        }

        public override void CheckForSlotToPullForm()
        {
            throw new System.NotImplementedException();
        }

        public override void CheckForSlotsToPushTo()
        {
            throw new System.NotImplementedException();
        }

        private void InputSlotChanged(bool fillStatus)
        {
            if (fillStatus && !outputs[0].IsOccupied && !outputs[1].IsOccupied)
            {
                int itemLength = inputs[0].SlotContent.Item.ResourceIDs.Length;
                if(itemLength < 1) return;
                int item1Length = Mathf.CeilToInt(itemLength / 2f), item2Length = item1Length - item1Length;
                ItemContainer item = inputs[0].EmptySlot();
                int[] contentItem1= new int[item1Length], contentItem2=new int[item2Length];
                for (int i = 0; i < itemLength; i++)
                {
                    if (i < item1Length) contentItem1[i] = item.Item.ResourceIDs[i];
                    else contentItem2[i-item1Length] = item.Item.ResourceIDs[i];
                }
                item.Destroy();

                outputs[0].FillSlot(ItemContainer.CreateNewContainer(new Item(contentItem1),outputs[0]));
                outputs[1].FillSlot(ItemContainer.CreateNewContainer(new Item(contentItem2),outputs[1]));
            }
        }

        public Slot GetOutputSlot(PlacedBuilding caller, Slot destination)
        {
            throw new System.NotImplementedException();
        }

        public Slot GetInputSlot(PlacedBuilding caller, Slot destination)
        {
            throw new System.NotImplementedException();
        }
    }
}
