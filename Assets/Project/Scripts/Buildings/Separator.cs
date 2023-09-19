using System;
using System.Collections;
using Project.Scripts.Buildings.BuildingFoundation;
using Project.Scripts.Buildings.Parts;
using Project.Scripts.ItemSystem;
using Project.Scripts.SlotSystem;
using UnityEngine;

namespace Project.Scripts.Buildings
{
    public class Separator : PlacedBuilding, IHaveOutput,IHaveInput, IConveyorDestination
    {
        private static float SepartionPerSecond = 0.25f;

        private Coroutine SeparactionProcess;
        private bool CanStartSeparation => inputs[0].IsOccupied && !outputs[0].IsOccupied && !outputs[1].IsOccupied;
        protected override void StartWorking()
        {
            base.StartWorking();
            CheckForSlotToPullForm();
            CheckForSlotsToPushTo();
        }

        protected override void SetUpSlots(FacingDirection facingDirection)
        {
            foreach (Slot input in inputs) input.OnSlotContentChanged += SlotChanged;
            foreach (Slot output in outputs) output.OnSlotContentChanged += SlotChanged;

            mySlotValidationHandler = MyPlacedBuildingData.directionID switch
            {
                0 => Resources.Load<SlotValidationHandler>("Buildings/SlotValidation/Separator/SeparatorUp"),
                1 => Resources.Load<SlotValidationHandler>("Buildings/SlotValidation/Separator/SeparatorRight"),
                2 => Resources.Load<SlotValidationHandler>("Buildings/SlotValidation/Separator/SeparatorDown"),
                3 => Resources.Load<SlotValidationHandler>("Buildings/SlotValidation/Separator/SeparatorLeft"),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private void SlotChanged(bool fillStatus)
        {
            if (!CanStartSeparation) return;
            SeparactionProcess ??= StartCoroutine(TrySeparate());
        }

        private IEnumerator TrySeparate()
        {
            do
            {
                int itemLength = inputs[0].SlotContent.Item.ResourceIDs.Length;
                if(itemLength < 1) break;
                int item1Length = Mathf.CeilToInt(itemLength / 2f), item2Length = itemLength - item1Length;
                ItemContainer item = inputs[0].EmptySlot();
                int[] contentItem1= new int[item1Length], contentItem2=new int[item2Length];
                for (int i = 0; i < itemLength; i++)
                {
                    if (i < item1Length) contentItem1[i] = item.Item.ResourceIDs[i];
                    else contentItem2[i-item1Length] = item.Item.ResourceIDs[i];
                }
                item.Destroy();

                outputs[0].FillSlot(ItemUtility.GetItemContainerWith(new Item(contentItem1),outputs[0]));
                outputs[1].FillSlot(ItemUtility.GetItemContainerWith(new Item(contentItem2),outputs[1]));

                yield return new WaitForSeconds(1 / SepartionPerSecond);

            } while (CanStartSeparation);

            SeparactionProcess = null;
        }

        public override void CheckForSlotToPullForm()
        {
            base.CheckForSlotToPullForm();
            if (!subedToConveyorTick)
            {
                ConveyorBelt.ConveyorTick += StartConveyorChainTickUpdate;
                subedToConveyorTick = true;
            }
        }

        public Slot GetOutputSlot(PlacedBuilding caller, Slot destination)
        {
            if (!mySlotValidationHandler.ValidateOutputSlotRequest(this, caller, out int index)) return null;
            if (slotsToPushTo[index]) return null;
            slotsToPushTo[index] = destination;
            return outputs[index];
        }

        public Slot GetInputSlot(PlacedBuilding caller, Slot destination)
        {
            if (slotsToPullFrom[0]) return null;
            slotsToPullFrom[0] = destination;
            if (!subedToConveyorTick)
            {
                ConveyorBelt.ConveyorTick += StartConveyorChainTickUpdate;
                subedToConveyorTick = true;
            }
            return inputs[0];
        }

        public void StartConveyorChainTickUpdate()
        {
            bool resp = false;
            foreach (Slot receiver in slotsToPullFrom)
            {
                if(!receiver) continue;
                IReceiveConveyorChainTickUpdate receive = receiver.gameObject.GetComponentInParent<IReceiveConveyorChainTickUpdate>();
                if(receive ==null) continue;
                StartCoroutine(ConveyorBelt.ConveyorChainTickUpdateHandler(receive));
                resp = true;
            }
            
            PullItem();

            switch (resp)
            {
                case false when subedToConveyorTick:
                    ConveyorBelt.ConveyorTick -= StartConveyorChainTickUpdate;
                    subedToConveyorTick = false;
                    break;
                case true when !subedToConveyorTick:
                    ConveyorBelt.ConveyorTick += StartConveyorChainTickUpdate;
                    subedToConveyorTick = true;
                    break;
            }
        }
    }
}
