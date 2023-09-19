using System;
using Project.Scripts.Buildings.BuildingFoundation;
using Project.Scripts.Buildings.Parts;
using Project.Scripts.EntitySystem.Components.Transmission;
using Project.Scripts.SlotSystem;
using Unity.Entities;
using UnityEngine;

namespace Project.Scripts.Buildings
{
    public class Combiner : PlacedBuildingEntity, IEntityInput,IEntityOutput
    {
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
            
            CheckForSlotToPullForm();
            CheckForSlotsToPushTo();
        }

        public bool GetInput(PlacedBuildingEntity caller, out Entity entity, out int inputIndex)
        {
            entity = default;
            inputIndex = default;
            if (!mySlotValidationHandler.ValidateInputSlotRequest(this, caller, out inputIndex)) return false;
            InputDataComponent input =_entityManager.GetBuffer<InputDataComponent>(BuildingEntity)[inputIndex];
            if (input.EntityToPullFrom != default) return false;
            input.EntityToPullFrom = entity;
            return true;
        }

        public bool GetOutput(PlacedBuildingEntity caller, out Entity entity, out int outputIndex)
        {
            entity = default;
            outputIndex = default;
            if (!mySlotValidationHandler.ValidateInputSlotRequest(this, caller, out outputIndex)) return false;
            OutputDataComponent output =_entityManager.GetBuffer<OutputDataComponent>(BuildingEntity)[outputIndex];
            if (output.EntityToPushTo != default) return false;
            output.EntityToPushTo = entity;
            return true;
        }
    }

}
