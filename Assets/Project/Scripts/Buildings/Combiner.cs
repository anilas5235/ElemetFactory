using System;
using System.Collections;
using System.Collections.Generic;
using Project.Scripts.Buildings.Parts;
using Project.Scripts.ItemSystem;
using Project.Scripts.SlotSystem;
using UnityEngine;

namespace Project.Scripts.Buildings
{
    public class Combiner : PlacedBuilding,IHaveInput,IHaveOutput
    {
        private static float CombinationsPerSecond = .25f;

        private Coroutine CombineProcess;

        protected override void StartWorking()
        {
            CheckForSlotToPullForm();
            CheckForSlotsToPushTo();
        }

        public Slot GetInputSlot(PlacedBuilding caller, Slot destination)
        {
            return mySlotValidationHandler.ValidateInputSlotRequest(this, caller, out int index)
                ? inputs[index]
                : null;
        }

        public Slot GetOutputSlot(PlacedBuilding caller, Slot destination)
        {
            return mySlotValidationHandler.ValidateOutputSlotRequest(this,caller,out int index)
                ? outputs[index]
                : null;
        }

        private void StartCombining(bool fillStatus)
        {
           if(!fillStatus || CombineProcess != null)return;
           if(!inputs[0].IsOccupied|| !inputs[1].IsOccupied || outputs[0].IsOccupied) return;
           CombineProcess = StartCoroutine(TryCombine());
        }

        private IEnumerator TryCombine()
        {
            do
            {
                ItemContainer container1 = inputs[0].EmptySlot();
                ItemContainer container2 = inputs[1].EmptySlot();
                int[] combIDs = new int[container1.Item.ResourceIDs.Length + container2.Item.ResourceIDs.Length];
                container1.Item.ResourceIDs.CopyTo(combIDs, 0);
                container1.Destroy();
                container2.Item.ResourceIDs.CopyTo(combIDs, container1.Item.ResourceIDs.Length);
                container2.Destroy();

                outputs[0].FillSlot(ItemUtility.GetItemContainerWith(combIDs, outputs[0]));

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
    }
}
