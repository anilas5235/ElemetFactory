using System;
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

        protected override void StartWorking()
        {
            CheckForSlotToPullForm();
            CheckForSlotsToPushTo();
            inputs[0].OnSlotContentChanged += TryCombine;
            inputs[1].OnSlotContentChanged += TryCombine;
            outputs[0].OnSlotContentChanged += TryCombine;
        }

        public Slot GetInputSlot(PlacedBuildingData caller, Slot destination)
        {
            return mySlotValidationHandler.ValidateInputSlotRequest(MyGridObject.Position, caller.origin,
                (FacingDirection)caller.directionID)
                ? ((caller.origin- MyGridObject.Position).magnitude>1.1f? inputs[1]: inputs[0])
                : null;
        }

        public Slot GetOutputSlot(PlacedBuildingData caller, Slot destination)
        {
            return mySlotValidationHandler.ValidateOutputSlotRequest(MyGridObject.Position, caller.origin,
                (FacingDirection)caller.directionID)
                ? outputs[0]
                : null;
        }

        private void TryCombine(bool fill)
        {
            if(!fill)return;
            if(!inputs[0].IsOccupied|| !inputs[1].IsOccupied || outputs[0].IsOccupied) return;

            ItemContainer container1 = inputs[0].ExtractFromSlot();
            ItemContainer container2 = inputs[1].ExtractFromSlot();
            int[] combIDs = new int[container1.Item.ResourceIDs.Length + container2.Item.ResourceIDs.Length];
            container1.Item.ResourceIDs.CopyTo(combIDs, 0);
            container1.Destroy();
            container2.Item.ResourceIDs.CopyTo(combIDs, container1.Item.ResourceIDs.Length);
            container2.Destroy();
            
            outputs[0].PutIntoSlot(ItemUtility.GetItemContainerWith(combIDs));
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
        }
    }
}
