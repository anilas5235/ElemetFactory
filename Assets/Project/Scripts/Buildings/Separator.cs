using Project.Scripts.Grid.DataContainers;
using Project.Scripts.Utilities;
using Project.Scripts.Visualisation;
using UnityEngine;

namespace Project.Scripts.Buildings
{
    public class Separator : PlacedBuilding
    {
        private static float SepartionPerSecond = 0.25f;

        private Slot InputSlot, Output1Slot, Output2Slot;

        protected override void StartWorking()
        {
            
        }

        public override Slot GetInputSlot(GridObject callerPosition, Slot destination)
        {
            return null;
        }

        public override Slot GetOutputSlot(GridObject callerPosition, Slot destination)
        {
            return null;
        }

        protected override void SetUpSlots(BuildingScriptableData.Directions direction)
        {
            throw new System.NotImplementedException();
        }

        public override void CheckForInputs()
        {
            throw new System.NotImplementedException();
        }

        public override void CheckForOutputs()
        {
            throw new System.NotImplementedException();
        }

        private void InputSlotChanged(bool fillStatus)
        {
            if (fillStatus && !Output1Slot.IsOccupied && !Output2Slot.IsOccupied)
            {
                int itemLength = InputSlot.SlotContent.Item.ResourceIDs.Length;
                if(itemLength < 1) return;
                int item1Length = Mathf.CeilToInt(itemLength / 2f), item2Length = item1Length - item1Length;
                ItemContainer item = InputSlot.EmptySlot();
                int[] contentItem1= new int[item1Length], contentItem2=new int[item2Length];
                for (int i = 0; i < itemLength; i++)
                {
                    if (i < item1Length) contentItem1[i] = item.Item.ResourceIDs[i];
                    else contentItem2[i-item1Length] = item.Item.ResourceIDs[i];
                }

                Output1Slot.FillSlot(ItemContainer.CreateNewContainer(new Item(contentItem1),Output1Slot));
                Output2Slot.FillSlot(ItemContainer.CreateNewContainer(new Item(contentItem2),Output2Slot));
            }
        }
    }
}
