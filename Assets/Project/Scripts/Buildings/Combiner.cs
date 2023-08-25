using System;
using Project.Scripts.Buildings.Parts;
using Project.Scripts.SlotSystem;
using UnityEngine;

namespace Project.Scripts.Buildings
{
    public class Combiner : PlacedBuilding,IHaveInput,IHaveOutput
    {
        private static float CombinationsPerSecond = .25f;
        private SlotValidationHandler mySlotValidationHandler;

        [SerializeField] protected Slot outPut1, outPut2, input;

        protected override void StartWorking()
        {
            throw new System.NotImplementedException();
        }

        public Slot GetInputSlot(PlacedBuildingData caller, Slot destination)
        {
            throw new System.NotImplementedException();
        }

        public Slot GetOutputSlot(PlacedBuildingData caller, Slot destination)
        {
            throw new System.NotImplementedException();
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

        public override void CheckForSlotToPullForm()
        {
            throw new System.NotImplementedException();
        }

        public override void CheckForSlotsToPushTo()
        {
            throw new System.NotImplementedException();
        }
    }
}
