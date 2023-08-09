using UnityEngine;

namespace Project.Scripts.Buildings
{
    public class Separator : PlacedBuilding
    {
        private static float SepartionPerSecond = 0.25f;

        private Slot Input, Output1, Output2;

        protected override void StartWorking()
        {
            Input = new Slot(Slot.SlotBehaviour.Input);
            Input.OnSlotContentChanged += InputSlotChanged;
            Output1 = new Slot(Slot.SlotBehaviour.Output);
            Output2 = new Slot(Slot.SlotBehaviour.Output);
        }

        private void InputSlotChanged(bool fillStatus)
        {
            if (fillStatus && !Output1.IsOccupied && !Output2.IsOccupied)
            {
                int itemLength = Input.SlotContent.ResourceIDs.Length;
                if(itemLength < 1) return;
                int item1Length = Mathf.CeilToInt(itemLength / 2f), item2Length = item1Length - item1Length;
                Item item = Input.EmptySlot();
                int[] contentItem1= new int[item1Length], contentItem2=new int[item2Length];
                for (int i = 0; i < itemLength; i++)
                {
                    if (i < item1Length) contentItem1[i] = item.ResourceIDs[i];
                    else contentItem2[i] = item.ResourceIDs[i];
                }

                Output1.FillSlot(new Item(contentItem1));
                Output2.FillSlot(new Item(contentItem2));
            }
        }
    }
}
