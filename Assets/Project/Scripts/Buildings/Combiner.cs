using System;
using System.Collections;
using System.Linq;
using Project.Scripts.Buildings.BuildingFoundation;
using Project.Scripts.Buildings.Parts;
using Project.Scripts.ItemSystem;
using Project.Scripts.SlotSystem;
using UnityEngine;

namespace Project.Scripts.Buildings
{
    public class Combiner : PlacedBuilding,IHaveInput,IHaveOutput,IConveyorDestination
    {
        private static float CombinationsPerSecond = .25f;

        private Coroutine CombineProcess;

        protected override void StartWorking()
        {
            base.StartWorking();
            CheckForSlotToPullForm();
            CheckForSlotsToPushTo();
        }

        public Slot GetInputSlot(PlacedBuilding caller, Slot destination)
        {
            if (!mySlotValidationHandler.ValidateInputSlotRequest(this, caller, out int index)) return null;
            if (slotsToPullFrom[index]) return null;
            slotsToPullFrom[index] = destination;
            if (!subedToConveyorTick)
            {
                ConveyorBelt.ConveyorTick += StartConveyorChainTickUpdate;
                subedToConveyorTick = true;
            }
            return inputs[index];
        }

        public Slot GetOutputSlot(PlacedBuilding caller, Slot destination)
        {
            if (slotsToPushTo[0]) return null;
            slotsToPushTo[0] = destination;
            return outputs[0];
        }

        public override void CheckForSlotToPullForm()
        {
            base.CheckForSlotToPullForm();
            if (subedToConveyorTick) return;
            ConveyorBelt.ConveyorTick += StartConveyorChainTickUpdate;
            subedToConveyorTick = true;
        }

        private void StartCombining(bool fillStatus)
        {
           if(CombineProcess != null)return;
           if(!inputs[0].IsOccupied|| !inputs[1].IsOccupied || outputs[0].IsOccupied) return;
           CombineProcess = StartCoroutine(TryCombine());
        }

        private IEnumerator TryCombine()
        {
            do
            {
                ItemContainer container1 = inputs[0].EmptySlot();
                ItemContainer container2 = inputs[1].EmptySlot();
                int[] combIDs = new int[container1.Item.resourceIDs.Length + container2.Item.resourceIDs.Length];
                container1.Item.resourceIDs.CopyTo(combIDs, 0);
                container1.Destroy();
                container2.Item.resourceIDs.CopyTo(combIDs, container1.Item.resourceIDs.Length);
                container2.Destroy();

                outputs[0].FillSlot(ItemUtility.GetItemContainerWith(combIDs, outputs[0]));
                
                for (int i = 0; i < outputs.Length; i++)
                {
                    if(outputs[i].IsOccupied && slotsToPushTo[i] && !slotsToPushTo[i].IsOccupied) slotsToPushTo[i].PutIntoSlot(outputs[i].ExtractFromSlot());
                }

                yield return new WaitForSeconds(1 / CombinationsPerSecond);

            } while (inputs[0].IsOccupied && inputs[1].IsOccupied && !outputs[0].IsOccupied);

            CombineProcess = null;
        }

        protected override void SetUpSlots(FacingDirection facingDirection)
        {
            mySlotValidationHandler = MyPlacedBuildingData.directionID switch
            {
                0 => Resources.Load<SlotValidationHandler>("Buildings/SlotValidation/Combiner/CombinerUp"),
                1 => Resources.Load<SlotValidationHandler>("Buildings/SlotValidation/Combiner/CombinerRight"),
                2 => Resources.Load<SlotValidationHandler>("Buildings/SlotValidation/Combiner/CombinerDown"),
                3 => Resources.Load<SlotValidationHandler>("Buildings/SlotValidation/Combiner/CombinerLeft"),
                _ => throw new ArgumentOutOfRangeException()
            };
            inputs[0].OnSlotContentChanged += StartCombining;
            inputs[1].OnSlotContentChanged += StartCombining;
            outputs[0].OnSlotContentChanged += StartCombining;
        }

        public override void Destroy()
        {
            if (subedToConveyorTick) ConveyorBelt.ConveyorTick -= StartConveyorChainTickUpdate;
            base.Destroy();
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
