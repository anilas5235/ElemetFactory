using System.Collections.Generic;
using System.Linq;
using Project.Scripts.Grid.DataContainers;
using Project.Scripts.Utilities;
using UnityEngine;

namespace Project.Scripts.Buildings
{
    public class Separator : PlacedBuilding
    {
        private static float SepartionPerSecond = 0.25f;

        private Slot InputSlot, Output1Slot, Output2Slot;

        protected override void StartWorking()
        {
            InputSlot = new Slot(Slot.SlotBehaviour.Input);
            InputSlot.OnSlotContentChanged += InputSlotChanged;
            Output1Slot = new Slot(Slot.SlotBehaviour.Output);
            Output2Slot = new Slot(Slot.SlotBehaviour.Output);
            Vector2Int outputpos1 =
                GeneralConstants.NeighbourOffsets2D4[MyPlacedBuildingData.directionID] + MyPlacedBuildingData.origin;
            int directionId = MyPlacedBuildingData.directionID;
            directionId--;
            if (directionId < 0) directionId = 3;
            validOutputSorource = new List<Vector2Int>()
            {
                outputpos1,
                outputpos1 + GeneralConstants.NeighbourOffsets2D4[directionId]
            };
            validInputSorource = new List<Vector2Int>()
            {
                -1 * GeneralConstants.NeighbourOffsets2D4[MyPlacedBuildingData.directionID] +
                MyPlacedBuildingData.origin,
            };
        }

        protected override Slot GetInputSlot(GridObject callerPosition)
        {
            return validInputSorource.Any(i => callerPosition.Position == i) ? InputSlot : null;
        }

        protected override Slot GetOutputSlot(GridObject callerPosition)
        {
            for (int i = 0; i < validOutputSorource.Count; i++)
            {
                if (callerPosition.Position == validOutputSorource[i])
                {
                    return i == 0 ? Output1Slot : Output2Slot;
                }
            }
            
            return null;
        }

        private void InputSlotChanged(bool fillStatus)
        {
            if (fillStatus && !Output1Slot.IsOccupied && !Output2Slot.IsOccupied)
            {
                int itemLength = InputSlot.SlotContent.ResourceIDs.Length;
                if(itemLength < 1) return;
                int item1Length = Mathf.CeilToInt(itemLength / 2f), item2Length = item1Length - item1Length;
                Item item = InputSlot.EmptySlot();
                int[] contentItem1= new int[item1Length], contentItem2=new int[item2Length];
                for (int i = 0; i < itemLength; i++)
                {
                    if (i < item1Length) contentItem1[i] = item.ResourceIDs[i];
                    else contentItem2[i-item1Length] = item.ResourceIDs[i];
                }

                Output1Slot.FillSlot(new Item(contentItem1));
                Output2Slot.FillSlot(new Item(contentItem2));
            }
        }
    }
}
